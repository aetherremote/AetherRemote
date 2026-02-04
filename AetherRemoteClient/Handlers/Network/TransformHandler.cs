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
using AetherRemoteCommon.Domain.Network.Transform;
using AetherRemoteCommon.Util;
using Microsoft.AspNetCore.SignalR.Client;

namespace AetherRemoteClient.Handlers.Network;

// ReSharper disable once ConvertToPrimaryConstructor

/// <summary>
///     Handles a <see cref="TransformCommand"/>
/// </summary>
public class TransformHandler : AbstractNetworkHandler, IDisposable
{
    // Const
    private const string Operation = "Transform";
    
    // Injected
    private readonly IdentityService _identity;
    private readonly LogService _log;
    private readonly CharacterTransformationManager _characterTransformation;
    
    // Instantiated
    private readonly IDisposable _handler;
    
    /// <summary>
    ///     <inheritdoc cref="TransformHandler"/>
    /// </summary>
    public TransformHandler(
        AccountService account,
        FriendsListService friends,
        IdentityService identity,
        LogService log, 
        NetworkService network, 
        PauseService pause, 
        CharacterTransformationManager characterTransformation) : base(account, friends, log, pause)
    {
       _identity = identity;
       _log = log;
       _characterTransformation = characterTransformation;

       _handler = network.Connection.On<TransformCommand, ActionResult<Unit>>(HubMethod.Transform, Handle);
    }
    
    /// <summary>
    ///     <inheritdoc cref="TransformHandler"/>
    /// </summary>
    private async Task<ActionResult<Unit>> Handle(TransformCommand request)
    {
        Plugin.Log.Verbose($"{request}");
        
        // Setup permissions
        var primary = request.GlamourerApplyType.ToPrimaryPermission();
        var elevated = request.LockCode is null 
            ? ElevatedPermissions.None 
            : ElevatedPermissions.PermanentTransformation;
        
        // Build permissions
        var permissions = new ResolvedPermissions(primary, SpeakPermissions2.None, elevated);
        
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
            await permanentTransformationHandler.ApplyPermanentTransformation(friend.NoteOrFriendCode, request.LockCode,
                request.GlamourerData, request.GlamourerApplyType);
        }
        else
        {
            await characterTransformationManager.ApplyGenericTransformation(request.GlamourerData, request.GlamourerApplyType);
        }
        */
        
        // Try to apply the transformation
        var result = await _characterTransformation.ApplyGenericTransformation(request.GlamourerData, request.GlamourerApplyType);
        if (result.Success is not ApplyGenericTransformationErrorCode.Success)
        {
            // Log the failure
            Plugin.Log.Warning($"[TransformHandler.Handle] Applying a transformation failed, {result.Success}");
            _log.Custom($"{friend.NoteOrFriendCode} tried to transform you, but an internal error occured");
            return ActionResultBuilder.Fail(ActionResultEc.ClientPluginDependency);
        }

        // Set your new identity
        _identity.AddAlteration(IdentityAlterationType.Transformation, friend.NoteOrFriendCode);
        
        // Log the success
        _log.Custom($"{friend.NoteOrFriendCode} transformed you");
        return ActionResultBuilder.Ok();
    }

    public void Dispose()
    {
        _handler.Dispose();
        GC.SuppressFinalize(this);
    }
}