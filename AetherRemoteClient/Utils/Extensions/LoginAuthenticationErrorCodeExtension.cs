using AetherRemoteClient.Domain.Enums;
using AetherRemoteCommon.Domain.Enums;

namespace AetherRemoteClient.Utils.Extensions;

public static class LoginAuthenticationErrorCodeExtension
{
    extension(LoginAuthenticationErrorCode errorCode)
    {
        /// <summary> Converts the <see cref="LoginAuthenticationErrorCode"/> to domain <see cref="ArAuthAuthenticationErrorCode"/> </summary>
        public ArAuthAuthenticationErrorCode ToArAuthAuthenticationErrorCode()
        {
            var translated = errorCode switch
            {
                LoginAuthenticationErrorCode.Uninitialized => ArAuthAuthenticationErrorCode.Uninitialized,
                LoginAuthenticationErrorCode.Success => ArAuthAuthenticationErrorCode.Success,
                LoginAuthenticationErrorCode.VersionMismatch => ArAuthAuthenticationErrorCode.VersionMismatch,
                LoginAuthenticationErrorCode.UnknownSecret => ArAuthAuthenticationErrorCode.UnknownSecret,
                LoginAuthenticationErrorCode.Unknown => ArAuthAuthenticationErrorCode.Unknown,
                _ => ArAuthAuthenticationErrorCode.UnboundScope
            };
            
            if (translated is ArAuthAuthenticationErrorCode.UnboundScope)
                Plugin.Log.Warning($"[LoginAuthenticationErrorCodeExtension] Unbound Error Code: {errorCode}");

            return translated;
        }
    }
}