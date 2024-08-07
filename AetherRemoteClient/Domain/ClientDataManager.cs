namespace AetherRemoteClient.Domain;

/// <summary>
/// Responsible for managing all local client data such as friend code, friend list, etc.
/// </summary>
public class ClientDataManager
{
    public string? FriendCode = null;

    public FriendList FriendList = new();

    public ClientDataManager() { }
}
