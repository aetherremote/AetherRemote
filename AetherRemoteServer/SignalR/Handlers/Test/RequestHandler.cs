using AetherRemoteServer.Managers;
using AetherRemoteServer.Services;
using AetherRemoteServer.Services.Database;

namespace AetherRemoteServer.SignalR.Handlers.Test;

public partial class RequestHandler
{
    private readonly DatabaseService _databaseService;
    private readonly PresenceService _presenceService;
   
    private readonly PossessionManager _possessionManager;
    
    // TODO: REMOVE THIS
    private readonly ForwardedRequestManager _forwardedRequestManager;
    
    private readonly ILogger<RequestHandler> _logger;

    public RequestHandler(
        DatabaseService databaseService,
        PresenceService presenceService,
        PossessionManager possessionManager,
        ForwardedRequestManager forwardedRequestManager,
        ILogger<RequestHandler> logger)
    {
        _databaseService = databaseService;
        _presenceService = presenceService;
        _possessionManager = possessionManager;
        _forwardedRequestManager = forwardedRequestManager;
        _logger = logger;
    }
}