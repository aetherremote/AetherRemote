using AetherRemoteClient.Accessors.Glamourer;
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
        if (ActiveChanges)
            await RemoveAllCollections();

        var index = await Plugin.RunOnFramework(() =>
        {
            for (ushort i = 0; i < Plugin.ObjectTable.Length; i++)
            {
                if (Plugin.ObjectTable[i]?.Name.TextValue == targetCharacterName)
                    return i;
            }

            return ushort.MaxValue;
        }).ConfigureAwait(false);

        if (index == ushort.MaxValue)
            return ModSwapErrorCode.NoIndex; // Didn't find a match

        var resources = await penumbraAccessor.CallGetGameObjectResourcePaths(index).ConfigureAwait(false);
        if (resources is null)
            return ModSwapErrorCode.NoResources; // Didn't get resources

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

        if (currentBodySwapCollection != Guid.Empty)
            await penumbraAccessor.CallDeleteTemporaryCollection(currentBodySwapCollection).ConfigureAwait(false);

        var meta = await penumbraAccessor.CallGetMetaManipulations(index).ConfigureAwait(false);

        var guid = await penumbraAccessor.CallCreateTemporaryCollection(TemporaryCollectionName).ConfigureAwait(false);
        if (guid == Guid.Empty)
            return ModSwapErrorCode.EmptyGuid;

        var modResult = await penumbraAccessor.CallAddTemporaryMod(TemporaryModName, guid, paths, meta, Priority).ConfigureAwait(false);
        if (modResult == false)
        {
            await RemoveAllCollections();
            return ModSwapErrorCode.ModAddError;
        } 

        if (Plugin.ClientState.LocalPlayer is null)
        {
            await RemoveAllCollections();
            return ModSwapErrorCode.NoLocalPlayer;
        }

        await penumbraAccessor.CallAssignTemporaryCollection(guid, 0, true).ConfigureAwait(false);

        currentBodySwapCollection = guid;
        return ModSwapErrorCode.Success;
    }

    public async Task RemoveAllCollections()
    {
        if (currentBodySwapCollection == Guid.Empty)
            return;

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
    }
}
