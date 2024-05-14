using AetherRemoteClient.Providers;
using AetherRemoteCommon;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Become;
using AetherRemoteCommon.Domain.Network.Emote;
using AetherRemoteCommon.Domain.Network.Speak;
using Dalamud.Plugin.Services;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Linq;

namespace AetherRemoteClient.Domain;

/// <summary>
/// Listens for commands from the server. Will also perform validation and enqueue actions to <see cref="ActionQueueProvider"/>
/// </summary>
public class NetworkListener
{
    private readonly ActionQueueProvider actionQueueProvider;
    private readonly EmoteProvider emoteProvider;
    private readonly NetworkProvider networkProvider;
    private readonly IPluginLog logger;

    /// <summary>
    /// Listens for commands from the server. Will also perform validation and enqueue actions to <see cref="ActionQueueProvider"/>
    /// </summary>
    public NetworkListener(
        ActionQueueProvider actionQueueProvider,
        EmoteProvider emoteProvider,
        NetworkProvider networkProvider, 
        IPluginLog logger)
    {
        this.actionQueueProvider = actionQueueProvider;
        this.emoteProvider = emoteProvider;
        this.networkProvider = networkProvider;
        this.logger = logger;

        networkProvider.Connection.On(Constants.ApiOnlineStatus, (OnlineStatusExecute execute) => { SetOnlineStatus(execute); });
        networkProvider.Connection.On(Constants.ApiBecome, (BecomeExecute execute) => { HandleBecome(execute); });
        networkProvider.Connection.On(Constants.ApiEmote, (EmoteExecute execute) => { HandleEmote(execute); });
        networkProvider.Connection.On(Constants.ApiSpeak, (SpeakExecute execute) => { HandleSpeak(execute); });
    }

    public void SetOnlineStatus(OnlineStatusExecute execute)
    {
        var friend = networkProvider.FriendList?.FindFriend(execute.FriendCode);
        if (friend == null)
            return;

        friend.Online = execute.Online;
    }

    public void HandleBecome(BecomeExecute execute)
    {
        var validFriend = networkProvider.FriendList?.FindFriend(execute.SenderFriendCode);
        if (validFriend == null)
        {
            var message = $"Filtered out \'Become\' command from {execute.SenderFriendCode} who is not on your friend list";
            AetherRemoteLogging.Log(execute.SenderFriendCode, message, DateTime.Now, LogType.Error);
            return;
        }

        var hasPermission = PermissionChecker.HasGlamourerPermission(execute.GlamourerApplyType, validFriend.Permissions);
        if (hasPermission == false)
        {
            var message = $"Filtered out \'Become\' command from {execute.SenderFriendCode} who does not have {execute.GlamourerApplyType} permissions";
            AetherRemoteLogging.Log(execute.SenderFriendCode, message, DateTime.Now, LogType.Error);
            return;
        }
        
        actionQueueProvider.EnqueueBecomeAction(execute.SenderFriendCode, execute.GlamourerData, execute.GlamourerApplyType);
    }

    public void HandleEmote(EmoteExecute execute)
    {
        var validFriend = networkProvider.FriendList?.FindFriend(execute.SenderFriendCode);
        if (validFriend == null)
        {
            var message = $"Filtered out \'Emote\' command from {execute.SenderFriendCode} who is not on your friend list";
            AetherRemoteLogging.Log(execute.SenderFriendCode, message, DateTime.Now, LogType.Error);
            return;
        }

        var hasPermission = PermissionChecker.HasEmotePermission(validFriend.Permissions);
        if (hasPermission == false)
        {
            var message = $"Filtered out \'Emote\' command from {execute.SenderFriendCode} who does not have Emote permissions";
            AetherRemoteLogging.Log(execute.SenderFriendCode, message, DateTime.Now, LogType.Error);
            return;
        }

        var validEmote = emoteProvider.Emotes.Any(emote => emote == execute.Emote);
        if (validEmote == false)
        {
            var message = $"Filtered out \'Emote\' command from {execute.SenderFriendCode} that contained an invalid emote";
            AetherRemoteLogging.Log(execute.SenderFriendCode, message, DateTime.Now, LogType.Error);
            return;
        }

        actionQueueProvider.EnqueueEmoteAction(execute.SenderFriendCode, execute.Emote);
    }

    public void HandleSpeak(SpeakExecute execute)
    {
        var validFriend = networkProvider.FriendList?.FindFriend(execute.SenderFriendCode);
        if (validFriend == null)
        {
            var message = $"Filtered out \'Speak\' command from {execute.SenderFriendCode} who is not on your friend list";
            AetherRemoteLogging.Log(execute.SenderFriendCode, message, DateTime.Now, LogType.Error);
            return;
        }

        var hasPermission = PermissionChecker.HasSpeakPermission(execute.ChatMode, validFriend.Permissions);
        if (hasPermission == false)
        {
            var message = $"Filtered out \'Speak\' command from {execute.SenderFriendCode} who does not have {execute.ChatMode} permissions";
            AetherRemoteLogging.Log(execute.SenderFriendCode, message, DateTime.Now, LogType.Error);
            return;
        }

        actionQueueProvider.EnqueueSpeakAction(execute.SenderFriendCode, execute.Message, execute.ChatMode, execute.Extra);
    } 
}
