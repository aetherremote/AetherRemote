using System;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Infrastructure.Authentication;
using AetherRemoteClient.Services;

namespace AetherRemoteClient.UI.Views.Login;

public partial class LoginView : IDisposable, IView
{
    // IView property
    public View View => View.Login;
    
    // Injected
    private readonly AuthenticationInfrastructure _authenticationInfrastructure;
    private readonly ActiveSessionService _activeSessionService;
    private readonly SecretsService _secretsService;
    private readonly NetworkService _networkService;
    
    public LoginView(
        AuthenticationInfrastructure authenticationInfrastructure,
        ActiveSessionService activeSessionService,
        SecretsService secretsService,
        NetworkService networkService)
    {
        _authenticationInfrastructure = authenticationInfrastructure;
        _activeSessionService = activeSessionService;
        _secretsService = secretsService;
        _networkService = networkService;
    }
    
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}