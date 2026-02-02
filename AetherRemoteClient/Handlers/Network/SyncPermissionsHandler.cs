using System;
using AetherRemoteClient.Services;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.SyncOnlineStatus;
using AetherRemoteCommon.Domain.Network.SyncPermissions;
using Microsoft.AspNetCore.SignalR.Client;

namespace AetherRemoteClient.Handlers.Network;

/// <summary>
///     Handles a <see cref="SyncPermissionsCommand"/>
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
        
        _handler = network.Connection.On<SyncPermissionsCommand>(HubMethod.SyncPermissions, Handle);
    }
    
    /// <summary>
    ///     <inheritdoc cref="SyncPermissionsHandler"/>
    /// </summary>
    private void Handle(SyncPermissionsCommand command)
    {
        if (_friends.Get(command.SenderFriendCode) is not { } friend)
            return;
        
        friend.PermissionsGrantedByFriend = UserPermissions.From(command.PermissionsGrantedBySender);
    }

    public void Dispose()
    {
        _handler.Dispose();
        GC.SuppressFinalize(this);
    }
}