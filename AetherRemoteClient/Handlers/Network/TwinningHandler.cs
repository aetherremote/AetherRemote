using System.Threading.Tasks;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Twinning;
using AetherRemoteCommon.Util;

namespace AetherRemoteClient.Handlers.Network;

/// <summary>
///     handles a <see cref="TwinningForwardedRequest"/>
/// </summary>
public class TwinningHandler(
    IdentityService identityService,
    LogService logService,
    PermanentLockService permanentLockService,
    PermissionManager permissionManager,
    ModManager modManager,
    PermanentTransformationManager permanentTransformationManager)
{
    // Const
    private const string Operation = "Twinning";
    
    /// <summary>
    ///     <inheritdoc cref="TwinningForwardedRequest"/>
    /// </summary>
    public async Task<ActionResult<Unit>> Handle(TwinningForwardedRequest request)
    {
        Plugin.Log.Verbose($"{request}");
        
        if (permanentLockService.IsLocked)
            return ActionResultBuilder.Fail(ActionResultEc.ClientPermanentlyTransformed);
        
        var primary = request.SwapAttributes.ToPrimaryPermission();
        primary |= PrimaryPermissions2.Twinning;
        
        var elevated = request.LockCode is null 
            ? ElevatedPermissions.None 
            : ElevatedPermissions.PermanentTransformation;
        
        var permissions = new UserPermissions(primary, SpeakPermissions2.None, elevated);
        
        var result = permissionManager.GetAndCheckSenderByUserPermissions(Operation, request.SenderFriendCode, permissions);
        if (result.Result is not ActionResultEc.Success)
            return ActionResultBuilder.Fail(result.Result);
        
        if (result.Value is not { } friend)
            return ActionResultBuilder.Fail(ActionResultEc.ValueNotSet);

        // Actually apply glamourer, mods, etc...
        if (await modManager.Assimilate(request.CharacterName, request.SwapAttributes) is { } permanentTransformationData)
        {
            // If there is a lock, save the permanent transformation data
            if (request.LockCode is not null)
            {
                // Save
                permanentTransformationData.UnlockCode = request.LockCode;
                await permanentTransformationManager.Lock(permanentTransformationData);
            }
            
            // Set your new identity
            identityService.AddAlteration(IdentityAlterationType.Twinning, friend.NoteOrFriendCode);
        }
        
        // Log success
        logService.Custom($"{friend.NoteOrFriendCode} twinned you with {request.CharacterName}");
        return ActionResultBuilder.Ok();
    }
}