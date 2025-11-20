using AetherRemoteCommon.Domain;

namespace AetherRemoteClient.Domain;

/// <summary>
///     Represents a friend you have granted permissions to
/// </summary>
public class Friend(
    string friendCode,
    bool online,
    string? note = null,
    UserPermissions? permissionsGrantedToFriend = null,
    UserPermissions? permissionsGrantedByFriend = null)
{
    /// <summary>
    ///     Unique identifier representing a friend
    /// </summary>
    public readonly string FriendCode = friendCode;
    
    /// <summary>
    ///     A note to help more easily identify a friend
    /// </summary>
    public string? Note = note;
    
    /// <summary>
    ///     If a friend is online or not
    /// </summary>
    public bool Online = online;
    
    /// <summary>
    ///     The permissions you have granted to a friend
    /// </summary>
    public UserPermissions PermissionsGrantedToFriend = permissionsGrantedToFriend ?? new UserPermissions();
    
    /// <summary>
    ///     The permissions a friend has granted you
    /// </summary>
    public UserPermissions PermissionsGrantedByFriend = permissionsGrantedByFriend ?? new UserPermissions();

    /// <summary>
    ///     The last time a command was sent to this user
    /// </summary>
    public long LastInteractedWith = 0;
    
    /// <summary>
    ///     Gets the note if available, otherwise the friend code
    /// </summary>
    public string NoteOrFriendCode => Note ?? FriendCode;
}