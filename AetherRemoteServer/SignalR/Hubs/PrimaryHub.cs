using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.AddFriend;
using AetherRemoteCommon.Domain.Network.BodySwap;
using AetherRemoteCommon.Domain.Network.Customize;
using AetherRemoteCommon.Domain.Network.Emote;
using AetherRemoteCommon.Domain.Network.GetAccountData;
using AetherRemoteCommon.Domain.Network.Honorific;
using AetherRemoteCommon.Domain.Network.Hypnosis;
using AetherRemoteCommon.Domain.Network.HypnosisStop;
using AetherRemoteCommon.Domain.Network.Moodles;
using AetherRemoteCommon.Domain.Network.RemoveFriend;
using AetherRemoteCommon.Domain.Network.Speak;
using AetherRemoteCommon.Domain.Network.Transform;
using AetherRemoteCommon.Domain.Network.Twinning;
using AetherRemoteCommon.Domain.Network.UpdateFriend;
using AetherRemoteServer.Domain;
using AetherRemoteServer.Domain.Interfaces;
using AetherRemoteServer.SignalR.Handlers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Hubs;

[Authorize]
public class PrimaryHub(
    // Services
    IRequestLoggingService requestLoggingService,
    
    // Managers
    OnlineStatusUpdateHandler onlineStatusUpdateHandler,

    // Handlers
    AddFriendHandler addFriendHandler,
    BodySwapHandler bodySwapHandler,
    CustomizePlusHandler customizePlusHandler,
    EmoteHandler emoteHandler,
    GetAccountDataHandler getAccountDataHandler,
    HonorificHandler honorificHandler,
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
        LogWithBehavior($"[GetAccountData] Sender = {friendCode}", LogMode.Disk);
        return await getAccountDataHandler.Handle(friendCode, request);
    }

    #endregion
    
    #region Friend Management

    [HubMethodName(HubMethod.AddFriend)]
    public async Task<AddFriendResponse> AddFriend(AddFriendRequest request)
    {
        var friendCode = FriendCode;
        LogWithBehavior($"[AddFriendRequest] Sender = {friendCode}, Target = {request.TargetFriendCode}", LogMode.Both);
        return await addFriendHandler.Handle(friendCode, request, Clients);
    }
    
    [HubMethodName(HubMethod.RemoveFriend)]
    public async Task<RemoveFriendResponse> RemoveFriend(RemoveFriendRequest request)
    {
        var friendCode = FriendCode;
        LogWithBehavior($"[RemoveFriendRequest] Sender = {friendCode}, Target = {request.TargetFriendCode}", LogMode.Both);
        return await removeFriendHandler.Handle(friendCode, request);
    }
    
    [HubMethodName(HubMethod.UpdateFriend)]
    public async Task<UpdateFriendResponse> UpdateFriend(UpdateFriendRequest request)
    {
        var friendCode = FriendCode;
        LogWithBehavior($"[UpdateFriendRequest] Sender = {friendCode}, Target = {request.TargetFriendCode}, Permissions = {request.Permissions}", LogMode.Disk);
        return await updateFriendHandler.Handle(friendCode, request, Clients);
    }

    #endregion
    
    #region Moderated Actions
    
    [HubMethodName(HubMethod.Speak)]
    public async Task<ActionResponse> Speak(SpeakRequest request)
    {
        var friendCode = FriendCode;
        LogWithBehavior($"[SpeakRequest] Sender = {friendCode}, Targets = {string.Join(", ", request.TargetFriendCodes)}, Message = {request.Message}", LogMode.Both);
        return await speakHandler.Handle(friendCode, request, Clients);
    }
    
    [HubMethodName(HubMethod.Hypnosis)]
    public async Task<ActionResponse> Hypnosis(HypnosisRequest request)
    {
        var friendCode = FriendCode;
        LogWithBehavior($"[HypnosisRequest] Sender = {friendCode}, Targets = {string.Join(", ", request.TargetFriendCodes)}, Words = {string.Join(", ", request.Data.TextWords)}", LogMode.Both);
        return await hypnosisHandler.Handle(friendCode, request, Clients);
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
    
    [HubMethodName(HubMethod.Honorific)]
    public async Task<ActionResponse> Honorific(HonorificRequest request)
    {
        var friendCode = FriendCode;
        LogWithBehavior($"[HonorificRequest] Sender = {friendCode}, Honorific = {request.Honorific}", LogMode.Console);
        return await honorificHandler.Handle(friendCode, request, Clients);
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

    /// <summary>
    ///     Special logging instruction for either console or file
    /// </summary>
    private void LogWithBehavior(string message, LogMode mode)
    {
        if ((mode & LogMode.Console) == LogMode.Console)
            logger.LogInformation("{Message}", message);
        
        if ((mode & LogMode.Disk) == LogMode.Disk)
            requestLoggingService.Log(message);
    }

    [Flags]
    private enum LogMode
    {
        Console = 1 << 0,
        Disk = 1 << 1,
        Both = Console | Disk
    }
}