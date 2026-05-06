using System;
using System.Threading.Tasks;
using AetherRemoteClient.Services;
using AetherRemoteClient.Utils;

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
    ///     Event protection for <see cref="LoginFinished"/>
    /// </summary>
    public bool HasLoginFinished { get; private set; }
    
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

    private void OnLogin() => _ = OnLoginAsync().ConfigureAwait(false);
    private async Task OnLoginAsync()
    {
        // Make sure the local player is present
        if (await DalamudUtilities.RunOnFramework(() => Plugin.ObjectTable.LocalPlayer).ConfigureAwait(false) is not { } player)
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
            
        // Set the event protection lines
        HasLoginFinished = true;
        
        // Ensure that all the values for various action responses and results are met (this check could go anywhere)
        ActionResponseParser.SanityCheck();
            
        // Initiate a connection to the server if auto login is set to true
        if (Plugin.CharacterConfiguration.AutoLogin is true)
            await _networkService.StartAsync().ConfigureAwait(false);
    }
    
    private void OnLogout(int type, int code) => _ = OnLogoutAsync().ConfigureAwait(false);
    private async Task OnLogoutAsync()
    {
        await _networkService.StopAsync();
            
        // Reset event protection
        HasLoginFinished = false;
    }

    public void Dispose()
    {
        Plugin.ClientState.Login -= OnLogin;
        Plugin.ClientState.Logout -= OnLogout;
        GC.SuppressFinalize(this);
    }
}