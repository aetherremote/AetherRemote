using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Possession;
using AetherRemoteCommon.Domain.Network.Possession.End;
using AetherRemoteServer.Domain.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Handlers;

public class PossessionEndHandler(IForwardedRequestManager forwarder, IPossessionManager possessionManager)
{
    private const string Method = HubMethod.Possession.End;
    private static readonly UserPermissions Required = new(PrimaryPermissions2.None, SpeakPermissions2.None, ElevatedPermissions.Possession);
    
    public async Task<PossessionResponse> Handle(string senderFriendCode, IHubCallerClients clients)
    {
        if (possessionManager.TryGetSession(senderFriendCode) is not { } session)
            return new PossessionResponse(PossessionResponseEc.SenderNotInSession, PossessionResultEc.Uninitialized);

        // Always purge the session regardless if the other person fails to process the request
        possessionManager.TryRemoveSession(session);
        
        var friendCodeToNotify = session.GhostFriendCode == senderFriendCode ? session.HostFriendCode : session.GhostFriendCode;
        var command = new PossessionEndCommand(senderFriendCode);
        return await forwarder.CheckPossessionAndInvoke(senderFriendCode, friendCodeToNotify, Method, Required, command, clients);
    }
}