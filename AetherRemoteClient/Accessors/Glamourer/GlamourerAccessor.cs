using AetherRemoteCommon.Domain.CommonGlamourerApplyType;
using Glamourer.Api.Enums;
using Glamourer.Api.IpcSubscribers;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace AetherRemoteClient.Accessors.Glamourer;

/// <summary>
/// Provides access to Glamourer's exposed methods
/// </summary>
public class GlamourerAccessor : IDisposable
{
    // Consts
    private const int RequiredMajorVersion = 1;
    private const int RequiredMinorVersion = 3;
    private const int TestApiIntervalInSeconds = 30;

    // When Mare updates a local glamourer profile, it locks to prevent local tampering
    // Unfortunately, we need this key to unlock the profile to get the state.
    // https://github.com/Penumbra-Sync/client/blob/main/MareSynchronos/Interop/Ipc/IpcCallerGlamourer.cs#L31
    private const uint MareLockCode = 0x6D617265;

    // Glamourer Api
    private readonly ApiVersion apiVersion;
    private readonly ApplyState applyState;
    private readonly GetStateBase64 getStateBase64;
    private readonly RevertState revertState;
    private readonly RevertToAutomation revertToAutomation;

    private readonly Timer periodicGlamourerTest;

    // Glamourer Installed?
    private bool glamourerUsable = false;

    /// <summary>
    /// <inheritdoc cref="GlamourerAccessor"/>
    /// </summary>
    public GlamourerAccessor()
    {
        apiVersion = new ApiVersion(Plugin.PluginInterface);
        applyState = new ApplyState(Plugin.PluginInterface);
        getStateBase64 = new GetStateBase64(Plugin.PluginInterface);
        revertState = new RevertState(Plugin.PluginInterface);
        revertToAutomation = new RevertToAutomation(Plugin.PluginInterface);

        periodicGlamourerTest = new Timer(TestApiIntervalInSeconds * 1000);
        periodicGlamourerTest.AutoReset = true;
        periodicGlamourerTest.Elapsed += PeriodicCheckApi;
        periodicGlamourerTest.Start();

        CheckApi();
    }

    /// <summary>
    /// Is the glamourer api available for use?
    /// </summary>
    public bool IsGlamourerUsable => glamourerUsable;

    /// <summary>
    /// Reverts back to original game defaults
    /// </summary>
    public async Task<bool> RevertToGame(ushort objectIndex = 0)
    {
        return await Plugin.RunOnFramework(() =>
        {
            try
            {
                var result = revertState.Invoke(objectIndex);
                Plugin.Log.Verbose($"[RevertToGame ObjectIndex] {result} for {objectIndex}");
                return result == GlamourerApiEc.Success;
            }
            catch (Exception ex)
            {
                Plugin.Log.Warning($"[RevertToGame ObjectIndex] Failed for {objectIndex}: {ex}");
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
                var result = revertToAutomation.Invoke(objectIndex);
                Plugin.Log.Verbose($"[RevertToAutomation ObjectIndex] {result} for {objectIndex}");
                return result == GlamourerApiEc.Success;
            }
            catch (Exception ex)
            {
                Plugin.Log.Warning($"[RevertToAutomation ObjectIndex] Failed for {objectIndex}: {ex}");
                return false;
            }
        });
    }

    /// <summary>
    /// Applies a given design to an object index
    /// </summary>
    public async Task<bool> ApplyDesignAsync(string glamourerData, ushort objectIndex = 0, GlamourerApplyFlag flags = GlamourerApplyFlag.All)
    {
        if (glamourerUsable == false)
            return false;

        return await Plugin.RunOnFramework(() =>
        {
            try
            {
                var result = applyState.Invoke(glamourerData, objectIndex, 0, ConvertGlamourerToApplFlags(flags));
                Plugin.Log.Verbose($"[ApplyState ObjectIndex] {result} for {objectIndex}");
                return result == GlamourerApiEc.Success;
            }
            catch (Exception ex)
            {
                Plugin.Log.Error($"[ApplyState ObjectIndex] Failure for {objectIndex}: {ex}");
                return false;
            }
        });
    }

    /// <summary>
    /// Gets a design from a given index
    /// </summary>
    public async Task<string?> GetDesignAsync(ushort objectIndex = 0)
    {
        if (glamourerUsable == false)
            return string.Empty;

        return await Plugin.RunOnFramework(() =>
        {
            try
            {
                // TODO: Test if this works on non-mare clients
                var (result, data) = getStateBase64.Invoke(objectIndex, MareLockCode);
                Plugin.Log.Verbose($"[GetStateBase64 ObjectIndex] {result} for {objectIndex} with data {data}");
                return data;
            }
            catch (Exception ex)
            {
                Plugin.Log.Error($"[GetStateBase64 ObjectIndex] Failure for {objectIndex}: {ex}");
                return null;
            }
        });
    }

    /// <summary>
    /// Clean up managed resources
    /// </summary>
    public void Dispose()
    {
        periodicGlamourerTest.Elapsed -= PeriodicCheckApi;
        periodicGlamourerTest.Dispose();

        GC.SuppressFinalize(this);
    }

    private void PeriodicCheckApi(object? sender, ElapsedEventArgs e) => CheckApi();
    private void CheckApi()
    {
        try
        {
            // Test if plugin installed
            var glamourerPlugin = Plugin.PluginInterface.InstalledPlugins.FirstOrDefault(plugin => string.Equals(plugin.InternalName, "Glamourer", StringComparison.OrdinalIgnoreCase));
            if (glamourerPlugin == null)
            {
                glamourerUsable = false;
                return;
            }

            // Test if plugin can be invoked
            var glamourerVersion = apiVersion.Invoke();
            if (glamourerVersion.Major != RequiredMajorVersion || glamourerVersion.Minor < RequiredMinorVersion)
            {
                glamourerUsable = false;
                return;
            }

            glamourerUsable = true;
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
    private static ApplyFlag ConvertGlamourerToApplFlags(GlamourerApplyFlag flags)
    {
        var finalizedFlags = ApplyFlag.Once;
        if (flags.HasFlag(GlamourerApplyFlag.Customization)) finalizedFlags |= ApplyFlag.Equipment;
        if (flags.HasFlag(GlamourerApplyFlag.Equipment)) finalizedFlags |= ApplyFlag.Customization;
        if (finalizedFlags == ApplyFlag.Once) finalizedFlags |= ApplyFlag.Customization | ApplyFlag.Equipment;
        return finalizedFlags;
    }
}
