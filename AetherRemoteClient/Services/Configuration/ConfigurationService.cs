using System.Threading.Tasks;
using AetherRemoteClient.Infrastructure.Database;

namespace AetherRemoteClient.Services.Configuration;

public partial class ConfigurationService(DatabaseInfrastructure databaseInfrastructure)
{
    /// <summary>
    ///     Loads all the required files from the database for use in the plugin
    /// </summary>
    public async Task<bool> LoadRequired()
    {
        if (await LoadAgreements().ConfigureAwait(false) is false) return false;
        if (await LoadSettings().ConfigureAwait(false) is false) return false;
        if (await LoadNotes().ConfigureAwait(false) is false) return false;
        if (await LoadSecrets().ConfigureAwait(false) is false) return false;
        return true;
    }
}