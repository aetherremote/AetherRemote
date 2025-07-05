using System.Threading.Tasks;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Twinning;
using AetherRemoteCommon.Util;
using AetherRemoteCommon.V2.Domain;

namespace AetherRemoteClient.Handlers.Network;

/// <summary>
///     handles a <see cref="TwinningForwardedRequest"/>
/// </summary>
public class TwinningHandler(
    IdentityService identityService,
    LogService logService,
    ForwardedRequestManager forwardedRequestManager,
    ModManager modManager)
{
    // Const
    private const string Operation = "Twinning";
    
    /// <summary>
    ///     <inheritdoc cref="TwinningForwardedRequest"/>
    /// </summary>
    public async Task<ActionResult<Unit>> Handle(TwinningForwardedRequest request)
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
        
        // Log success
        logService.Custom($"{friend.NoteOrFriendCode} twinned you with {request.CharacterName}");
        return ActionResultBuilder.Ok();
    }
}