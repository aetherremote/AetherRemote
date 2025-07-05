namespace AetherRemoteCommon.Domain.Network.AddFriend;

public enum AddFriendEc
{
    /// <summary>
    ///     Default value, never should be encountered
    /// </summary>
    Uninitialized,
    
    /// <summary>
    ///     Attempted to add a friend who does not exist
    /// </summary>
    NoSuchFriendCode,
    
    /// <summary>
    ///     Attempted to add a friend who already is a friend
    /// </summary>
    AlreadyFriends,
    
    /// <summary>
    ///     An unknown error occurred
    /// </summary>
    Unknown,
    
    /// <summary>
    ///     The operation succeeded
    /// </summary>
    Success
}