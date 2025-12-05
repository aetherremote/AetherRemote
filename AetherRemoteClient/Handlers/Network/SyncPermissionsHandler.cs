using System;
using AetherRemoteClient.Services;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.SyncOnlineStatus;
using AetherRemoteCommon.Domain.Network.SyncPermissions;
using Microsoft.AspNetCore.SignalR.Client;

namespace AetherRemoteClient.Handlers.Network;

/// <summary>
///     Handles a <see cref="SyncPermissionsForwardedRequest"/>
/// </summary>
public class SyncPermissionsHandler : IDisposable
{
    // Injected
    private readonly FriendsListService _friends;
    
    // Instantiated
    private readonly IDisposable _handler;

    /// <summary>
    ///     <inheritdoc cref="SyncPermissionsHandler"/>
    /// </summary>
    public SyncPermissionsHandler(FriendsListService friends, NetworkService network)
    {
        _friends = friends;
        
        _handler = network.Connection.On<SyncPermissionsForwardedRequest>(HubMethod.SyncPermissions, Handle);
    }
    
    /// <summary>
    ///     <inheritdoc cref="SyncPermissionsHandler"/>
    /// </summary>
    private void Handle(SyncPermissionsForwardedRequest forwardedRequest)
    {
        if (_friends.Get(forwardedRequest.SenderFriendCode) is not { } friend)
            return;

        friend.PermissionsGrantedByFriend = forwardedRequest.PermissionsGrantedBySender;
    }

    public void Dispose()
    {
        _handler.Dispose();
        GC.SuppressFinalize(this);
    }
}