using System;
using System.Threading.Tasks;
using AetherRemoteClient.Handlers.Network.Base;
using AetherRemoteClient.Managers;
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
    private readonly PossessionManager _manager;
    
    public PossessionEndHandler(FriendsListService friends, LogService log, NetworkService network, PauseService pause, PossessionManager manager) : base(friends, log, pause)
    {
        _handler = network.Connection.On<PossessionEndCommand, PossessionResultEc>(HubMethod.Possession.End, Handle);
        _manager = manager;
    }

    private PossessionResultEc Handle(PossessionEndCommand command)
    {
        _manager.EndPossessing();
        return PossessionResultEc.Success;
    }
    
    public void Dispose()
    {
        _handler.Dispose();
        GC.SuppressFinalize(this);
    }
}