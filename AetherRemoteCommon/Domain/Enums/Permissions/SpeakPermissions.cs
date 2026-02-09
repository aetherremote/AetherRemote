namespace AetherRemoteCommon.Domain.Enums.Permissions;

[Flags]
public enum SpeakPermissions
{
    // No Permissions
    None = 0,
    
    // General Channels
    Say = 1 << 0,
    Yell = 1 << 1,
    Shout = 1 << 2,
    Tell = 1 << 3,
    Party = 1 << 4,
    Alliance = 1 << 5,
    FreeCompany = 1 << 6,
    PvPTeam = 1 << 7,
    Echo = 1 << 8,
    Roleplay = 1 << 9,
    
    // Linkshells
    Ls1 = 1 << 10,
    Ls2 = 1 << 11,
    Ls3 = 1 << 12,
    Ls4 = 1 << 13,
    Ls5 = 1 << 14,
    Ls6 = 1 << 15,
    Ls7 = 1 << 16,
    Ls8 = 1 << 17,
    
    // Cross-world Linkshells
    Cwl1 = 1 << 18,
    Cwl2 = 1 << 19,
    Cwl3 = 1 << 20,
    Cwl4 = 1 << 21,
    Cwl5 = 1 << 22,
    Cwl6 = 1 << 23,
    Cwl7 = 1 << 24,
    Cwl8 = 1 << 25
}