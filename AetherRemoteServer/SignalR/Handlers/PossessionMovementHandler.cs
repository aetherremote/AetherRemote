using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Possession;
using AetherRemoteCommon.Domain.Network.Possession.Movement;
using AetherRemoteServer.Domain.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Handlers;

public class PossessionMovementHandler(
    IForwardedRequestManager forwarder,
    IPossessionManager possessionManager,
    ILogger<PossessionMovementHandler> logger)
{
    private const string Method = HubMethod.Possession.Movement;
    private static readonly UserPermissions Required = new(PrimaryPermissions2.None, SpeakPermissions2.None, ElevatedPermissions.Possession);
    
    public async Task Handle(string senderFriendCode, PossessionMovementRequest request, IHubCallerClients clients)
    {
        // TODO: Decide how to handle cases like this, because all of these should never execute code, but if any of them are incorrect
        //          then the session probably should be cancelled. Some of them can be graceful, but not all of them.
        
        // Check if the sender is in a session
        if (possessionManager.TryGetSession(senderFriendCode) is not { } session)
        {
            logger.LogWarning("{Sender} sent invalid possession movement request {Error}", senderFriendCode, PossessionResponseEc.SenderNotInSession);
            return;
        }
        
        // If they are, make sure they are the ghost, who is the only person who should be sending here
        if (session.GhostFriendCode != senderFriendCode)
        {
            logger.LogWarning("{Sender} sent invalid possession movement request {Error}", senderFriendCode, PossessionResponseEc.SenderNotGhost);
            return;
        }
        
        // TODO: There are more things we can test here, such as if the target is still online,
        //          if the sender's session matches the friend code of the target's session, etc...

        var command = new PossessionMovementCommand(senderFriendCode, request.Horizontal, request.Vertical, request.Turn, request.Backwards);
        await forwarder.CheckPossessionAndSend(senderFriendCode, session.HostFriendCode, Method, Required, command, clients);
    }
}