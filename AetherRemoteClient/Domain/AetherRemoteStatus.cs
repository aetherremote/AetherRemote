using System;

namespace AetherRemoteClient.Domain;

/// <summary>
///     Represents a status applied to a character via Aether Remote
/// </summary>
public record AetherRemoteStatus(Friend Applier, DateTime ApplicationTime);