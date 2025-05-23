namespace AetherRemoteCommon.Domain.Enums;

/// <summary>
///     Enum for what attributes about a character should be copied during a twinning or body swap action.
/// </summary>
[Flags]
public enum CharacterAttributes
{
    None = 0,
    Mods = 1 << 0,
    Moodles = 1 << 1,
    CustomizePlus = 1 << 2
}