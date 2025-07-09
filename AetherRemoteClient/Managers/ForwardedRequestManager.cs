using AetherRemoteClient.Domain;
using AetherRemoteClient.Services;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;

namespace AetherRemoteClient.Managers;

/// <summary>
///     TODO
/// </summary>
public class ForwardedRequestManager(FriendsListService friendsListService, LogService logService, PauseService pauseService)
{
    /// <summary>
    ///     TODO
    /// </summary>
    public ActionResult<Friend> Placehold(string operation, string friendCode, PrimaryPermissions2 permissions)
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
        
        // Success, Has Permissions
        if ((friend.PermissionsGrantedToFriend.Primary & permissions) == permissions)
            return ActionResultBuilder.Ok(friend);
        
        // Lacks Permission
        logService.LackingPermissions(operation, friend.NoteOrFriendCode);
        return ActionResultBuilder.Fail<Friend>(ActionResultEc.ClientHasNotGrantedSenderPermissions);
    }

    /// <summary>
    ///     
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="friendCode"></param>
    /// <param name="permissions"></param>
    /// <returns></returns>
    public ActionResult<Friend> Placeholder(string operation, string friendCode, UserPermissions permissions)
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
    
    /// <summary>
    ///     TODO
    /// </summary>
    public ActionResult<Friend> Placehold(string operation, string friendCode, SpeakPermissions2 permissions)
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
        
        // Success, Has Permissions
        if ((friend.PermissionsGrantedToFriend.Speak & permissions) == permissions)
            return ActionResultBuilder.Ok(friend);
        
        // Lacks Permission
        logService.LackingPermissions(operation, friend.NoteOrFriendCode);
        return ActionResultBuilder.Fail<Friend>(ActionResultEc.ClientHasNotGrantedSenderPermissions);
    }
}