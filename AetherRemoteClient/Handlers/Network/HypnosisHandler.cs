using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Hypnosis;

namespace AetherRemoteClient.Handlers.Network;

/// <summary>
///     TODO
/// </summary>
public class HypnosisHandler(
    LogService logService,
    SpiralService spiralService,
    ForwardedRequestManager forwardedRequestManager)
{
    // Const
    private const string Operation = "Hypnosis";
    private const PrimaryPermissions2 Permissions = PrimaryPermissions2.Hypnosis;
    
    /// <summary>
    ///     <inheritdoc cref="HypnosisHandler"/>
    /// </summary>
    public ActionResult<Unit> Handle(HypnosisForwardedRequest request)
    {
        Plugin.Log.Info($"{request}");

        var placeholder = forwardedRequestManager.Placehold(Operation, request.SenderFriendCode, Permissions);
        if (placeholder.Result is not ActionResultEc.Success)
            return ActionResultBuilder.Fail(placeholder.Result);
        
        if (placeholder.Value is not { } friend)
            return ActionResultBuilder.Fail(ActionResultEc.ValueNotSet);
        
        // Already being hypnotized
        if (spiralService.IsBeingHypnotized)
        {
            logService.Custom($"Rejected hypnosis spiral from {friend.NoteOrFriendCode} because you're already being hypnotized");
            return ActionResultBuilder.Fail(ActionResultEc.ClientBeingHypnotized);
        }
        
        spiralService.StartSpiral(friend.NoteOrFriendCode, request.Spiral);
        logService.Custom($"{friend.NoteOrFriendCode} began to hypnotize you");
        return ActionResultBuilder.Ok();
    }
}