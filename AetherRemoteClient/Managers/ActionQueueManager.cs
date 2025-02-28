using System;
using System.Collections.Concurrent;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Services;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Util;

namespace AetherRemoteClient.Managers;

/// <summary>
///     Responsible for sending actions to the in-game chat
/// </summary>
public class ActionQueueManager(ChatService chatService)
{
    private const int MinProcessTime = 1100;
    private const int MaxProcessTime = 2400;

    private readonly ConcurrentQueue<ChatAction> _actions = new();
    private readonly Random _random = new();
    
    private DateTime _timeLastUpdated = DateTime.Now;
    private double _timeUntilNextProcess;
    
    /// <summary>
    ///     Enqueues a chat command to take place. An empty queue will process a message immediately.
    /// </summary>
    public void Enqueue(Friend sender, string message, ChatChannel channel, string? extra)
    {
        string command;
        string log;
        switch (channel)
        {
            case ChatChannel.Tell:
                if (extra is null)
                {
                    Plugin.Log.Warning("Could not enqueue tell action because target data is missing");
                    return;
                }
                
                command = $"/{channel.ChatCommand()} {extra} {message}";
                log = $"{sender.NoteOrFriendCode} made you send a tell to {extra} saying {message}";
                break;
            
            case ChatChannel.Linkshell:
            case ChatChannel.CrossWorldLinkshell:
                if (extra is null)
                {
                    Plugin.Log.Warning("Could not enqueue linkshell action because linkshell number is missing");
                    return;
                }
                
                command = $"/{channel.ChatCommand()}{extra} {message}";
                log = $"{sender.NoteOrFriendCode} made you send a message in {channel.Beautify()} {extra} saying {message}";
                break;
            
            case ChatChannel.ChatEmote:
                command = $"/{channel.ChatCommand()} {message}";
                log = $"{sender.NoteOrFriendCode} made you emote the following: {message}";
                break;

            case ChatChannel.Say:
            case ChatChannel.Echo:
            case ChatChannel.Yell:
            case ChatChannel.Shout:
            case ChatChannel.Party:
            case ChatChannel.Alliance:
            case ChatChannel.FreeCompany:
            case ChatChannel.NoviceNetwork:
            case ChatChannel.PvPTeam:
            default:
                command = $"/{channel.ChatCommand()} {message}";
                log = $"{sender.NoteOrFriendCode} made you send a message in {channel.Beautify()} chat saying {message}";
                break;
        }
        
        _actions.Enqueue(new ChatAction { Command = command, Log = log });
    }
    
    /// <summary>
    ///     Must be called once every framework ticks
    /// </summary>
    public void Update()
    {
        if (_actions.IsEmpty || Plugin.ClientState.LocalPlayer is null)
            return;

        var now = DateTime.Now;
        var delta = (now - _timeLastUpdated).TotalMilliseconds;
        _timeLastUpdated = now;

        if (_timeUntilNextProcess > 0)
        {
            _timeUntilNextProcess -= delta;
            return;
        }
        
        // Begin processing a message
        if (_actions.TryDequeue(out var action) is false)
        {
            Plugin.Log.Warning("Something went wrong processing an action!");
            return;
        }

        // Cannot send messages during pvp
        if (Plugin.ClientState.IsPvPExcludingDen)
            return; 
        
        chatService.SendMessage(action.Command);
        Plugin.Log.Info(action.Log);
        
        // Queue next process
        _timeUntilNextProcess = _random.Next(MinProcessTime, MaxProcessTime);
    }

    /// <summary>
    ///     Clears all pending actions
    /// </summary>
    public void Clear() => _actions.Clear();
    
    private class ChatAction
    {
        /// <summary>
        ///     Message that is executed in chat
        /// </summary>
        public string Command = string.Empty;
        
        /// <summary>
        ///     Message that is posted in logs
        /// </summary>
        public string Log = string.Empty;
    }
}