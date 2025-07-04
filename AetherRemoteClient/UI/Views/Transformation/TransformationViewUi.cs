using System.Numerics;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Services;
using AetherRemoteClient.UI.Components.Friends;
using AetherRemoteClient.Utils;
using AetherRemoteCommon.Domain.Enums.New;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using ImGuiNET;

namespace AetherRemoteClient.UI.Views.Transformation;

public class TransformationViewUi(
    FriendsListComponentUi friendsList,
    TransformationViewUiController controller,
    CommandLockoutService commandLockoutService,
    FriendsListService friendsListService) : IDrawable
{
    // Const
    private static readonly Vector2 IconSize = new(32);
    
    public void Draw()
    {
        ImGui.BeginChild("TransformationContent", AetherRemoteStyle.ContentSize, false, AetherRemoteStyle.ContentFlags);

        if (friendsListService.Selected.Count is 0)
        {
            SharedUserInterfaces.ContentBox("", AetherRemoteStyle.PanelBackground, true, () =>
            {
                SharedUserInterfaces.TextCentered("You must select at least one friend");
            });

            ImGui.EndChild();
            ImGui.SameLine();
            friendsList.Draw();
            return;
        }
        
        var windowWidth = ImGui.GetWindowWidth() * 0.5f; // 0.5 is a minor mathematical optimization
        SharedUserInterfaces.ContentBox("TransformationOptions", AetherRemoteStyle.PanelBackground, true, () =>
        {
            SharedUserInterfaces.MediumText("Options");

            if (ImGui.Checkbox("Customization", ref controller.ApplyCustomization))
                controller.SelectedApplyTypePermissions ^= PrimaryPermissions2.GlamourerCustomization;
            
            ImGui.SameLine(windowWidth);
            if (ImGui.Checkbox("Equipment", ref controller.ApplyEquipment))
                controller.SelectedApplyTypePermissions ^= PrimaryPermissions2.GlamourerEquipment;
        });

        SharedUserInterfaces.ContentBox("TransformationQuickActions", AetherRemoteStyle.PanelBackground, true, () =>
        {
            SharedUserInterfaces.MediumText("Quick Actions");

            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.User, IconSize))
                controller.CopyOwnGlamourer();
            SharedUserInterfaces.Tooltip("Copies your glamourer data into the box below");
            
            ImGui.SameLine();
            
            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Crosshairs, IconSize))
                controller.CopyTargetGlamourer();
            SharedUserInterfaces.Tooltip("Copies your target's glamourer data into the box below");
            
            ImGui.SameLine();
            
            if(SharedUserInterfaces.IconButton(FontAwesomeIcon.Paste, IconSize))
                controller.CopyFromClipboard();
            SharedUserInterfaces.Tooltip("Paste glamourer data from your clipboard");
        });
        
        var friendsLackingPermissions = controller.GetFriendsLackingPermissions();
        if (friendsLackingPermissions.Count is not 0)
        {
            SharedUserInterfaces.ContentBox("TransformationLackingPermissions", AetherRemoteStyle.PanelBackground, true, () =>
            {
                SharedUserInterfaces.MediumText("Lacking Permissions", ImGuiColors.DalamudYellow);
                ImGui.SameLine();
                ImGui.AlignTextToFramePadding();
                SharedUserInterfaces.Icon(FontAwesomeIcon.ExclamationTriangle, ImGuiColors.DalamudYellow);
                SharedUserInterfaces.Tooltip("Commands send to these people will not be processed");
                ImGui.TextWrapped(string.Join(", ", friendsLackingPermissions));
            });
        }

        if (controller.ApplyCustomization is false && controller.ApplyEquipment is false)
        {
            SharedUserInterfaces.ContentBox("TransformationSelectOptions", AetherRemoteStyle.PanelBackground, true, () =>
            {
                SharedUserInterfaces.MediumText("You must select at least one transformation option", ImGuiColors.DalamudYellow);
            });
        }

        SharedUserInterfaces.ContentBox("TransformationSend", AetherRemoteStyle.PanelBackground, false, () =>
        {
            SharedUserInterfaces.MediumText("Glamourer Data");
            var width = (windowWidth - ImGui.GetStyle().WindowPadding.X) * 2;
            ImGui.SetNextItemWidth(width);
            var shouldSendTransform = ImGui.InputTextWithHint("##GlamourerData", "Glamourer data", ref controller.GlamourerCode, 5000, ImGuiInputTextFlags.EnterReturnsTrue);
            
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
                controller.Transform();
            }
        });

        ImGui.EndChild();
        ImGui.SameLine();
        friendsList.Draw();
    }
}