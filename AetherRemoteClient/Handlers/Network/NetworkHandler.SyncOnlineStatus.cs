using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network.SyncOnlineStatus;

namespace AetherRemoteClient.Handlers.Network;

public partial class NetworkHandler
{
    /// <summary>
    ///     <inheritdoc cref="SyncOnlineStatusHandler"/>
    /// </summary>
    private void HandleSyncOnlineStatus(SyncOnlineStatusCommand action)
    {
        if (_friendsListService.Get(action.SenderFriendCode) is not { } friend)
            return;
        
        friend.Status = action.Status;

        if (friend.Status is FriendOnlineStatus.Offline)
        {
            _selectionManager.Deselect(friend);
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