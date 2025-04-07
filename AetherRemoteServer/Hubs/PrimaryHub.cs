using AetherRemoteCommon.Domain.Network;
using AetherRemoteServer.Authentication;
using AetherRemoteServer.Managers;
using AetherRemoteServer.Registries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.Hubs;

[Authorize]
public class PrimaryHub(
    ConnectedClientsManager connectedClientsManager,
    HubRequestHandlerRegistry handlerRegistry,
    ILogger<PrimaryHub> logger) : Hub
{
    /// <summary>
    ///     Friend Code obtained from authenticated token claims
    /// </summary>
    private string FriendCode =>
        Context.User?.Claims
            .FirstOrDefault(claim => string.Equals(claim.Type, AuthClaimTypes.FriendCode, StringComparison.Ordinal))
            ?.Value ?? throw new Exception("FriendCode not present in claims");

    [HubMethodName(HubMethod.AddFriend)]
    public async Task<AddFriendResponse> AddFriend(AddFriendRequest request)
    {
        logger.LogInformation("{Request}", request);
        return await handlerRegistry.AddFriendHandler.Handle(FriendCode, request);
    }

    [HubMethodName(HubMethod.BodySwap)]
    public async Task<BodySwapResponse> BodySwap(BodySwapRequest request)
    {
        logger.LogInformation("{Request}", request);
        return await handlerRegistry.BodySwapHandler.Handle(FriendCode, request, Clients);
    }

    [HubMethodName(HubMethod.Emote)]
    public async Task<BaseResponse> Emote(EmoteRequest request)
    {
        logger.LogInformation("{Request}", request);
        return await handlerRegistry.EmoteHandler.Handle(FriendCode, request, Clients);
    }

    [HubMethodName(HubMethod.GetAccountData)]
    public async Task<GetAccountDataResponse> GetAccountData(GetAccountDataRequest request)
    {
        return await handlerRegistry.GetAccountDataHandler.Handle(FriendCode, request);
    }

    [HubMethodName(HubMethod.Moodles)]
    public async Task<BaseResponse> GetMoodlesAction(MoodlesRequest request)
    {
        return await handlerRegistry.MoodlesHandler.Handle(FriendCode, request, Clients);
    }

    [HubMethodName(HubMethod.RemoveFriend)]
    public async Task<BaseResponse> RemoveFriend(RemoveFriendRequest request)
    {
        logger.LogInformation("{Request}", request);
        return await handlerRegistry.RemoveFriendHandler.Handle(FriendCode, request);
    }

    [HubMethodName(HubMethod.Speak)]
    public async Task<BaseResponse> Speak(SpeakRequest request)
    {
        logger.LogInformation("{Request}", request);
        return await handlerRegistry.SpeakHandler.Handle(FriendCode, request, Clients);
    }

    [HubMethodName(HubMethod.Transform)]
    public async Task<BaseResponse> Transform(TransformRequest request)
    {
        logger.LogInformation("{Request}", request);
        return await handlerRegistry.TransformHandler.Handle(FriendCode, request, Clients);
    }

    [HubMethodName(HubMethod.Twinning)]
    public async Task<BaseResponse> Twinning(TwinningRequest request)
    {
        logger.LogInformation("{Request}", request);
        return await handlerRegistry.TwinningHandler.Handle(FriendCode, request, Clients);
    }

    [HubMethodName(HubMethod.UpdateFriend)]
    public async Task<BaseResponse> UpdateFriend(UpdateFriendRequest request)
    {
        logger.LogInformation("{Request}", request);
        return await handlerRegistry.UpdateFriendHandler.Handle(FriendCode, request, Clients);
    }
    
    [HubMethodName(HubMethod.CustomizePlus)]
    public async Task<BaseResponse> CustomizePlus(CustomizePlusRequest request)
    {
        logger.LogInformation("{Request}", request);
        return await handlerRegistry.CustomizePlusHandler.Handle(FriendCode, request, Clients);
    }

    /// <summary>
    ///     Handles when a client connects to the hub
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        await connectedClientsManager.ProcessFriendOnlineStatusChange(FriendCode, true, Context, Clients);
        await base.OnConnectedAsync();
    }

    /// <summary>
    ///     Handles when a client disconnects from the hub
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await connectedClientsManager.ProcessFriendOnlineStatusChange(FriendCode, false, Context, Clients);
        await base.OnDisconnectedAsync(exception);
    }
}