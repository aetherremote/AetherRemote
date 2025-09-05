using System;
using System.IO;
using System.Threading.Tasks;
using AetherRemoteClient.Domain.Configurations;
using AetherRemoteClient.Domain.Hypnosis;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AetherRemoteClient.Services;

public static class ConfigurationService
{
    // Configurations
    private const string ConfigurationFileName = "Configuration.json";
    private static readonly string ConfigurationFilePath = Path.Combine(Plugin.PluginInterface.GetPluginConfigDirectory(), ConfigurationFileName);
    
    // Character Configurations
    private const string CharactersFolderName = "Characters";
    private static readonly string CharactersFolderPath = Path.Combine(Plugin.PluginInterface.GetPluginConfigDirectory(), CharactersFolderName);
    
    // Hypnosis Profiles
    private const string HypnosisFolderName = "Hypnosis";
    public static readonly string HypnosisFolderPath = Path.Combine(Plugin.PluginInterface.GetPluginConfigDirectory(), HypnosisFolderName);
    
    /// <summary>
    ///     Loads the plugin configuration file
    /// </summary>
    public static async Task<Configuration?> LoadConfiguration()
    {
        // Check if the configuration file doesn't exist
        if (File.Exists(ConfigurationFilePath) is false)
        {
            try
            {
                // Create the directory
                await Task.Run(() => Directory.CreateDirectory(Plugin.PluginInterface.GetPluginConfigDirectory())).ConfigureAwait(false);

                // Create a new configuration
                var configuration = new Configuration();
                
                // Check to see if there is a legacy configuration present
                if (Plugin.LegacyConfiguration is { } legacyConfiguration)
                {
                    // Copy the notes
                    configuration.Notes = legacyConfiguration.Notes;
                }
                
                // Save the new configuration
                await SaveConfiguration(configuration).ConfigureAwait(false);
                
                // Return it
                return configuration;
            }
            catch (Exception e)
            {
                // Maybe the plugin should not be use-able at this state?
                Plugin.Log.Info($"[ConfigurationService] Unable to create configuration file, {e}");
                return null;
            }
        }
        
        try
        {
            // Read the config
            var json = await File.ReadAllTextAsync(ConfigurationFilePath).ConfigureAwait(false);

            // Parse it into a JObject
            var configuration = await Task.Run(() => JObject.Parse(json)).ConfigureAwait(false);
            
            // Check the version of the config for any possible upgrades
            switch (configuration["Version"]?.Value<int>())
            {
                // Current
                case 1:
                    return await Task.Run(() => configuration.ToObject<Configuration>()).ConfigureAwait(false);

                // Parse failure
                case null:
                    Plugin.Log.Error("[ConfigurationService] Unable to find configuration version");
                    return null;

                // Unknown
                default:
                    Plugin.Log.Warning("[ConfigurationService] Unsupported configuration version");
                    return null;
            }
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[ConfigurationService] Unable to load configuration, {e}");
            return null;
        }
    }

    /// <summary>
    ///     Save the plugin configuration file
    /// </summary>
    public static async Task SaveConfiguration(Configuration configuration)
    {
        // Serialize to json
        var json = await Task.Run(() => JsonConvert.SerializeObject(configuration, Formatting.Indented)).ConfigureAwait(false);
        
        try
        {
            // Write to disk
            await File.WriteAllTextAsync(ConfigurationFilePath, json).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[ConfigurationService] Unable to save configuration, {e}");
        }
    }
    
    /// <summary>
    ///     Loads the character configuration for provided character
    /// </summary>
    public static async Task<CharacterConfiguration?> LoadCharacterConfiguration(string characterName, string characterWorld)
    {
        // Combine the name and world to get a unique identifier as filename
        var fullNameFileName = string.Concat(characterName, " - ",  characterWorld, ".json");
        
        // Combine the folder path and the character's full name
        var fullNamePath = Path.Combine(CharactersFolderPath, fullNameFileName);
        
        // Check if the configuration file doesn't exist
        if (File.Exists(fullNamePath) is false)
        {
            try
            {
                // Create the directory and write an empty file
                await Task.Run(() => Directory.CreateDirectory(CharactersFolderPath)).ConfigureAwait(false);

                // Create a new empty configuration
                var configuration = new CharacterConfiguration
                {
                    Name = characterName,
                    World = characterWorld
                };
                
                // Save it to disk
                await SaveCharacterConfiguration(configuration).ConfigureAwait(false);
                
                // Return it
                return configuration;
            }
            catch (Exception e)
            {
                // Maybe the plugin should not be use-able at this state?
                Plugin.Log.Info($"[ConfigurationService] Unable to create configuration file, {e}");
                return null;
            }
        }

        try
        {
            // Read the config
            var json = await File.ReadAllTextAsync(fullNamePath).ConfigureAwait(false);

            // Parse it into a JObject
            var configuration = await Task.Run(() => JObject.Parse(json)).ConfigureAwait(false);
            
            // Check the version of the config for any possible upgrades
            switch (configuration["Version"]?.Value<int>())
            {
                // Current
                case 1:
                    return await Task.Run(() => configuration.ToObject<CharacterConfiguration>()).ConfigureAwait(false);

                // Parse failure
                case null:
                    Plugin.Log.Error("[ConfigurationService] Unable to find character configuration version");
                    return null;

                // Unknown
                default:
                    Plugin.Log.Warning("[ConfigurationService] Unsupported character configuration version");
                    return null;
            }
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[ConfigurationService] Unable to load character configuration, {e}");
            return null;
        }
    }

    /// <summary>
    ///     Saves the character configuration
    /// </summary>
    public static async Task SaveCharacterConfiguration(CharacterConfiguration configuration)
    {
        // Combine the name and world to get a unique identifier as filename
        var fullNameFileName = string.Concat(configuration.Name, " - ",  configuration.World, ".json");
        
        // Combine the folder path and the character's full name
        var fullNamePath = Path.Combine(CharactersFolderPath, fullNameFileName);
        
        // Serialize to json
        var json = await Task.Run(() => JsonConvert.SerializeObject(configuration, Formatting.Indented)).ConfigureAwait(false);
        
        try
        {
            // Write to disk
            await File.WriteAllTextAsync(fullNamePath, json).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[ConfigurationService] Unable to save character configuration, {e}");
        }
    }

    /// <summary>
    ///     Loads a hypnosis profile
    /// </summary>
    public static async Task<HypnosisProfile?> LoadHypnosisProfile(string hypnosisProfileName)
    {
        // Combine the name and world to get a unique identifier as filename
        var fullHypnosisProfileName = string.Concat(hypnosisProfileName, ".json");
        
        // Combine the folder path and the character's full name
        var fullHypnosisProfilePath = Path.Combine(HypnosisFolderPath, fullHypnosisProfileName);

        // Check if the configuration file doesn't exist
        if (File.Exists(fullHypnosisProfilePath) is false)
            return null;

        try
        {
            // Read the config
            var json = await File.ReadAllTextAsync(fullHypnosisProfilePath).ConfigureAwait(false);
            
            // Parse it into a JObject
            var hypnosisProfile = JObject.Parse(json);
            
            // Check the version of the config for any possible upgrades
            switch (hypnosisProfile["Version"]?.Value<int>())
            {
                // Current
                case 1:
                    return await Task.Run(() => hypnosisProfile.ToObject<HypnosisProfile>()).ConfigureAwait(false);

                // Parse failure
                case null:
                    Plugin.Log.Error("[ConfigurationService] Unable to find hypnosis profile version");
                    return null;

                // Unknown
                default:
                    Plugin.Log.Warning("[ConfigurationService] Unsupported hypnosis profile version");
                    return null;
            }
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[ConfigurationService] Unable to load hypnosis profile, {e}");
            return null;
        }
    }

    /// <summary>
    ///     Saves a hypnosis profile
    /// </summary>
    public static async Task SaveHypnosisProfile(HypnosisProfile profile)
    {
        // Combine the name and world to get a unique identifier as filename
        var fullHypnosisProfileName = string.Concat(profile.Name, ".json");
        
        // Combine the folder path and the character's full name
        var fullHypnosisProfilePath = Path.Combine(HypnosisFolderPath, fullHypnosisProfileName);
        
        // Serialize to json
        var json = await Task.Run(() => JsonConvert.SerializeObject(profile, Formatting.Indented)).ConfigureAwait(false);
        
        try
        {
            // Create the directory if it doesn't exist
            await Task.Run(() => Directory.CreateDirectory(HypnosisFolderPath)).ConfigureAwait(false);
            
            // Write to disk
            await File.WriteAllTextAsync(fullHypnosisProfilePath, json).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[ConfigurationService] Unable to save hypnosis profile, {e}");
        }
    }

    /// <summary>
    ///     Deletes a hypnosis profile
    /// </summary>
    public static async Task DeleteHypnosisProfile(string hypnosisProfileName)
    {
        // Combine the name and world to get a unique identifier as filename
        var fullHypnosisProfileName = string.Concat(hypnosisProfileName, ".json");
        
        // Combine the folder path and the character's full name
        var fullHypnosisProfilePath = Path.Combine(HypnosisFolderPath, fullHypnosisProfileName);

        // Check if the configuration file doesn't exist
        if (File.Exists(fullHypnosisProfilePath) is false)
            return;

        try
        {
            await Task.Run(() => File.Delete(fullHypnosisProfilePath)).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[ConfigurationService] Unable to delete hypnosis profile, {e}");
        }
    }
}