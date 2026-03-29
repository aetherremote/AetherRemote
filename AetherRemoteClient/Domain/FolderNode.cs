using System;
using System.Collections.Generic;
using System.Linq;

namespace AetherRemoteClient.Domain;

public class FolderNode<T>(string name, T? content, Dictionary<string, FolderNode<T>>? children = null)
{
    public readonly string Name = name;
    public readonly T? Content = content;
    public readonly Dictionary<string, FolderNode<T>> Children = children ?? [];

    public bool IsFolder => Content is null;
    
    /// <summary>
    ///     Sort the folder structure recursively
    /// </summary>
    public static void SortTree(FolderNode<T> root)
    {
        // Copy all the children from this node and sort them by folder, then name
        var sorted = root.Children.Values
            .OrderByDescending(node => node.IsFolder)
            .ThenBy(node => node.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
        
        // Clear all the children with the values sorted and copied
        root.Children.Clear();

        // Reintroduce because dictionaries preserve insertion order
        foreach (var node in sorted)
            root.Children[node.Name] = node;
        
        // Recursively sort the remaining children
        foreach (var child in root.Children.Values)
            SortTree(child);
    }
}