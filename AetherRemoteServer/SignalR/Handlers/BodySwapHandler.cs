using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.New;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.V2.Domain.Enum;
using AetherRemoteCommon.V2.Domain.Network;
using AetherRemoteCommon.V2.Domain.Network.BodySwap;
using AetherRemoteServer.Domain.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Handlers;

/// <summary>
///     Handles the logic for fulfilling a <see cref="BodySwapRequest"/>
/// </summary>
public class BodySwapHandler(
    IConnectionsService connections,
    IForwardedRequestManager forwardedRequestManager,
    ILogger<BodySwapHandler> logger)
{
    /// <summary>
    ///     Handles the request
    /// </summary>
    public async Task<BodySwapResponse> Handle(string sender, BodySwapRequest request, IHubCallerClients clients)
    {
        if (connections.IsUserExceedingRequestLimit(sender))
        {
            logger.LogWarning("{Friend} exceeded request limit", sender);
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

        var characterNames = new List<string>();
        foreach (var targetFriendCode in targets)
        {
            if (connections.TryGetClient(targetFriendCode) is not { } client)
                return new BodySwapResponse(ActionResponseEc.TargetOffline);

            characterNames.Add(client.CharacterName);
        }
        
        if (request.SenderCharacterName is not null)
            characterNames.Add(request.SenderCharacterName);
        
        var deranged = Derange(characterNames);
        return await forwardedRequestManager.SendBodySwap(sender, targets, deranged, request.SwapAttributes, clients);
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