using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums.Permissions;

namespace AetherRemoteServer.Domain;

/// <summary>
///     A bidirectional set of permissions
/// </summary>
public record TwoWayPermissions
{
    /// <summary>
    ///     The owner of the permissions
    /// </summary>
    public readonly string FriendCode;
    
    /// <summary>
    ///     The target of the permissions
    /// </summary>
    public readonly string TargetFriendCode;
    
    /// <summary>
    ///     The permissions the owner has granted to the target
    /// </summary>
    public readonly UserPermissions PermissionsGrantedTo;
    
    /// <summary>
    ///     The permissions the target has granted to the owner
    /// </summary>
    public readonly UserPermissions? PermissionsGrantedBy;
    
    /// <summary>
    ///     <inheritdoc cref="TwoWayPermissions"/>
    /// </summary>
    public TwoWayPermissions(string friendCode, string targetFriendCode, PrimaryPermissions2 primary, SpeakPermissions2 speak, ElevatedPermissions elevated)
    {
        FriendCode = friendCode;
        TargetFriendCode = targetFriendCode;
        PermissionsGrantedTo = new UserPermissions(primary, speak, elevated);
        PermissionsGrantedBy = null;
    }

    /// <summary>
    ///     <inheritdoc cref="TwoWayPermissions"/>
    /// </summary>
    public TwoWayPermissions(string friendCode, string targetFriendCode, PrimaryPermissions2 primary, SpeakPermissions2 speak, ElevatedPermissions elevated, PrimaryPermissions2 primary2, SpeakPermissions2 speak2, ElevatedPermissions elevated2)
    {
        FriendCode = friendCode;
        TargetFriendCode = targetFriendCode;
        PermissionsGrantedTo = new UserPermissions(primary, speak, elevated);
        PermissionsGrantedBy = new UserPermissions(primary2, speak2, elevated2);
    }
}