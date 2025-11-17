using System.Threading.Tasks;
using AetherRemoteClient.Dependencies.Moodles.Services;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Moodles;

namespace AetherRemoteClient.Handlers.Network;

// ReSharper disable once ConvertToPrimaryConstructor

/// <summary>
///     Handles a <see cref="MoodlesForwardedRequest"/>
/// </summary>
public class MoodlesHandler(MoodlesService moodlesService, LogService logService, PermissionsCheckerManager permissionsCheckerManager)
{
    
    // Const
    private const string Operation = "Moodles";
    private static readonly UserPermissions Permissions = new(PrimaryPermissions2.Moodles, SpeakPermissions2.None, ElevatedPermissions.None);

    /// <summary>
    ///     <inheritdoc cref="MoodlesHandler"/>
    /// </summary>
    public async Task<ActionResult<Unit>> Handle(MoodlesForwardedRequest request)
    {
        var placeholder = permissionsCheckerManager.GetSenderAndCheckPermissions(Operation, request.SenderFriendCode, Permissions);
        if (placeholder.Result is not ActionResultEc.Success)
            return ActionResultBuilder.Fail(placeholder.Result);
        
        if (placeholder.Value is not { } friend)
            return ActionResultBuilder.Fail(ActionResultEc.ValueNotSet);
        
        // Attempt to apply the Moodle
        if (await moodlesService.ApplyMoodle(request.Info).ConfigureAwait(false))
        {
            logService.Custom($"{friend.NoteOrFriendCode} applied {MoodlesService.RemoveTagsFromTitle(request.Info.Title)} to you");
            return ActionResultBuilder.Ok();
        }

        logService.Custom($"{friend.NoteOrFriendCode} tried to apply a Moodle to you but an error occurred");
        return ActionResultBuilder.Fail(ActionResultEc.Unknown);
    }
}