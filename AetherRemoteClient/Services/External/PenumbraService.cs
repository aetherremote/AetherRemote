using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Penumbra.Api.Enums;
using Penumbra.Api.IpcSubscribers;

namespace AetherRemoteClient.Services.External;

/// <summary>
///     Provides access to Penumbra IPCs
/// </summary>
public class PenumbraService
{
    // Penumbra API
    private readonly AddTemporaryMod _addTemporaryMod;
    private readonly GetGameObjectResourcePaths _getGameObjectResourcePaths;
    private readonly GetMetaManipulations _getMetaManipulations;
    private readonly GetCollectionForObject _getCollectionForObject;
    private readonly RemoveTemporaryMod _removeTemporaryMod;
    private readonly RedrawObject _redrawObject;

    /// <summary>
    ///     Is the penumbra api available for use?
    /// </summary>
    private readonly bool _penumbraAvailable;

    /// <summary>
    ///     <inheritdoc cref="PenumbraService"/>
    /// </summary>
    public PenumbraService()
    {
        _addTemporaryMod = new AddTemporaryMod(Plugin.PluginInterface);
        _getGameObjectResourcePaths = new GetGameObjectResourcePaths(Plugin.PluginInterface);
        _getMetaManipulations = new GetMetaManipulations(Plugin.PluginInterface);
        _removeTemporaryMod = new RemoveTemporaryMod(Plugin.PluginInterface);
        _getCollectionForObject = new GetCollectionForObject(Plugin.PluginInterface);
        _redrawObject = new RedrawObject(Plugin.PluginInterface);

        try
        {
            var version = new ApiVersion(Plugin.PluginInterface).Invoke();
            if (version.Breaking < 5)
                return;

            _penumbraAvailable = true;
        }
        catch (Exception)
        {
            // Ignored
        }

        Plugin.Log.Verbose($"[PenumbraService] Penumbra available: {_penumbraAvailable}");
    }

    /// <summary>
    ///     Calls penumbra's GetGameObjectResourcePaths function
    /// </summary>
    /// <returns>A list of modified objects, mapping the modified object to the target path</returns>
    public async Task<Dictionary<string, string>> GetGameObjectResourcePaths(ushort index)
    {
        if (_penumbraAvailable)
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
                        foreach (var item in kvp.Value)
                            paths.Add(item, kvp.Key);
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
        if (_penumbraAvailable)
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
        if (_penumbraAvailable)
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
    public async Task<bool> AddTemporaryMod(string tag, Guid collectionGuid, Dictionary<string, string> modifiedPaths,
        string meta, int priority = 0)
    {
        if (_penumbraAvailable)
            return await Plugin.RunOnFramework(() =>
            {
                try
                {
                    var result = _addTemporaryMod.Invoke(tag, collectionGuid, modifiedPaths, meta, priority);
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
    public async Task<bool> CallRemoveTemporaryMod(string tag, Guid collectionId, int priority)
    {
        if (_penumbraAvailable)
            return await Plugin.RunOnFramework(() =>
            {
                try
                {
                    var result = _removeTemporaryMod.Invoke(tag, collectionId, priority);
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
        if (_penumbraAvailable)
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