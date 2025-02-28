using System;
using System.Collections.Generic;
using System.Linq;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Services;
using AetherRemoteClient.Utils;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network;

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

            var input = new EmoteRequest
            {
                DisplayLogMessage = DisplayLogMessage,
                Emote = EmoteSelection,
                TargetFriendCodes = friendsListService.Selected.Select(friend => friend.FriendCode).ToList()
            };
        
            var response = await networkService.InvokeAsync<EmoteRequest, BaseResponse>(HubMethod.Emote, input).ConfigureAwait(false);
            if (Plugin.DeveloperMode || response.Success)
            {
                EmoteSelection = string.Empty;
                Plugin.NotificationManager.AddNotification(NotificationHelper.Success(
                    "Successfully issued emote command", string.Empty));
            }
            else
            {
                Plugin.NotificationManager.AddNotification(NotificationHelper.Warning(
                    "Unable to issue emote command", response.Message));
            }
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
            if (selected.PermissionsGrantedByFriend.Primary.HasFlag(PrimaryPermissions.Emote) is false)
                thoseWhoYouLackPermissionsFor.Add(selected.NoteOrFriendCode);
        }
        return thoseWhoYouLackPermissionsFor;
    }
}