using System.Collections.Concurrent;
using AetherRemoteServer.Domain;

namespace AetherRemoteServer.Managers;

/// <summary>
///     Manages possession sessions
/// </summary>
public class PossessionManager
{
    // Instantiate
    private readonly ConcurrentDictionary<string, Session> _sessions = [];

    /// <summary>
    ///     Adds two people to a possession session
    /// </summary>
    public void TryAddSession(string ghostFriendCode, string hostFriendCode, Session session)
    {
        _sessions.TryAdd(ghostFriendCode, session);
        _sessions.TryAdd(hostFriendCode, session);
    }
    
    /// <summary>
    ///     Attempts to get a possession session
    /// </summary>
    /// <remarks>Both Ghost and Host have mappings to the same session</remarks>
    public Session? TryGetSession(string friendCode)
    {
        return _sessions.TryGetValue(friendCode, out var session) ? session : null;
    }

    /// <summary>
    ///     Attempts to remove a session
    /// </summary>
    /// <remarks>Removes both Ghost and Host mappings as well</remarks>
    public void TryRemoveSession(Session session)
    {
        _sessions.TryRemove(session.HostFriendCode, out _);
        _sessions.TryRemove(session.GhostFriendCode, out _);
    }
}