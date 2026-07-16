namespace AetherRemoteCommon.V2.Domain;

public enum ResponseStatus
{
    /// <summary> This status was uninitialized, indicative of a larger bug or problem </summary>
    Uninitialized,
    
    /// <summary> The request was successful </summary>
    Success,
    
    /// <summary> An unknown error occurred </summary>
    Unknown,
    
    /// <summary> Too few targets for the request </summary>
    TooFewTargets,
    
    /// <summary> Too many targets for the request </summary>
    TooManyTargets,
    
    /// <summary> Too many requests made too quickly </summary>
    TooManyRequests,
    
    /// <summary> The data in the request was invalid or malformed </summary>
    BadRequest
}