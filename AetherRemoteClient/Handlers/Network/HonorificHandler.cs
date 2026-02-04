using System;
using System.Threading.Tasks;
using AetherRemoteClient.Dependencies.Honorific.Services;
using AetherRemoteClient.Handlers.Network.Base;
using AetherRemoteClient.Services;
using AetherRemoteClient.Utils;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Honorific;
using Microsoft.AspNetCore.SignalR.Client;

namespace AetherRemoteClient.Handlers.Network;

/// <summary>
///     Handles a <see cref="HonorificCommand"/>
/// </summary>
public class HonorificHandler : AbstractNetworkHandler, IDisposable
{
    // Const
    private const string Operation = "Honorific";
    private static readonly ResolvedPermissions Permissions = new(PrimaryPermissions2.Honorific, SpeakPermissions2.None, ElevatedPermissions.None);
    
    // Injected
    private readonly HonorificService _honorific;
    private readonly LogService _log;
    
    // Instantiated
    private readonly IDisposable _handler;
    
    /// <summary>
    ///     <inheritdoc cref="HonorificHandler"/>
    /// </summary>
    public HonorificHandler(
        AccountService account, 
        FriendsListService friends, 
        HonorificService honorific, 
        LogService log, 
        NetworkService network, 
        PauseService pause) : base(account, friends, log, pause)
    {
        _honorific = honorific;
        _log = log;
        
        _handler = network.Connection.On<HonorificCommand, ActionResult<Unit>>(HubMethod.Honorific, Handle);
    }

    /// <summary>
    ///     <inheritdoc cref="EmoteHandler"/>
    /// </summary>
    private async Task<ActionResult<Unit>> Handle(HonorificCommand request)
    {
        Plugin.Log.Verbose($"{request}");
        
        var sender = TryGetFriendWithCorrectPermissions(Operation, request.SenderFriendCode, Permissions);
        if (sender.Result is not ActionResultEc.Success)
            return ActionResultBuilder.Fail(sender.Result);
        
        if (sender.Value is not { } friend)
            return ActionResultBuilder.Fail(ActionResultEc.ValueNotSet);
        
        try
        {
            if (await Plugin.RunOnFramework(() => Plugin.ObjectTable.LocalPlayer?.ObjectIndex).ConfigureAwait(false) is not { } index)
                return ActionResultBuilder.Fail(ActionResultEc.ClientNoLocalPlayer);

            if (await _honorific.SetCharacterTitle(index, request.Honorific).ConfigureAwait(false))
            {
                NotificationHelper.Honorific();
                _log.Custom($"{friend.NoteOrFriendCode} applied the {request.Honorific.Title} honorific to you");
                return ActionResultBuilder.Ok();
            }
            
            _log.Custom($"{friend.NoteOrFriendCode} failed to apply an honorific to you");
            return ActionResultBuilder.Fail(ActionResultEc.ClientPluginDependency);
        }
        catch (Exception e)
        {
            _log.Custom($"{friend.NoteOrFriendCode} unexpectedly failed to apply an honorific to you");
            Plugin.Log.Warning($"[HonorificHandler.Handle] {e}");
            return ActionResultBuilder.Fail(ActionResultEc.Unknown);
        }
    }
    
    public void Dispose()
    {
        _handler.Dispose();
        GC.SuppressFinalize(this);
    }
}