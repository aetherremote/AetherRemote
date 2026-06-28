using System;

namespace AetherRemoteClient.Domain.Enums;

/// <summary>
///     Global Settings for the plugin
/// </summary>

[Flags]
public enum GlobalSetting
{
    // ============== WARNING ==============
    // This file is a database schema file.
    // If you make changes, review database.
    // ============== WARNING ==============
    
    /// <summary> Is the plugin in safe mode </summary>
    SafeMode = 0,
    
    /// <summary> Should the plugin show information on the Dtr bar </summary>
    ShowOnDtrBar = 1 << 0
}