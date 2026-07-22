using System;
using System.Threading.Tasks;
using AetherRemoteClient.Utils.Extensions;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Network.Domain;
using AetherRemoteCommon.Network.Domain.Payloads;
using AetherRemoteCommon.Network.Enums;

namespace AetherRemoteClient.Handlers.Network;

public partial class NetworkHandler
{
    private static readonly ResolvedPermissions HonorificPermissions = new(PrimaryPermissions.Honorific, SpeakPermissions.None, ElevatedPermissions.None);
    
    private async Task<RoutedResponse<NoPayload>> HandleHonorific(RoutedRequest<HonorificPayload> request)
    {
        Plugin.Log.Verbose($"{request}");
        
        if (_friendsListService.Get(request.SenderFriendCode) is not { } sender)
            return new RoutedResponse<NoPayload>(RoutedResponseStatus.NotFriends);
        
        if (GetValidationError("Honorific", sender, HonorificPermissions) is { } error)
            return new RoutedResponse<NoPayload>(error);
        
        try
        {
            if (await _honorificService.SetCharacterTitle(request.Payload.Data.FromHonorificDto()).ConfigureAwait(false) is false)
                return new RoutedResponse<NoPayload>(RoutedResponseStatus.RuntimeError);
            
            _statusService.SetHonorific(sender);
            
            _logService.Custom($"{sender.NoteOrFriendCode} applied the {request.Payload.Data.Title} honorific to you");
            return new RoutedResponse<NoPayload>(RoutedResponseStatus.Success);
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[HonorificHandler.Handle] {e}");
            _logService.Custom($"{sender.NoteOrFriendCode} unexpectedly failed to apply an honorific to you");
            return new RoutedResponse<NoPayload>(RoutedResponseStatus.Unknown);
        }
    }
}