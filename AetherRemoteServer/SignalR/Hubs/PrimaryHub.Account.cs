using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Network.Domain.Commands;
using AetherRemoteCommon.Network.Enums.ErrorCodes;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Hubs;

public partial class PrimaryHub
{
    [HubMethodName(HubMethod.InitializeSession)]
    public async Task<InitializeSessionResponse> InitializeSession(InitializeSessionRequest request)
    {
        var friendCode = FriendCode;
        var response = await requestHandler.InitializeSessionHandler.Initialize(friendCode, Context.ConnectionId, request);
        if (response.Result is GetAccountDataEc.Success)
            _ = requestHandler.OnlineNotificationHandler.Notify(friendCode, true, Clients); // Send, notifications, but don't block the return
        
        return response;
    }

    [HubMethodName(HubMethod.TerminateSession)]
    public Task TerminateSession(TerminateSessionRequest request)
    {
        var friendCode = FriendCode;
        if (requestHandler.TerminateSessionHandler.Terminate(friendCode))
            _ = requestHandler.OnlineNotificationHandler.Notify(friendCode, false, Clients); // Send, notifications, but don't block the return
        
        return Task.CompletedTask;
    }
}
