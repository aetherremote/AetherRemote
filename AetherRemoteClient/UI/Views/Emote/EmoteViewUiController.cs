using System.Collections.Generic;
using System.Threading.Tasks;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteCommon.Domain.Enums.Permissions;

namespace AetherRemoteClient.UI.Views.Emote;

/// <summary>
///     Handles events from the <see cref="EmoteViewUi"/>
/// </summary>
public class EmoteViewUiController(EmoteService emoteService, NetworkCommandManager networkCommandManager, SelectionManager selectionManager)
{
    public readonly ListFilter<string> EmotesListFilter = new(emoteService.Emotes, FilterEmote);
    public string EmoteSelection = string.Empty;
    public bool DisplayLogMessage = false;

    private static bool FilterEmote(string emote, string searchTerm) => emote.Contains(searchTerm);

    /// <summary>
    ///     Handles the "send button" from the Ui
    /// </summary>
    public async Task Send()
    {
        if (emoteService.Emotes.Contains(EmoteSelection) is false)
            return;

        await networkCommandManager.SendEmote(selectionManager.GetSelectedFriendCodes(), EmoteSelection, DisplayLogMessage).ConfigureAwait(false);
        EmoteSelection = string.Empty;
    }

    /// <summary>
    ///     Calculates the friends who you lack correct permissions to send to
    /// </summary>
    /// <returns></returns>
    public List<string> GetFriendsLackingPermissions()
    {
        var thoseWhoYouLackPermissionsFor = new List<string>();
        foreach (var selected in selectionManager.Selected)
        {
            if (selected.PermissionsGrantedByFriend is null)
                continue;
            
            if ((selected.PermissionsGrantedByFriend.Primary & PrimaryPermissions.Emote) != PrimaryPermissions.Emote)
                thoseWhoYouLackPermissionsFor.Add(selected.NoteOrFriendCode);
        }
        return thoseWhoYouLackPermissionsFor;
    }
}