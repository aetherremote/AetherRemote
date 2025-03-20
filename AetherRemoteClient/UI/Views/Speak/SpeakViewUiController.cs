using System;
using System.Collections.Generic;
using System.Linq;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Services;
using AetherRemoteClient.Utils;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Util;
using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.Game.Character;

namespace AetherRemoteClient.UI.Views.Speak;

/// <summary>
///     Handles events from the <see cref="SpeakViewUi"/>
/// </summary>
public class SpeakViewUiController
{
    private readonly FriendsListService _friendsListService;
    private readonly NetworkService _networkService;
    private readonly WorldService _worldService;

    public readonly string[] LinkshellNumbers = ["1", "2", "3", "4", "5", "6", "7", "8"];
    public readonly ListFilter<string> WorldsListFilter;
    public readonly string[] ChatModeOptions;

    public ChatChannel ChannelSelect;
    public int ChannelSelectionIndex;
    public int LinkshellSelection;
    
    public string CharacterName = string.Empty;
    public string WorldName = string.Empty;
    public string Message = string.Empty;

    /// <summary>
    ///     <inheritdoc cref="SpeakViewUiController"/>
    /// </summary>
    public SpeakViewUiController(FriendsListService friendsListService, NetworkService networkService,
        WorldService worldService)
    {
        _friendsListService = friendsListService;
        _networkService = networkService;
        _worldService = worldService;
        
        WorldsListFilter = new ListFilter<string>(worldService.WorldNames, FilterWorld);
        ChatModeOptions =
            (from ChatChannel mode in Enum.GetValues(typeof(ChatChannel)) select mode.Beautify()).ToArray();
    }

    /// <summary>
    ///     Fills the <see cref="CharacterName"/> and <see cref="WorldName"/> with local player data
    /// </summary>
    public void FillWithPlayerData()
    {
        if (Plugin.ClientState.LocalPlayer is not {} target)
            return;
        
        SetTellTarget(target);
    }
    
    /// <summary>
    ///     Fills the <see cref="CharacterName"/> and <see cref="WorldName"/> with target player data
    /// </summary>
    public void FillWithTargetData()
    {
        if (Plugin.TargetManager.Target is not {} target)
            return;
        
        SetTellTarget(target);
    }
    
    private unsafe void SetTellTarget(IGameObject target)
    {
        var character = CharacterManager.Instance()->LookupBattleCharaByEntityId(target.EntityId);
        if (character is null)
            return;
        
        var id = character->HomeWorld;
        var home = _worldService.TryGetWorldById(id);
        if (home is null)
            return;
        
        CharacterName = character->NameString ?? CharacterName;
        WorldName = home;
    }

    /// <summary>
    ///     Handles the "send message" button from the Ui
    /// </summary>
    public async void SendMessage()
    {
        try
        {
            if (Message.Length is 0)
                return;

            var extra = ChannelSelect switch
            {
                ChatChannel.Tell => $"{CharacterName}@{WorldName}",
                ChatChannel.Linkshell or ChatChannel.CrossWorldLinkshell => LinkshellSelection.ToString(),
                _ => null
            };

            var input = new SpeakRequest
            {
                ChatChannel = ChannelSelect, Extra = extra, Message = Message,
                TargetFriendCodes = _friendsListService.Selected.Select(friend => friend.FriendCode).ToList()
            };

            var response = await _networkService.InvokeAsync<SpeakRequest, BaseResponse>(HubMethod.Speak, input);
            if (Plugin.DeveloperMode || response.Success)
            {
                Message = string.Empty;
                Plugin.NotificationManager.AddNotification(NotificationHelper.Success(
                    "Successfully issued speak command", string.Empty));

                if (ChannelSelect is not ChatChannel.Echo)
                    return;
                
                foreach (var friend in _friendsListService.Selected)
                    Plugin.ChatGui.Print($">>{friend.NoteOrFriendCode}: {input.Message}");
            }
            else
            {
                Plugin.NotificationManager.AddNotification(NotificationHelper.Warning(
                    "Unable to issue speak command", response.Message));
            }
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"Unable to send message, {e.Message}");
        }
    }

    public List<string> GetFriendsLackingPermissions()
    {
        var thoseWhoYouLackPermissionsFor = new List<string>();
        foreach (var selected in _friendsListService.Selected)
        {
            if (selected.PermissionsGrantedByFriend.Primary.HasFlag(PrimaryPermissions.Speak) is false)
            {
                thoseWhoYouLackPermissionsFor.Add(selected.NoteOrFriendCode);
                continue;
            }

            switch (ChannelSelect)
            {
                case ChatChannel.Linkshell:
                case ChatChannel.CrossWorldLinkshell:
                    if (PermissionsChecker.Speak(selected.PermissionsGrantedByFriend.Linkshell, LinkshellSelection) is false)
                    {
                        thoseWhoYouLackPermissionsFor.Add(selected.NoteOrFriendCode);
                        continue;
                    }
                    break;
                
                case ChatChannel.Say:
                case ChatChannel.ChatEmote:
                case ChatChannel.Echo:
                case ChatChannel.Yell:
                case ChatChannel.Shout:
                case ChatChannel.Tell:
                case ChatChannel.Party:
                case ChatChannel.Alliance:
                case ChatChannel.FreeCompany:
                case ChatChannel.NoviceNetwork:
                case ChatChannel.PvPTeam:
                default:
                    if (PermissionsChecker.Speak(selected.PermissionsGrantedByFriend.Primary, ChannelSelect) is false)
                    {
                        thoseWhoYouLackPermissionsFor.Add(selected.NoteOrFriendCode);
                        continue;
                    }
                    break;
            }
        }
        
        return thoseWhoYouLackPermissionsFor;
    }

    private static bool FilterWorld(string world, string searchTerm) =>
        world.Contains(searchTerm, StringComparison.OrdinalIgnoreCase);
}