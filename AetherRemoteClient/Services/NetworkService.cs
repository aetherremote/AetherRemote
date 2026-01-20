using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AetherRemoteClient.Utils;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network.GetToken;
using AetherRemoteCommon.Domain.Network.LoginAuthentication;
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
    // private const string HubUrl = "https://localhost:5006/primaryHub";
    // private const string PostUrl = "https://localhost:5006/api/auth/login";
    private const string HubUrl = "https://foxitsvc.com:5017/primaryHub";
    private const string PostUrl = "https://foxitsvc.com:5017/api/auth/login";
    // private const string HubUrl = "https://foxitsvc.com:5006/primaryHub";
    // private const string PostUrl = "https://foxitsvc.com:5006/api/auth/login";
#else
    private const string HubUrl = "https://foxitsvc.com:5006/primaryHub";
    private const string PostUrl = "https://foxitsvc.com:5006/api/auth/login";
#endif

    private static readonly JsonSerializerOptions DeserializationOptions = new() { PropertyNameCaseInsensitive = true };
    
    /// <summary>
    ///     Access token required to connect to the SignalR hub
    /// </summary>
    private string? _token = string.Empty;

    /// <summary>
    ///     If the plugin has begun the connection process
    /// </summary>
    public bool Connecting;

    /// <summary>
    ///     <inheritdoc cref="NetworkService"/>
    /// </summary>
    public NetworkService()
    {
        Connection = new HubConnectionBuilder().WithUrl(HubUrl, options =>
            {
                // ReSharper disable once RedundantTypeArgumentsOfMethod
                options.AccessTokenProvider = () => Task.FromResult<string?>(_token);
            })
            .WithAutomaticReconnect()
            .AddMessagePackProtocol(options =>
            {
                options.SerializerOptions = MessagePackSerializerOptions.Standard.WithSecurity(MessagePackSecurity.UntrustedData);
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

        Connecting = true;
        
        try
        {
            // Try to get the Token
            if (await TryAuthenticateSecret().ConfigureAwait(false) is { } token)
            {
                _token = token;
            
                await Connection.StartAsync().ConfigureAwait(false);

                if (Connection.State is HubConnectionState.Connected)
                {
                    Connected?.Invoke();
                    NotificationHelper.Success("[Aether Remote] Connected", string.Empty);
                }
                else
                { 
                    NotificationHelper.Warning("[Aether Remote] Unable to connect", "See developer console for more information");
                }
            }
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"[NetworkService] [StartAsync] {e.Message}]");
            NotificationHelper.Warning("[Aether Remote] Could not connect", "See developer console for more information");
        }
        
        Connecting = false;
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
    /// <returns></returns>
    public async Task<T> InvokeAsync<T>(string method, object request)
    {
        if (Connection.State is not HubConnectionState.Connected)
        {
            Plugin.Log.Warning("[NetworkService] No connection established");
            return Activator.CreateInstance<T>();
        }

        try
        {
            Plugin.Log.Verbose($"[NetworkService] Request: {request}");
            var response = await Connection.InvokeAsync<T>(method, request);
            Plugin.Log.Verbose($"[NetworkService] Response: {response}");
            return response;
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"[NetworkService] [InvokeAsync] {e}");
            return Activator.CreateInstance<T>();
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

    private static async Task<string?> TryAuthenticateSecret()
    {
        if (Plugin.CharacterConfiguration?.Secret is null)
        {
            Plugin.Log.Warning("[NetworkService.TryAuthenticateSecret] You do not have a secret to provide for authentication");
            return null;
        }
        
        using var client = new HttpClient();
        var request = new GetTokenRequest(Plugin.CharacterConfiguration.Secret, Plugin.Version);
        var payload = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        try
        {
            var response = await client.PostAsync(PostUrl, payload).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (JsonSerializer.Deserialize<LoginAuthenticationResult>(content, DeserializationOptions) is not { } result)
            {
                Plugin.Log.Warning("[NetworkService.TryAuthenticateSecret] A deserialization error occurred");
                return null;
            }

            switch (result.ErrorCode)
            {
                case LoginAuthenticationErrorCode.Success:
                    return result.Secret;

                case LoginAuthenticationErrorCode.VersionMismatch:
                    NotificationHelper.Error("Aether Remote - Client Outdated", "You will need to update the plugin before connecting to the servers.");
                    return null;

                case LoginAuthenticationErrorCode.UnknownSecret:
                    NotificationHelper.Error("Aether Remote - Invalid Secret", "The secret you provided is either empty, or invalid. If you believe this is a mistake, please reach out to the developer.");
                    return null;

                case LoginAuthenticationErrorCode.Uninitialized:
                case LoginAuthenticationErrorCode.Unknown:
                default:
                    NotificationHelper.Error("Aether Remote - Unable to Connect", $"Something went wrong while connecting to the server, {result.ErrorCode}");
                    return null;
            }
        }
        catch (HttpRequestException)
        {
            NotificationHelper.Warning("Authentication Server Down", "Please wait and try again later. You can monitor or report this problem in the discord if it persists");
            return null;
        }
        catch (Exception e)
        {
            Plugin.Log.Error(e.ToString());
            return null;
        }
    }

    public void Dispose()
    {
        Connection.Reconnected -= OnReconnected;
        Connection.Reconnecting -= OnReconnecting;
        Connection.Closed -= OnClosed;

        Connection.StopAsync().ConfigureAwait(false);
        Connection.DisposeAsync().AsTask().GetAwaiter().GetResult();

        GC.SuppressFinalize(this);
    }
}