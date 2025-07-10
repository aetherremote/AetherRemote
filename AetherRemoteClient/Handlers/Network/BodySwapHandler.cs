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
    ForwardedRequestManager forwardedRequestManager,
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
        if (permanentLockService.CurrentLock is not null)
            return ActionResultBuilder.Fail(ActionResultEc.ClientPermanentlyTransformed);
        
        var primary = request.SwapAttributes.ToPrimaryPermission();
        var elevated = request.LockCode is null 
            ? ElevatedPermissions.None 
            : ElevatedPermissions.PermanentTransformation;

        var permissions = new UserPermissions(primary, SpeakPermissions2.None, elevated);

        var placeholder = forwardedRequestManager.Placeholder(Operation, request.SenderFriendCode, permissions);
        if (placeholder.Result is not ActionResultEc.Success)
            return ActionResultBuilder.Fail(placeholder.Result);
        
        if (placeholder.Value is not { } friend)
            return ActionResultBuilder.Fail(ActionResultEc.ValueNotSet);
        
        // Actually apply glamourer, mods, etc...
        if (await modManager.Assimilate(request.CharacterName, request.SwapAttributes) is { } permanentTransformationData)
        {
            // If there is a lock, save the permanent transformation data
            if (request.LockCode.HasValue)
            {
                // Save
                permanentTransformationManager.Save(permanentTransformationData);
            }
        }
        
        // Set your new identity
        identityService.Identity = request.CharacterName;
        
        // Log Success
        logService.Custom($"{friend.NoteOrFriendCode} swapped your body with {request.CharacterName}'s");
        return ActionResultBuilder.Ok();
    }
}