using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.CommonFriendPermissions;
using System;

using CommonFriend = AetherRemoteCommon.Domain.CommonFriend.Friend;

namespace AetherRemoteClient.Domain;

[Serializable]
public class Friend
{
    /// <summary> 
    /// Id of the friend (UserId)
    /// </summary>
    public string FriendCode { get; set; }

    /// <summary>
    /// A name set by the client to identify a friend more easily
    /// </summary>
    public string? Note { get; set; }

    /// <summary>
    /// Friend preferences
    /// </summary>
    public FriendPermissions Permissions { get; set; }

    /// <summary>
    /// Returns a friend's given note, or their id
    /// </summary>
    public string NoteOrFriendCode => Note ?? FriendCode;

    /// <summary>
    /// Is this friend currently connected to the server
    /// </summary>
    [NonSerialized]
    public bool Online = false;

    public Friend(string friendCode, string? note = null, FriendPermissions? permissions = null)
    {
        FriendCode = friendCode;
        Note = note;
        Permissions = permissions ?? new();
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

    /// <summary>
    /// Converts Friend to CommonFriend
    /// </summary>
    /// <returns></returns>
    public CommonFriend Convert()
    {
        return new(FriendCode, Note, Permissions);
    }
}
