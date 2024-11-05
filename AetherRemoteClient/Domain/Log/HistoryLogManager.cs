using System.Collections.Generic;

namespace AetherRemoteClient.Domain.Log;

/// <summary>
/// Manages all log history of outgoing and incoming commands
/// </summary>
public class HistoryLogManager
{
    /// <summary>
    /// A list of all log history
    /// </summary>
    public readonly List<AbstractHistoryLog> History = [];

    /// <summary>
    /// Clears all logs
    /// </summary>
    public void Clear() => History.Clear();

    /// <summary>
    /// Logs a standard history event
    /// </summary>
    public void LogHistory(string message) => History.Add(new HistoryLog(message));

    /// <summary>
    /// Logs a standard history event, but with glamourer data appended to the end
    /// </summary>
    public void LogHistoryGlamourer(string message, string data) => History.Add(new HistoryLogWithGlamourer(message, data));
}
