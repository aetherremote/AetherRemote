using System;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.SyncOnlineStatus;
using Microsoft.AspNetCore.SignalR.Client;

namespace AetherRemoteClient.Handlers.Network;

/// <summary>
///     Handles a <see cref="SyncOnlineStatusCommand"/>
/// </summary>
public class SyncOnlineStatusHandler : IDisposable
{
    // Injected
    private readonly FriendsListService _friends;
    private readonly SelectionManager _selection;
    
    // Instantiated
    private readonly IDisposable _handler;
    
    /// <summary>
    ///     <inheritdoc cref="SyncOnlineStatusHandler"/>
    /// </summary>
    public SyncOnlineStatusHandler(FriendsListService friends, NetworkService network, SelectionManager selection)
    {
        _friends = friends;
        _selection = selection;
        
        _handler = network.Connection.On<SyncOnlineStatusCommand>(HubMethod.SyncOnlineStatus, Handle);
    }
    
    /// <summary>
    ///     <inheritdoc cref="SyncOnlineStatusHandler"/>
    /// </summary>
    private void Handle(SyncOnlineStatusCommand action)
    {
        if (_friends.Get(action.SenderFriendCode) is not { } friend)
            return;
        
        friend.Status = action.Status;

        if (friend.Status is FriendOnlineStatus.Offline)
        {
            _selection.Deselect(friend);
            return;
        }

        if (action.Permissions is null)
        {
            Plugin.Log.Warning("[SyncOnlineStatusHandler.Handle] Permissions are not set");
            return;
        }
        
        friend.PermissionsGrantedByFriend = action.Permissions;
    }

    public void Dispose()
    {
        _handler.Dispose();
        GC.SuppressFinalize(this);
    }
}