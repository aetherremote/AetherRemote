using System.Threading.Tasks;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Dependencies.Glamourer;
using AetherRemoteClient.Domain.Enums;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteClient.Services.Dependencies;
using AetherRemoteClient.Utils;
using AetherRemoteCommon.Domain.Enums;

namespace AetherRemoteClient.Handlers;

/// <summary>
///     Manages aspects of keeping a character in a permanently transformed state
/// </summary>
public class PermanentTransformationHandler(
    CharacterTransformationManager characterTransformationManager,
    PermanentTransformationLockService permanentTransformationLockService,
    GlamourerService glamourerService)
{
    // Instantiated
    private PermanentTransformationData? _permanentTransformationData;

    /// <summary>
    ///     <inheritdoc cref="PermanentTransformationLockService.Locked"/>
    /// </summary>
    public bool IsPermanentTransformed => permanentTransformationLockService.Locked;
    
    /// <summary>
    ///     Load a permanent transformation from the stored configuration
    /// </summary>
    public async Task Load(string characterName, string characterWorld)
    {
        var perma = new PermanentTransformationData();
        // if (await databaseService.GetPermanentTransformationForPlayer(characterName, characterWorld) is not { } permanentTransformationData)
        // {
        //     Plugin.Log.Verbose($"[PermanentTransformationManager] No permanent transformation found for player {characterName} - {characterWorld}");
        //    return;
        // }

        // TODO: Error handle
        await characterTransformationManager.ApplyPerm(perma);//
        
        
        permanentTransformationLockService.Lock(perma.Key);
        
        _permanentTransformationData = perma;
    }

    /// <summary>
    ///     Permanently apply a character transformation to the local player
    /// </summary>
    public async Task<bool> ApplyPermanentCharacterTransformation(string sender, string key, string characterName, CharacterAttributes characterAttributes)
    {
        if (permanentTransformationLockService.Locked)
        {
            Plugin.Log.Warning("[PermanentTransformationManager] Cannot apply permanent character transformation because local player is already locked");
            return false;
        }
        
        var result = await characterTransformationManager.ApplyCharacterTransformation(characterName, characterAttributes);
        if (result.Success is not ApplyCharacterTransformationErrorCode.Success)
        {
            Plugin.Log.Error($"[PermanentTransformationManager] Unable to apply character transformation of {characterName}");
            return false;
        }

        if (result.Data is not { } permanentTransformationData)
        {
            Plugin.Log.Error("[PermanentTransformationManager] Expected data was not present after character transformation");
            return false;
        }

        permanentTransformationLockService.Lock(permanentTransformationData.Key);
        
        _permanentTransformationData = permanentTransformationData;
        _permanentTransformationData.Sender = sender;
        _permanentTransformationData.Key = key;
        
        return true;
    }

    public async Task<bool> ApplyPermanentTransformation(string sender, string key, string design, GlamourerApplyFlags glamourerApplyFlags)
    {
        if (permanentTransformationLockService.Locked)
        {
            Plugin.Log.Warning("[PermanentTransformationManager] Cannot apply permanent character transformation because local player is already locked");
            return false;
        }

        var result = await characterTransformationManager.ApplyGenericTransformation(design, glamourerApplyFlags);
        if (result.Success is not ApplyGenericTransformationErrorCode.Success)
        {
            Plugin.Log.Error("[PermanentTransformationManager] Unable to apply generic transformation");
            return false;
        }

        if (result.GlamourerJObject is not { } glamourerJObject)
        {
            Plugin.Log.Error("[PermanentTransformationManager] Expected data was not present after character transformation");
            return false;
        }

        if (GlamourerDesignHelper.FromJObject(glamourerJObject) is not { } glamourerDesign)
        {
            // TODO: Logging
            return false;
        }
        
        permanentTransformationLockService.Lock(key);
        
        _permanentTransformationData = new PermanentTransformationData
        {
            Sender = sender,
            GlamourerDesign = glamourerDesign,
            GlamourerApplyType = glamourerApplyFlags,
            Key = key
        };

        return true;
    }

    public bool TryClearPermanentTransformation(string key)
    {
        if (permanentTransformationLockService.Unlock(key) is false)
            return false;

        _permanentTransformationData = null;
        return true;
    }

    public void ForceClearPermanentTransformation()
    {
        if (permanentTransformationLockService.Key is null)
            return;

        TryClearPermanentTransformation(permanentTransformationLockService.Key);
    }

    /// <summary>
    ///     Resolves any differences between the stored permanent transformation and the current character
    ///     if a permanent transformation is present
    /// </summary>
    // TODO: Logging
    public async Task ResolveDifferencesAfterGlamourerUpdate()
    {
        if (permanentTransformationLockService.Locked is false || _permanentTransformationData is not { } data)
        {
            Plugin.Log.Verbose("[PermanentTransformationManager] Local character is not locked, ignoring glamourer update");
            return;
        }

        if (await glamourerService.GetDesignComponentsAsync() is not { } localCharacterDesign)
            return;

        if (GlamourerDesignHelper.FromJObject(localCharacterDesign) is not { } design)
            return;

        if (LocalPlayerDesignChanged(design, data))
            await characterTransformationManager.ApplyPerm(data);
    }

    private static bool LocalPlayerDesignChanged(GlamourerDesign localDesign, PermanentTransformationData data)
    {
        if ((data.GlamourerApplyType & GlamourerApplyFlags.Equipment) is GlamourerApplyFlags.Equipment)
        {
            // Check if the equipped items differ
            if (LocalPlayerEquippedItemsChanged(localDesign.Equipment, data.GlamourerDesign.Equipment))
                return true;
            
            // Check if dictionary size for the materials is different
            if (localDesign.Materials.Count != data.GlamourerDesign.Materials.Count)
                return true;
            
            // Iterate over all the local design keys
            foreach (var kvp in localDesign.Materials)
            {
                // Check if the key is present in the permanent design
                if (data.GlamourerDesign.Materials.TryGetValue(kvp.Key, out var material) is false)
                    return true;

                // Check if the objects differ
                if (material.IsEqualTo(kvp.Value) is false)
                    return true;
            }
        }
        
        if ((data.GlamourerApplyType & GlamourerApplyFlags.Customization) is GlamourerApplyFlags.Customization)
        {
            // Check if customize differs
            if (LocalPlayerCustomizeChanged(localDesign.Customize, data.GlamourerDesign.Customize))
                return true;
            
            // Check if parameters differ
            if (localDesign.Parameters.IsEqualTo(data.GlamourerDesign.Parameters) is false)
                return true;
        }

        return false;
    }

    /// <summary>
    ///     Tests to see if any equipment marked with 'apply' are different
    /// </summary>
    private static bool LocalPlayerEquippedItemsChanged(GlamourerEquipment localEquipment, GlamourerEquipment permanentEquipment)
    {
        // Only check if the permanent transformation affects a certain item
        if (permanentEquipment.Head.Apply && permanentEquipment.Head.IsEqualTo(localEquipment.Head) is false) return true;
        if (permanentEquipment.Body.Apply && permanentEquipment.Body.IsEqualTo(localEquipment.Body) is false) return true;
        if (permanentEquipment.Hands.Apply && permanentEquipment.Hands.IsEqualTo(localEquipment.Hands) is false) return true;
        if (permanentEquipment.Legs.Apply && permanentEquipment.Legs.IsEqualTo(localEquipment.Legs) is false) return true;
        if (permanentEquipment.Feet.Apply && permanentEquipment.Feet.IsEqualTo(localEquipment.Feet) is false) return true;
        if (permanentEquipment.Ears.Apply && permanentEquipment.Ears.IsEqualTo(localEquipment.Ears) is false) return true;
        if (permanentEquipment.Neck.Apply && permanentEquipment.Neck.IsEqualTo(localEquipment.Neck) is false) return true;
        if (permanentEquipment.Wrists.Apply && permanentEquipment.Wrists.IsEqualTo(localEquipment.Wrists) is false) return true;
        if (permanentEquipment.RFinger.Apply && permanentEquipment.RFinger.IsEqualTo(localEquipment.RFinger) is false) return true;
        if (permanentEquipment.LFinger.Apply && permanentEquipment.LFinger.IsEqualTo(localEquipment.LFinger) is false) return true;
        return false;
    }

    /// <summary>
    ///     Tests to see if any Customize marked with 'apply' are different
    /// </summary>
    private static bool LocalPlayerCustomizeChanged(GlamourerCustomize localCustomize, GlamourerCustomize permanentCustomize)
    {
        // Only check if permanent transformation affects a certain item
        if (permanentCustomize.BodyType.Apply && localCustomize.BodyType.IsEqualTo(permanentCustomize.BodyType) is false) return true;
        if (permanentCustomize.BustSize.Apply && localCustomize.BustSize.IsEqualTo(permanentCustomize.BustSize) is false) return true;
        if (permanentCustomize.Clan.Apply && localCustomize.Clan.IsEqualTo(permanentCustomize.Clan) is false) return true;
        if (permanentCustomize.Eyebrows.Apply && localCustomize.Eyebrows.IsEqualTo(permanentCustomize.Eyebrows) is false) return true;
        if (permanentCustomize.EyeColorLeft.Apply && localCustomize.EyeColorLeft.IsEqualTo(permanentCustomize.EyeColorLeft) is false) return true;
        if (permanentCustomize.EyeColorRight.Apply && localCustomize.EyeColorRight.IsEqualTo(permanentCustomize.EyeColorRight) is false) return true;
        if (permanentCustomize.EyeShape.Apply && localCustomize.EyeShape.IsEqualTo(permanentCustomize.EyeShape) is false) return true;
        if (permanentCustomize.Face.Apply && localCustomize.Face.IsEqualTo(permanentCustomize.Face) is false) return true;
        if (permanentCustomize.FacePaint.Apply && localCustomize.FacePaint.IsEqualTo(permanentCustomize.FacePaint) is false) return true;
        if (permanentCustomize.FacePaintColor.Apply && localCustomize.FacePaintColor.IsEqualTo(permanentCustomize.FacePaintColor) is false) return true;
        if (permanentCustomize.FacePaintReversed.Apply && localCustomize.FacePaintReversed.IsEqualTo(permanentCustomize.FacePaintReversed) is false) return true;
        if (permanentCustomize.FacialFeature1.Apply && localCustomize.FacialFeature1.IsEqualTo(permanentCustomize.FacialFeature1) is false) return true;
        if (permanentCustomize.FacialFeature2.Apply && localCustomize.FacialFeature2.IsEqualTo(permanentCustomize.FacialFeature2) is false) return true;
        if (permanentCustomize.FacialFeature3.Apply && localCustomize.FacialFeature3.IsEqualTo(permanentCustomize.FacialFeature3) is false) return true;
        if (permanentCustomize.FacialFeature4.Apply && localCustomize.FacialFeature4.IsEqualTo(permanentCustomize.FacialFeature4) is false) return true;
        if (permanentCustomize.FacialFeature5.Apply && localCustomize.FacialFeature5.IsEqualTo(permanentCustomize.FacialFeature5) is false) return true;
        if (permanentCustomize.FacialFeature6.Apply && localCustomize.FacialFeature6.IsEqualTo(permanentCustomize.FacialFeature6) is false) return true;
        if (permanentCustomize.FacialFeature7.Apply && localCustomize.FacialFeature7.IsEqualTo(permanentCustomize.FacialFeature7) is false) return true;
        if (permanentCustomize.Gender.Apply && localCustomize.Gender.IsEqualTo(permanentCustomize.Gender) is false) return true;
        if (permanentCustomize.HairColor.Apply && localCustomize.HairColor.IsEqualTo(permanentCustomize.HairColor) is false) return true;
        if (permanentCustomize.Hairstyle.Apply && localCustomize.Hairstyle.IsEqualTo(permanentCustomize.Hairstyle) is false) return true;
        if (permanentCustomize.Height.Apply && localCustomize.Height.IsEqualTo(permanentCustomize.Height) is false) return true;
        if (permanentCustomize.Highlights.Apply && localCustomize.Highlights.IsEqualTo(permanentCustomize.Highlights) is false) return true;
        if (permanentCustomize.HighlightsColor.Apply && localCustomize.HighlightsColor.IsEqualTo(permanentCustomize.HighlightsColor) is false) return true;
        if (permanentCustomize.Jaw.Apply && localCustomize.Jaw.IsEqualTo(permanentCustomize.Jaw) is false) return true;
        if (permanentCustomize.LegacyTattoo.Apply && localCustomize.LegacyTattoo.IsEqualTo(permanentCustomize.LegacyTattoo) is false) return true;
        if (permanentCustomize.LipColor.Apply && localCustomize.LipColor.IsEqualTo(permanentCustomize.LipColor) is false) return true;
        if (permanentCustomize.Lipstick.Apply && localCustomize.Lipstick.IsEqualTo(permanentCustomize.Lipstick) is false) return true;
        if (permanentCustomize.Mouth.Apply && localCustomize.Mouth.IsEqualTo(permanentCustomize.Mouth) is false) return true;
        if (permanentCustomize.MuscleMass.Apply && localCustomize.MuscleMass.IsEqualTo(permanentCustomize.MuscleMass) is false) return true;
        if (permanentCustomize.Nose.Apply && localCustomize.Nose.IsEqualTo(permanentCustomize.Nose) is false) return true;
        if (permanentCustomize.Race.Apply && localCustomize.Race.IsEqualTo(permanentCustomize.Race) is false) return true;
        if (permanentCustomize.SkinColor.Apply && localCustomize.SkinColor.IsEqualTo(permanentCustomize.SkinColor) is false) return true;
        if (permanentCustomize.SmallIris.Apply && localCustomize.SmallIris.IsEqualTo(permanentCustomize.SmallIris) is false) return true;
        if (permanentCustomize.TailShape.Apply && localCustomize.TailShape.IsEqualTo(permanentCustomize.TailShape) is false) return true;
        if (permanentCustomize.TattooColor.Apply && localCustomize.TattooColor.IsEqualTo(permanentCustomize.TattooColor) is false) return true;
        // Skip Wetness
        return false;
    }
}