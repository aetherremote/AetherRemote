using MessagePack;

namespace AetherRemoteCommon.Domain.Network.GetAccountData;

[MessagePackObject(keyAsPropertyName: true)]
public record GetAccountDataRequest
{
    public string CharacterName { get; set; } = string.Empty;

    public GetAccountDataRequest()
    {
    }

    public GetAccountDataRequest(string characterName)
    {
        CharacterName = characterName;
    }
}