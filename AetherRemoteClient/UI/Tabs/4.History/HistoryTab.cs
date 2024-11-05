using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Log;
using AetherRemoteClient.Domain.UI;
using Dalamud.Interface;
using ImGuiNET;
using System;
using System.Numerics;

namespace AetherRemoteClient.UI.Tabs.History;

public class HistoryTab(HistoryLogManager historyLogManager) : ITab
{
    // Instantiated
    private readonly ListFilter<AbstractHistoryLog> _historyLogSearchFilter = new(historyLogManager.History, FilterHistoryLogs);
    private string _searchTerm = string.Empty;

    public void Draw()
    {
        if (ImGui.BeginTabItem("History"))
        {
            var width = -1 * (ImGui.GetStyle().WindowPadding.X * 2 + ImGui.GetFontSize());
            ImGui.SetNextItemWidth(width);

            if (ImGui.InputTextWithHint("##SearchLog", "Search", ref _searchTerm, 128))
                _historyLogSearchFilter.UpdateSearchTerm(_searchTerm);

            ImGui.SameLine();
            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.TrashAlt))
                historyLogManager.Clear();

            SharedUserInterfaces.Tooltip("Clear all the current logs.");

            if (ImGui.BeginChild("LogArea", Vector2.Zero, true))
            {
                for (var i = _historyLogSearchFilter.List.Count - 1; i >= 0; i--)
                {
                    var log = _historyLogSearchFilter.List[i];
                    log.Build();
                }

                ImGui.EndChild();
            }

            ImGui.EndTabItem();
        }
    }

    public void Dispose() => GC.SuppressFinalize(this);
    private static bool FilterHistoryLogs(AbstractHistoryLog log, string searchTerm) => log.Message.Contains(searchTerm, StringComparison.OrdinalIgnoreCase);
}
