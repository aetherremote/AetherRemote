using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Network.Domain;
using AetherRemoteCommon.Network.Domain.Payloads;
using AetherRemoteCommon.Network.Enums;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Hubs;

public partial class PrimaryHub
{
    [HubMethodName(HubMethod.BodySwap)]
    public async Task<Response<BodySwapResponse>> BodySwap(Request<BodySwapPayload> request)
    {
        if (request.Payload.LockCode is not null)
            return new Response<BodySwapResponse>(ResponseStatus.Disabled, []);
        
        return await requestHandler.BodySwapHandler.Execute(FriendCode, request, Clients);
    }
    
    [HubMethodName(HubMethod.Transform)]
    public async Task<Response<NoPayload>> Transform(Request<TransformationPayload> request)
    {
        if (request.Payload.LockCode is not null)
            return new Response<NoPayload>(ResponseStatus.Disabled, []);
        
        return await requestHandler.TransformationHandler.Execute(FriendCode, request, Clients);
    }

    [HubMethodName(HubMethod.Twinning)]
    public async Task<Response<NoPayload>> Twinning(Request<TwinningPayload> request)
    {
        if (request.Payload.LockCode is not null)
            return new Response<NoPayload>(ResponseStatus.Disabled, []);
        
        return await requestHandler.TwinningHandler.Execute(FriendCode, request, Clients);
    }
    
    [HubMethodName(HubMethod.Mimicry)]
    public async Task<Response<MimicryResponse>> Mimicry(Request<MimicryPayload> request)
    {
        if (request.Payload.LockCode is not null)
            return new Response<MimicryResponse>(ResponseStatus.Disabled, []);
        
        return await requestHandler.MimicryHandler.Execute(FriendCode, request, Clients);
    }
}