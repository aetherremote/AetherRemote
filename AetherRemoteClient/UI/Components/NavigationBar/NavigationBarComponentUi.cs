using System.Numerics;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteClient.Utils;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Microsoft.AspNetCore.SignalR.Client;

namespace AetherRemoteClient.UI.Components.NavigationBar;

public class NavigationBarComponentUi(NetworkService networkService, ViewService viewService, SelectionManager selection)
{
    // Const
    private static readonly Vector2 AlignButtonTextLeft = new(0, 0.5f);
    
    public void Draw()
    {
        var spacing = ImGui.GetStyle().ItemSpacing;
        var windowPadding = ImGui.GetStyle().WindowPadding;
        var size = new Vector2(AetherRemoteStyle.NavBarDimensions.X - windowPadding.X * 2, 25);
        var offset = windowPadding with { Y = (size.Y - ImGui.GetFontSize()) * 0.5f };

        ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, AetherRemoteStyle.Rounding);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, AetherRemoteStyle.Rounding);

        if (ImGui.BeginChild("###MainWindowNavBar", AetherRemoteStyle.NavBarDimensions, true, ImGuiWindowFlags.NoScrollbar))
        {
            ImGui.PushStyleVar(ImGuiStyleVar.ButtonTextAlign, AlignButtonTextLeft);

            if (networkService.Connection.State is HubConnectionState.Connected)
            {
                ImGui.TextUnformatted("General");
                NavBarButton(FontAwesomeIcon.User, "Status", View.Status, size, offset, spacing);
                NavBarButton(FontAwesomeIcon.UserFriends, "Friends", View.Friends, size, offset, spacing);
                NavBarButton(FontAwesomeIcon.Pause, "Pause", View.Pause, size, offset, spacing);

                ImGui.TextUnformatted("Actions");
                NavBarButton(FontAwesomeIcon.PeopleArrows, "Body Swap", View.BodySwap, size, offset, spacing);
                NavBarButton(FontAwesomeIcon.Plus, "Customize+", View.CustomizePlus, size, offset, spacing);
                NavBarButton(FontAwesomeIcon.Smile, "Emote", View.Emote, size, offset, spacing);
                NavBarButton(FontAwesomeIcon.Stopwatch, "Hypnosis", View.Hypnosis, size, offset, spacing);
                NavBarButton(FontAwesomeIcon.Icons, "Moodles", View.Moodles, size, offset, spacing);
                NavBarButton(FontAwesomeIcon.Comments, "Speak", View.Speak, size, offset, spacing);
                NavBarButton(FontAwesomeIcon.WandMagicSparkles, "Transformation", View.Transformation, size, offset, spacing);
                NavBarButton(FontAwesomeIcon.PeopleGroup, "Twinning", View.Twinning, size, offset, spacing);
                
                ImGui.TextUnformatted("Configuration");
                NavBarButton(FontAwesomeIcon.History, "History", View.History, size, offset, spacing);
            }
            else
            {
                ImGui.TextUnformatted("General");
                NavBarButton(FontAwesomeIcon.Plug, "Login", View.Login, size, offset, spacing);

                ImGui.TextUnformatted("Configuration");
            }

            NavBarButton(FontAwesomeIcon.Wrench, "Settings", View.Settings, size, offset, spacing);
            
#if DEBUG
            // ImGui.TextUnformatted("Testing");
            // NavBarButton(FontAwesomeIcon.Bug, "Debug", View.Debug, size, offset, spacing);
#endif

            ImGui.PopStyleVar();
            ImGui.EndChild();
        }

        ImGui.PopStyleVar(2);
    }
    
    private void NavBarButton(FontAwesomeIcon icon, string text, View view, Vector2 size, Vector2 offset, Vector2 spacing)
    {
        var begin = ImGui.GetCursorPos();
        if (viewService.CurrentView == view)
        {
            ImGui.PushStyleColor(ImGuiCol.Button, AetherRemoteStyle.PrimaryColor);
            ImGui.Button($"##{text}", size);
            ImGui.PopStyleColor();
        }
        else
        {
            if (ImGui.Button($"##{text}", size))
            {
                viewService.CurrentView = view;
            
                // Required to in cases where you move from things like Friends -> Speak, and the friend you were editing was offline
                selection.ClearOfflineFriends();
            }
        }

        ImGui.SetCursorPos(begin + offset);

        SharedUserInterfaces.Icon(icon);
        ImGui.SameLine();
        ImGui.TextUnformatted(text);
        ImGui.SetCursorPos(begin + new Vector2(0, size.Y + spacing.Y));
    }
}