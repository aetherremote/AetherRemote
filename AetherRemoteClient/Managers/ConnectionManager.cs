using System;
using System.Threading.Tasks;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Services;
using AetherRemoteClient.Services.Configuration;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.GetAccountData;

namespace AetherRemoteClient.Managers;

/// <summary>
///     Manages connection and disconnection events from the server
/// </summary>
public class ConnectionManager(
    ActiveSessionService activeSessionService,
    ConfigurationService configurationService,
    FriendsListService friendsListService, 
    NetworkService networkService, 
    ViewService viewService) : IDisposable
{
    /// <summary>
    ///     Attempt to connect to the server
    /// </summary>
    /// <remarks> Utilizes the SecretId in <see cref="ActiveSessionService"/> to retrieve the secret</remarks>
    public async Task<bool> TryConnectToServerAsync()
    {
        if (activeSessionService.PendingSecretId is not { } pendingSecretId || 
            activeSessionService.CharacterName is not { } characterName || 
            activeSessionService.CharacterWorld is not { } characterWorld)
            return false;
        
        if (configurationService.Secrets.TryGetValue(pendingSecretId, out var secret) is false)
            return false;
        
        if (await networkService.ConnectToServerAsync(secret.Value).ConfigureAwait(false) is false)
            return false;
        
        networkService.Disconnected += OnDisconnected;
        
        var request = new GetAccountDataRequest(characterName, characterWorld);
        var response = await networkService.InvokeAsync<GetAccountDataResponse>(HubMethod.GetAccountData, request).ConfigureAwait(false);
        if (response.Result is not GetAccountDataEc.Success)
        {
            Plugin.Log.Fatal($"[ConnectionManager.TryConnectToServerAsync] Failed to get account data {response.Result}");
            networkService.Disconnected -= OnDisconnected;
            await networkService.DisconnectFromServerAsync().ConfigureAwait(false);
            return false;
        }

        if (await activeSessionService.UpdateAccountDetails(response.AccountFriendCode, response.AccountGlobalPermissions).ConfigureAwait(false) is false)
        {
            Plugin.Log.Fatal($"[ConnectionManager.TryConnectToServerAsync] Failed to initialize account details");
            networkService.Disconnected -= OnDisconnected;
            await networkService.DisconnectFromServerAsync().ConfigureAwait(false);
            return false;
        }
        
        friendsListService.Clear();
        foreach (var friend in response.AccountFriends)
        {
            var note = configurationService.GetNoteFor(friend.TargetFriendCode);
            friendsListService.Add(new Friend(friend.TargetFriendCode, friend.Status, note, friend.PermissionsGrantedTo, friend.PermissionsGrantedBy));
        }
        
        viewService.Home();
        return true;
    }

    private Task OnDisconnected()
    {
        viewService.ResetView();
        networkService.Disconnected -= OnDisconnected;
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        networkService.Disconnected -= OnDisconnected;
        GC.SuppressFinalize(this);
    }
}