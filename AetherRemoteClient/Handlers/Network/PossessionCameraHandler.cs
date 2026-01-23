using System;
using AetherRemoteClient.Handlers.Network.Base;
using AetherRemoteClient.Services;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Possession;
using AetherRemoteCommon.Domain.Network.Possession.Camera;
using Microsoft.AspNetCore.SignalR.Client;

namespace AetherRemoteClient.Handlers.Network;

public class PossessionCameraHandler : AbstractNetworkHandler, IDisposable
{
    // Instantiated
    private readonly IDisposable _handler;
    
    public PossessionCameraHandler(FriendsListService friends, LogService log, NetworkService network, PauseService pause) : base(friends, log, pause)
    {
        _handler = network.Connection.On<PossessionCameraCommand, PossessionResultEc>(HubMethod.Possession.Camera, Handle);
    }

    private PossessionResultEc Handle(PossessionCameraCommand command)
    {
        return PossessionResultEc.Success;
    }
    
    public void Dispose()
    {
        _handler.Dispose();
        GC.SuppressFinalize(this);
    }
}