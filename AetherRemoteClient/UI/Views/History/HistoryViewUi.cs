using System.Numerics;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Utils;
using Dalamud.Bindings.ImGui;

namespace AetherRemoteClient.UI.Views.History;

public class HistoryViewUi(HistoryViewUiController controller) : IDrawable
{
    public void Draw()
    {
        ImGui.BeginChild("PermissionContent", Vector2.Zero, false, ImGuiWindowFlags.NoBackground);
        
        SharedUserInterfaces.ContentBox("HistorySearch", AetherRemoteStyle.PanelBackground, true, () =>
        {
            SharedUserInterfaces.MediumText("History");

            if (ImGui.InputTextWithHint("##Search", "Search", ref controller.Search, 200))
                controller.Logs.UpdateSearchTerm(controller.Search);
        });
        
        SharedUserInterfaces.ContentBox("HistoryLog", AetherRemoteStyle.PanelBackground, false, () =>
        {
            for (var i = controller.Logs.List.Count - 1; i >= 0; i--)
            {
                var log = controller.Logs.List[i];
                ImGui.TextUnformatted(log.TimeStamp.ToLongTimeString());
                ImGui.SameLine();
                ImGui.TextUnformatted(log.Message);
            }
        });
        
        ImGui.EndChild();
    }
}