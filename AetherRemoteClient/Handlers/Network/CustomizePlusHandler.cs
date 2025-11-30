using System;
using System.Text;
using System.Threading.Tasks;
using AetherRemoteClient.Dependencies.CustomizePlus.Services;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Customize;

namespace AetherRemoteClient.Handlers.Network;

// ReSharper disable once ConvertToPrimaryConstructor

/// <summary>
///     Handles a <see cref="CustomizeForwardedRequest"/>
/// </summary>
public class CustomizePlusHandler(CustomizePlusService customizePlusService, LogService logService, PermissionsCheckerManager permissionsCheckerManager)
{
    // Const
    private const string Operation = "Customize+";
    private static readonly UserPermissions Permissions = new(PrimaryPermissions2.CustomizePlus, SpeakPermissions2.None, ElevatedPermissions.None);
    
    /// <summary>
    ///     <inheritdoc cref="MoodlesHandler"/>
    /// </summary>
    public async Task<ActionResult<Unit>> Handle(CustomizeForwardedRequest request)
    {
        Plugin.Log.Info($"{request}");
        
        var placeholder = permissionsCheckerManager.GetSenderAndCheckPermissions(Operation, request.SenderFriendCode, Permissions);
        if (placeholder.Result is not ActionResultEc.Success)
            return ActionResultBuilder.Fail(placeholder.Result);
        
        if (placeholder.Value is not { } friend)
            return ActionResultBuilder.Fail(ActionResultEc.ValueNotSet);

        try
        {
            var bytes = Convert.FromBase64String(request.Data);
            var json = Encoding.UTF8.GetString(bytes);
            
            if (await customizePlusService.DeleteTemporaryCustomizeAsync().ConfigureAwait(false) is false)
            {
                Plugin.Log.Warning("[CustomizePlusHandler] Unable to delete existing customize");
                return ActionResultBuilder.Fail(ActionResultEc.ClientPluginDependency);
            }
            
            if (await customizePlusService.ApplyCustomizeAsync(json).ConfigureAwait(false) is false)
            {
                Plugin.Log.Warning("[CustomizePlusHandler] Unable to apply customize");
                return ActionResultBuilder.Fail(ActionResultEc.ClientPluginDependency);
            }
            
            logService.Custom($"{friend.NoteOrFriendCode} applied a customize plus template to you");
            return ActionResultBuilder.Ok();
        }
        catch (Exception e)
        {
            logService.Custom($"{friend.NoteOrFriendCode} tried to apply a customization template to you but failed unexpectedly");
            Plugin.Log.Error($"Unexpected exception while handling customize plus action, {e.Message}");
            return ActionResultBuilder.Fail(ActionResultEc.Unknown);
        }
    }
}