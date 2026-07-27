using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Network.Domain.Commands;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Hubs;

public partial class PrimaryHub
{
    [HubMethodName(HubMethod.AddFriend)]
    public async Task<AddFriendResponse> AddFriend(AddFriendRequest request)
    {
        var friendCode = FriendCode;
        LogWithBehavior($"[AddFriendRequest] Sender = {friendCode}, Target = {request.TargetFriendCode}", LogMode.Both);
        return await requestHandler.AddFriendHandler.Execute(friendCode, request, Clients);
    }
    
    [HubMethodName(HubMethod.RemoveFriend)]
    public async Task<RemoveFriendResponse> RemoveFriend(RemoveFriendRequest request)
    {
        var friendCode = FriendCode;
        LogWithBehavior($"[RemoveFriendRequest] Sender = {friendCode}, Target = {request.TargetFriendCode}", LogMode.Both);
        return await requestHandler.RemoveFriendHandler.Execute(friendCode, request, Clients);
    }
    
    [HubMethodName(HubMethod.UpdateFriend)]
    public async Task<UpdateFriendResponse> UpdateFriend(UpdateFriendRequest request)
    {
        var friendCode = FriendCode;
        LogWithBehavior($"[UpdateFriendRequest] Sender = {friendCode}, Target = {request.TargetFriendCode}, Permissions = {request.Permissions}", LogMode.Disk);
        return await requestHandler.UpdateFriendHandler.Execute(friendCode, request, Clients);
    }
    
    [HubMethodName(HubMethod.UpdateGlobalPermissions)]
    public async Task<UpdateGlobalPermissionsResponse> UpdateGlobalPermissions(UpdateGlobalPermissionsRequest request)
    {
        return await requestHandler.UpdateGlobalPermissionsHandlerHandler.Execute(FriendCode, request, Clients);
    }
}