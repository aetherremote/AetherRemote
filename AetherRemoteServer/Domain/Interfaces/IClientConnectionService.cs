namespace AetherRemoteServer.Domain.Interfaces;

/// <summary>
///     TODO
/// </summary>
public interface IClientConnectionService
{
    /// <summary>
    ///     Adds a client to the list of connected clients
    /// </summary>
    public void TryAddClient(string friendCode, ClientInfo info);

    /// <summary>
    ///     Removes a client to the list of connected clients
    /// </summary>
    public void TryRemoveClient(string friendCode);
    
    /// <summary>
    ///     Retrieves a <see cref="ClientInfo"/> corresponding to friend code
    /// </summary>
    public ClientInfo? TryGetClient(string friendCode);
    
    /// <summary>
    ///     Checks if a friend code is sending commands too frequently
    /// </summary>
    public bool IsUserExceedingRequestLimit(string friendCode);
}