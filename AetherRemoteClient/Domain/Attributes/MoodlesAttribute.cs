using System.Threading.Tasks;
using AetherRemoteClient.Dependencies;
using AetherRemoteClient.Domain.Interfaces;

namespace AetherRemoteClient.Domain.Attributes;

/// <summary>
///     Handles storing and applying of moodles attributes
/// </summary>
public class MoodlesAttribute(MoodlesDependency moodlesDependency, nint objectAddress) : ICharacterAttribute
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
        if (await moodlesDependency.GetMoodles(objectAddress).ConfigureAwait(false) is { } moodles)
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

        if (await moodlesDependency.SetMoodles(address, Moodles).ConfigureAwait(false) is false)
        {
            Plugin.Log.Warning("[MoodlesAttribute] Could not apply moodles");
            return false;
        }

        data.MoodlesData = Moodles;
        return true;
    }
}