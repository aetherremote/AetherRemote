using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Possession;
using AetherRemoteCommon.Domain.Network.Possession.End;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Handlers;

public partial class RequestHandler
{
    public async Task<PossessionResponse> HandlePossessionEnd(string senderFriendCode, IHubCallerClients clients)
    {
        if (_possessionManager.TryGetSession(senderFriendCode) is not { } session)
            return new PossessionResponse(PossessionResponseEc.SenderNotInSession, PossessionResultEc.Uninitialized);

        // Always purge the session regardless if the other person fails to process the request
        _possessionManager.TryRemoveSession(session);
        
        var friendCodeToNotify = session.GhostFriendCode == senderFriendCode ? session.HostFriendCode : session.GhostFriendCode;
        var command = new PossessionEndCommand(senderFriendCode);
        return await _forwardedRequestManager.CheckPossessionAndInvoke(
            senderFriendCode, 
            friendCodeToNotify, 
            HubMethod.Possession.End, 
            new ResolvedPermissions(PrimaryPermissions.None, SpeakPermissions.None, ElevatedPermissions.Possession), 
            command, 
            clients);
    }
}