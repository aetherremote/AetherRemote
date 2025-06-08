using System.Threading.Tasks;
using AetherRemoteClient.Ipc;
using AetherRemoteClient.Services;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Util;
using AetherRemoteCommon.V2.Domain.Network.Transform;

namespace AetherRemoteClient.Handlers.Network;

/// <summary>
///     Handles a <see cref="TransformForwardedRequest"/>
/// </summary>
public class TransformHandler(
    FriendsListService friendsListService, 
    OverrideService overrideService, 
    LogService logService,
    GlamourerIpc glamourerIpc)
{
    /// <summary>
    ///     <inheritdoc cref="TransformHandler"/>
    /// </summary>
    public async Task Handle(TransformForwardedRequest forwardedRequest)
    {
        // Not friends
        if (friendsListService.Get(forwardedRequest.SenderFriendCode) is not { } friend)
        {
            logService.NotFriends("Transform", forwardedRequest.SenderFriendCode);
            return;
        }
        
        // Plugin in safe mode
        if (Plugin.Configuration.SafeMode)
        {
            logService.SafeMode("Transform", friend.NoteOrFriendCode);
            return;
        }

        // Handle customization permissions
        if ((forwardedRequest.GlamourerApplyType & GlamourerApplyFlags.Customization) == GlamourerApplyFlags.Customization)
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
        if ((forwardedRequest.GlamourerApplyType & GlamourerApplyFlags.Equipment) == GlamourerApplyFlags.Equipment)
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
        if (await glamourerIpc.ApplyDesignAsync(forwardedRequest.GlamourerData, forwardedRequest.GlamourerApplyType).ConfigureAwait(false) is false)
        {
            Plugin.Log.Warning($"Failed to handle transformation request from {friend.NoteOrFriendCode}");
            logService.Custom($"{friend.NoteOrFriendCode} tried to transform you, but an unexpected error occured.");
            return;
        }
        
        // Log Success
        logService.Custom($"{friend.NoteOrFriendCode} transformed you");
    }
}