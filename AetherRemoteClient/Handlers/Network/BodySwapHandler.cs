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

// ReSharper disable once ConvertToPrimaryConstructor

/// <summary>
///     Handles a <see cref="BodySwapForwardedRequest"/>
/// </summary>
public class BodySwapHandler(IdentityService identityService, LogService logService, CharacterTransformationManager characterTransformationManager, PermissionsCheckerManager permissionsCheckerManager)
{
    // Const
    private const string Operation = "Body Swap";
    
    /// <summary>
    ///     <inheritdoc cref="BodySwapHandler"/>
    /// </summary>
    public async Task<ActionResult<Unit>> Handle(BodySwapForwardedRequest request)
    {
        Plugin.Log.Verbose($"{request}");
        
        // TODO: Re-enable when a mare solution is found
        // if (_permanentTransformationHandler.IsPermanentTransformed)
        //    return ActionResultBuilder.Fail(ActionResultEc.ClientPermanentlyTransformed);
        
        var primary = request.SwapAttributes.ToPrimaryPermission();
        primary |= PrimaryPermissions2.BodySwap;
        
        var elevated = request.LockCode is null 
            ? ElevatedPermissions.None 
            : ElevatedPermissions.PermanentTransformation;

        var permissions = new UserPermissions(primary, SpeakPermissions2.None, elevated);

        var result = permissionsCheckerManager.GetSenderAndCheckPermissions(Operation, request.SenderFriendCode, permissions);
        if (result.Result is not ActionResultEc.Success)
            return ActionResultBuilder.Fail(result.Result);
        
        if (result.Value is not { } friend)
            return ActionResultBuilder.Fail(ActionResultEc.ValueNotSet);

        // TODO: Re-enable when a mare solution is found
        /*
        if (request.LockCode is not null)
        {
            await _permanentTransformationHandler.ApplyPermanentCharacterTransformation(friend.NoteOrFriendCode,
                request.LockCode, request.CharacterName, request.SwapAttributes);
        }
        else
        {
            await _characterTransformationManager.ApplyCharacterTransformation(request.CharacterName, request.SwapAttributes);
        }
        */
        
        await characterTransformationManager.ApplyCharacterTransformation(request.CharacterName, request.SwapAttributes);
        
        // Set your new identity
        identityService.AddAlteration(IdentityAlterationType.BodySwap, friend.NoteOrFriendCode);
        
        // Log Success
        logService.Custom($"{friend.NoteOrFriendCode} swapped your body with {request.CharacterName}'s");
        return ActionResultBuilder.Ok();
    }
}