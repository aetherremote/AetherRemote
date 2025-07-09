using System.Threading.Tasks;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Ipc;
using AetherRemoteCommon.Domain.Enums;

namespace AetherRemoteClient.Domain.Attributes;

/// <summary>
///     Handles storing and applying of glamourer attributes
/// </summary>
public class GlamourerAttribute(GlamourerIpc glamourerIpc, ushort objectIndex) : ICharacterAttribute
{
    // Instantiated
    private string _glamourerData = string.Empty;

    /// <summary>
    ///     <inheritdoc cref="ICharacterAttribute.Store"/>
    /// </summary>
    public async Task<bool> Store()
    {
        if (await glamourerIpc.GetDesignAsync(objectIndex).ConfigureAwait(false) is { } glamourerData)
        {
            _glamourerData = glamourerData;
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
        if (await glamourerIpc.ApplyDesignAsync(_glamourerData, GlamourerApplyFlags.All) is false)
        {
            Plugin.Log.Warning("[ModAttribute] Could not apply glamourer data");
            return false;
        }
        
        data.GlamourerData = _glamourerData;
        data.GlamourerApplyFlags = GlamourerApplyFlags.All;
        return true;
    }
}