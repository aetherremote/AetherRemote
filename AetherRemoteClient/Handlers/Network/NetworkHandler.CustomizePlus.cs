using System;
using System.Text;
using System.Threading.Tasks;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Customize;

namespace AetherRemoteClient.Handlers.Network;

public partial class NetworkHandler
{
    private static readonly ResolvedPermissions CustomizePlusPermissions = new(PrimaryPermissions.CustomizePlus, SpeakPermissions.None, ElevatedPermissions.None);
    
    private async Task<ActionResult<Unit>> HandleCustomizePlus(CustomizeCommand request)
    {
        Plugin.Log.Verbose($"{request}");
        
        var sender = TryGetFriendWithCorrectPermissions("Customize+", request.SenderFriendCode, CustomizePlusPermissions);
        if (sender.Result is not ActionResultEc.Success)
            return ActionResultBuilder.Fail(sender.Result);
        
        if (sender.Value is not { } friend)
            return ActionResultBuilder.Fail(ActionResultEc.ValueNotSet);

        try
        {
            var json = Encoding.UTF8.GetString(request.JsonBoneDataBytes);

            if (request.Additive)
            {
                if (await _customizePlusService.ApplyCustomizeAdditive(json).ConfigureAwait(false) is false)
                {
                    Plugin.Log.Warning("[CustomizePlusHandler] Unable to apply customize");
                    return ActionResultBuilder.Fail(ActionResultEc.ClientPluginDependency);
                }
            }
            else
            {
                if (await _customizePlusService.DeleteTemporaryCustomizeAsync().ConfigureAwait(false) is false)
                {
                    Plugin.Log.Warning("[CustomizePlusHandler] Unable to delete existing customize");
                    return ActionResultBuilder.Fail(ActionResultEc.ClientPluginDependency);
                }
                
                if (await _customizePlusService.ApplyCustomizeAsync(json).ConfigureAwait(false) is false)
                {
                    Plugin.Log.Warning("[CustomizePlusHandler] Unable to apply customize");
                    return ActionResultBuilder.Fail(ActionResultEc.ClientPluginDependency);
                }
            }
            
            _statusManager.SetCustomizePlus(friend);
            _logService.Custom($"{friend.NoteOrFriendCode} applied a customize plus template to you");
            return ActionResultBuilder.Ok();
        }
        catch (Exception e)
        {
            _logService.Custom($"{friend.NoteOrFriendCode} tried to apply a customization template to you but failed unexpectedly");
            Plugin.Log.Error($"Unexpected exception while handling customize plus action, {e.Message}");
            return ActionResultBuilder.Fail(ActionResultEc.Unknown);
        }
    }
}