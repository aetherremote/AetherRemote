using System;

namespace AetherRemoteClient.Domain.Dependencies.Glamourer.Components;

public class GlamourerColor
{
    private const float Tolerance = 1e-5f;

    public bool Apply;
    public float Red;
    public float Green;
    public float Blue;
    
    public GlamourerColor Clone() => (GlamourerColor)MemberwiseClone();

    public bool IsEqualTo(GlamourerColor other)
    {
        if (Apply !=  other.Apply) return false;
        if (Math.Abs(Red - other.Red) > Tolerance) return false;
        if (Math.Abs(Green - other.Green) > Tolerance) return false;
        if (Math.Abs(Blue - other.Blue) > Tolerance) return false;
        return true;
    }
}