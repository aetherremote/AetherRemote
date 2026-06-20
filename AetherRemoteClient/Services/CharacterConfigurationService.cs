using System.Threading.Tasks;
using AetherRemoteClient.Infrastructure.Database;
using AetherRemoteClient.Utils;

namespace AetherRemoteClient.Services;

public class CharacterConfigurationService(DatabaseInfrastructure database)
{
    public DatabaseInfrastructure.CharacterConfiguration? Current { get; private set; }

    public async Task<bool> SetSecretForCharacter(long secretId)
    {
        if (Current is null)
            return false;

        if (await database.SetCharacterConfigurationSecret(Current.Id, secretId).ConfigureAwait(false) is false)
            return false;
        
        Current.SecretId = secretId;
        return true;
    }
    
    public async Task<bool> LoadCharacterConfiguration()
    {
        if (await DalamudUtilities.TryGetLocalPlayer().ConfigureAwait(false) is not { } player)
            return false;
        
        var name = player.Name.ToString();
        var world = player.HomeWorld.Value.Name.ToString();

        if (await database.GetCharacterConfiguration(name, world).ConfigureAwait(false) is not { } configuration)
            return false;
        
        Current = configuration;
        return true;
    }
}