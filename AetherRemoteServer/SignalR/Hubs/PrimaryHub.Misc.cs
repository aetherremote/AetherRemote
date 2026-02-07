using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Customize;
using AetherRemoteCommon.Domain.Network.Honorific;
using AetherRemoteCommon.Domain.Network.Moodles;
using AetherRemoteCommon.Domain.Network.UpdateGlobalPermissions;
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
        LogWithBehavior($"[HonorificRequest] Sender = {friendCode}, Targets = {string.Join(", ", request.TargetFriendCodes)}, Honorific = {request.Honorific}", LogMode.Console);
        return await honorificHandler.Handle(friendCode, request, Clients);
    }
    
    [HubMethodName(HubMethod.Moodles)]
    public async Task<ActionResponse> Moodles(MoodlesRequest request)
    {
        var friendCode = FriendCode;
        LogWithBehavior($"[MoodlesRequest] Sender = {friendCode}, Targets = {string.Join(", ", request.TargetFriendCodes)}, Moodle = {request.Info.Title}", LogMode.Console);
        return await moodlesHandler.Handle(friendCode, request, Clients);
    }
    
    [HubMethodName(HubMethod.UpdateGlobalPermissions)]
    public async Task<ActionResponseEc> UpdateGlobalPermissions(UpdateGlobalPermissionsRequest request)
    {
        return await updateGlobalPermissionsHandler.Handle(FriendCode, request, Clients);
    }
}