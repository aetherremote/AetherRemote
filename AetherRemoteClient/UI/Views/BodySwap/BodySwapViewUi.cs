using System.Numerics;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Services;
using AetherRemoteClient.UI.Components.Friends;
using AetherRemoteClient.Utils;
using AetherRemoteCommon.Domain.Enums.New;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using ImGuiNET;

namespace AetherRemoteClient.UI.Views.BodySwap;

public class BodySwapViewUi(
    FriendsListComponentUi friendsList,
    BodySwapViewUiController controller,
    CommandLockoutService commandLockoutService,
    FriendsListService friendsListService) : IDrawable
{
    public void Draw()
    {
        ImGui.BeginChild("BodySwapContent", AetherRemoteStyle.ContentSize, false, AetherRemoteStyle.ContentFlags);

        if (friendsListService.Selected.Count is 0)
        {
            SharedUserInterfaces.ContentBox("BodySwapSelectMoreFriends", AetherRemoteStyle.PanelBackground, true, () =>
            {
                SharedUserInterfaces.TextCentered("You must select at least one friend");
            });

            ImGui.EndChild();
            ImGui.SameLine();
            friendsList.Draw();
            return;
        }
        
        SharedUserInterfaces.ContentBox("BodySwapInfo", AetherRemoteStyle.PanelBackground, true, () =>
        {
            ImGui.TextWrapped("Body swapping must be done in rendering distance of your target(s). You can undo a body swap on yourself by going into the 'Status' tab, or reverting your character in glamourer.");
        });

        var half = ImGui.GetWindowWidth() * 0.5f;
        SharedUserInterfaces.ContentBox("BodySwapOptions", AetherRemoteStyle.PanelBackground, true, () =>
        {
            SharedUserInterfaces.MediumText("Options");
            if (ImGui.Checkbox("Swap Mods", ref controller.SwapMods))
                controller.SelectedAttributesPermissions ^= PrimaryPermissions2.Mods;

            ImGui.SameLine();
            ImGui.SetCursorPosX(half);
            if (ImGui.Checkbox("Swap Moodles", ref controller.SwapMoodles))
                controller.SelectedAttributesPermissions ^= PrimaryPermissions2.Moodles;

            ImGui.Spacing();
            if (ImGui.Checkbox("Swap Customize+", ref controller.SwapCustomizePlus))
                controller.SelectedAttributesPermissions ^= PrimaryPermissions2.CustomizePlus;

            ImGui.Spacing();

            ImGui.Checkbox("Include Self", ref controller.IncludeSelfInSwap);
            SharedUserInterfaces.Tooltip("Include yourself in the targets to body swap");
        });

        var friendsLackingPermissions = controller.GetFriendsLackingPermissions();
        if (friendsLackingPermissions.Count is not 0)
        {
            SharedUserInterfaces.ContentBox("BodySwapLackingPermissions", AetherRemoteStyle.PanelBackground, true, () =>
            {
                SharedUserInterfaces.MediumText("Lacking Permissions", ImGuiColors.DalamudYellow);
                ImGui.SameLine();
                ImGui.AlignTextToFramePadding();
                SharedUserInterfaces.Icon(FontAwesomeIcon.ExclamationTriangle, ImGuiColors.DalamudYellow);
                SharedUserInterfaces.Tooltip("Commands send to these people will not be processed");
                ImGui.TextWrapped(string.Join(", ", friendsLackingPermissions));
            });
        }

        SharedUserInterfaces.ContentBox("BodySwapSend", AetherRemoteStyle.PanelBackground, false, () =>
        {
            SharedUserInterfaces.MediumText("Body Swap");

            var width = new Vector2(ImGui.GetWindowWidth() - ImGui.GetStyle().WindowPadding.X * 2, 0);
            if (commandLockoutService.IsLocked)
            {
                ImGui.BeginDisabled();
                ImGui.Button("Swap", width);
                ImGui.EndDisabled();
            }
            else
            {
                if (ImGui.Button("Swap", width) is false)
                    return;

                commandLockoutService.Lock();
                controller.Swap();
            }
        });

        ImGui.EndChild();
        ImGui.SameLine();
        friendsList.Draw();
    }
}