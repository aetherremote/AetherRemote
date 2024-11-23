using AetherRemoteCommon.Domain.CommonChatMode;
using AetherRemoteCommon.Domain.Permissions;

namespace AetherRemoteCommon.Domain;

public static class PermissionChecker
{
    public static bool HasValidSpeakPermissions(ChatMode chatMode, UserPermissions permissions, int linkshellNumber)
    {
        if (permissions.Primary.HasFlag(PrimaryPermissions.Speak) is false)
            return false;

        return chatMode switch
        {
            ChatMode.Say => permissions.Primary.HasFlag(PrimaryPermissions.Say),
            ChatMode.Yell => permissions.Primary.HasFlag(PrimaryPermissions.Yell),
            ChatMode.Shout => permissions.Primary.HasFlag(PrimaryPermissions.Shout),
            ChatMode.Tell => permissions.Primary.HasFlag(PrimaryPermissions.Tell),
            ChatMode.Party => permissions.Primary.HasFlag(PrimaryPermissions.Party),
            ChatMode.Alliance => permissions.Primary.HasFlag(PrimaryPermissions.Alliance),
            ChatMode.FreeCompany => permissions.Primary.HasFlag(PrimaryPermissions.FreeCompany),
            ChatMode.PvPTeam => permissions.Primary.HasFlag(PrimaryPermissions.PvPTeam),
            ChatMode.Linkshell => HasValidLinkshellPermissions(linkshellNumber, permissions),
            ChatMode.CrossWorldLinkshell => HasValidCrossWorldLinkshellPermissions(linkshellNumber, permissions),
            _ => false
        };
    }

    private static bool HasValidLinkshellPermissions(int linkshellNumber, UserPermissions permissions)
    {
        if (linkshellNumber is < 1 or > 8) return false;
        var linkshellPermission = (int)LinkshellPermissions.Ls1 << (linkshellNumber - 1);
        return permissions.Linkshell.HasFlag((LinkshellPermissions)linkshellPermission);
    }

    private static bool HasValidCrossWorldLinkshellPermissions(int linkshellNumber, UserPermissions permissions)
    {
        if (linkshellNumber is < 1 or > 8) return false;
        var linkshellPermission = (int)LinkshellPermissions.Cwl1 << (linkshellNumber - 1);
        return permissions.Linkshell.HasFlag((LinkshellPermissions)linkshellPermission);
    }
}
