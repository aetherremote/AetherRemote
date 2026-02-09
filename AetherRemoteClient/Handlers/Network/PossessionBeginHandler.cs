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
using AetherRemoteCommon.Domain.Network.Possession.Begin;
using Microsoft.AspNetCore.SignalR.Client;

namespace AetherRemoteClient.Handlers.Network;

public class PossessionBeginHandler : AbstractNetworkHandler, IDisposable
{
    // Const
    private const string Operation = "PossessionBegin";
    private static readonly ResolvedPermissions Permissions = new(PrimaryPermissions.None, SpeakPermissions.None, ElevatedPermissions.Possession);
    
    // Instantiated
    private readonly IDisposable _handler;
    private readonly LogService _log;
    private readonly PossessionManager _manager;
    
    public PossessionBeginHandler(
        AccountService account, 
        FriendsListService friends, 
        LogService log, 
        NetworkService network, 
        PauseService pause, 
        PossessionManager manager) : base(account, friends, log, pause)
    {
        _handler = network.Connection.On<PossessionBeginCommand, PossessionResultEc>(HubMethod.Possession.Begin, Handle);
        _log = log;
        _manager = manager;
    }

    private async Task<PossessionResultEc> Handle(PossessionBeginCommand command)
    {
        Plugin.Log.Verbose($"{command}");
        
        // If the client has not accepted the agreement
        if (AgreementsService.HasAgreedTo(AgreementsService.Agreements.MoodlesWarning) is false)
            return PossessionResultEc.HasNotAcceptedAgreement;
        
        var sender = TryGetFriendWithCorrectPermissions(Operation, command.SenderFriendCode, Permissions);
        if (sender.Result is not ActionResultEc.Success)
        {
            Plugin.Log.Warning($"[PossessionBeginHandler.Handle] {sender.Result}");
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

        // They sent us a move mode that doesn't exist
        if (command.MoveMode > 1)
            return PossessionResultEc.BadData;

        var result = await _manager.BecomePossessed(command.MoveMode).ConfigureAwait(false);
        if (result is PossessionResultEc.Success)
            _log.Custom($"You were possessed by {sender.Value?.FriendCode ?? string.Empty}");

        return result;
    }
    
    public void Dispose()
    {
        _handler.Dispose();
        GC.SuppressFinalize(this);
    }
}