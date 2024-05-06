using AetherRemoteCommon.Domain.CommonFriendPermissions;

namespace AetherRemoteCommon.Domain.CommonFriend;

[Serializable]
public class Friend
{
    /// <summary>
    /// Id of the friend (UserId)
    /// </summary>
    public string FriendCode;

    /// <summary>
    /// A name set by the client to identify a friend more easily
    /// </summary>
    /// 
    public string? Note;

    /// <summary>
    /// Friend preferences
    /// </summary>
    public FriendPermissions Permissions;

    /// <summary>
    /// 
    /// </summary>
    [NonSerialized]
    public bool Online;

    public Friend()
    {
        FriendCode = string.Empty;
        Note = null;
        Permissions = new();
        Online = false;
    }

    public Friend(string friendCode, string? note = null, FriendPermissions? preferences = null, bool online = false)
    {
        FriendCode = friendCode;
        Note = note;
        Permissions = preferences ?? new();
        Online = online;
    }

    public override string ToString()
    {
        var sb = new AetherRemoteStringBuilder("Friend");
        sb.AddVariable("FriendCode", FriendCode);
        sb.AddVariable("Note", Note);
        sb.AddVariable("Permissions", Permissions);
        sb.AddVariable("Online", Online);
        return sb.ToString();
    }
}
