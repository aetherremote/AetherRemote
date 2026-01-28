using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;

namespace AetherRemoteClient.Domain;

/// <summary>
///     Represents a friend you have granted permissions to
/// </summary>
public class Friend
{
    /// <summary>
    ///     Do not use this constructor, it is a placeholder for possession functions
    /// </summary>
    public Friend()
    {
        FriendCode = string.Empty;
        PermissionsGrantedToFriend = new UserPermissions();
        PermissionsGrantedByFriend = new UserPermissions();
    }

    public Friend(string friendCode, FriendOnlineStatus status, string? note = null, UserPermissions? permissionsGrantedToFriend = null, UserPermissions? permissionsGrantedByFriend = null)
    {
        FriendCode = friendCode;
        Status = status;
        Note = note;
        PermissionsGrantedToFriend = permissionsGrantedToFriend ?? new UserPermissions();
        PermissionsGrantedByFriend = permissionsGrantedByFriend ?? new UserPermissions();
    }

    /// <summary>
    ///     Unique identifier representing a friend
    /// </summary>
    public readonly string FriendCode;
    
    /// <summary>
    ///     A note to help more easily identify a friend
    /// </summary>
    public string? Note;
    
    /// <summary>
    ///     If a friend is online or not
    /// </summary>
    public FriendOnlineStatus Status;
    
    /// <summary>
    ///     The permissions you have granted to a friend
    /// </summary>
    public UserPermissions PermissionsGrantedToFriend;
    
    /// <summary>
    ///     The permissions a friend has granted you
    /// </summary>
    public UserPermissions PermissionsGrantedByFriend;

    /// <summary>
    ///     The last time a command was sent to this user
    /// </summary>
    public long LastInteractedWith = 0;
    
    /// <summary>
    ///     Gets the note if available, otherwise the friend code
    /// </summary>
    public string NoteOrFriendCode => Note ?? FriendCode;
}