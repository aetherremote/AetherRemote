using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Services;

namespace AetherRemoteClient.UI.Views.History;

public partial class HistoryView : IDrawable
{
    public HistoryView(
        LogService logService)
    {
        _logs = new ListFilter<InternalLog>(logService.Logs, FilterPredicate);
    }
}