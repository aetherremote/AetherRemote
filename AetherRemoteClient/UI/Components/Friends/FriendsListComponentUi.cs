using System.Collections.Generic;
using System.Numerics;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Enums;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Style;
using AetherRemoteClient.Utils;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Colors;

// ReSharper disable once RedundantBoolCompare

namespace AetherRemoteClient.UI.Components.Friends;

public class FriendsListComponentUi(FriendsListComponentUiController controller, SelectionManager selectionManager)
{
    public void Draw(bool displayAddFriendsBox = false, bool displayOfflineFriends = false)
    {
        if (ImGui.BeginChild("FriendsListComponent", new Vector2(AetherRemoteDimensions.NavBar.X - AetherRemoteImGui.WindowPadding.X, 0), false, AetherRemoteStyle.ContentFlags) is false)
        {
            ImGui.EndChild();
            return;
        }
        
        var width = ImGui.GetWindowWidth() - ImGui.GetCursorPosX() - AetherRemoteImGui.WindowPadding.X * 2;
        
        SharedUserInterfaces.ContentBox("FriendsListSearch", AetherRemoteStyle.PanelBackground, true, () =>
        {
            ImGui.TextUnformatted("Search");
            ImGui.SetNextItemWidth(width - AetherRemoteImGui.WindowPadding.X - AetherRemoteDimensions.IconButton.X);
            if (ImGui.InputTextWithHint("###SearchFriendInputText", "Friend", ref controller.SearchText, 24))
                controller.Filter.UpdateSearchTerm(controller.SearchText);

            ImGui.SameLine();

            if (controller.Filter.SortMode is FilterSortMode.Alphabetically)
            {
                if (SharedUserInterfaces.IconButton(FontAwesomeIcon.SortAlphaDown, AetherRemoteDimensions.IconButton, "Filtering by Alphabetical"))
                    controller.ToggleSortMode();
            }
            else
            {
                if (SharedUserInterfaces.IconButton(FontAwesomeIcon.SortAmountUp, AetherRemoteDimensions.IconButton, "Filtering by most recent interaction"))
                    controller.ToggleSortMode();
            }
        });

        var height = displayAddFriendsBox
            ? AetherRemoteImGui.WindowPadding.Y * 3 + AetherRemoteImGui.FramePadding.Y * 4 + ImGui.GetFontSize() * 2 + AetherRemoteImGui.ItemSpacing.Y
            : 0;

        if (ImGui.BeginChild("###PermissionViewFriendsList", new Vector2(0, -height), true))
        {
            var online = new List<Friend>();
            var offline = new List<Friend>();

            foreach (var friend in controller.Filter.List)
                (friend.Online ? online : offline).Add(friend);

            ImGui.TextColored(ImGuiColors.HealerGreen, "Online");
            foreach (var friend in online)
            {
                if (ImGui.Selectable($"{friend.NoteOrFriendCode}##{friend.FriendCode}", selectionManager.Contains(friend)))
                    selectionManager.Select(friend, ImGui.GetIO().KeyCtrl);
            }

            if (displayOfflineFriends)
            {
                ImGui.TextColored(ImGuiColors.DalamudRed, "Offline");
                foreach (var friend in offline)
                {
                    if (ImGui.Selectable($"{friend.NoteOrFriendCode}###{friend.FriendCode}", selectionManager.Contains(friend)))
                        selectionManager.Select(friend, ImGui.GetIO().KeyCtrl);
                }
            }

            ImGui.EndChild();
        }

        if (displayAddFriendsBox)
        {
            ImGui.Spacing();

            SharedUserInterfaces.ContentBox("whgdwuih", AetherRemoteStyle.PanelBackground, false, () =>
            {
                ImGui.SetNextItemWidth(width);
                ImGui.InputTextWithHint("###AddFriendInputText", "Friend code", ref controller.FriendCodeToAdd, 128);
                if (ImGui.Button("Add Friend", new Vector2(width, 0)))
                    _ = controller.Add();
            });
        }
        
        ImGui.EndChild();
    }
}