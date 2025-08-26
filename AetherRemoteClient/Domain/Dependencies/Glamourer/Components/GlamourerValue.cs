namespace AetherRemoteClient.Domain.Dependencies.Glamourer.Components;

public class GlamourerValue
{
    public bool Apply;
    public uint Value;

    public GlamourerValue Clone() => (GlamourerValue)MemberwiseClone();
    
    public bool IsEqualTo(GlamourerValue other)
    {
        if (Apply != other.Apply) return false;
        if (Value != other.Value) return false;
        return true;
    }
}