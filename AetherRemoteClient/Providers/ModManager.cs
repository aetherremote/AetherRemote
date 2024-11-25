using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AetherRemoteClient.Accessors.Glamourer;
using AetherRemoteClient.Accessors.Penumbra;
using AetherRemoteClient.Domain.Events;
using AetherRemoteClient.Uncategorized;

namespace AetherRemoteClient.Providers;

/// <summary>
/// Provides methods for interacting with other player's mods
/// </summary>
public class ModManager : IDisposable
{
    // Const
    private const string TemporaryCollectionName = "AetherRemoteBodySwapCollection";
    private const string TemporaryModName = "AetherRemoteBodySwapMods";
    private const int Priority = 99;

    // Injected
    private readonly PenumbraAccessor _penumbraAccessor;
    private readonly GlamourerAccessor _glamourerAccessor;
    private readonly NetworkProvider _networkProvider;
    
    public ModManager(PenumbraAccessor penumbraAccessor, GlamourerAccessor glamourerAccessor, NetworkProvider networkProvider)
    {
        _penumbraAccessor = penumbraAccessor;
        _glamourerAccessor = glamourerAccessor;
        _networkProvider = networkProvider;
        
        _glamourerAccessor.LocalPlayerResetOrReapply += OnLocalPlayerResetOrReapply;
        _networkProvider.ServerDisconnected += OnServerDisconnect;
    }
    
    private Guid _currentModSwapCollection = Guid.Empty;
    
    /// <summary>
    /// Gets a target character's mod data
    /// </summary>
    public async Task GetAndSetTargetMods(string targetCharacterName)
    {
        Plugin.Log.Verbose("[ModManager] Begin Swap");
        if (_currentModSwapCollection != Guid.Empty)
        {
            Plugin.Log.Verbose("[ModManager] Active connection present, removing");
            await RemoveAllCollections();
        }
        
        var index = await Plugin.RunOnFramework(() =>
        {
            for (ushort i = 0; i < GameObjectManager.GetObjectTableLength() ; i++)
            {
                if (GameObjectManager.GetObjectTableItem(i)?.Name.TextValue != targetCharacterName)
                    continue;
                
                Plugin.Log.Verbose($"[ModManager] Found index {i} for {targetCharacterName}");
                return i;
            }

            return ushort.MaxValue;
        }).ConfigureAwait(false);

        if (index is ushort.MaxValue)
        {
            Plugin.Log.Verbose($"[ModManager] No index match for {targetCharacterName}");
            return;
        }

        var resources = await _penumbraAccessor.CallGetGameObjectResourcePaths(index).ConfigureAwait(false);
        var paths = new Dictionary<string, string>();
        foreach (var resource in resources)
        {
            if (resource is null) continue;
            foreach(var kvp in resource)
            {
                if (kvp.Value.Count == 1 && kvp.Key == kvp.Value.First())
                    continue;

                foreach(var item in kvp.Value)
                    paths.Add(item, kvp.Key);
            }
        }

        var meta = await _penumbraAccessor.CallGetMetaManipulations(index).ConfigureAwait(false);

        _currentModSwapCollection = await _penumbraAccessor.CallCreateTemporaryCollection(TemporaryCollectionName).ConfigureAwait(false);
        if (_currentModSwapCollection == Guid.Empty)
        {
            Plugin.Log.Verbose("[ModManager] Unable to create temporary collection");
            return;
        }

        var modResult = await _penumbraAccessor.CallAddTemporaryMod(TemporaryModName, _currentModSwapCollection, paths, meta, Priority).ConfigureAwait(false);
        if (modResult == false)
        {
            Plugin.Log.Verbose("[ModManager] Unable to add temporary mod");
            await RemoveAllCollections();
            return;
        } 

        if (GameObjectManager.LocalPlayerExists() is false)
        {
            Plugin.Log.Verbose("[ModManager] No local player present");
            await RemoveAllCollections();
            return;
        }

        if (await _penumbraAccessor.CallAssignTemporaryCollection(_currentModSwapCollection).ConfigureAwait(false) == false)
        {
            Plugin.Log.Verbose("[ModManager] Could not assign collection");
            await RemoveAllCollections();
            return;
        }

        Plugin.Log.Verbose("[ModManager] Setting collection to be {_currentModSwapCollection}");
    }

    public async Task RemoveAllCollections()
    {
        Plugin.Log.Verbose("[ModManager] [Remove All Collections] Beginning");
        if (_currentModSwapCollection == Guid.Empty)
        {
            Plugin.Log.Verbose("[ModManager] [Remove All Collections] Nothing to remove");
            return;
        }

        await _penumbraAccessor.CallRemoveTemporaryMod(TemporaryModName, _currentModSwapCollection, Priority);
        await _penumbraAccessor.CallDeleteTemporaryCollection(_currentModSwapCollection);
        _currentModSwapCollection = Guid.Empty;
        Plugin.Log.Verbose("[ModManager] [Remove All Collections] Finished");
    }
    
    private async void OnLocalPlayerResetOrReapply(object? sender, GlamourerStateChangedEventArgs e)
    {
        try
        {
            await RemoveAllCollections();
        }
        catch (Exception exception)
        {
            Plugin.Log.Warning($"Failed to handle OnLocalPlayerResetOrReapply: {exception}");
        }
    }
    
    private async void OnServerDisconnect(object? sender, EventArgs e)
    {
        try
        {
            await RemoveAllCollections();
        }
        catch (Exception exception)
        {
            Plugin.Log.Warning($"[ModManager] Exception while handling server disconnect: {exception}");
        }
    }

    public void Dispose()
    {
        _glamourerAccessor.LocalPlayerResetOrReapply -= OnLocalPlayerResetOrReapply;
        _networkProvider.ServerDisconnected -= OnServerDisconnect;
        GC.SuppressFinalize(this);
    }
}