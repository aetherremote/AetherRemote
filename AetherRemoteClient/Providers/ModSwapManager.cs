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
    private readonly PenumbraAccessor penumbraAccessor = penumbraAccessor;

    /// <summary>
    /// Check if there is a currently applied change
    /// </summary>
    public bool ActiveChanges => currentBodySwapCollection != Guid.Empty;

    private Guid currentBodySwapCollection = Guid.Empty;

    public async Task<ModSwapErrorCode> SwapMods(string targetCharacterName)
    {
        Plugin.Log.Verbose($"[ModSwapManager] Begin Swap");
        if (ActiveChanges)
            await RemoveAllCollections();

        var index = await Plugin.RunOnFramework(() =>
        {
            for (ushort i = 0; i < Plugin.ObjectTable.Length; i++)
            {
                if (Plugin.ObjectTable[i]?.Name.TextValue == targetCharacterName)
                {
                    Plugin.Log.Verbose($"[ModSwapManager] Found index {i} for {targetCharacterName}");
                    return i;
                }
            }

            return ushort.MaxValue;
        }).ConfigureAwait(false);

        if (index == ushort.MaxValue)
        {
            Plugin.Log.Verbose($"[ModSwapManager] No index match for {targetCharacterName}");
            return ModSwapErrorCode.NoIndex;
        }

        var resources = await penumbraAccessor.CallGetGameObjectResourcePaths(index).ConfigureAwait(false);
        if (resources is null)
        {
            Plugin.Log.Verbose($"[ModSwapManager] Could not get resources for {targetCharacterName}");
            return ModSwapErrorCode.NoResources;
        }

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

        var guid = await penumbraAccessor.CallCreateTemporaryCollection(TemporaryCollectionName).ConfigureAwait(false);
        if (guid == Guid.Empty)
        {
            Plugin.Log.Verbose($"[ModSwapManager] Unable to create temporary collection");
            return ModSwapErrorCode.EmptyGuid;
        }

        var modResult = await penumbraAccessor.CallAddTemporaryMod(TemporaryModName, guid, paths, meta, Priority).ConfigureAwait(false);
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

        if (await penumbraAccessor.CallAssignTemporaryCollection(guid, 0, true).ConfigureAwait(false) == false)
        {
            Plugin.Log.Verbose($"[ModSwapManager] Could not assign collection");
            return ModSwapErrorCode.CouldNotAssignCollection;
        }

        Plugin.Log.Verbose($"[ModSwapManager] Setting collection to be {guid}");
        currentBodySwapCollection = guid;
        return ModSwapErrorCode.Success;
    }

    public async Task RemoveAllCollections()
    {
        Plugin.Log.Verbose("[Mod Swap] [Remove All Collections] Beginning");
        if (currentBodySwapCollection == Guid.Empty)
        {
            Plugin.Log.Verbose("[Mod Swap] [Remove All Collections] Nothing to remove");
            return;
        }

        await penumbraAccessor.CallRemoveTemporaryMod(TemporaryModName, currentBodySwapCollection, Priority);
        await penumbraAccessor.CallDeleteTemporaryCollection(currentBodySwapCollection);
        currentBodySwapCollection = Guid.Empty;
    }

    public enum ModSwapErrorCode
    {
        Success,
        NoIndex,
        NoResources,
        EmptyGuid,
        ModAddError,
        NoLocalPlayer,
        CouldNotAssignCollection
    }
}
