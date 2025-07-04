using AetherRemoteCommon.Domain.Enums;

namespace AetherRemoteCommon.Domain.Network;

public static class ActionResultBuilder
{
    public static ActionResult<Unit> Ok() => new() { Result = ActionResultEc.Success };
    public static ActionResult<T> Ok<T>(T value) => new() { Result = ActionResultEc.Success, Value = value };
    public static ActionResult<Unit> Fail(ActionResultEc error) => new() { Result = error };
    public static ActionResult<T> Fail<T>(ActionResultEc error) => new() { Result = error };
}