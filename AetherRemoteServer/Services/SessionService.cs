using System.Collections.Concurrent;
using AetherRemoteServer.Domain;

namespace AetherRemoteServer.Services;

/// <summary>
///     Provides access to underlying sessions associated with a friend code
/// </summary>
public class SessionService
{
    private readonly ConcurrentDictionary<string, UserSession> _sessions = [];

    /// <summary>
    ///     Starts a new session for a friend code
    /// </summary>
    public bool StartSession(string friendCode, string connectionId, string characterName, string characterWorld)
    {
        if (_sessions.TryGetValue(friendCode, out var session))
        {
            if (session.OnlineStatus is OnlineStatus.Online)
                return false;
            
            _sessions.TryRemove(friendCode, out _);
        }
        
        return _sessions.TryAdd(friendCode, new UserSession(connectionId, characterName, characterWorld));
    }
    
    /// <summary>
    ///     Get the friend code's active session, if one exists
    /// </summary>
    public UserSession? GetSession(string friendCode)
    {
        return _sessions.GetValueOrDefault(friendCode);
    }

    /// <summary>
    ///     Checks if a friend code is online (they have an active session)
    /// </summary>
    public bool IsOnline(string friendCode)
    {
        return _sessions.TryGetValue(friendCode, out var session) && session.OnlineStatus == OnlineStatus.Online;
    }
    
    /// <summary>
    ///     Updates the connectivity status for a friend code, useful for tracking connections, reconnections, and disconnections
    /// </summary>
    public void UpdateConnectivityStatus(string friendCode, OnlineStatus status, string? connectionId = null)
    {
        if (_sessions.TryGetValue(friendCode, out var session) is false)
            return;

        if (session.ConnectionId != connectionId)
            return;

        session.OnlineStatus = status;
        session.ConnectionId = connectionId;
    }

    /// <summary>
    ///     Ends an existing session for a friend code
    /// </summary>
    public bool EndSession(string friendCode)
    {
        return _sessions.TryRemove(friendCode, out _);
    }
}