using System.Collections.Generic;
using System.Linq;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Interfaces;

namespace AetherRemoteClient.UI.Views;

/// <summary>
///     Domain concept to encapsulate all the views registered in the dependency injection framework
/// </summary>
public class ViewRegistry
{
    private readonly Dictionary<View, IView> _views;

    /// <summary> <inheritdoc cref="ViewRegistry"/> </summary>
    public ViewRegistry(IEnumerable<IView> views)
    {
        _views = views.ToDictionary(view => view.View);
    }
   
    /// <summary>
    ///     Get a view from associated enum value
    /// </summary>
    public IView Get(View view) => _views[view];
}