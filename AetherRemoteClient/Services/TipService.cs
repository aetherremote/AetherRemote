using System;

namespace AetherRemoteClient.Services;

/// <summary>
///     
/// </summary>
public class TipService
{
    private static readonly Random Random = new();

    private static readonly string[] Tips =
    [
        "You can hold CTRL and click to select multiple people at the same time",
        "Keep your plugins up to date to ensure compatibility"
    ];

    /// <summary>
    ///     The current tip to be displayed
    /// </summary>
    public string CurrentTip = Tips[Random.Next(0, Tips.Length)];

    private int _lastTipIndex;

    /// <summary>
    ///     Get a random tip that wasn't the last one
    /// </summary>
    public void NextTip()
    {
        var next = Random.Next(0, Tips.Length);
        while (next == _lastTipIndex)
        {
            next = Random.Next(0, Tips.Length);
        }

        _lastTipIndex = next;
        CurrentTip = Tips[next];
    }
}