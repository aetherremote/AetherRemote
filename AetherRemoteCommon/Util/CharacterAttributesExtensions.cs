using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;

namespace AetherRemoteCommon.Util;

/// <summary>
///     Extension class for <see cref="CharacterAttributes"/>
/// </summary>
public static class CharacterAttributesExtensions
{
    /// <summary>
    ///     Converts <see cref="CharacterAttributes"/> to <see cref="PrimaryPermissions2"/>
    /// </summary>
    public static PrimaryPermissions2 ToPrimaryPermission(this CharacterAttributes attributes)
    {
        var permissions = PrimaryPermissions2.None;
        if ((attributes & CharacterAttributes.Mods) == CharacterAttributes.Mods)
            permissions |= PrimaryPermissions2.Mods;

        if ((attributes & CharacterAttributes.Moodles) == CharacterAttributes.Moodles)
            permissions |= PrimaryPermissions2.Moodles;

        if ((attributes & CharacterAttributes.CustomizePlus) == CharacterAttributes.CustomizePlus)
            permissions |= PrimaryPermissions2.CustomizePlus;

        return permissions;
    }
}