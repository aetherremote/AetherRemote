namespace AetherRemoteCommon.Domain.Enums.Permissions;

[Flags]
public enum PrimaryPermissions
{
    // No Permissions
    None = 0,
    
    // XIV
    Emote = 1 << 0,
    
    // External Plugins
    Glamourer = 1 << 1,
    Mods = 1 << 2,
    BodySwap = 1 << 3,
    Twinning = 1 << 4,
    CustomizePlus = 1 << 5,
    Moodles = 1 << 6,
    Hypnosis = 1 << 7,
    Honorific = 1 << 8
}