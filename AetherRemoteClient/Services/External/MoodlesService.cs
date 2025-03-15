using System;
using System.Threading.Tasks;
using System.Timers;
using Dalamud.Plugin.Ipc;

namespace AetherRemoteClient.Services.External;

/// <summary>
///     Provides access to Moodles IPCs
/// </summary>
public class MoodlesService
{
    // Const
    private const int TestApiIntervalInSeconds = 45;

    // Moodles API
    private readonly ICallGateSubscriber<int> _version;
    private readonly ICallGateSubscriber<nint, string> _get;
    private readonly ICallGateSubscriber<nint, string, object> _set;
    private readonly ICallGateSubscriber<nint, object> _clear;

    // Check Moodles API
    private readonly Timer _periodicMoodlesTest;

    /// <summary>
    ///     Is the moodles api available for use?
    /// </summary>
    private bool _moodlesAvailable;

    /// <summary>
    ///     <inheritdoc cref="MoodlesService"/>
    /// </summary>
    public MoodlesService()
    {
        _version = Plugin.PluginInterface.GetIpcSubscriber<int>("Moodles.Version");
        _get = Plugin.PluginInterface.GetIpcSubscriber<nint, string>("Moodles.GetStatusManagerByPtr");
        _set = Plugin.PluginInterface.GetIpcSubscriber<nint, string, object>("Moodles.SetStatusManagerByPtr");
        _clear = Plugin.PluginInterface.GetIpcSubscriber<nint, object>("Moodles.ClearStatusManagerByPtr");

        _periodicMoodlesTest = new Timer(TestApiIntervalInSeconds * 1000);
        _periodicMoodlesTest.AutoReset = true;
        _periodicMoodlesTest.Elapsed += PeriodicCheckApi;
        _periodicMoodlesTest.Start();

        PeriodicCheckApi();
    }

    /// <summary>
    ///     Retrieves a target's moodles
    /// </summary>
    public async Task<string?> GetMoodles(nint objectTableAddress)
    {
        if (_moodlesAvailable is false)
        {
            Plugin.Log.Warning("[MoodlesService] [GetMoodles] Moodles is not installed!");
            return null;
        }

        try
        {
            return await Plugin.RunOnFramework(() => _get.InvokeFunc(objectTableAddress)).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[MoodlesService] [GetMoodles] Unexpected error for address {objectTableAddress}: {e}");
            return null;
        }
    }

    /// <summary>
    ///     Sets a target's moodles
    /// </summary>
    public async Task<bool> SetMoodles(nint objectTableAddress, string moodle)
    {
        if (_moodlesAvailable is false)
        {
            Plugin.Log.Warning("[MoodlesService] [SetMoodles] Moodles is not installed!");
            return false;
        }

        try
        {
            await Plugin.RunOnFramework(() => _set.InvokeAction(objectTableAddress, moodle)).ConfigureAwait(false);
            return true;
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[MoodlesService] [SetMoodles] Unexpected error for address {objectTableAddress}: {e}");
            return false;
        }
    }

    /// <summary>
    ///     Clear a target's moodles
    /// </summary>
    public async Task<bool> ClearMoodles(nint objectTableAddress)
    {
        if (_moodlesAvailable is false)
        {
            Plugin.Log.Warning("[MoodlesService] [ClearMoodles] Moodles is not installed!");
            return false;
        }

        try
        {
            await Plugin.RunOnFramework(() => _clear.InvokeAction(objectTableAddress)).ConfigureAwait(false);
            return true;
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[MoodlesService] [ClearMoodles] Unexpected error for address {objectTableAddress}: {e}");
            return false;
        }
    }

    private void PeriodicCheckApi(object? sender = null, ElapsedEventArgs? eventArgs = null)
    {
        try
        {
            _moodlesAvailable = _version.InvokeFunc() is 1;
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"Something went wrong trying to check for moodles plugin: {e}");
            _moodlesAvailable = false;
        }
    }
}