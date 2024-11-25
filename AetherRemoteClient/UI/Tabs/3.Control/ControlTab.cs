using AetherRemoteClient.Accessors.Glamourer;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Events;
using AetherRemoteClient.Domain.Log;
using AetherRemoteClient.Domain.UI;
using AetherRemoteClient.Providers;
using AetherRemoteClient.UI.Tabs.Views;
using AetherRemoteCommon;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace AetherRemoteClient.UI.Tabs.Control;

public class ControlTab : ITab
{
    // Constants
    private static readonly Vector4 IconOnlineColor = ImGuiColors.ParsedGreen;
    private static readonly Vector2 LockButtonSize = new(40, 40);
    private const int DefaultWindowPadding = 8;
    private const string DeselectAllButtonText = "Deselect All";

    // Injected
    private readonly ClientDataManager _clientDataManager;

    // Instantiated
    private readonly CommandLockoutManager _commandLockoutManager = new();
    
    private readonly SpeakView _speakView;
    private readonly EmoteView _emoteView;
    private readonly TransformationView _transformationView;
    private readonly ExtraView _extraView;

    private readonly ListFilter<Friend> _friendListFilter;

    // Variables
    private bool _lockCurrentTargets;

    // Variables - Friend List
    private string _searchInputText = "";

    public ControlTab(
        ClientDataManager clientDataManager,
        EmoteProvider emoteProvider,
        GlamourerAccessor glamourerAccessor,
        HistoryLogProvider historyLogProvider,
        ModManager modManager,
        NetworkProvider networkProvider,
        WorldProvider worldProvider)
    {
        _clientDataManager = clientDataManager;
        _friendListFilter = new ListFilter<Friend>(clientDataManager.FriendsList.Friends, FilterFriends);
        
        _speakView = new SpeakView(clientDataManager, _commandLockoutManager, historyLogProvider, networkProvider, worldProvider);
        _emoteView = new EmoteView(clientDataManager, _commandLockoutManager, emoteProvider, historyLogProvider, networkProvider);
        _transformationView = new TransformationView(clientDataManager, _commandLockoutManager, glamourerAccessor, historyLogProvider, networkProvider);
        _extraView = new ExtraView(clientDataManager, _commandLockoutManager, glamourerAccessor, historyLogProvider, modManager, networkProvider);

        clientDataManager.FriendsList.OnFriendDeleted += HandleFriendDeleted;
        clientDataManager.FriendsList.OnFriendsListCleared += HandleFriendsListCleared;
        clientDataManager.FriendsList.OnFriendOnlineStatusChanged += HandleFriendOnlineStatusChanged;
    }

    public void Draw()
    {
        // If we are not in the control tab, return
        if (ImGui.BeginTabItem("Control") == false) return;
        
        // Grab a reference to the style
        var style = ImGui.GetStyle();

        // The height of the footer containing the friend code input text and the add friend button
        var deselectButtonHeight = ImGui.CalcTextSize(DeselectAllButtonText).Y + style.FramePadding.Y * 2 + style.ItemSpacing.Y;

        // Store Y coord at the top of the screen
        var windowTopPosY = ImGui.GetCursorPosY();

        // Selection Mode
        ImGui.AlignTextToFramePadding();
        ImGui.BeginGroup();
        SharedUserInterfaces.Icon(FontAwesomeIcon.User);

        ImGui.SameLine();

        // Single Mode Selection
        var selectMode = _clientDataManager.TargetManager.SingleSelectionMode ? 0 : 1;
        if (ImGui.RadioButton("##SelectModeSingle", ref selectMode, 0))
        {
            _clientDataManager.TargetManager.SingleSelectionMode = true;
            _lockCurrentTargets = false;
        }

        ImGui.EndGroup();
        SharedUserInterfaces.Tooltip("Single Selection Mode");

        ImGui.SameLine();

        var text = selectMode == 0 ? "Single" : "Multi";
        ImGui.SetCursorPosX(style.WindowPadding.X + MainWindow.FriendListSize.X / 2 - ImGui.CalcTextSize(text).X / 2 - style.FramePadding.X / 2);

        ImGui.Text(text);

        ImGui.SameLine(MainWindow.FriendListSize.X - style.WindowPadding.X - style.FramePadding.X - ImGui.GetFontSize() * 2);

        // Multi Mode Selection
        ImGui.BeginGroup();
        if (ImGui.RadioButton("##SelectModeMultiple", ref selectMode, 1))
        {
            _clientDataManager.TargetManager.SingleSelectionMode = false;
            _lockCurrentTargets = false;
        }

        ImGui.SameLine();
        SharedUserInterfaces.Icon(FontAwesomeIcon.Users);

        ImGui.EndGroup();
        SharedUserInterfaces.Tooltip("Multi Selection Mode");

        // Setup friend search
        ImGui.SetNextItemWidth(MainWindow.FriendListSize.X);
        if (ImGui.InputTextWithHint("##SearchFriendListInputText", "Search", ref _searchInputText, Constraints.FriendNicknameCharLimit))
            _friendListFilter.UpdateSearchTerm(_searchInputText);

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
            foreach (var friend in _friendListFilter.List)
            {
                if (friend.Online)
                    DrawSelectableFriend(friend);
            }

            ImGui.EndChild();
        }
        ImGui.PopStyleVar();

        if (ImGui.Button(DeselectAllButtonText, MainWindow.FriendListSize))
            _clientDataManager.TargetManager.Clear();

        ImGui.EndTabItem();
    }

    private void DrawSelectableFriend(Friend friend)
    {
        // Draw Selectable Text
        ImGui.SetCursorPosX(DefaultWindowPadding + ImGui.GetFontSize() + ImGui.GetStyle().FramePadding.X * 2);
        
        var selected = _clientDataManager.TargetManager.Selected(friend.FriendCode);
        var friendNote = Plugin.Configuration.Notes.GetValueOrDefault(friend.FriendCode);
        var selectableId = $"{friendNote ?? friend.FriendCode}###{friend.FriendCode}";
        if (ImGui.Selectable(selectableId, selected, ImGuiSelectableFlags.SpanAllColumns))
        {
            if (_lockCurrentTargets == false)
                _clientDataManager.TargetManager.ToggleSelect(friend);
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
        if (_clientDataManager.TargetManager.Targets.IsEmpty)
        {
            var defaultFontSize = ImGui.GetFontSize();
            SharedUserInterfaces.PushBigFont();
            ImGui.SetCursorPosY(ImGui.GetWindowHeight() / 2 - ImGui.GetFontSize() - defaultFontSize);
            SharedUserInterfaces.TextCentered("Select Friends");
            SharedUserInterfaces.PopBigFont();

            SharedUserInterfaces.TextCentered("Pick a friend to begin");
            return;
        }

        string text;
        if (_clientDataManager.TargetManager.Targets.Count > 1)
        {
            text = $"{_clientDataManager.TargetManager.Targets.Count} Friends";
        }
        else
        {
            var firstFriend = _clientDataManager.TargetManager.Targets.First();
            text = Plugin.Configuration.Notes.TryGetValue(firstFriend.Key, out var note) ? note : firstFriend.Key;
        }

        SharedUserInterfaces.BigTextCentered(text, ImGuiColors.ParsedOrange);

        ImGui.SameLine();
        ImGui.SetCursorPosX(ImGui.GetWindowWidth() - LockButtonSize.X - ImGui.GetStyle().WindowPadding.X);
        var lockIcon = _lockCurrentTargets ? FontAwesomeIcon.Lock : FontAwesomeIcon.LockOpen;
        if (SharedUserInterfaces.IconButton(lockIcon, LockButtonSize))
            _lockCurrentTargets = !_lockCurrentTargets;

        SharedUserInterfaces.Tooltip(_lockCurrentTargets ? "Click to unlock current targets" : "Click to lock current targets");

        _speakView.Draw();

        ImGui.Separator();
        _emoteView.Draw();

        ImGui.Separator();
        _transformationView.Draw();

        ImGui.Separator();
        _extraView.Draw();
    }

    private void HandleFriendDeleted(object? sender, FriendDeletedEventArgs e)
    {
        _clientDataManager.TargetManager.Deselect(e.Friend.FriendCode);

        if (_clientDataManager.TargetManager.Targets.IsEmpty)
            _lockCurrentTargets = false;
    }

    private void HandleFriendsListCleared(object? sender, FriendsListDeletedEventArgs e)
    {
        _lockCurrentTargets = false;
    }
    
    private void HandleFriendOnlineStatusChanged(object? sender, FriendOnlineStatusChangedEventArgs e)
    {
        if(e.Friend.Online == false)
            _clientDataManager.TargetManager.Deselect(e.Friend.FriendCode);
    }

    private static bool FilterFriends(Friend friend, string searchTerm)
    {
        var containedInNote = false;
        if (Plugin.Configuration.Notes.TryGetValue(searchTerm, out var note))
            containedInNote = note.Contains(searchTerm, StringComparison.OrdinalIgnoreCase);

        return containedInNote || friend.FriendCode.Contains(searchTerm, StringComparison.OrdinalIgnoreCase);
    }

    public void Dispose()
    {
        _clientDataManager.FriendsList.OnFriendDeleted -= HandleFriendDeleted;
        _clientDataManager.FriendsList.OnFriendsListCleared -= HandleFriendsListCleared;
        _clientDataManager.FriendsList.OnFriendOnlineStatusChanged -= HandleFriendOnlineStatusChanged;

        _commandLockoutManager.Dispose();
        
        _speakView.Dispose();
        _emoteView.Dispose();
        _transformationView.Dispose();
        _extraView.Dispose();

        GC.SuppressFinalize(this);
    }
}
