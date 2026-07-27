namespace AetherRemoteCommon.Network.Enums.ErrorCodes;

public enum GetAccountDataEc
{
    /// <summary> This status was uninitialized, indicative of a larger bug or problem </summary>
    Uninitialized,
    
    /// <summary> The request was successful </summary>
    Success,
    
    /// <summary> An unknown error occurred </summary>
    Unknown,
    
    /// <summary> Already logged in </summary>
    AlreadyLoggedIn
}
