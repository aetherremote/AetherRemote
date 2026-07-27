using AetherRemoteCommon.Network.Enums.ErrorCodes;

namespace AetherRemoteCommon.Network.Domain.Api;

public record GetTokenResponse(
    GetTokenEc Result,
    string? Secret
);
