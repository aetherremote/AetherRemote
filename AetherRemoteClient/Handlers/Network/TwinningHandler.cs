using System;
using System.Threading.Tasks;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Enums;
using AetherRemoteClient.Handlers.Network.Base;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Twinning;
using AetherRemoteCommon.Util;
using Microsoft.AspNetCore.SignalR.Client;

namespace AetherRemoteClient.Handlers.Network;

/// <summary>
///     Handles a <see cref="TwinningForwardedRequest"/>
/// </summary>
public class TwinningHandler : AbstractNetworkHandler, IDisposable
{
    // Const
    private const string Operation = "Twinning";
    
    // Injected
    private readonly IdentityService _identity;
    private readonly LogService _log;
    private readonly CharacterTransformationManager _characterTransformation;
    
    // Instantiated
    private readonly IDisposable _handler;
    
    /// <summary>
    ///     <inheritdoc cref="TwinningHandler"/>
    /// </summary>
    public TwinningHandler(FriendsListService friends, IdentityService identity, LogService log, NetworkService network, PauseService pause, CharacterTransformationManager characterTransformation) : base(friends, log, pause)
    {
        _identity = identity;
        _log = log;
        _characterTransformation = characterTransformation;

        _handler = network.Connection.On<TwinningForwardedRequest, ActionResult<Unit>>(HubMethod.Twinning, Handle);
    }
    
    /// <summary>
    ///     <inheritdoc cref="TwinningHandler"/>
    /// </summary>
    private async Task<ActionResult<Unit>> Handle(TwinningForwardedRequest request)
    {
        Plugin.Log.Verbose($"{request}");
        
        var primary = request.SwapAttributes.ToPrimaryPermission() | PrimaryPermissions2.Twinning;
        var elevated = request.LockCode is null 
            ? ElevatedPermissions.None 
            : ElevatedPermissions.PermanentTransformation;
        
        var permissions = new UserPermissions(primary, SpeakPermissions2.None, elevated);
        
        var sender = TryGetFriendWithCorrectPermissions(Operation, request.SenderFriendCode, permissions);
        if (sender.Result is not ActionResultEc.Success)
            return ActionResultBuilder.Fail(sender.Result);
        
        if (sender.Value is not { } friend)
            return ActionResultBuilder.Fail(ActionResultEc.ValueNotSet);

        // TODO: Re-enable when a mare solution is found
        /*
        if (permanentTransformationHandler.IsPermanentTransformed)
            return ActionResultBuilder.Fail(ActionResultEc.ClientPermanentlyTransformed);
         
        if (request.LockCode is not null)
        {
            await permanentTransformationHandler.ApplyPermanentCharacterTransformation(friend.NoteOrFriendCode,
                request.LockCode, request.CharacterName, request.SwapAttributes);
        }
        else
        {
            await characterTransformationManager.ApplyCharacterTransformation(request.CharacterName, request.SwapAttributes);
        }
        */
        
        // Try to apply the transformation
        var result = await _characterTransformation.ApplyCharacterTransformation(request.CharacterName, request.SwapAttributes);
        if (result.Success is not ApplyCharacterTransformationErrorCode.Success)
        {
            // Log the failure
            Plugin.Log.Warning($"[TwinningHandler.Handle] Applying a twinning failed, {result.Success}");
            _log.Custom($"{friend.NoteOrFriendCode} tried to twin with you, but an internal error occured");
            return ActionResultBuilder.Fail(ActionResultEc.ClientPluginDependency);
        }
        
        // Set your new identity
        _identity.AddAlteration(IdentityAlterationType.Twinning, friend.NoteOrFriendCode);
        
        // Log success
        _log.Custom($"{friend.NoteOrFriendCode} twinned you with {request.CharacterName}");
        return ActionResultBuilder.Ok();
    }

    public void Dispose()
    {
        _handler.Dispose();
        GC.SuppressFinalize(this);
    }
}