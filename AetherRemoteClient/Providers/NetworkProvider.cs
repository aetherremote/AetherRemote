using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AetherRemoteCommon.Domain.Network;
using Microsoft.AspNetCore.SignalR.Client;

namespace AetherRemoteClient.Providers;

/// <summary>
/// Provides a connection and methods to invoke hub methods on the SignalR server
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
    
    // Instantiated
    private readonly HubConnection _connection;

    /// <summary>
    /// Event fired when the connection to the server is established
    /// </summary>
    public event EventHandler? ServerConnected;
    
    /// <summary>
    /// Event fired when the connection to the server is lost
    /// </summary>
    public event EventHandler? ServerDisconnected;
    
    /// <summary>
    /// Check if we are connected to the server
    /// </summary>
    public bool Connected => _connection.State is HubConnectionState.Connected;
    
    /// <summary>
    /// <inheritdoc cref="NetworkProvider"/>
    /// </summary>
    public NetworkProvider()
    {
        _connection = new HubConnectionBuilder().WithUrl(HubUrl, options =>
        {
            options.AccessTokenProvider = async () => await GetToken().ConfigureAwait(false);
        }).Build();
    }

    /// <summary>
    /// Invokes a method on the server hub
    /// </summary>
    public async Task<TU> InvokeCommand<T, TU>(string commandName, T request)
    {
        if (_connection.State is not HubConnectionState.Connected)
        {
            Plugin.Log.Warning("Cannot invoke commands while server is disconnected");
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
            Plugin.Log.Error($"Error invoking {commandName} command: {ex}");
            return Activator.CreateInstance<TU>();
        }
    }

    /// <summary>
    /// Attempt to connect to the server
    /// </summary>
    public async Task Connect()
    {
        if (Plugin.DeveloperMode)
            return;
        
        // Already connected
        if (_connection.State is HubConnectionState.Connected)
        {
            Plugin.Log.Information("Already connected, ignoring connection attempt");
            return;
        }

        // Attempt the connection
        try
        {
            await _connection.StartAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Plugin.Log.Warning($"Connection attempt failed: {ex}");
            return;
        }

        // If connection failed for some reason, clean up
        if (_connection.State is not HubConnectionState.Connected)
        {
            Plugin.Log.Warning("Connecting to the server was unsuccessful");
            return;
        }

        // Server State Events
        _connection.Closed += ServerConnectionClosed;
        ServerConnected?.Invoke(this, EventArgs.Empty);
    }
    
    /// <summary>
    /// Registers a handler for signalr methods being invoked from the server
    /// </summary>
    public IDisposable RegisterHandler<T>(string methodName, Action<T> handler)
    {
        return _connection.On(methodName, handler);
    }
    
    /// <summary>
    /// <inheritdoc cref="RegisterHandler{T}(string, Action{T})"/>
    /// </summary>
    public IDisposable RegisterHandler<T>(string methodName, Func<T, Task> handler)
    {
        return _connection.On(methodName, handler);
    }

    /// <summary>
    /// <inheritdoc cref="RegisterHandler{T}(string, Action{T})"/>
    /// </summary>
    public IDisposable RegisterHandlerAsync<TRequest, TResult>(string methodName, Func<TRequest, Task<TResult>> handler)
    {
        return _connection.On(methodName, handler);
    }

    /// <summary>
    /// Disconnects from the server
    /// </summary>
    public async Task Disconnect()
    {
        await _connection.StopAsync();
        _connection.Closed -= ServerConnectionClosed;
    }
    
    /// <summary>
    /// Get the connection status to the server
    /// </summary>
    public HubConnectionState State => _connection.State;
    
    /// <summary>
    /// Sends a POST to the login server to get a JWT Token
    /// </summary>
    /// <returns>JWT Token if successful, otherwise null</returns>
    private static async Task<string?> GetToken()
    {
        try
        {
            using var client = new HttpClient();
            var raw = new LoginRequest(Plugin.Configuration.Secret, Plugin.Version);
            var payload = new StringContent(JsonSerializer.Serialize(raw), Encoding.UTF8, "application/json");
            var post = await client.PostAsync(PostUrl, payload).ConfigureAwait(false);
            if (post.IsSuccessStatusCode)
            {
                Plugin.Log.Verbose("Successfully authenticated");
                return await post.Content.ReadAsStringAsync().ConfigureAwait(false);
            }

            var errorMessage = post.StatusCode switch
            {
                System.Net.HttpStatusCode.Unauthorized =>
                    "Unable to connect, invalid secret. Please register in the discord or reach out for assistance.",
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
    
    // Fired when disconnecting from the server
    private Task ServerConnectionClosed(Exception? arg)
    {
        ServerDisconnected?.Invoke(this, EventArgs.Empty);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Standard dispose function
    /// </summary>
    public async void Dispose()
    {
        try
        {
            await _connection.StopAsync();
            await _connection.DisposeAsync();
            _connection.Closed -= ServerConnectionClosed;
            GC.SuppressFinalize(this);
        }
        catch (Exception exception)
        {
            Plugin.Log.Error($"Error while disposing NetworkProvider: {exception}");
        }
    }
}