namespace AetherRemoteCommon.V2;

public record AetherRemoteAction<T>
{
    public AetherRemoteActionErrorCode Result { get; set; }
    public T? Value { get; set; }
}