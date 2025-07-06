using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;

namespace AetherRemoteCommon.Util;

/// <summary>
///     Extension methods for <see cref="GlamourerApplyFlags"/>
/// </summary>
public static class GlamourerApplyFlagsExtensions
{
    /// <summary>
    ///     Converts glamourer apply flags to the corresponding required permissions
    /// </summary>
    public static PrimaryPermissions2 ToPrimaryPermission(this GlamourerApplyFlags applyFlags)
    {
        var permissions = PrimaryPermissions2.None;
        if ((applyFlags & GlamourerApplyFlags.Customization) == GlamourerApplyFlags.Customization)
            permissions |= PrimaryPermissions2.GlamourerCustomization;

        if ((applyFlags & GlamourerApplyFlags.Equipment) == GlamourerApplyFlags.Equipment)
            permissions |= PrimaryPermissions2.GlamourerEquipment;

        return permissions;
    }
}