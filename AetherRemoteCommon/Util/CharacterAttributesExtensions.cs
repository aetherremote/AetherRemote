using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.New;

namespace AetherRemoteCommon.Util;

/// <summary>
///     TODO
/// </summary>
public static class CharacterAttributesExtensions
{
    /// <summary>
    ///     TODO
    /// </summary>
    public static PrimaryPermissions2 ToPrimaryPermission(this CharacterAttributes attributes)
    {
        var permissions = PrimaryPermissions2.Twinning;
        if ((attributes & CharacterAttributes.Mods) == CharacterAttributes.Mods)
            permissions |= PrimaryPermissions2.Mods;

        if ((attributes & CharacterAttributes.Moodles) == CharacterAttributes.Moodles)
            permissions |= PrimaryPermissions2.Moodles;

        if ((attributes & CharacterAttributes.CustomizePlus) == CharacterAttributes.CustomizePlus)
            permissions |= PrimaryPermissions2.CustomizePlus;

        return permissions;
    }
}