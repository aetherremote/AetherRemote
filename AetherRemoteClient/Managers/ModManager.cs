using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AetherRemoteClient.Domain.Enums;
using AetherRemoteClient.Domain.Events;
using AetherRemoteClient.Services.External;
using AetherRemoteCommon.Domain.Enums;

namespace AetherRemoteClient.Managers;

/// <summary>
///     Class response for managing temporary mods from one character to another
/// </summary>
public class ModManager : IDisposable
{
    // Const
    private const string TemporaryModName = "AetherRemoteMods";
    private const int Priority = 99;

    // Injected
    private readonly GlamourerService _glamourerService;
    private readonly PenumbraService _penumbraService;

    /// <summary>
    ///     <inheritdoc cref="ModManager"/>
    /// </summary>
    public ModManager(GlamourerService glamourerService, PenumbraService penumbraService)
    {
        _glamourerService = glamourerService;
        _penumbraService = penumbraService;

        _glamourerService.LocalPlayerResetOrReapply += OnPlayerResetOrReapply;
    }

    public void Dispose()
    {
        _glamourerService.LocalPlayerResetOrReapply -= OnPlayerResetOrReapply;
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Get a target player's glamourer data along with optional additional parameters
    /// </summary>
    public async Task<bool> Assimilate(string targetCharacterName, CharacterAttributes attributes)
    {
        // Get Current Collection
        var collection = await _penumbraService.GetCollection().ConfigureAwait(false);

        // Remove Existing Temp Mods
        await _penumbraService.CallRemoveTemporaryMod(TemporaryModName, collection, Priority).ConfigureAwait(false);

        var index = await Plugin.RunOnFramework(() =>
        {
            for (ushort i = 0; i < Plugin.ObjectTable.Length; i++)
            {
                if (Plugin.ObjectTable[i]?.Name.TextValue == targetCharacterName)
                    return i;
            }

            return ushort.MaxValue;
        }).ConfigureAwait(false);

        // If the object was not found in the table, exit
        if (index is ushort.MaxValue)
        {
            Plugin.Log.Warning($"Unable to find {targetCharacterName} in object table");
            return false;
        }

        // Get Glamourer design
        if (await _glamourerService.GetDesignAsync(index).ConfigureAwait(false) is not { } glamourer)
        {
            Plugin.Log.Warning($"Unable to find {targetCharacterName} in object table");
            return false;
        }

        // Begin getting data
        var characterData = new CharacterData
        {
            GlamourerData = glamourer
        };

        // Get mods if the option was provided
        if ((attributes & CharacterAttributes.Mods) == CharacterAttributes.Mods)
        {
            characterData.ModInfo = new PenumbraCharacterModInfo
            {
                ModifiedPaths = await _penumbraService.GetGameObjectResourcePaths(index).ConfigureAwait(false),
                MetaData = await _penumbraService.GetMetaManipulations(index).ConfigureAwait(false)
            };
        }

        // Pause to allow others viewing your character details from their clients
        await Task.Delay(3000).ConfigureAwait(false);

        // Apply mods if the option was provided
        if ((attributes & CharacterAttributes.Mods) == CharacterAttributes.Mods)
        {
            if (characterData.ModInfo is null)
            {
                Plugin.Log.Warning($"Expected mods from {targetCharacterName} but none were present");
                return false;
            }

            if (await Plugin.RunOnFramework(() => Plugin.ClientState.LocalPlayer).ConfigureAwait(false) is null)
            {
                Plugin.Log.Warning($"Unable to assimilate {targetCharacterName}'s mods because you don't have a body");
                return false;
            }

            if (await _penumbraService
                    .AddTemporaryMod(TemporaryModName, collection, characterData.ModInfo.ModifiedPaths, characterData.ModInfo.MetaData,
                        Priority).ConfigureAwait(false) is false)
            {
                Plugin.Log.Warning($"Could not add {targetCharacterName}'s mods");
                return false;
            }
        }

        // Apply Glamourer
        if (await _glamourerService.ApplyDesignAsync(characterData.GlamourerData, GlamourerApplyFlag.All).ConfigureAwait(false) is false)
        {
            Plugin.Log.Warning($"Could not apply {targetCharacterName}'s glamourer data");
            return false;
        }

        return true;
    }

    /// <summary>
    ///     If a client reverts to game or automation, removed the temporary mods created.
    /// </summary>
    private async void OnPlayerResetOrReapply(object? sender, GlamourerStateChangedEventArgs e)
    {
        try
        {
            var current = await _penumbraService.GetCollection().ConfigureAwait(false);
            await _penumbraService.CallRemoveTemporaryMod(TemporaryModName, current, Priority).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            Plugin.Log.Warning($"Unknown error while resetting temporary mods, {exception.Message}");
        }
    }

    /// <summary>
    ///     Data representing the things penumbra has changed about a character
    /// </summary>
    private class PenumbraCharacterModInfo
    {
        /// <summary>
        ///     Dictionary of paths modified by penumbra. The key is the local object path, and the value is the path
        ///     penumbra has overwritten. This can be a file path or an internal game object path.
        /// </summary>
        public Dictionary<string, string> ModifiedPaths = [];
        
        /// <summary>
        ///     The metadata of a character modified by penumbra.
        /// </summary>
        public string MetaData = string.Empty;
    }

    /// <summary>
    ///     Data representing the things that make up a character
    /// </summary>
    private class CharacterData
    {
        /// <summary>
        ///     The character's glamourer data
        /// </summary>
        public string GlamourerData = string.Empty;
        
        /// <summary>
        ///     The character's penumbra mod info
        /// </summary>
        public PenumbraCharacterModInfo? ModInfo;
    }
}