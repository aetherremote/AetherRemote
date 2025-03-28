using System;
using System.Collections.Generic;
using System.Linq;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Events;
using AetherRemoteCommon.Domain.Enums;
using ImGuiNET;

namespace AetherRemoteClient.Services;

/// <summary>
///     Class responsible for processing any changes to the friend's list
/// </summary>
public class FriendsListService
{
    /// <summary>
    ///     List of all friends. Never directly modify this list, use the service methods.
    /// </summary>
    public readonly List<Friend> Friends = [];

    /// <summary>
    ///     Set of all selected friends
    /// </summary>
    public readonly HashSet<Friend> Selected = [];

    /// <summary>
    ///     Event fired when a friend is deleted from the friend's list
    /// </summary>
    public event EventHandler<FriendDeletedEventArgs>? FriendDeletedEvent;

    /// <summary>
    ///     Event fired when the friend's list is deleted
    /// </summary>
    public event EventHandler<FriendsListDeletedEventArgs>? FriendsListDeletedEvent;

    /// <summary>
    ///     Event fired when the selected targets have changed
    /// </summary>
    public event EventHandler<SelectedChangedEventArgs>? SelectedChangedEvent;

    /// <summary>
    ///     Attempts to get a friend by friend code
    /// </summary>
    public Friend? Get(string friendCode) => Friends.FirstOrDefault(friend => friend.FriendCode == friendCode);

    /// <summary>
    ///     Adds a friend locally
    /// </summary>
    public void Add(string friendCode, string? note, bool online) => Friends.Add(new Friend(friendCode, note, online));
    
    /// <summary>
    ///     Adds a friend locally
    /// </summary>
    public void Add(Friend friend) => Friends.Add(friend);

    /// <summary>
    ///     <inheritdoc cref="FriendsListService"/>
    /// </summary>
    public FriendsListService()
    {
        if (Plugin.DeveloperMode is false)
            return;
        
        var onlineFriend = new Friend("Friend 1", null, true);
        onlineFriend.PermissionsGrantedByFriend.Primary |= PrimaryPermissions.Say | PrimaryPermissions.Speak;
        Friends.Add(onlineFriend);
        
        var offlineFriend = new Friend("Friend 2");
        Friends.Add(offlineFriend);
        
        Selected.Add(onlineFriend);
    }
    
    /// <summary>
    ///     Deletes a friend locally
    /// </summary>
    public void Delete(Friend friend)
    {
        Friends.Remove(friend);

        if (Selected.Remove(friend))
            SelectedChangedEvent?.Invoke(this, new SelectedChangedEventArgs(Selected));

        FriendDeletedEvent?.Invoke(this, new FriendDeletedEventArgs(friend));
    }

    /// <summary>
    ///     Clears the friend's list of all friends
    /// </summary>
    public void Clear()
    {
        Friends.Clear();
        FriendsListDeletedEvent?.Invoke(this, new FriendsListDeletedEventArgs());
    }

    /// <summary>
    ///     Selects a friend, and if control is held, selects multiple
    /// </summary>
    public void Select(Friend friend)
    {
        if (ImGui.GetIO().KeyCtrl)
        {
            if (Selected.Add(friend) is false)
                Selected.Remove(friend);
        }
        else
        {
            if (Selected.Count is 1 && Selected.Contains(friend))
                return;

            Selected.Clear();
            Selected.Add(friend);
        }

        SelectedChangedEvent?.Invoke(this, new SelectedChangedEventArgs(Selected));
    }

    /// <summary>
    ///     Removes any currently offline friends from your currently selected list
    /// </summary>
    public void PurgeOfflineFriendsFromSelect()
    {
        foreach (var friend in Selected)
            if (friend.Online is false)
                Selected.Remove(friend);
    }
}