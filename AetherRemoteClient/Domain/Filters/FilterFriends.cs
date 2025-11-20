using System;
using System.Collections.Generic;
using System.Linq;
using AetherRemoteClient.Domain.Enums;

namespace AetherRemoteClient.Domain.Filters;

/// <summary>
///     Filters and sorts the friends list. This could be turned into a generic with some more work.
/// </summary>
public class FilterFriends(Func<IReadOnlyList<Friend>> source)
{
    // What to filter the search by
    private string _searchTerm = string.Empty;
    
    /// <summary>
    ///     Exposed filtered list
    /// </summary>
    public IReadOnlyList<Friend> List => _list;
    private List<Friend> _list = [];
    
    /// <summary>
    ///     The sorting mode to use
    /// </summary>
    public FilterSortMode SortMode = FilterSortMode.Alphabetically;

    /// <summary>
    ///     Updates the search term for filtering and refreshes the list
    /// </summary>
    /// <param name="searchTerm"></param>
    public void UpdateSearchTerm(string searchTerm)
    {
        if (_searchTerm == searchTerm)
            return;
        
        _searchTerm = searchTerm;
        
        Refresh();
    }
    
    /// <summary>
    ///     Refreshes the list, applying filtering and sorting as they are at the time of the refresh
    /// </summary>
    public void Refresh()
    { 
        var original = source.Invoke();

        var list = _searchTerm == string.Empty
            ? original
            : original.Where(friend => friend.NoteOrFriendCode.Contains(_searchTerm, StringComparison.OrdinalIgnoreCase));
        
        switch (SortMode)
        {
            case FilterSortMode.Alphabetically:
            default:
                _list = list.OrderBy(friend => friend.NoteOrFriendCode).ToList();
                break;
            
            case FilterSortMode.Recency:
                _list = list.OrderByDescending(friend => friend.LastInteractedWith).ToList();
                break;
        }
    }
}