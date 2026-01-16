using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Emote;
using AetherRemoteCommon.Domain.Network.Speak;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Hubs;

public partial class PrimaryHub
{
    [HubMethodName(HubMethod.Speak)]
    public async Task<ActionResponse> Speak(SpeakRequest request)
    {
        var friendCode = FriendCode;
        LogWithBehavior($"[SpeakRequest] Sender = {friendCode}, Targets = {string.Join(", ", request.TargetFriendCodes)}, Message = {request.Message}", LogMode.Both);
        return await speakHandler.Handle(friendCode, request, Clients);
    }
    
    [HubMethodName(HubMethod.Emote)]
    public async Task<ActionResponse> Emote(EmoteRequest request)
    {
        return await emoteHandler.Handle(FriendCode, request, Clients);
    }
}