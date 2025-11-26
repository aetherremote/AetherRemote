using System;
using System.Numerics;
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
            ImGui.InputTextWithHint("##DesignSearchBar", "Search", ref controller.SearchTerm, 32);

            ImGui.SameLine();

            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Sync, null, "Refresh Designs"))
                controller.RefreshDesigns();
        });
        
        var headerHeight = ImGui.GetCursorPosY() - begin;
        var designContextBoxSize = new Vector2(0, ImGui.GetWindowHeight() - headerHeight - padding.X * 6 - SendDesignButtonHeight * 2);
        
        if (ImGui.BeginChild("##DesignsDisplayBox", designContextBoxSize, true, ImGuiWindowFlags.NoScrollbar))
        {
            var half = ImGui.GetWindowWidth() * 0.5f;
            foreach (var folder in controller.FilteredDesigns)
            {
                if (folder.Designs.Count is 0)
                    continue;
                
                if (ImGui.CollapsingHeader(folder.Path))
                {
                    ImGui.PushStyleColor(ImGuiCol.Header, AetherRemoteStyle.PrimaryColor);
                    for (var i = 0; i < folder.Designs.Count; i++)
                    {
                        var design = folder.Designs[i];
                        var size = i % 2 is 0
                            ? new Vector2(half - padding.X * 2, 0)
                            : new Vector2(half - padding.X, 0);
                        
                        if (design.Color is uint.MaxValue)
                        {
                            if (ImGui.Selectable(design.Name, controller.SelectedDesignId == design.Id, ImGuiSelectableFlags.None, size))
                                controller.SelectedDesignId = design.Id;
                        }
                        else
                        {
                            ImGui.PushStyleColor(ImGuiCol.Text, design.Color);
                            if (ImGui.Selectable(design.Name, controller.SelectedDesignId == design.Id, ImGuiSelectableFlags.None, size))
                                controller.SelectedDesignId = design.Id;
                            ImGui.PopStyleColor();
                        }
                        
                        if (i % 2 is 0 && i < folder.Designs.Count - 1)
                            ImGui.SameLine(half);
                    }
                    
                    ImGui.PopStyleColor();
                }
            }
            
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
            else if (controller.GetFriendsLackingPermissions().Count is not 0)
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
                        controller.SendDesign();
                }
            }
        });

        ImGui.EndChild();
        ImGui.SameLine();
        friendsList.Draw();
    }
}