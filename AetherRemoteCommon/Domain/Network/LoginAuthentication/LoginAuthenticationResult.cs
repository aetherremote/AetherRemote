using AetherRemoteCommon.Domain.Enums;

namespace AetherRemoteCommon.Domain.Network.LoginAuthentication;

public class LoginAuthenticationResult
{
    public LoginAuthenticationErrorCode ErrorCode { get; set; }
    public string? Secret { get; set; }

    public LoginAuthenticationResult()
    {
    }
    
    public LoginAuthenticationResult(LoginAuthenticationErrorCode errorCode, string? secret = null)
    {
        ErrorCode = errorCode;
        Secret = secret;
    }
}