namespace AetherRemoteClient.Domain.Dependencies.Glamourer.Components;

public class GlamourerBonus
{
    public bool Apply;
    public ulong BonusId;

    public GlamourerBonus Clone() => (GlamourerBonus)MemberwiseClone();

    public bool IsEqualTo(GlamourerBonus other)
    {
        if (BonusId != other.BonusId) return false;
        if (Apply != other.Apply) return false;
        return true;
    }
}