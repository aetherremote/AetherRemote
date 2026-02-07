using System.Threading.Tasks;
using AetherRemoteClient.Dependencies.Honorific.Services;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Utils;
using AetherRemoteCommon.Dependencies.Honorific.Domain;

namespace AetherRemoteClient.Domain.Attributes;

// ReSharper disable RedundantBoolCompare

public class HonorificAttribute(HonorificService honorific, ushort characterIndex) : ICharacterAttribute
{
    private HonorificInfo _honorific = new();
    
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
        if (await honorific.SetCharacterTitle(_honorific).ConfigureAwait(false) is false)
        {
            Plugin.Log.Warning("[HonorificAttribute.Apply] Could not set title");
            return false;
        }

        NotificationHelper.Honorific();
        
        // TODO: Update PermanentTransformationData with Honorific
        return true;
    }
}