using System.Collections.Concurrent;
using AetherRemoteCommon;
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
        
        return (DateTime.UtcNow - presence.Last).TotalSeconds < Constraints.GlobalCommandCooldownInSeconds;
    }
}

public class Presence(string connectionId, string characterName, string characterWorld)
{
    public string ConnectionId = connectionId;
    public string CharacterName = characterName;
    public string CharacterWorld = characterWorld;
    public DateTime Last = DateTime.MinValue;
}