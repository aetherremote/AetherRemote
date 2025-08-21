using System;

namespace AetherRemoteClient.Domain.Glamourer.Components;

public class GlamourerPercentage
{
    private const float Tolerance = 1e-5f;

    public bool Apply;
    public float Percentage;

    public GlamourerPercentage Clone() => (GlamourerPercentage)MemberwiseClone();
    
    public bool IsEqualTo(GlamourerPercentage other)
    {
        if (Apply != other.Apply) return false;
        if (Math.Abs(Percentage - other.Percentage) > Tolerance) return false;
        return true;
    }
}