using System;
using AetherRemoteClient.Handlers.Network.Base;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Possession;
using AetherRemoteCommon.Domain.Network.Possession.Movement;
using Microsoft.AspNetCore.SignalR.Client;

namespace AetherRemoteClient.Handlers.Network;

public class PossessionMovementHandler : AbstractNetworkHandler, IDisposable
{
    // Const
    private const string Operation = "PossessionMovement";
    private static readonly UserPermissions Permissions = new(PrimaryPermissions2.Hypnosis, SpeakPermissions2.None, ElevatedPermissions.Possession);
    
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
        Plugin.Log.Verbose($"{command}");
        var sender = TryGetFriendWithCorrectPermissions(Operation, command.SenderFriendCode, Permissions);
        if (sender.Result is not ActionResultEc.Success)
        {
            Plugin.Log.Warning($"[PossessionMovementHandler.Handle] {sender.Result}");
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
        if (command.Horizontal is < -1 or > 1)
            return PossessionResultEc.BadData;

        if (command.Vertical is < -1 or > 1)
            return PossessionResultEc.BadData;
        
        if (command.Turn is < -1 or > 1)
            return PossessionResultEc.BadData;

        if (command.Backwards > 1)
            return PossessionResultEc.BadData;
        
        _manager.SetMovementDirection(command.Horizontal, command.Vertical, command.Turn, command.Backwards);
        return PossessionResultEc.Success;
    }
    
    public void Dispose()
    {
        _handler.Dispose();
        GC.SuppressFinalize(this);
    }
}