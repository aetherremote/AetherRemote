using System.Numerics;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Services;
using AetherRemoteClient.Utils;
using ImGuiNET;

namespace AetherRemoteClient.UI.Views.History;

public class HistoryViewUi(LogService logService) : IDrawable
{
    // Instantiated
    private readonly HistoryViewUiController _controller = new(logService);
    
    public bool Draw()
    {
        ImGui.BeginChild("PermissionContent", Vector2.Zero, false, ImGuiWindowFlags.NoBackground);
        
        SharedUserInterfaces.ContentBox(AetherRemoteStyle.PanelBackground, () =>
        {
            SharedUserInterfaces.MediumText("History");

            if (ImGui.InputTextWithHint("##Search", "Search", ref _controller.Search, 200))
                _controller.Logs.UpdateSearchTerm(_controller.Search);
        });
        
        SharedUserInterfaces.ContentBox(AetherRemoteStyle.PanelBackground, () =>
        {
            for (var i = _controller.Logs.List.Count - 1; i >= 0; i--)
            {
                var log = _controller.Logs.List[i];
                ImGui.TextUnformatted(log.TimeStamp.ToLongTimeString());
                ImGui.SameLine();
                ImGui.TextUnformatted(log.Message);
            }
        }, true, false);
        
        ImGui.EndChild();
        return false;
    }
}