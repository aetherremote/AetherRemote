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
    public static bool ValidListOfFriendCodes(List<string> friendCodes)
    {
        foreach (var friendCode in friendCodes)
            if (friendCode.Length is < Constraints.FriendCodeMinimumLength or > Constraints.FriendCodeMaximumLength)
                return false;

        return true;
    }

    /// <summary>
    ///     Are the message and extra lengths confined within the limitations we've defined in <see cref="Constraints.Speak"/>
    /// </summary>
    public static bool ValidMessageLengths(string message, string? extra)
    {
        if (message.Length is < Constraints.Speak.MessageMin or > Constraints.Speak.MessageMax)
            return false;

        if (extra is null)
            return true;

        return extra.Length is >= Constraints.Speak.MessageExtraMin and <= Constraints.Speak.MessageExtraMax;
    }
}