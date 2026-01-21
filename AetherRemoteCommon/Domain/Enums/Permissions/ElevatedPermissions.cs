namespace AetherRemoteCommon.Domain.Enums.Permissions;

/// <summary>
///     Elevated permissions represent a subset of permissions that may allow for more intrusive behavior
/// </summary>
[Flags]
public enum ElevatedPermissions
{
    // No Permissions
    None = 0,
    
    /// <summary>
    ///     Allows for the saving of an appearance permanently
    /// </summary>
    PermanentTransformation = 1 << 0,
    
    /// <summary>
    ///     Allows for the possessing of another character
    /// </summary>
    Possession = 1 << 1
}