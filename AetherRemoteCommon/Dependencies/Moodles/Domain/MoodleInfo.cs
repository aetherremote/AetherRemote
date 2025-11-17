using AetherRemoteCommon.Dependencies.Moodles.Enums;
using MessagePack;

namespace AetherRemoteCommon.Dependencies.Moodles.Domain;

[MessagePackObject(keyAsPropertyName: true)]
public record MoodleInfo
{
    public Guid Guid { get; set; }
    public int IconId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public MoodleType Type { get; set; }
    public string Applier { get; set; } = string.Empty;
    public bool Dispellable { get; set; }
    public int Stacks { get; set; }
    public bool Persistent { get; set; }
    public int Days { get; set; }
    public int Hours { get; set; }
    public int Minutes { get; set; }
    public int Seconds { get; set; }
    public bool NoExpire { get; set; }
    public bool AsPermanent { get; set; }
    public Guid StatusOnRemoval { get; set; }
    public string CustomVfxPath { get; set; } = string.Empty;
    public bool StackOnReapply { get; set; }
    public int StacksIncOnReapply { get; set; }
}