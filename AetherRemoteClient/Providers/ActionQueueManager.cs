using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Log;
using AetherRemoteCommon;
using AetherRemoteCommon.Domain.CommonChatMode;
using System;
using System.Collections.Concurrent;
using System.Text;
using AetherRemoteClient.Uncategorized;

namespace AetherRemoteClient.Providers;

/// <summary>
/// Queues actions on the main XIV thread.
/// </summary>
public class ActionQueueManager(ChatProvider chatProvider, HistoryLogProvider historyLogProvider)
{
    private const int MinProcessTime = 6000;
    private const int MaxProcessTime = 10000;

    private readonly ConcurrentQueue<IChatAction> _queue = new();
    private readonly Random _random = new();

    private DateTime _timeLastUpdated = DateTime.Now;
    private double _timeUntilNextProcess;

    private void Process(IChatAction action)
    {
        if (GameObjectManager.LocalPlayerExists() is false)
            return;

        // TODO: Revisit this
        var finalizedChatCommand = action.BuildAction();
        if (finalizedChatCommand.Length > Constraints.SpeakCommandCharLimit + Constraints.TellTargetLimit)
        {
            Plugin.Log.Warning($"Prevented processing a message over {Constraints.SpeakCommandCharLimit + Constraints.TellTargetLimit} characters");
            return;
        }

        chatProvider.SendMessage(finalizedChatCommand);

        var message = action.BuildLog();
        Plugin.Log.Information(message);
        historyLogProvider.LogHistory(message);
    }

    /// <summary>
    /// Updates the action queue provider. This should be called once per framework update
    /// </summary>
    public void Update()
    {
        if (_queue.IsEmpty || GameObjectManager.LocalPlayerExists() is false)
            return;

        var now = DateTime.Now;
        var delta = (now - _timeLastUpdated).TotalMilliseconds;
        _timeLastUpdated = now;

        if (_timeUntilNextProcess <= 0)
        {
            if (_queue.TryDequeue(out var value))
            {
                Process(value);
            }
            else
            {
                Plugin.Log.Warning("Something went wrong processing an action!");
            }

            _timeUntilNextProcess = _random.Next(MinProcessTime, MaxProcessTime);
        }
        else
        {
            _timeUntilNextProcess -= delta;
        }
    }

    /// <summary>
    /// Adds an emote command to the queue
    /// </summary>
    public void EnqueueEmoteAction(string sender, string emote, bool displayLogMessage)
    {
        var action = new EmoteAction(sender, emote, displayLogMessage);
        _queue.Enqueue(action);
    }

    /// <summary>
    /// Adds a speak command to the queue
    /// </summary>
    public void EnqueueSpeakAction(string sender, string message, ChatMode channel, string? extra)
    {
        var action = new SpeakAction(sender, message, channel, extra);
        _queue.Enqueue(action);
    }

    /// <summary>
    /// Clears all the action queues
    /// </summary>
    public void Clear() => _queue.Clear();

    /// <summary>
    /// Container holding the information required to process an emote command
    /// </summary>
    private readonly struct EmoteAction(string sender, string emote, bool displayLogMessage) : IChatAction
    {
        public readonly string BuildAction() => $"/{emote}{(displayLogMessage ? string.Empty : " motion")}";
        public readonly string BuildLog() => $"{sender} made you do the {emote} emote";
    }

    /// <summary>
    /// Container holding the information required to process a speak command
    /// </summary>
    private readonly struct SpeakAction(string sender, string message, ChatMode channel, string? extra) : IChatAction
    {
        public readonly string BuildAction() => channel switch
        {
            ChatMode.Say => $"/{(extra == "1" ? "em" : "say")} {message}",
            ChatMode.Linkshell => $"/l{extra} {message}",
            ChatMode.CrossWorldLinkshell => $"/cwl{extra} {message}",
            ChatMode.Tell => $"/t {extra} {message}",
            _ => $"/{channel.Command()} {message}",
        };

        public readonly string BuildLog() => channel switch
        {
            ChatMode.Linkshell => $"{sender} made you say \"{message}\" in {channel.Beautify()} {extra}.",
            ChatMode.CrossWorldLinkshell => $"{sender} made you say \"{message}\" in {channel.Beautify()} {extra}.",
            ChatMode.Tell => $"{sender} made you say \"{message}\" to {extra} in a tell.",
            _ => $"{sender} made you say \"{message}\" in {channel.Beautify()} chat.",
        };
    }

    private interface IChatAction
    {
        public string BuildAction();
        public string BuildLog();
    }
}
