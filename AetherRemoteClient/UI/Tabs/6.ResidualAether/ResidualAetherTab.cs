using AetherRemoteClient.Domain.UI;
using AetherRemoteClient.Providers;
using Dalamud.Interface.Colors;
using ImGuiNET;
using System;
using System.Numerics;

namespace AetherRemoteClient.UI.Tabs.ResidualAether;

public class ResidualAetherTab(ModSwapManager modSwapManager) : ITab
{
    public void Draw()
    {
        if (modSwapManager.ActiveChanges == false)
            return;

        if (ImGui.BeginTabItem("Residual Aether"))
        {
            if (ImGui.BeginChild("SettingsArea", Vector2.Zero, true))
            {
                ImGui.SetCursorPosY(ImGui.GetWindowHeight() / 3);

                SharedUserInterfaces.PushBigFont();
                SharedUserInterfaces.TextCentered("Foreign aether clings to your body!", ImGuiColors.ParsedOrange);
                SharedUserInterfaces.PopBigFont();

                SharedUserInterfaces.TextCentered("You have been body swapped, or experienced twinning with another person.");
                SharedUserInterfaces.TextCentered("Your appearance and outfits may have residual effects lingering from the transformation.");

                ImGui.Spacing();

                ImGui.SetCursorPosX(ImGui.GetWindowWidth() / 2 - 100);
                if (ImGui.Button("Cleanse Residual Aether", new Vector2(200, 0)))
                    _ = modSwapManager.RemoveAllCollections();

                ImGui.EndChild();
            }

            ImGui.EndTabItem();
        }
    }

    public void Dispose() => GC.SuppressFinalize(this);
}
