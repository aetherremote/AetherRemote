using System;

namespace AetherRemoteClient.Domain.Log;

/// <summary>
/// Provides instruction framework for how a history log should look
/// </summary>
public abstract class AbstractHistoryLog
{
    public readonly string Message;
    public readonly DateTime Time;

    /// <summary>
    /// <inheritdoc cref="AbstractHistoryLog"/>
    /// </summary>
    public AbstractHistoryLog(string message, DateTime? time = null)
    {
        Message = message;
        Time = time ?? DateTime.Now;
    }

    /// <summary>
    /// Builds the ImGui components to display this message
    /// </summary>
    public abstract void Build();
}
