using AetherRemoteCommon.Domain.Permissions.V2;

namespace AetherRemoteCommon.Domain.Network.Commands;

public struct UpdateLocalPermissionsCommand
{
    public string FriendCode { get; set; }
    public UserPermissionsV2 PermissionsGrantedToUser { get; set; }

    public UpdateLocalPermissionsCommand(string friendCode, UserPermissionsV2 permissionsGrantedToUser)
    {
        FriendCode = friendCode;
        PermissionsGrantedToUser = permissionsGrantedToUser;
    }

    public readonly override string ToString()
    {
        var sb = new AetherRemoteStringBuilder("UpdateLocalPermissionsCommand");
        sb.AddVariable("FriendCode", FriendCode);
        sb.AddVariable("PermissionsGrantedToUser", PermissionsGrantedToUser);
        return sb.ToString();
    }
}
