namespace AetherRemoteServer.Domain;

/// <summary>
///     Configuration object required for server
/// </summary>
/// <param name="DatabasePath">Path to the .db file</param>
/// <param name="CertificateCrtPath">Path to the .crt certificate file, can be empty for development environments</param>
/// <param name="CertificateKeyPath">Path to the .key certificate file, can be empty for development environments</param>
/// <param name="SigningKey">Signing key for JWTs</param>
public record Configuration(string DatabasePath, string CertificateCrtPath, string CertificateKeyPath, string SigningKey);