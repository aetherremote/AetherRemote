using System.Threading.Tasks;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Util;

namespace AetherRemoteClient.Handlers.Network;

/// <summary>
///     handles a <see cref="TwinningAction"/>
/// </summary>
public class TwinningHandler(
    FriendsListService friendsListService,
    IdentityService identityService,
    OverrideService overrideService, 
    LogService logService,
    ModManager modManager)
{
    /// <summary>
    ///     <inheritdoc cref="TwinningAction"/>
    /// </summary>
    public async Task Handle(TwinningAction action)
    {
        // Not friends
        if (friendsListService.Get(action.SenderFriendCode) is not { } friend)
        {
            logService.NotFriends("Twinning", action.SenderFriendCode);
            return;
        }
        
        // Plugin in safe mode
        if (Plugin.Configuration.SafeMode)
        {
            logService.SafeMode("Twinning", friend.NoteOrFriendCode);
            return;
        }

        // Overriding twinning
        if (overrideService.HasActiveOverride(PrimaryPermissions.Twinning))
        {
            logService.Override("Twinning", friend.NoteOrFriendCode);
            return;
        }
        
        // Lacking permissions for twinning
        if (friend.PermissionsGrantedToFriend.Has(PrimaryPermissions.Twinning) is false)
        {
            logService.LackingPermissions("Twinning", friend.NoteOrFriendCode);
            return;
        }
        
        // If mods are an attribute...
        if (action.SwapAttributes.HasFlag(CharacterAttributes.Mods))
        {
            // Overriding mods
            if (overrideService.HasActiveOverride(PrimaryPermissions.Mods))
            {
                logService.Override("Twinning", friend.NoteOrFriendCode);
                return;
            }

            // Lacking permissions for mods
            if (friend.PermissionsGrantedToFriend.Has(PrimaryPermissions.Mods) is false)
            {
                logService.LackingPermissions("Twinning", friend.NoteOrFriendCode);
                return;
            }
        }
        
        // If moodles are an attribute...
        if (action.SwapAttributes.HasFlag(CharacterAttributes.Moodles))
        {
            // Overriding mods
            if (overrideService.HasActiveOverride(PrimaryPermissions.Moodles))
            {
                logService.Override("Twinning", friend.NoteOrFriendCode);
                return;
            }

            // Lacking permissions for mods
            if (friend.PermissionsGrantedToFriend.Has(PrimaryPermissions.Moodles) is false)
            {
                logService.LackingPermissions("Twinning", friend.NoteOrFriendCode);
                return;
            }
        }
        
        // Check if local body is present
        if (await Plugin.RunOnFramework(() => Plugin.ClientState.LocalPlayer is null).ConfigureAwait(false))
        { 
            logService.MissingLocalBody("Twinning", friend.NoteOrFriendCode);
            return;
        }

        // Actually apply glamourer, mods, etc...
        await modManager.Assimilate(action.Identity.GameObjectName, action.SwapAttributes);

        // Set your new identity
        identityService.Identity = action.Identity.CharacterName;
        
        // Log success
        logService.Custom($"{friend.NoteOrFriendCode} twinned you with {action.Identity.CharacterName}");
    }
}