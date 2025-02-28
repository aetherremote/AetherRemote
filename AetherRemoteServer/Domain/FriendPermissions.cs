using AetherRemoteCommon.Domain;
using FriendCode = string;

namespace AetherRemoteServer.Domain;

/// <summary>
/// Encapsulates the permissions a friend has granted to others
/// </summary>
public class FriendPermissions
{
    /// <summary>
    /// Maps a Friend Code to User Permissions
    /// </summary>
    public Dictionary<FriendCode, UserPermissions> Permissions { get; set; } = [];
}