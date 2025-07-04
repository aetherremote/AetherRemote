namespace AetherRemoteCommon.Domain.Enums;

/// <summary>
///     Represents a result from an action command
/// </summary>
public enum ActionResponseEc
{
    /// <summary>
    ///     Default value, never should be encountered
    /// </summary>
    Uninitialized,
    
    /// <summary>
    ///     Requests are being sent too frequently
    /// </summary>
    TooManyRequests,
    
    /// <summary>
    ///     There are too many targets for this operation. The max operation count can be found in the 
    ///     <see cref="Constraints"/> class.
    /// </summary>
    TooManyTargets,
    
    /// <summary>
    ///     There are too few targets for this operation.
    /// </summary>
    TooFewTargets,
    
    /// <summary>
    ///     There are targets that are offline
    /// </summary>
    TargetOffline,
    
    /// <summary>
    ///     When the sender did not provide valid data in a request
    /// </summary>
    BadDataInRequest,
    
    /// <summary>
    ///     An unknown error occurred
    /// </summary>
    Unknown,
    
    /// <summary>
    ///     The operation succeeded
    /// </summary>
    Success
}