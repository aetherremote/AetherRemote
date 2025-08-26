using System.Numerics;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Utils;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Colors;

namespace AetherRemoteClient.UI.Views.Possession;

public class PossessionViewUi(PossessionViewUiController controller) : IDrawable
{
    public void Draw()
    {
        ImGui.BeginChild("PossessionContent", Vector2.Zero);
        
        ImGui.SetCursorPosY(ImGui.GetWindowHeight() * 0.5f - SharedUserInterfaces.BigFontSize);
        SharedUserInterfaces.BigTextCentered("Spooky... What could this mean?");
        SharedUserInterfaces.TextCentered("(Coming soon)", ImGuiColors.DalamudGrey);
        
        ImGui.EndChild();
    }
}