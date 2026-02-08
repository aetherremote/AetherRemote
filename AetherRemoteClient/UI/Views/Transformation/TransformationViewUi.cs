using System;
using System.Collections.Generic;
using System.Numerics;
using AetherRemoteClient.Dependencies.Glamourer.Domain;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteClient.UI.Components.Friends;
using AetherRemoteClient.Utils;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;

namespace AetherRemoteClient.UI.Views.Transformation;

public class TransformationViewUi(
    FriendsListComponentUi friendsList,
    TransformationViewUiController controller,
    CommandLockoutService commandLockoutService,
    SelectionManager selectionManager) : IDrawable
{
    // Const
    private const int SendDesignButtonHeight = 40;
    
    public void Draw()
    {
        ImGui.BeginChild("TransformationContent", AetherRemoteStyle.ContentSize, false, AetherRemoteStyle.ContentFlags);
        
        var width = ImGui.GetWindowWidth();
        var padding = new Vector2(ImGui.GetStyle().WindowPadding.X, 0);

        var begin = ImGui.GetCursorPosY();
        SharedUserInterfaces.ContentBox("TransformDesignSearch", AetherRemoteStyle.PanelBackground, true, () =>
        {
            SharedUserInterfaces.MediumText("Select Design");

            ImGui.SetNextItemWidth(width - padding.X * 4 - ImGui.GetFontSize());
            if (ImGui.InputTextWithHint("##DesignSearchBar", "Search", ref controller.SearchTerm, 32))
                controller.FilterDesignsBySearchTerm();

            ImGui.SameLine();

            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Sync, null, "Refresh Designs"))
                _ = controller.RefreshGlamourerDesigns();
        });
        
        // TODO: Factor out ImGui calls -> AetherRemoteImGui
        var headerHeight = ImGui.GetCursorPosY() - begin;
        var designContextBoxSize = new Vector2(0, ImGui.GetWindowHeight() - headerHeight - padding.X * 6 - SendDesignButtonHeight * 2);
        if (ImGui.BeginChild("##DesignsDisplayBox", designContextBoxSize, true, ImGuiWindowFlags.NoScrollbar))
        {
            if (controller.Designs is { } designs)
                DrawTree(designs);
            
            ImGui.EndChild();
        }
        
        ImGui.Spacing();

        SharedUserInterfaces.ContentBox("DesignOptions", AetherRemoteStyle.PanelBackground, true, () =>
        {
            if (controller.ShouldApplyCustomization)
            {
                ImGui.PushStyleColor(ImGuiCol.Button, AetherRemoteStyle.PrimaryColor);
                if (SharedUserInterfaces.IconButton(FontAwesomeIcon.User, new Vector2(SendDesignButtonHeight)))
                    controller.ShouldApplyCustomization = !controller.ShouldApplyCustomization;
                ImGui.PopStyleColor();
            }
            else
            {
                if (SharedUserInterfaces.IconButton(FontAwesomeIcon.User, new Vector2(SendDesignButtonHeight)))
                    controller.ShouldApplyCustomization = !controller.ShouldApplyCustomization;
            }
            
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip(controller.ShouldApplyCustomization ? "Currently applying customizations, click to disable" : "Not applying customizations, click to enable");
            
            ImGui.SameLine();

            if (controller.ShouldApplyEquipment)
            {
                ImGui.PushStyleColor(ImGuiCol.Button, AetherRemoteStyle.PrimaryColor);
                if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Tshirt, new Vector2(SendDesignButtonHeight)))
                    controller.ShouldApplyEquipment = !controller.ShouldApplyEquipment;
                ImGui.PopStyleColor();
            }
            else
            {
                if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Tshirt, new Vector2(SendDesignButtonHeight)))
                    controller.ShouldApplyEquipment = !controller.ShouldApplyEquipment;
            }
            
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip(controller.ShouldApplyEquipment ? "Currently applying equipment, click to disable" : "Not applying equipment, click to enable");
        });
        
        SharedUserInterfaces.ContentBox("DesignSend", AetherRemoteStyle.PanelBackground, false, () =>
        {
            if (selectionManager.Selected.Count is 0)
            {
                ImGui.BeginDisabled();
                ImGui.Button("You must select at least one friend", new Vector2(ImGui.GetWindowWidth() - padding.X * 2, SendDesignButtonHeight));
                ImGui.EndDisabled();
            }
            else if (controller.SelectedDesignId == Guid.Empty)
            {
                ImGui.BeginDisabled();
                ImGui.Button("You must select a design", new Vector2(ImGui.GetWindowWidth() - padding.X * 2, SendDesignButtonHeight));
                ImGui.EndDisabled();
            }
            else if (controller.ShouldApplyCustomization is false && controller.ShouldApplyEquipment is false)
            {
                ImGui.BeginDisabled();
                ImGui.Button("You must select at least customize or equipment", new Vector2(ImGui.GetWindowWidth() - padding.X * 2, SendDesignButtonHeight));
                ImGui.EndDisabled();
            }
            else if (controller.MissingPermissionsForATarget())
            {
                ImGui.BeginDisabled();
                ImGui.Button("You lack permissions for one or more of your targets", new Vector2(ImGui.GetWindowWidth() - padding.X * 2, SendDesignButtonHeight));
                ImGui.EndDisabled();
            }
            else
            {
                if (commandLockoutService.IsLocked)
                {
                    ImGui.BeginDisabled();
                    ImGui.Button("Transform", new Vector2(ImGui.GetWindowWidth() - padding.X * 2, SendDesignButtonHeight));
                    ImGui.EndDisabled();
                }
                else
                {
                    if (ImGui.Button("Transform", new Vector2(ImGui.GetWindowWidth() - padding.X * 2, SendDesignButtonHeight)))
                        _ = controller.SendDesign().ConfigureAwait(false);
                }
            }
        });

        ImGui.EndChild();
        ImGui.SameLine();
        friendsList.Draw();
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
                // ReSharper disable once InvertIf
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
                if (controller.SelectedDesignId == node.Content.Id)
                {
                    ImGui.PushStyleColor(ImGuiCol.Header, AetherRemoteStyle.PrimaryColor);
                    ImGui.Selectable(node.Name, true);
                    ImGui.PopStyleColor();
                }
                else
                {
                    if (ImGui.Selectable(node.Name))
                        if (node.Content is { } design)
                            controller.SelectedDesignId = design.Id;
                }
            }
        }
    }
}