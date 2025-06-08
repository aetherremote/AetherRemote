using AetherRemoteClient.Services;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.V2.Domain.Network.SyncOnlineStatus;
using AetherRemoteCommon.V2.Domain.Network.SyncPermissions;

namespace AetherRemoteClient.Handlers.Network;

/// <summary>
///     Handles a <see cref="SyncPermissionsForwardedRequest"/>
/// </summary>
public class SyncPermissionsHandler(FriendsListService friendsListService)
{
    /// <summary>
    ///     <inheritdoc cref="SyncOnlineStatusForwardedRequest"/>
    /// </summary>
    public void Handle(SyncPermissionsForwardedRequest forwardedRequest)
    {
        if (friendsListService.Get(forwardedRequest.SenderFriendCode) is not { } friend)
            return;

        friend.PermissionsGrantedByFriend = forwardedRequest.PermissionsGrantedBySender;
    }
}