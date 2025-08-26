using System;

namespace AetherRemoteClient.Domain.Dependencies.Glamourer.Components;

public class GlamourerColorAlpha
{
    private const float Tolerance = 1e-5f;

    public bool Apply;
    public float Red;
    public float Green;
    public float Blue;
    public float Alpha;
    
    public GlamourerColorAlpha Clone() => (GlamourerColorAlpha)MemberwiseClone();
    
    public bool IsEqualTo(GlamourerColorAlpha other)
    {
        if (Apply != other.Apply) return false;
        if (Math.Abs(Red - other.Red) > Tolerance) return false;
        if (Math.Abs(Green - other.Green) > Tolerance) return false;
        if (Math.Abs(Blue - other.Blue) > Tolerance) return false;
        if (Math.Abs(Alpha - other.Alpha) > Tolerance) return false;
        return true;
    }
}