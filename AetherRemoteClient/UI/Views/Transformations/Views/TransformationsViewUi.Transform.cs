using System.Collections.Generic;
using System.Numerics;
using AetherRemoteClient.Dependencies.Glamourer.Domain;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Style;
using AetherRemoteClient.Utils;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;

namespace AetherRemoteClient.UI.Views.Transformations.Views;

public partial class TransformationsViewUi
{
    private void DrawTransformView(float width, float footerHeight)
    {
        var fontSize = ImGui.GetFontSize();
        SharedUserInterfaces.ContentBox("TransformDesignSearch", AetherRemoteColors.PanelColor, true, () =>
        {
            SharedUserInterfaces.MediumText("Select Design");

            ImGui.SetNextItemWidth(width - AetherRemoteImGui.WindowPadding.X * 4 - fontSize);
            if (ImGui.InputTextWithHint("##DesignSearchBar", "Search", ref controller.SearchTerm, 32))
                controller.FilterDesignsBySearchTerm();

            ImGui.SameLine();

            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Sync, null, "Refresh Designs"))
                _ = controller.RefreshGlamourerDesigns();
        });
        
        if (ImGui.BeginChild("##DesignsDisplayBox", new Vector2(0, -footerHeight - AetherRemoteImGui.WindowPadding.X), true, ImGuiWindowFlags.NoScrollbar))
        {
            
            
            if (controller.Designs is { } designs)
                DrawTree(designs);
            
            ImGui.EndChild();
        }
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
                    ImGui.PushStyleColor(ImGuiCol.Header, AetherRemoteColors.PrimaryColor);
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