using System;
using System.Threading.Tasks;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteClient.Services.Configuration;
using AetherRemoteClient.Utils;

namespace AetherRemoteClient.Handlers;

/// <summary>
///     Provides a single entry point for logging into the game
/// </summary>
public class LoginHandler : IDisposable
{
    // Injected
    private readonly ActiveSessionService _activeSessionService;
    private readonly ConfigurationService _configurationService;
    private readonly NetworkService _networkService;
    private readonly ConnectionManager _connectionManager;
    private readonly DtrManager _dtrManager;
    
    /// <summary>
    ///     <inheritdoc cref="LoginHandler"/>
    /// </summary>
    public LoginHandler(
        ActiveSessionService activeSessionService,
        ConfigurationService configurationService,
        NetworkService networkService,
        ConnectionManager connectionManager,
        DtrManager dtrManager)
    {
        // Store injected services
        _activeSessionService = activeSessionService;
        _configurationService = configurationService;
        _networkService = networkService;
        _connectionManager = connectionManager;
        _dtrManager = dtrManager;
        
        // Subscribe to log in events
        Plugin.ClientState.Login += OnLogin;
        Plugin.ClientState.Logout += OnLogout;
        
        // If we're already logged in, fire the login function
        if (Plugin.ClientState.IsLoggedIn)
            OnLogin();
    }

    private void OnLogin() => _ = OnLoginAsync().ConfigureAwait(false);
    private async Task OnLoginAsync()
    {
        // Able to safely do this before any failure states since the character is logged in
        if (_configurationService.ShowOnDtrBar)
            _dtrManager.EnableDtrBar();
        
        if (await DalamudUtilities.TryGetLocalPlayer().ConfigureAwait(false) is not { } player)
            return;
        
        var name = player.Name.ToString();
        var world = player.HomeWorld.Value.Name.ToString();
        
        if (await _activeSessionService.StartNewSession(name, world) is false)
            return;
        
        if (_activeSessionService.AutoLogin)
            await _connectionManager.TryConnectToServerAsync().ConfigureAwait(false);
    }
    
    private void OnLogout(int type, int code) => _ = OnLogoutAsync().ConfigureAwait(false);
    private async Task OnLogoutAsync()
    {
        await _networkService.DisconnectFromServerAsync().ConfigureAwait(false);
        
        _activeSessionService.ClearAllSessionData();
    }

    public void Dispose()
    {
        Plugin.ClientState.Login -= OnLogin;
        Plugin.ClientState.Logout -= OnLogout;
        GC.SuppressFinalize(this);
    }
}