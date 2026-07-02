using System.Collections.Generic;
using System.Threading.Tasks;
using AetherRemoteClient.Domain;
using AetherRemoteCommon.Domain.Enums.Permissions;

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

        await _networkCommandManager.SendEmote(_selectionManager.GetSelectedFriendCodes(), _emoteSelection, _displayLogMessage).ConfigureAwait(false);
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