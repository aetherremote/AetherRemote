using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Penumbra.Api.Enums;
using Penumbra.Api.IpcSubscribers;

namespace AetherRemoteClient.Accessors.Penumbra;

/// <summary>
/// Provides access to Penumbra's exposed methods
/// </summary>
public class PenumbraAccessor : IDisposable
{
    // Const
    private const int TestApiIntervalInSeconds = 60;

    // Penumbra API
    private readonly AddTemporaryMod _addTemporaryMod;
    private readonly ApiVersion _apiVersion;
    private readonly AssignTemporaryCollection _assignTemporaryCollection;
    private readonly CreateTemporaryCollection _createTemporaryCollection;
    private readonly DeleteTemporaryCollection _deleteTemporaryCollection;
    private readonly GetGameObjectResourcePaths _getGameObjectResourcePaths;
    private readonly GetMetaManipulations _getMetaManipulations;
    private readonly RemoveTemporaryMod _removeTemporaryMod;

    // Installed?
    private readonly Timer _periodicPenumbraTest;
    private bool _penumbraUsable;

    public PenumbraAccessor()
    {
        _addTemporaryMod = new AddTemporaryMod(Plugin.PluginInterface);
        _apiVersion = new ApiVersion(Plugin.PluginInterface);
        _assignTemporaryCollection = new AssignTemporaryCollection(Plugin.PluginInterface);
        _createTemporaryCollection = new CreateTemporaryCollection(Plugin.PluginInterface);
        _deleteTemporaryCollection = new DeleteTemporaryCollection(Plugin.PluginInterface);
        _getGameObjectResourcePaths = new GetGameObjectResourcePaths(Plugin.PluginInterface);
        _getMetaManipulations = new GetMetaManipulations(Plugin.PluginInterface);
        _removeTemporaryMod = new RemoveTemporaryMod(Plugin.PluginInterface);

        _periodicPenumbraTest = new Timer(TestApiIntervalInSeconds * 1000);
        _periodicPenumbraTest.AutoReset = true;
        _periodicPenumbraTest.Elapsed += PeriodicCheckApi;
        _periodicPenumbraTest.Start();

        CheckApi();
    }

    /// <summary>
    /// <inheritdoc cref="GetGameObjectResourcePaths"/>
    /// </summary>
    public async Task<Dictionary<string, HashSet<string>>?[]> CallGetGameObjectResourcePaths(ushort objectIndex)
    {
        if (_penumbraUsable)
        {
            return await Plugin.RunOnFramework(() =>
            {
                try
                {
                    return _getGameObjectResourcePaths.Invoke(objectIndex);
                }
                catch (Exception ex)
                {
                    Plugin.Log.Warning($"[Penumbra::GetGameObjectResourcePaths] Failure, {ex}");
                    return [];
                }
            });
        }

        Plugin.Log.Warning("[Penumbra::GetGameObjectResourcePaths] Penumbra is not installed!");
        return [];
    }

    /// <summary>
    /// <inheritdoc cref="CreateTemporaryCollection"/>
    /// </summary>
    public async Task<Guid> CallCreateTemporaryCollection(string collectionName)
    {
        if (_penumbraUsable)
        {
            return await Plugin.RunOnFramework(() =>
            {
                try
                {
                    var guid = _createTemporaryCollection.Invoke(collectionName);
                    Plugin.Log.Verbose($"[Penumbra::CreateTemporaryCollection] {guid} for {collectionName}");
                    return guid;
                }
                catch (Exception ex)
                {
                    Plugin.Log.Warning($"[Penumbra::CreateTemporaryCollection] Failure, {ex}");
                    return Guid.Empty;
                }
            });
        }

        Plugin.Log.Warning("[Penumbra::CreateTemporaryCollection] Penumbra is not installed!");
        return Guid.Empty;
    }

    /// <summary>
    /// <inheritdoc cref="DeleteTemporaryCollection"/>
    /// </summary>
    public async Task<bool> CallDeleteTemporaryCollection(Guid collectionId)
    {
        if (_penumbraUsable)
        {
            return await Plugin.RunOnFramework(() =>
            {
                try
                {
                    var result = _deleteTemporaryCollection.Invoke(collectionId);
                    Plugin.Log.Verbose($"[Penumbra::CreateTemporaryCollection] {result} for {collectionId}");
                    return result is PenumbraApiEc.Success or PenumbraApiEc.NothingChanged;
                }
                catch (Exception ex)
                {
                    Plugin.Log.Warning($"[Penumbra::CreateTemporaryCollection] Failure, {ex}");
                    return false;
                }
            });
        }

        Plugin.Log.Warning("[Penumbra::CreateTemporaryCollection] Penumbra is not installed!");
        return false;
    }

    /// <summary>
    /// <inheritdoc cref="GetMetaManipulations"/>
    /// </summary>
    public async Task<string> CallGetMetaManipulations(ushort objectIndex)
    {
        if (_penumbraUsable)
        {
            return await Plugin.RunOnFramework(() =>
            {
                try
                {
                    var meta = _getMetaManipulations.Invoke(objectIndex);
                    Plugin.Log.Verbose($"[Penumbra::GetMetaManipulations] {meta} for {objectIndex}");
                    return meta;
                }
                catch (Exception ex)
                {
                    Plugin.Log.Warning($"[Penumbra::GetMetaManipulations] Failure, {ex}");
                    return string.Empty;
                }
            });
        }

        Plugin.Log.Warning("[Penumbra::GetMetaManipulations] Penumbra is not installed!");
        return string.Empty;
    }

    /// <summary>
    /// <inheritdoc cref="AddTemporaryMod"/>
    /// </summary>
    public async Task<bool> CallAddTemporaryMod(string tag, Guid collectionGuid,
        Dictionary<string, string> modifiedPaths, string meta, int priority = 0)
    {
        if (_penumbraUsable)
        {
            return await Plugin.RunOnFramework(() =>
            {
                try
                {
                    var result = _addTemporaryMod.Invoke(tag, collectionGuid, modifiedPaths, meta, priority);
                    Plugin.Log.Verbose(
                        $"[Penumbra::AddTemporaryMod] {result} for {tag} - {collectionGuid} - {priority}");
                    return result == PenumbraApiEc.Success;
                }
                catch (Exception ex)
                {
                    Plugin.Log.Warning($"[Penumbra::AddTemporaryMod] Failure, {ex}");
                    return false;
                }
            });
        }

        Plugin.Log.Warning("[Penumbra::AddTemporaryMod] Penumbra is not installed!");
        return false;
    }

    /// <summary>
    /// <inheritdoc cref="RemoveTemporaryMod"/>
    /// </summary>
    public async Task<bool> CallRemoveTemporaryMod(string tag, Guid collectionId, int priority)
    {
        if (_penumbraUsable)
        {
            return await Plugin.RunOnFramework(() =>
            {
                try
                {
                    var result = _removeTemporaryMod.Invoke(tag, collectionId, priority);
                    Plugin.Log.Verbose($"[Penumbra::AddTemporaryMod] {result} for {tag} - {collectionId} - {priority}");
                    return result is PenumbraApiEc.Success or PenumbraApiEc.NothingChanged;
                }
                catch (Exception ex)
                {
                    Plugin.Log.Warning($"[Penumbra::AddTemporaryMod] Failure, {ex}");
                    return false;
                }
            });
        }

        Plugin.Log.Warning("[Penumbra::AddTemporaryMod] Penumbra is not installed!");
        return false;
    }

    /// <summary>
    /// <inheritdoc cref="AssignTemporaryCollection"/>
    /// </summary>
    public async Task<bool> CallAssignTemporaryCollection(Guid collectionGuid, int actorIndex = 0,
        bool forceAssignment = true)
    {
        if (_penumbraUsable)
        {
            return await Plugin.RunOnFramework(() =>
            {
                try
                {
                    var result = _assignTemporaryCollection.Invoke(collectionGuid, actorIndex, forceAssignment);
                    Plugin.Log.Verbose(
                        $"[Penumbra::AssignTemporaryCollection] {result} for {collectionGuid} - {actorIndex}");
                    return result == PenumbraApiEc.Success;
                }
                catch (Exception ex)
                {
                    Plugin.Log.Warning($"[Penumbra::AssignTemporaryCollection] Failure, {ex}");
                    return false;
                }
            });
        }

        Plugin.Log.Warning("[Penumbra::AssignTemporaryCollection] Penumbra is not installed!");
        return false;
    }

    private void PeriodicCheckApi(object? sender, ElapsedEventArgs e) => CheckApi();

    private void CheckApi()
    {
        try
        {
            // Test if plugin installed
            var penumbraPlugin = Plugin.PluginInterface.InstalledPlugins.FirstOrDefault(plugin =>
                string.Equals(plugin.InternalName, "Penumbra", StringComparison.OrdinalIgnoreCase));
            if (penumbraPlugin is null)
            {
                _penumbraUsable = false;
                return;
            }

            // Test if plugin can be invoked
            var penumbraVersion = _apiVersion.Invoke();
            if (penumbraVersion.Breaking < 1)
            {
                _penumbraUsable = false;
                return;
            }

            _penumbraUsable = true;
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