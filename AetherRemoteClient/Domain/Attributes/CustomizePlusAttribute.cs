using System;
using System.Collections;
using System.Threading.Tasks;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Ipc;

namespace AetherRemoteClient.Domain.Attributes;

/// <summary>
///     Handles storing and applying of CustomizePlus attributes
/// </summary>
public class CustomizePlusAttribute(CustomizePlusIpc customizePlusIpc, string character) : ICharacterAttribute
{
    // Instantiated/
    private IList _customizePlusTemplates = new ArrayList();

    /// <summary>
    ///     <inheritdoc cref="ICharacterAttribute.Store"/>
    /// </summary>
    public async Task<bool> Store()
    {
        if (await Task.Run(() => customizePlusIpc.GetActiveTemplates(character)).ConfigureAwait(false) is { } templates)
        {
            _customizePlusTemplates = templates;
            return true;
        }

        Plugin.Log.Warning("[CustomizePlusAttribute] Could not customize templates");
        return false;
    }

    /// <summary>
    ///     <inheritdoc cref="ICharacterAttribute.Apply"/>
    /// </summary>
    public async Task<bool> Apply(PermanentTransformationData data)
    {
        if (await customizePlusIpc.DeleteCustomize().ConfigureAwait(false) is false)
        {
            Plugin.Log.Warning("[CustomizePlusAttribute] Could not deleting existing profile before applying new one");
            return false;
        }

        if (await customizePlusIpc.ApplyCustomize(_customizePlusTemplates).ConfigureAwait(false) is false)
        {
            await customizePlusIpc.DeleteCustomize().ConfigureAwait(false);
            Plugin.Log.Warning("[CustomizePlusAttribute] Could not apply customize plus templates");
            return false;
        }

        try
        {
            // Convert template list
            if (await customizePlusIpc.SerializeTemplates(_customizePlusTemplates).ConfigureAwait(false) is not { } text)
            {
                Plugin.Log.Warning("[CustomizePlusAttribute] Could not convert templates to string");
                return false;
            }
            
            // Save to permanent transformation data
            data.CustomizePlusData = text;
            return true;
        }
        catch (Exception e)
        {
            Plugin.Log.Info($"[CustomizePlusAttribute] Could not serialize customize plus templates, {e.Message}");
            return false;
        }
    }
}