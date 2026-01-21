namespace AetherRemoteCommon.Domain.Network.Possession;

public enum PossessionResponseEc
{
    Uninitialized,
    
    TargetOffline,
    TargetNotFriends,
    LacksPermissions,
    Timeout,
    TooManyRequests,
    BadDataInRequest,
    
    SenderAlreadyInSession,
    SenderNotInSession,
    SenderNotGhost,
    
    TargetAlreadyInSession,
    TargetNotInSession,
    TargetInSessionButNotOnline,
    
    SessionMismatch,
    
    Success,
    Unknown
}