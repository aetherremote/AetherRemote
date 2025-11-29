using System;

namespace AetherRemoteClient.Dependencies.CustomizePlus.Domain;

/// <summary>
///     Represents a CustomizePlus profile
/// </summary>
public record Profile(Guid Guid, string Name, string Path);