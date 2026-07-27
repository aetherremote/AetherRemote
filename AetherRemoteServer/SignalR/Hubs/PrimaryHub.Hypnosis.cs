using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Network.Domain;
using AetherRemoteCommon.Network.Domain.Payloads;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Hubs;

public partial class PrimaryHub
{
    [HubMethodName(HubMethod.Hypnosis)]
    public async Task<Response<NoPayload>> Hypnosis(Request<HypnosisPayload> request)
    {
        var friendCode = FriendCode;
        LogWithBehavior($"[HypnosisRequest] Sender = {friendCode}, Targets = {string.Join(", ", request.TargetFriendCodes)}, Words = {string.Join(", ", request.Payload.Data.TextWords)}", LogMode.Both);
        return await requestHandler.HypnosisHandler.Execute(friendCode, request, Clients);
    }
    
    [HubMethodName(HubMethod.HypnosisStop)]
    public async Task<Response<NoPayload>> HypnosisStop(Request<HypnosisStopPayload> request)
    {
        return await requestHandler.HypnosisStopHandler.Execute(FriendCode, request, Clients);
    }
}