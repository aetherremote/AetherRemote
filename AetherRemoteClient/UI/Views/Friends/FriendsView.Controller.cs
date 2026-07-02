using System.Linq;
using System.Threading.Tasks;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Utils;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.RemoveFriend;
using AetherRemoteCommon.Domain.Network.UpdateFriend;
using AetherRemoteCommon.Domain.Network.UpdateGlobalPermissions;

namespace AetherRemoteClient.UI.Views.Friends;

public partial class FriendsView
{
    // Instantiated
    private IndividualPermissions _individual = new();
    private GlobalPermissions _global = new();

    private bool PendingChangesIndividual()
    {
        if (_selectionManager.Selected.FirstOrDefault() is not { } friend)
            return false;
        
        // TODO: This should definitely be cached...
        return _individual.IsEqualTo(IndividualPermissions.From(friend.PermissionsGrantedToFriend)) is false;
    }

    private bool PendingChangesGlobal()
    {
        // TODO: This should definitely be cached...
        return _global.IsEqualTo(GlobalPermissions.From(_accountService.GlobalPermissions)) is false;
    }

    /// <summary>
    ///     Sends a request to the server to save the global permissions
    /// </summary>
    private async Task SaveGlobalPermissions()
    {
        var resolved = GlobalPermissions.To(_global);
        var request = new UpdateGlobalPermissionsRequest(resolved);
        var response = await _networkService.InvokeAsync<ActionResponseEc>(HubMethod.UpdateGlobalPermissions, request).ConfigureAwait(false);
        if (response is not ActionResponseEc.Success)
        {
            NotificationHelper.Error("Updating Global Permissions Failed", "This should never happen, report this to a developer.");
            Plugin.Log.Warning("[FriendsViewUiController.SaveGlobalPermissions] Unsuccessful");
            return;
        }
        
        NotificationHelper.Success("Successfully Updated Global Permissions", string.Empty);
        _accountService.SetGlobalPermissions(resolved);
    }

    /// <summary>
    ///     Sends a request to the server to save an individual permission
    /// </summary>
    private async Task SaveIndividualPermissions()
    {
        // Only save if it's one person selected
        if (_selectionManager.Selected.Count is not 1 || _selectionManager.Selected.FirstOrDefault() is not { } friend)
            return;

        // Set the note
        if (_individual.Note == string.Empty)
        {
            friend.Note = null;
            await _notesService.RemoveNote(friend.FriendCode).ConfigureAwait(false);
        }
        else
        {
            friend.Note = _individual.Note;
            await _notesService.AddNote(friend.FriendCode, friend.Note).ConfigureAwait(false);
        }

        // Construct the request and send it
        var raw = IndividualPermissions.To(_individual);
        var request = new UpdateFriendRequest(friend.FriendCode, raw);
        var response = await _networkService.InvokeAsync<UpdateFriendResponse>(HubMethod.UpdateFriend, request).ConfigureAwait(false);
        if (response.Result is not UpdateFriendEc.Success)
        {
            NotificationHelper.Error("Updating Individual Permissions Failed", "This should never happen, report this to a developer.");
            Plugin.Log.Warning("[FriendsViewUiController.SaveGlobalPermissions] Unsuccessful");
            return;
        }
        
        NotificationHelper.Success("Successfully Updated Individual Permissions", string.Empty);
        friend.PermissionsGrantedToFriend = raw;
    }

    /// <summary>
    ///     Also known as 'unfriending' someone
    /// </summary>
    private async Task DeleteIndividualPermissions()
    {
        // Only delete if it's one person selected
        if (_selectionManager.Selected.Count is not 1 || _selectionManager.Selected.FirstOrDefault() is not { } friend)
            return;
        
        var request = new RemoveFriendRequest(friend.FriendCode);
        var response = await _networkService.InvokeAsync<RemoveFriendResponse>(HubMethod.RemoveFriend, request).ConfigureAwait(false);
        switch (response.Result)
        {
            case RemoveFriendEc.Success:
                NotificationHelper.Success("Successfully Removed Friend", string.Empty);
                _friendsListService.Delete(friend);
                return;
            
            case RemoveFriendEc.NotFriends:
                NotificationHelper.Error("Remove Friend Failed", "You cannot remove a friend you were not friends with in the first place.");
                break;

            case RemoveFriendEc.Uninitialized:
            case RemoveFriendEc.Unknown:
            default:
                NotificationHelper.Error("Remove Friend Failed", $"This should never happen, report this to a developer. Error Code {response.Result}");
                break;
        }

        // Switch case for success will exit early, leaving only the failure cases to print this message
        Plugin.Log.Warning($"[FriendsViewUiController.DeleteIndividualPermissions] Unsuccessful {response.Result}");
    }

    /// <summary>
    ///     Handle when the global permissions are updated
    /// </summary>
    private Task OnGlobalPermissionsUpdated(ResolvedPermissions globalPermissions)
    {
        _global = GlobalPermissions.From(globalPermissions);
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Handle when a new friend is selected
    /// </summary>
    private void OnFriendSelected(object? sender, Friend e)
    {
        if (_selectionManager.Selected.FirstOrDefault() is not { } friend)
            return;

        _individual = IndividualPermissions.From(friend.PermissionsGrantedToFriend);
        _individual.Note = friend.Note ?? string.Empty;
    }
}