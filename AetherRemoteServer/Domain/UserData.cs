using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.CommonFriend;

namespace AetherRemoteServer.Domain;

[Serializable]
public class UserData
{
    public string Secret { get; set; } = string.Empty;
    public string FriendCode { get; set; } = string.Empty;
    public List<Friend> FriendList { get; set; } = [];

    public override string ToString()
    {
        var sb = new AetherRemoteStringBuilder("UserData");
        sb.AddVariable("Secret", Secret);
        sb.AddVariable("FriendCode", FriendCode);
        sb.AddVariable("FriendList", FriendList);
        return sb.ToString();
    }
}
