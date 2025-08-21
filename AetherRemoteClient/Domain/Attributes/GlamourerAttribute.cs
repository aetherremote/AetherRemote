using System.Threading.Tasks;
using AetherRemoteClient.Dependencies;
using AetherRemoteClient.Domain.Enums;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Services;
using AetherRemoteClient.Utils;
using AetherRemoteCommon.Domain.Enums;
using Newtonsoft.Json.Linq;

namespace AetherRemoteClient.Domain.Attributes;

/// <summary>
///     Handles storing and applying of glamourer attributes
/// </summary>
public class GlamourerAttribute(CharacterTransformationService characterTransformationService, GlamourerDependency glamourerDependency, ushort objectIndex) : ICharacterAttribute
{
    // Instantiated
    private JObject _glamourerData = new();

    /// <summary>
    ///     <inheritdoc cref="ICharacterAttribute.Store"/>
    /// </summary>
    public async Task<bool> Store()
    {
        if (await glamourerDependency.GetDesignComponentsAsync(objectIndex).ConfigureAwait(false) is { } components)
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
        var result = await characterTransformationService.ApplyGenericTransformation(_glamourerData, GlamourerApplyFlags.All);
        if (result.Success is not ApplyGenericTransformationErrorCode.Success)
        {
            // TODO: Logging
            return false;
        }

        if (GlamourerDesignHelper.FromJObject(result.GlamourerJObject) is not { } design)
        {
            // TODO: Logging
            return false;
        }
        
        data.GlamourerDesign = design;
        data.GlamourerApplyType = GlamourerApplyFlags.All;
        return true;
    }
}