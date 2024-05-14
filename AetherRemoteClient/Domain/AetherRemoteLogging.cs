using AetherRemoteCommon.Domain.CommonChatMode;
using AetherRemoteCommon.Domain.CommonGlamourerApplyType;
using System;
using System.Collections.Generic;
using System.Text;

namespace AetherRemoteClient.Domain;

public static class AetherRemoteLogging
{
    // List of all logs
    public static readonly List<LogEntry> Logs = [];

    // Events
    public static event EventHandler? OnErrorLogged;

    public static void Log(string sender, string message, DateTime timestamp, LogType type)
    {
        if (type == LogType.Error)
            OnErrorLogged?.Invoke(null, EventArgs.Empty);

        var log = new LogEntry(sender, message, timestamp, type);
        Logs.Add(log);
    }

    public static string FormatSpeakLog(string target, ChatMode chatMode, string message, string? extra)
    {
        var sb = new StringBuilder();
        sb.Append("You made ");
        sb.Append(target);
        if (chatMode == ChatMode.Tell)
        {
            sb.Append("send a tell to ");
            sb.Append(extra);
            sb.Append(" saying: \"");
            sb.Append(message);
            sb.Append("\".");
        }
        else
        {
            sb.Append("say: \"");
            sb.Append(message);
            sb.Append("\" in ");
            sb.Append(chatMode.ToCondensedString());
            if (chatMode == ChatMode.Linkshell || chatMode == ChatMode.CrossworldLinkshell)
            {
                sb.Append(extra);
            }
            sb.Append('.');
        }

        return sb.ToString();
    }

    public static string FormatEmoteLog(string target, string emote)
    {
        var sb = new StringBuilder();
        sb.Append("You made ");
        sb.Append(target);
        sb.Append(" do the ");
        sb.Append(emote);
        sb.Append(" emote.");

        return sb.ToString();
    }

    public static string FormatBecomeLog(string target, GlamourerApplyType glamourerApplyType, string glamourerData)
    {
        var sb = new StringBuilder();
        sb.Append("You made ");
        sb.Append(target);
        switch (glamourerApplyType)
        {
            case GlamourerApplyType.Equipment:
                sb.Append(" wear this outfit: [");
                break;

            case GlamourerApplyType.Customization:
                sb.Append(" transform into this person: [");
                break;

            case GlamourerApplyType.CustomizationAndEquipment:
                sb.Append(" transform into a perfect copy of this person: [");
                break;
        }

        sb.Append(glamourerData);
        sb.Append("].");

        return sb.ToString();
    }
}

public struct LogEntry(string sender, string message, DateTime timestamp, LogType type)
{
    public string Sender = sender;
    public string Message = message;
    public DateTime Timestamp = timestamp;
    public LogType Type = type;
}

public enum LogType
{
    Sent,
    Recieved,
    Info,
    Error
}
