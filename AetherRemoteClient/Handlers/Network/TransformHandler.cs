using System.Threading.Tasks;
using AetherRemoteClient.Services;
using AetherRemoteClient.Services.External;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Util;

namespace AetherRemoteClient.Handlers.Network;

/// <summary>
///     Handles a <see cref="TransformAction"/>
/// </summary>
public class TransformHandler(
    FriendsListService friendsListService, 
    GlamourerService glamourerService,
    OverrideService overrideService, 
    LogService logService)
{
    /// <summary>
    ///     <inheritdoc cref="TransformHandler"/>
    /// </summary>
    public async Task Handle(TransformAction action)
    {
        // Not friends
        if (friendsListService.Get(action.SenderFriendCode) is not { } friend)
        {
            logService.NotFriends("Transform", action.SenderFriendCode);
            return;
        }
        
        // Plugin in safe mode
        if (Plugin.Configuration.SafeMode)
        {
            logService.SafeMode("Transform", friend.NoteOrFriendCode);
            return;
        }

        // Handle customization permissions
        if ((action.GlamourerApplyType & GlamourerApplyFlag.Customization) == GlamourerApplyFlag.Customization)
        {
            // Overriding customizations
            if (overrideService.HasActiveOverride(PrimaryPermissions.Customization))
            {
                logService.Override("Transform", friend.NoteOrFriendCode);
                return;
            }

            // Lacking permissions for customizations
            if (friend.PermissionsGrantedToFriend.Has(PrimaryPermissions.Customization) is false)
            {
                logService.LackingPermissions("Transform", friend.NoteOrFriendCode);
                return;
            }
        }

        // Handle equipment permissions
        if ((action.GlamourerApplyType & GlamourerApplyFlag.Equipment) == GlamourerApplyFlag.Equipment)
        {
            // Overriding equipment
            if (overrideService.HasActiveOverride(PrimaryPermissions.Equipment))
            {
                logService.Override("Transform", friend.NoteOrFriendCode);
                return;
            }

            // Lacking permissions for equipment
            if (friend.PermissionsGrantedToFriend.Has(PrimaryPermissions.Equipment) is false)
            {
                logService.LackingPermissions("Transform", friend.NoteOrFriendCode);
                return;
            }
        }
        
        // Check if local body is present
        if (await Plugin.RunOnFramework(() => Plugin.ClientState.LocalPlayer is null).ConfigureAwait(false))
        {
            logService.MissingLocalBody("Transform", friend.NoteOrFriendCode);
            return;
        }
        
        // Attempt to apply
        if (await glamourerService.ApplyDesignAsync(action.GlamourerData, action.GlamourerApplyType).ConfigureAwait(false) is false)
        {
            Plugin.Log.Warning($"Failed to handle transformation request from {friend.NoteOrFriendCode}");
            logService.Custom($"{friend.NoteOrFriendCode} tried to transform you, but an unexpected error occured.");
            return;
        }
        
        // Log Success
        logService.Custom($"{friend.NoteOrFriendCode} transformed you");
    }
}