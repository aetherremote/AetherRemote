using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AetherRemoteClient.Dependencies.CustomizePlus.Services;
using AetherRemoteClient.Dependencies.Glamourer.Services;
using AetherRemoteClient.Dependencies.Honorific.Domain;
using AetherRemoteClient.Dependencies.Honorific.Services;
using AetherRemoteClient.Dependencies.Moodles.Services;
using AetherRemoteClient.Dependencies.Penumbra.Services;
using AetherRemoteClient.Utils;
using AetherRemoteCommon.Domain.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Newtonsoft.Json.Linq;

// ReSharper disable InvertIf

namespace AetherRemoteClient.Managers;

public class CharacterTransformationManager(
    CustomizePlusService customizePlusService,
    GlamourerService glamourerService, 
    HonorificService honorificService, 
    MoodlesService moodlesService, 
    PenumbraService penumbraService) : IDisposable
{
    // Control how long the plugin should wait before initiating a transformation, useful for clients with high network latency
    private const int TransformationDelayInMilliseconds = 3000;

    // The collection that has the temporary mods from any body swap / twinning
    private Guid? _collectionThatHasAetherRemoteMods;

    // The status manager for the local player, used to restore the original Moodles applied to the local player if something goes wrong
    private string? _moodlesLocalPlayerStatusManager;

    /// <summary>
    ///     Applies a glamourer code to the local player
    /// </summary>
    public async Task<bool> ApplyTransformation(string glamourerCode, GlamourerApplyFlags applyFlags)
    {
        if (GlamourerService.ConvertGlamourerBase64StringToJObject(glamourerCode) is not { } glamourerCodeAsComponents)
        {
            Plugin.Log.Error("[CharacterTransformationManager.ApplyTransformation] Could not deserialize glamourer code. If you see this please contact the developer!");
            return false;
        }
        
        return await ApplyTransformation(glamourerCodeAsComponents, applyFlags).ConfigureAwait(false);
    }
    
    /// <summary>
    ///     Applies glamourer components to the local player
    /// </summary>
    public async Task<bool> ApplyTransformation(JObject glamourerJObject, GlamourerApplyFlags applyFlags)
    {
        if (await DalamudUtilities.TryGetLocalPlayer().ConfigureAwait(false) is not { } localPlayer)
        {
            Plugin.Log.Error("[CharacterTransformationManager.ApplyTransformation] Could not get the local player");
            return false;
        }

        if (await glamourerService.GetDesignComponentsAsync(localPlayer.ObjectIndex).ConfigureAwait(false) is not { } glamourerDesignComponents)
        {
            Plugin.Log.Error("[CharacterTransformationManager.ApplyTransformation] Could not get glamourer design components");
            return false;
        }
        
        if (GlamourerService.SanitizeGlamourerAdvancedDyes(glamourerDesignComponents, glamourerJObject) is not { } glamourerDesignComponentsSanitized)
        {
            Plugin.Log.Error("[CharacterTransformationManager.ApplyTransformation] Could not properly sanitize advanced dyes");
            return false;
        }
        
        return await glamourerService.ApplyDesignAsync(glamourerDesignComponentsSanitized, applyFlags, localPlayer.ObjectIndex).ConfigureAwait(false);
    }
    
    /// <summary>
    ///     Transforms the local character into target character
    /// </summary>
    public async Task<bool> ApplyFullScaleTransformation(string characterName, string characterWorld, CharacterAttributes characterAttributes)
    {
        if (await TryRemoveExistingMods().ConfigureAwait(false) is false)
        {
            Plugin.Log.Error("[CharacterTransformationManager.ApplyFullScaleTransformation] Could not remove existing mods");
            return false;
        }
        
        if (await DalamudUtilities.TryGetPlayerFromObjectTable(characterName, characterWorld).ConfigureAwait(false) is not { } targetPlayerObject)
        {
            Plugin.Log.Error("[CharacterTransformationManager.ApplyFullScaleTransformation] Could not find target player");
            return false;
        }

        if (await StoreCharacterAttributes(characterName, characterWorld, targetPlayerObject, characterAttributes).ConfigureAwait(false) is not { } storedAttributes)
        {
            Plugin.Log.Error("[CharacterTransformationManager.ApplyFullScaleTransformation] Failed to store character attributes");
            return false;
        }
        
        // In the case of body swapping, there needs to be a delay so other people can get a snapshot our character before we transform
        await Task.Delay(TransformationDelayInMilliseconds).ConfigureAwait(false);
        
        // Apply those stored changed back
        if (await ApplyStoredAttributes(characterAttributes, storedAttributes).ConfigureAwait(false) is false)
        {
            Plugin.Log.Error("[CharacterTransformationManager.ApplyFullScaleTransformation] Failed to apply character attributes");
            return false;
        }

        return true;
    }

    /// <summary>
    ///     Returns the collection that has modified Aether Remote mods in it, or null if it has not been set
    /// </summary>
    public Guid? TryGetCollectionThatHasAetherRemoteMods()
    {
        return _collectionThatHasAetherRemoteMods;
    }
    
    /// <summary>
    ///     Store all attributes of a given character to be applied
    /// </summary>
    private async Task<StoredAttributes?> StoreCharacterAttributes(string characterName, string characterWorld, IGameObject playerObject, CharacterAttributes characterAttributes)
    {
        // Get a snapshot of what our target looks like now so it can be applied later
        var storedAttributes = new StoredAttributes();

        // Save glamourer data if either are set
        if ((characterAttributes & (CharacterAttributes.GlamourerCustomization | CharacterAttributes.GlamourerEquipment)) is not CharacterAttributes.None)
        {
            // This is an error state because we should have received them
            if (await glamourerService.GetDesignComponentsAsync(playerObject.ObjectIndex).ConfigureAwait(false) is not { } glamourerDesignComponents)
            {
                Plugin.Log.Error("[CharacterTransformationManager.StoreCharacterAttributes] Unable to store glamourer data");
                return null;
            }
                
            // Set the attribute
            storedAttributes.GlamourerDesignComponents = glamourerDesignComponents;   
        }
        
        // Save penumbra data
        if ((characterAttributes & CharacterAttributes.PenumbraMods) is CharacterAttributes.PenumbraMods)
        {
            // TODO: Should these fail too?
            storedAttributes.PenumbraModifiedPaths = await penumbraService.GetGameObjectResourcePaths(playerObject.ObjectIndex).ConfigureAwait(false);
            storedAttributes.PenumbraMetaManipulations = await penumbraService.GetMetaManipulations(playerObject.ObjectIndex).ConfigureAwait(false);
        }
        
        // Save honorific
        if ((characterAttributes & CharacterAttributes.Honorific) is CharacterAttributes.Honorific)
        {
            // This is an error state because we should have received them
            if (await honorificService.GetCharacterTitle(playerObject.ObjectIndex).ConfigureAwait(false) is not { } honorific)
            {
                Plugin.Log.Error("[CharacterTransformationManager.StoreCharacterAttributes] Unable to store honorific data");
                return null;
            }
            
            // Set the attribute
            storedAttributes.Honorific = honorific;
        }
        
        // Save Moodles
        if ((characterAttributes & CharacterAttributes.Moodles) is CharacterAttributes.Moodles)
        {
            // This is an error state because we should have received them
            if (await moodlesService.GetStatusManager(playerObject.Address).ConfigureAwait(false) is not { } moodles)
            {
                Plugin.Log.Error("[CharacterTransformationManager.StoreCharacterAttributes] Unable to store moodles data");
                return null;
            }
            
            // Set the attribute
            storedAttributes.Moodles = moodles;
            
            // We'll try to save the local player's Moodles too just in case
            if (await DalamudUtilities.TryGetLocalPlayer().ConfigureAwait(false) is { } localPlayer)
                _moodlesLocalPlayerStatusManager = await moodlesService.GetStatusManager(localPlayer.Address).ConfigureAwait(false);
        }
        
        // Save Customize+
        if ((characterAttributes & CharacterAttributes.CustomizePlus) is CharacterAttributes.CustomizePlus)
        {
            // This is an error state because we should have received them
            if (await customizePlusService.TryGetActiveProfileOnCharacter(characterName, characterWorld).ConfigureAwait(false) is not { } profile)
            {
                Plugin.Log.Error("[CharacterTransformationManager.StoreCharacterAttributes] Unable to store customize+ data");
                return null;
            }
            
            storedAttributes.CustomizePlusTemplate = profile;
        }
        
        // All attributes have been stored, this can be returned for use in application or permanent transformations
        return storedAttributes;
    }
    
    /// <summary>
    ///     Apply all attributes of a given character to the local character
    /// </summary>
    private async Task<bool> ApplyStoredAttributes(CharacterAttributes characterAttributes, StoredAttributes storedAttributes)
    {
        // Get the local player we will be applying these things to
        if (await DalamudUtilities.TryGetLocalPlayer().ConfigureAwait(false) is not { } localPlayer)
        {
            Plugin.Log.Error("[CharacterTransformationManager.ApplyStoredAttributes] Could not find local player");
            await TryRevert().ConfigureAwait(false);
            return false;
        }
        
        // Apply the Customize+ data if it exists
        if (storedAttributes.CustomizePlusTemplate is { } customizePlusTemplate)
        {
            // Since we're dealing with reflection, no harm in doing another delete first
            if (await customizePlusService.DeleteTemporaryCustomizeAsync().ConfigureAwait(false) is false)
            {
                Plugin.Log.Error("[CharacterTransformationManager.ApplyStoredAttributes] Unable to delete existing customize+ data");
                await TryRevert().ConfigureAwait(false);
            }

            // If the string was empty, that means we just didn't find a C+ profile on them, but we still can't deserialize it, so we set it to null
            var final = customizePlusTemplate == string.Empty ? null : customizePlusTemplate;
            if (await customizePlusService.ApplyCustomizeAsync(final).ConfigureAwait(false) is false)
            {
                Plugin.Log.Error("[CharacterTransformationManager.ApplyStoredAttributes] Unable to apply customize+ data");
                await TryRevert().ConfigureAwait(false);
                return false;
            }
        }

        // Apply the Honorific data if it exists
        if (storedAttributes.Honorific is { } honorific)
        {
            if (await honorificService.SetCharacterTitle(honorific).ConfigureAwait(false) is false)
            {
                Plugin.Log.Error("[CharacterTransformationManager.ApplyStoredAttributes] Unable to apply honorific data");
                await TryRevert().ConfigureAwait(false);
                return false;
            }
        }

        // Apply the Moodles data if it exists
        if (storedAttributes.Moodles is { } moodles)
        {
            if (await moodlesService.SetStatusManager(localPlayer.Address, moodles).ConfigureAwait(false) is false)
            {
                Plugin.Log.Error("[CharacterTransformationManager.ApplyStoredAttributes] Unable to apply moodles data");
                await TryRevert().ConfigureAwait(false);
                return false;
            }
        }
        
        // Apply the Penumbra data if it exists (apparently this is a pattern? Rider knows best I guess...)
        if (storedAttributes is { PenumbraModifiedPaths: { } penumbraModifiedPaths, PenumbraMetaManipulations: { } penumbraMetaManipulations })
        {
            // Get the currently active collection guid
            var guid = await penumbraService.GetCollection().ConfigureAwait(false);
            if (await penumbraService.AddTemporaryMod(guid, penumbraModifiedPaths, penumbraMetaManipulations).ConfigureAwait(false) is false)
            {
                Plugin.Log.Error("[CharacterTransformationManager.ApplyStoredAttributes] Unable to apply penumbra data");
                await TryRevert().ConfigureAwait(false);
                return false;
            }

            // We need to store the last collection guid we applied mods to
            _collectionThatHasAetherRemoteMods = guid;
        }
        
        // Wait a short second to make sure everything applies and propagates
        await Task.Delay(100).ConfigureAwait(false);
        
        // Apply the Glamourer data if it exists
        if (storedAttributes.GlamourerDesignComponents is { } glamourerDesignComponents)
        {
            var applyFlags = ExtractApplyFlagsFromCharacterAttributes(characterAttributes);
            if (await glamourerService.ApplyDesignAsync(glamourerDesignComponents, applyFlags, localPlayer.ObjectIndex).ConfigureAwait(false) is false)
            {
                Plugin.Log.Error("[CharacterTransformationManager.ApplyStoredAttributes] Unable to apply glamourer data");
                await TryRevert().ConfigureAwait(false);
                return false;
            }
        }

        return true;
    }

    /// <summary>
    ///     Reverts the local character back to their default, to be used as a fall-back if something goes wrong
    /// </summary>
    private async Task TryRevert()
    {
        honorificService.ClearCharacterTitle();
        await TryRemoveExistingMods().ConfigureAwait(false);
        await customizePlusService.DeleteTemporaryCustomizeAsync().ConfigureAwait(false);
        
        if (_collectionThatHasAetherRemoteMods is not null)
            await penumbraService.RemoveTemporaryMod(_collectionThatHasAetherRemoteMods.Value).ConfigureAwait(false);

        if (await DalamudUtilities.TryGetLocalPlayer().ConfigureAwait(false) is { } localPlayer)
        {
            if (_moodlesLocalPlayerStatusManager is not null)
                await moodlesService.SetStatusManager(localPlayer.Address, _moodlesLocalPlayerStatusManager).ConfigureAwait(false);
            
            await glamourerService.RevertToAutomation(localPlayer.ObjectIndex).ConfigureAwait(false);
        }

        _collectionThatHasAetherRemoteMods = null;
        _moodlesLocalPlayerStatusManager = null;
    }

    /// <summary>
    ///     Light wrapper to help remove existing mods before doing something new
    /// </summary>
    private async Task<bool> TryRemoveExistingMods()
    {
        // Exit gracefully if there are no mods to remove
        if (_collectionThatHasAetherRemoteMods is null)
            return true;

        // Try to remove the temporary mods from the stored collection
        return await penumbraService.RemoveTemporaryMod(_collectionThatHasAetherRemoteMods.Value).ConfigureAwait(false);
    }

    /// <summary>
    ///     Wrapper to pull out the apply flags from the character attribute
    /// </summary>
    private static GlamourerApplyFlags ExtractApplyFlagsFromCharacterAttributes(CharacterAttributes characterAttributes)
    {
        var applyFlags = GlamourerApplyFlags.Once;
        if ((characterAttributes & CharacterAttributes.GlamourerCustomization) is CharacterAttributes.GlamourerCustomization)
            applyFlags |= GlamourerApplyFlags.Customization;
        
        if ((characterAttributes & CharacterAttributes.GlamourerEquipment) is CharacterAttributes.GlamourerEquipment)
            applyFlags |= GlamourerApplyFlags.Equipment;

        return applyFlags;
    }
    
    private class StoredAttributes
    {
        public JObject? GlamourerDesignComponents;
        public Dictionary<string, string>? PenumbraModifiedPaths;
        public string? PenumbraMetaManipulations;
        public HonorificCustomTitle? Honorific;
        public string? Moodles;
        public string? CustomizePlusTemplate;
    }

    public void Dispose()
    {
        if (_collectionThatHasAetherRemoteMods is not null)
            penumbraService.RemoveTemporaryMod(_collectionThatHasAetherRemoteMods.Value).ConfigureAwait(false);
        
        GC.SuppressFinalize(this);
    }
}