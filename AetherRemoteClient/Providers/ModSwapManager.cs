using AetherRemoteClient.Accessors.Penumbra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AetherRemoteClient.Providers;

public class ModSwapManager(PenumbraAccessor penumbraAccessor)
{
    // Const
    private const string TemporaryCollectionName = "AetherRemoteBodySwapCollection";
    private const string TemporaryModName = "AetherRemoteBodySwapMods";
    private const int Priority = 99;

    // Injected

    /// <summary>
    /// Check if there is a currently applied change
    /// </summary>
    public bool ActiveChanges => _currentModSwapCollection != Guid.Empty;

    private Guid _currentModSwapCollection = Guid.Empty;

    public async Task<ModSwapErrorCode> SwapMods(string targetCharacterName)
    {
        Plugin.Log.Verbose($"[ModSwapManager] Begin Swap");
        if (ActiveChanges)
            await RemoveAllCollections();

        var index = await Plugin.RunOnFramework(() =>
        {
            for (ushort i = 0; i < Plugin.ObjectTable.Length; i++)
            {
                if (Plugin.ObjectTable[i]?.Name.TextValue != targetCharacterName) continue;
                Plugin.Log.Verbose($"[ModSwapManager] Found index {i} for {targetCharacterName}");
                return i;
            }

            return ushort.MaxValue;
        }).ConfigureAwait(false);

        if (index == ushort.MaxValue)
        {
            Plugin.Log.Verbose($"[ModSwapManager] No index match for {targetCharacterName}");
            return ModSwapErrorCode.NoIndex;
        }

        var resources = await penumbraAccessor.CallGetGameObjectResourcePaths(index).ConfigureAwait(false);
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

        var meta = await penumbraAccessor.CallGetMetaManipulations(index).ConfigureAwait(false);

        _currentModSwapCollection = await penumbraAccessor.CallCreateTemporaryCollection(TemporaryCollectionName).ConfigureAwait(false);
        if (_currentModSwapCollection == Guid.Empty)
        {
            Plugin.Log.Verbose($"[ModSwapManager] Unable to create temporary collection");
            return ModSwapErrorCode.EmptyGuid;
        }

        var modResult = await penumbraAccessor.CallAddTemporaryMod(TemporaryModName, _currentModSwapCollection, paths, meta, Priority).ConfigureAwait(false);
        if (modResult == false)
        {
            Plugin.Log.Verbose($"[ModSwapManager] Unable to add temporary mod");
            await RemoveAllCollections();
            return ModSwapErrorCode.ModAddError;
        } 

        if (Plugin.ClientState.LocalPlayer is null)
        {
            Plugin.Log.Verbose($"[ModSwapManager] No local player present");
            await RemoveAllCollections();
            return ModSwapErrorCode.NoLocalPlayer;
        }

        if (await penumbraAccessor.CallAssignTemporaryCollection(_currentModSwapCollection, 0, true).ConfigureAwait(false) == false)
        {
            Plugin.Log.Verbose($"[ModSwapManager] Could not assign collection");
            await RemoveAllCollections();
            return ModSwapErrorCode.CouldNotAssignCollection;
        }

        Plugin.Log.Verbose($"[ModSwapManager] Setting collection to be {_currentModSwapCollection}");
        return ModSwapErrorCode.Success;
    }

    public async Task RemoveAllCollections()
    {
        Plugin.Log.Verbose("[Mod Swap] [Remove All Collections] Beginning");
        if (_currentModSwapCollection == Guid.Empty)
        {
            Plugin.Log.Verbose("[Mod Swap] [Remove All Collections] Nothing to remove");
            return;
        }

        await penumbraAccessor.CallRemoveTemporaryMod(TemporaryModName, _currentModSwapCollection, Priority);
        await penumbraAccessor.CallDeleteTemporaryCollection(_currentModSwapCollection);
        _currentModSwapCollection = Guid.Empty;
    }

    public enum ModSwapErrorCode
    {
        Success,
        NoIndex,
        EmptyGuid,
        ModAddError,
        NoLocalPlayer,
        CouldNotAssignCollection
    }
}
