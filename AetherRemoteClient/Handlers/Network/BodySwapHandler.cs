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
using AetherRemoteCommon.Domain.Network.BodySwap;
using AetherRemoteCommon.Util;
using Microsoft.AspNetCore.SignalR.Client;

namespace AetherRemoteClient.Handlers.Network;

/// <summary>
///     Handles a <see cref="BodySwapCommand"/>
/// </summary>
public class BodySwapHandler : AbstractNetworkHandler, IDisposable
{
    // Const
    private const string Operation = "Body Swap";
    
    // Injected
    private readonly IdentityService _identity;
    private readonly LogService _log;
    private readonly CharacterTransformationManager _characterTransformation;
    
    // Instantiated
    private readonly IDisposable _handler;
    
    /// <summary>
    ///     <inheritdoc cref="BodySwapHandler"/>
    /// </summary>
    public BodySwapHandler(FriendsListService friends, IdentityService identity, LogService log, NetworkService network, PauseService pause, CharacterTransformationManager characterTransformation) : base(friends, log, pause)
    {
        _identity = identity;
        _log = log;
        _characterTransformation = characterTransformation;

        _handler = network.Connection.On<BodySwapCommand, ActionResult<Unit>>(HubMethod.BodySwap, Handle);
    }
    
    /// <summary>
    ///     <inheritdoc cref="BodySwapHandler"/>
    /// </summary>
    private async Task<ActionResult<Unit>> Handle(BodySwapCommand request)
    {
        Plugin.Log.Verbose($"{request}");
        
        var primary = request.SwapAttributes.ToPrimaryPermission() | PrimaryPermissions2.BodySwap;
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
        if (_permanentTransformationHandler.IsPermanentTransformed)
            return ActionResultBuilder.Fail(ActionResultEc.ClientPermanentlyTransformed);
            
        if (request.LockCode is not null)
        {
            await _permanentTransformationHandler.ApplyPermanentCharacterTransformation(friend.NoteOrFriendCode,
                request.LockCode, request.CharacterName, request.SwapAttributes);
        }
        else
        {
            await _characterTransformationManager.ApplyCharacterTransformation(request.CharacterName, request.SwapAttributes);
        }
        */
        
        // Try to apply the transformation
        var result = await _characterTransformation.ApplyCharacterTransformation(request.CharacterName, request.SwapAttributes);
        if (result.Success is not ApplyCharacterTransformationErrorCode.Success)
        {
            // Log the failure
            Plugin.Log.Warning($"[BodySwapHandler.Handle] Applying a body swap failed, {result.Success}");
            _log.Custom($"{friend.NoteOrFriendCode} tried to body swap with you, but an internal error occured");
            return ActionResultBuilder.Fail(ActionResultEc.ClientPluginDependency);
        }
        
        // Set your new identity
        _identity.AddAlteration(IdentityAlterationType.BodySwap, friend.NoteOrFriendCode);
        
        // Log Success
        _log.Custom($"{friend.NoteOrFriendCode} swapped your body with {request.CharacterName}'s");
        return ActionResultBuilder.Ok();
    }

    public void Dispose()
    {
        _handler.Dispose();
        GC.SuppressFinalize(this);
    }
}