using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AetherRemoteClient.Dependencies.Penumbra.Services;
using AetherRemoteClient.Domain.Interfaces;

namespace AetherRemoteClient.Domain.Attributes;

/// <summary>
///     Handles storing and applying of mod attributes
/// </summary>
public class ModsAttribute(PenumbraService penumbraService, Guid collection, ushort objectIndex) : ICharacterAttribute
{
    // Instantiated
    private Dictionary<string, string> _modifiedPaths = [];
    private string _metaData = string.Empty;
    
    /// <summary>
    ///     <inheritdoc cref="ICharacterAttribute.Store"/>
    /// </summary>
    public async Task<bool> Store()
    {
        _modifiedPaths = await penumbraService.GetGameObjectResourcePaths(objectIndex).ConfigureAwait(false);
        _metaData = await penumbraService.GetMetaManipulations(objectIndex).ConfigureAwait(false);
        return true;
    }
    
    /// <summary>
    ///     <inheritdoc cref="ICharacterAttribute.Apply"/>
    /// </summary>
    public async Task<bool> Apply(PermanentTransformationData data)
    {
        if (await penumbraService.AddTemporaryMod(collection, _modifiedPaths, _metaData).ConfigureAwait(false) is false)
        {
            Plugin.Log.Warning("[ModAttribute] Could not apply mods");
            return false;
        }
        
        data.ModPathData = _modifiedPaths;
        data.ModMetaData = _metaData;
        return true;
    }
}