using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AetherRemoteClient.Dependencies.Glamourer.Domain;
using AetherRemoteClient.Domain;

namespace AetherRemoteClient.UI.Views.Transformations.Controllers;

public partial class TransformationsViewUiController
{
        /// <summary>
    ///     Search for the design we'd like to send
    /// </summary>
    public string SearchTerm = string.Empty;
    
    /// <summary>
    ///     The currently selected Guid of the Design to send
    /// </summary>
    public Guid SelectedDesignId = Guid.Empty;

    
    /// <summary>
    ///     Cached list of designs
    /// </summary>
    private List<FolderNode<Design>>? _sorted;
    
    /// <summary>
    ///     Filtered cached list of designs
    /// </summary>
    private List<FolderNode<Design>>? _filtered;
    
    /// <summary>
    ///     The designs to display in the Ui
    /// </summary>
    public List<FolderNode<Design>>? Designs => SearchTerm == string.Empty ? _sorted : _filtered;
    
    /// <summary>
    ///     Filters the sorted design list by search term
    /// </summary>
    public void FilterDesignsBySearchTerm()
    {
        _filtered = _sorted is not null 
            ? FilterFolderNodes(_sorted, SearchTerm).ToList() 
            : null;
    }
    
    /// <summary>
    ///     Recursive method to filter nodes based on both folders and content names
    /// </summary>
    private List<FolderNode<Design>> FilterFolderNodes(IEnumerable<FolderNode<Design>> nodes, string searchTerms)
    {
        // Reset the selected so possibly unselected designs aren't stored
        SelectedDesignId = Guid.Empty;
        
        // Iterate to determine what stays and what goes
        var results = new List<FolderNode<Design>>();
        foreach (var node in nodes)
        {
            // The recursive part, filtering on the children to see if there were any matches
            var children = FilterFolderNodes(node.Children.Values, searchTerms).ToDictionary(n => n.Name);

            // Check if the item inside the folder's name matches
            var matches = node.Content is not null && node.Content.Name.Contains(searchTerms, StringComparison.OrdinalIgnoreCase);
            
            // If this is a folder with no children, exclude it
            if (matches is false && children.Count is 0)
                continue;
            
            // Add
            results.Add(new FolderNode<Design>(node.Name, node.Content, children));
        }
        
        return results;
    }
    
    public async Task RefreshGlamourerDesigns()
    {
        SelectedDesignId = Guid.Empty;

        if (await _glamourerService.GetDesignList().ConfigureAwait(false) is not { } designs)
            return;
        
        var root = new FolderNode<Design>("Root", null);
        foreach (var design in designs)
        {
            var parts = design.Path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var current = root;

            for (var i = 0; i < parts.Length; i++)
            {
                var part = parts[i];
                if (current.Children.TryGetValue(part, out var node) is false)
                {
                    node = new FolderNode<Design>(part, i == parts.Length - 1 ? design : null);
                    current.Children[part] = node;
                }

                current = node;
            }
        }

        // The dictionary provided by glamourer is not sorted
        SortTree(root);
        
        // Assignment
        _sorted = root.Children.Values.ToList();
    }
    
    /// <summary>
    ///     The dictionary returned by glamourer is not sorted, so we will recursively go through and sort the children
    /// </summary>
    private static void SortTree<T>(FolderNode<T> root)
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