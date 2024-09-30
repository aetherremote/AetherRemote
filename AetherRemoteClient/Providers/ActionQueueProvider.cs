using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Log;
using AetherRemoteCommon.Domain.CommonChatMode;
using System;
using System.Collections.Concurrent;

namespace AetherRemoteClient.Providers;

/// <summary>
/// Queues actions on the main XIV thread.
/// </summary>
public class ActionQueueProvider
{
    // Injected
    private readonly Chat chat;
    private readonly HistoryLogManager historyLogManager;

    private const int MinProcessTime = 6000;
    private const int MaxProcessTime = 10000;

    private readonly ConcurrentQueue<IChatAction> queue = new();
    private readonly Random random = new();

    private DateTime timeLastUpdated = DateTime.Now;
    private double timeUntilNextProcess = 0;

    public ActionQueueProvider(Chat chat, HistoryLogManager historyLogManager)
    {
        this.chat = chat;
        this.historyLogManager = historyLogManager;
    }

    private void Process(IChatAction action)
    {
        if (Plugin.ClientState.LocalPlayer == null)
            return;

        chat.SendMessage(action.BuildAction());

        var message = action.BuildLog();
        Plugin.Log.Information(message);
        historyLogManager.LogHistory(message);
    }

    public void Update()
    {
        if (queue.IsEmpty || Plugin.ClientState.LocalPlayer == null)
            return;

        var now = DateTime.Now;
        var delta = (now - timeLastUpdated).TotalMilliseconds;
        timeLastUpdated = now;

        if (timeUntilNextProcess <= 0)
        {
            if (queue.TryDequeue(out var value))
            {
                if (value == null)
                    return;

                Process(value);
            }
            else
            {
                Plugin.Log.Warning($"Something went wrong processing an action!");
            }

            timeUntilNextProcess = random.Next(MinProcessTime, MaxProcessTime);
        }
        else
        {
            timeUntilNextProcess -= delta;
        }
    }

    public void EnqueueEmoteAction(string sender, string emote)
    {
        var action = new EmoteAction(sender, emote);
        queue.Enqueue(action);
    }

    public void EnqueueSpeakAction(string sender, string message, ChatMode channel, string? extra)
    {
        var action = new SpeakAction(sender, message, channel, extra);
        queue.Enqueue(action);
    }

    /// <summary>
    /// Clears all the action queues
    /// </summary>
    public void Clear() => queue.Clear();

    private struct EmoteAction(string sender, string emote) : IChatAction
    {
        public string Sender = sender;
        public string Emote = emote;

        public readonly string BuildAction() => $"/{Emote} motion";
        public readonly string BuildLog() => $"{Sender} made you do the {Emote} emote";
    }

    private struct SpeakAction(string sender, string message, ChatMode channel, string? extra) : IChatAction
    {
        public string Sender = sender;
        public string Message = message;
        public ChatMode Channel = channel;
        public string? Extra = extra;

        public readonly string BuildAction() => Channel switch
        {
            ChatMode.Linkshell => $"/l{Extra} {Message}",
            ChatMode.CrossworldLinkshell => $"/cwl{Extra} {Message}",
            ChatMode.Tell => $"/t {Extra} {Message}",
            _ => $"/{Channel.Command()} {Message}",
        };

        public readonly string BuildLog() => Channel switch
        {
            ChatMode.Linkshell => $"{Sender} made you say \"{Message}\" in {Channel.Beautify()} {Extra}.",
            ChatMode.CrossworldLinkshell => $"{Sender} made you say \"{Message}\" in {Channel.Beautify()} {Extra}.",
            ChatMode.Tell => $"{Sender} made you say \"{Message}\" to {Extra} in a tell.",
            _ => $"{Sender} made you say \"{Message}\" in {Channel.Beautify()} chat.",
        };
    }

    private interface IChatAction
    {
        public string BuildAction();
        public string BuildLog();
    }
}
