using System.Numerics;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteClient.Utils;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Microsoft.AspNetCore.SignalR.Client;

namespace AetherRemoteClient.UI.Views.Login;

public class LoginViewUi(LoginViewUiController controller, NetworkService networkService) : IDrawable
{
    private const ImGuiInputTextFlags SecretInputFlags =
        ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.Password | ImGuiInputTextFlags.AutoSelectAll;
    
    public void Draw()
    {
        ImGui.BeginChild("LoginContent", Vector2.Zero, false, AetherRemoteStyle.ContentFlags);

        ImGui.AlignTextToFramePadding();

        SharedUserInterfaces.ContentBox("LoginHeader", AetherRemoteStyle.PanelBackground, true, () =>
        {
            SharedUserInterfaces.BigTextCentered("Aether Remote");
            SharedUserInterfaces.TextCentered(Plugin.Version.ToString());
        });

        SharedUserInterfaces.ContentBox("LoginSecret", AetherRemoteStyle.PanelBackground, true, () =>
        {
            var shouldConnect = false;

            SharedUserInterfaces.MediumText("Enter Secret");
            if (ImGui.InputTextWithHint("##SecretInput", "Secret", ref controller.Secret, 120, SecretInputFlags))
                shouldConnect = true;

            ImGui.SameLine();
            var disable = networkService.Connection.State is not HubConnectionState.Disconnected;
            if (disable)
            {
                ImGui.BeginDisabled();
                ImGui.Button("Connect");
                ImGui.EndDisabled();
            }
            else
            {
                if (ImGui.Button("Connect"))
                    shouldConnect = true;
            }

            if (shouldConnect)
                controller.Connect();

            ImGui.Spacing();

            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(4, 0));

            ImGui.TextUnformatted("Need a secret? Join the");
            ImGui.SameLine();
            ImGui.PushStyleColor(ImGuiCol.Text, AetherRemoteStyle.DiscordBlue);
            var size = ImGui.CalcTextSize("discord");
            if (ImGui.Selectable("discord", false, ImGuiSelectableFlags.None, size))
                LoginViewUiController.OpenDiscordLink();

            ImGui.PopStyleColor();
            ImGui.SameLine();
            ImGui.TextUnformatted("to generate one.");

            ImGui.PopStyleVar();
        });

        SharedUserInterfaces.ContentBox("CharacterConfiguration", AetherRemoteStyle.PanelBackground, true, () =>
        {
            SharedUserInterfaces.Icon(FontAwesomeIcon.ExclamationTriangle, ImGuiColors.DalamudYellow);
            ImGui.SameLine();
            ImGui.TextWrapped("Aether Remote now operates on a per-character configuration system.");
        });

        if (Plugin.LegacyConfiguration is not null)
        {
            SharedUserInterfaces.ContentBox("LegacyConfiguration", AetherRemoteStyle.PanelBackground, true, () =>
            {
                SharedUserInterfaces.MediumText("Legacy Configuration");
                ImGui.TextUnformatted("Click");
                ImGui.SameLine();
                ImGui.PushStyleColor(ImGuiCol.Text, AetherRemoteStyle.DiscordBlue);
                var size = ImGui.CalcTextSize("here");
                if (ImGui.Selectable("here", false, ImGuiSelectableFlags.None, size))
                    LoginViewUiController.CopyOriginalSecret();

                ImGui.PopStyleColor();
                ImGui.SameLine();
                ImGui.TextUnformatted("to copy your original secret to the clipboard.");
            });
        }

        ImGui.EndChild();
    }
}