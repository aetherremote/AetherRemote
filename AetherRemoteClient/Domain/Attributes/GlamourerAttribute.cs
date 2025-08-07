using System.Threading.Tasks;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Ipc;
using AetherRemoteCommon.Domain.Enums;
using Newtonsoft.Json.Linq;

namespace AetherRemoteClient.Domain.Attributes;

/// <summary>
///     Handles storing and applying of glamourer attributes
/// </summary>
public class GlamourerAttribute(GlamourerIpc glamourerIpc, ushort objectIndex) : ICharacterAttribute
{
    // Instantiated
    private JObject _glamourerData = new();

    /// <summary>
    ///     <inheritdoc cref="ICharacterAttribute.Store"/>
    /// </summary>
    public async Task<bool> Store()
    {
        if (await glamourerIpc.GetDesignComponentsAsync(objectIndex).ConfigureAwait(false) is { } components)
        {
            _glamourerData = components;
            return true;
        }

        Plugin.Log.Warning("[GlamourerAttribute] Could not retrieve glamourer data");
        return false;
    }

    /// <summary>
    ///     <inheritdoc cref="ICharacterAttribute.Apply"/>
    /// </summary>
    public async Task<bool> Apply(PermanentTransformationData data)
    {
        // Get local character data
        if (await glamourerIpc.GetDesignComponentsAsync().ConfigureAwait(false) is not { } local)
        {
            Plugin.Log.Warning("[GlamourerAttribute] Unable to get local player design components");
            return false;
        }
        
        // Merge the required advanced dyes to reset
        GlamourerIpc.ModifyJObjectToRevertExistingAdvancedDyes(local, _glamourerData);
        
        // Apply the newly converted design
        if (await glamourerIpc.ApplyDesignAsync(_glamourerData, GlamourerApplyFlags.All) is false)
        {
            Plugin.Log.Warning("[GlamourerAttribute] Could not apply glamourer data");
            return false;
        }
        
        data.GlamourerData = _glamourerData;
        return true;
    }
}