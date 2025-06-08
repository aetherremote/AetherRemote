using MessagePack;

namespace AetherRemoteCommon.V2.Domain.Network.AddFriend;

[MessagePackObject(keyAsPropertyName: true)]
public class AddFriendRequest
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