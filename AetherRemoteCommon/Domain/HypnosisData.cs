using AetherRemoteCommon.Domain.Enums;
using MessagePack;

namespace AetherRemoteCommon.Domain;

[MessagePackObject(keyAsPropertyName: true)]
public record HypnosisData
{
    public int SpiralArms { get; set; }
    public int SpiralTurns { get; set; }
    public int SpiralCurve { get; set; }
    public int SpiralThickness { get; set; }
    public int SpiralSpeed { get; set; }
    public HypnosisSpiralDirection SpiralDirection { get; set; }
    public uint SpiralColor { get; set; }

    public int TextDuration { get; set; }
    public int TextDelay { get; set; }
    public string[] TextWords { get; set; } = [];
    public HypnosisTextMode TextMode { get; set; }
    public uint TextColor { get; set; }

    public override string ToString()
    {
        return $"SpiralArms: {SpiralArms}, SpiralTurns: {SpiralTurns}, SpiralCurve: {SpiralCurve}, SpiralThickness: {SpiralThickness}, SpiralSpeed: {SpiralSpeed}, SpiralDirection: {SpiralDirection}, SpiralColor: {SpiralColor}, TextDuration: {TextDuration}, TextDelay: {TextDelay}, TextWords: [{string.Join(", ", TextWords)}], TextMode: {TextMode}, TextColor: {TextColor}";
    }
}