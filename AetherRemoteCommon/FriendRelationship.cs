using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using MessagePack;

namespace AetherRemoteCommon;

/// <summary>
///     A relationship between two users as seen from the owning user's perspective
/// </summary>
[MessagePackObject(keyAsPropertyName: true)]
public record FriendRelationship
{
    /// <summary>
    ///     The friend code of the target user
    /// </summary>
    public string TargetFriendCode { get; set; } = string.Empty;
    
    /// <summary>
    ///     The online status of the target user
    /// </summary>
    public FriendOnlineStatus Status { get; set; }
    
    /// <summary>
    ///     The permissions the owning user has granted the target user
    /// </summary>
    public UserPermissions PermissionsGrantedTo { get; set; } = new();
    
    /// <summary>
    ///     The permissions the target yser has granted the owning user
    /// </summary>
    public UserPermissions? PermissionsGrantedBy { get; set; }

    /// <summary>
    ///     <inheritdoc cref="FriendRelationship"/>
    /// </summary>
    public FriendRelationship()
    {
    }

    /// <summary>
    ///     <inheritdoc cref="FriendRelationship"/>
    /// </summary>
    public FriendRelationship(string targetFriendCode, FriendOnlineStatus status, UserPermissions permissionsGrantedTo, UserPermissions? permissionsGrantedBy)
    {
        TargetFriendCode = targetFriendCode;
        Status = status;
        PermissionsGrantedTo = permissionsGrantedTo;
        PermissionsGrantedBy = permissionsGrantedBy;
    }
}