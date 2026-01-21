using System.Collections.Concurrent;
using AetherRemoteServer.Domain.Interfaces;

namespace AetherRemoteServer.Managers;

public class PossessionManager : IPossessionManager
{
    private readonly ConcurrentDictionary<string, Session> _sessions = [];

    public void TryAddSession(string ghostFriendCode, string hostFriendCode, Session session)
    {
        _sessions.TryAdd(ghostFriendCode, session);
        _sessions.TryAdd(hostFriendCode, session);
    }
    
    public Session? TryGetSession(string friendCode)
    {
        return _sessions.TryGetValue(friendCode, out var session) ? session : null;
    }

    public void TryRemoveSession(Session session)
    {
        _sessions.TryRemove(session.HostFriendCode, out _);
        _sessions.TryRemove(session.GhostFriendCode, out _);
    }
}

public class Session(string ghostFriendCode, string hostFriendCode)
{
    public string GhostFriendCode = ghostFriendCode;
    public string HostFriendCode = hostFriendCode;
}