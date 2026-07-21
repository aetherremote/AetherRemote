namespace AetherRemoteCommon.Network.Domain.Api;

public record GetTokenRequest(
    string Secret,
    Version Version
);
