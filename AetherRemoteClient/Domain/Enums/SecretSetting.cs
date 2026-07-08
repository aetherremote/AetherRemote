using System;

namespace AetherRemoteClient.Domain.Enums;

/// <summary>
///     Settings for a specific secret (account) logged in in the plugin
/// </summary>

[Flags]
public enum SecretSetting
{
    // ============== WARNING ==============
    // This file is a database schema file.
    // If you make changes, review database.
    // ============== WARNING ==============
    
    /// <summary> Should the plugin attempt to log in automatically </summary>
    AutoLogin = 0
}