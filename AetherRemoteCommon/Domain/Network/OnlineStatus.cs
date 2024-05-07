namespace AetherRemoteCommon.Domain.Network;

public struct OnlineStatusExecute
{
    public string FriendCode { get; set; }
    public bool Online { get; set; }

    public OnlineStatusExecute(string friendCode, bool online)
    {
        FriendCode = friendCode;
        Online = online;
    }
}
