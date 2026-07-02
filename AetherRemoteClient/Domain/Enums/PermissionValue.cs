using AetherRemoteClient.UI.Views.Friends;

namespace AetherRemoteClient.Domain.Enums;

/// <summary>
///     A permission set for use in <see cref="FriendsView"/>
/// </summary>
public enum PermissionValue
{
    Deny = 0,
    Inherit = 1,
    Allow = 2
}