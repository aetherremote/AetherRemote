using System.Numerics;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Utils;
using Dalamud.Interface.Colors;
using ImGuiNET;

namespace AetherRemoteClient.UI.Views.Settings;

public class SettingsViewUi : IDrawable
{
    private readonly SettingsViewUiController _controller = new();
    
    private static readonly Vector2 CheckboxPadding = new(8, 0);
    
    public bool Draw()
    {
        ImGui.BeginChild("SettingsContent", Vector2.Zero, false, AetherRemoteStyle.ContentFlags);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemInnerSpacing, CheckboxPadding);

        SharedUserInterfaces.ContentBox(AetherRemoteStyle.PanelBackground, () =>
        {
            SharedUserInterfaces.MediumText("General");
            ImGui.Checkbox("Auto Connect", ref Plugin.Configuration.AutoLogin);
        });

        SharedUserInterfaces.ContentBox(AetherRemoteStyle.PanelBackground, () =>
        {
            SharedUserInterfaces.MediumText("Emergency Actions");
            ImGui.AlignTextToFramePadding();
            if (ImGui.Checkbox("Safe mode is", ref Plugin.Configuration.SafeMode))
                Plugin.Configuration.Save();

            SharedUserInterfaces.Tooltip(
                ["Enabling safe mode will cancel any commands sent to you and", " prevent further ones from being processed"]);

            ImGui.SameLine();
            if (Plugin.Configuration.SafeMode)
                ImGui.TextColored(ImGuiColors.HealerGreen, "ON");
            else
                ImGui.TextColored(ImGuiColors.DalamudRed, "OFF");
        });

        ImGui.PopStyleVar();
        ImGui.EndChild();
        
        return false;
    }
}