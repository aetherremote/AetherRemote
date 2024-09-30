namespace AetherRemoteCommon.Domain.CommonGlamourerApplyType;

[Flags]
public enum GlamourerApplyFlag : ulong
{
    Once = 1uL,
    Equipment = 2uL,
    Customization = 4uL,
    Lock = 8uL
}
