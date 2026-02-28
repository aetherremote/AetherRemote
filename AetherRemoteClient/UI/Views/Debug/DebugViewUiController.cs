using System.Threading.Tasks;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;

namespace AetherRemoteClient.UI.Views.Debug;

public class DebugViewUiController(StatusManager statusManager, ViewService viewService)
{
    public async Task Debug()
    {

        var entry = Plugin.DtrBar.Get("AetherRemote");
        entry.Text = "[Aether Remote]";
        entry.OnClick = args =>
        {
            viewService.CurrentView = View.Status;
        };
        entry.Tooltip = $"You have 3 things affecting you";
        
        
        // Do Something
        await Task.Delay(1000).ConfigureAwait(false);
    }
    
    public async Task Debug2()
    {
        // Do Something
        await Task.Delay(1000).ConfigureAwait(false);
    }
}