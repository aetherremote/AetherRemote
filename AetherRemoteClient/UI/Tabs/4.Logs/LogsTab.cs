using AetherRemoteClient.Domain;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using ImGuiNET;
using System;
using System.Numerics;

namespace AetherRemoteClient.UI.Tabs.Logs;

public class LogsTab : ITab
{
    private readonly ListFilter<LogEntry> logSearchFilter = new(AetherRemoteLogging.Logs, FilterLogEntry);
    private string searchLogTerm = string.Empty;

    /// <summary>
    /// Number of unseen erros.
    /// </summary>
    private int unseenErrors = 0;

    public LogsTab()
    {
        AetherRemoteLogging.OnErrorLogged += OnErrorLogged;
    }

    private void OnErrorLogged(object? sender, EventArgs e)
    {
        unseenErrors++;
    }

    public void Draw()
    {
        // Display number of unseen errors in the tab name
        var tab = unseenErrors > 0 ? $"Logs ({unseenErrors})" : "Logs";
        if (ImGui.BeginTabItem(tab))
        {
            // Reset errors once we tab in
            unseenErrors = 0;

            var width = (ImGui.GetStyle().WindowPadding.X * 2) + ImGui.GetFontSize();
            ImGui.SetNextItemWidth(-width);

            if (ImGui.InputTextWithHint("##SearchLog", "Search", ref searchLogTerm, 128))
                logSearchFilter.UpdateSearchTerm(searchLogTerm);

            ImGui.SameLine();
            SharedUserInterfaces.IconButton(FontAwesomeIcon.TrashAlt);
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.TextUnformatted("Clears all of the current logs");
                ImGui.EndTooltip();
            }

            if (ImGui.BeginChild("LogArea", Vector2.Zero, true))
            {
                for (var i = logSearchFilter.List.Count - 1; i > 0; i--)
                {
                    var log = logSearchFilter.List[i];
                    switch (log.Type)
                    {
                        case LogType.Sent:
                            ImGui.TextUnformatted($"[{log.Timestamp.ToShortTimeString()}]");
                            ImGui.SameLine();
                            ImGui.TextColored(ImGuiColors.HealerGreen, "Sent");
                            break;

                        case LogType.Recieved:
                            ImGui.TextUnformatted($"[{log.Timestamp.ToShortTimeString()}]");
                            ImGui.SameLine();
                            ImGui.TextColored(ImGuiColors.TankBlue, "Recieved");
                            break;

                        case LogType.Info:
                            ImGui.TextUnformatted($"[{log.Timestamp.ToShortTimeString()}]");
                            ImGui.SameLine();
                            ImGui.TextUnformatted("Info");
                            break;

                        case LogType.Error:
                            ImGui.TextUnformatted($"[{log.Timestamp.ToShortTimeString()}]");
                            ImGui.SameLine();
                            ImGui.TextColored(ImGuiColors.DalamudRed, "Error");
                            break;
                    }

                    ImGui.TextUnformatted(log.Message);
                    ImGui.Separator();
                }

                ImGui.EndChild();
            }

            ImGui.EndTabItem();
        }
    }

    private static bool FilterLogEntry(LogEntry entry, string searchTerm)
    {
        // In the message
        if (entry.Message.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            return true;

        // In the sender
        if (entry.Sender.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            return true;

        // In Type
        if (entry.Type.ToString().Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }

    public void Dispose()
    {
        logSearchFilter.Dispose();
        AetherRemoteLogging.OnErrorLogged -= OnErrorLogged;
        GC.SuppressFinalize(this);
    }
}
