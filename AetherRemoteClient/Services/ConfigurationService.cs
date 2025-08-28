using System;
using System.IO;
using AetherRemoteClient.Domain.Configurations;
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
    
    /// <summary>
    ///     Loads the plugin configuration file
    /// </summary>
    public static Configuration? LoadConfiguration()
    {
        // Check if the configuration file doesn't exist
        if (File.Exists(ConfigurationFilePath) is false)
        {
            try
            {
                // Create the directory
                Directory.CreateDirectory(Plugin.PluginInterface.GetPluginConfigDirectory());

                // Create a new configuration
                var configuration = new Configuration();
                
                // Check to see if there is a legacy configuration present
                if (Plugin.LegacyConfiguration is { } legacyConfiguration)
                {
                    // Copy the notes
                    configuration.Notes = legacyConfiguration.Notes;
                }
                
                // Save the new configuration
                SaveConfiguration(configuration);
                
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
            var json = File.ReadAllText(ConfigurationFilePath);

            // Parse it into a JObject
            var configuration = JObject.Parse(json);
            
            // Check the version of the config for any possible upgrades
            switch (configuration["Version"]?.Value<int>())
            {
                // Current
                case 1:
                    return configuration.ToObject<Configuration>();

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
    public static void SaveConfiguration(Configuration configuration)
    {
        // Serialize to json
        var json = JsonConvert.SerializeObject(configuration, Formatting.Indented);
        
        try
        {
            // Write to disk
            File.WriteAllText(ConfigurationFilePath, json);
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[ConfigurationService] Unable to save configuration, {e}");
        }
    }
    
    /// <summary>
    ///     Loads the character configuration for provided character
    /// </summary>
    public static CharacterConfiguration? LoadCharacterConfiguration(string characterName, string characterWorld)
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
                Directory.CreateDirectory(CharactersFolderPath);

                // Create a new empty configuration
                var configuration = new CharacterConfiguration
                {
                    Name = characterName,
                    World = characterWorld
                };
                
                // Save it to disk
                SaveCharacterConfiguration(configuration);
                
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
            var json = File.ReadAllText(fullNamePath);

            // Parse it into a JObject
            var configuration = JObject.Parse(json);
            
            // Check the version of the config for any possible upgrades
            switch (configuration["Version"]?.Value<int>())
            {
                // Current
                case 1:
                    return configuration.ToObject<CharacterConfiguration>();

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
    public static void SaveCharacterConfiguration(CharacterConfiguration configuration)
    {
        // Combine the name and world to get a unique identifier as filename
        var fullNameFileName = string.Concat(configuration.Name, " - ",  configuration.World, ".json");
        
        // Combine the folder path and the character's full name
        var fullNamePath = Path.Combine(CharactersFolderPath, fullNameFileName);
        
        // Serialize to json
        var json = JsonConvert.SerializeObject(configuration, Formatting.Indented);
        
        try
        {
            // Write to disk
            File.WriteAllText(fullNamePath, json);
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[ConfigurationService] Unable to save character configuration, {e}");
        }
    }
}