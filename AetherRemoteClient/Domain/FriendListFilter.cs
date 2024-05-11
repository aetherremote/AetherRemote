using System.Collections.Generic;
using System;
using AetherRemoteCommon.Domain.CommonFriend;
using System.Timers;
using System.Threading.Tasks;
using AetherRemoteClient.Providers;

namespace AetherRemoteClient.Domain;

public class FriendListFilter : IDisposable
{
    private const int DelayStartFilter = 256;

    private readonly Timer timer;
    private readonly NetworkProvider networkProvider;
    private readonly Func<Friend, string, bool> filterPredicate;
    private readonly List<Friend> filteredList = [];

    private string searchTerm = string.Empty;

    public List<Friend> List
    {
        get
        {
            if (searchTerm == string.Empty)
                return networkProvider.FriendList?.Friends ?? [];

            return filteredList;
        }
    }

    public FriendListFilter(NetworkProvider networkProvider, Func<Friend, string, bool> filterPredicate)
    {
        timer = new Timer(DelayStartFilter);
        timer.Elapsed += async (sender, e) => await Task.Run(FilterList);

        this.networkProvider = networkProvider;
        this.filterPredicate = filterPredicate;
    }

    private void FilterList()
    {
        filteredList.Clear();
        foreach (var item in networkProvider.FriendList?.Friends ?? [])
        {
            if (filterPredicate.Invoke(item, searchTerm))
                filteredList.Add(item);
        }
    }

    public void UpdateSearchTerm(string newSearchTerm)
    {
        if (searchTerm == newSearchTerm)
            return;

        searchTerm = newSearchTerm;
        timer.Stop();
        timer.Start();
    }

    public void Dispose()
    {
        timer.Dispose();
        GC.SuppressFinalize(this);
    }
}
