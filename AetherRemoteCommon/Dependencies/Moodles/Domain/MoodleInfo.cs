using AetherRemoteCommon.Dependencies.Moodles.Enums;
using MessagePack;

namespace AetherRemoteCommon.Dependencies.Moodles.Domain;

[MessagePackObject(keyAsPropertyName: true)]
public record MoodleInfo
{
    public int Version { get; set; }
    public Guid Guid { get; set; }
    public int IconId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CustomVfxPath { get; set; } = string.Empty;
    public long ExpireTicks { get; set; }
    public MoodleType Type { get; set; }
    public int Stacks { get; set; }
    public int StackSteps { get; set; }
    public uint Modifiers { get; set; }
    public Guid ChainedStatus { get; set; }
    public MoodleChainTrigger ChainTrigger { get; set; }
    public string Applier { get ; set; } = string.Empty;
    public string Dispeller { get; set; } = string.Empty;
    public bool Permanent { get; set; }
}