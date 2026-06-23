using System;

namespace AetherRemoteClient.Domain.Enums;

/// <summary>
///     Agreements for the plugin
/// </summary>

[Flags]
public enum Agreement
{
    // ============== WARNING ==============
    // This file is a database schema file.
    // If you make changes, review database.
    // ============== WARNING ==============
    
    /// <summary> The user accepts the risks of using possession </summary>
    Possession = 0
}