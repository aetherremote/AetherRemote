using System.Threading.Tasks;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Hypnosis;

namespace AetherRemoteClient.Handlers.Network;

/// <summary>
///     Handles a <see cref="HypnosisForwardedRequest"/>
/// </summary>
public class HypnosisHandler(LogService logService, HypnosisManager hypnosisManager, PermissionsCheckerManager permissionsCheckerManager)
{
    // Const
    private const string Operation = "Hypnosis";
    private static readonly UserPermissions Permissions = new(PrimaryPermissions2.Hypnosis, SpeakPermissions2.None, ElevatedPermissions.None);
    
    /// <summary>
    ///     <inheritdoc cref="HypnosisHandler"/>
    /// </summary>
    public async Task<ActionResult<Unit>> Handle(HypnosisForwardedRequest request)
    {
        Plugin.Log.Info($"{request}");

        // Verify the sender has valid permissions
        var sender = permissionsCheckerManager.GetSenderAndCheckPermissions(Operation, request.SenderFriendCode, Permissions);
        if (sender.Result is not ActionResultEc.Success)
            return ActionResultBuilder.Fail(sender.Result);
        
        // Verify the sender actually sent real data
        if (sender.Value is not { } friend)
            return ActionResultBuilder.Fail(ActionResultEc.ValueNotSet);
        
        // If you're already being hypnotized
        if (hypnosisManager.IsBeingHypnotized)
        {
            // If the sender is the one who initiated it
            if (hypnosisManager.Hypnotist?.FriendCode == request.SenderFriendCode)
            {
                // If they turned the stop command on, stop
                if (request.Stop)
                {
                    // Stop being hypnotized, silly
                    hypnosisManager.Wake();
                }
            }
            else
            {
                // Bounce their request
                logService.Custom($"Rejected hypnosis spiral from {friend.NoteOrFriendCode} because you're already being hypnotized");
                return ActionResultBuilder.Fail(ActionResultEc.ClientBeingHypnotized);
            }
        }
        
        // Begin the hypnosis
        await hypnosisManager.Hypnotize(friend, request.Data);
        
        // Log
        logService.Custom($"{friend.NoteOrFriendCode} began to hypnotize you");
        return ActionResultBuilder.Ok();
    }
}