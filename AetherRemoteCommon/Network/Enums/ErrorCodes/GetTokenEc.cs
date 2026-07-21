namespace AetherRemoteCommon.Network.Enums.ErrorCodes;

public enum GetTokenEc
{
    // =========== WARNING ============
    // If you update or change any 
    // of these values make sure you 
    // also update the client extension 
    // method as well.
    // =========== WARNING ============
    
    /// <summary> This status was uninitialized, indicative of a larger bug or problem </summary>
    Uninitialized,
    
    /// <summary> The request was successful </summary>
    Success,
    
    /// <summary> An unknown error occurred </summary>
    Unknown,
    
    /// <summary>The version the client attempted to validate against is likely outdated</summary>
    VersionMismatch,
    
    /// <summary>The secret the client attempted to validate against doesn't exist</summary>
    UnknownSecret
}
