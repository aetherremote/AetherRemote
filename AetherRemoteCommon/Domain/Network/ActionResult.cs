using AetherRemoteCommon.Domain.Enums;
using MessagePack;

namespace AetherRemoteCommon.Domain.Network;

[MessagePackObject(true)]
public record ActionResult<T>
{
    public ActionResultEc Result { get; set; }
    public T? Value { get; set; }
}