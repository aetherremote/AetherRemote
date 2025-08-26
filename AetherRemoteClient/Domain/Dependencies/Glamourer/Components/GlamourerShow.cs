namespace AetherRemoteClient.Domain.Dependencies.Glamourer.Components;

public class GlamourerShow
{
    public bool Apply;
    public bool Show;

    public GlamourerShow Clone() => (GlamourerShow)MemberwiseClone();

    public override string ToString()
    {
        return $"Show: {Show}, Apply: {Apply}, Show: {Show}";
    }
    
    public bool IsEqualTo(GlamourerShow other)
    {
        if (Apply != other.Apply) return false;
        if (Show != other.Show) return false;
        return true;
    }
}