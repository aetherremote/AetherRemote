namespace AetherRemoteCommon.Domain.Enums;

/// <summary>
///     Represents the result of a database command
/// </summary>
public enum DatabaseResultEc
{
    /// <summary>
    ///     Default value, never should be encountered
    /// </summary>
    Uninitialized,
    
    /// <summary>
    ///     No operation was performed (such as no rows updated)
    /// </summary>
    NoOp,
    
    /// <summary>
    ///     No such person exists
    /// </summary>
    NoSuchFriendCode,
    
    /// <summary>
    ///     Already friends with a target
    /// </summary>
    AlreadyFriends,
    
    /// <summary>
    ///     Unknown issue occurred
    /// </summary>
    Unknown,
    
    /// <summary>
    ///     Action was successful
    /// </summary>
    Success
}