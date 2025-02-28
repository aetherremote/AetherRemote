using System;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Services;

namespace AetherRemoteClient.UI.Views.History;

public class HistoryViewUiController(LogService logService)
{
    /// <summary>
    ///     Search string to add or remove logs from the list
    /// </summary>
    public string Search = string.Empty;

    /// <summary>
    ///     List of logs to render in the view
    /// </summary>
    public readonly ListFilter<InternalLog> Logs = new(logService.Logs, FilterPredicate);

    /// <summary>
    ///     Searches properties about the log to match against the search term.
    ///     Supports searching the message portion only at the moment.
    /// </summary>
    private static bool FilterPredicate(InternalLog log, string searchTerm)
    {
        return log.Message.Contains(searchTerm, StringComparison.OrdinalIgnoreCase);
    }
}