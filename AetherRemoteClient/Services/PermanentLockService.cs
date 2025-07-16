namespace AetherRemoteClient.Services;

/// <summary>
///     Manages all the resources relating to a locked appearance
/// </summary>
public class PermanentLockService
{
    /// <summary>
    ///     The value of the current lock
    /// </summary>
    public string CurrentLock { get; private set; } = string.Empty;

    /// <summary>
    ///     If there is a lock set
    /// </summary>
    public bool IsLocked => CurrentLock is not "";
    
    /// <summary>
    ///     Sets a new key if one isn't already set
    /// </summary>
    /// <param name="key"></param>
    public bool Lock(string key)
    {
        // If there isn't a lock, set the lock key
        if (CurrentLock == string.Empty)
        {
            CurrentLock = key;
            Plugin.Log.Info($"[PermanentLockService] Successfully set key to {key}");
            return true;
        }
        
        // Cannot set a new lock because one already exists
        Plugin.Log.Info($"[PermanentLockService] CurrentLock is already set, cannot add new key of {key}");
        return false;
    }

    /// <summary>
    ///     Attempts to unlock using the current key
    /// </summary>
    public bool Unlock(string key)
    {
        // If the current key matches, unlock
        if (key == CurrentLock)
        {
            CurrentLock = string.Empty;
            return true;
        }
        
        // Current key did not match
        Plugin.Log.Info($"[PermanentLockService] Incorrect key {key}");
        return false;
    }
}