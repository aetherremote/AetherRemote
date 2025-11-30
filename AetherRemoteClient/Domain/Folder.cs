using System.Collections.Generic;

namespace AetherRemoteClient.Domain;

public record Folder<T>(string Path, List<T> Content);