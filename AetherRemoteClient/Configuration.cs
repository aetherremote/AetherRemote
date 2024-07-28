using Dalamud.Configuration;
using Dalamud.Plugin;
using System;

namespace AetherRemoteClient;

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
    public bool AutoLogin = true;

    [NonSerialized]
    private IDalamudPluginInterface? pluginInterface;

    public void Initialize(IDalamudPluginInterface pluginInterface)
    {
        this.pluginInterface = pluginInterface;
    }

    public void Save()
    {
        pluginInterface!.SavePluginConfig(this);
    }
}
