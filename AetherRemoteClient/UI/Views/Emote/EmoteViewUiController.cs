using System;
using System.Collections.Generic;
using System.Linq;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Services;
using AetherRemoteClient.Utils;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Emote;
using AetherRemoteCommon.V2.Domain.Network;

namespace AetherRemoteClient.UI.Views.Emote;

/// <summary>
///     Handles events from the <see cref="EmoteViewUi"/>
/// </summary>
public class EmoteViewUiController(EmoteService emoteService, FriendsListService friendsListService, NetworkService networkService)
{
    public readonly ListFilter<string> EmotesListFilter = new(emoteService.Emotes, FilterEmote);
    public string EmoteSelection = string.Empty;
    public bool DisplayLogMessage = false;

    private static bool FilterEmote(string emote, string searchTerm) => emote.Contains(searchTerm);

    /// <summary>
    ///     Handles the "send button" from the Ui
    /// </summary>
    public async void Send()
    {
        try
        {
            if (emoteService.Emotes.Contains(EmoteSelection) is false)
                return;

            var request = new EmoteRequest(friendsListService.SelectedFriendCodes, EmoteSelection, DisplayLogMessage);
            var response = await networkService.InvokeAsync<ActionResponse>(HubMethod.Emote, request).ConfigureAwait(false);
            if (response.Result is ActionResponseEc.Success)
                EmoteSelection = string.Empty;
            
            ActionResponseParser.Parse("Emote", response);
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"Unable to emote, {e.Message}");
        }
    }

    /// <summary>
    ///     Calculates the friends who you lack correct permissions to send to
    /// </summary>
    /// <returns></returns>
    public List<string> GetFriendsLackingPermissions()
    {
        var thoseWhoYouLackPermissionsFor = new List<string>();
        foreach (var selected in friendsListService.Selected)
        {
            if ((selected.PermissionsGrantedByFriend.Primary & PrimaryPermissions2.Emote) != PrimaryPermissions2.Emote)
                thoseWhoYouLackPermissionsFor.Add(selected.NoteOrFriendCode);
        }
        return thoseWhoYouLackPermissionsFor;
    }
}