using AetherRemoteCommon.Domain.Enums.Permissions;
using MessagePack;

namespace AetherRemoteCommon.Domain;

/// <summary>
///     Stores the primary and linkshell permissions that make up the total permissions a user can grant another
/// </summary>
[MessagePackObject(keyAsPropertyName: true)]
public record UserPermissions
{
    /// <summary>
    ///     Primary permissions
    /// </summary>
    public PrimaryPermissions2 Primary { get; set; }

    /// <summary>
    ///     Speak permissions
    /// </summary>
    public SpeakPermissions2 Speak { get; set; }
    
    /// <summary>
    ///     Elevated permissions
    /// </summary>
    public ElevatedPermissions Elevated { get; set; }

    /// <summary>
    ///     <inheritdoc cref="UserPermissions"/>
    /// </summary>
    public UserPermissions()
    {
    }

    /// <summary>
    ///     <inheritdoc cref="UserPermissions"/>
    /// </summary>
    public UserPermissions(PrimaryPermissions2 primary, SpeakPermissions2 speak, ElevatedPermissions elevated)
    {
        Primary = primary;
        Speak = speak;
        Elevated = elevated;
    }

    public static UserPermissions From(ResolvedPermissions permissions)
    {
        return new UserPermissions
        {
            Primary = permissions.Primary,
            Speak = permissions.Speak,
            Elevated = permissions.Elevated
        };
    }
}