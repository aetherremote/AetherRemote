using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.BodySwap;
using AetherRemoteServer.Domain.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.Managers;

/// <summary>
///     <inheritdoc cref="IForwardedRequestManager"/>
/// </summary>
public class ForwardedRequestManager(IConnectionsService connections, IDatabaseService database, ILogger<ForwardedRequestManager> logger) : IForwardedRequestManager
{
    private static readonly TimeSpan TimeOutDuration = TimeSpan.FromSeconds(8);
    
    public async Task<ActionResponse> CheckPermissionsAndSend(string sender, List<string> targets, string method, UserPermissions permissions, ForwardedActionRequest request, IHubCallerClients clients)
    {
        var results = new Dictionary<string, ActionResultEc>();
        var pending = new Task<ActionResult<Unit>>[targets.Count];
        for (var i = 0; i < targets.Count; i++)
        {
            var targetFriendCode = targets[i];
            if (connections.TryGetClient(targetFriendCode) is not { } connectionClient)
            {
                pending[i] = Task.FromResult(ActionResultBuilder.Fail(ActionResultEc.TargetOffline));
                continue;
            }

            var targetPermissions = await database.GetPermissions(targetFriendCode);
            if (targetPermissions.Permissions.TryGetValue(sender, out var permissionsGranted) is false)
            {
                pending[i] = Task.FromResult(ActionResultBuilder.Fail(ActionResultEc.TargetNotFriends));
                continue;
            }
            
            var primary = (permissionsGranted.Primary & permissions.Primary) == permissions.Primary;
            var speak = (permissionsGranted.Speak & permissions.Speak) == permissions.Speak;
            var elevated = (permissionsGranted.Elevated & permissions.Elevated) == permissions.Elevated;
            if (primary is false || speak is false || elevated is false)
            {
                pending[i] = Task.FromResult(ActionResultBuilder.Fail(ActionResultEc.TargetHasNotGrantedSenderPermissions));
                continue;
            }
            
            try
            {
                var client = clients.Client(connectionClient.ConnectionId);
                pending[i] = ForwardRequestWithTimeout<Unit>(method, client, request);
            }
            catch (Exception e)
            {
                logger.LogWarning("{Issuer} send action to {Target} failed, {Error}", sender, targetFriendCode, e.Message);
                pending[i] = Task.FromResult(ActionResultBuilder.Fail(ActionResultEc.Unknown));
            }
        }
        
        var completed = await Task.WhenAll(pending).ConfigureAwait(false);
        for(var i = 0; i < completed.Length; i++)
            results.Add(targets[i], completed[i].Result);

        return new ActionResponse(results);
    }
    
    public async Task<BodySwapResponse> CheckPermissionsAndSendBodySwap(string sender, List<string> targets, List<string> characters, CharacterAttributes attributes, UserPermissions permissions, IHubCallerClients clients)
    {
        var results = new Dictionary<string, ActionResultEc>();
        var pending = new Task<ActionResult<Unit>>[targets.Count];
        for (var i = 0; i < targets.Count; i++)
        {
            var targetFriendCode = targets[i];
            if (connections.TryGetClient(targetFriendCode) is not { } connectionClient)
            {
                pending[i] = Task.FromResult(ActionResultBuilder.Fail(ActionResultEc.TargetOffline));
                continue;
            }

            var targetPermissions = await database.GetPermissions(targetFriendCode);
            if (targetPermissions.Permissions.TryGetValue(sender, out var permissionsGranted) is false)
            {
                pending[i] = Task.FromResult(ActionResultBuilder.Fail(ActionResultEc.TargetNotFriends));
                continue;
            }
            
            var primary = (permissionsGranted.Primary & permissions.Primary) == permissions.Primary;
            var elevated = (permissionsGranted.Elevated & permissions.Elevated) == permissions.Elevated;
            if (primary is false || elevated is false)
            {
                pending[i] = Task.FromResult(ActionResultBuilder.Fail(ActionResultEc.TargetHasNotGrantedSenderPermissions));
                continue;
            }
            
            var request = new BodySwapForwardedRequest(sender, characters[i], attributes);
            
            try
            {
                var client = clients.Client(connectionClient.ConnectionId);
                pending[i] = ForwardRequestWithTimeout<Unit>(HubMethod.BodySwap, client, request);
            }
            catch (Exception e)
            {
                logger.LogWarning("{Issuer} send action to {Target} failed, {Error}", sender, targetFriendCode, e.Message);
                pending[i] = Task.FromResult(ActionResultBuilder.Fail(ActionResultEc.Unknown));
            }
        }
        
        var completed = await Task.WhenAll(pending).ConfigureAwait(false);
        for(var i = 0; i < completed.Length; i++)
            results.Add(targets[i], completed[i].Result);
        
        return new BodySwapResponse(results, targets.Count < characters.Count ? characters[^1] : null);
    }
    
    /// <summary>
    ///     Forwards a request to a client with a timeout of 8 seconds
    /// </summary>
    public static Task<ActionResult<T>> ForwardRequestWithTimeout<T>(
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