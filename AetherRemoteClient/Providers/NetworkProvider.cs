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

namespace AetherRemoteClient.Providers;

/// <summary>
/// Provisions a connection to the server and exposes methods to interact with the server
/// </summary>
public class NetworkProvider(ActionQueueProvider actionQueueProvider,
    ClientDataManager clientDataManager,
    EmoteProvider emoteProvider,
    GlamourerAccessor glamourerAccessor,
    HistoryLogManager historyLogManager,
    ModSwapManager modSwapManager,
    WorldProvider worldProvider) : IDisposable
{
#if DEBUG
    private const string HubUrl = "https://foxitsvc.com:5006/primaryHub";
    private const string PostUrl = "https://foxitsvc.com:5006/api/auth/login";
#else
    private const string HubUrl = "https://foxitsvc.com:5006/primaryHub";
    private const string PostUrl = "https://foxitsvc.com:5006/api/auth/login";
#endif

    // Const
    private const GlamourerApplyFlag CustomizationFlags = GlamourerApplyFlag.Once | GlamourerApplyFlag.Customization;
    private const GlamourerApplyFlag EquipmentFlags = GlamourerApplyFlag.Once | GlamourerApplyFlag.Equipment;

    // Instantiated
    private HubConnection? _connection;

    /// <summary>
    /// Is there a connection to the server
    /// </summary>
    public bool Connected => _connection?.State is HubConnectionState.Connected;

    /// <summary>
    /// The current state of the server connection
    /// </summary>
    public HubConnectionState State => _connection?.State ?? HubConnectionState.Disconnected;

    /// <summary>
    /// Invokes a method on the server hub
    /// </summary>
    public async Task<TU> InvokeCommand<T, TU>(string commandName, T request)
    {
        if (_connection is null)
        {
            Plugin.Log.Warning($"Cannot invoke commands while server is disconnected");
            return Activator.CreateInstance<TU>();
        }

        try
        {
            Plugin.Log.Verbose($"[{commandName}] Request: {request}");
            var response = await _connection.InvokeAsync<TU>(commandName, request);
            Plugin.Log.Verbose($"[{commandName}] Response: {response}");
            return response;
        }
        catch (Exception ex)
        {
            Plugin.Log.Error($"Exception while invoking command: {ex}");
            return Activator.CreateInstance<TU>();
        }
    }

    /// <summary>
    /// Attempt to connect to the server
    /// </summary>
    public async Task Connect(string secret)
    {
        if (Plugin.DeveloperMode)
            return;

        if (_connection is not null)
            await Disconnect().ConfigureAwait(false);

        var token = await GetToken(secret).ConfigureAwait(false);
        if (token is null) return;

        try
        {
            _connection = new HubConnectionBuilder().WithUrl(HubUrl, options =>
            {
                options.AccessTokenProvider = async () => await Task.FromResult(token).ConfigureAwait(false);
            }).Build();

            await _connection.StartAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Plugin.Log.Warning($"Connection attempt failed: {ex}");
            _ = CleanUp();
            return;
        }

        if (_connection.State is not HubConnectionState.Connected)
        {
            _ = CleanUp();
            return;
        }

        // Server Events
        _connection.Closed += ServerConnectionClosed;

        // Handle Events
        _connection.On(Network.Commands.UpdateOnlineStatus,         (UpdateOnlineStatusCommand command) => { HandleOnlineStatus(command); });
        _connection.On(Network.Commands.UpdateLocalPermissions,     (UpdateLocalPermissionsCommand command) => { HandleLocalPermissions(command); });
        _connection.On(Network.Commands.Emote,                      (EmoteCommand command) => { HandleEmote(command); });
        _connection.On(Network.Commands.Speak,                      (SpeakCommand command) => { HandleSpeak(command); });
        _connection.On(Network.Commands.Transform,                  (TransformCommand command) => { _ = HandleTransform(command); });
        _connection.On(Network.Commands.BodySwap,                   (BodySwapCommand command) => { _ = HandleBodySwap(command); });
        _connection.On(Network.Commands.Revert,                     (RevertCommand command) => { HandleRevert(command); });

        // Query Events
        _connection.On(Network.BodySwapQuery,                       async (BodySwapQueryRequest request) => await HandleBodySwapQuery(request));

        // Retrieve user detail
        await RequestUserDetails();
    }

    private async Task RequestUserDetails()
    {
        var request = new LoginDetailsRequest();
        var response = await InvokeCommand<LoginDetailsRequest, LoginDetailsResponse>(Network.LoginDetails, request);
        if (response.Success == false)
        {
            Plugin.Log.Warning($"Unable to retrieve login details: {response.Message}");
            return;
        }

        clientDataManager.FriendCode = response.FriendCode;
        clientDataManager.FriendsList.ConvertServerPermissionsToLocal(response.PermissionsGrantedToOthers, response.PermissionsGrantedByOthers);
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
        if (_connection is not null)
        {
            _connection.Closed -= ServerConnectionClosed;
            await _connection.DisposeAsync();
            _connection = null;
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
            if (post.IsSuccessStatusCode) return await post.Content.ReadAsStringAsync().ConfigureAwait(false);
            
            var errorMessage = post.StatusCode switch
            {
                System.Net.HttpStatusCode.Unauthorized => "Unable to connect, invalid secret. Please register in the discord or reach out for assistance.",
                _ => $"Post was unsuccessful. Status Code: {post.StatusCode}"
            };

            Plugin.Log.Warning(errorMessage);
            return null;
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
        Plugin.Log.Verbose($"{command}");
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
        Plugin.Log.Verbose($"{command}");

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
        Plugin.Log.Verbose($"{command}");

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

        if (Plugin.ClientState.LocalPlayer is null)
        {
            Plugin.Log.Warning($"{noteOrFriendCode} tried to revert you, but you do not have a body to revert");
            return;
        }

        switch (command.RevertType)
        {
            case RevertType.Automation:
                _ = glamourerAccessor.RevertToAutomation();
                break;
            case RevertType.Game:
                _ = glamourerAccessor.RevertToGame();
                break;
            case RevertType.None:
            default:
                break;    
        }
    }

    private void HandleSpeak(SpeakCommand command)
    {
        Plugin.Log.Verbose($"{command}");

        var noteOrFriendCode = command.SenderFriendCode;
        var friend = clientDataManager.FriendsList.FindFriend(command.SenderFriendCode);
        if (friend is null)
        {
            var message = HistoryLog.NotFriends("Speak", noteOrFriendCode);
            Plugin.Log.Information(message);
            historyLogManager.LogHistory(message);
            return;
        }
        
        var linkshellResult = 0;
        if (command.ChatMode is ChatMode.Linkshell or ChatMode.CrossworldLinkshell)
            linkshellResult = int.TryParse(command.Extra ?? string.Empty, out var linkshellNumber) ? linkshellNumber : linkshellResult;
        
        if (PermissionChecker.HasValidSpeakPermissions(command.ChatMode, friend.PermissionsGrantedToFriend, linkshellResult) == false)
        {
            var message = HistoryLog.LackingPermissions("Speak", noteOrFriendCode);
            Plugin.Log.Information(message);
            historyLogManager.LogHistory(message);
            return;
        }

        if (command.ChatMode is ChatMode.Tell)
        {
            var split = command.Extra?.Split('@');
            if(split is not { Length: 2 } || worldProvider.IsValidWorld(split[1]) == false)
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
        Plugin.Log.Verbose($"{command}");

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

        if (Plugin.ClientState.LocalPlayer is null)
        {
            Plugin.Log.Warning($"{noteOrFriendCode} tried to transform you, but you do not have a body to transform");
            return;
        }

        var result = await glamourerAccessor.ApplyDesignAsync(command.GlamourerData, 0, command.ApplyFlags).ConfigureAwait(false);
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

        if (Plugin.ClientState.LocalPlayer is null)
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

        var result = await glamourerAccessor.ApplyDesignAsync(command.CharacterData);
        if (result)
        {
            var message = $"{noteOrFriendCode} swapped your body with {command.SenderFriendCode}'s body";
            Plugin.Log.Information(message);
            historyLogManager.LogHistoryGlamourer(message, command.CharacterData);
        }
    }

    private async Task<BodySwapQueryResponse> HandleBodySwapQuery(BodySwapQueryRequest request)
    {
        Plugin.Log.Verbose($"{request}");

        // TODO: Hook this into configuration
        var noteOrFriendCode = request.SenderFriendCode;
        var friend = clientDataManager.FriendsList.FindFriend(request.SenderFriendCode);
        if (friend is null)
        {
            var message = HistoryLog.NotFriends("Body Swap Query", noteOrFriendCode);
            Plugin.Log.Information(message);
            historyLogManager.LogHistory(message);
            return new BodySwapQueryResponse();
        }

        if (PermissionChecker.HasValidTransformPermissions(GlamourerApplyFlag.All, friend.PermissionsGrantedToFriend) == false)
        {
            var message = HistoryLog.LackingPermissions("Body Swap Query", noteOrFriendCode);
            Plugin.Log.Information(message);
            historyLogManager.LogHistory(message);
            return new BodySwapQueryResponse();
        }

        var characterData = await glamourerAccessor.GetDesignAsync();
        if (characterData is null)
        {
            Plugin.Log.Warning($"{noteOrFriendCode} attempted to scan your body for a swap, but the scan failed");
            return new BodySwapQueryResponse();
        }
        
        if (request.SwapMods == false) return new BodySwapQueryResponse(null, characterData);
        
        var characterName = Plugin.ClientState.LocalPlayer?.Name.ToString();
        if (characterName is not null) return new BodySwapQueryResponse(characterName, characterData);
        
        Plugin.Log.Warning($"{noteOrFriendCode} attempted to scan your body for a swap, but you don't have a body to scan");
        return new BodySwapQueryResponse();
    }
}
