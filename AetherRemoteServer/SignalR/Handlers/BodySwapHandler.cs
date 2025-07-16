using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.BodySwap;
using AetherRemoteCommon.Util;
using AetherRemoteServer.Domain.Interfaces;
using AetherRemoteServer.Managers;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Handlers;

/// <summary>
///     Handles the logic for fulfilling a <see cref="BodySwapRequest"/>
/// </summary>
public class BodySwapHandler(
    IConnectionsService connections,
    IDatabaseService database,
    ILogger<BodySwapHandler> logger)
{
    /// <summary>
    ///     Handles the request
    /// </summary>
    public async Task<BodySwapResponse> Handle(string sender, BodySwapRequest request, IHubCallerClients clients)
    {
        if (connections.TryGetClient(sender) is not { } connectedClient)
        {
            logger.LogWarning("{Sender} tried to issue a command but is not in the connections list", sender);
            return new BodySwapResponse(ActionResponseEc.UnexpectedState);
        }

        if (connections.IsUserExceedingRequestLimit(connectedClient))
        {
            logger.LogWarning("{Sender} exceeded request limit", sender);
            return new BodySwapResponse(ActionResponseEc.TooManyRequests);
        }

        var targets = request.TargetFriendCodes;
        switch (targets.Count)
        {
            case 0 when request.SenderCharacterName is null:
                logger.LogWarning("{Friend} tried to swap with no targets", sender);
                return new BodySwapResponse(ActionResponseEc.TooFewTargets);

            case 0:
                logger.LogWarning("{Friend} tried to swap bodies with themself", sender);
                return new BodySwapResponse(ActionResponseEc.TooFewTargets);

            case 1 when request.SenderCharacterName is null:
                logger.LogWarning("{Friend} tried to swap bodies with only one target", sender);
                return new BodySwapResponse(ActionResponseEc.TooFewTargets);
        }
        
        // Convert the swap attributes to primary permissions
        var primary = request.SwapAttributes.ToPrimaryPermission();
        primary |= PrimaryPermissions2.BodySwap;

        // Check elevated
        var elevated = ElevatedPermissions.None;
        if (request.LockCode is not null)
            elevated = ElevatedPermissions.PermanentTransformation;
        
        // Build permission set to check against
        var permissions = new UserPermissions(primary, SpeakPermissions2.None, elevated);

        // Get the names of everyone involved in the swap
        var characters = new List<string>();
        foreach (var targetFriendCode in targets)
        {
            if (connections.TryGetClient(targetFriendCode) is not { } connectionClient)
                return new BodySwapResponse(ActionResponseEc.TargetOffline);

            var targetPermissions = await database.GetPermissions(targetFriendCode);
            if (targetPermissions.Permissions.TryGetValue(sender, out var permissionsGranted) is false)
                return new BodySwapResponse(ActionResponseEc.TargetBodySwapIsNotFriends);
            
            // Body swap will only every make use of primary and elevated permissions
            if (((permissionsGranted.Primary & permissions.Primary) == permissions.Primary) is false || ((permissionsGranted.Elevated & permissions.Elevated) == permissions.Elevated) is false)
                return new BodySwapResponse(ActionResponseEc.TargetBodySwapLacksPermissions);
            
            characters.Add(connectionClient.CharacterName);
        }

        // Including yourself if you marked as such
        if (request.SenderCharacterName is not null)
            characters.Add(request.SenderCharacterName);

        // Shuffle everyone around
        var deranged = Derange(characters);

        var results = new Dictionary<string, ActionResultEc>();
        var pending = new Task<ActionResult<Unit>>[targets.Count];
        for (var i = 0; i < targets.Count; i++)
        {
            var targetFriendCode = targets[i];
            
            // Construct the tailored request
            var forwarded = new BodySwapForwardedRequest(sender, deranged[i], request.SwapAttributes, request.LockCode);

            // Double-check the target is still online
            if (connections.TryGetClient(targetFriendCode) is not { } connectionClient)
                return new BodySwapResponse(ActionResponseEc.TargetOffline);
            
            try
            {
                var client = clients.Client(connectionClient.ConnectionId);
                pending[i] = ForwardedRequestManager.ForwardRequestWithTimeout<Unit>(HubMethod.BodySwap, client, forwarded);
            }
            catch (Exception e)
            {
                logger.LogWarning("{Issuer} send action to {Target} failed, {Error}", sender, targetFriendCode,
                    e.Message);
                pending[i] = Task.FromResult(ActionResultBuilder.Fail(ActionResultEc.Unknown));
            }
        }

        var completed = await Task.WhenAll(pending).ConfigureAwait(false);
        for (var i = 0; i < completed.Length; i++)
            results.Add(targets[i], completed[i].Result);

        return new BodySwapResponse(results, targets.Count < deranged.Count ? deranged[^1] : null);
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