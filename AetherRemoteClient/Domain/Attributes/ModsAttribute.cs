using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Ipc;

namespace AetherRemoteClient.Domain.Attributes;

/// <summary>
///     Handles storing and applying of mod attributes
/// </summary>
public class ModsAttribute(PenumbraIpc penumbraIpc, Guid collection, ushort objectIndex) : ICharacterAttribute
{
    // Instantiated
    private Dictionary<string, string> _modifiedPaths = [];
    private string _metaData = string.Empty;
    
    /// <summary>
    ///     <inheritdoc cref="ICharacterAttribute.Store"/>
    /// </summary>
    public async Task<bool> Store()
    {
        _modifiedPaths = await penumbraIpc.GetGameObjectResourcePaths(objectIndex).ConfigureAwait(false);
        _metaData = await penumbraIpc.GetMetaManipulations(objectIndex).ConfigureAwait(false);
        return true;
    }
    
    /// <summary>
    ///     <inheritdoc cref="ICharacterAttribute.Apply"/>
    /// </summary>
    public async Task<bool> Apply()
    {
        // TODO: Verify if a body is actually needed or not
        if (await penumbraIpc.AddTemporaryMod(collection, _modifiedPaths, _metaData).ConfigureAwait(false))
            return true;
        
        Plugin.Log.Warning("[ModAttribute] Could not apply mods");
        return false;
    }
}