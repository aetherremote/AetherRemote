using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.AddFriend;
using AetherRemoteCommon.Domain.Network.BodySwap;
using AetherRemoteCommon.Domain.Network.Customize;
using AetherRemoteCommon.Domain.Network.Emote;
using AetherRemoteCommon.Domain.Network.GetAccountData;
using AetherRemoteCommon.Domain.Network.Hypnosis;
using AetherRemoteCommon.Domain.Network.HypnosisStop;
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
    HypnosisStopHandler hypnosisStopHandler,
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
    private string FriendCode => Context.User?.FindFirst(AuthClaimTypes.FriendCode)?.Value ?? throw new Exception("FriendCode not present in claims");

    #region Account Management

    [HubMethodName(HubMethod.GetAccountData)]
    public async Task<GetAccountDataResponse> GetAccountData(GetAccountDataRequest request)
    {
        var friendCode = FriendCode;
        logger.LogInformation("[GetAccountData] Sender = {Sender}", friendCode);
        return await getAccountDataHandler.Handle(FriendCode, request);
    }

    #endregion
    
    #region Friend Management

    [HubMethodName(HubMethod.AddFriend)]
    public async Task<AddFriendResponse> AddFriend(AddFriendRequest request)
    {
        var friendCode = FriendCode;
        logger.LogInformation("[AddFriendRequest] Sender = {Sender}, Target = {Targets}", friendCode, request.TargetFriendCode);
        return await addFriendHandler.Handle(FriendCode, request, Clients);
    }
    
    [HubMethodName(HubMethod.RemoveFriend)]
    public async Task<RemoveFriendResponse> RemoveFriend(RemoveFriendRequest request)
    {
        var friendCode = FriendCode;
        logger.LogInformation("[RemoveFriendRequest] Sender = {Sender}, Target = {Targets}", friendCode, request.TargetFriendCode);
        return await removeFriendHandler.Handle(FriendCode, request);
    }
    
    [HubMethodName(HubMethod.UpdateFriend)]
    public async Task<UpdateFriendResponse> UpdateFriend(UpdateFriendRequest request)
    {
        var friendCode = FriendCode;
        logger.LogInformation("[UpdateFriendRequest] Sender = {Sender}, Target = {Targets}, Permissions = {Permissions}", friendCode, request.TargetFriendCode, request.Permissions);
        return await updateFriendHandler.Handle(FriendCode, request, Clients);
    }

    #endregion
    
    #region Moderated Actions
    
    [HubMethodName(HubMethod.Speak)]
    public async Task<ActionResponse> Speak(SpeakRequest request)
    {
        var friendCode = FriendCode;
        logger.LogInformation("[SpeakRequest] Sender = {Sender}, Targets = {Targets}, Message = {Message}", friendCode, string.Join(", ", request.TargetFriendCodes), request.Message);
        return await speakHandler.Handle(friendCode, request, Clients);
    }
    
    [HubMethodName(HubMethod.Hypnosis)]
    public async Task<ActionResponse> Hypnosis(HypnosisRequest request)
    {
        var friendCode = FriendCode;
        logger.LogInformation("[HypnosisRequest] Sender = {Sender}, Targets = {Targets}, Words = {Words}", friendCode, string.Join(", ", request.TargetFriendCodes), string.Join(", ", request.Data.TextWords));
        return await hypnosisHandler.Handle(FriendCode, request, Clients);
    }
    
    #endregion

    #region Actions

    [HubMethodName(HubMethod.BodySwap)]
    public async Task<ActionResponse> BodySwap(BodySwapRequest request)
    {
        if (request.LockCode is not null)
            return new ActionResponse(ActionResponseEc.Disabled);
        
        return await bodySwapHandler.Handle(FriendCode, request, Clients);
    }
    
    [HubMethodName(HubMethod.CustomizePlus)]
    public async Task<ActionResponse> CustomizePlus(CustomizeRequest request)
    {
        return await customizePlusHandler.Handle(FriendCode, request, Clients);
    }

    [HubMethodName(HubMethod.Emote)]
    public async Task<ActionResponse> Emote(EmoteRequest request)
    {
        return await emoteHandler.Handle(FriendCode, request, Clients);
    }
    
    [HubMethodName(HubMethod.HypnosisStop)]
    public async Task<ActionResponse> HypnosisStop(HypnosisStopRequest request)
    {
        return await hypnosisStopHandler.Handle(FriendCode, request, Clients);
    }
    
    [HubMethodName(HubMethod.Moodles)]
    public async Task<ActionResponse> GetMoodlesAction(MoodlesRequest request)
    {
        return await moodlesHandler.Handle(FriendCode, request, Clients);
    }
    
    [HubMethodName(HubMethod.Transform)]
    public async Task<ActionResponse> Transform(TransformRequest request)
    {
        if (request.LockCode is not null)
            return new ActionResponse(ActionResponseEc.Disabled);
        
        return await transformHandler.Handle(FriendCode, request, Clients);
    }

    [HubMethodName(HubMethod.Twinning)]
    public async Task<ActionResponse> Twinning(TwinningRequest request)
    {
        if (request.LockCode is not null)
            return new ActionResponse(ActionResponseEc.Disabled);
        
        var friendCode = FriendCode;
        logger.LogInformation("[TwinningRequest] Sender = {Sender}, Targets = {Targets}, Attributes = {Attributes}", friendCode, string.Join(", ", request.TargetFriendCodes), request.SwapAttributes);
        return await twinningHandler.Handle(FriendCode, request, Clients);
    }
    
    #endregion

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