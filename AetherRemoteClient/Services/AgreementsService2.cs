using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using AetherRemoteClient.Infrastructure.Database;

namespace AetherRemoteClient.Services;

public class AgreementsService2(DatabaseInfrastructure database)
{
    private Dictionary<string, bool> _agreements = [];
    private bool _dirty;
    
    public ImmutableDictionary<string, bool> Agreements
    {
        get
        {
            if (_dirty is false)
                return field;

            field = _agreements.ToImmutableDictionary();
            _dirty = false;

            return field;
        }
    } = [];
    
    public async Task<bool> LoadAgreements()
    {
        if (await database.GetAgreements().ConfigureAwait(false) is not { } agreements)
            return false;

        _agreements = agreements;
        _dirty = true;
        return true;
    }
}