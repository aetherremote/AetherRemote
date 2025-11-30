using System;
using System.Collections;
using System.Threading.Tasks;
using AetherRemoteClient.Dependencies.CustomizePlus.Services;
using AetherRemoteClient.Domain.Interfaces;

namespace AetherRemoteClient.Domain.Attributes;

/// <summary>
///     Handles storing and applying of CustomizePlus attributes
/// </summary>
public class CustomizePlusAttribute(CustomizePlusService customizePlusService, string character) : ICharacterAttribute
{
    // Instantiated/
    private IList _customizePlusTemplates = new ArrayList();

    /// <summary>
    ///     <inheritdoc cref="ICharacterAttribute.Store"/>
    /// </summary>
    public async Task<bool> Store()
    {
        // TODO: Update once body swapping is enabled
        
        /*
        if (await Task.Run(() => customizePlusService.GetActiveTemplates(character)).ConfigureAwait(false) is { } templates)
        {
            _customizePlusTemplates = templates;
            return true;
        }
        */

        Plugin.Log.Warning("[CustomizePlusAttribute] Could not customize templates");
        return false;
    }

    /// <summary>
    ///     <inheritdoc cref="ICharacterAttribute.Apply"/>
    /// </summary>
    public async Task<bool> Apply(PermanentTransformationData data)
    {
        // TODO: Update once body swapping is enabled
        
        /*
        if (await customizePlusService.DeleteCustomize().ConfigureAwait(false) is false)
        {
            Plugin.Log.Warning("[CustomizePlusAttribute] Could not deleting existing profile before applying new one");
            return false;
        }

        if (await customizePlusService.ApplyCustomize(_customizePlusTemplates).ConfigureAwait(false) is false)
        {
            await customizePlusService.DeleteCustomize().ConfigureAwait(false);
            Plugin.Log.Warning("[CustomizePlusAttribute] Could not apply customize plus templates");
            return false;
        }

        try
        {
            // Convert template list
            if (await customizePlusService.SerializeTemplates(_customizePlusTemplates).ConfigureAwait(false) is not { } text)
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
        */

        return true;
    }
}