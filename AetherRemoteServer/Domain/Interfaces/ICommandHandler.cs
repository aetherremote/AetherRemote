using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.Domain.Interfaces;

public interface ICommandHandler<in TRequest, TResponse>
{
    public Task<TResponse> Execute(string senderFriendCode, TRequest request, IHubCallerClients clients);
}