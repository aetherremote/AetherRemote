using System;
using System.Text;
using AetherRemoteClient.Handlers.Network.Base;
using AetherRemoteClient.Services;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Emote;
using Microsoft.AspNetCore.SignalR.Client;

namespace AetherRemoteClient.Handlers.Network;

/// <summary>
///     Handles a <see cref="EmoteCommand"/>
/// </summary>
public class EmoteHandler : AbstractNetworkHandler, IDisposable
{
    // Const
    private const string Operation = "Emote";
    private static readonly ResolvedPermissions Permissions = new(PrimaryPermissions.Emote, SpeakPermissions.None, ElevatedPermissions.None);
    
    // Injected
    private readonly EmoteService _emote;
    private readonly LogService _log;
    
    // Instantiated
    private readonly IDisposable _handler;

    /// <summary>
    ///     <inheritdoc cref="EmoteHandler"/>
    /// </summary>
    public EmoteHandler(
        AccountService account, 
        EmoteService emote, 
        FriendsListService friends, 
        LogService log, 
        NetworkService network, 
        PauseService pause) : base(account, friends, log, pause)
    {
        _emote = emote;
        _log = log;
        
        _handler = network.Connection.On<EmoteCommand, ActionResult<Unit>>(HubMethod.Emote, Handle);
    }
    
    /// <summary>
    ///     <inheritdoc cref="EmoteHandler"/>
    /// </summary>
    private ActionResult<Unit> Handle(EmoteCommand request)
    {
        Plugin.Log.Verbose($"{request}");
        
        var sender = TryGetFriendWithCorrectPermissions(Operation, request.SenderFriendCode, Permissions);
        if (sender.Result is not ActionResultEc.Success)
            return ActionResultBuilder.Fail(sender.Result);
        
        if (sender.Value is not { } friend)
            return ActionResultBuilder.Fail(ActionResultEc.ValueNotSet);

        // Check if real emote
        if (_emote.Emotes.Contains(request.Emote) is false)
        {
            _log.InvalidData(Operation, friend.NoteOrFriendCode);
            return ActionResultBuilder.Fail(ActionResultEc.ClientBadData);
        }

        // Construct command
        var command = new StringBuilder();
        command.Append('/');
        command.Append(request.Emote);
        if (request.DisplayLogMessage is false)
            command.Append(" <mo>");
        
        // Execute command
        ChatService.SendMessage(command.ToString());
        
        // Log success
        _log.Custom($"{friend.NoteOrFriendCode} made you do the {request.Emote} emote");
        
        // Success
        return ActionResultBuilder.Ok();
    }

    public void Dispose()
    {
        _handler.Dispose();
        GC.SuppressFinalize(this);
    }
}