namespace AetherRemoteCommon.Domain.Enums.New;

[Flags]
public enum SpeakPermissions2
{
    // No Permissions
    None = 0,
    
    // General Channels
    Say = 1 << 1,
    Yell = 1 << 2,
    Shout = 1 << 3,
    Tell = 1 << 4,
    Party = 1 << 5,
    Alliance = 1 << 6,
    FreeCompany = 1 << 7,
    PvPTeam = 1 << 8,
    Echo = 1 << 9,
    Roleplay = 1 << 10,
    
    // Linkshells
    Ls1 = 1 << 11,
    Ls2 = 1 << 12,
    Ls3 = 1 << 13,
    Ls4 = 1 << 14,
    Ls5 = 1 << 15,
    Ls6 = 1 << 16,
    Ls7 = 1 << 17,
    Ls8 = 1 << 18,
    
    // Cross-world Linkshells
    Cwl1 = 1 << 19,
    Cwl2 = 1 << 20,
    Cwl3 = 1 << 21,
    Cwl4 = 1 << 22,
    Cwl5 = 1 << 23,
    Cwl6 = 1 << 24,
    Cwl7 = 1 << 25,
    Cwl8 = 1 << 27
}