using System;
using AetherRemoteClient.Services;

namespace AetherRemoteClient.Managers;

/// <summary>
///     Provides a single entry point for logging into the game
/// </summary>
public class LoginManager : IDisposable
{
    // Injected
    private readonly NetworkService _networkService;

    /// <summary>
    ///     If we have finished processing all the login events
    /// </summary>
    public event Action? LoginFinished;
    
    /// <summary>
    ///     <inheritdoc cref="LoginManager"/>
    /// </summary>
    public LoginManager(NetworkService networkService)
    {
        // Store injected services
        _networkService = networkService;
        
        // Subscribe to log in events
        Plugin.ClientState.Login += OnLogin;
        Plugin.ClientState.Logout += OnLogout;
        
        // If we're already logged in, fire the login function
        if (Plugin.ClientState.IsLoggedIn)
            OnLogin();
    }

    private async void OnLogin()
    {
        try
        {
            // Make sure the local player is present
            if (await Plugin.RunOnFramework(() => Plugin.ClientState.LocalPlayer).ConfigureAwait(false) is not { } player)
                return;

            // Store the name and world for readability
            var name = player.Name.ToString();
            var world = player.HomeWorld.Value.Name.ToString();

            // Load the character configuration
            if (await ConfigurationService.LoadCharacterConfiguration(name, world).ConfigureAwait(false) is not { } characterConfiguration)
                return;

            // Set the character configuration
            Plugin.CharacterConfiguration = characterConfiguration;

            // Emit an event
            LoginFinished?.Invoke();
            
            // Initiate a connection to the server if auto login is set to true
            if (Plugin.CharacterConfiguration.AutoLogin is true)
                await _networkService.StartAsync().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[LoginManager] Unexpected error handling login event, {e}");
        }
    }
    
    private async void OnLogout(int type, int code)
    {
        try
        {
            await _networkService.StopAsync();
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[LoginManager] Unexpected error handling logout event, {e}");
        }
    }

    public void Dispose()
    {
        Plugin.ClientState.Login -= OnLogin;
        Plugin.ClientState.Logout -= OnLogout;
        GC.SuppressFinalize(this);
    }
}