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
    
    /// <summary>
    ///     When the plugin fails to find the MoveMode configuration value
    /// </summary>
    FailedToReadCharacterConfiguration,
    
    /// <summary>
    ///     An agreement has not been accepted
    /// </summary>
    HasNotAcceptedAgreement,
    
    BadData,
    
    Success,
    
    Unknown
}