using AetherRemoteCommon.Domain.CommonChatMode;
using AetherRemoteCommon.Domain.Permissions.V2;

namespace AetherRemoteCommon.Domain;

public static class PermissionChecker
{
    public static bool HasValidSpeakPermissions(ChatMode chatMode, UserPermissionsV2 permissions, int linkshellNumber)
    {
        if (permissions.Primary.HasFlag(PrimaryPermissionsV2.Speak) is false)
            return false;

        return chatMode switch
        {
            ChatMode.Say => permissions.Primary.HasFlag(PrimaryPermissionsV2.Say),
            ChatMode.Yell => permissions.Primary.HasFlag(PrimaryPermissionsV2.Yell),
            ChatMode.Shout => permissions.Primary.HasFlag(PrimaryPermissionsV2.Shout),
            ChatMode.Tell => permissions.Primary.HasFlag(PrimaryPermissionsV2.Tell),
            ChatMode.Party => permissions.Primary.HasFlag(PrimaryPermissionsV2.Party),
            ChatMode.Alliance => permissions.Primary.HasFlag(PrimaryPermissionsV2.Alliance),
            ChatMode.FreeCompany => permissions.Primary.HasFlag(PrimaryPermissionsV2.FreeCompany),
            ChatMode.PvPTeam => permissions.Primary.HasFlag(PrimaryPermissionsV2.PvPTeam),
            ChatMode.Linkshell => HasValidLinkshellPermissions(linkshellNumber, permissions),
            ChatMode.CrossWorldLinkshell => HasValidCrossWorldLinkshellPermissions(linkshellNumber, permissions),
            _ => false
        };
    }

    private static bool HasValidLinkshellPermissions(int linkshellNumber, UserPermissionsV2 permissions)
    {
        if (linkshellNumber is < 1 or > 8) return false;
        var linkshellPermission = (int)LinkshellPermissionsV2.Ls1 << (linkshellNumber - 1);
        return permissions.Linkshell.HasFlag((LinkshellPermissionsV2)linkshellPermission);
    }

    private static bool HasValidCrossWorldLinkshellPermissions(int linkshellNumber, UserPermissionsV2 permissions)
    {
        if (linkshellNumber is < 1 or > 8) return false;
        var linkshellPermission = (int)LinkshellPermissionsV2.Cwl1 << (linkshellNumber - 1);
        return permissions.Linkshell.HasFlag((LinkshellPermissionsV2)linkshellPermission);
    }
}
