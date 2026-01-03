using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Enums;
using AetherRemoteClient.Domain.Filters;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteClient.Utils;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.AddFriend;

namespace AetherRemoteClient.UI.Components.Friends;

/// <summary>
///     Handles events and other tasks for <see cref="FriendsListComponentUi" />
/// </summary>
public class FriendsListComponentUiController : IDisposable
{
    private readonly FriendsListService _friendsListService;
    private readonly NetworkService _networkService;
    private readonly SelectionManager _selectionManager;
    
    public readonly FilterFriends Filter;

    public readonly List<Friend> Pending = [];
    
    public FriendsListComponentUiController(FriendsListService friendsListService, NetworkService networkService, SelectionManager selectionManager)
    {
        _friendsListService = friendsListService;
        _networkService = networkService;
        _selectionManager = selectionManager;
        
        Filter = new FilterFriends(() => _friendsListService.Friends);

        _friendsListService.FriendAdded += OnFriendsListChanged;
        _friendsListService.FriendDeleted += OnFriendsListChanged;
        _friendsListService.FriendsListCleared += OnFriendsListChanged;
        
        _selectionManager.FriendsInteractedWith += OnFriendsListChanged;
    }
    
    /// <summary>
    ///     String containing the friend code you intend to add
    /// </summary>
    public string FriendCodeToAdd = string.Empty;

    /// <summary>
    ///     String containing the friend you intend to find
    /// </summary>
    public string SearchText = string.Empty;

    /// <summary>
    ///     Handles adding a friend to your friends list
    /// </summary>
    public async Task Add()
    {
        if (FriendCodeToAdd == string.Empty)
            return;

        // Remove spaces in the beginning or end
        FriendCodeToAdd = FriendCodeToAdd.Trim();

        if (_friendsListService.Contains(FriendCodeToAdd))
        {
            NotificationHelper.Warning("Friend Already Exists", "Unable to add friend because friend already exists");
            return;
        }
        
        var request = new AddFriendRequest(FriendCodeToAdd);
        var response = await _networkService.InvokeAsync<AddFriendResponse>(HubMethod.AddFriend, request).ConfigureAwait(false);

        if (response.Result is AddFriendEc.Success)
        {
            var friend = new Friend(request.TargetFriendCode, response.Online);
            
            _friendsListService.Add(friend);
            
            _selectionManager.Select(friend, false);
            
            FriendCodeToAdd = string.Empty;
            NotificationHelper.Success("Successfully Added Friend", $"Successfully added {FriendCodeToAdd} as a friend");
        }
        else
        {
            NotificationHelper.Error("Failed to Add Friend", $"{response.Result}");
        }
    }

    public void ToggleSortMode()
    {
        Filter.SortMode = Filter.SortMode is FilterSortMode.Alphabetically ? FilterSortMode.Recency : FilterSortMode.Alphabetically;
        Filter.Refresh();
    }
    
    private void OnFriendsListChanged(object? sender, object? _)
    {
        Filter.Refresh();
    }

    public void Dispose()
    {
        _friendsListService.FriendAdded -= OnFriendsListChanged;
        _friendsListService.FriendDeleted -= OnFriendsListChanged;
        _friendsListService.FriendsListCleared -= OnFriendsListChanged;
        
        _selectionManager.FriendsInteractedWith -= OnFriendsListChanged;
        
       GC.SuppressFinalize(this);
    }
}