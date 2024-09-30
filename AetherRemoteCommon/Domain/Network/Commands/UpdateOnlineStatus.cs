namespace AetherRemoteCommon.Domain.Network.Commands;

public struct UpdateOnlineStatusCommand
{
    public string FriendCode { get; set; }
    public bool Online { get; set; }

    public UpdateOnlineStatusCommand(string friendCode, bool online)
    {
        FriendCode = friendCode;
        Online = online;
    }

    public override readonly string ToString()
    {
        var sb = new AetherRemoteStringBuilder("UpdateOnlineStatusCommand");
        sb.AddVariable("FriendCode", FriendCode);
        sb.AddVariable("Online", Online);
        return sb.ToString();
    }
}
