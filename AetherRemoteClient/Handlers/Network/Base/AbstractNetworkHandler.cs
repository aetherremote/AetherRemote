using AetherRemoteClient.Domain;
using AetherRemoteClient.Services;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Util;

namespace AetherRemoteClient.Handlers.Network.Base;

public abstract class AbstractNetworkHandler(AccountService account, FriendsListService friendsListService, LogService logService, PauseService pauseService)
{
    protected ActionResult<Friend> TryGetFriendWithCorrectPermissions(string operation, string friendCode, ResolvedPermissions permissions)
    {
        // Not friends
        if (friendsListService.Get(friendCode) is not { } friend)
        {
            logService.NotFriends(operation, friendCode);
            return ActionResultBuilder.Fail<Friend>(ActionResultEc.ClientNotFriends);
        }
        
        // Plugin in safe mode
        if (Plugin.Configuration.SafeMode)
        {
            logService.SafeMode(operation, friend.NoteOrFriendCode);
            return ActionResultBuilder.Fail<Friend>(ActionResultEc.ClientInSafeMode);
        }

        // Friend Paused
        if (pauseService.IsFriendPaused(friend.FriendCode))
        {
            logService.FriendPaused(operation, friend.NoteOrFriendCode);
            return ActionResultBuilder.Fail<Friend>(ActionResultEc.ClientHasSenderPaused);
        }

        // Feature Paused
        if (pauseService.IsFeaturePaused(permissions))
        {
            logService.FeaturePaused(operation, friend.NoteOrFriendCode);
            return ActionResultBuilder.Fail<Friend>(ActionResultEc.ClientHasFeaturePaused);
        }
        
        // Resolve
        var resolved = PermissionResolver.Resolve(account.GlobalPermissions, friend.PermissionsGrantedToFriend);
        
        // Test Primary Permissions
        if ((resolved.Primary & permissions.Primary) != permissions.Primary)
        {
            logService.LackingPermissions(operation, friend.NoteOrFriendCode);
            return ActionResultBuilder.Fail<Friend>(ActionResultEc.ClientHasNotGrantedSenderPermissions);
        }
        
        // Test Speak Permissions
        if ((resolved.Speak & permissions.Speak) != permissions.Speak)
        {
            logService.LackingPermissions(operation, friend.NoteOrFriendCode);
            return ActionResultBuilder.Fail<Friend>(ActionResultEc.ClientHasNotGrantedSenderPermissions);
        }
        
        // Test Elevated Permissions
        if ((resolved.Elevated & permissions.Elevated) != permissions.Elevated)
        {
            logService.LackingPermissions(operation, friend.NoteOrFriendCode);
            return ActionResultBuilder.Fail<Friend>(ActionResultEc.ClientHasNotGrantedSenderPermissions);
        }
        
        return ActionResultBuilder.Ok(friend);
    }
}