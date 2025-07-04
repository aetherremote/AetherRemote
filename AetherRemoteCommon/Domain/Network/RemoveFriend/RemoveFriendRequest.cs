using MessagePack;

namespace AetherRemoteCommon.V2.Domain.Network.RemoveFriend;

[MessagePackObject(keyAsPropertyName: true)]
public record RemoveFriendRequest
{
    public string TargetFriendCode { get; set; } = string.Empty;

    public RemoveFriendRequest()
    {
    }

    public RemoveFriendRequest(string targetFriendCode)
    {
        TargetFriendCode = targetFriendCode;
    }
}