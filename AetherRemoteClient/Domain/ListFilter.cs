using System;
using System.Collections.Generic;
using System.Linq;

namespace AetherRemoteClient.Domain;

/// <summary>
///     Filters a list by search term with caching support to reduce repeat calls
/// </summary>
public class ListFilter<T>(List<T> source, Func<T, string, bool> filterPredicate)
{
    private readonly List<T> _source = source;
    private List<T> _filteredList = source;
    private string _searchTerm = string.Empty;

    /// <summary>
    ///     The filtered list
    /// </summary>
    public List<T> List => _searchTerm == string.Empty ? _source : _filteredList;

    /// <summary>
    ///     Updates the filter search term, and apply new filter it if the search term changed
    /// </summary>
    public void UpdateSearchTerm(string searchTerm)
    {
        if (_searchTerm == searchTerm)
            return;

        var filteredList = _source.Where(t => filterPredicate.Invoke(t, searchTerm)).ToList();

        _searchTerm = searchTerm;
        _filteredList = filteredList;
    }
}