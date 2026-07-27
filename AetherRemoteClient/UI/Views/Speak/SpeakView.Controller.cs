using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AetherRemoteClient.Domain;
using AetherRemoteCommon;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Network.Domain;
using AetherRemoteCommon.Network.Domain.Payloads;
using AetherRemoteCommon.Util;
using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.Game.Character;

namespace AetherRemoteClient.UI.Views.Speak;

public partial class SpeakView
{
    private readonly string[] _linkshellNumbers = ["1", "2", "3", "4", "5", "6", "7", "8"];
    private readonly ListFilter<string> _worldsListFilter;
    private readonly string[] _chatModeOptions;

    private ChatChannel _channelSelect;
    private int _channelSelectionIndex;
    private int _linkshellSelection;

    private string _characterName = string.Empty;
    private string _worldName = string.Empty;
    private string _message = string.Empty;

    /// <summary>
    ///     Fills the <see cref="_characterName"/> and <see cref="_worldName"/> with local player data
    /// </summary>
    private void FillWithPlayerData()
    {
        if (Plugin.ObjectTable.LocalPlayer is not {} target)
            return;
        
        SetTellTarget(target);
    }
    
    /// <summary>
    ///     Fills the <see cref="_characterName"/> and <see cref="_worldName"/> with target player data
    /// </summary>
    private void FillWithTargetData()
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
        
        _characterName = character->NameString ?? _characterName;
        _worldName = home;
    }

    /// <summary>
    ///     Handles the "send message" button from the Ui
    /// </summary>
    private async Task SendMessage()
    {
        if (_message.Length < Constraints.Speak.MessageMin)
            return;

        var extra = _channelSelect switch
        {
            ChatChannel.Tell => $"{_characterName}@{_worldName}",
            ChatChannel.Linkshell or ChatChannel.CrossWorldLinkshell => (_linkshellSelection + 1).ToString(),
            _ => null
        };

        var targets = _selectionManager.GetSelectedFriendCodes();
        if (targets.Count is 0)
            return;
        
        _commandLockoutService.Lock();
        var payload = new SpeakPayload(_message, _channelSelect, extra);
        await _networkRequestManager.Send<SpeakPayload, NoPayload>(targets, HubMethod.Speak, payload).ConfigureAwait(false);
    }

    private List<string> GetFriendsLackingPermissions()
    {
        var permissions = _channelSelect.ToSpeakPermissions((_linkshellSelection + 1).ToString());
        var thoseWhoYouLackPermissionsFor = new List<string>();
        foreach (var selected in _selectionManager.Selected)
        {
            if (selected.PermissionsGrantedByFriend is null)
                continue;
            
            if ((selected.PermissionsGrantedByFriend.Speak & permissions) != permissions)
                thoseWhoYouLackPermissionsFor.Add(selected.NoteOrFriendCode);
        }
        
        return thoseWhoYouLackPermissionsFor;
    }

    private static bool FilterWorld(string world, string searchTerm) => world.Contains(searchTerm, StringComparison.OrdinalIgnoreCase);
}