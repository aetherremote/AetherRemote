using AetherRemoteCommon.Domain;

namespace AetherRemoteClient.Domain;

/// <summary>
/// Container for all the information that makes up a friend
/// </summary>
public class Friend
{
    public readonly string FriendCode;
    public bool Online;
    public UserPermissions PermissionsGrantedToFriend;
    public UserPermissions PermissionsGrantedByFriend;
    
    /// <summary>
    /// <inheritdoc cref="Friend"/>
    /// </summary>
    public Friend(
        string friendCode, 
        bool online = false, 
        UserPermissions permissionsGrantedToFriend = UserPermissions.None,
        UserPermissions permissionsGrantedByFriend = UserPermissions.None)
    {
        FriendCode = friendCode;
        Online = online;
        PermissionsGrantedToFriend = permissionsGrantedToFriend;
        PermissionsGrantedByFriend = permissionsGrantedByFriend;
    }

    public override string ToString()
    {
        var sb = new AetherRemoteStringBuilder("LocalFriendData");
        sb.AddVariable("FriendCode", FriendCode);
        sb.AddVariable("Online", Online);
        sb.AddVariable("PermissionsGrantedToFriend", PermissionsGrantedToFriend);
        sb.AddVariable("PermissionsGrantedByFriend", PermissionsGrantedByFriend);
        return sb.ToString();
    }
}
