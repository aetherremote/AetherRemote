using System;
using System.Threading.Tasks;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Services;
using AetherRemoteClient.Utils;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.AddFriend;

namespace AetherRemoteClient.UI.Components.Friends;

/// <summary>
///     Handles events and other tasks for <see cref="FriendsListComponentUi" />
/// </summary>
public class FriendsListComponentUiController(FriendsListService friendsListService, NetworkService networkService)
{
    /// <summary>
    ///     String containing the friend code you intend to add
    /// </summary>
    public string FriendCodeToAdd = string.Empty;

    /// <summary>
    ///     String containing the friend you intend to find
    /// </summary>
    public string SearchText = string.Empty;
    
    /// <summary>
    ///     Filters the friend's list to allow for easier rendering on the UI
    /// </summary>
    public readonly ListFilter<Friend> FriendListFilter = new(friendsListService.Friends, FilterPredicate);

    /// <summary>
    ///     Handles adding a friend to your friends list
    /// </summary>
    public async Task Add()
    {
        if (FriendCodeToAdd == string.Empty)
            return;

        var friend = friendsListService.Get(FriendCodeToAdd);
        if (friend is not null)
            NotificationHelper.Warning("Friend Already Exists", "Unable to add friend because friend already exists");

        var request = new AddFriendRequest(FriendCodeToAdd);
        var response =
            await networkService.InvokeAsync<AddFriendResponse>(HubMethod.AddFriend, request).ConfigureAwait(false);

        if (response.Result is AddFriendEc.Success)
        {
            friendsListService.Add(FriendCodeToAdd, null, response.Online);
            
            // TODO: Switch the selected friend to the one you just added
            
            FriendCodeToAdd = string.Empty;
            NotificationHelper.Success("Successfully Added Friend", $"Successfully added {FriendCodeToAdd} as a friend");
        }
        else
        {
            NotificationHelper.Error("Failed to Add Friend", $"{response.Result}");
        }
    }
    
    private static bool FilterPredicate(Friend friend, string searchTerm)
    {
        return friend.NoteOrFriendCode.Contains(searchTerm, StringComparison.OrdinalIgnoreCase);
    }
}