using System;
using System.Collections.Generic;

namespace AetherRemoteClient.Domain.Enums;

/// <summary>
///     Settings for the plugin
/// </summary>

[Flags]
public enum Setting
{
    // ============== WARNING ==============
    // This file is a database schema file.
    // If you make changes, review database.
    // ============== WARNING ==============
    
    /// <summary> Should the plugin attempt to log in automatically </summary>
    AutoLogin = 0,
    
    /// <summary> Is the plugin in safe mode </summary>
    SafeMode = 1 << 0,
    
    /// <summary> Should the plugin show information on the Dtr bar </summary>
    ShowDtrBar = 1 << 1,
}