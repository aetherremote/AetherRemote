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
    private const ApplyFlag DefaultApplyFlags = ApplyFlag.Once | ApplyFlag.Customization | ApplyFlag.Equipment;

    // When Mare updates a local glamourer profile, it locks to prevent local tampering
    // Unfortunately, we need this key to unlock the profile to get the state.
    // https://github.com/Penumbra-Sync/client/blob/main/MareSynchronos/Interop/Ipc/IpcCallerGlamourer.cs#L31
    private const uint MareLockCode = 0x6D617265;

    // Glamourer Api
    private readonly ApiVersion glamourerApiVersion;
    private readonly ApplyStateName glamourerApiApplyDesign;
    private readonly GetStateBase64Name glamourerApiGetDesign;
    private readonly Timer periodicGlamourerTest;

    // Glamourer Installed?
    private bool glamourerUsable = false;

    /// <summary>
    /// <inheritdoc cref="GlamourerAccessor"/>
    /// </summary>
    public GlamourerAccessor()
    {
        glamourerApiVersion = new ApiVersion(Plugin.PluginInterface);
        glamourerApiApplyDesign = new ApplyStateName(Plugin.PluginInterface);
        glamourerApiGetDesign = new GetStateBase64Name(Plugin.PluginInterface);

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
    /// Applies a glamourer design to specified character.
    /// </summary>
    public async Task<bool> ApplyDesignAsync(string characterName, string glamourerData, GlamourerApplyFlag flags)
    {
        return await ApplyDesignAsync(characterName, glamourerData, Convert(flags));
    }

    /// <summary>
    /// Applies a glamourer design to specified character.
    /// </summary>
    public async Task<bool> ApplyDesignAsync(string characterName, string glamourerData, ApplyFlag flags = DefaultApplyFlags)
    {
        if (glamourerUsable == false)
            return false;

        var result = await RunOnFramework(() =>
        {
            try
            {
                Plugin.Log.Verbose($"Attempting to apply glamourer design {glamourerData} to {characterName} with flags {flags}");
                glamourerApiApplyDesign.Invoke(glamourerData, characterName, 0, flags);
                return true;
            }
            catch (Exception ex)
            {
                Plugin.Log.Error($"Something went wrong trying to apply glamourer design: {ex}");
                return false;
            }
        });

        return result;
    }

    /// <summary>
    /// Gets glamourer design from target character
    /// </summary>
    public async Task<string?> GetDesignAsync(string characterName)
    {
        if (glamourerUsable == false)
            return string.Empty;

        return await RunOnFramework(() =>
        {
            try
            {
                var result = glamourerApiGetDesign.Invoke(characterName);
                if (result.Item1 == GlamourerApiEc.InvalidKey)
                    result = glamourerApiGetDesign.Invoke(characterName, MareLockCode);

                return result.Item2;
            }
            catch (Exception ex)
            {
                Plugin.Log.Error($"Something went wrong trying to get glamourer design: {ex}");
                return string.Empty;
            }
        });
    }

    /// <summary>
    /// Converts from <see cref="AetherRemoteCommon"/> <see cref="GlamourerApplyFlag"/> to Glamourer <see cref="ApplyFlag"/>
    /// </summary>
    /// <param name="glamourerApplyFlags"></param>
    /// <returns></returns>
    public static ApplyFlag Convert(GlamourerApplyFlag glamourerApplyFlags) => (ApplyFlag)(ulong)glamourerApplyFlags;

    /// <summary>
    /// Do not utilize await in any functions passed to this function 
    /// </summary>
    private static async Task<T> RunOnFramework<T>(Func<T> func)
    {
        if (Plugin.Framework.IsInFrameworkUpdateThread)
            return func.Invoke();

        return await Plugin.Framework.RunOnFrameworkThread(func).ConfigureAwait(false);
    }

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
            var glamourerVersion = glamourerApiVersion.Invoke();
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

    private void PeriodicCheckApi(object? sender, ElapsedEventArgs e) => CheckApi();

    public void Dispose()
    {
        periodicGlamourerTest.Elapsed -= PeriodicCheckApi;
        periodicGlamourerTest.Dispose();

        GC.SuppressFinalize(this);
    }
}
