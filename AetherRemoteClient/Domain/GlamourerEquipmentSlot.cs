using System;

namespace AetherRemoteClient.Domain;

[Flags]
public enum GlamourerEquipmentSlot
{
    None = 0,
    Head = 1 << 0,
    Body = 1 << 1,
    Hands = 1 << 2,
    Legs = 1 << 3,
    Feet = 1 << 4,
    Ears = 1 << 5,
    Neck = 1 << 6,
    Wrists = 1 << 7,
    RFinger = 1 << 8,
    LFinger = 1 << 9
}