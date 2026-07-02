using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Interfaces;

namespace AetherRemoteClient.UI.Views.Debug;

public partial class DebugView : IView
{
    // IView property
    public View View => View.Debug;
}