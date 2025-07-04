using System.Numerics;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Services;
using AetherRemoteClient.Utils;
using ImGuiNET;
using Microsoft.AspNetCore.SignalR.Client;

namespace AetherRemoteClient.UI.Views.Login;

public class LoginViewUi(LoginViewUiController controller, NetworkService networkService) : IDrawable
{
    private const ImGuiInputTextFlags SecretInputFlags =
        ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.Password;
    
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
            if (ImGui.InputTextWithHint("##SecretInput", "Secret", ref controller.Secret, 100, SecretInputFlags))
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

        ImGui.EndChild();
    }
}