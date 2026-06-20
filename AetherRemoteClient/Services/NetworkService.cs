using System;
using System.Diagnostics;
using System.Threading.Tasks;
using AetherRemoteClient.Domain.Enums;
using AetherRemoteClient.Domain.Exceptions.Network;
using AetherRemoteClient.Domain.Network;
using AetherRemoteClient.Infrastructure.Authentication;
using AetherRemoteClient.Utils;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Possession;
using MessagePack;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

namespace AetherRemoteClient.Services;

/// <summary>
///     Provides fields and methods to interact with the underlying SignalR connection
/// </summary>
public class NetworkService : IAsyncDisposable
{
    
#if DEBUG
    private const string HubUrl = "https://localhost:5006/primaryHub";
    // private const string HubUrl = "https://foxitsvc.com:5017/primaryHub";
    // private const string HubUrl = "https://foxitsvc.com:5006/primaryHub";
#else
    private const string HubUrl = "https://foxitsvc.com:5006/primaryHub";
#endif
    
    // SignalR hub connection, the entry point for all connectivity to the actual server
    private readonly HubConnection _connection;
    
    /// <summary> Connected to the server successfully, either by reconnection or manual connection </summary>
    public event Func<Task>? Connected;

    /// <summary> Disconnected from the server, either by disruption or manual intervention </summary>
    public event Func<Task>? Disconnected;

    /// <summary> The SignalR connection status </summary>
    public ConnectionState State => _connection.State switch
    {
        HubConnectionState.Disconnected => ConnectionState.Disconnected,
        HubConnectionState.Connected => ConnectionState.Connected,
        HubConnectionState.Connecting => ConnectionState.Connecting,
        HubConnectionState.Reconnecting => ConnectionState.Reconnecting,
        _ => throw new UnreachableException($"[NetworkService.State] {nameof(_connection.State)}")
    };
    
    /// <summary> Creates a listener for a specific method handled by provided method group </summary>
    public IDisposable ListenFunc<T>(string name, Func<T, ActionResult<Unit>> handler) => _connection.On(name, handler);
    
    /// <summary> <inheritdoc cref="ListenFunc"/> </summary>
    public IDisposable ListenFuncAsync<T>(string name, Func<T, Task<ActionResult<Unit>>> handler) => _connection.On(name, handler);
    
    /// <summary> <inheritdoc cref="ListenFunc"/> </summary>
    public IDisposable ListenAction<T>(string name, Action<T> handler) => _connection.On(name, handler);
    
    /// <summary> <inheritdoc cref="ListenFunc"/> </summary>
    public IDisposable ListenActionAsync<T>(string name, Action<Task<T>> handler) => _connection.On(name, handler);
    
    /// <summary> <inheritdoc cref="ListenFunc"/> </summary>
    public IDisposable ListenPossession<T>(string name, Func<T, PossessionResultEc> handler) => _connection.On(name, handler);
    
    /// <summary> <inheritdoc cref="ListenFunc"/> </summary>
    public IDisposable ListenPossessionAsync<T>(string name, Func<T, Task<PossessionResultEc>> handler) => _connection.On(name, handler);
    
    /// <summary> <inheritdoc cref="NetworkService"/> </summary>
    public NetworkService(AuthenticationInfrastructure authenticationInfrastructure)
    {
        _connection = new HubConnectionBuilder().WithUrl(HubUrl, options =>
            {
                options.AccessTokenProvider = async () => await authenticationInfrastructure.GetTokenAsync().ConfigureAwait(false);
            })
            .WithAutomaticReconnect(new InfiniteRetryPolicy())
            .AddMessagePackProtocol(options =>
            {
                options.SerializerOptions = MessagePackSerializerOptions.Standard.WithSecurity(MessagePackSecurity.UntrustedData);
            })
            .Build();

        _connection.Reconnected += OnConnected;
        _connection.Reconnecting += OnDisconnected;
        _connection.Closed += OnDisconnected;
    }
    
    /// <summary> Attempts to connect to the SignalR server </summary>
    public async Task ConnectToServerAsync()
    {
        if (_connection.State is not HubConnectionState.Disconnected) return;

        try
        {
            // All exceptions in this function stem from the AuthenticationInfrastructure class
            await _connection.StartAsync().ConfigureAwait(false);
        }
        catch (ArAuthAuthenticationException e)
        {
            switch (e.ErrorCode)
            {
                // ==== Success cases, nothing to display to the client or log ====
                case ArAuthAuthenticationErrorCode.Success:
                    Plugin.Log.Info("[NetworkService.ConnectToServerAsync] Connected successfully");
                    break;
                
                // ==== Failure cases, but the client should be notified meaningfully ====
                case ArAuthAuthenticationErrorCode.UnknownSecret:
                    Plugin.Log.Warning("[NetworkService.ConnectToServerAsync] Invalid secret, this could be because the secret does not exist, or has been banned");
                    NotificationHelper.Warning("Invalid Secret", "The secret you tried to connect with doesn't exist");
                    break;
                
                case ArAuthAuthenticationErrorCode.AuthenticationServerUnreachable:
                    Plugin.Log.Warning("[NetworkService.ConnectToServerAsync] Servers are down, try again later");
                    NotificationHelper.Warning("Servers Down", "Unable to connect to servers, try again later");
                    break;
                
                case ArAuthAuthenticationErrorCode.VersionMismatch:
                    Plugin.Log.Warning("[NetworkService.ConnectToServerAsync] Version mismatch, update your client to latest version");
                    NotificationHelper.Warning("Outdated Client", "Please update your client to the latest version and try again");
                    break;
                
                // ==== Failure cases, but the heavy lifting should be in the console ====
                case ArAuthAuthenticationErrorCode.Uninitialized:
                case ArAuthAuthenticationErrorCode.SecretNotSetOrInvalid:
                case ArAuthAuthenticationErrorCode.InvalidOrMalformedToken:
                case ArAuthAuthenticationErrorCode.Unknown:
                case ArAuthAuthenticationErrorCode.UnboundScope:
                default:
                    Plugin.Log.Warning($"[NetworkService.ConnectToServerAsync] {e}");
                    NotificationHelper.Warning("Unable to Connect to Server", "See more details by opening the developer console by typing /xllog");
                    break;
            }
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[NetworkService.ConnectToServerAsync] {e}");
        }
        finally
        {
            if (_connection.State is HubConnectionState.Connected)
                Connected?.Invoke();
        }
    }
    
    /// <summary> Attempts to disconnect from the SignalR server </summary>
    public async Task DisconnectFromServerAsync()
    {
        if (_connection.State is HubConnectionState.Disconnected) return;

        try
        {
            await _connection.StopAsync().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[NetworkService.DisconnectFromServerAsync] {e}");
        }
    }
    
    /// <summary>
    ///     Invokes a method on the server and awaits a result
    /// </summary>
    /// <param name="method">Hub Method Name (More details in <see cref="HubMethod"/>)</param>
    /// <param name="request">Request Payload (More details in <see cref="ActionRequest"/>)</param>
    /// <returns>Response Payload (More details in <see cref="ActionResponse"/>)</returns>
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
            Plugin.Log.Error($"[NetworkService.InvokeAsync] {e}");
            return Activator.CreateInstance<T>();
        }
    }
    
    // Connection event wrappers
    private Task OnConnected(string? arg) => Connected?.Invoke() ?? Task.CompletedTask;
    private Task OnDisconnected(Exception? arg) => Disconnected?.Invoke() ?? Task.CompletedTask;

    public async ValueTask DisposeAsync()
    {
        _connection.Reconnected -= OnConnected;
        _connection.Reconnecting -= OnDisconnected;
        _connection.Closed -= OnDisconnected;
        
        await _connection.StopAsync().ConfigureAwait(false);
        await _connection.DisposeAsync().ConfigureAwait(false);
        
        GC.SuppressFinalize(this);
    }
}