using AetherRemoteClient.UI.Views.Transformations;

namespace AetherRemoteClient.Domain.Enums;

/// <summary>
///     Used inside of <see cref="TransformationsView"/> to represent which part of the Ui to render, and send network events as
/// </summary>
public enum TransformationMode
{
    Transform,
    BodySwap,
    Twinning,
    Mimicry
}