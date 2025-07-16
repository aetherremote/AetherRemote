using System;
using System.Threading.Tasks;
using AetherRemoteClient.Ipc;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Customize;

namespace AetherRemoteClient.Handlers.Network;

public class CustomizePlusHandler(
    LogService logService,
    CustomizePlusIpc customize,
    PermissionManager permissionManager)
{
    // Const
    private const string Operation = "Customize+";
    private const PrimaryPermissions2 Permissions = PrimaryPermissions2.CustomizePlus;
    
    /// <summary>
    ///     <inheritdoc cref="MoodlesHandler"/>
    /// </summary>
    public async Task<ActionResult<Unit>> Handle(CustomizeForwardedRequest request)
    {
        Plugin.Log.Info($"{request}");
        
        var placeholder = permissionManager.GetAndCheckSenderByPrimaryPermissions(Operation, request.SenderFriendCode, Permissions);
        if (placeholder.Result is not ActionResultEc.Success)
            return ActionResultBuilder.Fail(placeholder.Result);
        
        if (placeholder.Value is not { } friend)
            return ActionResultBuilder.Fail(ActionResultEc.ValueNotSet);

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