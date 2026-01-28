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
    
    BadData,
    
    Success,
    
    Unknown
}