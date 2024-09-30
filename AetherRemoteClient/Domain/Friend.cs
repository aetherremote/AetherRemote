using AetherRemoteCommon.Domain;

namespace AetherRemoteClient.Domain;

/// <summary>
/// Placeholder for local friend implementation
/// </summary>
public class Friend
{
    public string FriendCode;
    public bool Online;
    public UserPermissions Permissions;
    
    public Friend(string friendCode, bool online = false, UserPermissions permissions = UserPermissions.None)
    {
        FriendCode = friendCode;
        Online = online;
        Permissions = permissions;
    }

    public override string ToString()
    {
        var sb = new AetherRemoteStringBuilder("LocalFriendData");
        sb.AddVariable("FriendCode", FriendCode);
        sb.AddVariable("Online", Online);
        sb.AddVariable("Permissions", Permissions);
        return sb.ToString();
    }
}
