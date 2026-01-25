using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Possession;
using AetherRemoteCommon.Domain.Network.Possession.Camera;
using AetherRemoteServer.Domain.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Handlers;

public class PossessionCameraHandler(
    IPresenceService presences,
    IForwardedRequestManager forwarder,
    IPossessionManager possessionManager,
    ILogger<PossessionMovementHandler> logger)
{
    private const string Method = HubMethod.Possession.Camera;
    private static readonly UserPermissions Required = new(PrimaryPermissions2.None, SpeakPermissions2.None, ElevatedPermissions.Possession);
    
    public async Task<PossessionResponse> Handle(string senderFriendCode, PossessionCameraRequest request, IHubCallerClients clients)
    {
        if (presences.IsUserExceedingPossession(senderFriendCode))
            return new PossessionResponse(PossessionResponseEc.TooManyRequests, PossessionResultEc.Uninitialized);
        
        // Check if the sender is in a session
        if (possessionManager.TryGetSession(senderFriendCode) is not { } session)
        {
            logger.LogWarning("{Sender} sent invalid possession camera request {Error}", senderFriendCode, PossessionResponseEc.SenderNotInSession);
            return new PossessionResponse(PossessionResponseEc.SenderNotInSession, PossessionResultEc.Uninitialized);
        }
        
        // If they are, make sure they are the ghost, who is the only person who should be sending here
        if (session.GhostFriendCode != senderFriendCode)
        {
            logger.LogWarning("{Sender} sent invalid possession camera request {Error}", senderFriendCode, PossessionResponseEc.SenderNotGhost);
            return new PossessionResponse(PossessionResponseEc.SenderNotGhost, PossessionResultEc.Uninitialized);
        }
        
        // TODO: There are more things we can test here, such as if the target is still online,
        //          if the sender's session matches the friend code of the target's session, etc...

        var command = new PossessionCameraCommand(senderFriendCode, request.HorizontalRotation, request.VerticalRotation, request.Zoom);
        return await forwarder.CheckPossessionAndInvoke(senderFriendCode, session.HostFriendCode, Method, Required, command, clients);
    }
}