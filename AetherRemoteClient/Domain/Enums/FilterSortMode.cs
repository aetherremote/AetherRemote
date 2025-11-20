namespace AetherRemoteClient.Domain.Enums;

/// <summary>
///     Applies sorting rules to a filtering class
/// </summary>
public enum FilterSortMode
{
    /// <summary>
    ///     Filter alphabetically
    /// </summary>
    Alphabetically,
    
    /// <summary>
    ///     Filter by the last time an item was interacted with
    /// </summary>
    Recency
}