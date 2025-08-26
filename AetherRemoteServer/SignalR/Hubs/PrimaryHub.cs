using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.AddFriend;
using AetherRemoteCommon.Domain.Network.BodySwap;
using AetherRemoteCommon.Domain.Network.Customize;
using AetherRemoteCommon.Domain.Network.Emote;
using AetherRemoteCommon.Domain.Network.GetAccountData;
using AetherRemoteCommon.Domain.Network.Hypnosis;
using AetherRemoteCommon.Domain.Network.Moodles;
using AetherRemoteCommon.Domain.Network.RemoveFriend;
using AetherRemoteCommon.Domain.Network.Speak;
using AetherRemoteCommon.Domain.Network.Transform;
using AetherRemoteCommon.Domain.Network.Twinning;
using AetherRemoteCommon.Domain.Network.UpdateFriend;
using AetherRemoteServer.Domain;
using AetherRemoteServer.SignalR.Handlers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Hubs;

[Authorize]
public class PrimaryHub(
    // Managers
    OnlineStatusUpdateHandler onlineStatusUpdateHandler,

    // Handlers
    AddFriendHandler addFriendHandler,
    BodySwapHandler bodySwapHandler,
    CustomizePlusHandler customizePlusHandler,
    EmoteHandler emoteHandler,
    GetAccountDataHandler getAccountDataHandler,
    HypnosisHandler hypnosisHandler,
    MoodlesHandler moodlesHandler,
    RemoveFriendHandler removeFriendHandler,
    SpeakHandler speakHandler,
    TransformHandler transformHandler,
    TwinningHandler twinningHandler,
    UpdateFriendHandler updateFriendHandler,

    // Logger
    ILogger<PrimaryHub> logger) : Hub
{
    /// <summary>
    ///     Friend Code obtained from authenticated token claims
    /// </summary>
    private string FriendCode => Context.User?.FindFirst(AuthClaimTypes.FriendCode)?.Value ??
                                 throw new Exception("FriendCode not present in claims");

    [HubMethodName(HubMethod.AddFriend)]
    public async Task<AddFriendResponse> AddFriend(AddFriendRequest request)
    {
        logger.LogInformation("{Request}", request);
        return await addFriendHandler.Handle(FriendCode, request, Clients);
    }

    [HubMethodName(HubMethod.BodySwap)]
    public async Task<ActionResponse> BodySwap(BodySwapRequest request)
    {
        logger.LogInformation("{Request}", request);
        return new BodySwapResponse(ActionResponseEc.Disabled);
        return await bodySwapHandler.Handle(FriendCode, request, Clients);
    }

    [HubMethodName(HubMethod.Emote)]
    public async Task<ActionResponse> Emote(EmoteRequest request)
    {
        logger.LogInformation("{Request}", request);
        return await emoteHandler.Handle(FriendCode, request, Clients);
    }

    [HubMethodName(HubMethod.GetAccountData)]
    public async Task<GetAccountDataResponse> GetAccountData(GetAccountDataRequest request)
    {
        return await getAccountDataHandler.Handle(FriendCode, request);
    }

    [HubMethodName(HubMethod.Moodles)]
    public async Task<ActionResponse> GetMoodlesAction(MoodlesRequest request)
    {
        return new ActionResponse(ActionResponseEc.Disabled);
        return await moodlesHandler.Handle(FriendCode, request, Clients);
    }

    [HubMethodName(HubMethod.RemoveFriend)]
    public async Task<RemoveFriendResponse> RemoveFriend(RemoveFriendRequest request)
    {
        logger.LogInformation("{Request}", request);
        return await removeFriendHandler.Handle(FriendCode, request);
    }

    [HubMethodName(HubMethod.Speak)]
    public async Task<ActionResponse> Speak(SpeakRequest request)
    {
        logger.LogInformation("{Request}", request);
        return await speakHandler.Handle(FriendCode, request, Clients);
    }

    [HubMethodName(HubMethod.Transform)]
    public async Task<ActionResponse> Transform(TransformRequest request)
    {
        logger.LogInformation("{Request}", request);
        return new ActionResponse(ActionResponseEc.Disabled);
        return await transformHandler.Handle(FriendCode, request, Clients);
    }

    [HubMethodName(HubMethod.Twinning)]
    public async Task<ActionResponse> Twinning(TwinningRequest request)
    {
        logger.LogInformation("{Request}", request);
        return new ActionResponse(ActionResponseEc.Disabled);
        return await twinningHandler.Handle(FriendCode, request, Clients);
    }

    [HubMethodName(HubMethod.UpdateFriend)]
    public async Task<UpdateFriendResponse> UpdateFriend(UpdateFriendRequest request)
    {
        logger.LogInformation("{Request}", request);
        return await updateFriendHandler.Handle(FriendCode, request, Clients);
    }

    [HubMethodName(HubMethod.CustomizePlus)]
    public async Task<ActionResponse> CustomizePlus(CustomizeRequest request)
    {
        logger.LogInformation("{Request}", request);
        return new ActionResponse(ActionResponseEc.Disabled);
        return await customizePlusHandler.Handle(FriendCode, request, Clients);
    }

    [HubMethodName(HubMethod.Hypnosis)]
    public async Task<ActionResponse> Hypnosis(HypnosisRequest request)
    {
        logger.LogInformation("{Request}", request);
        return new ActionResponse(ActionResponseEc.Disabled);
        return await hypnosisHandler.Handle(FriendCode, request, Clients);
    }

    /// <summary>
    ///     Handles when a client connects to the hub
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        await onlineStatusUpdateHandler.Handle(FriendCode, true, Context, Clients);
        await base.OnConnectedAsync();
    }

    /// <summary>
    ///     Handles when a client disconnects from the hub
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await onlineStatusUpdateHandler.Handle(FriendCode, false, Context, Clients);
        await base.OnDisconnectedAsync(exception);
    }
}