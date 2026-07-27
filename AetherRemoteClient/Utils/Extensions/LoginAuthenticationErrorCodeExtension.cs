using AetherRemoteClient.Domain.Enums;
using AetherRemoteCommon.Network.Enums.ErrorCodes;

namespace AetherRemoteClient.Utils.Extensions;

public static class LoginAuthenticationErrorCodeExtension
{
    extension(GetTokenEc errorCode)
    {
        public ArAuthAuthenticationErrorCode ToArAuthAuthenticationErrorCode()
        {
            var translated = errorCode switch
            {
                GetTokenEc.Uninitialized => ArAuthAuthenticationErrorCode.Uninitialized,
                GetTokenEc.VersionMismatch => ArAuthAuthenticationErrorCode.VersionMismatch,
                GetTokenEc.UnknownSecret => ArAuthAuthenticationErrorCode.UnknownSecret,
                GetTokenEc.Unknown => ArAuthAuthenticationErrorCode.Unknown,
                _ => ArAuthAuthenticationErrorCode.UnboundScope
            };
            
            if (translated is ArAuthAuthenticationErrorCode.UnboundScope)
                Plugin.Log.Warning($"[LoginAuthenticationErrorCodeExtension] Unbound Error Code: {errorCode}");

            return translated;
        }
    }
}