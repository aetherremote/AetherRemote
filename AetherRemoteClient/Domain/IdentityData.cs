namespace AetherRemoteClient.Domain;

public record IdentityAlteration(IdentityAlterationType Type, string Sender);

public enum IdentityAlterationType
{
    None,
    Transformation,
    Twinning,
    BodySwap,
    Unknown
}