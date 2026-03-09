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
    public static PrimaryPermissions ToPrimaryPermissions(this CharacterAttributes attributes)
    {
        var permissions = PrimaryPermissions.None;
        if ((attributes & CharacterAttributes.GlamourerCustomization) is CharacterAttributes.GlamourerCustomization)
            permissions |= PrimaryPermissions.GlamourerCustomization;
        
        if ((attributes & CharacterAttributes.GlamourerEquipment) is CharacterAttributes.GlamourerEquipment)
            permissions |= PrimaryPermissions.GlamourerEquipment;
        
        if ((attributes & CharacterAttributes.PenumbraMods) is CharacterAttributes.PenumbraMods)
            permissions |= PrimaryPermissions.Mods;
        
        if ((attributes & CharacterAttributes.Honorific) is CharacterAttributes.Honorific)
            permissions |= PrimaryPermissions.Honorific;
        
        if ((attributes & CharacterAttributes.Moodles) is CharacterAttributes.Moodles)
            permissions |= PrimaryPermissions.Moodles;
        
        if ((attributes & CharacterAttributes.CustomizePlus) is CharacterAttributes.CustomizePlus)
            permissions |= PrimaryPermissions.CustomizePlus;
        
        return permissions;
    }
}