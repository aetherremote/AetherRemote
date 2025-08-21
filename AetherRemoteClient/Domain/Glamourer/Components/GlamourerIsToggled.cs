namespace AetherRemoteClient.Domain.Glamourer.Components;

public class GlamourerIsToggled
{
    public bool Apply;
    public bool IsToggled;
    
    public GlamourerIsToggled Clone() => (GlamourerIsToggled)MemberwiseClone();

    public bool IsEqualTo(GlamourerIsToggled other)
    {
        if (Apply != other.Apply) return false;
        if (IsToggled != other.IsToggled) return false;
        return true;
    }
}