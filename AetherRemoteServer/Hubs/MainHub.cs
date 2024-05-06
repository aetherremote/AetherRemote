using AetherRemoteCommon;
using AetherRemoteCommon.Domain.Network.Become;
using AetherRemoteCommon.Domain.Network.CreateOrUpdateFriend;
using AetherRemoteCommon.Domain.Network.DeleteFriend;
using AetherRemoteCommon.Domain.Network.Emote;
using AetherRemoteCommon.Domain.Network.Login;
using AetherRemoteCommon.Domain.Network.Speak;
using AetherRemoteServer.Services;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.Hubs;

public class MainHub : Hub
{
    public static readonly NetworkService NetworkService = new();

    [HubMethodName(Constants.ApiLogin)]
    public LoginResponse Login(LoginRequest request)
    {
        var connectionId = Context.ConnectionId;
        var result = NetworkService.Login(connectionId, request.Secret);
        return new LoginResponse(result.Success, result.Message, result.FriendCode, result.FriendList);
    }

    [HubMethodName(Constants.ApiCreateOrUpdateFriend)]
    public CreateOrUpdateFriendResponse CreateOrUpdateFriend(CreateOrUpdateFriendRequest request)
    {
        var result = NetworkService.CreateOrUpdateFriend(request.Secret, request.Friend);
        return new CreateOrUpdateFriendResponse(result.Success, result.Message, result.Online);
    }

    [HubMethodName(Constants.ApiDeleteFriend)]
    public DeleteFriendResponse DeleteFriend(DeleteFriendRequest request)
    {
        var result = NetworkService.DeleteFriend(request.Secret, request.FriendCode);
        return new DeleteFriendResponse(result.Success, result.Message);
    }

    [HubMethodName(Constants.ApiBecome)]
    public BecomeResponse Become(BecomeRequest request)
    {
        var result = NetworkService.Become(request.Secret, request.TargetFriendCodes, request.GlamourerApplyType, request.GlamourerData, Clients);
        return new BecomeResponse(result.Success, result.Message);
    }

    [HubMethodName(Constants.ApiEmote)]
    public EmoteResponse Emote(EmoteRequest request)
    {
        var result = NetworkService.Emote(request.Secret, request.TargetFriendCodes, request.Emote, Clients);
        return new EmoteResponse(result.Success, result.Message);
    }

    [HubMethodName(Constants.ApiSpeak)]
    public SpeakResponse Speak(SpeakRequest request)
    {
        var result = NetworkService.Speak(request.Secret, request.TargetFriendCodes, request.Message, request.ChatMode, request.Extra, Clients);
        return new SpeakResponse(result.Success, result.Message);
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        // Clean up the disconnected client
        NetworkService.Logout(Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }
}
