using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using AetherRemoteClient.Domain;

namespace AetherRemoteClient.Services.Configuration;

public partial class ConfigurationService
{
    private Dictionary<long, Secret> _secrets = [];
    private bool _dirtySecrets;
    
    /// <summary> List of all the secrets from the database </summary>
    public ImmutableDictionary<long, Secret> Secrets
    {
        get
        {
            if (_dirtySecrets is false)
                return field;

            field = _secrets.ToImmutableDictionary();
            _dirtySecrets = false;

            return field;
        }
    } = [];
    
    /// <summary> Adds a new secret to the database </summary>
    public async Task<bool> AddSecret(string secretName, string secretValue)
    {
        if (await databaseInfrastructure.AddSecret(secretName, secretValue).ConfigureAwait(false) is not { } secret)
            return false;
        
        _secrets.Add(secret.Id, secret);
        _dirtySecrets = true;
        return true;
    }
    
    /// <summary> Adds a new secret to the database </summary>
    public async Task<bool> RenameSecret(long secretId, string secretName)
    {
        if (await databaseInfrastructure.RenameSecret(secretId, secretName).ConfigureAwait(false) is false)
            return false;
        
        var secret = _secrets[secretId];
        _secrets[secretId] = new Secret(secretId, secretName, secret.Value, secret.CreatedAt);
        _dirtySecrets = true;
        return true;
    }
    
    /// <summary> Deletes a secret from database, if one exists </summary>
    public async Task<bool> DeleteSecret(long secretId)
    {
        if (await databaseInfrastructure.DeleteSecret(secretId).ConfigureAwait(false) is false)
            return false;
        
        _secrets.Remove(secretId);
        _dirtySecrets = true;
        return true;
    }
    
    /// <summary> Load all the secrets from the database </summary>
    private async Task<bool> LoadSecrets()
    {
        if (await databaseInfrastructure.GetSecrets().ConfigureAwait(false) is not { } secrets)
            return false;

        _secrets = secrets;
        _dirtySecrets = true;
        return true;
    }
}