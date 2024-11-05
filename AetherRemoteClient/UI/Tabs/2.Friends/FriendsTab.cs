using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Events;
using AetherRemoteClient.Domain.UI;
using AetherRemoteClient.Providers;
using AetherRemoteCommon;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Network;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace AetherRemoteClient.UI.Tabs.Friends;

/// <summary>
/// Container for all UI elements of the Friend Tab
/// </summary>
public class FriendsTab : ITab
{
    // Constants
    private static readonly Vector2 FriendListPadding = new(0, 2);
    private static readonly Vector2 RoundButtonSize = new(40, 40);
    private static readonly Vector2 SmallButtonSize = new(24, 0);

    // Injected
    private readonly ClientDataManager _clientDataManager;
    private readonly NetworkProvider _networkProvider;

    // Instantiated
    private readonly ListFilter<Friend> _friendListFilter;

    // Input text reference for adding a friend
    private string _addFriendInputText = "";
    private string _searchInputText = "";

    // Focused Friend
    private Friend? _focusedFriend;
    private string _focusedFriendNote = string.Empty;
    private bool[] _focusedPermissions = ConvertUserPermissionsToBooleans(UserPermissions.None);

    // Processes
    private bool _shouldProcessAddFriend, _shouldProcessDeleteFriend, _shouldProcessSaveFriend;

    /// <summary>
    /// <inheritdoc cref="FriendsTab"/>
    /// </summary>
    public FriendsTab(ClientDataManager clientDataManager, NetworkProvider networkProvider)
    {
        _clientDataManager = clientDataManager;
        _networkProvider = networkProvider;
        _friendListFilter = new ListFilter<Friend>(clientDataManager.FriendsList.Friends, FilterFriends);
        _clientDataManager.FriendsList.OnFriendsListCleared += HandleFriendsListDeleted;
    }

    public void Draw()
    {
        // Grab a reference to the style
        var style = ImGui.GetStyle();

        // Reset
        _shouldProcessAddFriend = _shouldProcessSaveFriend = _shouldProcessDeleteFriend = false;

        // The height of the footer containing the friend code input text and the add friend button
        var footerHeight = (ImGui.CalcTextSize("Add Friend").Y + style.FramePadding.Y * 2 + style.ItemSpacing.Y) * 2;

        if (ImGui.BeginTabItem("Friends"))
        {
            // Draw the settings area beside the search bar using the remaining space
            ImGui.SetNextItemWidth(MainWindow.FriendListSize.X);
            if (ImGui.InputTextWithHint("##SearchFriendListInputText", "Search", ref _searchInputText, Constraints.FriendNicknameCharLimit))
                _friendListFilter.UpdateSearchTerm(_searchInputText);

            // Save the cursor at the bottom of the search input text before calling ImGui.SameLine for use later
            var bottomOfSearchInputText = ImGui.GetCursorPosY();
            ImGui.SameLine();

            // Draw the settings area beside the search bar using the remaining space
            if (ImGui.BeginChild("FriendSettingsArea", Vector2.Zero, true))
            {
                DrawFriendSetting();
                ImGui.EndChild();
            }

            // Set the cursor back and begin drawing add friend input text & button
            ImGui.SetCursorPosY(bottomOfSearchInputText);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, FriendListPadding);

            // By setting the Y value as negative, the window will be that many pixels up from the bottom
            if (ImGui.BeginChild("FriendListArea", new Vector2(MainWindow.FriendListSize.X, -1 * footerHeight), true))
            {
                var onlineFriends = new List<Friend>();
                var offlineFriends = new List<Friend>();
                foreach (var friend in _friendListFilter.List)
                    (friend.Online ? onlineFriends : offlineFriends).Add(friend);

                if (ImGui.TreeNodeEx($"Online ({onlineFriends.Count})", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    foreach (var friend in onlineFriends)
                        DrawSelectableFriend(friend);

                    ImGui.TreePop();
                }

                if (ImGui.TreeNodeEx($"Offline ({offlineFriends.Count})", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    foreach (var friend in offlineFriends)
                        DrawSelectableFriend(friend);

                    ImGui.TreePop();
                }

                ImGui.EndChild();
            }
            ImGui.PopStyleVar();

            ImGui.SetNextItemWidth(MainWindow.FriendListSize.X);
            if (ImGui.InputTextWithHint("##FriendCodeInputText", "Friend Code", ref _addFriendInputText, Constraints.FriendCodeCharLimit, ImGuiInputTextFlags.EnterReturnsTrue))
                _shouldProcessAddFriend = true;

            if (ImGui.Button("Add Friend", MainWindow.FriendListSize))
                _shouldProcessAddFriend = true;

            ImGui.EndTabItem();
        }

        if (_shouldProcessAddFriend) ProcessAddFriend();
        if (_shouldProcessSaveFriend) ProcessSaveFriend();
        if (_shouldProcessDeleteFriend) ProcessDeleteFriend();
    }

    private void DrawFriendSetting()
    {
        if (_focusedFriend == null)
        {
            SharedUserInterfaces.PushBigFont();
            var windowCenter = new Vector2(ImGui.GetWindowSize().X / 2, ImGui.GetWindowSize().Y / 2);
            var selectFriendButtonSize = ImGui.CalcTextSize("Select Friend");
            var cursorPos = new Vector2(windowCenter.X - (selectFriendButtonSize.X / 2), windowCenter.Y - selectFriendButtonSize.Y);
            ImGui.SetCursorPos(cursorPos);
            ImGui.Text("Select Friend");
            SharedUserInterfaces.PopBigFont();

            SharedUserInterfaces.TextCentered("Start by selecting a friend from the left");
        }
        else
        {
            SharedUserInterfaces.BigTextCentered(_focusedFriend.FriendCode, ImGuiColors.ParsedOrange);

            ImGui.SameLine();
            var deleteButtonPosition = ImGui.GetWindowSize() - ImGui.GetStyle().WindowPadding - RoundButtonSize;
            ImGui.SetCursorPosX(deleteButtonPosition.X);
            ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 100f);
            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Trash, RoundButtonSize))
                _shouldProcessDeleteFriend = true;

            ImGui.PopStyleVar();
            SharedUserInterfaces.Tooltip("Delete Friend");

            ImGui.SetNextItemWidth(MainWindow.FriendListSize.X);
            ImGui.InputTextWithHint("Note##EditingFriendCode", "Note", ref _focusedFriendNote, Constraints.FriendCodeCharLimit);
            ImGui.SameLine();
            SharedUserInterfaces.Icon(FontAwesomeIcon.QuestionCircle);
            SharedUserInterfaces.Tooltip("A note or 'nickname' to more easily identify a friend");

            ImGui.Separator();
            SharedUserInterfaces.TextCentered("General Permissions");

            if (ImGui.BeginTable("GeneralPermissionsTable", 2))
            {
                ImGui.TableNextColumn();
                ImGui.Checkbox("Allow Speak", ref _focusedPermissions[GetBitPosition(UserPermissions.Speak)]);
                ImGui.SameLine();
                SharedUserInterfaces.Icon(FontAwesomeIcon.QuestionCircle);
                SharedUserInterfaces.Tooltip("Allows friend to say things as you");

                ImGui.TableNextColumn();
                ImGui.Checkbox("Allow Emotes", ref _focusedPermissions[GetBitPosition(UserPermissions.Emote)]);
                ImGui.SameLine();
                SharedUserInterfaces.Icon(FontAwesomeIcon.QuestionCircle);
                SharedUserInterfaces.Tooltip("Allows friend to make you emote");

                ImGui.EndTable();
            }

            ImGui.Separator();
            SharedUserInterfaces.TextCentered("Channel Permissions");

            SharedUserInterfaces.DisableIf(_focusedPermissions[GetBitPosition(UserPermissions.Speak)] == false, () =>
            {
                ImGui.SameLine(ImGui.GetWindowSize().X - (ImGui.GetStyle().WindowPadding.X * 2) - (SmallButtonSize.X * 2));
                if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Check, SmallButtonSize)) SetAllChannelPermissions(true);
                SharedUserInterfaces.Tooltip("Allow All");

                ImGui.SameLine();
                if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Ban, SmallButtonSize)) SetAllChannelPermissions(false);
                SharedUserInterfaces.Tooltip("Disallow All");

                if (ImGui.BeginTable("SpeakPermissionsTable", 3))
                {
                    ImGui.TableNextColumn(); ImGui.Checkbox("Say", ref _focusedPermissions[GetBitPosition(UserPermissions.Say)]);
                    ImGui.TableNextColumn(); ImGui.Checkbox("Yell", ref _focusedPermissions[GetBitPosition(UserPermissions.Yell)]);
                    ImGui.TableNextColumn(); ImGui.Checkbox("Shout", ref _focusedPermissions[GetBitPosition(UserPermissions.Shout)]);
                    ImGui.TableNextColumn(); ImGui.Checkbox("Tell", ref _focusedPermissions[GetBitPosition(UserPermissions.Tell)]);
                    ImGui.TableNextColumn(); ImGui.Checkbox("Party", ref _focusedPermissions[GetBitPosition(UserPermissions.Party)]);
                    ImGui.TableNextColumn(); ImGui.Checkbox("Alliance", ref _focusedPermissions[GetBitPosition(UserPermissions.Alliance)]);
                    ImGui.TableNextColumn(); ImGui.Checkbox("Free Company", ref _focusedPermissions[GetBitPosition(UserPermissions.FreeCompany)]);
                    ImGui.TableNextColumn(); ImGui.Checkbox("PVP Team", ref _focusedPermissions[GetBitPosition(UserPermissions.PvPTeam)]);
                    ImGui.EndTable();
                }

                ImGui.Text("Linkshell");
                for (var i = 0; i < 7; i++)
                {
                    ImGui.Checkbox($"{i + 1}##LS", ref _focusedPermissions[GetBitPosition(UserPermissions.LS1) + i]);
                    ImGui.SameLine();
                }

                ImGui.Checkbox("8##LS", ref _focusedPermissions[GetBitPosition(UserPermissions.LS8)]);

                ImGui.Text("Cross-world Linkshells");
                for (var i = 0; i < 7; i++)
                {
                    ImGui.Checkbox($"{i + 1}##CWL", ref _focusedPermissions[GetBitPosition(UserPermissions.CWL1) + i]);
                    ImGui.SameLine();
                }

                ImGui.Checkbox("8##CWL", ref _focusedPermissions[GetBitPosition(UserPermissions.CWL8)]);
            });

            ImGui.Separator();
            SharedUserInterfaces.TextCentered("Glamourer Permissions");

            if (ImGui.BeginTable("GlamourerPermissionsTable", 2))
            {
                ImGui.TableNextColumn();
                ImGui.Checkbox("Allow Customization", ref _focusedPermissions[GetBitPosition(UserPermissions.Customization)]);
                ImGui.SameLine();
                SharedUserInterfaces.Icon(FontAwesomeIcon.QuestionCircle);
                SharedUserInterfaces.Tooltip("Allows friend to change your appearance");

                ImGui.TableNextColumn();
                ImGui.Checkbox("Allow Equipment", ref _focusedPermissions[GetBitPosition(UserPermissions.Equipment)]);
                ImGui.SameLine();
                SharedUserInterfaces.Icon(FontAwesomeIcon.QuestionCircle);
                SharedUserInterfaces.Tooltip("Allows friend to change your outfit");

                ImGui.TableNextColumn();
                ImGui.Checkbox("Allow Mod Swaps", ref _focusedPermissions[GetBitPosition(UserPermissions.ModSwap)]);
                ImGui.SameLine();
                SharedUserInterfaces.Icon(FontAwesomeIcon.QuestionCircle);
                SharedUserInterfaces.Tooltip("Allows your mods to be swapped during body swap and twinning commands.");

                ImGui.EndTable();
            }

            var saveButtonPosition = ImGui.GetWindowSize() - ImGui.GetStyle().WindowPadding - RoundButtonSize;
            ImGui.SetCursorPos(saveButtonPosition);
            ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 100f);
            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Save, RoundButtonSize) && _focusedFriend != null)
                _shouldProcessSaveFriend = true;

            SharedUserInterfaces.Tooltip("Save Friend");

            if (PendingChanges())
            {
                ImGui.SetCursorPosY(ImGui.GetWindowHeight() - ImGui.GetFontSize() - ImGui.GetStyle().WindowPadding.Y);
                SharedUserInterfaces.TextCentered("You have pending changes!", ImGuiColors.DalamudOrange);
            }

            ImGui.PopStyleVar();
        }
    }

    private void DrawSelectableFriend(Friend friend)
    {
        var onlineStatus = friend.Online;
        var friendNote = Plugin.Configuration.Notes.GetValueOrDefault(friend.FriendCode);
        ImGui.SetCursorPosX(8 + ImGui.GetFontSize() + (ImGui.GetStyle().FramePadding.X * 2));

        // Draw Selectable Text
        if (onlineStatus == false) ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudGrey);

        var selectableId = $"{friendNote ?? friend.FriendCode}###{friend.FriendCode}";
        if (ImGui.Selectable(selectableId, _focusedFriend == friend, ImGuiSelectableFlags.SpanAllColumns))
        {
            _focusedFriend = friend;
            _focusedFriendNote = friendNote ?? string.Empty;
            _focusedPermissions = ConvertUserPermissionsToBooleans(friend.PermissionsGrantedToFriend);
        }

        if (onlineStatus == false) ImGui.PopStyleColor();

        // Draw Icon
        ImGui.SameLine(8);
        ImGui.PushStyleColor(ImGuiCol.Text, onlineStatus ? ImGuiColors.ParsedGreen : ImGuiColors.DPSRed);
        SharedUserInterfaces.Icon(FontAwesomeIcon.User);
        ImGui.PopStyleColor();
    }

    private async void ProcessAddFriend()
    {
        var friendCode = _addFriendInputText;
        _addFriendInputText = "";

        if (string.IsNullOrEmpty(friendCode)) return;
        if (_clientDataManager.FriendsList.FindFriend(friendCode) != null) return;

        var (success, online) = await CreateOrUpdateFriend(friendCode);
        if (success)
            _clientDataManager.FriendsList.CreateFriend(friendCode, online);
    }

    private async void ProcessSaveFriend()
    {
        // Guard
        if (_focusedFriend == null) return;

        // Always save the note
        if (_focusedFriendNote == string.Empty)
            Plugin.Configuration.Notes.Remove(_focusedFriend.FriendCode);
        else
            Plugin.Configuration.Notes[_focusedFriend.FriendCode] = _focusedFriendNote;
        Plugin.Configuration.Save();

        // Convert permissions back to UserPermissions
        var permissions = ConvertBooleansToUserPermissions(_focusedPermissions);
        var (success, online) = await CreateOrUpdateFriend(_focusedFriend.FriendCode, permissions).ConfigureAwait(false);
        if (success)
        {
            // Only set locally if success on server
            _focusedFriend.Online = online;
            _focusedFriend.PermissionsGrantedToFriend = permissions;
        }
    }

    private async Task<(bool, bool)> CreateOrUpdateFriend(string friendCode, UserPermissions permissions = UserPermissions.None)
    {
        if (Plugin.DeveloperMode) return (true, true);

        var request = new CreateOrUpdatePermissionsRequest(friendCode, permissions);
        var response = await _networkProvider.InvokeCommand<CreateOrUpdatePermissionsRequest, CreateOrUpdatePermissionsResponse>(Network.Permissions.CreateOrUpdate, request);
        if (response.Success == false)
            Plugin.Log.Warning($"Unable to add friend {friendCode}. {response.Message}");

        return (response.Success, response.Online);
    }

    private async void ProcessDeleteFriend()
    {
        // Guard
        if (_focusedFriend == null) return;

        var success = await DeleteFriend(_focusedFriend);
        if (success)
            _clientDataManager.FriendsList.DeleteFriend(_focusedFriend.FriendCode);

        ResetFocusedFriend();
    }

    private async Task<bool> DeleteFriend(Friend friend)
    {
        if (Plugin.DeveloperMode) return true;

        var request = new DeletePermissionsRequest(friend.FriendCode);
        var response = await _networkProvider.InvokeCommand<DeletePermissionsRequest, DeletePermissionsResponse>(Network.Permissions.Delete, request);
        if (response.Success == false)
            Plugin.Log.Warning($"Unable to delete friend {friend.FriendCode}. {response.Message}");

        return response.Success;
    }

    public void Dispose()
    {
        _clientDataManager.FriendsList.OnFriendsListCleared -= HandleFriendsListDeleted;
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Resets Focused Friend related variables to their defaults
    /// </summary>
    private void ResetFocusedFriend()
    {
        _focusedFriend = null;
        _focusedFriendNote = string.Empty;
        _focusedPermissions = [];
    }

    /// <summary>
    /// Sets all text channel permissions to true or false
    /// </summary>
    private void SetAllChannelPermissions(bool enabled)
    {
        for(var i = GetBitPosition(UserPermissions.Say); i < GetBitPosition(UserPermissions.CWL8) + 1; i++)
            _focusedPermissions[i] = enabled;
    }

    /// <summary>
    /// Checks to see if the friend being edited has pending changes
    /// </summary>
    private bool PendingChanges()
    {
        if (_focusedFriend is null) return false;
        if (_focusedFriendNote != (Plugin.Configuration.Notes.TryGetValue(_focusedFriend.FriendCode, out var note) ? note : string.Empty))
            return true;

        var permissions = ConvertBooleansToUserPermissions(_focusedPermissions);
        return permissions != _focusedFriend.PermissionsGrantedToFriend;
    }

    /// <summary>
    /// ImGui UI Elements do not support bit masks. We must convert to an array of booleans.
    /// </summary>
    private static bool[] ConvertUserPermissionsToBooleans(UserPermissions permissions)
    {
        var length = Enum.GetValues(typeof(UserPermissions)).Length;
        var result = new bool[length];
        for(var i = 0; i < length; i++)
            result[i] = ((int)permissions & (1 << i)) != 0;

        return result;
    }

    /// <summary>
    /// This method undoes the conversion of <see cref="ConvertUserPermissionsToBooleans"/>
    /// </summary>
    private static UserPermissions ConvertBooleansToUserPermissions(bool[] permissions)
    {
        var result = UserPermissions.None;
        for (var i = 0; i < permissions.Length; i++)
            if (permissions[i])
                result |= (UserPermissions)(1 << i);

        return result;
    }

    /// <summary>
    /// Given a permission, calculate what position it would be in the boolean list returned by <see cref="ConvertUserPermissionsToBooleans"/>
    /// </summary>
    private static int GetBitPosition(UserPermissions permission)
    {
        var position = 0;
        var value = (int)permission;

        while (value > 1)
        {
            value >>= 1;
            position++;
        }

        return position;
    }

    private static bool FilterFriends(Friend friend, string searchTerm)
    {
        var containedInNote = false;
        if (Plugin.Configuration.Notes.TryGetValue(searchTerm, out var note))
            containedInNote = note.Contains(searchTerm, StringComparison.OrdinalIgnoreCase);

        return containedInNote || friend.FriendCode.Contains(searchTerm, StringComparison.OrdinalIgnoreCase);
    }

    private void HandleFriendsListDeleted(object? sender, FriendsListDeletedEventArgs e) => ResetFocusedFriend();
}
