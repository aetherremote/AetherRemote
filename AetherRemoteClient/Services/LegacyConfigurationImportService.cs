using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Infrastructure.Database;
using Newtonsoft.Json.Linq;

namespace AetherRemoteClient.Services;

/// <summary>
///     Class responsible for any types of upgrades required for configurations
/// </summary>
public class LegacyConfigurationImportService(DatabaseInfrastructure databaseInfrastructure)
{
    private const string ConfigurationFileName = "Configuration.json";
    private static readonly string ConfigurationFilePath = Path.Combine(Plugin.PluginInterface.GetPluginConfigDirectory(), ConfigurationFileName);
    
    // Character Configurations
    private const string CharactersFolderName = "Characters";
    private static readonly string CharactersFolderPath = Path.Combine(Plugin.PluginInterface.GetPluginConfigDirectory(), CharactersFolderName);
    
    /// <summary>
    ///     Scans for legacy configuration and character files and updates them automatically
    /// </summary>
    public async Task ScanForConfigurationsAndImport()
    {
        try
        {
            var configurationFileExists = File.Exists(ConfigurationFilePath);
            var charactersDirectoryExists = Directory.Exists(CharactersFolderPath);

            if (configurationFileExists is false && charactersDirectoryExists is false)
                return;
            
            var configurationJson = await File.ReadAllTextAsync(ConfigurationFilePath).ConfigureAwait(false);
            var configuration = JToken.Parse(configurationJson);
            
            var safeMode = configuration["SafeMode"]?.ToObject<bool>() ?? false;
            var showOnDtrBar = configuration["ShowOnDtrBar"]?.ToObject<bool>() ?? false;
            var notes = configuration["Notes"]?.ToObject<Dictionary<string, string>>() ?? [];
            
            var secrets = new Dictionary<string, List<Character>>();
            var characterPaths = Directory.EnumerateFiles(CharactersFolderPath, "*.json");
            foreach (var characterPath in characterPaths)
            {
                var json = await File.ReadAllTextAsync(characterPath).ConfigureAwait(false);
                var character = JToken.Parse(json);

                if (character["Name"]?.ToObject<string>() is not { } name) continue;
                if (character["World"]?.ToObject<string>() is not { } world) continue;
                if (character["Secret"]?.ToObject<string>() is not { } secret) continue;

                if (string.IsNullOrEmpty(secret)) continue;
                
                if (secrets.TryGetValue(secret, out var characters) is false)
                {
                    characters = [];
                    secrets.Add(secret, characters);
                }
                
                characters.Add(new Character(name, world));
            }

            // This utilizes transactions to make sure the entire upgrade happens at once
            if (await databaseInfrastructure.ImportLegacyConfigurationValues(notes, secrets, safeMode, showOnDtrBar).ConfigureAwait(false))
            {
                File.Delete(ConfigurationFilePath);
                Directory.Delete(CharactersFolderPath, true);
            }
            else
            {
                Plugin.Log.Warning("[LegacyConfigurationImportService.ScanForConfigurationsAndImport] Import unsuccessful");
            }
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[LegacyConfigurationImportService.ScanForConfigurationsAndImport] {e}");
        }
    }
}