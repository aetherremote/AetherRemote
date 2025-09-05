using System;
using System.Threading.Tasks;
using AetherRemoteClient.Services;

namespace AetherRemoteClient.Domain.Configurations;

/// <summary>
///     The individual character configuration file 
/// </summary>
[Serializable]
public class CharacterConfiguration
{
    /// <summary>
    ///     Configuration version
    /// </summary>
    public int Version = 1;

    /// <summary>
    ///     Character's name
    /// </summary>
    public string Name = string.Empty;

    /// <summary>
    ///     Character's home world
    /// </summary>
    public string World = string.Empty;
    
    /// <summary>
    ///     Should the plugin login automatically
    /// </summary>
    public bool AutoLogin;
    
    /// <summary>
    ///     The secret to use for this character
    /// </summary>
    public string Secret = string.Empty;
    
    /// <summary>
    ///     Save the configuration
    /// </summary>
    public async Task Save() => await ConfigurationService.SaveCharacterConfiguration(this).ConfigureAwait(false);
}