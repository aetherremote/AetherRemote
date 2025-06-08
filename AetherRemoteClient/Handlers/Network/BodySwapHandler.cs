using System.Threading.Tasks;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Util;

namespace AetherRemoteClient.Handlers.Network;

/// <summary>
///     Handles a <see cref="BodySwapAction"/>
/// </summary>
public class BodySwapHandler(
    FriendsListService friendsListService,
    IdentityService identityService,
    OverrideService overrideService,
    LogService logService,
    ModManager modManager)
{
    /// <summary>
    ///     <inheritdoc cref="BodySwapHandler"/>
    /// </summary>
    public async Task Handle(BodySwapAction action)
    {
        // Not friends
        if (friendsListService.Get(action.SenderFriendCode) is not { } friend)
        {
            logService.NotFriends("Body Swap", action.SenderFriendCode);
            return;
        }
        
        // Plugin in safe mode
        if (Plugin.Configuration.SafeMode)
        {
            logService.SafeMode("Body Swap", friend.NoteOrFriendCode);
            return;
        }

        // Overriding body swaps
        if (overrideService.HasActiveOverride(PrimaryPermissions.BodySwap))
        {
            logService.Override("Body Swap", friend.NoteOrFriendCode);
            return;
        }
        
        // Lacking permissions for body swap
        if (friend.PermissionsGrantedToFriend.Has(PrimaryPermissions.BodySwap) is false)
        {
            logService.LackingPermissions("Body Swap", friend.NoteOrFriendCode);
            return;
        }
        
        // If mods are an attribute...
        if (action.SwapAttributes.HasFlag(CharacterAttributes.Mods))
        {
            // Overriding mods
            if (overrideService.HasActiveOverride(PrimaryPermissions.Mods))
            {
                logService.Override("Body Swap", friend.NoteOrFriendCode);
                return;
            }

            // Lacking permissions for mods
            if (friend.PermissionsGrantedToFriend.Has(PrimaryPermissions.Mods) is false)
            {
                logService.LackingPermissions("Body Swap", friend.NoteOrFriendCode);
                return;
            }
        }
        
        // If moodles are an attribute...
        if (action.SwapAttributes.HasFlag(CharacterAttributes.Moodles))
        {
            // Overriding mods
            if (overrideService.HasActiveOverride(PrimaryPermissions.Moodles))
            {
                logService.Override("Body Swap", friend.NoteOrFriendCode);
                return;
            }

            // Lacking permissions for mods
            if (friend.PermissionsGrantedToFriend.Has(PrimaryPermissions.Moodles) is false)
            {
                logService.LackingPermissions("Body Swap", friend.NoteOrFriendCode);
                return;
            }
        }
        
        // If customize plus is an attribute...
        if (action.SwapAttributes.HasFlag(CharacterAttributes.CustomizePlus))
        {
            // Overriding mods
            if (overrideService.HasActiveOverride(PrimaryPermissions.Customize))
            {
                logService.Override("Body Swap", friend.NoteOrFriendCode);
                return;
            }

            // Lacking permissions for mods
            if (friend.PermissionsGrantedToFriend.Has(PrimaryPermissions.Customize) is false)
            {
                logService.LackingPermissions("Body Swap", friend.NoteOrFriendCode);
                return;
            }
        }
        
        // Check if local body is present
        if (await Plugin.RunOnFramework(() => Plugin.ClientState.LocalPlayer is null).ConfigureAwait(false))
        { 
            logService.MissingLocalBody("Body Swap", friend.NoteOrFriendCode);
            return;
        }
        
        // Actually apply glamourer, mods, etc...
        if (await modManager.Assimilate(action.Identity.GameObjectName, action.SwapAttributes) is false)
            return;

        // Set your new identity
        identityService.Identity = action.Identity.CharacterName;
        
        // Log Success
        logService.Custom($"{friend.NoteOrFriendCode} swapped your body with {action.Identity.CharacterName}'s");
    }
}