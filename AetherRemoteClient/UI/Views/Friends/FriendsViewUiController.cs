using System;
using System.Linq;
using System.Threading.Tasks;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteClient.Utils;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.RemoveFriend;
using AetherRemoteCommon.Domain.Network.UpdateFriend;
using AetherRemoteCommon.Domain.Network.UpdateGlobalPermissions;

namespace AetherRemoteClient.UI.Views.Friends;

public class FriendsViewUiController : IDisposable
{
    // Injected
    private readonly AccountService _account;
    private readonly FriendsListService _friends;
    private readonly NetworkService _network;
    private readonly SelectionManager _selection;
    
    // Instantiated
    public IndividualPermissions Individual = new();
    public GlobalPermissions Global = new();

    public bool PendingChangesIndividual()
    {
        if (_selection.Selected.FirstOrDefault() is not { } friend)
            return false;
        
        // TODO: This should definitely be cached...
        return Individual.IsEqualTo(IndividualPermissions.From(friend.PermissionsGrantedToFriend)) is false;
    }
    
    public bool PendingChangesGlobal()
    {
        // TODO: This should definitely be cached...
        return Global.IsEqualTo(GlobalPermissions.From(_account.GlobalPermissions)) is false;
    }

    public FriendsViewUiController(AccountService account, FriendsListService friends, NetworkService network, SelectionManager selection)
    {
        _account = account;
        _friends = friends;
        _network = network;
        _selection = selection;

        _account.GlobalPermissionsUpdated += OnGlobalPermissionsUpdated;
        _selection.FriendSelected += OnFriendSelected;
    }

    /// <summary>
    ///     Sends a request to the server to save the global permissions
    /// </summary>
    public async Task SaveGlobalPermissions()
    {
        var resolved = GlobalPermissions.To(Global);
        var request = new UpdateGlobalPermissionsRequest(resolved);
        var response = await _network.InvokeAsync<ActionResponseEc>(HubMethod.UpdateGlobalPermissions, request).ConfigureAwait(false);
        if (response is not ActionResponseEc.Success)
        {
            NotificationHelper.Error("Updating Global Permissions Failed", "This should never happen, report this to a developer.");
            Plugin.Log.Warning("[FriendsViewUiController.SaveGlobalPermissions] Unsuccessful");
            return;
        }
        
        NotificationHelper.Success("Successfully Updated Global Permissions", string.Empty);
        _account.SetGlobalPermissions(resolved);
    }

    /// <summary>
    ///     Sends a request to the server to save an individual permission
    /// </summary>
    public async Task SaveIndividualPermissions()
    {
        // Only save if it's one person selected
        if (_selection.Selected.Count is not 1 || _selection.Selected.FirstOrDefault() is not { } friend)
            return;

        // Set the note
        friend.Note = Individual.Note == string.Empty ? null : Individual.Note;

        // Construct the request and send it
        var raw = IndividualPermissions.To(Individual);
        var request = new UpdateFriendRequest(friend.FriendCode, raw);
        var response = await _network.InvokeAsync<UpdateFriendResponse>(HubMethod.UpdateFriend, request).ConfigureAwait(false);
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
    public async Task DeleteIndividualPermissions()
    {
        // Only delete if it's one person selected
        if (_selection.Selected.Count is not 1 || _selection.Selected.FirstOrDefault() is not { } friend)
            return;
        
        var request = new RemoveFriendRequest(friend.FriendCode);
        var response = await _network.InvokeAsync<RemoveFriendResponse>(HubMethod.RemoveFriend, request).ConfigureAwait(false);
        switch (response.Result)
        {
            case RemoveFriendEc.Success:
                NotificationHelper.Success("Successfully Removed Friend", string.Empty);
                _friends.Delete(friend);
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
        Global = GlobalPermissions.From(globalPermissions);
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Handle when a new friend is selected
    /// </summary>
    private void OnFriendSelected(object? sender, Friend e)
    {
        if (_selection.Selected.FirstOrDefault() is not { } friend)
            return;

        Individual = IndividualPermissions.From(friend.PermissionsGrantedToFriend);
        Individual.Note = friend.Note ?? string.Empty;
    }

    public void Dispose()
    {
        _account.GlobalPermissionsUpdated -= OnGlobalPermissionsUpdated;
        _selection.FriendSelected -= OnFriendSelected;
        GC.SuppressFinalize(this);
    }
}