using System;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using Dalamud.Utility;

namespace AetherRemoteClient.UI.Views.Login;

public class LoginViewUiController(NetworkManager networkManager)
{
    /// <summary>
    ///     User inputted secret
    /// </summary>
    public string Secret = Plugin.Configuration.Secret;
    
    public async void Connect()
    {
        try
        {
            Plugin.Configuration.Secret = Secret;
            Plugin.Configuration.Save();
            
            await networkManager.StartAsync();
        }
        catch (Exception)
        {
            // ignored
        }
    }
    
    public static void OpenDiscordLink() => Util.OpenLink("https://discord.com/invite/aetherremote");
}