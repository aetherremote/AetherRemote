using System;
using AetherRemoteClient.Handlers.Network.Base;
using AetherRemoteClient.Managers.Possession;
using AetherRemoteClient.Services;
using AetherRemoteCommon;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Possession;
using AetherRemoteCommon.Domain.Network.Possession.Camera;
using Microsoft.AspNetCore.SignalR.Client;

namespace AetherRemoteClient.Handlers.Network;

public class PossessionCameraHandler : AbstractNetworkHandler, IDisposable
{
    // Const
    private const string Operation = "PossessionCamera";
    private static readonly UserPermissions Permissions = new(PrimaryPermissions2.None, SpeakPermissions2.None, ElevatedPermissions.Possession);
    
    // Instantiated
    private readonly IDisposable _handler;
    private readonly PossessionManager _manager;
    
    public PossessionCameraHandler(FriendsListService friends, LogService log, NetworkService network, PauseService pause, PossessionManager manager) : base(friends, log, pause)
    {
        _handler = network.Connection.On<PossessionCameraCommand, PossessionResultEc>(HubMethod.Possession.Camera, Handle);
        _manager = manager;
    }

    private PossessionResultEc Handle(PossessionCameraCommand command)
    {
        Plugin.Log.Verbose($"{command}");
        var sender = TryGetFriendWithCorrectPermissions(Operation, command.SenderFriendCode, Permissions);
        if (sender.Result is not ActionResultEc.Success)
        {
            Plugin.Log.Warning($"[PossessionCameraHandler.Handle] {sender.Result}");
            return sender.Result switch
            {
                ActionResultEc.ClientNotFriends => PossessionResultEc.NotFriends,
                ActionResultEc.ClientInSafeMode => PossessionResultEc.SafeMode,
                ActionResultEc.ClientHasSenderPaused => PossessionResultEc.Paused,
                ActionResultEc.ClientHasFeaturePaused => PossessionResultEc.FeaturePaused,
                ActionResultEc.ClientHasNotGrantedSenderPermissions => PossessionResultEc.LackingPermissions,
                _ => PossessionResultEc.Unknown
            };
        }
        
        // Data validation
        if (command.HorizontalRotation is < Constraints.Possession.HorizontalMin or > Constraints.Possession.HorizontalMax)
            return PossessionResultEc.BadData;

        if (command.VerticalRotation is < Constraints.Possession.VerticalRotationMin or > Constraints.Possession.VerticalRotationMax)
            return PossessionResultEc.BadData;
        
        if (command.Zoom is < Constraints.Possession.ZoomMin or > Constraints.Possession.ZoomMax)
            return PossessionResultEc.BadData;
        
        return _manager.SetCameraDestination(command.HorizontalRotation, command.VerticalRotation, command.Zoom);
    }
    
    public void Dispose()
    {
        _handler.Dispose();
        GC.SuppressFinalize(this);
    }
}