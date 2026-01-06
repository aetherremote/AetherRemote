namespace AetherRemoteServer.Domain;

public enum AddFriendResult
{
    Uninitialized, // Not initialized over the wire
    Unknown, // Exception 
    NoSuchAccountId,
    AlreadyFriends, // No-Op
    Pending, // Success, they haven't added back
    Success // Success, they added back
}