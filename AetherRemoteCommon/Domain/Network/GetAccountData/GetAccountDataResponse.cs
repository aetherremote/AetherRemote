using MessagePack;

namespace AetherRemoteCommon.Domain.Network.GetAccountData;

// ReSharper disable PropertyCanBeMadeInitOnly.Global

[MessagePackObject(keyAsPropertyName: true)]
public record GetAccountDataResponse
{
    public GetAccountDataEc Result { get; set; }
    public string FriendCode { get; set; } = string.Empty;
    public List<FriendRelationship> Relationships { get; set; } = [];

    public GetAccountDataResponse()
    {
    }
    
    public GetAccountDataResponse(GetAccountDataEc result)
    {
        Result = result;
    }

    public GetAccountDataResponse(string friendCode, List<FriendRelationship> relationships)
    {
        Result = GetAccountDataEc.Success;
        FriendCode = friendCode;
        Relationships = relationships;
    }
}