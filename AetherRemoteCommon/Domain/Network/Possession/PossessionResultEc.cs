namespace AetherRemoteCommon.Domain.Network.Possession;

public enum PossessionResultEc
{
    Uninitialized,
    
    NotFriends,
    
    SafeMode,
    
    Paused,
    
    FeaturePaused,
    
    LackingPermissions,
    
    AlreadyBeingPossessedOrPossessing,
    
    /// <summary>
    ///     When the target of a possession command reports that they are no longer possessed (desync)
    /// </summary>
    PossessionDesynchronization,
    
    BadData,
    
    Success,
    
    Unknown
}