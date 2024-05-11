using AetherRemoteCommon.Domain.CommonChatMode;
using AetherRemoteCommon.Domain.CommonFriendPermissions;
using AetherRemoteCommon.Domain.CommonGlamourerApplyType;

namespace AetherRemoteCommon.Domain;

public static class PermissionChecker
{
    public static bool HasGlamourerPermission(GlamourerApplyType applyType, FriendPermissions permissions)
    {
        return applyType switch
        {
            GlamourerApplyType.Customization => permissions.ChangeAppearance,
            GlamourerApplyType.Equipment => permissions.ChangeEquipment,
            GlamourerApplyType.CustomizationAndEquipment => permissions.ChangeAppearance || permissions.ChangeAppearance,
            _ => false,
        };
    }

    public static bool HasSpeakPermission(ChatMode chatMode, FriendPermissions permissions)
    {
        if (permissions.Speak == false)
            return false;

        return chatMode switch
        {
            ChatMode.Alliance => permissions.Alliance,
            ChatMode.CrossworldLinkshell => permissions.CrossworldLinkshell,
            ChatMode.FreeCompany => permissions.FreeCompany,
            ChatMode.Linkshell => permissions.Linkshell,
            ChatMode.Party => permissions.Party,
            ChatMode.PvPTeam => permissions.PvPTeam,
            ChatMode.Say => permissions.Say,
            ChatMode.Shout => permissions.Shout,
            ChatMode.Tell => permissions.Tell,
            ChatMode.Yell => permissions.Yell,
            _ => false,
        };
    }

    public static bool HasEmotePermission(FriendPermissions permissions)
    {
        return permissions.Emote;
    }
}
