using System.Linq;
using System.Threading.Tasks;
using AetherRemoteClient.Managers;
using AetherRemoteCommon.Domain.Enums.Permissions;
using Dalamud.Utility;

namespace AetherRemoteClient.UI.Views.Possession;

public class PossessionViewUiController(PossessionManager possessions, SelectionManager selectionManager)
{
    public static void OpenFeedbackLink() => Util.OpenLink("https://tenor.com/view/%E3%83%95%E3%83%AD%E3%83%BC%E3%83%A9%E3%82%A4%E3%83%88-flow-endfield-gif-12757389959336405366");
    
    public async Task Possess()
    {
        if (selectionManager.Selected.FirstOrDefault() is not { } friend)
            return;
            
        await possessions.TryBeginPossession(friend.FriendCode).ConfigureAwait(false);
    }

    public async Task Unpossess()
    {
        await possessions.TryEndPossession().ConfigureAwait(false);
    }

    public static async Task AcceptPossessionTermsOfService()
    {
        Plugin.Configuration.AcceptedPossessionAgreement = true;
        await Plugin.Configuration.Save().ConfigureAwait(false);
    }

    public bool MissingPermissionsForATarget()
    {
        foreach (var friend in selectionManager.Selected)
            if ((friend.PermissionsGrantedByFriend.Elevated & ElevatedPermissions.Possession) is not ElevatedPermissions.Possession)
                return true;

        return false;
    }
}