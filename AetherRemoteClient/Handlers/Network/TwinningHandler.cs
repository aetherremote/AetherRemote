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
public class TwinningHandler(IdentityService identityService, LogService logService, CharacterTransformationManager characterTransformationManager, PermissionsCheckerManager permissionsCheckerManager)
{
    // Const
    private const string Operation = "Twinning";
    
    /// <summary>
    ///     <inheritdoc cref="TwinningForwardedRequest"/>
    /// </summary>
    public async Task<ActionResult<Unit>> Handle(TwinningForwardedRequest request)
    {
        Plugin.Log.Verbose($"{request}");
        
        // TODO: Re-enable when a mare solution is found
        // if (permanentTransformationHandler.IsPermanentTransformed)
        //    return ActionResultBuilder.Fail(ActionResultEc.ClientPermanentlyTransformed);
        
        var primary = request.SwapAttributes.ToPrimaryPermission();
        primary |= PrimaryPermissions2.Twinning;
        
        var elevated = request.LockCode is null 
            ? ElevatedPermissions.None 
            : ElevatedPermissions.PermanentTransformation;
        
        var permissions = new UserPermissions(primary, SpeakPermissions2.None, elevated);
        
        var friendActionResult = permissionsCheckerManager.GetSenderAndCheckPermissions(Operation, request.SenderFriendCode, permissions);
        if (friendActionResult.Result is not ActionResultEc.Success)
            return ActionResultBuilder.Fail(friendActionResult.Result);
        
        if (friendActionResult.Value is not { } friend)
            return ActionResultBuilder.Fail(ActionResultEc.ValueNotSet);

        // TODO: Re-enable when a mare solution is found
        /*
        if (request.LockCode is not null)
        {
            await permanentTransformationHandler.ApplyPermanentCharacterTransformation(friend.NoteOrFriendCode,
                request.LockCode, request.CharacterName, request.SwapAttributes);
        }
        else
        {
            await characterTransformationManager.ApplyCharacterTransformation(request.CharacterName, request.SwapAttributes);
        }
        */
        
        await characterTransformationManager.ApplyCharacterTransformation(request.CharacterName, request.SwapAttributes);
        
        // Set your new identity
        identityService.AddAlteration(IdentityAlterationType.Twinning, friend.NoteOrFriendCode);
        
        // Log success
        logService.Custom($"{friend.NoteOrFriendCode} twinned you with {request.CharacterName}");
        return ActionResultBuilder.Ok();
    }
}