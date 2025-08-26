using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Hypnosis;

// ReSharper disable once ConvertToPrimaryConstructor

namespace AetherRemoteClient.Handlers.Network;

/// <summary>
///     Handles a <see cref="HypnosisForwardedRequest"/>
/// </summary>
public class HypnosisHandler(LogService logService, SpiralService spiralService, PermissionsCheckerManager permissionsCheckerManager)
{
    // Const
    private const string Operation = "Hypnosis";
    private static readonly UserPermissions Permissions = new(PrimaryPermissions2.Hypnosis, SpeakPermissions2.None, ElevatedPermissions.None);
    
    /// <summary>
    ///     <inheritdoc cref="HypnosisHandler"/>
    /// </summary>
    public ActionResult<Unit> Handle(HypnosisForwardedRequest request)
    {
        Plugin.Log.Info($"{request}");

        var placeholder = permissionsCheckerManager.GetSenderAndCheckPermissions(Operation, request.SenderFriendCode, Permissions);
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