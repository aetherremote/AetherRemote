using System.Collections.Generic;

namespace AetherRemoteClient.Domain;

public class FolderNode<T>(string name, T? content, Dictionary<string, FolderNode<T>>? children)
{
    public readonly string Name = name;
    public readonly T? Content = content;
    public readonly Dictionary<string, FolderNode<T>> Children = children ?? [];
}