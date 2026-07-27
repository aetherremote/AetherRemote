using System.Collections.Generic;
using System.Threading.Tasks;
using AetherRemoteClient.Domain;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Network.Domain;
using AetherRemoteCommon.Network.Domain.Payloads;
using AetherRemoteCommon.Network.Enums;

namespace AetherRemoteClient.UI.Views.Emote;

public partial class EmoteView
{
    private readonly ListFilter<string> _emotesListFilter;
    private string _emoteSelection = string.Empty;
    private bool _displayLogMessage;

    private static bool FilterEmote(string emote, string searchTerm) => emote.Contains(searchTerm);

    /// <summary>
    ///     Handles the "send button" from the Ui
    /// </summary>
    private async Task Send()
    {
        if (_emoteService.Emotes.Contains(_emoteSelection) is false)
            return;

        var targets = _selectionManager.GetSelectedFriendCodes();
        if (targets.Count is 0)
            return;
        
        _commandLockoutService.Lock();
        var payload = new EmotePayload(_emoteSelection, _displayLogMessage);
        var response = await _networkRequestManager.Send<EmotePayload, NoPayload>(targets, HubMethod.Emote, payload).ConfigureAwait(false);

        if (response.Status is ResponseStatus.Success)
            _emoteSelection = string.Empty;
    }

    /// <summary>
    ///     Calculates the friends who you lack correct permissions to send to
    /// </summary>
    /// <returns></returns>
    private List<string> GetFriendsLackingPermissions()
    {
        var thoseWhoYouLackPermissionsFor = new List<string>();
        foreach (var selected in _selectionManager.Selected)
        {
            if (selected.PermissionsGrantedByFriend is null)
                continue;
            
            if ((selected.PermissionsGrantedByFriend.Primary & PrimaryPermissions.Emote) != PrimaryPermissions.Emote)
                thoseWhoYouLackPermissionsFor.Add(selected.NoteOrFriendCode);
        }
        return thoseWhoYouLackPermissionsFor;
    }
}