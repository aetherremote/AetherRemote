using System;
using System.Threading.Tasks;
using System.Timers;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Events;
using AetherRemoteClient.Ipc;
using AetherRemoteClient.Services;
using AetherRemoteClient.Utils;
using Newtonsoft.Json.Linq;

namespace AetherRemoteClient.Managers;

/// <summary>
///     Manages saving and loading of permanent transformations
/// </summary>
public class PermanentTransformationManager : IDisposable
{
    // Const
    private const string EquipmentJObjectName = "Equipment";
    private const string MainHandJObjectName = "MainHand";
    private const string OffHandJObjectName = "OffHand";
    
    // Injected
    private readonly GlamourerIpc _glamourer;
    private readonly PenumbraIpc _penumbra;
    private readonly CustomizePlusIpc _customizePlus;
    private readonly MoodlesIpc _moodlesIpc;
    private readonly IdentityService _identityService;
    private readonly PermanentLockService _permanentLockService;
    
    /// <summary>
    ///     A timer that will be used so not as to spam glamourer with requests.
    /// </summary>
    private readonly Timer _revertTimer = new(2000);

    /// <summary>
    ///     The current saved appearance when the client was originally locked
    /// </summary>
    private JObject? _currentSavedAppearance;
    
    /// <summary>
    ///     <inheritdoc cref="PermanentTransformationManager"/>
    /// </summary>
    public PermanentTransformationManager(
        MoodlesIpc moodlesIpc, 
        CustomizePlusIpc customizePlus, 
        PenumbraIpc penumbra, 
        GlamourerIpc glamourer, 
        IdentityService identityService, 
        PermanentLockService permanentLockService)
    {
        _penumbra = penumbra;
        _customizePlus = customizePlus;
        _moodlesIpc = moodlesIpc;
        _glamourer = glamourer;
        _identityService = identityService;
        _permanentLockService = permanentLockService;

        _revertTimer.AutoReset = false;
        _revertTimer.Enabled = false;
        _revertTimer.Elapsed += RevertToLockedTransformation;
        
        _glamourer.LocalPlayerChanged += OnPlayerGlamourerUpdated;
    }

    /// <summary>
    ///     Saves a new permanent transformation for the local character
    /// </summary>
    public async Task<bool> Lock(PermanentTransformationData data)
    {
        // Save the unlock code
        if (_permanentLockService.Lock(data.UnlockCode) is false)
        {
            Plugin.Log.Warning("[PermanentTransformationManager] Could not lock because the client is already locked");
            return false;
        }
        
        // Add the configuration values
        if (Plugin.Configuration.PermanentTransformations.TryAdd(_identityService.Character.FullName, data) is false)
        {
            Plugin.Log.Warning("[PermanentTransformationManager] Couldn't add the transformation because one already exists");
            return false;
        }
        
        // Save
        Plugin.Configuration.Save();
        
        // Get the current appearance
        if (await _glamourer.GetDesignComponentsAsync() is not { } components)
        {
            Plugin.Log.Warning("[PermanentTransformationManager] Couldn't get the design components");
            return false;
        }
        
        // Notify the client
        NotificationHelper.Info("You have been permanently transformed", "One of your friends has locked you in your current form. You will need to retrieve the key from them or use safe mode in an emergency.");
        
        // Remove any fields we don't want to check
        RemoveUnwantedFields(components);
        
        // Set the local cached appearance JObject data
        _currentSavedAppearance = components;
        return true;
    }

    /// <summary>
    ///     Unlock the current permanent transformation
    /// </summary>
    public void Unlock(string key)
    {
        // Already unlocked
        if (_permanentLockService.IsLocked is false)
        {
            Plugin.Log.Info("[PermanentTransformationManager] There is nothing to unlock, skipping...");
            return;
        }

        // Try to unlock
        if (_permanentLockService.Unlock(key) is false)
        {
            NotificationHelper.Warning("Incorrect Pin", $"\"{key}\" was not the correct pin, \"{_permanentLockService.CurrentLock}\" is correct");
            return;
        }
        
        // Success, notify the client
        NotificationHelper.Success("Successfully Unlocked", string.Empty);
        
        // Remove the current permanent swap
        Plugin.Configuration.PermanentTransformations.Remove(_identityService.Character.FullName);
        Plugin.Configuration.Save();
    }

    /// <summary>
    ///     Forcefully unlock the appearance
    /// </summary>
    public void ForceUnlock()
    {
        // Stop the timer
        _revertTimer.Stop();
        Plugin.Log.Info("[PermanentTransformationManager] Initiated forceful unlocking");
        Unlock(_permanentLockService.CurrentLock);
    }

    /// <summary>
    ///     Attempts to load the provided permanent transformation
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public async Task Load(PermanentTransformationData data)
    {
        // Already locked
        if (_permanentLockService.IsLocked)
        {
            Plugin.Log.Warning("[PermanentTransformationManager] Unable to load because the client is already locked");
            return;
        }

        // Lock the client
        if (_permanentLockService.Lock(data.UnlockCode) is false)
        {
            Plugin.Log.Warning("[PermanentTransformationManager] Unable to lock");
            return;
        }

        // Apply the data
        await ApplySavedAppearance(data);

        // Get the appearance to save the components
        if (await _glamourer.GetDesignComponentsAsync() is not { } components)
        {
            Plugin.Log.Warning("[PermanentTransformationManager] Design components are empty");
            return;
        }
            
        // Remove any fields we don't want to check
        RemoveUnwantedFields(components);
        
        // Set the current saved appearance to be the components
        _currentSavedAppearance = components;
    }
    
    /// <summary>
    ///     Applies a given <see cref="PermanentTransformationData"/> to the client
    /// </summary>
    private async Task ApplySavedAppearance(PermanentTransformationData data)
    {
        // Always apply glamourer
        await _glamourer.ApplyDesignAsync(data.GlamourerData, data.GlamourerApplyFlags).ConfigureAwait(false);

        // Apply Mods
        if (data.ModPathData is not null)
        {
            var collection = await _penumbra.GetCollection();
            await _penumbra.AddTemporaryMod(collection, data.ModPathData, data.ModMetaData!).ConfigureAwait(false);
        }
        
        // Apply Customize
        if (data.CustomizePlusData is not null)
        {
            // Delete any existing profiles
            await _customizePlus.DeleteCustomize().ConfigureAwait(false);
            
            // Deserialize back into a list of templates
            if (await _customizePlus.DeserializeTemplates(data.CustomizePlusData).ConfigureAwait(false) is not { } templates)
            {
                Plugin.Log.Warning("[PermanentTransformationManager] Failed to deserialized saved C+ template data");
                return;
            }
            
            // Apply templates
            if (await _customizePlus.ApplyCustomize(templates).ConfigureAwait(false) is false)
            {
                // Delete any partial profiles that were created
                await _customizePlus.DeleteCustomize().ConfigureAwait(false);
                Plugin.Log.Warning("[PermanentTransformationManager] Failed to apply saved C+ template data");
                return;
            }
        }

        // Apply Moodles
        if (data.MoodlesData is not null)
        {
            if (await Plugin.RunOnFramework(() => Plugin.ObjectTable[0]?.Address).ConfigureAwait(false) is { } address)
            {
                await _moodlesIpc.SetMoodles(address, data.MoodlesData).ConfigureAwait(false);
            }
        }
    }
    
    private void OnPlayerGlamourerUpdated(object? sender, GlamourerStateChangedEventArgs args)
    {
        // Skip starting the timer if there is no lock
        if (_permanentLockService.IsLocked is false)
        {
            Plugin.Log.Verbose("[PermanentTransformationManager] Skipping update from glamourer because local player is not locked");
            return;
        }
        
        // Restart the timer
        _revertTimer.Stop();
        _revertTimer.Start();
    }
    
    /// <summary>
    ///     Reverts the local player to the locked appearance 
    /// </summary>
    private async void RevertToLockedTransformation(object? sender, ElapsedEventArgs e)
    {
        try
        {
            // Get the design as components
            if (await _glamourer.GetDesignComponentsAsync().ConfigureAwait(false) is not { } components)
            {
                Plugin.Log.Warning("[PermanentTransformationManager] Could not get design components");
                return;
            }

            // Remove the fields we don't want to check
            RemoveUnwantedFields(components);

            // Check to see if there are differences and exit if there are none
            if (JToken.DeepEquals(_currentSavedAppearance, components))
            {
                Plugin.Log.Info("[PermanentTransformationManager] Local client changed, but not anything outward on their appearance");
                return;
            }
            
            // A change has been detected, reverting the player
            Plugin.Log.Info("Local player attempted to reload with a permanent transformation, reverting...");

            
            // Get the local saved permanent transformation data
            if (Plugin.Configuration.PermanentTransformations.TryGetValue(_identityService.Character.FullName, out var data) is false)
            {
                // No local character data
                Plugin.Log.Warning("[PermanentTransformationManager] No permanent transformation data set for local character");
                return;
            }
            
            await ApplySavedAppearance(data);
        }
        catch (Exception ex)
        {
            Plugin.Log.Warning($"Unexpected issue enforcing a permanent transformation {ex.Message}");
        }
    }
    
    /// <summary>
    ///     Removes unwanted fields from the JSON object returned from glamourer's <see cref="GlamourerIpc"/>
    /// </summary>
    private static void RemoveUnwantedFields(JObject components)
    {
        // Get the equipment, which should always exist
        if (components[EquipmentJObjectName] is not JObject equipment)
        {
            Plugin.Log.Warning("[PermanentTransformationManager] No equipment found when one was expected");
            return;
        }
        
        // Remove these two fields
        equipment.Remove(MainHandJObjectName);
        equipment.Remove(OffHandJObjectName);
    }

    public void Dispose()
    {
        _revertTimer.Elapsed -= RevertToLockedTransformation;
        _glamourer.LocalPlayerResetOrReapply -= OnPlayerGlamourerUpdated;
        GC.SuppressFinalize(this);
    }
}