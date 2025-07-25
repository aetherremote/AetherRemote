using System;
using Glamourer.Api.Enums;

namespace AetherRemoteClient.Domain.Events;

/// <summary>
///     Event data for when the glamourer state has changed meaningfully 
/// </summary>
public class GlamourerStateChangedEventArgs(StateFinalizationType stateChangeType) : EventArgs
{
    public readonly StateFinalizationType StateChangeType = stateChangeType;
}