using System.Numerics;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Ipc;
using AetherRemoteClient.Utils;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using ImGuiNET;

namespace AetherRemoteClient.UI.Views.Settings;

public class SettingsViewUi(CustomizePlusIpc customize, GlamourerIpc glamourer, MoodlesIpc moodles, PenumbraIpc penumbra) : IDrawable
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
            [
                "Enabling safe mode will cancel any commands sent to you and",
                " prevent further ones from being processed"
            ]);

            ImGui.SameLine();
            if (Plugin.Configuration.SafeMode)
                ImGui.TextColored(ImGuiColors.HealerGreen, "ON");
            else
                ImGui.TextColored(ImGuiColors.DalamudRed, "OFF");
        });

        SharedUserInterfaces.ContentBox(AetherRemoteStyle.PanelBackground, () =>
        {
            SharedUserInterfaces.MediumText("Dependencies");
            ImGui.TextColored(ImGuiColors.DalamudGrey,
                "Install these additional plugins for a more complete experience");

            ImGui.Spacing();

            ImGui.TextUnformatted("Penumbra");
            ImGui.SameLine();
            DrawCheckmarkOrCrossOut(penumbra.ApiAvailable);

            ImGui.TextUnformatted("Glamourer");
            ImGui.SameLine();
            DrawCheckmarkOrCrossOut(glamourer.ApiAvailable);

            ImGui.TextUnformatted("Moodles");
            ImGui.SameLine();
            DrawCheckmarkOrCrossOut(moodles.ApiAvailable);
            
            ImGui.TextUnformatted("Customize+");
            ImGui.SameLine();
            DrawCheckmarkOrCrossOut(customize.ApiAvailable);
        });

        ImGui.PopStyleVar();
        ImGui.EndChild();

        return false;
    }

    private static void DrawCheckmarkOrCrossOut(bool apiAvailable)
    {
        if (apiAvailable)
            SharedUserInterfaces.Icon(FontAwesomeIcon.Check, ImGuiColors.HealerGreen);
        else
            SharedUserInterfaces.Icon(FontAwesomeIcon.Times, ImGuiColors.DalamudRed);
    }
}