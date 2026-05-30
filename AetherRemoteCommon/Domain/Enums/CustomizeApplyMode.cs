namespace AetherRemoteCommon.Domain.Enums;

/// <summary>
///     Describes how a Customize+ profile should be applied
/// </summary>
public enum CustomizeApplyMode
{
    /// <summary>
    ///     Default value, never should be encountered
    /// </summary>
    Uninitialized,
    
    /// <summary>
    ///     Default apply mode, which can be summarized as "overwrite" or "replace" the current profile
    /// </summary>
    Default,
    
    /// <summary>
    ///     Merge the new profile data with the existing one
    /// </summary>
    Merge
}