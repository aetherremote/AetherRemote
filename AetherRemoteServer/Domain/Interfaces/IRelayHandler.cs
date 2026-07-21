using AetherRemoteCommon.Network.Domain;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.Domain.Interfaces;

public interface IRelayHandler<TPayload, TResult>
{
    public Task<Response<TResult>> Execute(string senderFriendCode, Request<TPayload> request, IHubCallerClients clients);
}