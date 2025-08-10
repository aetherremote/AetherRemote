using System.Numerics;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Ipc;
using AetherRemoteClient.Utils;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Colors;

namespace AetherRemoteClient.UI.Views.Settings;

public class SettingsViewUi(
    SettingsViewUiController controller,
    CustomizePlusIpc customize,
    GlamourerIpc glamourer,
    MoodlesIpc moodles,
    PenumbraIpc penumbra) : IDrawable
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
                controller.UpdateSafeMode(Plugin.Configuration.SafeMode);

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
            if (ImGui.Checkbox("Auto Connect", ref Plugin.Configuration.AutoLogin))
                Plugin.Configuration.Save();
        });
        
        SharedUserInterfaces.ContentBox("SettingsHypnosis", AetherRemoteStyle.PanelBackground, true, () =>
        {
            SharedUserInterfaces.MediumText("Hypnosis");
            ImGui.SameLine();
            SharedUserInterfaces.Icon(FontAwesomeIcon.QuestionCircle);
            SharedUserInterfaces.Tooltip("Setting Min/Max speed limits the spirals sent by others to your preferences");
            
            ImGui.TextUnformatted("Minimum Spiral Speed");
            if (ImGui.SliderInt("##MinimumSpeed", ref controller.MinimumSpiralSpeed, 0, 100))
                if (controller.MinimumSpiralSpeed > controller.MaximumSpiralSpeed)
                    controller.MinimumSpiralSpeed = controller.MaximumSpiralSpeed;
            
            ImGui.TextUnformatted("Maximum Spiral Speed");
            if(ImGui.SliderInt("##MaximumSpeed", ref controller.MaximumSpiralSpeed, 0, 100))
                if (controller.MaximumSpiralSpeed < controller.MinimumSpiralSpeed)
                    controller.MaximumSpiralSpeed = controller.MinimumSpiralSpeed;
            
        });

        SharedUserInterfaces.ContentBox("SettingsDependencies", AetherRemoteStyle.PanelBackground, true, () =>
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
    }

    private static void DrawCheckmarkOrCrossOut(bool apiAvailable)
    {
        if (apiAvailable)
            SharedUserInterfaces.Icon(FontAwesomeIcon.Check, ImGuiColors.HealerGreen);
        else
            SharedUserInterfaces.Icon(FontAwesomeIcon.Times, ImGuiColors.DalamudRed);
    }
}