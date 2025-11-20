using System;
using System.Collections.Generic;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Services;

namespace AetherRemoteClient.Managers;

/// <summary>
///     Manages the selection process and the selected 
/// </summary>
public class SelectionManager : IDisposable
{
    // Injected
    private readonly FriendsListService _friendsListService;
    
    /// <summary>
    ///     Exposed list of who is selected
    /// </summary>
    public IReadOnlySet<Friend> Selected => _selected;
    private readonly HashSet<Friend> _selected = [];
    
    /// <summary>
    ///     Event fired when a friend is selected
    /// </summary>
    public event EventHandler<Friend>? FriendSelected;
    
    /// <summary>
    ///     Event fired when a friend is deselected
    /// </summary>
    public event EventHandler<HashSet<Friend>>? FriendsDeselected;
    
    /// <summary>
    ///     Event fired when something about the selected friends has been modified, such as an internal value
    /// </summary>
    public event EventHandler? FriendsInteractedWith;
    
    /// <summary>
    ///     <inheritdoc cref="SelectionManager"/>
    /// </summary>
    public SelectionManager(FriendsListService friendsListService)
    {
        _friendsListService = friendsListService;
        
        _friendsListService.FriendDeleted += OnFriendDeleted;
        _friendsListService.FriendsListCleared += OnFriendsListCleared;
    }

    /// <summary>
    ///     Check to see if the provided friend is selected
    /// </summary>
    public bool Contains(Friend friend)
    {
        return _selected.Contains(friend);
    }

    /// <summary>
    ///     Select a friend in accordance to the selection mode (single or multi-select), which may toggle a friend's selected status
    /// <remarks>Triggers <see cref="FriendSelected"/> or <see cref="FriendsDeselected"/></remarks>
    /// </summary>
    public void Select(Friend friend, bool multiSelectMode)
    {
        if (multiSelectMode)
        {
            if (_selected.Add(friend))
            {
                FriendSelected?.Invoke(this, friend);
            }
            else
            {
                _selected.Remove(friend);
                FriendsDeselected?.Invoke(this, [friend]);
            }
        }
        else
        {
            if (_selected.Count is 1 && _selected.Contains(friend))
                return;
        
            Clear();
            
            _selected.Add(friend);
            FriendSelected?.Invoke(this, friend);
        }
    }

    /// <summary>
    ///     Deselects a friend
    /// <remarks>Triggers <see cref="FriendsDeselected"/></remarks>
    /// </summary>
    public void Deselect(Friend friend)
    {
        if (_selected.Remove(friend))
            FriendsDeselected?.Invoke(this, [friend]);
    }

    /// <summary>
    ///     Clears the selected list
    /// <remarks>Triggers <see cref="FriendsDeselected"/></remarks>
    /// </summary>
    public void Clear()
    {
        var selected = new HashSet<Friend>(_selected);
        
        _selected.Clear();
        FriendsDeselected?.Invoke(this, selected);
    }

    /// <summary>
    ///     Gets a list of all the friend codes selected, commonly used to get a list of all the targets to send a command to
    /// <remarks>Triggers <see cref="FriendsInteractedWith"/></remarks>
    /// </summary>
    public List<string> GetSelectedFriendCodes()
    {
        var list = new List<string>();
        foreach (var friend in _selected)
        {
            list.Add(friend.FriendCode);
            friend.LastInteractedWith = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
        
        FriendsInteractedWith?.Invoke(this, EventArgs.Empty);
        return list;
    }
    
    private void OnFriendDeleted(object? sender, Friend friend) => Deselect(friend);

    private void OnFriendsListCleared(object? sender, EventArgs e) => Clear();

    public void Dispose()
    {
        _friendsListService.FriendDeleted -= OnFriendDeleted;
        _friendsListService.FriendsListCleared -= OnFriendsListCleared;
        GC.SuppressFinalize(this);
    }
}