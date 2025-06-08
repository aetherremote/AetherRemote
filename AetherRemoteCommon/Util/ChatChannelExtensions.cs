using AetherRemoteCommon.Domain.Enums;

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
    ///     Converts a <see cref="ChatChannel"/> into the corresponding <see cref="PrimaryPermissions"/> or null if
    ///     no mapping can be made.
    /// </summary>
    public static PrimaryPermissions ToPrimaryPermissions(this ChatChannel chatChannel)
    {
        return chatChannel switch
        {
            ChatChannel.Say => PrimaryPermissions.Say,
            ChatChannel.Yell => PrimaryPermissions.Yell,
            ChatChannel.Shout => PrimaryPermissions.Shout,
            ChatChannel.Tell => PrimaryPermissions.Tell,
            ChatChannel.Party => PrimaryPermissions.Party,
            ChatChannel.Alliance => PrimaryPermissions.Alliance,
            ChatChannel.FreeCompany => PrimaryPermissions.FreeCompany,
            ChatChannel.PvPTeam => PrimaryPermissions.PvPTeam,
            ChatChannel.Echo => PrimaryPermissions.Echo,
            ChatChannel.Roleplay => PrimaryPermissions.ChatEmote,
            _ => PrimaryPermissions.None
        };
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