using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Network.Domain.Commands;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Hubs;

public partial class PrimaryHub
{
    [HubMethodName(HubMethod.GetAccountData)]
    public async Task<GetAccountDataResponse> GetAccountData(GetAccountDataRequest request)
    {
        return await requestHandler.GetAccountDataHandler.Execute(FriendCode, Context.ConnectionId, request);
    }
}