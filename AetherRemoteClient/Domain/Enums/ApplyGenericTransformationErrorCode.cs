namespace AetherRemoteClient.Domain.Enums;

public enum ApplyGenericTransformationErrorCode
{
    Uninitialized,
    Success,
    FailedBase64Conversion,
    FailedToGetDesign,
    FailedToRemoveAdvancedDyes,
    FailedToApplyDesign,
    Unknown
}