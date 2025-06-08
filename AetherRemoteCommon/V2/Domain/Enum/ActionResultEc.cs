namespace AetherRemoteCommon.V2.Domain.Enum;

public enum ActionResultEc
{
    Uninitialized,
    
    // Client Side
    ClientNotFriends,
    ClientInSafeMode,
    ClientHasOverride,
    ClientHasNotGrantedSenderPermissions,
    ClientBadData,
    ClientPluginDependency,
    
    // Server -> Client
    TargetOffline,
    TargetNotFriends,
    TargetHasNotGrantedSenderPermissions,
    TargetTimeout,
    
    Success,
    
    ValueNotSet,
    
    Unknown
}