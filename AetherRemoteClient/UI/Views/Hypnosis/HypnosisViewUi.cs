using System;
using System.Numerics;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Services;
using AetherRemoteClient.UI.Components.Friends;
using AetherRemoteClient.Utils;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Colors;

namespace AetherRemoteClient.UI.Views.Hypnosis;

public class HypnosisViewUi(
    FriendsListComponentUi friendsList,
    HypnosisViewUiController controller,
    CommandLockoutService commandLockoutService,
    FriendsListService friendsListService) : IDrawable
{
    private static readonly Vector2 IconSize = new(40);
    
    public void Draw()
    {
        ImGui.BeginChild("HypnosisContent", AetherRemoteStyle.ContentSize, false, AetherRemoteStyle.ContentFlags);

        var width = ImGui.GetWindowWidth();
        var windowPadding = ImGui.GetStyle().WindowPadding;
        
        if (friendsListService.Selected.Count is 0)
        {
            SharedUserInterfaces.ContentBox("HypnosisSelectMoreFriends", AetherRemoteStyle.PanelBackground, true, () =>
            {
                SharedUserInterfaces.TextCentered("You must select at least one friend");
            });

            ImGui.EndChild();
            ImGui.SameLine();
            friendsList.Draw();
            return;
        }
        
        SharedUserInterfaces.ContentBox("HypnosisPreview", AetherRemoteStyle.PanelBackground, true, () =>
        {
            ImGui.BeginGroup();
            SharedUserInterfaces.MediumText("Preview");
            controller.RenderPreviewSpiral();
            ImGui.Dummy(HypnosisViewUiController.SpiralSize);
            ImGui.EndGroup();
            
            ImGui.SameLine();
            
            ImGui.BeginGroup();
            SharedUserInterfaces.MediumText("Text");
            var x = width - ImGui.GetCursorPosX() - windowPadding.X;
            var y = HypnosisViewUiController.SpiralSize.Y - ImGui.GetFontSize() - windowPadding.X;
            
            var size = new Vector2(x, y);
            if (ImGui.InputTextMultiline("##PreviewText", ref controller.PreviewText, 4000, size))
                controller.UpdateWordBank();
            
            ImGui.RadioButton("Random", ref controller.PreviewTextMode, 0);
            ImGui.SameLine();
            ImGui.RadioButton("In Order", ref controller.PreviewTextMode, 1);
            ImGui.EndGroup();
            
            ImGui.TextUnformatted("Spiral Speed");
            ImGui.SetNextItemWidth(width - ImGui.GetCursorPosX() - windowPadding.X);
            ImGui.SliderInt("##SpiralSpeed", ref controller.SpiralSpeed, 0, 100);
            
            ImGui.ColorEdit4("Spiral Color", ref controller.SpiralColor);
            
            ImGui.TextUnformatted("Text Speed");
            ImGui.SetNextItemWidth(width - ImGui.GetCursorPosX() - windowPadding.X);
            if (ImGui.SliderInt("##TextSpeed", ref controller.PreviewTextInterval, 1, 10))
                controller.UpdatePreviewTestSpeed();
            
            ImGui.ColorEdit4("Text color", ref controller.PreviewTextColor);
            ImGui.TextUnformatted("Spiral Duration");
            
            ImGui.SetNextItemWidth(120);
            if (ImGui.InputInt("Minutes", ref controller.SpiralDuration, 5))
                controller.SpiralDuration = Math.Max(0, controller.SpiralDuration);
            SharedUserInterfaces.Tooltip("0 Minutes for indefinitely");
        });
        
        var friendsLackingPermissions = controller.GetFriendsLackingPermissions();
        if (friendsLackingPermissions.Count is not 0)
        {
            SharedUserInterfaces.ContentBox("HypnosisLackingPermissions", AetherRemoteStyle.PanelBackground, true, () =>
            {
                SharedUserInterfaces.MediumText("Lacking Permissions", ImGuiColors.DalamudYellow);
                ImGui.SameLine();
                ImGui.AlignTextToFramePadding();
                SharedUserInterfaces.Icon(FontAwesomeIcon.ExclamationTriangle, ImGuiColors.DalamudYellow);
                SharedUserInterfaces.Tooltip("Commands send to these people will not be processed");
                ImGui.TextWrapped(string.Join(", ", friendsLackingPermissions));
            });
        }
        
        SharedUserInterfaces.ContentBox("HypnosisSend", AetherRemoteStyle.PanelBackground, false, () =>
        {
            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Eye, IconSize, "Preview, disable with /ar stop or in the status tab"))
                controller.PreviewSpiral();
            ImGui.SameLine();
            
            var size = new Vector2(width - ImGui.GetCursorPosX() - windowPadding.X, 40);
            if (commandLockoutService.IsLocked)
            {
                ImGui.BeginDisabled();
                ImGui.Button("Hypnotize", size);
                ImGui.EndDisabled();
            }
            else
            {
                if (ImGui.Button("Hypnotize", size) is false)
                    return;

                commandLockoutService.Lock();
                controller.SendSpiral();
            }
        });
        
        if (ImGui.BeginPopup("CR"))
        {
            ImGui.ColorPicker4("Spiral Color", ref controller.SpiralColor);
            ImGui.EndPopup();
        }

        if (ImGui.BeginPopup("AR"))
        {
            ImGui.ColorPicker4("Text Color", ref controller.PreviewTextColor);
            ImGui.EndPopup();
        }
        
        ImGui.EndChild();
        ImGui.SameLine();
        friendsList.Draw();
    }
}