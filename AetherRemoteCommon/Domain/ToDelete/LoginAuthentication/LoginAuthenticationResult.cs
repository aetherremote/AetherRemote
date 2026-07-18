using AetherRemoteCommon.Domain.Enums;

namespace AetherRemoteCommon.Domain.Network.LoginAuthentication;

public record LoginAuthenticationResult(
    LoginAuthenticationErrorCode ErrorCode,
    string? Secret
);