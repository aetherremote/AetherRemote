using System.Threading.Tasks;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Network.Domain;
using AetherRemoteCommon.Network.Domain.Payloads;
using AetherRemoteCommon.Network.Enums;

namespace AetherRemoteClient.Handlers.Network;

public partial class NetworkHandler
{
    private static readonly ResolvedPermissions HypnosisPermissions = new(PrimaryPermissions.Hypnosis, SpeakPermissions.None, ElevatedPermissions.None);
    
    private async Task<RoutedResponse<NoPayload>> HandleHypnosis(RoutedRequest<HypnosisPayload> request)
    {
        Plugin.Log.Verbose($"{request}");

        if (_friendsListService.Get(request.SenderFriendCode) is not { } sender)
            return new RoutedResponse<NoPayload>(RoutedResponseStatus.NotFriends);
        
        if (GetValidationError("Hypnosis", sender, HypnosisPermissions) is { } error)
            return new RoutedResponse<NoPayload>(error);
        
        if (_hypnosisManager.IsBeingHypnotized)
        {
            // If the sender is the one who initiated it, do nothing
            if (_hypnosisManager.Hypnotist?.FriendCode == request.SenderFriendCode)
            {
            }
            else
            {
                // Bounce their request
                _logService.Custom($"Rejected hypnosis spiral from {sender.NoteOrFriendCode} because you're already being hypnotized");
                return new RoutedResponse<NoPayload>(RoutedResponseStatus.BeingHypnotized);
            }
        }
        
        // Begin the hypnosis
        await _hypnosisManager.Hypnotize(sender, request.Payload.Data);
        
        // Log
        _statusService.SetHypnosis(sender);
        _logService.Custom($"{sender.NoteOrFriendCode} began to hypnotize you");
        return new RoutedResponse<NoPayload>(RoutedResponseStatus.Success);
    }
}