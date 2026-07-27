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
    private async Task<RoutedResponse<NoPayload>> HandleBodySwap(RoutedRequest<BodySwapRoutedPayload> request)
    {
        Plugin.Log.Verbose($"{request}");

        if (_friendsListService.Get(request.SenderFriendCode) is not { } sender)
            return new RoutedResponse<NoPayload>(RoutedResponseStatus.NotFriends);
        
        var primary = request.Payload.SwapAttributes.ToPrimaryPermissions() | PrimaryPermissions.BodySwap;
        var elevated = request.Payload.LockCode is null 
            ? ElevatedPermissions.None 
            : ElevatedPermissions.PermanentTransformation;

        var requiredPermissions = new ResolvedPermissions(primary, SpeakPermissions.None, elevated);

        if (GetValidationError("BodySwap", sender, requiredPermissions) is { } error)
            return new RoutedResponse<NoPayload>(error);

        if (await _characterTransformationManager.ApplyFullScaleTransformation(
                request.Payload.CharacterName,
                request.Payload.CharacterWorld,
                request.Payload.SwapAttributes).ConfigureAwait(false) is false)
        {
            return new RoutedResponse<NoPayload>(RoutedResponseStatus.RuntimeError);
        }
        
        // Set the Statuses of everything we applied
        UpdateStatusServicePostBodySwapOrTwinning(sender, request.Payload.SwapAttributes);
        
        // Log Success
        _logService.Custom($"{sender.NoteOrFriendCode} swapped your body with {request.Payload.CharacterName}'s");
        return new RoutedResponse<NoPayload>(RoutedResponseStatus.Success);
    }
}