using AetherRemoteCommon.Domain.Enums;

namespace AetherRemoteCommon.V2.Network.Api;

public record GetTokenResponse(
    LoginAuthenticationErrorCode ErrorCode,
    string? Secret
);
