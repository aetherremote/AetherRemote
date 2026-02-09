using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;

namespace AetherRemoteCommon.Util;

/// <summary>
///     Extension class for <see cref="CharacterAttributes"/>
/// </summary>
public static class CharacterAttributesExtensions
{
    /// <summary>
    ///     Converts <see cref="CharacterAttributes"/> to <see cref="PrimaryPermissions"/>
    /// </summary>
    public static PrimaryPermissions ToPrimaryPermission(this CharacterAttributes attributes)
    {
        var permissions = PrimaryPermissions.None;
        if ((attributes & CharacterAttributes.Mods) == CharacterAttributes.Mods)
            permissions |= PrimaryPermissions.Mods;

        if ((attributes & CharacterAttributes.Moodles) == CharacterAttributes.Moodles)
            permissions |= PrimaryPermissions.Moodles;

        if ((attributes & CharacterAttributes.CustomizePlus) == CharacterAttributes.CustomizePlus)
            permissions |= PrimaryPermissions.CustomizePlus;

        return permissions;
    }
}