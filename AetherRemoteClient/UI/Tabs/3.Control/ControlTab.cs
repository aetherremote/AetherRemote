using AetherRemoteClient.Accessors.Glamourer;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Events;
using AetherRemoteClient.Domain.Log;
using AetherRemoteClient.Domain.UI;
using AetherRemoteClient.Providers;
using AetherRemoteClient.UI.Tabs.Modules;
using AetherRemoteCommon;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using ImGuiNET;
using System;
using System.Linq;
using System.Numerics;

namespace AetherRemoteClient.UI.Tabs.Control;

public class ControlTab : ITab
{
    // Constants
    private static readonly Vector4 IconOnlineColor = ImGuiColors.ParsedGreen;
    private static readonly Vector2 LockButtonSize = new(40, 40);
    private static readonly int DefaultWindowPadding = 8;
    private static readonly string DeselectAllButtonText = "Deselect All";

    // Injected
    private readonly ClientDataManager clientDataManager;

    // Instantiated
    private readonly CommandLockoutManager commandLockoutManager = new();
    private readonly EmoteModule emoteModule;
    private readonly ExtraModule extraModule;
    private readonly TransformationModule glamourerModule;
    private readonly SpeakModule speakModule;
    private readonly ListFilter<Friend> friendListFilter;

    // Variables
    private bool lockCurrentTargets = false;

    // Variables - Friend List
    private string searchInputText = "";

    public ControlTab(
        ClientDataManager clientDataManager,
        EmoteProvider emoteProvider,
        GlamourerAccessor glamourerAccessor,
        HistoryLogManager historyLogManager,
        NetworkProvider networkProvider)
    {
        this.clientDataManager = clientDataManager;

        friendListFilter = new ListFilter<Friend>(clientDataManager.FriendsList.Friends, FilterFriends);

        emoteModule = new EmoteModule(clientDataManager, commandLockoutManager, emoteProvider, historyLogManager, networkProvider);
        extraModule = new ExtraModule(clientDataManager, commandLockoutManager, glamourerAccessor, historyLogManager, networkProvider);
        glamourerModule = new TransformationModule(clientDataManager, commandLockoutManager, glamourerAccessor, historyLogManager, networkProvider);
        speakModule = new SpeakModule(clientDataManager, commandLockoutManager, historyLogManager, networkProvider);

        clientDataManager.FriendsList.OnFriendDeleted += HandleFriendDeleted;
        clientDataManager.FriendsList.OnFriendsListCleared += HandleFriendsListCleared;
    }

    public void Draw()
    {
        // If we are not in the control tab, return
        if (ImGui.BeginTabItem("Control"))
        {
            // Grab a reference to the style
            var style = ImGui.GetStyle();

            // The height of the footer containing the friend code input text and the add friend button
            var deselectButtonHeight = (ImGui.CalcTextSize(DeselectAllButtonText).Y + (style.FramePadding.Y * 2) + style.ItemSpacing.Y);

            // Store Y coord at the top of the screen
            var windowTopPosY = ImGui.GetCursorPosY();

            // Selection Mode
            ImGui.AlignTextToFramePadding();
            ImGui.BeginGroup();
            SharedUserInterfaces.Icon(FontAwesomeIcon.User);

            ImGui.SameLine();

            // Single Mode Selection
            var selectMode = clientDataManager.TargetManager.SingleSelectionMode ? 0 : 1;
            if (ImGui.RadioButton("##SelectModeSingle", ref selectMode, 0))
            {
                clientDataManager.TargetManager.SingleSelectionMode = true;
                lockCurrentTargets = false;
            }

            ImGui.EndGroup();
            SharedUserInterfaces.Tooltip("Single Selection Mode");

            ImGui.SameLine();

            var text = selectMode == 0 ? "Single" : "Multi";
            ImGui.SetCursorPosX(style.WindowPadding.X + (MainWindow.FriendListSize.X / 2) - (ImGui.CalcTextSize(text).X / 2) - (style.FramePadding.X / 2));

            ImGui.Text(text);

            ImGui.SameLine(MainWindow.FriendListSize.X - style.WindowPadding.X - style.FramePadding.X - (ImGui.GetFontSize() * 2));

            // Multi Mode Selection
            ImGui.BeginGroup();
            if (ImGui.RadioButton("##SelectModeMultiple", ref selectMode, 1))
            {
                clientDataManager.TargetManager.SingleSelectionMode = false;
                lockCurrentTargets = false;
            }

            ImGui.SameLine();
            SharedUserInterfaces.Icon(FontAwesomeIcon.Users);

            ImGui.EndGroup();
            SharedUserInterfaces.Tooltip("Multi Selection Mode");

            // Setup friend search
            ImGui.SetNextItemWidth(MainWindow.FriendListSize.X);
            if (ImGui.InputTextWithHint("##SearchFriendListInputText", "Search", ref searchInputText, Constraints.FriendNicknameCharLimit))
                friendListFilter.UpdateSearchTerm(searchInputText);

            // Save the cursor at the bottom of the search input text before calling ImGui.SameLine for use later
            var bottomOfSearchInputText = ImGui.GetCursorPosY();

            // Reset cursor back to top of screen
            ImGui.SameLine();
            ImGui.SetCursorPosY(windowTopPosY);

            // Draw the control panel area beside the search bar using the remaining space
            if (ImGui.BeginChild("ControlPanelArea", Vector2.Zero, true))
            {
                DrawControlPanel();
                ImGui.EndChild();
            }

            // Set the cursor back and begin drawing add friend input text & button
            ImGui.SetCursorPosY(bottomOfSearchInputText);

            // Draw the friend list area
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 2));
            if (ImGui.BeginChild("FriendListArea", new Vector2(MainWindow.FriendListSize.X, -1 * deselectButtonHeight), true))
            {
                foreach (var friend in friendListFilter.List)
                {
                    if (friend.Online)
                        DrawSelectableFriend(friend);
                }

                ImGui.EndChild();
            }
            ImGui.PopStyleVar();

            if (ImGui.Button(DeselectAllButtonText, MainWindow.FriendListSize))
                clientDataManager.TargetManager.Clear();

            ImGui.EndTabItem();
        }
    }

    private void DrawSelectableFriend(Friend friend)
    {
        // Draw Selectable Text
        ImGui.SetCursorPosX(DefaultWindowPadding + ImGui.GetFontSize() + (ImGui.GetStyle().FramePadding.X * 2));
        
        var selected = clientDataManager.TargetManager.Selected(friend.FriendCode);
        var friendNote = Plugin.Configuration.Notes.TryGetValue(friend.FriendCode, out var note) ? note : null;
        var selectableId = $"{friendNote ?? friend.FriendCode}###{friend.FriendCode}";
        if (ImGui.Selectable(selectableId, selected, ImGuiSelectableFlags.SpanAllColumns))
        {
            if (lockCurrentTargets == false)
                clientDataManager.TargetManager.ToggleSelect(friend.FriendCode);
        }

        // Draw Icon
        ImGui.SameLine(DefaultWindowPadding);
        ImGui.PushStyleColor(ImGuiCol.Text, IconOnlineColor);
        SharedUserInterfaces.Icon(FontAwesomeIcon.User);
        ImGui.PopStyleColor();
    }

    private void DrawControlPanel()
    {
        // No friend selected
        if (clientDataManager.TargetManager.Targets.Count == 0)
        {
            var defaultFontSize = ImGui.GetFontSize();
            SharedUserInterfaces.PushBigFont();
            ImGui.SetCursorPosY((ImGui.GetWindowHeight() / 2) - ImGui.GetFontSize() - defaultFontSize);
            SharedUserInterfaces.TextCentered("Select Friends");
            SharedUserInterfaces.PopBigFont();

            SharedUserInterfaces.TextCentered($"Select at least one or more friends from the left");
            return;
        }

        string text;
        if (clientDataManager.TargetManager.Targets.Count > 1)
        {
            text = $"{clientDataManager.TargetManager.Targets.Count} Friends";
        }
        else
        {
            var friendCode = clientDataManager.TargetManager.Targets.First();
            text = Plugin.Configuration.Notes.TryGetValue(friendCode, out var note) ? note : friendCode;
        }

        SharedUserInterfaces.BigTextCentered(text, ImGuiColors.ParsedOrange);

        ImGui.SameLine();
        ImGui.SetCursorPosX(ImGui.GetWindowWidth() - LockButtonSize.X - ImGui.GetStyle().WindowPadding.X);
        var lockIcon = lockCurrentTargets ? FontAwesomeIcon.Lock : FontAwesomeIcon.LockOpen;
        if (SharedUserInterfaces.IconButton(lockIcon, LockButtonSize))
            lockCurrentTargets = !lockCurrentTargets;

        SharedUserInterfaces.Tooltip(lockCurrentTargets ? "Click to unlock current targets" : "Click to lock current targets");

        speakModule.Draw();

        ImGui.Separator();
        emoteModule.Draw();

        ImGui.Separator();
        glamourerModule.Draw();

        ImGui.Separator();
        extraModule.Draw();
    }

    private void HandleFriendDeleted(object? sender, FriendDeletedEventArgs e)
    {
        clientDataManager.TargetManager.Deselect(e.Friend.FriendCode);

        if (clientDataManager.TargetManager.Targets.Count == 0)
            lockCurrentTargets = false;
    }

    private void HandleFriendsListCleared(object? sender, FriendsListDeletedEventArgs e)
    {
        lockCurrentTargets = false;
    }

    private bool FilterFriends(Friend friend, string searchTerm)
    {
        var containedInNote = false;
        if (Plugin.Configuration.Notes.TryGetValue(searchTerm, out var note))
            containedInNote = note.Contains(searchTerm, StringComparison.OrdinalIgnoreCase);

        return containedInNote || friend.FriendCode.Contains(searchTerm, StringComparison.OrdinalIgnoreCase);
    }

    public void Dispose()
    {
        clientDataManager.FriendsList.OnFriendDeleted -= HandleFriendDeleted;
        clientDataManager.FriendsList.OnFriendsListCleared -= HandleFriendsListCleared;

        commandLockoutManager.Dispose();
        emoteModule.Dispose();
        glamourerModule.Dispose();
        speakModule.Dispose();
        extraModule.Dispose();

        GC.SuppressFinalize(this);
    }
}
