using System.Text;
using AetherRemoteCommon;
using Newtonsoft.Json.Linq;

namespace AetherRemoteServer.Utilities;

public static class VerificationUtilities
{
    /// <summary>
    ///     Validates that the bytes provided are indeed JSON values
    /// </summary>
    public static bool IsJsonBytes(byte[] bytes)
    {
        try
        {
            var json = Encoding.UTF8.GetString(bytes);
            JObject.Parse(json);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    ///     Validates that all provided friend codes are within the allowed length
    /// </summary>
    public static bool IsValidListOfFriendCodes(List<string> friendCodes)
    {
        foreach (var friendCode in friendCodes)
            if (friendCode.Length is < Constraints.FriendCodeMinimumLength or > Constraints.FriendCodeMaximumLength)
                return false;

        return true;
    }
}