using System;
using System.Collections.Generic;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;

namespace AetherRemoteClient.Domain.Configurations;

/// <summary>
///     The global configuration
/// </summary>
[Serializable]
public class Configuration
{
    /// <summary>
    ///     Configuration version
    /// </summary>
    public int Version = 1;

    /// <summary>
    ///     Is the plugin in safe mode
    /// </summary>
    public bool SafeMode = false;

    /// <summary>
    ///     Map of friend code to note
    /// </summary>
    public Dictionary<string, string> Notes = [];

    /// <summary>
    ///     Save the configuration
    /// </summary>
    public void Save() => ConfigurationService.SaveConfiguration(this);
}