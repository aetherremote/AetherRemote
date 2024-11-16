using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Permissions.V2;

namespace AetherRemoteClient.Domain;

/// <summary>
/// Container for all the information that makes up a friend
/// </summary>
public class Friend
{
    public readonly string FriendCode;
    public bool Online;
    public UserPermissionsV2 PermissionsGrantedToFriend;
    public UserPermissionsV2 PermissionsGrantedByFriend;
    
    /// <summary>
    /// <inheritdoc cref="Friend"/>
    /// </summary>
    public Friend(
        string friendCode, 
        bool online = false, 
        UserPermissionsV2? permissionsGrantedToFriend = null, 
        UserPermissionsV2? permissionsGrantedByFriend = null)
    {
        FriendCode = friendCode;
        Online = online;
        PermissionsGrantedToFriend = permissionsGrantedToFriend ?? new UserPermissionsV2();
        PermissionsGrantedByFriend = permissionsGrantedByFriend ?? new UserPermissionsV2();
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
