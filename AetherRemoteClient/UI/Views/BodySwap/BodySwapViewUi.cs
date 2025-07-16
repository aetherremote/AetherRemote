using System.Numerics;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Services;
using AetherRemoteClient.UI.Components.Friends;
using AetherRemoteClient.Utils;
using AetherRemoteCommon.Domain.Enums.Permissions;
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
    // Const
    private static readonly Vector2 IconSize = new(32);
    
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
        
        SharedUserInterfaces.ContentBox("BodySwapOptions", AetherRemoteStyle.PanelBackground, true, () =>
        {
            SharedUserInterfaces.MediumText("Options");
            
            SharedUserInterfaces.IconOptionButton(FontAwesomeIcon.User, IconSize, "Customization - Always Enabled", true);
            
            ImGui.SameLine();
            SharedUserInterfaces.IconOptionButton(FontAwesomeIcon.Tshirt, IconSize, "Equipment - Always Enabled", true);
            
            ImGui.SameLine();
            if (SharedUserInterfaces.IconOptionButton(FontAwesomeIcon.Wrench, IconSize, "Mods", controller.SwapMods))
            {
                controller.SwapMods = !controller.SwapMods;
                controller.SelectedAttributesPermissions ^= PrimaryPermissions2.Mods;
            }
            
            ImGui.SameLine();
            if (SharedUserInterfaces.IconOptionButton(FontAwesomeIcon.Icons, IconSize, "Moodles", controller.SwapMoodles))
            {
                controller.SwapMoodles = !controller.SwapMoodles;
                controller.SelectedAttributesPermissions ^= PrimaryPermissions2.Moodles;
            }
            
            ImGui.SameLine();
            if (SharedUserInterfaces.IconOptionButton(FontAwesomeIcon.Plus, IconSize, "Customize Plus", controller.SwapCustomizePlus))
            {
                controller.SwapCustomizePlus = !controller.SwapCustomizePlus;
                controller.SelectedAttributesPermissions ^= PrimaryPermissions2.CustomizePlus;
            }

            ImGui.Checkbox("Include Self", ref controller.IncludeSelfInSwap);
            SharedUserInterfaces.Tooltip("Include yourself in the targets to body swap");
        });
        
        var windowWidth = ImGui.GetWindowWidth() * 0.5f; // 0.5 is a minor mathematical optimization
        if (controller.AllSelectedTargetsHaveElevatedPermissions())
            SharedUserInterfaces.ContentBox("TransformationElevatedPermissions", AetherRemoteStyle.ElevatedBackground, true, () =>
            {
                SharedUserInterfaces.MediumText("Permanent Transformation");
                ImGui.Checkbox("Enable", ref controller.PermanentTransformation);
                if (controller.PermanentTransformation is false)
                    return;
                
                ImGui.SameLine(windowWidth);
                ImGui.SetNextItemWidth(ImGui.GetFontSize() * 4);
                ImGui.InputText("Pin", ref controller.UnlockPin, 4);
                SharedUserInterfaces.Tooltip(
                [
                    "Your targets can use this PIN to unlock their appearance later if you provide it to them",
                    "They can unlock it from the Status tab or by using the safeword command or safe mode"
                ]);
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