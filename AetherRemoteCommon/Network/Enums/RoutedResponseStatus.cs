namespace AetherRemoteCommon.Network.Enums;

public enum RoutedResponseStatus
{
    /// <summary> This status was uninitialized, indicative of a larger bug or problem </summary>
    Uninitialized,
    
    /// <summary> The request was successful </summary>
    Success,
    
    /// <summary> An unknown error occurred </summary>
    Unknown,
    
    /// <summary> The request timed out </summary>
    Timeout,
    
    /// <summary> One of the targets was offline </summary>
    Offline,
    
    /// <summary> The target was not a friend of the sender </summary>
    NotFriends,
    
    /// <summary> The target did not grant the sender adequate permissions </summary>
    LackingPermissions,
    
    /// <summary> The target was in safe mode </summary>
    SafeMode,
    
    /// <summary> The target has the sender or the operation paused </summary>
    Paused,
    
    /// <summary> The data in the request was invalid or malformed </summary>
    BadRequest,
    
    /// <summary> The target encountered an unexpected error running the operation </summary>
    RuntimeError,
    
    /// <summary> The target is being hypnotized by another user </summary>
    BeingHypnotized
}
