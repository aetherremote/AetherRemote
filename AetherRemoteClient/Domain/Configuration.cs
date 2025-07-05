using System;
using System.Collections.Generic;
using Dalamud.Configuration;

namespace AetherRemoteClient.Domain;

/// <summary>
///     Configurations for the plugin, also used to store client-side information like last used secret, and notes
/// </summary>
[Serializable]
public class Configuration : IPluginConfiguration
{
    /// <summary>
    ///     Should the plugin automatically log the player in?
    /// </summary>
    public bool AutoLogin = false;

    /// <summary>
    ///     Is the plugin in safe mode
    /// </summary>
    public bool SafeMode = false;

    /// <summary>
    ///     Secret used to authenticate all calls to the server
    /// </summary>
    public string Secret { get; set; } = string.Empty;

    /// <summary>
    ///     Maps FriendCode to Note, used to get a note for a specific person
    /// </summary>
    public Dictionary<string, string> Notes { get; set; } = [];

    /// <summary>
    ///     Maps a character in the format CharacterName@WorldName to a <see cref="PermanentTransformationData"/>
    /// </summary>
    public Dictionary<string, PermanentTransformationData> PermanentTransformations { get; set; } = [];
    
    /// <summary>
    ///     Plugin configuration version
    /// </summary>
    public int Version { get; set; } = 1;

    /// <summary>
    ///     Save configuration to file
    /// </summary>
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}