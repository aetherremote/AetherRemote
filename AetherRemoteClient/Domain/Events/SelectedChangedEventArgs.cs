using System;
using System.Collections.Generic;

namespace AetherRemoteClient.Domain.Events;

/// <summary>
///     Event data for when the selected friends are changed
/// </summary>
/// <param name="selected"></param>
public class SelectedChangedEventArgs(HashSet<Friend> selected) : EventArgs
{
    /// <summary>
    ///     The new list of selected friends
    /// </summary>
    public readonly HashSet<Friend> Selected = selected;
}