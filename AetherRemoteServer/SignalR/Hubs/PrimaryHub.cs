using AetherRemoteServer.Domain;
using AetherRemoteServer.Services;
using AetherRemoteServer.SignalR.Handlers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Hubs;

[Authorize]
public partial class PrimaryHub(
    // Services
    RequestLoggingService requestLoggingService,
    SessionService sessionService,
    
    // Handler
    AggregateRequestHandler requestHandler,

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
    public override Task OnConnectedAsync()
    {
        sessionService.UpdateConnectivityStatus(FriendCode, OnlineStatus.Online);
        return base.OnConnectedAsync();
    }

    /// <summary>
    ///     Handles when a client disconnects from the hub
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var friendCode = FriendCode;
        var disconnectedConnectionId = Context.ConnectionId;
        sessionService.UpdateConnectivityStatus(friendCode, OnlineStatus.Disconnected);

        await Task.Delay(15 * 1000, Context.ConnectionAborted);

        if (sessionService.GetSession(friendCode) is not { } session)
            return;

        if (session.ConnectionId != disconnectedConnectionId) // The connection id changed, which is partially handled by them being online, but this is good form
            return;

        if (session.OnlineStatus is OnlineStatus.Online) // They connected back in the meantime, safely ignore
            return;
        
        _ = requestHandler.OnlineNotificationHandler.Notify(friendCode, false, Clients);
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
