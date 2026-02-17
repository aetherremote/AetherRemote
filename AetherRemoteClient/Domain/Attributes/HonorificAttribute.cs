using System.Threading.Tasks;
using AetherRemoteClient.Dependencies.Honorific.Domain;
using AetherRemoteClient.Dependencies.Honorific.Services;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Utils;

namespace AetherRemoteClient.Domain.Attributes;

public class HonorificAttribute(HonorificService honorific, ushort characterIndex) : ICharacterAttribute
{
    private HonorificCustomTitle? _honorific;
    
    public async Task<bool> Store()
    {
        if (await honorific.GetCharacterTitle(characterIndex).ConfigureAwait(false) is not { } json)
        {
            Plugin.Log.Warning("[HonorificAttribute.Store] Could not get character's title");
            return false;
        }
        
        _honorific = json;
        return true;
    }

    public async Task<bool> Apply(PermanentTransformationData data)
    {
        if (_honorific is null)
            return false;
        
        if (await honorific.SetCharacterTitle(_honorific).ConfigureAwait(false) is false)
        {
            Plugin.Log.Warning("[HonorificAttribute.Apply] Could not set title");
            return false;
        }

        NotificationHelper.Honorific();
        return true;
    }
}