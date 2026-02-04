namespace AetherRemoteServer.Domain;

public class Session(string ghostFriendCode, string hostFriendCode)
{
    public string GhostFriendCode = ghostFriendCode;
    public string HostFriendCode = hostFriendCode;
}