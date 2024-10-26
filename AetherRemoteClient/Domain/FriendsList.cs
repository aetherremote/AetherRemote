using AetherRemoteClient.Domain.Events;
using AetherRemoteCommon.Domain;
using Dalamud.Interface.Animation.EasingFunctions;
using System;
using System.Collections.Generic;

namespace AetherRemoteClient.Domain;

/// <summary>
/// Local storage of <see cref="Friend"/> data
/// </summary>
public class FriendsList
{
    /// <summary>
    /// Local <see cref="List{T}"/> of <see cref="Friend"/>
    /// </summary>
    public List<Friend> Friends { get; private set; } = [];

    /// <summary>
    /// Fired when a friend is deleted from the friends list using <see cref="DeleteFriend"/>
    /// </summary>
    public event EventHandler<FriendDeletedEventArgs>? OnFriendDeleted;

    /// <summary>
    /// Fired when the friends list is cleared
    /// </summary>
    public event EventHandler<FriendsListDeletedEventArgs>? OnFriendsListCleared;

    /// <summary>
    /// Converts a permission map from the server into friends list format
    /// </summary>
    public void ConvertServerPermissionsToLocal(
        Dictionary<string, UserPermissions>? permissionsGrantedToOthers,
        Dictionary<string, UserPermissions>? permissionsGrantedByOthers)
    {
        if (permissionsGrantedToOthers == null || permissionsGrantedByOthers == null)
            return;

        Friends.Clear();
        foreach(var kvp in permissionsGrantedToOthers)
        {
            var friendCode = kvp.Key;
            var permissionsGrantedToFriend = kvp.Value;
            var online = permissionsGrantedByOthers.TryGetValue(friendCode, out var permissionsGrantedByFriend);
            Friends.Add(new(friendCode, online, permissionsGrantedToFriend, permissionsGrantedByFriend));
        }
    }

    /// <summary>
    /// Creates a <see cref="Friend"/> and adds them to the friends list
    /// </summary>
    public void CreateFriend(string friendCode, bool online, UserPermissions permissionsGrantedByFriend = UserPermissions.None)
    {
        var existing = FindFriend(friendCode);
        if (existing == null)
            Friends.Add(new Friend(friendCode, online, UserPermissions.None, permissionsGrantedByFriend));
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
    /// Clears the friends list
    /// </summary>
    public void Clear()
    {
        Friends.Clear();
        OnFriendsListCleared?.Invoke(this, new());
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

    /// <summary>
    /// Updates the permissions another <see cref="Friend"/> gives the user
    /// </summary>
    public void UpdateLocalPermissions(string friendCode, UserPermissions permissions)
    {
        var friend = FindFriend(friendCode);
        if (friend == null)
            return;

        friend.PermissionsGrantedByFriend = permissions;
    }
}
