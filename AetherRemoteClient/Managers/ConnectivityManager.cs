using System;
using System.Threading.Tasks;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Services;
using AetherRemoteCommon.Domain.Network;

namespace AetherRemoteClient.Managers;

/// <summary>
///     Handles the state of the plugin's connectivity.
///     This includes connections and reconnections.
/// </summary>
public class ConnectivityManager : IDisposable
{
    private readonly FriendsListService _friendsListService;
    private readonly IdentityService _identityService;
    private readonly NetworkService _networkService;

    /// <summary>
    /// <inheritdoc cref="ConnectivityManager"/>
    /// </summary>
    public ConnectivityManager(FriendsListService friendsListService, IdentityService identityService, NetworkService networkService)
    {
        _friendsListService = friendsListService;
        _identityService = identityService;
        _networkService = networkService;
        
        // Connectivity Events
        _networkService.Connected += OnConnect;
        _networkService.Disconnected += OnDisconnect;
        Plugin.ClientState.Login += ClientLoggedIntoGame;
        Plugin.ClientState.Logout += ClientLoggedOutOfGame;

        if (Plugin.ClientState.IsLoggedIn)
            ClientLoggedIntoGame();
    }

    private Task OnConnect() => GetAndSetAccountData();

    private async Task GetAndSetAccountData()
    {
        var input = new GetAccountDataRequest();
        var result = await _networkService
            .InvokeAsync<GetAccountDataResponse>(HubMethod.GetAccountData, input)
            .ConfigureAwait(false);
        
        if (result.Success is false)
        {
            Plugin.Log.Fatal($"[NetworkHandler] Failed to get account data, {result.Message}");
            await _networkService.StopAsync().ConfigureAwait(false);
            return;
        }

        _identityService.FriendCode = result.FriendCode;
        await _identityService.SetIdentityToCurrentCharacter().ConfigureAwait(false);

        _friendsListService.Clear();
        foreach (var (friendCode, permissionsGrantedToFriend) in result.PermissionsGrantedToOthers)
        {
            var online = result.PermissionsGrantedByOthers.TryGetValue(friendCode, out var permissionsGrantedByOther);
            Plugin.Configuration.Notes.TryGetValue(friendCode, out var note);

            var friend = new Friend(friendCode, note, online, permissionsGrantedToFriend, permissionsGrantedByOther);
            _friendsListService.Add(friend);
        }
    }

    private async void ClientLoggedIntoGame()
    {
        try
        {
            if (Plugin.DeveloperMode)
                return;

            if (Plugin.Configuration.AutoLogin)
                await _networkService.StartAsync().ConfigureAwait(false);
        }
        catch (Exception)
        {
            // Ignored
        }
    }

    private async void ClientLoggedOutOfGame(int type, int code)
    {
        try
        {
            if (Plugin.DeveloperMode)
                return;

            await _networkService.StopAsync().ConfigureAwait(false);
        }
        catch (Exception)
        {
            // Ignored
        }
    }
    
    private Task OnDisconnect()
    {
        _friendsListService.Selected.Clear();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _networkService.Connected -= OnConnect;
        Plugin.ClientState.Login -= ClientLoggedIntoGame;
        Plugin.ClientState.Logout -= ClientLoggedOutOfGame;
        GC.SuppressFinalize(this);
    }
}