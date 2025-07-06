using System.Collections.Concurrent;
using AetherRemoteCommon;
using AetherRemoteServer.Domain;
using AetherRemoteServer.Domain.Interfaces;
using FriendCode = string;

namespace AetherRemoteServer.Services;

public class ConnectionsService : IConnectionsService
{
    private readonly ConcurrentDictionary<FriendCode, ClientInfo> _connectedClients = [];

    public void TryAddClient(string friendCode, ClientInfo info) =>
        _connectedClients[friendCode] = info;

    public void TryRemoveClient(string friendCode) =>
        _connectedClients.TryRemove(friendCode, out _);

    public ClientInfo? TryGetClient(string friendCode) =>
        _connectedClients.TryGetValue(friendCode, out var connectedClientInfo) ? connectedClientInfo : null;

    public bool IsUserExceedingRequestLimit(string issuerFriendCode)
    {
        if (_connectedClients.TryGetValue(issuerFriendCode, out var issuer) is false)
            return true;
        
        return (DateTime.UtcNow - issuer.LastAction).TotalSeconds < Constraints.ExternalCommandCooldownInSeconds;
    }
    
    public bool IsUserExceedingRequestLimit(ClientInfo clientInfo)
    {
        return (DateTime.UtcNow - clientInfo.LastAction).TotalSeconds < Constraints.ExternalCommandCooldownInSeconds;
    }
}