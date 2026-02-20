using System.Threading.Tasks;
using AetherRemoteClient.Domain.Enums;
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
        
        var primary = request.SwapAttributes.ToPrimaryPermission() | PrimaryPermissions.BodySwap;
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
        var result = await _characterTransformationManager.ApplyCharacterTransformation(request.CharacterName, request.SwapAttributes);
        if (result.Success is not ApplyCharacterTransformationErrorCode.Success)
        {
            // Log the failure
            Plugin.Log.Warning($"[BodySwapHandler.Handle] Applying a body swap failed, {result.Success}");
            _logService.Custom($"{friend.NoteOrFriendCode} tried to body swap with you, but an internal error occured");
            return ActionResultBuilder.Fail(ActionResultEc.ClientPluginDependency);
        }
        
        // Log Success
        _logService.Custom($"{friend.NoteOrFriendCode} swapped your body with {request.CharacterName}'s");
        return ActionResultBuilder.Ok();
    }
}