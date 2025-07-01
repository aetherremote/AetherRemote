using System;
using AetherRemoteClient.Services;
using Dalamud.Utility;

namespace AetherRemoteClient.UI.Views.Login;

public class LoginViewUiController(NetworkService networkService)
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
            
            await networkService.StartAsync();
        }
        catch (Exception)
        {
            // ignored
        }
    }
    
    public static void OpenDiscordLink() => Util.OpenLink("https://discord.com/invite/aetherremote");
}