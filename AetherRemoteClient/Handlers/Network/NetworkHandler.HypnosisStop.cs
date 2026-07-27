using System.Threading.Tasks;
using AetherRemoteClient.Utils;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Network.Domain;
using AetherRemoteCommon.Network.Domain.Payloads;
using AetherRemoteCommon.Network.Enums;

namespace AetherRemoteClient.Handlers.Network;

public partial class NetworkHandler
{
    private static readonly ResolvedPermissions HypnosisStopPermissions = new(PrimaryPermissions.Hypnosis, SpeakPermissions.None, ElevatedPermissions.None);
    
    private async Task<RoutedResponse<NoPayload>> HandleHypnosisStop(RoutedRequest<HypnosisStopPayload> request)
    {
        Plugin.Log.Verbose($"{request}");
        
        if (_friendsListService.Get(request.SenderFriendCode) is not { } sender)
            return new RoutedResponse<NoPayload>(RoutedResponseStatus.NotFriends);

        if (GetValidationError("HypnosisStop", sender, HypnosisStopPermissions) is { } error)
            return new RoutedResponse<NoPayload>(error);
        
        // If they're not being hypnotized, No-Op
        if (_hypnosisManager.IsBeingHypnotized is false)
            return new RoutedResponse<NoPayload>(RoutedResponseStatus.Success);
        
        // If they're the one who sent the hypnosis request in the first place
        if (_hypnosisManager.Hypnotist?.FriendCode == request.SenderFriendCode)
        {
            await DalamudUtilities.RunOnFramework(() => _hypnosisManager.Wake()).ConfigureAwait(false);
            return new RoutedResponse<NoPayload>(RoutedResponseStatus.Success);
        }

        // Bounce their request
        _logService.Custom($"Rejected hypnosis spiral from {sender.NoteOrFriendCode} because you're already being hypnotized");
        return new RoutedResponse<NoPayload>(RoutedResponseStatus.BeingHypnotized);
    }
}