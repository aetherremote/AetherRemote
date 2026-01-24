using System;
using AetherRemoteClient.Handlers.Network.Base;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Possession;
using AetherRemoteCommon.Domain.Network.Possession.Begin;
using Microsoft.AspNetCore.SignalR.Client;

namespace AetherRemoteClient.Handlers.Network;

public class PossessionBeginHandler : AbstractNetworkHandler, IDisposable
{
    // Instantiated
    private readonly IDisposable _handler;
    private readonly PossessionManager _manager;
    
    public PossessionBeginHandler(FriendsListService friends, LogService log, NetworkService network, PauseService pause, PossessionManager manager) : base(friends, log, pause)
    {
        _handler = network.Connection.On<PossessionBeginCommand, PossessionResultEc>(HubMethod.Possession.Begin, Handle);
        _manager = manager;
    }

    private PossessionResultEc Handle(PossessionBeginCommand command)
    {
        _manager.TryBecomePossessed();
        return PossessionResultEc.Success;
    }
    
    public void Dispose()
    {
        _handler.Dispose();
        GC.SuppressFinalize(this);
    }
}