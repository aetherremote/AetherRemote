using System;
using System.IO;
using System.Threading.Tasks;
using AetherRemoteClient.Domain.Hypnosis;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AetherRemoteClient.Services;

public static class ConfigurationService
{
    // Hypnosis Profiles
    private const string HypnosisFolderName = "Hypnosis";
    public static readonly string HypnosisFolderPath = Path.Combine(Plugin.PluginInterface.GetPluginConfigDirectory(), HypnosisFolderName);

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