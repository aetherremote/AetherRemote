using System.Threading.Tasks;
using AetherRemoteClient.Domain.Enums;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Twinning;
using AetherRemoteCommon.Util;

namespace AetherRemoteClient.Handlers.Network;

public partial class NetworkHandler
{
    private async Task<ActionResult<Unit>> HandleTwinning(TwinningCommand request)
    {
        Plugin.Log.Verbose($"{request}");
        
        var primary = request.SwapAttributes.ToPrimaryPermission() | PrimaryPermissions.Twinning;
        var elevated = request.LockCode is null 
            ? ElevatedPermissions.None 
            : ElevatedPermissions.PermanentTransformation;
        
        var permissions = new ResolvedPermissions(primary, SpeakPermissions.None, elevated);
        
        var sender = TryGetFriendWithCorrectPermissions("Twinning", request.SenderFriendCode, permissions);
        if (sender.Result is not ActionResultEc.Success)
            return ActionResultBuilder.Fail(sender.Result);
        
        if (sender.Value is not { } friend)
            return ActionResultBuilder.Fail(ActionResultEc.ValueNotSet);
        
        // Try to apply the transformation
        var result = await _characterTransformationManager.ApplyCharacterTransformation(request.CharacterName, request.SwapAttributes);
        if (result.Success is not ApplyCharacterTransformationErrorCode.Success)
        {
            // Log the failure
            Plugin.Log.Warning($"[TwinningHandler.Handle] Applying a twinning failed, {result.Success}");
            _logService.Custom($"{friend.NoteOrFriendCode} tried to twin with you, but an internal error occured");
            return ActionResultBuilder.Fail(ActionResultEc.ClientPluginDependency);
        }
        
        // Log success
        _logService.Custom($"{friend.NoteOrFriendCode} twinned you with {request.CharacterName}");
        return ActionResultBuilder.Ok();
    }
}