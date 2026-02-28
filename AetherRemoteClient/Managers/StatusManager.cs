using System;
using AetherRemoteClient.Domain;

namespace AetherRemoteClient.Managers;

/// <summary>
///     Keep track of what parts of your character are modified
/// </summary>
public class StatusManager
{
    /// <summary>
    ///     Event fired when a status is changed
    /// </summary>
    /// <remarks>True if a status was added, false if a status was removed</remarks>
    public event Action? StatusChanged;
    
    /// <summary>
    ///     Retrieves the amount of statuses currently affecting the player
    /// </summary>
    /// <returns></returns>
    public uint GetStatusCount()
    {
        var count = 0U;
        if (CustomizePlus is not null) count++;
        if (GlamourerPenumbra is not null) count++;
        if (Honorific is not null) count++;
        if (Hypnosis is not null) count++;
        if (Possession is not null) count++;
        return count;
    }
    
    /// <summary>
    ///     Any Customize changes
    /// </summary>
    public AetherRemoteStatus? CustomizePlus { get; private set; }
    public void SetCustomizePlus(Friend applier)
    {
        CustomizePlus = new AetherRemoteStatus(applier, DateTime.Now);
        StatusChanged?.Invoke();
    }
    public void ClearCustomizePlus()
    {
        CustomizePlus = null;
        StatusChanged?.Invoke();
    }

    /// <summary>
    ///     Any glamourer or penumbra changes
    /// </summary>
    public AetherRemoteStatus? GlamourerPenumbra { get; private set; }
    public void SetGlamourerPenumbra(Friend applier)
    {
        GlamourerPenumbra = new AetherRemoteStatus(applier, DateTime.Now);
        StatusChanged?.Invoke();
    }
    public void ClearGlamourerPenumbra()
    {
        GlamourerPenumbra = null;
        StatusChanged?.Invoke();
    }

    /// <summary>
    ///     Any Honorific changes
    /// </summary>
    public AetherRemoteStatus? Honorific { get; private set; }
    public void SetHonorific(Friend applier)
    {
        Honorific = new AetherRemoteStatus(applier, DateTime.Now);
        StatusChanged?.Invoke();
    }
    public void ClearHonorific()
    {
        Honorific = null;
        StatusChanged?.Invoke();
    }

    /// <summary>
    ///     Any hypnosis
    /// </summary>
    public AetherRemoteStatus? Hypnosis { get; private set; }
    public void SetHypnosis(Friend applier)
    {
        Hypnosis = new AetherRemoteStatus(applier, DateTime.Now);
        StatusChanged?.Invoke();
    }
    public void ClearHypnosis()
    {
        Hypnosis = null;
        StatusChanged?.Invoke();
    }

    /// <summary>
    ///     Any possession
    /// </summary>
    public AetherRemoteStatus? Possession { get; private set; }
    public void SetPossession(Friend applier)
    {
        Possession = new AetherRemoteStatus(applier, DateTime.Now);
        StatusChanged?.Invoke();
    }
    public void ClearPossession()
    {
        Possession = null;
        StatusChanged?.Invoke();
    }
}
