namespace AetherRemoteCommon.Network.Enums.ErrorCodes;

public enum RemoveFriendEc
{
    /// <summary> This status was uninitialized, indicative of a larger bug or problem </summary>
    Uninitialized,
    
    /// <summary> The request was successful </summary>
    Success,
    
    /// <summary> An unknown error occurred </summary>
    Unknown,
    
    /// <summary> That target was not friends with you </summary>
    NotFriends,
    
    /// <summary> The submitted friend code doesn't exist </summary>
    NoSuchFriendCode,
    
    /// <summary> Too many requests made too quickly </summary>
    TooManyRequests,
    
    /// <summary> The data in the request was invalid or malformed </summary>
    BadRequest
}
