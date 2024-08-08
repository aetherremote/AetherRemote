using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Logger;
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
    private readonly AetherRemoteLogger logger;
    private readonly ClientDataManager clientDataManager;

    // Endpoint
    private const string ConnectionUrl = "http://75.72.17.196:25565/mainHub";

    // Network
    public readonly HubConnection Connection = new HubConnectionBuilder().WithUrl(ConnectionUrl).Build();
    public bool Connected => Connection.State == HubConnectionState.Connected;

    public NetworkProvider(AetherRemoteLogger logger, ClientDataManager clientDataManager)
    {
        this.logger = logger;
        this.clientDataManager = clientDataManager;

        if (Plugin.DeveloperMode)
        {
            clientDataManager.FriendCode = "Dev Mode";
            clientDataManager.FriendList.Add("Friend1", false);
            clientDataManager.FriendList.Add("Friend2", true);
            clientDataManager.FriendList.Add("Friend3", true);
            clientDataManager.FriendList.Add("Friend4", false);
            clientDataManager.FriendList.Add("Friend5", false);
        }
    }

    #region === Connect ===
    public async Task<bool> Connect(string secret)
    {
        if (Plugin.DeveloperMode)
            return true;

        if (Connection.State != HubConnectionState.Disconnected)
            return false;

        if (await ConnectToServer() == false) return false;
        if (await LoginToServer(secret) == false)
        {
            Disconnect();
            return false;
        }

        return true;
    }

    private async Task<bool> ConnectToServer()
    {
        try
        {
            await Task.Run(() => Connection.StartAsync());

            if (Connection.State == HubConnectionState.Connected)
                return true;
        }
        catch (HttpRequestException) { logger.Warning($"Unable to connect to the server. Is it down?"); }
        catch (Exception ex) { logger.Warning($"Unable to connect to the server. {ex.Message}"); }

        return false;
    }

    private async Task<bool> LoginToServer(string secret)
    {
        try
        {
            var request = new LoginRequest(secret);
            var response = await InvokeCommand<LoginRequest, LoginResponse>(Constants.ApiLogin, request);
            if (response.Success)
            {
                clientDataManager.FriendCode = response.FriendCode;
                clientDataManager.FriendList.Friends = response.FriendList;
            }

            return response.Success;
        }
        catch (Exception ex)
        {
            logger.Warning($"Caught exception while attempting to log into the server: {ex.Message}");
            return false;
        }
    }

    public async void Disconnect()
    {
        if (Plugin.DeveloperMode == false)
            await Connection.StopAsync();

        clientDataManager.FriendCode = null;
        clientDataManager.FriendList.Friends = [];
    }
    #endregion

    #region === Friend List ===
    public async Task<(bool, bool)> CreateOrUpdateFriend(string secret, string friendCode)
    {
        var friend = new Friend(friendCode);
        return await CreateOrUpdateFriend(secret, friend);
    }

    public async Task<(bool, bool)> CreateOrUpdateFriend(string secret, Friend friend)
    {
        if (Plugin.DeveloperMode)
            return (true, true);

        var request = new CreateOrUpdateFriendRequest(secret, friend);
        var response = await InvokeCommand<CreateOrUpdateFriendRequest, CreateOrUpdateFriendResponse>(Constants.ApiCreateOrUpdateFriend, request);
        if (response.Success == false)
            logger.Warning($"Unable to add friend {friend.FriendCode}. {response.Message}");

        return (response.Success, response.Online);
    }

    public async Task<bool> DeleteFriend(string secret, string friendCode)
    {
        if (Plugin.DeveloperMode)
            return true;

        var request = new DeleteFriendRequest(secret, friendCode);
        var response = await InvokeCommand<DeleteFriendRequest, DeleteFriendResponse>(Constants.ApiDeleteFriend, request);
        if (response.Success == false)
            logger.Warning($"Unable to delete friend {friendCode}. {response.Message}");

        return response.Success;
    }
    #endregion

    #region === Commands ===
    public async Task<bool> IssueBecomeCommand(string secret, List<Friend> targets, string glamourerData, GlamourerApplyType glamourerApplyType)
    {
        if (Plugin.DeveloperMode)
            return true;

        var targetFriendCodes = targets.Select(friend => friend.FriendCode).ToList();
        var request = new BecomeRequest(secret, targetFriendCodes, glamourerData, glamourerApplyType);
        var response = await InvokeCommand<BecomeRequest, BecomeResponse>(Constants.ApiBecome, request);
        if (response.Success == false)
            logger.Warning($"Unable to issue become command. {response.Message}");

        return response.Success;
    }

    public async Task<bool> IssueEmoteCommand(string secret, List<Friend> targets, string emote)
    {
        if (Plugin.DeveloperMode)
            return true;

        var targetFriendCodes = targets.Select(friend => friend.FriendCode).ToList();
        var request = new EmoteRequest(secret, targetFriendCodes, emote);
        var response = await InvokeCommand<EmoteRequest, EmoteResponse>(Constants.ApiEmote, request);
        if (response.Success == false)
            logger.Warning($"Unable to issue emote command. {response.Message}");

        return response.Success;
    }   

    public async Task<bool> IssueSpeakCommand(string secret, List<Friend> targets, string message, ChatMode chatMode, string? extra)
    {
        if (Plugin.DeveloperMode)
            return true;

        var targetFriendCodes = targets.Select(friend => friend.FriendCode).ToList();
        var request = new SpeakRequest(secret, targetFriendCodes, message, chatMode, extra);
        var response = await InvokeCommand<SpeakRequest, SpeakResponse>(Constants.ApiSpeak, request);
        if (response.Success == false)
            logger.Warning($"Unable to issue speak command. {response.Message}");

        return response.Success;
    }
    #endregion

    private async Task<U> InvokeCommand<T, U>(string commandName, T request)
    {
        logger.Information($"[{commandName}] Request: {request}");
        var response = await Connection.InvokeAsync<U>(commandName, request);
        logger.Information($"[{commandName}] Response: {response}");
        return response;
    }

    public async void Dispose()
    {
        GC.SuppressFinalize(this);
        await Connection.DisposeAsync();
    }
}
