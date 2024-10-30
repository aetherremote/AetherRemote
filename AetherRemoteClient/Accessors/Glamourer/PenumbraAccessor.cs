using Penumbra.Api.Enums;
using Penumbra.Api.IpcSubscribers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace AetherRemoteClient.Accessors.Glamourer;

public class PenumbraAccessor : IDisposable
{
    // Consts
    private const int TestApiIntervalInSeconds = 30;

    // Penumbra API
    private readonly ApiVersion apiVersion;
    private readonly GetGameObjectResourcePaths getGameObjectResourcePaths;
    private readonly CreateTemporaryCollection createTemporaryCollection;
    private readonly GetMetaManipulations getMetaManipulations;
    private readonly AddTemporaryMod addTemporaryMod;
    private readonly AssignTemporaryCollection assignTemporaryCollection;

    // Installed?
    private readonly Timer periodicPenumbraTest;
    private bool penumbraUsable = false;

    public PenumbraAccessor()
    {
        apiVersion = new(Plugin.PluginInterface);
        getGameObjectResourcePaths = new(Plugin.PluginInterface);
        createTemporaryCollection = new(Plugin.PluginInterface);
        getMetaManipulations = new(Plugin.PluginInterface);
        addTemporaryMod = new(Plugin.PluginInterface);
        assignTemporaryCollection = new(Plugin.PluginInterface);

        periodicPenumbraTest = new Timer(TestApiIntervalInSeconds * 1000);
        periodicPenumbraTest.AutoReset = true;
        periodicPenumbraTest.Elapsed += PeriodicCheckApi;
        periodicPenumbraTest.Start();

        CheckApi();
    }

    /// <summary>
    /// <inheritdoc cref="GetGameObjectResourcePaths"/>
    /// </summary>
    public static async Task<Dictionary<string, HashSet<string>>?[]> CallGetGameObjectResourcePaths(ushort objectIndex)
    {
        return await Plugin.RunOnFramework(() =>
        {
            try
            {
                var resources = new GetGameObjectResourcePaths(Plugin.PluginInterface).Invoke(objectIndex);
                if (resources is null)
                    Plugin.Log.Warning($"Penumbra::GetGameObjectResourcePaths returned null for {objectIndex}");

                return resources ?? [];
            }
            catch (Exception ex)
            {
                Plugin.Log.Warning($"Exception while calling Penumbra::GetGameObjectResourcePaths, {ex}");
                return [];
            }
        });
    }

    /// <summary>
    /// <inheritdoc cref="CreateTemporaryCollection"/>
    /// </summary>
    public static async Task<Guid> CallCreateTemporaryCollection(string collectionName)
    {
        return await Plugin.RunOnFramework(() =>
        {
            try
            {
                return new CreateTemporaryCollection(Plugin.PluginInterface).Invoke(collectionName);
            }
            catch (Exception ex)
            {
                Plugin.Log.Warning($"Exception while calling Penumbra::CreateTemporaryCollection, {ex}");
                return Guid.Empty;
            }
        });
    }

    /// <summary>
    /// <inheritdoc cref="GetMetaManipulations"/>
    /// </summary>
    public static async Task<string> CallGetMetaManipulations(ushort objectIndex)
    {
        return await Plugin.RunOnFramework(() =>
        {
            try
            {
                var meta = new GetMetaManipulations(Plugin.PluginInterface).Invoke(objectIndex);
                if (meta is null)
                    Plugin.Log.Warning($"Penumbra::GetMetaManipulations returned null for {objectIndex}");

                return meta ?? string.Empty;
            }
            catch (Exception ex)
            {
                Plugin.Log.Warning($"Exception while calling Penumbra::GetMetaManipulations, {ex}");
                return string.Empty;
            }
        });
    }

    /// <summary>
    /// <inheritdoc cref="AddTemporaryMod"/>
    /// </summary>
    public static async Task<bool> CallAddTemporaryMod(string tag, Guid collectionGuid, 
        Dictionary<string, string> modifiedPaths,  string meta, int priority = 0)
    {
        return await Plugin.RunOnFramework(() =>
        {
            try
            {
                var result = new AddTemporaryMod(Plugin.PluginInterface).Invoke(tag, collectionGuid, modifiedPaths, meta, priority);
                return result == PenumbraApiEc.Success;
            }
            catch (Exception ex)
            {
                Plugin.Log.Warning($"Exception while calling Penumbra::AddTemporaryMod, {ex}");
                return false;
            }
        });
    }

    /// <summary>
    /// <inheritdoc cref="AssignTemporaryCollection"/>
    /// </summary>
    public static async Task<bool> CallAssignTemporaryCollection(Guid collectionGuid, int actorIndex = 0, bool forceAssignment = true)
    {
        return await Plugin.RunOnFramework(() =>
        {
            try
            {
                var success = new AssignTemporaryCollection(Plugin.PluginInterface).Invoke(collectionGuid, actorIndex, forceAssignment);
                return success == PenumbraApiEc.Success;
            }
            catch (Exception ex)
            {
                Plugin.Log.Warning($"Exception while calling Penumbra::AssignTemporaryCollection, {ex}");
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
