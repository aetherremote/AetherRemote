using AetherRemoteClient.Domain.Events;
using AetherRemoteCommon.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AetherRemoteClient.Domain;

/// <summary>
/// Local storage of <see cref="Friend"/> data
/// </summary>
public class FriendsList
{
    /// <summary>
    /// Local <see cref="List{T}"/> of <see cref="Friend"/>
    /// </summary>
    public List<Friend> Friends = [];

    /// <summary>
    /// Fired when a friend is deleted from the friends list using <see cref="DeleteFriend"/>
    /// </summary>
    public event EventHandler<FriendDeletedEventArgs>? OnFriendDeleted;

    /// <summary>
    /// Converts a permission map from the server into friends list format
    /// </summary>
    public void ConvertServerPermissionsToLocal(Dictionary<string, UserPermissions>? permissionsMap, HashSet<string>? onlineSet)
    {
        if (permissionsMap == null || onlineSet == null)
            return;

        var result = new List<Friend>();
        foreach(var kvp in permissionsMap)
        {
            var friendCode = kvp.Key;
            var online = onlineSet.Contains(friendCode);
            var permissions = kvp.Value;

            result.Add(new(friendCode, online, permissions));
        }

        Friends = result;
    }

    /// <summary>
    /// Creates a <see cref="Friend"/> and adds them to the friends list
    /// </summary>
    public void CreateOrUpdateFriend(string friendCode, bool online = false)
    {
        CreateOrUpdateFriend(new Friend(friendCode, online));
    }

    /// <summary>
    /// Updates a <see cref="Friend"/> and adds them to the friends list
    /// </summary>
    public void CreateOrUpdateFriend(Friend friend)
    {
        var existing = FindFriend(friend.FriendCode);
        if (existing == null)
        {
            Friends.Add(friend);
        }
        else
        {
            existing.Online = friend.Online;
            existing.Permissions = friend.Permissions;
        }
    }

    /// <summary>
    /// Deletes a friend from the friends list
    /// </summary>
    public void DeleteFriend(string friendCode)
    {
        var friend = FindFriend(friendCode);
        if (friend == null)
            return;

        Friends.Remove(friend);
        OnFriendDeleted?.Invoke(this, new(friend));
    }

    /// <summary>
    /// Finds and returns a <see cref="Friend"/> from the friends list, if they exist in the list
    /// </summary>
    public Friend? FindFriend(string friendCode)
    {
        return Friends.Find(friend => friend.FriendCode == friendCode);
    }

    /// <summary>
    /// Updates a <see cref="Friend"/> online status
    /// </summary>
    public void UpdateFriendOnlineStatus(string friendCode, bool online)
    {
        var friend = FindFriend(friendCode);
        if (friend == null)
            return;

        friend.Online = online;
    }
}
