using System;
using System.Threading.Tasks;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Services;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.GetAccountData;

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
    private readonly ViewService _viewService;

    /// <summary>
    /// <inheritdoc cref="ConnectivityManager"/>
    /// </summary>
    public ConnectivityManager(FriendsListService friendsListService, IdentityService identityService, NetworkService networkService, ViewService viewService)
    {
        _friendsListService = friendsListService;
        _identityService = identityService;
        _networkService = networkService;
        _viewService = viewService;
        
        // Connectivity Events
        _networkService.Connected += OnConnectedToServer;
        _networkService.Disconnected += OnDisconnectedFromServer;
        Plugin.ClientState.Login += ClientLoggedIntoGame;
        Plugin.ClientState.Logout += ClientLoggedOutOfGame;

        if (Plugin.ClientState.IsLoggedIn)
            ClientLoggedIntoGame();
    }

    /// <summary>
    ///     Fired when connected to the server
    /// </summary>
    private async Task OnConnectedToServer()
    {
        await GetAndSetAccountData();
        _viewService.CurrentView = View.Status;
    }

    /// <summary>
    ///     Fired when disconnected from the server
    /// </summary>
    private async Task OnDisconnectedFromServer()
    {
        await ClearAccountData();
        if (_viewService.CurrentView is not View.Settings)
            _viewService.CurrentView = View.Login;
    }
    
    /// <summary>
    ///     Calls the server to get all information relating to this client
    /// </summary>
    private async Task GetAndSetAccountData()
    {
        if (await Plugin.RunOnFramework(() => Plugin.ClientState.LocalPlayer) is not { } player)
            return;
        
        var input = new GetAccountDataRequest(player.Name.ToString());
        var response = await _networkService
            .InvokeAsync<GetAccountDataResponse>(HubMethod.GetAccountData, input)
            .ConfigureAwait(false);
        
        if (response.Result is not GetAccountDataEc.Success)
        {
            Plugin.Log.Fatal($"[NetworkHandler] Failed to get account data, {response.Result}");
            await _networkService.StopAsync().ConfigureAwait(false);
            return;
        }

        _identityService.FriendCode = response.FriendCode;
        await _identityService.SetIdentityToCurrentCharacter().ConfigureAwait(false);

        _friendsListService.Clear();
        foreach (var (friendCode, permissionsGrantedToFriend) in response.PermissionsGrantedToOthers)
        {
            var online = response.PermissionsGrantedByOthers.TryGetValue(friendCode, out var permissionsGrantedByOther);
            Plugin.Configuration.Notes.TryGetValue(friendCode, out var note);

            var friend = new Friend(friendCode, note, online, permissionsGrantedToFriend, permissionsGrantedByOther);
            _friendsListService.Add(friend);
        }
    }
    
    /// <summary>
    ///     Called when disconnected from the server
    /// </summary>
    /// <returns></returns>
    private Task ClearAccountData()
    {
        _friendsListService.Selected.Clear();
        return Task.CompletedTask;
    }

    private async void ClientLoggedIntoGame()
    {
        try
        {
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
            await _networkService.StopAsync().ConfigureAwait(false);
        }
        catch (Exception)
        {
            // Ignored
        }
    }

    public void Dispose()
    {
        _networkService.Connected -= OnConnectedToServer;
        _networkService.Disconnected -= OnDisconnectedFromServer;
        Plugin.ClientState.Login -= ClientLoggedIntoGame;
        Plugin.ClientState.Logout -= ClientLoggedOutOfGame;
        GC.SuppressFinalize(this);
    }
}