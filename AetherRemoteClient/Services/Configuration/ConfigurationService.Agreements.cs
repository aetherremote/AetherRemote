using System.Threading.Tasks;
using AetherRemoteClient.Domain.Enums;

namespace AetherRemoteClient.Services.Configuration;

public partial class ConfigurationService
{
    /// <summary> If the user has agreed to <see cref="Agreement.Possession"/> or not </summary>
    public bool AgreedToPossession { get; private set; }
    
    /// <summary> Set an agreement to be agreed to </summary>
    public async Task<bool> AgreeTo(Agreement agreement)
    {
        if (await databaseInfrastructure.SetAgreement(agreement, true).ConfigureAwait(false) is false)
            return false;

        switch (agreement)
        {
            case Agreement.Possession:
                return true;
            
            default:
                Plugin.Log.Warning($"[ConfigurationService.AgreeTo] Unknown agreement {agreement}");
                return false;
        }
    }
    
    /// <summary> Load all agreements from the database </summary>
    private async Task<bool> LoadAgreements()
    {
        if (await databaseInfrastructure.GetAgreements().ConfigureAwait(false) is not { } agreements)
            return false;

        AgreedToPossession = agreements.TryGetValue(Agreement.Possession, out var possession) && possession;
        return true;
    }
}