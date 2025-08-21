using AetherRemoteClient.Domain.Enums;

namespace AetherRemoteClient.Domain;

public class ApplyCharacterTransformationResult(ApplyCharacterTransformationErrorCode success, PermanentTransformationData? data)
{
    public readonly ApplyCharacterTransformationErrorCode Success = success;
    public readonly PermanentTransformationData? Data = data;
}