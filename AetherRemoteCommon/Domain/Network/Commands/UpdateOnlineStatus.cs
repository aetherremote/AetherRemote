using AetherRemoteCommon.Domain.Permissions;

namespace AetherRemoteCommon.Domain.Network.Commands;

public struct UpdateOnlineStatusCommand
{
    public string FriendCode { get; set; }
    public bool Online { get; set; }
    public UserPermissions Permissions { get; set; }

    public UpdateOnlineStatusCommand(string friendCode, bool online, UserPermissions permissions)
    {
        FriendCode = friendCode;
        Online = online;
        Permissions = permissions;
    }

    public readonly override string ToString()
    {
        var sb = new AetherRemoteStringBuilder("UpdateOnlineStatusCommand");
        sb.AddVariable("FriendCode", FriendCode);
        sb.AddVariable("Online", Online);
        sb.AddVariable("Permissions", Permissions);
        return sb.ToString();
    }
}
