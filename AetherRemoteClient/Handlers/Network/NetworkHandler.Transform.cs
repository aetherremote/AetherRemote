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
    private async Task<RoutedResponse<NoPayload>> HandleTransform(RoutedRequest<TransformationPayload> request)
    {
        Plugin.Log.Verbose($"{request}");
        
        if (_friendsListService.Get(request.SenderFriendCode) is not { } sender)
            return new RoutedResponse<NoPayload>(RoutedResponseStatus.NotFriends);
        
        var primary = request.Payload.GlamourerApplyType.ToPrimaryPermission();
        var elevated = request.Payload.LockCode is null 
            ? ElevatedPermissions.None 
            : ElevatedPermissions.PermanentTransformation;
        
        var permissions = new ResolvedPermissions(primary, SpeakPermissions.None, elevated);
        
        if (GetValidationError("CustomizePlus", sender, permissions) is { } error)
            return new RoutedResponse<NoPayload>(error);

        if (await _characterTransformationManager.ApplyTransformation(
                request.Payload.GlamourerData,
                request.Payload.GlamourerApplyType,
                sender).ConfigureAwait(false) is false)
        {
            return new RoutedResponse<NoPayload>(RoutedResponseStatus.RuntimeError);
        }
        
        _logService.Custom($"{sender.NoteOrFriendCode} transformed you");
        return new RoutedResponse<NoPayload>(RoutedResponseStatus.Success);
    }
}