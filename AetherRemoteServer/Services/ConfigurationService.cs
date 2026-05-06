using AetherRemoteServer.Domain;
using Newtonsoft.Json;

namespace AetherRemoteServer.Services;

/// <summary>
///     Simple service to manage configuration values not appropriate for app settings
/// </summary>
public static class ConfigurationService
{
    private const string ConfigurationFolderName = "configuration";
    private static readonly string ConfigurationPath = Path.Combine(Directory.GetCurrentDirectory(), ConfigurationFolderName, "paths.json");
    private static readonly Configuration DefaultConfiguration = new(string.Empty, string.Empty, string.Empty, string.Empty);

    /// <summary>
    ///     Attempts to load the config. If a configuration file isn't found, one will be generated.
    /// </summary>
    public static Configuration? Load()
    {
        try
        {
            if (File.Exists(ConfigurationPath) is false)
            {
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), ConfigurationFolderName));
                File.WriteAllText(ConfigurationPath, JsonConvert.SerializeObject(DefaultConfiguration, Formatting.Indented));
                return null;
            }
            
            var json = File.ReadAllText(ConfigurationPath);
            if (JsonConvert.DeserializeObject<Configuration>(json) is not { } configuration)
            {
                Console.WriteLine("[Configuration.Load] Unable to deserialize configuration.");
                return null;
            }
            
            // Certificate paths can be empty since development doesn't use them
            if (configuration.DatabasePath == string.Empty || configuration.SigningKey == string.Empty)
            {
                Console.WriteLine($"[Configuration.Load] Paths not set in {ConfigurationPath}, please edit the values and run the application again.");
                return null;
            }
            
            return configuration;
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"[Configuration.Load] {e}");
            return null;
        }
    }
}