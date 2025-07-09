using System;
using System.Threading.Tasks;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Events;
using AetherRemoteClient.Ipc;
using AetherRemoteClient.Services;

namespace AetherRemoteClient.Managers;

/// <summary>
///     Manages saving and loading of permanent transformations
/// </summary>
public class PermanentTransformationManager : IDisposable
{
    private readonly GlamourerIpc _glamourer;
    private readonly IdentityService _identityService;
    private readonly PermanentLockService _permanentLockService;
    
    public PermanentTransformationManager(GlamourerIpc glamourer, IdentityService identityService, PermanentLockService permanentLockService)
    {
        _glamourer = glamourer;
        _identityService = identityService;
        _permanentLockService = permanentLockService;

        _glamourer.LocalPlayerResetOrReapply += OnAttemptedResetOrReapply;
    }

    /// <summary>
    ///     Saves a new permanent transformation for the local character
    /// </summary>
    public bool Save(PermanentTransformationData data)
    {
        // Save the unlock code
        _permanentLockService.CurrentLock = data.UnlockCode;
        
        // Add the configuration values
        if (Plugin.Configuration.PermanentTransformations.TryAdd(_identityService.Character.FullName, data) is false)
            return false;
        
        // Save the configuration
        Plugin.Configuration.Save();
        return true;
    }

    /// <summary>
    ///     Unlock the current permanent transformation
    /// </summary>
    public void Unlock(uint key)
    {
        // If there is not a lock
        if (_permanentLockService.CurrentLock is null)
            return;

        // Incorrect key
        // TODO: Add notification for incorrect key
        if (_permanentLockService.CurrentLock != key)
            return;
        
        // Remove the lock from the service
        _permanentLockService.CurrentLock = null;
        
        // Remove the current permanent swap
        Plugin.Configuration.PermanentTransformations.Remove(_identityService.Character.FullName);
        Plugin.Configuration.Save();
    }

    /// <summary>
    ///     Attempts to load the provided permanent transformation
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public async Task<bool> Load(PermanentTransformationData data)
    {
        // Set the current unlock code
        _permanentLockService.CurrentLock = data.UnlockCode;
        
        // Always apply glamourer
        // We cannot apply a key here, otherwise Mare will be unable to read the local player
        await _glamourer.ApplyDesignAsync(data.GlamourerData, data.GlamourerApplyFlags);
        
        // TODO: Other types
        
        return true;
    }
    
    /// <summary>
    ///     This function is responsible for reapplying the stored permanent transformation is the player is currently under a permanent transformation
    /// </summary>
    private async void OnAttemptedResetOrReapply(object? sender, GlamourerStateChangedEventArgs _)
    {
        try
        {
            // If the player isn't under any kind of permanent transformation, ignore
            if (_permanentLockService.CurrentLock is null)
                return;
        
            // Load whatever the current data is and apply it again
            if (Plugin.Configuration.PermanentTransformations.TryGetValue(_identityService.Character.FullName, out var transformation) is false)
            {
                Plugin.Log.Warning("Could not find permanent transformation when one was expected");
                return;
            }
            
            // TODO: Add revert notification that you are permanently locked
            Plugin.Log.Info("Local player attempted to reload with a permanent transformation, reverting...");
            await Task.Delay(1000);
            
            // Load the transformation data
            await Load(transformation);
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"Unexpected issue enforcing a permanent transformation {e.Message}");
        }
    }

    public void Dispose()
    {
        _glamourer.LocalPlayerResetOrReapply -= OnAttemptedResetOrReapply;
        GC.SuppressFinalize(this);
    }
}