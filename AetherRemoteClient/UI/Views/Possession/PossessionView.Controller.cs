using System.Linq;
using System.Threading.Tasks;
using AetherRemoteClient.Domain.Enums;
using AetherRemoteClient.Utils;
using AetherRemoteCommon.Domain.Enums.Permissions;

namespace AetherRemoteClient.UI.Views.Possession;

public partial class PossessionView
{
    private async Task Possess()
    {
        if (_selectionManager.Selected.FirstOrDefault() is not { } friend)
            return;
            
        if (await _possessionManager.Possess(friend).ConfigureAwait(false))
            NotificationHelper.Success("Possession Successful", "Enjoy your new body!");
    }

    private async Task Unpossess()
    {
        if (await _possessionManager.Unpossess(true).ConfigureAwait(false))
            NotificationHelper.Success("Unpossess Successful", string.Empty);
    }

    private async Task AcceptPossessionTermsOfService()
    {
        if (await _configurationService.AgreeTo(Agreement.Possession).ConfigureAwait(true) is false)
            NotificationHelper.Warning("Unable to Accept Agreement", "To see more information, please type /xllog to open the developer console");
    }

    private bool MissingPermissionsForATarget()
    {
        foreach (var friend in _selectionManager.Selected)
        {
            if (friend.PermissionsGrantedByFriend is null)
                continue;
            
            if ((friend.PermissionsGrantedByFriend.Elevated & ElevatedPermissions.Possession) is not ElevatedPermissions.Possession)
                return true;
        }

        return false;
    }
}