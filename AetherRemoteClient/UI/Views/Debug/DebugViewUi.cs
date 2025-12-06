using AetherRemoteClient.Domain.Interfaces;
using Dalamud.Bindings.ImGui;

namespace AetherRemoteClient.UI.Views.Debug;

public class DebugViewUi(DebugViewUiController controller) : IDrawable
{
    public void Draw()
    {
        if (ImGui.Button("Debug"))
        {
            controller.Debug();
        }
    }
}