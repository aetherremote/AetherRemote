using System.Numerics;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Services;
using AetherRemoteClient.Utils;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using ImGuiNET;

namespace AetherRemoteClient.UI.Views.Twinning;

public class TwinningViewUi(
    CommandLockoutService commandLockoutService,
    FriendsListService friendsListService,
    IdentityService identityService,
    NetworkService networkService): IDrawable
{
    private readonly TwinningViewUiController _controller = new(friendsListService, identityService, networkService);
    
    public bool Draw()
    {
        ImGui.BeginChild("TwinningContent", AetherRemoteStyle.ContentSize, false, AetherRemoteStyle.ContentFlags);
        
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
                "Twinning must be done in rendering distance of your target(s). You can undo a twinning on yourself by going into the 'Status' tab, or reverting your character in glamourer. ");
        });
        
        SharedUserInterfaces.ContentBox(AetherRemoteStyle.PanelBackground, () =>
        {
            SharedUserInterfaces.MediumText("Options");
            ImGui.Checkbox("Swap Mods", ref _controller.SwapMods);
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
                _controller.Twin();
            }
        });
        
        ImGui.EndChild();
        return true;
    }
}