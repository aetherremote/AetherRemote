using System.Threading.Tasks;
using Dalamud.Utility;

namespace AetherRemoteClient.UI.Views.Login;

public partial class LoginView
{
    private async Task Connect()
    {
        await _connectionManager.TryConnectToServerAsync().ConfigureAwait(false);
    }

    private static void OpenDiscordLink() => Util.OpenLink("https://discord.com/invite/aetherremote");
}