using AetherRemoteClient.Accessors.Glamourer;
using AetherRemoteClient.Domain.Log;
using AetherRemoteClient.Providers;
using AetherRemoteCommon;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.CommonGlamourerApplyType;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Commands;
using Microsoft.AspNetCore.SignalR.Client;
using System.Threading.Tasks;

namespace AetherRemoteClient.Domain;

public class NetworkHandler
{
    // Const
    private const GlamourerApplyFlag CustomizationFlags = GlamourerApplyFlag.Once | GlamourerApplyFlag.Customization;
    private const GlamourerApplyFlag EquipmentFlags = GlamourerApplyFlag.Once | GlamourerApplyFlag.Equipment;
    private const GlamourerApplyFlag CustomizationAndEquipmentFlags = GlamourerApplyFlag.Once | GlamourerApplyFlag.Customization | GlamourerApplyFlag.Equipment;
    private const GlamourerApplyFlag BodySwapFlags = GlamourerApplyFlag.Customization | GlamourerApplyFlag.Equipment;

    // Injected
    private readonly ActionQueueProvider actionQueueProvider;
    private readonly ClientDataManager clientDataManager;
    private readonly EmoteProvider emoteProvider;
    private readonly GlamourerAccessor glamourerAccessor;
    private readonly HistoryLogManager historyLogManager;

    public NetworkHandler(
        ActionQueueProvider actionQueueProvider,
        ClientDataManager clientDataManager,
        EmoteProvider emoteProvider,
        GlamourerAccessor glamourerAccessor,
        HistoryLogManager historyLogManager,
        HubConnection connection)
    {
        this.actionQueueProvider = actionQueueProvider;
        this.clientDataManager = clientDataManager;
        this.emoteProvider = emoteProvider;
        this.glamourerAccessor = glamourerAccessor;
        this.historyLogManager = historyLogManager;

        // Execute
        connection.On(Network.Commands.UpdateOnlineStatus, (UpdateOnlineStatusCommand command) => { HandleOnlineStatus(command); });
        connection.On(Network.Commands.Emote, (EmoteCommand command) => { HandleEmote(command); });
        connection.On(Network.Commands.Speak, (SpeakCommand command) => { HandleSpeak(command); });
        connection.On(Network.Commands.Transform, (TransformCommand command) => { _ = HandleTransform(command); });
        connection.On(Network.Commands.BodySwap, (BodySwapCommand command) => { _ = HandleBodySwap(command); });

        // Query
        connection.On(Network.BodySwapQuery, async (BodySwapQueryRequest request) => await HandleBodySwapQuery(request));
    }

    private void HandleOnlineStatus(UpdateOnlineStatusCommand command)
    {
        Plugin.Log.Verbose(command.ToString());
        clientDataManager.FriendsList.UpdateFriendOnlineStatus(command.FriendCode, command.Online);
    }

    private void HandleEmote(EmoteCommand command)
    {
        Plugin.Log.Verbose(command.ToString());

        var noteOrFriendCode = command.SenderFriendCode;
        var friend = clientDataManager.FriendsList.FindFriend(command.SenderFriendCode);
        if (friend == null)
        {
            var message = HistoryLog.NotFriends("Emote", noteOrFriendCode);
            Plugin.Log.Information(message);
            historyLogManager.LogHistory(message);
            return;
        }

        if (PermissionChecker.HasValidEmotePermissions(friend.Permissions) == false)
        {
            var message = HistoryLog.LackingPermissions("Emote", noteOrFriendCode);
            Plugin.Log.Information(message);
            historyLogManager.LogHistory(message);
            return;
        }

        if (emoteProvider.ValidEmote(command.Emote) == false)
        {
            var message = HistoryLog.InvalidData("Emote", noteOrFriendCode);
            Plugin.Log.Warning(message);
            return;
        }

        actionQueueProvider.EnqueueEmoteAction(command.SenderFriendCode, command.Emote);
    }

    private void HandleSpeak(SpeakCommand command)
    {
        Plugin.Log.Verbose(command.ToString());

        var noteOrFriendCode = command.SenderFriendCode;
        var friend = clientDataManager.FriendsList.FindFriend(command.SenderFriendCode);
        if (friend == null)
        {
            var message = HistoryLog.NotFriends("Speak", noteOrFriendCode);
            Plugin.Log.Information(message);
            historyLogManager.LogHistory(message);
            return;
        }

        if (PermissionChecker.HasValidSpeakPermissions(command.ChatMode, friend.Permissions) == false)
        {
            var message = HistoryLog.LackingPermissions("Speak", noteOrFriendCode);
            Plugin.Log.Information(message);
            historyLogManager.LogHistory(message);
            return;
        }

        actionQueueProvider.EnqueueSpeakAction(command.SenderFriendCode, command.Message, command.ChatMode, command.Extra);
    }

    private async Task HandleTransform(TransformCommand command)
    {
        Plugin.Log.Verbose(command.ToString());

        var noteOrFriendCode = command.SenderFriendCode;
        var friend = clientDataManager.FriendsList.FindFriend(command.SenderFriendCode);
        if (friend == null)
        {
            var message = HistoryLog.NotFriends("Transform", noteOrFriendCode);
            Plugin.Log.Information(message);
            historyLogManager.LogHistory(message);
            return;
        }

        if (PermissionChecker.HasValidTransformPermissions(command.ApplyFlags, friend.Permissions) == false)
        {
            var message = HistoryLog.LackingPermissions("Transform", noteOrFriendCode);
            Plugin.Log.Information(message);
            historyLogManager.LogHistory(message);
            return;
        }

        var characterName = LocalPlayerName();
        if (characterName == null)
        {
            Plugin.Log.Warning($"{noteOrFriendCode} tried to transform you, but you do not have a body to transform");
            return;
        }

        var result = await glamourerAccessor.ApplyDesignAsync(characterName, command.GlamourerData, command.ApplyFlags).ConfigureAwait(false);
        if (result)
        {
            var message = command.ApplyFlags switch
            {
                CustomizationFlags => $"{noteOrFriendCode} changed your appearance",
                EquipmentFlags => $"{noteOrFriendCode} changed your outfit",
                CustomizationAndEquipmentFlags => $"{noteOrFriendCode} changed your outfit and appearance",
                _ => $"{noteOrFriendCode} changed you"
            };

            Plugin.Log.Information(message);
            historyLogManager.LogHistoryGlamourer(message, command.GlamourerData);
        }
    }

    private async Task HandleBodySwap(BodySwapCommand command)
    {
        Plugin.Log.Verbose(command.ToString());

        var noteOrFriendCode = command.SenderFriendCode;
        var friend = clientDataManager.FriendsList.FindFriend(command.SenderFriendCode);
        if (friend == null)
        {
            var message = HistoryLog.NotFriends("Body Swap", noteOrFriendCode);
            Plugin.Log.Information(message);
            historyLogManager.LogHistory(message);
            return;
        }

        if (PermissionChecker.HasValidTransformPermissions(BodySwapFlags, friend.Permissions) == false)
        {
            var message = HistoryLog.LackingPermissions("Body Swap", noteOrFriendCode);
            Plugin.Log.Information(message);
            historyLogManager.LogHistory(message);
            return;
        }

        var characterName = LocalPlayerName();
        if (characterName == null)
        {
            Plugin.Log.Warning($"{noteOrFriendCode} attempted to swap your body, but you don't have a body to swap");
            return;
        }

        var result = await glamourerAccessor.ApplyDesignAsync(characterName, command.CharacterData).ConfigureAwait(false);
        if (result)
        {
            var message = $"{noteOrFriendCode} swapped your body with {command.SenderFriendCode}'s body";
            Plugin.Log.Information(message);
            historyLogManager.LogHistoryGlamourer(message, command.CharacterData);
        }
    }

    private async Task<BodySwapQueryResponse> HandleBodySwapQuery(BodySwapQueryRequest request)
    {
        Plugin.Log.Verbose(request.ToString());

        var noteOrFriendCode = request.SenderFriendCode;
        var friend = clientDataManager.FriendsList.FindFriend(request.SenderFriendCode);
        if (friend == null)
        {
            var message = HistoryLog.NotFriends("Body Swap Query", noteOrFriendCode);
            Plugin.Log.Information(message);
            historyLogManager.LogHistory(message);
            return new();
        }

        if (PermissionChecker.HasValidTransformPermissions(BodySwapFlags, friend.Permissions) == false)
        {
            var message = HistoryLog.LackingPermissions("Body Swap Query", noteOrFriendCode);
            Plugin.Log.Information(message);
            historyLogManager.LogHistory(message);
            return new();
        }

        var characterName = LocalPlayerName();
        if (characterName == null)
        {
            Plugin.Log.Warning($"{noteOrFriendCode} attempted to scan your body for a swap, but you don't have a body to scan");
            return new();
        }

        var characterData = await glamourerAccessor.GetDesignAsync(characterName);
        if (characterData == null)
        {
            Plugin.Log.Warning($"{noteOrFriendCode} attempted to scan your body for a swap, but the scan failed");
            return new();
        }

        return new BodySwapQueryResponse(characterData);
    }

    /// <summary>
    /// Attempts to retrieve the name of the local player instance. This will be null is the player is on the main menu or loading to a new zone
    /// </summary>
    private static string? LocalPlayerName() => Plugin.ClientState.LocalPlayer?.Name.ToString();
}
