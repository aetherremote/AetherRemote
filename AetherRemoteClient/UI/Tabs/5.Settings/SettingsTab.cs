using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.UI;
using AetherRemoteClient.Providers;
using Dalamud.Interface.Colors;
using ImGuiNET;
using System;
using System.Numerics;

namespace AetherRemoteClient.UI.Tabs.Settings;

public class SettingsTab : ITab
{
    private readonly ActionQueueProvider actionQueueProvider;
    private readonly ClientDataManager clientDataManager;
    private readonly Configuration configuration;

    public SettingsTab(
        ActionQueueProvider actionQueueProvider,
        ClientDataManager clientDataManager,
        Configuration configuration)
    {
        this.actionQueueProvider = actionQueueProvider;
        this.clientDataManager = clientDataManager;
        this.configuration = configuration;
    }

    public void Draw()
    {
        if (ImGui.BeginTabItem("Settings"))
        {
            if (ImGui.BeginChild("SettingsArea", Vector2.Zero, true))
            {
                SharedUserInterfaces.MediumText("General Settings");

                ImGui.Checkbox("Auto Login", ref configuration.AutoLogin);
                SharedUserInterfaces.Tooltip("Should the plugin automatically connect to the server?");

                SharedUserInterfaces.MediumText("Emergency Actions");
                if (ImGui.Button("Clear Action Queue"))
                    actionQueueProvider.Clear();

                SharedUserInterfaces.Tooltip(
                    [
                    "Clears all pending commands waiting to execute.",
                    "Useful if someone has spammed you with commands."
                    ]);

                ImGui.SameLine();

                Vector4 color;
                string buttonText;
                if (clientDataManager.SafeMode)
                {
                    buttonText = "Safemode On";
                    color = ImGuiColors.DPSRed;
                }
                else
                {
                    buttonText = "Safemode Off";
                    color = ImGuiColors.HealerGreen;
                }

                ImGui.PushStyleColor(ImGuiCol.Button, color);
                if (ImGui.Button(buttonText, new Vector2(120, 0)))
                {
                    actionQueueProvider.Clear();
                    clientDataManager.SafeMode = !clientDataManager.SafeMode;
                }
                ImGui.PopStyleColor();
                SharedUserInterfaces.Tooltip("Temporarily prevents all actions from executing and clears the queue.");

                ImGui.EndChild();
            }

            ImGui.EndTabItem();
        }
    }

    public void Dispose() => GC.SuppressFinalize(this);
}
