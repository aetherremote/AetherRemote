using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Customize;
using AetherRemoteCommon.Domain.Network.Honorific;
using AetherRemoteCommon.Domain.Network.Moodles;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Hubs;

public partial class PrimaryHub
{
    [HubMethodName(HubMethod.CustomizePlus)]
    public async Task<ActionResponse> CustomizePlus(CustomizeRequest request)
    {
        return await customizePlusHandler.Handle(FriendCode, request, Clients);
    }
    
    [HubMethodName(HubMethod.Honorific)]
    public async Task<ActionResponse> Honorific(HonorificRequest request)
    {
        var friendCode = FriendCode;
        LogWithBehavior($"[HonorificRequest] Sender = {friendCode}, Honorific = {request.Honorific}", LogMode.Console);
        return await honorificHandler.Handle(friendCode, request, Clients);
    }
    
    [HubMethodName(HubMethod.Moodles)]
    public async Task<ActionResponse> GetMoodlesAction(MoodlesRequest request)
    {
        return await moodlesHandler.Handle(FriendCode, request, Clients);
    }
}