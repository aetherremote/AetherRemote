namespace AetherRemoteCommon.Domain.Network.GetToken;

/// <summary>
/// Request to log into the server
/// </summary>
public record GetTokenRequest
{
    public string Secret { get; init; } = string.Empty;
    public Version Version { get; init; } = new();

    public GetTokenRequest()
    {
    }

    public GetTokenRequest(string secret, Version version)
    {
        Secret = secret;
        Version = version;
    }
}