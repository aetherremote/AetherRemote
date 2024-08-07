using AetherRemoteClient.Accessors.Glamourer;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Events;
using AetherRemoteClient.Domain.Logger;
using AetherRemoteClient.Providers;
using AetherRemoteClient.UI.Tabs.Friends;
using AetherRemoteClient.UI.Tabs.Modules;
using AetherRemoteCommon;
using AetherRemoteCommon.Domain.CommonFriend;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Plugin.Services;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Timers;

namespace AetherRemoteClient.UI.Tabs.Control;

public class ControlTab : ITab
{
    // Constants
    private static readonly Vector4 IconOnlineColor = ImGuiColors.ParsedGreen;
    private static readonly Vector2 LockButtonSize = new(40, 40);
    private static readonly int LockoutDuration = 2500;
    private static readonly int DefaultWindowPadding = 8;
    private static readonly string DeselectAllButtonText = "Deselect All";

    // Variables
    private bool lockCurrentTargets = false;

    // Variables - Spam Prevention
    private readonly Timer commandLockoutTimer = new(LockoutDuration);

    // Variables - Friend List
    private string searchInputText = "";
    private readonly FriendListFilter friendSearchFilter;

    private readonly ControlTargetManager controlTargetManager = new();
    private readonly EmoteModule emoteModule;
    private readonly GlamourerModule glamourerModule;
    private readonly SpeakModule speakModule;

    public ControlTab(Configuration configuration, GlamourerAccessor glamourerAccessor, EmoteProvider emoteProvider, NetworkProvider networkProvider,
        AetherRemoteLogger logger, IClientState clientState, ITargetManager targetManager)
    {
        friendSearchFilter = new(networkProvider, (friend, searchTerm) => { return friend.NoteOrFriendCode.Contains(searchTerm); });

        emoteModule = new EmoteModule(configuration, emoteProvider, networkProvider, logger, controlTargetManager, commandLockoutTimer);
        glamourerModule = new GlamourerModule(configuration, glamourerAccessor, networkProvider, logger, clientState, targetManager, controlTargetManager, commandLockoutTimer);
        speakModule = new SpeakModule(configuration, networkProvider, logger, clientState, targetManager, controlTargetManager, commandLockoutTimer);

        FriendsTab.OnFriendDeleted += FriendDeleted;
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
            var selectMode = controlTargetManager.Mode == ControlTargetManager.SelectionMode.Single ? 0 : 1;
            if (ImGui.RadioButton("##SelectModeSingle", ref selectMode, 0))
                controlTargetManager.UpdateSelectionMode(ControlTargetManager.SelectionMode.Single);

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
                controlTargetManager.UpdateSelectionMode(ControlTargetManager.SelectionMode.Multiple);

            ImGui.SameLine();
            SharedUserInterfaces.Icon(FontAwesomeIcon.Users);

            ImGui.EndGroup();
            SharedUserInterfaces.Tooltip("Multi Selection Mode");

            // Setup friend search
            ImGui.SetNextItemWidth(MainWindow.FriendListSize.X);
            if (ImGui.InputTextWithHint("##SearchFriendListInputText", "Search", ref searchInputText, Constants.FriendNicknameCharLimit))
                friendSearchFilter.UpdateSearchTerm(searchInputText);

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
                DrawFriendList();
                ImGui.EndChild();
            }
            ImGui.PopStyleVar();

            if (ImGui.Button(DeselectAllButtonText, MainWindow.FriendListSize))
                controlTargetManager.DeselectAll();

            ImGui.EndTabItem();
        }
    }

    private void DrawFriendList()
    {
        var onlineFriends = new List<Friend>();
        foreach (var friend in friendSearchFilter.List)
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
        // Draw Selectable Text
        ImGui.SetCursorPosX(DefaultWindowPadding + ImGui.GetFontSize() + (ImGui.GetStyle().FramePadding.X * 2));
        if (ImGui.Selectable($"{friend.NoteOrFriendCode}", controlTargetManager.IsSelected(friend), ImGuiSelectableFlags.SpanAllColumns))
            if (lockCurrentTargets == false)
                controlTargetManager.ToggleSelected(friend);

        // Draw Icon
        ImGui.SameLine(DefaultWindowPadding);
        ImGui.PushStyleColor(ImGuiCol.Text, IconOnlineColor);
        SharedUserInterfaces.Icon(FontAwesomeIcon.User);
        ImGui.PopStyleColor();
    }

    private void DrawControlPanel()
    {
        // No friend selected
        if (controlTargetManager.MinimumTargetsMet == false)
        {
            var defaultFontSize = ImGui.GetFontSize();
            SharedUserInterfaces.PushBigFont();
            ImGui.SetCursorPosY((ImGui.GetWindowHeight() / 2) - ImGui.GetFontSize() - defaultFontSize);
            SharedUserInterfaces.TextCentered("Select Friends");
            SharedUserInterfaces.PopBigFont();

            SharedUserInterfaces.TextCentered($"Select at least one or more friends from the left");
            return;
        }

        var text = controlTargetManager.Targets.Count < 2 ? controlTargetManager.Targets[0].NoteOrFriendCode : $"{controlTargetManager.Targets.Count} Friends";
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
    }

    private void FriendDeleted(object? sender, FriendDeletedEventArgs e)
    {
        controlTargetManager.Deselect(e.Friend);
    }

    public void Dispose()
    {
        // Modules
        emoteModule.Dispose();
        glamourerModule.Dispose();
        speakModule.Dispose();

        // Filters
        friendSearchFilter.Dispose();

        // Timers
        commandLockoutTimer.Dispose();

        FriendsTab.OnFriendDeleted -= FriendDeleted;
        GC.SuppressFinalize(this);
    }
}
