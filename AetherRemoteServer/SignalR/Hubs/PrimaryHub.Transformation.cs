using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.BodySwap;
using AetherRemoteCommon.Domain.Network.Transform;
using AetherRemoteCommon.Domain.Network.Twinning;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Hubs;

public partial class PrimaryHub
{
    [HubMethodName(HubMethod.BodySwap)]
    public async Task<ActionResponse> BodySwap(BodySwapRequest request)
    {
        if (request.LockCode is not null)
            return new ActionResponse(ActionResponseEc.Disabled, []);
        
        return await bodySwapHandler.Handle(FriendCode, request, Clients);
    }
    
    [HubMethodName(HubMethod.Transform)]
    public async Task<ActionResponse> Transform(TransformRequest request)
    {
        if (request.LockCode is not null)
            return new ActionResponse(ActionResponseEc.Disabled, []);
        
        return await transformHandler.Handle(FriendCode, request, Clients);
    }

    [HubMethodName(HubMethod.Twinning)]
    public async Task<ActionResponse> Twinning(TwinningRequest request)
    {
        if (request.LockCode is not null)
            return new ActionResponse(ActionResponseEc.Disabled, []);
        
        return await twinningHandler.Handle(FriendCode, request, Clients);
    }
}