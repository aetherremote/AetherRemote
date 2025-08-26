using System;
using System.IO;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteClient.Utils;
using Dalamud.Bindings.ImGui;
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
    }
    
    public async void Connect()
    {
        try
        {
            // Only save if the configuration is set
            if (Plugin.CharacterConfiguration is null)
                return;
            
            // Set the secret
            Plugin.CharacterConfiguration.Secret = Secret;
            
            // Save the configuration
            Plugin.CharacterConfiguration.Save();
            
            // Try to connect to the server
            await _networkService.StartAsync();
        }
        catch (Exception)
        {
            // ignored
        }
    }

    public static void CopyOriginalSecret()
    {
        // Check if the legacy configuration isn't null
        if (Plugin.LegacyConfiguration?.Secret is not { } secret)
            return;
        
        // Copy to clipboard
        ImGui.SetClipboardText(secret);
        
        // Notify the client
        NotificationHelper.Success("Copied secret to clipboard", string.Empty);
        
        // Try to remove the old configuration file
        try
        {
            // Create the file path
            var legacyConfigurationFilePath = Plugin.PluginInterface.GetPluginConfigDirectory() + ".json";
            
            // Delete the file if it exists
            if (File.Exists(legacyConfigurationFilePath))
                File.Delete(legacyConfigurationFilePath);
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[LoginViewUiController] Error while attempting to delete old configuration files, {e}");
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