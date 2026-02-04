using AetherRemoteCommon.Domain;

namespace AetherRemoteCommon.Util;

/// <summary>
///     Provides methods for helping resolve permissions
/// </summary>
public static class PermissionResolver
{
    /// <summary>
    ///     Resolve a set of permissions, collapsing them into a set of three enums
    /// </summary>
    public static ResolvedPermissions Resolve(ResolvedPermissions global, RawPermissions raw)
    {
        var effectivePrimary = (global.Primary | raw.PrimaryAllow) & ~raw.PrimaryDeny;
        var effectiveSpeak = (global.Speak | raw.SpeakAllow) & ~raw.SpeakDeny;
        var effectiveElevated = (global.Elevated | raw.ElevatedAllow) & ~raw.ElevatedDeny;
        return new ResolvedPermissions(effectivePrimary, effectiveSpeak, effectiveElevated);
    }
}