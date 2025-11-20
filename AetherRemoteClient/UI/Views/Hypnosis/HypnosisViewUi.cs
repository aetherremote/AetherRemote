using System;
using System.Numerics;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteClient.UI.Components.Friends;
using AetherRemoteClient.Utils;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;

namespace AetherRemoteClient.UI.Views.Hypnosis;

public class HypnosisViewUi(
    FriendsListComponentUi friendsList,
    HypnosisViewUiController controller,
    CommandLockoutService commandLockoutService,
    SelectionManager selectionManager) : IDrawable
{
    // Const
    private static readonly Vector2 IconSize = new(40);
    
    // Preview window controls
    private bool _showPreviewWindow;
    
    public void Draw()
    {
        ImGui.BeginChild("HypnosisContent", AetherRemoteStyle.ContentSize, false, AetherRemoteStyle.ContentFlags);
        
        if (selectionManager.Selected.Count is 0)
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
        
        var width = ImGui.GetWindowWidth();
        var halfWidth = width * 0.5f;
        var padding = ImGui.GetStyle().WindowPadding.X;
        var fontSize = ImGui.GetFontSize();
        var itemWidth = (width - padding * 3) * 0.5f;
        
        SharedUserInterfaces.ContentBox("HypnosisLoadSpiral", AetherRemoteStyle.PanelBackground, true, () =>
        {
            SharedUserInterfaces.MediumText("Spirals");
            SharedUserInterfaces.ComboWithFilter("##LoadSpiralInputText", "Name", ref controller.SaveLoadSpiralSearchText, width - padding * 8 - fontSize * 3, controller.SaveLoadSpiralFileOptionsListFilter);
            
            ImGui.SameLine();
            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Save, null, "Save"))
                controller.SaveHypnosisProfileToDisk();
            
            ImGui.SameLine();
            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.FileImport, null, "Load"))
                controller.LoadHypnosisProfileFromDisk();
            
            ImGui.SameLine();
            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Trash, null, "Delete (Hold Alt)") && (ImGui.IsKeyDown(ImGuiKey.RightAlt) || ImGui.IsKeyDown(ImGuiKey.LeftAlt)))
                controller.DeleteHypnosisProfileFromDisk();
            
            ImGui.Spacing();
            
            var importExportButtonWidth = new Vector2(width * 0.5f - padding * 1.5f, 0);
            if(ImGui.Button("Export to clipboard", importExportButtonWidth))
                controller.ExportToClipboard();
            ImGui.SameLine();
            if(ImGui.Button("Import from clipboard", importExportButtonWidth))
                controller.ImportFromClipboard();
        });
        
        SharedUserInterfaces.ContentBox("HypnosisSpiralConfiguration", AetherRemoteStyle.PanelBackground, true, () =>
        {
            SharedUserInterfaces.MediumText("Spiral Configuration");
            
            ImGui.TextUnformatted("Arms");
            ImGui.SameLine(halfWidth);
            ImGui.TextUnformatted("Turns");
            
            ImGui.SetNextItemWidth(itemWidth);
            if (ImGui.SliderInt("##SpiralArms", ref controller.SpiralArms, 1, 5))
                controller.BeginSpiralRefreshTimer();
            
            ImGui.SameLine();
            
            ImGui.SetNextItemWidth(itemWidth);
            if (ImGui.SliderInt("##SpiralTurns", ref controller.SpiralTurns, 1, 10))
                controller.BeginSpiralRefreshTimer();
            
            ImGui.TextUnformatted("Curvature");
            ImGui.SameLine(halfWidth);
            ImGui.TextUnformatted("Thickness");
            
            ImGui.SetNextItemWidth(itemWidth);
            if (ImGui.SliderInt("##SpiralCurve", ref controller.SpiralCurve, 1, 10))
                controller.BeginSpiralRefreshTimer();
            
            ImGui.SameLine();
            
            ImGui.SetNextItemWidth(itemWidth);
            if (ImGui.SliderInt("##SpiralThickness", ref controller.SpiralThickness, 1, 10))
                controller.BeginSpiralRefreshTimer();

            ImGui.TextUnformatted("Speed");
            ImGui.SameLine(halfWidth);
            ImGui.TextUnformatted("Color");
            
            ImGui.SetNextItemWidth(itemWidth);
            if (ImGui.SliderInt("##SpiralSpeed", ref controller.SpiralSpeed, 0, 10))
                controller.SetSpeed();
            
            ImGui.SameLine();
            
            ImGui.SetNextItemWidth(itemWidth);
            if (ImGui.ColorEdit4("##SpiralColor", ref controller.SpiralColor))
                controller.SetColorSpiral();
            
            ImGui.TextUnformatted("Direction");
            if (ImGui.RadioButton("Inward", ref controller.SpiralDirection, 0))
                controller.SetDirection();            
            ImGui.SameLine();
            
            if (ImGui.RadioButton("Outward", ref controller.SpiralDirection, 1))
                controller.SetDirection();  
        });
        
        SharedUserInterfaces.ContentBox("HypnosisTextConfiguration", AetherRemoteStyle.PanelBackground, true, () =>
        {
            SharedUserInterfaces.MediumText("Spiral Text Configuration");
            
            ImGui.TextUnformatted("Delay");
            ImGui.SameLine(halfWidth);
            ImGui.TextUnformatted("Duration");
            
            ImGui.SetNextItemWidth(itemWidth);
            if (ImGui.SliderInt("##TextDelay", ref controller.TextDelay, 0, 10))
                controller.SetDelay();
            
            ImGui.SameLine();
            
            ImGui.SetNextItemWidth(itemWidth);
            if (ImGui.SliderInt("##TextDuration", ref controller.TextDuration, 0, 10))
                controller.SetDuration();
                
            ImGui.TextUnformatted("Order");
            ImGui.SameLine(halfWidth);
            ImGui.TextUnformatted("Color");
            
            if (ImGui.RadioButton("Sequential", ref controller.TextMode, 0))
                controller.SetMode();
                
            ImGui.SameLine();
            
            if (ImGui.RadioButton("Random", ref controller.TextMode, 1))
                controller.SetMode();
            
            ImGui.SameLine((width - padding) * 0.5f);
            ImGui.SetNextItemWidth(itemWidth);
            if (ImGui.ColorEdit4("##TextColor", ref controller.TextColor))
                controller.SetColorText();
            
            ImGui.TextUnformatted("Words");
            if (ImGui.InputTextMultiline("##WordBank", ref controller.TextWords, 2024, new Vector2(width - padding * 2 ,0)))
                controller.BeginTextRefreshTimer();
        });
        
        SharedUserInterfaces.ContentBox("HypnosisSendCommand", AetherRemoteStyle.PanelBackground, false, () =>
        {
            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Eye, IconSize, "Open the preview spiral window"))
                _showPreviewWindow = true;
            
            ImGui.SameLine();
            
            var size = new Vector2(width - ImGui.GetCursorPosX() - IconSize.X - padding * 2, 40);
            if (commandLockoutService.IsLocked)
            {
                ImGui.BeginDisabled();
                ImGui.Button("Hypnotize", size);
                
                ImGui.SameLine();

                SharedUserInterfaces.IconButton(FontAwesomeIcon.Square, IconSize);
                ImGui.EndDisabled();
            }
            else
            {
                if (ImGui.Button("Hypnotize", size))
                {
                    controller.SendHypnosis();
                    commandLockoutService.Lock();
                }
                
                ImGui.SameLine();

                if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Square, IconSize, "Send a command to your targets to stop any current spirals if you issued them."))
                {
                    controller.StopHypnosis();
                    commandLockoutService.Lock();
                }
            }
        });
        
        if (_showPreviewWindow)
        {
            ImGui.SetNextWindowSize(HypnosisViewUiController.DefaultPreviewWindowSize, ImGuiCond.FirstUseEver);
            ImGui.Begin("Spiral Preview", ref _showPreviewWindow, ImGuiWindowFlags.NoScrollbar);
            
            // Retrieve relevant information
            var draw = ImGui.GetWindowDrawList();
            var size = ImGui.GetContentRegionAvail();
            var position = ImGui.GetCursorScreenPos();
            
            // Render spiral with a clipping rectangle
            draw.PushClipRect(position, position + size);
            controller.RenderSpiralAndText(draw, size, position);
            draw.PopClipRect();
            
            // Test for window size changing
            if (Math.Abs(size.X - controller.PreviousPreviewWindowSize.X) > 0.01)
                controller.BeginTextRefreshTimer();
            if (Math.Abs(size.Y - controller.PreviousPreviewWindowSize.Y) > 0.01)
                controller.BeginTextRefreshTimer();
            
            // Always set last size
            controller.PreviousPreviewWindowSize = size;
            
            ImGui.End();
        }
        
        ImGui.EndChild();
        
        ImGui.SameLine();
        
        friendsList.Draw();
    }
}