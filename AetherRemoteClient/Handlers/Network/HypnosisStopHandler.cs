using System;
using System.Threading.Tasks;
using AetherRemoteClient.Handlers.Network.Base;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.HypnosisStop;
using Microsoft.AspNetCore.SignalR.Client;

namespace AetherRemoteClient.Handlers.Network;

/// <summary>
///     Handles a <see cref="HypnosisStopCommand"/>
/// </summary>
public class HypnosisStopHandler : AbstractNetworkHandler, IDisposable
{
    // Const
    private const string Operation = "Hypnosis Stop";
    private static readonly ResolvedPermissions Permissions = new(PrimaryPermissions2.Hypnosis, SpeakPermissions2.None, ElevatedPermissions.None);

    // Injected
    private readonly LogService _log;
    private readonly HypnosisManager _hypnosis;
    
    // Instantiated
    private readonly IDisposable _handler;
    
    /// <summary>
    ///     <inheritdoc cref="HypnosisStopHandler"/>
    /// </summary>
    public HypnosisStopHandler(
        AccountService account, 
        FriendsListService friends, 
        LogService log,
        NetworkService network, 
        PauseService pause, 
        HypnosisManager hypnosis) : base(account, friends, log, pause)
    {
        _log = log;
        _hypnosis = hypnosis;
        
        _handler = network.Connection.On<HypnosisStopCommand, ActionResult<Unit>>(HubMethod.HypnosisStop, Handle);
    }
    
    /// <summary>
    ///     <inheritdoc cref="HypnosisHandler"/>
    /// </summary>
    private async Task<ActionResult<Unit>> Handle(HypnosisStopCommand request)
    {
        Plugin.Log.Verbose($"{request}");
        
        var sender = TryGetFriendWithCorrectPermissions(Operation, request.SenderFriendCode, Permissions);
        if (sender.Result is not ActionResultEc.Success)
            return ActionResultBuilder.Fail(sender.Result);
        
        if (sender.Value is not { } friend)
            return ActionResultBuilder.Fail(ActionResultEc.ValueNotSet);

        // If they're not being hypnotized, No-Op
        if (_hypnosis.IsBeingHypnotized is false)
            return ActionResultBuilder.Ok();
        
        // If they're the one who sent the hypnosis request in the first place
        if (_hypnosis.Hypnotist?.FriendCode == request.SenderFriendCode)
        {
            await Plugin.RunOnFramework(() => _hypnosis.Wake()).ConfigureAwait(false);
            return ActionResultBuilder.Ok();
        }

        // Bounce their request
        _log.Custom($"Rejected hypnosis spiral from {friend.NoteOrFriendCode} because you're already being hypnotized");
        return ActionResultBuilder.Fail(ActionResultEc.ClientBeingHypnotized);
    }

    public void Dispose()
    {
        _handler.Dispose();
        GC.SuppressFinalize(this);
    }
}