using AetherRemoteCommon.V2;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Handlers.Base;

public interface IRelayHandler<TPayload, TResult>
{
    public Task<Response<TResult>> Execute(string senderFriendCode, Request<TPayload> request, IHubCallerClients clients);
}