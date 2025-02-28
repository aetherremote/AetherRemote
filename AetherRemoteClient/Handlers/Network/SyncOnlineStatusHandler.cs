using AetherRemoteClient.Services;
using AetherRemoteCommon.Domain.Network;

namespace AetherRemoteClient.Handlers.Network;

/// <summary>
///     Handles a <see cref="SyncOnlineStatusAction"/>
/// </summary>
public class SyncOnlineStatusHandler(FriendsListService friendsListService)
{
    /// <summary>
    ///     <inheritdoc cref="SyncOnlineStatusHandler"/>
    /// </summary>
    public void Handle(SyncOnlineStatusAction action)
    {
        if (friendsListService.Get(action.SenderFriendCode) is not { } friend)
            return;

        friend.Online = action.Online;

        if (action.Online is false)
        {
            friendsListService.Selected.Remove(friend);
            return;
        }

        if (action.Permissions is null)
        {
            Plugin.Log.Warning("Excepted permissions to not be null when updating an online friend's permissions");
            return;
        }
        
        friend.PermissionsGrantedByFriend = action.Permissions;
    }
}