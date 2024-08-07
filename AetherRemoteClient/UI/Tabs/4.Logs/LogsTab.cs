using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Logger;
using Dalamud.Interface;
using ImGuiNET;
using System;
using System.Numerics;

namespace AetherRemoteClient.UI.Tabs.Logs;

public class LogsTab : ITab
{
    private readonly ListFilter<AetherRemoteInternalLog> logSearchFilter;
    private string searchLogTerm;

    public LogsTab(AetherRemoteLogger logger)
    {
        logSearchFilter = new(logger.InternalLogs, FilterLogs);
        searchLogTerm = string.Empty;
    }

    public void Draw()
    {
        if (ImGui.BeginTabItem("Logs"))
        {
            var width = ((ImGui.GetStyle().WindowPadding.X * 2) + ImGui.GetFontSize()) * -1;
            ImGui.SetNextItemWidth(width);

            if (ImGui.InputTextWithHint("##SearchLog", "Search", ref searchLogTerm, 128))
                logSearchFilter.UpdateSearchTerm(searchLogTerm);

            ImGui.SameLine();
            SharedUserInterfaces.IconButton(FontAwesomeIcon.TrashAlt);
            SharedUserInterfaces.Tooltip("Clear all the current logs.");

            if (ImGui.BeginChild("LogArea", Vector2.Zero, true))
            {
                for (var i = logSearchFilter.List.Count - 1; i >= 0; i--)
                {
                    var log = logSearchFilter.List[i];

                    // TODO: Implement symbols + colors
                    ImGui.TextUnformatted($"[{log.Timestamp.ToShortTimeString()}]");
                    ImGui.SameLine();
                    ImGui.TextUnformatted(log.Message);
                    ImGui.Separator();
                }

                ImGui.EndChild();
            }

            ImGui.EndTabItem();
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    private static bool FilterLogs(AetherRemoteInternalLog entry, string searchTerm)
    {
        // TODO: Filter implementation once AetherRemoteLog is more defined.
        return true;
    }
}
