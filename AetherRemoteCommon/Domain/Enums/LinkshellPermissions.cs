namespace AetherRemoteCommon.Domain.Enums;

/// <summary>
/// Permissions controlling which linkshells can be talked in
/// </summary>
[Flags]
public enum LinkshellPermissions
{
    // No Permissions
    None = 0,
    
    // Linkshell Permissions
    Ls1 = 1 << 0,
    Ls2 = 1 << 1,
    Ls3 = 1 << 2,
    Ls4 = 1 << 3,
    Ls5 = 1 << 4,
    Ls6 = 1 << 5,
    Ls7 = 1 << 6,
    Ls8 = 1 << 7,

    // Cross-world Linkshell
    Cwl1 = 1 << 8,
    Cwl2 = 1 << 9,
    Cwl3 = 1 << 10,
    Cwl4 = 1 << 11,
    Cwl5 = 1 << 12,
    Cwl6 = 1 << 13,
    Cwl7 = 1 << 14,
    Cwl8 = 1 << 15
}