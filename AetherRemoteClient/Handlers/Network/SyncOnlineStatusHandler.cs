using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteCommon.Domain.Network.SyncOnlineStatus;

namespace AetherRemoteClient.Handlers.Network;

/// <summary>
///     Handles a <see cref="SyncOnlineStatusForwardedRequest"/>
/// </summary>
public class SyncOnlineStatusHandler(FriendsListService friendsListService, SelectionManager selectionManager)
{
    /// <summary>
    ///     <inheritdoc cref="SyncOnlineStatusHandler"/>
    /// </summary>
    public void Handle(SyncOnlineStatusForwardedRequest action)
    {
        if (friendsListService.Get(action.SenderFriendCode) is not { } friend)
            return;
        
        friend.Online = action.Online;

        if (!friend.Online)
        {
            selectionManager.Deselect(friend);
            return;
        }

        if (action.Permissions is null)
        {
            Plugin.Log.Warning("[SyncOnlineStatusHandler.Handle] Permissions are not set");
            return;
        }
        
        friend.PermissionsGrantedByFriend = action.Permissions;
    }
}