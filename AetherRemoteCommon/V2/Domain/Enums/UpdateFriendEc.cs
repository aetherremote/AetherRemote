namespace AetherRemoteCommon.V2.Domain.Enums;

public enum UpdateFriendEc
{
    /// <summary> This status was uninitialized, indicative of a larger bug or problem </summary>
    Uninitialized,
    
    /// <summary> The request was successful </summary>
    Success,
    
    /// <summary> An unknown error occurred </summary>
    Unknown,
    
    /// <summary> No update </summary>
    NoOp
}
