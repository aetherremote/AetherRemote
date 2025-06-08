using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.New;

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
    ///     TODO
    /// </summary>
    public static SpeakPermissions2 ToSpeakPermissions(this ChatChannel chatChannel, string? extra = null)
    {
        return chatChannel switch
        {
            ChatChannel.Say => SpeakPermissions2.Say,
            ChatChannel.Roleplay => SpeakPermissions2.Roleplay,
            ChatChannel.Echo => SpeakPermissions2.Echo,
            ChatChannel.Yell => SpeakPermissions2.Yell,
            ChatChannel.Shout => SpeakPermissions2.Shout,
            ChatChannel.Tell => SpeakPermissions2.Tell,
            ChatChannel.Party => SpeakPermissions2.Party,
            ChatChannel.Alliance => SpeakPermissions2.Alliance,
            ChatChannel.FreeCompany => SpeakPermissions2.FreeCompany,
            ChatChannel.PvPTeam => SpeakPermissions2.PvPTeam,
            ChatChannel.Linkshell => ConvertToLinkshell(SpeakPermissions2.Ls1, extra),
            ChatChannel.CrossWorldLinkshell => ConvertToLinkshell(SpeakPermissions2.Cwl1, extra),
            _ => SpeakPermissions2.None
        };
    }
    
    private static SpeakPermissions2 ConvertToLinkshell(SpeakPermissions2 starting, string? extra)
    {
        return int.TryParse(extra, out var number)
            ? (SpeakPermissions2)((int)starting << (number - 1))
            : SpeakPermissions2.None;
    }
    
    /// <summary>
    ///     Converts a <see cref="ChatChannel"/> into the corresponding <see cref="LinkshellPermissions"/> or null if
    ///     no mapping can be made.
    /// </summary>
    public static LinkshellPermissions ToLinkshellPermissions(this ChatChannel chatChannel, int linkshellNumber)
    {
        if (linkshellNumber is < 1 or > 8)
            return LinkshellPermissions.None;

        return chatChannel switch
        {
            ChatChannel.Linkshell => (LinkshellPermissions)((int)LinkshellPermissions.Ls1 << (linkshellNumber - 1)),
            ChatChannel.CrossWorldLinkshell => (LinkshellPermissions)((int)LinkshellPermissions.Cwl1 << (linkshellNumber - 1)),
            _ => LinkshellPermissions.None
        };
    }
}