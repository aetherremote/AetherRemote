using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AetherRemoteClient.Ipc;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Moodles;
using MemoryPack;
using Moodles.Data;
using Newtonsoft.Json;

namespace AetherRemoteClient.Handlers.Network;

/// <summary>
///     Handles a <see cref="MoodlesForwardedRequest"/>
/// </summary>
public class MoodlesHandler(
    LogService logService,
    MoodlesIpc moodles,
    PenumbraIpc penumbra,
    ForwardedRequestManager forwardedRequestManager)
{
    // Const
    private const string Operation = "Moodles";
    private const PrimaryPermissions2 Permissions = PrimaryPermissions2.Moodles;
    
    // Instantiated
    private readonly MemoryPackSerializerOptions _serializerOptions = new()
    {
        StringEncoding = StringEncoding.Utf16
    };

    /// <summary>
    ///     <inheritdoc cref="MoodlesHandler"/>
    /// </summary>
    public async Task<ActionResult<Unit>> Handle(MoodlesForwardedRequest request)
    {
        Plugin.Log.Info($"{request}");

        var placeholder = forwardedRequestManager.Placehold(Operation, request.SenderFriendCode, Permissions);
        if (placeholder.Result is not ActionResultEc.Success)
            return ActionResultBuilder.Fail(placeholder.Result);
        
        if (placeholder.Value is not { } friend)
            return ActionResultBuilder.Fail(ActionResultEc.ValueNotSet);

        try
        {
            // Get local player's object table address
            var address = await Plugin.RunOnFramework(() => Plugin.ObjectTable[0]?.Address).ConfigureAwait(false);
            if (address is null)
            {
                Plugin.Log.Warning("[MoodlesHandler] Object table address is null, aborting");
                logService.Custom($"{friend.NoteOrFriendCode} tried to apply a moodle but failed unexpectedly");
                return ActionResultBuilder.Fail(ActionResultEc.ClientPluginDependency);
            }

            // Deserialize the input string into a moodle
            var moodle = JsonConvert.DeserializeObject<MyStatus>(request.Moodle);
            if (moodle is null)
            {
                Plugin.Log.Warning("[MoodlesHandler] Moodle deserialization failed, aborting");
                logService.Custom($"{friend.NoteOrFriendCode} tried to apply a moodle but failed unexpectedly");
                return ActionResultBuilder.Fail(ActionResultEc.ClientPluginDependency);
            }
            
            // To be a valid moodle, an expiration time must be included
            moodle.ExpiresAt = moodle.TotalDurationSeconds is 0
                ? long.MaxValue
                : DateTimeOffset.Now.ToUnixTimeMilliseconds() * moodle.TotalDurationSeconds * 1000;

            // Get the existing moodles
            var existingMoodlesBase64String = await moodles.GetMoodles(address.Value).ConfigureAwait(false);
            if (existingMoodlesBase64String is null)
            {
                logService.Custom($"{friend.NoteOrFriendCode} tried to apply a moodle but couldn't retrieve moodles");
                return ActionResultBuilder.Fail(ActionResultEc.ClientPluginDependency);
            }

            // Make a list of moodles
            var existingMoodles = new List<MyStatus>();

            // If the client already has moodles, deserialize those and add to it
            if (existingMoodlesBase64String.Length is not 0)
            {
                // Convert the moodle list from a string to byte array
                var bytesMoodles = Convert.FromBase64String(existingMoodlesBase64String);

                // Deserialize the byte array into a moodle list
                existingMoodles = MemoryPackSerializer.Deserialize<List<MyStatus>>(bytesMoodles, _serializerOptions);
                if (existingMoodles is null)
                {
                    Plugin.Log.Warning("[MoodlesHandler] Existing moodles deserialization failed, aborting");
                    logService.Custom($"{friend.NoteOrFriendCode} tired to apply moodle but deserialization failed");
                    return ActionResultBuilder.Fail(ActionResultEc.ClientPluginDependency);
                }
            }

            // Find a matching moodle
            var index = existingMoodles.FindIndex(m => m.Title == moodle.Title);

            // If we found an index...
            if (index > -1)
            {
                // If it is stackable...
                if (existingMoodles[index].StackOnReapply)
                {
                    // Update the stacks
                    Plugin.Log.Info($"[MoodleHandler] Stacking {moodle.Title}");
                    existingMoodles[index].Stacks++;
                }
                else
                {
                    // Otherwise, exit early
                    Plugin.Log.Info($"[MoodleHandler] Moodle {moodle.Title} already exists and is not stackable");
                    return ActionResultBuilder.Ok();
                }
            }
            else
            {
                // If we didn't, just add the moodle
                existingMoodles.Add(moodle);
            }

            // Re-serialize and convert to string
            var packagedMoodles = MemoryPackSerializer.Serialize(existingMoodles, _serializerOptions);
            var packagedMoodlesBase64String = Convert.ToBase64String(packagedMoodles);

            // Apply moodles
            var set = await moodles.SetMoodles(address.Value, packagedMoodlesBase64String).ConfigureAwait(false);
            if (set is false)
            {
                logService.Custom($"{friend.NoteOrFriendCode} tried to apply a moodle to you but failed");
                ActionResultBuilder.Fail(ActionResultEc.ClientPluginDependency);
            }

            // Redraw client so mare picks up changes
            if (await penumbra.CallRedraw() is false)
                Plugin.Log.Warning("[MoodlesHandler] Unable to redraw");

            // Log success
            logService.Custom($"{friend.NoteOrFriendCode} applied the {moodle.Title} moodle to you");
            return ActionResultBuilder.Ok();
        }
        catch (Exception e)
        {
            logService.Custom($"{friend.NoteOrFriendCode} tried to apply a moodle to you but failed unexpectedly");
            Plugin.Log.Error($"Unexpected exception while handling moodles action, {e.Message}");
            return ActionResultBuilder.Fail(ActionResultEc.Unknown);
        }
    }
}