namespace AetherRemoteCommon.V2.Domain;

public enum RoutedResponseStatus
{
    Uninitialized,
    Success,
    Unknown,
    Timeout,
    NotFriends,
    NotGrantedPermissions,
    NotOnline
}
