using AetherRemoteCommon;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Possession;
using AetherRemoteCommon.Domain.Network.Possession.Camera;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Handlers.Test;

public partial class RequestHandler
{
    public async Task<PossessionResponse> HandlePossessionCamera(string senderFriendCode, PossessionCameraRequest request, IHubCallerClients clients)
    {
        if (_presenceService.IsUserExceedingPossession(senderFriendCode))
            return new PossessionResponse(PossessionResponseEc.TooManyRequests, PossessionResultEc.Uninitialized);

        // TODO: Remove after invalid data is discovered from common client use
        if (request.HorizontalRotation is < Constraints.Possession.HorizontalMin or > Constraints.Possession.HorizontalMax || request.VerticalRotation is < Constraints.Possession.VerticalMin or > Constraints.Possession.VerticalMax || request.Zoom is < Constraints.Possession.ZoomMin or > Constraints.Possession.ZoomMax)
        {
            _logger.LogWarning("{Sender} sent invalid camera request data {Data}", senderFriendCode, request);
            return new PossessionResponse(PossessionResponseEc.BadDataInRequest, PossessionResultEc.Uninitialized);
        }
        
        if (_possessionManager.TryGetSession(senderFriendCode) is not { } session)
            return new PossessionResponse(PossessionResponseEc.SenderNotInSession, PossessionResultEc.Uninitialized);
        
        if (session.GhostFriendCode != senderFriendCode)
            return new PossessionResponse(PossessionResponseEc.SenderNotGhost, PossessionResultEc.Uninitialized);
        
        var command = new PossessionCameraCommand(senderFriendCode, request.HorizontalRotation, request.VerticalRotation, request.Zoom);
        var response = await _forwardedRequestManager.CheckPossessionAndInvoke(
            senderFriendCode, 
            session.HostFriendCode, 
            HubMethod.Possession.Camera, 
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