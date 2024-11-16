using AetherRemoteCommon.Domain.Permissions.V2;

namespace AetherRemoteCommon.Domain.Network.Commands;

public struct UpdateOnlineStatusCommand
{
    public string FriendCode { get; set; }
    public bool Online { get; set; }
    public UserPermissionsV2 Permissions { get; set; }

    public UpdateOnlineStatusCommand(string friendCode, bool online, UserPermissionsV2 permissions)
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
