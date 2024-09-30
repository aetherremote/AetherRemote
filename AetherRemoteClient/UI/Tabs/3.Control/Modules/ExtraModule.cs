using AetherRemoteClient.Accessors.Glamourer;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Log;
using AetherRemoteClient.Domain.UI;
using AetherRemoteClient.Providers;
using AetherRemoteCommon;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using ImGuiNET;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AetherRemoteClient.UI.Tabs.Modules;

public class ExtraModule : IControlTableModule
{
    // Injected
    private readonly ClientDataManager clientDataManager;
    private readonly CommandLockoutManager commandLockoutManager;
    private readonly GlamourerAccessor glamourerAccessor;
    private readonly HistoryLogManager historyLogManager;
    private readonly NetworkProvider networkProvider;

    public ExtraModule(
        ClientDataManager clientDataManager,
        CommandLockoutManager commandLockoutManager,
        GlamourerAccessor glamourerAccessor,
        HistoryLogManager historyLogManager,
        NetworkProvider networkProvider)
    {
        this.clientDataManager = clientDataManager;
        this.commandLockoutManager = commandLockoutManager;
        this.glamourerAccessor = glamourerAccessor;
        this.historyLogManager = historyLogManager;
        this.networkProvider = networkProvider;
    }

    public void Draw()
    {
        SharedUserInterfaces.MediumText("Extra", ImGuiColors.ParsedOrange);
        ImGui.SameLine();
        ImGui.AlignTextToFramePadding();
        SharedUserInterfaces.Icon(FontAwesomeIcon.QuestionCircle);
        SharedUserInterfaces.Tooltip("A collection of extra features or shortcuts");

        SharedUserInterfaces.DisableIf(commandLockoutManager.IsLocked, () =>
        {
            if (ImGui.Button("Bodyswap"))
                _ = ProcessBodySwap();
        });

        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.TextColored(ImGuiColors.ParsedOrange, "[Description]");
            ImGui.Text("Attempts to swap bodies with yourself and selected targets randomly.");

            ImGui.Separator();

            ImGui.TextColored(ImGuiColors.ParsedOrange, "[Required Plugins]");
            ImGui.BulletText("Glamourer");
            ImGui.BulletText("Mare Synchronos");

            ImGui.TextColored(ImGuiColors.DalamudGrey, "[Required Permissions]");
            ImGui.BulletText("Customization");
            ImGui.BulletText("Equipment");
            ImGui.EndTooltip();
        }

        ImGui.SameLine();

        SharedUserInterfaces.DisableIf(commandLockoutManager.IsLocked, () =>
        {
            if (ImGui.Button("Twinning"))
                _ = ProcessTwinning();
        });

        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.TextColored(ImGuiColors.ParsedOrange, "[Description]");
            ImGui.Text("Attempts to transform selected targets into you.");

            ImGui.Separator();

            ImGui.TextColored(ImGuiColors.ParsedOrange, "[Required Plugins]");
            ImGui.BulletText("Glamourer");
            ImGui.BulletText("Mare Synchronos");

            ImGui.TextColored(ImGuiColors.DalamudGrey, "[Required Permissions]");
            ImGui.BulletText("Customization");
            ImGui.BulletText("Equipment");
            ImGui.EndTooltip();
        }
    }

    private async Task ProcessTwinning()
    {
        // Await local data
        var localData = await GetLocalPlayerData();
        if (localData.Success == false)
        {
            // TODO: Toast explaining what went wrong
            Plugin.Log.Warning($"Unable to process twinning command, {localData.Message}!");
            return;
        }

        // Initiate UI Lockout
        commandLockoutManager.Lock(Constraints.ExternalCommandCooldownInSeconds);

        var targets = clientDataManager.TargetManager.Targets.ToList();
        var (successful, newBodyData) = await networkProvider.IssueBodySwapCommand(targets, localData.CharacterData).ConfigureAwait(false);
        if (successful)
        {
            if (newBodyData == null)
            {
                // TODO: Toast explaining what went wrong
                Plugin.Log.Warning("Your new body was lost in the aether during transfer! Tell a developer");
                return;
            }

            var result = await glamourerAccessor.ApplyDesignAsync(localData.CharacterName, newBodyData).ConfigureAwait(false);
            if (result)
            {
                var message = $"You issued yourself and {string.Join(", ", targets)} to swap bodies";
                Plugin.Log.Information(message);
                historyLogManager.LogHistoryGlamourer(message, newBodyData);
            }
        }
        else
        {
            // TODO: Make a toast with what went wrong
        }
    }

    private async Task ProcessBodySwap()
    {
        // Await local data
        var localData = await GetLocalPlayerData();
        if (localData.Success == false)
        {
            // TODO: Toast explaining what went wrong
            Plugin.Log.Warning($"Unable to process body swap command, {localData.Message}");
            return;
        }

        // Initiate UI Lockout
        commandLockoutManager.Lock(Constraints.ExternalCommandCooldownInSeconds);

        var targets = clientDataManager.TargetManager.Targets.ToList();
        var (successful, newBodyData) = await networkProvider.IssueBodySwapCommand(targets, localData.CharacterData).ConfigureAwait(false);
        if (successful)
        {
            if (newBodyData == null)
            {
                // TODO: Toast explaining what went wrong
                Plugin.Log.Warning("Your new body was lost in the aether during transfer! Tell a developer!");
                return;
            }

            var result = await glamourerAccessor.ApplyDesignAsync(localData.CharacterName, newBodyData).ConfigureAwait(false);
            if (result)
            {
                var message = $"You issued yourself and {string.Join(", ", targets)} to swap bodies";
                Plugin.Log.Information(message);
                historyLogManager.LogHistoryGlamourer(message, newBodyData);
            }
        }
        else
        {
            // TODO: Make a toast with what went wrong
        }
    }

    public void Dispose() => GC.SuppressFinalize(this);

    /// <summary>
    /// Retrieves local player's name, and glamourer data
    /// </summary>
    private async Task<LocalData> GetLocalPlayerData()
    {
        var _characterName = Plugin.ClientState.LocalPlayer?.Name.ToString();
        if (_characterName == null)
            return new(false, "No local body");

        var _characterData = await glamourerAccessor.GetDesignAsync(_characterName).ConfigureAwait(false);
        if (_characterData == null)
            return new(false, "No glamourer data");

        return new(true, string.Empty, _characterName, _characterData);
    }

    private readonly struct LocalData
    {
        public readonly bool Success;
        public readonly string Message;
        public readonly string CharacterName;
        public readonly string CharacterData;

        public LocalData(bool success, string message = "", string characterName = "", string characterData = "")
        {
            Success = success;
            Message = message;
            CharacterName = characterName;
            CharacterData = characterData;
        }
    }
}
