using System;
using System.Threading.Tasks;
using AetherRemoteClient.Infrastructure.Authentication;
using AetherRemoteClient.Infrastructure.Database;
using AetherRemoteClient.Services;
using Dalamud.Utility;

namespace AetherRemoteClient.UI.Views.Login;

public class LoginViewUiController(
    AuthenticationInfrastructure authenticationInfrastructure,
    CharacterConfigurationService characterConfigurationService,
    NetworkService networkService,
    SecretsService secretsService) : IDisposable
{
    public DatabaseInfrastructure.Secret? GetCurrentSecret()
    {
        if (characterConfigurationService.Current?.SecretId is not { } secretId)
            return null;
        
        secretsService.Secrets.TryGetValue(secretId, out var secret);
        return secret;
    }

    public async Task SetSecret(DatabaseInfrastructure.Secret secret)
    {
        if (await characterConfigurationService.SetSecretForCharacter(secret.Id).ConfigureAwait(false) is false)
            return;
        
        authenticationInfrastructure.SetSecret(secret.Value);
    }

    public async Task Connect()
    {
        await networkService.ConnectToServerAsync().ConfigureAwait(false);
    }
    
    public static void OpenDiscordLink() => Util.OpenLink("https://discord.com/invite/aetherremote");
    
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}