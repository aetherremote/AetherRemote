using System.Threading.Tasks;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Transform;
using AetherRemoteCommon.Util;

namespace AetherRemoteClient.Handlers.Network;

/// <summary>
///     Handles a <see cref="TransformForwardedRequest"/>
/// </summary>
public class TransformHandler(
    CharacterTransformationService characterTransformationService,
    IdentityService identityService,
    LogService logService,
    PermissionManager permissionManager,
    PermanentTransformationManager permanentTransformationManager)
{
    // Const
    private const string Operation = "Transform";
    
    /// <summary>
    ///     <inheritdoc cref="TransformHandler"/>
    /// </summary>
    public async Task<ActionResult<Unit>> Handle(TransformForwardedRequest request)
    {
        Plugin.Log.Verbose($"{request}");
        
        if (permanentTransformationManager.IsPermanentTransformed)
            return ActionResultBuilder.Fail(ActionResultEc.ClientPermanentlyTransformed);
        
        // Setup permissions
        var primary = request.GlamourerApplyType.ToPrimaryPermission();
        var elevated = request.LockCode is null 
            ? ElevatedPermissions.None 
            : ElevatedPermissions.PermanentTransformation;
        
        // Build permissions
        var permissions = new UserPermissions(primary, SpeakPermissions2.None, elevated);
        
        // Validate Permission
        var friendActionResult = permissionManager.GetAndCheckSenderByUserPermissions(Operation, request.SenderFriendCode, permissions);
        if (friendActionResult.Result is not ActionResultEc.Success)
            return ActionResultBuilder.Fail(friendActionResult.Result);
        
        if (friendActionResult.Value is not { } friend)
            return ActionResultBuilder.Fail(ActionResultEc.ValueNotSet);

        // TODO: Error handling
        if (request.LockCode is not null)
        {
            await permanentTransformationManager.ApplyPermanentTransformation(friend.NoteOrFriendCode, request.LockCode,
                request.GlamourerData, request.GlamourerApplyType);
        }
        else
        {
            await characterTransformationService.ApplyGenericTransformation(request.GlamourerData, request.GlamourerApplyType);
        }
        
        // Set your new identity
        identityService.AddAlteration(IdentityAlterationType.Transformation, friend.NoteOrFriendCode);
        
        // Log Success
        logService.Custom($"{friend.NoteOrFriendCode} transformed you");
        return ActionResultBuilder.Ok();
    }
}