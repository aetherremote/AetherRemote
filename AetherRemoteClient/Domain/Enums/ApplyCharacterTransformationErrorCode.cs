namespace AetherRemoteClient.Domain.Enums;

public enum ApplyCharacterTransformationErrorCode
{
    Uninitialized,
    Success,
    FailedToClearExistingMods,
    FailedToFindCharacter,
    FailedToStoreAttributes,
    FailedToApplyAttributes,
    Unknown
}