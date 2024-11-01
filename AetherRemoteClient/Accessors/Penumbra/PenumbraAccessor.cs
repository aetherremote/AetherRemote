using Penumbra.Api.Enums;
using Penumbra.Api.IpcSubscribers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace AetherRemoteClient.Accessors.Penumbra;

/// <summary>
/// Provides access to Penumbra's exposed methods
/// </summary>
public class PenumbraAccessor : IDisposable
{
    // Consts
    private const int TestApiIntervalInSeconds = 30;

    // Penumbra API
    private readonly AddTemporaryMod addTemporaryMod;
    private readonly ApiVersion apiVersion;
    private readonly AssignTemporaryCollection assignTemporaryCollection;
    private readonly CreateTemporaryCollection createTemporaryCollection;
    private readonly DeleteTemporaryCollection deleteTemporaryCollection;
    private readonly GetGameObjectResourcePaths getGameObjectResourcePaths;
    private readonly GetMetaManipulations getMetaManipulations;
    private readonly RemoveTemporaryMod removeTemporaryMod;

    // Installed?
    private readonly Timer periodicPenumbraTest;
    private bool penumbraUsable = false;

    public PenumbraAccessor()
    {
        addTemporaryMod = new(Plugin.PluginInterface);
        apiVersion = new(Plugin.PluginInterface);
        assignTemporaryCollection = new(Plugin.PluginInterface);
        createTemporaryCollection = new(Plugin.PluginInterface);
        deleteTemporaryCollection = new(Plugin.PluginInterface);
        getGameObjectResourcePaths = new(Plugin.PluginInterface);
        getMetaManipulations = new(Plugin.PluginInterface);
        removeTemporaryMod = new(Plugin.PluginInterface);

        periodicPenumbraTest = new Timer(TestApiIntervalInSeconds * 1000);
        periodicPenumbraTest.AutoReset = true;
        periodicPenumbraTest.Elapsed += PeriodicCheckApi;
        periodicPenumbraTest.Start();

        CheckApi();
    }

    /// <summary>
    /// <inheritdoc cref="GetGameObjectResourcePaths"/>
    /// </summary>
    public async Task<Dictionary<string, HashSet<string>>?[]> CallGetGameObjectResourcePaths(ushort objectIndex)
    {
        if (penumbraUsable == false)
        {
            Plugin.Log.Warning("[Penumbra::GetGameObjectResourcePaths] Penumbra is not installed!");
            return [];
        }

        return await Plugin.RunOnFramework(() =>
        {
            try
            {
                var resources = getGameObjectResourcePaths.Invoke(objectIndex);
                if (resources is null)
                    Plugin.Log.Warning($"[Penumbra::GetGameObjectResourcePaths] returned null for {objectIndex}");

                return resources ?? [];
            }
            catch (Exception ex)
            {
                Plugin.Log.Warning($"[Penumbra::GetGameObjectResourcePaths] Failure, {ex}");
                return [];
            }
        });
    }

    /// <summary>
    /// <inheritdoc cref="CreateTemporaryCollection"/>
    /// </summary>
    public async Task<Guid> CallCreateTemporaryCollection(string collectionName)
    {
        if (penumbraUsable == false)
        {
            Plugin.Log.Warning("[Penumbra::CreateTemporaryCollection] Penumbra is not installed!");
            return Guid.Empty;
        }

        return await Plugin.RunOnFramework(() =>
        {
            try
            {
                var guid = createTemporaryCollection.Invoke(collectionName);
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

    /// <summary>
    /// <inheritdoc cref="DeleteTemporaryCollection"/>
    /// </summary>
    public async Task<bool> CallDeleteTemporaryCollection(Guid collectionId)
    {
        if (penumbraUsable == false)
        {
            Plugin.Log.Warning("[Penumbra::CreateTemporaryCollection] Penumbra is not installed!");
            return false;
        }

        return await Plugin.RunOnFramework(() =>
        {
            try
            {
                var result = deleteTemporaryCollection.Invoke(collectionId);
                Plugin.Log.Verbose($"[Penumbra::CreateTemporaryCollection] {result} for {collectionId}");
                return result is PenumbraApiEc.Success || result is PenumbraApiEc.NothingChanged;
            }
            catch (Exception ex)
            {
                Plugin.Log.Warning($"[Penumbra::CreateTemporaryCollection] Failure, {ex}");
                return false;
            }
        });
    }

    /// <summary>
    /// <inheritdoc cref="GetMetaManipulations"/>
    /// </summary>
    public async Task<string> CallGetMetaManipulations(ushort objectIndex)
    {
        if (penumbraUsable == false)
        {
            Plugin.Log.Warning("[Penumbra::GetMetaManipulations] Penumbra is not installed!");
            return string.Empty;
        }

        return await Plugin.RunOnFramework(() =>
        {
            try
            {
                var meta = getMetaManipulations.Invoke(objectIndex);
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

    /// <summary>
    /// <inheritdoc cref="AddTemporaryMod"/>
    /// </summary>
    public async Task<bool> CallAddTemporaryMod(string tag, Guid collectionGuid,
        Dictionary<string, string> modifiedPaths, string meta, int priority = 0)
    {
        if (penumbraUsable == false)
        {
            Plugin.Log.Warning("[Penumbra::AddTemporaryMod] Penumbra is not installed!");
            return false;
        }

        return await Plugin.RunOnFramework(() =>
        {
            try
            {
                var result = addTemporaryMod.Invoke(tag, collectionGuid, modifiedPaths, meta, priority);
                Plugin.Log.Verbose($"[Penumbra::AddTemporaryMod] {result} for {tag} - {collectionGuid} - {priority}");
                return result == PenumbraApiEc.Success;
            }
            catch (Exception ex)
            {
                Plugin.Log.Warning($"[Penumbra::AddTemporaryMod] Failure, {ex}");
                return false;
            }
        });
    }

    /// <summary>
    /// <inheritdoc cref="RemoveTemporaryMod"/>
    /// </summary>
    public async Task<bool> CallRemoveTemporaryMod(string tag, Guid collectionId, int priority)
    {
        if (penumbraUsable == false)
        {
            Plugin.Log.Warning("[Penumbra::AddTemporaryMod] Penumbra is not installed!");
            return false;
        }

        return await Plugin.RunOnFramework(() =>
        {
            try
            {
                var result = removeTemporaryMod.Invoke(tag, collectionId, priority);
                Plugin.Log.Verbose($"[Penumbra::AddTemporaryMod] {result} for {tag} - {collectionId} - {priority}");
                return result == PenumbraApiEc.Success || result == PenumbraApiEc.NothingChanged;
            }
            catch (Exception ex)
            {
                Plugin.Log.Warning($"[Penumbra::AddTemporaryMod] Failure, {ex}");
                return false;
            }
        });
    }

    /// <summary>
    /// <inheritdoc cref="AssignTemporaryCollection"/>
    /// </summary>
    public async Task<bool> CallAssignTemporaryCollection(Guid collectionGuid, int actorIndex = 0, bool forceAssignment = true)
    {
        if (penumbraUsable == false)
        {
            Plugin.Log.Warning("[Penumbra::AssignTemporaryCollection] Penumbra is not installed!");
            return false;
        }

        return await Plugin.RunOnFramework(() =>
        {
            try
            {
                var result = assignTemporaryCollection.Invoke(collectionGuid, actorIndex, forceAssignment);
                Plugin.Log.Verbose($"[Penumbra::AssignTemporaryCollection] {result} for {collectionGuid} - {actorIndex}");
                return result == PenumbraApiEc.Success;
            }
            catch (Exception ex)
            {
                Plugin.Log.Warning($"[Penumbra::AssignTemporaryCollection] Failure, {ex}");
                return false;
            }
        });
    }

    private void PeriodicCheckApi(object? sender, ElapsedEventArgs e) => CheckApi();
    private void CheckApi()
    {
        try
        {
            // Test if plugin installed
            var penumbraPlugin = Plugin.PluginInterface.InstalledPlugins.FirstOrDefault(plugin => string.Equals(plugin.InternalName, "Penumbra", StringComparison.OrdinalIgnoreCase));
            if (penumbraPlugin == null)
            {
                penumbraUsable = false;
                return;
            }

            // Test if plugin can be invoked
            var penumbraVersion = apiVersion.Invoke();
            if (penumbraVersion.Breaking < 1)
            {
                penumbraUsable = false;
                return;
            }

            penumbraUsable = true;
        }
        catch (Exception ex)
        {
            Plugin.Log.Error($"Something went wrong trying to check for penumbra plugin: {ex}");
        }
    }

    public void Dispose()
    {
        periodicPenumbraTest.Elapsed -= PeriodicCheckApi;
        periodicPenumbraTest.Dispose();

        GC.SuppressFinalize(this);
    }
}
