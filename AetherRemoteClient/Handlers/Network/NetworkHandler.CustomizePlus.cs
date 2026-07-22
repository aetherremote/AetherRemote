using System;
using System.Text;
using System.Threading.Tasks;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Network.Domain;
using AetherRemoteCommon.Network.Domain.Payloads;
using AetherRemoteCommon.Network.Enums;

namespace AetherRemoteClient.Handlers.Network;

public partial class NetworkHandler
{
    private static readonly ResolvedPermissions CustomizePlusPermissions = new(PrimaryPermissions.CustomizePlus, SpeakPermissions.None, ElevatedPermissions.None);
    
    private async Task<RoutedResponse<NoPayload>> HandleCustomizePlus(RoutedRequest<CustomizePlusPayload> request)
    {
        Plugin.Log.Verbose($"{request}");
        
        if (_friendsListService.Get(request.SenderFriendCode) is not { } sender)
            return new RoutedResponse<NoPayload>(RoutedResponseStatus.NotFriends);

        if (GetValidationError("CustomizePlus", sender, CustomizePlusPermissions) is { } error)
            return new RoutedResponse<NoPayload>(error);

        try
        {
            var json = Encoding.UTF8.GetString(request.Payload.JsonBoneDataBytes);
            var success = request.Payload.ApplyMode switch
            {
                CustomizeApplyMode.Default => await _customizePlusService.ApplyCustomize(json).ConfigureAwait(false),
                CustomizeApplyMode.Merge => await _customizePlusService.ApplyMergeCustomize(json).ConfigureAwait(false),
                _ => false
            };

            if (success is false)
            {
                Plugin.Log.Warning("[CustomizePlusHandler] Unable to apply customize");
                return new RoutedResponse<NoPayload>(RoutedResponseStatus.RuntimeError);
            }
            
            _statusService.SetCustomizePlus(sender);
            _logService.Custom($"{sender.NoteOrFriendCode} applied a customize plus template to you");
            return new RoutedResponse<NoPayload>(RoutedResponseStatus.Success);
        }
        catch (Exception e)
        {
            _logService.Custom($"{sender.NoteOrFriendCode} tried to apply a customization template to you but failed unexpectedly");
            Plugin.Log.Error($"Unexpected exception while handling customize plus action, {e.Message}");
            return new  RoutedResponse<NoPayload>(RoutedResponseStatus.Unknown);
        }
    }
}