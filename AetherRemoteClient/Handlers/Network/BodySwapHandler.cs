using System.Threading.Tasks;
using AetherRemoteClient.Domain;
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
    CharacterTransformationService characterTransformationService,
    IdentityService identityService,
    LogService logService,
    PermissionManager permissionManager,
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
        
        if (permanentTransformationManager.IsPermanentTransformed)
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

        // TODO: Error handling
        if (request.LockCode is not null)
        {
            await permanentTransformationManager.ApplyPermanentCharacterTransformation(friend.NoteOrFriendCode,
                request.LockCode, request.CharacterName, request.SwapAttributes);
        }
        else
        {
            await characterTransformationService.ApplyCharacterTransformation(request.CharacterName, request.SwapAttributes);
        }
        
        // Set your new identity
        identityService.AddAlteration(IdentityAlterationType.BodySwap, friend.NoteOrFriendCode);
        
        // Log Success
        logService.Custom($"{friend.NoteOrFriendCode} swapped your body with {request.CharacterName}'s");
        return ActionResultBuilder.Ok();
    }
}