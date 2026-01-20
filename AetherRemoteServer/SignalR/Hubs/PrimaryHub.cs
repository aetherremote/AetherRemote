using AetherRemoteServer.Domain;
using AetherRemoteServer.Domain.Interfaces;
using AetherRemoteServer.SignalR.Handlers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Hubs;

[Authorize]
public partial class PrimaryHub(
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

    /// <summary>
    ///     Handles when a client connects to the hub
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        await onlineStatusUpdateHandler.Handle(FriendCode, true, Clients);
        await base.OnConnectedAsync();
    }

    /// <summary>
    ///     Handles when a client disconnects from the hub
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await onlineStatusUpdateHandler.Handle(FriendCode, false, Clients);
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