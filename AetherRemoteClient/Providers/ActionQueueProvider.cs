using AetherRemoteClient.Accessors.Glamourer;
using AetherRemoteClient.Domain.Logger;
using AetherRemoteCommon.Domain.CommonChatMode;
using AetherRemoteCommon.Domain.CommonGlamourerApplyType;
using Dalamud.Plugin.Services;
using System;
using System.Collections.Concurrent;
using System.Text;

namespace AetherRemoteClient.Providers;

/// <summary>
/// Queues actions on the main XIV thread.
/// </summary>
public class ActionQueueProvider(Chat chat, GlamourerAccessor glamourerAccessor, AetherRemoteLogger logger, IClientState clientState)
{
    // Data
    private readonly ChatActionQueue chatActionQueue = new(logger, clientState, chat);
    private readonly GlamourerActionQueue glamourerActionQueue = new(logger, clientState, glamourerAccessor);

    public void Update()
    {
        chatActionQueue.Update();
        glamourerActionQueue.Update();
    }

    public void EnqueueBecomeAction(string sender, string data, GlamourerApplyType applyType)
    {
        var action = new BecomeAction(sender, data, applyType);
        glamourerActionQueue.EnqueueAction(action);
    }

    public void EnqueueEmoteAction(string sender, string emote)
    {
        var action = new EmoteAction(sender, emote);
        chatActionQueue.EnqueueAction(action);
    }

    public void EnqueueSpeakAction(string sender, string message, ChatMode channel, string? extra)
    {
        var action = new SpeakAction(sender, message, channel, extra);
        chatActionQueue.EnqueueAction(action);
    }

    private class ChatActionQueue(AetherRemoteLogger logger, IClientState clientState, Chat chat) : ActionQueue<IChatAction>(logger, clientState)
    {
        private readonly IClientState clientState = clientState;
        private readonly Chat chat = chat;

        protected override void Process(IChatAction action)
        {
            if (clientState.LocalPlayer == null)
                return;

            chat.SendMessage(action.Build());

            action.Log();
        }
    }

    private class GlamourerActionQueue(AetherRemoteLogger logger, IClientState clientState, GlamourerAccessor glamourerAccessor) : ActionQueue<BecomeAction>(logger, clientState)
    {
        private readonly IClientState clientState = clientState;
        private readonly GlamourerAccessor glamourerAccessor = glamourerAccessor;

        protected override void Process(BecomeAction action)
        {
            if (clientState.LocalPlayer == null)
                return;

            var localPlayerName = clientState.LocalPlayer.Name.ToString();

            glamourerAccessor.ApplyDesign(localPlayerName, action.Data, action.ApplyType);

            action.Log();
        }
    }

    private abstract class ActionQueue<T>(AetherRemoteLogger logger, IClientState clientState)
    {
        protected virtual int MinProcessTime { get; set; } = 6000;
        protected virtual int MaxProcessTime { get; set; } = 10000;

        private readonly IClientState clientState = clientState;

        private readonly AetherRemoteLogger logger = logger;
        private readonly ConcurrentQueue<T> queue = new();
        private readonly Random random = new();

        private DateTime timeLastUpdated = DateTime.Now;
        private double timeUntilNextProcess = 0;

        protected abstract void Process(T action);

        public void EnqueueAction(T action)
        {
            queue.Enqueue(action);
        }

        public void Update()
        {
            if (queue.IsEmpty || clientState.LocalPlayer == null)
                return;

            var now = DateTime.Now;
            var delta = (now - timeLastUpdated).TotalMilliseconds;
            timeLastUpdated = now;

            if (timeUntilNextProcess <= 0)
            {
                try
                {
                    if (queue.TryDequeue(out var value))
                    {
                        if (value == null)
                            return;

                        Process(value);
                    }
                }
                catch (Exception e)
                {
                    logger.Error($"Something went wrong processing an action: {e}");
                }

                timeUntilNextProcess = random.Next(MinProcessTime, MaxProcessTime);
            }
            else
            {
                timeUntilNextProcess -= delta;
            }
        }
    }

    private class EmoteAction(string sender, string emote) : IChatAction
    {
        public string Sender = sender;
        public string Emote = emote;

        public string Build()
        {
            return $"/{Emote} motion";
        }

        public void Log()
        {
            var sb = new StringBuilder();
            sb.Append(Sender);
            sb.Append(" made you do the ");
            sb.Append(Emote);
            sb.Append(" emote.");

            // TODO: Actually log properly
        }
    }

    private class BecomeAction(string sender, string data, GlamourerApplyType applyType) : IQueueAction
    {
        public string Sender = sender;
        public string Data = data;
        public GlamourerApplyType ApplyType = applyType;

        public void Log()
        {
            var sb = new StringBuilder();
            sb.Append(Sender);
            switch (ApplyType)
            {
                case GlamourerApplyType.Equipment:
                    sb.Append(" made you wear this outfit: [");
                    break;

                case GlamourerApplyType.Customization:
                    sb.Append(" transformed you into this person: [");
                    break;

                case GlamourerApplyType.CustomizationAndEquipment:
                    sb.Append(" transformed you into a perfect copy of this person: [");
                    break;
            }

            sb.Append(Data);
            sb.Append("].");

            // TODO: Actually log properly
        }
    }

    private class SpeakAction(string sender, string message, ChatMode channel, string? extra) : IChatAction
    {
        public string Sender = sender;
        public string Message = message;
        public ChatMode Channel = channel;
        public string? Extra = extra;

        public string Build()
        {
            var chatCommand = new StringBuilder();

            chatCommand.Append('/');
            chatCommand.Append(Channel.ToChatCommand());

            if (Channel == ChatMode.Linkshell || Channel == ChatMode.CrossworldLinkshell)
                chatCommand.Append(Extra);

            chatCommand.Append(' ');

            if (Channel == ChatMode.Tell)
            {
                chatCommand.Append(Extra);
                chatCommand.Append(' ');
            }

            chatCommand.Append(Message);

            return chatCommand.ToString();
        }

        public void Log()
        {
            var sb = new StringBuilder();
            sb.Append(Sender);
            sb.Append(" made you ");
            if (Channel == ChatMode.Tell)
            {
                sb.Append("send a tell to ");
                sb.Append(Extra);
                sb.Append(" saying: \"");
                sb.Append(Message);
                sb.Append("\".");
            }
            else
            {
                sb.Append("say: \"");
                sb.Append(Message);
                sb.Append("\" in ");
                sb.Append(Channel.ToCondensedString());
                if (Channel == ChatMode.Linkshell || Channel == ChatMode.CrossworldLinkshell)
                {
                    sb.Append(Extra);
                }
                sb.Append('.');
            }

            // TODO: Actually log properly
        }
    }

    private interface IChatAction : IQueueAction
    {
        public string Build();
    }

    private interface IQueueAction
    {
        public void Log();
    }
}
