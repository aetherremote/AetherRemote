namespace AetherRemoteCommon.V2.Domain.Network.UpdateFriend;

public enum UpdateFriendEc
{
    /// <summary>
    ///     Default value, never should be encountered
    /// </summary>
    Uninitialized,
    
    /// <summary>
    ///     No update was performed
    /// </summary>
    NoOp,
    
    /// <summary>
    ///     An unknown error occurred
    /// </summary>
    Unknown,
    
    /// <summary>
    ///     The operation succeeded
    /// </summary>
    Success
}