using MessagePack;

namespace AetherRemoteCommon.Domain.Network.RemoveFriend;

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