using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.AddFriend;
using AetherRemoteCommon.Domain.Network.RemoveFriend;
using AetherRemoteCommon.Domain.Network.UpdateFriend;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Hubs;

public partial class PrimaryHub
{
    [HubMethodName(HubMethod.AddFriend)]
    public async Task<AddFriendResponse> AddFriend(AddFriendRequest request)
    {
        var friendCode = FriendCode;
        LogWithBehavior($"[AddFriendRequest] Sender = {friendCode}, Target = {request.TargetFriendCode}", LogMode.Both);
        return await requestHandler.HandleAddFriend(friendCode, request, Clients);
    }
    
    [HubMethodName(HubMethod.RemoveFriend)]
    public async Task<RemoveFriendResponse> RemoveFriend(RemoveFriendRequest request)
    {
        var friendCode = FriendCode;
        LogWithBehavior($"[RemoveFriendRequest] Sender = {friendCode}, Target = {request.TargetFriendCode}", LogMode.Both);
        return await requestHandler.HandleRemoveFriend(friendCode, request, Clients);
    }
    
    [HubMethodName(HubMethod.UpdateFriend)]
    public async Task<UpdateFriendResponse> UpdateFriend(UpdateFriendRequest request)
    {
        var friendCode = FriendCode;
        LogWithBehavior($"[UpdateFriendRequest] Sender = {friendCode}, Target = {request.TargetFriendCode}, Permissions = {request.Permissions}", LogMode.Disk);
        return await requestHandler.HandleUpdateFriend(friendCode, request, Clients);
    }
}