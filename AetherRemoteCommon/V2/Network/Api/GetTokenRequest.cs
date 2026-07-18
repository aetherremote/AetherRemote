namespace AetherRemoteCommon.V2.Network.Api;

public record GetTokenRequest(
    string Secret,
    Version Version
);
