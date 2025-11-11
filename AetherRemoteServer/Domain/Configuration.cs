using Newtonsoft.Json;

namespace AetherRemoteServer.Domain;

[Serializable]
public class Configuration(
    string certificatePasswordPath,
    string certificatePath,
    string betaDatabasePath,
    string releaseDatabasePath,
    string signingKey)
{
    private static readonly string ConfigurationPath = Path.Combine(Directory.GetCurrentDirectory(), "Configuration", "Paths.json");
    
    public readonly string CertificatePasswordPath = certificatePasswordPath;
    public readonly string CertificatePath = certificatePath;
    public readonly string BetaDatabasePath = betaDatabasePath;
    public readonly string ReleaseDatabasePath = releaseDatabasePath;
    public readonly string SigningKey = signingKey;

    public static Configuration? Load()
    {
        try
        {
            if (!File.Exists(ConfigurationPath))
            {
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "Configuration"));
                File.WriteAllText(ConfigurationPath, JsonConvert.SerializeObject(Default, Formatting.Indented));
                Console.WriteLine($"[Configuration] [Load] Configuration file created at {ConfigurationPath}, please edit the values and run the application again.");
                return null;
            }
            
            var json = File.ReadAllText(ConfigurationPath);
            if (JsonConvert.DeserializeObject<Configuration>(json) is not { } configuration)
            {
                Console.WriteLine("[Configuration] [Load] Unable to deserialize configuration.");
                return null;
            }

            if (configuration.HasDefaultValues())
            {
                Console.WriteLine($"[Configuration] [Load] Paths not set in {ConfigurationPath}, please edit the values and run the application again.");
                return null;
            }
            
            Console.WriteLine("[Configuration] [Load] Configuration successfully loaded.");
            return configuration;
        }
        catch (Exception e)
        {
            Console.WriteLine($"[Configuration] [Load] An unknown error occured, {e.Message}.");
            return null;
        }
    }

    private bool HasDefaultValues() => CertificatePath is Empty || 
                                       CertificatePasswordPath is Empty ||
                                       BetaDatabasePath is Empty || 
                                       ReleaseDatabasePath is Empty ||
                                       SigningKey is Empty;

    private const string Empty = "Empty"; 
    private static readonly Configuration Default = new(Empty, Empty, Empty, Empty, Empty);
}