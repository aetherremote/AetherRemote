using System;
using System.Linq;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Events;
using AetherRemoteClient.Services;
using AetherRemoteClient.UI.Components.Friends;
using AetherRemoteClient.Utils;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.V2.Domain.Network.RemoveFriend;
using AetherRemoteCommon.V2.Domain.Network.UpdateFriend;

namespace AetherRemoteClient.UI.Views.Friends;

/// <summary>
///     Handles events and other tasks for <see cref="FriendsViewUi" />
/// </summary>
public class FriendsViewUiController : IDisposable
{
    // Injected
    private readonly FriendsListService _friendsListService;
    private readonly NetworkService _networkService;

    // Instantiated
    private Friend? _friendBeingEdited;
    private BooleanUserPermissions2 _friendBeingEditedUserPermissionsOriginal = new();

    /// <summary>
    ///     Friend Code is display
    /// </summary>
    public string FriendCode = string.Empty;
    
    /// <summary>
    ///     Note to display
    /// </summary>
    public string Note = string.Empty;
    
    /// <summary>
    ///     The current friend whose permissions you are editing
    /// </summary>
    public BooleanUserPermissions2 EditingUserPermissions = new();

    /// <summary>
    ///     <inheritdoc cref="FriendsViewUiController" />
    /// </summary>
    public FriendsViewUiController(FriendsListService friendsListService, NetworkService networkService)
    {
        _networkService = networkService;
        _friendsListService = friendsListService;
        _friendsListService.SelectedChangedEvent += OnSelectedChangedEvent;
    }

    /// <summary>
    ///     Handles the Save Button from the UI
    /// </summary>
    public async void Save()
    {
        try
        {
            if (_friendBeingEdited is null)
                return;

            if (Note == string.Empty)
                Plugin.Configuration.Notes.Remove(FriendCode);
            else
                Plugin.Configuration.Notes[FriendCode] = Note;

            Plugin.Configuration.Save();

            if (PendingChanges() is false)
                return;

            var permissions = BooleanUserPermissions2.To(EditingUserPermissions);

            var input = new UpdateFriendRequest
            {
                TargetFriendCode = FriendCode,
                Permissions = permissions
            };

            var response = await _networkService.InvokeAsync<UpdateFriendResponse>(HubMethod.UpdateFriend, input).ConfigureAwait(false);
            if (response.Result is UpdateFriendEc.Success)
            {
                _friendBeingEdited.Note = Note == string.Empty ? null : Note;
                _friendBeingEdited.PermissionsGrantedToFriend = permissions;
                _friendBeingEditedUserPermissionsOriginal = BooleanUserPermissions2.From(permissions);
                
                NotificationHelper.Success("Successfully saved friend", string.Empty);
            }
            else
            {
                NotificationHelper.Warning("Unable to save friend", string.Empty);
            }
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"Unable to save friend, {e.Message}");
        }
    }

    /// <summary>
    ///     Handles the Delete Button from the UI
    /// </summary>
    public async void Delete()
    {
        try
        {
            if (_friendBeingEdited is null)
                return;

            var input = new RemoveFriendRequest { TargetFriendCode = FriendCode };
            var response = await _networkService.InvokeAsync<RemoveFriendResponse>(HubMethod.RemoveFriend, input);
            if (response.Result is RemoveFriendEc.Success)
            {
                _friendsListService.Delete(_friendBeingEdited);
                _friendBeingEdited = null;
                
                NotificationHelper.Success("Successfully deleted friend", string.Empty);
            }
            else
            {
                NotificationHelper.Warning("Unable to delete friend", string.Empty);
            }
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"Unable to delete friend, {e.Message}");
        }
    }
    
    /// <summary>
    ///     Checks to see if there are pending changes to the friend you are currently editing
    /// </summary>
    public bool PendingChanges()
    {
        if (_friendBeingEdited is null)
            return false;

        if (EditingUserPermissions.Equals(_friendBeingEditedUserPermissionsOriginal) is false)
            return true;
        
        var note = Note == string.Empty ? null : Note;
        return note != _friendBeingEdited.Note;
    }

    /// <summary>
    ///     TODO
    /// </summary>
    public void SetAllSpeakPermissions(bool allowed)
    {
        EditingUserPermissions.Say = allowed;
        EditingUserPermissions.Yell = allowed;
        EditingUserPermissions.Shout = allowed;
        EditingUserPermissions.Tell = allowed;
        EditingUserPermissions.Party = allowed;
        EditingUserPermissions.Alliance = allowed;
        EditingUserPermissions.FreeCompany = allowed;
        EditingUserPermissions.PvPTeam = allowed;
        EditingUserPermissions.Echo = allowed;
        EditingUserPermissions.Roleplay = allowed;
        EditingUserPermissions.Ls1 = allowed;
        EditingUserPermissions.Ls2 = allowed;
        EditingUserPermissions.Ls3 = allowed;
        EditingUserPermissions.Ls4 = allowed;
        EditingUserPermissions.Ls5 = allowed;
        EditingUserPermissions.Ls6 = allowed;
        EditingUserPermissions.Ls7 = allowed;
        EditingUserPermissions.Ls8 = allowed;
        EditingUserPermissions.Cwl1 = allowed;
        EditingUserPermissions.Cwl2 = allowed;
        EditingUserPermissions.Cwl3 = allowed;
        EditingUserPermissions.Cwl4 = allowed;
        EditingUserPermissions.Cwl5 = allowed;
        EditingUserPermissions.Cwl6 = allowed;
        EditingUserPermissions.Cwl7 = allowed;
        EditingUserPermissions.Cwl8 = allowed;
    }

    /// <summary>
    ///     Handles event fired from <see cref="FriendsListComponentUiController" />
    /// </summary>
    private void OnSelectedChangedEvent(object? sender, SelectedChangedEventArgs e)
    {
        if (e.Selected.Count is not 1)
            return;

        _friendBeingEdited = e.Selected.First();
        _friendBeingEditedUserPermissionsOriginal = BooleanUserPermissions2.From(_friendBeingEdited.PermissionsGrantedToFriend);

        FriendCode = _friendBeingEdited.FriendCode;
        Note = _friendBeingEdited.Note ?? string.Empty;
        EditingUserPermissions = BooleanUserPermissions2.From(_friendBeingEdited.PermissionsGrantedToFriend);
    }

    public void Dispose()
    {
        _friendsListService.SelectedChangedEvent -= OnSelectedChangedEvent;
        GC.SuppressFinalize(this);
    }
}