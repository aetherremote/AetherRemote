using System;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteClient.Services.Configuration;

namespace AetherRemoteClient.UI.Views.Login;

public partial class LoginView : IDisposable, IView
{
    // IView property
    public View View => View.Login;
    
    // Injected
    private readonly ActiveSessionService _activeSessionService;
    private readonly ConfigurationService _configurationService;
    private readonly ViewService _viewService;
    private readonly ConnectionManager _connectionManager;
    
    public LoginView(
        ActiveSessionService activeSessionService,
        ConfigurationService configurationService,
        ViewService viewService,
        ConnectionManager connectionManager)
    {
        _activeSessionService = activeSessionService;
        _configurationService = configurationService;
        _viewService = viewService;
        _connectionManager = connectionManager;
    }
    
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}