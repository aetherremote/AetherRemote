using System;
using System.Collections.Generic;
using AetherRemoteClient.Domain;

namespace AetherRemoteClient.Services;

/// <summary>
///     Provides access and operations to the friends list
/// </summary>
public class FriendsListService
{
    /// <summary>
    ///     Exposed list of friends
    /// </summary>
    public IReadOnlyList<Friend> Friends => _friends;
    private readonly List<Friend> _friends = [];
    
    /// <summary>
    ///     Event fired when a new friend is added
    /// </summary>
    public event EventHandler<Friend>? FriendAdded;
    
    /// <summary>
    ///     Event fired when a friend is deleted
    /// </summary>
    public event EventHandler<Friend>? FriendDeleted;
    
    /// <summary>
    ///     Event fired when the friends list is cleared
    /// </summary>
    public event EventHandler? FriendsListCleared;

    /// <summary>
    ///     Attempts to get a friend from the friends list, null if not found
    /// </summary>
    public Friend? Get(string friendCode)
    {
        foreach (var friend in _friends)
            if (friend.FriendCode == friendCode)
                return friend;

        return null;
    }

    /// <summary>
    ///     Checks to see if a friend is in the friends list
    /// </summary>
    public bool Contains(string friendCode)
    {
        foreach (var friend in _friends)
            if (friend.FriendCode == friendCode)
                return true;

        return false;
    }

    /// <summary>
    ///     Adds a new friend to the list
    /// <remarks>Triggers <see cref="FriendAdded"/></remarks>
    /// </summary>
    public void Add(Friend friend)
    {
        _friends.Add(friend);
        FriendAdded?.Invoke(this, friend);
    }

    /// <summary>
    ///     Deletes a friend from the list
    /// <remarks>Triggers <see cref="FriendDeleted"/></remarks>
    /// </summary>
    public void Delete(Friend friend)
    {
        _friends.Remove(friend);
        FriendDeleted?.Invoke(this, friend);
    }

    /// <summary>
    ///     Clears the friends list
    /// <remarks>Triggers <see cref="FriendsListCleared"/></remarks>
    /// </summary>
    public void Clear()
    {
        _friends.Clear();
        FriendsListCleared?.Invoke(this, EventArgs.Empty);
    }
}