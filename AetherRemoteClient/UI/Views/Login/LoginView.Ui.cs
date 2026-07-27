using System.Numerics;
using AetherRemoteClient.UI.Style;
using AetherRemoteClient.Utils;
using Dalamud.Bindings.ImGui;

namespace AetherRemoteClient.UI.Views.Login;

public partial class LoginView
{
    public void Draw()
    {
        ImGui.BeginChild("LoginContent", Vector2.Zero, false, AetherRemoteImGui.ContentFlags);

        ImGui.AlignTextToFramePadding();

        SharedUserInterfaces.ContentBox("LoginHeader", AetherRemoteColors.PanelColor, true, () =>
        {
            SharedUserInterfaces.BigTextCentered("Aether Remote");
            SharedUserInterfaces.TextCentered(Plugin.Version.ToString());
        });
        
        SharedUserInterfaces.ContentBox("LoginSecretSelect", AetherRemoteColors.PanelColor, true, () =>
        {
            SharedUserInterfaces.MediumText("Login with Secret");

            var preview = _activeSessionService.PendingSecretId is not { } secretId
                ? "Select a secret to log in with"
                : _configurationService.Secrets.TryGetValue(secretId, out var value)
                    ? value.Name
                    : "<<Unable to find secret>>";
            
            if (ImGui.BeginCombo("##SecretSelect", preview))
            {
                foreach (var secret in _configurationService.Secrets)
                    if (ImGui.Selectable(secret.Value.Name))
                        _ = _activeSessionService.UpdatePendingSecretId(secret.Key).ConfigureAwait(false);
                
                ImGui.EndCombo();
            }
            
            ImGui.SameLine();
            
            // TODO: Make more clear feedback as to the state of the connection
            if (ImGui.Button("Connect"))
                _ = Connect().ConfigureAwait(false);
            
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(4, 0));
            ImGui.TextUnformatted("Need a secret? Join the");
            ImGui.SameLine();
            ImGui.PushStyleColor(ImGuiCol.Text, AetherRemoteColors.DiscordBlue);
            var size = ImGui.CalcTextSize("discord");
            if (ImGui.Selectable("discord", false, ImGuiSelectableFlags.None, size))
                OpenDiscordLink();
            ImGui.PopStyleColor();
            ImGui.SameLine();
            ImGui.TextUnformatted("to generate one.");
            ImGui.PopStyleVar();
        });

        SharedUserInterfaces.ContentBox2("ConfigurationUpdateNotice", AetherRemoteColors.PanelColor, true, () =>
        {
            SharedUserInterfaces.MediumText("Configurations and Secrets");
            ImGui.TextWrapped("There have been significant changes to configurations and secrets in AR. You can find the changes in the Settings tab.");
        });
        
        ImGui.EndChild();
    }
}