using AetherRemoteClient.UI.Views.Transformations;
using AetherRemoteClient.UI.Views.Transformations.Controllers;
using AetherRemoteClient.UI.Views.Transformations.Views;

namespace AetherRemoteClient.Domain.Enums;

/// <summary>
///     Used inside of <see cref="UI.Views.Transformations.Views.TransformationsViewUi"/> and <see cref="UI.Views.Transformations.Controllers.TransformationsViewUiController"/> to represent which part of the Ui to render, and send network events as
/// </summary>
public enum TransformationMode
{
    Transform,
    BodySwap,
    Twinning,
    Mimicry
}