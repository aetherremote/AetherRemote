namespace AetherRemoteServer.Domain;

/// <summary>
///     Represents a client connected to the server and the information required for them to issue commands
///     and for the server to issue commands to them.
/// </summary>
public class ClientInfo(string connectionId)
{
    /// <summary>
    ///     Signal R connection id granted to this client
    /// </summary>
    public readonly string ConnectionId = connectionId;
    
    /// <summary>
    ///     The last time this user submitted a command
    /// </summary>
    public DateTime LastAction = DateTime.Now;
}