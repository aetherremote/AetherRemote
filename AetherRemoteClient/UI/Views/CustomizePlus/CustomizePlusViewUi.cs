using System.Numerics;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Services;
using AetherRemoteClient.Utils;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using ImGuiNET;

namespace AetherRemoteClient.UI.Views.CustomizePlus;

public class CustomizePlusViewUi(
    CommandLockoutService commandLockoutService,
    FriendsListService friendsListService,
    NetworkService networkService) : IDrawable
{
    // Const
    private static readonly Vector2 IconSize = new(32);
    
    // Instantiated
    private readonly CustomizePlusViewUiController _controller = new(friendsListService, networkService);
    
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
        
        var windowWidthHalf = ImGui.GetWindowWidth() * 0.5f;
        SharedUserInterfaces.ContentBox(AetherRemoteStyle.PanelBackground, () =>
        {
            SharedUserInterfaces.MediumText("Saved Templates");
            ImGui.TextColored(ImGuiColors.DalamudGrey, "Coming in a future update");
        });

        SharedUserInterfaces.ContentBox(AetherRemoteStyle.PanelBackground, () =>
        {
            SharedUserInterfaces.MediumText("Quick Actions");
            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Paste, IconSize))
                _controller.Customize = ImGui.GetClipboardText();
            SharedUserInterfaces.Tooltip("Paste Customize+ data from clipboard");
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
            SharedUserInterfaces.MediumText("Customize Data");

            var width = (windowWidthHalf - ImGui.GetStyle().WindowPadding.X) * 2;
            ImGui.SetNextItemWidth(width);
            var shouldSendCustomize = ImGui.InputTextWithHint("##CustomizeData", "Customize data", ref _controller.Customize, 5000,
                ImGuiInputTextFlags.EnterReturnsTrue);

            ImGui.Spacing();

            if (commandLockoutService.IsLocked)
            {
                ImGui.BeginDisabled();
                ImGui.Button("Apply Customize", new Vector2(width, 0));
                ImGui.EndDisabled();
            }
            else
            {
                if (ImGui.Button("Apply Customize", new Vector2(width, 0)))
                    shouldSendCustomize = true;

                if (shouldSendCustomize is false)
                    return;

                commandLockoutService.Lock();
                _controller.SendCustomize();
            }
        });
        
        ImGui.EndChild();
        return true;
    }
}