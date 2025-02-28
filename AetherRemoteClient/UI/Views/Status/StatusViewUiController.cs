using System;
using AetherRemoteClient.Services;
using AetherRemoteClient.Services.External;

namespace AetherRemoteClient.UI.Views.Status;

public class StatusViewUiController(GlamourerService glamourerService, NetworkService networkService, IdentityService identityService)
{
    public async void Disconnect()
    {
        try
        {
            await networkService.StopAsync().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"[StatusViewUiController] Unable to disconnect from the server, {e.Message}");
        }
    }

    public async void ResetIdentity()
    {
        try
        {
            await glamourerService.RevertToAutomation().ConfigureAwait(false);
            await identityService.SetIdentityToCurrentCharacter().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"[StatusViewUiController] Unable to reset identity, {e.Message}");
        }
    }
}