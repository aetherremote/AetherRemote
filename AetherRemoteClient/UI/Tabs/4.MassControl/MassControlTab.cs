using AetherRemoteClient.Domain;
using Dalamud.Interface.Colors;
using ImGuiNET;
using System;
using System.Numerics;

namespace AetherRemoteClient.UI.Tabs.MassControl;

public class MassControlTab : ITab
{
    public void Draw()
    {
        if (ImGui.BeginTabItem("Mass Control"))
        {
            if (ImGui.BeginChild("FriendSettingsArea", Vector2.Zero, true))
            {
                SharedUserInterfaces.PushBigFont();
                ImGui.SetCursorPosY((ImGui.GetWindowHeight() / 2) - (ImGui.GetFontSize() * 2));
                SharedUserInterfaces.PopBigFont();

                SharedUserInterfaces.BigTextCentered("Work In Progress");
                SharedUserInterfaces.MediumTextCentered("Feature implemented, but needs safety refinement");
                SharedUserInterfaces.TextCentered("See discord for more information", ImGuiColors.DalamudGrey);

                ImGui.EndChild();
            }

            ImGui.EndTabItem();
        }
    }

    public void Dispose() { GC.SuppressFinalize(this); }
}
