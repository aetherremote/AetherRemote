using System;
using AetherRemoteClient.Handlers.Network.Base;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Possession;
using AetherRemoteCommon.Domain.Network.Possession.Movement;
using Microsoft.AspNetCore.SignalR.Client;

namespace AetherRemoteClient.Handlers.Network;

public class PossessionMovementHandler : AbstractNetworkHandler, IDisposable
{
    // Instantiated
    private readonly IDisposable _handler;
    private readonly PossessionManager _manager;
    
    public PossessionMovementHandler(FriendsListService friends, LogService log, NetworkService network, PauseService pause, PossessionManager manager) : base(friends, log, pause)
    {
        _handler = network.Connection.On<PossessionMovementCommand, PossessionResultEc>(HubMethod.Possession.Movement, Handle);
        _manager = manager;
    }

    private PossessionResultEc Handle(PossessionMovementCommand command)
    {
        _manager.SetMovementDirection(command.Horizontal, command.Vertical, command.Turn, command.Backwards);
        return PossessionResultEc.Success;
    }
    
    public void Dispose()
    {
        _handler.Dispose();
        GC.SuppressFinalize(this);
    }
}