using System;
using AetherRemoteClient.Handlers.Network.Base;
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
    
    public PossessionBeginHandler(FriendsListService friends, LogService log, NetworkService network, PauseService pause) : base(friends, log, pause)
    {
        _handler = network.Connection.On<PossessionBeginCommand, PossessionResultEc>(HubMethod.Possession.Begin, Handle);
    }

    private PossessionResultEc Handle(PossessionBeginCommand command)
    {
        return PossessionResultEc.Success;
    }
    
    public void Dispose()
    {
        _handler.Dispose();
        GC.SuppressFinalize(this);
    }
}