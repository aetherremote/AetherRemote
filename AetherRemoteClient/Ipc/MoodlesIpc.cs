using System;
using System.Threading.Tasks;
using AetherRemoteClient.Domain.Interfaces;
using Dalamud.Plugin.Ipc;

namespace AetherRemoteClient.Ipc;

/// <summary>
///     Provides access to Moodles
/// </summary>
public class MoodlesIpc : IExternalPlugin
{
    // Moodles API
    private readonly ICallGateSubscriber<nint, string> _get;
    private readonly ICallGateSubscriber<nint, string, object> _set;
    private readonly ICallGateSubscriber<int> _version;
    
    /// <summary>
    ///     Is Moodles available for use?
    /// </summary>
    public bool ApiAvailable;

    /// <summary>
    ///     <inheritdoc cref="MoodlesIpc"/>
    /// </summary>
    public MoodlesIpc()
    {
        _get = Plugin.PluginInterface.GetIpcSubscriber<nint, string>("Moodles.GetStatusManagerByPtr");
        _set = Plugin.PluginInterface.GetIpcSubscriber<nint, string, object>("Moodles.SetStatusManagerByPtr");
        _version = Plugin.PluginInterface.GetIpcSubscriber<int>("Moodles.Version");
        
        TestIpcAvailability();
    }
    
    /// <summary>
    ///     Tests for availability for Moodles
    /// </summary>
    public void TestIpcAvailability()
    {
        try
        {
            ApiAvailable = _version.InvokeFunc() is 1;
        }
        catch (Exception)
        {
            ApiAvailable = false;
        }
    }
    
    /// <summary>
    ///     Retrieves a target's moodles
    /// </summary>
    /// <param name="address">Object table address of the target whose moodles you will get</param>
    public async Task<string?> GetMoodles(nint address)
    {
        if (ApiAvailable)
            return await Plugin.RunOnFramework(() =>
            {
                try
                {
                    return _get.InvokeFunc(address);
                }
                catch (Exception e)
                {
                    Plugin.Log.Error(
                        $"[MoodlesService] Unexpectedly failed getting moodles for {address}, {e.Message}");
                    return null;
                }
            }).ConfigureAwait(false);

        Plugin.Log.Warning($"[MoodlesService] Unable to get moodles for {address} because moodles is not available");
        return null;
    }

    /// <summary>
    ///     Sets a target's moodles
    /// </summary>
    /// <param name="address">Object table address of the target whose moodles you will get</param>
    /// <param name="moodles">The string of moodles to set</param>
    public async Task<bool> SetMoodles(nint address, string moodles)
    {
        if (ApiAvailable)
            return await Plugin.RunOnFramework(() =>
            {
                try
                {
                    _set.InvokeAction(address, moodles);
                    return true;
                }
                catch (Exception e)
                {
                    Plugin.Log.Error($"[MoodlesService] Unexpected failed to set moodles for {address}, {e.Message}");
                    return false;
                }
            }).ConfigureAwait(false);

        Plugin.Log.Warning($"[MoodlesService] Unable to set moodles for {address} because moodles is not available");
        return false;
    }
}