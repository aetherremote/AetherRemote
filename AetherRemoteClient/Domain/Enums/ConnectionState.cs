using AetherRemoteClient.Services;
using Microsoft.AspNetCore.SignalR.Client;

namespace AetherRemoteClient.Domain.Enums;

/// <summary>
///     Wrapper class for <see cref="HubConnectionState"/> as to not expose SignalR elements outside of <see cref="NetworkService"/>
/// </summary>
public enum ConnectionState
{
    /// <summary>
    ///     <inheritdoc cref="HubConnectionState.Disconnected"/>
    /// </summary>
    Disconnected,
    
    /// <summary>
    ///     <inheritdoc cref="HubConnectionState.Connected"/>
    /// </summary>
    Connected,
    
    /// <summary>
    ///     <inheritdoc cref="HubConnectionState.Connecting"/>
    /// </summary>
    Connecting,
    
    /// <summary>
    ///     <inheritdoc cref="HubConnectionState.Reconnecting"/>
    /// </summary>
    Reconnecting
}