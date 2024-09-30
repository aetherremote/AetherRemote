using AetherRemoteCommon.Domain;

namespace AetherRemoteServer.Domain;

/// <summary>
/// Represents a user stored in the database
/// </summary>
public readonly struct UserDb(string friendCode, string secret, bool isAdmin)
{
    public readonly string FriendCode = friendCode;
    public readonly string Secret = secret;
    public readonly bool IsAdmin = isAdmin;

    public override string ToString()
    {
        var sb = new AetherRemoteStringBuilder("UserDb");
        sb.AddVariable("FriendCode", FriendCode);
        sb.AddVariable("Secret", Secret);
        sb.AddVariable("IsAdmin", IsAdmin);
        return sb.ToString();
    }
}
