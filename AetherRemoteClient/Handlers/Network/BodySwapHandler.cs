using System.Threading.Tasks;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.BodySwap;
using AetherRemoteCommon.Util;

namespace AetherRemoteClient.Handlers.Network;

/// <summary>
///     Handles a <see cref="BodySwapForwardedRequest"/>
/// </summary>
public class BodySwapHandler(
    IdentityService identityService,
    LogService logService,
    PermanentLockService permanentLockService,
    PermissionManager permissionManager,
    ModManager modManager,
    PermanentTransformationManager permanentTransformationManager)
{
    // Const
    private const string Operation = "Body Swap";
    
    /// <summary>
    ///     <inheritdoc cref="BodySwapHandler"/>
    /// </summary>
    public async Task<ActionResult<Unit>> Handle(BodySwapForwardedRequest request)
    {
        Plugin.Log.Verbose($"{request}");
        
        if (permanentLockService.IsLocked)
            return ActionResultBuilder.Fail(ActionResultEc.ClientPermanentlyTransformed);
        
        var primary = request.SwapAttributes.ToPrimaryPermission();
        primary |= PrimaryPermissions2.BodySwap;
        
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
            // If there is a lock code present, attempt to lock
            if (request.LockCode is not null)
            {
                permanentTransformationData.UnlockCode = request.LockCode;
                await permanentTransformationManager.Lock(permanentTransformationData);
            }
        }
        
        // Set your new identity
        identityService.Identity = request.CharacterName;
        
        // Log Success
        logService.Custom($"{friend.NoteOrFriendCode} swapped your body with {request.CharacterName}'s");
        return ActionResultBuilder.Ok();
    }
}