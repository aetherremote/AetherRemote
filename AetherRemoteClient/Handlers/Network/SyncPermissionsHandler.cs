using AetherRemoteClient.Services;
using AetherRemoteCommon.Domain.Network;

namespace AetherRemoteClient.Handlers.Network;

/// <summary>
///     Handles a <see cref="SyncPermissionsAction"/>
/// </summary>
public class SyncPermissionsHandler(FriendsListService friendsListService)
{
    /// <summary>
    ///     <inheritdoc cref="SyncOnlineStatusAction"/>
    /// </summary>
    public void Handle(SyncPermissionsAction action)
    {
        if (friendsListService.Get(action.SenderFriendCode) is not { } friend)
            return;

        friend.PermissionsGrantedByFriend = action.PermissionsGrantedBySender;
    }
}