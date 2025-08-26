using AetherRemoteClient.Domain;
using AetherRemoteClient.Services;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network;

namespace AetherRemoteClient.Managers;

/// <summary>
///     Manager to provide an all-in-one-place permission checking suite of functions
/// </summary>
/// <param name="friendsListService"></param>
/// <param name="logService"></param>
/// <param name="pauseService"></param>
public class PermissionsCheckerManager(FriendsListService friendsListService, LogService logService, PauseService pauseService)
{
    /// <summary>
    ///     Retrieves a friend and checks to see if you contain the provided permissions with them
    /// </summary>
    /// <param name="operation">The name of the operation we're checking</param>
    /// <param name="friendCode">The friend code to retrieve</param>
    /// <param name="permissions">The permissions to test</param>
    /// <returns>The <see cref="Friend"/> corresponding to the friend code provided</returns>
    public ActionResult<Friend> GetSenderAndCheckPermissions(string operation, string friendCode, UserPermissions permissions)
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
        
        // Test Primary Permissions
        if ((friend.PermissionsGrantedToFriend.Primary & permissions.Primary) != permissions.Primary)
        {
            logService.LackingPermissions(operation, friend.NoteOrFriendCode);
            return ActionResultBuilder.Fail<Friend>(ActionResultEc.ClientHasNotGrantedSenderPermissions);
        }
        
        // Test Speak Permissions
        if ((friend.PermissionsGrantedToFriend.Speak & permissions.Speak) != permissions.Speak)
        {
            logService.LackingPermissions(operation, friend.NoteOrFriendCode);
            return ActionResultBuilder.Fail<Friend>(ActionResultEc.ClientHasNotGrantedSenderPermissions);
        }
        
        // Test Elevated Permissions
        if ((friend.PermissionsGrantedToFriend.Elevated & permissions.Elevated) != permissions.Elevated)
        {
            logService.LackingPermissions(operation, friend.NoteOrFriendCode);
            return ActionResultBuilder.Fail<Friend>(ActionResultEc.ClientHasNotGrantedSenderPermissions);
        }
        
        return ActionResultBuilder.Ok(friend);
    }
}