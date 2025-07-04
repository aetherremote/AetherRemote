namespace AetherRemoteCommon.Domain.Enums.Permissions;

[Flags]
public enum PrimaryPermissions2
{
    // No Permissions
    None = 0,
    
    // XIV
    Emote = 1 << 0,
    
    // External Plugins
    GlamourerCustomization = 1 << 1,
    GlamourerEquipment =  1 << 2,
    Mods =  1 << 3,
    BodySwap =  1 << 4,
    Twinning =  1 << 5,
    CustomizePlus =  1 << 6,
    Moodles =  1 << 7,
    Hypnosis =  1 << 8
}