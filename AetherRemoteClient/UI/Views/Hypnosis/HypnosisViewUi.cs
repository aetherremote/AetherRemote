using System.Numerics;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Services;
using AetherRemoteClient.Utils;
using ImGuiNET;

namespace AetherRemoteClient.UI.Views.Hypnosis;

public class HypnosisViewUi(FriendsListService friendsListService) : IDrawable
{
    private readonly HypnosisViewUiController _controller = new();

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
            SharedUserInterfaces.MediumText("Spiral Preview");
            _controller.RenderPreviewSpiral();
            ImGui.Dummy(_controller.SpiralSize);
            ImGui.EndGroup();
            
            ImGui.SameLine();
            
            ImGui.BeginGroup();
            SharedUserInterfaces.MediumText("Word Bank");
            var size = new Vector2(ImGui.GetWindowWidth() - ImGui.GetCursorPosX() - ImGui.GetStyle().WindowPadding.X, _controller.SpiralSize.Y);
            if (ImGui.InputTextMultiline("##Etche2", ref _controller.PreviewText, 4000, size))
                _controller.UpdateWordBank();
            ImGui.EndGroup();
            
            ImGui.TextUnformatted("Spiral Speed");
            ImGui.SetNextItemWidth(ImGui.GetWindowWidth() - ImGui.GetCursorPosX() - ImGui.GetStyle().WindowPadding.X );
            ImGui.SliderInt("##Etche", ref _controller.SpiralSpeed, 1, 100);
            
            ImGui.ColorEdit4("Spiral Color", ref _controller.SpiralColor);

            ImGui.TextUnformatted("Text Speed");
            ImGui.SetNextItemWidth(ImGui.GetWindowWidth() - ImGui.GetCursorPosX() - ImGui.GetStyle().WindowPadding.X );
            if (ImGui.SliderInt("##Etche222", ref _controller.PreviewTextInterval, 1, 10))
                _controller.UpdatePreviewTestSpeed();
            
            ImGui.ColorEdit4("Text color", ref _controller.PreviewTextColor);
            
            ImGui.TextUnformatted("Text Fade Speed");
            
        });
        
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