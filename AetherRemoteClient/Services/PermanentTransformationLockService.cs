namespace AetherRemoteClient.Services;

/// <summary>
///     Light wrapper for managing a locked state
/// </summary>
public class PermanentTransformationLockService
{
    /// <summary>
    ///     The key to the current lock, or null if lock is not present
    /// </summary>
    public string? Key { get; private set; }
    
    /// <summary>
    ///     if there is a lock present
    /// </summary>
    public bool Locked => Key is not null;

    /// <summary>
    ///     Attempts to set the lock of the local player
    /// </summary>
    /// <param name="key">A four-character key</param>
    /// <returns>If the lock was set</returns>
    public bool Lock(string key)
    {
        Plugin.Log.Verbose($"[PermanentTransformationLockService] [Lock] Attempting to lock with key {key}");
        
        if (Locked)
        {
            Plugin.Log.Warning("[PermanentTransformationLockService] [Lock] Already locked");
            return false;
        }

        if (key.Length is not 4)
        {
            Plugin.Log.Warning($"[PermanentTransformationLockService] [Lock] Key {key} must be exactly 4 characters");
            return false;
        }
        
        Plugin.Log.Verbose($"[PermanentTransformationLockService] [Lock] Successfully locked with key {key}");
        Key = key;
        return true;
    }

    /// <summary>
    ///     Attempts to clear the lock of the local player
    /// </summary>
    /// <param name="key">The four-character key to try</param>
    /// <returns>If the lock was cleared</returns>
    public bool Unlock(string key)
    {
        Plugin.Log.Verbose($"[PermanentTransformationLockService] [Unlock] Attempting to unlock with key {key}");
        
        if (Locked is false)
        {
            Plugin.Log.Verbose("[PermanentTransformationLockService] [Unlock] Nothing to unlock");
            return true;
        }
        
        if (key.Length is not 4)
        {
            Plugin.Log.Verbose($"[PermanentTransformationLockService] [Unlock] Key {key} must be exactly 4 characters");
            return false;
        }

        if (Key == key)
        {
            Plugin.Log.Verbose($"[PermanentTransformationLockService] [Unlock] Successfully unlocked with key {key}");
            Key = null;
            return true;
        }
        
        Plugin.Log.Verbose($"[PermanentTransformationLockService] [Unlock] Incorrect key {key}");
        return false;
    }
}