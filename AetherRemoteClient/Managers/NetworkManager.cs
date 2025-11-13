using System.Collections.Generic;
using System.Threading.Tasks;
using AetherRemoteClient.Services;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.BodySwap;

namespace AetherRemoteClient.Managers;

public class NetworkManager(NetworkService networkService)
{
    public async Task<BodySwapResponse> InvokeBodySwap(List<string> targets, CharacterAttributes attributes, string? senderCharacterName, string? lockCode)
    {
        var request = new BodySwapRequest(targets, attributes, senderCharacterName, lockCode);
        return await networkService.InvokeAsync<BodySwapResponse>(HubMethod.BodySwap, request);
    }
}