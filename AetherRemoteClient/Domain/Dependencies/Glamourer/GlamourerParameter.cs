using AetherRemoteClient.Domain.Dependencies.Glamourer.Components;

namespace AetherRemoteClient.Domain.Dependencies.Glamourer;

public class GlamourerParameter
{
    public GlamourerColor FeatureColor = new();
    public GlamourerColor HairDiffuse = new();
    public GlamourerColor HairHighlight = new();
    public GlamourerColor LeftEye = new();
    public GlamourerColor RightEye = new();
    public GlamourerColor SkinDiffuse = new();
    public GlamourerColorAlpha DecalColor = new();
    public GlamourerColorAlpha LipDiffuse = new();
    public GlamourerValue FacePaintUvMultiplier = new();
    public GlamourerValue FacePaintUvOffset = new();
    public GlamourerPercentage LeftLimbalIntensity = new();
    public GlamourerPercentage RightLimbalIntensity = new();
    public GlamourerPercentage MuscleTone = new();

    public GlamourerParameter Clone()
    {
        var copy = (GlamourerParameter)MemberwiseClone();
        copy.FeatureColor = FeatureColor.Clone();
        copy.HairDiffuse = HairDiffuse.Clone();
        copy.HairHighlight = HairHighlight.Clone();
        copy.LeftEye = LeftEye.Clone();
        copy.RightEye = RightEye.Clone();
        copy.SkinDiffuse = SkinDiffuse.Clone();
        copy.DecalColor = DecalColor.Clone();
        copy.LipDiffuse = LipDiffuse.Clone();
        copy.FacePaintUvMultiplier = FacePaintUvMultiplier.Clone();
        copy.FacePaintUvOffset = FacePaintUvOffset.Clone();
        copy.LeftLimbalIntensity = LeftLimbalIntensity.Clone();
        copy.RightLimbalIntensity = RightLimbalIntensity.Clone();
        copy.MuscleTone = MuscleTone.Clone();
        return copy;
    }

    public bool IsEqualTo(GlamourerParameter other)
    {
        if (FeatureColor.IsEqualTo(other.FeatureColor) is false) return false;
        if (HairDiffuse.IsEqualTo(other.HairDiffuse) is false) return false;
        if (HairHighlight.IsEqualTo(other.HairHighlight) is false) return false;
        if (LeftEye.IsEqualTo(other.LeftEye) is false) return false;
        if (RightEye.IsEqualTo(other.RightEye) is false) return false;
        if (SkinDiffuse.IsEqualTo(other.SkinDiffuse) is false) return false;
        if (DecalColor.IsEqualTo(other.DecalColor) is false) return false;
        if (LipDiffuse.IsEqualTo(other.LipDiffuse) is false) return false;
        if (FacePaintUvMultiplier.IsEqualTo(other.FacePaintUvMultiplier) is false) return false;
        if (FacePaintUvOffset.IsEqualTo(other.FacePaintUvOffset) is false) return false;
        if (LeftLimbalIntensity.IsEqualTo(other.LeftLimbalIntensity) is false) return false;
        if (RightLimbalIntensity.IsEqualTo(other.RightLimbalIntensity) is false) return false;
        if (MuscleTone.IsEqualTo(other.MuscleTone) is false) return false;
        return true;
    }
}