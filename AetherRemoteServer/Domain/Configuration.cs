using System.Text.Json;

namespace AetherRemoteServer.Domain;

/// <summary>
/// Provides configuration values for the server
/// </summary>
public class Configuration
{
    private const string DefaultKey = "KEY";
    private const string DefaultCertificatePath = "CERTIFICATE";
    private const string DefaultCertificatePasswordPath = "PASSWORD";
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    
    public readonly string SigningKey;
    public readonly string CertificatePath;
    public readonly string CertificatePasswordPath;

    /// <summary>
    /// <inheritdoc cref="Configuration"/>
    /// </summary>
    public Configuration()
    {
        var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "config");
        var filePath = Path.Combine(directoryPath, "database.config");

        if (Directory.Exists(directoryPath) is false)
            Directory.CreateDirectory(directoryPath);

        if (File.Exists(filePath) is false)
        {
            var defaultConfig = new ConfigurationData
            {
                SigningKey = DefaultKey,
                CertificatePath = DefaultCertificatePath,
                CertificatePasswordPath = DefaultCertificatePasswordPath,
            };

            var defaultContent = JsonSerializer.Serialize(defaultConfig, JsonOptions);
            File.WriteAllText(filePath, defaultContent);
        }

        var json = File.ReadAllText(filePath);
        var config = JsonSerializer.Deserialize<ConfigurationData>(json) ??
                     throw new InvalidOperationException("Failed to deserialize server configuration data");

        if (config.SigningKey is DefaultKey || config.CertificatePath is DefaultCertificatePath ||
            config.CertificatePasswordPath is DefaultCertificatePasswordPath)
        {
            throw new InvalidOperationException("Configuration values must be set before running the server");
        }

        SigningKey = config.SigningKey;
        CertificatePath = config.CertificatePath;
        CertificatePasswordPath = config.CertificatePasswordPath;
    }
    
    private class ConfigurationData
    {
        public string SigningKey { get; init; } = string.Empty;
        public string CertificatePath { get; init; } = string.Empty;
        public string CertificatePasswordPath { get; init; } = string.Empty;
    }
}
