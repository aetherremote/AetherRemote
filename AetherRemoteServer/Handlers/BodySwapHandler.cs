using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteServer.Managers;
using AetherRemoteServer.Services;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.Handlers;

/// <summary>
///     Handles the logic for fulfilling a <see cref="BodySwapRequest"/>
/// </summary>
public class BodySwapHandler(
    DatabaseService databaseService,
    ConnectedClientsManager connectedClientsManager,
    ILogger<BodySwapHandler> logger)
{
    /// <summary>
    ///     Handles the request
    /// </summary>
    public async Task<BodySwapResponse> Handle(string issuerFriendCode, BodySwapRequest request, IHubCallerClients clients)
    {
        if (connectedClientsManager.IsUserExceedingRequestLimit(issuerFriendCode))
        {
            logger.LogWarning("{Friend} exceeded request limit", issuerFriendCode);
            return new BodySwapResponse { Success = false, Message = "Exceeded request limit" };
        }

        switch (request.TargetFriendCodes.Count)
        {
            case 0 when request.Identity is null:
                logger.LogWarning("{Friend} tried to swap with no targets", issuerFriendCode);
                return new BodySwapResponse { Success = false, Message = "No targets selected" };

            case 0:
                logger.LogWarning("{Friend} tried to swap bodies with themself", issuerFriendCode);
                return new BodySwapResponse { Success = false, Message = "Cannot body swap with just yourself" };

            case 1 when request.Identity is null:
                logger.LogWarning("{Friend} tried to swap bodies with only one target", issuerFriendCode);
                return new BodySwapResponse { Success = false, Message = "You must select more than one target" };
        }

        var cancel = new CancellationTokenSource();
        var tasks = new Task<BodySwapQueryResponse>[request.TargetFriendCodes.Count];
        var connections = new string[request.TargetFriendCodes.Count];
        for (var i = 0; i < request.TargetFriendCodes.Count; i++)
        {
            var target = request.TargetFriendCodes[i];
            if (connectedClientsManager.ConnectedClients.TryGetValue(target, out var client) is false)
            {
                logger.LogWarning("{Issuer} targeted {Target} but they are offline, aborting", issuerFriendCode, target);
                await cancel.CancelAsync();
                return new BodySwapResponse
                {
                    Success = false,
                    Message = "One or more targets offline"
                };
            }

            var permissions = await databaseService.GetPermissions(target);
            if (permissions.Permissions.TryGetValue(issuerFriendCode, out var permissionsGranted) is false)
            {
                logger.LogWarning("{Issuer} targeted {Target} who is not a friend, aborting", issuerFriendCode, target);
                await cancel.CancelAsync();
                return new BodySwapResponse
                {
                    Success = false,
                    Message = "You are not friends with one or more targets"
                };
            }

            if (permissionsGranted.Primary.HasFlag(PrimaryPermissions.BodySwap) is false)
            {
                logger.LogWarning("{Issuer} targeted {Target} but lacks permissions, aborting", issuerFriendCode, target);
                await cancel.CancelAsync();
                return new BodySwapResponse
                {
                    Success = false,
                    Message = "You are lacking body swap permissions with one or more targets"
                };
            }
            
            if (request.SwapAttributes.HasFlag(CharacterAttributes.Mods) && permissionsGranted.Primary.HasFlag(PrimaryPermissions.Mods) is false)
            {
                logger.LogWarning("{Issuer} targeted {Target} but lacks mod permissions, aborting", issuerFriendCode, target);
                await cancel.CancelAsync();
                return new BodySwapResponse
                {
                    Success = false,
                    Message = "You are lacking mod permissions with one or more targets"
                };
            }

            var query = new BodySwapQueryRequest { SenderFriendCode = issuerFriendCode };
            try
            {
                tasks[i] = clients.Client(client.ConnectionId)
                    .InvokeAsync<BodySwapQueryResponse>(HubMethod.BodySwapQuery, query, cancel.Token);
            }
            catch (Exception e)
            {
                logger.LogError("{Issuer} query action to {Target} failed, {Error}", issuerFriendCode, target, e.Message);
                await cancel.CancelAsync();
                return new BodySwapResponse
                {
                    Success = false,
                    Message = "Unknown Error"
                };
            }

            connections[i] = client.ConnectionId;
        }

        var timeout = Task.Delay(10000, cancel.Token);
        var pending = Task.WhenAll(tasks);

        var waiting = await Task.WhenAny(pending, timeout);
        if (waiting == timeout)
        {
            logger.LogError("{Issuer} request timed out, aborting", issuerFriendCode);
            await cancel.CancelAsync();
            return new BodySwapResponse
            {
                Success = false,
                Message = "Request timed out"
            };
        }

        var responses = await pending;
        var identities = new List<CharacterIdentity>();
        foreach (var response in responses)
        {
            if (response.Identity is null)
            {
                logger.LogError("{Issuer} request had a target who couldn't offer their body, aborting", issuerFriendCode);
                await cancel.CancelAsync();
                return new BodySwapResponse
                {
                    Success = false,
                    Message = "One or more targets unavailable to swap"
                };
            }

            identities.Add(response.Identity);
        }

        if (request.Identity is not null)
            identities.Add(request.Identity);

        var deranged = Derange(identities);
        for (var i = 0; i < request.TargetFriendCodes.Count; i++)
        {
            var command = new BodySwapAction
            {
                SenderFriendCode = issuerFriendCode,
                SwapAttributes = request.SwapAttributes,
                Identity = deranged[i]
            };

            try
            {
                await clients.Client(connections[i]).SendAsync(HubMethod.BodySwap, command, cancel.Token);
            }
            catch (Exception e)
            {
                logger.LogError("{Issuer} request aborting due to unknown error, {Error}", issuerFriendCode, e.Message);
                await cancel.CancelAsync();
                return new BodySwapResponse
                {
                    Success = false,
                    Message = "Unknown Error"
                };
            }
        }

        return new BodySwapResponse
        {
            Success = true,
            Identity = request.Identity is null ? null : deranged[^1]
        };
    }

    /// <summary>
    ///     Derange a list, ensuring every element ends up in an index different from its starting position
    /// </summary>
    /// <param name="input">A list of more than two elements</param>
    /// <returns>The deranged list</returns>
    private static List<T> Derange<T>(List<T> input)
    {
        if (input.Count < 2)
            return input;

        var size = input.Count;
        var derangement = new List<T>(input);
        var random = new Random();

        do
        {
            for (var i = 0; i < size; i++)
            {
                var j = random.Next(size);
                (derangement[i], derangement[j]) = (derangement[j], derangement[i]);
            }
        } while (ValidDerangement(input, derangement) is false);

        return derangement;
    }

    /// <summary>
    ///     Test if the list is in a deranged state
    /// </summary>
    private static bool ValidDerangement<T>(List<T> original, List<T> deranged)
    {
        // ReSharper disable once LoopCanBeConvertedToQuery
        for (var i = 0; i < original.Count; i++)
        {
            if (EqualityComparer<T>.Default.Equals(original[i], deranged[i]))
                return false;
        }

        return true;
    }
}