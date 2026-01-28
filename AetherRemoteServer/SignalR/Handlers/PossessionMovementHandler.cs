using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Possession;
using AetherRemoteCommon.Domain.Network.Possession.Movement;
using AetherRemoteServer.Domain.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Handlers;

public class PossessionMovementHandler(
    IPresenceService presences,
    IForwardedRequestManager forwarder,
    IPossessionManager possessionManager,
    ILogger<PossessionMovementHandler> logger)
{
    private const string Method = HubMethod.Possession.Movement;
    private static readonly UserPermissions Required = new(PrimaryPermissions2.None, SpeakPermissions2.None, ElevatedPermissions.Possession);
    
    public async Task<PossessionResponse> Handle(string senderFriendCode, PossessionMovementRequest request, IHubCallerClients clients)
    {
        // General validation on the values
        if (ValidatePossessionMovementRequest(senderFriendCode, request) is { } error)
        {
            logger.LogWarning("{Sender} sent invalid possess movement request {Error}", senderFriendCode, error);
            return new PossessionResponse(error, PossessionResultEc.Uninitialized);
        }
        
        // Check if the sender is in a session
        if (possessionManager.TryGetSession(senderFriendCode) is not { } session)
        {
            logger.LogWarning("{Sender} sent invalid possession movement request {Error}", senderFriendCode, PossessionResponseEc.SenderNotInSession);
            return new PossessionResponse(PossessionResponseEc.SenderNotInSession, PossessionResultEc.Uninitialized);
        }
        
        // If they are, make sure they are the ghost, who is the only person who should be sending here
        if (session.GhostFriendCode != senderFriendCode)
        {
            logger.LogWarning("{Sender} sent invalid possession movement request {Error}", senderFriendCode, PossessionResponseEc.SenderNotGhost);
            return new PossessionResponse(PossessionResponseEc.SenderNotGhost, PossessionResultEc.Uninitialized);
        }
        
        var command = new PossessionMovementCommand(senderFriendCode, request.Horizontal, request.Vertical, request.Turn, request.Backwards);
        return await forwarder.CheckPossessionAndInvoke(senderFriendCode, session.HostFriendCode, Method, Required, command, clients);
    }
    
    private PossessionResponseEc? ValidatePossessionMovementRequest(string senderFriendCode, PossessionMovementRequest request)
    {
        if (presences.IsUserExceedingPossession(senderFriendCode))
            return PossessionResponseEc.TooManyRequests;

        if (request.Horizontal is < -1 or > 1)
            return PossessionResponseEc.BadDataInRequest;
        
        if (request.Vertical is < -1 or > 1)
            return PossessionResponseEc.BadDataInRequest;
        
        if (request.Turn is < -1 or > 1)
            return PossessionResponseEc.BadDataInRequest;
        
        if (request.Backwards > 1)
            return PossessionResponseEc.BadDataInRequest;
        
        return null;
    }
}