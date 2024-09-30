using AetherRemoteCommon.Domain.CommonChatMode;
using AetherRemoteCommon.Domain.CommonGlamourerApplyType;

namespace AetherRemoteCommon.Domain;

public static class PermissionChecker
{
    public static bool HasValidTransformPermissions(GlamourerApplyFlag applyFlags, UserPermissions permissions)
    {
        if (applyFlags.HasFlag(GlamourerApplyFlag.Customization) && !permissions.HasFlag(UserPermissions.Customization))
            return false;

        if (applyFlags.HasFlag(GlamourerApplyFlag.Equipment) && !permissions.HasFlag(UserPermissions.Equipment))
            return false;

        return true;
    }

    public static bool HasValidEmotePermissions(UserPermissions user)
    {
        return user.HasFlag(UserPermissions.Emote);
    }

    public static bool HasValidSpeakPermissions(ChatMode chatMode, UserPermissions permissions, int linkshellNumber = 0)
    {
        if (permissions.HasFlag(UserPermissions.Speak) == false)
            return false;

        return chatMode switch
        {
            ChatMode.Say => permissions.HasFlag(UserPermissions.Say),
            ChatMode.Yell => permissions.HasFlag(UserPermissions.Yell),
            ChatMode.Shout => permissions.HasFlag(UserPermissions.Shout),
            ChatMode.Tell => permissions.HasFlag(UserPermissions.Tell),
            ChatMode.Party => permissions.HasFlag(UserPermissions.Party),
            ChatMode.Alliance => permissions.HasFlag(UserPermissions.Alliance),
            ChatMode.FreeCompany => permissions.HasFlag(UserPermissions.FreeCompany),
            ChatMode.PvPTeam => permissions.HasFlag(UserPermissions.PvPTeam),
            ChatMode.Linkshell => HasValidLinkshellPermissions(linkshellNumber, permissions),
            ChatMode.CrossworldLinkshell => HasValidCrossworldLinkshellPermissions(linkshellNumber, permissions),
            _ => false
        };
    }

    private static bool HasValidLinkshellPermissions(int linkshellNumber, UserPermissions permissions)
    {
        if (linkshellNumber < 1 || linkshellNumber > 8) return false;
        var linkshellPermission = (int)UserPermissions.LS1 << (linkshellNumber - 1);
        return permissions.HasFlag((UserPermissions)linkshellPermission);
    }

    private static bool HasValidCrossworldLinkshellPermissions(int linkshellNumber, UserPermissions permissions)
    {
        if (linkshellNumber < 1 || linkshellNumber > 8) return false;
        var linkshellPermission = (int)UserPermissions.CWL1 << (linkshellNumber - 1);
        return permissions.HasFlag((UserPermissions)linkshellPermission);
    }
}
