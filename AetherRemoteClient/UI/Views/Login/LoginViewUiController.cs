using System;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using Dalamud.Utility;

namespace AetherRemoteClient.UI.Views.Login;

public class LoginViewUiController : IDisposable
{
    // Injected
    private readonly NetworkService _networkService;
    private readonly LoginManager _loginManager;
    
    /// <summary>
    ///     User inputted secret
    /// </summary>
    public string Secret = string.Empty;
    
    public LoginViewUiController(NetworkService networkService, LoginManager loginManager)
    {
        _networkService = networkService;
        _loginManager = loginManager;
        _loginManager.LoginFinished += OnLoginFinished;
        if (_loginManager.HasLoginFinished)
            OnLoginFinished();
    }
    
    public async void Connect()
    {
        try
        {
            // Only save if the configuration is set
            if (Plugin.CharacterConfiguration is null)
                return;

            // Don't save if the string is empty
            if (Secret == string.Empty)
                return;
            
            // Set the secret
            Plugin.CharacterConfiguration.Secret = Secret;
            
            // Save the configuration
            await Plugin.CharacterConfiguration.Save().ConfigureAwait(false);
            
            // Try to connect to the server
            await _networkService.StartAsync();
        }
        catch (Exception)
        {
            // ignored
        }
    }
    
    public static void OpenDiscordLink() => Util.OpenLink("https://discord.com/invite/aetherremote");

    private void OnLoginFinished()
    {
        if (Plugin.CharacterConfiguration is null)
            return;
        
        Secret = Plugin.CharacterConfiguration.Secret;
    }
    
    public void Dispose()
    {
        _loginManager.LoginFinished -= OnLoginFinished;
        GC.SuppressFinalize(this);
    }
}