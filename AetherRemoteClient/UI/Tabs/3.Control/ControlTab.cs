using AetherRemoteClient.Accessors.Glamourer;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Providers;
using AetherRemoteCommon;
using AetherRemoteCommon.Domain.CommonChatMode;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Plugin.Services;
using ImGuiNET;
using System;
using System.Numerics;
using System.Threading.Tasks;

namespace AetherRemoteClient.UI.Tabs.Control;

// TODO: Add brief timeout values to prevent spam sending + give visual feedback

public class ControlTab : ITab
{
    // Constants
    private const ImGuiTableFlags FriendListTableFlags = ImGuiTableFlags.Borders;
    private const int LinkshellSelectorWidth = 42;

    private static readonly Vector2 QuestionIconOffset = CalcQuestionButtonOffset();
    private static readonly Vector2 LockButtonSize = new(40, 40);
    private static readonly int SendButtonWidth = 40;
    private static readonly int TransformButtonWidth = 80;

    // Dependencies
    private readonly Configuration configuration;
    private readonly GlamourerAccessor glamourerAccessor;
    private readonly EmoteProvider emoteProvider;
    private readonly NetworkProvider networkProvider;
    private readonly IClientState clientState;
    private readonly IPluginLog logger;
    private readonly ITargetManager targetManager;

    // Variables - Friend List
    private string searchInputText = "";
    private readonly ListFilter<Friend> friendListSearchFilter;

    // Variables

    private bool lockCurrentFriend = false;
    private Friend? currentFriend = null;

    // Variables - Speak
    private ChatMode chatMode = ChatMode.Say;
    private int shellNumber = 1;
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
        this.glamourerAccessor = glamourerAccessor;
        this.emoteProvider = emoteProvider;
        this.networkProvider = networkProvider;
        this.clientState = clientState;
        this.logger = logger;
        this.targetManager = targetManager;

        friendListSearchFilter = new(networkProvider.FriendList?.Friends ?? [], (friend, searchTerm) => { return friend.NoteOrFriendCode.Contains(searchTerm); });
        emoteSearchFilter = new(emoteProvider.Emotes, (emote, searchTerm) => { return emote.Contains(searchTerm); });
    }

    public void Draw()
    {
        if (ImGui.BeginTabItem("Control"))
        {
            ImGui.SetNextItemWidth(MainWindow.FriendListSize.X);
            if (ImGui.InputTextWithHint("##SearchFriendListInputText", "Search", ref searchInputText, Constants.FriendNicknameCharLimit))
            {
                friendListSearchFilter.UpdateSearchTerm(searchInputText);
            }
            
            // Save the cursor at the bottom of the search input text before calling ImGui.SameLine for use later
            var bottomOfSearchInputText = ImGui.GetCursorPosY();

            ImGui.SameLine();

            // Draw the settings area beside the search bar using the remaining space
            if (ImGui.BeginChild("FriendSettingsArea", Vector2.Zero, true))
            {
                DrawControlPanel();
                ImGui.EndChild();
            }

            // Set the cursor back and begin drawing add friend input text & button
            ImGui.SetCursorPosY(bottomOfSearchInputText);

            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
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
        if (ImGui.BeginTable("FriendListTable", 1, FriendListTableFlags))
        {
            foreach (var friend in networkProvider.FriendList?.Friends ?? [])
            {
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                SharedUserInterfaces.Icon(FontAwesomeIcon.User);
                ImGui.SameLine();

                if (ImGui.Selectable($"{friend.NoteOrFriendCode}", (currentFriend == friend), ImGuiSelectableFlags.SpanAllColumns))
                {
                    if (lockCurrentFriend == false)
                        currentFriend = friend;
                }
            }

            ImGui.EndTable();
        }
    }

    private void DrawControlPanel()
    {
        if (currentFriend == null)
        {
            SharedUserInterfaces.PushBigFont();
            ImGui.SetCursorPosY((ImGui.GetWindowHeight() / 2) - ImGui.GetFontSize());
            SharedUserInterfaces.TextCentered("Select Friend");
            SharedUserInterfaces.PopBigFont();

            SharedUserInterfaces.TextCentered("Start by selecting a friend from the left");
            return;
        }

        SharedUserInterfaces.BigTextCentered(currentFriend.NoteOrFriendCode, ImGuiColors.ParsedOrange);

        ImGui.SameLine();
        ImGui.SetCursorPosX(ImGui.GetWindowWidth() - LockButtonSize.X - ImGui.GetStyle().WindowPadding.X);
        var lockIcon = lockCurrentFriend ? FontAwesomeIcon.Lock : FontAwesomeIcon.LockOpen;
        if (SharedUserInterfaces.IconButton(lockIcon, LockButtonSize))
        {
            lockCurrentFriend = !lockCurrentFriend;
        }
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text(lockCurrentFriend ? "Click to unlock current friend" : "Click to lock current friend");
            ImGui.EndTooltip();
        }

        DrawSpeakModule();

        ImGui.Separator();
        DrawEmoteModule();

        ImGui.Separator();
        DrawGlamourerModule();
    }

    private void DrawSpeakModule()
    {
        var shouldProcessSpeakCommand = false;

        SharedUserInterfaces.MediumTextCentered("Speak", null, QuestionIconOffset);
        ImGui.SameLine();
        ImGui.AlignTextToFramePadding();
        SharedUserInterfaces.Icon(FontAwesomeIcon.QuestionCircle);
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text("Force selected friend to send a message in specified chat.\nSome channels will require additional input.");
            ImGui.EndTooltip();
        }

        SharedUserInterfaces.MediumText("Channel:", ImGuiColors.ParsedOrange);
        ImGui.SameLine();
        SharedUserInterfaces.MediumText(chatMode.ToCondensedString());

        // TODO: Make input for LS/CWLS
        // TODO: Make input for Tell

        if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Comment))
        {
            ImGui.OpenPopup("ChatModeSelector");
        }

        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text("Change chat channel");
            ImGui.EndTooltip();
        }

        if (ImGui.BeginPopup("ChatModeSelector"))
        {
            foreach (ChatMode mode in Enum.GetValues(typeof(ChatMode)))
            {
                if (ImGui.Selectable(mode.ToCondensedString(), mode == chatMode))
                {
                    chatMode = mode;
                }
            }

            ImGui.EndPopup();
        }

        ImGui.SameLine();

        ImGui.SetNextItemWidth(-1 * (SendButtonWidth + ImGui.GetStyle().WindowPadding.X));
        if (ImGui.InputTextWithHint("###MessageInputBox", "Message", ref message, 500, ImGuiInputTextFlags.EnterReturnsTrue))
            shouldProcessSpeakCommand = true;

        ImGui.SameLine();

        if (ImGui.Button("Send", new Vector2(SendButtonWidth, 0)))
            shouldProcessSpeakCommand = true;

        if (shouldProcessSpeakCommand) { _ = ProcessSpeakCommand(); }
    }

    public void DrawEmoteModule()
    {
        var shouldProcessEmoteCommand = false;

        SharedUserInterfaces.MediumTextCentered("Emote", null, QuestionIconOffset);
        ImGui.SameLine();
        ImGui.AlignTextToFramePadding();
        SharedUserInterfaces.Icon(FontAwesomeIcon.QuestionCircle);
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text("Force selected friend to preform an emote.");
            ImGui.EndTooltip();
        }

        SharedUserInterfaces.MediumText("Emote", ImGuiColors.ParsedOrange);

        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(4, 4));
        SharedUserInterfaces.ComboWithFilter(ref emote, "Emote", emoteSearchFilter);
        ImGui.PopStyleVar();
        
        ImGui.SameLine();

        if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Play))
        {
            shouldProcessEmoteCommand = true;
        }

        if (shouldProcessEmoteCommand) { _ = ProcessEmoteCommand(); }
    }

    public void DrawGlamourerModule()
    {
        var shouldProcessBecomeCommand = false;
        var glamourerInstalled = glamourerAccessor.IsGlamourerInstalled;

        SharedUserInterfaces.MediumTextCentered("Transformation", null, QuestionIconOffset);
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

        if (SharedUserInterfaces.IconButton(FontAwesomeIcon.User))
            CopyMyGlamourerData();

        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text("Copies your glamourer data into the input");
            ImGui.EndTooltip();
        }

        ImGui.SameLine();

        if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Crosshairs))
            CopyTargetGlamourerData();

        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text("Copies your target's glamourer data into the input");
            ImGui.EndTooltip();
        }

        ImGui.SameLine();

        if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Broom))
        {
            glamourerData = "";
        }

        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text("Clear glamourer data");
            ImGui.EndTooltip();
        }

        ImGui.SetNextItemWidth(-1 * (TransformButtonWidth + ImGui.GetStyle().WindowPadding.X));
        if (ImGui.InputTextWithHint("###GlamourerDataInput", "Enter glamourer data", ref glamourerData, Constants.GlamourerDataCharLimit, ImGuiInputTextFlags.EnterReturnsTrue))
        {
            shouldProcessBecomeCommand = true;
        }

        ImGui.SameLine();

        if (ImGui.Button("Transform", new Vector2(TransformButtonWidth, 0)))
        {
            shouldProcessBecomeCommand = true;
        }

        ImGui.Spacing();

        ImGui.Checkbox("Change Appearance", ref applyCustomization);
        ImGui.SameLine();
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 45);
        ImGui.Checkbox("Change Equipment", ref applyEquipment);

        if (shouldProcessBecomeCommand) { _ = ProcessBecomeCommand(); }

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
            extra = shellNumber.ToString();
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

            // Reset message
            message = "";
        }
        else
        {
            /* TODO */
        }
    }

    private async Task ProcessEmoteCommand()
    {
        if (currentFriend == null || message.Length <= 0)
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

            // Reset emote
            emote = "";
        }
        else
        {
            /* TODO */
        }
    }

    private async Task ProcessBecomeCommand()
    {
        if (currentFriend == null || message.Length <= 0 || glamourerData.Length == 0)
            return;

        var glamourerApplyType = GlamourerAccessor.ConvertBoolsToApplyType(applyCustomization, applyEquipment);

        var secret = configuration.Secret;
        var result = await networkProvider.Become(secret, [currentFriend], glamourerData, glamourerApplyType);
        if (result.Success)
        {
            var applyType = GlamourerAccessor.ConvertBoolsToApplyType(applyCustomization, applyEquipment);
            var log = AetherRemoteLogging.FormatBecomeLog(currentFriend.NoteOrFriendCode, applyType, glamourerData);
            AetherRemoteLogging.Log("Me", log, DateTime.Now, LogType.Sent);

            // Reset glamourer data
            glamourerData = "";
        }
        else
        {
            /* TODO */
        }
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
}
