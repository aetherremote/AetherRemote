using System;
using System.Threading.Tasks;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.GetAccountData;

namespace AetherRemoteClient.Handlers;

/// <summary>
///     Handles the state of the plugin's connectivity.
///     This includes connections and reconnections.
/// </summary>
public class ConnectivityHandler : IDisposable
{
    private readonly ConfigurationService _configurationService;
    private readonly FriendsListService _friendsListService;
    private readonly IdentityService _identityService;
    private readonly NetworkManager _networkManager;
    private readonly ViewService _viewService;
    private readonly DependencyManager _dependencyManager;
    private readonly PermanentTransformationManager _permanentTransformationManager;

    /// <summary>
    /// <inheritdoc cref="ConnectivityHandler"/>
    /// </summary>
    public ConnectivityHandler(ConfigurationService configurationService, FriendsListService friendsListService, IdentityService identityService, NetworkManager networkManager, ViewService viewService, DependencyManager dependencyManager, PermanentTransformationManager permanentTransformationManager)
    {
        _configurationService = configurationService;
        _friendsListService = friendsListService;
        _identityService = identityService;
        _networkManager = networkManager;
        _viewService = viewService;
        _dependencyManager = dependencyManager;
        _permanentTransformationManager = permanentTransformationManager;
        
        // Connectivity Events
        _networkManager.Connected += OnConnectedToServer;
        _networkManager.Disconnected += OnDisconnectedFromServer;
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
        var response = await _networkManager
            .InvokeAsync<GetAccountDataResponse>(HubMethod.GetAccountData, input)
            .ConfigureAwait(false);
        
        if (response.Result is not GetAccountDataEc.Success)
        {
            Plugin.Log.Fatal($"[NetworkHandler] Failed to get account data, {response.Result}");
            await _networkManager.StopAsync().ConfigureAwait(false);
            return;
        }

        _identityService.FriendCode = response.FriendCode;

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
            // Force a dependency retry
            _dependencyManager.ForceTestAvailability();
            
            // Get the local player
            if (await Plugin.RunOnFramework(() => Plugin.ClientState.LocalPlayer) is not { } player)
                return;

            var name = player.Name.ToString();
            var world = player.HomeWorld.Value.Name.ToString();
            
            // Store the local character for use later on in the plugin
            _identityService.Character = new LocalCharacter(name, world);
            
            // Load the character configuration file for this character
            await _configurationService.Load(name, world);
            
            // Check for any permanent transformations for this character
            await _permanentTransformationManager.Load(name, world);
            
            // Automatically log in if needed
            if (Plugin.Configuration.AutoLogin)
                await _networkManager.StartAsync().ConfigureAwait(false);
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
            await _networkManager.StopAsync().ConfigureAwait(false);
        }
        catch (Exception)
        {
            // Ignored
        }
    }

    public void Dispose()
    {
        _networkManager.Connected -= OnConnectedToServer;
        _networkManager.Disconnected -= OnDisconnectedFromServer;
        Plugin.ClientState.Login -= ClientLoggedIntoGame;
        Plugin.ClientState.Logout -= ClientLoggedOutOfGame;
        GC.SuppressFinalize(this);
    }
}