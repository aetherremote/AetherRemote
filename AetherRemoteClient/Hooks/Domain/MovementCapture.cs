using System;

namespace AetherRemoteClient.Hooks.Domain;

public readonly struct MovementCapture(float horizontal, float vertical, float turn, byte backwards)
{
    private const float Tolerance = 0.00001F;
    
    public readonly float Horizontal = horizontal;
    public readonly float Vertical = vertical;
    public readonly float Turn = turn;
    public readonly byte Backwards = backwards;

    public bool IsEqualTo(MovementCapture other)
    {
        return Math.Abs(Horizontal - other.Horizontal) < Tolerance 
               && Math.Abs(Vertical - other.Vertical) < Tolerance 
               && Math.Abs(Turn - other.Turn) < Tolerance 
               && Backwards == other.Backwards;
    }
}