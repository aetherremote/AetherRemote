namespace AetherRemoteCommon.Domain.Network;

/// <summary>
/// Request to log into the server
/// </summary>
public record FetchTokenRequest
{
    public string Secret { get; init; } = string.Empty;
    public Version Version { get; init; } = new();
}