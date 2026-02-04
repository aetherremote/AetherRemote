using System;
using System.Text;
using System.Threading.Tasks;
using AetherRemoteClient.Dependencies.CustomizePlus.Services;
using AetherRemoteClient.Handlers.Network.Base;
using AetherRemoteClient.Services;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Customize;
using Microsoft.AspNetCore.SignalR.Client;

namespace AetherRemoteClient.Handlers.Network;

/// <summary>
///     Handles a <see cref="CustomizeCommand"/>
/// </summary>
public class CustomizePlusHandler : AbstractNetworkHandler, IDisposable
{
    // Const
    private const string Operation = "Customize+";
    private static readonly ResolvedPermissions Permissions = new(PrimaryPermissions2.CustomizePlus, SpeakPermissions2.None, ElevatedPermissions.None);
    
    // Injected
    private readonly CustomizePlusService _customize;
    private readonly LogService _log;
    
    // Instantiated
    private readonly IDisposable _handler;
    
    /// <summary>
    ///     <inheritdoc cref="CustomizePlusHandler"/>
    /// </summary>
    public CustomizePlusHandler(
        AccountService account, 
        CustomizePlusService customize, 
        FriendsListService friends, 
        LogService log, 
        NetworkService network, 
        PauseService pause) : base(account, friends, log, pause)
    {
        _customize = customize;
        _log = log;

        _handler = network.Connection.On<CustomizeCommand, ActionResult<Unit>>(HubMethod.CustomizePlus, Handle);
    }
    
    /// <summary>
    ///     <inheritdoc cref="MoodlesHandler"/>
    /// </summary>
    private async Task<ActionResult<Unit>> Handle(CustomizeCommand request)
    {
        Plugin.Log.Verbose($"{request}");
        
        var sender = TryGetFriendWithCorrectPermissions(Operation, request.SenderFriendCode, Permissions);
        if (sender.Result is not ActionResultEc.Success)
            return ActionResultBuilder.Fail(sender.Result);
        
        if (sender.Value is not { } friend)
            return ActionResultBuilder.Fail(ActionResultEc.ValueNotSet);

        try
        {
            var json = Encoding.UTF8.GetString(request.JsonBoneDataBytes);
            if (await _customize.DeleteTemporaryCustomizeAsync().ConfigureAwait(false) is false)
            {
                Plugin.Log.Warning("[CustomizePlusHandler] Unable to delete existing customize");
                return ActionResultBuilder.Fail(ActionResultEc.ClientPluginDependency);
            }
            
            if (await _customize.ApplyCustomizeAsync(json).ConfigureAwait(false) is false)
            {
                Plugin.Log.Warning("[CustomizePlusHandler] Unable to apply customize");
                return ActionResultBuilder.Fail(ActionResultEc.ClientPluginDependency);
            }
            
            _log.Custom($"{friend.NoteOrFriendCode} applied a customize plus template to you");
            return ActionResultBuilder.Ok();
        }
        catch (Exception e)
        {
            _log.Custom($"{friend.NoteOrFriendCode} tried to apply a customization template to you but failed unexpectedly");
            Plugin.Log.Error($"Unexpected exception while handling customize plus action, {e.Message}");
            return ActionResultBuilder.Fail(ActionResultEc.Unknown);
        }
    }

    public void Dispose()
    {
        _handler.Dispose();
        GC.SuppressFinalize(this);
    }
}