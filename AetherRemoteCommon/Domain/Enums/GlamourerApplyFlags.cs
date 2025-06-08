namespace AetherRemoteCommon.Domain.Enums;

/// <summary>
/// Controls how glamourer will apply data to a character
/// </summary>
[Flags]
public enum GlamourerApplyFlags : ulong
{
    Once = 1uL,
    Equipment = 2uL,
    Customization = 4uL,
    All = Once | Equipment | Customization
}
