using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AetherRemoteClient.Domain.Enums;
using AetherRemoteClient.Domain.Network;
using AetherRemoteClient.Utils;
using AetherRemoteCommon;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.GetToken;
using AetherRemoteCommon.Domain.Network.LoginAuthentication;
using AetherRemoteCommon.Domain.Network.Possession;
using MessagePack;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

namespace AetherRemoteClient.Services;

/// <summary>
///     Provides methods to interact with the server
/// </summary>
public class NetworkService : IDisposable
{
#if DEBUG
    private const string HubUrl = "https://localhost:5006/primaryHub";
    private const string PostUrl = "https://localhost:5006/api/auth/login";
    // private const string HubUrl = "https://foxitsvc.com:5017/primaryHub";
    // private const string PostUrl = "https://foxitsvc.com:5017/api/auth/login";
    // private const string HubUrl = "https://foxitsvc.com:5006/primaryHub";
    // private const string PostUrl = "https://foxitsvc.com:5006/api/auth/login";
#else
    private const string HubUrl = "https://foxitsvc.com:5006/primaryHub";
    private const string PostUrl = "https://foxitsvc.com:5006/api/auth/login";
#endif
    
    // Serialization options for converting camel case to pascal case in deserialization
    private static readonly JsonSerializerOptions DeserializationOptions = new() { PropertyNameCaseInsensitive = true };
    
    // Long-lived HTTP Client
    private static readonly HttpClient Client = new();
    
    // Signal R
    private readonly HubConnection _connection;

    // Secret, used for getting the JWT
    private string? _secret;
    
    // Token resources, used for caching access
    private  string? _cachedToken;
    private static DateTime _tokenExpiration;

    /// <summary>
    ///     Event fired when the server successfully connects, either by reconnection or manual connection
    /// </summary>
    public event Func<Task>? Connected;

    /// <summary>
    ///     Event fired when the server connection is lost, either by disruption or manual intervention
    /// </summary>
    public event Func<Task>? Disconnected;

    /// <summary>
    ///     The state of the connection to the server
    /// </summary>
    public ConnectionState State => _connection.State switch
    {
        HubConnectionState.Disconnected => ConnectionState.Disconnected,
        HubConnectionState.Connected => ConnectionState.Connected,
        HubConnectionState.Connecting => ConnectionState.Connecting,
        HubConnectionState.Reconnecting => ConnectionState.Reconnecting,
        _ => throw new UnreachableException($"[NetworkService.State] {nameof(_connection.State)}")
    };
    
    /// <summary>
    ///     <inheritdoc cref="NetworkService"/>
    /// </summary>
    public NetworkService()
    {
        _connection = new HubConnectionBuilder().WithUrl(HubUrl, options =>
            {
                options.AccessTokenProvider = async () => await TryGetSecret().ConfigureAwait(false);
            })
            .WithAutomaticReconnect(new InfiniteRetryPolicy())
            .AddMessagePackProtocol(options =>
            {
                options.SerializerOptions = MessagePackSerializerOptions.Standard.WithSecurity(MessagePackSecurity.UntrustedData);
            })
            .Build();

        _connection.Reconnected += OnReconnected;
        _connection.Reconnecting += OnReconnecting;
        _connection.Closed += OnClosed;
    }

    /// <summary>
    ///     Begins a connection to the server
    /// </summary>
    public async Task StartAsync(string secret)
    {
        if (_connection.State is not HubConnectionState.Disconnected)
            return;

        // If this is a different secret from last time, invalidate what we have currently
        if (_secret != secret)
        {
            _cachedToken = null;
            _tokenExpiration = DateTime.MinValue;
        }

        _secret = secret;
        
        try
        {
            await _connection.StartAsync().ConfigureAwait(false);

            if (_connection.State is HubConnectionState.Connected)
            {
                Connected?.Invoke();
            }
            else
            { 
                NotificationHelper.Warning("[Aether Remote] Unable to connect", "See developer console for more information");
            }
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"[NetworkService.StartAsync] {e}");
            NotificationHelper.Warning("[Aether Remote] Unable to connect", "See developer console for more information");
        }
    }

    /// <summary>
    ///     Ends a connection to the server
    /// </summary>
    public async Task StopAsync()
    {
        if (_connection.State is HubConnectionState.Disconnected)
            return;

        try
        {
            await _connection.StopAsync().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"[NetworkService.StopAsync] {e}]");
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
        if (_connection.State is not HubConnectionState.Connected)
        {
            Plugin.Log.Warning("[NetworkService.InvokeAsync] No connection established");
            return Activator.CreateInstance<T>();
        }

        try
        {
            Plugin.Log.Verbose($"[NetworkService.InvokeAsync] Request: {request}");
            var response = await _connection.InvokeAsync<T>(method, request).ConfigureAwait(false);
            Plugin.Log.Verbose($"[NetworkService.InvokeAsync] Response: {response}");
            return response;
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"[NetworkService.InvokeAsync] {e}");
            return Activator.CreateInstance<T>();
        }
    }
    
    /// <summary>
    ///     Creates a listener for a specific method handled by provided method group
    /// </summary>
    public IDisposable ListenFunc<T>(string name, Func<T, ActionResult<Unit>> handler) => _connection.On(name, handler);
    
    /// <summary>
    ///     <inheritdoc cref="ListenFunc"/>
    /// </summary>
    public IDisposable ListenFuncAsync<T>(string name, Func<T, Task<ActionResult<Unit>>> handler) => _connection.On(name, handler);
    
    /// <summary>
    ///     <inheritdoc cref="ListenFunc"/>
    /// </summary>
    public IDisposable ListenAction<T>(string name, Action<T> handler) => _connection.On(name, handler);
    
    /// <summary>
    ///     <inheritdoc cref="ListenFunc"/>
    /// </summary>
    public IDisposable ListenActionAsync<T>(string name, Action<Task<T>> handler) => _connection.On(name, handler);
    
    /// <summary>
    ///     <inheritdoc cref="ListenFunc"/>
    /// </summary>
    public IDisposable ListenPossession<T>(string name, Func<T, PossessionResultEc> handler) => _connection.On(name, handler);
    
    /// <summary>
    ///     <inheritdoc cref="ListenFunc"/>
    /// </summary>
    public IDisposable ListenPossessionAsync<T>(string name, Func<T, Task<PossessionResultEc>> handler) => _connection.On(name, handler);

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
    
    private async Task<string?> TryGetSecret()
    {
        // If we have a cached token, and it isn't expired, use it
        if (_cachedToken is not null && DateTime.UtcNow < _tokenExpiration)
            return _cachedToken;
        
        if (await TryAuthenticateSecret().ConfigureAwait(false) is not { } token)
            return null;
        
        // Caching
        _cachedToken = token;
        _tokenExpiration = DateTime.UtcNow.AddHours(Constraints.TokenExpirationInHours);
        
        return token;
    }
    
    private async Task<string?> TryAuthenticateSecret()
    {
        if (_secret is null) return null;
        
        var request = new GetTokenRequest(_secret, Plugin.Version);
        var payload = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        try
        {
            var response = await Client.PostAsync(PostUrl, payload).ConfigureAwait(false);
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
        _connection.Reconnected -= OnReconnected;
        _connection.Reconnecting -= OnReconnecting;
        _connection.Closed -= OnClosed;

        _connection.StopAsync().ConfigureAwait(false);
        _connection.DisposeAsync().AsTask().GetAwaiter().GetResult();

        GC.SuppressFinalize(this);
    }
}