using System.Numerics;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteClient.Utils;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using ImGuiNET;

namespace AetherRemoteClient.UI.Views.BodySwap;

public class BodySwapViewUi(
    CommandLockoutService commandLockoutService,
    IdentityService identityService,
    FriendsListService friendsListService,
    NetworkService networkService,
    ModManager modManager) : IDrawable
{
    private readonly BodySwapViewUiController _controller = new(identityService, friendsListService, networkService,
        modManager);

    public bool Draw()
    {
        ImGui.BeginChild("BodySwapContent", AetherRemoteStyle.ContentSize, false, AetherRemoteStyle.ContentFlags);
        
        if (friendsListService.Selected.Count is 0)
        {
            SharedUserInterfaces.ContentBox(AetherRemoteStyle.PanelBackground,
                () => { SharedUserInterfaces.TextCentered("You must select at least one friend"); });

            ImGui.EndChild();
            return true;
        }

        SharedUserInterfaces.ContentBox(AetherRemoteStyle.PanelBackground, () =>
        {
            SharedUserInterfaces.MediumText("Information");
            ImGui.TextWrapped(
                "Body swapping must be done in rendering distance of your target(s). You can undo a body swap on yourself by going into the 'Status' tab, or reverting your character in glamourer. ");
        });

        var half = ImGui.GetWindowWidth() * 0.5f;
        SharedUserInterfaces.ContentBox(AetherRemoteStyle.PanelBackground, () =>
        {
            SharedUserInterfaces.MediumText("Options");
            ImGui.Checkbox("Swap Mods", ref _controller.SwapMods);
            
            ImGui.SameLine();
            ImGui.SetCursorPosX(half);
            ImGui.Checkbox("Swap Moodles", ref _controller.SwapMoodles);
            
            ImGui.Spacing();
            
            ImGui.Checkbox("Include Self", ref _controller.IncludeSelfInSwap);
            SharedUserInterfaces.Tooltip("Include yourself in the targets to body swap");
        });

        var friendsLackingPermissions = _controller.GetFriendsLackingPermissions();
        if (friendsLackingPermissions.Count is not 0)
        {
            SharedUserInterfaces.ContentBox(AetherRemoteStyle.PanelBackground, () =>
            {
                SharedUserInterfaces.MediumText("Lacking Permissions", ImGuiColors.DalamudYellow);
                ImGui.SameLine();
                ImGui.AlignTextToFramePadding();
                SharedUserInterfaces.Icon(FontAwesomeIcon.ExclamationTriangle, ImGuiColors.DalamudYellow);
                SharedUserInterfaces.Tooltip("Commands send to these people will not be processed");
                ImGui.TextWrapped(string.Join(", ", friendsLackingPermissions));
            });
        }

        SharedUserInterfaces.ContentBox(AetherRemoteStyle.PanelBackground, () =>
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
                _controller.Swap();
            }
        });

        ImGui.EndChild();
        return true;
    }
}