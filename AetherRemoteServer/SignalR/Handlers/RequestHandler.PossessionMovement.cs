using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Possession;
using AetherRemoteCommon.Domain.Network.Possession.Movement;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Handlers;

public partial class RequestHandler
{
    public async Task<PossessionResponse> HandlePossessionMovement(string senderFriendCode, PossessionMovementRequest request, IHubCallerClients clients)
    {
        if (_presenceService.IsUserExceedingPossession(senderFriendCode))
            return new PossessionResponse(PossessionResponseEc.TooManyRequests, PossessionResultEc.Uninitialized);
        
        if (request.Horizontal is < -1 or > 1 || request.Vertical is < -1 or > 1 || request.Turn is < -1 or > 1 || request.Backwards > 1)
            return new PossessionResponse(PossessionResponseEc.BadDataInRequest, PossessionResultEc.Uninitialized);
        
        if (_possessionManager.TryGetSession(senderFriendCode) is not { } session)
            return new PossessionResponse(PossessionResponseEc.SenderNotInSession, PossessionResultEc.Uninitialized);
        
        if (session.GhostFriendCode != senderFriendCode)
            return new PossessionResponse(PossessionResponseEc.SenderNotGhost, PossessionResultEc.Uninitialized);
        
        var command = new PossessionMovementCommand(senderFriendCode, request.Horizontal, request.Vertical, request.Turn, request.Backwards);
        var response = await _forwardedRequestManager.CheckPossessionAndInvoke(
            senderFriendCode, 
            session.HostFriendCode, 
            HubMethod.Possession.Movement, 
            new ResolvedPermissions(PrimaryPermissions.None, SpeakPermissions.None, ElevatedPermissions.Possession), 
            command, 
            clients);
        
        // If the result has a desync, remove the session
        if (response.Result is PossessionResultEc.PossessionDesynchronization)
        {
            _logger.LogWarning("{Sender} has desynced with {Host}, removing session", senderFriendCode, session.HostFriendCode);
            _possessionManager.TryRemoveSession(session);
        }

        return response;
    }
}