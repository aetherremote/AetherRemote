using System.Numerics;
using AetherRemoteClient.Dependencies.CustomizePlus.Services;
using AetherRemoteClient.Dependencies.Glamourer.Services;
using AetherRemoteClient.Dependencies.Moodles.Services;
using AetherRemoteClient.Dependencies.Penumbra.Services;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Utils;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Colors;

namespace AetherRemoteClient.UI.Views.Settings;

public class SettingsViewUi(SettingsViewUiController controller, PenumbraService penumbraService, GlamourerService glamourerService, MoodlesService moodlesService, CustomizePlusService customizePlusService) : IDrawable
{
    private static readonly Vector2 CheckboxPadding = new(8, 0);

    public void Draw()
    {
        ImGui.BeginChild("SettingsContent", Vector2.Zero, false, AetherRemoteStyle.ContentFlags);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemInnerSpacing, CheckboxPadding);

        SharedUserInterfaces.ContentBox("", AetherRemoteStyle.PanelBackground, true, () =>
        {
            SharedUserInterfaces.MediumText("Emergency Actions");
            ImGui.AlignTextToFramePadding();
            if (ImGui.Checkbox("Safe mode is", ref Plugin.Configuration.SafeMode))
                controller.EnterSafeMode(Plugin.Configuration.SafeMode);

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
        
        SharedUserInterfaces.ContentBox("SettingsGeneral", AetherRemoteStyle.PanelBackground, true, () =>
        {
            SharedUserInterfaces.MediumText("General");

            // Only draw the remaining UI elements if the character configuration value is set
            if (Plugin.CharacterConfiguration is null)
                return;
            
            if (ImGui.Checkbox("Auto Connect", ref Plugin.CharacterConfiguration.AutoLogin))
                controller.SaveConfiguration();
        });
        
        SharedUserInterfaces.ContentBox("SettingsDependencies", AetherRemoteStyle.PanelBackground, true, () =>
        {
            SharedUserInterfaces.MediumText("Dependencies");
            ImGui.TextColored(ImGuiColors.DalamudGrey,
                "Install these additional plugins for a more complete experience");

            ImGui.Spacing();

            ImGui.TextUnformatted("Penumbra");
            ImGui.SameLine();
            DrawCheckmarkOrCrossOut(penumbraService.ApiAvailable);

            ImGui.TextUnformatted("Glamourer");
            ImGui.SameLine();
            DrawCheckmarkOrCrossOut(glamourerService.ApiAvailable);

            ImGui.TextUnformatted("Moodles");
            ImGui.SameLine();
            DrawCheckmarkOrCrossOut(moodlesService.ApiAvailable);
            
            ImGui.TextUnformatted("Customize+");
            ImGui.SameLine();
            DrawCheckmarkOrCrossOut(customizePlusService.ApiAvailable);
        });

        ImGui.PopStyleVar();
        ImGui.EndChild();
    }
    
    private static void DrawCheckmarkOrCrossOut(bool apiAvailable)
    {
        if (apiAvailable)
            SharedUserInterfaces.Icon(FontAwesomeIcon.Check, ImGuiColors.HealerGreen);
        else
            SharedUserInterfaces.Icon(FontAwesomeIcon.Times, ImGuiColors.DalamudRed);
    }
    
}