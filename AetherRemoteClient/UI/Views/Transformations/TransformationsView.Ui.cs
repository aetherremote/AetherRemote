using System;
using System.Collections.Generic;
using System.Numerics;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Enums;
using AetherRemoteClient.Domain.Glamourer;
using AetherRemoteClient.UI.Style;
using AetherRemoteClient.Utils;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;

namespace AetherRemoteClient.UI.Views.Transformations;

public partial class TransformationsView
{
    // Const
    private static readonly float FooterHeight = AetherRemoteDimensions.SendCommandButtonHeight + AetherRemoteImGui.WindowPadding.X * 2f;
    
    // Tooltip Text
    private const string RequiresGlamourer = "Requires the Glamourer plugin for you and your targets";
    private const string RequiresPenumbra = "Requires the Penumbra plugin for you and your targets";
    private const string RequiresMoodles = "Requires the Moodles plugin for you and your targets";
    private const string RequiresCustomize = "Requires the Customize plugin for you and your targets";
    private const string RequiresHonorific = "Requires the Honorific plugin for you and your targets";
    
    public void Draw()
    {
        if (ImGui.BeginChild("Transformations", AetherRemoteDimensions.ContentSize, false, AetherRemoteImGui.ContentFlags))
        {
            var width = ImGui.GetWindowWidth();
            var fontSize = ImGui.GetFontSize();

            // Header
            DrawHeader(width, fontSize);
            
            // Content
            if (_mode is TransformationMode.Transform)
            {
                DrawTransformContent(width, fontSize);
                DrawSendButtonTransform(width);
            }
            else
            {
                DrawTotalTransformContent(width);
                DrawSendButtonTotalTransformation(width);
            }
            
            ImGui.EndChild();
            ImGui.SameLine();
            _friendsListComponentUi.Draw();
        }
    }

    private void DrawHeader(float width, float fontSize)
    {
        SharedUserInterfaces.ContentBox("TransformationsHeader", AetherRemoteColors.PanelColor, true, () =>
        {
            // Pre-calculate the header dimension button size
            var headerButtonDimensions = new Vector2((width - AetherRemoteImGui.WindowPadding.X * 5) * 0.25f, 0);
            
            // Header
            SharedUserInterfaces.MediumTextCentered("Mode", width);
            ImGui.SameLine(width - fontSize - AetherRemoteImGui.WindowPadding.X * 2);
            
            // Small tutorial, refer to larger one once it is ready
            SharedUserInterfaces.Icon(FontAwesomeIcon.QuestionCircle);
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.TextUnformatted("\"Mode\" refers to how the transformation will be handled.");
                ImGui.TextUnformatted("• Transform - Sends glamourer data, be that of a player, or npc (WIP)");
                ImGui.TextUnformatted("• Body Swap - Switches your body, attributes and all, with your target's");
                ImGui.TextUnformatted("• Twinning - Turns your target into you, attributes and all");
                ImGui.TextUnformatted("• Mimic - Turns you into your target, attributes and all");
                ImGui.EndTooltip();
            }
            
            // Snapshot the current mode and create nav buttons
            var value = _mode;
            DrawHeaderNavButton(TransformationMode.Transform, "Transform##NavButtonTransformation", headerButtonDimensions, value); ImGui.SameLine();
            DrawHeaderNavButton(TransformationMode.BodySwap, "Body Swap##NavButtonTransformation", headerButtonDimensions, value); ImGui.SameLine();
            DrawHeaderNavButton(TransformationMode.Twinning, "Twinning##NavButtonTransformation", headerButtonDimensions, value); ImGui.SameLine();
            DrawHeaderNavButton(TransformationMode.Mimicry, "Mimicry##NavButtonTransformation", headerButtonDimensions, value);
        });
    }

    // Transformation (Glamourer)
    private void DrawTransformContent(float width, float fontSize)
    {
        SharedUserInterfaces.ContentBox("TransformationContent", AetherRemoteColors.PanelColor, true, () =>
        {
            ImGui.SetNextItemWidth(width - AetherRemoteImGui.WindowPadding.X * 4 - fontSize);
            if (ImGui.InputTextWithHint("##DesignSearchBar", "Design Search", ref _designSearchTerm, 32))
                FilterDesignsBySearchTerm();

            ImGui.SameLine();

            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Sync, null, "Refresh Designs"))
                _ = RefreshGlamourerDesigns();
        });
        
        var sendMenuHeight = FooterHeight + AetherRemoteImGui.WindowPadding.X;
        if (ImGui.BeginChild("##DesignsDisplayBox", new Vector2(0, -sendMenuHeight), true, ImGuiWindowFlags.NoScrollbar))
        {
            if (Designs is { } designs)
                DrawTree(designs);
            
            ImGui.EndChild();
        }
        
        ImGui.Spacing();
    }

    // Body Swap, Twinning, Mimicry
    private void DrawTotalTransformContent(float width)
    {
        if (ImGui.BeginChild("TotalTransformationContent", new Vector2(0, -FooterHeight - AetherRemoteImGui.WindowPadding.X), true))
        {
            var rowOneButtonWidth = (width - AetherRemoteImGui.WindowPadding.X * 3) * 0.5f;
            var rowTwoButtonWidth = (width - AetherRemoteImGui.WindowPadding.X * 4) * 0.33333f;
            
            SharedUserInterfaces.MediumText("Primary Attributes");
            if (DrawAttributeButton(FontAwesomeIcon.User, rowOneButtonWidth, "Customization", _swapGlamourerCustomization, RequiresGlamourer))
                _swapGlamourerCustomization = !_swapGlamourerCustomization;
            ImGui.SameLine();
            
            if (DrawAttributeButton(FontAwesomeIcon.Tshirt,rowOneButtonWidth, "Equipment", _swapGlamourerEquipment, RequiresGlamourer))
                _swapGlamourerEquipment = !_swapGlamourerEquipment;

            SharedUserInterfaces.MediumText("Extra Attributes");
            if (DrawAttributeButton(FontAwesomeIcon.Wrench, rowTwoButtonWidth, "Mods", _swapPenumbraMods, RequiresPenumbra))
                _swapPenumbraMods = !_swapPenumbraMods;
            ImGui.SameLine();
            
            if (DrawAttributeButton(FontAwesomeIcon.Icons, rowTwoButtonWidth,"Moodles", _swapMoodles, RequiresMoodles))
                _swapMoodles = !_swapMoodles;
            
            ImGui.SameLine();
            if (DrawAttributeButton(FontAwesomeIcon.Plus, rowTwoButtonWidth,"Customize+", _swapCustomizePlus, RequiresCustomize))
                _swapCustomizePlus = !_swapCustomizePlus;
            
            // Spacing for new row
            ImGui.Spacing();
            
            if (DrawAttributeButton(FontAwesomeIcon.Crown, rowTwoButtonWidth,"Honorific", _swapHonorific, RequiresHonorific))
                _swapHonorific = !_swapHonorific;
            
            ImGui.EndChild();
        }
        
        ImGui.Spacing();
    }

    private void DrawSendButtonTransform(float width)
    {
        SharedUserInterfaces.ContentBox("TransformSend", AetherRemoteColors.PanelColor, false, () =>
        {
            // Button properties
            var buttonLabel = "Transform";
            var buttonSize = new Vector2(width - AetherRemoteImGui.WindowPadding.X * 2, AetherRemoteDimensions.SendCommandButtonHeight);
            var shouldDisableButton = false;

            if (_selectionManager.Selected.Count is 0)
            {
                shouldDisableButton = true;
                buttonLabel = "You must select at least one friend";
            }
            else if (_designSelectedId == Guid.Empty)
            {
                shouldDisableButton = true;
                buttonLabel = "You must select a design";
            }
            else if (_swapGlamourerCustomization is false && _swapGlamourerEquipment is false)
            {
                shouldDisableButton = true;
                buttonLabel = "You must select at least customization or equipment";
            }
            else if (MissingPermissionsForATarget())
            {
                shouldDisableButton = true;
                buttonLabel = "You lack permissions for at least one target";
            }
            else if (_commandLockoutService.IsLocked)
            {
                shouldDisableButton = true;
                buttonLabel = "Please Wait";
            }
            
            if (shouldDisableButton) ImGui.BeginDisabled();
            if (ImGui.Button(buttonLabel, buttonSize))
                _ = Send().ConfigureAwait(false);
            if (shouldDisableButton) ImGui.EndDisabled();
        });
    }

    private void DrawSendButtonTotalTransformation(float width)
    {
        SharedUserInterfaces.ContentBox("TotalTransformationSend", AetherRemoteColors.PanelColor, false, () =>
        {
            // Button properties
            var buttonLabel = _mode switch
            {
                TransformationMode.BodySwap => "Body Swap",
                TransformationMode.Twinning => "Twin",
                TransformationMode.Mimicry => "Mimic",
                _ => "???"
            };
            var buttonSize = new Vector2(width - AetherRemoteImGui.WindowPadding.X * 2, AetherRemoteDimensions.SendCommandButtonHeight);
            var shouldDisableButton = false;

            if (_selectionManager.Selected.Count is 0)
            {
                shouldDisableButton = true;
                buttonLabel = "You must select at least one friend";
            }
            else if (
                _swapGlamourerCustomization is false && 
                _swapGlamourerEquipment is false &&
                _swapCustomizePlus is false &&
                _swapHonorific is false &&
                _swapMoodles is false &&
                _swapPenumbraMods is false)
            {
                shouldDisableButton = true;
                buttonLabel = "You must select at least one attribute";
            }
            else if (MissingPermissionsForATarget())
            {
                shouldDisableButton = true;
                buttonLabel = "You lack permissions for at least one target";
            }
            else if (_commandLockoutService.IsLocked)
            {
                shouldDisableButton = true;
                buttonLabel = "Please Wait";
            }
            
            if (shouldDisableButton) ImGui.BeginDisabled();
            if (ImGui.Button(buttonLabel, buttonSize))
                _ = Send().ConfigureAwait(false);
            if (shouldDisableButton) ImGui.EndDisabled();
        });
    }

    /// <summary>
    ///     Draw the buttons for navigation in the header
    /// </summary>
    private void DrawHeaderNavButton(TransformationMode mode, string label, Vector2 headerNavButtonDimensions, TransformationMode currentValue)
    {
        if (currentValue == mode) ImGui.PushStyleColor(ImGuiCol.Button, AetherRemoteColors.PrimaryColor);
        if (ImGui.Button(label, headerNavButtonDimensions))
            _mode = mode;
        if (currentValue == mode) ImGui.PopStyleColor();
    }
    
    /// <summary>
    ///     Draw the button for selecting an attribute with total transformations
    /// </summary>
    private static bool DrawAttributeButton(FontAwesomeIcon icon, float width, string text, bool selected, string? tooltip)
    {
        var font = ImGui.GetFontSize();
        var padding = ImGui.GetStyle().WindowPadding.X;

        var size = new Vector2(width, (font + padding) * 2f);
        var label = "\n" + text;
        
        ImGui.BeginGroup();
        
        bool pressed;
        if (selected)
        {
            ImGui.PushStyleColor(ImGuiCol.Button, AetherRemoteColors.PrimaryColor);
            pressed = ImGui.Button(label, size);
            ImGui.PopStyleColor();
        }
        else
        {
            pressed = ImGui.Button(label, size);
        }

        var iconRect = ImGui.GetItemRectMin();
        iconRect.X += (width - font) * 0.5f;
        iconRect.Y += padding;
        
        ImGui.PushFont(UiBuilder.IconFont);
        ImGui.SetCursorScreenPos(iconRect);
        ImGui.TextUnformatted(icon.ToIconString());
        ImGui.PopFont();
        
        ImGui.EndGroup();
        
        if (tooltip is not null && ImGui.IsItemHovered())
            ImGui.SetTooltip(tooltip);

        return pressed;
    }
    
    /// <summary>
    ///     Renders a recursive tree view of the Glamourer designs
    /// </summary>
    private void DrawTree(IEnumerable<FolderNode<Design>> nodes)
    {
        foreach (var node in nodes)
        {
            // Folder node
            if (node.Content is null)
            {
                // Create the node
                if (ImGui.TreeNodeEx(node.Name, ImGuiTreeNodeFlags.SpanAvailWidth | ImGuiTreeNodeFlags.Framed))
                {
                    // Recursively draw the children inside the tree node
                    DrawTree(node.Children.Values);
                    
                    // Close the tree
                    ImGui.TreePop();
                }
            }
            // Leaf node, that contains the actual content
            else
            {
                if (_designSelectedId == node.Content.Id)
                {
                    ImGui.PushStyleColor(ImGuiCol.Header, AetherRemoteColors.PrimaryColor);
                    ImGui.Selectable(node.Name, true);
                    ImGui.PopStyleColor();
                }
                else
                {
                    if (ImGui.Selectable(node.Name))
                        if (node.Content is { } design)
                            _designSelectedId = design.Id;
                }
            }
        }
    }
}