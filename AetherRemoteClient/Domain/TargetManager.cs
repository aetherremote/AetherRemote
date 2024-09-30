using AetherRemoteClient.Domain.Events;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace AetherRemoteClient.Domain;

/// <summary>
/// Manages a list of targets and functions that interact with them
/// </summary>
public class TargetManager : IDisposable
{
    // Injected
    private readonly FriendsList friendsList;

    /// <summary>
    /// <inheritdoc cref="TargetManager"/>
    /// </summary>
    public TargetManager(FriendsList friendsList)
    {
        this.friendsList = friendsList;
        this.friendsList.OnFriendDeleted += HandleFriendDeleted;
    }

    /// <summary>
    /// List of target friend
    /// </summary>
    public ImmutableHashSet<string> Targets { get; private set; } = [];

    /// <summary>
    /// Select only a single friend code, or multiple
    /// </summary>
    public bool SingleSelectionMode
    {
        get { return singleSelectionMode; }
        set
        {
            singleSelectionMode = value;
            if (singleSelectionMode)
                Targets = Targets.Count > 0 ? [Targets.First()] : [];
        }
    }

    // Internal value
    private bool singleSelectionMode = true;

    /// <summary>
    /// Returns if friend code is selected
    /// </summary>
    public bool Selected(string friendCode) => Targets.Contains(friendCode);

    /// <summary>
    /// Toggles a friend code
    /// </summary>
    public void ToggleSelect(string friendCode)
    {
        if (singleSelectionMode)
        {
            Targets = [friendCode];
        }
        else
        {
            if (Targets.Contains(friendCode))
            {
                Targets = Targets.Remove(friendCode);
            }
            else
            {
                Targets = Targets.Add(friendCode);
            }
        }
    }

    /// <summary>
    /// Deselects all friend codes
    /// </summary>
    public void Clear() => Targets.Clear();

    /// <summary>
    /// Handle event fired when a friend is deleted
    /// </summary>
    private void HandleFriendDeleted(object? sender, FriendDeletedEventArgs e)
    {
        Targets = Targets.Remove(e.Friend.FriendCode);
    }

    public void Dispose()
    {
        friendsList.OnFriendDeleted -= HandleFriendDeleted;
        GC.SuppressFinalize(this);
    }
}
