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
    IdentityService identityService,
    LogService logService,
    PermanentLockService permanentLockService,
    GlamourerIpc glamourerIpc,
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
        
        if (permanentLockService.IsLocked)
            return ActionResultBuilder.Fail(ActionResultEc.ClientPermanentlyTransformed);
        
        // Setup permissions
        var primary = request.GlamourerApplyType.ToPrimaryPermission();
        var elevated = request.LockCode is null 
            ? ElevatedPermissions.None 
            : ElevatedPermissions.PermanentTransformation;
        
        // Build permissions
        var permissions = new UserPermissions(primary, SpeakPermissions2.None, elevated);
        
        // Validate Permission
        var result = permissionManager.GetAndCheckSenderByUserPermissions(Operation, request.SenderFriendCode, permissions);
        if (result.Result is not ActionResultEc.Success)
            return ActionResultBuilder.Fail(result.Result);
        
        if (result.Value is not { } friend)
            return ActionResultBuilder.Fail(ActionResultEc.ValueNotSet);
        
        // Convert to JObject
        if (GlamourerIpc.ConvertGlamourerBase64StringToJObject(request.GlamourerData) is not { } converted)
        {
            Plugin.Log.Warning("[GlamourerIpc] Unable to serialize string JObject");
            return ActionResultBuilder.Fail(ActionResultEc.ClientPluginDependency);
        }
        
        // Get local character data
        if (await glamourerIpc.GetDesignComponentsAsync().ConfigureAwait(false) is not { } local)
        {
            Plugin.Log.Warning("[GlamourerIpc] Unable to get local player design components");
            return ActionResultBuilder.Fail(ActionResultEc.ClientPluginDependency);
        }
        
        // Merge the required advanced dyes to reset
        GlamourerIpc.ModifyJObjectToRevertExistingAdvancedDyes(local, converted);
        
        // Apply the newly converted design
        if (await glamourerIpc.ApplyDesignAsync(converted, request.GlamourerApplyType).ConfigureAwait(false) is false)
        {
            Plugin.Log.Warning($"[TransformHandler] Failed to process transformation request from {friend.NoteOrFriendCode}");
            logService.Custom($"{friend.NoteOrFriendCode} tried to transform you, but an unexpected error occured.");
            return ActionResultBuilder.Fail(ActionResultEc.ClientPluginDependency);
        }
        
        // If there is a lock, save the permanent transformation data
        if (request.LockCode is not null)
        {
            // Adds the parts we want to save
            var permanent = new PermanentTransformationData
            {
                Sender = result.Value.NoteOrFriendCode,
                AlterationType = IdentityAlterationType.Transformation,
                GlamourerData = converted,
                UnlockCode = request.LockCode
            };

            // Save
            await permanentTransformationManager.Lock(permanent);
        }
        
        // Set your new identity
        identityService.AddAlteration(IdentityAlterationType.Transformation, friend.NoteOrFriendCode);
        
        // Log Success
        logService.Custom($"{friend.NoteOrFriendCode} transformed you");
        return ActionResultBuilder.Ok();
    }
}