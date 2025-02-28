using System;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using AetherRemoteClient.Domain.Events;
using AetherRemoteCommon.Domain.Enums;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using Glamourer.Api.Enums;
using Glamourer.Api.Helpers;
using Glamourer.Api.IpcSubscribers;

namespace AetherRemoteClient.Services.External;

/// <summary>
///     Provides access to Glamourer IPCs
/// </summary>
public class GlamourerService : IDisposable
{
    // Const
    private const int RequiredMajorVersion = 1;
    private const int RequiredMinorVersion = 3;
    private const int TestApiIntervalInSeconds = 60;

    // When Mare updates a local glamourer profile, it locks to prevent local tampering.
    // Unfortunately, we need this key to unlock the profile to get the state.
    // https://github.com/Penumbra-Sync/client/blob/main/MareSynchronos/Interop/Ipc/IpcCallerGlamourer.cs#L31
    private const uint MareLockCode = 0x6D617265;

    // Glamourer Api
    private readonly ApiVersion _apiVersion;
    private readonly ApplyState _applyState;
    private readonly GetStateBase64 _getStateBase64;
    private readonly RevertState _revertState;
    private readonly RevertToAutomation _revertToAutomation;

    // Glamourer Events
    private readonly EventSubscriber<IntPtr, StateChangeType> _stateChangedWithType;

    // Check Glamourer Api
    private readonly Timer _periodicGlamourerTest;
    
    /// <summary>
    ///     <inheritdoc cref="GlamourerService" />
    /// </summary>
    public GlamourerService()
    {
        _apiVersion = new ApiVersion(Plugin.PluginInterface);
        _applyState = new ApplyState(Plugin.PluginInterface);
        _getStateBase64 = new GetStateBase64(Plugin.PluginInterface);
        _revertState = new RevertState(Plugin.PluginInterface);
        _revertToAutomation = new RevertToAutomation(Plugin.PluginInterface);

        _periodicGlamourerTest = new Timer(TestApiIntervalInSeconds * 1000);
        _periodicGlamourerTest.AutoReset = true;
        _periodicGlamourerTest.Elapsed += PeriodicCheckApi;
        _periodicGlamourerTest.Start();

        _stateChangedWithType = StateChangedWithType.Subscriber(Plugin.PluginInterface);
        _stateChangedWithType.Event += OnGlamourerStateChanged;

        CheckApi();
    }

    /// <summary>
    ///     Is the glamourer api available for use?
    /// </summary>
    private bool GlamourerUsable { get; set; }

    /// <summary>
    ///     Event fired when the local player's character is reverted to game or automation
    /// </summary>
    public event EventHandler<GlamourerStateChangedEventArgs>? LocalPlayerResetOrReapply;

    /// <summary>
    ///     Reverts to original game defaults
    /// </summary>
    public async Task<bool> RevertToGame(ushort objectIndex = 0)
    {
        return await Plugin.RunOnFramework(() =>
        {
            try
            {
                var result = _revertState.Invoke(objectIndex);
                return result is GlamourerApiEc.Success;
            }
            catch (Exception ex)
            {
                Plugin.Log.Warning($"[GlamourerService] [RevertToGame] Failed for {objectIndex}, {ex}");
                return false;
            }
        }).ConfigureAwait(false);
    }

    /// <summary>
    ///     Reverts to original automation
    /// </summary>
    public async Task<bool> RevertToAutomation(ushort objectIndex = 0)
    {
        return await Plugin.RunOnFramework(() =>
        {
            try
            {
                var result = _revertToAutomation.Invoke(objectIndex);
                return result is GlamourerApiEc.Success;
            }
            catch (Exception ex)
            {
                Plugin.Log.Warning($"[GlamourerService] [RevertToAutomation] Failed for {objectIndex}: {ex}");
                return false;
            }
        }).ConfigureAwait(false);
    }

    /// <summary>
    ///     Applies a given design to an object index
    /// </summary>
    public async Task<bool> ApplyDesignAsync(string glamourerData, GlamourerApplyFlag flags, ushort objectIndex = 0)
    {
        if (GlamourerUsable is false)
            return false;

        return await Plugin.RunOnFramework(() =>
        {
            try
            {
                var result = _applyState.Invoke(glamourerData, objectIndex, 0, ConvertGlamourerToApplyFlags2(flags));
                return result is GlamourerApiEc.Success;
            }
            catch (Exception ex)
            {
                Plugin.Log.Error($"[GlamourerService] [ApplyDesignAsync] Failure for {objectIndex}: {ex}");
                return false;
            }
        }).ConfigureAwait(false);
    }

    /// <summary>
    ///     Gets a design from a given index
    /// </summary>
    public async Task<string?> GetDesignAsync(ushort objectIndex = 0)
    {
        if (GlamourerUsable is false)
            return string.Empty;

        return await Plugin.RunOnFramework(() =>
        {
            try
            {
                var (_, data) = _getStateBase64.Invoke(objectIndex, MareLockCode);
                return data;
            }
            catch (Exception ex)
            {
                Plugin.Log.Error($"[GlamourerService] [GetDesignAsync] Failure for {objectIndex}: {ex}");
                return null;
            }
        }).ConfigureAwait(false);
    }

    /// <summary>
    ///     The Event fired when a glamourer state is changed.
    ///     Will re-fire an event if the change is a Reset or Reapply, and it is caused by the player.
    /// </summary>
    private unsafe void OnGlamourerStateChanged(IntPtr objectIndexPointer, StateChangeType stateChangeType)
    {
        try
        {
            if (stateChangeType is not (StateChangeType.Reset or StateChangeType.Reapply))
                return;

            var objectIndex = (GameObject*)objectIndexPointer;
            if (objectIndex->ObjectIndex is 0)
                LocalPlayerResetOrReapply?.Invoke(this, new GlamourerStateChangedEventArgs());
        }
        catch (Exception e)
        {
            Plugin.Log.Warning(
                $"[GlamourerService] [OnGlamourerStateChanged] Exception while processing glamourer state change event: {e}");
        }
    }

    private void PeriodicCheckApi(object? sender, ElapsedEventArgs e)
    {
        CheckApi();
    }

    private void CheckApi()
    {
        try
        {
            // Test if plugin installed
            var glamourerPlugin = Plugin.PluginInterface.InstalledPlugins.FirstOrDefault(plugin =>
                string.Equals(plugin.InternalName, "Glamourer", StringComparison.OrdinalIgnoreCase));
            if (glamourerPlugin is null)
            {
                GlamourerUsable = false;
                return;
            }

            // Test if plugin can be invoked
            var glamourerVersion = _apiVersion.Invoke();
            if (glamourerVersion.Major is not RequiredMajorVersion || glamourerVersion.Minor < RequiredMinorVersion)
            {
                GlamourerUsable = false;
                return;
            }

            GlamourerUsable = true;
        }
        catch (Exception ex)
        {
            Plugin.Log.Error($"[GlamourerService] Something went wrong trying to check for glamourer plugin: {ex}");
        }
    }

    // Experimental to see if the swapping works
    private static ApplyFlag ConvertGlamourerToApplyFlags2(GlamourerApplyFlag flags)
    {
        var applyFlags = ApplyFlag.Once;
        if (flags.HasFlag(GlamourerApplyFlag.Customization)) applyFlags |= ApplyFlag.Customization;
        if (flags.HasFlag(GlamourerApplyFlag.Equipment)) applyFlags |= ApplyFlag.Equipment;
        if (applyFlags is ApplyFlag.Once) applyFlags |= ApplyFlag.Customization | ApplyFlag.Equipment;
        return applyFlags;
    }

    public void Dispose()
    {
        _periodicGlamourerTest.Elapsed -= PeriodicCheckApi;
        _periodicGlamourerTest.Stop();
        _periodicGlamourerTest.Dispose();

        _stateChangedWithType.Event -= OnGlamourerStateChanged;
        _stateChangedWithType.Disable();
        GC.SuppressFinalize(this);
    }
}