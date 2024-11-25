using System;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using AetherRemoteClient.Domain.Events;
using AetherRemoteCommon.Domain.CommonGlamourerApplyType;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using Glamourer.Api.Enums;
using Glamourer.Api.Helpers;
using Glamourer.Api.IpcSubscribers;

namespace AetherRemoteClient.Accessors.Glamourer;

/// <summary>
/// Provides access to Glamourer exposed methods
/// </summary>
public class GlamourerAccessor : IDisposable
{
    // Const
    private const int RequiredMajorVersion = 1;
    private const int RequiredMinorVersion = 3;
    private const int TestApiIntervalInSeconds = 60;

    // When Mare updates a local glamourer profile, it locks to prevent local tampering
    // Unfortunately, we need this key to unlock the profile to get the state.
    // https://github.com/Penumbra-Sync/client/blob/main/MareSynchronos/Interop/Ipc/IpcCallerGlamourer.cs#L31
    private const uint MareLockCode = 0x6D617265;

    // Check Glamourer Api
    private readonly Timer _periodicGlamourerTest;

    // Glamourer Api
    private readonly ApiVersion _apiVersion;
    private readonly ApplyState _applyState;
    private readonly GetStateBase64 _getStateBase64;
    private readonly RevertState _revertState;
    private readonly RevertToAutomation _revertToAutomation;

    // Glamourer Events
    private readonly EventSubscriber<IntPtr, StateChangeType> _stateChangedWithType;

    /// <summary>
    /// Event fired when the local player's character is reverted to game or automation
    /// </summary>
    public event EventHandler<GlamourerStateChangedEventArgs>? LocalPlayerResetOrReapply;

    /// <summary>
    /// <inheritdoc cref="GlamourerAccessor"/>
    /// </summary>
    public GlamourerAccessor()
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
    /// Event fired when a glamourer state is changed. Will re-fire an event if the change is a Reset or Reapply and
    /// it is caused by the player.
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
                $"[Glamourer::StateChangedWithType] Exception while processing glamourer state change event: {e}");
        }
    }

    /// <summary>
    /// Is the glamourer api available for use?
    /// </summary>
    public bool IsGlamourerUsable { get; private set; }

    /// <summary>
    /// Reverts back to original game defaults
    /// </summary>
    public async Task<bool> RevertToGame(ushort objectIndex = 0)
    {
        return await Plugin.RunOnFramework(() =>
        {
            try
            {
                var result = _revertState.Invoke(objectIndex);
                Plugin.Log.Verbose($"[Glamourer::RevertState] {result} for {objectIndex}");
                return result == GlamourerApiEc.Success;
            }
            catch (Exception ex)
            {
                Plugin.Log.Warning($"[Glamourer::RevertState] Failed for {objectIndex}: {ex}");
                return false;
            }
        });
    }

    /// <summary>
    /// Reverts back to original automation
    /// </summary>
    public async Task<bool> RevertToAutomation(ushort objectIndex = 0)
    {
        return await Plugin.RunOnFramework(() =>
        {
            try
            {
                var result = _revertToAutomation.Invoke(objectIndex);
                Plugin.Log.Verbose($"[Glamourer::RevertToAutomation] {result} for {objectIndex}");
                return result == GlamourerApiEc.Success;
            }
            catch (Exception ex)
            {
                Plugin.Log.Warning($"[Glamourer::RevertToAutomation] Failed for {objectIndex}: {ex}");
                return false;
            }
        });
    }

    /// <summary>
    /// Applies a given design to an object index
    /// </summary>
    public async Task<bool> ApplyDesignAsync(string glamourerData, ushort objectIndex = 0,
        GlamourerApplyFlag flags = GlamourerApplyFlag.All)
    {
        if (IsGlamourerUsable == false)
            return false;

        return await Plugin.RunOnFramework(() =>
        {
            try
            {
                var result = _applyState.Invoke(glamourerData, objectIndex, 0, ConvertGlamourerToApplyFlags(flags));
                Plugin.Log.Verbose($"[Glamourer::ApplyState] {result} for {objectIndex}");
                return result == GlamourerApiEc.Success;
            }
            catch (Exception ex)
            {
                Plugin.Log.Error($"[Glamourer::ApplyState] Failure for {objectIndex}: {ex}");
                return false;
            }
        });
    }

    /// <summary>
    /// Gets a design from a given index
    /// </summary>
    public async Task<string?> GetDesignAsync(ushort objectIndex = 0)
    {
        if (IsGlamourerUsable is false)
            return string.Empty;

        return await Plugin.RunOnFramework(() =>
        {
            try
            {
                var (result, data) = _getStateBase64.Invoke(objectIndex, MareLockCode);
                Plugin.Log.Verbose($"[Glamourer::GetStateBase64] {result} for {objectIndex} with data {data}");

                if (result is GlamourerApiEc.InvalidKey)
                    Plugin.Log.Warning("[Glamourer::GetStateBase64] Could not get design.");

                return data;
            }
            catch (Exception ex)
            {
                Plugin.Log.Error($"[Glamourer::GetStateBase64] Failure for {objectIndex}: {ex}");
                return null;
            }
        });
    }

    /// <summary>
    /// Clean up managed resources
    /// </summary>
    public void Dispose()
    {
        _periodicGlamourerTest.Elapsed -= PeriodicCheckApi;
        _periodicGlamourerTest.Stop();
        _periodicGlamourerTest.Dispose();

        _stateChangedWithType.Event -= OnGlamourerStateChanged;
        _stateChangedWithType.Disable();

        GC.SuppressFinalize(this);
    }

    private void PeriodicCheckApi(object? sender, ElapsedEventArgs e) => CheckApi();

    private void CheckApi()
    {
        try
        {
            // Test if plugin installed
            var glamourerPlugin = Plugin.PluginInterface.InstalledPlugins.FirstOrDefault(plugin =>
                string.Equals(plugin.InternalName, "Glamourer", StringComparison.OrdinalIgnoreCase));
            if (glamourerPlugin is null)
            {
                IsGlamourerUsable = false;
                return;
            }

            // Test if plugin can be invoked
            var glamourerVersion = _apiVersion.Invoke();
            if (glamourerVersion.Major is not RequiredMajorVersion || glamourerVersion.Minor < RequiredMinorVersion)
            {
                IsGlamourerUsable = false;
                return;
            }

            IsGlamourerUsable = true;
        }
        catch (Exception ex)
        {
            Plugin.Log.Error($"Something went wrong trying to check for glamourer plugin: {ex}");
        }
    }

    /// <summary>
    /// Glamourer flags seem to be subtractive.
    /// AetherRemote implemented them additively. The flags must be switched.
    /// There is a chance this is also just a bug. Who knows! I certainly don't.
    /// </summary>
    private static ApplyFlag ConvertGlamourerToApplyFlags(GlamourerApplyFlag flags)
    {
        var finalizedFlags = ApplyFlag.Once;
        if (flags.HasFlag(GlamourerApplyFlag.Customization)) finalizedFlags |= ApplyFlag.Equipment;
        if (flags.HasFlag(GlamourerApplyFlag.Equipment)) finalizedFlags |= ApplyFlag.Customization;
        if (finalizedFlags == ApplyFlag.Once) finalizedFlags |= ApplyFlag.Customization | ApplyFlag.Equipment;
        return finalizedFlags;
    }
}