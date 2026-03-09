namespace AetherRemoteCommon.Domain.Enums;

/// <summary>
///     Enum for what attributes about a character should be copied during a twinning or body swap action.
/// </summary>
[Flags]
public enum CharacterAttributes
{
    None = 0,
    GlamourerCustomization = 1 << 0,
    GlamourerEquipment = 1 << 1,
    PenumbraMods = 1 << 2,
    Honorific = 1 << 3,
    Moodles = 1 << 4,
    CustomizePlus = 1 << 5,
}