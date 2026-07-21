using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Network.Domain;
using AetherRemoteCommon.Network.Domain.Payloads;
using AetherRemoteCommon.Network.Enums;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Hubs;

public partial class PrimaryHub
{
    [HubMethodName(HubMethod.CustomizePlus)]
    public async Task<Response<NoPayload>> CustomizePlus(Request<CustomizePlusPayload> request)
    {
        return await requestHandler.CustomizePlusHandler.Execute(FriendCode, request, Clients);
    }
    
    [HubMethodName(HubMethod.Honorific)]
    public async Task<Response<NoPayload>> Honorific(Request<HonorificPayload> request)
    {
        var friendCode = FriendCode;
        LogWithBehavior($"[HonorificRequest] Sender = {friendCode}, Targets = {string.Join(", ", request.TargetFriendCodes)}, Honorific = {request.Payload.Data.Title}", LogMode.Console);
        return await requestHandler.HonorificHandler.Execute(friendCode, request, Clients);
    }
    
    [HubMethodName(HubMethod.Moodles)]
    public async Task<Response<NoPayload>> Moodles(Request<MoodlesPayload> request)
    {
        return new Response<NoPayload>(ResponseStatus.Disabled, []);
        
        var friendCode = FriendCode;
        LogWithBehavior($"[MoodlesRequest] Sender = {friendCode}, Targets = {string.Join(", ", request.TargetFriendCodes)}, Moodle = {request.Payload.Info.Title}", LogMode.Console);
        return await requestHandler.MoodlesHandler.Execute(friendCode, request, Clients);
    }
}