using System;
using System.Numerics;
using AetherRemoteClient.UI.Style;
using AetherRemoteClient.Utils;
using AetherRemoteCommon;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;

namespace AetherRemoteClient.UI.Views.Hypnosis;

public partial class HypnosisView
{
    // Const
    private static readonly Vector2 IconSize = new(40);
    
    // Preview window controls
    private bool _showPreviewWindow;
    
    public void Draw()
    {
        ImGui.BeginChild("HypnosisContent", AetherRemoteDimensions.ContentSize, false, AetherRemoteImGui.ContentFlags);
        
        if (_selectionManager.Selected.Count is 0)
        {
            SharedUserInterfaces.ContentBox("", AetherRemoteColors.PanelColor, true, () =>
            {
                SharedUserInterfaces.TextCentered("You must select at least one friend");
            });

            ImGui.EndChild();
            ImGui.SameLine();
            _friendsListComponentUi.Draw();
            return;
        }
        
        var width = ImGui.GetWindowWidth();
        var halfWidth = width * 0.5f;
        var padding = ImGui.GetStyle().WindowPadding.X;
        var fontSize = ImGui.GetFontSize();
        var itemWidth = (width - padding * 3) * 0.5f;
        
        SharedUserInterfaces.ContentBox("HypnosisLoadSpiral", AetherRemoteColors.PanelColor, true, () =>
        {
            SharedUserInterfaces.MediumText("Spirals");
            SharedUserInterfaces.ComboWithFilter("##LoadSpiralInputText", "Name", ref _saveLoadSpiralSearchText, width - padding * 8 - fontSize * 3, _saveLoadSpiralFileOptionsListFilter);
            
            ImGui.SameLine();
            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Save, null, "Save"))
                SaveHypnosisProfileToDisk();
            
            ImGui.SameLine();
            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.FileImport, null, "Load"))
                LoadHypnosisProfileFromDisk();
            
            ImGui.SameLine();
            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Trash, null, "Delete (Hold Alt)") && (ImGui.IsKeyDown(ImGuiKey.RightAlt) || ImGui.IsKeyDown(ImGuiKey.LeftAlt)))
                DeleteHypnosisProfileFromDisk();
            
            ImGui.Spacing();
            
            var importExportButtonWidth = new Vector2(width * 0.5f - padding * 1.5f, 0);
            if(ImGui.Button("Export to clipboard", importExportButtonWidth))
                ExportToClipboard();
            ImGui.SameLine();
            if(ImGui.Button("Import from clipboard", importExportButtonWidth))
                ImportFromClipboard();
        });
        
        SharedUserInterfaces.ContentBox("HypnosisSpiralConfiguration", AetherRemoteColors.PanelColor, true, () =>
        {
            SharedUserInterfaces.MediumText("Spiral Configuration");
            
            ImGui.TextUnformatted("Arms");
            ImGui.SameLine(halfWidth);
            ImGui.TextUnformatted("Turns");
            
            ImGui.SetNextItemWidth(itemWidth);
            if (ImGui.SliderInt("##SpiralArms", ref _spiralArms, Constraints.Hypnosis.ArmsMin, Constraints.Hypnosis.ArmsMax))
                BeginSpiralRefreshTimer();
            
            ImGui.SameLine();
            
            ImGui.SetNextItemWidth(itemWidth);
            if (ImGui.SliderInt("##SpiralTurns", ref _spiralTurns, Constraints.Hypnosis.TurnsMin, Constraints.Hypnosis.TurnsMax))
                BeginSpiralRefreshTimer();
            
            ImGui.TextUnformatted("Curvature");
            ImGui.SameLine(halfWidth);
            ImGui.TextUnformatted("Thickness");
            
            ImGui.SetNextItemWidth(itemWidth);
            if (ImGui.SliderInt("##SpiralCurve", ref _spiralCurve, Constraints.Hypnosis.CurvesMin, Constraints.Hypnosis.CurvesMax))
                BeginSpiralRefreshTimer();
            
            ImGui.SameLine();
            
            ImGui.SetNextItemWidth(itemWidth);
            if (ImGui.SliderInt("##SpiralThickness", ref _spiralThickness, Constraints.Hypnosis.ThicknessMin, Constraints.Hypnosis.ThicknessMax))
                BeginSpiralRefreshTimer();

            ImGui.TextUnformatted("Speed");
            ImGui.SameLine(halfWidth);
            ImGui.TextUnformatted("Color");
            
            ImGui.SetNextItemWidth(itemWidth);
            if (ImGui.SliderInt("##SpiralSpeed", ref _spiralSpeed, Constraints.Hypnosis.SpeedMin, Constraints.Hypnosis.SpeedMax))
                SetSpeed();
            
            ImGui.SameLine();
            
            ImGui.SetNextItemWidth(itemWidth);
            if (ImGui.ColorEdit4("##SpiralColor", ref _spiralColor))
                SetColorSpiral();
            
            ImGui.TextUnformatted("Direction");
            if (ImGui.RadioButton("Inward", ref _spiralDirection, 0))
                SetDirection();            
            ImGui.SameLine();
            
            if (ImGui.RadioButton("Outward", ref _spiralDirection, 1))
                SetDirection();  
        });
        
        SharedUserInterfaces.ContentBox("HypnosisTextConfiguration", AetherRemoteColors.PanelColor, true, () =>
        {
            SharedUserInterfaces.MediumText("Spiral Text Configuration");
            
            ImGui.TextUnformatted("Delay");
            ImGui.SameLine(halfWidth);
            ImGui.TextUnformatted("Duration");
            
            ImGui.SetNextItemWidth(itemWidth);
            if (ImGui.SliderInt("##TextDelay", ref _textDelay, Constraints.Hypnosis.TextDelayMin, Constraints.Hypnosis.TextDelayMax))
                SetDelay();
            
            ImGui.SameLine();
            
            ImGui.SetNextItemWidth(itemWidth);
            if (ImGui.SliderInt("##TextDuration", ref _textDuration, Constraints.Hypnosis.TextDurationMin, Constraints.Hypnosis.TextDurationMax))
                SetDuration();
                
            ImGui.TextUnformatted("Order");
            ImGui.SameLine(halfWidth);
            ImGui.TextUnformatted("Color");
            
            if (ImGui.RadioButton("Sequential", ref _textMode, 0))
                SetMode();
                
            ImGui.SameLine();
            
            if (ImGui.RadioButton("Random", ref _textMode, 1))
                SetMode();
            
            ImGui.SameLine((width - padding) * 0.5f);
            ImGui.SetNextItemWidth(itemWidth);
            if (ImGui.ColorEdit4("##TextColor", ref _textColor))
                SetColorText();
            
            ImGui.TextUnformatted("Words");
            
            ImGui.SameLine();
            SharedUserInterfaces.Icon(FontAwesomeIcon.QuestionCircle);
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip("Separate words or phrases with a new line");
            
            if (ImGui.InputTextMultiline("##WordBank", ref _textWords, Constraints.Hypnosis.TextWordsMax, new Vector2(width - padding * 2 ,0)))
                BeginTextRefreshTimer();
        });
        
        SharedUserInterfaces.ContentBox("HypnosisSendCommand", AetherRemoteColors.PanelColor, false, () =>
        {
            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Eye, IconSize, "Open the preview spiral window"))
                _showPreviewWindow = true;
            
            ImGui.SameLine();
            
            var size = new Vector2(width - ImGui.GetCursorPosX() - IconSize.X - padding * 2, 40);
            if (_commandLockoutService.IsLocked)
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
                    SendHypnosis();
                    _commandLockoutService.Lock();
                }
                
                ImGui.SameLine();

                if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Square, IconSize, "Send a command to your targets to stop any current spirals if you issued them."))
                {
                    StopHypnosis();
                    _commandLockoutService.Lock();
                }
            }
        });
        
        if (_showPreviewWindow)
        {
            ImGui.SetNextWindowSize(DefaultPreviewWindowSize, ImGuiCond.FirstUseEver);
            ImGui.Begin("Spiral Preview", ref _showPreviewWindow, ImGuiWindowFlags.NoScrollbar);
            
            // Retrieve relevant information
            var draw = ImGui.GetWindowDrawList();
            var size = ImGui.GetContentRegionAvail();
            var position = ImGui.GetCursorScreenPos();
            
            // Render spiral with a clipping rectangle
            draw.PushClipRect(position, position + size);
            RenderSpiralAndText(draw, size, position);
            draw.PopClipRect();
            
            // Test for window size changing
            if (Math.Abs(size.X - _previousPreviewWindowSize.X) > 0.01)
                BeginTextRefreshTimer();
            if (Math.Abs(size.Y - _previousPreviewWindowSize.Y) > 0.01)
                BeginTextRefreshTimer();
            
            // Always set last size
            _previousPreviewWindowSize = size;
            
            ImGui.End();
        }
        
        ImGui.EndChild();
        
        ImGui.SameLine();
        
        _friendsListComponentUi.Draw();
    }
}