using System;

namespace AetherRemoteClient.Dependencies.CustomizePlus.Domain;

/// <summary>
///     Represents a CustomizePlus profile
/// </summary>
public record ProfileIdentifier(Guid Guid, string Name, string Path);