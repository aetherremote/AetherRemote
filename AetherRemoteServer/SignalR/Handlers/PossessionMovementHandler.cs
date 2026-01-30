using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Possession;
using AetherRemoteCommon.Domain.Network.Possession.Movement;
using AetherRemoteServer.Domain.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Handlers;

public class PossessionMovementHandler(IPresenceService presences, IForwardedRequestManager forwarder, IPossessionManager possessionManager, ILogger<PossessionMovementHandler> logger)
{
    private const string Method = HubMethod.Possession.Movement;
    private static readonly UserPermissions Required = new(PrimaryPermissions2.None, SpeakPermissions2.None, ElevatedPermissions.Possession);
    
    public async Task<PossessionResponse> Handle(string senderFriendCode, PossessionMovementRequest request, IHubCallerClients clients)
    {
        if (presences.IsUserExceedingPossession(senderFriendCode))
            return new PossessionResponse(PossessionResponseEc.TooManyRequests, PossessionResultEc.Uninitialized);
        
        if (request.Horizontal is < -1 or > 1 || request.Vertical is < -1 or > 1 || request.Turn is < -1 or > 1 || request.Backwards > 1)
            return new PossessionResponse(PossessionResponseEc.BadDataInRequest, PossessionResultEc.Uninitialized);
        
        if (possessionManager.TryGetSession(senderFriendCode) is not { } session)
            return new PossessionResponse(PossessionResponseEc.SenderNotInSession, PossessionResultEc.Uninitialized);
        
        if (session.GhostFriendCode != senderFriendCode)
            return new PossessionResponse(PossessionResponseEc.SenderNotGhost, PossessionResultEc.Uninitialized);
        
        var command = new PossessionMovementCommand(senderFriendCode, request.Horizontal, request.Vertical, request.Turn, request.Backwards);
        var response = await forwarder.CheckPossessionAndInvoke(senderFriendCode, session.HostFriendCode, Method, Required, command, clients);
        
        // If the result has a desync, remove the session
        if (response.Result is PossessionResultEc.PossessionDesynchronization)
        {
            logger.LogWarning("{Sender} has desynced with {Host}, removing session", senderFriendCode, session.HostFriendCode);
            possessionManager.TryRemoveSession(session);
        }

        return response;
    }
}