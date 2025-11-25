using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AetherRemoteClient.Dependencies.Glamourer.Services;
using AetherRemoteClient.Dependencies.Moodles.Services;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Attributes;
using AetherRemoteClient.Domain.Dependencies.Glamourer;
using AetherRemoteClient.Domain.Enums;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Services.Dependencies;
using AetherRemoteClient.Utils;
using AetherRemoteCommon.Domain.Enums;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Newtonsoft.Json.Linq;

namespace AetherRemoteClient.Managers;

/// <summary>
///     Manages the application of transformations to the local player
/// </summary>
public class CharacterTransformationManager(
    CustomizePlusService customizePlusService, 
    GlamourerService glamourerService, 
    MoodlesService moodlesService, 
    PenumbraService penumbraService)
{
    // Control how long the plugin should wait before initiating a transformation, useful for clients with high network latency
    private const int TransformationDelayInMilliseconds = 3000;
    
    /// <summary>
    ///     Applies a glamourer code to the local player
    /// </summary> 
    public async Task<ApplyGenericTransformationResult> ApplyGenericTransformation(string glamourerCode, GlamourerApplyFlags flags)
    {
        // Convert to JObject
        if (GlamourerService.ConvertGlamourerBase64StringToJObject(glamourerCode) is not { } glamourerCodeAsComponents)
            return new ApplyGenericTransformationResult(ApplyGenericTransformationErrorCode.FailedBase64Conversion, null);
        
        return await ApplyGenericTransformation(glamourerCodeAsComponents, flags).ConfigureAwait(false);
    }

    /// <summary>
    ///     <inheritdoc cref="ApplyGenericTransformation(string, GlamourerApplyFlags)"/>
    /// </summary>
    public async Task<ApplyGenericTransformationResult> ApplyGenericTransformation(JObject glamourerJObject, GlamourerApplyFlags flags)
    {
        // Get local character data
        if (await glamourerService.GetDesignComponentsAsync(0).ConfigureAwait(false) is not { } local)
            return new ApplyGenericTransformationResult(ApplyGenericTransformationErrorCode.FailedToGetDesign, null);
        
        // Append any details to the converted JObject to clean up the dyes
        if (GlamourerService.CreateJObjectToRevertExistingAdvancedDyes(local, glamourerJObject) is not { } glamourerCodeAsComponentsWithoutAdvancedDyes)
            return new ApplyGenericTransformationResult(ApplyGenericTransformationErrorCode.FailedToRemoveAdvancedDyes, null);
        
        // Apply the newly converted design
        return await glamourerService.ApplyDesignAsync(glamourerCodeAsComponentsWithoutAdvancedDyes, flags, 0).ConfigureAwait(false) 
            ? new ApplyGenericTransformationResult(ApplyGenericTransformationErrorCode.Success, glamourerJObject)
            : new ApplyGenericTransformationResult(ApplyGenericTransformationErrorCode.FailedToApplyDesign, null);
    }
    
    /// <summary>
    ///     Applies another character to the local player
    /// </summary>
    /// <param name="characterName">The character to transform into</param>
    /// <param name="characterAttributes">The attributes of the character we want to transform into</param>
    public async Task<ApplyCharacterTransformationResult> ApplyCharacterTransformation(string characterName, CharacterAttributes characterAttributes)
    {
        // Try to remove the existing mods on the current collection
        if (await TryRemoveExistingMods().ConfigureAwait(false) is not { } collection)
            return new ApplyCharacterTransformationResult(ApplyCharacterTransformationErrorCode.FailedToClearExistingMods, null);
        
        // Try to get the target player to transform into from the object table
        if (await TryGetPlayerFromObjectTable(characterName).ConfigureAwait(false) is not { } gameObject)
            return new ApplyCharacterTransformationResult(ApplyCharacterTransformationErrorCode.FailedToFindCharacter, null);
        
        // Try to store all the character data we will use in this transformation
        if (await TryGetPlayerAttributes(characterAttributes, gameObject, collection).ConfigureAwait(false) is not { } attributes)
            return new ApplyCharacterTransformationResult(ApplyCharacterTransformationErrorCode.FailedToStoreAttributes, null);
        
        // Await a moment for other clients to get our local client's data
        await Task.Delay(TransformationDelayInMilliseconds).ConfigureAwait(false);
        
        // Ready an object to store all the transformation data
        var permanent = new PermanentTransformationData();
        
        // Iterate over all the attributes and try to apply them one by one
        foreach(var attribute in attributes)
            if (await attribute.Apply(permanent).ConfigureAwait(false) is false)
                return new ApplyCharacterTransformationResult(ApplyCharacterTransformationErrorCode.FailedToApplyAttributes, null);
        
        // Return success with the transformation data
        return new ApplyCharacterTransformationResult(ApplyCharacterTransformationErrorCode.Success, permanent);
    }
    
    public async Task ApplyPerm(PermanentTransformationData permanentTransformationData)
    {
        // Try to remove the existing mods on the current collection
        if (await TryRemoveExistingMods().ConfigureAwait(false) is not { } collection)
            return;
        
        // Get local character data
        if (await glamourerService.GetDesignComponentsAsync(0).ConfigureAwait(false) is not { } localDesignJObject)
            return;

        // Convert to a glamourer design
        if (GlamourerDesignHelper.FromJObject(localDesignJObject) is not { } localDesign)
            return;
        
        // Get a list of the materials to revert
        var designWithAdvancedDyesToRevert = AppendAdvanceDyesToRevertToNewGlamourerDesign(localDesign, permanentTransformationData.GlamourerDesign);
        
        // Convert back to JObject
        var convertedDesign = GlamourerDesignHelper.ToJObject(designWithAdvancedDyesToRevert);
        
        // Plugin.Log.Info($"Applying: {convertedDesign}");
        ImGui.SetClipboardText(convertedDesign.ToString());
        
        // Apply Glamourer
        await glamourerService.ApplyDesignAsync(convertedDesign, permanentTransformationData.GlamourerApplyType, 0).ConfigureAwait(false);

        // Apply Mods
        if (permanentTransformationData.ModMetaData is not null && permanentTransformationData.ModPathData is not null)
            await penumbraService.AddTemporaryMod(collection, permanentTransformationData.ModPathData, permanentTransformationData.ModMetaData).ConfigureAwait(false);

        // Apply Customize
        if (permanentTransformationData.CustomizePlusData is not null)
            await customizePlusService.ApplyCustomize(permanentTransformationData.CustomizePlusData).ConfigureAwait(false);

        // Apply Moodles
        if (permanentTransformationData.MoodlesData is not null)
            if (await Plugin.RunOnFramework(() => Plugin.ObjectTable[0]?.Address).ConfigureAwait(false) is { } address)
            {
                // TODO: Readd
                //await moodlesService.SetMoodles(address, permanentTransformationData.MoodlesData).ConfigureAwait(false);
            }
    }

    private async Task<Guid?> TryRemoveExistingMods()
    {
        // Get Current Collection
        var collection = await penumbraService.GetCollection().ConfigureAwait(false);

        // If the collection guid is the empty guid return
        if (collection == Guid.Empty)
            return null;
        
        // Remove Existing Temp Mods
        if (await penumbraService.CallRemoveTemporaryMod(collection).ConfigureAwait(false) is false)
            return null;
        
        // Return the collection
        return collection;
    }

    private static async Task<IGameObject?> TryGetPlayerFromObjectTable(string characterName)
    {
        try
        {
            // Get a game object for target player in object table
            var gameObject = await Plugin.RunOnFramework(() =>
            {
                // Iterate through the object table
                for (ushort i = 0; i < Plugin.ObjectTable.Length; i++)
                {
                    // Continue to the next object if the current is null
                    if (Plugin.ObjectTable[i] is not { } gameObject)
                        continue;

                    // If the object is a player and the name of it is our character's name, return it
                    if (gameObject.ObjectKind is ObjectKind.Player && gameObject.Name.TextValue == characterName)
                        return Plugin.ObjectTable[i];
                }

                // No objects found that matched
                return null;
            }).ConfigureAwait(false);

            // If the object was not found in the table, exit
            if (gameObject is null)
                Plugin.Log.Warning($"[CharacterTransformationManager] [TryGetPlayerFromObjectTable] Unable to find {characterName} in object table");
            
            // Return the result
            return gameObject;
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[CharacterTransformationManager] [TryGetPlayerFromObjectTable] Encountered an unexpected error {e}");
            return null;
        }
    }

    // TODO: Refactor to removing Attributes
    private async Task<List<ICharacterAttribute>?> TryGetPlayerAttributes(CharacterAttributes characterAttributes, IGameObject gameObject, Guid collection)
    {
        // Create a new attribute list
        var attributes = new List<ICharacterAttribute>();
        
        // Store Glamourer always
        var glamourerAttribute = new GlamourerAttribute(this, glamourerService, gameObject.ObjectIndex);
        if (await glamourerAttribute.Store().ConfigureAwait(false) is false)
            return null;
        
        // Add glamourer attribute
        attributes.Add(glamourerAttribute);

        // Check if mods are one of the attributes to store
        if ((characterAttributes & CharacterAttributes.Mods) is CharacterAttributes.Mods)
        {
            // Create mod attribute
            var modsAttribute = new ModsAttribute(penumbraService, collection, gameObject.ObjectIndex);
            if (await modsAttribute.Store().ConfigureAwait(false) is false)
                return null;
            
            // Add mods attribute
            attributes.Add(modsAttribute);
        }

        // Check if moodles are one of the attributes to store
        if ((characterAttributes & CharacterAttributes.Moodles) is CharacterAttributes.Moodles)
        {
            // Create Moodles attribute
            var moodlesAttribute = new MoodlesAttribute(moodlesService, gameObject.Address);
            if (await moodlesAttribute.Store().ConfigureAwait(false) is false)
                return null;
            
            // Add Moodles attribute
            attributes.Add(moodlesAttribute);
        }

        // Check if CustomizePlus is one of the attributes to store
        if ((characterAttributes & CharacterAttributes.CustomizePlus) is CharacterAttributes.CustomizePlus)
        {
            // Store CustomizePlus attribute
            var customizePlusAttribute = new CustomizePlusAttribute(customizePlusService, gameObject.Name.TextValue);
            if (await customizePlusAttribute.Store().ConfigureAwait(false) is false)
                return null;
            
            // Add CustomizePlus attribute
            attributes.Add(customizePlusAttribute);
        }
        
        // Return attributes
        return attributes;
    }

    private static GlamourerDesign AppendAdvanceDyesToRevertToNewGlamourerDesign(GlamourerDesign localDesign, GlamourerDesign targetDesign)
    {
        // Clone the target design
        var finalDesign = targetDesign.Clone();
        
        // Iterate over all the materials on the local design
        foreach (var material in localDesign.Materials)
        {
            // Check to see if this material affects a piece of equipment in the permanent transformation
            var slot = GlamourerDesignHelper.ToEquipmentSlot(material.Key);
            if (AffectsEquipmentSlot(slot, finalDesign.Equipment) is false)
                continue;
            
            // Check to see if the material is already present in the permanent transformation, and ignore it if is since it will be overwritten anyway
            if (finalDesign.Materials.ContainsKey(material.Key))
                continue;

            // Copy the material
            var clone = material.Value.Clone();
            
            // Mark it to revert when applied
            clone.Revert = true;
            
            // Add to the final design
            finalDesign.Materials.Add(material.Key, clone);
        }

        // Return everything modified
        return finalDesign;
    }

    private static bool AffectsEquipmentSlot(GlamourerEquipmentSlot slot, GlamourerEquipment equipment)
    {
        return slot switch
        {
            GlamourerEquipmentSlot.None => false,
            GlamourerEquipmentSlot.Head => equipment.Head.Apply,
            GlamourerEquipmentSlot.Body => equipment.Body.Apply,
            GlamourerEquipmentSlot.Hands => equipment.Hands.Apply,
            GlamourerEquipmentSlot.Legs => equipment.Legs.Apply,
            GlamourerEquipmentSlot.Feet => equipment.Feet.Apply,
            GlamourerEquipmentSlot.Ears => equipment.Ears.Apply,
            GlamourerEquipmentSlot.Neck => equipment.Neck.Apply,
            GlamourerEquipmentSlot.Wrists => equipment.Wrists.Apply,
            GlamourerEquipmentSlot.RFinger => equipment.RFinger.Apply,
            GlamourerEquipmentSlot.LFinger => equipment.LFinger.Apply,
            _ => false
        };
    }
}