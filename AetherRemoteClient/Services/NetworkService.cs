using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AetherRemoteClient.Utils;
using AetherRemoteCommon.Domain.Network;
using MessagePack;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

namespace AetherRemoteClient.Services;

/// <summary>
///     Provides methods to interact with the server
/// </summary>
public class NetworkService : IDisposable
{
    /// <summary>
    ///     The Signal R connection
    /// </summary>
    public readonly HubConnection Connection;

    /// <summary>
    ///     Event fired when the server successfully connects, either by reconnection or manual connection
    /// </summary>
    public event Func<Task>? Connected;

    /// <summary>
    ///     Event fired when the server connection is lost, either by disruption or manual intervention
    /// </summary>
    public event Func<Task>? Disconnected;
    
#if DEBUG
    // Local
    // private const string HubUrl = "https://localhost:5006/primaryHub";
    // private const string PostUrl = "https://localhost:5006/api/auth/login";

    // Beta
    private const string HubUrl = "https://foxitsvc.com:5006/primaryHub";
    private const string PostUrl = "https://foxitsvc.com:5006/api/auth/login";
#else
    // Prod
    private const string HubUrl = "https://foxitsvc.com:5006/primaryHub";
    private const string PostUrl = "https://foxitsvc.com:5006/api/auth/login";
#endif

    /// <summary>
    ///     <inheritdoc cref="NetworkService"/>
    /// </summary>
    public NetworkService()
    {
        Connection = new HubConnectionBuilder().WithUrl(HubUrl,
                options => { options.AccessTokenProvider = async () => await FetchToken().ConfigureAwait(false); })
            .WithAutomaticReconnect()
            .AddMessagePackProtocol(options =>
            {
                options.SerializerOptions = MessagePackSerializerOptions.Standard
                    .WithSecurity(MessagePackSecurity.UntrustedData);
            })
            .Build();

        Connection.Reconnected += OnReconnected;
        Connection.Reconnecting += OnReconnecting;
        Connection.Closed += OnClosed;
    }

    /// <summary>
    ///     Begins a connection to the server
    /// </summary>
    public async Task StartAsync()
    {
        if (Connection.State is not HubConnectionState.Disconnected)
        {
            Plugin.Log.Verbose("[NetworkService] Network connection is pending or already established");
            return;
        }

        try
        {
            await Connection.StartAsync().ConfigureAwait(false);

            if (Connection.State is HubConnectionState.Connected)
            {
                Connected?.Invoke();
                var notification = NotificationHelper.Success("[Aether Remote] Connected", string.Empty);
                Plugin.NotificationManager.AddNotification(notification);
            }
            else
            {
                var notification = NotificationHelper.Warning("[Aether Remote] Unable to connect",
                    "See developer console for more information");
                Plugin.NotificationManager.AddNotification(notification);
            }
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"[NetworkService] [StartAsync] {e.Message}]");
            var notification = NotificationHelper.Warning("[Aether Remote] Could not connect",
                "See developer console for more information");
            Plugin.NotificationManager.AddNotification(notification);
        }
    }

    /// <summary>
    ///     Ends a connection to the server
    /// </summary>
    public async Task StopAsync()
    {
        if (Connection.State is HubConnectionState.Disconnected)
        {
            Plugin.Log.Verbose("[NetworkService] Network connection is already disconnected");
            return;
        }

        try
        {
            await Connection.StopAsync().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"[NetworkService] [StopAsync] Error, {e.Message}]");
        }
    }

    /// <summary>
    ///     Invokes a method on the server and awaits a result
    /// </summary>
    /// <param name="method">The name of the method to call</param>
    /// <param name="request">The request object to send</param>
    /// <typeparam name="T">Request object (Example <see cref="AddFriendRequest" />)</typeparam>
    /// <typeparam name="TU">Response object (Example <see cref="AddFriendResponse" />)</typeparam>
    /// <returns></returns>
    public async Task<TU> InvokeAsync<T, TU>(string method, T request)
    {
        if (Connection.State is not HubConnectionState.Connected)
        {
            Plugin.Log.Warning("[NetworkService] No connection established");
            return Activator.CreateInstance<TU>();
        }

        try
        {
            Plugin.Log.Verbose($"[NetworkService] Request: {request}");
            var response = await Connection.InvokeAsync<TU>(method, request);
            Plugin.Log.Verbose($"[NetworkService] Response: {response}");
            return response;
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"[NetworkService] [InvokeAsync] {e.Message}");
            return Activator.CreateInstance<TU>();
        }
    }

    private Task OnReconnected(string? arg)
    {
        Connected?.Invoke();
        return Task.CompletedTask;
    }

    private Task OnClosed(Exception? arg)
    {
        Disconnected?.Invoke();
        return Task.CompletedTask;
    }

    private Task OnReconnecting(Exception? arg)
    {
        Disconnected?.Invoke();
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Sends a POST to the login server to get a JWT Token
    /// </summary>
    /// <returns>JWT Token if successful, otherwise null</returns>
    private static async Task<string?> FetchToken()
    {
        try
        {
            using var client = new HttpClient();
            var request = new FetchTokenRequest
            {
                Secret = Plugin.Configuration.Secret,
                Version = Plugin.Version
            };

            var payload = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(PostUrl, payload).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                Plugin.Log.Verbose("[NetworkHelper] Successfully authenticated");
                return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }

            var error = response.StatusCode switch
            {
                HttpStatusCode.Unauthorized => "[NetworkHelper] Unable to authenticate, invalid secret",
                HttpStatusCode.BadRequest => "[NetworkHelper] Unable to authenticate, outdated client",
                _ => $"[NetworkHelper] Unable to authenticate, {response.StatusCode}"
            };

            Plugin.Log.Warning(error);
            return null;
        }
        catch (HttpRequestException e)
        {
            var error = e.StatusCode switch
            {
                null => "[NetworkHelper] Unable to connect to authentication server",
                _ => e.Message
            };

            Plugin.Log.Warning(error);
            return null;
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"[NetworkHelper] Unable to send POST to server, {e.Message}");
            return null;
        }
    }

    public async void Dispose()
    {
        try
        {
            Connection.Reconnected -= OnReconnected;
            Connection.Reconnecting -= OnReconnecting;
            Connection.Closed -= OnClosed;

            await Connection.StopAsync().ConfigureAwait(false);
            await Connection.DisposeAsync().ConfigureAwait(false);
            GC.SuppressFinalize(this);
        }
        catch (Exception)
        {
            // Ignore
        }
    }
}