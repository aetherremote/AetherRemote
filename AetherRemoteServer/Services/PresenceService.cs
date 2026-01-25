using System.Collections.Concurrent;
using AetherRemoteServer.Domain;
using AetherRemoteServer.Domain.Interfaces;

namespace AetherRemoteServer.Services;

public class PresenceService : IPresenceService
{
    private readonly ConcurrentDictionary<string, Presence> _presences = [];

    public Presence? TryGet(string friendCode) => _presences.TryGetValue(friendCode, out var presence) ? presence : null;
    
    public void Add(string friendCode, Presence presence) => _presences.TryAdd(friendCode, presence);
    
    public void Remove(string friendCode) => _presences.TryRemove(friendCode, out _);
    
    public bool IsUserExceedingCooldown(string friendCode)
    {
        if (_presences.TryGetValue(friendCode, out var presence) is false)
            return true;

        return presence.GeneralBucket.TryConsumeToken() is false;
    }

    public bool IsUserExceedingPossession(string friendCode)
    {
        if (_presences.TryGetValue(friendCode, out var presence) is false)
            return true;

        return presence.PossessionBucket.TryConsumeToken() is false;
    }
}
