using System.Linq;
using System.Threading.Tasks;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Log;
using AetherRemoteClient.Providers;
using AetherRemoteCommon;
using AetherRemoteCommon.Domain.Network.Commands;

namespace AetherRemoteClient.UI.Tabs.Managers;

public class EmoteManager(
    ClientDataManager clientDataManager, 
    CommandLockoutManager commandLockoutManager,
    EmoteProvider emoteProvider, 
    HistoryLogManager historyLogManager, 
    NetworkProvider networkProvider)
{
    // Variables - Emote
    public string Emote = "";
    public bool SendLogMessage;
    
    public async Task SendEmote()
    {
        if (clientDataManager.TargetManager.Targets.Count > Constraints.MaximumTargetsForInGameOperations) return;
        if ((Emote.Length > 0 && emoteProvider.ValidEmote(Emote)) == false) return;
        
        commandLockoutManager.Lock();

        var targets = clientDataManager.TargetManager.Targets.Keys.ToList();
        var request = new EmoteRequest(targets, Emote, SendLogMessage);
        var result = await networkProvider.InvokeCommand<EmoteRequest, EmoteResponse>(Network.Commands.Emote, request);
        if (result.Success)
        {
            var message = $"You issued {string.Join(", ", targets)} to do the {Emote} emote";
            Plugin.Log.Information(message);
            historyLogManager.LogHistory(message);
        }
        else
        {
            Plugin.Log.Warning($"Issuing emote command unsuccessful: {result.Message}");
        }
    }
}