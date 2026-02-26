using System.Threading.Tasks;
using AetherRemoteClient.Services;
using AetherRemoteClient.Utils;
using AetherRemoteCommon;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network.Possession;
using AetherRemoteCommon.Domain.Network.Possession.Begin;
using AetherRemoteCommon.Domain.Network.Possession.Camera;
using AetherRemoteCommon.Domain.Network.Possession.End;
using AetherRemoteCommon.Domain.Network.Possession.Movement;

namespace AetherRemoteClient.Handlers.Network;

public partial class NetworkHandler
{
    private static readonly ResolvedPermissions PossessionBeginPermissions = new(PrimaryPermissions.None, SpeakPermissions.None, ElevatedPermissions.Possession);
    private static readonly ResolvedPermissions PossessionCameraPermissions = new(PrimaryPermissions.None, SpeakPermissions.None, ElevatedPermissions.Possession);
    private static readonly ResolvedPermissions PossessionEndPermissions = new(PrimaryPermissions.None, SpeakPermissions.None, ElevatedPermissions.Possession);
    private static readonly ResolvedPermissions PossessionMovementPermissions = new(PrimaryPermissions.None, SpeakPermissions.None, ElevatedPermissions.Possession);
    
    private async Task<PossessionResultEc> HandlePossessionBegin(PossessionBeginCommand command)
    {
        Plugin.Log.Verbose($"{command}");
        
        // If the client has not accepted the agreement
        if (AgreementsService.HasAgreedTo(AgreementsService.Agreements.Possession) is false)
            return PossessionResultEc.HasNotAcceptedAgreement;
        
        var sender = TryGetFriendWithCorrectPermissions("PossessionBegin", command.SenderFriendCode, PossessionBeginPermissions);
        if (sender.Result is not ActionResultEc.Success)
        {
            Plugin.Log.Warning($"[PossessionBeginHandler.Handle] {sender.Result}");
            return sender.Result switch
            {
                ActionResultEc.ClientNotFriends => PossessionResultEc.NotFriends,
                ActionResultEc.ClientInSafeMode => PossessionResultEc.SafeMode,
                ActionResultEc.ClientHasSenderPaused => PossessionResultEc.Paused,
                ActionResultEc.ClientHasFeaturePaused => PossessionResultEc.FeaturePaused,
                ActionResultEc.ClientHasNotGrantedSenderPermissions => PossessionResultEc.LackingPermissions,
                _ => PossessionResultEc.Unknown
            };
        }

        // They sent us a move mode that doesn't exist
        if (command.MoveMode > 1)
            return PossessionResultEc.BadData;

        var result = await _possessionManager.BecomePossessed(command.MoveMode).ConfigureAwait(false);
        if (result is PossessionResultEc.Success)
            _logService.Custom($"You were possessed by {sender.Value?.FriendCode ?? string.Empty}");

        return result;
    }
    
    private PossessionResultEc HandlePossessionCamera(PossessionCameraCommand command)
    {
        Plugin.Log.Verbose($"{command}");
        var sender = TryGetFriendWithCorrectPermissions("PossessionCamera", command.SenderFriendCode, PossessionCameraPermissions);
        if (sender.Result is not ActionResultEc.Success)
        {
            Plugin.Log.Warning($"[PossessionCameraHandler.Handle] {sender.Result}");
            return sender.Result switch
            {
                ActionResultEc.ClientNotFriends => PossessionResultEc.NotFriends,
                ActionResultEc.ClientInSafeMode => PossessionResultEc.SafeMode,
                ActionResultEc.ClientHasSenderPaused => PossessionResultEc.Paused,
                ActionResultEc.ClientHasFeaturePaused => PossessionResultEc.FeaturePaused,
                ActionResultEc.ClientHasNotGrantedSenderPermissions => PossessionResultEc.LackingPermissions,
                _ => PossessionResultEc.Unknown
            };
        }
        
        // Data validation
        if (command.HorizontalRotation is < Constraints.Possession.HorizontalMin or > Constraints.Possession.HorizontalMax)
            return PossessionResultEc.BadData;

        if (command.VerticalRotation is < Constraints.Possession.VerticalMin or > Constraints.Possession.VerticalMax)
            return PossessionResultEc.BadData;
        
        if (command.Zoom is < Constraints.Possession.ZoomMin or > Constraints.Possession.ZoomMax)
            return PossessionResultEc.BadData;
        
        return _possessionManager.SetCameraDestination(command.HorizontalRotation, command.VerticalRotation, command.Zoom);
    }
    
    private async Task<PossessionResultEc> HandlePossessionEnd(PossessionEndCommand command)
    {
        Plugin.Log.Verbose($"{command}");
        var sender = TryGetFriendWithCorrectPermissions("PossessionEnd", command.SenderFriendCode, PossessionEndPermissions);
        if (sender.Result is not ActionResultEc.Success)
        {
            Plugin.Log.Warning($"[PossessionEndHandler.Handle] {sender.Result}");
            return sender.Result switch
            {
                ActionResultEc.ClientNotFriends => PossessionResultEc.NotFriends,
                ActionResultEc.ClientInSafeMode => PossessionResultEc.SafeMode,
                ActionResultEc.ClientHasSenderPaused => PossessionResultEc.Paused,
                ActionResultEc.ClientHasFeaturePaused => PossessionResultEc.FeaturePaused,
                ActionResultEc.ClientHasNotGrantedSenderPermissions => PossessionResultEc.LackingPermissions,
                _ => PossessionResultEc.Unknown
            };
        }

        if (_possessionManager.Possessed)
        {
            if (await _possessionManager.Expel(false).ConfigureAwait(false))
                NotificationHelper.Success("Possession Ended", string.Empty);
            
            _logService.Custom($"{sender.Value?.FriendCode ?? string.Empty} stopped possessing you");
        }

        if (_possessionManager.Possessing)
        {
            if (await _possessionManager.Unpossess(false).ConfigureAwait(false))
                NotificationHelper.Success("Possession Ended", string.Empty);
            
            _logService.Custom($"{sender.Value?.FriendCode ?? string.Empty} expelled you from their body");
        }

        _statusService.Possession = null;
        return PossessionResultEc.Success;
    }
    
    private PossessionResultEc HandlePossessionMovement(PossessionMovementCommand command)
    {
        Plugin.Log.Verbose($"{command}");
        var sender = TryGetFriendWithCorrectPermissions("PossessionMovement", command.SenderFriendCode, PossessionMovementPermissions);
        if (sender.Result is not ActionResultEc.Success)
        {
            Plugin.Log.Warning($"[PossessionMovementHandler.Handle] {sender.Result}");
            return sender.Result switch
            {
                ActionResultEc.ClientNotFriends => PossessionResultEc.NotFriends,
                ActionResultEc.ClientInSafeMode => PossessionResultEc.SafeMode,
                ActionResultEc.ClientHasSenderPaused => PossessionResultEc.Paused,
                ActionResultEc.ClientHasFeaturePaused => PossessionResultEc.FeaturePaused,
                ActionResultEc.ClientHasNotGrantedSenderPermissions => PossessionResultEc.LackingPermissions,
                _ => PossessionResultEc.Unknown
            };
        }
        
        // Data validation
        if (command.Horizontal is < -1 or > 1)
            return PossessionResultEc.BadData;

        if (command.Vertical is < -1 or > 1)
            return PossessionResultEc.BadData;
        
        if (command.Turn is < -1 or > 1)
            return PossessionResultEc.BadData;

        if (command.Backwards > 1)
            return PossessionResultEc.BadData;
        
        return _possessionManager.SetMovementDirection(command.Horizontal, command.Vertical, command.Turn, command.Backwards);
    }
}