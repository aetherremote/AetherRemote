using AetherRemoteClient.UI.Tabs.Control;
using AetherRemoteCommon.Domain.CommonFriend;
using System.Collections.Generic;
using System.Linq;

namespace AetherRemoteClient.Domain;

/// <summary>
/// For use with <see cref="ControlTab"/> to manage selected targets for mass control
/// </summary>
public class ControlTargetManager
{
    /// <summary>
    /// List of targets currently selected
    /// </summary>
    public readonly List<Friend> Targets = [];

    /// <summary>
    /// The number of targets required to send a command
    /// </summary>
    public readonly int MinimumTargetsRequired = 1;

    /// <summary>
    /// The max number of people allowed for Speak or Emote commands
    /// </summary>
    public readonly int MaximumTargetsForInGameOperations = 3;

    /// <summary>
    /// How the controller target manager should handle selecting friends
    /// </summary>
    public SelectionMode Mode { get; private set; } = SelectionMode.Single;

    /// <summary>
    /// If the minimum required number of targets have been met
    /// </summary>
    public bool MinimumTargetsMet => Targets.Count >= MinimumTargetsRequired;

    /// <summary>
    /// Update Selection Mode
    /// </summary>
    public void UpdateSelectionMode(SelectionMode Mode)
    {
        this.Mode = Mode;
        if (Mode == SelectionMode.Single)
        {
            if (Targets.Count > 1)
                Targets.RemoveRange(1, Targets.Count - 1);
        }
    }

    /// <summary>
    /// Returns if a friend is selected
    /// </summary>
    public bool IsSelected(Friend friend)
    {
        return Targets.Any(target => target.FriendCode == friend.FriendCode);
    }

    /// <summary>
    /// Toggles a friend's selected status
    /// </summary>
    public void ToggleSelected(Friend friend)
    {
        if (Mode == SelectionMode.Single)
        {
            Targets.Clear();
            Targets.Add(friend);
        }
        else
        {
            var index = Targets.FindIndex(target => target.FriendCode == friend.FriendCode);
            if (index < 0)
                Targets.Add(friend);
            else
                Targets.RemoveAt(index);
        }
    }

    /// <summary>
    /// Removes target
    /// </summary>
    public void Deselect(Friend friend)
    {
        Targets.RemoveAll(target => target.FriendCode == friend.FriendCode);
    }

    /// <summary>
    /// Removes all targets
    /// </summary>
    public void DeselectAll()
    {
        Targets.Clear();
    }

    public enum SelectionMode
    {
        Single,
        Multiple
    }
}
