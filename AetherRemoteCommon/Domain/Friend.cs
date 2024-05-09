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
    [NonSerialized]
    public bool Online = false;

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
        permissions.AllowSpeak = Permissions.AllowSpeak;
        permissions.AllowEmote = Permissions.AllowEmote;
        permissions.AllowChangeAppearance = Permissions.AllowChangeAppearance;
        permissions.AllowChangeEquipment = Permissions.AllowChangeEquipment;
        permissions.AllowSay = Permissions.AllowSay;
        permissions.AllowYell = Permissions.AllowYell;
        permissions.AllowShout = Permissions.AllowShout;
        permissions.AllowTell = Permissions.AllowTell;
        permissions.AllowParty = Permissions.AllowParty;
        permissions.AllowAlliance = Permissions.AllowAlliance;
        permissions.AllowFreeCompany = Permissions.AllowFreeCompany;
        permissions.AllowLinkshell = Permissions.AllowLinkshell;
        permissions.AllowCrossworldLinkshell = Permissions.AllowCrossworldLinkshell;
        permissions.AllowPvPTeam = Permissions.AllowPvPTeam;

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
