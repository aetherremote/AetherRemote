using System.Numerics;
using AetherRemoteClient.UI.Style;
using AetherRemoteClient.Utils;
using Dalamud.Bindings.ImGui;

namespace AetherRemoteClient.UI.Views.History;

public partial class HistoryView
{
    public void Draw()
    {
        ImGui.BeginChild("PermissionContent", Vector2.Zero, false, ImGuiWindowFlags.NoBackground);
        
        SharedUserInterfaces.ContentBox("HistorySearch", AetherRemoteColors.PanelColor, true, () =>
        {
            SharedUserInterfaces.MediumText("History");

            if (ImGui.InputTextWithHint("##Search", "Search", ref _search, 200))
                _logs.UpdateSearchTerm(_search);
        });
        
        SharedUserInterfaces.ContentBox("HistoryLog", AetherRemoteColors.PanelColor, false, () =>
        {
            for (var i = _logs.List.Count - 1; i >= 0; i--)
            {
                var log = _logs.List[i];
                ImGui.TextUnformatted($"[{log.TimeStamp.ToLongTimeString()}] {log.Message}");
            }
        });
        
        ImGui.EndChild();
    }
}