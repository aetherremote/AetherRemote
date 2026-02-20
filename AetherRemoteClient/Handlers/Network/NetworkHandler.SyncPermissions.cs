using AetherRemoteCommon.Domain.Network.SyncPermissions;

namespace AetherRemoteClient.Handlers.Network;

public partial class NetworkHandler
{
    private void HandleSyncPermissions(SyncPermissionsCommand command)
    {
        if (_friendsListService.Get(command.SenderFriendCode) is not { } friend)
            return;
        
        friend.PermissionsGrantedByFriend = command.PermissionsGrantedBySender;
    }
}