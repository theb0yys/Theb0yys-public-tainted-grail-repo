using BepInEx;
using BepInEx.Configuration;
using UnityEngine;

namespace AvalonBearCompanion;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "kane.tgfoa.avalon-bear-companion";
    public const string PluginName = "Avalon Bear Companion";
    public const string PluginVersion = "0.1.0";

    private const string DefaultTemplateName = "Spec_AnimalBear";
    private const string DefaultTemplateGuid = "c45508309b84907429f83d1361918fc2";
    private const string ReviewCandidateId = "AVALON-PET-BEAR-001";
    private const string ReviewStatus = "BlockedUntilManualProof";
    private const int FoaDiagnosticSpawnerRefs = 6;
    private const int AvalonSceneSpawnerRefs = 0;
    private const string EvidenceStatus = "seen-by-foa-diagnostic-tool-only";

    private ConfigEntry<bool> _enabled = null!;
    private ConfigEntry<bool> _researchModeOnly = null!;
    private ConfigEntry<string> _templateName = null!;
    private ConfigEntry<string> _templateGuid = null!;
    private ConfigEntry<bool> _logTargetOnLoad = null!;
    private ConfigEntry<bool> _enableDisabledSummonTest = null!;
    private ConfigEntry<KeyCode> _disabledSummonTestHotkey = null!;

    private void Awake()
    {
        _enabled = Config.Bind("General", "Enabled", true, "Enable this diagnostic scaffold.");
        _researchModeOnly = Config.Bind("Research", "ResearchModeOnly", true, "Keep bear companion runtime behavior blocked until shortlist, actor safety, faction, transition, and save/load research pass.");
        _templateName = Config.Bind("Target", "TemplateName", DefaultTemplateName, "Candidate bear LocationTemplate name. Diagnostics only.");
        _templateGuid = Config.Bind("Target", "TemplateGuid", DefaultTemplateGuid, "Candidate bear LocationTemplate GUID. Diagnostics only.");
        _logTargetOnLoad = Config.Bind("Diagnostics", "LogTargetOnLoad", true, "Log target metadata when the scaffold loads.");
        _enableDisabledSummonTest = Config.Bind("Prototype", "EnableDisabledSummonTest", false, "Diagnostic gate only. When enabled with a hotkey, logs that summon behavior is still blocked. It never spawns actors.");
        _disabledSummonTestHotkey = Config.Bind("Prototype", "DisabledSummonTestHotkey", KeyCode.None, "Optional hotkey for the disabled summon-test log. Default None avoids hotkey conflicts.");

        Logger.LogInfo($"{PluginName} {PluginVersion} loaded. Enabled={_enabled.Value}; ResearchModeOnly={_researchModeOnly.Value}; Target={_templateName.Value}[{_templateGuid.Value}].");
        if (_logTargetOnLoad.Value)
        {
            Logger.LogInfo($"{PluginName} target is diagnostics-only. Review={ReviewCandidateId}; Status={ReviewStatus}; Evidence={EvidenceStatus}; FoaDiagnosticSpawnerRefs={FoaDiagnosticSpawnerRefs}; AvalonSceneSpawnerRefs={AvalonSceneSpawnerRefs}; safeSpawnCandidate=false; rosterApproved=false; behavior=blocked.");
        }
    }

    private void Update()
    {
        if (!_enabled.Value
            || !_enableDisabledSummonTest.Value
            || _disabledSummonTestHotkey.Value == KeyCode.None
            || !Input.GetKeyDown(_disabledSummonTestHotkey.Value))
        {
            return;
        }

        Logger.LogWarning($"{PluginName} disabled summon test requested for {_templateName.Value}[{_templateGuid.Value}] review={ReviewCandidateId}, but behavior is blocked. This scaffold does not call SpawnLocation, alter factions, add ally components, or write save data.");
    }
}
