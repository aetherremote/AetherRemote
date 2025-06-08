namespace AetherRemoteCommon.V2.Domain.Network.RemoveFriend;

public enum RemoveFriendEc
{
    /// <summary>
    ///     Default value, never should be encountered
    /// </summary>
    Uninitialized,
    
    /// <summary>
    ///     When the person you are trying to remove is not your friend
    /// </summary>
    NotFriends,
    
    /// <summary>
    ///     An unknown error occurred
    /// </summary>
    Unknown,
    
    /// <summary>
    ///     The operation succeeded
    /// </summary>
    Success
}