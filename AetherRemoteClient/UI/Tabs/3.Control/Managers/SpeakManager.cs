using System.Linq;
using System.Threading.Tasks;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Log;
using AetherRemoteClient.Providers;
using AetherRemoteCommon;
using AetherRemoteCommon.Domain.CommonChatMode;
using AetherRemoteCommon.Domain.Network.Commands;
using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.Game.Character;

namespace AetherRemoteClient.UI.Tabs.Managers;

public class SpeakManager(
    ClientDataManager clientDataManager,
    CommandLockoutManager commandLockoutManager,
    HistoryLogManager historyLogManager,
    NetworkProvider networkProvider,
    WorldProvider worldProvider)
{
    // Variables - Speak
    public ChatMode ChatMode = ChatMode.Say;
    public int LinkshellNumber = 1;
    public string Message = "";
    public int UseEmoteInsteadOfSay;
    public string TellTargetName = "";
    public string TellTargetWorld = "";
    
    public async Task SendSpeak()
    {
        if (Plugin.DeveloperMode) return;
        if (clientDataManager.TargetManager.Targets.Keys.Count > Constraints.MaximumTargetsForInGameOperations) return;
        if (string.IsNullOrEmpty(Message)) return;
        
        var extra = ChatMode switch
        {
            ChatMode.Linkshell or ChatMode.CrossWorldLinkshell => LinkshellNumber.ToString(),
            ChatMode.Tell => $"{TellTargetName}@{TellTargetWorld}",
            ChatMode.Say => UseEmoteInsteadOfSay.ToString(),
            _ => null
        };

        commandLockoutManager.Lock();

        var targets = clientDataManager.TargetManager.Targets.Keys.ToList();
        var request = new SpeakRequest(targets, Message, ChatMode, extra);
        var result = await networkProvider.InvokeCommand<SpeakRequest, SpeakResponse>(Network.Commands.Speak, request);
        if (result.Success)
        {
            var targetNames = string.Join(", ", targets);
            var logMessage = ChatMode switch
            {
                ChatMode.Linkshell => $"You issued {targetNames} to say \"{Message}\" in LS{extra}.",
                ChatMode.CrossWorldLinkshell => $"You issued {targetNames} to say \"{Message}\" in CWL{extra}.",
                ChatMode.Tell => $"You issued {targetNames} to say \"{Message}\" in a tell to {extra}",
                _ => $"You issued {targetNames} to say \"{Message}\" in {ChatMode.Beautify()} chat",
            };

            Plugin.Log.Information(logMessage);
            historyLogManager.LogHistory(logMessage);
            Message = string.Empty;
        }
        else
        {
            Plugin.Log.Warning($"Issuing speak command unsuccessful: {result.Message}");
        }
    }
    
    public unsafe void SetTellTargetFor(IGameObject? target)
    {
        if (target is null) return;

        var character = CharacterManager.Instance()->LookupBattleCharaByEntityId(target.EntityId);
        if (character is null) return;

        var homeWorldId = character->HomeWorld;
        var homeWorld = worldProvider.TryGetWorldById(homeWorldId);
        if (homeWorld is null) return;

        TellTargetName = character->NameString ?? TellTargetName;
        TellTargetWorld = homeWorld;
    }
}