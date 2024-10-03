using System;
using System.Collections.Generic;

namespace AetherRemoteClient.Domain;

/// <summary>
/// Filters a list by search term with caching support to reduce repeat calls
/// </summary>
public class ListFilter<T>
{
    private readonly List<T> source;
    private readonly Func<T, string, bool> filterPredicate;

    private List<T> filteredList;
    private string searchTerm = string.Empty;

    /// <summary>
    /// The filtered list
    /// </summary>
    public List<T> List
    {
        get
        {
            if (searchTerm == string.Empty)
                return source;

            return filteredList;
        }
    }

    /// <summary>
    /// <inheritdoc cref="ListFilter{T}"/>
    /// </summary>
    public ListFilter(List<T> source, Func<T, string, bool> filterPredicate)
    {
        this.source = source;
        filteredList = source;

        this.filterPredicate = filterPredicate;
    }

    /// <summary>
    /// Updates the filter search term, and apply new filter it if the search term changed
    /// </summary>
    public void UpdateSearchTerm(string searchTerm)
    {
        if (this.searchTerm == searchTerm) return;

        var filteredList = new List<T>();
        foreach (var item in source)
        {
            if (filterPredicate.Invoke(item, searchTerm))
                filteredList.Add(item);
        }

        this.searchTerm = searchTerm;
        this.filteredList = filteredList;
    }
}
