using System.Threading.Tasks;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
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
    ForwardedRequestManager forwardedRequestManager,
    ModManager modManager)
{
    // Const
    private const string Operation = "Body Swap";
    
    /// <summary>
    ///     <inheritdoc cref="BodySwapHandler"/>
    /// </summary>
    public async Task<ActionResult<Unit>> Handle(BodySwapForwardedRequest request)
    {
        Plugin.Log.Info($"{request}");

        var permissions = request.SwapAttributes.ToPrimaryPermission();
        var placeholder = forwardedRequestManager.Placehold(Operation, request.SenderFriendCode, permissions);
        if (placeholder.Result is not ActionResultEc.Success)
            return ActionResultBuilder.Fail(placeholder.Result);
        
        if (placeholder.Value is not { } friend)
            return ActionResultBuilder.Fail(ActionResultEc.ValueNotSet);
        
        // Actually apply glamourer, mods, etc...
        // TODO: Handle with return statement
        await modManager.Assimilate(request.CharacterName, request.SwapAttributes);

        // Set your new identity
        identityService.Identity = request.CharacterName;
        
        // Log Success
        logService.Custom($"{friend.NoteOrFriendCode} swapped your body with {request.CharacterName}'s");
        return ActionResultBuilder.Ok();
    }
}