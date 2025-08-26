using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AetherRemoteClient.Domain.Interfaces;
using Penumbra.Api.Enums;
using Penumbra.Api.IpcSubscribers;

namespace AetherRemoteClient.Services.Dependencies;

/// <summary>
///     Provides access to Penumbra
/// </summary>
public class PenumbraService : IExternalPlugin
{
    // Const
    private const string TemporaryModName = "AetherRemoteMods";
    private const int Priority = 999;
    
    // Penumbra API
    private readonly AddTemporaryMod _addTemporaryMod;
    private readonly GetGameObjectResourcePaths _getGameObjectResourcePaths;
    private readonly GetMetaManipulations _getMetaManipulations;
    private readonly GetCollectionForObject _getCollectionForObject;
    private readonly RemoveTemporaryMod _removeTemporaryMod;
    private readonly RedrawObject _redrawObject;
    private readonly ApiVersion _version;
    
    /// <summary>
    ///     Is Penumbra available for use?
    /// </summary>
    public bool ApiAvailable;

    /// <summary>
    ///     <see cref="PenumbraService"/>
    /// </summary>
    public PenumbraService()
    {
        _addTemporaryMod = new AddTemporaryMod(Plugin.PluginInterface);
        _getGameObjectResourcePaths = new GetGameObjectResourcePaths(Plugin.PluginInterface);
        _getMetaManipulations = new GetMetaManipulations(Plugin.PluginInterface);
        _removeTemporaryMod = new RemoveTemporaryMod(Plugin.PluginInterface);
        _getCollectionForObject = new GetCollectionForObject(Plugin.PluginInterface);
        _redrawObject = new RedrawObject(Plugin.PluginInterface);
        _version = new ApiVersion(Plugin.PluginInterface);
        
        TestIpcAvailability();
    }
    
    /// <summary>
    ///     Tests for availability to Penumbra
    /// </summary>
    public void TestIpcAvailability()
    {
        try
        {
            ApiAvailable = _version.Invoke().Breaking > 4;
        }
        catch (Exception)
        {
            ApiAvailable = false;
        }
    }
    
    /// <summary>
    ///     Calls penumbra's GetGameObjectResourcePaths function
    /// </summary>
    /// <returns>A list of modified objects, mapping the modified object to the target path</returns>
    public async Task<Dictionary<string, string>> GetGameObjectResourcePaths(ushort index)
    {
        // TODO: There is an issue with multiple named files having the same name causing issues
        //          An example of which would be if someone has ../../something_SOMETHING.tex
        //          and ../../something_something.tex
        //          There is a possibility of fixing this just by forcing everything lowercase
        //          however I don't know enough about mod paths to know if this is problematic
        
        if (ApiAvailable)
            return await Plugin.RunOnFramework(() =>
            {
                try
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
                                if (item.EndsWith(".imc") || kvp.Key.EndsWith(".imc"))
                                {
                                    Plugin.Log.Verbose($"Skipping .imc redirect {item} --> {kvp.Key}");
                                    continue;
                                }
                                
                                paths.TryAdd(item, kvp.Key);
                            }
                        }
                    }

                    return paths;
                }
                catch (Exception e)
                {
                    Plugin.Log.Warning(
                        $"[PenumbraService] Unexpectedly failed getting resource paths for index {index}, {e.Message}");
                    return [];
                }
            }).ConfigureAwait(false);

        Plugin.Log.Warning(
            $"[PenumbraService] Failed to get object resource paths for index {index} because penumbra is not available");
        return [];
    }

    /// <summary>
    ///     Calls penumbra's GetMetaManipulations function
    /// </summary>
    /// <returns>A character's metadata</returns>
    public async Task<string> GetMetaManipulations(ushort index)
    {
        if (ApiAvailable)
            return await Plugin.RunOnFramework(() =>
            {
                try
                {
                    return _getMetaManipulations.Invoke(index);
                }
                catch (Exception e)
                {
                    Plugin.Log.Warning(
                        $"[PenumbraService] Unexpectedly failed getting manipulations for index {index}, {e.Message}");
                    return string.Empty;
                }
            }).ConfigureAwait(false);

        Plugin.Log.Warning(
            $"[PenumbraService] Failed to get manipulations for index {index} because penumbra is not available");
        return string.Empty;
    }

    /// <summary>
    ///     Calls penumbra's GetCollectionForObject function
    /// </summary>
    /// <param name="index">Object table index of the collection to get, defaulted to the local player's</param>
    /// <returns>Collection GUID</returns>
    public async Task<Guid> GetCollection(int index = 0)
    {
        if (ApiAvailable)
            return await Plugin.RunOnFramework(() =>
            {
                try
                {
                    return _getCollectionForObject.Invoke(index).EffectiveCollection.Id;
                }
                catch (Exception e)
                {
                    Plugin.Log.Warning(
                        $"[PenumbraService] Unexpectedly failed to get collection for index {index}, {e.Message}");
                    return Guid.Empty;
                }
            }).ConfigureAwait(false);

        Plugin.Log.Warning(
            $"[PenumbraService] Failed to get collection for index {index} because penumbra is not available");
        return Guid.Empty;
    }

    /// <summary>
    ///     Calls penumbra's AddTemporaryMod function
    /// </summary>
    /// <returns><see cref="bool"/> indicating success</returns>
    public async Task<bool> AddTemporaryMod(Guid collectionGuid, Dictionary<string, string> modifiedPaths, string meta)
    {
        if (ApiAvailable)
            return await Plugin.RunOnFramework(() =>
            {
                try
                {
                    var result = _addTemporaryMod.Invoke(TemporaryModName, collectionGuid, modifiedPaths, meta, Priority);
                    if (result is PenumbraApiEc.Success)
                        return true;

                    Plugin.Log.Warning($"[PenumbraService] Adding temporary mod was unsuccessful, result was {result}");
                    return false;
                }
                catch (Exception e)
                {
                    Plugin.Log.Warning($"[PenumbraService] Adding temporary mod failed unexpectedly, {e.Message}");
                    return false;
                }
            }).ConfigureAwait(false);

        Plugin.Log.Warning("[PenumbraService] Unable to add temporary mod because penumbra is not available");
        return false;
    }

    /// <summary>
    ///     Calls penumbra's RemoveTemporaryMod function
    /// </summary>
    /// <returns><see cref="bool"/> indicating success</returns>
    public async Task<bool> CallRemoveTemporaryMod(Guid collectionId)
    {
        if (ApiAvailable)
            return await Plugin.RunOnFramework(() =>
            {
                try
                {
                    var result = _removeTemporaryMod.Invoke(TemporaryModName, collectionId, Priority);
                    if (result is PenumbraApiEc.Success or PenumbraApiEc.NothingChanged)
                        return true;

                    Plugin.Log.Warning(
                        $"[PenumbraService] Removing temporary mod was unsuccessful, result was {result}");
                    return false;
                }
                catch (Exception e)
                {
                    Plugin.Log.Warning($"[PenumbraService] Removing temporary mod failed unexpectedly, {e.Message}");
                    return false;
                }
            }).ConfigureAwait(false);

        Plugin.Log.Warning("[PenumbraService] Unable to remove temporary mod because penumbra is not available");
        return false;
    }

    /// <summary>
    ///     Redraws the local client
    /// </summary>
    public async Task<bool> CallRedraw(int objectIndex = 0)
    {
        if (ApiAvailable)
            return await Plugin.RunOnFramework(() =>
            {
                try
                {
                    _redrawObject.Invoke(objectIndex);
                    return true;
                }
                catch (Exception e)
                {
                    Plugin.Log.Warning($"[PenumbraService] Redrawing failed unexpectedly, {e.Message}");
                    return false;
                }
            }).ConfigureAwait(false);

        Plugin.Log.Warning("[PenumbraService] Unable to redraw because penumbra is not available");
        return false;
    }
}