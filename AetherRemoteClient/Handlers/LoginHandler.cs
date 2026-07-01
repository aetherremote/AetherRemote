using System;
using System.Threading.Tasks;
using AetherRemoteClient.Infrastructure.Authentication;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteClient.Utils;

namespace AetherRemoteClient.Handlers;

/// <summary>
///     Provides a single entry point for logging into the game
/// </summary>
public class LoginHandler : IDisposable
{
    // Injected
    private readonly AuthenticationInfrastructure _authenticationInfrastructure;
    private readonly ActiveSessionService _activeSessionService;
    private readonly GlobalSettingsService _globalSettingsService;
    private readonly NetworkService _networkService;
    private readonly SecretsService _secretsService;
    private readonly SettingsService _settingsService;
    private readonly DtrManager _dtrManager;
    
    /// <summary>
    ///     Event fired when all plugin initialization for a logged-into-game character has completed.
    /// </summary>
    /// <remarks> Only Ui Controllers should react to this event </remarks>
    public event Action? LoginInitializationCompleted;

    /// <summary> Guard for the event </summary>
    public bool HasLoginInitializationCompleted;
    
    /// <summary>
    ///     <inheritdoc cref="LoginHandler"/>
    /// </summary>
    public LoginHandler(
        AuthenticationInfrastructure authenticationInfrastructure,
        ActiveSessionService activeSessionService,
        GlobalSettingsService globalSettingsService,
        NetworkService networkService,
        SecretsService secretsService,
        SettingsService settingsService,
        DtrManager dtrManager)
    {
        // Store injected services
        _authenticationInfrastructure = authenticationInfrastructure;
        _activeSessionService = activeSessionService;
        _globalSettingsService = globalSettingsService;
        _networkService = networkService;
        _secretsService = secretsService;
        _settingsService = settingsService;
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
        // TODO: This is a pretty big one, but if something goes wrong here, the plugin is unusable.
        
        if (await DalamudUtilities.TryGetLocalPlayer().ConfigureAwait(false) is not { } player)
            return;
        
        var name = player.Name.ToString();
        var world = player.HomeWorld.Value.Name.ToString();
        
        // TODO: Decide on what should happen if this fails
        await _activeSessionService.InitializeCharacter(name, world).ConfigureAwait(false);

        // Now if the character has an associate secret, we can initialize things require for the plugin
        if (_activeSessionService.SecretId is { } secretId)
        {
            // Don't need to set secret in the active session service because we only care about the secret's id which is already set there
            if (_secretsService.Secrets.TryGetValue(secretId, out var secret))
                _authenticationInfrastructure.SetSecret(secret.Value);
            else
                Plugin.Log.Warning($"[LoginManager.OnLoginAsync] SecretId {secretId} does not have corresponding secret");

            if (await _settingsService.LoadSettings(secretId).ConfigureAwait(false) is false)
                Plugin.Log.Warning($"[LoginManager.OnLoginAsync] Failed to load settings for SecretId {secretId}");
        }
        
        // Emit event for Ui controllers
        LoginInitializationCompleted?.Invoke();
        HasLoginInitializationCompleted = true;
        
        if (_globalSettingsService.ShowOnDtrBar)
            _dtrManager.EnableDtrBar();
        
        if (_settingsService.AutoLogin)
            await _networkService.ConnectToServerAsync().ConfigureAwait(false);
    }
    
    private void OnLogout(int type, int code) => _ = OnLogoutAsync().ConfigureAwait(false);
    private async Task OnLogoutAsync()
    {
        await _networkService.DisconnectFromServerAsync().ConfigureAwait(false);
        HasLoginInitializationCompleted = false;
    }

    public void Dispose()
    {
        Plugin.ClientState.Login -= OnLogin;
        Plugin.ClientState.Logout -= OnLogout;
        GC.SuppressFinalize(this);
    }
}