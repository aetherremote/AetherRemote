using System;
using System.Numerics;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Services;
using AetherRemoteClient.Utils;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using ImGuiNET;

namespace AetherRemoteClient.UI.Views.Hypnosis;

public class HypnosisViewUi(FriendsListService friendsListService, NetworkService networkService, SpiralService spiralService) : IDrawable
{
    private readonly HypnosisViewUiController _controller = new(friendsListService, networkService, spiralService);

    public bool Draw()
    {
        ImGui.BeginChild("HypnosisContent", AetherRemoteStyle.ContentSize, false, AetherRemoteStyle.ContentFlags);
        
        if (friendsListService.Selected.Count is 0)
        {
            SharedUserInterfaces.ContentBox(AetherRemoteStyle.PanelBackground,
                () => { SharedUserInterfaces.TextCentered("You must select at least one friend"); });

            ImGui.EndChild();
            return true;
        }
        
        SharedUserInterfaces.ContentBox(AetherRemoteStyle.PanelBackground, () =>
        {
            ImGui.BeginGroup();
            SharedUserInterfaces.MediumText("Preview");
            _controller.RenderPreviewSpiral();
            ImGui.Dummy(HypnosisViewUiController.SpiralSize);
            ImGui.EndGroup();
            
            ImGui.SameLine();
            
            ImGui.BeginGroup();
            SharedUserInterfaces.MediumText("Text");
            var x = ImGui.GetWindowWidth() - ImGui.GetCursorPosX() - ImGui.GetStyle().WindowPadding.X;
            var y = HypnosisViewUiController.SpiralSize.Y - ImGui.GetFontSize() - ImGui.GetStyle().WindowPadding.X;
            
            var size = new Vector2(x, y);
            if (ImGui.InputTextMultiline("##Etche2", ref _controller.PreviewText, 4000, size))
                _controller.UpdateWordBank();
            
            ImGui.RadioButton("Random", ref _controller.PreviewTextMode, 0);
            ImGui.SameLine();
            ImGui.RadioButton("In Order", ref _controller.PreviewTextMode, 1);
            ImGui.EndGroup();
            
            ImGui.TextUnformatted("Spiral Speed");
            ImGui.SetNextItemWidth(ImGui.GetWindowWidth() - ImGui.GetCursorPosX() - ImGui.GetStyle().WindowPadding.X );
            ImGui.SliderInt("##Etche", ref _controller.SpiralSpeed, 0, 100);
            
            ImGui.ColorEdit4("Spiral Color", ref _controller.SpiralColor);
            
            ImGui.TextUnformatted("Text Speed");
            ImGui.SetNextItemWidth(ImGui.GetWindowWidth() - ImGui.GetCursorPosX() - ImGui.GetStyle().WindowPadding.X);
            if (ImGui.SliderInt("##Etche222", ref _controller.PreviewTextInterval, 1, 10))
                _controller.UpdatePreviewTestSpeed();
            
            
            ImGui.ColorEdit4("Text color", ref _controller.PreviewTextColor);
            
            ImGui.TextUnformatted("Spiral Duration");
            
            
            ImGui.SetNextItemWidth(120);
            if (ImGui.InputInt("Minutes", ref _controller.SpiralDuration, 5))
                _controller.SpiralDuration = Math.Max(0, _controller.SpiralDuration);
            SharedUserInterfaces.Tooltip("0 Minutes for indefinitely");
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
            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Eye, new Vector2(40), "Preview, disable with /ar stop or in the status tab"))
                _controller.PreviewSpiral();
            ImGui.SameLine();
            if (ImGui.Button("Hypnotize",
                    new Vector2(ImGui.GetWindowWidth() - ImGui.GetCursorPosX() - ImGui.GetStyle().WindowPadding.X, 40)))
                _controller.SendSpiral();
        }, true, false);
        
        if (ImGui.BeginPopup("CR"))
        {
            ImGui.ColorPicker4("Spiral Color", ref _controller.SpiralColor);
            ImGui.EndPopup();
        }

        if (ImGui.BeginPopup("AR"))
        {
            ImGui.ColorPicker4("Text Color", ref _controller.PreviewTextColor);
            ImGui.EndPopup();
        }
        
        ImGui.EndChild();
        return true;
    }
}