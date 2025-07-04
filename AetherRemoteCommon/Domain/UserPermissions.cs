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
    ///     TODO
    /// </summary>
    public UserPermissions()
    {
    }

    /// <summary>
    ///     TODO
    /// </summary>
    public UserPermissions(PrimaryPermissions2 primary, SpeakPermissions2 speak)
    {
        Primary = primary;
        Speak = speak;
    }
}