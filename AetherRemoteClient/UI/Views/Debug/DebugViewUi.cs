using AetherRemoteClient.Domain.Interfaces;
using Dalamud.Bindings.ImGui;

namespace AetherRemoteClient.UI.Views.Debug;

public class DebugViewUi(DebugViewUiController controller) : IDrawable
{
    public void Draw()
    {
        ImGui.BeginGroup();
        if (ImGui.Button("Debug"))
        {
            _ = controller.Debug().ConfigureAwait(false);
        }
        
        ImGui.SameLine();

        if (ImGui.Button("Debug2"))
        {
            _ = controller.Debug2().ConfigureAwait(false);
        }
        
        ImGui.EndGroup();
    }
}