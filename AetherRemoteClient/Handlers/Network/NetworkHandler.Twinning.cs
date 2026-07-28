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
    private async Task<RoutedResponse<NoPayload>> HandleTwinning(RoutedRequest<TwinningPayload> request)
    {
        Plugin.Log.Verbose($"{request}");
        
        if (_friendsListService.Get(request.SenderFriendCode) is not { } sender)
            return new RoutedResponse<NoPayload>(RoutedResponseStatus.NotFriends);
        
        var primary = request.Payload.SwapAttributes.ToPrimaryPermissions() | PrimaryPermissions.Twinning;
        var elevated = request.Payload.LockCode is null 
            ? ElevatedPermissions.None 
            : ElevatedPermissions.PermanentTransformation;
        
        var requiredPermissions = new ResolvedPermissions(primary, SpeakPermissions.None, elevated);
        
        if (GetValidationError("Twinning", sender, requiredPermissions) is { } error)
            return new RoutedResponse<NoPayload>(error);

        if (await _characterTransformationManager.ApplyFullScaleTransformation(
                request.Payload.CharacterName,
                request.Payload.CharacterWorld,
                request.Payload.SwapAttributes,
                sender).ConfigureAwait(false) is false)
        {
            return new RoutedResponse<NoPayload>(RoutedResponseStatus.RuntimeError);
        }
        
        // Log success
        _logService.Custom($"{sender.NoteOrFriendCode} twinned you with {request.Payload.CharacterName}");
        return new RoutedResponse<NoPayload>(RoutedResponseStatus.Success);
    }
}