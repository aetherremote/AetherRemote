using System;
using System.Collections;
using System.Threading.Tasks;
using AetherRemoteClient.Dependencies;
using AetherRemoteClient.Domain.Interfaces;

namespace AetherRemoteClient.Domain.Attributes;

/// <summary>
///     Handles storing and applying of CustomizePlus attributes
/// </summary>
public class CustomizePlusAttribute(CustomizePlusDependency customizePlusDependency, string character) : ICharacterAttribute
{
    // Instantiated/
    private IList _customizePlusTemplates = new ArrayList();

    /// <summary>
    ///     <inheritdoc cref="ICharacterAttribute.Store"/>
    /// </summary>
    public async Task<bool> Store()
    {
        if (await Task.Run(() => customizePlusDependency.GetActiveTemplates(character)).ConfigureAwait(false) is { } templates)
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
        if (await customizePlusDependency.DeleteCustomize().ConfigureAwait(false) is false)
        {
            Plugin.Log.Warning("[CustomizePlusAttribute] Could not deleting existing profile before applying new one");
            return false;
        }

        if (await customizePlusDependency.ApplyCustomize(_customizePlusTemplates).ConfigureAwait(false) is false)
        {
            await customizePlusDependency.DeleteCustomize().ConfigureAwait(false);
            Plugin.Log.Warning("[CustomizePlusAttribute] Could not apply customize plus templates");
            return false;
        }

        try
        {
            // Convert template list
            if (await customizePlusDependency.SerializeTemplates(_customizePlusTemplates).ConfigureAwait(false) is not { } text)
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