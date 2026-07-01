using System.Threading.Tasks;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Infrastructure.Database;
using AetherRemoteClient.Utils;

namespace AetherRemoteClient.Services;

/// <summary>
///     Provides access to the character configuration's
/// </summary>
public class CharacterConfigurationService(DatabaseInfrastructure database)
{
    /// <summary>
    ///     The currently loaded configuration
    /// </summary>
    public CharacterConfiguration? Current { get; private set; }

    /// <summary>
    ///     Sets the secret this character will be associated with
    /// </summary>
    public async Task<bool> SetSecretForCharacter(long secretId)
    {
        if (Current is null)
            return false;

        if (await database.SetCharacterSecretId(Current.Id, secretId).ConfigureAwait(false) is false)
            return false;
        
        Current.SecretId = secretId;
        return true;
    }
    
    /// <summary>
    ///     Loads the character configuration associated with this character
    /// </summary>
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