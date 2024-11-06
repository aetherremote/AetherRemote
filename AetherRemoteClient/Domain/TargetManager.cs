using System.Collections.Concurrent;
using System.Linq;

namespace AetherRemoteClient.Domain;

/// <summary>
/// Manages a list of targets and functions that interact with them
/// </summary>
public class TargetManager
{
    /// <summary>
    /// Dictionary of friend code mapped to friend object
    /// </summary>
    public readonly ConcurrentDictionary<string, Friend> Targets = [];

    /// <summary>
    /// Select only a single friend code, or multiple
    /// </summary>
    public bool SingleSelectionMode
    {
        get => _singleSelectionMode;
        set
        {
            _singleSelectionMode = value;
            if (_singleSelectionMode == false) return;
            if (Targets.Count <= 1) return;

            var kvp = Targets.First();
            Targets.Clear();
            Targets[kvp.Key] = kvp.Value;
        }
    }

    // Internal value
    private bool _singleSelectionMode = true;

    /// <summary>
    /// Returns if friend code is selected
    /// </summary>
    public bool Selected(string friendCode) => Targets.ContainsKey(friendCode);

    /// <summary>
    /// Toggles a friend code
    /// </summary>
    public void ToggleSelect(Friend friend)
    {
        var friendCode = friend.FriendCode;
        if (_singleSelectionMode)
        {
            if (Targets.TryGetValue(friendCode, out _))  return;

            Targets.Clear();
            Targets[friendCode] = friend;
        }
        else
        {
            if (Targets.ContainsKey(friendCode))
                Targets.TryRemove(friendCode, out _);
            else
                Targets.TryAdd(friendCode, friend);
        }
    }

    /// <summary>
    /// Deselects friend from target list
    /// </summary>
    public void Deselect(string friendCode) => Targets.TryRemove(friendCode, out _);

    /// <summary>
    /// Deselects all friend codes
    /// </summary>
    public void Clear() => Targets.Clear();
}
