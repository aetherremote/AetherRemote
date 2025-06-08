using AetherRemoteCommon.Domain.Enums;

namespace AetherRemoteCommon.Util;

public static class PermissionsChecker
{
    /// <summary>
    ///     Check if provided chat mode is granted in primary permissions
    /// </summary>
    public static bool Speak(PrimaryPermissions permissions, ChatChannel chatChannel)
    {
        if (permissions.HasFlag(PrimaryPermissions.Speak) is false)
            return false;
        
        return chatChannel switch
        {
            ChatChannel.Say => permissions.HasFlag(PrimaryPermissions.Say),
            ChatChannel.Yell => permissions.HasFlag(PrimaryPermissions.Yell),
            ChatChannel.Shout => permissions.HasFlag(PrimaryPermissions.Shout),
            ChatChannel.Tell => permissions.HasFlag(PrimaryPermissions.Tell),
            ChatChannel.Party => permissions.HasFlag(PrimaryPermissions.Party),
            ChatChannel.Alliance => permissions.HasFlag(PrimaryPermissions.Alliance),
            ChatChannel.FreeCompany => permissions.HasFlag(PrimaryPermissions.FreeCompany),
            ChatChannel.PvPTeam => permissions.HasFlag(PrimaryPermissions.PvPTeam),
            ChatChannel.Echo => permissions.HasFlag(PrimaryPermissions.Echo),
            ChatChannel.Roleplay => permissions.HasFlag(PrimaryPermissions.ChatEmote),
            _ => false
        };
    }

    /// <summary>
    ///     Check if provided linkshell is granted in linkshell permissions
    /// </summary>
    public static bool Speak(LinkshellPermissions permissions, int linkshell)
    {
        if (linkshell is < 0 or > 7)
            return false;

        var ls = (int)LinkshellPermissions.Ls1 << linkshell;
        var cwl = (int)LinkshellPermissions.Cwl1 << linkshell;

        return permissions.HasFlag((LinkshellPermissions)ls) || permissions.HasFlag((LinkshellPermissions)cwl);
    }
}