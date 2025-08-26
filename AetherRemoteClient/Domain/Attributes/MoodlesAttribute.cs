using System.Threading.Tasks;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Services.Dependencies;

namespace AetherRemoteClient.Domain.Attributes;

/// <summary>
///     Handles storing and applying of moodles attributes
/// </summary>
public class MoodlesAttribute(MoodlesService moodlesService, nint objectAddress) : ICharacterAttribute
{
    /// <summary>
    ///     Moodles saved while storing
    /// </summary>
    public string Moodles = string.Empty;
    
    /// <summary>
    ///     <inheritdoc cref="ICharacterAttribute.Store"/>
    /// </summary>
    public async Task<bool> Store()
    {
        if (await moodlesService.GetMoodles(objectAddress).ConfigureAwait(false) is { } moodles)
        {
            Moodles = moodles;
            return true;
        }
        
        Plugin.Log.Warning("[MoodlesAttribute] Could not retrieve moodles");
        return false;
    }

    /// <summary>
    ///     <inheritdoc cref="ICharacterAttribute.Apply"/>
    /// </summary>
    public async Task<bool> Apply(PermanentTransformationData data)
    {
        if (await Plugin.RunOnFramework(() => Plugin.ObjectTable[0]?.Address).ConfigureAwait(false) is not { } address)
        {
            Plugin.Log.Warning("[MoodlesAttribute] Could not get local character address");
            return false;
        }

        if (await moodlesService.SetMoodles(address, Moodles).ConfigureAwait(false) is false)
        {
            Plugin.Log.Warning("[MoodlesAttribute] Could not apply moodles");
            return false;
        }

        data.MoodlesData = Moodles;
        return true;
    }
}