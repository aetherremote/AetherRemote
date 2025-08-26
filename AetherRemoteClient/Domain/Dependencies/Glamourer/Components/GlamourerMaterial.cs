using System;

namespace AetherRemoteClient.Domain.Dependencies.Glamourer.Components;

public class GlamourerMaterial
{
    private const float Tolerance = 1e-5f;
    
    public bool Enabled;
    public bool Revert;
    public float Gloss;
    public float DiffuseR;
    public float DiffuseG;
    public float DiffuseB;
    public float EmissiveR;
    public float EmissiveG;
    public float EmissiveB;
    public float SpecularR;
    public float SpecularG;
    public float SpecularB;
    public float SpecularA;

    public GlamourerMaterial Clone() => (GlamourerMaterial)MemberwiseClone();

    public bool IsEqualTo(GlamourerMaterial other)
    {
        if (Enabled != other.Enabled) return false;
        if (Revert != other.Revert) return false;
        if (Math.Abs(Gloss - other.Gloss) > Tolerance) return false;
        if (Math.Abs(DiffuseR - other.DiffuseR) > Tolerance) return false;
        if (Math.Abs(DiffuseG - other.DiffuseG) > Tolerance) return false;
        if (Math.Abs(DiffuseB - other.DiffuseB) > Tolerance) return false;
        if (Math.Abs(EmissiveR - other.EmissiveR) > Tolerance) return false;
        if (Math.Abs(EmissiveG - other.EmissiveG) > Tolerance) return false;
        if (Math.Abs(EmissiveB - other.EmissiveB) > Tolerance) return false;
        if (Math.Abs(SpecularR - other.SpecularR) > Tolerance) return false;
        if (Math.Abs(SpecularG - other.SpecularG) > Tolerance) return false;
        if (Math.Abs(SpecularB - other.SpecularB) > Tolerance) return false;
        if (Math.Abs(SpecularA - other.SpecularA) > Tolerance) return false;
        return true;
    }
}