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
    ///     The friend code already exists
    /// </summary>
    FriendCodeAlreadyExists,
    
    /// <summary>
    ///     Already friends with a target
    /// </summary>
    AlreadyFriends,
    
    /// <summary>
    ///     Successfully added a user, but awaiting them to add you back
    /// </summary>
    Pending,
    
    /// <summary>
    ///     Unknown issue occurred
    /// </summary>
    Unknown,
    
    /// <summary>
    ///     Action was successful
    /// </summary>
    Success
}