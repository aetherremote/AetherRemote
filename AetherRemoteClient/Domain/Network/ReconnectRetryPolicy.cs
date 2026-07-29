using System;
using Microsoft.AspNetCore.SignalR.Client;

namespace AetherRemoteClient.Domain.Network;

/// <summary>
///     Introduces a reconnection policy focused on handling disconnections
/// </summary>
public class ReconnectRetryPolicy : IRetryPolicy
{
    private static readonly TimeSpan[] Delays =
    [
        TimeSpan.FromSeconds(2),
        TimeSpan.FromSeconds(4),
        TimeSpan.FromSeconds(8),
        TimeSpan.FromSeconds(16),
        TimeSpan.FromSeconds(32),
        TimeSpan.FromSeconds(64),
        TimeSpan.FromSeconds(128),
        TimeSpan.FromSeconds(256)
    ];
    
    public TimeSpan? NextRetryDelay(RetryContext retryContext)
    {
        return retryContext.PreviousRetryCount >= Delays.Length ? Delays[^1] : Delays[retryContext.PreviousRetryCount];
    }
}