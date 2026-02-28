using System.Threading.Tasks;
using AetherRemoteClient.Domain.Enums;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Transform;
using AetherRemoteCommon.Util;

namespace AetherRemoteClient.Handlers.Network;

public partial class NetworkHandler
{
    private async Task<ActionResult<Unit>> HandleTransform(TransformCommand request)
    {
        Plugin.Log.Verbose($"{request}");
        
        // Setup permissions
        var primary = request.GlamourerApplyType.ToPrimaryPermission();
        var elevated = request.LockCode is null 
            ? ElevatedPermissions.None 
            : ElevatedPermissions.PermanentTransformation;
        
        // Build permissions
        var permissions = new ResolvedPermissions(primary, SpeakPermissions.None, elevated);
        
        var sender = TryGetFriendWithCorrectPermissions("Transform", request.SenderFriendCode, permissions);
        if (sender.Result is not ActionResultEc.Success)
            return ActionResultBuilder.Fail(sender.Result);
        
        if (sender.Value is not { } friend)
            return ActionResultBuilder.Fail(ActionResultEc.ValueNotSet);
        
        // Try to apply the transformation
        var result = await _characterTransformationManager.ApplyGenericTransformation(request.GlamourerData, request.GlamourerApplyType);
        if (result.Success is not ApplyGenericTransformationErrorCode.Success)
        {
            // Log the failure
            Plugin.Log.Warning($"[TransformHandler.Handle] Applying a transformation failed, {result.Success}");
            _logService.Custom($"{friend.NoteOrFriendCode} tried to transform you, but an internal error occured");
            return ActionResultBuilder.Fail(ActionResultEc.ClientPluginDependency);
        }
        
        // Log the success
        _statusManager.SetGlamourerPenumbra(friend);
        _logService.Custom($"{friend.NoteOrFriendCode} transformed you");
        return ActionResultBuilder.Ok();
    }
}