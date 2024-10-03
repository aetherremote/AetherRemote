using Dalamud.Configuration;
using System;
using System.Collections.Generic;

namespace AetherRemoteClient.Domain;

/// <summary>
/// Configurations for the plugin, also used to store client-side information like last used secret, and notes
/// </summary>
[Serializable]
public class Configuration : IPluginConfiguration
{
    /// <summary>
    /// Plugin configuration version
    /// </summary>
    public int Version { get; set; } = 1;

    /// <summary>
    /// Secret used to authenticate all calls to the server
    /// </summary>
    public string Secret { get; set; } = string.Empty;

    /// <summary>
    /// Should the plugin automatically log the player in
    /// </summary>
    public bool AutoLogin = false;

    /// <summary>
    /// Maps FriendCode to Note, used to get a note for a specific person
    /// </summary>
    public Dictionary<string, string> Notes { get; set; } = [];

    /// <summary>
    /// Save configuration to file
    /// </summary>
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
