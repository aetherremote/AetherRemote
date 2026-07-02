using Dalamud.Bindings.ImGui;

namespace AetherRemoteClient.UI.Views.Debug;

public partial class DebugView
{
    public void Draw()
    {
        ImGui.BeginGroup();
        if (ImGui.Button("Debug"))
        {
            _ = Debug().ConfigureAwait(false);
        }
        
        ImGui.SameLine();

        if (ImGui.Button("Debug2"))
        {
            _ = Debug2().ConfigureAwait(false);
        }
        
        ImGui.EndGroup();
    }
}