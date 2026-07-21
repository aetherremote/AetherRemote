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
    TargetOffline,
    
    /// <summary> The target was not a friend of the sender </summary>
    TargetNotFriends,
    
    /// <summary> The target did not grant the sender adequate permissions </summary>
    TargetHasNotGrantedPermissions
}
