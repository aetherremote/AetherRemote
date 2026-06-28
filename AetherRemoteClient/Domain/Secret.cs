using System;

namespace AetherRemoteClient.Domain;

/// <summary>
///     Domain representation of a secret and its properties
/// </summary>
public record Secret(long Id, string Name, string Value, DateTime CreatedAt);