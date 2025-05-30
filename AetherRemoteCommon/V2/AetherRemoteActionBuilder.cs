namespace AetherRemoteCommon.V2;

public class AetherRemoteActionBuilder
{
    public static AetherRemoteAction<Unit> Ok() => new() { Result = AetherRemoteActionErrorCode.Success };
    public static AetherRemoteAction<T> Ok<T>(T value) => new() { Result = AetherRemoteActionErrorCode.Success, Value = value };
    public static AetherRemoteAction<Unit> Fail(AetherRemoteActionErrorCode error) => new() { Result = error };
    public static AetherRemoteAction<T> Fail<T>(AetherRemoteActionErrorCode error) => new() { Result = error };
}