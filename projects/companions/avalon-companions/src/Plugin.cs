using BepInEx;
using BepInEx.Configuration;
using AvalonCompanions.Patches;
using HarmonyLib;
using UnityEngine;

namespace AvalonCompanions;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
[DefaultExecutionOrder(-32000)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "kane.tgfoa.avalon-companions";
    public const string PluginName = "Avalon Companions";
    public const string PluginVersion = "0.2.17";

    internal static ConfigEntry<bool> Enabled = null!;
    internal static ConfigEntry<bool> ResearchModeOnly = null!;
    internal static ConfigEntry<bool> LogPetSystemDiagnostics = null!;
    internal static ConfigEntry<bool> WritePetTemplateDump = null!;
    internal static ConfigEntry<bool> WritePetCreatureShortlistDump = null!;
    internal static ConfigEntry<bool> WritePetCreatureDiagnosticToolCrosscheck = null!;
    internal static ConfigEntry<int> DiagnosticToolMinimumSpawnerRefs = null!;
    internal static ConfigEntry<string> PetCreatureShortlistTerms = null!;
    internal static ConfigEntry<bool> EnableQrkoPetPrototype = null!;
    internal static ConfigEntry<bool> EnablePetCompanionRoster = null!;
    internal static ConfigEntry<string> QrkoPetTemplateGuid = null!;
    internal static ConfigEntry<int> SelectedPetIndex = null!;
    internal static ConfigEntry<KeyboardShortcut> SpawnQrkoPetShortcut = null!;
    internal static ConfigEntry<KeyboardShortcut> NextPetShortcut = null!;
    internal static ConfigEntry<KeyboardShortcut> PreviousPetShortcut = null!;
    internal static ConfigEntry<KeyboardShortcut> RecallPetShortcut = null!;
    internal static ConfigEntry<KeyboardShortcut> DismissPetShortcut = null!;
    internal static ConfigEntry<KeyboardShortcut> TogglePetFollowShortcut = null!;
    internal static ConfigEntry<KeyboardShortcut> TogglePanelShortcut = null!;
    internal static ConfigEntry<bool> EnableDebugPanel = null!;
    internal static ConfigEntry<bool> ShowCommandStatus = null!;
    internal static ConfigEntry<float> CommandStatusSeconds = null!;
    internal static ConfigEntry<bool> ShowCompanionHudIcon = null!;
    internal static ConfigEntry<string> CompanionHudIconCorner = null!;
    internal static ConfigEntry<int> CompanionHudIconSize = null!;
    internal static ConfigEntry<bool> EnableNativeDefendAssist = null!;
    internal static ConfigEntry<bool> EnableLifecycleSafetyGuard = null!;
    internal static ConfigEntry<bool> WriteCompanionLifecycleDump = null!;
    internal static ConfigEntry<int> FollowRangeProfile = null!;
    internal static ConfigEntry<string> RememberedCompanionGuid = null!;
    internal static ConfigEntry<string> RememberedCompanionName = null!;
    internal static ConfigEntry<string> RememberedCommandMode = null!;
    internal static ConfigEntry<string> RememberedFollowRange = null!;
    internal static ConfigEntry<bool> EnableNativeCommandMenu = null!;
    internal static ConfigEntry<bool> EnableCustomDialogueInterface = null!;
    internal static ConfigEntry<bool> EnableNativeQuickCommands = null!;
    internal static ConfigEntry<bool> WriteCompanionCommandLog = null!;
    internal static ConfigEntry<bool> WriteCompanionAiBoundaryDiagnostics = null!;
    internal static ConfigEntry<float> CompanionAiBoundaryDiagnosticSeconds = null!;
    internal static ConfigEntry<bool> WriteCompanionAiProfileAudit = null!;
    internal static ConfigEntry<float> CompanionAiProfileAuditSeconds = null!;
    internal static ConfigEntry<bool> EnableProfileDrivenNativeAssist = null!;
    internal static ConfigEntry<float> ProfileDrivenNativeAssistSeconds = null!;
    internal static ConfigEntry<bool> EnableCompanionTrustRuntime = null!;
    internal static ConfigEntry<bool> EnableCompanionBondPolicyRuntime = null!;
    internal static ConfigEntry<bool> EnableCompanionProfileStore = null!;
    internal static ConfigEntry<bool> WriteCompanionTrustLog = null!;
    internal static ConfigEntry<bool> ActorControlProofEnabled = null!;
    internal static ConfigEntry<bool> ActorControlDryRunOnly = null!;
    internal static ConfigEntry<bool> DumpActorControlCandidates = null!;
    internal static ConfigEntry<float> ActorControlScanRadius = null!;
    internal static ConfigEntry<string> SelectedActorControlCandidateGuid = null!;
    internal static ConfigEntry<bool> AllowOneSessionPetControl = null!;
    internal static ConfigEntry<bool> AllowDefendAction = null!;
    internal static ConfigEntry<bool> EnableAvalonCoreDiagnosticBridge = null!;

    private Harmony? _harmony;

    private void Awake()
    {
        Enabled = Config.Bind("General", "Enabled", true, "Enable this mod.");
        ResearchModeOnly = Config.Bind(
            "Research",
            "ResearchModeOnly",
            true,
            "Keep companion runtime behavior disabled until follower targets, persistence, and save/load safety are researched.");
        LogPetSystemDiagnostics = Config.Bind(
            "Diagnostics",
            "LogPetSystemDiagnostics",
            false,
            "Log existing FoA pet/summon system availability once per session. Does not spawn, transform, persist, or command actors.");
        WritePetTemplateDump = Config.Bind(
            "Diagnostics",
            "WritePetTemplateDump",
            false,
            "Write GUID-backed Spec_Pet_* template candidates to this mod's BepInEx config folder once per session. Does not spawn, transform, persist, or command actors.");
        WritePetCreatureShortlistDump = Config.Bind(
            "Diagnostics",
            "WritePetCreatureShortlistDump",
            false,
            "Write scene-spawner-backed pet/creature LocationTemplate candidate CSVs to this mod's BepInEx config folder once per session. Does not approve roster entries or spawn actors.");
        WritePetCreatureDiagnosticToolCrosscheck = Config.Bind(
            "Diagnostics",
            "WritePetCreatureDiagnosticToolCrosscheck",
            true,
            "When pet/creature shortlist diagnostics run, read the latest FOA-Diagnostic Tool spawner_refs.csv and write comparison CSVs. Diagnostics only; does not approve roster entries or spawn actors.");
        DiagnosticToolMinimumSpawnerRefs = Config.Bind(
            "Diagnostics",
            "DiagnosticToolMinimumSpawnerRefs",
            100,
            "Minimum FOA-Diagnostic Tool spawner_refs.csv row count used for crosscheck selection. This skips low-context startup dumps; set to 0 to use the newest dump even if small.");
        PetCreatureShortlistTerms = Config.Bind(
            "Diagnostics",
            "PetCreatureShortlistTerms",
            "Spec_Pet,Pet_Qrko,Pet,Animal,Wolf,Bear,Deer,Cow,Pig,Chicken,Sheep,Dog,Rat,Boar,Creature,Monster,EnemyMonster,CorpseEater,Redcap,Flamegobbler,Grindylow,Wyrdspirit,Sharg,Floatling,Tadpole,Zombie,Drowner,Skeleton,Bullrat,Wyrd",
            "Comma-separated name/search terms used to classify pet/creature LocationTemplate candidates for diagnostics.");
        EnableQrkoPetPrototype = Config.Bind(
            "Prototype",
            "EnableQrkoPetPrototype",
            false,
            "Legacy enable for the throwaway-save Qrko pet prototype. Prefer Companions.EnablePetCompanionRoster.");
        EnablePetCompanionRoster = Config.Bind(
            "Companions",
            "EnablePetCompanionRoster",
            EnableQrkoPetPrototype.Value,
            "Enable the Avalon pet and animal companion roster. Requires Research.ResearchModeOnly=false and uses only reviewed LocationTemplate GUIDs.");
        QrkoPetTemplateGuid = Config.Bind(
            "Prototype",
            "QrkoPetTemplateGuid",
            "cc30c92e0699e3c41be92d1e99057354",
            "Canonical LocationTemplate GUID for the prototype pet. Default is Spec_Pet_Qrko from local runtime diagnostics.");
        SelectedPetIndex = Config.Bind(
            "Companions",
            "SelectedPetIndex",
            0,
            "Selected pet roster index. Use the next/previous pet shortcuts to change this in game.");
        SpawnQrkoPetShortcut = Config.Bind(
            "Companions",
            "SpawnQrkoPetShortcut",
            new KeyboardShortcut(KeyCode.KeypadPlus),
            "Summon or swap to the selected Avalon companion.");
        NextPetShortcut = Config.Bind(
            "Companions",
            "NextPetShortcut",
            new KeyboardShortcut(KeyCode.KeypadMultiply),
            "Select the next Avalon companion.");
        PreviousPetShortcut = Config.Bind(
            "Companions",
            "PreviousPetShortcut",
            new KeyboardShortcut(KeyCode.KeypadDivide),
            "Select the previous Avalon companion.");
        RecallPetShortcut = Config.Bind(
            "Companions",
            "RecallPetShortcut",
            new KeyboardShortcut(KeyCode.KeypadEnter),
            "Recall active managed pet companions to the hero.");
        DismissPetShortcut = Config.Bind(
            "Companions",
            "DismissPetShortcut",
            new KeyboardShortcut(KeyCode.KeypadMinus),
            "Dismiss active managed pet companions.");
        TogglePetFollowShortcut = Config.Bind(
            "Companions",
            "TogglePetFollowShortcut",
            new KeyboardShortcut(KeyCode.Keypad0),
            "Toggle active managed pet companions between follow and stay.");
        TogglePanelShortcut = Config.Bind(
            "Companions",
            "TogglePanelShortcut",
            new KeyboardShortcut(KeyCode.KeypadPeriod),
            "Open or close the temporary Avalon Companions debug panel.");
        EnableDebugPanel = Config.Bind(
            "Debug",
            "EnableDebugPanel",
            true,
            "Keep the temporary IMGUI companion debug panel available while the native dialogue command surface is researched.");
        ShowCommandStatus = Config.Bind(
            "Companions",
            "ShowCommandStatus",
            true,
            "Show a short on-screen status message when pet companion controls are used.");
        CommandStatusSeconds = Config.Bind(
            "Companions",
            "CommandStatusSeconds",
            3f,
            "Seconds to show pet companion command status messages.");
        ShowCompanionHudIcon = Config.Bind(
            "Companions",
            "ShowCompanionHudIcon",
            true,
            "Show a passive on-screen icon for the active managed companion while normal gameplay HUD is visible.");
        CompanionHudIconCorner = Config.Bind(
            "Companions",
            "CompanionHudIconCorner",
            "TopLeft",
            "Screen corner for the passive companion HUD icon. Supported values: TopLeft, BottomRight.");
        CompanionHudIconSize = Config.Bind(
            "Companions",
            "CompanionHudIconSize",
            88,
            "Pixel size for the passive companion HUD icon. Values are clamped between 56 and 128.");
        EnableNativeDefendAssist = Config.Bind(
            "Companions",
            "EnableNativeDefendAssist",
            true,
            "Allow automatic managed one-session native pet allies to enter combat when the hero has attackers outside explicit Defend mode. Uses the game's NpcHeroPetAlly targeting path; does not choose targets directly.");
        EnableLifecycleSafetyGuard = Config.Bind(
            "Companions",
            "EnableLifecycleSafetyGuard",
            true,
            "Keep the one-session companion lifecycle conservative across scene changes, long hero moves, rest, and reload checks: mark managed companions not saved, remove unexpected untracked one-session allies, and discard excess active roster actors. Does not add persistence.");
        WriteCompanionLifecycleDump = Config.Bind(
            "Diagnostics",
            "WriteCompanionLifecycleDump",
            true,
            "Append companion lifecycle snapshots to companion-lifecycle.csv for transition, rest, quit/reload, and duplicate/orphan validation. Does not add persistence or spawn actors.");
        FollowRangeProfile = Config.Bind(
            "Companions",
            "FollowRangeProfile",
            1,
            "Current follow range profile for managed one-session companions: 0=Close, 1=Normal, 2=Far. This only affects Avalon recall/catch-up placement and does not add custom AI pathing.");
        RememberedCompanionGuid = Config.Bind(
            "Continuity",
            "RememberedCompanionGuid",
            string.Empty,
            "Metadata-only last selected companion GUID. Used for status and manual Summon/Swap continuity only; does not restore, respawn, save, or re-adopt actors.");
        RememberedCompanionName = Config.Bind(
            "Continuity",
            "RememberedCompanionName",
            string.Empty,
            "Metadata-only last selected companion display name. Used for status text only; does not restore, respawn, save, or re-adopt actors.");
        RememberedCommandMode = Config.Bind(
            "Continuity",
            "RememberedCommandMode",
            "Follow",
            "Metadata-only last command mode: Follow, Stay, or Defend. Applied only to future explicit player commands; does not restore, respawn, save, or re-adopt actors.");
        RememberedFollowRange = Config.Bind(
            "Continuity",
            "RememberedFollowRange",
            "Normal",
            "Metadata-only last follow range: Close, Normal, or Far. Mirrors Companions.FollowRangeProfile for status; does not restore, respawn, save, or re-adopt actors.");
        EnableNativeCommandMenu = Config.Bind(
            "Companions",
            "EnableNativeCommandMenu",
            true,
            "Attach one runtime-only native Companion prompt that opens the plugin-owned custom dialogue command interface for managed one-session companions. Does not add dialogue graphs, save-owned interactions, targeting, or persistence.");
        EnableCustomDialogueInterface = Config.Bind(
            "Companions",
            "EnableCustomDialogueInterface",
            true,
            "Open the plugin-owned companion dialogue command interface from the runtime native Companion prompt. Does not add StoryBookmarks, story graphs, targeting, custom AI, or persistence.");
        EnableNativeQuickCommands = Config.Bind(
            "Companions",
            "EnableNativeQuickCommands",
            true,
            "Fallback only when Companions.EnableNativeCommandMenu=false. Attaches rotating runtime-only native quick-command actions to managed one-session companions. Uses the same Follow, Stay, Defend, Come Close, Recall, and Dismiss paths as the debug panel; does not add dialogue graphs, targeting, or persistence.");
        WriteCompanionCommandLog = Config.Bind(
            "Diagnostics",
            "WriteCompanionCommandLog",
            true,
            "Append command decisions to companion-command-log.csv. Records what Avalon command ran or why it was blocked; does not add targeting, persistence, or new actor behavior.");
        WriteCompanionAiBoundaryDiagnostics = Config.Bind(
            "Diagnostics",
            "WriteCompanionAiBoundaryDiagnostics",
            false,
            "Append read-only native AI, movement, target, and relation state for active managed companions to companion-ai-boundary.csv. Does not command, move, target, persist, or execute Avalon Core behavior.");
        CompanionAiBoundaryDiagnosticSeconds = Config.Bind(
            "Diagnostics",
            "CompanionAiBoundaryDiagnosticSeconds",
            2f,
            "Minimum seconds between companion AI boundary diagnostic samples. Values below 0.5 are clamped.");
        WriteCompanionAiProfileAudit = Config.Bind(
            "Diagnostics",
            "WriteCompanionAiProfileAudit",
            false,
            "Append read-only CompanionAiProfile and CompanionAiIntent rows for active managed companions to companion-ai-profile.csv. Evidence only; does not command, move, target, persist, or execute Avalon Core behavior.");
        CompanionAiProfileAuditSeconds = Config.Bind(
            "Diagnostics",
            "CompanionAiProfileAuditSeconds",
            2f,
            "Minimum seconds between companion AI profile audit samples. Values below 0.5 are clamped.");
        EnableProfileDrivenNativeAssist = Config.Bind(
            "Companions",
            "EnableProfileDrivenNativeAssist",
            false,
            "Allow CompanionAiIntent classifications to trigger only existing native-safe companion actions: catch-up recall and native hero-pet ally defend prompts. Does not add custom targeting, pathing, persistence, or Core behavior.");
        ProfileDrivenNativeAssistSeconds = Config.Bind(
            "Companions",
            "ProfileDrivenNativeAssistSeconds",
            1f,
            "Minimum seconds between profile-driven native assist evaluations. Values below 0.5 are clamped.");
        EnableCompanionTrustRuntime = Config.Bind(
            "Companions",
            "EnableCompanionTrustRuntime",
            true,
            "Enable runtime-only companion trust and loyalty scores from approved command/runtime events. Does not persist, restore, tame, train, target, or move actors.");
        EnableCompanionBondPolicyRuntime = Config.Bind(
            "Companions",
            "EnableCompanionBondPolicyRuntime",
            true,
            "Allow runtime-only trust/loyalty bond level to modify Avalon-owned command timing: catch-up interval, catch-up threshold, and native defend prompt cooldown. Does not add stats, targeting, movement overrides, persistence, or Core behavior.");
        EnableCompanionProfileStore = Config.Bind(
            "Companions",
            "EnableCompanionProfileStore",
            true,
            "Persist approved companion profile fields only: reviewed identity, trust, loyalty, last trust event, last command mode, last follow range, and effective bond. Does not save actors, restore companions, auto-respawn, store location/health/AI/target state, take save ownership, or change command behavior.");
        WriteCompanionTrustLog = Config.Bind(
            "Diagnostics",
            "WriteCompanionTrustLog",
            true,
            "Append runtime-only trust and loyalty score changes to companion-command-log.csv when command logging is enabled. Does not persist or change behavior.");
        ActorControlProofEnabled = Config.Bind(
            "ActorControlProof",
            "Enabled",
            false,
            "Enable actor-control proof diagnostics. Diagnostics only; does not spawn, move, target, persist, or command actors.");
        ActorControlDryRunOnly = Config.Bind(
            "ActorControlProof",
            "DryRunOnly",
            true,
            "Keep actor-control proof commands as log/CSV dry runs. No live actor state is changed.");
        DumpActorControlCandidates = Config.Bind(
            "ActorControlProof",
            "DumpActorControlCandidates",
            false,
            "Write actor-control candidate, component, and dry-run command CSVs once per loaded save.");
        ActorControlScanRadius = Config.Bind(
            "ActorControlProof",
            "ScanRadius",
            35f,
            "Radius around the hero used by actor-control diagnostics.");
        SelectedActorControlCandidateGuid = Config.Bind(
            "ActorControlProof",
            "SelectedCandidateGuid",
            string.Empty,
            "Optional reviewed candidate Location ID or template GUID used for dry-run command logging.");
        AllowOneSessionPetControl = Config.Bind(
            "ActorControlProof",
            "AllowOneSessionPetControl",
            false,
            "Reserved for a later reviewed one-session pet/summon control proof. This build still logs dry-run command decisions only.");
        AllowDefendAction = Config.Bind(
            "ActorControlProof",
            "AllowDefendAction",
            false,
            "Reserved for a later faction-matrix-gated defend proof. This build never forces attack, target, or faction behavior.");
        EnableAvalonCoreDiagnosticBridge = Config.Bind(
            "AvalonCore",
            "EnableDiagnosticBridge",
            true,
            "Read Avalon Core host trust reports by optional reflection and log one compact read-only diagnostic summary when Avalon Core is installed. Does not add a dependency, register adapters, register companion profiles, or change gameplay.");

        _harmony = new Harmony(PluginGuid);
        CompanionPatchRegistry.Apply(_harmony, Logger);

        Logger.LogInfo($"{PluginName} {PluginVersion} loaded. Enabled={Enabled.Value}; ResearchModeOnly={ResearchModeOnly.Value}; LogPetSystemDiagnostics={LogPetSystemDiagnostics.Value}; WritePetTemplateDump={WritePetTemplateDump.Value}; WritePetCreatureShortlistDump={WritePetCreatureShortlistDump.Value}; WritePetCreatureDiagnosticToolCrosscheck={WritePetCreatureDiagnosticToolCrosscheck.Value}; DiagnosticToolMinimumSpawnerRefs={DiagnosticToolMinimumSpawnerRefs.Value}; EnablePetCompanionRoster={EnablePetCompanionRoster.Value}; SpawnQrkoPetShortcut={SpawnQrkoPetShortcut.Value}; EnableDebugPanel={EnableDebugPanel.Value}; ShowCompanionHudIcon={ShowCompanionHudIcon.Value}; CompanionHudIconCorner={CompanionHudIconCorner.Value}; CompanionHudIconSize={CompanionHudIconSize.Value}; EnableLifecycleSafetyGuard={EnableLifecycleSafetyGuard.Value}; WriteCompanionLifecycleDump={WriteCompanionLifecycleDump.Value}; FollowRangeProfile={FollowRangeProfile.Value}; RememberedCompanionGuid={RememberedCompanionGuid.Value}; RememberedCommandMode={RememberedCommandMode.Value}; RememberedFollowRange={RememberedFollowRange.Value}; EnableNativeCommandMenu={EnableNativeCommandMenu.Value}; EnableCustomDialogueInterface={EnableCustomDialogueInterface.Value}; EnableNativeQuickCommands={EnableNativeQuickCommands.Value}; WriteCompanionCommandLog={WriteCompanionCommandLog.Value}; WriteCompanionTrustLog={WriteCompanionTrustLog.Value}; WriteCompanionAiBoundaryDiagnostics={WriteCompanionAiBoundaryDiagnostics.Value}; CompanionAiBoundaryDiagnosticSeconds={CompanionAiBoundaryDiagnosticSeconds.Value}; WriteCompanionAiProfileAudit={WriteCompanionAiProfileAudit.Value}; CompanionAiProfileAuditSeconds={CompanionAiProfileAuditSeconds.Value}; EnableProfileDrivenNativeAssist={EnableProfileDrivenNativeAssist.Value}; ProfileDrivenNativeAssistSeconds={ProfileDrivenNativeAssistSeconds.Value}; EnableCompanionTrustRuntime={EnableCompanionTrustRuntime.Value}; EnableCompanionBondPolicyRuntime={EnableCompanionBondPolicyRuntime.Value}; EnableCompanionProfileStore={EnableCompanionProfileStore.Value}; ActorControlProofEnabled={ActorControlProofEnabled.Value}; DumpActorControlCandidates={DumpActorControlCandidates.Value}; ActorControlDryRunOnly={ActorControlDryRunOnly.Value}; EnableAvalonCoreDiagnosticBridge={EnableAvalonCoreDiagnosticBridge.Value}.");
    }

    private void Update()
    {
        AvalonCoreDiagnosticBridge.TryLog(Logger);
        PetSystemDiagnostics.TryLog(Logger);
        ActorControlDiagnostics.TryLog(Logger);
        PetCompanionController.Update(Logger);
    }

    private void LateUpdate()
    {
        PetCompanionController.LateUpdate();
    }

    private void OnGUI()
    {
        PetCompanionController.DrawGui(Logger);
    }

    private void OnDestroy()
    {
        Config.Save();
        _harmony?.UnpatchSelf();
        PetCompanionController.Shutdown();
    }
}
