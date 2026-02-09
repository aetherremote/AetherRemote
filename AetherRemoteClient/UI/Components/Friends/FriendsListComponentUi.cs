using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Enums;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Style;
using AetherRemoteClient.Utils;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Colors;

namespace AetherRemoteClient.UI.Components.Friends;

public class FriendsListComponentUi(FriendsListComponentUiController controller, SelectionManager selectionManager)
{
    // Const
    private const SpeakPermissions2 LinkshellMask =
        SpeakPermissions2.Ls1 |
        SpeakPermissions2.Ls2 |
        SpeakPermissions2.Ls3 |
        SpeakPermissions2.Ls4 |
        SpeakPermissions2.Ls5 |
        SpeakPermissions2.Ls6 |
        SpeakPermissions2.Ls7 |
        SpeakPermissions2.Ls8;
    
    private const SpeakPermissions2 CrossWorldLinkshellMask =
        SpeakPermissions2.Cwl1 |
        SpeakPermissions2.Cwl2 |
        SpeakPermissions2.Cwl3 |
        SpeakPermissions2.Cwl4 |
        SpeakPermissions2.Cwl5 |
        SpeakPermissions2.Cwl6 |
        SpeakPermissions2.Cwl7 |
        SpeakPermissions2.Cwl8;
    
    private static readonly PrimaryPermissions2 AllPrimaryPermissionsMask = GetAllFlags<PrimaryPermissions2>();
    private static readonly SpeakPermissions2 AllSpeakPermissionsMask = GetAllFlags<SpeakPermissions2>();
    private static readonly ElevatedPermissions AllElevatedPermissionsMask = GetAllFlags<ElevatedPermissions>();
    
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
            ? AetherRemoteImGui.WindowPadding.Y * 3 + AetherRemoteImGui.FramePadding.Y * 4 + ImGui.GetFontSize() * 2 + AetherRemoteImGui.ItemSpacing.Y * 2
            : 0;

        if (ImGui.BeginChild("###PermissionViewFriendsList", new Vector2(0, -height), true))
        {
            var pending = new List<Friend>();
            var online = new List<Friend>();
            var offline = new List<Friend>();

            foreach (var friend in controller.Filter.List)
            {
                var list = friend.Status switch
                {
                    FriendOnlineStatus.Pending => pending,
                    FriendOnlineStatus.Online => online,
                    _ => offline
                };
                
                list.Add(friend);
            }
            
            if (displayOfflineFriends && pending.Count > 0)
            {
                ImGui.TextColored(ImGuiColors.ParsedPink, "Pending");
                foreach (var friend in pending)
                    RenderSelectionForFriend(friend, width);
            }

            ImGui.TextColored(ImGuiColors.HealerGreen, "Online");
            foreach (var friend in online)
                RenderSelectionForFriend(friend, width);

            if (displayOfflineFriends)
            {
                ImGui.TextColored(ImGuiColors.DalamudRed, "Offline");
                foreach (var friend in offline)
                    RenderSelectionForFriend(friend, width);
            }

            ImGui.EndChild();
        }

        if (displayAddFriendsBox)
        {
            ImGui.Spacing();

            SharedUserInterfaces.ContentBox("AddFriendContentBox", AetherRemoteStyle.PanelBackground, false, () =>
            {
                ImGui.SetNextItemWidth(width);
                ImGui.InputTextWithHint("###AddFriendInputText", "Friend code", ref controller.FriendCodeToAdd, 128);

                if (ImGui.IsItemHovered())
                    ImGui.SetTooltip("Friend codes are case sensitive");
                
                ImGui.Spacing();
                if (ImGui.Button("Add Friend", new Vector2(width, 0)))
                    _ = controller.Add();
            });
        }
        
        ImGui.EndChild();
    }

    /// <summary>
    ///     Horrible naming choice, but it draws the selectable that lets you pick a friend
    /// </summary>
    private void RenderSelectionForFriend(Friend friend, float width)
    {
        if (ImGui.Selectable($"{friend.NoteOrFriendCode}##{friend.FriendCode}", selectionManager.Contains(friend)))
            selectionManager.Select(friend, ImGui.GetIO().KeyCtrl);

        // Only render the following text if hovered
        if (ImGui.IsItemHovered() is false)
            return;
        
        // Account for the scrollbar existing
        ImGui.SameLine(width - (ImGui.GetScrollMaxY() > 0 ? AetherRemoteImGui.WindowPadding.X * 3 : AetherRemoteImGui.WindowPadding.X));
                
        SharedUserInterfaces.Icon(FontAwesomeIcon.Eye);
        
        if (ImGui.IsItemHovered())
        {
            ImGui.SetNextWindowSize(AetherRemoteDimensions.Tooltip);
            
            
            if (friend.PermissionsGrantedByFriend is not { } permissions)
            {
                ImGui.SetTooltip("Your friend has not added you back yet.");
            }
            else
            {
                ImGui.BeginTooltip();
                SharedUserInterfaces.PushMediumFont();
                SharedUserInterfaces.TextCentered("Permissions Granted by Friend");
                SharedUserInterfaces.PopMediumFont();
                DisplayPermissions("Primary permissions", permissions.Primary, PrimaryPermissions2.None, AllPrimaryPermissionsMask);
                DisplayPermissions("Speak permissions", permissions.Speak, SpeakPermissions2.None, AllSpeakPermissionsMask);
                DisplayPermissions("Elevated permissions", permissions.Elevated, ElevatedPermissions.None, AllElevatedPermissionsMask);
                ImGui.EndTooltip();
            }
        }
    }

    private static void DisplayPermissions<T>(string title, T permissions, T none, T all) where T : struct, Enum
    {
        ImGui.TextUnformatted(title);
        ImGui.Separator();

        if (permissions.Equals(none))
        {
            ImGui.BulletText("You do not have permissions in this category");
            return;
        }

        if (permissions.Equals(all))
        {
            ImGui.BulletText("You have all permissions in this category");
            return;
        }

        var ls = new StringBuilder();
        var cwl = new StringBuilder();
        
        foreach (var flag in Enum.GetValues<T>())
        {
            if (flag.Equals(none))
                continue;

            var converted = Convert.ToUInt64(flag);
            if ((Convert.ToUInt64(permissions) & converted) == converted)
            {
                if ((converted & (ulong)LinkshellMask) != 0)
                {
                    ls.Append(flag.ToString()[^1]);
                    ls.Append(", ");
                    continue;
                }
                
                if ((converted & (ulong)CrossWorldLinkshellMask) != 0)
                {
                    cwl.Append(flag.ToString()[^1]);
                    cwl.Append(", ");
                    continue;
                }
                
                ImGui.BulletText(flag.ToString());
            }
        }

        if (ls.Length > 0)
        {
            ls.Remove(ls.Length - 2, 2);
            ImGui.BulletText("Linkshells: " + ls);
        }

        if (cwl.Length > 0)
        {
            cwl.Remove(cwl.Length - 2, 2);
            ImGui.BulletText("CrossWorld-Linkshells: " + cwl);
        }
    }

    private static T GetAllFlags<T>() where T : Enum
    {
        var mask = 0UL;
        foreach (var value in Enum.GetValues(typeof(T)))
            mask |= Convert.ToUInt64(value);
        
        return (T)Enum.ToObject(typeof(T), mask);
    }
}