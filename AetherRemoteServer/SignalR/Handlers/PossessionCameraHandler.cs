using AetherRemoteCommon;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Possession;
using AetherRemoteCommon.Domain.Network.Possession.Camera;
using AetherRemoteServer.Managers;
using AetherRemoteServer.Services;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Handlers;

public class PossessionCameraHandler(PresenceService presences, ForwardedRequestManager forwarder, PossessionManager possessionManager, ILogger<PossessionCameraHandler> logger)
{
    private const string Method = HubMethod.Possession.Camera;
    private static readonly ResolvedPermissions Required = new(PrimaryPermissions2.None, SpeakPermissions2.None, ElevatedPermissions.Possession);
    
    public async Task<PossessionResponse> Handle(string senderFriendCode, PossessionCameraRequest request, IHubCallerClients clients)
    {
        if (presences.IsUserExceedingPossession(senderFriendCode))
            return new PossessionResponse(PossessionResponseEc.TooManyRequests, PossessionResultEc.Uninitialized);

        // TODO: Remove after invalid data is discovered from common client use
        if (request.HorizontalRotation is < Constraints.Possession.HorizontalMin or > Constraints.Possession.HorizontalMax || request.VerticalRotation is < Constraints.Possession.VerticalMin or > Constraints.Possession.VerticalMax || request.Zoom is < Constraints.Possession.ZoomMin or > Constraints.Possession.ZoomMax)
        {
            logger.LogWarning("{Sender} sent invalid camera request data {Data}", senderFriendCode, request);
            return new PossessionResponse(PossessionResponseEc.BadDataInRequest, PossessionResultEc.Uninitialized);
        }
        
        if (possessionManager.TryGetSession(senderFriendCode) is not { } session)
            return new PossessionResponse(PossessionResponseEc.SenderNotInSession, PossessionResultEc.Uninitialized);
        
        if (session.GhostFriendCode != senderFriendCode)
            return new PossessionResponse(PossessionResponseEc.SenderNotGhost, PossessionResultEc.Uninitialized);
        
        var command = new PossessionCameraCommand(senderFriendCode, request.HorizontalRotation, request.VerticalRotation, request.Zoom);
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