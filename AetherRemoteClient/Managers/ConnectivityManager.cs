using System;
using System.Threading.Tasks;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Services;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.V2.Domain.Network.GetAccountData;

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
        if (await Plugin.RunOnFramework(() => Plugin.ClientState.LocalPlayer) is not { } player)
            return;
        
        var input = new GetAccountDataRequest(player.Name.ToString());
        var response = await _networkService
            .InvokeAsync<GetAccountDataResponse>(HubMethod.GetAccountData, input)
            .ConfigureAwait(false);
        
        if (response.Result is not GetAccountDataEc.Success)
        {
            // TODO: Maybe add a pop-up response here?   
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