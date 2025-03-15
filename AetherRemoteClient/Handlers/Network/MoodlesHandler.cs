using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AetherRemoteClient.Services;
using AetherRemoteClient.Services.External;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Util;
using MemoryPack;
using Moodles.Data;
using Newtonsoft.Json;

namespace AetherRemoteClient.Handlers.Network;

/// <summary>
///     Handles a <see cref="MoodlesAction"/>
/// </summary>
public class MoodlesHandler(
    FriendsListService friendsListService,
    MoodlesService moodlesService,
    OverrideService overrideService,
    PenumbraService penumbraService,
    LogService logService)
{
    // Instantiated
    private readonly MemoryPackSerializerOptions _serializerOptions = new()
    {
        StringEncoding = StringEncoding.Utf16
    };

    /// <summary>
    ///     <inheritdoc cref="MoodlesHandler"/>
    /// </summary>
    public async Task Handle(MoodlesAction action)
    {
        Plugin.Log.Info($"{action}");
        
        // Not friends
        if (friendsListService.Get(action.SenderFriendCode) is not { } friend)
        {
            logService.NotFriends("Moodles", action.SenderFriendCode);
            return;
        }

        // Plugin in safe mode
        if (Plugin.Configuration.SafeMode)
        {
            logService.SafeMode("Moodles", friend.NoteOrFriendCode);
            return;
        }

        // Overriding moodles
        if (overrideService.HasActiveOverride(PrimaryPermissions.Moodles))
        {
            logService.Override("Moodles", friend.NoteOrFriendCode);
            return;
        }

        // Lacking permissions for body swap
        if (friend.PermissionsGrantedToFriend.Has(PrimaryPermissions.Moodles) is false)
        {
            logService.LackingPermissions("Moodles", friend.NoteOrFriendCode);
            return;
        }

        try
        {
            // Get local player's object table address
            var address = await Plugin.RunOnFramework(() => Plugin.ObjectTable[0]?.Address).ConfigureAwait(false);
            if (address is null)
            {
                Plugin.Log.Warning("[MoodlesHandler] Object table address is null, aborting");
                logService.Custom($"{friend.NoteOrFriendCode} tried to apply a moodle but failed unexpectedly");
                return;
            }

            // Deserialize the input string into a moodle
            var moodle = JsonConvert.DeserializeObject<MyStatus>(action.Moodle);
            if (moodle is null)
            {
                Plugin.Log.Warning("[MoodlesHandler] Moodle deserialization failed, aborting");
                logService.Custom($"{friend.NoteOrFriendCode} tried to apply a moodle but failed unexpectedly");
                return;
            }

            // To be a valid moodle, an expiration time must be included
            moodle.ExpiresAt = long.MaxValue;

            // Get the existing moodles
            var existingMoodlesBase64String = await moodlesService.GetMoodles(address.Value).ConfigureAwait(false);
            if (existingMoodlesBase64String is null)
            {
                logService.Custom($"{friend.NoteOrFriendCode} tried to apply a moodle but failed unexpectedly");
                return;
            }

            // Indicate if the client does not have moodles, testing purposes for now
            if (existingMoodlesBase64String.Length is 0)
            {
                Plugin.Log.Warning("Target does not have any moodles or moodles returned empty string");
            }

            // Convert the moodle list from a string to byte array
            var bytesMoodles = Convert.FromBase64String(existingMoodlesBase64String);

            // Deserialize the byte array into a moodle list
            var moodles = MemoryPackSerializer.Deserialize<List<MyStatus>>(bytesMoodles, _serializerOptions);
            if (moodles is null)
            {
                Plugin.Log.Warning("[MoodlesHandler] Existing moodles deserialization failed, aborting");
                logService.Custom($"{friend.NoteOrFriendCode} tried to apply a moodle but failed unexpectedly");
                return;
            }

            // Add the new moodle
            moodles.Add(moodle);

            // Re-serialize and convert to string
            var packagedMoodles = MemoryPackSerializer.Serialize(moodles, _serializerOptions);
            var packagedMoodlesBase64String = Convert.ToBase64String(packagedMoodles);

            // Apply moodles
            await moodlesService.SetMoodles(address.Value, packagedMoodlesBase64String).ConfigureAwait(false);

            // Redraw client so mare picks up changes
            await penumbraService.CallRedraw();
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"Unexpected exception while handling moodles action, {e.Message}");
        }
    }
}