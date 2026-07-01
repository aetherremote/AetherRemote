using System;
using System.Threading.Tasks;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Handlers;
using AetherRemoteClient.Infrastructure.Authentication;
using AetherRemoteClient.Services;
using Dalamud.Utility;

namespace AetherRemoteClient.UI.Views.Login;

public class LoginViewUiController : IDisposable
{
    private readonly AuthenticationInfrastructure _authenticationInfrastructure;
    private readonly ActiveSessionService _activeSessionService;
    private readonly SecretsService _secretsService;
    private readonly NetworkService _networkService;
    private readonly LoginHandler _loginHandler;
    
    public LoginViewUiController(
        AuthenticationInfrastructure authenticationInfrastructure,
        ActiveSessionService activeSessionService,
        SecretsService secretsService,
        NetworkService networkService,
        LoginHandler loginHandler)
    {
        _authenticationInfrastructure = authenticationInfrastructure;
        _activeSessionService = activeSessionService;
        _secretsService = secretsService;
        _networkService = networkService;
        _loginHandler = loginHandler;
    }

    /// <summary>
    ///     If the secret has been selected in the Ui at least once, always return that, otherwise, return whatever
    ///     value exists in the active sessions service, if there is one set at all
    /// </summary>
    public Secret? SelectedSecret
    {
        get
        {
            if (field is not null) return field;

            return _activeSessionService.SecretId is { } secretId
                ? _secretsService.Secrets.TryGetValue(secretId, out var secret)
                    ? secret
                    : null
                : null;
        }
        set;
    }
    
    public async Task Connect()
    {
        if (SelectedSecret is null) return;
        
        _authenticationInfrastructure.SetSecret(SelectedSecret.Value);
        await _activeSessionService.SetSecretId(SelectedSecret.Id).ConfigureAwait(false);
        await _networkService.ConnectToServerAsync().ConfigureAwait(false);
    }
    
    public static void OpenDiscordLink() => Util.OpenLink("https://discord.com/invite/aetherremote");
    
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}