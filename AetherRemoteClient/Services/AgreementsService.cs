using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using AetherRemoteClient.Domain.Enums;
using AetherRemoteClient.Infrastructure.Database;

namespace AetherRemoteClient.Services;

/// <summary>
///     Provides access to any agreements the user as agreed to since using the plugin
/// </summary>
public class AgreementsService(DatabaseInfrastructure database)
{
    private Dictionary<Agreement, bool> _agreements = [];
    private bool _dirty;
    
    /// <summary> Has the user agreed to possess </summary>
    public bool AgreedToPossession => _agreements.TryGetValue(Agreement.Possession, out var value) && value;
    
    /// <summary> List of all agreements the client has or hasn't agreed to </summary>
    public ImmutableDictionary<Agreement, bool> Agreements
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

    /// <summary> Agree to a specific agreement </summary>
    public async Task<bool> AgreeToAgreement(Agreement agreement)
    {
        if (await database.SetAgreement(agreement, true).ConfigureAwait(false) is false)
            return false;

        _agreements[agreement] = true;
        _dirty = true;
        return true;
    }
    
    /// <summary> Load all the agreements from the database </summary>
    /// <remarks> Call this once when the plugin loads initially </remarks>
    public async Task<bool> LoadAgreements()
    {
        if (await database.GetAgreements().ConfigureAwait(false) is not { } agreements)
            return false;

        _agreements = agreements;
        _dirty = true;
        return true;
    }
}