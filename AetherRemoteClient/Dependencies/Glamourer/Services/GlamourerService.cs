using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AetherRemoteClient.Dependencies.Glamourer.Domain;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Events;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteCommon.Domain.Enums;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using Glamourer.Api.Enums;
using Glamourer.Api.Helpers;
using Glamourer.Api.IpcSubscribers;
using Newtonsoft.Json.Linq;

namespace AetherRemoteClient.Dependencies.Glamourer.Services;

/// <summary>
///     Provides access to Glamourer
/// </summary>
public class GlamourerService : IExternalPlugin, IDisposable
{
    // Default folder for designs without homes
    private const string Uncategorized = "Uncategorized";
    
    // Const
    private const int ExpectedMajor = 1;
    private const int ExpectedMinor = 7;
    
    // Syncing solutions lock other player profiles to prevent tampering by the client. To view the glamourer data
    // of these other players, we need a key to unlock them. Since there are multiple services, we must try multiple
    // keys to account for all possibilities.
    private const uint MareLockCode = 0x6D617265; // Used by PlayerSync, LaciSynchroni, and Lightless
    private const uint BnuyLockCode = 0x626E7579; // Used by Snowcloak

    // An array containing all the lock codes for various syncing solutions
    private static readonly uint[] LockCodes = [MareLockCode, BnuyLockCode];
    
    // Glamourer Api
    private readonly ApiVersion _apiVersion;
    
    // Glamourer Api Design
    private readonly GetDesignBase64 _getDesignBase64;
    private readonly GetDesignListExtended _getDesignListExtended;
    
    // Glamourer Api State
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
    ///     <inheritdoc cref="IExternalPlugin.IpcReady"/>
    /// </summary>
    public event EventHandler? IpcReady;

    /// <summary>
    ///     <inheritdoc cref="GlamourerService"/>
    /// </summary>
    public GlamourerService()
    {
        _apiVersion = new ApiVersion(Plugin.PluginInterface);

        _getDesignBase64 = new GetDesignBase64(Plugin.PluginInterface);
        _getDesignListExtended = new GetDesignListExtended(Plugin.PluginInterface);
        
        _applyState = new ApplyState(Plugin.PluginInterface);
        _getState = new GetState(Plugin.PluginInterface);
        _getStateBase64 = new GetStateBase64(Plugin.PluginInterface);
        _revertState = new RevertState(Plugin.PluginInterface);
        _revertToAutomation = new RevertToAutomation(Plugin.PluginInterface);

        _stateChangedWithType = StateChangedWithType.Subscriber(Plugin.PluginInterface);
        _stateChangedWithType.Event += OnGlamourerStateChanged;
        
        _stateFinalizedWithType = StateFinalized.Subscriber(Plugin.PluginInterface);
        _stateFinalizedWithType.Event += OnGlamourerStateFinalized;
    }

    /// <summary>
    ///     Tests for availability to Glamourer
    /// </summary>
    public async Task<bool> TestIpcAvailability()
    {
        // Set everything to disabled state
        ApiAvailable = false;
        
        // Invoke Api
        var version = await Plugin.RunOnFrameworkSafely(() => _apiVersion.Invoke()).ConfigureAwait(false);

        // Test for proper versioning
        if (version.Major is not ExpectedMajor || version.Minor < ExpectedMinor)
            return false;

        // Mark as ready
        ApiAvailable = true;
        IpcReady?.Invoke(this, EventArgs.Empty);
        return true;
    }

    /// <summary>
    ///     Gets all the local player's glamourer designs
    /// </summary>
    public async Task<List<Design>?> GetDesignList()
    {
        if (ApiAvailable is false)
            return null;
        
        try
        {
            var designs = new List<Design>();
            foreach (var design in await Plugin.RunOnFramework(() => _getDesignListExtended.Invoke()).ConfigureAwait(false))
                designs.Add(new Design(design.Key, design.Value.DisplayName, design.Value.FullPath, design.Value.DisplayColor));

            return designs;
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[GlamourerService.GetDesignList] {e}");
            return null;
        }
    }
    
    /// <summary>
    ///     Reverts to original automation
    /// </summary>
    /// <param name="index">Object table index to revert</param>
    public async Task<bool> RevertToAutomation(ushort index)
    {
        if (!ApiAvailable)
        {
            Plugin.Log.Warning($"[GlamourerService.RevertToAutomation] Unable to revert index {index} because glamourer is not available");
            return false;
        }
        
        try
        {
            var result = await Plugin.RunOnFramework(() => _revertToAutomation.Invoke(index)).ConfigureAwait(false);
            return LogAndProcessResult("[RevertToAutomation]", index, result);
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[GlamourerService.RevertToAutomation] Actor index {index} failed to revert unexpectedly, {e}");
            return false;
        }
    }
    
    /// <summary>
    ///     Applies a given design to an object index
    /// </summary>
    /// <param name="glamourerData">Glamourer data to apply</param>
    /// <param name="flags">How should this be applied?</param>
    /// <param name="index">Object table index to revert</param>
    public async Task<bool> ApplyDesignAsync(string glamourerData, GlamourerApplyFlags flags, ushort index)
    {
        // Check if we can call this
        if (!ApiAvailable)
        {
            Plugin.Log.Warning($"[GlamourerService] [ApplyDesignAsync] Unable to apply design to actor index {index} because glamourer is not available");
            return false;
        }

        try
        {
            // Convert the flags to glamourer domain
            var converted = ConvertGlamourerToApplyFlags(flags);
        
            // Invoke the function
            var result = await Plugin.RunOnFramework(() => _applyState.Invoke(glamourerData, index, 0, converted)).ConfigureAwait(false);
        
            // Process results
            return LogAndProcessResult("[ApplyDesignAsync]", index, result);
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[GlamourerService] [ApplyDesignAsync] Actor index {index} failed to apply design unexpectedly, {e}");
            return false;
        }
    }

    /// <summary>
    ///     Applies a given design to an object index
    /// </summary>
    /// <param name="glamourerData">Glamourer data to apply</param>
    /// <param name="flags">How should this be applied?</param>
    /// <param name="index">Object table index to revert</param>
    public async Task<bool> ApplyDesignAsync(JObject glamourerData, GlamourerApplyFlags flags, ushort index)
    {
        // Check if we can call this
        if (!ApiAvailable)
        {
            Plugin.Log.Warning($"[GlamourerService] [ApplyDesignAsync] Unable to apply design to actor index {index} because glamourer is not available");
            return false;
        }

        try
        {
            // Convert the flags to glamourer domain
            var converted = ConvertGlamourerToApplyFlags(flags);
        
            // Invoke the function
            var result = await Plugin.RunOnFramework(() => _applyState.Invoke(glamourerData, index, 0, converted)).ConfigureAwait(false);
        
            // Process results
            return LogAndProcessResult("[ApplyDesignAsync]", index, result);
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[GlamourerService] [ApplyDesignAsync] Actor index {index} failed to apply design unexpectedly, {e}");
            return false;
        }
    }
    
    /// <summary>
    ///     Get a design from glamourer by providing Guid
    /// </summary>
    public async Task<string?> GetDesignAsync(Guid designId)
    {
        // Check if we can call this
        if (!ApiAvailable)
        {
            Plugin.Log.Warning("[GlamourerService.GetDesignAsync] Api not available");
            return null;
        }

        try
        {
            return await Plugin.RunOnFramework(() => _getDesignBase64.Invoke(designId)).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[GlamourerService.GetDesignAsync] An expected error occurred, {e}");
            return null;
        }
    }

    /// <summary>
    ///     Gets a design from a given index
    /// </summary>
    /// <param name="index">Object table index to get the design for</param>
    public async Task<string?> GetDesignAsync(ushort index)
    {
        // Check if we can call this
        if (!ApiAvailable)
        {
            Plugin.Log.Warning($"[GlamourerService] [GetDesignAsync] Unable to get design for actor index {index} because glamourer is not available");
            return null;
        }

        try
        {
            // Iterate over all the codes
            foreach (var key in LockCodes)
            {
                // Invoke the function
                var (code, data) = await Plugin.RunOnFramework(() => _getStateBase64.Invoke(index, key)).ConfigureAwait(false);
                
                // Process result
                switch (code)
                {
                    // If successful, just return the data
                    case GlamourerApiEc.Success:
                        return data;
                        
                    // If the key was invalid, just continue and try the next
                    case GlamourerApiEc.InvalidKey:
                        Plugin.Log.Verbose($"[GlamourerService] [GetDesignAsync] Key {code} was invalid");
                        continue;
                        
                    // Otherwise, just return null
                    case GlamourerApiEc.NothingDone:
                    case GlamourerApiEc.ActorNotFound:
                    case GlamourerApiEc.ActorNotHuman:
                    case GlamourerApiEc.DesignNotFound:
                    case GlamourerApiEc.ItemInvalid:
                    case GlamourerApiEc.InvalidState:
                    default:
                        Plugin.Log.Warning($"[GlamourerService] [GetDesignAsync] Key {code} resulted in an invalid state of {code}");
                        return null;
                }
            }
            
            Plugin.Log.Warning($"[GlamourerService] [GetDesignAsync] Could not find a valid key for actor {index}. This is likely due to using an unsupported syncing solution.");
            return null;
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"[GlamourerService] [GetDesignAsync] Getting design failed unexpectedly, {e}");
            return null;
        }
    }

    /// <summary>
    ///     Gets a design from a given index as a JSON object that contains all changed parts of a glamourer character
    /// </summary>
    public async Task<JObject?> GetDesignComponentsAsync(ushort index)
    {
        // Check if we can call this
        if (!ApiAvailable)
        {
            Plugin.Log.Warning($"[GlamourerService] [GetDesignComponentsAsync] Unable to get design for actor index {index} because glamourer is not available");
            return null;
        }
        
        try
        {
            // Iterate over all the codes
            foreach (var key in LockCodes)
            {
                // Invoke the function
                var (code, data) = await Plugin.RunOnFramework(() => _getState.Invoke(index, key)).ConfigureAwait(false);
                
                // Process result
                switch (code)
                {
                    // If successful, just return the data
                    case GlamourerApiEc.Success:
                        return data;
                        
                    // If the key was invalid, just continue and try the next
                    case GlamourerApiEc.InvalidKey:
                        Plugin.Log.Verbose($"[GlamourerService] [GetDesignComponentsAsync] Key {code} was invalid");
                        continue;
                        
                    // Otherwise, just return null
                    case GlamourerApiEc.NothingDone:
                    case GlamourerApiEc.ActorNotFound:
                    case GlamourerApiEc.ActorNotHuman:
                    case GlamourerApiEc.DesignNotFound:
                    case GlamourerApiEc.ItemInvalid:
                    case GlamourerApiEc.InvalidState:
                    default:
                        Plugin.Log.Warning($"[GlamourerService] [GetDesignComponentsAsync] Key {code} resulted in an invalid state of {code}");
                        return null;
                }
            }
            
            Plugin.Log.Warning($"[GlamourerService] [GetDesignComponentsAsync] Could not find a valid key for actor {index}. This is likely due to using an unsupported syncing solution.");
            return null;
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"[GlamourerService] [GetDesignComponentsAsync] Getting design failed unexpectedly, {e}");
            return null;
        }
    }
    
    /// <summary>
    ///     Converts domain <see cref="GlamourerApplyFlags"/> to Glamourer <see cref="ApplyFlag"/>
    /// </summary>
    private static ApplyFlag ConvertGlamourerToApplyFlags(GlamourerApplyFlags flags)
    {
        // Always start with the 'once' flag
        var apply = ApplyFlag.Once;
        
        // If the 'customization' flag is set, convert
        if ((flags & GlamourerApplyFlags.Customization) is not 0)
            apply |= ApplyFlag.Customization;
        
        // If the 'equipment' flag is set, convert
        if ((flags & GlamourerApplyFlags.Equipment) is not 0)
            apply |= ApplyFlag.Equipment;
        
        // If only the 'once' flag is set, add both 'customization' and 'equipment' flags
        if ((flags & (GlamourerApplyFlags.Customization | GlamourerApplyFlags.Equipment)) is 0)
            apply |= ApplyFlag.Customization | ApplyFlag.Equipment;
        
        return apply;
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

    /// <summary>
    ///     A light wrapper to log the resulting operation and convert to a boolean
    /// </summary>
    /// <param name="operation">The name of the operation. An example would be "RevertToGame"</param>
    /// <param name="actor">The target actor id</param>
    /// <param name="error">The resulting error code from the operation</param>
    private static bool LogAndProcessResult(string operation, ushort actor, GlamourerApiEc error)
    {
        switch (error)
        {
            case GlamourerApiEc.Success or GlamourerApiEc.NothingDone:
                return true;
            
            case GlamourerApiEc.ActorNotFound:
                Plugin.Log.Warning($"[GlamourerService] [{operation}] Actor {actor} not found");
                return false;
            
            case GlamourerApiEc.ActorNotHuman:
                Plugin.Log.Warning($"[GlamourerService] [{operation}] Actor {actor} is not a valid target");
                return false;
            
            case GlamourerApiEc.InvalidKey:
                Plugin.Log.Warning($"[GlamourerService] [{operation}] Invalid key. This likely means you are using an unsupported syncing solution");
                return false;
            
            default:
                Plugin.Log.Warning($"[GlamourerService] [{operation}] The operation did not success, {error}");
                return false;
        }
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