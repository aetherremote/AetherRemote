namespace AetherRemoteCommon.Domain;

[Flags]
public enum UserPermissions
{
    // No Permissions
    None = 0,

    // General Permissions
    Speak = 1 << 0,
    Emote = 1 << 1,

    // Glamourer Permissions
    Customization = 1 << 2,
    Equipment = 1 << 3,

    // Channel Permissions
    Say = 1 << 4,
    Yell = 1 << 5,
    Shout = 1 << 6,
    Tell = 1 << 7,
    Party = 1 << 8,
    Alliance = 1 << 9,
    FreeCompany = 1 << 10,
    PvPTeam = 1 << 11,

    // Linkshell Permissions
    LS1 = 1 << 12,
    LS2 = 1 << 13,
    LS3 = 1 << 14,
    LS4 = 1 << 15,
    LS5 = 1 << 16,
    LS6 = 1 << 17,
    LS7 = 1 << 18,
    LS8 = 1 << 19,

    // Crossworld Linkshell
    CWL1 = 1 << 20,
    CWL2 = 1 << 21,
    CWL3 = 1 << 22,
    CWL4 = 1 << 23,
    CWL5 = 1 << 24,
    CWL6 = 1 << 25,
    CWL7 = 1 << 26,
    CWL8 = 1 << 27
}
