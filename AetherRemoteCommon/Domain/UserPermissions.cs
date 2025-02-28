using AetherRemoteCommon.Domain.Enums;
using MessagePack;

namespace AetherRemoteCommon.Domain;

/// <summary>
/// Stores the primary and linkshell permissions that make up the total permissions a user can grant another
/// </summary>
[MessagePackObject(keyAsPropertyName: true)]
public record UserPermissions
{
    /// <summary>
    /// Main permissions
    /// </summary>
    public PrimaryPermissions Primary { get; set; }

    /// <summary>
    /// Linkshell permissions
    /// </summary>
    public LinkshellPermissions Linkshell { get; set; }
}