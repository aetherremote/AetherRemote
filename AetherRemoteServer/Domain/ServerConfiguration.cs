using System.Text.Json;

namespace AetherRemoteServer.Domain;

public class ServerConfiguration
{
    private const string DEFAULT_KEY = "KEY";
    private const string DEFAULT_CERTIFICATE_PATH = "CERTIFICATE";
    private const string DEFAULT_CERTIFICATE_PASSWORD_PATH = "PASSWORD";
    private const string DEFAULT_ACCOUNT_SECRET = "ACCOUNT";
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public readonly string SigningKey;
    public readonly string CertificatePath;
    public readonly string CertificatePasswordPath;
    public readonly string AdminAccountSecret;

    public ServerConfiguration()
    {
        var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "config");
        var filePath = Path.Combine(directoryPath, "database.config");

        if (Directory.Exists(directoryPath) == false)
            Directory.CreateDirectory(directoryPath);

        if (File.Exists(filePath) == false)
        {
            var serialized = new ConfigurationData
            {
                SigningKey = DEFAULT_KEY,
                CertificatePath = DEFAULT_CERTIFICATE_PATH,
                CertificatePasswordPath = DEFAULT_CERTIFICATE_PASSWORD_PATH,
                AdminAccountSecret = DEFAULT_ACCOUNT_SECRET
            };

            var content = JsonSerializer.Serialize(serialized, JsonOptions);
            File.WriteAllText(filePath, content);
        }

        var json = File.ReadAllText(filePath);
        var configuration = JsonSerializer.Deserialize<ConfigurationData>(json)!;
        if (configuration.SigningKey == DEFAULT_KEY 
            || configuration.CertificatePath == DEFAULT_CERTIFICATE_PATH
            || configuration.CertificatePasswordPath == DEFAULT_CERTIFICATE_PASSWORD_PATH
            || configuration.AdminAccountSecret == DEFAULT_ACCOUNT_SECRET)
        {
            throw new Exception("You must set configuration values first.");
        }

        SigningKey = configuration.SigningKey;
        CertificatePath = configuration.CertificatePath;
        CertificatePasswordPath = configuration.CertificatePasswordPath;
        AdminAccountSecret = configuration.AdminAccountSecret;
    }
}

public class ConfigurationData
{
    public string SigningKey { get; set; } = string.Empty;
    public string CertificatePath { get; set; } = string.Empty;
    public string CertificatePasswordPath { get; set; } = string.Empty;
    public string AdminAccountSecret { get; set; } = string.Empty;
}
