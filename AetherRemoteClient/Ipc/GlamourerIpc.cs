using System;
using System.Threading.Tasks;
using AetherRemoteClient.Domain.Events;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteCommon.Domain.Enums;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using Glamourer.Api.Enums;
using Glamourer.Api.Helpers;
using Glamourer.Api.IpcSubscribers;
using Newtonsoft.Json.Linq;

namespace AetherRemoteClient.Ipc;

/// <summary>
///     Provides access to Glamourer
/// </summary>
public class GlamourerIpc : IExternalPlugin, IDisposable
{
    // When Mare updates a local glamourer profile, it locks to prevent local tampering.
    // Unfortunately, we need this key to unlock the profile to get the state.
    // https://github.com/Penumbra-Sync/client/blob/main/MareSynchronos/Interop/Ipc/IpcCallerGlamourer.cs#L31
    private const uint MareLockCode = 0x6D617265;

    // Glamourer Api
    private readonly ApiVersion _apiVersion;
    private readonly ApplyState _applyState;
    private readonly GetState _getState;
    private readonly GetStateBase64 _getStateBase64;
    private readonly RevertToAutomation _revertToAutomation;

    // Glamourer Events
    private readonly EventSubscriber<IntPtr, StateFinalizationType> _stateFinalizedWithType;

    /// <summary>
    ///     Event fired when the local player's character is reverted to game or automation
    /// </summary>
    public event EventHandler<GlamourerStateChangedEventArgs>? LocalPlayerResetOrReapply;

    /// <summary>
    ///     Event fired every time glamourer detects a change
    /// </summary>
    public event EventHandler<GlamourerStateChangedEventArgs>? LocalPlayerChanged;

    /// <summary>
    ///     Is Glamourer available for use?
    /// </summary>
    public bool ApiAvailable;

    /// <summary>
    ///     <inheritdoc cref="GlamourerIpc"/>
    /// </summary>
    public GlamourerIpc()
    {
        _apiVersion = new ApiVersion(Plugin.PluginInterface);
        _applyState = new ApplyState(Plugin.PluginInterface);
        _getState = new GetState(Plugin.PluginInterface);
        _getStateBase64 = new GetStateBase64(Plugin.PluginInterface);
        _revertToAutomation = new RevertToAutomation(Plugin.PluginInterface);

        _stateFinalizedWithType = StateFinalized.Subscriber(Plugin.PluginInterface);
        _stateFinalizedWithType.Event += OnGlamourerStateChanged;

        TestIpcAvailability();
    }

    /// <summary>
    ///     Tests for availability to Glamourer
    /// </summary>
    public void TestIpcAvailability()
    {
        try
        {
            ApiAvailable = _apiVersion.Invoke() is { Major: 1, Minor: > 3 };
        }
        catch (Exception)
        {
            ApiAvailable = false;
        }
    }

    /// <summary>
    ///     Reverts to original automation
    /// </summary>
    /// <param name="index">Object table index to revert</param>
    public async Task<bool> RevertToAutomation(ushort index = 0)
    {
        if (ApiAvailable)
            return await Plugin.RunOnFramework(() =>
            {
                try
                {
                    var result = _revertToAutomation.Invoke(index);
                    if (result is GlamourerApiEc.Success)
                        return true;

                    Plugin.Log.Warning($"[GlamourerIpc] Reverting object index {index} unsuccessful, {result}");
                    return false;
                }
                catch (Exception e)
                {
                    Plugin.Log.Warning($"[GlamourerIpc] Reverting object index {index} failed, {e.Message}");
                    return false;
                }
            }).ConfigureAwait(false);

        Plugin.Log.Warning($"[GlamourerIpc] Unable to revert index {index} because glamourer is not available");
        return false;
    }

    /// <summary>
    ///     Applies a given design to an object index
    /// </summary>
    /// <param name="glamourerData">Glamourer data to apply</param>
    /// <param name="flags">How should this be applied?</param>
    /// <param name="index">Object table index to revert</param>
    public async Task<bool> ApplyDesignAsync(string glamourerData, GlamourerApplyFlags flags, ushort index = 0)
    {
        return await ApplyDesign(glamourerData, flags, index);
    }

    /// <summary>
    ///     Applies a given design to an object index
    /// </summary>
    /// <param name="glamourerData">Glamourer data to apply</param>
    /// <param name="flags">How should this be applied?</param>
    /// <param name="index">Object table index to revert</param>
    public async Task<bool> ApplyDesignAsync(JObject glamourerData, GlamourerApplyFlags flags, ushort index = 0)
    {
        return await ApplyDesign(glamourerData, flags, index);
    }

    /// <summary>
    ///     Applies a given design to an object index by <see cref="string"/> or <see cref="JObject"/>
    /// </summary>
    private async Task<bool> ApplyDesign(object glamourerData, GlamourerApplyFlags flags, ushort index)
    {
        if (ApiAvailable)
            return await Plugin.RunOnFramework(() =>
            {
                try
                {
                    GlamourerApiEc result;
                    switch (glamourerData)
                    {
                        case JObject data:
                            result = _applyState.Invoke(data, index, 0, ConvertGlamourerToApplyFlags(flags));
                            break;
                        case string data:
                            result = _applyState.Invoke(data, index, 0, ConvertGlamourerToApplyFlags(flags));
                            break;
                        default:
                            Plugin.Log.Warning(
                                $"[GlamourerIpc] Unsupported glamourer data type while applying design {glamourerData.GetType().Name}");
                            return false;
                    }

                    if (result is GlamourerApiEc.Success)
                        return true;

                    Plugin.Log.Warning($"[GlamourerIpc] Unable to apply design to object {index}, {result}");
                    return false;
                }
                catch (Exception e)
                {
                    Plugin.Log.Error($"[GlamourerIpc] Unexpected error while applying design to object {index}, {e}");
                    return false;
                }
            });
        
        Plugin.Log.Warning("[GlamourerIpc] Unable to revert to automation because glamourer is not available");
        return false;
    }

    /// <summary>
    ///     Gets a design from a given index
    /// </summary>
    /// <param name="index">Object table index to get the design for</param>
    /// <param name="key">Key to get this object</param>
    public async Task<string?> GetDesignAsync(ushort index = 0, uint key = MareLockCode)
    {
        if (ApiAvailable)
            return await Plugin.RunOnFramework(() =>
            {
                try
                {
                    var (_, data) = _getStateBase64.Invoke(index, key);
                    return data;
                }
                catch (Exception e)
                {
                    Plugin.Log.Error(
                        $"[GlamourerIpc] Failed unexpectedly to get design for object index {index}, {e.Message}");
                    return null;
                }
            }).ConfigureAwait(false);

        Plugin.Log.Warning("[GlamourerIpc] Unable to get design because glamourer is not available");
        return null;
    }

    /// <summary>
    ///     Gets a design from a given index as a JSON object that contains all changed parts of a glamourer character
    /// </summary>
    /// <param name="index"></param>
    public async Task<JObject?> GetDesignComponentsAsync(ushort index = 0)
    {
        if (ApiAvailable)
            return await Plugin.RunOnFramework(() =>
            {
                try
                {
                    var (_, data) = _getState.Invoke(index);
                    return data;
                }
                catch (Exception e)
                {
                    Plugin.Log.Error(
                        $"[GlamourerIpc] Failed unexpectedly to get design components for object index {index}, {e.Message}");
                    return null;
                }
            }).ConfigureAwait(false);

        Plugin.Log.Warning("[GlamourerIpc] Unable to get design because glamourer is not available");
        return null;
    }
    
    /// <summary>
    ///     Converts domain <see cref="GlamourerApplyFlags"/> to Glamourer <see cref="ApplyFlag"/>
    /// </summary>
    private static ApplyFlag ConvertGlamourerToApplyFlags(GlamourerApplyFlags flags)
    {
        var applyFlags = ApplyFlag.Once;
        if (flags.HasFlag(GlamourerApplyFlags.Customization)) applyFlags |= ApplyFlag.Customization;
        if (flags.HasFlag(GlamourerApplyFlags.Equipment)) applyFlags |= ApplyFlag.Equipment;
        if (applyFlags is ApplyFlag.Once) applyFlags |= ApplyFlag.Customization | ApplyFlag.Equipment;
        return applyFlags;
    }

    private unsafe void OnGlamourerStateChanged(IntPtr objectIndexPointer, StateFinalizationType stateChangeType)
    {
        try
        {
            var objectIndex = (GameObject*)objectIndexPointer;
            if (objectIndex->ObjectIndex is not 0)
                return;
            
            LocalPlayerChanged?.Invoke(this, new GlamourerStateChangedEventArgs(stateChangeType));

            switch (stateChangeType)
            {
                case StateFinalizationType.Revert:
                case StateFinalizationType.RevertCustomize:
                case StateFinalizationType.RevertEquipment:
                case StateFinalizationType.RevertAdvanced:
                case StateFinalizationType.RevertAutomation:
                case StateFinalizationType.Reapply:
                case StateFinalizationType.ReapplyAutomation:
                    LocalPlayerResetOrReapply?.Invoke(this, new GlamourerStateChangedEventArgs(stateChangeType));
                    break;
                
                case StateFinalizationType.ModelChange:
                case StateFinalizationType.DesignApplied:
                case StateFinalizationType.Gearset:
                default:
                    Plugin.Log.Verbose("[GlamourerIpc] Ignored state change type {0}", stateChangeType);
                    break;
            }
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[GlamourerIpc] Unexpectedly failed processing glamourer state change, {e.Message}");
        }
    }

    public void Dispose()
    {
        _stateFinalizedWithType.Event -= OnGlamourerStateChanged;
        _stateFinalizedWithType.Disable();
        GC.SuppressFinalize(this);
    }
}