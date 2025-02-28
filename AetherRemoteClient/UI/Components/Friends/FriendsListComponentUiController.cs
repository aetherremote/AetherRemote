using System;
using System.Threading.Tasks;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Services;
using AetherRemoteClient.Utils;
using AetherRemoteCommon.Domain.Network;

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
    public ListFilter<Friend> FriendListFilter = new(friendsListService.Friends, FilterPredicate);

    /// <summary>
    ///     Handles adding a friend to your friends list
    /// </summary>
    public async Task Add()
    {
        if (FriendCodeToAdd == string.Empty)
            return;

        var friend = friendsListService.Get(FriendCodeToAdd);
        if (friend is not null)
            Plugin.NotificationManager.AddNotification(NotificationHelper.Warning(
                "Friend Already Exists", "Unable to add friend because friend already exists"));

        var request = new AddFriendRequest { TargetFriendCode = FriendCodeToAdd };
        var result =
            await networkService.InvokeAsync<AddFriendRequest, AddFriendResponse>(HubMethod.AddFriend, request).ConfigureAwait(false);

        if (Plugin.DeveloperMode || result.Success)
        {
            friendsListService.Add(FriendCodeToAdd, null, result.Online);

            Plugin.NotificationManager.AddNotification(NotificationHelper.Success(
                "Successfully Added Friend", $"Successfully added {FriendCodeToAdd} as a friend"));
        }
        else
        {
            Plugin.NotificationManager.AddNotification(NotificationHelper.Error(
                "Failed to Add Friend", $"{result.Message}"));
        }

        FriendCodeToAdd = string.Empty;
    }
    
    private static bool FilterPredicate(Friend friend, string searchTerm)
    {
        return friend.NoteOrFriendCode.Contains(searchTerm, StringComparison.OrdinalIgnoreCase);
    }
}