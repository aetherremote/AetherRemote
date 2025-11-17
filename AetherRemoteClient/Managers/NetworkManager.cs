using System.Collections.Generic;
using System.Threading.Tasks;
using AetherRemoteClient.Dependencies.Moodles.Domain;
using AetherRemoteClient.Services;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.BodySwap;
using AetherRemoteCommon.Domain.Network.Moodles;

namespace AetherRemoteClient.Managers;

public class NetworkManager(FriendsListService friendsListService, NetworkService networkService)
{
    public async Task<BodySwapResponse> InvokeBodySwap(List<string> targets, CharacterAttributes attributes, string? senderCharacterName, string? lockCode)
    {
        var request = new BodySwapRequest(targets, attributes, senderCharacterName, lockCode);
        return await networkService.InvokeAsync<BodySwapResponse>(HubMethod.BodySwap, request).ConfigureAwait(false);
    }

    public async Task<ActionResponse> SendMoodle(Moodle moodle)
    {
        var request = new MoodlesRequest(friendsListService.SelectedFriendCodes, moodle.Info);
        return await networkService.InvokeAsync<ActionResponse>(HubMethod.Moodles, request).ConfigureAwait(false);
    }
}