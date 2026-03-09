using System.Threading.Tasks;
using AetherRemoteClient.Utils;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.BodySwap;
using AetherRemoteCommon.Util;

namespace AetherRemoteClient.Handlers.Network;

public partial class NetworkHandler
{
    private async Task<ActionResult<Unit>> HandleBodySwap(BodySwapCommand request)
    {
        Plugin.Log.Verbose($"{request}");
        
        var primary = request.SwapAttributes.ToPrimaryPermissions() | PrimaryPermissions.BodySwap;
        var elevated = request.LockCode is null 
            ? ElevatedPermissions.None 
            : ElevatedPermissions.PermanentTransformation;

        var permissions = new ResolvedPermissions(primary, SpeakPermissions.None, elevated);

        var sender = TryGetFriendWithCorrectPermissions("Body Swap", request.SenderFriendCode, permissions);
        if (sender.Result is not ActionResultEc.Success)
            return ActionResultBuilder.Fail(sender.Result);
        
        if (sender.Value is not { } friend)
            return ActionResultBuilder.Fail(ActionResultEc.ValueNotSet);
        
        // Try to apply the transformation
        var result = await _characterTransformationManager.ApplyFullScaleTransformation(request.CharacterName, request.CharacterWorld, request.SwapAttributes).ConfigureAwait(false);
        if (result is false)
        {
            NotificationHelper.Warning("Something went wrong", $"{friend.NoteOrFriendCode} tried to swap your body, but an error occurred. Type /xllog in chat to find out more.");
            _logService.Custom($"{friend.NoteOrFriendCode} tried to body swap with you, but an internal error occured");
            return ActionResultBuilder.Fail(ActionResultEc.ClientPluginDependency);
        }
        
        // Set the Statuses of everything we applied
        UpdateStatusServicePostBodySwapOrTwinning(friend, request.SwapAttributes);
        
        // Log Success
        _logService.Custom($"{friend.NoteOrFriendCode} swapped your body with {request.CharacterName}'s");
        return ActionResultBuilder.Ok();
    }
}