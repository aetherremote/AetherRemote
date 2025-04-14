using System.Threading.Tasks;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Ipc;

namespace AetherRemoteClient.Domain.Attributes;

/// <summary>
///     Handles storing and applying of moodles attributes
/// </summary>
public class MoodlesAttribute(MoodlesIpc moodlesIpc, nint objectAddress) : ICharacterAttribute
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
        if (await moodlesIpc.GetMoodles(objectAddress).ConfigureAwait(false) is { } moodles)
        {
            Moodles = moodles;
            return true;
        }
        
        Plugin.Log.Warning("[MoodlesAttribute] Could not retrieve moodles");
        return false;
    }

    /// <summary>
    ///     
    /// </summary>
    public async Task<bool> Apply()
    {
        // TODO: Verify if a body is actually needed or not
        if (await Plugin.RunOnFramework(() => Plugin.ObjectTable[0]?.Address).ConfigureAwait(false) is not { } address)
        {
            Plugin.Log.Warning("[MoodlesAttribute] Could not get local character address");
            return false;
        }
        
        if (await moodlesIpc.SetMoodles(address, Moodles).ConfigureAwait(false))
            return true;
        
        Plugin.Log.Warning("[MoodlesAttribute] Could not apply moodles");
        return false;
    }
}