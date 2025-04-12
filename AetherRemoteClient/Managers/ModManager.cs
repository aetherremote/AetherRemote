using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using AetherRemoteClient.Domain.Events;
using AetherRemoteClient.Ipc;
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
    public async Task<bool> Assimilate(string targetCharacterName, CharacterAttributes attributes)
    {
        // Get Current Collection
        var collection = await _penumbra.GetCollection().ConfigureAwait(false);

        // Remove Existing Temp Mods
        await _penumbra.CallRemoveTemporaryMod(TemporaryModName, collection, Priority).ConfigureAwait(false);

        // Get a game object for target player in object table
        var gameObject = await Plugin.RunOnFramework(() =>
        {
            for (ushort i = 0; i < Plugin.ObjectTable.Length; i++)
            {
                if (Plugin.ObjectTable[i]?.Name.TextValue == targetCharacterName)
                    return Plugin.ObjectTable[i];
            }

            return null;
        }).ConfigureAwait(false);

        // If the object was not found in the table, exit
        if (gameObject is null)
        {
            Plugin.Log.Warning($"Unable to find {targetCharacterName} in object table");
            return false;
        }

        // Get Glamourer design
        if (await _glamourer.GetDesignAsync(gameObject.ObjectIndex).ConfigureAwait(false) is not { } glamourer)
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
                ModifiedPaths =
                    await _penumbra.GetGameObjectResourcePaths(gameObject.ObjectIndex).ConfigureAwait(false),
                MetaData = await _penumbra.GetMetaManipulations(gameObject.ObjectIndex).ConfigureAwait(false)
            };
        }

        // Get moodles if the option was provided
        if ((attributes & CharacterAttributes.Moodles) == CharacterAttributes.Moodles)
        {
            // TODO: Store original moodles before swap

            characterData.Moodles = await _moodles.GetMoodles(gameObject.Address).ConfigureAwait(false);
            if (characterData.Moodles is null)
                Plugin.Log.Warning("[ModManager] Moodles were null.");
        }

        if ((attributes & CharacterAttributes.CustomizePlus) == CharacterAttributes.CustomizePlus)
        {
            characterData.CustomizePlusTemplates = _customizePlus.GetActiveTemplatesOnCharacter(targetCharacterName);
            if (characterData.CustomizePlusTemplates is null)
                Plugin.Log.Warning("[ModManager] CustomizePlus data was null.");
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

            if (await _penumbra
                    .AddTemporaryMod(TemporaryModName, collection, characterData.ModInfo.ModifiedPaths,
                        characterData.ModInfo.MetaData,
                        Priority).ConfigureAwait(false) is false)
            {
                Plugin.Log.Warning($"Could not add {targetCharacterName}'s mods");
                return false;
            }
        }

        // Apply moodles if the option was provided
        if ((attributes & CharacterAttributes.Moodles) == CharacterAttributes.Moodles)
        {
            if (characterData.Moodles is null)
            {
                Plugin.Log.Warning($"Expected mods from {targetCharacterName} but none were present");
                return false;
            }

            if (await Plugin.RunOnFramework(() => Plugin.ClientState.LocalPlayer).ConfigureAwait(false) is null)
            {
                Plugin.Log.Warning(
                    $"Unable to assimilate {targetCharacterName}'s moodles because you don't have a body");
                return false;
            }

            var ownAddress = await Plugin.RunOnFramework(() => Plugin.ObjectTable[0]?.Address).ConfigureAwait(false);
            if (ownAddress is null)
            {
                Plugin.Log.Warning($"Could not find own address");
                return false;
            }

            if (await _moodles.SetMoodles(ownAddress.Value, characterData.Moodles)
                    .ConfigureAwait(false) is false)
            {
                Plugin.Log.Warning($"Could not add {targetCharacterName}'s moodles");
                return false;
            }
        }

        // Apply moodles if the option was provided
        if ((attributes & CharacterAttributes.CustomizePlus) == CharacterAttributes.CustomizePlus)
        {
            if (characterData.CustomizePlusTemplates is not null)
            {
                await Plugin.RunOnFramework(() => _customizePlus.ApplyCustomize(characterData.CustomizePlusTemplates))
                    .ConfigureAwait(false);
            }
            else
            {
                Plugin.Log.Warning($"Expected mods from {targetCharacterName} but none were present");
            }
        }

        // Apply Glamourer
        if (await _glamourer.ApplyDesignAsync(characterData.GlamourerData, GlamourerApplyFlag.All)
                .ConfigureAwait(false) is false)
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
            _customizePlus.DeleteCustomize();
            var current = await _penumbra.GetCollection().ConfigureAwait(false);
            await _penumbra.CallRemoveTemporaryMod(TemporaryModName, current, Priority).ConfigureAwait(false);
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

        /// <summary>
        ///     The character's current moodles
        /// </summary>
        public string? Moodles = string.Empty;

        /// <summary>
        ///     The character's customize plus template
        /// </summary>
        public IList? CustomizePlusTemplates = null;
    }
}