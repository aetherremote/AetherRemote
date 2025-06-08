using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.New;

namespace AetherRemoteCommon.Util;

/// <summary>
///     Extension methods for <see cref="GlamourerApplyFlags"/>
/// </summary>
public static class GlamourerApplyFlagsExtensions
{
    /// <summary>
    ///     TODO
    /// </summary>
    public static PrimaryPermissions2 ToPrimaryPermission(this GlamourerApplyFlags applyFlags)
    {
        var permissions = PrimaryPermissions2.Twinning;
        if ((applyFlags & GlamourerApplyFlags.Customization) == GlamourerApplyFlags.Customization)
            permissions |= PrimaryPermissions2.GlamourerCustomization;

        if ((applyFlags & GlamourerApplyFlags.Equipment) == GlamourerApplyFlags.Equipment)
            permissions |= PrimaryPermissions2.GlamourerEquipment;

        return permissions;
    }
}