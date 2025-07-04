using System.Numerics;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Services;
using AetherRemoteClient.UI.Components.Friends;
using AetherRemoteClient.Utils;
using AetherRemoteCommon.Domain.Enums.Permissions;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using ImGuiNET;

namespace AetherRemoteClient.UI.Views.Twinning;

public class TwinningViewUi(
    FriendsListComponentUi friendsList,
    TwinningViewUiController controller,
    CommandLockoutService commandLockoutService,
    FriendsListService friendsListService): IDrawable
{
    public void Draw()
    {
        ImGui.BeginChild("TwinningContent", AetherRemoteStyle.ContentSize, false, AetherRemoteStyle.ContentFlags);
        
        if (friendsListService.Selected.Count is 0)
        {
            SharedUserInterfaces.ContentBox("TwinningSelectMoreFriends", AetherRemoteStyle.PanelBackground, true, () =>
            {
                SharedUserInterfaces.TextCentered("You must select at least one friend");
            });

            ImGui.EndChild();
            ImGui.SameLine();
            friendsList.Draw();
            return;
        }
        
        SharedUserInterfaces.ContentBox("TwinningInfo", AetherRemoteStyle.PanelBackground, true, () =>
        {
            SharedUserInterfaces.MediumText("Information");
            ImGui.TextWrapped(
                "Twinning must be done in rendering distance of your target(s). You can undo a twinning on yourself by going into the 'Status' tab, or reverting your character in glamourer. ");
        });
        
        var half = ImGui.GetWindowWidth() * 0.5f;
        SharedUserInterfaces.ContentBox("TwinningOptions", AetherRemoteStyle.PanelBackground, true, () =>
        {
            SharedUserInterfaces.MediumText("Options");
            if (ImGui.Checkbox("Swap Mods", ref controller.SwapMods))
                controller.SelectedAttributesPermissions ^= PrimaryPermissions2.Mods;
            
            ImGui.SameLine();
            ImGui.SetCursorPosX(half);
            if (ImGui.Checkbox("Swap Moodles", ref controller.SwapMoodles))
                controller.SelectedAttributesPermissions ^= PrimaryPermissions2.Moodles;
            
            ImGui.Spacing();
            
            if(ImGui.Checkbox("Swap Customize+", ref controller.SwapCustomizePlus))
                controller.SelectedAttributesPermissions ^= PrimaryPermissions2.CustomizePlus;
        });
        
        var friendsLackingPermissions = controller.GetFriendsLackingPermissions();
        if (friendsLackingPermissions.Count is not 0)
        {
            SharedUserInterfaces.ContentBox("TwinningLackingPermissions", AetherRemoteStyle.PanelBackground, true, () =>
            {
                SharedUserInterfaces.MediumText("Lacking Permissions", ImGuiColors.DalamudYellow);
                ImGui.SameLine();
                ImGui.AlignTextToFramePadding();
                SharedUserInterfaces.Icon(FontAwesomeIcon.ExclamationTriangle, ImGuiColors.DalamudYellow);
                SharedUserInterfaces.Tooltip("Commands send to these people will not be processed");
                ImGui.TextWrapped(string.Join(", ", friendsLackingPermissions));
            });
        }
        
        SharedUserInterfaces.ContentBox("TwinningSend", AetherRemoteStyle.PanelBackground, false, () =>
        {
            SharedUserInterfaces.MediumText("Twinning");

            var width = new Vector2(ImGui.GetWindowWidth() - ImGui.GetStyle().WindowPadding.X * 2, 0);
            if (commandLockoutService.IsLocked)
            {
                ImGui.BeginDisabled();
                ImGui.Button("Twin", width);
                ImGui.EndDisabled();
            }
            else
            {
                if (ImGui.Button("Twin", width) is false)
                    return;
                
                commandLockoutService.Lock();
                controller.Twin();
            }
        });
        
        ImGui.EndChild();
        ImGui.SameLine();
        friendsList.Draw();
    }
}