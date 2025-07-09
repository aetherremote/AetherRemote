using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Attributes;
using AetherRemoteClient.Domain.Events;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Ipc;
using AetherRemoteCommon.Domain.Enums;
using Dalamud.Game.ClientState.Objects.Enums;

namespace AetherRemoteClient.Managers;

/// <summary>
///     Class response for managing temporary mods from one character to another
/// </summary>
public class ModManager : IDisposable
{
    // Injected
    private readonly CustomizePlusIpc _customizePlus;
    private readonly GlamourerIpc _glamourer;
    private readonly MoodlesIpc _moodles;
    private readonly PenumbraIpc _penumbra;

    /// <summary>
    ///     <inheritdoc cref="ModManager"/>
    /// </summary>
    public ModManager(CustomizePlusIpc customizePlus, GlamourerIpc glamourer, MoodlesIpc moodles, PenumbraIpc penumbra)
    {
        _customizePlus = customizePlus;
        _glamourer = glamourer;
        _moodles = moodles;
        _penumbra = penumbra;

        _glamourer.LocalPlayerResetOrReapply += OnPlayerResetOrReapply;
    }

    public void Dispose()
    {
        _glamourer.LocalPlayerResetOrReapply -= OnPlayerResetOrReapply;
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Get a target player's glamourer data along with optional additional parameters
    /// </summary>
    public async Task<PermanentTransformationData?> Assimilate(string targetCharacterName, CharacterAttributes attributes)
    {
        // Get Current Collection
        var collection = await _penumbra.GetCollection().ConfigureAwait(false);

        // Remove Existing Temp Mods
        await _penumbra.CallRemoveTemporaryMod(collection).ConfigureAwait(false);

        // Get a game object for target player in object table
        var gameObject = await Plugin.RunOnFramework(() =>
        {
            for (ushort i = 0; i < Plugin.ObjectTable.Length; i++)
            {
                if (Plugin.ObjectTable[i] is not { } gameObject)
                    continue;

                if (gameObject.Name.TextValue == targetCharacterName && gameObject.ObjectKind is ObjectKind.Player)
                    return Plugin.ObjectTable[i];
            }

            return null;
        }).ConfigureAwait(false);

        // If the object was not found in the table, exit
        if (gameObject is null)
        {
            Plugin.Log.Warning($"Unable to find {targetCharacterName} in object table");
            return null;
        }

        // Store a list of all the attributes we add
        var assimilatedAttributes = new List<ICharacterAttribute>();

        // Store Glamourer always
        var glamourerAttribute = new GlamourerAttribute(_glamourer, gameObject.ObjectIndex);
        if (await glamourerAttribute.Store().ConfigureAwait(false))
            assimilatedAttributes.Add(glamourerAttribute);

        // Store Mods
        if ((attributes & CharacterAttributes.Mods) is CharacterAttributes.Mods)
        {
            var modsAttribute = new ModsAttribute(_penumbra, collection, gameObject.ObjectIndex);
            if (await modsAttribute.Store().ConfigureAwait(false))
                assimilatedAttributes.Add(modsAttribute);
        }

        // Store Moodles
        if ((attributes & CharacterAttributes.Moodles) is CharacterAttributes.Moodles)
        {
            var moodlesAttribute = new MoodlesAttribute(_moodles, gameObject.Address);
            if (await moodlesAttribute.Store().ConfigureAwait(false))
                assimilatedAttributes.Add(moodlesAttribute);
        }

        // Store CustomizePlus
        if ((attributes & CharacterAttributes.CustomizePlus) is CharacterAttributes.CustomizePlus)
        {
            var customizePlusAttribute = new CustomizePlusAttribute(_customizePlus, targetCharacterName);
            if (await customizePlusAttribute.Store().ConfigureAwait(false))
                assimilatedAttributes.Add(customizePlusAttribute);
        }

        // Pause to allow others viewing your character details from their clients
        await Task.Delay(3000).ConfigureAwait(false);

        // Pass in a permanent object to store data
        var permanent = new PermanentTransformationData();
        
        // Apply all the attributes we added in the above steps
        foreach (var attribute in assimilatedAttributes)
            await attribute.Apply(permanent).ConfigureAwait(false);
        
        return permanent;
    }

    /// <summary>
    ///     If a client reverts to game or automation, removed the temporary mods created.
    /// </summary>
    private async void OnPlayerResetOrReapply(object? sender, GlamourerStateChangedEventArgs e)
    {
        try
        {
            await _customizePlus.DeleteCustomize();
            var current = await _penumbra.GetCollection().ConfigureAwait(false);
            await _penumbra.CallRemoveTemporaryMod(current).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            Plugin.Log.Warning($"Unknown error while resetting temporary mods, {exception.Message}");
        }
    }
}