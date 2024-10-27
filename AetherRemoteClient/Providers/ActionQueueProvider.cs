using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Log;
using AetherRemoteCommon;
using AetherRemoteCommon.Domain.CommonChatMode;
using System;
using System.Collections.Concurrent;
using System.Text;

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

    /// <summary>
    /// <inheritdoc cref="ActionQueueProvider"/>
    /// </summary>
    public ActionQueueProvider(Chat chat, HistoryLogManager historyLogManager)
    {
        this.chat = chat;
        this.historyLogManager = historyLogManager;
    }

    private void Process(IChatAction action)
    {
        if (Plugin.ClientState.LocalPlayer == null)
            return;

        // TODO: Revisit this
        var finalizedChatCommand = action.BuildAction();
        if (finalizedChatCommand.Length > Constraints.SpeakCommandCharLimit + Constraints.TellTargetLimit)
        {
            Plugin.Log.Warning($"Prevented processing a message over {Constraints.SpeakCommandCharLimit + Constraints.TellTargetLimit} characters");
            return;
        }

        chat.SendMessage(finalizedChatCommand);

        var message = action.BuildLog();
        Plugin.Log.Information(message);
        historyLogManager.LogHistory(message);
    }

    /// <summary>
    /// Updates the action queue provider. This should be called once per framework update
    /// </summary>
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

    /// <summary>
    /// Adds an emote command to the queue
    /// </summary>
    public void EnqueueEmoteAction(string sender, string emote, bool displayLogMessage)
    {
        var action = new EmoteAction(sender, emote, displayLogMessage);
        queue.Enqueue(action);
    }

    /// <summary>
    /// Adds a speak command to the queue
    /// </summary>
    public void EnqueueSpeakAction(string sender, string message, ChatMode channel, string? extra)
    {
        var action = new SpeakAction(sender, message, channel, extra);
        queue.Enqueue(action);
    }

    /// <summary>
    /// Clears all the action queues
    /// </summary>
    public void Clear() => queue.Clear();

    /// <summary>
    /// Container holding the information required to process an emote command
    /// </summary>
    private struct EmoteAction(string sender, string emote, bool displayLogMessage) : IChatAction
    {
        public string Sender = sender;
        public string Emote = emote;
        public bool DisplayLogMessage = displayLogMessage;

        public readonly string BuildAction() => $"/{Emote}{(DisplayLogMessage ? string.Empty : " motion")}";
        public readonly string BuildLog() => $"{Sender} made you do the {Emote} emote";
    }

    /// <summary>
    /// Container holding the information required to process a speak command
    /// </summary>
    private struct SpeakAction(string sender, string message, ChatMode channel, string? extra) : IChatAction
    {
        public string Sender = sender;
        public string Message = message;
        public ChatMode Channel = channel;
        public string? Extra = extra;

        public readonly string BuildAction() => Channel switch
        {
            ChatMode.Say => $"/{(Extra == "1" ? "em" : "say")} {Message}",
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
