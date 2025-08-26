using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using AetherRemoteClient.Domain.Events;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteCommon.Domain.Enums;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using Glamourer.Api.Enums;
using Glamourer.Api.Helpers;
using Glamourer.Api.IpcSubscribers;
using Newtonsoft.Json.Linq;

namespace AetherRemoteClient.Services.Dependencies;

/// <summary>
///     Provides access to Glamourer
/// </summary>
public class GlamourerService : IExternalPlugin, IDisposable
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
    private readonly RevertState _revertState;
    private readonly RevertToAutomation _revertToAutomation;

    // Glamourer Events
    private readonly EventSubscriber<IntPtr, StateChangeType> _stateChangedWithType;
    private readonly EventSubscriber<IntPtr, StateFinalizationType> _stateFinalizedWithType;

    /// <summary>
    ///     Event fired when the local player's character is reverted to game or automation
    /// </summary>
    public event EventHandler<GlamourerStateChangedEventArgs>? LocalPlayerResetOrReapply;

    /// <summary>
    ///     Event fired every time glamourer detects a change
    /// </summary>
    public event EventHandler? LocalPlayerChanged;

    /// <summary>
    ///     Is Glamourer available for use?
    /// </summary>
    public bool ApiAvailable;

    /// <summary>
    ///     <inheritdoc cref="GlamourerService"/>
    /// </summary>
    public GlamourerService()
    {
        _apiVersion = new ApiVersion(Plugin.PluginInterface);
        _applyState = new ApplyState(Plugin.PluginInterface);
        _getState = new GetState(Plugin.PluginInterface);
        _getStateBase64 = new GetStateBase64(Plugin.PluginInterface);
        _revertState = new RevertState(Plugin.PluginInterface);
        _revertToAutomation = new RevertToAutomation(Plugin.PluginInterface);

        _stateChangedWithType = StateChangedWithType.Subscriber(Plugin.PluginInterface);
        _stateChangedWithType.Event += OnGlamourerStateChanged;
        
        _stateFinalizedWithType = StateFinalized.Subscriber(Plugin.PluginInterface);
        _stateFinalizedWithType.Event += OnGlamourerStateFinalized;

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
    public async Task<bool> RevertToGame(ushort index = 0)
    {
        if (ApiAvailable)
            return await Plugin.RunOnFramework(() =>
            {
                try
                {
                    var result = _revertState.Invoke(index, 0, ApplyFlag.Equipment | ApplyFlag.Customization | ApplyFlag.Once);
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
                            Plugin.Log.Warning($"[GlamourerIpc] Unsupported glamourer data type while applying design {glamourerData.GetType().Name}");
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
    public async Task<JObject?> GetDesignComponentsAsync(ushort index = 0, uint key = MareLockCode)
    {
        if (ApiAvailable)
            return await Plugin.RunOnFramework(() =>
            {
                try
                {
                    var (_, data) = _getState.Invoke(index, key);
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
    
    private unsafe void OnGlamourerStateChanged(IntPtr objectIndexPointer, StateChangeType stateChangeType)
    {
        try
        {
            // Ignore everything that isn't the local player
            var objectIndex = (GameObject*)objectIndexPointer;
            if (objectIndex->ObjectIndex is not 0)
                return;
            
            // Invoke
            LocalPlayerChanged?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[GlamourerIpc] Unexpectedly failed processing glamourer state change, {e.Message}");
        }
    }

    private unsafe void OnGlamourerStateFinalized(IntPtr objectIndexPointer, StateFinalizationType stateFinalizationType)
    {
        try
        {
            // Ignore everything that isn't the local player
            var objectIndex = (GameObject*)objectIndexPointer;
            if (objectIndex->ObjectIndex is not 0)
                return;
            
            switch (stateFinalizationType)
            {
                case StateFinalizationType.Revert:
                case StateFinalizationType.RevertCustomize:
                case StateFinalizationType.RevertEquipment:
                case StateFinalizationType.RevertAdvanced:
                case StateFinalizationType.RevertAutomation:
                case StateFinalizationType.Reapply:
                case StateFinalizationType.ReapplyAutomation:
                    LocalPlayerResetOrReapply?.Invoke(this, new GlamourerStateChangedEventArgs(stateFinalizationType));
                    break;
                
                case StateFinalizationType.ModelChange:
                case StateFinalizationType.DesignApplied:
                case StateFinalizationType.Gearset:
                default:
                    Plugin.Log.Verbose("[GlamourerIpc] Ignored state change type {0}", stateFinalizationType);
                    break;
            }
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[GlamourerIpc] Unexpectedly failed processing glamourer state finalization, {e.Message}");
        }
    }

    public static JObject? CreateJObjectToRevertExistingAdvancedDyes(JObject currentDesign, JObject targetDesign)
    {
        // Create a new copy
        if (targetDesign.DeepClone() is not JObject copiedTargetDesign)
        {
            Plugin.Log.Warning("[GlamourerIpc] Could not clone target JObject");
            return null;
        }
        
        // Get materials of both sets
        if (currentDesign["Materials"] is not JObject currentDesignMaterials)
        {
            return null;
        }
        
        if (targetDesign["Materials"] is not JObject targetDesignMaterials)
        {
            return null;
        }

        // Iterate over all the advanced dyes from the existing JObject
        foreach (var material in currentDesignMaterials.Properties())
        {
            // Target has the material, which will always get updated
            if (targetDesign.ContainsKey(material.Name))
                continue;
            
            // Copy to a new object to avoid referencing even though this is just a JObject
            if (material.Value.DeepClone() is not JObject copy)
            {
                Plugin.Log.Warning($"[GlamourerIpc] Could not clone material {material.Value}");
                continue;
            }
            // Set the revert field to be true
            copy["Revert"] = true;

            // Assign that newly created material (which contains pending revert data) to the target properties.
            targetDesignMaterials[material.Name] = copy;
        }

        return copiedTargetDesign;
    }

    /// <summary>
    ///     Converts a glamourer string of character data into a JObject
    /// </summary>
    public static JObject? ConvertGlamourerBase64StringToJObject(string base64String)
    {
        try
        {
            // Convert from string64 to bytes
            var compressedBytes = Convert.FromBase64String(base64String);
        
            // Glamourer compresses bytes, see https://github.com/Ottermandias/Glamourer/blob/main/Glamourer/Api/StateApi.cs#L311
            var decompressedBytes = Decompress(compressedBytes);
        
            // Convert to raw JSON string
            var raw = Encoding.UTF8.GetString(decompressedBytes);
        
            // Parse
            return JObject.Parse(raw);
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"[GlamourerIpc] Unexpectedly failed converting glamourer base 64 string, {e}");
            return null;
        }
    }

    /// <summary>
    ///     <see href="https://github.com/Ottermandias/Glamourer/blob/d6df9885dcc15940e1263d50533b1afd3d6c05a8/Glamourer/Utility/CompressExtensions.cs#L32-L42">Decompression method matching glamourer</see>
    /// </summary>
    private static byte[] Decompress(byte[] compressed)
    {
        // Add the bytes to a stream
        using var stream = new MemoryStream(compressed, 1, compressed.Length - 1);
        
        // Decompress the stream 
        using var zip = new GZipStream(stream, CompressionMode.Decompress);
        
        // Create a results buffer
        using var results = new MemoryStream();
        
        // Copy the decompressed bytes to the new buffer
        zip.CopyTo(results);
        
        // Convert stream to byte array and return
        return results.ToArray();
    }

    public void Dispose()
    {
        _stateChangedWithType.Event -= OnGlamourerStateChanged;
        _stateChangedWithType.Disable();
        
        _stateFinalizedWithType.Event -= OnGlamourerStateFinalized;
        _stateFinalizedWithType.Disable();
        GC.SuppressFinalize(this);
    }
}