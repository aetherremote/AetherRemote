namespace AetherRemoteCommon.Domain.Network.GetAccountData;

public enum GetAccountDataEc
{
    /// <summary>
    ///     Default value, never should be encountered
    /// </summary>
    Uninitialized,
    
    /// <summary>
    ///     Sender is not online... Somehow...
    /// </summary>
    NotOnline,
    
    /// <summary>
    ///     The operation succeeded
    /// </summary>
    Success
}