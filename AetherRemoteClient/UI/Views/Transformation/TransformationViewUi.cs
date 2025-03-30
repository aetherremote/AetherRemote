using System.Numerics;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Ipc;
using AetherRemoteClient.Services;
using AetherRemoteClient.Utils;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using ImGuiNET;

namespace AetherRemoteClient.UI.Views.Transformation;

public class TransformationViewUi(
    CommandLockoutService commandLockoutService,
    FriendsListService friendsListService,
    NetworkService networkService,
    GlamourerIpc glamourer) : IDrawable
{
    // Const
    private static readonly Vector2 IconSize = new(32);
    
    // Instantiated
    private readonly TransformationViewUiController _controller = new(friendsListService, networkService, glamourer);

    public bool Draw()
    {
        ImGui.BeginChild("TransformationContent", AetherRemoteStyle.ContentSize, false, AetherRemoteStyle.ContentFlags);

        if (friendsListService.Selected.Count is 0)
        {
            SharedUserInterfaces.ContentBox(AetherRemoteStyle.PanelBackground,
                () => { SharedUserInterfaces.TextCentered("You must select at least one friend"); });

            ImGui.EndChild();
            return true;
        }
        
        var windowWidth = ImGui.GetWindowWidth() * 0.5f; // 0.5 is a minor mathematical optimization
        SharedUserInterfaces.ContentBox(AetherRemoteStyle.PanelBackground, () =>
        {
            SharedUserInterfaces.MediumText("Options");

            if (ImGui.Checkbox("Customization", ref _controller.ApplyCustomization))
            {
                if (_controller.ApplyCustomization is false && _controller.ApplyEquipment is false)
                    _controller.ApplyEquipment = true;
            }
            
            ImGui.SameLine(windowWidth);
            if (ImGui.Checkbox("Equipment", ref _controller.ApplyEquipment))
            {
                if (_controller.ApplyCustomization is false && _controller.ApplyEquipment is false)
                    _controller.ApplyCustomization = true;
            }
        });

        SharedUserInterfaces.ContentBox(AetherRemoteStyle.PanelBackground, () =>
        {
            SharedUserInterfaces.MediumText("Quick Actions");

            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.User, IconSize))
                _controller.CopyOwnGlamourer();
            SharedUserInterfaces.Tooltip("Copies your glamourer data into the box below");
            
            ImGui.SameLine();
            
            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Crosshairs, IconSize))
                _controller.CopyTargetGlamourer();
            SharedUserInterfaces.Tooltip("Copies your target's glamourer data into the box below");
            
            ImGui.SameLine();
            
            if(SharedUserInterfaces.IconButton(FontAwesomeIcon.Paste, IconSize))
                _controller.CopyFromClipboard();
            SharedUserInterfaces.Tooltip("Paste glamourer data from your clipboard");
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
            SharedUserInterfaces.MediumText("Glamourer Data");
            var width = (windowWidth - ImGui.GetStyle().WindowPadding.X) * 2;
            ImGui.SetNextItemWidth(width);
            var shouldSendTransform = ImGui.InputTextWithHint("##GlamourerData", "Glamourer data", ref _controller.GlamourerCode, 5000, ImGuiInputTextFlags.EnterReturnsTrue);
            
            ImGui.Spacing();

            if (commandLockoutService.IsLocked)
            {
                ImGui.BeginDisabled();
                ImGui.Button("Transform", new Vector2(width, 0));
                ImGui.EndDisabled();
            }
            else
            {
                if (ImGui.Button("Transform", new Vector2(width, 0)))
                    shouldSendTransform = true;

                if (shouldSendTransform is false)
                    return;
                
                commandLockoutService.Lock();
                _controller.Transform();
            }
        });

        ImGui.EndChild();
        return true;
    }
}