namespace AetherRemoteCommon.V2.Domain.Enum;

public enum ActionResultEc
{
    Uninitialized,
    
    // Client Side
    ClientNotFriends,
    ClientInSafeMode,
    ClientHasFeaturePaused,
    ClientHasSenderPaused,
    ClientHasNotGrantedSenderPermissions,
    ClientBadData,
    ClientPluginDependency,
    ClientBeingHypnotized,
    
    // Server -> Client
    TargetOffline,
    TargetNotFriends,
    TargetHasNotGrantedSenderPermissions,
    TargetTimeout,
    
    // Success
    Success,
    
    // Should never happen
    ValueNotSet,
    
    // Unknown & Exception
    Unknown
}