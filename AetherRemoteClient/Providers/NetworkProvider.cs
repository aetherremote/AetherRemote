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
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

#pragma warning disable CS0162

namespace AetherRemoteClient.Providers;

public class NetworkProvider : IDisposable
{
#if DEBUG
    // Const
    private const string HubUrl = "https://localhost:5006/primaryHub";
    private const string PostUrl = "https://localhost:5006/api/auth/login";
#else
    // Const
    private const string HubUrl = "https://foxitsvc.com:5006/primaryHub";
    private const string PostUrl = "https://foxitsvc.com:5006/api/auth/login";
#endif


    // Inject
    private readonly ActionQueueProvider actionQueueProvider;
    private readonly ClientDataManager clientDataManager;
    private readonly EmoteProvider emoteProvider;
    private readonly GlamourerAccessor glamourerAccessor;
    private readonly HistoryLogManager historyLogManager;

    // Instantiated
    private NetworkHandler? networkHandler = null;
    private HubConnection? connection = null;

    public NetworkProvider(
        ActionQueueProvider actionQueueProvider,
        ClientDataManager clientDataManager,
        EmoteProvider emoteProvider,
        GlamourerAccessor glamourerAccessor,
        HistoryLogManager historyLogManager)
    {
        this.actionQueueProvider = actionQueueProvider;
        this.clientDataManager = clientDataManager;
        this.emoteProvider = emoteProvider;
        this.glamourerAccessor = glamourerAccessor;
        this.historyLogManager = historyLogManager;

        if (Plugin.DeveloperMode)
        {
            clientDataManager.FriendCode = "Dev Mode";
            clientDataManager.FriendsList.CreateOrUpdateFriend("Friend1", false);
            clientDataManager.FriendsList.CreateOrUpdateFriend("Friend2", true);
            clientDataManager.FriendsList.CreateOrUpdateFriend("Friend3", true);
            clientDataManager.FriendsList.CreateOrUpdateFriend("Friend4", false);
            clientDataManager.FriendsList.CreateOrUpdateFriend("Friend5", false);
        }
    }

    // TODO: Cancellation token across requests
    public async Task Connect(string secret)
    {
        if (Plugin.DeveloperMode)
            return;

        if (connection != null)
            await Disconnect().ConfigureAwait(false);

        var token = string.Empty;
        try
        {
            using var client = new HttpClient();
            var payload = new StringContent(JsonSerializer.Serialize(secret), Encoding.UTF8, "application/json");
            var post = await client.PostAsync(PostUrl, payload).ConfigureAwait(false);
            if (post.IsSuccessStatusCode == false)
                return;

            token = await post.Content.ReadAsStringAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Plugin.Log.Warning($"Login post failed: {ex}");
            return;
        }

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
            return;
        }

        if (connection.State == HubConnectionState.Connected)
        {
            networkHandler = new(actionQueueProvider, clientDataManager, emoteProvider, glamourerAccessor, historyLogManager, connection);
            connection.Closed += ServerConnectionClosed;

            // TODO: Retry strategy
            if (await GetAndSetLoginDetails().ConfigureAwait(false) == false)
            {
               
            }
        }
    }

    public async Task<bool> GetAndSetLoginDetails()
    {
        if (Plugin.DeveloperMode)
            return true;

        var request = new LoginDetailsRequest();
        var response = await InvokeCommand<LoginDetailsRequest, LoginDetailsResponse>(Network.LoginDetails, request);
        if (response.Success == false)
        {
            Plugin.Log.Warning($"Unable to retrieve login details: {response.Message}");
            return false;
        }

        clientDataManager.FriendCode = response.FriendCode;
        clientDataManager.FriendsList.ConvertServerPermissionsToLocal(response.Permissions, response.Online);
        return true;
    }

    public async Task<(bool, bool)> CreateOrUpdateFriend(string friendCode, UserPermissions permissions = UserPermissions.None)
    {
        if (Plugin.DeveloperMode)
            return (true, true);

        var request = new CreateOrUpdatePermissionsRequest(friendCode, permissions);
        var response = await InvokeCommand<CreateOrUpdatePermissionsRequest, CreateOrUpdatePermissionsResponse>(Network.Permissions.CreateOrUpdate, request);
        if (response.Success == false)
            Plugin.Log.Warning($"Unable to add friend {friendCode}. {response.Message}");

        return (response.Success, response.Online);
    }

    public async Task<bool> DeleteFriend(Friend friend)
    {
        if (Plugin.DeveloperMode)
            return true;

        var request = new DeletePermissionsRequest(friend.FriendCode);
        var response = await InvokeCommand<DeletePermissionsRequest, DeletePermissionsResponse>(Network.Permissions.Delete, request);
        if (response.Success == false)
            Plugin.Log.Warning($"Unable to delete friend {friend.FriendCode}. {response.Message}");

        return response.Success;
    }

    public async Task<bool> IssueEmoteCommand(List<string> targets, string emote)
    {
        if (Plugin.DeveloperMode)
            return true;

        var request = new EmoteRequest(targets, emote);
        var result = await InvokeCommand<EmoteRequest, EmoteResponse>(Network.Commands.Emote, request);
        if (result.Success == false)
            Plugin.Log.Warning($"Issuing emote command unsuccessful: {result.Message}");

        return result.Success;
    }

    public async Task<bool> IssueTransformCommand(List<string> targets, string glamourerData, GlamourerApplyFlag applyType)
    {
        if (Plugin.DeveloperMode)
            return true;

        var request = new TransformRequest(targets, glamourerData, applyType);
        var result = await InvokeCommand<TransformRequest, TransformResponse>(Network.Commands.Transform, request);
        if (result.Success == false)
            Plugin.Log.Warning($"Issuing transform command unsuccessful: {result.Message}");

        return result.Success;
    }

    public async Task<bool> IssueSpeakCommand(List<string> targets, string message, ChatMode chatMode, string? extra)
    {
        if (Plugin.DeveloperMode)
            return true;

        var request = new SpeakRequest(targets, message, chatMode, extra);
        var result = await InvokeCommand<SpeakRequest, SpeakResponse>(Network.Commands.Speak, request);
        if (result.Success == false)
            Plugin.Log.Warning($"Issuing speak command unsuccessful: {result.Message}");

        return result.Success;
    }

    public async Task<(bool, string?)> IssueBodySwapCommand(List<string> targets, string characterData)
    {
        if (Plugin.DeveloperMode)
            return (true, null);

        var request = new BodySwapRequest(targets, characterData);
        var result = await InvokeCommand<BodySwapRequest, BodySwapResponse>(Network.Commands.BodySwap, request);
        if (result.Success == false)
            Plugin.Log.Warning($"Issuing body swap command unsuccessful: {result.Message}");

        return (result.Success, result.CharacterData);
    }

    private async Task<U> InvokeCommand<T, U>(string commandName, T request)
    {
        if (connection == null)
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

    public async Task Disconnect()
    {
        if (Plugin.DeveloperMode == false)
        {
            networkHandler = null;
            if (connection != null)
            {
                connection.Closed -= ServerConnectionClosed;
                await connection.StopAsync().ConfigureAwait(false);
                await connection.DisposeAsync().ConfigureAwait(false);
            }
        }

        clientDataManager.FriendCode = null;
        clientDataManager.FriendsList.Friends.Clear();
        clientDataManager.TargetManager.Clear();
    }

    private Task ServerConnectionClosed(Exception? exception)
    {
        Plugin.Log.Information("Server connection closed, what should we do?");
        return Task.CompletedTask;
    }

    /// <summary>
    /// Is there a connection to the server
    /// </summary>
    public bool Connected => IsConnected();
    private bool IsConnected()
    {
        if (connection == null)
            return false;

        return connection.State == HubConnectionState.Connected;
    }

    /// <summary>
    /// The current state of the server connection
    /// </summary>
    public HubConnectionState State => GetServerState();
    private HubConnectionState GetServerState()
    {
        if (connection == null)
            return HubConnectionState.Disconnected;

        return connection.State;
    }

    public async void Dispose()
    {
        if (connection != null)
        {
            connection.Closed -= ServerConnectionClosed;
            await connection.StopAsync().ConfigureAwait(false);
            await connection.DisposeAsync().ConfigureAwait(false);
        }

        GC.SuppressFinalize(this);
    }
}
