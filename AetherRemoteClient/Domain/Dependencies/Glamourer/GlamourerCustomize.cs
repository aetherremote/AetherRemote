using AetherRemoteClient.Domain.Dependencies.Glamourer.Components;

namespace AetherRemoteClient.Domain.Dependencies.Glamourer;

public class GlamourerCustomize
{
    public uint ModelId;
    public GlamourerValue BodyType = new();
    public GlamourerValue BustSize = new();
    public GlamourerValue Clan = new();
    public GlamourerValue Eyebrows = new();
    public GlamourerValue EyeColorLeft = new();
    public GlamourerValue EyeColorRight = new();
    public GlamourerValue EyeShape = new();
    public GlamourerValue Face = new();
    public GlamourerValue FacePaint = new();
    public GlamourerValue FacePaintColor = new();
    public GlamourerValue FacePaintReversed = new();
    public GlamourerValue FacialFeature1 = new();
    public GlamourerValue FacialFeature2 = new();
    public GlamourerValue FacialFeature3 = new();
    public GlamourerValue FacialFeature4 = new();
    public GlamourerValue FacialFeature5 = new();
    public GlamourerValue FacialFeature6 = new();
    public GlamourerValue FacialFeature7 = new();
    public GlamourerValue Gender = new();
    public GlamourerValue HairColor = new();
    public GlamourerValue Hairstyle = new();
    public GlamourerValue Height = new();
    public GlamourerValue Highlights = new();
    public GlamourerValue HighlightsColor = new();
    public GlamourerValue Jaw = new();
    public GlamourerValue LegacyTattoo = new();
    public GlamourerValue LipColor = new();
    public GlamourerValue Lipstick = new();
    public GlamourerValue Mouth = new();
    public GlamourerValue MuscleMass = new();
    public GlamourerValue Nose = new();
    public GlamourerValue Race = new();
    public GlamourerValue SkinColor = new();
    public GlamourerValue SmallIris = new();
    public GlamourerValue TailShape = new();
    public GlamourerValue TattooColor = new();
    public GlamourerValue Wetness = new();

    public GlamourerCustomize Clone()
    {
        var copy = (GlamourerCustomize)MemberwiseClone();
        copy.BodyType = BodyType.Clone();
        copy.BustSize = BustSize.Clone();
        copy.Clan = Clan.Clone();
        copy.Eyebrows = Eyebrows.Clone();
        copy.EyeColorLeft = EyeColorLeft.Clone();
        copy.EyeColorRight = EyeColorRight.Clone();
        copy.EyeShape = EyeShape.Clone();
        copy.Face = Face.Clone();
        copy.FacePaint = FacePaint.Clone();
        copy.FacePaintColor = FacePaintColor.Clone();
        copy.FacePaintReversed = FacePaintReversed.Clone();
        copy.FacialFeature1 = FacialFeature1.Clone();
        copy.FacialFeature2 = FacialFeature2.Clone();
        copy.FacialFeature3 = FacialFeature3.Clone();
        copy.FacialFeature4 = FacialFeature4.Clone();
        copy.FacialFeature5 = FacialFeature5.Clone();
        copy.FacialFeature6 = FacialFeature6.Clone();
        copy.FacialFeature7 = FacialFeature7.Clone();
        copy.Gender = Gender.Clone();
        copy.HairColor = HairColor.Clone();
        copy.Hairstyle = Hairstyle.Clone();
        copy.Height = Height.Clone();
        copy.Highlights = Highlights.Clone();
        copy.HighlightsColor = HighlightsColor.Clone();
        copy.Jaw = Jaw.Clone();
        copy.LegacyTattoo = LegacyTattoo.Clone();
        copy.LipColor = LipColor.Clone();
        copy.Lipstick = Lipstick.Clone();
        copy.Mouth = Mouth.Clone();
        copy.MuscleMass = MuscleMass.Clone();
        copy.Nose = Nose.Clone();
        copy.Race = Race.Clone();
        copy.SkinColor = SkinColor.Clone();
        copy.SmallIris = SmallIris.Clone();
        copy.TailShape = TailShape.Clone();
        copy.TattooColor = TattooColor.Clone();
        copy.Wetness = Wetness.Clone();
        return copy;
    }
}