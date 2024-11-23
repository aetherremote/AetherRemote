namespace AetherRemoteCommon.Domain.Permissions;

/// <summary>
/// Stores the primary and linkshell permissions that make up the total permissions a user can grant another
/// </summary>
public class UserPermissions(PrimaryPermissions primary, LinkshellPermissions linkshell)
{
    /// <summary>
    /// <inheritdoc cref="UserPermissions"/>
    /// </summary>
    public UserPermissions() : this(PrimaryPermissions.None, LinkshellPermissions.None)
    {
    }

    /// <summary>
    /// Main permissions
    /// </summary>
    public PrimaryPermissions Primary { get; set; } = primary;
    
    /// <summary>
    /// Linkshell permissions
    /// </summary>
    public LinkshellPermissions Linkshell { get; set; } = linkshell;
}