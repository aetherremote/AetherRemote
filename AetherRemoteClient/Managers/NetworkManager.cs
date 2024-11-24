using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AetherRemoteClient.Accessors.Glamourer;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Log;
using AetherRemoteClient.Providers;
using AetherRemoteClient.Uncategorized;
using AetherRemoteCommon;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.CommonChatMode;
using AetherRemoteCommon.Domain.CommonGlamourerApplyType;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Commands;
using AetherRemoteCommon.Domain.Permissions;
using Microsoft.AspNetCore.SignalR.Client;

namespace AetherRemoteClient.Managers;

/// <summary>
/// Manages incoming network connections and handles them accordingly
/// </summary>
public class NetworkManager : IDisposable
{
    // Const
    private const GlamourerApplyFlag CustomizationFlags = GlamourerApplyFlag.Once | GlamourerApplyFlag.Customization;
    private const GlamourerApplyFlag EquipmentFlags = GlamourerApplyFlag.Once | GlamourerApplyFlag.Equipment;
    
    // Instantiate
    private readonly ActionQueueProvider _actionQueueProvider;
    private readonly ClientDataManager _clientDataManager;
    private readonly EmoteProvider _emoteProvider;
    private readonly GlamourerAccessor _glamourerAccessor;
    private readonly HistoryLogManager _historyLogManager;
    private readonly ModManager _modManager;
    private readonly NetworkProvider _networkProvider;
    private readonly WorldProvider _worldProvider;
    
    /// <summary>
    /// List of all the subscribers created when calling the <see cref="HubConnection.On(string, Type[], Func{TResult}, object)"/>
    /// </summary>
    private readonly List<IDisposable> _connectionSubscribers = [];

    /// <summary>
    /// <inheritdoc cref="NetworkManager"/>
    /// </summary>
    public NetworkManager(
        ActionQueueProvider actionQueueProvider,
        ClientDataManager clientDataManager,
        EmoteProvider emoteProvider,
        GlamourerAccessor glamourerAccessor,
        HistoryLogManager historyLogManager,
        ModManager modManager,
        NetworkProvider networkProvider,
        WorldProvider worldProvider)
    {
        _actionQueueProvider = actionQueueProvider;
        _clientDataManager = clientDataManager;
        _emoteProvider = emoteProvider;
        _glamourerAccessor = glamourerAccessor;
        _historyLogManager = historyLogManager;
        _modManager = modManager;
        _networkProvider = networkProvider;
        _worldProvider = worldProvider;
        
        _connectionSubscribers.Add(networkProvider.RegisterHandler<UpdateOnlineStatusCommand>(Network.Commands.UpdateOnlineStatus, HandleUpdateOnlineStatus));
        _connectionSubscribers.Add(networkProvider.RegisterHandler<UpdateLocalPermissionsCommand>(Network.Commands.UpdateLocalPermissions, HandleLocalPermissions));
        _connectionSubscribers.Add(networkProvider.RegisterHandler<EmoteCommand>(Network.Commands.Emote, HandleEmote));
        _connectionSubscribers.Add(networkProvider.RegisterHandler<SpeakCommand>(Network.Commands.Speak, HandleSpeak));
        _connectionSubscribers.Add(networkProvider.RegisterHandler<TransformCommand>(Network.Commands.Transform, HandleTransform));
        _connectionSubscribers.Add(networkProvider.RegisterHandler<RevertCommand>(Network.Commands.Revert, HandleRevert));
        _connectionSubscribers.Add(networkProvider.RegisterHandler<BodySwapCommand>(Network.Commands.BodySwap, HandleBodySwap));
        _connectionSubscribers.Add(networkProvider.RegisterHandlerAsync<BodySwapQueryRequest, BodySwapQueryResponse>(Network.BodySwapQuery, HandleBodySwapQuery));
    }
    
    private void HandleUpdateOnlineStatus(UpdateOnlineStatusCommand command)
    {
        Plugin.Log.Verbose($"{command}");
        _clientDataManager.FriendsList.UpdateFriendOnlineStatus(command.FriendCode, command.Online);
        _clientDataManager.FriendsList.UpdateLocalPermissions(command.FriendCode, command.Permissions);
    }

    private void HandleLocalPermissions(UpdateLocalPermissionsCommand command)
    {
        Plugin.Log.Verbose($"{command}");
        _clientDataManager.FriendsList.UpdateLocalPermissions(command.FriendCode, command.PermissionsGrantedToUser);
    }

    private void HandleEmote(EmoteCommand command)
    {
        Plugin.Log.Verbose($"{command}");
        
        var noteOrFriendCode = Plugin.Configuration.Notes.TryGetValue(command.SenderFriendCode, out var note) ? note : command.SenderFriendCode;
        var friend = _clientDataManager.FriendsList.FindFriend(command.SenderFriendCode);
        if (friend is null)
        {
            var message = HistoryLog.NotFriends("Emote", noteOrFriendCode);
            Plugin.Log.Warning(message);
            _historyLogManager.LogHistory(message);
            return;
        }

        if (friend.PermissionsGrantedToFriend.Primary.HasFlag(PrimaryPermissions.Emote) is false)
        {
            var message = HistoryLog.LackingPermissions("Emote", noteOrFriendCode);
            Plugin.Log.Warning(message);
            _historyLogManager.LogHistory(message);
            return;
        }

        if (_emoteProvider.ValidEmote(command.Emote) is false)
        {
            var message = HistoryLog.InvalidData("Emote", noteOrFriendCode);
            Plugin.Log.Warning(message);
            _historyLogManager.LogHistory(message);
            return;
        }
        
        _actionQueueProvider.EnqueueEmoteAction(command.SenderFriendCode, command.Emote, command.DisplayLogMessage);
    }
    
    private void HandleSpeak(SpeakCommand command)
    {
        Plugin.Log.Verbose($"{command}");
        
        var noteOrFriendCode = Plugin.Configuration.Notes.TryGetValue(command.SenderFriendCode, out var note) ? note : command.SenderFriendCode;
        var friend = _clientDataManager.FriendsList.FindFriend(command.SenderFriendCode);
        if (friend is null)
        {
            var message = HistoryLog.NotFriends("Speak", noteOrFriendCode);
            Plugin.Log.Information(message);
            _historyLogManager.LogHistory(message);
            return;
        }

        var linkshellResult = 0;
        if (command.ChatMode is ChatMode.Linkshell or ChatMode.CrossWorldLinkshell)
            linkshellResult = int.TryParse(command.Extra ?? string.Empty, out var linkshellNumber) ? linkshellNumber : linkshellResult;

        if (PermissionChecker.HasValidSpeakPermissions(command.ChatMode, friend.PermissionsGrantedToFriend, linkshellResult) == false)
        {
            var message = HistoryLog.LackingPermissions("Speak", noteOrFriendCode);
            Plugin.Log.Information(message);
            _historyLogManager.LogHistory(message);
            return;
        }

        if (command.ChatMode is ChatMode.Tell)
        {
            var split = command.Extra?.Split('@');
            if(split is not { Length: 2 } || _worldProvider.IsValidWorld(split[1]) == false)
            {
                var message = HistoryLog.InvalidData("Speak", noteOrFriendCode);
                Plugin.Log.Warning(message);
                return;
            }
        }

        _actionQueueProvider.EnqueueSpeakAction(command.SenderFriendCode, command.Message, command.ChatMode, command.Extra);
    }
    
    private async Task HandleTransform(TransformCommand command)
    {
        Plugin.Log.Verbose($"{command}");

        var noteOrFriendCode = command.SenderFriendCode;
        var friend = _clientDataManager.FriendsList.FindFriend(command.SenderFriendCode);
        if (friend is null)
        {
            var message = HistoryLog.NotFriends("Transform", noteOrFriendCode);
            Plugin.Log.Information(message);
            _historyLogManager.LogHistory(message);
            return;
        }

        // Check permissions for customize
        if (command.ApplyFlags.HasFlag(GlamourerApplyFlag.Customization) &&
            friend.PermissionsGrantedToFriend.Primary.HasFlag(PrimaryPermissions.Customization) is false)
        {
            var message = HistoryLog.LackingPermissions("Transform - Customization", noteOrFriendCode);
            Plugin.Log.Information(message);
            _historyLogManager.LogHistory(message);
            return;
        }

        // Check permissions for equipment
        if (command.ApplyFlags.HasFlag(GlamourerApplyFlag.Equipment) &&
            friend.PermissionsGrantedToFriend.Primary.HasFlag(PrimaryPermissions.Equipment) is false)
        {
            var message = HistoryLog.LackingPermissions("Transform - Equipment", noteOrFriendCode);
            Plugin.Log.Information(message);
            _historyLogManager.LogHistory(message);
            return;
        }

        if (GameObjectManager.LocalPlayerExists() is false)
        {
            Plugin.Log.Warning($"{noteOrFriendCode} tried to transform you, but you do not have a body to transform");
            return;
        }

        var result = await _glamourerAccessor.ApplyDesignAsync(command.GlamourerData, 0, command.ApplyFlags).ConfigureAwait(false);
        if (result)
        {
            var message = command.ApplyFlags switch
            {
                CustomizationFlags => $"{noteOrFriendCode} changed your appearance",
                EquipmentFlags => $"{noteOrFriendCode} changed your outfit",
                GlamourerApplyFlag.All => $"{noteOrFriendCode} changed your outfit and appearance",
                _ => $"{noteOrFriendCode} changed you"
            };

            Plugin.Log.Information(message);
            _historyLogManager.LogHistoryGlamourer(message, command.GlamourerData);
        }
    }
    
    private void HandleRevert(RevertCommand command)
    {
        Plugin.Log.Verbose($"{command}");

        var noteOrFriendCode = command.SenderFriendCode;
        var friend = _clientDataManager.FriendsList.FindFriend(command.SenderFriendCode);
        if (friend == null)
        {
            var message = HistoryLog.NotFriends("Revert", noteOrFriendCode);
            Plugin.Log.Information(message);
            _historyLogManager.LogHistory(message);
            return;
        }

        // TODO: Confirm this is correct
        if ((friend.PermissionsGrantedToFriend.Primary & (PrimaryPermissions.Customization | PrimaryPermissions.Equipment)) == 0)
        {
            var message = HistoryLog.LackingPermissions("Revert", noteOrFriendCode);
            Plugin.Log.Information(message);
            _historyLogManager.LogHistory(message);
            return;
        }

        if (GameObjectManager.LocalPlayerExists() is false)
        {
            Plugin.Log.Warning($"{noteOrFriendCode} tried to revert you, but you do not have a body to revert");
            return;
        }

        switch (command.RevertType)
        {
            case RevertType.Automation:
                _ = _glamourerAccessor.RevertToAutomation();
                break;
            case RevertType.Game:
                _ = _glamourerAccessor.RevertToGame();
                break;
            case RevertType.None:
            default:
                break;    
        }
    }
    
    private async Task HandleBodySwap(BodySwapCommand command)
    {
        Plugin.Log.Verbose($"{command}");

        var noteOrFriendCode = command.SenderFriendCode;
        var friend = _clientDataManager.FriendsList.FindFriend(command.SenderFriendCode);
        if (friend is null)
        {
            var message = HistoryLog.NotFriends("Body Swap", noteOrFriendCode);
            Plugin.Log.Information(message);
            _historyLogManager.LogHistory(message);
            return;
        }

        if (friend.PermissionsGrantedToFriend.Primary.HasFlag(PrimaryPermissions.BodySwap) is false)
        {
            var message = HistoryLog.LackingPermissions("Body Swap", noteOrFriendCode);
            Plugin.Log.Information(message);
            _historyLogManager.LogHistory(message);
            return;
        }

        if (GameObjectManager.LocalPlayerExists() is false)
        {
            Plugin.Log.Warning($"{noteOrFriendCode} attempted to swap your body, but you don't have a body to swap");
            return;
        }

        // CharacterName will only be present if we are expecting to swap mods
        if (command.CharacterName is not null)
        {
            if (friend.PermissionsGrantedToFriend.Primary.HasFlag(PrimaryPermissions.Mods) is false)
            {
                var message = HistoryLog.LackingPermissions("Mod Swap", noteOrFriendCode);
                Plugin.Log.Information(message);
                _historyLogManager.LogHistory(message);
                return;
            }

            await _modManager.GetAndSetTargetMods(command.CharacterName);
        }

        var result = await _glamourerAccessor.ApplyDesignAsync(command.CharacterData);
        if (result)
        {
            var message = $"{noteOrFriendCode} swapped your body with {command.SenderFriendCode}'s body";
            Plugin.Log.Information(message);
            _historyLogManager.LogHistoryGlamourer(message, command.CharacterData);
        }
    }

    private async Task<BodySwapQueryResponse> HandleBodySwapQuery(BodySwapQueryRequest request)
    {
        Plugin.Log.Verbose($"{request}");

        // TODO: Hook this into configuration
        var noteOrFriendCode = request.SenderFriendCode;
        var friend = _clientDataManager.FriendsList.FindFriend(request.SenderFriendCode);
        if (friend is null)
        {
            var message = HistoryLog.NotFriends("Body Swap Query", noteOrFriendCode);
            Plugin.Log.Information(message);
            _historyLogManager.LogHistory(message);
            return new BodySwapQueryResponse();
        }

        if (friend.PermissionsGrantedToFriend.Primary.HasFlag(PrimaryPermissions.BodySwap) is false)
        {
            var message = HistoryLog.LackingPermissions("Body Swap Query", noteOrFriendCode);
            Plugin.Log.Information(message);
            _historyLogManager.LogHistory(message);
            return new BodySwapQueryResponse();
        }

        var characterData = await _glamourerAccessor.GetDesignAsync();
        if (characterData is null)
        {
            Plugin.Log.Warning($"{noteOrFriendCode} attempted to scan your body for a swap, but the scan failed");
            return new BodySwapQueryResponse();
        }
        
        if (request.SwapMods == false) return new BodySwapQueryResponse(null, characterData);

        var characterName = GameObjectManager.GetLocalPlayerName();
        if (characterName is not null) return new BodySwapQueryResponse(characterName, characterData);
        
        Plugin.Log.Warning($"{noteOrFriendCode} attempted to scan your body for a swap, but you don't have a body to scan");
        return new BodySwapQueryResponse();
    }

    public void Dispose()
    {
        foreach (var subscriber in _connectionSubscribers)
            subscriber.Dispose();
        
        GC.SuppressFinalize(this);
    }
}