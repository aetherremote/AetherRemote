using AetherRemoteServer.Managers;
using AetherRemoteServer.Services;
using DatabaseInfrastructure = AetherRemoteServer.Infrastructure.Database.DatabaseInfrastructure;

namespace AetherRemoteServer.SignalR.Handlers;

public partial class RequestHandler
{
    private readonly DatabaseInfrastructure _databaseInfrastructure;
    private readonly PresenceService _presenceService;
   
    private readonly PossessionManager _possessionManager;
    
    // TODO: REMOVE THIS
    private readonly ForwardedRequestManager _forwardedRequestManager;
    
    private readonly ILogger<RequestHandler> _logger;

    public RequestHandler(
        DatabaseInfrastructure databaseInfrastructure,
        PresenceService presenceService,
        PossessionManager possessionManager,
        ForwardedRequestManager forwardedRequestManager,
        ILogger<RequestHandler> logger)
    {
        _databaseInfrastructure = databaseInfrastructure;
        _presenceService = presenceService;
        _possessionManager = possessionManager;
        _forwardedRequestManager = forwardedRequestManager;
        _logger = logger;
    }
}