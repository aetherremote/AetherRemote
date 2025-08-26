namespace AetherRemoteClient.Domain.Dependencies.Glamourer.Components;

public class GlamourerItem
{
    public bool Apply;
    public bool ApplyCrest;
    public bool ApplyStain;
    public bool Crest;
    public uint ItemId;
    public uint Stain;
    public uint Stain2;

    public GlamourerItem Clone() => (GlamourerItem)MemberwiseClone();

    public bool IsEqualTo(GlamourerItem other)
    {
        if (Apply != other.Apply) return false;
        if (ApplyCrest != other.ApplyCrest) return false;
        if (ApplyStain != other.ApplyStain) return false;
        if (Crest != other.Crest) return false;
        if (ItemId != other.ItemId) return false;
        if (Stain != other.Stain) return false;
        if (Stain2 != other.Stain2) return false;
        return true;
    }
}