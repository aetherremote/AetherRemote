using System;
using AetherRemoteClient.Handlers.Network.Base;
using AetherRemoteClient.Services;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Possession;
using AetherRemoteCommon.Domain.Network.Possession.End;
using Microsoft.AspNetCore.SignalR.Client;

namespace AetherRemoteClient.Handlers.Network;

public class PossessionEndHandler : AbstractNetworkHandler, IDisposable
{
    // Instantiated
    private readonly IDisposable _handler;
    
    public PossessionEndHandler(FriendsListService friends, LogService log, NetworkService network, PauseService pause) : base(friends, log, pause)
    {
        _handler = network.Connection.On<PossessionEndCommand, PossessionResultEc>(HubMethod.Possession.End, Handle);
    }

    private PossessionResultEc Handle(PossessionEndCommand command)
    {
        return PossessionResultEc.Success;
    }
    
    public void Dispose()
    {
        _handler.Dispose();
        GC.SuppressFinalize(this);
    }
}