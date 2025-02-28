namespace AetherRemoteServer.Domain;

/// <summary>
///     Internal domain object representing a connected user
/// </summary>
public record User(string FriendCode, string Secret, bool IsAdmin);