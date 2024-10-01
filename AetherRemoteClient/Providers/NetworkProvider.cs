using AetherRemoteClient.Accessors.Glamourer;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Log;
using AetherRemoteCommon;
using AetherRemoteCommon.Domain.Network;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

#pragma warning disable CS0162

namespace AetherRemoteClient.Providers;

public class NetworkProvider : IDisposable
{
    #if DEBUG
    private const string HubUrl = "https://localhost:5006/primaryHub";
    private const string PostUrl = "https://localhost:5006/api/auth/login";
    #else
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

    /// <summary>
    /// Is there a connection to the server
    /// </summary>
    public bool Connected => connection != null && connection.State == HubConnectionState.Connected;

    /// <summary>
    /// The current state of the server connection
    /// </summary>
    public HubConnectionState State => connection == null ? HubConnectionState.Disconnected : connection.State;

    /// <summary>
    /// Invokes a method on the server hub
    /// </summary>
    public async Task<U> InvokeCommand<T, U>(string commandName, T request)
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

    // TODO: Cancellation token across requests
    public async Task Connect(string secret)
    {
        if (Plugin.DeveloperMode)
            return;

        if (connection != null)
            await Disconnect().ConfigureAwait(false);

        var token = await GetToken(secret).ConfigureAwait(false);

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

    private static async Task<string> GetToken(string secret)
    {
        try
        {
            using var client = new HttpClient();
            var payload = new StringContent(JsonSerializer.Serialize(secret), Encoding.UTF8, "application/json");
            var post = await client.PostAsync(PostUrl, payload).ConfigureAwait(false);
            if (post.IsSuccessStatusCode == false)
                return string.Empty;

            return await post.Content.ReadAsStringAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Plugin.Log.Warning($"Token post failed: {ex}");
            return string.Empty;
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
