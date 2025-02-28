using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;

namespace AetherRemoteCommon.Util;

/// <summary>
///     Extensions for <see cref="UserPermissions"/>
/// </summary>
public static class UserPermissionsExtensions
{
    /// <summary>
    ///     Does the permissions set have the provided <see cref="PrimaryPermissions"/>?
    /// </summary>
    public static bool Has(this UserPermissions userPermissions, PrimaryPermissions permissions)
    {
        return (userPermissions.Primary & permissions) == permissions;
    }
    
    /// <summary>
    ///     Does the permissions set have the provided <see cref="LinkshellPermissions"/>?
    /// </summary>
    public static bool Has(this UserPermissions userPermissions, LinkshellPermissions permissions)
    {
        return (userPermissions.Linkshell & permissions) == permissions;
    }
}