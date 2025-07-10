using System.Threading.Tasks;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Ipc;
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
    LogService logService,
    PermanentLockService permanentLockService,
    GlamourerIpc glamourerIpc,
    ForwardedRequestManager forwardedRequestManager,
    PermanentTransformationManager permanentTransformationManager)
{
    // Const
    private const string Operation = "Transform";
    
    /// <summary>
    ///     <inheritdoc cref="TransformHandler"/>
    /// </summary>
    public async Task<ActionResult<Unit>> Handle(TransformForwardedRequest request)
    {
        if (permanentLockService.CurrentLock is not null)
            return ActionResultBuilder.Fail(ActionResultEc.ClientPermanentlyTransformed);
        
        // Setup permissions
        var primary = request.GlamourerApplyType.ToPrimaryPermission();
        var elevated = request.LockCode is null 
            ? ElevatedPermissions.None 
            : ElevatedPermissions.PermanentTransformation;
        
        // Build permissions
        var permissions = new UserPermissions(primary, SpeakPermissions2.None, elevated);
        
        // Validate Permission
        var placeholder = forwardedRequestManager.Placeholder(Operation, request.SenderFriendCode, permissions);
        if (placeholder.Result is not ActionResultEc.Success)
            return ActionResultBuilder.Fail(placeholder.Result);
        
        if (placeholder.Value is not { } friend)
            return ActionResultBuilder.Fail(ActionResultEc.ValueNotSet);
        
        // Attempt to apply
        if (await glamourerIpc.ApplyDesignAsync(request.GlamourerData, request.GlamourerApplyType).ConfigureAwait(false) is false)
        {
            Plugin.Log.Warning($"Failed to handle transformation request from {friend.NoteOrFriendCode}");
            logService.Custom($"{friend.NoteOrFriendCode} tried to transform you, but an unexpected error occured.");
            return ActionResultBuilder.Fail(ActionResultEc.ClientPluginDependency);
        }
        
        // If there is a lock, save the permanent transformation data
        if (request.LockCode.HasValue)
        {
            // Adds the parts we want to save
            var permanent = new PermanentTransformationData
            {
                GlamourerData = request.GlamourerData,
                GlamourerApplyFlags = request.GlamourerApplyType,
                UnlockCode = request.LockCode.Value
            };

            // Save
            permanentTransformationManager.Save(permanent);
        }
        
        // Log Success
        logService.Custom($"{friend.NoteOrFriendCode} transformed you");
        return ActionResultBuilder.Ok();
    }
}