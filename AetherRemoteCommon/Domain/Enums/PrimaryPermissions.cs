namespace AetherRemoteCommon.Domain.Enums;

/// <summary>
/// Permissions controlling which aspects of the plugin can be interacted with
/// </summary>
[Flags]
public enum PrimaryPermissions
{
    // No Permissions
    None = 0,

    // General Permissions
    Speak = 1 << 0,
    Emote = 1 << 1,
    
    // Channel Permissions
    Say = 1 << 2,
    Yell = 1 << 3,
    Shout = 1 << 4,
    Tell = 1 << 5,
    Party = 1 << 6,
    Alliance = 1 << 7,
    FreeCompany = 1 << 8,
    PvPTeam = 1 << 9,
    Echo = 1 << 10,
    ChatEmote = 1 << 16,

    // Transformation Permissions
    Customization = 1 << 11,
    Equipment = 1 << 12,
    Mods = 1 << 13,
    BodySwap = 1 << 14,
    Twinning = 1 << 15,
    
    // Misc
    Moodles = 1 << 17
}