using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Possession;
using AetherRemoteCommon.Domain.Network.Possession.Begin;
using AetherRemoteCommon.Domain.Network.Possession.Camera;
using AetherRemoteCommon.Domain.Network.Possession.End;
using AetherRemoteCommon.Domain.Network.Possession.Movement;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Hubs;

public partial class PrimaryHub
{
    [HubMethodName(HubMethod.Possession.Begin)]
    public async Task<PossessionBeginResponse> PossessionBegin(PossessionBeginRequest request)
    {
        logger.LogInformation("{Request}", request);
        var friendCode = FriendCode;
        LogWithBehavior($"[PossessionBegin] Sender = {friendCode}, Target = {request.TargetFriendCode}", LogMode.Console);
        return await possessionBeginHandler.Handle(friendCode, request, Clients);
    }
    
    [HubMethodName(HubMethod.Possession.Camera)]
    public async Task<PossessionResponse> PossessionCamera(PossessionCameraRequest request)
    {
        // logger.LogInformation("{Request}", request);
        var friendCode = FriendCode;
        // LogWithBehavior($"[PossessionCameraRequest] Sender = {friendCode}", LogMode.Console);
        return await possessionCameraHandler.Handle(friendCode, request, Clients);
    }
    
    [HubMethodName(HubMethod.Possession.Movement)]
    public async Task<PossessionResponse> PossessionMovement(PossessionMovementRequest request)
    {
        // logger.LogInformation("{Request}", request);
        var friendCode = FriendCode;
        // LogWithBehavior($"[PossessionMovementRequest] Sender = {friendCode}", LogMode.Console);
        return await possessionMovementHandler.Handle(friendCode, request, Clients);
    }
    
    [HubMethodName(HubMethod.Possession.End)]
    public async Task<PossessionResponse> PossessionEnd(PossessionEndRequest request)
    {
        logger.LogInformation("{Request}", request);
        var friendCode = FriendCode;
        LogWithBehavior($"[PossessionEndRequest] Sender = {friendCode}", LogMode.Console);
        return await possessionEndHandler.Handle(friendCode, Clients);
    }
}