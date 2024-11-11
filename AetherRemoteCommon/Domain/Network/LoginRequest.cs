namespace AetherRemoteCommon.Domain.Network;

public class LoginRequest(string secret, Version version)
{
    public string Secret { get; } = secret;
    public Version Version { get; } = version;
}