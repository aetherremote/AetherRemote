using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using AetherRemoteClient.Infrastructure.Database;

namespace AetherRemoteClient.Services;

public class SecretsService(DatabaseInfrastructure database)
{
    private Dictionary<long, DatabaseInfrastructure.Secret> _secrets = [];
    private bool _dirty;
    
    public ImmutableDictionary<long, DatabaseInfrastructure.Secret> Secrets
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
    
    public async Task<bool> LoadSecrets()
    {
        if (await database.GetSecrets().ConfigureAwait(false) is not { } secrets)
            return false;

        _secrets = secrets;
        _dirty = true;
        return true;
    }

    public async Task<bool> AddSecret(string secretName, string secretValue)
    {
        if (await database.AddSecret(secretName, secretValue).ConfigureAwait(false) is not { } secret)
            return false;
        
        _secrets.Add(secret.Id, secret);
        _dirty = true;
        return true;
    }
    
    public async Task<bool> RemoveSecret(long secretId)
    {
        if (await database.RemoveSecret(secretId).ConfigureAwait(false) is false)
            return false;
        
        _secrets.Remove(secretId);
        _dirty = true;
        return true;
    }

    public async Task<int> CountUsage(long secretId) => await database.GetCharacterUsingSecretCount(secretId).ConfigureAwait(false);
}