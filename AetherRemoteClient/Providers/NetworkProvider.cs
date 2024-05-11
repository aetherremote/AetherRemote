using AetherRemoteClient.Domain;
using AetherRemoteCommon;
using AetherRemoteCommon.Domain.CommonChatMode;
using AetherRemoteCommon.Domain.CommonFriend;
using AetherRemoteCommon.Domain.CommonGlamourerApplyType;
using AetherRemoteCommon.Domain.Network.Become;
using AetherRemoteCommon.Domain.Network.CreateOrUpdateFriend;
using AetherRemoteCommon.Domain.Network.DeleteFriend;
using AetherRemoteCommon.Domain.Network.Emote;
using AetherRemoteCommon.Domain.Network.Login;
using AetherRemoteCommon.Domain.Network.Speak;
using Dalamud.Plugin.Services;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace AetherRemoteClient.Providers;

public class NetworkProvider : IDisposable
{
    // Inject
    private readonly IPluginLog logger;

    // Endpoint
    private const string ConnectionUrl = "http://75.73.3.71:25565/mainHub";

    // Network
    public readonly HubConnection Connection = new HubConnectionBuilder().WithUrl(ConnectionUrl).Build();

    // State
    public ServerConnectionState ConnectionState = ServerConnectionState.Disconnected;

    // Data
    public string? FriendCode { get; private set; } = null;
    public FriendList? FriendList { get; private set; } = null;

    public NetworkProvider(IPluginLog logger)
    {
        this.logger = logger;
        Connection.Closed += Closed;
        Connection.Reconnecting += Reconnecting;
        Connection.Reconnected += Reconnected;

        if (Plugin.DeveloperMode)
        {
            FriendCode = "DevMode";

            var newGuy = new Friend("OnlineFriend8");
            newGuy.Online = true;

            FriendList = new FriendList([
                new("OnlineFriend1") { Online = true },
                new("OnlineFriend2") { Online = false },
                new("OnlineFriend3") { Online = false },
                new("OnlineFriend4") { Online = true },
                new("OnlineFriend5") { Online = false },
                new("OnlineFriend6") { Online = true },
                new("OnlineFriend7") { Online = true }]);
        }
    }

    #region === Connect ===
    public async Task<AsyncResult> Connect(string secret)
    {
        if (Plugin.DeveloperMode)
            return new AsyncResult(true, "DeveloperMode Enabled");

        if (Connection.State != HubConnectionState.Disconnected)
            return new AsyncResult(false, "Pending connection in progress");

        ConnectionState = ServerConnectionState.Connecting;

        var connectionResult = await ConnectToServer();
        if (connectionResult.Success == false)
        {
            ConnectionState = ServerConnectionState.Disconnected;
            return connectionResult;
        }

        var loginResult = await LoginToServer(secret);
        if (loginResult.Success == false)
        {
            ConnectionState = ServerConnectionState.Disconnected;
            await Task.Run(() => Connection.StopAsync());
            return loginResult;
        }

        ConnectionState = ServerConnectionState.Connected;
        return new AsyncResult(true);
    }

    private async Task<AsyncResult> ConnectToServer()
    {
        try
        {
            await Task.Run(() => Connection.StartAsync());

            if (Connection.State == HubConnectionState.Connected)
                return new AsyncResult(true);
        }
        catch (HttpRequestException) { /* Server likely down */ }
        catch (Exception) { /* Something else */ }

        return new AsyncResult(false, "Failed to connect to server");
    }

    private async Task<AsyncResult> LoginToServer(string secret)
    {
        if (Plugin.DeveloperMode)
        {
            FriendCode = "DevMode";
            return new AsyncResult(true, "DeveloperMode Enabled");
        }

        try
        {
            var request = new LoginRequest(secret);
            var response = await InvokeCommand<LoginRequest, LoginResponse>(Constants.ApiLogin, request);
            if (response.Success)
            {
                FriendCode = response.FriendCode;
                FriendList = new(response.FriendList);
            }

            return new AsyncResult(response.Success, response.Message);
        }
        catch (Exception ex)
        {
            return new AsyncResult(false, ex.Message);
        }
    }

    public async void Disconnect()
    {
        if (Plugin.DeveloperMode == false)
            await Connection.StopAsync();

        ConnectionState = ServerConnectionState.Disconnected;
        FriendCode = null;
    }
    #endregion

    #region === Friend List ===
    public async Task<ResultWithOnlineStatus> CreateOrUpdateFriend(string secret, string friendCode)
    {
        var friend = new Friend(friendCode);
        return await CreateOrUpdateFriend(secret, friend);
    }

    public async Task<ResultWithOnlineStatus> CreateOrUpdateFriend(string secret, Friend friend)
    {
        if (Plugin.DeveloperMode)
            return new ResultWithOnlineStatus(true, "DeveloperMode Enabled");

        var request = new CreateOrUpdateFriendRequest(secret, friend);
        var response = await InvokeCommand<CreateOrUpdateFriendRequest, CreateOrUpdateFriendResponse>(Constants.ApiCreateOrUpdateFriend, request);
        return new ResultWithOnlineStatus(response.Success, response.Message);
    }

    public async Task<AsyncResult> DeleteFriend(string secret, string friendCode)
    {
        if (Plugin.DeveloperMode)
            return new AsyncResult(true, "DeveloperMode Enabled");

        var request = new DeleteFriendRequest(secret, friendCode);
        var response = await InvokeCommand<DeleteFriendRequest, DeleteFriendResponse>(Constants.ApiDeleteFriend, request);
        return new AsyncResult(response.Success, response.Message);
    }
    #endregion

    #region === Commands ===
    public async Task<AsyncResult> Become(string secret, List<Friend> targets, string glamourerData, GlamourerApplyType glamourerApplyType)
    {
        if (Plugin.DeveloperMode)
            return new AsyncResult(true, "DeveloperMode Enabled");

        var targetFriendCodes = targets.Select(friend => friend.FriendCode).ToList();
        var request = new BecomeRequest(secret, targetFriendCodes, glamourerData, glamourerApplyType);
        var response = await InvokeCommand<BecomeRequest, BecomeResponse>(Constants.ApiBecome, request);
        return new AsyncResult(response.Success, response.Message);
    }

    public async Task<AsyncResult> Emote(string secret, List<Friend> targets, string emote)
    {
        if (Plugin.DeveloperMode)
            return new AsyncResult(true, "DeveloperMode Enabled");

        var targetFriendCodes = targets.Select(friend => friend.FriendCode).ToList();
        var request = new EmoteRequest(secret, targetFriendCodes, emote);
        var response = await InvokeCommand<EmoteRequest, EmoteResponse>(Constants.ApiEmote, request);
        return new AsyncResult(response.Success, response.Message);
    }   

    public async Task<AsyncResult> Speak(string secret, List<Friend> targets, string message, ChatMode chatMode, string? extra)
    {
        if (Plugin.DeveloperMode)
            return new AsyncResult(true, "DeveloperMode Enabled");

        var targetFriendCodes = targets.Select(friend => friend.FriendCode).ToList();
        var request = new SpeakRequest(secret, targetFriendCodes, message, chatMode, extra);
        var response = await InvokeCommand<SpeakRequest, SpeakResponse>(Constants.ApiSpeak, request);
        return new AsyncResult(response.Success, response.Message);
    }
    #endregion

    private async Task<U> InvokeCommand<T, U>(string commandName, T request)
    {
        logger.Info($"[{commandName}] Request: {request}");
        var response = await Connection.InvokeAsync<U>(commandName, request);
        logger.Info($"[{commandName}] Response: {response}");
        return response;
    }

    public async void Dispose()
    {
        GC.SuppressFinalize(this);
        Connection.Reconnecting -= Reconnecting;
        Connection.Reconnected -= Reconnected;
        await Connection.DisposeAsync();
    }

    private async Task Closed(Exception? exception)
    {
        await Task.Run(() => { ConnectionState = ServerConnectionState.Disconnected; });
    }

    private async Task Reconnecting(Exception? exception)
    {
        await Task.Run(() => { ConnectionState = ServerConnectionState.Reconnecting; });
    }

    private async Task Reconnected(string? arg)
    {
        await Task.Run(() => { ConnectionState = ServerConnectionState.Connected; });
    }
}
