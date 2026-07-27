using AetherRemoteCommon.Network.Domain;
using AetherRemoteCommon.Network.Domain.Payloads;

namespace AetherRemoteClient.Handlers.Network;

public partial class NetworkHandler
{
    private void HandleSyncPermissions(Message<SyncPermissionsPayload> message)
    {
        if (_friendsListService.Get(message.SenderFriendCode) is not { } friend)
            return;
        
        friend.PermissionsGrantedByFriend = message.Payload.PermissionsGrantedBySender;
    }
}