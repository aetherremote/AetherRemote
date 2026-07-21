using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Network.Domain;
using AetherRemoteCommon.Network.Domain.Payloads;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Hubs;

public partial class PrimaryHub
{
    [HubMethodName(HubMethod.Speak)]
    public async Task<Response<NoPayload>> Speak(Request<SpeakPayload> request)
    {
        var friendCode = FriendCode;
        LogWithBehavior($"[SpeakRequest] Sender = {friendCode}, Targets = {string.Join(", ", request.TargetFriendCodes)}, Message = {request.Payload.Message}", LogMode.Both);
        return await requestHandler.SpeakHandler.Execute(friendCode, request, Clients);
    }
    
    [HubMethodName(HubMethod.Emote)]
    public async Task<Response<NoPayload>> Emote(Request<EmotePayload> request)
    {
        return await requestHandler.EmoteHandler.Execute(FriendCode, request, Clients);
    }
}