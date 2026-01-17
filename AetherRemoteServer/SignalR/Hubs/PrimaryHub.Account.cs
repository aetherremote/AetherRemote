using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.GetAccountData;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Hubs;

public partial class PrimaryHub
{
    [HubMethodName(HubMethod.GetAccountData)]
    public async Task<GetAccountDataResponse> GetAccountData(GetAccountDataRequest request)
    {
        return await getAccountDataHandler.Handle(FriendCode, Context.ConnectionId, request);
    }
}