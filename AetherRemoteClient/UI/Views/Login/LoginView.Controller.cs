using System;
using System.Threading.Tasks;
using AetherRemoteClient.Domain;
using Dalamud.Utility;

namespace AetherRemoteClient.UI.Views.Login;

public partial class LoginView
{
        
    // Used to lock out the connect button for a moment after pressing it
    private DateTime _connectAttemptDisabledUntil = DateTime.MinValue;

    private async Task Connect()
    {
        _connectAttemptDisabledUntil = DateTime.UtcNow.AddSeconds(5);
        await _connectionManager.TryConnectToServerAsync().ConfigureAwait(false);
    }

    private void NavigateToSecretSettings() => _viewService.Navigate(View.Settings);

    private static void OpenDiscordLink() => Util.OpenLink("https://discord.com/invite/aetherremote");
}