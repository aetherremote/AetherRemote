using System.Threading.Tasks;
using AetherRemoteClient.Domain;
using Dalamud.Utility;

namespace AetherRemoteClient.UI.Views.Login;

public partial class LoginView
{
    /// <summary>
    ///     If the secret has been selected in the Ui at least once, always return that, otherwise, return whatever
    ///     value exists in the active sessions service, if there is one set at all
    /// </summary>
    private Secret? SelectedSecret
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

    private async Task Connect()
    {
        if (SelectedSecret is null) return;
        
        _authenticationInfrastructure.SetSecret(SelectedSecret.Value);
        await _activeSessionService.SetSecretId(SelectedSecret.Id).ConfigureAwait(false);
        await _networkService.ConnectToServerAsync().ConfigureAwait(false);
    }

    private static void OpenDiscordLink() => Util.OpenLink("https://discord.com/invite/aetherremote");
}