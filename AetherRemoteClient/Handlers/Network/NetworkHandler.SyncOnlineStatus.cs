using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Network.Domain;
using AetherRemoteCommon.Network.Domain.Payloads;

namespace AetherRemoteClient.Handlers.Network;

public partial class NetworkHandler
{
    private void HandleSyncOnlineStatus(Message<SyncOnlineStatusPayload> message)
    {
        if (_friendsListService.Get(message.SenderFriendCode) is not { } friend)
            return;
        
        friend.Status = message.Payload.Status;
        if (friend.Status is FriendOnlineStatus.Offline)
        {
            _selectionManager.Deselect(friend);
            return;
        }

        if (message.Payload.Permissions is null)
        {
            Plugin.Log.Warning("[SyncOnlineStatusHandler.Handle] Permissions are not set");
            return;
        }
        
        friend.PermissionsGrantedByFriend = message.Payload.Permissions;
    }
}