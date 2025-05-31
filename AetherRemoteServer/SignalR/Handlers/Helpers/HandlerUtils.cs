using AetherRemoteCommon.V2.Domain;
using AetherRemoteCommon.V2.Domain.Enum;
using AetherRemoteCommon.V2.Domain.Network.Base;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Handlers.Helpers;

public static class HandlerUtils
{
    private static readonly TimeSpan TimeOutDuration = TimeSpan.FromSeconds(8);

    public static Task<ActionResult<T>> AwaitResponsesWithTimeout<T>(
        string methodName,
        ISingleClientProxy clientProxy, 
        ForwardedActionRequest forwardedRequest)
    {
        var token = new CancellationTokenSource(TimeOutDuration);
        return clientProxy.InvokeAsync<ActionResult<T>>(methodName, forwardedRequest, token.Token)
            .ContinueWith(task =>
            {
                if (task.IsCanceled || token.IsCancellationRequested)
                    return ActionResultBuilder.Fail<T>(ActionResultEc.TargetTimeout);

                return task.IsFaulted
                    ? ActionResultBuilder.Fail<T>(ActionResultEc.Unknown)
                    : task.Result;
            }, TaskContinuationOptions.ExecuteSynchronously);
    }
}