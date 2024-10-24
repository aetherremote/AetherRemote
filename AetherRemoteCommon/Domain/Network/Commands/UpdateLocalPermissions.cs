namespace AetherRemoteCommon.Domain.Network.Commands;

public struct UpdateLocalPermissionsCommand
{
    public string FriendCode { get; set; }
    public UserPermissions PermissionsGrantedToUser { get; set; }

    public UpdateLocalPermissionsCommand(string friendCode, UserPermissions permissionsGrantedToUser)
    {
        FriendCode = friendCode;
        PermissionsGrantedToUser = permissionsGrantedToUser;
    }

    public override readonly string ToString()
    {
        var sb = new AetherRemoteStringBuilder("UpdateLocalPermissionsCommand");
        sb.AddVariable("FriendCode", FriendCode);
        sb.AddVariable("PermissionsGrantedToUser", PermissionsGrantedToUser);
        return sb.ToString();
    }
}
