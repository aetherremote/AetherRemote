using AetherRemoteCommon;
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
        // General validation on the values
        if (ValidatePossessionCameraRequest(senderFriendCode, request) is { } error)
        {
            logger.LogWarning("{Sender} sent invalid possess camera request {Error}", senderFriendCode, error);
            return new PossessionResponse(error, PossessionResultEc.Uninitialized);
        }
        
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
        
        var command = new PossessionCameraCommand(senderFriendCode, request.HorizontalRotation, request.VerticalRotation, request.Zoom);
        return await forwarder.CheckPossessionAndInvoke(senderFriendCode, session.HostFriendCode, Method, Required, command, clients);
    }
    
    private PossessionResponseEc? ValidatePossessionCameraRequest(string senderFriendCode, PossessionCameraRequest request)
    {
        if (presences.IsUserExceedingPossession(senderFriendCode))
            return PossessionResponseEc.TooManyRequests;

        if (request.HorizontalRotation is < Constraints.Possession.HorizontalMin or > Constraints.Possession.HorizontalMax)
            return PossessionResponseEc.BadDataInRequest;

        if (request.VerticalRotation is < Constraints.Possession.VerticalRotationMin or > Constraints.Possession.VerticalRotationMax)
            return PossessionResponseEc.BadDataInRequest;
        
        if (request.Zoom is < Constraints.Possession.ZoomMin or > Constraints.Possession.ZoomMax)
            return PossessionResponseEc.BadDataInRequest;
        
        return null;
    }
}