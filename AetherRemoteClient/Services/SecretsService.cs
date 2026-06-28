using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Infrastructure.Database;

namespace AetherRemoteClient.Services;

/// <summary>
///     Provides access to any secrets the user has 
/// </summary>
public class SecretsService(DatabaseInfrastructure database)
{
    private Dictionary<long, Secret> _secrets = [];
    private bool _dirty;
    
    /// <summary> List of all secrets the client has </summary>
    public ImmutableDictionary<long, Secret> Secrets
    {
        get
        {
            if (_dirty is false)
                return field;

            field = _secrets.ToImmutableDictionary();
            _dirty = false;

            return field;
        }
    } = [];
    
    /// <summary> Load all the secrets from the database </summary>
    /// <remarks> Call this once when the plugin loads initially </remarks>
    public async Task<bool> LoadSecrets()
    {
        if (await database.GetSecrets().ConfigureAwait(false) is not { } secrets)
            return false;

        _secrets = secrets;
        _dirty = true;
        return true;
    }

    /// <summary> Add a new secret </summary>
    public async Task<bool> AddSecret(string secretName, string secretValue)
    {
        if (await database.AddSecret(secretName, secretValue).ConfigureAwait(false) is not { } secret)
            return false;
        
        _secrets.Add(secret.Id, secret);
        _dirty = true;
        return true;
    }
    
    /// <summary> Remove a secret </summary>
    public async Task<bool> RemoveSecret(long secretId)
    {
        if (await database.DeleteSecret(secretId).ConfigureAwait(false) is false)
            return false;
        
        _secrets.Remove(secretId);
        _dirty = true;
        return true;
    }

    /// <summary> Count the number of characters using a secret id </summary>
    public async Task<int> CountUsage(long secretId) => await database.GetSecretUsageCount(secretId).ConfigureAwait(false);
}