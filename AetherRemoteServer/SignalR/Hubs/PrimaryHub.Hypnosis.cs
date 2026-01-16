using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Hypnosis;
using AetherRemoteCommon.Domain.Network.HypnosisStop;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Hubs;

public partial class PrimaryHub
{
    [HubMethodName(HubMethod.Hypnosis)]
    public async Task<ActionResponse> Hypnosis(HypnosisRequest request)
    {
        var friendCode = FriendCode;
        LogWithBehavior($"[HypnosisRequest] Sender = {friendCode}, Targets = {string.Join(", ", request.TargetFriendCodes)}, Words = {string.Join(", ", request.Data.TextWords)}", LogMode.Both);
        return await hypnosisHandler.Handle(friendCode, request, Clients);
    }
    
    [HubMethodName(HubMethod.HypnosisStop)]
    public async Task<ActionResponse> HypnosisStop(HypnosisStopRequest request)
    {
        return await hypnosisStopHandler.Handle(FriendCode, request, Clients);
    }
}