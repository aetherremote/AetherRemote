using System;
using System.Text;
using AetherRemoteClient.Handlers.Network.Base;
using AetherRemoteClient.Services;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Speak;
using AetherRemoteCommon.Util;
using Microsoft.AspNetCore.SignalR.Client;

namespace AetherRemoteClient.Handlers.Network;

/// <summary>
///     Handles a <see cref="SpeakCommand"/>
/// </summary>
public class SpeakHandler : AbstractNetworkHandler, IDisposable
{
    // Const
    private const string Operation = "Speak";
    
    // Injected
    private readonly ActionQueueService _actionQueue;
    private readonly LogService _log;
    
    // Instantiated
    private readonly IDisposable _handler;

    /// <summary>
    ///     <inheritdoc cref="SpeakHandler"/>
    /// </summary>
    public SpeakHandler(ActionQueueService actionQueue, FriendsListService friends, LogService log, NetworkService network, PauseService pause) : base(friends, log, pause)
    {
        _actionQueue = actionQueue;
        _log = log;

        _handler = network.Connection.On<SpeakCommand, ActionResult<Unit>>(HubMethod.Speak, Handle);
    }
    
    /// <summary>
    ///     <inheritdoc cref="SpeakHandler"/>
    /// </summary>
    private ActionResult<Unit> Handle(SpeakCommand request)
    {
        Plugin.Log.Verbose($"{request}");
        
        var speakPermissions = request.ChatChannel.ToSpeakPermissions(request.Extra);
        var permissions = new UserPermissions(PrimaryPermissions2.None, speakPermissions, ElevatedPermissions.None);
        
        var sender = TryGetFriendWithCorrectPermissions(Operation, request.SenderFriendCode, permissions);
        if (sender.Result is not ActionResultEc.Success)
            return ActionResultBuilder.Fail(sender.Result);
        
        if (sender.Value is not { } friend)
            return ActionResultBuilder.Fail(ActionResultEc.ValueNotSet);

        // Add the action to the action queue to be sent when available
        _actionQueue.Enqueue(friend, request.Message, request.ChatChannel, request.Extra);

        // Build a proper log message with specific formatting
        var log = new StringBuilder();
        log.Append(friend.NoteOrFriendCode);
        log.Append(" made you say ");
        log.Append(request.Message);
        switch (request.ChatChannel)
        {
            case ChatChannel.Linkshell:
            case ChatChannel.CrossWorldLinkshell:
                log.Append(" in ");
                log.Append(request.ChatChannel.Beautify());
                log.Append(request.Extra);
                break;
            
            case ChatChannel.Tell:
                log.Append(" in a tell to ");
                log.Append(request.Extra);
                break;

            case ChatChannel.Say:
            case ChatChannel.Roleplay:
            case ChatChannel.Echo:
            case ChatChannel.Yell:
            case ChatChannel.Shout:
            case ChatChannel.Party:
            case ChatChannel.Alliance:
            case ChatChannel.FreeCompany:
            case ChatChannel.PvPTeam:
            default:
                log.Append(" in ");
                log.Append(request.ChatChannel.Beautify());
                log.Append(" chat");
                break;
        }
        
        // Add log to history
        _log.Custom(log.ToString());
        return ActionResultBuilder.Ok();
    }

    public void Dispose()
    {
        _handler.Dispose();
        GC.SuppressFinalize(this);
    }
}