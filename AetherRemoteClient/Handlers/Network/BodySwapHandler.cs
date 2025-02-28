using System.Threading.Tasks;
using AetherRemoteClient.Domain.Enums;
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

        if (action.SwapMods)
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
        
        // Check if local body is present
        if (await Plugin.RunOnFramework(() => Plugin.ClientState.LocalPlayer is null).ConfigureAwait(false))
        { 
            logService.MissingLocalBody("Body Swap", friend.NoteOrFriendCode);
            return;
        }
        
        // Add attributes from the input
        var attributes = CharacterAttributes.None;
        if (action.SwapMods)
            attributes |= CharacterAttributes.Mods;

        // Actually apply glamourer, mods, etc...
        if (await modManager.Assimilate(action.Identity.GameObjectName, attributes) is false)
            return;

        // Set your new identity
        identityService.Identity = action.Identity.CharacterName;
        
        // Log Success
        logService.Custom($"{friend.NoteOrFriendCode} swapped your body with {action.Identity.CharacterName}'s");
    }
}