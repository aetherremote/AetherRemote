using System.Threading.Tasks;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.HypnosisStop;

namespace AetherRemoteClient.Handlers.Network;

/// <summary>
///     Handles a <see cref="HypnosisStopForwardedRequest"/>
/// </summary>
public class HypnosisStopHandler(LogService logService, HypnosisManager hypnosisManager, PermissionsCheckerManager permissionsCheckerManager)
{
    // Const
    private const string Operation = "Hypnosis Stop";
    private static readonly UserPermissions Permissions = new(PrimaryPermissions2.Hypnosis, SpeakPermissions2.None, ElevatedPermissions.None);

    /// <summary>
    ///     <inheritdoc cref="HypnosisHandler"/>
    /// </summary>
    public async Task<ActionResult<Unit>> Handle(HypnosisStopForwardedRequest request)
    {
        Plugin.Log.Info($"{request}");
        
        // Verify the sender has valid permissions
        var sender = permissionsCheckerManager.GetSenderAndCheckPermissions(Operation, request.SenderFriendCode, Permissions);
        if (sender.Result is not ActionResultEc.Success)
            return ActionResultBuilder.Fail(sender.Result);
        
        // Verify the sender actually sent real data
        if (sender.Value is not { } friend)
            return ActionResultBuilder.Fail(ActionResultEc.ValueNotSet);

        // If they're not being hypnotized, No-Op
        if (hypnosisManager.IsBeingHypnotized is false)
            return ActionResultBuilder.Ok();
        
        // If they're the one who sent the hypnosis request in the first place
        if (hypnosisManager.Hypnotist?.FriendCode == request.SenderFriendCode)
        {
            hypnosisManager.Wake();
            return ActionResultBuilder.Ok();
        }
        else
        {
            // Bounce their request
            logService.Custom($"Rejected hypnosis spiral from {friend.NoteOrFriendCode} because you're already being hypnotized");
            return ActionResultBuilder.Fail(ActionResultEc.ClientBeingHypnotized);
        }
    }
}