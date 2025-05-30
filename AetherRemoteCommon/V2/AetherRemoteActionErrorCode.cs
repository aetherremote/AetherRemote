namespace AetherRemoteCommon.V2;

public enum AetherRemoteActionErrorCode
{
    Uninitialized,
    
    TargetOffline,
    TargetNotFriends,
    TargetHasNotGrantedSenderPermissions,
    
    Success,
    
    Unknown
}