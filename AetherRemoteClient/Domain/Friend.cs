using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Permissions;

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
        UserPermissions? permissionsGrantedToFriend = null, 
        UserPermissions? permissionsGrantedByFriend = null)
    {
        FriendCode = friendCode;
        Online = online;
        PermissionsGrantedToFriend = permissionsGrantedToFriend ?? new UserPermissions();
        PermissionsGrantedByFriend = permissionsGrantedByFriend ?? new UserPermissions();
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
