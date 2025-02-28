using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Penumbra.Api.Enums;
using Penumbra.Api.IpcSubscribers;

namespace AetherRemoteClient.Services.External;

/// <summary>
///     Provides access to Penumbra IPCs
/// </summary>
public class PenumbraService : IDisposable
{
    // Const
    private const int TestApiIntervalInSeconds = 45;

    // Penumbra API
    private readonly AddTemporaryMod _addTemporaryMod;
    private readonly ApiVersion _apiVersion;
    private readonly GetGameObjectResourcePaths _getGameObjectResourcePaths;
    private readonly GetMetaManipulations _getMetaManipulations;
    private readonly GetCollectionForObject _getCollectionForObject;
    
    // Check Penumbra Api
    private readonly Timer _periodicPenumbraTest;
    private readonly RemoveTemporaryMod _removeTemporaryMod;

    /// <summary>
    ///     Is the penumbra api available for use?
    /// </summary>
    private bool _penumbraAvailable;
    
    /// <summary>
    ///     <inheritdoc cref="PenumbraService"/>
    /// </summary>
    public PenumbraService()
    {
        _addTemporaryMod = new AddTemporaryMod(Plugin.PluginInterface);
        _apiVersion = new ApiVersion(Plugin.PluginInterface);
        _getGameObjectResourcePaths = new GetGameObjectResourcePaths(Plugin.PluginInterface);
        _getMetaManipulations = new GetMetaManipulations(Plugin.PluginInterface);
        _removeTemporaryMod = new RemoveTemporaryMod(Plugin.PluginInterface);
        _getCollectionForObject = new GetCollectionForObject(Plugin.PluginInterface);
        
        _periodicPenumbraTest = new Timer(TestApiIntervalInSeconds * 1000);
        _periodicPenumbraTest.AutoReset = true;
        _periodicPenumbraTest.Elapsed += PeriodicCheckApi;
        _periodicPenumbraTest.Start();

        PeriodicCheckApi();
    }

    /// <summary>
    ///     Calls penumbra's GetGameObjectResourcePaths function
    /// </summary>
    /// <returns>A list of modified objects, mapping the modified object to the target path</returns>
    public async Task<Dictionary<string, string>> GetGameObjectResourcePaths(ushort objectIndex)
    {
        if (_penumbraAvailable is false)
        {
            Plugin.Log.Warning("[PenumbraService] [CallGetGameObjectResourcePaths] Penumbra is not installed!");
            return [];
        }

        var gameObjectResourcePaths = await Plugin.RunOnFramework(() =>
        {
            try
            {
                return _getGameObjectResourcePaths.Invoke(objectIndex);
            }
            catch (Exception ex)
            {
                Plugin.Log.Warning($"[PenumbraService] [CallGetGameObjectResourcePaths] Failure, {ex}");
                return [];
            }
        }).ConfigureAwait(false);
        
        var paths = new Dictionary<string, string>();
        foreach (var resource in gameObjectResourcePaths)
        {
            if (resource is null)
                continue;

            foreach (var kvp in resource)
            {
                //if (kvp.Value.Count is 1 && kvp.Key == kvp.Value.First())
                //    continue;
                //

                foreach (var item in kvp.Value)
                    paths.Add(item, kvp.Key);
            }
        }
        
        return paths;
    }

    /// <summary>
    ///     Calls penumbra's GetMetaManipulations function
    /// </summary>
    /// <returns>A character's metadata</returns>
    public async Task<string> GetMetaManipulations(ushort objectIndex)
    {
        if (_penumbraAvailable)
            return await Plugin.RunOnFramework(() =>
            {
                try
                {
                    return _getMetaManipulations.Invoke(objectIndex);
                }
                catch (Exception ex)
                {
                    Plugin.Log.Warning($"[PenumbraService] [CallGetMetaManipulations] Failure, {ex}");
                    return string.Empty;
                }
            }).ConfigureAwait(false);
        
        Plugin.Log.Warning("[PenumbraService] [GetMetaManipulations] Penumbra is not installed!");
        return string.Empty;
    }

    /// <summary>
    ///     Calls penumbra's GetCollectionForObject function
    /// </summary>
    /// <param name="index">Object table index of the collection to get, defaulted to the local player's</param>
    /// <returns>Collection GUID</returns>
    public async Task<Guid> GetCollection(int index = 0)
    {
        if (_penumbraAvailable)
            return await Plugin.RunOnFramework(() =>
            {
                try
                {
                    var result = _getCollectionForObject.Invoke(index);
                    return result.EffectiveCollection.Id;
                }
                catch (Exception ex) 
                {
                    Plugin.Log.Warning($"[PenumbraService] [GetCollectionForObject] Failure, {ex}");
                    return Guid.Empty;
                }
            }).ConfigureAwait(false);

        Plugin.Log.Warning("[PenumbraService] [GetCollectionForObject] Penumbra is not installed!");
        return Guid.Empty;
    }

    /// <summary>
    ///     Calls penumbra's AddTemporaryMod function
    /// </summary>
    /// <returns><see cref="bool"/> indicating success</returns>
    public async Task<bool> AddTemporaryMod(string tag, Guid collectionGuid, Dictionary<string, string> modifiedPaths, string meta, int priority = 0)
    {
        if (_penumbraAvailable)
            return await Plugin.RunOnFramework(() =>
            {
                try
                {
                    var result = _addTemporaryMod.Invoke(tag, collectionGuid, modifiedPaths, meta, priority);
                    return result is PenumbraApiEc.Success;
                }
                catch (Exception ex)
                {
                    Plugin.Log.Warning($"[PenumbraService] [CallAddTemporaryMod] Failure, {ex}");
                    return false;
                }
            }).ConfigureAwait(false);

        Plugin.Log.Warning("[PenumbraService] [CallAddTemporaryMod] Penumbra is not installed!");
        return false;
    }

    /// <summary>
    ///     Calls penumbra's RemoveTemporaryMod function
    /// </summary>
    /// <returns><see cref="bool"/> indicating success</returns>
    public async Task<bool> CallRemoveTemporaryMod(string tag, Guid collectionId, int priority)
    {
        if (_penumbraAvailable)
            return await Plugin.RunOnFramework(() =>
            {
                try
                {
                    var result = _removeTemporaryMod.Invoke(tag, collectionId, priority);
                    return result is PenumbraApiEc.Success or PenumbraApiEc.NothingChanged;
                }
                catch (Exception ex)
                {
                    Plugin.Log.Warning($"[PenumbraService] [CallRemoveTemporaryMod] Failure, {ex}");
                    return false;
                }
            }).ConfigureAwait(false);

        Plugin.Log.Warning("[PenumbraService] [CallRemoveTemporaryMod] Penumbra is not installed!");
        return false;
    }

    private void PeriodicCheckApi(object? sender = null, ElapsedEventArgs? e = null)
    {
        try
        {
            // Test if plugin installed
            var penumbraPlugin = Plugin.PluginInterface.InstalledPlugins.FirstOrDefault(plugin =>
                string.Equals(plugin.InternalName, "Penumbra", StringComparison.OrdinalIgnoreCase));
            if (penumbraPlugin is null)
            {
                _penumbraAvailable = false;
                return;
            }

            // Test if plugin can be invoked
            var penumbraVersion = _apiVersion.Invoke();
            if (penumbraVersion.Breaking < 1)
            {
                _penumbraAvailable = false;
                return;
            }

            _penumbraAvailable = true;
        }
        catch (Exception ex)
        {
            Plugin.Log.Error($"Something went wrong trying to check for penumbra plugin: {ex}");
        }
    }
    
    public void Dispose()
    {
        _periodicPenumbraTest.Elapsed -= PeriodicCheckApi;
        _periodicPenumbraTest.Stop();
        _periodicPenumbraTest.Dispose();
        GC.SuppressFinalize(this);
    }
}