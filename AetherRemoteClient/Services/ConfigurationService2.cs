using System.Collections.Generic;
using System.Threading.Tasks;
using AetherRemoteClient.Infrastructure.Database;

namespace AetherRemoteClient.Services;

/// <summary>
///     Provides access to all kinds of configuration values, secrets, and more
/// </summary>
public class ConfigurationService2(DatabaseInfrastructure database)
{
    
}

/// <summary>
///     Provides access to any agreements accepted by the client
/// </summary>
public class AgreementService2(DatabaseInfrastructure database)
{
    private readonly Dictionary<string, bool> _agreements = [];

    public async Task LoadAgreements()
    {
        
    }
}