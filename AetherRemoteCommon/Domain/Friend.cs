using AetherRemoteCommon.Domain.CommonFriendPermissions;

namespace AetherRemoteCommon.Domain.CommonFriend;

[Serializable]
public class Friend
{
    /// <summary>
    /// FriendCode, which represents a unique Id to identify each user.
    /// </summary>
    public string FriendCode { get; set; }

    /// <summary>
    /// A note or nickname to more easily identify a user.
    /// </summary>
    public string? Note { get; set; }

    /// <summary>
    /// Permissions for what a friend may or may do.
    /// </summary>
    public FriendPermissions Permissions { get; set; }

    /// <summary>
    /// Return the note if a friend has one, otherwise their friend code.
    /// </summary>
    public string NoteOrFriendCode => Note ?? FriendCode;

    /// <summary>
    /// Online status of a friend
    /// </summary>
    public bool Online { get; set; } = false;

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

    /// <summary>
    /// Creates a deep copy of <see cref="Friend"/>
    /// </summary>
    /// <returns></returns>
    public Friend Copy()
    {
        var permissions = new FriendPermissions();
        permissions.Speak = Permissions.Speak;
        permissions.Emote = Permissions.Emote;
        permissions.ChangeAppearance = Permissions.ChangeAppearance;
        permissions.ChangeEquipment = Permissions.ChangeEquipment;
        permissions.Say = Permissions.Say;
        permissions.Yell = Permissions.Yell;
        permissions.Shout = Permissions.Shout;
        permissions.Tell = Permissions.Tell;
        permissions.Party = Permissions.Party;
        permissions.Alliance = Permissions.Alliance;
        permissions.FreeCompany = Permissions.FreeCompany;
        permissions.Linkshell = Permissions.Linkshell;
        permissions.CrossworldLinkshell = Permissions.CrossworldLinkshell;
        permissions.PvPTeam = Permissions.PvPTeam;

        var copy = new Friend();
        copy.FriendCode = FriendCode;
        copy.Note = Note;
        copy.Permissions = permissions;

        return copy;
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
