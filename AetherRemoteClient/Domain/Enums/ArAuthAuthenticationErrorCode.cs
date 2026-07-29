using AetherRemoteCommon.Network.Enums.ErrorCodes;

namespace AetherRemoteClient.Domain.Enums;

/// <summary>
///     The final result of attempting to validate a secret
/// </summary>
public enum ArAuthAuthenticationErrorCode
{
    /// <inheritdoc cref="GetTokenEc.Uninitialized"/>
    Uninitialized,
    
    /// <inheritdoc cref="GetTokenEc.Unknown"/>
    Unknown,
    
    /// <summary> The client attempted to connect with an incompatible version of the client plugin </summary>
    VersionMismatch,
    
    /// <summary> The client attempted to connect with a secret that did not correspond to a valid or active secret </summary>
    UnknownSecret,
    
    /// <summary> The authentication could not be reached </summary>
    AuthenticationServerUnreachable,
    
    /// <summary> The secret to send to the authentication server was not set </summary>
    SecretNotSetOrInvalid,
    
    /// <summary> The token returned was invalid or malformed in some way </summary>
    InvalidOrMalformedToken,
    
    /// <summary> A value in <see cref="GetTokenEc"/> was not properly mapped to these values </summary>
    UnboundScope
}
