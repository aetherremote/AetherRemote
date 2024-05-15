using AetherRemoteClient.Accessors.Glamourer;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Events;
using AetherRemoteClient.Providers;
using AetherRemoteClient.UI.Tabs.Friends;
using AetherRemoteCommon;
using AetherRemoteCommon.Domain.CommonChatMode;
using AetherRemoteCommon.Domain.CommonFriend;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Logging;
using Dalamud.Plugin.Services;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using System.Timers;

namespace AetherRemoteClient.UI.Tabs.Control;

public class ControlTab : ITab
{
    // Constants
    private static readonly Vector4 IconOnlineColor = ImGuiColors.ParsedGreen;
    private static readonly Vector4 IconOfflineColor = ImGuiColors.DPSRed;
    private static readonly Vector2 LockButtonSize = new(40, 40);
    private static readonly int SendButtonWidth = 40;
    private static readonly int TransformButtonWidth = 80;
    private static readonly int LockoutDuration = 2500;
    private readonly Vector2 questionIconOffset;

    // Dependencies
    private readonly Configuration configuration;
    private readonly GlamourerAccessor glamourerAccessor;
    private readonly EmoteProvider emoteProvider;
    private readonly NetworkProvider networkProvider;
    private readonly IClientState clientState;
    private readonly IPluginLog logger;
    private readonly ITargetManager targetManager;

    // Variables
    private bool lockCurrentFriend = false;
    private Friend? currentFriend = null;

    // Variables - Spam Prevention
    private bool lockoutActive = false;
    private readonly Timer commandLockoutTimer;

    // Variables - Friend List
    private string searchInputText = "";
    private readonly FriendListFilter friendSearchFilter;

    // Variables - Speak
    private ChatMode chatMode = ChatMode.Say;
    private int linkshellNumber = 1;
    private string tellTarget = "";
    private string message = "";

    // Variables - Emote
    private string emote = "";
    private readonly ListFilter<string> emoteSearchFilter;

    // Variables - Glamourer
    private string glamourerData = "";
    private bool applyCustomization = true;
    private bool applyEquipment = true;

    public ControlTab(Configuration configuration, GlamourerAccessor glamourerAccessor, EmoteProvider emoteProvider, NetworkProvider networkProvider, 
        IClientState clientState, IPluginLog logger, ITargetManager targetManager)
    {
        this.configuration = configuration;
        this.glamourerAccessor = glamourerAccessor;
        this.emoteProvider = emoteProvider;
        this.networkProvider = networkProvider;
        this.clientState = clientState;
        this.logger = logger;
        this.targetManager = targetManager;

        friendSearchFilter = new(networkProvider, (friend, searchTerm) => { return friend.NoteOrFriendCode.Contains(searchTerm); });
        emoteSearchFilter = new(emoteProvider.Emotes, (emote, searchTerm) => { return emote.Contains(searchTerm); });

        commandLockoutTimer = new(LockoutDuration);
        commandLockoutTimer.Elapsed += ReleaseLockout;

        FriendsTab.OnFriendDeleted += FriendDeleted;

        questionIconOffset = CalcQuestionButtonOffset();
    }

    public void Draw()
    {
        if (ImGui.BeginTabItem("Control"))
        {
            ImGui.SetNextItemWidth(MainWindow.FriendListSize.X);
            if (ImGui.InputTextWithHint("##SearchFriendListInputText", "Search", ref searchInputText, Constants.FriendNicknameCharLimit))
                friendSearchFilter.UpdateSearchTerm(searchInputText);

            // Save the cursor at the bottom of the search input text before calling ImGui.SameLine for use later
            var bottomOfSearchInputText = ImGui.GetCursorPosY();

            ImGui.SameLine();

            // Draw the control panel area beside the search bar using the remaining space
            if (ImGui.BeginChild("ControlPanelArea", Vector2.Zero, true))
            {
                DrawControlPanel();
                ImGui.EndChild();
            }

            // Set the cursor back and begin drawing add friend input text & button
            ImGui.SetCursorPosY(bottomOfSearchInputText);

            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 2));
            if (ImGui.BeginChild("FriendListArea", new Vector2(150, 0), true))
            {
                DrawFriendList();
                ImGui.EndChild();
            }
            ImGui.PopStyleVar();

            ImGui.EndTabItem();
        }
    }

    private void DrawFriendList()
    {
        var onlineFriends = new List<Friend>();
        foreach (var friend in networkProvider.FriendList?.Friends ?? [])
            if (friend.Online) onlineFriends.Add(friend);

        if (ImGui.TreeNodeEx($"Online ({onlineFriends.Count})", ImGuiTreeNodeFlags.DefaultOpen))
        {
            foreach (var friend in onlineFriends)
                DrawSelectableFriend(friend);

            ImGui.TreePop();
        }
    }

    private void DrawSelectableFriend(Friend friend)
    {
        var onlineStatus = friend.Online;
        ImGui.SetCursorPosX(8 + ImGui.GetFontSize() + (ImGui.GetStyle().FramePadding.X * 2));

        // Draw Selectable Text
        if (onlineStatus == false) ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudGrey);
        if (ImGui.Selectable($"{friend.NoteOrFriendCode}", currentFriend == friend, ImGuiSelectableFlags.SpanAllColumns))
            currentFriend = lockCurrentFriend ? currentFriend : friend;

        if (onlineStatus == false) ImGui.PopStyleColor();

        // Draw Icon
        ImGui.SameLine(8);
        ImGui.PushStyleColor(ImGuiCol.Text, onlineStatus ? IconOnlineColor : IconOfflineColor);
        SharedUserInterfaces.Icon(FontAwesomeIcon.User);
        ImGui.PopStyleColor();
    }

    private void DrawControlPanel()
    {
        // No friend selected
        if (currentFriend == null)
        {
            SharedUserInterfaces.PushBigFont();
            ImGui.SetCursorPosY((ImGui.GetWindowHeight() / 2) - ImGui.GetFontSize());
            SharedUserInterfaces.TextCentered("Select Friend");
            SharedUserInterfaces.PopBigFont();

            SharedUserInterfaces.TextCentered("Start by selecting a friend from the left");
            return;
        }

        // If the friend you are controlling goes offline
        if (currentFriend.Online == false)
        {
            ReleaseCurrentFriend();
            return;
        }

        SharedUserInterfaces.BigTextCentered(currentFriend.NoteOrFriendCode, ImGuiColors.ParsedOrange);

        ImGui.SameLine();
        ImGui.SetCursorPosX(ImGui.GetWindowWidth() - LockButtonSize.X - ImGui.GetStyle().WindowPadding.X);
        var lockIcon = lockCurrentFriend ? FontAwesomeIcon.Lock : FontAwesomeIcon.LockOpen;
        if (SharedUserInterfaces.IconButton(lockIcon, LockButtonSize))
            lockCurrentFriend = !lockCurrentFriend;

        SharedUserInterfaces.Tooltip(lockCurrentFriend ? "Click to unlock current friend" : "Click to lock current friend");

        DrawSpeakModule();

        ImGui.Separator();
        DrawEmoteModule();

        ImGui.Separator();
        DrawGlamourerModule();
    }

    private void DrawSpeakModule()
    {
        var style = ImGui.GetStyle();
        var shouldProcessSpeakCommand = false;

        SharedUserInterfaces.MediumTextCentered("Speak", null, questionIconOffset);
        ImGui.SameLine();
        ImGui.AlignTextToFramePadding();
        SharedUserInterfaces.Icon(FontAwesomeIcon.QuestionCircle);
        SharedUserInterfaces.Tooltip("Force selected friend to send a message in specified channel.");

        SharedUserInterfaces.MediumText("Channel:", ImGuiColors.ParsedOrange);
        ImGui.SameLine();
        SharedUserInterfaces.MediumText(chatMode.ToCondensedString());

        if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Comment))
            ImGui.OpenPopup("ChatModeSelector");

        SharedUserInterfaces.Tooltip("Change chat channel");

        if (ImGui.BeginPopup("ChatModeSelector"))
        {
            foreach (ChatMode mode in Enum.GetValues(typeof(ChatMode)))
                if (ImGui.Selectable(mode.ToCondensedString(), mode == chatMode))
                    chatMode = mode;

            ImGui.EndPopup();
        }

        ImGui.SameLine();

        ImGui.SetNextItemWidth(-1 * (SendButtonWidth + style.WindowPadding.X));
        if (ImGui.InputTextWithHint("###MessageInputBox", "Message", ref message, 500, ImGuiInputTextFlags.EnterReturnsTrue))
            shouldProcessSpeakCommand = true;

        ImGui.SameLine();

        var lockout = lockoutActive;
        if (lockout) ImGui.BeginDisabled();
        if (ImGui.Button("Send", new Vector2(SendButtonWidth, 0)))
            shouldProcessSpeakCommand = true;
        if (lockout) ImGui.EndDisabled();

        if (chatMode == ChatMode.Tell)
        {
            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.User))
                tellTarget = clientState.LocalPlayer?.Name.ToString() ?? tellTarget;

            SharedUserInterfaces.Tooltip("Copy my name");
            ImGui.SameLine();

            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Crosshairs))
                tellTarget = targetManager.Target?.Name.ToString() ?? tellTarget;

            SharedUserInterfaces.Tooltip("Copy my target's name");
            ImGui.SameLine();

            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Broom))
                tellTarget = "";

            SharedUserInterfaces.Tooltip("Clear the tell target input field");

            ImGui.SameLine();

            ImGui.SetNextItemWidth(ImGui.GetWindowWidth() / 2);
            ImGui.InputTextWithHint("##TellTargetInput", "Tell Target", ref tellTarget, Constants.PlayerNameCharLimit);
        }
        else if (chatMode == ChatMode.Linkshell || chatMode == ChatMode.CrossworldLinkshell)
        {
            ImGui.SetCursorPosX((style.WindowPadding.X * 2) + (style.FramePadding.X * 2) + ImGui.GetFontSize());

            ImGui.SetNextItemWidth(50);
            if (ImGui.BeginCombo("Linkshell Number", linkshellNumber.ToString()))
            {
                for(var i = 1; i < 9; i++)
                {
                    var selected = i == linkshellNumber;
                    if (ImGui.Selectable(i.ToString(), selected))
                        linkshellNumber = i;
                    if (selected)
                        ImGui.SetItemDefaultFocus();
                }

                ImGui.EndCombo();
            }

            ImGui.SameLine();
            SharedUserInterfaces.Icon(FontAwesomeIcon.ExclamationCircle);
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                SharedUserInterfaces.TextCentered("Caution!");
                ImGui.Text("Which number a linkshell corresponds with may differ from person to person.");
                ImGui.EndTooltip();
            }
        }

        if (shouldProcessSpeakCommand && lockoutActive == false)
        {
            Lockout();
            _ = ProcessSpeakCommand();
        }
    }

    public void DrawEmoteModule()
    {
        var shouldProcessEmoteCommand = false;

        SharedUserInterfaces.MediumTextCentered("Emote", null, questionIconOffset);
        ImGui.SameLine();
        ImGui.AlignTextToFramePadding();
        SharedUserInterfaces.Icon(FontAwesomeIcon.QuestionCircle);
        SharedUserInterfaces.Tooltip("Force selected friend to preform an emote.");

        SharedUserInterfaces.MediumText("Emote", ImGuiColors.ParsedOrange);

        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(4, 4));
        SharedUserInterfaces.ComboWithFilter(ref emote, "Emote", emoteSearchFilter);
        ImGui.PopStyleVar();
        
        ImGui.SameLine();

        var lockout = lockoutActive;
        if (lockout) ImGui.BeginDisabled();
        if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Play))
            shouldProcessEmoteCommand = true;
        ImGui.EndDisabled();

        if (shouldProcessEmoteCommand && lockoutActive == false)
        {
            Lockout();
            _ = ProcessEmoteCommand();
        }
    }

    public void DrawGlamourerModule()
    {
        var shouldProcessBecomeCommand = false;
        var glamourerInstalled = glamourerAccessor.IsGlamourerInstalled;

        SharedUserInterfaces.MediumTextCentered("Transformation", null, questionIconOffset);
        ImGui.SameLine();
        ImGui.AlignTextToFramePadding();
        SharedUserInterfaces.Icon(FontAwesomeIcon.QuestionCircle);
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            SharedUserInterfaces.TextCentered("Requires Glamourer, Optionally Mare");
            ImGui.Text("Force current friend to change their appearance and/or equipment.");
            ImGui.EndTooltip();
        }

        SharedUserInterfaces.MediumText("Glamourer", glamourerInstalled ? ImGuiColors.ParsedOrange : ImGuiColors.DalamudGrey);
        if (glamourerInstalled == false)
            ImGui.BeginDisabled();

        ImGui.SetNextItemWidth(-1 * (TransformButtonWidth + ImGui.GetStyle().WindowPadding.X));
        if (ImGui.InputTextWithHint("###GlamourerDataInput", "Enter glamourer data", ref glamourerData, Constants.GlamourerDataCharLimit, ImGuiInputTextFlags.EnterReturnsTrue))
            shouldProcessBecomeCommand = true;

        ImGui.SameLine();

        var lockout = lockoutActive;
        if (lockout) ImGui.BeginDisabled();
        if (ImGui.Button("Transform", new Vector2(TransformButtonWidth, 0)))
            shouldProcessBecomeCommand = true;
        if (lockout) ImGui.EndDisabled();

        ImGui.Spacing();

        if (SharedUserInterfaces.IconButton(FontAwesomeIcon.User))
            CopyMyGlamourerData();

        SharedUserInterfaces.Tooltip("Copy my glamourer data");
        ImGui.SameLine();

        if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Crosshairs))
            CopyTargetGlamourerData();

        SharedUserInterfaces.Tooltip("Copy my taregt's glamourer data");
        ImGui.SameLine();

        if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Broom))
            glamourerData = "";

        SharedUserInterfaces.Tooltip("Clear glamourer data input field");

        ImGui.SameLine();

        var a = ImGui.CalcTextSize("Appearance").X;
        var b = ImGui.CalcTextSize("Equipment").X;
        ImGui.SetCursorPosX(ImGui.GetWindowWidth() - a - b - (ImGui.GetFontSize() * 2) - (ImGui.GetStyle().WindowPadding.X * 4) - ImGui.GetStyle().FramePadding.X);

        ImGui.Checkbox("Appearance", ref applyCustomization);
        ImGui.SameLine();
        ImGui.Checkbox("Equipment", ref applyEquipment);

        if (shouldProcessBecomeCommand && lockoutActive == false)
        {
            Lockout();
            _ = ProcessBecomeCommand();
        }

        if (glamourerInstalled == false)
            ImGui.EndDisabled();
    }

    private async Task ProcessSpeakCommand()
    {
        if (currentFriend == null || message.Length <= 0)
            return;

        string? extra = null;
        if (chatMode == ChatMode.Linkshell || chatMode == ChatMode.CrossworldLinkshell)
        {
            extra = linkshellNumber.ToString();
        }
        else if (chatMode == ChatMode.Tell)
        {
            if (tellTarget.Length > 0)
                extra = tellTarget;
        }

        var secret = configuration.Secret;
        var result = await networkProvider.Speak(secret, [currentFriend], message, chatMode, extra);
        if (result.Success)
        {
            var log = AetherRemoteLogging.FormatSpeakLog(currentFriend.NoteOrFriendCode, chatMode, message, extra);
            AetherRemoteLogging.Log("Me", log, DateTime.Now, LogType.Sent);
        }
        else
        {
            var log = $"Failed to make ${currentFriend.NoteOrFriendCode} speak.";
            AetherRemoteLogging.Log("Me", log, DateTime.Now, LogType.Error);
        }

        // Reset message
        message = "";
    }

    private async Task ProcessEmoteCommand()
    {
        if (currentFriend == null || emote.Length <= 0)
            return;

        var validEmote = emoteProvider.Emotes.Contains(emote);
        if (validEmote == false)
            return;

        var secret = configuration.Secret;
        var result = await networkProvider.Emote(secret, [currentFriend], emote);
        if (result.Success)
        {
            var log = AetherRemoteLogging.FormatEmoteLog(currentFriend.NoteOrFriendCode, emote);
            AetherRemoteLogging.Log("Me", log, DateTime.Now, LogType.Sent);
        }
        else
        {
            var log = $"Failed to make ${currentFriend.NoteOrFriendCode} emote.";
            AetherRemoteLogging.Log("Me", log, DateTime.Now, LogType.Error);
        }

        // Reset emote
        emote = "";
    }

    private async Task ProcessBecomeCommand()
    {
        if (currentFriend == null || glamourerData.Length == 0)
            return;

        var glamourerApplyType = GlamourerAccessor.ConvertBoolsToApplyType(applyCustomization, applyEquipment);

        var secret = configuration.Secret;
        var result = await networkProvider.Become(secret, [currentFriend], glamourerData, glamourerApplyType);
        if (result.Success)
        {
            var applyType = GlamourerAccessor.ConvertBoolsToApplyType(applyCustomization, applyEquipment);
            var log = AetherRemoteLogging.FormatBecomeLog(currentFriend.NoteOrFriendCode, applyType, glamourerData);
            AetherRemoteLogging.Log("Me", log, DateTime.Now, LogType.Sent);
        }
        else
        {
            var log = $"Failed to transform ${currentFriend.NoteOrFriendCode}.";
            AetherRemoteLogging.Log("Me", log, DateTime.Now, LogType.Error);
        }

        // Reset glamourer data
        glamourerData = "";
    }

    private void CopyMyGlamourerData()
    {
        var playerName = clientState.LocalPlayer?.Name.ToString();
        if (playerName == null)
            return;

        var data = glamourerAccessor.GetCustomization(playerName);
        if (data == null)
            return;

        glamourerData = data;
    }

    private void CopyTargetGlamourerData()
    {
        var targetName = targetManager.Target?.Name.ToString();
        if (targetName == null)
            return;

        var data = glamourerAccessor.GetCustomization(targetName);
        if (data == null)
            return;

        glamourerData = data;
    }

    private static Vector2 CalcQuestionButtonOffset()
    {
        var offset = SharedUserInterfaces.CalcIconSize(FontAwesomeIcon.QuestionCircle);
        offset.Y = 0;
        offset.X *= 0.5f;
        return offset;
    }

    private void ReleaseCurrentFriend()
    {
        currentFriend = null;
        lockCurrentFriend = false;
    }

    private void FriendDeleted(object? sender, FriendDeletedEventArgs e)
    {
        if (currentFriend?.FriendCode == e.FriendCode)
            ReleaseCurrentFriend();
    }

    private void Lockout()
    {
        commandLockoutTimer.Stop();
        commandLockoutTimer.Start();

        lockoutActive = true;
    }

    private void ReleaseLockout(object? sender, ElapsedEventArgs e)
    {
        lockoutActive = false;
    }

    public void Dispose()
    {
        friendSearchFilter.Dispose();
        emoteSearchFilter.Dispose();
        commandLockoutTimer.Dispose();
        FriendsTab.OnFriendDeleted -= FriendDeleted;
        GC.SuppressFinalize(this);
    }
}
