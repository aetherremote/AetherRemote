using System.Collections.Generic;
using System.Numerics;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteClient.Utils;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Colors;

namespace AetherRemoteClient.UI.Components.Friends;

public class FriendsListComponentUi(FriendsListService friendsListService, NetworkManager networkManager)
{
    private readonly FriendsListComponentUiController _controller = new(friendsListService, networkManager);

    public void Draw(bool displayAddFriendsBox = false, bool displayOfflineFriends = false)
    {
        var style = ImGui.GetStyle();
        var windowPadding = style.WindowPadding;
        var framePadding = style.FramePadding;
        
        var childWidth = new Vector2(AetherRemoteStyle.NavBarDimensions.X - windowPadding.X, 0);
        if (ImGui.BeginChild("FriendListComponentChild", childWidth, false, AetherRemoteStyle.ContentFlags))
        {
            var width = 0f;
            SharedUserInterfaces.ContentBox("FriendsListSearchFriend", AetherRemoteStyle.PanelBackground, true, () =>
            {
                width = ImGui.GetWindowWidth() - ImGui.GetCursorPosX() - windowPadding.X;
                ImGui.TextUnformatted("Search");
                ImGui.SetNextItemWidth(width);
                if (ImGui.InputTextWithHint("###SearchFriendInputText", "Friend", ref _controller.SearchText, 128))
                    _controller.FriendListFilter.UpdateSearchTerm(_controller.SearchText);
            });

            var height = displayAddFriendsBox
                ? windowPadding.Y * 3 + framePadding.Y * 4 + ImGui.GetFontSize() * 2 + ImGui.GetStyle().ItemSpacing.Y
                : 0;
            
            if (ImGui.BeginChild("###PermissionViewFriendsList", new Vector2(0, -height), true))
            {
                var online = new List<Friend>();
                var offline = new List<Friend>();
                
                foreach (var friend in _controller.FriendListFilter.List)
                    (friend.Online ? online : offline).Add(friend);
                
                ImGui.TextColored(ImGuiColors.HealerGreen, "Online");
                foreach (var friend in online)
                {
                    if (ImGui.Selectable($"{friend.NoteOrFriendCode}###{friend.FriendCode}",
                            friendsListService.Selected.Contains(friend)))
                        friendsListService.Select(friend);
                }

                if (displayOfflineFriends)
                {
                    ImGui.TextColored(ImGuiColors.DalamudRed, "Offline");
                    foreach (var friend in offline)
                    {
                        if (ImGui.Selectable($"{friend.NoteOrFriendCode}###{friend.FriendCode}",
                                friendsListService.Selected.Contains(friend)))
                            friendsListService.Select(friend);
                    }
                }
                
                ImGui.EndChild();
            }

            if (displayAddFriendsBox)
            {
                ImGui.Spacing();

                SharedUserInterfaces.ContentBox("FriendsListAddFriend", AetherRemoteStyle.PanelBackground, false, () =>
                {
                    ImGui.SetNextItemWidth(width);
                    ImGui.InputTextWithHint("###AddFriendInputText", "Friend code", ref _controller.FriendCodeToAdd, 128);
                    if (ImGui.Button("Add Friend", new Vector2(width, 0)))
                        _ = _controller.Add();
                });
            }
            
            ImGui.EndChild();
        }
    }
}