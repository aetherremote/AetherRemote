using MessagePack;

namespace AetherRemoteCommon.Domain.Network;

[MessagePackObject]
public record ActionResponseTest(
    [property: Key(0)] ActionResponseTestEc Code,
    [property: Key(1)] Dictionary<string, ActionClientResult> TargetResults
);

[MessagePackObject]
public record ActionResponseTest<T>(
    [property: Key(0)] ActionResponseTestEc Code,
    [property: Key(1)] Dictionary<string, ActionClientResult<T>> TargetResults
);

public enum ActionResponseTestEc;

[MessagePackObject]
public record ActionClientResult(
    [property: Key(0)] ActionClientResultEc Code
);

[MessagePackObject]
public record ActionClientResult<T>(
    [property: Key(0)] ActionClientResultEc Code,
    [property: Key(1)] T? Value
);

public enum ActionClientResultEc;