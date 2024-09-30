using System;
using System.Collections.Generic;

namespace AetherRemoteClient.Domain;

public class ListFilter<T>
{
    private readonly List<T> source;
    private readonly Func<T, string, bool> filterPredicate;

    private List<T> filteredList;
    private string searchTerm = string.Empty;

    public List<T> List
    {
        get
        {
            if (searchTerm == string.Empty)
                return source;

            return filteredList;
        }
    }

    public ListFilter(List<T> source, Func<T, string, bool> filterPredicate)
    {
        this.source = source;
        filteredList = source;

        this.filterPredicate = filterPredicate;
    }

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
