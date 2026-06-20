namespace AetherRemoteCommon.Domain.Enums;

/// <summary>
///     The result of attempting to validate a secret against the authentication endpoint
/// </summary>
public enum LoginAuthenticationErrorCode
{
    // =========== WARNING ============
    // If you update or change any 
    // of these values make sure you 
    // also update the client extension 
    // method as well.
    // =========== WARNING ============
    
    /// <summary>This value was never set, and likely indicates an error in logic</summary>
    Uninitialized,
    
    /// <summary>Authentication was successful</summary>
    Success,
    
    /// <summary>The version the client attempted to validate against is likely outdated</summary>
    VersionMismatch,
    
    /// <summary>The secret the client attempted to validate against doesn't exist</summary>
    UnknownSecret,
    
    /// <summary>An unknown error occurred</summary>
    Unknown
    
    // =========== WARNING ============
    // If you update or change any 
    // of these values make sure you 
    // also update the client extension 
    // method as well.
    // =========== WARNING ============
}