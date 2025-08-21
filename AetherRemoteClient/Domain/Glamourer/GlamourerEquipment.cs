using AetherRemoteClient.Domain.Glamourer.Components;

namespace AetherRemoteClient.Domain.Glamourer;

public class GlamourerEquipment
{
    public GlamourerItem MainHand = new();
    public GlamourerItem OffHand = new();
    public GlamourerItem Head = new();
    public GlamourerItem Body = new();
    public GlamourerItem Hands = new();
    public GlamourerItem Legs = new();
    public GlamourerItem Feet = new();
    public GlamourerItem Ears = new();
    public GlamourerItem Neck = new();
    public GlamourerItem Wrists = new();
    public GlamourerItem RFinger = new();
    public GlamourerItem LFinger = new();
    public GlamourerShow Hat = new();
    public GlamourerShow VieraEars = new();
    public GlamourerShow Weapon = new();
    public GlamourerIsToggled Visor = new();

    public GlamourerEquipment Clone()
    {
        var copy = (GlamourerEquipment)MemberwiseClone();
        copy.MainHand = MainHand.Clone();
        copy.OffHand = OffHand.Clone();
        copy.Head = Head.Clone();
        copy.Body = Body.Clone();
        copy.Hands = Hands.Clone();
        copy.Legs = Legs.Clone();
        copy.Feet = Feet.Clone();
        copy.Ears = Ears.Clone();
        copy.Neck = Neck.Clone();
        copy.Wrists = Wrists.Clone();
        copy.RFinger = RFinger.Clone();
        copy.LFinger = LFinger.Clone();
        copy.Hat = Hat.Clone();
        copy.VieraEars = VieraEars.Clone();
        copy.Weapon = Weapon.Clone();
        copy.Visor = Visor.Clone();
        return copy;
    }
}