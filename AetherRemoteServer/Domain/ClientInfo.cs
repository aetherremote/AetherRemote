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
    ///     The local character name associated with this client.
    ///     This can be exploited by modifying the request.
    ///     A regular client will only return the local player, but it is possible a modified client could return another name.
    ///     A fix can be implemented by enforcing all accounts onboard to Aether Remote to do a lodestone registration process.
    /// </summary>
    public string CharacterName = string.Empty;
    
    /// <summary>
    ///     The last time this user submitted a command
    /// </summary>
    public DateTime LastAction = DateTime.Now;
}