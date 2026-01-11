namespace AetherRemoteServer.Domain.Shared;

/// <summary>
///     An account registered to a discord id
/// </summary>
public record Account(string FriendCode, string Secret, bool Admin);