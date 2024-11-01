using AetherRemoteClient.Accessors.Glamourer;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Log;
using AetherRemoteCommon;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.CommonChatMode;
using AetherRemoteCommon.Domain.CommonGlamourerApplyType;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Commands;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

#pragma warning disable CS0162

namespace AetherRemoteClient.Providers;

/// <summary>
/// Provisions a connection to the server and exposes methods to interact with the server
/// </summary>
public class NetworkProvider : IDisposable
{
#if DEBUG
    private const string HubUrl = "https://localhost:5006/primaryHub";
    private const string PostUrl = "https://localhost:5006/api/auth/login";
#else
    private const string HubUrl = "https://foxitsvc.com:5006/primaryHub";
    private const string PostUrl = "https://foxitsvc.com:5006/api/auth/login";
#endif

    // Const
    private const GlamourerApplyFlag CustomizationFlags = GlamourerApplyFlag.Once | GlamourerApplyFlag.Customization;
    private const GlamourerApplyFlag EquipmentFlags = GlamourerApplyFlag.Once | GlamourerApplyFlag.Equipment;

    // Inject
    private readonly ActionQueueProvider actionQueueProvider;
    private readonly ClientDataManager clientDataManager;
    private readonly EmoteProvider emoteProvider;
    private readonly GlamourerAccessor glamourerAccessor;
    private readonly HistoryLogManager historyLogManager;
    private readonly ModSwapManager modSwapManager;
    private readonly WorldProvider worldProvider;

    // Instantiated
    private HubConnection? connection = null;

    /// <summary>
    /// <inheritdoc cref="NetworkProvider"/>
    /// </summary>
    public NetworkProvider(
        ActionQueueProvider actionQueueProvider,
        ClientDataManager clientDataManager,
        EmoteProvider emoteProvider,
        GlamourerAccessor glamourerAccessor,
        HistoryLogManager historyLogManager,
        ModSwapManager modSwapManager,
        WorldProvider worldProvider)
    {
        this.actionQueueProvider = actionQueueProvider;
        this.clientDataManager = clientDataManager;
        this.emoteProvider = emoteProvider;
        this.glamourerAccessor = glamourerAccessor;
        this.historyLogManager = historyLogManager;
        this.modSwapManager = modSwapManager;
        this.worldProvider = worldProvider;
    }

    /// <summary>
    /// Is there a connection to the server
    /// </summary>
    public bool Connected => connection is not null && connection.State == HubConnectionState.Connected;

    /// <summary>
    /// The current state of the server connection
    /// </summary>
    public HubConnectionState State => connection is not null ? connection.State : HubConnectionState.Disconnected;

    /// <summary>
    /// Invokes a method on the server hub
    /// </summary>
    public async Task<U> InvokeCommand<T, U>(string commandName, T request)
    {
        if (connection is null)
        {
            Plugin.Log.Warning($"Cannot invoke commands while server is disconnected");
            return Activator.CreateInstance<U>();
        }

        try
        {
            Plugin.Log.Verbose($"[{commandName}] Request: {request}");
            var response = await connection.InvokeAsync<U>(commandName, request);
            Plugin.Log.Verbose($"[{commandName}] Response: {response}");
            return response;
        }
        catch (Exception ex)
        {
            Plugin.Log.Error($"Exception while invoking command: {ex}");
            return Activator.CreateInstance<U>();
        }
    }

    /// <summary>
    /// Attempt to connect to the server
    /// </summary>
    public async Task Connect(string secret)
    {
        if (Plugin.DeveloperMode)
            return;

        if (connection != null)
            await Disconnect().ConfigureAwait(false);

        var token = await GetToken(secret).ConfigureAwait(false);
        if (token == null) return;

        try
        {
            connection = new HubConnectionBuilder().WithUrl(HubUrl, options =>
            {
                options.AccessTokenProvider = async () =>
                {
                    return await Task.FromResult(token).ConfigureAwait(false);
                };
            }).Build();

            await connection.StartAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Plugin.Log.Warning($"Connection attempt failed: {ex}");
            _ = CleanUp();
            return;
        }

        if (connection.State != HubConnectionState.Connected)
        {
            _ = CleanUp();
            return;
        }

        // Server Events
        connection.Closed += ServerConnectionClosed;

        // Handle Events
        connection.On(Network.Commands.UpdateOnlineStatus, (UpdateOnlineStatusCommand command) => { HandleOnlineStatus(command); });
        connection.On(Network.Commands.UpdateLocalPermissions, (UpdateLocalPermissionsCommand command) => { HandleLocalPermissions(command); });
        connection.On(Network.Commands.Emote, (EmoteCommand command) => { HandleEmote(command); });
        connection.On(Network.Commands.Speak, (SpeakCommand command) => { HandleSpeak(command); });
        connection.On(Network.Commands.Transform, (TransformCommand command) => { _ = HandleTransform(command); });
        connection.On(Network.Commands.BodySwap, (BodySwapCommand command) => { _ = HandleBodySwap(command); });
        connection.On(Network.Commands.Revert, (RevertCommand command) => { HandleRevert(command); });

        // Query Events
        connection.On(Network.BodySwapQuery, async (BodySwapQueryRequest request) => await HandleBodySwapQuery(request));

        // Retrieve user detail
        await RequestUserDetails();
    }

    private async Task<bool> RequestUserDetails()
    {
        var request = new LoginDetailsRequest();
        var response = await InvokeCommand<LoginDetailsRequest, LoginDetailsResponse>(Network.LoginDetails, request);
        if (response.Success == false)
        {
            Plugin.Log.Warning($"Unable to retrieve login details: {response.Message}");
            return false;
        }

        clientDataManager.FriendCode = response.FriendCode;
        clientDataManager.FriendsList.ConvertServerPermissionsToLocal(response.PermissionsGrantedToOthers, response.PermissionsGrantedByOthers);
        return true;
    }

    /// <summary>
    /// Disconnects from the server
    /// </summary>
    public async Task Disconnect() => await CleanUp();
    private Task ServerConnectionClosed(Exception? exception) => CleanUp();

    public async void Dispose()
    {
        await CleanUp();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Cleans up all SignalR resources, and resets local resources
    /// </summary>
    private async Task CleanUp()
    {
        if (connection is not null)
        {
            connection.Closed -= ServerConnectionClosed;
            await connection.DisposeAsync();
            connection = null;
        }

        clientDataManager.FriendCode = null;
        clientDataManager.FriendsList.Clear();
        clientDataManager.TargetManager.Clear();
    }

    private static async Task<string?> GetToken(string secret)
    {
        try
        {
            using var client = new HttpClient();
            var payload = new StringContent(JsonSerializer.Serialize(secret), Encoding.UTF8, "application/json");
            var post = await client.PostAsync(PostUrl, payload).ConfigureAwait(false);
            if (post.IsSuccessStatusCode == false)
            {
                var errorMessage = post.StatusCode switch
                {
                    System.Net.HttpStatusCode.Unauthorized => "Unable to connect, invalid secret. Please register in the discord or reach out for assistance.",
                    _ => $"Post was unsuccessful. Status Code: {post.StatusCode}"
                };

                Plugin.Log.Warning(errorMessage);
                return null;
            }

            return await post.Content.ReadAsStringAsync().ConfigureAwait(false);
        }
        catch (HttpRequestException ex)
        {
            var errorMessage = ex.StatusCode switch
            {
                null => "Unable to connect to server, server is likely offline.",
                _ => ex.Message
            };

            Plugin.Log.Warning(errorMessage);
            return null;
        }
        catch (Exception ex)
        {
            Plugin.Log.Warning($"Token post failed, tell a developer: {ex}");
            return null;
        }
    }

    private void HandleOnlineStatus(UpdateOnlineStatusCommand command)
    {
        Plugin.Log.Verbose(command.ToString());
        clientDataManager.FriendsList.UpdateFriendOnlineStatus(command.FriendCode, command.Online);
        clientDataManager.FriendsList.UpdateLocalPermissions(command.FriendCode, command.Permissions);
    }

    private void HandleLocalPermissions(UpdateLocalPermissionsCommand command)
    {
        Plugin.Log.Verbose($"{command}");
        clientDataManager.FriendsList.UpdateLocalPermissions(command.FriendCode, command.PermissionsGrantedToUser);
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

        if (PermissionChecker.HasValidEmotePermissions(friend.PermissionsGrantedToFriend) == false)
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

        actionQueueProvider.EnqueueEmoteAction(command.SenderFriendCode, command.Emote, command.DisplayLogMessage);
    }

    private void HandleRevert(RevertCommand command)
    {
        Plugin.Log.Verbose(command.ToString());

        var noteOrFriendCode = command.SenderFriendCode;
        var friend = clientDataManager.FriendsList.FindFriend(command.SenderFriendCode);
        if (friend == null)
        {
            var message = HistoryLog.NotFriends("Revert", noteOrFriendCode);
            Plugin.Log.Information(message);
            historyLogManager.LogHistory(message);
            return;
        }

        if (PermissionChecker.HasAnyTransformPermissions(friend.PermissionsGrantedToFriend) == false)
        {
            var message = HistoryLog.LackingPermissions("Revert", noteOrFriendCode);
            Plugin.Log.Information(message);
            historyLogManager.LogHistory(message);
            return;
        }

        var characterName = LocalPlayerName();
        if (characterName == null)
        {
            Plugin.Log.Warning($"{noteOrFriendCode} tried to revert you, but you do not have a body to revert");
            return;
        }

        if (command.RevertType == RevertType.Automation)
            _ = glamourerAccessor.RevertToAutomation(characterName);
        else if (command.RevertType == RevertType.Game)
            _ = glamourerAccessor.RevertToGame(characterName);
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

        if (PermissionChecker.HasValidSpeakPermissions(command.ChatMode, friend.PermissionsGrantedToFriend) == false)
        {
            var message = HistoryLog.LackingPermissions("Speak", noteOrFriendCode);
            Plugin.Log.Information(message);
            historyLogManager.LogHistory(message);
            return;
        }

        if (command.ChatMode == ChatMode.Tell)
        {
            var split = command.Extra?.Split('@');
            if(split == null || split.Length != 2 || worldProvider.IsValidWorld(split[1]) == false)
            {
                var message = HistoryLog.InvalidData("Speak", noteOrFriendCode);
                Plugin.Log.Warning(message);
                return;
            }
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

        if (PermissionChecker.HasValidTransformPermissions(command.ApplyFlags, friend.PermissionsGrantedToFriend) == false)
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
                GlamourerApplyFlag.All => $"{noteOrFriendCode} changed your outfit and appearance",
                _ => $"{noteOrFriendCode} changed you"
            };

            Plugin.Log.Information(message);
            historyLogManager.LogHistoryGlamourer(message, command.GlamourerData);
        }
    }

    private async Task HandleBodySwap(BodySwapCommand command)
    {
        Plugin.Log.Verbose($"{command}");

        var noteOrFriendCode = command.SenderFriendCode;
        var friend = clientDataManager.FriendsList.FindFriend(command.SenderFriendCode);
        if (friend == null)
        {
            var message = HistoryLog.NotFriends("Body Swap", noteOrFriendCode);
            Plugin.Log.Information(message);
            historyLogManager.LogHistory(message);
            return;
        }

        if (PermissionChecker.HasValidTransformPermissions(GlamourerApplyFlag.All, friend.PermissionsGrantedToFriend) == false)
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

        // CharacterName will only be present if we are expecting to swap mods
        if (command.CharacterName is not null)
        {
            if (friend.PermissionsGrantedToFriend.HasFlag(UserPermissions.ModSwap) == false)
            {
                var message = HistoryLog.LackingPermissions("Mod Swap", noteOrFriendCode);
                Plugin.Log.Information(message);
                historyLogManager.LogHistory(message);
                return;
            }

            await modSwapManager.SwapMods(command.CharacterName);
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

        if (PermissionChecker.HasValidTransformPermissions(GlamourerApplyFlag.All, friend.PermissionsGrantedToFriend) == false)
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

        return new BodySwapQueryResponse(request.SwapMods ? characterName : null, characterData);
    }

    /// <summary>
    /// Attempts to retrieve the name of the local player instance. This will be null is the player is on the main menu or loading to a new zone
    /// </summary>
    private static string? LocalPlayerName() => Plugin.ClientState.LocalPlayer?.Name.ToString();
}
