using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Utils;
using Penumbra.Api.Enums;
using Penumbra.Api.IpcSubscribers;

namespace AetherRemoteClient.Services;

/// <summary>
///     Provides access to Penumbra
/// </summary>
public class PenumbraService : IExternalPlugin
{
    // Const
    private const string TemporaryModName = "AetherRemoteMods";
    private const int Priority = int.MaxValue - 32; // Give other mods the opportunity to override if needed;
    
    // Const
    private const int ExpectedMajor = 4;
    
    // Penumbra API
    private readonly AddTemporaryMod _addTemporaryMod = new(Plugin.PluginInterface);
    private readonly ApiVersion _apiVersion = new(Plugin.PluginInterface);
    private readonly GetGameObjectResourcePaths _getGameObjectResourcePaths = new(Plugin.PluginInterface);
    private readonly GetMetaManipulations _getMetaManipulations = new(Plugin.PluginInterface);
    private readonly GetCollectionForObject _getCollectionForObject = new(Plugin.PluginInterface);
    private readonly RemoveTemporaryMod _removeTemporaryMod = new(Plugin.PluginInterface);
    
    /// <summary>
    ///     Is Penumbra available for use?
    /// </summary>
    public bool ApiAvailable;
        
    /// <summary>
    ///     <inheritdoc cref="IExternalPlugin.IpcReady"/>
    /// </summary>
    public event EventHandler? IpcReady;
    
    /// <summary>
    ///     Tests for availability to Penumbra
    /// </summary>
    public async Task<bool> TestIpcAvailability()
    {
        // Set everything to disabled state
        ApiAvailable = false;

        try
        {
            // Invoke Api
            var version = await DalamudUtilities.RunOnFramework(() => _apiVersion.Invoke()).ConfigureAwait(false);
            
            // Test for proper versioning
            if (version.Breaking < ExpectedMajor)
                return false;
            
            // Mark as ready
            ApiAvailable = true;
            IpcReady?.Invoke(this, EventArgs.Empty);
            return true;
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[PenumbraService.TestIpcAvailability] {e}");
            return false;
        }
    }
    
    // TODO: Bring this in line with an error state returning null instead of an empty dictionary
    /// <summary>
    ///     Calls penumbra's GetGameObjectResourcePaths function
    /// </summary>
    /// <returns>A list of modified objects, mapping the modified object to the target path</returns>
    public async Task<Dictionary<string, string>> GetGameObjectResourcePaths(ushort index)
    {
        if (ApiAvailable is false)
        {
            Plugin.Log.Warning("[PenumbraService.GetGameObjectResourcePaths] Api not available");
            return [];
        }

        try
        {
            return await DalamudUtilities.RunOnFramework(() =>
            {
                var resources = _getGameObjectResourcePaths.Invoke(index);
                var paths = new Dictionary<string, string>();
                foreach (var resource in resources)
                {
                    if (resource is null)
                        continue;
                    
                    foreach (var kvp in resource)
                    {
                        foreach (var item in kvp.Value)
                        {
                            // Penumbra does not allow .imc redirects
                            if (item.EndsWith(".imc") || kvp.Key.EndsWith(".imc"))
                                continue;
                                
                            paths.TryAdd(item, kvp.Key);
                        }
                    }
                }

                return paths;
            }).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[PenumbraService.GetGameObjectResourcePaths] {e}");
            return [];
        }
    }

    // TODO: Bring this in line with an error state returning null instead of an empty string
    /// <summary>
    ///     Calls penumbra's GetMetaManipulations function
    /// </summary>
    /// <returns>A character's metadata</returns>
    public async Task<string> GetMetaManipulations(ushort index)
    {
        if (ApiAvailable is false)
        {
            Plugin.Log.Warning("[PenumbraService.GetMetaManipulations] Api not available");
            return string.Empty;
        }

        try
        {
            return await DalamudUtilities.RunOnFramework(() => _getMetaManipulations.Invoke(index)).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[PenumbraService.GetMetaManipulations] {e}");
            return string.Empty;
        }
    }

    // TODO: Look into making this maybe nullable to where if something fails it is a clue error state?
    /// <summary>
    ///     Calls penumbra's GetCollectionForObject function
    /// </summary>
    /// <param name="index">Object table index of the collection to get, defaulted to the local player's</param>
    /// <returns>Collection GUID</returns>
    public async Task<Guid> GetCollection(int index = 0)
    {
        if (ApiAvailable is false)
        {
            Plugin.Log.Warning("[PenumbraService.GetCollection] Api not available");
            return Guid.Empty;
        }

        try
        {
            return await DalamudUtilities.RunOnFramework(() => _getCollectionForObject.Invoke(index).EffectiveCollection.Id).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[PenumbraService.GetCollection] {e}");
            return Guid.Empty;
        }
    }

    /// <summary>
    ///     Calls penumbra's AddTemporaryMod function
    /// </summary>
    /// <returns><see cref="bool"/> indicating success</returns>
    public async Task<bool> AddTemporaryMod(Guid collectionGuid, Dictionary<string, string> modifiedPaths, string meta)
    {
        if (ApiAvailable is false)
        {
            Plugin.Log.Warning("[PenumbraService.AddTemporaryMod] Api not available");
            return false;
        }

        try
        {
            var result = await DalamudUtilities.RunOnFramework(() => _addTemporaryMod.Invoke(TemporaryModName, collectionGuid, modifiedPaths, meta, Priority)).ConfigureAwait(false);
            switch (result)
            {
                case PenumbraApiEc.Success:
                    return true;
                        
                case PenumbraApiEc.InvalidGamePath:
                    NotificationHelper.Error("Invalid Game Files", "The person you are being transformed into contains mods that do not all have valid game file paths. Most commonly, this happens when a mod maker includes two or more assets with the same name, but different punctuation: \"aether_remote.png\" and \"Aether_Remote.png\" as an example.");
                    break;
            }

            Plugin.Log.Warning($"[PenumbraService.AddTemporaryMod] Adding temporary mod was unsuccessful, result was {result}");
            return false;
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[PenumbraService.AddTemporaryMod] {e}");
            return false;
        }
    }

    /// <summary>
    ///     Removes the temporary mod collection from the specified collection
    /// </summary>
    /// <remarks>Returns true if the result is Success, NothingChanged, or CollectionMissing</remarks>
    public async Task<bool> RemoveTemporaryMod(Guid collectionId)
    {
        if (ApiAvailable is false)
        {
            Plugin.Log.Warning("[PenumbraService.RemoveTemporaryMod] Api not available");
            return false;
        }
        
        try
        {
            return await DalamudUtilities.RunOnFramework(() =>
            {
                var result = _removeTemporaryMod.Invoke(TemporaryModName, collectionId, Priority);
                if (result is PenumbraApiEc.Success or PenumbraApiEc.NothingChanged or PenumbraApiEc.CollectionMissing)
                    return true;
                
                Plugin.Log.Warning($"[PenumbraService.RemoveTemporaryMod] Ipc call unsuccessful, {result}");
                return false;
            }).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[PenumbraService.RemoveTemporaryMod] {e}");
            return false;
        }
    }
}