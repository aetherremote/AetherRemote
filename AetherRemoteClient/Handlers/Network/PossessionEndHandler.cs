using System;
using System.Threading.Tasks;
using AetherRemoteClient.Handlers.Network.Base;
using AetherRemoteClient.Managers.Possession;
using AetherRemoteClient.Services;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Possession;
using AetherRemoteCommon.Domain.Network.Possession.End;
using Microsoft.AspNetCore.SignalR.Client;

namespace AetherRemoteClient.Handlers.Network;

public class PossessionEndHandler : AbstractNetworkHandler, IDisposable
{
    // Const
    private const string Operation = "PossessionEnd";
    private static readonly UserPermissions Permissions = new(PrimaryPermissions2.None, SpeakPermissions2.None, ElevatedPermissions.Possession);
    
    // Instantiated
    private readonly IDisposable _handler;
    private readonly LogService _log;
    private readonly PossessionManager _manager;
    
    public PossessionEndHandler(FriendsListService friends, LogService log, NetworkService network, PauseService pause, PossessionManager manager) : base(friends, log, pause)
    {
        _handler = network.Connection.On<PossessionEndCommand, PossessionResultEc>(HubMethod.Possession.End, Handle);
        _log = log;
        _manager = manager;
    }

    private async Task<PossessionResultEc> Handle(PossessionEndCommand command)
    {
        Plugin.Log.Verbose($"{command}");
        var sender = TryGetFriendWithCorrectPermissions(Operation, command.SenderFriendCode, Permissions);
        if (sender.Result is not ActionResultEc.Success)
        {
            Plugin.Log.Warning($"[PossessionEndHandler.Handle] {sender.Result}");
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

        if (_manager.Possessed)
        {
            await _manager.Expel(true).ConfigureAwait(false);
            _log.Custom($"{sender.Value?.FriendCode ?? string.Empty} stopped possessing you");
        }

        if (_manager.Possessing)
        {
            await _manager.Unpossess(true).ConfigureAwait(false);
            _log.Custom($"{sender.Value?.FriendCode ?? string.Empty} expelled you from their body");
        }

        return PossessionResultEc.Success;
    }
    
    public void Dispose()
    {
        _handler.Dispose();
        GC.SuppressFinalize(this);
    }
}