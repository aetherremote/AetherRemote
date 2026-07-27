using System.Threading.Tasks;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Network.Domain;
using AetherRemoteCommon.Network.Domain.Payloads;
using AetherRemoteCommon.Network.Enums;
using AetherRemoteCommon.Util;

namespace AetherRemoteClient.Handlers.Network;

public partial class NetworkHandler
{
    private Task<RoutedResponse<NoPayload>> HandleMimicry(RoutedRequest<MimicryPayload> request)
    {
        Plugin.Log.Verbose($"{request}");
        
        if (_friendsListService.Get(request.SenderFriendCode) is not { } sender)
            return Task.FromResult(new RoutedResponse<NoPayload>(RoutedResponseStatus.NotFriends));

        var primary = request.Payload.Attributes.ToPrimaryPermissions() | PrimaryPermissions.Mimicry;
        var elevated = request.Payload.LockCode is null 
            ? ElevatedPermissions.None 
            : ElevatedPermissions.PermanentTransformation;

        var requiredPermissions = new ResolvedPermissions(primary, SpeakPermissions.None, elevated);
        
        if (GetValidationError("Mimicry", sender, requiredPermissions) is { } error)
            return Task.FromResult(new RoutedResponse<NoPayload>(error));

        _logService.Custom($"{sender.NoteOrFriendCode} mimicked your appearance");
        return Task.FromResult(new RoutedResponse<NoPayload>(RoutedResponseStatus.Success));
    }
}