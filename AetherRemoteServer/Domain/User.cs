using System.Collections.Concurrent;

namespace AetherRemoteServer.Domain;

/// <summary>
/// Organizes information about a connected user. To be used with a <see cref="ConcurrentDictionary{TKey, TValue}"/> mapping friend code to this object
/// </summary>
public class User(string connectionId, DateTime? lastAction = null)
{
    /// <summary>
    /// ConnectionId of the user
    /// </summary>
    public readonly string ConnectionId = connectionId;

    /// <summary>
    /// Last time the user submitted an action
    /// </summary>
    public DateTime LastAction = lastAction ?? DateTime.Now;
}
