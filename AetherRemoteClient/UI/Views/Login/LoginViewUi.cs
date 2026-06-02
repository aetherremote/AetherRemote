using System.Numerics;
using AetherRemoteClient.Domain.Enums;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Services;
using AetherRemoteClient.UI.Style;
using AetherRemoteClient.Utils;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Colors;

namespace AetherRemoteClient.UI.Views.Login;

public class LoginViewUi(LoginViewUiController controller, NetworkService networkService) : IDrawable
{
    private const ImGuiInputTextFlags SecretInputFlags = ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.Password | ImGuiInputTextFlags.AutoSelectAll;
    
    public void Draw()
    {
        ImGui.BeginChild("LoginContent", Vector2.Zero, false, AetherRemoteImGui.ContentFlags);

        ImGui.AlignTextToFramePadding();

        SharedUserInterfaces.ContentBox("LoginHeader", AetherRemoteColors.PanelColor, true, () =>
        {
            SharedUserInterfaces.BigTextCentered("Aether Remote");
            SharedUserInterfaces.TextCentered(Plugin.Version.ToString());
        });

        SharedUserInterfaces.ContentBox("LoginSecret", AetherRemoteColors.PanelColor, true, () =>
        {
            var shouldConnect = false;

            SharedUserInterfaces.MediumText("Enter Secret");
            if (ImGui.InputTextWithHint("##SecretInput", "Secret", ref controller.Secret, 120, SecretInputFlags))
                shouldConnect = true;

            ImGui.SameLine();
            if (networkService.State is ConnectionState.Disconnected)
            {
                if (ImGui.Button("Connect"))
                    shouldConnect = true;
            }
            else
            {
                ImGui.BeginDisabled();
                ImGui.Button("Connect");
                ImGui.EndDisabled();
            }

            if (shouldConnect)
                _ = controller.Connect().ConfigureAwait(false);

            ImGui.Spacing();

            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(4, 0));

            ImGui.TextUnformatted("Need a secret? Join the");
            ImGui.SameLine();
            ImGui.PushStyleColor(ImGuiCol.Text, AetherRemoteColors.DiscordBlue);
            var size = ImGui.CalcTextSize("discord");
            if (ImGui.Selectable("discord", false, ImGuiSelectableFlags.None, size))
                LoginViewUiController.OpenDiscordLink();

            ImGui.PopStyleColor();
            ImGui.SameLine();
            ImGui.TextUnformatted("to generate one.");

            ImGui.PopStyleVar();
        });

        SharedUserInterfaces.ContentBox("CharacterConfiguration", AetherRemoteColors.PanelColor, true, () =>
        {
            SharedUserInterfaces.Icon(FontAwesomeIcon.ExclamationTriangle, ImGuiColors.DalamudYellow);
            ImGui.SameLine();
            ImGui.TextWrapped("Aether Remote now operates on a per-character configuration system.");
        });

        ImGui.EndChild();
    }
}