using AetherRemoteClient.Domain;
using ImGuiNET;
using System.Numerics;

namespace AetherRemoteClient.UI.Tabs.Settings;

public class SettingsTab(Configuration configuration) : ITab
{
    private readonly Configuration configuration = configuration;

    public void Draw()
    {
        if (ImGui.BeginTabItem("Settings"))
        {
            if(ImGui.BeginChild("SettingsArea", Vector2.Zero, true))
            {
                SharedUserInterfaces.MediumText("General Settings");

                ImGui.Checkbox("Auto Login", ref configuration.AutoLogin);
                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.TextUnformatted("Should the plugin automatically connect to the server?");
                    ImGui.EndTooltip();
                }

                ImGui.EndChild();
            }

            ImGui.EndTabItem();
        }
    }
}
