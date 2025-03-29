using System;
using System.Threading.Tasks;
using Dalamud.Plugin.Ipc;

namespace AetherRemoteClient.Services.External;

/// <summary>
///     Provides access to Moodles IPCs
/// </summary>
public class MoodlesService
{
    // Moodles API
    private readonly ICallGateSubscriber<nint, string> _get;
    private readonly ICallGateSubscriber<nint, string, object> _set;

    /// <summary>
    ///     Is the moodles api available for use?
    /// </summary>
    private readonly bool _moodlesAvailable;

    /// <summary>
    ///     <inheritdoc cref="MoodlesService"/>
    /// </summary>
    public MoodlesService()
    {
        _get = Plugin.PluginInterface.GetIpcSubscriber<nint, string>("Moodles.GetStatusManagerByPtr");
        _set = Plugin.PluginInterface.GetIpcSubscriber<nint, string, object>("Moodles.SetStatusManagerByPtr");

        try
        {
            _moodlesAvailable = Plugin.PluginInterface.GetIpcSubscriber<int>("Moodles.Version").InvokeFunc() is 1;
        }
        catch (Exception)
        {
            // Ignored
        }

        Plugin.Log.Verbose($"[MoodlesService] Moodles available: {_moodlesAvailable}");
    }

    /// <summary>
    ///     Retrieves a target's moodles
    /// </summary>
    /// <param name="address">Object table address of the target whose moodles you will get</param>
    public async Task<string?> GetMoodles(nint address)
    {
        if (_moodlesAvailable)
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
    /// /// <param name="moodles">The string of moodles to set</param>
    public async Task<bool> SetMoodles(nint address, string moodles)
    {
        if (_moodlesAvailable)
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