using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;

namespace AetherRemoteCommon.Util;

/// <summary>
///     Extension methods for <see cref="ChatChannel"/>
/// </summary>
public static class ChatChannelExtensions
{
    /// <summary>
    ///     Converts a chat channel to a more readable format
    /// </summary>
    public static string Beautify(this ChatChannel chatChannel)
    {
        return chatChannel switch
        {
            ChatChannel.Linkshell => "LS",
            ChatChannel.FreeCompany => "Free Company",
            ChatChannel.CrossWorldLinkshell => "CWLS",
            ChatChannel.PvPTeam => "PvP Team",
            ChatChannel.Roleplay => "Chat Emote",
            _ => chatChannel.ToString()
        };
    }

    /// <summary>
    ///     Gets the chat command for the corresponding chat channel without the /
    /// </summary>
    public static string ChatCommand(this ChatChannel chatChannel)
    {
        return chatChannel switch
        {
            ChatChannel.Say => "s",
            ChatChannel.Yell => "y",
            ChatChannel.Shout => "sh",
            ChatChannel.Tell => "t",
            ChatChannel.Party => "p",
            ChatChannel.Alliance => "a",
            ChatChannel.FreeCompany => "fc",
            ChatChannel.Linkshell => "l",
            ChatChannel.CrossWorldLinkshell => "cwl",
            ChatChannel.PvPTeam => "pt",
            ChatChannel.Roleplay => "em",
            ChatChannel.Echo => "echo",
            _ => "Not Implemented"
        };
    }

    /// <summary>
    ///     Convert a chat channel to speak permissions
    /// </summary>
    public static SpeakPermissions ToSpeakPermissions(this ChatChannel chatChannel, string? extra = null)
    {
        return chatChannel switch
        {
            ChatChannel.Say => SpeakPermissions.Say,
            ChatChannel.Roleplay => SpeakPermissions.Roleplay,
            ChatChannel.Echo => SpeakPermissions.Echo,
            ChatChannel.Yell => SpeakPermissions.Yell,
            ChatChannel.Shout => SpeakPermissions.Shout,
            ChatChannel.Tell => SpeakPermissions.Tell,
            ChatChannel.Party => SpeakPermissions.Party,
            ChatChannel.Alliance => SpeakPermissions.Alliance,
            ChatChannel.FreeCompany => SpeakPermissions.FreeCompany,
            ChatChannel.PvPTeam => SpeakPermissions.PvPTeam,
            ChatChannel.Linkshell => ConvertToLinkshell(SpeakPermissions.Ls1, extra),
            ChatChannel.CrossWorldLinkshell => ConvertToLinkshell(SpeakPermissions.Cwl1, extra),
            _ => SpeakPermissions.None
        };
    }
    
    private static SpeakPermissions ConvertToLinkshell(SpeakPermissions starting, string? extra)
    {
        return int.TryParse(extra, out var number)
            ? (SpeakPermissions)((int)starting << (number - 1))
            : SpeakPermissions.None;
    }
}