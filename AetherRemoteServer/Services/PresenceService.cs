using System.Collections.Concurrent;
using AetherRemoteServer.Domain;

namespace AetherRemoteServer.Services;

/// <summary>
///     
/// </summary>
public class PresenceService
{
    // Instantiated
    private readonly ConcurrentDictionary<string, Presence> _presences = [];

    /// <summary>
    ///     Try to get a <see cref="Presence"/> from friend code
    /// </summary>
    public Presence? TryGet(string friendCode)
    {
        return _presences.TryGetValue(friendCode, out var presence) ? presence : null;
    }

    /// <summary>
    ///     Try to add a friend code
    /// </summary>
    public void Add(string friendCode, string connectionId, string characterName, string characterWorld)
    {
        _presences.TryAdd(friendCode, new Presence(connectionId, characterName, characterWorld));
    }

    /// <summary>
    ///     Try to remove a friend code
    /// </summary>
    public void Remove(string friendCode)
    {
        _presences.TryRemove(friendCode, out _);
    }
    
    /// <summary>
    ///     Check to see if a friend code is exceeding the general cooldown
    /// </summary>
    public bool IsUserExceedingCooldown(string friendCode)
    {
        if (_presences.TryGetValue(friendCode, out var presence) is false)
            return true;

        return presence.GeneralBucket.TryConsumeToken() is false;
    }

    /// <summary>
    ///     Check to see if a friend code is exceeding the possession cooldown
    /// </summary>
    public bool IsUserExceedingPossession(string friendCode)
    {
        if (_presences.TryGetValue(friendCode, out var presence) is false)
            return true;

        return presence.PossessionBucket.TryConsumeToken() is false;
    }
}
