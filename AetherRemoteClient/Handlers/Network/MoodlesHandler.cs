using System;
using System.Threading.Tasks;
using AetherRemoteClient.Dependencies.Moodles.Services;
using AetherRemoteClient.Handlers.Network.Base;
using AetherRemoteClient.Services;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Moodles;
using Microsoft.AspNetCore.SignalR.Client;

namespace AetherRemoteClient.Handlers.Network;

/// <summary>
///     Handles a <see cref="MoodlesCommand"/>
/// </summary>
public class MoodlesHandler : AbstractNetworkHandler, IDisposable
{
    // Const
    private const string Operation = "Moodles";
    private static readonly ResolvedPermissions Permissions = new(PrimaryPermissions2.Moodles, SpeakPermissions2.None, ElevatedPermissions.None);

    // Injected
    private readonly LogService _log;
    private readonly MoodlesService _moodles;
    
    // Instantiated
    private readonly IDisposable _handler;
    
    /// <summary>
    ///     <inheritdoc cref="MoodlesHandler"/>
    /// </summary>
    public MoodlesHandler(
        AccountService account, 
        FriendsListService friends,
        LogService log,
        MoodlesService moodles,
        NetworkService network, 
        PauseService pause) : base(account, friends, log, pause)
    {
        _log = log;
        _moodles = moodles;

        _handler = network.Connection.On<MoodlesCommand, ActionResult<Unit>>(HubMethod.Moodles, Handle);
    }
    
    /// <summary>
    ///     <inheritdoc cref="MoodlesHandler"/>
    /// </summary>
    private async Task<ActionResult<Unit>> Handle(MoodlesCommand request)
    {
        Plugin.Log.Verbose($"{request}");

        // If the client has not accepted the agreement
        if (AgreementsService.HasAgreedTo(AgreementsService.Agreements.MoodlesWarning) is false)
            return ActionResultBuilder.Fail(ActionResultEc.HasNotAcceptedAgreement);
        
        var sender = TryGetFriendWithCorrectPermissions(Operation, request.SenderFriendCode, Permissions);
        if (sender.Result is not ActionResultEc.Success)
            return ActionResultBuilder.Fail(sender.Result);
        
        if (sender.Value is not { } friend)
            return ActionResultBuilder.Fail(ActionResultEc.ValueNotSet);
        
        // Attempt to apply the Moodle
        if (await _moodles.ApplyMoodle(request.Info).ConfigureAwait(false))
        {
            _log.Custom($"{friend.NoteOrFriendCode} applied {MoodlesService.RemoveTagsFromTitle(request.Info.Title)} to you");
            return ActionResultBuilder.Ok();
        }

        _log.Custom($"{friend.NoteOrFriendCode} tried to apply a Moodle to you but an error occurred");
        return ActionResultBuilder.Fail(ActionResultEc.Unknown);
    }

    public void Dispose()
    {
        _handler.Dispose();
        GC.SuppressFinalize(this);
    }
}