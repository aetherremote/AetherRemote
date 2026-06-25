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
    private readonly CharacterConfigurationService _characterConfigurationService;
    private readonly NetworkService _networkService;
    private readonly SecretsService _secretsService;
    private readonly SettingsService _settingsService;
    private readonly DtrManager _dtrManager;
    
    /// <summary>
    ///     <inheritdoc cref="LoginHandler"/>
    /// </summary>
    public LoginHandler(
        AuthenticationInfrastructure authenticationInfrastructure,
        CharacterConfigurationService characterConfigurationService,
        NetworkService networkService,
        SecretsService secretsService,
        SettingsService settingsService,
        DtrManager dtrManager)
    {
        // Store injected services
        _authenticationInfrastructure = authenticationInfrastructure;
        _characterConfigurationService = characterConfigurationService;
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
        
        // Load the character configuration. This will create a new configuration if it is the character's first time
        if (await _characterConfigurationService.LoadCharacterConfiguration().ConfigureAwait(false) is false)
            return;

        // Now if the character has an associate secret, we can initialize things require for the plugin
        if (_characterConfigurationService.Current?.SecretId is { } secretId)
        {
            if (_secretsService.Secrets.TryGetValue(secretId, out var secret))
                _authenticationInfrastructure.SetSecret(secret.Value);
            else
                Plugin.Log.Warning($"[LoginManager.OnLoginAsync] SecretId {secretId} does not have corresponding secret");

            if (await _settingsService.LoadSettings(secretId).ConfigureAwait(false) is false)
                Plugin.Log.Warning($"[LoginManager.OnLoginAsync] Failed to load settings for SecretId {secretId}");
        }
        
        // Ensure that all the values for various action responses and results are met (this check could go anywhere)
        ActionResponseParser.SanityCheck();
        
        if (_settingsService.ShowDtrBar)
            _dtrManager.EnableDtrBar();
        
        // Check if this secret has auto login enabled, and connect if so
        if (_settingsService.AutoLogin)
            await _networkService.ConnectToServerAsync().ConfigureAwait(false);
    }
    
    private void OnLogout(int type, int code) => _ = OnLogoutAsync().ConfigureAwait(false);
    private async Task OnLogoutAsync()
    {
        await _networkService.DisconnectFromServerAsync().ConfigureAwait(false);
    }

    public void Dispose()
    {
        Plugin.ClientState.Login -= OnLogin;
        Plugin.ClientState.Logout -= OnLogout;
        GC.SuppressFinalize(this);
    }
}