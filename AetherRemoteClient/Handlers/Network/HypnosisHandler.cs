using System;
using System.Threading.Tasks;
using AetherRemoteClient.Handlers.Network.Base;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Hypnosis;
using Microsoft.AspNetCore.SignalR.Client;

namespace AetherRemoteClient.Handlers.Network;

/// <summary>
///     Handles a <see cref="HypnosisCommand"/>
/// </summary>
public class HypnosisHandler : AbstractNetworkHandler, IDisposable
{
    // Const
    private const string Operation = "Hypnosis";
    private static readonly ResolvedPermissions Permissions = new(PrimaryPermissions2.Hypnosis, SpeakPermissions2.None, ElevatedPermissions.None);
    
    // Injected
    private readonly LogService _log;
    private readonly HypnosisManager _hypnosis;
    
    // Instantiated
    private readonly IDisposable _handler;

    /// <summary>
    ///     <inheritdoc cref="HypnosisHandler"/>
    /// </summary>
    public HypnosisHandler(
        AccountService account, 
        FriendsListService friends, 
        LogService log, 
        NetworkService network, 
        PauseService pause, 
        HypnosisManager hypnosis) : base(account, friends, log, pause)
    {
        _log = log;
        _hypnosis = hypnosis;
        
        _handler = network.Connection.On<HypnosisCommand, ActionResult<Unit>>(HubMethod.Hypnosis, Handle);
    }
    
    /// <summary>
    ///     <inheritdoc cref="HypnosisHandler"/>
    /// </summary>
    private async Task<ActionResult<Unit>> Handle(HypnosisCommand request)
    {
        Plugin.Log.Verbose($"{request}");

        var sender = TryGetFriendWithCorrectPermissions(Operation, request.SenderFriendCode, Permissions);
        if (sender.Result is not ActionResultEc.Success)
            return ActionResultBuilder.Fail(sender.Result);
        
        if (sender.Value is not { } friend)
            return ActionResultBuilder.Fail(ActionResultEc.ValueNotSet);
        
        // If you're already being hypnotized
        if (_hypnosis.IsBeingHypnotized)
        {
            // If the sender is the one who initiated it
            if (_hypnosis.Hypnotist?.FriendCode == request.SenderFriendCode)
            {
                // Do nothing
            }
            else
            {
                // Bounce their request
                _log.Custom($"Rejected hypnosis spiral from {friend.NoteOrFriendCode} because you're already being hypnotized");
                return ActionResultBuilder.Fail(ActionResultEc.ClientBeingHypnotized);
            }
        }
        
        // Begin the hypnosis
        await _hypnosis.Hypnotize(friend, request.Data);
        
        // Log
        _log.Custom($"{friend.NoteOrFriendCode} began to hypnotize you");
        return ActionResultBuilder.Ok();
    }

    public void Dispose()
    {
        _handler.Dispose();
        GC.SuppressFinalize(this);
    }
}