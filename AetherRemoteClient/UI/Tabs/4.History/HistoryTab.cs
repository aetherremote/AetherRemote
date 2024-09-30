using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Log;
using AetherRemoteClient.Domain.UI;
using Dalamud.Interface;
using ImGuiNET;
using System;
using System.Numerics;

namespace AetherRemoteClient.UI.Tabs.History;

public class HistoryTab : ITab
{
    // Injected
    private readonly HistoryLogManager historyLogManager;

    private readonly ListFilter<AbstractHistoryLog> historyLogSearchFilter;
    private string searchTerm;

    public HistoryTab(HistoryLogManager historyLogManager)
    {
        this.historyLogManager = historyLogManager;

        searchTerm = string.Empty;
        historyLogSearchFilter = new(historyLogManager.History, FilterHistoryLogs);
    }

    public void Draw()
    {
        if (ImGui.BeginTabItem("History"))
        {
            var width = -1 * ((ImGui.GetStyle().WindowPadding.X * 2) + ImGui.GetFontSize());
            ImGui.SetNextItemWidth(width);

            if (ImGui.InputTextWithHint("##SearchLog", "Search", ref searchTerm, 128))
                historyLogSearchFilter.UpdateSearchTerm(searchTerm);

            ImGui.SameLine();
            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.TrashAlt))
                historyLogManager.Clear();

            SharedUserInterfaces.Tooltip("Clear all the current logs.");

            if (ImGui.BeginChild("LogArea", Vector2.Zero, true))
            {
                for (var i = historyLogSearchFilter.List.Count - 1; i >= 0; i--)
                {
                    var log = historyLogSearchFilter.List[i];
                    log.Build();
                }

                ImGui.EndChild();
            }

            ImGui.EndTabItem();
        }
    }

    public void Dispose() => GC.SuppressFinalize(this);
    private bool FilterHistoryLogs(AbstractHistoryLog log, string searchTerm) => log.Message.Contains(searchTerm, StringComparison.OrdinalIgnoreCase);
}
