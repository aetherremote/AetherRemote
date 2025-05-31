using AetherRemoteCommon.V2.Domain.Enum;
using MessagePack;

namespace AetherRemoteCommon.V2.Domain;

[MessagePackObject(true)]
public record ActionResult<T>
{
    public ActionResultEc Result { get; set; }
    public T? Value { get; set; }
}