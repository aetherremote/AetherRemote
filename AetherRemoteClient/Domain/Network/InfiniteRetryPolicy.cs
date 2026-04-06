using System;
using Microsoft.AspNetCore.SignalR.Client;

namespace AetherRemoteClient.Domain.Network;

/// <summary>
///     Introduces an infinite reconnection policy
/// </summary>
public class InfiniteRetryPolicy : IRetryPolicy
{
    // TODO: We can attach a static event to this class and listen for retry attempts for better clarity in the Ui
    
    public TimeSpan? NextRetryDelay(RetryContext context)
    {
        var retryCount = context.PreviousRetryCount;
        if (retryCount > 10)
            retryCount = 10;
        
        return TimeSpan.FromSeconds(Math.Pow(2, retryCount));
    }
}