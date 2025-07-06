using MessagePack;

namespace AetherRemoteCommon.Domain.Network.AddFriend;

[MessagePackObject(keyAsPropertyName: true)]
public record AddFriendRequest
{
    public string TargetFriendCode { get; set; } = string.Empty;

    public AddFriendRequest()
    {
    }

    public AddFriendRequest(string targetFriendCode)
    {
        TargetFriendCode = targetFriendCode;
    }
}