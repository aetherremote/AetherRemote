namespace AetherRemoteServer.Domain.Schema.Primary;

[Flags]
public enum PrimaryPermissionsSchemaV1
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

    // Transformation Permissions
    Customization = 1 << 11,
    Equipment = 1 << 12,
    Mods = 1 << 13,
    BodySwap = 1 << 14,
    Twinning = 1 << 15
}