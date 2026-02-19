using System.Numerics;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Services;
using AetherRemoteClient.Style;
using AetherRemoteClient.Utils;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Colors;

namespace AetherRemoteClient.UI.Views.Status;

public class StatusViewUi(StatusViewUiController controller, StatusService statusService) : IDrawable
{
    public void Draw()
    {
        ImGui.BeginChild("StatusContent", Vector2.Zero, false, AetherRemoteImGui.ContentFlags);

        var windowWidth = ImGui.GetWindowWidth();
        
        SharedUserInterfaces.ContentBox("StatusHeader", AetherRemoteColors.PanelColor, true, () =>
        {
            SharedUserInterfaces.BigTextCentered("Statuses");
            SharedUserInterfaces.TextCentered("Many things in this plugin will change or control your character, which will show up here.");
        });
        
        SharedUserInterfaces.ContentBox("StatusList", AetherRemoteColors.PanelColor, true, () =>
        {
            var count = 0;
            if (statusService.Mind is not null)
            {
                count++;
            }

            if (statusService.Spirit is not null)
            {
                count++;
            }

            if (statusService.Body is not null)
            {
                count++;
            }

            if (statusService.Identity is not null)
            {
                count++;
            }

            if (statusService.Proportions is not null)
            {
                count++;
            }

            if (count is 0)
            {
                SharedUserInterfaces.PushMediumFont();
                SharedUserInterfaces.TextCentered("You have nothing affecting you");
                SharedUserInterfaces.PopMediumFont();
                
                SharedUserInterfaces.TextCentered("(You should fix that)", ImGuiColors.DalamudGrey);
            }
        });

        ImGui.EndChild();
    }
}
