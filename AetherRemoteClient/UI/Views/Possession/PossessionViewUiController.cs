using System;
using System.Linq;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Possession;
using AetherRemoteCommon.Domain.Network.Possession.End;

namespace AetherRemoteClient.UI.Views.Possession;

public class PossessionViewUiController(NetworkService network, PossessionManager possessions, SelectionManager selectionManager)
{
    public async void Possess()
    {
        try
        {
            if (selectionManager.Selected.FirstOrDefault() is not { } friend)
                return;
            
            await possessions.TryBeginPossession(friend.FriendCode);

            // TODO: Better notification
        }
        catch (Exception)
        {
            // Ignore
        }
    }

    public async void Unpossess()
    {
        try
        {
            var selected = selectionManager.Selected;

            if (selected.Count is not 1)
                return;
            
            var request = new PossessionEndRequest();
            var response = await network.InvokeAsync<PossessionResponse>(HubMethod.Possession.End, request).ConfigureAwait(false);

            if (response.Response is not PossessionResponseEc.Success || response.Result is not PossessionResultEc.Success)
            {
                Plugin.Log.Warning($"[PossessionViewUiController.Unpossess] Possession end failed with codes {response.Response} - {response.Result}");
                
                // TODO: Better notification
                
                return;
            }

            possessions.EndPossessing();
            
            // TODO: Better notification
        }
        catch (Exception)
        {
            // Ignore
        }
    }
}