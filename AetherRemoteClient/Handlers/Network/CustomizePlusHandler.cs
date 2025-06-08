using System;
using System.Threading.Tasks;
using AetherRemoteClient.Ipc;
using AetherRemoteClient.Services;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Util;
using AetherRemoteCommon.V2.Domain;
using AetherRemoteCommon.V2.Domain.Enum;
using AetherRemoteCommon.V2.Domain.Network.Customize;

namespace AetherRemoteClient.Handlers.Network;

public class CustomizePlusHandler(
    FriendsListService friendsListService,
    OverrideService overrideService,
    LogService logService,
    CustomizePlusIpc customize)
{
    /// <summary>
    ///     <inheritdoc cref="MoodlesHandler"/>
    /// </summary>
    public async Task<ActionResult<Unit>> Handle(CustomizeForwardedRequest request)
    {
        Plugin.Log.Info($"{request}");
        
        // Not friends
        if (friendsListService.Get(request.SenderFriendCode) is not { } friend)
        {
            logService.NotFriends("CustomizePlus", request.SenderFriendCode);
            return ActionResultBuilder.Fail(ActionResultEc.ClientNotFriends);
        }

        // Plugin in safe mode
        if (Plugin.Configuration.SafeMode)
        {
            logService.SafeMode("CustomizePlus", friend.NoteOrFriendCode);
            return ActionResultBuilder.Fail(ActionResultEc.ClientInSafeMode);
        }

        // Overriding moodles
        if (overrideService.HasActiveOverride(PrimaryPermissions.Customize))
        {
            logService.Override("CustomizePlus", friend.NoteOrFriendCode);
            return ActionResultBuilder.Fail(ActionResultEc.ClientHasOverride);
        }

        // Lacking permissions for moodles
        if (friend.PermissionsGrantedToFriend.Has(PrimaryPermissions.Customize) is false)
        {
            logService.LackingPermissions("CustomizePlus", friend.NoteOrFriendCode);
            return ActionResultBuilder.Fail(ActionResultEc.ClientHasNotGrantedSenderPermissions);
        }

        try
        {
            if (await customize.DeleteCustomize().ConfigureAwait(false) is false)
            {
                Plugin.Log.Warning("[CustomizePlusHandler] Unable to delete existing customize");
                return ActionResultBuilder.Fail(ActionResultEc.ClientPluginDependency);
            }
            
            if (await customize.ApplyCustomize(request.Data).ConfigureAwait(false) is false)
            {
                Plugin.Log.Warning("[CustomizePlusHandler] Unable to apply customize");
                return ActionResultBuilder.Fail(ActionResultEc.ClientPluginDependency);
            }
            
            logService.Custom($"{friend.NoteOrFriendCode} applied a customize plus template to you");
            return ActionResultBuilder.Ok();
        }
        catch (Exception e)
        {
            logService.Custom($"{friend.NoteOrFriendCode} tried to apply a customization template to you but failed unexpectedly");
            Plugin.Log.Error($"Unexpected exception while handling customize plus action, {e.Message}");
            return ActionResultBuilder.Fail(ActionResultEc.Unknown);
        }
    }
}