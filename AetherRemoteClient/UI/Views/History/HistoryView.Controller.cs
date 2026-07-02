using System;
using AetherRemoteClient.Domain;

namespace AetherRemoteClient.UI.Views.History;

public partial class HistoryView
{
    /// <summary>
    ///     Search string to add or remove logs from the list
    /// </summary>
    private string _search = string.Empty;

    /// <summary>
    ///     List of logs to render in the view
    /// </summary>
    private readonly ListFilter<InternalLog> _logs;

    /// <summary>
    ///     Searches properties about the log to match against the search term.
    ///     Supports searching the message portion only at the moment.
    /// </summary>
    private static bool FilterPredicate(InternalLog log, string searchTerm)
    {
        return log.Message.Contains(searchTerm, StringComparison.OrdinalIgnoreCase);
    }
}