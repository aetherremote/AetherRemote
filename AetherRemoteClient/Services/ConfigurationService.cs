using System.Threading.Tasks;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Infrastructure;

namespace AetherRemoteClient.Services;

/// <summary>
///     Configuration values for various aspects of the plugin
/// </summary>
public class ConfigurationService(DatabaseInfrastructure databaseInfrastructure)
{
    public CharacterConfiguration Configuration { get; private set; } = new();

    public async Task Load(string characterName, string characterWorld)
    {
        Configuration = await databaseInfrastructure.GetConfigurationForPlayer(characterName, characterWorld);
    }
    
    public async Task<bool> Save()
    {
        return await databaseInfrastructure.SaveCharacterConfigurationForPlayer(Configuration);
    }
}