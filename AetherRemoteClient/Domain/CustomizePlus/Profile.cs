using System;

namespace AetherRemoteClient.Domain.CustomizePlus;

/// <summary>
///     Represents a CustomizePlus profile
/// </summary>
public record Profile(Guid Id, string Name, string Path);