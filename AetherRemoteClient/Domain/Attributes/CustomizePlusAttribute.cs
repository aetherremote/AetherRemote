using System.Threading.Tasks;
using AetherRemoteClient.Dependencies.CustomizePlus.Services;
using AetherRemoteClient.Domain.Interfaces;

namespace AetherRemoteClient.Domain.Attributes;

/// <summary>
///     Handles storing and applying of CustomizePlus attributes
/// </summary>
public class CustomizePlusAttribute(CustomizePlusService customize, string character) : ICharacterAttribute
{
    // Instantiated/
    private string _customizeTemplateJson = string.Empty;

    /// <summary>
    ///     <inheritdoc cref="ICharacterAttribute.Store"/>
    /// </summary>
    public async Task<bool> Store()
    {
        if (await customize.TryGetActiveProfileOnCharacter(character).ConfigureAwait(false) is not { } json)
        {
            Plugin.Log.Warning("[CustomizePlusAttribute.Apply] Could not get customize templates");
            return false;
        }
        
        _customizeTemplateJson = json;
        return true;
    }

    /// <summary>
    ///     <inheritdoc cref="ICharacterAttribute.Apply"/>
    /// </summary>
    public async Task<bool> Apply(PermanentTransformationData data)
    {
        if (await customize.DeleteTemporaryCustomizeAsync().ConfigureAwait(false) is false)
        {
            Plugin.Log.Warning("[CustomizePlusAttribute.Apply] Could not deleting existing profile before applying new one");
            return false;
        }

        return _customizeTemplateJson == string.Empty
            ? await customize.ApplyCustomizeAsync().ConfigureAwait(false)
            : await customize.ApplyCustomizeAsync(_customizeTemplateJson).ConfigureAwait(false);
    }
}