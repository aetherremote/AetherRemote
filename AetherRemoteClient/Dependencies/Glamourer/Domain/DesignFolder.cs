using System.Collections.Generic;

namespace AetherRemoteClient.Dependencies.Glamourer.Domain;

public record DesignFolder(string Path, List<Design> Designs);