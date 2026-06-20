using AetherRemoteCommon.Domain.Enums;

namespace AetherRemoteClient.Domain.Enums;

/// <summary>
///     The final result of attempting to validate a secret
/// </summary>
public enum ArAuthAuthenticationErrorCode
{
    /// <summary> <inheritdoc cref="LoginAuthenticationErrorCode.Uninitialized"/> </summary>
    Uninitialized,
    
    /// <summary> <inheritdoc cref="LoginAuthenticationErrorCode.Success"/> </summary>
    Success,
    
    /// <summary> <inheritdoc cref="LoginAuthenticationErrorCode.VersionMismatch"/> </summary>
    VersionMismatch,
    
    /// <summary> <inheritdoc cref="LoginAuthenticationErrorCode.UnknownSecret"/> </summary>
    UnknownSecret,
    
    /// <summary> The authentication could not be reached </summary>
    AuthenticationServerUnreachable,
    
    /// <summary> The secret to send to the authentication server was not set </summary>
    SecretNotSetOrInvalid,
    
    /// <summary> The token returned was invalid or malformed in some way </summary>
    InvalidOrMalformedToken,
    
    /// <summary> <inheritdoc cref="LoginAuthenticationErrorCode.Unknown"/> </summary>
    Unknown,
    
    /// <summary> A value in <inheritdoc cref="LoginAuthenticationErrorCode"/> was not properly mapped to <inheritdoc cref="ArAuthAuthenticationErrorCode"/> </summary>
    UnboundScope
}