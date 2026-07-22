using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Network.Domain;
using AetherRemoteCommon.Network.Enums;
using AetherRemoteServer.Services;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.Managers;

public class RelayManager(PermissionsService permissionsService, PresenceService presenceService)
{
    private readonly TimeSpan _timeOutDuration = TimeSpan.FromSeconds(8);

    /// <summary>
    ///     Simple relayer, taking the content of a request and forwarding
    /// </summary>
    public async Task<Dictionary<string, RoutedResponseStatus>> Relay<TPayload, TResponse>(
        string senderFriendCode,
        string hubMethod, 
        Request<TPayload> request, 
        ResolvedPermissions required, 
        IHubCallerClients clients)
    {
        var count = request.TargetFriendCodes.Count;
        var routed = new RoutedRequest<TPayload>(senderFriendCode, request.Payload);
        var tasks = new Task<RoutedResponse<TResponse>>[count];
        for (var i = 0; i < count; i++)
        {
            if (await permissionsService.ValidatePermissions(senderFriendCode, request.TargetFriendCodes[i], required) is { } error)
            {
                tasks[i] = Task.FromResult(new RoutedResponse<TResponse>(error));
                continue;
            }

            if (presenceService.TryGet(request.TargetFriendCodes[i]) is not { } presence)
            {
                tasks[i] = Task.FromResult(new RoutedResponse<TResponse>(RoutedResponseStatus.Offline));
                continue;
            }

            tasks[i] = Send<TPayload, TResponse>(hubMethod, routed, clients.Client(presence.ConnectionId));
        }

        var completed = await Task.WhenAll(tasks);
        var results = new Dictionary<string, RoutedResponseStatus>(count);
        for (var i = 0; i < count; i++)
            results[request.TargetFriendCodes[i]] = completed[i].Status;
        return results;
    }

    /// <summary>
    ///     Directly send a routed request to a single client
    /// </summary>
    public async Task<RoutedResponse<TRoutedResponsePayload>> Send<TRequestPayload, TRoutedResponsePayload>(
        string method, 
        RoutedRequest<TRequestPayload> routed, 
        ISingleClientProxy client)
    {
        using var token = new CancellationTokenSource(_timeOutDuration);

        try
        {
            return await client.InvokeAsync<RoutedResponse<TRoutedResponsePayload>>(method, routed, token.Token);
        }
        catch (OperationCanceledException)
        {
            return new RoutedResponse<TRoutedResponsePayload>(RoutedResponseStatus.Timeout);
        }
        catch
        {
            return new RoutedResponse<TRoutedResponsePayload>(RoutedResponseStatus.Unknown);
        }
    }
}