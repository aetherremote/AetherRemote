using System;
using System.Linq;
using AetherRemoteClient.Dependencies.CustomizePlus.Domain;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Interfaces;
using Dalamud.Bindings.ImGui;

namespace AetherRemoteClient.UI.Views.Debug;

public class DebugViewUi(DebugViewUiController controller) : IDrawable
{
    public void Draw()
    {
        ImGui.BeginGroup();
        if (ImGui.Button("Debug"))
        {
            controller.Debug();
        }
        
        ImGui.SameLine();

        if (ImGui.Button("Debug2"))
        {
            controller.Debug2();
        }

        if (controller._node is null)
            return;
        
        DrawTree(controller._node);
        ImGui.EndGroup();

    }
    
    public static void DrawTree(FolderNode<Profile> node)
    {
        foreach (var child in node.Children.Values.OrderBy(n => n.Content).ThenBy(n => n.Name))
        {
            if (child.Content is null)
            {
                if (ImGui.TreeNode(child.Name))
                {
                    DrawTree(child);
                    ImGui.TreePop();
                }
            }
            else
            {
                if (ImGui.Selectable(child.Name))
                    Plugin.Log.Info($"{child.Content?.Guid}");
            }
        }
    }
}