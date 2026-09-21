using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Awaken.TG.Main.AI;
using Awaken.TG.Main.AI.Movement;
using Awaken.TG.Main.AI.SummonsAndAllies;
using Awaken.TG.Main.Character;
using Awaken.TG.Main.Fights;
using Awaken.TG.Main.Fights.Factions;
using Awaken.TG.Main.Fights.NPCs;
using Awaken.TG.Main.Fights.NPCs.Providers;
using Awaken.TG.MVC;
using Awaken.TG.Main.Heroes;
using Awaken.TG.Main.Heroes.Interactions;
using Awaken.TG.Main.Locations;
using Awaken.TG.Main.Locations.Actions;
using Awaken.TG.Main.Locations.Pets;
using Awaken.TG.Main.Locations.Pets.Variants;
using Awaken.TG.Main.Locations.Setup;
using Awaken.TG.Main.Locations.Spawners;
using Awaken.TG.Main.Templates;
using AvalonCompanions.Framework;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace AvalonCompanions.Patches;

internal static partial class PetCompanionController
{
    private const int PanelWindowId = 648219;
    private const int DialogueWindowId = 648220;
    private const float SpawnOffsetMeters = 1.75f;
    private const float RecallSpacingMeters = 0.6f;
    private const float CreatureCandidateCatchUpDistance = 22f;
    private const float CreatureCandidateRecallSpacingMeters = 1.6f;
    private const float CreatureCandidateFollowTickSeconds = 0.75f;
    private const float CreatureCandidateDefendTickSeconds = 0.35f;
    private const float CreatureCandidateDefendPromptCooldownSeconds = 2.5f;
    private const float CreatureCandidateDefendLogSeconds = 4f;
    private const float LifecycleSafetyTickSeconds = 2f;
    private const float LifecycleLongHeroMoveMeters = 80f;
    private const float MinimumAiBoundaryDiagnosticSeconds = 0.5f;
    private const float AiProfileAuditSummaryLogSeconds = 30f;
    private const float TamingEncounterProbeRadius = 35f;
    private const float CloseFollowPlacementScale = 0.72f;
    private const float NormalFollowPlacementScale = 1.0f;
    private const float FarFollowPlacementScale = 1.35f;
    private const float CloseCreatureCatchUpDistance = 14f;
    private const float NormalCreatureCatchUpDistance = CreatureCandidateCatchUpDistance;
    private const float FarCreatureCatchUpDistance = 34f;
    private const float RecoveryOutOfRangeDistance = FarCreatureCatchUpDistance;
    private const float MinimumPanelWidth = 620f;
    private const float MinimumPanelHeight = 620f;
    private const float MinimumDialogueWidth = 620f;
    private const float MinimumDialogueHeight = 520f;
    private const float PanelScreenMargin = 28f;
    private const float HeaderDragHeight = 62f;
    private const float CompanionHudLookupIntervalSeconds = 0.5f;
    private const int CompanionProfileStoreSchemaVersion = 1;
    private const string CompanionHudIconBackgroundId = "hud.companion-icon-background";
    private const string CompanionProfileStoreFileName = "companion-profiles.tsv";
    private const string OneSessionOffensiveCreatureBoundary =
        "One-session offensive creature candidate from FOA-Diagnostic Tool 0.4.3 creature catalog evidence. Uses native NpcHeroPetAlly ownership and defend entry only; no custom targeting or save persistence path is approved.";
    private const string OneSessionUndeadCreatureBoundary =
        "One-session undead creature candidate from FOA-Diagnostic Tool 0.4.3 creature catalog evidence. Uses native NpcHeroPetAlly ownership and defend entry only; no custom targeting or save persistence path is approved.";
    private const string OneSessionPassiveAnimalBoundary =
        "One-session animal candidate from FOA-Diagnostic Tool 0.4.3 creature catalog evidence. Uses native NpcHeroPetAlly ownership for follow/recall/dismiss; combat capability is not guaranteed and no save persistence path is approved.";

    private static readonly NativeCompanionCommand[] NativeQuickCommandCycle =
    {
        NativeCompanionCommand.Follow,
        NativeCompanionCommand.Stay,
        NativeCompanionCommand.Defend,
        NativeCompanionCommand.ComeClose,
        NativeCompanionCommand.Recall,
        NativeCompanionCommand.Recover,
        NativeCompanionCommand.Dismiss,
    };

    private static readonly PetRosterEntry[] Roster =
    {
        new PetRosterEntry("Qrko", "Spec_Pet_Qrko", "cc30c92e0699e3c41be92d1e99057354", runtimeApproved: true, requiresPetComponent: true, placementRight: SpawnOffsetMeters, placementBack: 0f, reviewId: "AVALON-PET-QRKO-001", blockReason: string.Empty),
        new PetRosterEntry("Qrko 02", "Spec_Pet_Qrko_02", "6e22cbf2d8d366f4ea674e822d40904a", runtimeApproved: true, requiresPetComponent: true, placementRight: SpawnOffsetMeters, placementBack: 0f, reviewId: "AVALON-PET-QRKO-002", blockReason: string.Empty),
        new PetRosterEntry("Qrko 03", "Spec_Pet_Qrko_03", "61304dcf14d3fff40892fea15be634c2", runtimeApproved: true, requiresPetComponent: true, placementRight: SpawnOffsetMeters, placementBack: 0f, reviewId: "AVALON-PET-QRKO-003", blockReason: string.Empty),
        new PetRosterEntry("Qrko 05", "Spec_Pet_Qrko_05", "1356b1187a7dd5f458b10d47ab016297", runtimeApproved: true, requiresPetComponent: true, placementRight: SpawnOffsetMeters, placementBack: 0f, reviewId: "AVALON-PET-QRKO-005", blockReason: string.Empty),
        new PetRosterEntry("Wolf Candidate", "Spec_AnimalWolf", "9086dee514edc644b9b55890d885db3f", runtimeApproved: true, requiresPetComponent: false, placementRight: 2.6f, placementBack: 5.2f, reviewId: "AVALON-PET-WOLF-001", blockReason: "One-session creature candidate from FOA-Diagnostic Tool spawner evidence. Uses native NpcHeroPetAlly ownership and defend entry only; no custom targeting or save persistence path is approved."),
        new PetRosterEntry("Bear Candidate", "Spec_AnimalBear", "c45508309b84907429f83d1361918fc2", runtimeApproved: true, requiresPetComponent: false, placementRight: 3.2f, placementBack: 6.8f, reviewId: "AVALON-PET-BEAR-001", blockReason: "One-session creature candidate from FOA-Diagnostic Tool spawner evidence. Uses native NpcHeroPetAlly ownership and defend entry only; no custom targeting or save persistence path is approved."),
        new PetRosterEntry("Deer Candidate", "Spec_AnimalDeer", "467a9c9208854394cbb77032251288a1", runtimeApproved: true, requiresPetComponent: false, placementRight: -2.6f, placementBack: 5.2f, reviewId: "AVALON-PET-DEER-001", blockReason: "One-session passive animal candidate from FOA-Diagnostic Tool spawner evidence. Uses native NpcHeroPetAlly ownership for follow/recall/dismiss only; combat capability is not guaranteed and no save persistence path is approved."),
        new PetRosterEntry("Pig Candidate", "Spec_AnimalPig", "3e24c74d7d86f6743b05d4c3587de57d", runtimeApproved: true, requiresPetComponent: false, placementRight: -2.8f, placementBack: 6.4f, reviewId: "AVALON-PET-PIG-001", blockReason: "One-session passive animal candidate from FOA-Diagnostic Tool spawner evidence. Uses native NpcHeroPetAlly ownership for follow/recall/dismiss only; combat capability is not guaranteed and no save persistence path is approved."),
        new PetRosterEntry("Cow Candidate", "Spec_AnimalCow", "6217a5ecb1ff2914b854b9304d9bd54b", runtimeApproved: true, requiresPetComponent: false, placementRight: -3.4f, placementBack: 7.6f, reviewId: "AVALON-PET-COW-001", blockReason: "One-session passive animal candidate from FOA-Diagnostic Tool spawner evidence. Uses native NpcHeroPetAlly ownership for follow/recall/dismiss only; combat capability is not guaranteed and no save persistence path is approved."),
        new PetRosterEntry("Bullrat Candidate", "Spec_EnemyMonster_T4_Bullrat", "dc69c95f2c2930841aab6b7cfe48b4d5", runtimeApproved: true, requiresPetComponent: false, placementRight: 2.4f, placementBack: 6.2f, reviewId: "AVALON-PET-BULLRAT-001", blockReason: "One-session offensive creature candidate from FOA-Diagnostic Tool spawner evidence. Uses native NpcHeroPetAlly ownership and defend entry only; no custom targeting or save persistence path is approved."),
        new PetRosterEntry("Bear Interaction Candidate", "Spec_AnimalBear_Interactions", "63fa2f502163174468799959dd025319", runtimeApproved: true, requiresPetComponent: false, placementRight: 3.4f, placementBack: 8.2f, reviewId: "AVALON-PET-BEAR-INTERACTIONS-001", blockReason: OneSessionPassiveAnimalBoundary),
        new PetRosterEntry("Corpse Eater Candidate", "Spec_EnemyMonster_T1_CorpseEater", "1a41678c288c2264c8bcfad7a6eb3ba3", runtimeApproved: true, requiresPetComponent: false, placementRight: 2.8f, placementBack: 6.8f, reviewId: "AVALON-CREATURE-CORPSE-EATER-001", blockReason: OneSessionOffensiveCreatureBoundary),
        new PetRosterEntry("Redcap Candidate", "Spec_EnemyMonster_T1_Redcap", "a32e5074492cce34f89ff0667fdb41b7", runtimeApproved: true, requiresPetComponent: false, placementRight: 2.4f, placementBack: 6.2f, reviewId: "AVALON-CREATURE-REDCAP-001", blockReason: OneSessionOffensiveCreatureBoundary),
        new PetRosterEntry("Flamegobbler Candidate", "Spec_EnemyMonster_T1_Flamegobbler", "b6bf58d3c36663048bc83341ff0111d2", runtimeApproved: true, requiresPetComponent: false, placementRight: 2.8f, placementBack: 6.8f, reviewId: "AVALON-CREATURE-FLAMEGOBBLER-001", blockReason: OneSessionOffensiveCreatureBoundary),
        new PetRosterEntry("Grindylow Candidate", "Spec_EnemyMonster_T1_Grindylow", "fa79aaa0bff59484dab2cf35c5ea805c", runtimeApproved: true, requiresPetComponent: false, placementRight: 2.6f, placementBack: 6.4f, reviewId: "AVALON-CREATURE-GRINDYLOW-001", blockReason: OneSessionOffensiveCreatureBoundary),
        new PetRosterEntry("Wyrdspirit Candidate", "Spec_EnemyMonster_T1_Wyrdspirit", "843643575fa01ba4292e60afb9291fea", runtimeApproved: true, requiresPetComponent: false, placementRight: 2.8f, placementBack: 7.2f, reviewId: "AVALON-CREATURE-WYRDSPIRIT-001", blockReason: OneSessionOffensiveCreatureBoundary),
        new PetRosterEntry("Sharg Candidate", "Spec_EnemyMonster_T2_ShargHoS", "324e9b5ed131ce34eb12a520cdb2b52a", runtimeApproved: true, requiresPetComponent: false, placementRight: 3.0f, placementBack: 7.4f, reviewId: "AVALON-CREATURE-SHARG-HOS-001", blockReason: OneSessionOffensiveCreatureBoundary),
        new PetRosterEntry("Ogre Candidate", "Spec_EnemyMonster_T3_Ogre", "3f7d4ccf62c440b40b1fca822ef6ac1b", runtimeApproved: true, requiresPetComponent: false, placementRight: 4.8f, placementBack: 10.4f, reviewId: "AVALON-CREATURE-OGRE-001", blockReason: OneSessionOffensiveCreatureBoundary),
        new PetRosterEntry("Zombie Candidate", "Spec_EnemyZombie_T1_Classic", "1d110a8ec95ab1745a364562ec311e50", runtimeApproved: true, requiresPetComponent: false, placementRight: 2.4f, placementBack: 6.2f, reviewId: "AVALON-CREATURE-ZOMBIE-CLASSIC-001", blockReason: OneSessionUndeadCreatureBoundary),
        new PetRosterEntry("Harder Zombie Candidate", "Spec_EnemyZombie_T1_ClassicHarder", "2b46e149155c56441b7c91cb1745ec6c", runtimeApproved: true, requiresPetComponent: false, placementRight: 2.6f, placementBack: 6.6f, reviewId: "AVALON-CREATURE-ZOMBIE-HARDER-001", blockReason: OneSessionUndeadCreatureBoundary),
        new PetRosterEntry("Drowner Candidate", "Spec_EnemyZombie_T1_Drowner", "bb613531c5d3bf5499ea3b8103a4024e", runtimeApproved: true, requiresPetComponent: false, placementRight: 2.4f, placementBack: 6.2f, reviewId: "AVALON-CREATURE-DROWNER-001", blockReason: OneSessionUndeadCreatureBoundary),
        new PetRosterEntry("Armored Drowner Candidate", "Spec_EnemyZombie_T1_DrownerFullArmor", "b131b8352811a1b4488c21ae8682fbbe", runtimeApproved: true, requiresPetComponent: false, placementRight: 2.6f, placementBack: 6.6f, reviewId: "AVALON-CREATURE-DROWNER-FULL-ARMOR-001", blockReason: OneSessionUndeadCreatureBoundary),
        new PetRosterEntry("Head-Armored Drowner Candidate", "Spec_EnemyZombie_T1_DrownerHeadArmor", "cb6b4e81043784946bcbf56b91ebc968", runtimeApproved: true, requiresPetComponent: false, placementRight: 2.8f, placementBack: 6.8f, reviewId: "AVALON-CREATURE-DROWNER-HEAD-ARMOR-001", blockReason: OneSessionUndeadCreatureBoundary),
        new PetRosterEntry("Skeleton 1H Candidate", "Spec_EnemySkeleton_Melee1H", "386190b9f098e414c8d88472306aaad8", runtimeApproved: true, requiresPetComponent: false, placementRight: 2.4f, placementBack: 6.2f, reviewId: "AVALON-CREATURE-SKELETON-1H-001", blockReason: OneSessionUndeadCreatureBoundary),
        new PetRosterEntry("Skeleton 2H Candidate", "Spec_EnemySkeleton_Melee2H", "6af0ddfc613f7e44a81d40e2979f514b", runtimeApproved: true, requiresPetComponent: false, placementRight: 2.8f, placementBack: 6.8f, reviewId: "AVALON-CREATURE-SKELETON-2H-001", blockReason: OneSessionUndeadCreatureBoundary),
        new PetRosterEntry("Floatling Candidate", "Spec_SoS_EnemyMonster_T4_Floatling_WithSpawnAnimation", "560b1076391307948b338acf33d85a1a", runtimeApproved: true, requiresPetComponent: false, placementRight: 2.8f, placementBack: 7.2f, reviewId: "AVALON-CREATURE-FLOATLING-001", blockReason: OneSessionOffensiveCreatureBoundary),
        new PetRosterEntry("Tadpole Candidate", "Spec_SoS_EnemyMonster_T4_TadpoleHatchery", "cea93c44977ce3940be214156db7a0e1", runtimeApproved: true, requiresPetComponent: false, placementRight: 2.6f, placementBack: 6.6f, reviewId: "AVALON-CREATURE-TADPOLE-HATCHERY-001", blockReason: OneSessionOffensiveCreatureBoundary),
    };

    private static readonly List<Location> CreatureCandidateLocations = new List<Location>();
    private static readonly List<InputModuleState> InputModuleStates = new List<InputModuleState>();
    private static readonly Dictionary<Location, float> NextCreatureCandidateDefendPromptAt = new Dictionary<Location, float>();
    private static readonly Dictionary<string, CompanionCoreRuntimeProfile> CoreRuntimeProfiles = new Dictionary<string, CompanionCoreRuntimeProfile>(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, CompanionProgressionStoreRecord> ProgressionProfileStore = new Dictionary<string, CompanionProgressionStoreRecord>(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<Location, string> NativeCommandSurfaceKeys = new Dictionary<Location, string>();
    private static readonly HashSet<Texture2D> SharedPanelTextures = new HashSet<Texture2D>();

    private static bool _blockedByResearchModeLogged;
    private static bool _readyLogged;
    private static ManualLogSource? _logger;
    private static CompanionMode _companionMode = CompanionMode.Follow;
    private static bool _creatureCandidateFollow = true;
    private static float _nextCreatureCandidateFollowTick;
    private static float _nextCreatureCandidateDefendTick;
    private static float _nextCreatureCandidateDefendLog;
    private static bool _lastDefendHadAttackers;
    private static int _lastDefendAttackerCount;
    private static float _nextLifecycleSafetyTick;
    private static float _nextAiBoundaryDiagnosticAt;
    private static float _nextAiProfileAuditAt;
    private static float _nextAiProfileAuditSummaryLogAt;
    private static float _nextProfileDrivenNativeAssistAt;
    private static bool _hasLifecycleHeroCoords;
    private static Vector3 _lastLifecycleHeroCoords;
    private static string _lastLifecycleScene = string.Empty;
    private static bool _lifecycleDumpFailureLogged;
    private static bool _commandLogFailureLogged;
    private static bool _profileStoreLoaded;
    private static bool _profileStoreLoadFailureLogged;
    private static bool _profileStoreSaveFailureLogged;
    private static bool _aiBoundaryDiagnosticFailureLogged;
    private static bool _aiProfileAuditFailureLogged;
    private static bool _tamingEncounterProbeFailureLogged;
    private static string _lastLifecycleSummary = "Lifecycle: not sampled";
    private static string _lastAiProfileAuditLogKey = string.Empty;
    private static bool _continuityStateLoaded;
    private static bool _hasRememberedContinuityMetadata;
    private static string _lastContinuityStatusLogKey = string.Empty;
    private static int _nativeQuickCommandCursor;
    private static string _statusText = string.Empty;
    private static float _statusUntil;
    private static bool _panelVisible;
    private static bool _dialogueVisible;
    private static Location? _dialogueLocation;
    private static bool _controllerCursorScopeActive;
    private static bool _taintedInterfaceScopeActive;
    private static bool _cursorCaptured;
    private static bool _cursorWasVisible;
    private static CursorLockMode _cursorWasLocked;
    private static bool _inputModulesCaptured;
    private static Rect _panelRect;
    private static bool _panelInitialized;
    private static int _lastPanelScreenWidth;
    private static int _lastPanelScreenHeight;
    private static Rect _dialogueRect;
    private static bool _dialogueInitialized;
    private static int _lastDialogueScreenWidth;
    private static int _lastDialogueScreenHeight;
    private static Texture2D? _panelTexture;
    private static Texture2D? _panelAltTexture;
    private static Texture2D? _buttonTexture;
    private static Texture2D? _buttonHoverTexture;
    private static Texture2D? _selectedButtonTexture;
    private static Texture2D? _dangerButtonTexture;
    private static Texture2D? _dangerHoverTexture;
    private static Texture2D? _rowTexture;
    private static GUIStyle? _titleStyle;
    private static GUIStyle? _labelStyle;
    private static GUIStyle? _mutedStyle;
    private static GUIStyle? _buttonStyle;
    private static GUIStyle? _selectedButtonStyle;
    private static GUIStyle? _dangerButtonStyle;
    private static GUIStyle? _statusStyle;
    private static GUIStyle? _windowStyle;
    private static GUIStyle? _headerStyle;
    private static GUIStyle? _sectionStyle;
    private static GUIStyle? _footerStyle;
    private static bool _sharedStylesLogged;
    private static float _nextHudCompanionLookupAt;
    private static bool _hudCompanionEntryAvailable;
    private static PetRosterEntry _hudCompanionEntry;
    private static string _hudCompanionIconId = string.Empty;
    private static Texture2D? _hudCompanionIconTexture;
    private static Texture2D? _hudCompanionBackgroundTexture;
    private static float _nextHudBackgroundLookupAt;

    internal static bool IsPanelVisible => _panelVisible;

    internal static bool IsDialogueVisible => _dialogueVisible;

    internal static bool IsCompanionUiVisible => _panelVisible || _dialogueVisible;

    internal static void Update(ManualLogSource logger)
    {
        _logger = logger;
        LoadContinuityStateOnce();

        if (!Plugin.Enabled.Value || !Plugin.EnablePetCompanionRoster.Value)
        {
            RemoveAllNativeCommandActions();

            if (_panelVisible)
            {
                SetPanelVisible(false, logger);
            }

            if (_dialogueVisible)
            {
                SetDialogueVisible(false, logger);
            }

            RestorePanelInputState();
            UpdateCursor();
            return;
        }

        HandlePanelToggle(logger);

        if (Plugin.ResearchModeOnly.Value)
        {
            RemoveAllNativeCommandActions();
            if (_dialogueVisible)
            {
                SetDialogueVisible(false, logger);
            }

            if (!_blockedByResearchModeLogged)
            {
                _blockedByResearchModeLogged = true;
                logger.LogWarning("Pet companion roster is enabled, but Research.ResearchModeOnly=true blocks runtime companion controls.");
            }

            HandleUnavailableCommandHotkeys(logger, "Research.ResearchModeOnly=true");
            UpdateCursor();
            return;
        }

        string runtimeBlockReason = GetRuntimeBlockReason();
        if (!string.IsNullOrEmpty(runtimeBlockReason))
        {
            RemoveAllNativeCommandActions();
            if (_dialogueVisible)
            {
                SetDialogueVisible(false, logger);
            }

            HandleUnavailableCommandHotkeys(logger, runtimeBlockReason);
            UpdateCursor();
            return;
        }

        if (!_readyLogged)
        {
            _readyLogged = true;
            PetRosterEntry selected = GetSelectedPet();
            logger.LogInfo(
                "Pet companion roster ready. "
                + $"{Plugin.SpawnQrkoPetShortcut.Value}=summon/swap, "
                + $"{Plugin.NextPetShortcut.Value}=next, "
                + $"{Plugin.PreviousPetShortcut.Value}=previous, "
                + $"{Plugin.RecallPetShortcut.Value}=recall, "
                + $"{Plugin.DismissPetShortcut.Value}=dismiss, "
                + $"{Plugin.TogglePetFollowShortcut.Value}=follow/stay. "
                + $"Selected={selected.DisplayName}[{selected.Guid}]; RuntimeApproved={selected.RuntimeApproved}; RequiresPetComponent={selected.RequiresPetComponent}; Review={selected.ReviewId}.");
        }

        EnsureNativeCommandActions(logger);
        TickCompanionLifecycleGuard(logger);
        ReportRememberedContinuityStatus(logger);
        TickCompanionAiBoundaryDiagnostics(logger);
        TickCompanionAiProfileAudit(logger);
        TickProfileDrivenNativeAssist(logger);

        if (Plugin.NextPetShortcut.Value.IsDown())
        {
            SelectRelativePet(logger, 1);
        }

        if (Plugin.PreviousPetShortcut.Value.IsDown())
        {
            SelectRelativePet(logger, -1);
        }

        if (Plugin.RecallPetShortcut.Value.IsDown())
        {
            RecallManagedPets(logger);
        }

        if (Plugin.TogglePetFollowShortcut.Value.IsDown())
        {
            ToggleFollowStayMode(logger);
        }

        if (Plugin.DismissPetShortcut.Value.IsDown())
        {
            DismissManagedPets(logger, "dismiss shortcut");
        }

        if (Plugin.SpawnQrkoPetShortcut.Value.IsDown())
        {
            SummonSelectedPet(logger);
        }

        if (_panelVisible)
        {
            Input.ResetInputAxes();
            EnsurePanelInputBlocked();
        }
        else if (_dialogueVisible)
        {
            EnsureDialogueUiHost(logger);
        }

        TickCreatureCandidateFollow(logger);
        TickCreatureCandidateDefend(logger);
        UpdateCursor();
    }

    internal static void LateUpdate()
    {
        if (!IsCompanionUiVisible)
        {
            return;
        }

        EnsureCursorForPanel();
        if (_panelVisible)
        {
            Input.ResetInputAxes();
            EnsurePanelInputBlocked();
        }
        else if (_dialogueVisible)
        {
            EnsureDialogueUiHost(_logger);
        }
    }

    internal static void Shutdown()
    {
        if (_logger != null && (Plugin.EnableLifecycleSafetyGuard.Value || Plugin.WriteCompanionLifecycleDump.Value))
        {
            RunCompanionLifecycleSafetyPass(_logger, "shutdown", forceDump: true);
        }

        SetPanelVisible(false, logger: null);
        SetDialogueVisible(false, logger: null);
        DestroyDialogueUiHost();
        ClearExternalDialogueCommands();
        RemoveAllNativeCommandActions();
        CreatureCandidateLocations.Clear();
        NextCreatureCandidateDefendPromptAt.Clear();
        NativeCommandSurfaceKeys.Clear();
        CoreRuntimeProfiles.Clear();
        _aiRuntimeOwnerId = string.Empty;
        ClearAiRuntimePendingObservation();
        _lastDefendHadAttackers = false;
        _lastDefendAttackerCount = 0;
        RestorePanelInputState();
        RestoreCursorState();
    }

    internal static bool IsNativeCompanionCommandAvailable(Location location, Hero? hero, IInteractableWithHero interactable)
    {
        return ShouldUseNativeCommandMenu() && IsNativeCompanionActionBaseAvailable(location, hero, interactable);
    }

    private static bool IsNativeCompanionActionBaseAvailable(Location location, Hero? hero, IInteractableWithHero interactable)
    {
        if (location == null || location.HasBeenDiscarded || hero == null)
        {
            return false;
        }

        if (!ReferenceEquals(interactable, location))
        {
            return false;
        }

        return string.IsNullOrEmpty(GetRuntimeBlockReason()) && IsManagedNativeInteractionCompanion(location);
    }

    private static bool ShouldUseNativeCommandMenu()
    {
        return Plugin.EnableNativeCommandMenu.Value && Plugin.EnableCustomDialogueInterface.Value;
    }

    private static bool ShouldUseNativeQuickCommands()
    {
        return !Plugin.EnableNativeCommandMenu.Value && Plugin.EnableNativeQuickCommands.Value;
    }

    internal static bool IsNativeCompanionQuickCommandAvailable(Location location, Hero? hero, IInteractableWithHero interactable, NativeCompanionCommand command)
    {
        if (!ShouldUseNativeQuickCommands() || !IsNativeCompanionActionBaseAvailable(location, hero, interactable))
        {
            return false;
        }

        return command == GetCurrentNativeQuickCommand(location);
    }

    private static NativeCompanionCommand GetCurrentNativeQuickCommand(Location? location = null)
    {
        int start = NormalizeNativeQuickCommandCursor(_nativeQuickCommandCursor);
        for (int offset = 0; offset < NativeQuickCommandCycle.Length; offset++)
        {
            NativeCompanionCommand command = NativeQuickCommandCycle[(start + offset) % NativeQuickCommandCycle.Length];
            if (IsNativeQuickCommandRunnable(command, location))
            {
                return command;
            }
        }

        return NativeCompanionCommand.Recall;
    }

    private static bool IsNativeQuickCommandRunnable(NativeCompanionCommand command, Location? location = null)
    {
        if (location != null && !IsLocationCommandPolicyAllowed(location, GetPolicyAction(command)))
        {
            return false;
        }

        return command switch
        {
            NativeCompanionCommand.Follow => _companionMode != CompanionMode.Follow,
            NativeCompanionCommand.Stay => _companionMode != CompanionMode.Stay,
            NativeCompanionCommand.Defend => _companionMode != CompanionMode.Defend,
            NativeCompanionCommand.ComeClose => true,
            NativeCompanionCommand.Recall => true,
            NativeCompanionCommand.Recover => true,
            NativeCompanionCommand.Dismiss => true,
            _ => false,
        };
    }

    private static int NormalizeNativeQuickCommandCursor(int cursor)
    {
        int length = NativeQuickCommandCycle.Length;
        return ((cursor % length) + length) % length;
    }

    private static void AdvanceNativeQuickCommandCursor(NativeCompanionCommand command)
    {
        int index = Array.IndexOf(NativeQuickCommandCycle, command);
        _nativeQuickCommandCursor = NormalizeNativeQuickCommandCursor(index < 0 ? _nativeQuickCommandCursor + 1 : index + 1);
    }

    private static void ResetNativeQuickCommandCursor()
    {
        _nativeQuickCommandCursor = 0;
    }

    private static void QueueResponsiveNativeRefresh()
    {
        _nextCreatureCandidateFollowTick = 0f;
        _nextCreatureCandidateDefendTick = 0f;
    }

    private static CompanionCoreRuntimeProfile GetOrCreateCoreRuntimeProfile(PetRosterEntry entry)
    {
        if (!CoreRuntimeProfiles.TryGetValue(entry.Guid, out CompanionCoreRuntimeProfile profile))
        {
            profile = new CompanionCoreRuntimeProfile(
                entry,
                ClassifyRuntimeCombatRole(entry),
                ClassifyRuntimeTemperament(entry),
                ClassifyRuntimeCommandCapabilities(entry),
                ComputeRuntimeProfileSeed(entry),
                Time.frameCount);
            TryHydrateProgressionProfileFromStore(entry, profile);
            CoreRuntimeProfiles[entry.Guid] = profile;
        }

        profile.LastKnownMode = _companionMode;
        profile.LastKnownFollowRange = GetFollowRangeProfile();
        profile.LastSeenFrame = Time.frameCount;
        return profile;
    }

    private static void TouchCoreRuntimeProfile(
        ManualLogSource? logger,
        PetRosterEntry entry,
        string command,
        string reason,
        CompanionRuntimeProfileState state,
        int affectedCount)
    {
        CompanionCoreRuntimeProfile profile = GetOrCreateCoreRuntimeProfile(entry);
        profile.RuntimeState = state;
        profile.LastCommand = command;
        profile.LastCommandReason = reason;
        profile.LastCommandFrame = Time.frameCount;
        if (state == CompanionRuntimeProfileState.Active)
        {
            profile.ActivationCount++;
        }

        CompanionTrustChange? trustChange = TryApplyCompanionTrustEvent(profile, command, state, reason);
        if (logger != null)
        {
            WriteCoreRuntimeProfileAudit(logger, profile, command, reason, affectedCount);
            if (trustChange.HasValue && trustChange.Value.Changed)
            {
                WriteCompanionTrustAudit(logger, profile, trustChange.Value, command, reason, affectedCount);
            }
        }

        SaveProgressionProfileStore(logger, profile, affectedCount);
    }

    private static int TouchCoreRuntimeProfiles(
        ManualLogSource? logger,
        IEnumerable<Location> locations,
        string command,
        string reason,
        CompanionRuntimeProfileState state)
    {
        int touched = 0;
        HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (Location location in locations)
        {
            if (location == null || location.HasBeenDiscarded || !TryGetRosterEntry(location, out PetRosterEntry entry))
            {
                continue;
            }

            if (!seen.Add(entry.Guid))
            {
                continue;
            }

            TouchCoreRuntimeProfile(logger, entry, command, reason, state, affectedCount: 1);
            touched++;
        }

        return touched;
    }

    private static void WriteCoreRuntimeProfileAudit(
        ManualLogSource logger,
        CompanionCoreRuntimeProfile profile,
        string command,
        string reason,
        int affectedCount)
    {
        string auditReason =
            $"event={command}; profile=v{profile.ContractVersion}; companion={profile.DisplayName}[{profile.TemplateGuid}]; "
            + $"role={profile.CombatRole}; temperament={profile.Temperament}; state={profile.RuntimeState}; "
            + $"mode={GetModeLabel(profile.LastKnownMode)}; range={GetFollowRangeLabel(profile.LastKnownFollowRange)}; "
            + $"capabilities={FormatRuntimeCapabilities(profile.CommandCapabilities)}; seed={profile.ProfileSeed}; "
            + $"{FormatCompanionTrustAuditSegment(profile)}; {FormatCompanionBondPolicyAuditSegment(profile)}; "
            + $"createdFrame={profile.CreatedFrame}; lastSeenFrame={profile.LastSeenFrame}; "
            + $"activations={profile.ActivationCount}; source={reason}; runtimeOnly=true; "
            + "touchesTargeting=false; touchesPersistence=false; coreExecuted=false";
        WriteCompanionCommandLog(logger, "runtime-profile", affectedCount, blocked: false, reason: auditReason);
    }

    private static string GetCoreRuntimeProfileStatusLine(PetRosterEntry entry)
    {
        CompanionCoreRuntimeProfile profile = GetOrCreateCoreRuntimeProfile(entry);
        return $"Runtime profile: v{profile.ContractVersion}; role={profile.CombatRole}; temperament={profile.Temperament}; state={profile.RuntimeState}; caps={FormatRuntimeCapabilities(profile.CommandCapabilities)}";
    }

    private static string GetCompanionTrustStatusLine(PetRosterEntry entry)
    {
        if (!Plugin.EnableCompanionTrustRuntime.Value)
        {
            return "Trust runtime: disabled";
        }

        CompanionTrustProfile trust = GetOrCreateCoreRuntimeProfile(entry).TrustProfile;
        return $"Bond summary: {GetCompanionBondSummaryLine(entry)}; last={trust.LastEvent}";
    }

    private static string GetCompanionBondPolicyStatusLine(PetRosterEntry entry)
    {
        CompanionBondPolicyDecision policy = GetCompanionBondPolicy(entry);
        if (!Plugin.EnableCompanionTrustRuntime.Value)
        {
            return "Bond policy: unavailable; trust runtime disabled";
        }

        if (!Plugin.EnableCompanionBondPolicyRuntime.Value)
        {
            return "Bond policy: disabled";
        }

        return $"Bond policy: {policy.BondLevel}; catch-up interval x{policy.CatchUpIntervalMultiplier:0.##}; "
            + $"threshold x{policy.CatchUpDistanceMultiplier:0.##}; defend cooldown x{policy.DefendCooldownMultiplier:0.##}; "
            + $"auto defend={(policy.AllowsAutomaticDefendAssist ? "On" : "Explicit only")}; {GetCompanionBondEffectPhrase(policy)}";
    }

    private static string GetCompanionBondSummaryLine(PetRosterEntry entry)
    {
        if (!Plugin.EnableCompanionTrustRuntime.Value)
        {
            return "trust runtime disabled";
        }

        CompanionCoreRuntimeProfile profile = GetOrCreateCoreRuntimeProfile(entry);
        CompanionTrustProfile trust = profile.TrustProfile;
        CompanionBondPolicyDecision policy = GetCompanionBondPolicy(profile);
        return $"{policy.BondLevel} bond; trust {trust.TrustScore} ({trust.TrustLevel}); loyalty {trust.LoyaltyScore} ({trust.LoyaltyLevel})";
    }

    private static string GetCompanionBondDialogueLine(PetRosterEntry entry)
    {
        if (!Plugin.EnableCompanionTrustRuntime.Value)
        {
            return string.Empty;
        }

        CompanionBondPolicyDecision policy = GetCompanionBondPolicy(entry);
        return $"Bond: {GetCompanionBondSummaryLine(entry)}. {GetCompanionBondEffectPhrase(policy)}.";
    }

    private static string GetCompanionBondEffectPhrase(CompanionBondPolicyDecision policy)
    {
        if (!Plugin.EnableCompanionBondPolicyRuntime.Value || !policy.Enabled)
        {
            return "baseline response";
        }

        return policy.BondLevel switch
        {
            CompanionBondLevel.Wary => "slower response; auto-defend needs explicit Defend",
            CompanionBondLevel.Trusted => "faster response; softer penalties",
            CompanionBondLevel.Loyal => "fastest response; strongest penalty protection",
            _ => "baseline response",
        };
    }

    private static void TryHydrateProgressionProfileFromStore(PetRosterEntry entry, CompanionCoreRuntimeProfile profile)
    {
        if (!Plugin.EnableCompanionProfileStore.Value)
        {
            return;
        }

        LoadProgressionProfileStoreOnce(_logger);
        string key = GetProgressionProfileStoreKey(entry);
        if (!ProgressionProfileStore.TryGetValue(key, out CompanionProgressionStoreRecord record))
        {
            return;
        }

        if (!string.Equals(record.TemplateGuid, entry.Guid, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(record.ReviewId, entry.ReviewId, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        profile.TrustProfile.RestoreProfileOnly(
            record.TrustScore,
            record.LoyaltyScore,
            record.LastTrustEvent,
            "loaded profile-only progression store; no actor restored",
            Time.frameCount);
        profile.LastCommandReason = "profile-only progression store loaded; no actor restored";
    }

    private static void SaveProgressionProfileStore(ManualLogSource? logger, CompanionCoreRuntimeProfile profile, int affectedCount)
    {
        if (!Plugin.EnableCompanionProfileStore.Value)
        {
            return;
        }

        LoadProgressionProfileStoreOnce(logger);
        CompanionBondPolicyDecision policy = GetCompanionBondPolicy(profile);
        CompanionProgressionStoreRecord record = CompanionProgressionStoreRecord.FromProfile(profile, policy);
        string key = GetProgressionProfileStoreKey(profile);
        if (ProgressionProfileStore.TryGetValue(key, out CompanionProgressionStoreRecord existing) && existing.Equals(record))
        {
            return;
        }

        ProgressionProfileStore[key] = record;

        try
        {
            string folder = Path.Combine(Paths.ConfigPath, Plugin.PluginGuid);
            Directory.CreateDirectory(folder);
            string path = GetProgressionProfileStorePath();
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("profileSchemaVersion\ttemplateGuid\ttemplateName\tdisplayName\treviewId\trequiresPetComponent\truntimeApproved\ttrustScore\tloyaltyScore\ttrustLevel\tloyaltyLevel\tlastTrustEvent\tlastKnownMode\tlastKnownFollowRange\teffectiveBondLevel");
            foreach (CompanionProgressionStoreRecord saved in ProgressionProfileStore.Values
                         .OrderBy(saved => saved.DisplayName, StringComparer.OrdinalIgnoreCase)
                         .ThenBy(saved => saved.TemplateGuid, StringComparer.OrdinalIgnoreCase)
                         .ThenBy(saved => saved.ReviewId, StringComparer.OrdinalIgnoreCase))
            {
                builder.AppendLine(saved.ToTsvLine());
            }

            File.WriteAllText(path, builder.ToString(), Encoding.UTF8);
            if (logger != null)
            {
                string reason =
                    $"profileOnlyPersistence=true; schemaVersion={CompanionProfileStoreSchemaVersion}; file={CompanionProfileStoreFileName}; "
                    + $"key={record.TemplateGuid}|{record.ReviewId}; companion={record.DisplayName}; "
                    + "savedFields=profileSchemaVersion,templateGuid,templateName,displayName,reviewId,requiresPetComponent,runtimeApproved,"
                    + "trustScore,loyaltyScore,trustLevel,loyaltyLevel,lastTrustEvent,lastKnownMode,lastKnownFollowRange,effectiveBondLevel; "
                    + "noActorRestore=true; noSaveOwnership=true; noAutoRespawn=true; noActorReAdoption=true; "
                    + "storesLocation=false; storesHealth=false; storesAiState=false; storesTargetState=false; "
                    + "touchesCommands=false; touchesMovement=false; touchesTargeting=false; touchesActorPersistence=false; coreExecuted=false";
                WriteCompanionCommandLog(
                    logger,
                    "profile-store-save",
                    affectedCount,
                    blocked: false,
                    reason: reason,
                    touchesPersistence: true);
            }
        }
        catch (Exception ex)
        {
            if (!_profileStoreSaveFailureLogged)
            {
                _profileStoreSaveFailureLogged = true;
                logger?.LogWarning($"Companion profile store write failed: {ex.GetType().Name}: {ex.Message}");
            }
        }
    }

    private static void LoadProgressionProfileStoreOnce(ManualLogSource? logger)
    {
        if (_profileStoreLoaded)
        {
            return;
        }

        _profileStoreLoaded = true;
        ProgressionProfileStore.Clear();
        if (!Plugin.EnableCompanionProfileStore.Value)
        {
            return;
        }

        try
        {
            string path = GetProgressionProfileStorePath();
            if (!File.Exists(path))
            {
                return;
            }

            foreach (string line in File.ReadAllLines(path, Encoding.UTF8).Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                string[] fields = line.Split('\t');
                if (!CompanionProgressionStoreRecord.TryParse(fields, out CompanionProgressionStoreRecord record))
                {
                    continue;
                }

                ProgressionProfileStore[GetProgressionProfileStoreKey(record.TemplateGuid, record.ReviewId)] = record;
            }

            logger?.LogInfo(
                $"Companion profile store loaded {ProgressionProfileStore.Count} profile row(s). "
                + $"Path={path}; profileOnly=true; no actor restore, respawn, save ownership, location, health, AI, or target state loaded.");
        }
        catch (Exception ex)
        {
            if (!_profileStoreLoadFailureLogged)
            {
                _profileStoreLoadFailureLogged = true;
                logger?.LogWarning($"Companion profile store load failed: {ex.GetType().Name}: {ex.Message}");
            }
        }
    }

    private static string GetProgressionProfileStorePath()
    {
        return Path.Combine(Paths.ConfigPath, Plugin.PluginGuid, CompanionProfileStoreFileName);
    }

    private static string GetProgressionProfileStoreKey(PetRosterEntry entry)
    {
        return GetProgressionProfileStoreKey(entry.Guid, entry.ReviewId);
    }

    private static string GetProgressionProfileStoreKey(CompanionCoreRuntimeProfile profile)
    {
        return GetProgressionProfileStoreKey(profile.TemplateGuid, profile.ReviewId);
    }

    private static string GetProgressionProfileStoreKey(string templateGuid, string reviewId)
    {
        return $"{templateGuid}|{reviewId}";
    }

    private static string SanitizeProgressionStoreField(string value)
    {
        return (value ?? string.Empty)
            .Replace('\t', ' ')
            .Replace('\r', ' ')
            .Replace('\n', ' ');
    }

    private static string FormatCompanionTrustAuditSegment(CompanionCoreRuntimeProfile profile)
    {
        if (!Plugin.EnableCompanionTrustRuntime.Value)
        {
            return "trustRuntime=disabled";
        }

        CompanionTrustProfile trust = profile.TrustProfile;
        return $"trust={trust.TrustScore}({trust.TrustLevel}); loyalty={trust.LoyaltyScore}({trust.LoyaltyLevel}); trustLastEvent={trust.LastEvent}";
    }

    private static string FormatCompanionBondPolicyAuditSegment(CompanionCoreRuntimeProfile profile)
    {
        if (!Plugin.EnableCompanionTrustRuntime.Value)
        {
            return "bondPolicy=unavailable";
        }

        if (!Plugin.EnableCompanionBondPolicyRuntime.Value)
        {
            return "bondPolicy=disabled";
        }

        return "bondPolicy=" + GetCompanionBondPolicy(profile).FormatAuditSegment();
    }

    private static CompanionBondPolicyDecision GetCompanionBondPolicy(PetRosterEntry entry)
    {
        return GetCompanionBondPolicy(GetOrCreateCoreRuntimeProfile(entry));
    }

    private static CompanionBondPolicyDecision GetCompanionBondPolicy(Location location)
    {
        return TryGetRosterEntry(location, out PetRosterEntry entry)
            ? GetCompanionBondPolicy(entry)
            : CompanionBondPolicyDecision.Neutral("location has no companion runtime profile");
    }

    private static CompanionBondPolicyDecision GetCompanionBondPolicy(CompanionCoreRuntimeProfile profile)
    {
        return CompanionBondPolicy.Evaluate(
            profile,
            Plugin.EnableCompanionTrustRuntime.Value && Plugin.EnableCompanionBondPolicyRuntime.Value);
    }

    private static CompanionBondPolicyDecision GetActiveCompanionBondPolicy()
    {
        foreach (Location location in GetPetLocations())
        {
            if (location != null && !location.HasBeenDiscarded && IsRosterPet(location))
            {
                return GetCompanionBondPolicy(location);
            }
        }

        return GetCompanionBondPolicy(GetSelectedPet());
    }

    private static CompanionTrustChange? TryApplyCompanionTrustEvent(
        CompanionCoreRuntimeProfile profile,
        string command,
        CompanionRuntimeProfileState state,
        string reason)
    {
        if (!Plugin.EnableCompanionTrustRuntime.Value)
        {
            return null;
        }

        if (!TryMapCompanionTrustEvent(command, state, out CompanionTrustEvent trustEvent))
        {
            return null;
        }

        CompanionBondPolicyDecision bondPolicy = GetCompanionBondPolicy(profile);
        return profile.TrustProfile.Apply(
            trustEvent,
            reason,
            Time.frameCount,
            bondPolicy.TrustGainMultiplier,
            bondPolicy.TrustPenaltyMultiplier);
    }

    private static bool TryMapCompanionTrustEvent(
        string command,
        CompanionRuntimeProfileState state,
        out CompanionTrustEvent trustEvent)
    {
        if (state == CompanionRuntimeProfileState.Dismissed)
        {
            trustEvent = CompanionTrustEvent.Dismissed;
            return true;
        }

        if (state == CompanionRuntimeProfileState.Removed)
        {
            trustEvent = CompanionTrustEvent.Removed;
            return true;
        }

        string normalized = command.Trim().ToLowerInvariant();
        switch (normalized)
        {
            case "dialogue-open":
                trustEvent = CompanionTrustEvent.DialogueOpened;
                return true;
            case "summon/swap":
                trustEvent = CompanionTrustEvent.Summoned;
                return true;
            case "summon-existing-recall":
                trustEvent = CompanionTrustEvent.ExistingRecalled;
                return true;
            case "recall":
                trustEvent = CompanionTrustEvent.Recalled;
                return true;
            case "heel":
                trustEvent = CompanionTrustEvent.ComeClose;
                return true;
            case "mode-follow":
                trustEvent = CompanionTrustEvent.Follow;
                return true;
            case "mode-stay":
                trustEvent = CompanionTrustEvent.Stay;
                return true;
            case "mode-defend":
                trustEvent = CompanionTrustEvent.Defend;
                return true;
            case "auto-catch-up":
            case "ai-profile-native-catch-up":
                trustEvent = CompanionTrustEvent.NativeAssistCatchUp;
                return true;
            case "native-defend-prompt":
                trustEvent = CompanionTrustEvent.NativeAssistDefend;
                return true;
            case "recover-recall":
                trustEvent = CompanionTrustEvent.Recovered;
                return true;
            case "recover-blocked":
                trustEvent = CompanionTrustEvent.BlockedCommand;
                return true;
            case "lifecycle-check":
                trustEvent = CompanionTrustEvent.LifecycleChecked;
                return true;
        }

        if (normalized.StartsWith("select-", StringComparison.Ordinal))
        {
            trustEvent = CompanionTrustEvent.Selected;
            return true;
        }

        if (normalized.StartsWith("range-", StringComparison.Ordinal))
        {
            trustEvent = CompanionTrustEvent.RangeChanged;
            return true;
        }

        trustEvent = CompanionTrustEvent.Created;
        return false;
    }

    private static void WriteCompanionTrustAudit(
        ManualLogSource logger,
        CompanionCoreRuntimeProfile profile,
        CompanionTrustChange change,
        string command,
        string reason,
        int affectedCount)
    {
        if (!Plugin.WriteCompanionTrustLog.Value)
        {
            return;
        }

        string auditReason =
            $"event={change.Event}; sourceCommand={command}; companion={profile.DisplayName}[{profile.TemplateGuid}]; "
            + $"trust={change.PreviousTrust}->{change.TrustScore}({change.TrustLevel}); trustDelta={change.TrustDelta}; "
            + $"loyalty={change.PreviousLoyalty}->{change.LoyaltyScore}({change.LoyaltyLevel}); loyaltyDelta={change.LoyaltyDelta}; "
            + $"source={reason}; runtimeOnly=true; touchesTargeting=false; touchesPersistence=false; coreExecuted=false";
        WriteCompanionCommandLog(logger, "companion-trust", affectedCount, blocked: false, reason: auditReason);
    }

    private static CompanionCommandPolicyDecision EvaluateCommandPolicy(PetRosterEntry entry, CompanionCommandPolicyAction action)
    {
        return CompanionCommandPolicy.Evaluate(GetOrCreateCoreRuntimeProfile(entry), action);
    }

    private static bool IsCommandPolicyAllowed(PetRosterEntry entry, CompanionCommandPolicyAction action)
    {
        return EvaluateCommandPolicy(entry, action).Allowed;
    }

    private static bool IsLocationCommandPolicyAllowed(Location location, CompanionCommandPolicyAction action)
    {
        return TryGetRosterEntry(location, out PetRosterEntry entry) && IsCommandPolicyAllowed(entry, action);
    }

    private static CompanionCommandPolicyAction GetPolicyAction(NativeCompanionCommand command)
    {
        return command switch
        {
            NativeCompanionCommand.Follow => CompanionCommandPolicyAction.Follow,
            NativeCompanionCommand.Stay => CompanionCommandPolicyAction.Stay,
            NativeCompanionCommand.Defend => CompanionCommandPolicyAction.Defend,
            NativeCompanionCommand.ComeClose => CompanionCommandPolicyAction.ComeClose,
            NativeCompanionCommand.Recall => CompanionCommandPolicyAction.Recall,
            NativeCompanionCommand.Recover => CompanionCommandPolicyAction.Recover,
            NativeCompanionCommand.Dismiss => CompanionCommandPolicyAction.Dismiss,
            _ => CompanionCommandPolicyAction.Recall,
        };
    }

    private static CompanionCommandPolicyAction GetPolicyAction(CompanionMode mode)
    {
        return mode switch
        {
            CompanionMode.Stay => CompanionCommandPolicyAction.Stay,
            CompanionMode.Defend => CompanionCommandPolicyAction.Defend,
            _ => CompanionCommandPolicyAction.Follow,
        };
    }

    private static CompanionCommandPolicyAction GetPolicyAction(DialogueButtonKind kind)
    {
        return kind switch
        {
            DialogueButtonKind.ModeFollow => CompanionCommandPolicyAction.Follow,
            DialogueButtonKind.ModeStay => CompanionCommandPolicyAction.Stay,
            DialogueButtonKind.ModeDefend => CompanionCommandPolicyAction.Defend,
            DialogueButtonKind.RangeClose => CompanionCommandPolicyAction.Range,
            DialogueButtonKind.RangeNormal => CompanionCommandPolicyAction.Range,
            DialogueButtonKind.RangeFar => CompanionCommandPolicyAction.Range,
            DialogueButtonKind.Danger => CompanionCommandPolicyAction.Dismiss,
            _ => CompanionCommandPolicyAction.Recall,
        };
    }

    private static CompanionCommandPolicyAction GetPolicyAction(string commandId, DialogueButtonKind kind)
    {
        if (string.Equals(commandId, "follow", StringComparison.OrdinalIgnoreCase))
        {
            return CompanionCommandPolicyAction.Follow;
        }

        if (string.Equals(commandId, "stay", StringComparison.OrdinalIgnoreCase))
        {
            return CompanionCommandPolicyAction.Stay;
        }

        if (string.Equals(commandId, "defend", StringComparison.OrdinalIgnoreCase))
        {
            return CompanionCommandPolicyAction.Defend;
        }

        if (commandId.StartsWith("range-", StringComparison.OrdinalIgnoreCase))
        {
            return CompanionCommandPolicyAction.Range;
        }

        if (string.Equals(commandId, "come-close", StringComparison.OrdinalIgnoreCase))
        {
            return CompanionCommandPolicyAction.ComeClose;
        }

        if (string.Equals(commandId, "recall", StringComparison.OrdinalIgnoreCase))
        {
            return CompanionCommandPolicyAction.Recall;
        }

        if (string.Equals(commandId, "recover", StringComparison.OrdinalIgnoreCase))
        {
            return CompanionCommandPolicyAction.Recover;
        }

        if (string.Equals(commandId, "dismiss", StringComparison.OrdinalIgnoreCase))
        {
            return CompanionCommandPolicyAction.Dismiss;
        }

        return GetPolicyAction(kind);
    }

    private static bool TryAuthorizeCommandPolicy(
        ManualLogSource logger,
        CompanionCommandPolicyAction action,
        string command,
        List<Location> activeRoster,
        string surface,
        out CompanionCommandPolicyDecision decision)
    {
        PetRosterEntry entry = GetCommandPolicyEntry(activeRoster);
        decision = EvaluateCommandPolicy(entry, action);
        WriteCommandPolicyAudit(logger, entry, decision, command, surface, activeRoster.Count);
        if (decision.Allowed)
        {
            return true;
        }

        SetStatus(logger, GetCommandPolicyBlockedStatus(entry, decision), warning: true);
        return false;
    }

    private static PetRosterEntry GetCommandPolicyEntry(List<Location> activeRoster)
    {
        foreach (Location location in activeRoster)
        {
            if (location != null && !location.HasBeenDiscarded && TryGetRosterEntry(location, out PetRosterEntry entry))
            {
                return entry;
            }
        }

        return GetSelectedPet();
    }

    private static void WriteCommandPolicyAudit(
        ManualLogSource logger,
        PetRosterEntry entry,
        CompanionCommandPolicyDecision decision,
        string command,
        string surface,
        int activeRosterCount)
    {
        CompanionCoreRuntimeProfile profile = GetOrCreateCoreRuntimeProfile(entry);
        string result = decision.Allowed ? "allowed" : "blocked";
        string reason =
            $"surface={surface}; command={command}; action={decision.Action}; result={result}; "
            + $"companion={profile.DisplayName}[{profile.TemplateGuid}]; role={profile.CombatRole}; "
            + $"capabilities={FormatRuntimeCapabilities(profile.CommandCapabilities)}; activeRosterCount={activeRosterCount}; "
            + $"policyReason={decision.Reason}; runtimeOnly=true; touchesTargeting=false; touchesPersistence=false; coreExecuted=false";
        WriteCompanionCommandLog(logger, "command-policy", affectedCount: activeRosterCount, blocked: !decision.Allowed, reason: reason);
    }

    private static string GetCommandPolicyStatusLine(PetRosterEntry entry)
    {
        CompanionCommandPolicyDecision defend = EvaluateCommandPolicy(entry, CompanionCommandPolicyAction.Defend);
        return defend.Allowed
            ? "Command policy: Defend available through native-safe ally path."
            : $"Command policy: Defend disabled - {defend.Reason}.";
    }

    private static string GetCommandPolicyBlockedStatus(PetRosterEntry entry, CompanionCommandPolicyDecision decision)
    {
        string name = GetDialogueCompanionName(entry);
        if (decision.Action == CompanionCommandPolicyAction.Defend
            || decision.Action == CompanionCommandPolicyAction.NativeDefendPrompt)
        {
            return $"{name} cannot reliably defend; use Follow, Recall, or Dismiss.";
        }

        return $"{name} cannot take that order: {decision.Reason}.";
    }

    private static string FormatRuntimeCapabilities(CompanionRuntimeCommandCapabilities capabilities)
    {
        List<string> labels = new List<string>();
        if ((capabilities & CompanionRuntimeCommandCapabilities.Follow) != 0)
        {
            labels.Add("Follow");
        }

        if ((capabilities & CompanionRuntimeCommandCapabilities.Stay) != 0)
        {
            labels.Add("Stay");
        }

        if ((capabilities & CompanionRuntimeCommandCapabilities.Range) != 0)
        {
            labels.Add("Range");
        }

        if ((capabilities & CompanionRuntimeCommandCapabilities.Recall) != 0)
        {
            labels.Add("Recall");
        }

        if ((capabilities & CompanionRuntimeCommandCapabilities.Recover) != 0)
        {
            labels.Add("Recover");
        }

        if ((capabilities & CompanionRuntimeCommandCapabilities.Dismiss) != 0)
        {
            labels.Add("Dismiss");
        }

        if ((capabilities & CompanionRuntimeCommandCapabilities.NativeDefendPrompt) != 0)
        {
            labels.Add("NativeDefend");
        }

        return labels.Count == 0 ? "None" : string.Join("|", labels);
    }

    private static CompanionRuntimeCommandCapabilities ClassifyRuntimeCommandCapabilities(PetRosterEntry entry)
    {
        CompanionRuntimeCommandCapabilities capabilities =
            CompanionRuntimeCommandCapabilities.Follow
            | CompanionRuntimeCommandCapabilities.Stay
            | CompanionRuntimeCommandCapabilities.Range
            | CompanionRuntimeCommandCapabilities.Recall
            | CompanionRuntimeCommandCapabilities.Recover
            | CompanionRuntimeCommandCapabilities.Dismiss;

        CompanionRuntimeCombatRole role = ClassifyRuntimeCombatRole(entry);
        if (role == CompanionRuntimeCombatRole.NativePet
            || role == CompanionRuntimeCombatRole.OffensiveAlly
            || role == CompanionRuntimeCombatRole.UndeadAlly)
        {
            capabilities |= CompanionRuntimeCommandCapabilities.NativeDefendPrompt;
        }

        return capabilities;
    }

    private static CompanionRuntimeCombatRole ClassifyRuntimeCombatRole(PetRosterEntry entry)
    {
        if (entry.RequiresPetComponent)
        {
            return CompanionRuntimeCombatRole.NativePet;
        }

        if (ReviewedNativeDefendAnimalPolicy.IsApproved(entry.Guid))
        {
            return CompanionRuntimeCombatRole.OffensiveAlly;
        }

        string key = GetRuntimeProfileKey(entry);
        if (ContainsRuntimeProfileTerm(key, "Zombie")
            || ContainsRuntimeProfileTerm(key, "Drowner")
            || ContainsRuntimeProfileTerm(key, "Skeleton")
            || ContainsRuntimeProfileTerm(key, "Undead"))
        {
            return CompanionRuntimeCombatRole.UndeadAlly;
        }

        if (ContainsRuntimeProfileTerm(key, "Offensive")
            || ContainsRuntimeProfileTerm(key, "EnemyMonster")
            || ContainsRuntimeProfileTerm(key, "Monster")
            || ContainsRuntimeProfileTerm(key, "Bullrat")
            || ContainsRuntimeProfileTerm(key, "CorpseEater")
            || ContainsRuntimeProfileTerm(key, "Redcap")
            || ContainsRuntimeProfileTerm(key, "Flamegobbler")
            || ContainsRuntimeProfileTerm(key, "Grindylow")
            || ContainsRuntimeProfileTerm(key, "Wyrdspirit")
            || ContainsRuntimeProfileTerm(key, "Sharg")
            || ContainsRuntimeProfileTerm(key, "Ogre")
            || ContainsRuntimeProfileTerm(key, "Floatling")
            || ContainsRuntimeProfileTerm(key, "Tadpole"))
        {
            return CompanionRuntimeCombatRole.OffensiveAlly;
        }

        return CompanionRuntimeCombatRole.PassiveCompanion;
    }

    private static CompanionRuntimeTemperament ClassifyRuntimeTemperament(PetRosterEntry entry)
    {
        CompanionRuntimeCombatRole role = ClassifyRuntimeCombatRole(entry);
        if (role == CompanionRuntimeCombatRole.NativePet)
        {
            return CompanionRuntimeTemperament.Steady;
        }

        if (role == CompanionRuntimeCombatRole.UndeadAlly)
        {
            return CompanionRuntimeTemperament.Bound;
        }

        string key = GetRuntimeProfileKey(entry);
        if (ContainsRuntimeProfileTerm(key, "Wolf"))
        {
            return CompanionRuntimeTemperament.Pack;
        }

        if (ContainsRuntimeProfileTerm(key, "Deer")
            || ContainsRuntimeProfileTerm(key, "Pig")
            || ContainsRuntimeProfileTerm(key, "Cow"))
        {
            return CompanionRuntimeTemperament.Skittish;
        }

        if (ContainsRuntimeProfileTerm(key, "Bear")
            || ContainsRuntimeProfileTerm(key, "Bullrat")
            || ContainsRuntimeProfileTerm(key, "Ogre")
            || ContainsRuntimeProfileTerm(key, "Sharg"))
        {
            return CompanionRuntimeTemperament.Fierce;
        }

        return role == CompanionRuntimeCombatRole.OffensiveAlly
            ? CompanionRuntimeTemperament.Volatile
            : CompanionRuntimeTemperament.Steady;
    }

    private static int ComputeRuntimeProfileSeed(PetRosterEntry entry)
    {
        unchecked
        {
            int seed = 17;
            string key = entry.Guid + "|" + entry.ReviewId + "|" + entry.TemplateName;
            for (int i = 0; i < key.Length; i++)
            {
                seed = (seed * 31) + key[i];
            }

            return Math.Abs(seed % 10000);
        }
    }

    private static string GetRuntimeProfileKey(PetRosterEntry entry)
    {
        return $"{entry.DisplayName} {entry.TemplateName} {entry.ReviewId} {entry.BlockReason}";
    }

    private static bool ContainsRuntimeProfileTerm(string value, string term)
    {
        return value.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static CompanionTamingProfile BuildCompanionTamingProfile(PetRosterEntry entry)
    {
        CompanionRuntimeCombatRole role = ClassifyRuntimeCombatRole(entry);
        CompanionRuntimeTemperament temperament = ClassifyRuntimeTemperament(entry);
        CompanionTamingEligibility eligibility;
        string reason;

        if (!entry.RuntimeApproved)
        {
            eligibility = CompanionTamingEligibility.EvidenceInsufficient;
            reason = "roster entry is not runtime approved";
        }
        else if (entry.RequiresPetComponent)
        {
            eligibility = CompanionTamingEligibility.AlreadyCompanion;
            reason = "reviewed native pet companion identity; taming is not needed";
        }
        else if (role == CompanionRuntimeCombatRole.UndeadAlly)
        {
            eligibility = CompanionTamingEligibility.UndeadBlocked;
            reason = "undead roster identity remains blocked from taming";
        }
        else if (role == CompanionRuntimeCombatRole.OffensiveAlly)
        {
            eligibility = CompanionTamingEligibility.UnsafeHostile;
            reason = "hostile/offensive creature identity requires separate capture, safety, and AI gates";
        }
        else if (IsDirectAnimalTamingCandidate(entry))
        {
            eligibility = CompanionTamingEligibility.TameableCandidate;
            reason = "reviewed animal roster identity with plausible companion-lane taming fantasy; read-only only";
        }
        else if (IsPassiveOnlyTamingCandidate(entry))
        {
            eligibility = CompanionTamingEligibility.PassiveOnly;
            reason = "passive animal identity may support handling/bonding but combat taming is not approved";
        }
        else
        {
            eligibility = CompanionTamingEligibility.EvidenceInsufficient;
            reason = "roster identity lacks enough taming-specific evidence";
        }

        return new CompanionTamingProfile(
            eligibility,
            reason,
            entry.Guid,
            entry.TemplateName,
            entry.DisplayName,
            entry.ReviewId,
            entry.RequiresPetComponent,
            entry.RuntimeApproved,
            role,
            temperament);
    }

    private static bool IsDirectAnimalTamingCandidate(PetRosterEntry entry)
    {
        string key = GetRuntimeProfileKey(entry);
        return ContainsRuntimeProfileTerm(key, "Wolf")
            || (ContainsRuntimeProfileTerm(key, "Bear")
                && !ContainsRuntimeProfileTerm(key, "Interactions"));
    }

    private static bool IsPassiveOnlyTamingCandidate(PetRosterEntry entry)
    {
        string key = GetRuntimeProfileKey(entry);
        return ContainsRuntimeProfileTerm(key, "Deer")
            || ContainsRuntimeProfileTerm(key, "Pig")
            || ContainsRuntimeProfileTerm(key, "Cow")
            || ContainsRuntimeProfileTerm(key, "Bear_Interactions")
            || ContainsRuntimeProfileTerm(key, "Bear Interaction")
            || ContainsRuntimeProfileTerm(key, "PassiveAnimal");
    }

    private static string GetTamingEligibilityLabel(CompanionTamingEligibility eligibility)
    {
        return eligibility switch
        {
            CompanionTamingEligibility.TameableCandidate => "tameable candidate",
            CompanionTamingEligibility.AlreadyCompanion => "already companion",
            CompanionTamingEligibility.UnsafeHostile => "unsafe hostile",
            CompanionTamingEligibility.UndeadBlocked => "undead/blocked",
            CompanionTamingEligibility.PassiveOnly => "passive-only",
            _ => "evidence insufficient",
        };
    }

    private static string FormatTamingEligibilityCounts(IReadOnlyCollection<CompanionTamingProfile> profiles)
    {
        List<string> counts = new List<string>();
        foreach (CompanionTamingEligibility eligibility in Enum.GetValues(typeof(CompanionTamingEligibility)).Cast<CompanionTamingEligibility>())
        {
            counts.Add($"{eligibility}={profiles.Count(profile => profile.Eligibility == eligibility)}");
        }

        return string.Join("|", counts);
    }

    private static void LoadContinuityStateOnce()
    {
        if (_continuityStateLoaded)
        {
            return;
        }

        _continuityStateLoaded = true;
        _hasRememberedContinuityMetadata =
            !string.IsNullOrWhiteSpace(Plugin.RememberedCompanionGuid.Value)
            || !string.IsNullOrWhiteSpace(Plugin.RememberedCompanionName.Value);
        _companionMode = ParseCompanionMode(Plugin.RememberedCommandMode.Value);
        _creatureCandidateFollow = ShouldFollowForMode(_companionMode);

        int rememberedIndex = IndexOfRosterEntry(Plugin.RememberedCompanionGuid.Value);
        if (rememberedIndex >= 0)
        {
            Plugin.SelectedPetIndex.Value = rememberedIndex;
        }

        RememberContinuityMetadata(markAvailable: _hasRememberedContinuityMetadata);
    }

    private static CompanionMode ParseCompanionMode(string value)
    {
        if (string.Equals(value, "Stay", StringComparison.OrdinalIgnoreCase))
        {
            return CompanionMode.Stay;
        }

        if (string.Equals(value, "Defend", StringComparison.OrdinalIgnoreCase))
        {
            return CompanionMode.Defend;
        }

        return CompanionMode.Follow;
    }

    private static void RememberContinuityMetadata(bool markAvailable = true)
    {
        Plugin.RememberedCommandMode.Value = GetModeLabel(_companionMode);
        Plugin.RememberedFollowRange.Value = GetFollowRangeLabel(GetFollowRangeProfile());
        if (markAvailable)
        {
            PetRosterEntry selected = GetSelectedPet();
            Plugin.RememberedCompanionGuid.Value = selected.Guid;
            Plugin.RememberedCompanionName.Value = selected.DisplayName;
            _hasRememberedContinuityMetadata = true;
        }
    }

    private static bool TryGetRememberedContinuityEntry(out PetRosterEntry entry)
    {
        if (!_hasRememberedContinuityMetadata)
        {
            entry = default;
            return false;
        }

        int rememberedIndex = IndexOfRosterEntry(Plugin.RememberedCompanionGuid.Value);
        if (rememberedIndex >= 0)
        {
            entry = Roster[rememberedIndex];
            return true;
        }

        entry = default;
        return false;
    }

    private static void ReportRememberedContinuityStatus(ManualLogSource logger)
    {
        if (!TryGetRememberedContinuityEntry(out PetRosterEntry entry))
        {
            return;
        }

        if (GetPetLocations().Any(IsRosterPet))
        {
            _lastContinuityStatusLogKey = string.Empty;
            return;
        }

        string mode = GetModeLabel(_companionMode);
        string range = GetFollowRangeLabel(GetFollowRangeProfile());
        string key = $"{entry.Guid}|{mode}|{range}";
        if (string.Equals(_lastContinuityStatusLogKey, key, StringComparison.Ordinal))
        {
            return;
        }

        _lastContinuityStatusLogKey = key;
        string reason =
            $"remembered={entry.DisplayName}[{entry.Guid}]; mode={mode}; range={range}; "
            + "no active managed actor; manual Summon/Swap required; no actor restored, respawned, saved, or re-adopted";
        SetStatus(logger, $"Remembered {entry.DisplayName}; use Summon / Swap.");
        WriteCompanionCommandLog(logger, "continuity-status", affectedCount: 0, blocked: false, reason: reason);
        TouchCoreRuntimeProfile(
            logger,
            entry,
            "continuity-status",
            reason + "; runtime profile remains metadata-only",
            CompanionRuntimeProfileState.Known,
            affectedCount: 0);
    }

    internal static void OpenNativeCompanionCommand(Location location)
    {
        ManualLogSource? logger = _logger;
        if (!IsNativeCompanionCommandAvailable(location, Hero.Current, location))
        {
            SetStatus(logger, "This companion cannot take orders right now.", warning: true);
            return;
        }

        SelectLocationRosterEntry(location);

        QueueResponsiveNativeRefresh();
        if (_panelVisible)
        {
            SetPanelVisible(false, logger: null);
        }

        SetDialogueVisible(true, logger, location);
        if (TryGetRosterEntry(location, out PetRosterEntry entry))
        {
            string companionName = GetDialogueCompanionName(entry);
            SetStatus(logger, $"Companion menu opened: {companionName}");
            if (logger != null)
            {
                WriteCommandPolicyAudit(
                    logger,
                    entry,
                    EvaluateCommandPolicy(entry, CompanionCommandPolicyAction.DialogueOpen),
                    "dialogue-open",
                    "native-companion-prompt",
                    activeRosterCount: 1);
            }

            TouchCoreRuntimeProfile(
                logger,
                entry,
                "dialogue-open",
                "native Companion prompt opened plugin-owned dialogue surface",
                CompanionRuntimeProfileState.Active,
                affectedCount: 1);
        }
        else
        {
            SetStatus(logger, "Companion menu opened");
        }
    }

    internal static void RunNativeCompanionQuickCommand(Location location, NativeCompanionCommand command)
    {
        ManualLogSource? logger = _logger;
        if (logger == null || !IsNativeCompanionQuickCommandAvailable(location, Hero.Current, location, command))
        {
            if (logger != null && TryGetRosterEntry(location, out PetRosterEntry entry))
            {
                CompanionCommandPolicyDecision decision = EvaluateCommandPolicy(entry, GetPolicyAction(command));
                if (!decision.Allowed)
                {
                    WriteCommandPolicyAudit(logger, entry, decision, command.ToString(), "native-quick-command", activeRosterCount: 1);
                }
            }

            SetStatus(logger, "This companion cannot take that order right now.", warning: true);
            return;
        }

        SelectLocationRosterEntry(location);

        switch (command)
        {
            case NativeCompanionCommand.Follow:
                ApplyCompanionMode(logger, CompanionMode.Follow);
                break;
            case NativeCompanionCommand.Stay:
                ApplyCompanionMode(logger, CompanionMode.Stay);
                break;
            case NativeCompanionCommand.Defend:
                ApplyCompanionMode(logger, CompanionMode.Defend);
                break;
            case NativeCompanionCommand.ComeClose:
                HeelManagedPets(logger);
                break;
            case NativeCompanionCommand.Recall:
                RecallManagedPets(logger);
                break;
            case NativeCompanionCommand.Recover:
                RecoverManagedPets(logger);
                break;
            case NativeCompanionCommand.Dismiss:
                ResetNativeQuickCommandCursor();
                DismissManagedPets(logger, "native companion command");
                return;
        }

        QueueResponsiveNativeRefresh();
        AdvanceNativeQuickCommandCursor(command);
    }

    private static void SelectLocationRosterEntry(Location location)
    {
        if (TryGetRosterEntry(location, out PetRosterEntry entry))
        {
            int index = IndexOfRosterEntry(entry.Guid);
            if (index >= 0)
            {
                Plugin.SelectedPetIndex.Value = index;
                RememberContinuityMetadata();
            }
        }
    }

    private static string GetRuntimeBlockReason()
    {
        if (Plugin.ResearchModeOnly.Value)
        {
            return "Research.ResearchModeOnly=true";
        }

        if (Hero.Current == null)
        {
            return "load a save before using companion commands";
        }

        TemplatesProvider? provider = World.Services?.TryGet<TemplatesProvider>();
        if (provider?.AllLoaded != true)
        {
            return "waiting for templates to finish loading";
        }

        return string.Empty;
    }

    private static void HandlePanelToggle(ManualLogSource logger)
    {
        if (_dialogueVisible && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseDialogueFromCommandSurface(logger, "escape", preserveStatus: false);
            return;
        }

        if (!Plugin.EnableDebugPanel.Value)
        {
            if (_panelVisible)
            {
                SetPanelVisible(false, logger);
            }

            return;
        }

        if (Plugin.TogglePanelShortcut.Value.IsDown())
        {
            if (!_panelVisible && _dialogueVisible)
            {
                CloseDialogueFromCommandSurface(logger, "debug-panel-toggle", preserveStatus: true);
            }

            SetPanelVisible(!_panelVisible, logger);
        }

        if (_panelVisible && Input.GetKeyDown(KeyCode.Escape))
        {
            SetPanelVisible(false, logger);
        }
    }

    private static void HandleUnavailableCommandHotkeys(ManualLogSource logger, string reason)
    {
        if (Plugin.NextPetShortcut.Value.IsDown()
            || Plugin.PreviousPetShortcut.Value.IsDown()
            || Plugin.RecallPetShortcut.Value.IsDown()
            || Plugin.TogglePetFollowShortcut.Value.IsDown()
            || Plugin.DismissPetShortcut.Value.IsDown()
            || Plugin.SpawnQrkoPetShortcut.Value.IsDown())
        {
            SetStatus(logger, GetCommandUnavailableStatus(reason), warning: true);
        }
    }

    private static void SelectRelativePet(ManualLogSource logger, int delta)
    {
        int index = NormalizeIndex(Plugin.SelectedPetIndex.Value + delta);
        Plugin.SelectedPetIndex.Value = index;
        RememberContinuityMetadata();

        PetRosterEntry selected = Roster[index];
        if (selected.RequiresPetComponent)
        {
            SetStatus(logger, $"Selected pet companion: {index + 1}/{Roster.Length} {selected.DisplayName}");
        }
        else
        {
            SetStatus(logger, $"Selected creature candidate: {index + 1}/{Roster.Length} {selected.DisplayName}");
        }

        WriteCompanionCommandLog(logger, delta > 0 ? "select-next" : "select-previous", affectedCount: 0, blocked: false, reason: "selected roster entry; continuity metadata updated; no actor restored");
        WriteCommandPolicyAudit(
            logger,
            selected,
            EvaluateCommandPolicy(selected, CompanionCommandPolicyAction.Select),
            delta > 0 ? "select-next" : "select-previous",
            "selection-command",
            activeRosterCount: 0);
        TouchCoreRuntimeProfile(
            logger,
            selected,
            delta > 0 ? "select-next" : "select-previous",
            "selected roster entry; runtime profile updated in memory only",
            CompanionRuntimeProfileState.Selected,
            affectedCount: 0);

        if (GetPetLocations().Any(IsRosterPet))
        {
            SummonSelectedPet(logger);
        }
    }

    private static void SummonSelectedPet(ManualLogSource logger)
    {
        try
        {
            PetRosterEntry selected = GetSelectedPet();
            RememberContinuityMetadata();
            if (!selected.RuntimeApproved)
            {
                BlockCandidateCommand(logger, selected, "summon/swap");
                return;
            }

            if (!TryAuthorizeCommandPolicy(
                    logger,
                    CompanionCommandPolicyAction.SummonSwap,
                    "summon/swap",
                    new List<Location>(),
                    "summon-command",
                    out _))
            {
                return;
            }

            List<Location> petLocations = GetPetLocations();
            List<Location> nonRosterPets = petLocations.Where(location => !IsRosterPet(location)).ToList();
            if (nonRosterPets.Count > 0)
            {
                SetStatus(logger, $"Summon skipped: {nonRosterPets.Count} non-roster pet already exists.", warning: true);
                WriteCompanionCommandLog(logger, "summon/swap", affectedCount: 0, blocked: true, reason: "non-roster pet already exists");
                return;
            }

            List<Location> rosterPets = petLocations.Where(IsRosterPet).ToList();
            Location? activeSelected = rosterPets.FirstOrDefault(location => IsTemplate(location, selected.Guid));
            if (activeSelected != null)
            {
                RecallLocation(activeSelected, GetPlacementPosition(Hero.Current, selected, 0), Hero.Current.Rotation, ShouldFollowForMode(_companionMode));
                QueueResponsiveNativeRefresh();
                SetStatus(logger, $"Recalled {selected.DisplayName}");
                WriteCompanionCommandLog(logger, "summon-existing-recall", affectedCount: 1, blocked: false, reason: "selected companion already active; recalled instead of spawning");
                TouchCoreRuntimeProfile(
                    logger,
                    selected,
                    "summon-existing-recall",
                    "selected companion already active; recalled instead of spawning",
                    CompanionRuntimeProfileState.Active,
                    affectedCount: 1);
                RunCompanionLifecycleSafetyPass(logger, "summon-existing-recall", forceDump: true);
                return;
            }

            if (rosterPets.Count > 0)
            {
                DismissManagedPets(logger, "pet swap");
            }

            SpawnPet(logger, selected);
        }
        catch (Exception ex)
        {
            SetStatus(logger, $"Pet companion summon failed: {ex.GetType().Name}", warning: true);
            logger.LogWarning($"Pet companion summon failed: {ex.GetType().Name}: {ex.Message}");
        }
    }

    private static void SpawnPet(ManualLogSource logger, PetRosterEntry selected)
    {
        if (!selected.RuntimeApproved)
        {
            BlockCandidateCommand(logger, selected, "spawn");
            return;
        }

        LocationTemplate template = new TemplateReference(selected.Guid).Get<LocationTemplate>();
        if (template == null)
        {
            SetStatus(logger, $"Spawn skipped: {selected.DisplayName} template missing.", warning: true);
            WriteCompanionCommandLog(logger, "summon/swap", affectedCount: 0, blocked: true, reason: "template missing");
            return;
        }

        Hero hero = Hero.Current;
        Vector3 spawnPosition = GetPlacementPosition(hero, selected, 0);
        Location location = template.SpawnLocation(spawnPosition, hero.Rotation);
        location.MarkedNotSaved = true;

        bool hasPetElement = location.TryGetElement(out PetElement petElement);
        bool hasPetVariant = location.TryGetElement(out PetVariantBase petVariant);
        if (!hasPetElement && !hasPetVariant)
        {
            if (selected.RequiresPetComponent)
            {
                SetStatus(logger, $"Spawn failed: {selected.DisplayName} has no pet component.", warning: true);
                logger.LogWarning($"Pet companion spawned {template.name}, but it had no PetElement or PetVariantBase. Discarding it.");
                location.Discard();
                WriteCompanionCommandLog(logger, "summon/swap", affectedCount: 0, blocked: true, reason: "spawned pet had no pet component");
                return;
            }

            if (!TryMakeCreatureCandidateHeroAlly(location, selected, logger))
            {
                location.MarkedNotSaved = true;
                location.Discard();
                SetStatus(logger, $"Spawn failed: {selected.DisplayName} could not be made ally.", warning: true);
                WriteCompanionCommandLog(logger, "summon/swap", affectedCount: 0, blocked: true, reason: "native ally setup failed");
                return;
            }

            AddCreatureCandidateLocation(location, logger);
            _creatureCandidateFollow = ShouldFollowForMode(_companionMode);
            QueueResponsiveNativeRefresh();
            ResetNativeQuickCommandCursor();
            SetStatus(logger, $"Summoned allied {selected.DisplayName}");
            logger.LogInfo(
                $"Creature candidate spawned {selected.DisplayName}/{template.name}[{template.GUID}] at {spawnPosition}. "
                + $"Review={selected.ReviewId}; Location marked not saved; session tracking enabled; native NpcHeroPetAlly ownership applied; Mode={GetModeLabel(_companionMode)}; "
                + "follow uses catch-up recall only and defend uses native ally combat entry only when the hero has live attackers. "
                + "No custom targeting, quest, or save-persistence state was changed.");
            WriteCompanionCommandLog(logger, "summon/swap", affectedCount: 1, blocked: false, reason: "spawned one-session native pet ally candidate");
            TouchCoreRuntimeProfile(
                logger,
                selected,
                "summon/swap",
                "spawned one-session native pet ally candidate; runtime profile active",
                CompanionRuntimeProfileState.Active,
                affectedCount: 1);
            RunCompanionLifecycleSafetyPass(logger, "summon", forceDump: true);
            return;
        }

        ApplyModeToLocation(location, _companionMode);
        QueueResponsiveNativeRefresh();
        ResetNativeQuickCommandCursor();

        SetStatus(logger, $"Summoned {selected.DisplayName}");
        logger.LogInfo($"Pet companion spawned {selected.DisplayName}/{template.name}[{template.GUID}] at {spawnPosition}. Location marked not saved.");
        WriteCompanionCommandLog(logger, "summon/swap", affectedCount: 1, blocked: false, reason: "spawned managed pet companion");
        TouchCoreRuntimeProfile(
            logger,
            selected,
            "summon/swap",
            "spawned managed pet companion; runtime profile active",
            CompanionRuntimeProfileState.Active,
            affectedCount: 1);
        RunCompanionLifecycleSafetyPass(logger, "summon", forceDump: true);
    }

    private static bool TryMakeCreatureCandidateHeroAlly(Location location, PetRosterEntry selected, ManualLogSource logger)
    {
        try
        {
            Hero hero = Hero.Current;
            if (hero == null)
            {
                logger.LogWarning($"Creature candidate ally setup skipped for {selected.DisplayName}: Hero.Current is null.");
                return false;
            }

            if (!location.TryGetElement(out NpcElement npcElement) || npcElement == null)
            {
                logger.LogWarning(
                    $"Creature candidate ally setup skipped for {selected.DisplayName}/{location.Template?.name}[{location.Template?.GUID}]: spawned location has no NpcElement.");
                return false;
            }

            NpcElement npc = npcElement;
            if (!npc.HasElement<NpcHeroPetAlly>())
            {
                npc.OverrideFaction(hero.GetFactionTemplateForSummon(), FactionOverrideContext.Summon);
                npc.AddElement(new NpcHeroPetAlly(hero));
            }

            location.MarkedNotSaved = true;
            logger.LogInfo(
                $"Creature candidate ally setup applied for {selected.DisplayName}/{location.Template?.name}[{location.Template?.GUID}]. "
                + "Mirrors native NpcAllyPetVariant summon-faction plus NpcHeroPetAlly path.");
            return true;
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                $"Creature candidate ally setup failed for {selected.DisplayName}/{location.Template?.name}[{location.Template?.GUID}]: "
                + $"{ex.GetType().Name}: {ex.Message}");
            return false;
        }
    }

    private static void RecallManagedPets(ManualLogSource logger)
    {
        List<Location> managedPets = GetPetLocations().Where(IsRosterPet).ToList();
        if (managedPets.Count == 0)
        {
            SetStatus(logger, "No companion to recall.");
            WriteCompanionCommandLog(logger, "recall", affectedCount: 0, blocked: true, reason: "no active roster companion");
            return;
        }

        if (!TryAuthorizeCommandPolicy(
                logger,
                CompanionCommandPolicyAction.Recall,
                "recall",
                managedPets,
                "recall-command",
                out _))
        {
            return;
        }

        Hero hero = Hero.Current;
        for (int i = 0; i < managedPets.Count; i++)
        {
            PetRosterEntry entry = GetRosterEntry(managedPets[i]);
            RecallLocation(managedPets[i], GetPlacementPosition(hero, entry, i), hero.Rotation, ShouldFollowForMode(_companionMode));
        }

        SetStatus(logger, $"Recalled {FormatCompanionCount(managedPets.Count)}.");
        WriteCompanionCommandLog(logger, "recall", affectedCount: managedPets.Count, blocked: false, reason: "recalled active roster companions");
        TouchCoreRuntimeProfiles(
            logger,
            managedPets,
            "recall",
            "recalled active roster companions; runtime profile active",
            CompanionRuntimeProfileState.Active);
        QueueResponsiveNativeRefresh();
        RunCompanionLifecycleSafetyPass(logger, "recall", forceDump: true);
    }

    private static void HeelManagedPets(ManualLogSource logger)
    {
        List<Location> managedPets = GetPetLocations().Where(IsRosterPet).ToList();
        if (managedPets.Count == 0)
        {
            SetStatus(logger, "No companion to call close.");
            WriteCompanionCommandLog(logger, "heel", affectedCount: 0, blocked: true, reason: "no active roster companion");
            return;
        }

        if (!TryAuthorizeCommandPolicy(
                logger,
                CompanionCommandPolicyAction.ComeClose,
                "heel",
                managedPets,
                "heel-command",
                out _))
        {
            return;
        }

        SetFollowRange(logger, FollowRangeProfile.Close, recallActive: false);
        _companionMode = CompanionMode.Follow;
        _creatureCandidateFollow = true;
        RememberContinuityMetadata();

        Hero hero = Hero.Current;
        for (int i = 0; i < managedPets.Count; i++)
        {
            PetRosterEntry entry = GetRosterEntry(managedPets[i]);
            RecallLocation(managedPets[i], GetPlacementPosition(hero, entry, i), hero.Rotation, followAfterRecall: true);
            ApplyModeToLocation(managedPets[i], CompanionMode.Follow);
        }

        SetStatus(logger, $"Come Close: {FormatCompanionCount(managedPets.Count)} called to you.");
        WriteCompanionCommandLog(logger, "heel", affectedCount: managedPets.Count, blocked: false, reason: "set close range, follow mode, and recalled active roster companions");
        TouchCoreRuntimeProfiles(
            logger,
            managedPets,
            "heel",
            "set close range, follow mode, and recalled active roster companions; runtime profile active",
            CompanionRuntimeProfileState.Active);
        QueueResponsiveNativeRefresh();
        RunCompanionLifecycleSafetyPass(logger, "heel", forceDump: true);
    }

    private static void RecallLocation(Location location, Vector3 coords, Quaternion rotation, bool followAfterRecall)
    {
        location.MarkedNotSaved = true;

        if (location.TryGetElement(out PetElement petElement))
        {
            petElement.SetFollowing(followAfterRecall);
            petElement.Recall(coords);
        }
        else
        {
            location.MoveAndRotateTo(coords, rotation, teleport: true);
        }

        if (location.TryGetElement(out PetVariantBase petVariant))
        {
            petVariant.SetFollowing(followAfterRecall);
        }
    }

    private static void ToggleFollowStayMode(ManualLogSource logger)
    {
        ApplyCompanionMode(logger, _companionMode == CompanionMode.Follow ? CompanionMode.Stay : CompanionMode.Follow);
    }

    private static void ApplyCompanionMode(ManualLogSource logger, CompanionMode mode)
    {
        List<Location> managedPets = GetPetLocations().Where(IsRosterPet).ToList();
        if (!TryAuthorizeCommandPolicy(
                logger,
                GetPolicyAction(mode),
                "mode-" + GetModeLabel(mode).ToLowerInvariant(),
                managedPets,
                "mode-command",
                out _))
        {
            return;
        }

        _companionMode = mode;
        _creatureCandidateFollow = ShouldFollowForMode(mode);
        RememberContinuityMetadata();

        foreach (Location location in managedPets)
        {
            ApplyModeToLocation(location, mode);
        }

        int attackerCount = mode == CompanionMode.Defend ? CountLiveHeroAttackers(Hero.Current) : 0;
        DefendTickResult defendResult = mode == CompanionMode.Defend
            ? TriggerCreatureCandidateDefend(attackerCount, forcePrompt: true, logger)
            : DefendTickResult.Empty(attackerCount);
        string message = GetCompanionModeStatus(mode, managedPets.Count, attackerCount, defendResult);

        SetStatus(logger, message);
        WriteCompanionCommandLog(logger, "mode-" + GetModeLabel(mode).ToLowerInvariant(), affectedCount: managedPets.Count, blocked: false, reason: message);
        TouchCoreRuntimeProfiles(
            logger,
            managedPets,
            "mode-" + GetModeLabel(mode).ToLowerInvariant(),
            message + "; runtime profile mode updated",
            CompanionRuntimeProfileState.Active);
        if (managedPets.Count == 0)
        {
            TouchCoreRuntimeProfile(
                logger,
                GetSelectedPet(),
                "mode-" + GetModeLabel(mode).ToLowerInvariant(),
                message + "; selected runtime profile mode updated; no active actor",
                CompanionRuntimeProfileState.Selected,
                affectedCount: 0);
        }

        QueueResponsiveNativeRefresh();
    }

    private static void ApplyModeToLocation(Location location, CompanionMode mode)
    {
        location.MarkedNotSaved = true;
        bool follow = ShouldFollowForMode(mode);

        if (location.TryGetElement(out PetElement petElement))
        {
            petElement.SetFollowing(follow);
        }

        if (location.TryGetElement(out PetVariantBase petVariant))
        {
            petVariant.SetFollowing(follow);
        }
    }

    private static bool ShouldFollowForMode(CompanionMode mode)
    {
        return mode != CompanionMode.Stay;
    }

    private static void SetFollowRange(ManualLogSource logger, FollowRangeProfile range, bool recallActive)
    {
        FollowRangeProfile normalized = NormalizeFollowRange(range);
        Plugin.FollowRangeProfile.Value = (int)normalized;
        RememberContinuityMetadata();

        List<Location> managedPets = GetPetLocations().Where(IsRosterPet).ToList();
        if (!TryAuthorizeCommandPolicy(
                logger,
                CompanionCommandPolicyAction.Range,
                "range-" + GetFollowRangeLabel(normalized).ToLowerInvariant(),
                managedPets,
                "range-command",
                out _))
        {
            return;
        }

        if (recallActive && managedPets.Count > 0 && ShouldFollowForMode(_companionMode))
        {
            Hero hero = Hero.Current;
            for (int i = 0; i < managedPets.Count; i++)
            {
                PetRosterEntry entry = GetRosterEntry(managedPets[i]);
                RecallLocation(managedPets[i], GetPlacementPosition(hero, entry, i), hero.Rotation, followAfterRecall: true);
            }

            RunCompanionLifecycleSafetyPass(logger, "range-" + GetFollowRangeLabel(normalized).ToLowerInvariant(), forceDump: true);
        }

        string message = $"Distance: {GetPlayerRangeLabel(normalized)}.";
        if (managedPets.Count > 0 && recallActive && ShouldFollowForMode(_companionMode))
        {
            message += $" {FormatCompanionCount(managedPets.Count)} repositioned.";
        }

        SetStatus(logger, message);
        WriteCompanionCommandLog(logger, "range-" + GetFollowRangeLabel(normalized).ToLowerInvariant(), affectedCount: recallActive ? managedPets.Count : 0, blocked: false, reason: message);
        TouchCoreRuntimeProfiles(
            logger,
            managedPets,
            "range-" + GetFollowRangeLabel(normalized).ToLowerInvariant(),
            message + "; runtime profile range updated",
            managedPets.Count > 0 ? CompanionRuntimeProfileState.Active : CompanionRuntimeProfileState.Selected);
        if (managedPets.Count == 0)
        {
            TouchCoreRuntimeProfile(
                logger,
                GetSelectedPet(),
                "range-" + GetFollowRangeLabel(normalized).ToLowerInvariant(),
                message + "; selected runtime profile range updated; no active actor",
                CompanionRuntimeProfileState.Selected,
                affectedCount: 0);
        }

        QueueResponsiveNativeRefresh();
    }

    private static FollowRangeProfile GetFollowRangeProfile()
    {
        FollowRangeProfile normalized = NormalizeFollowRange((FollowRangeProfile)Plugin.FollowRangeProfile.Value);
        if (Plugin.FollowRangeProfile.Value != (int)normalized)
        {
            Plugin.FollowRangeProfile.Value = (int)normalized;
        }

        return normalized;
    }

    private static FollowRangeProfile NormalizeFollowRange(FollowRangeProfile range)
    {
        return range switch
        {
            FollowRangeProfile.Close => FollowRangeProfile.Close,
            FollowRangeProfile.Far => FollowRangeProfile.Far,
            _ => FollowRangeProfile.Normal,
        };
    }

    private static float GetFollowPlacementScale(PetRosterEntry entry)
    {
        return GetFollowRangeProfile() switch
        {
            FollowRangeProfile.Close => entry.RequiresPetComponent ? CloseFollowPlacementScale : NormalFollowPlacementScale,
            FollowRangeProfile.Far => FarFollowPlacementScale,
            _ => NormalFollowPlacementScale,
        };
    }

    private static float GetCreatureCatchUpDistance()
    {
        return GetCreatureCatchUpDistance(GetSelectedPet());
    }

    private static float GetCreatureCatchUpDistance(PetRosterEntry entry)
    {
        float baseDistance = GetFollowRangeProfile() switch
        {
            FollowRangeProfile.Close => CloseCreatureCatchUpDistance,
            FollowRangeProfile.Far => FarCreatureCatchUpDistance,
            _ => NormalCreatureCatchUpDistance,
        };

        return GetCompanionBondPolicy(entry).ApplyCatchUpDistance(baseDistance);
    }

    private static string GetFollowRangeLabel(FollowRangeProfile range)
    {
        return NormalizeFollowRange(range) switch
        {
            FollowRangeProfile.Close => "Close",
            FollowRangeProfile.Far => "Far",
            _ => "Normal",
        };
    }

    private static string GetModeLabel(CompanionMode mode)
    {
        return mode switch
        {
            CompanionMode.Follow => "Follow",
            CompanionMode.Stay => "Stay",
            CompanionMode.Defend => "Defend",
            _ => "Follow"
        };
    }

    private static string GetPlayerModeLabel(CompanionMode mode)
    {
        return mode switch
        {
            CompanionMode.Follow => "Follow",
            CompanionMode.Stay => "Hold Position",
            CompanionMode.Defend => "Defend",
            _ => "Follow"
        };
    }

    private static string GetPlayerRangeLabel(FollowRangeProfile range)
    {
        return NormalizeFollowRange(range) switch
        {
            FollowRangeProfile.Close => "Close",
            FollowRangeProfile.Far => "Far",
            _ => "Normal",
        };
    }

    private static string FormatCompanionCount(int count)
    {
        return count == 1 ? "1 companion" : $"{count} companions";
    }

    private static string FormatThreatCount(int count)
    {
        return count == 1 ? "1 threat" : $"{count} threats";
    }

    private static string GetCommandUnavailableStatus(string reason)
    {
        if (reason.IndexOf("Research.ResearchModeOnly", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return "Companion orders are disabled in the mod config.";
        }

        if (reason.IndexOf("load a save", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return "Load a save before giving orders.";
        }

        if (reason.IndexOf("templates", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return "Companion orders are still loading.";
        }

        return "Companion orders are unavailable right now.";
    }

    private static string GetCompanionModeStatus(
        CompanionMode mode,
        int activeCount,
        int attackerCount,
        DefendTickResult defendResult)
    {
        string label = GetPlayerModeLabel(mode);
        if (activeCount == 0)
        {
            return $"{label}: no active companion.";
        }

        return mode switch
        {
            CompanionMode.Stay => $"Hold Position: {FormatCompanionCount(activeCount)} holding.",
            CompanionMode.Defend => GetDefendModeStatus(activeCount, attackerCount, defendResult),
            _ => $"Follow: {FormatCompanionCount(activeCount)} following. Distance: {GetPlayerRangeLabel(GetFollowRangeProfile())}.",
        };
    }

    private static string GetDefendModeStatus(int activeCount, int attackerCount, DefendTickResult defendResult)
    {
        if (defendResult.Prompted > 0)
        {
            return $"Defend: {FormatCompanionCount(defendResult.Prompted)} engaging {FormatThreatCount(attackerCount)}.";
        }

        if (attackerCount > 0)
        {
            return $"Defend: {FormatCompanionCount(activeCount)} guarding you. Threats nearby: {attackerCount}.";
        }

        return $"Defend: {FormatCompanionCount(activeCount)} guarding you.";
    }

    private static string GetModeFeedbackLine(bool runtimeReady)
    {
        if (!runtimeReady)
        {
            return "Companion orders are unavailable.";
        }

        int activeCount = GetPetLocations().Count(IsRosterPet);
        int attackerCount = CountLiveHeroAttackers(Hero.Current);
        return _companionMode switch
        {
            CompanionMode.Stay => activeCount > 0
                ? "Hold Position - companion will stay put."
                : "Hold Position - no active companion.",
            CompanionMode.Defend => attackerCount > 0
                ? $"Defend - {FormatThreatCount(attackerCount)} nearby."
                : "Defend - waiting for a threat.",
            _ => activeCount > 0
                ? $"Follow - distance: {GetPlayerRangeLabel(GetFollowRangeProfile())}."
                : "Follow - no active companion.",
        };
    }

    private static List<string> GetActiveCompanionStatusLines(bool runtimeReady)
    {
        List<string> lines = new List<string>();
        if (!runtimeReady)
        {
            lines.Add("Companion status: unavailable.");
            return lines;
        }

        List<Location> activeRoster = GetPetLocations().Where(IsRosterPet).ToList();
        if (activeRoster.Count == 0)
        {
            if (TryGetRememberedContinuityEntry(out PetRosterEntry remembered))
            {
                lines.Add($"Companion status: none active; remembered {GetDialogueCompanionName(remembered)}.");
                lines.Add(
                    $"Last order: {GetPlayerModeLabel(_companionMode)} / {GetPlayerRangeLabel(GetFollowRangeProfile())}; "
                    + "Summon / Swap creates a new one-session companion.");
            }
            else
            {
                lines.Add("Companion status: none active.");
            }

            return lines;
        }

        Hero hero = Hero.Current;
        int attackerCount = CountLiveHeroAttackers(hero);
        float now = Time.realtimeSinceStartup;
        for (int i = 0; i < activeRoster.Count && i < 3; i++)
        {
            lines.Add(BuildCompanionStatusLine(activeRoster[i], i, hero, attackerCount, now));
        }

        if (activeRoster.Count > 3)
        {
            lines.Add($"+ {activeRoster.Count - 3} extra tracked actor(s); lifecycle guard should trim duplicates.");
        }

        return lines;
    }

    private static string BuildCompanionStatusLine(Location location, int slot, Hero? hero, int attackerCount, float now)
    {
        PetRosterEntry entry = GetRosterEntry(location);
        string name = string.IsNullOrWhiteSpace(entry.DisplayName) ? location.Template?.name ?? "Unknown" : entry.DisplayName;
        string distance = hero == null ? "distance n/a" : $"{Vector3.Distance(location.Coords, hero.Coords):0.#}m";
        string mode = GetCompanionModeStatus(location, attackerCount, now);
        string ally = HasNativeHeroPetAlly(location) ? "ally on" : entry.RequiresPetComponent ? "pet component" : "ally missing";
        string prompt = location.HasElement<AvalonCompanionCommandAction>() ? "dialogue prompt on" : entry.RequiresPetComponent ? "prompt n/a" : "prompt off";
        string save = location.MarkedNotSaved ? "not saved" : "save guard pending";
        return $"{slot + 1}. {name}: {distance}; {mode}; {ally}; {prompt}; {save}.";
    }

    private static string GetCompanionModeStatus(Location location, int attackerCount, float now)
    {
        switch (_companionMode)
        {
            case CompanionMode.Stay:
                return "Stay";
            case CompanionMode.Defend:
                if (attackerCount <= 0)
                {
                    return "Defend waiting";
                }

                if (!HasNativeHeroPetAlly(location))
                {
                    return $"Defend armed; attackers={attackerCount}; no native ally marker";
                }

                if (NextCreatureCandidateDefendPromptAt.TryGetValue(location, out float nextPromptAt) && now < nextPromptAt)
                {
                    return $"Defend cooldown {nextPromptAt - now:0.#}s; attackers={attackerCount}";
                }

                return $"Defend ready; attackers={attackerCount}";
            default:
                return $"Follow {GetFollowRangeLabel(GetFollowRangeProfile())}";
        }
    }

    private static void DismissManagedPets(ManualLogSource logger, string reason)
    {
        List<Location> managedPets = GetPetLocations().Where(IsRosterPet).ToList();
        if (managedPets.Count == 0)
        {
            SetStatus(logger, "No companion to dismiss.");
            WriteCompanionCommandLog(logger, "dismiss", affectedCount: 0, blocked: true, reason: "no active roster companion");
            return;
        }

        if (!TryAuthorizeCommandPolicy(
                logger,
                CompanionCommandPolicyAction.Dismiss,
                "dismiss",
                managedPets,
                "dismiss-command",
                out _))
        {
            return;
        }

        TouchCoreRuntimeProfiles(
            logger,
            managedPets,
            "dismiss",
            reason + "; active runtime profiles dismissed",
            CompanionRuntimeProfileState.Dismissed);

        foreach (Location location in managedPets)
        {
            RemoveNativeCommandAction(location);
            NextCreatureCandidateDefendPromptAt.Remove(location);
            location.MarkedNotSaved = true;
            location.Discard();
        }

        NextCreatureCandidateDefendPromptAt.Clear();
        _lastDefendHadAttackers = false;
        _lastDefendAttackerCount = 0;
        SetStatus(logger, $"Dismissed {FormatCompanionCount(managedPets.Count)}.");
        logger.LogInfo($"Dismissed {managedPets.Count} managed pet companion(s) for {reason}.");
        WriteCompanionCommandLog(logger, "dismiss", affectedCount: managedPets.Count, blocked: false, reason: reason);
        RunCompanionLifecycleSafetyPass(logger, "dismiss", forceDump: true);
    }

    private static void RecoverManagedPets(ManualLogSource logger)
    {
        RunCompanionLifecycleSafetyPass(logger, "recover-precheck", forceDump: true);

        Hero hero = Hero.Current;
        List<Location> managedPets = GetActiveManagedRosterLocations(logger);
        if (managedPets.Count == 0)
        {
            SetStatus(logger, "Recover checked: no active managed companion.");
            WriteCompanionCommandLog(logger, "recover", affectedCount: 0, blocked: true, reason: "no active roster companion");
            return;
        }

        if (!TryAuthorizeCommandPolicy(
                logger,
                CompanionCommandPolicyAction.Recover,
                "recover",
                managedPets,
                "recover-command",
                out _))
        {
            return;
        }

        int inspected = 0;
        int recalled = 0;
        int removed = 0;
        int invalid = 0;
        int dead = 0;
        int guardBlocked = 0;
        for (int i = 0; i < managedPets.Count; i++)
        {
            Location location = managedPets[i];
            if (location == null)
            {
                continue;
            }

            inspected++;
            PetRosterEntry entry = GetRosterEntry(location);
            CompanionRecoveryState state = GetCompanionRecoveryState(location, entry);
            if (state == CompanionRecoveryState.Invalid || state == CompanionRecoveryState.Dead)
            {
                if (state == CompanionRecoveryState.Dead)
                {
                    dead++;
                }
                else
                {
                    invalid++;
                }

                RemoveNativeCommandAction(location);
                CreatureCandidateLocations.Remove(location);
                NextCreatureCandidateDefendPromptAt.Remove(location);
                location.MarkedNotSaved = true;

                if (Plugin.EnableLifecycleSafetyGuard.Value)
                {
                    TouchCoreRuntimeProfile(
                        logger,
                        entry,
                        "recover-remove",
                        $"state={state}; lifecycle guard removed invalid/dead managed companion",
                        CompanionRuntimeProfileState.Removed,
                        affectedCount: 1);
                    location.Discard();
                    removed++;
                }
                else
                {
                    TouchCoreRuntimeProfile(
                        logger,
                        entry,
                        "recover-blocked",
                        $"state={state}; lifecycle guard disabled removal",
                        CompanionRuntimeProfileState.Active,
                        affectedCount: 1);
                    guardBlocked++;
                }

                continue;
            }

            float distance = Vector3.Distance(location.Coords, hero.Coords);
            if (distance >= RecoveryOutOfRangeDistance)
            {
                RecallLocation(location, GetPlacementPosition(hero, entry, i), hero.Rotation, ShouldFollowForMode(_companionMode));
                ApplyModeToLocation(location, _companionMode);
                TouchCoreRuntimeProfile(
                    logger,
                    entry,
                    "recover-recall",
                    $"distance={distance:0.##}; threshold={RecoveryOutOfRangeDistance:0.##}; live managed companion recalled",
                    CompanionRuntimeProfileState.Active,
                    affectedCount: 1);
                recalled++;
            }
        }

        RemoveDiscardedCreatureCandidates();
        RunCompanionLifecycleSafetyPass(logger, "recover", forceDump: true);

        string reason =
            $"inspected={inspected}; recalled={recalled}; removed={removed}; invalid={invalid}; dead={dead}; "
            + $"guardBlocked={guardBlocked}; threshold={RecoveryOutOfRangeDistance:0.##}; native ally path only";
        string status = guardBlocked > 0
            ? $"Recover checked {inspected}; {guardBlocked} removal(s) blocked by lifecycle guard off."
            : recalled > 0 || removed > 0
                ? $"Recover complete: {recalled} recalled, {removed} removed."
                : $"Recover checked {inspected}; no action needed.";

        SetStatus(logger, status, warning: guardBlocked > 0);
        logger.LogInfo($"Companion recovery completed. {reason}. No healing, respawn, targeting, persistence, or squad state was changed.");
        WriteCompanionCommandLog(logger, "recover", affectedCount: recalled + removed, blocked: guardBlocked > 0, reason: reason);
        QueueResponsiveNativeRefresh();
    }

    private static CompanionRecoveryState GetCompanionRecoveryState(Location location, PetRosterEntry entry)
    {
        if (location == null || location.HasBeenDiscarded)
        {
            return CompanionRecoveryState.Invalid;
        }

        bool hasPetElement = location.TryGetElement(out PetElement _);
        bool hasPetVariant = location.TryGetElement(out PetVariantBase _);
        bool hasNpcElement = location.TryGetElement(out NpcElement npcElement) && npcElement != null;

        if (entry.RequiresPetComponent)
        {
            if (!hasPetElement && !hasPetVariant)
            {
                return CompanionRecoveryState.Invalid;
            }

            return TryGetNpcCharacter(location, out ICharacter petCharacter) && !IsLiveCharacter(petCharacter)
                ? CompanionRecoveryState.Dead
                : CompanionRecoveryState.Live;
        }

        if (!hasNpcElement || !HasNativeHeroPetAlly(location))
        {
            return CompanionRecoveryState.Invalid;
        }

        return npcElement is ICharacter character && !IsLiveCharacter(character)
            ? CompanionRecoveryState.Dead
            : CompanionRecoveryState.Live;
    }

    private static bool TryGetNpcCharacter(Location location, out ICharacter character)
    {
        if (location != null
            && !location.HasBeenDiscarded
            && location.TryGetElement(out NpcElement npcElement)
            && npcElement is ICharacter npcCharacter)
        {
            character = npcCharacter;
            return true;
        }

        character = null!;
        return false;
    }

    private static bool IsLiveCharacter(ICharacter character)
    {
        try
        {
            return character != null && !character.WasDiscarded && character.IsAlive;
        }
        catch
        {
            return false;
        }
    }

    private static void TickCreatureCandidateFollow(ManualLogSource logger)
    {
        if (Plugin.EnableProfileDrivenNativeAssist.Value || IsAiRuntimeOwned)
        {
            return;
        }

        if (!_creatureCandidateFollow || Time.realtimeSinceStartup < _nextCreatureCandidateFollowTick)
        {
            return;
        }

        CompanionBondPolicyDecision activeBondPolicy = GetActiveCompanionBondPolicy();
        float followInterval = activeBondPolicy.ApplyCatchUpInterval(CreatureCandidateFollowTickSeconds);
        _nextCreatureCandidateFollowTick = Time.realtimeSinceStartup + followInterval;
        RemoveDiscardedCreatureCandidates();
        if (CreatureCandidateLocations.Count == 0)
        {
            return;
        }

        Hero hero = Hero.Current;
        for (int i = 0; i < CreatureCandidateLocations.Count; i++)
        {
            Location location = CreatureCandidateLocations[i];
            PetRosterEntry entry = GetRosterEntry(location);
            CompanionBondPolicyDecision bondPolicy = GetCompanionBondPolicy(entry);
            float catchUpDistance = GetCreatureCatchUpDistance(entry);
            float distance = Vector3.Distance(location.Coords, hero.Coords);
            if (distance <= catchUpDistance)
            {
                continue;
            }

            RecallLocation(location, GetPlacementPosition(hero, entry, i), hero.Rotation, followAfterRecall: true);
            string reason = $"range={GetFollowRangeLabel(GetFollowRangeProfile())}; distance={distance:0.##}; threshold={catchUpDistance:0.##}; "
                + $"interval={followInterval:0.##}; slot={i}; {bondPolicy.FormatAuditSegment()}";
            logger.LogInfo($"Creature candidate catch-up recalled {entry.DisplayName}/{location.Template?.name}[{location.Template?.GUID}] to the hero. {reason}; Location remains marked not saved.");
            WriteCompanionCommandLog(logger, "auto-catch-up", affectedCount: 1, blocked: false, reason: reason);
            TouchCoreRuntimeProfile(
                logger,
                entry,
                "auto-catch-up",
                reason + "; existing catch-up recall path only",
                CompanionRuntimeProfileState.Active,
                affectedCount: 1);
        }
    }

    private static void TickCreatureCandidateDefend(ManualLogSource logger)
    {
        if (Plugin.EnableProfileDrivenNativeAssist.Value || IsAiRuntimeOwned)
        {
            return;
        }

        bool explicitDefendMode = _companionMode == CompanionMode.Defend;
        if ((!explicitDefendMode && !Plugin.EnableNativeDefendAssist.Value) || Time.realtimeSinceStartup < _nextCreatureCandidateDefendTick)
        {
            return;
        }

        _nextCreatureCandidateDefendTick = Time.realtimeSinceStartup + CreatureCandidateDefendTickSeconds;
        RemoveDiscardedCreatureCandidates();
        if (CreatureCandidateLocations.Count == 0)
        {
            return;
        }

        Hero hero = Hero.Current;
        int attackerCount = CountLiveHeroAttackers(hero);
        CompanionBondPolicyDecision activeBondPolicy = GetActiveCompanionBondPolicy();
        bool canUseAutomaticDefend =
            Plugin.EnableNativeDefendAssist.Value
            && activeBondPolicy.AllowsAutomaticDefendAssist;
        bool canUseNativeDefend = explicitDefendMode || canUseAutomaticDefend;
        if (!canUseNativeDefend)
        {
            if (attackerCount > 0 && Time.realtimeSinceStartup >= _nextCreatureCandidateDefendLog)
            {
                _nextCreatureCandidateDefendLog = Time.realtimeSinceStartup + CreatureCandidateDefendLogSeconds;
                string reason = $"automatic native defend assist blocked by bond policy; attackers={attackerCount}; "
                    + $"{activeBondPolicy.FormatAuditSegment()}; touchesTargeting=false; touchesPersistence=false";
                logger.LogInfo($"Bond policy blocked automatic native defend assist. Attackers={attackerCount}; {activeBondPolicy.Reason}.");
                WriteCompanionCommandLog(logger, "bond-policy-auto-defend", affectedCount: 0, blocked: true, reason: reason);
            }

            return;
        }

        TrackDefendAttackerState(logger, attackerCount, explicitDefendMode);
        if (attackerCount <= 0)
        {
            return;
        }

        DefendTickResult result = TriggerCreatureCandidateDefend(attackerCount, forcePrompt: false, logger);
        if (result.Prompted > 0)
        {
            string source = explicitDefendMode ? "Defend mode" : "Native defend assist";
            float cooldown = activeBondPolicy.ApplyDefendCooldown(CreatureCandidateDefendPromptCooldownSeconds);
            string reason = $"{source}; attackers={attackerCount}; available={result.Available}; cooldown={cooldown:0.##}s; {activeBondPolicy.FormatAuditSegment()}";
            WriteCompanionCommandLog(logger, explicitDefendMode ? "defend-prompt" : "auto-defend-prompt", affectedCount: result.Prompted, blocked: false, reason: reason);

            if (Time.realtimeSinceStartup >= _nextCreatureCandidateDefendLog)
            {
                _nextCreatureCandidateDefendLog = Time.realtimeSinceStartup + CreatureCandidateDefendLogSeconds;
                logger.LogInfo($"{source} prompted {result.Prompted} managed creature candidate(s) to enter native ally combat with the hero's attackers. Attackers={attackerCount}; Available={result.Available}; Cooldown={cooldown:0.##}s.");
            }
        }
    }

    private static void TrackDefendAttackerState(ManualLogSource logger, int attackerCount, bool explicitDefendMode)
    {
        bool hasAttackers = attackerCount > 0;
        if (hasAttackers)
        {
            if (!_lastDefendHadAttackers || _lastDefendAttackerCount != attackerCount)
            {
                string source = explicitDefendMode ? "Defend mode" : "Native defend assist";
                logger.LogInfo($"{source} detected {attackerCount} live hero attacker(s). Native ally combat prompting is armed.");
                WriteCompanionCommandLog(logger, "defend-armed", affectedCount: 0, blocked: false, reason: $"{source}; attackers={attackerCount}; native ally path only");
            }
        }
        else if (_lastDefendHadAttackers)
        {
            NextCreatureCandidateDefendPromptAt.Clear();
            logger.LogInfo("Native defend state cleared because the hero has no live attackers.");
            WriteCompanionCommandLog(logger, "defend-clear", affectedCount: 0, blocked: false, reason: "hero has no live attackers; defend prompt cooldown reset");
        }

        _lastDefendHadAttackers = hasAttackers;
        _lastDefendAttackerCount = attackerCount;
    }

    private static DefendTickResult TriggerCreatureCandidateDefend(int attackerCount, bool forcePrompt, ManualLogSource? logger = null)
    {
        RemoveDiscardedCreatureCandidates();
        if (attackerCount <= 0 || CreatureCandidateLocations.Count == 0)
        {
            return DefendTickResult.Empty(attackerCount);
        }

        float now = Time.realtimeSinceStartup;
        int triggered = 0;
        int available = 0;
        int skippedCooldown = 0;
        foreach (Location location in CreatureCandidateLocations)
        {
            if (location == null || location.HasBeenDiscarded || !IsCreatureCandidate(location))
            {
                continue;
            }

            if (!location.TryGetElement(out NpcElement npcElement) || npcElement == null)
            {
                continue;
            }

            if (!IsLocationCommandPolicyAllowed(location, CompanionCommandPolicyAction.NativeDefendPrompt))
            {
                continue;
            }

            NpcHeroPetAlly petAlly = npcElement.TryGetElement<NpcHeroPetAlly>();
            if (petAlly == null || petAlly.HasBeenDiscarded)
            {
                continue;
            }

            available++;
            PetRosterEntry entry = GetRosterEntry(location);
            CompanionBondPolicyDecision bondPolicy = GetCompanionBondPolicy(entry);
            bool explicitDefendMode = _companionMode == CompanionMode.Defend;
            if (!forcePrompt && !explicitDefendMode && !bondPolicy.AllowsAutomaticDefendAssist)
            {
                skippedCooldown++;
                continue;
            }

            float defendCooldown = bondPolicy.ApplyDefendCooldown(CreatureCandidateDefendPromptCooldownSeconds);
            if (!forcePrompt
                && NextCreatureCandidateDefendPromptAt.TryGetValue(location, out float nextPromptAt)
                && now < nextPromptAt)
            {
                skippedCooldown++;
                continue;
            }

            location.MarkedNotSaved = true;
            petAlly.EnterCombat();
            NextCreatureCandidateDefendPromptAt[location] = now + defendCooldown;
            TouchCoreRuntimeProfile(
                logger,
                entry,
                "native-defend-prompt",
                $"attackers={attackerCount}; forcePrompt={forcePrompt.ToString().ToLowerInvariant()}; cooldown={defendCooldown:0.##}; {bondPolicy.FormatAuditSegment()}; existing native hero-pet ally path only",
                CompanionRuntimeProfileState.Active,
                affectedCount: 1);
            triggered++;
        }

        return new DefendTickResult(triggered, available, skippedCooldown, attackerCount);
    }

    private static int CountLiveHeroAttackers(Hero? hero)
    {
        if (hero == null)
        {
            return 0;
        }

        try
        {
            int count = 0;
            foreach (ICharacter attacker in hero.PossibleAttackers)
            {
                if (attacker != null && !attacker.WasDiscarded && attacker.IsAlive)
                {
                    count++;
                }
            }

            return count;
        }
        catch
        {
            return 0;
        }
    }

    private static Vector3 GetPlacementPosition(Hero hero, PetRosterEntry entry, int slot)
    {
        float spacing = entry.RequiresPetComponent ? RecallSpacingMeters : CreatureCandidateRecallSpacingMeters;
        float rangeScale = GetFollowPlacementScale(entry);
        Vector3 offset = (Vector3.right * entry.PlacementRight * rangeScale) + (Vector3.back * ((entry.PlacementBack * rangeScale) + spacing * slot));
        Vector3 target = hero.Coords + hero.Rotation * offset;
        return VerifyPlacementPosition(target, entry);
    }

    private static Vector3 VerifyPlacementPosition(Vector3 target, PetRosterEntry entry)
    {
        try
        {
            LocationTemplate template = new TemplateReference(entry.Guid).Get<LocationTemplate>();
            if (template != null)
            {
                return BaseLocationSpawner.VerifyPosition(target, template, allowSnapToGround: true);
            }
        }
        catch
        {
            return target;
        }

        return target;
    }

    internal static void DrawGui(ManualLogSource logger)
    {
        _logger = logger;

        if (_dialogueVisible)
        {
            if (!ShouldUseNativeCommandMenu())
            {
                SetDialogueVisible(false, logger);
                DrawStatus();
                return;
            }

            EnsureCursorForPanel();
            EnsureDialogueUiHost(logger);
        }

        if (_panelVisible)
        {
            if (!Plugin.EnableDebugPanel.Value)
            {
                SetPanelVisible(false, logger);
                DrawStatus();
                return;
            }

            EnsureCursorForPanel();
            EnsurePanelInputBlocked();
            EnsureStyles();
            EnsurePanelPlacement(force: false);
            GUI.depth = -1002;
            Rect drawnPanel = GUI.Window(PanelWindowId, _panelRect, _ => DrawPanelWindow(logger), GUIContent.none, _windowStyle!);
            _panelRect = ClampPanelToScreen(drawnPanel);
        }

        DrawCompanionHudIcon();
        DrawStatus();
    }

    private static void DrawDialogueWindow(ManualLogSource logger)
    {
        string runtimeBlockReason = GetRuntimeBlockReason();
        bool runtimeReady = string.IsNullOrEmpty(runtimeBlockReason);
        Location? location = GetDialogueLocation();
        bool hasCompanion = location != null;
        PetRosterEntry entry = hasCompanion ? GetRosterEntry(location!) : GetSelectedPet();
        string companionName = string.IsNullOrWhiteSpace(entry.DisplayName) ? "Companion" : entry.DisplayName;
        string active = runtimeReady ? GetActiveRosterPetName() : "Unavailable";
        bool canCommand = runtimeReady && hasCompanion;

        GUILayout.BeginVertical();

        GUILayout.BeginHorizontal(_headerStyle, GUILayout.Height(72f));
        GUILayout.BeginVertical();
        GUILayout.Label(companionName, _titleStyle);
        GUILayout.Label("Companion command dialogue", _mutedStyle);
        GUILayout.EndVertical();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Close", _dangerButtonStyle, GUILayout.Width(88f), GUILayout.Height(38f)))
        {
            SetDialogueVisible(false, logger: null);
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10f);
        GUILayout.BeginVertical(_sectionStyle, GUILayout.MinHeight(112f));
        GUILayout.Label("What do you need?", _labelStyle);
        GUILayout.Label($"Active: {active}", _mutedStyle);
        GUILayout.Label($"Mode: {GetModeFeedbackLine(runtimeReady)}", _mutedStyle);
        GUILayout.Label($"Follow range: {GetFollowRangeLabel(GetFollowRangeProfile())}", _mutedStyle);
        GUILayout.EndVertical();

        GUILayout.Space(10f);
        bool previousGuiEnabled = GUI.enabled;
        GUI.enabled = previousGuiEnabled && canCommand;

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Follow me", GetModeButtonStyle(CompanionMode.Follow), GUILayout.Height(44f)))
        {
            RunDialogueCommand(logger, commandLogger => ApplyCompanionMode(commandLogger, CompanionMode.Follow));
        }

        if (GUILayout.Button("Stay here", GetModeButtonStyle(CompanionMode.Stay), GUILayout.Height(44f)))
        {
            RunDialogueCommand(logger, commandLogger => ApplyCompanionMode(commandLogger, CompanionMode.Stay));
        }

        if (GUILayout.Button("Defend me", GetModeButtonStyle(CompanionMode.Defend), GUILayout.Height(44f)))
        {
            RunDialogueCommand(logger, commandLogger => ApplyCompanionMode(commandLogger, CompanionMode.Defend));
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(8f);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Close range", GetRangeButtonStyle(FollowRangeProfile.Close), GUILayout.Height(44f)))
        {
            RunDialogueCommand(logger, commandLogger => SetFollowRange(commandLogger, FollowRangeProfile.Close, recallActive: true));
        }

        if (GUILayout.Button("Normal range", GetRangeButtonStyle(FollowRangeProfile.Normal), GUILayout.Height(44f)))
        {
            RunDialogueCommand(logger, commandLogger => SetFollowRange(commandLogger, FollowRangeProfile.Normal, recallActive: true));
        }

        if (GUILayout.Button("Far range", GetRangeButtonStyle(FollowRangeProfile.Far), GUILayout.Height(44f)))
        {
            RunDialogueCommand(logger, commandLogger => SetFollowRange(commandLogger, FollowRangeProfile.Far, recallActive: true));
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(8f);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Come close", _buttonStyle, GUILayout.Height(44f)))
        {
            RunDialogueCommand(logger, HeelManagedPets);
        }

        if (GUILayout.Button("Recall", _buttonStyle, GUILayout.Height(44f)))
        {
            RunDialogueCommand(logger, RecallManagedPets);
        }

        if (GUILayout.Button("Recover", _buttonStyle, GUILayout.Height(44f)))
        {
            RunDialogueCommand(logger, RecoverManagedPets);
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(8f);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Dismiss", _dangerButtonStyle, GUILayout.Height(44f)))
        {
            RunDialogueCommand(logger, commandLogger => DismissManagedPets(commandLogger, "companion dialogue"));
        }

        GUI.enabled = previousGuiEnabled;
        if (GUILayout.Button("Goodbye", _buttonStyle, GUILayout.Height(44f)))
        {
            SetDialogueVisible(false, logger: null);
        }
        GUILayout.EndHorizontal();

        GUI.enabled = previousGuiEnabled;

        string status = runtimeReady
            ? canCommand
                ? (string.IsNullOrWhiteSpace(_statusText) ? "Ready" : _statusText)
                : "No active managed companion."
            : "Waiting: " + runtimeBlockReason;
        GUILayout.FlexibleSpace();
        GUILayout.BeginVertical(_footerStyle, GUILayout.MinHeight(58f));
        GUILayout.Label("Status: " + status, _statusStyle);
        GUILayout.EndVertical();
        GUILayout.EndVertical();

        GUI.DragWindow(new Rect(0f, 0f, Mathf.Max(180f, _dialogueRect.width - 104f), HeaderDragHeight));

        Event current = Event.current;
        if (current.type == EventType.MouseDown || current.type == EventType.MouseDrag || current.type == EventType.MouseUp)
        {
            current.Use();
        }
    }

    private static Location? GetDialogueLocation()
    {
        if (_dialogueLocation != null && !_dialogueLocation.HasBeenDiscarded && IsRosterPet(_dialogueLocation))
        {
            return _dialogueLocation;
        }

        Location? active = GetPetLocations().FirstOrDefault(IsRosterPet);
        _dialogueLocation = active;
        return active;
    }

    private static void RunDialogueCommand(ManualLogSource logger, Action<ManualLogSource> command)
    {
        command(logger);
        SetDialogueVisible(false, logger: null);
    }

    private static void DrawPanelWindow(ManualLogSource logger)
    {
        string runtimeBlockReason = GetRuntimeBlockReason();
        bool runtimeReady = string.IsNullOrEmpty(runtimeBlockReason);
        PetRosterEntry selected = GetSelectedPet();
        string active = runtimeReady ? GetActiveRosterPetName() : "Unavailable";
        string selectedGate = selected.RequiresPetComponent ? "pet" : "one-session candidate";

        GUILayout.BeginVertical();

        GUILayout.BeginHorizontal(_headerStyle, GUILayout.Height(70f));
        GUILayout.BeginVertical();
        GUILayout.Label("Avalon Companion Roster", _titleStyle);
        GUILayout.Label($"{Plugin.TogglePanelShortcut.Value}: roster panel   Esc: close", _mutedStyle);
        GUILayout.EndVertical();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Close", _dangerButtonStyle, GUILayout.Width(88f), GUILayout.Height(38f)))
        {
            SetPanelVisible(false, logger);
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10f);
        GUILayout.BeginVertical(_sectionStyle, GUILayout.MinHeight(244f));
        GUILayout.BeginHorizontal();
        bool drewIcon = DrawRosterIcon(selected, 82f);
        if (drewIcon)
        {
            GUILayout.Space(14f);
        }

        GUILayout.BeginVertical();
        GUILayout.Label($"{Plugin.SelectedPetIndex.Value + 1}/{Roster.Length} {selected.DisplayName}", _labelStyle);
        GUILayout.Label(selectedGate, _mutedStyle);
        GUILayout.Label($"Template: {selected.TemplateName}", _mutedStyle);
        GUILayout.Label($"Active: {active}", _mutedStyle);
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
        GUILayout.Space(8f);
        GUILayout.Label(GetCompanionTrustStatusLine(selected), _mutedStyle);
        GUILayout.Label(GetCompanionBondPolicyStatusLine(selected), _mutedStyle);
        GUILayout.Label(GetCommandPolicyStatusLine(selected), _mutedStyle);
        GUILayout.Label(GetLifecycleStatusLine(runtimeReady), _mutedStyle);
        foreach (string line in GetActiveCompanionStatusLines(runtimeReady))
        {
            GUILayout.Label(line, _statusStyle);
        }

        GUILayout.EndVertical();

        GUILayout.Space(10f);
        bool previousGuiEnabled = GUI.enabled;
        GUI.enabled = previousGuiEnabled && runtimeReady;

        GUILayout.BeginVertical(_sectionStyle, GUILayout.MinHeight(96f));
        GUILayout.Label("Summon", _labelStyle);
        GUILayout.Space(4f);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Previous", _buttonStyle, GUILayout.Width(128f), GUILayout.Height(46f)))
        {
            SelectRelativePet(logger, -1);
        }

        if (GUILayout.Button("Summon / Swap", _selectedButtonStyle, GUILayout.Height(46f)))
        {
            SummonSelectedPet(logger);
        }

        if (GUILayout.Button("Next", _buttonStyle, GUILayout.Width(128f), GUILayout.Height(46f)))
        {
            SelectRelativePet(logger, 1);
        }
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();

        GUILayout.Space(10f);
        GUILayout.BeginVertical(_sectionStyle, GUILayout.MinHeight(96f));
        GUILayout.Label("Active Companion", _labelStyle);
        GUILayout.Space(4f);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Recover Active", _buttonStyle, GUILayout.Height(44f)))
        {
            RecoverManagedPets(logger);
        }

        if (GUILayout.Button("Dismiss Active", _dangerButtonStyle, GUILayout.Height(44f)))
        {
            DismissManagedPets(logger, "panel button");
        }
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();

        GUILayout.Space(10f);
        GUILayout.BeginVertical(_sectionStyle, GUILayout.MinHeight(96f));
        GUILayout.Label("Diagnostics", _labelStyle);
        GUILayout.Space(4f);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Encounter Probe", _buttonStyle, GUILayout.Height(44f)))
        {
            RunManualTamingEncounterProbe(logger);
        }

        if (GUILayout.Button("Lifecycle Check", _buttonStyle, GUILayout.Height(44f)))
        {
            RunManualLifecycleCheck(logger);
        }

        if (GUILayout.Button("AI Boundary Check", _buttonStyle, GUILayout.Height(44f)))
        {
            RunManualAiBoundaryCheck(logger);
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(6f);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("AI Profile Check", _buttonStyle, GUILayout.Height(44f)))
        {
            RunManualAiProfileCheck(logger);
        }

        if (GUILayout.Button("Progression Gate", _buttonStyle, GUILayout.Height(44f)))
        {
            RunManualProgressionPersistenceGateCheck(logger);
        }

        if (GUILayout.Button("Taming Gate", _buttonStyle, GUILayout.Height(44f)))
        {
            RunManualTamingEligibilityGateCheck(logger);
        }
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();

        GUI.enabled = previousGuiEnabled;

        string status = runtimeReady
            ? (string.IsNullOrWhiteSpace(_statusText) ? "Ready" : _statusText)
            : "Waiting: " + runtimeBlockReason;
        GUILayout.FlexibleSpace();
        GUILayout.BeginVertical(_footerStyle, GUILayout.MinHeight(58f));
        GUILayout.Label("Status: " + status, _statusStyle);
        GUILayout.EndVertical();
        GUILayout.EndVertical();

        GUI.DragWindow(new Rect(0f, 0f, Mathf.Max(180f, _panelRect.width - 104f), HeaderDragHeight));

        Event current = Event.current;
        if (current.type == EventType.MouseDown || current.type == EventType.MouseDrag || current.type == EventType.MouseUp)
        {
            current.Use();
        }
    }

    private static bool DrawRosterIcon(PetRosterEntry entry, float size)
    {
        string iconId = GetRosterIconId(entry);
        Texture2D? icon = TaintedInterfaceBridge.GetIcon(iconId);
        if (icon == null)
        {
            return false;
        }

        GUILayout.Label(icon, GUIStyle.none, GUILayout.Width(size), GUILayout.Height(size));
        return true;
    }

    private static void DrawCompanionHudIcon()
    {
        if (Event.current.type != EventType.Repaint
            || !Plugin.ShowCompanionHudIcon.Value
            || !Plugin.Enabled.Value
            || !Plugin.EnablePetCompanionRoster.Value
            || Plugin.ResearchModeOnly.Value
            || _panelVisible
            || _dialogueVisible)
        {
            return;
        }

        if (!TryGetActiveHudCompanionEntry(out PetRosterEntry entry))
        {
            return;
        }

        string iconId = GetRosterIconId(entry);
        float size = Mathf.Clamp(Plugin.CompanionHudIconSize.Value, 56, 128);
        string corner = Plugin.CompanionHudIconCorner.Value?.Trim() ?? string.Empty;
        if (TaintedInterfaceBridge.DrawHudBadge(corner, size, iconId, CompanionHudIconBackgroundId))
        {
            return;
        }

        Texture2D? icon = GetCachedHudIcon(iconId);
        if (icon == null)
        {
            return;
        }

        Rect frameRect = GetCompanionHudIconRect(size);
        Texture2D? background = GetCompanionHudBackground();
        float iconSize = Mathf.Clamp(size * 0.54f, 32f, size - 18f);
        Rect iconRect = new Rect(
            frameRect.x + (frameRect.width - iconSize) * 0.5f,
            frameRect.y + (frameRect.height - iconSize) * 0.5f,
            iconSize,
            iconSize);

        Color previousColor = GUI.color;
        int previousDepth = GUI.depth;
        GUI.depth = -900;

        if (background != null)
        {
            GUI.color = new Color(1f, 1f, 1f, 0.92f);
            GUI.DrawTexture(frameRect, background, ScaleMode.ScaleToFit, true);
        }

        GUI.color = new Color(1f, 1f, 1f, 0.96f);
        GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit, true);
        GUI.color = previousColor;
        GUI.depth = previousDepth;
    }

    private static bool TryGetActiveHudCompanionEntry(out PetRosterEntry entry)
    {
        float now = Time.realtimeSinceStartup;
        if (now < _nextHudCompanionLookupAt)
        {
            entry = _hudCompanionEntryAvailable ? _hudCompanionEntry : default;
            return _hudCompanionEntryAvailable;
        }

        _nextHudCompanionLookupAt = now + CompanionHudLookupIntervalSeconds;
        Location? active = GetPetLocations().FirstOrDefault(IsRosterPet);
        if (active != null && TryGetRosterEntry(active, out PetRosterEntry current))
        {
            _hudCompanionEntry = current;
            _hudCompanionEntryAvailable = true;
            entry = current;
            return true;
        }

        _hudCompanionEntry = default;
        _hudCompanionEntryAvailable = false;
        entry = default;
        return false;
    }

    private static Texture2D? GetCachedHudIcon(string iconId)
    {
        if (string.Equals(_hudCompanionIconId, iconId, StringComparison.OrdinalIgnoreCase) && _hudCompanionIconTexture != null)
        {
            return _hudCompanionIconTexture;
        }

        _hudCompanionIconId = iconId;
        _hudCompanionIconTexture = TaintedInterfaceBridge.GetIcon(iconId);
        return _hudCompanionIconTexture;
    }

    private static Texture2D? GetCompanionHudBackground()
    {
        if (_hudCompanionBackgroundTexture != null)
        {
            return _hudCompanionBackgroundTexture;
        }

        float now = Time.realtimeSinceStartup;
        if (now < _nextHudBackgroundLookupAt)
        {
            return null;
        }

        _nextHudBackgroundLookupAt = now + 1f;
        _hudCompanionBackgroundTexture = TaintedInterfaceBridge.GetIcon(CompanionHudIconBackgroundId);
        return _hudCompanionBackgroundTexture;
    }

    private static Rect GetCompanionHudIconRect(float size)
    {
        string corner = Plugin.CompanionHudIconCorner.Value?.Trim() ?? string.Empty;
        bool bottomRight = string.Equals(corner, "BottomRight", StringComparison.OrdinalIgnoreCase)
            || string.Equals(corner, "Bottom Right", StringComparison.OrdinalIgnoreCase)
            || string.Equals(corner, "bottom-right", StringComparison.OrdinalIgnoreCase);

        float x = bottomRight ? Screen.width - size - 40f : 24f;
        float topLeftY = Mathf.Max(12f, 136f - size - 12f);
        float y = bottomRight ? Screen.height - size - 220f : topLeftY;
        float maxX = Mathf.Max(12f, Screen.width - size - 12f);
        float maxY = Mathf.Max(12f, Screen.height - size - 12f);
        return new Rect(
            Mathf.Clamp(x, 12f, maxX),
            Mathf.Clamp(y, 12f, maxY),
            size,
            size);
    }

    private static string GetRosterIconId(PetRosterEntry entry)
    {
        if (entry.RequiresPetComponent)
        {
            return "companion.pet";
        }

        string key = entry.TemplateName + " " + entry.DisplayName;
        if (ContainsIconKey(key, "Wolf"))
        {
            return "companion.wolf";
        }

        if (ContainsIconKey(key, "Bear"))
        {
            return "companion.bear";
        }

        if (ContainsIconKey(key, "Deer"))
        {
            return "companion.deer";
        }

        if (ContainsIconKey(key, "Pig"))
        {
            return "companion.pig";
        }

        if (ContainsIconKey(key, "Cow"))
        {
            return "companion.cow";
        }

        if (ContainsIconKey(key, "Bullrat"))
        {
            return "companion.bullrat";
        }

        if (ContainsIconKey(key, "CorpseEater") || ContainsIconKey(key, "Corpse Eater"))
        {
            return "companion.corpse-eater";
        }

        if (ContainsIconKey(key, "Redcap"))
        {
            return "companion.redcap";
        }

        if (ContainsIconKey(key, "Flamegobbler"))
        {
            return "companion.flamegobbler";
        }

        if (ContainsIconKey(key, "Grindylow"))
        {
            return "companion.grindylow";
        }

        if (ContainsIconKey(key, "Wyrdspirit"))
        {
            return "companion.wyrdspirit";
        }

        if (ContainsIconKey(key, "Sharg"))
        {
            return "companion.sharg";
        }

        if (ContainsIconKey(key, "Ogre"))
        {
            return "companion.ogre";
        }

        if (ContainsIconKey(key, "Drowner"))
        {
            return "companion.drowner";
        }

        if (ContainsIconKey(key, "Skeleton"))
        {
            return "companion.skeleton";
        }

        if (ContainsIconKey(key, "Zombie"))
        {
            return "companion.zombie";
        }

        if (ContainsIconKey(key, "Floatling"))
        {
            return "companion.floatling";
        }

        if (ContainsIconKey(key, "Tadpole"))
        {
            return "companion.tadpole";
        }

        return "companion.creature";
    }

    private static bool ContainsIconKey(string value, string token)
    {
        return value.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    internal static void DrawStatus()
    {
        if (!Plugin.ShowCommandStatus.Value || string.IsNullOrEmpty(_statusText) || Time.realtimeSinceStartup > _statusUntil)
        {
            return;
        }

        GUIStyle style = new GUIStyle(GUI.skin.box)
        {
            alignment = TextAnchor.MiddleLeft,
            fontSize = 18,
            wordWrap = false,
        };
        style.normal.textColor = Color.white;

        Rect rect = new Rect(20f, Screen.height - 92f, 560f, 36f);
        GUI.Box(rect, "Avalon Companions: " + _statusText, style);
    }

    private static void SetStatus(ManualLogSource? logger, string message, bool warning = false)
    {
        _statusText = message;
        _statusUntil = Time.realtimeSinceStartup + Math.Max(0.5f, Plugin.CommandStatusSeconds.Value);

        if (logger == null)
        {
            return;
        }

        if (warning)
        {
            logger.LogWarning(message);
        }
        else
        {
            logger.LogInfo(message);
        }
    }

    private static void WriteCompanionCommandLog(
        ManualLogSource logger,
        string command,
        int affectedCount,
        bool blocked,
        string reason,
        bool touchesTargeting = false,
        bool touchesPersistence = false)
    {
        if (!Plugin.WriteCompanionCommandLog.Value)
        {
            return;
        }

        try
        {
            string folder = Path.Combine(Paths.ConfigPath, Plugin.PluginGuid);
            Directory.CreateDirectory(folder);
            string path = Path.Combine(folder, "companion-command-log.csv");
            bool writeHeader = !File.Exists(path) || new FileInfo(path).Length == 0;

            PetRosterEntry selected = GetSelectedPet();
            List<Location> activeRoster = GetPetLocations().Where(IsRosterPet).ToList();

            StringBuilder builder = new StringBuilder();
            if (writeHeader)
            {
                builder.AppendLine("localTime,frame,command,mode,followRange,selectedIndex,selectedName,selectedGuid,activeRosterCount,affectedCount,blocked,touchesTargeting,touchesPersistence,reason");
            }

            AppendLifecycleCsvRow(
                builder,
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture),
                Time.frameCount.ToString(CultureInfo.InvariantCulture),
                command,
                GetModeLabel(_companionMode),
                GetFollowRangeLabel(GetFollowRangeProfile()),
                NormalizeIndex(Plugin.SelectedPetIndex.Value).ToString(CultureInfo.InvariantCulture),
                selected.DisplayName,
                selected.Guid,
                activeRoster.Count.ToString(CultureInfo.InvariantCulture),
                affectedCount.ToString(CultureInfo.InvariantCulture),
                blocked ? "true" : "false",
                touchesTargeting ? "true" : "false",
                touchesPersistence ? "true" : "false",
                reason);

            File.AppendAllText(path, builder.ToString(), Encoding.UTF8);
        }
        catch (Exception ex)
        {
            if (!_commandLogFailureLogged)
            {
                _commandLogFailureLogged = true;
                logger.LogWarning($"Companion command CSV write failed: {ex.GetType().Name}: {ex.Message}");
            }
        }
    }

    private static void TickCompanionAiBoundaryDiagnostics(ManualLogSource logger)
    {
        if (!Plugin.WriteCompanionAiBoundaryDiagnostics.Value)
        {
            return;
        }

        float interval = Plugin.CompanionAiBoundaryDiagnosticSeconds.Value;
        if (float.IsNaN(interval) || interval < MinimumAiBoundaryDiagnosticSeconds)
        {
            interval = MinimumAiBoundaryDiagnosticSeconds;
        }

        if (Time.realtimeSinceStartup < _nextAiBoundaryDiagnosticAt)
        {
            return;
        }

        _nextAiBoundaryDiagnosticAt = Time.realtimeSinceStartup + interval;

        try
        {
            List<Location> activeRoster = CollectAiBoundaryDiagnosticLocations(logger);
            if (activeRoster.Count == 0)
            {
                return;
            }

            WriteCompanionAiBoundarySnapshot(logger, "periodic", activeRoster);
            logger.LogInfo(
                "Companion AI boundary diagnostic sampled "
                + $"{activeRoster.Count} active managed companion(s); file=companion-ai-boundary.csv; "
                + "action=read-only; touchesCommands=False; touchesMovement=False; touchesTargeting=False; "
                + "touchesPersistence=False; coreExecuted=False.");
        }
        catch (Exception ex)
        {
            LogAiBoundaryDiagnosticFailure(logger, "sample", ex);
        }
    }

    private static void TickCompanionAiProfileAudit(ManualLogSource logger)
    {
        if (!Plugin.WriteCompanionAiProfileAudit.Value)
        {
            return;
        }

        float interval = Plugin.CompanionAiProfileAuditSeconds.Value;
        if (float.IsNaN(interval) || interval < MinimumAiBoundaryDiagnosticSeconds)
        {
            interval = MinimumAiBoundaryDiagnosticSeconds;
        }

        if (Time.realtimeSinceStartup < _nextAiProfileAuditAt)
        {
            return;
        }

        _nextAiProfileAuditAt = Time.realtimeSinceStartup + interval;

        try
        {
            List<Location> activeRoster = CollectAiBoundaryDiagnosticLocations(logger);
            WriteCompanionAiProfileSnapshot(logger, "periodic", activeRoster);
            string classified = activeRoster.Count == 0
                ? "no active managed companion"
                : $"{activeRoster.Count} active managed companion(s)";
            string logKey = $"{GetActiveSceneName()}|{classified}";
            if (!string.Equals(_lastAiProfileAuditLogKey, logKey, StringComparison.Ordinal)
                || Time.realtimeSinceStartup >= _nextAiProfileAuditSummaryLogAt)
            {
                _lastAiProfileAuditLogKey = logKey;
                _nextAiProfileAuditSummaryLogAt = Time.realtimeSinceStartup + AiProfileAuditSummaryLogSeconds;
                logger.LogInfo(
                    "Companion AI profile audit classified "
                    + $"{classified}; file=companion-ai-profile.csv; action=read-only; log=coalesced; "
                    + "touchesCommands=False; touchesMovement=False; touchesTargeting=False; "
                    + "touchesPersistence=False; coreExecuted=False.");
            }
        }
        catch (Exception ex)
        {
            LogAiProfileAuditFailure(logger, "sample", ex);
        }
    }

    private static void TickProfileDrivenNativeAssist(ManualLogSource logger)
    {
        if (!Plugin.EnableProfileDrivenNativeAssist.Value || IsAiRuntimeOwned)
        {
            return;
        }

        float interval = Plugin.ProfileDrivenNativeAssistSeconds.Value;
        if (float.IsNaN(interval) || interval < MinimumAiBoundaryDiagnosticSeconds)
        {
            interval = MinimumAiBoundaryDiagnosticSeconds;
        }

        if (Time.realtimeSinceStartup < _nextProfileDrivenNativeAssistAt)
        {
            return;
        }

        _nextProfileDrivenNativeAssistAt = Time.realtimeSinceStartup + interval;

        try
        {
            RunProfileDrivenNativeAssist(logger);
        }
        catch (Exception ex)
        {
            logger.LogWarning($"Profile-driven native assist failed: {ex.GetType().Name}: {ex.Message}");
        }
    }

    private static void RunProfileDrivenNativeAssist(ManualLogSource logger)
    {
        Hero hero = Hero.Current;
        if (hero == null)
        {
            return;
        }

        RemoveDiscardedCreatureCandidates();
        List<Location> activeRoster = CollectAiBoundaryDiagnosticLocations(logger);
        if (activeRoster.Count == 0)
        {
            return;
        }

        string scene = GetActiveSceneName();
        int trackedCreatureCount = CreatureCandidateLocations
            .Count(location => location != null && !location.HasBeenDiscarded);
        int catchUpCount = 0;
        int nativeCombatRows = 0;

        for (int i = 0; i < activeRoster.Count; i++)
        {
            Location location = activeRoster[i];
            CompanionAiProfile profile = BuildCompanionAiProfile(location, scene, activeRoster.Count, trackedCreatureCount);
            if (profile.Intent == CompanionAiIntent.FollowCatchUpCandidate && ShouldFollowForMode(_companionMode))
            {
                PetRosterEntry entry = GetRosterEntry(location);
                RecallLocation(location, GetPlacementPosition(hero, entry, i), hero.Rotation, followAfterRecall: true);
                TouchCoreRuntimeProfile(
                    logger,
                    entry,
                    "ai-profile-native-catch-up",
                    profile.IntentReason + "; existing catch-up recall path only",
                    CompanionRuntimeProfileState.Active,
                    affectedCount: 1);
                catchUpCount++;
            }
            else if (profile.Intent == CompanionAiIntent.NativeCombatObserved)
            {
                nativeCombatRows++;
            }
        }

        int defendPrompted = 0;
        int attackerCount = CountLiveHeroAttackers(hero);
        bool explicitDefendMode = _companionMode == CompanionMode.Defend;
        CompanionBondPolicyDecision activeBondPolicy = GetActiveCompanionBondPolicy();
        bool canUseNativeDefend =
            explicitDefendMode
            || (Plugin.EnableNativeDefendAssist.Value && activeBondPolicy.AllowsAutomaticDefendAssist);
        if (canUseNativeDefend)
        {
            TrackDefendAttackerState(logger, attackerCount, explicitDefendMode);
        }

        bool shouldPromptNativeDefend =
            attackerCount > 0
            && canUseNativeDefend
            && (nativeCombatRows > 0 || explicitDefendMode);
        if (shouldPromptNativeDefend)
        {
            DefendTickResult defendResult = TriggerCreatureCandidateDefend(attackerCount, forcePrompt: false, logger);
            defendPrompted = defendResult.Prompted;
        }

        if (catchUpCount > 0)
        {
            float followInterval = activeBondPolicy.ApplyCatchUpInterval(CreatureCandidateFollowTickSeconds);
            _nextCreatureCandidateFollowTick = Mathf.Max(
                _nextCreatureCandidateFollowTick,
                Time.realtimeSinceStartup + followInterval);
            string reason = "profile-driven native assist; intent=FollowCatchUpCandidate; "
                + $"range={GetFollowRangeLabel(GetFollowRangeProfile())}; affected={catchUpCount}; interval={followInterval:0.##}; "
                + $"{activeBondPolicy.FormatAuditSegment()}; "
                + "existing recall/catch-up path only; sibling follow tick deferred; "
                + "touchesTargeting=false; touchesPersistence=false";
            logger.LogInfo($"Profile-driven native assist recalled {catchUpCount} managed companion(s) through the existing catch-up path.");
            WriteCompanionCommandLog(logger, "ai-profile-native-catch-up", affectedCount: catchUpCount, blocked: false, reason: reason);
        }

        if (defendPrompted > 0)
        {
            _nextCreatureCandidateDefendTick = Mathf.Max(
                _nextCreatureCandidateDefendTick,
                Time.realtimeSinceStartup + CreatureCandidateDefendTickSeconds);
            string defendSource = nativeCombatRows > 0 ? "NativeCombatObserved" : "ExplicitDefendHeroAttackers";
            float defendCooldown = activeBondPolicy.ApplyDefendCooldown(CreatureCandidateDefendPromptCooldownSeconds);
            string reason = $"profile-driven native assist; intent={defendSource}; "
                + $"attackers={attackerCount}; nativeCombatRows={nativeCombatRows}; prompted={defendPrompted}; cooldown={defendCooldown:0.##}; "
                + $"{activeBondPolicy.FormatAuditSegment()}; "
                + "existing NpcHeroPetAlly.EnterCombat path only; sibling defend tick deferred; "
                + "touchesTargeting=false; touchesPersistence=false";
            logger.LogInfo($"Profile-driven native assist prompted {defendPrompted} managed companion(s) through the existing native ally combat path.");
            WriteCompanionCommandLog(logger, "ai-profile-native-defend", affectedCount: defendPrompted, blocked: false, reason: reason);
        }

        if (catchUpCount > 0 || defendPrompted > 0)
        {
            SetStatus(logger, $"Profile assist: catch-up {catchUpCount}, defend {defendPrompted}");
        }
    }

    private static List<Location> CollectAiBoundaryDiagnosticLocations(ManualLogSource logger)
    {
        List<Location> locations = new List<Location>();

        foreach (PetElement petElement in World.All<PetElement>().ToArraySlow())
        {
            if (petElement == null || petElement.HasBeenDiscarded)
            {
                continue;
            }

            AddLocation(locations, petElement.ParentModel);
        }

        foreach (PetVariantBase petVariant in World.All<PetVariantBase>().ToArraySlow())
        {
            if (petVariant == null || petVariant.HasBeenDiscarded)
            {
                continue;
            }

            AddLocation(locations, petVariant.ParentModel);
        }

        foreach (Location candidate in CreatureCandidateLocations.ToArray())
        {
            AddLocation(locations, candidate);
        }

        foreach (Location location in CollectRosterHeroPetAllyLocations(logger))
        {
            AddLocation(locations, location);
        }

        return locations
            .Where(location => location != null && !location.HasBeenDiscarded && IsRosterPet(location))
            .ToList();
    }

    private static void WriteCompanionAiBoundarySnapshot(ManualLogSource logger, string reason, List<Location> activeRoster)
    {
        try
        {
            string folder = Path.Combine(Paths.ConfigPath, Plugin.PluginGuid);
            Directory.CreateDirectory(folder);
            string path = Path.Combine(folder, "companion-ai-boundary.csv");
            bool writeHeader = !File.Exists(path) || new FileInfo(path).Length == 0;

            StringBuilder builder = new StringBuilder();
            if (writeHeader)
            {
                builder.AppendLine("localTime,frame,reason,scene,heroCoords,heroLiveAttackers,mode,followRange,activeRosterCount,trackedCreatureCount,templateGuid,templateName,displayName,locationId,debugName,coords,markedNotSaved,isTrackedCreature,hasPetElement,hasPetVariant,hasNpcElement,hasNpcHeroPetAlly,hasCommandAction,npcAlive,npcWorking,npcInCombat,npcInAlert,npcInIdle,npcInFlee,npcHeroVisibility,npcHeroVisible,npcAlertValue,npcHasPerception,npcIsUnconscious,currentTargetName,currentTargetType,currentTargetTemplateGuid,currentTargetDistance,possibleTargetCount,possibleAttackerCount,movementState,canMove,canOverrideDestination,shouldResetMovementSpeed,hasTargetOverride,hasHeroSummonTargetOverride,touchesCommands,touchesMovement,touchesTargeting,touchesPersistence,coreExecuted,notes");
            }

            string localTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
            string frame = Time.frameCount.ToString(CultureInfo.InvariantCulture);
            string scene = GetActiveSceneName();
            string heroCoords = Hero.Current == null ? string.Empty : FormatVector(Hero.Current.Coords);
            string heroLiveAttackers = CountLiveHeroAttackers(Hero.Current).ToString(CultureInfo.InvariantCulture);
            string mode = GetModeLabel(_companionMode);
            string followRange = GetFollowRangeLabel(GetFollowRangeProfile());
            string activeRosterCount = activeRoster.Count.ToString(CultureInfo.InvariantCulture);
            string trackedCreatureCount = CreatureCandidateLocations
                .Count(location => location != null && !location.HasBeenDiscarded)
                .ToString(CultureInfo.InvariantCulture);

            foreach (Location location in activeRoster)
            {
                AppendCompanionAiBoundaryRow(
                    builder,
                    localTime,
                    frame,
                    reason,
                    scene,
                    heroCoords,
                    heroLiveAttackers,
                    mode,
                    followRange,
                    activeRosterCount,
                    trackedCreatureCount,
                    location);
            }

            File.AppendAllText(path, builder.ToString(), Encoding.UTF8);
        }
        catch (Exception ex)
        {
            LogAiBoundaryDiagnosticFailure(logger, "csv", ex);
        }
    }

    private static void WriteCompanionAiProfileSnapshot(ManualLogSource logger, string reason, List<Location> activeRoster)
    {
        try
        {
            string folder = Path.Combine(Paths.ConfigPath, Plugin.PluginGuid);
            Directory.CreateDirectory(folder);
            string path = Path.Combine(folder, "companion-ai-profile.csv");
            bool writeHeader = !File.Exists(path) || new FileInfo(path).Length == 0;

            StringBuilder builder = new StringBuilder();
            if (writeHeader)
            {
                builder.AppendLine("localTime,frame,reason,scene,mode,followRange,activeRosterCount,trackedCreatureCount,intent,intentReason,templateGuid,templateName,displayName,locationId,debugName,coords,distanceToHero,catchUpThreshold,heroLiveAttackers,possibleTargetCount,possibleAttackerCount,movementState,canMove,canOverrideDestination,npcAlive,npcWorking,npcInCombat,npcInIdle,hasNpcElement,hasTargetOverride,hasHeroSummonTargetOverride,touchesCommands,touchesMovement,touchesTargeting,touchesPersistence,coreExecuted,notes");
            }

            string localTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
            string frame = Time.frameCount.ToString(CultureInfo.InvariantCulture);
            string scene = GetActiveSceneName();
            int activeRosterCount = activeRoster.Count;
            int trackedCreatureCount = CreatureCandidateLocations
                .Count(location => location != null && !location.HasBeenDiscarded);

            if (activeRoster.Count == 0)
            {
                AppendCompanionAiProfileRow(
                    builder,
                    localTime,
                    frame,
                    reason,
                    CreateNoActiveCompanionProfile(scene, trackedCreatureCount));
            }
            else
            {
                foreach (Location location in activeRoster)
                {
                    AppendCompanionAiProfileRow(
                        builder,
                        localTime,
                        frame,
                        reason,
                        BuildCompanionAiProfile(location, scene, activeRosterCount, trackedCreatureCount));
                }
            }

            File.AppendAllText(path, builder.ToString(), Encoding.UTF8);
        }
        catch (Exception ex)
        {
            LogAiProfileAuditFailure(logger, "csv", ex);
        }
    }

    private static List<Location> CollectTamingEncounterCandidates()
    {
        List<Location> candidates = new List<Location>();
        Hero hero = Hero.Current;
        if (hero == null)
        {
            return candidates;
        }

        float radius = GetTamingEncounterProbeRadius();
        float radiusSquared = radius * radius;
        foreach (Location location in World.All<Location>().ToArraySlow())
        {
            if (location == null || location.HasBeenDiscarded)
            {
                continue;
            }

            if (!TryGetTameableRosterEntry(location, out _))
            {
                continue;
            }

            float distanceSquared = (location.Coords - hero.Coords).sqrMagnitude;
            if (distanceSquared > radiusSquared && !CreatureCandidateLocations.Contains(location))
            {
                continue;
            }

            AddLocation(candidates, location);
        }

        return candidates
            .OrderBy(location => Vector3.Distance(location.Coords, hero.Coords))
            .ThenBy(location => location.Template?.name ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .ThenBy(location => location.ID ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static float GetTamingEncounterProbeRadius()
    {
        return TamingEncounterProbeRadius;
    }

    private static bool TryGetTameableRosterEntry(Location location, out PetRosterEntry entry)
    {
        foreach (PetRosterEntry candidate in Roster)
        {
            if (!IsTemplate(location, candidate.Guid))
            {
                continue;
            }

            CompanionTamingProfile profile = BuildCompanionTamingProfile(candidate);
            if (profile.Eligibility == CompanionTamingEligibility.TameableCandidate)
            {
                entry = candidate;
                return true;
            }
        }

        entry = default;
        return false;
    }

    private static void WriteTamingEncounterSnapshot(ManualLogSource logger, string reason, List<Location> candidates)
    {
        try
        {
            string folder = Path.Combine(Paths.ConfigPath, Plugin.PluginGuid);
            Directory.CreateDirectory(folder);
            string path = Path.Combine(folder, "companion-taming-encounter.csv");
            bool writeHeader = !File.Exists(path) || new FileInfo(path).Length == 0;

            StringBuilder builder = new StringBuilder();
            if (writeHeader)
            {
                builder.AppendLine("localTime,frame,reason,scene,heroCoords,scanRadius,candidateCount,templateGuid,templateName,displayName,reviewId,eligibility,locationId,debugName,locationDisplayName,coords,distanceFromHero,currentDomain,markedNotSaved,isTrackedCreature,isAvalonManaged,hasPetElement,hasPetVariant,hasNpcElement,hasNpcHeroPetAlly,hasCommandAction,npcAlive,npcWorking,npcInCombat,npcInAlert,npcInIdle,npcInFlee,npcHeroVisibility,npcHeroVisible,npcAlertValue,npcHasPerception,npcIsUnconscious,currentTargetName,currentTargetType,currentTargetTemplateGuid,currentTargetDistance,currentTargetIsHero,possibleTargetCount,possibleAttackerCount,heroInPossibleTargets,heroInPossibleAttackers,movementState,canMove,canOverrideDestination,shouldResetMovementSpeed,hasTargetOverride,hasHeroSummonTargetOverride,factionHints,ownershipHints,wildAiPackageLane,wildAiPackageMapping,postTameAiPackageLane,tameInteractionOwner,aiPackageCanPerformTame,touchesCommands,touchesMovement,touchesTargeting,touchesPersistence,coreExecuted,notes");
            }

            string localTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
            string frame = Time.frameCount.ToString(CultureInfo.InvariantCulture);
            string scene = GetActiveSceneName();
            string heroCoords = Hero.Current == null ? string.Empty : FormatVector(Hero.Current.Coords);
            string scanRadius = GetTamingEncounterProbeRadius().ToString("0.###", CultureInfo.InvariantCulture);
            string candidateCount = candidates.Count.ToString(CultureInfo.InvariantCulture);

            if (candidates.Count == 0)
            {
                AppendTamingEncounterNoCandidateRow(builder, localTime, frame, reason, scene, heroCoords, scanRadius, candidateCount);
            }
            else
            {
                foreach (Location location in candidates)
                {
                    AppendTamingEncounterRow(
                        builder,
                        localTime,
                        frame,
                        reason,
                        scene,
                        heroCoords,
                        scanRadius,
                        candidateCount,
                        location);
                }
            }

            File.AppendAllText(path, builder.ToString(), Encoding.UTF8);
        }
        catch (Exception ex)
        {
            if (!_tamingEncounterProbeFailureLogged)
            {
                _tamingEncounterProbeFailureLogged = true;
                logger.LogWarning($"Taming encounter CSV write failed: {ex.GetType().Name}: {ex.Message}");
            }
        }
    }

    private static void AppendTamingEncounterNoCandidateRow(
        StringBuilder builder,
        string localTime,
        string frame,
        string reason,
        string scene,
        string heroCoords,
        string scanRadius,
        string candidateCount)
    {
        string[] values = Enumerable.Repeat(string.Empty, 65).ToArray();
        values[0] = localTime;
        values[1] = frame;
        values[2] = reason;
        values[3] = scene;
        values[4] = heroCoords;
        values[5] = scanRadius;
        values[6] = candidateCount;
        values[54] = "none";
        values[55] = "none";
        values[56] = "kane.tgfoa.avalon-companions.ai-intent-profile";
        values[57] = "Avalon Companions player tame interaction";
        values[58] = "false";
        values[59] = "false";
        values[60] = "false";
        values[61] = "false";
        values[62] = "false";
        values[63] = "false";
        values[64] = "no reviewed live wolf/bear candidate within scan radius; read-only no-candidate row";
        AppendLifecycleCsvRow(builder, values);
    }

    private static void AppendTamingEncounterRow(
        StringBuilder builder,
        string localTime,
        string frame,
        string reason,
        string scene,
        string heroCoords,
        string scanRadius,
        string candidateCount,
        Location location)
    {
        bool discarded = location == null || location.HasBeenDiscarded;
        NpcElement? npcElement = null;
        bool hasNpcElement = !discarded && location!.TryGetElement(out npcElement) && npcElement != null;
        ICharacter? currentTarget = hasNpcElement ? ReadCurrentTargetNoChecks(npcElement) : null;
        TryGetTameableRosterEntry(location!, out PetRosterEntry entry);
        CompanionTamingProfile profile = BuildCompanionTamingProfile(entry);
        float? distanceFromHero = null;
        if (!discarded && Hero.Current != null)
        {
            distanceFromHero = Vector3.Distance(location!.Coords, Hero.Current.Coords);
        }

        bool isTrackedCreature = !discarded && CreatureCandidateLocations.Contains(location!);
        bool hasCommandAction = !discarded && location!.HasElement<AvalonCompanionCommandAction>();
        bool hasHeroPetAlly = !discarded && HasNativeHeroPetAlly(location!);
        bool isAvalonManaged = isTrackedCreature || hasCommandAction || NativeCommandSurfaceKeys.ContainsKey(location!);
        string factionHints = JoinDistinct(new[]
        {
            ReadNamedMemberHints("npc", npcElement, "Faction", "CurrentFaction", "FactionTemplate", "FactionOverride", "FactionOverrideContext"),
            ReadNamedMemberHints("location", location, "Faction", "CurrentFaction", "FactionTemplate", "FactionOverride", "FactionOverrideContext"),
        });
        string ownershipHints = JoinDistinct(new[]
        {
            ReadNamedMemberHints("npc", npcElement, "Owner", "Master", "PetOwner", "Summoner", "Ally", "ParentModel", "GenericParentModel"),
            ReadNamedMemberHints("location", location, "Owner", "Master", "PetOwner", "Summoner", "Ally", "ParentModel", "GenericParentModel"),
        });

        AppendLifecycleCsvRow(
            builder,
            localTime,
            frame,
            reason,
            scene,
            heroCoords,
            scanRadius,
            candidateCount,
            discarded ? string.Empty : location!.Template?.GUID ?? string.Empty,
            discarded ? string.Empty : location!.Template?.name ?? string.Empty,
            profile.DisplayName,
            profile.ReviewId,
            GetTamingEligibilityLabel(profile.Eligibility),
            discarded ? string.Empty : location!.ID ?? string.Empty,
            discarded ? string.Empty : location!.DebugName ?? string.Empty,
            discarded ? string.Empty : SafeObjectText(location!.DisplayName),
            discarded ? string.Empty : FormatVector(location!.Coords),
            FormatNullableFloat(distanceFromHero),
            discarded ? string.Empty : ReadCurrentDomain(location!),
            discarded ? string.Empty : BoolText(location!.MarkedNotSaved),
            discarded ? string.Empty : BoolText(isTrackedCreature),
            discarded ? string.Empty : BoolText(isAvalonManaged),
            discarded ? string.Empty : BoolText(location!.TryGetElement(out PetElement _)),
            discarded ? string.Empty : BoolText(location!.TryGetElement(out PetVariantBase _)),
            BoolText(hasNpcElement),
            discarded ? string.Empty : BoolText(hasHeroPetAlly),
            discarded ? string.Empty : BoolText(hasCommandAction),
            hasNpcElement ? ReadNpcBool(npcElement, npc => npc.IsAlive) : string.Empty,
            hasNpcElement ? ReadNpcAiBool(npcElement, ai => ai.Working) : string.Empty,
            hasNpcElement ? ReadNpcAiBool(npcElement, ai => ai.InCombat) : string.Empty,
            hasNpcElement ? ReadNpcAiBool(npcElement, ai => ai.InAlert) : string.Empty,
            hasNpcElement ? ReadNpcAiBool(npcElement, ai => ai.InIdle) : string.Empty,
            hasNpcElement ? ReadNpcAiBool(npcElement, ai => ai.InFlee) : string.Empty,
            hasNpcElement ? ReadNpcAiFloat(npcElement, ai => ai.HeroVisibility) : string.Empty,
            hasNpcElement ? ReadNpcAiBool(npcElement, ai => ai.HeroVisible) : string.Empty,
            hasNpcElement ? ReadNpcAiFloat(npcElement, ai => ai.AlertValue) : string.Empty,
            hasNpcElement ? ReadNpcBool(npcElement, npc => npc.HasPerception) : string.Empty,
            hasNpcElement ? ReadNpcBool(npcElement, npc => npc.IsUnconscious) : string.Empty,
            FormatTargetName(currentTarget),
            FormatTargetType(currentTarget),
            FormatTargetTemplateGuid(currentTarget),
            FormatTargetDistance(location, currentTarget),
            BoolText(currentTarget is Hero),
            hasNpcElement ? CountPossibleTargets(npcElement) : string.Empty,
            hasNpcElement ? CountPossibleAttackers(npcElement) : string.Empty,
            hasNpcElement ? ReadHeroInPossibleTargets(npcElement) : string.Empty,
            hasNpcElement ? ReadHeroInPossibleAttackers(npcElement) : string.Empty,
            hasNpcElement ? ReadMovementStateName(npcElement) : string.Empty,
            hasNpcElement ? ReadNpcBool(npcElement, NpcCanMoveHandler.CanMove) : string.Empty,
            hasNpcElement ? ReadNpcBool(npcElement, NpcCanMoveHandler.CanOverrideDestination) : string.Empty,
            hasNpcElement ? ReadNpcBool(npcElement, NpcCanMoveHandler.ShouldResetMovementSpeed) : string.Empty,
            hasNpcElement ? ReadNpcBool(npcElement, npc => npc.HasElement<TargetOverrideElement>()) : string.Empty,
            hasNpcElement ? ReadNpcBool(npcElement, npc => npc.HasElement<HeroSummonTargetOverride>()) : string.Empty,
            factionHints,
            ownershipHints,
            GetWildTamingAiPackageLane(profile),
            GetWildTamingAiPackageMapping(profile),
            "kane.tgfoa.avalon-companions.ai-intent-profile",
            "Avalon Companions player tame interaction",
            "false",
            "false",
            "false",
            "false",
            "false",
            "false",
            "read-only wild encounter evidence with AI package handoff labels; AI packages do not perform tame conversion; no capture, adoption, faction change, ally attachment, target recalc, movement, persistence, or Core behavior");
    }

    private static string GetWildTamingAiPackageLane(CompanionTamingProfile profile)
    {
        if (string.Equals(profile.TemplateName, "Spec_AnimalWolf", StringComparison.OrdinalIgnoreCase)
            || string.Equals(profile.TemplateName, "Spec_AnimalBear", StringComparison.OrdinalIgnoreCase))
        {
            return "kane.tgfoa.tainted-instincts.ai-creature-profiles";
        }

        return "none";
    }

    private static string GetWildTamingAiPackageMapping(CompanionTamingProfile profile)
    {
        if (string.Equals(profile.TemplateName, "Spec_AnimalWolf", StringComparison.OrdinalIgnoreCase)
            || string.Equals(profile.TemplateName, "Spec_AnimalBear", StringComparison.OrdinalIgnoreCase))
        {
            return "policy-only wild exact-profile lane";
        }

        return "no reviewed wild AI package lane";
    }

    private static CompanionAiProfile CreateNoActiveCompanionProfile(string scene, int trackedCreatureCount)
    {
        return new CompanionAiProfile(
            CompanionAiIntent.NoActiveCompanion,
            "no active managed companion was found",
            scene,
            _companionMode,
            GetFollowRangeProfile(),
            activeRosterCount: 0,
            trackedCreatureCount: trackedCreatureCount,
            templateGuid: string.Empty,
            templateName: string.Empty,
            displayName: string.Empty,
            locationId: string.Empty,
            debugName: string.Empty,
            coords: string.Empty,
            distanceToHero: null,
            catchUpThreshold: null,
            heroLiveAttackers: CountLiveHeroAttackers(Hero.Current),
            possibleTargetCount: null,
            possibleAttackerCount: null,
            movementState: string.Empty,
            canMove: null,
            canOverrideDestination: null,
            npcAlive: null,
            npcWorking: null,
            npcInCombat: null,
            npcInIdle: null,
            hasNpcElement: false,
            hasTargetOverride: null,
            hasHeroSummonTargetOverride: null);
    }

    private static CompanionAiProfile BuildCompanionAiProfile(
        Location location,
        string scene,
        int activeRosterCount,
        int trackedCreatureCount)
    {
        bool discarded = location == null || location.HasBeenDiscarded;
        NpcElement? npcElement = null;
        bool hasNpcElement = !discarded && location!.TryGetElement(out npcElement) && npcElement != null;
        PetRosterEntry entry = !discarded ? GetRosterEntry(location!) : default;
        float? distanceToHero = null;
        if (!discarded && Hero.Current != null)
        {
            distanceToHero = Vector3.Distance(location!.Coords, Hero.Current.Coords);
        }

        float? catchUpThreshold = !discarded ? GetCreatureCatchUpDistance(entry) : null;
        int heroLiveAttackers = CountLiveHeroAttackers(Hero.Current);
        bool? npcAlive = TryReadNpcBoolValue(npcElement, npc => npc.IsAlive, out bool npcAliveValue) ? npcAliveValue : null;
        bool? npcWorking = TryReadNpcAiBoolValue(npcElement, ai => ai.Working, out bool npcWorkingValue) ? npcWorkingValue : null;
        bool? npcInCombat = TryReadNpcAiBoolValue(npcElement, ai => ai.InCombat, out bool npcInCombatValue) ? npcInCombatValue : null;
        bool? npcInIdle = TryReadNpcAiBoolValue(npcElement, ai => ai.InIdle, out bool npcInIdleValue) ? npcInIdleValue : null;
        bool? canMove = TryReadNpcBoolValue(npcElement, NpcCanMoveHandler.CanMove, out bool canMoveValue) ? canMoveValue : null;
        bool? canOverrideDestination = TryReadNpcBoolValue(npcElement, NpcCanMoveHandler.CanOverrideDestination, out bool canOverrideDestinationValue)
            ? canOverrideDestinationValue
            : null;
        bool? hasTargetOverride = TryReadNpcBoolValue(npcElement, npc => npc.HasElement<TargetOverrideElement>(), out bool hasTargetOverrideValue)
            ? hasTargetOverrideValue
            : null;
        bool? hasHeroSummonTargetOverride = TryReadNpcBoolValue(npcElement, npc => npc.HasElement<HeroSummonTargetOverride>(), out bool hasHeroSummonTargetOverrideValue)
            ? hasHeroSummonTargetOverrideValue
            : null;
        int? possibleTargetCount = TryCountPossibleTargetsValue(npcElement, out int possibleTargetCountValue)
            ? possibleTargetCountValue
            : null;
        int? possibleAttackerCount = TryCountPossibleAttackersValue(npcElement, out int possibleAttackerCountValue)
            ? possibleAttackerCountValue
            : null;
        string movementState = hasNpcElement ? ReadMovementStateName(npcElement) : string.Empty;
        bool followCatchUpCandidate = _companionMode != CompanionMode.Stay
            && distanceToHero.HasValue
            && catchUpThreshold.HasValue
            && distanceToHero.Value > catchUpThreshold.Value;

        CompanionAiIntent intent = ClassifyCompanionAiIntent(
            hasNpcElement,
            npcAlive,
            npcWorking,
            npcInCombat,
            npcInIdle,
            canMove,
            possibleTargetCount,
            possibleAttackerCount,
            heroLiveAttackers,
            movementState,
            followCatchUpCandidate,
            out string intentReason);

        return new CompanionAiProfile(
            intent,
            intentReason,
            scene,
            _companionMode,
            GetFollowRangeProfile(),
            activeRosterCount,
            trackedCreatureCount,
            discarded ? string.Empty : location!.Template?.GUID ?? string.Empty,
            discarded ? string.Empty : location!.Template?.name ?? string.Empty,
            discarded ? string.Empty : entry.DisplayName,
            discarded ? string.Empty : location!.ID ?? string.Empty,
            discarded ? string.Empty : location!.DebugName ?? string.Empty,
            discarded ? string.Empty : FormatVector(location!.Coords),
            distanceToHero,
            catchUpThreshold,
            heroLiveAttackers,
            possibleTargetCount,
            possibleAttackerCount,
            movementState,
            canMove,
            canOverrideDestination,
            npcAlive,
            npcWorking,
            npcInCombat,
            npcInIdle,
            hasNpcElement,
            hasTargetOverride,
            hasHeroSummonTargetOverride);
    }

    private static CompanionAiIntent ClassifyCompanionAiIntent(
        bool hasNpcElement,
        bool? npcAlive,
        bool? npcWorking,
        bool? npcInCombat,
        bool? npcInIdle,
        bool? canMove,
        int? possibleTargetCount,
        int? possibleAttackerCount,
        int heroLiveAttackers,
        string movementState,
        bool followCatchUpCandidate,
        out string intentReason)
    {
        if (!hasNpcElement)
        {
            intentReason = "evidence insufficient: no NpcElement";
            return CompanionAiIntent.EvidenceInsufficient;
        }

        if (npcAlive != true)
        {
            intentReason = $"evidence insufficient: npcAlive={FormatNullableBool(npcAlive)}";
            return CompanionAiIntent.EvidenceInsufficient;
        }

        if (npcWorking != true)
        {
            intentReason = $"evidence insufficient: npcWorking={FormatNullableBool(npcWorking)}";
            return CompanionAiIntent.EvidenceInsufficient;
        }

        if (npcInCombat == true || heroLiveAttackers > 0 || possibleTargetCount.GetValueOrDefault() > 0 || possibleAttackerCount.GetValueOrDefault() > 0)
        {
            intentReason = "native combat observed: "
                + $"npcInCombat={FormatNullableBool(npcInCombat)}; "
                + $"heroLiveAttackers={heroLiveAttackers}; "
                + $"possibleTargets={FormatNullableInt(possibleTargetCount)}; "
                + $"possibleAttackers={FormatNullableInt(possibleAttackerCount)}";
            return CompanionAiIntent.NativeCombatObserved;
        }

        if (canMove == false)
        {
            intentReason = $"movement blocked by native state: canMove={FormatNullableBool(canMove)}; movementState={movementState}";
            return CompanionAiIntent.MovementBlocked;
        }

        if (_companionMode == CompanionMode.Defend)
        {
            intentReason = "defend mode active and no native combat evidence is present";
            return CompanionAiIntent.DefendWaitingForThreat;
        }

        if (followCatchUpCandidate)
        {
            intentReason = "follow/catch-up candidate: distance exceeds current follow-range threshold";
            return CompanionAiIntent.FollowCatchUpCandidate;
        }

        if (_companionMode == CompanionMode.Stay)
        {
            intentReason = "stay mode active and no native combat evidence is present";
            return CompanionAiIntent.StayPosition;
        }

        if (string.Equals(movementState, "Patrol", StringComparison.Ordinal))
        {
            intentReason = $"native idle patrol observed with no combat evidence; npcInIdle={FormatNullableBool(npcInIdle)}";
            return CompanionAiIntent.IdleNativePatrol;
        }

        if (possibleTargetCount == 0 && possibleAttackerCount == 0)
        {
            intentReason = "no native targets or attackers observed";
            return CompanionAiIntent.NoTargets;
        }

        intentReason = "evidence insufficient: native movement/target state did not map to a supported intent";
        return CompanionAiIntent.EvidenceInsufficient;
    }

    private static void AppendCompanionAiProfileRow(
        StringBuilder builder,
        string localTime,
        string frame,
        string reason,
        CompanionAiProfile profile)
    {
        AppendLifecycleCsvRow(
            builder,
            localTime,
            frame,
            reason,
            profile.Scene,
            GetModeLabel(profile.Mode),
            GetFollowRangeLabel(profile.FollowRange),
            profile.ActiveRosterCount.ToString(CultureInfo.InvariantCulture),
            profile.TrackedCreatureCount.ToString(CultureInfo.InvariantCulture),
            profile.Intent.ToString(),
            profile.IntentReason,
            profile.TemplateGuid,
            profile.TemplateName,
            profile.DisplayName,
            profile.LocationId,
            profile.DebugName,
            profile.Coords,
            FormatNullableFloat(profile.DistanceToHero),
            FormatNullableFloat(profile.CatchUpThreshold),
            FormatNullableInt(profile.HeroLiveAttackers),
            FormatNullableInt(profile.PossibleTargetCount),
            FormatNullableInt(profile.PossibleAttackerCount),
            profile.MovementState,
            FormatNullableBool(profile.CanMove),
            FormatNullableBool(profile.CanOverrideDestination),
            FormatNullableBool(profile.NpcAlive),
            FormatNullableBool(profile.NpcWorking),
            FormatNullableBool(profile.NpcInCombat),
            FormatNullableBool(profile.NpcInIdle),
            BoolText(profile.HasNpcElement),
            FormatNullableBool(profile.HasTargetOverride),
            FormatNullableBool(profile.HasHeroSummonTargetOverride),
            "false",
            "false",
            "false",
            "false",
            "false",
            "read-only profile/intent classification; no command, movement, target, persistence, or Core behavior executed");
    }

    private static void AppendCompanionAiBoundaryRow(
        StringBuilder builder,
        string localTime,
        string frame,
        string reason,
        string scene,
        string heroCoords,
        string heroLiveAttackers,
        string mode,
        string followRange,
        string activeRosterCount,
        string trackedCreatureCount,
        Location location)
    {
        bool discarded = location == null || location.HasBeenDiscarded;
        NpcElement? npcElement = null;
        bool hasNpcElement = !discarded && location!.TryGetElement(out npcElement) && npcElement != null;
        ICharacter? currentTarget = hasNpcElement ? ReadCurrentTargetNoChecks(npcElement) : null;
        PetRosterEntry entry = !discarded ? GetRosterEntry(location!) : default;
        string notes = hasNpcElement
            ? "read-only; target is unchecked relation target; no target recalc or TargetOverrideElement.GetTarget call"
            : "no NpcElement";

        AppendLifecycleCsvRow(
            builder,
            localTime,
            frame,
            reason,
            scene,
            heroCoords,
            heroLiveAttackers,
            mode,
            followRange,
            activeRosterCount,
            trackedCreatureCount,
            discarded ? string.Empty : location!.Template?.GUID ?? string.Empty,
            discarded ? string.Empty : location!.Template?.name ?? string.Empty,
            discarded ? string.Empty : entry.DisplayName,
            discarded ? string.Empty : location!.ID ?? string.Empty,
            discarded ? string.Empty : location!.DebugName ?? string.Empty,
            discarded ? string.Empty : FormatVector(location!.Coords),
            discarded ? string.Empty : BoolText(location!.MarkedNotSaved),
            discarded ? string.Empty : BoolText(CreatureCandidateLocations.Contains(location!)),
            discarded ? string.Empty : BoolText(location!.TryGetElement(out PetElement _)),
            discarded ? string.Empty : BoolText(location!.TryGetElement(out PetVariantBase _)),
            BoolText(hasNpcElement),
            discarded ? string.Empty : BoolText(HasNativeHeroPetAlly(location!)),
            discarded ? string.Empty : BoolText(location!.HasElement<AvalonCompanionCommandAction>()),
            hasNpcElement ? ReadNpcBool(npcElement, npc => npc.IsAlive) : string.Empty,
            hasNpcElement ? ReadNpcAiBool(npcElement, ai => ai.Working) : string.Empty,
            hasNpcElement ? ReadNpcAiBool(npcElement, ai => ai.InCombat) : string.Empty,
            hasNpcElement ? ReadNpcAiBool(npcElement, ai => ai.InAlert) : string.Empty,
            hasNpcElement ? ReadNpcAiBool(npcElement, ai => ai.InIdle) : string.Empty,
            hasNpcElement ? ReadNpcAiBool(npcElement, ai => ai.InFlee) : string.Empty,
            hasNpcElement ? ReadNpcAiFloat(npcElement, ai => ai.HeroVisibility) : string.Empty,
            hasNpcElement ? ReadNpcAiBool(npcElement, ai => ai.HeroVisible) : string.Empty,
            hasNpcElement ? ReadNpcAiFloat(npcElement, ai => ai.AlertValue) : string.Empty,
            hasNpcElement ? ReadNpcBool(npcElement, npc => npc.HasPerception) : string.Empty,
            hasNpcElement ? ReadNpcBool(npcElement, npc => npc.IsUnconscious) : string.Empty,
            FormatTargetName(currentTarget),
            FormatTargetType(currentTarget),
            FormatTargetTemplateGuid(currentTarget),
            FormatTargetDistance(location, currentTarget),
            hasNpcElement ? CountPossibleTargets(npcElement) : string.Empty,
            hasNpcElement ? CountPossibleAttackers(npcElement) : string.Empty,
            hasNpcElement ? ReadMovementStateName(npcElement) : string.Empty,
            hasNpcElement ? ReadNpcBool(npcElement, NpcCanMoveHandler.CanMove) : string.Empty,
            hasNpcElement ? ReadNpcBool(npcElement, NpcCanMoveHandler.CanOverrideDestination) : string.Empty,
            hasNpcElement ? ReadNpcBool(npcElement, NpcCanMoveHandler.ShouldResetMovementSpeed) : string.Empty,
            hasNpcElement ? ReadNpcBool(npcElement, npc => npc.HasElement<TargetOverrideElement>()) : string.Empty,
            hasNpcElement ? ReadNpcBool(npcElement, npc => npc.HasElement<HeroSummonTargetOverride>()) : string.Empty,
            "false",
            "false",
            "false",
            "false",
            "false",
            notes);
    }

    private static ICharacter? ReadCurrentTargetNoChecks(NpcElement? npcElement)
    {
        if (npcElement == null)
        {
            return null;
        }

        try
        {
            return npcElement.GetCurrentTargetNoChecks();
        }
        catch
        {
            return null;
        }
    }

    private static string ReadNpcBool(NpcElement? npcElement, Func<NpcElement, bool> read)
    {
        return TryReadNpcBoolValue(npcElement, read, out bool value)
            ? BoolText(value)
            : npcElement == null ? string.Empty : "error";
    }

    private static string ReadNpcAiBool(NpcElement? npcElement, Func<NpcAI, bool> read)
    {
        if (npcElement == null)
        {
            return string.Empty;
        }

        try
        {
            NpcAI npcAi = npcElement.NpcAI;
            return npcAi == null ? string.Empty : BoolText(read(npcAi));
        }
        catch
        {
            return "error";
        }
    }

    private static bool TryReadNpcBoolValue(NpcElement? npcElement, Func<NpcElement, bool> read, out bool value)
    {
        value = false;
        if (npcElement == null)
        {
            return false;
        }

        try
        {
            value = read(npcElement);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool TryReadNpcAiBoolValue(NpcElement? npcElement, Func<NpcAI, bool> read, out bool value)
    {
        value = false;
        if (npcElement == null)
        {
            return false;
        }

        try
        {
            NpcAI npcAi = npcElement.NpcAI;
            if (npcAi == null)
            {
                return false;
            }

            value = read(npcAi);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static string ReadNpcAiFloat(NpcElement? npcElement, Func<NpcAI, float> read)
    {
        if (npcElement == null)
        {
            return string.Empty;
        }

        try
        {
            NpcAI npcAi = npcElement.NpcAI;
            return npcAi == null ? string.Empty : read(npcAi).ToString("0.###", CultureInfo.InvariantCulture);
        }
        catch
        {
            return "error";
        }
    }

    private static string ReadMovementStateName(NpcElement? npcElement)
    {
        if (npcElement == null)
        {
            return string.Empty;
        }

        try
        {
            if (!npcElement.TryGetElement(out NpcMovement movement) || movement == null)
            {
                return string.Empty;
            }

            return movement.CurrentState?.GetType().Name ?? string.Empty;
        }
        catch
        {
            return "error";
        }
    }

    private static string CountPossibleTargets(NpcElement? npcElement)
    {
        return TryCountPossibleTargetsValue(npcElement, out int count)
            ? count.ToString(CultureInfo.InvariantCulture)
            : npcElement == null ? string.Empty : "error";
    }

    private static string CountPossibleAttackers(ICharacter? character)
    {
        return TryCountPossibleAttackersValue(character, out int count)
            ? count.ToString(CultureInfo.InvariantCulture)
            : character == null ? string.Empty : "error";
    }

    private static bool TryCountPossibleTargetsValue(NpcElement? npcElement, out int count)
    {
        count = 0;
        if (npcElement == null)
        {
            return false;
        }

        try
        {
            foreach (ICharacter target in npcElement.PossibleTargets)
            {
                if (target != null && !target.HasBeenDiscarded)
                {
                    count++;
                }
            }

            return true;
        }
        catch
        {
            count = 0;
            return false;
        }
    }

    private static bool TryCountPossibleAttackersValue(ICharacter? character, out int count)
    {
        count = 0;
        if (character == null)
        {
            return false;
        }

        try
        {
            foreach (ICharacter attacker in character.PossibleAttackers)
            {
                if (attacker != null && !attacker.HasBeenDiscarded)
                {
                    count++;
                }
            }

            return true;
        }
        catch
        {
            count = 0;
            return false;
        }
    }

    private static string ReadHeroInPossibleTargets(NpcElement? npcElement)
    {
        if (npcElement == null || Hero.Current == null)
        {
            return string.Empty;
        }

        try
        {
            ICharacter hero = Hero.Current;
            foreach (ICharacter target in npcElement.PossibleTargets)
            {
                if (target != null && !target.HasBeenDiscarded && ReferenceEquals(target, hero))
                {
                    return "true";
                }
            }

            return "false";
        }
        catch
        {
            return "error";
        }
    }

    private static string ReadHeroInPossibleAttackers(NpcElement? npcElement)
    {
        if (npcElement == null || Hero.Current == null)
        {
            return string.Empty;
        }

        try
        {
            ICharacter hero = Hero.Current;
            foreach (ICharacter attacker in npcElement.PossibleAttackers)
            {
                if (attacker != null && !attacker.HasBeenDiscarded && ReferenceEquals(attacker, hero))
                {
                    return "true";
                }
            }

            return "false";
        }
        catch
        {
            return "error";
        }
    }

    private static string ReadCurrentDomain(Location location)
    {
        try
        {
            return location.CurrentDomain.ToString();
        }
        catch
        {
            return "error";
        }
    }

    private static string ReadNamedMemberHints(string source, object? value, params string[] memberNames)
    {
        if (value == null)
        {
            return string.Empty;
        }

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        Type type = value.GetType();
        List<string> hints = new List<string>();
        foreach (string memberName in memberNames)
        {
            try
            {
                PropertyInfo? property = type.GetProperty(memberName, flags);
                if (property != null && property.GetIndexParameters().Length == 0)
                {
                    object? propertyValue = property.GetValue(value);
                    if (propertyValue != null)
                    {
                        hints.Add($"{source}.{memberName}={FormatObjectBrief(propertyValue)}");
                    }

                    continue;
                }

                FieldInfo? field = type.GetField(memberName, flags);
                if (field != null)
                {
                    object? fieldValue = field.GetValue(value);
                    if (fieldValue != null)
                    {
                        hints.Add($"{source}.{memberName}={FormatObjectBrief(fieldValue)}");
                    }
                }
            }
            catch (Exception ex)
            {
                hints.Add($"{source}.{memberName}=error:{ex.GetType().Name}");
            }
        }

        return JoinDistinct(hints);
    }

    private static string FormatObjectBrief(object value)
    {
        if (value is string text)
        {
            return TruncateForCsv(text, 96);
        }

        if (value is Location location)
        {
            return TruncateForCsv($"{location.DebugName ?? location.ID ?? "<location>"}[{location.Template?.GUID ?? string.Empty}]", 96);
        }

        if (value is Hero)
        {
            return "Hero";
        }

        Type type = value.GetType();
        string label = TryReadObjectLabel(value, "GUID")
            ?? TryReadObjectLabel(value, "Id")
            ?? TryReadObjectLabel(value, "ID")
            ?? TryReadObjectLabel(value, "name")
            ?? TryReadObjectLabel(value, "Name")
            ?? TryReadObjectLabel(value, "DebugName")
            ?? TryReadObjectLabel(value, "DisplayName")
            ?? value.ToString()
            ?? type.Name;
        return TruncateForCsv($"{type.Name}:{label}", 96);
    }

    private static string? TryReadObjectLabel(object value, string memberName)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        try
        {
            Type type = value.GetType();
            PropertyInfo? property = type.GetProperty(memberName, flags);
            if (property != null && property.GetIndexParameters().Length == 0)
            {
                object? propertyValue = property.GetValue(value);
                if (propertyValue != null)
                {
                    return propertyValue.ToString();
                }
            }

            FieldInfo? field = type.GetField(memberName, flags);
            object? fieldValue = field?.GetValue(value);
            return fieldValue?.ToString();
        }
        catch
        {
            return null;
        }
    }

    private static string SafeObjectText(object? value)
    {
        if (value == null)
        {
            return string.Empty;
        }

        try
        {
            return TruncateForCsv(value.ToString() ?? string.Empty, 160);
        }
        catch
        {
            return "error";
        }
    }

    private static string TruncateForCsv(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
        {
            return value;
        }

        return value.Substring(0, Math.Max(0, maxLength - 3)) + "...";
    }

    private static string JoinDistinct(IEnumerable<string> values)
    {
        return string.Join(
            "|",
            values
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(value => value, StringComparer.OrdinalIgnoreCase));
    }

    private static string FormatTargetName(ICharacter? target)
    {
        if (target == null)
        {
            return string.Empty;
        }

        try
        {
            return target.HasBeenDiscarded ? "<discarded>" : target.Name ?? string.Empty;
        }
        catch
        {
            return "error";
        }
    }

    private static string FormatTargetType(ICharacter? target)
    {
        return target == null ? string.Empty : target.GetType().Name;
    }

    private static string FormatTargetTemplateGuid(ICharacter? target)
    {
        if (target is NpcElement npcTarget)
        {
            return npcTarget.ParentModel?.Template?.GUID ?? npcTarget.Template?.GUID ?? string.Empty;
        }

        return target is Hero ? "Hero" : string.Empty;
    }

    private static string FormatTargetDistance(Location? location, ICharacter? target)
    {
        if (location == null || target == null)
        {
            return string.Empty;
        }

        try
        {
            return target.HasBeenDiscarded
                ? string.Empty
                : Vector3.Distance(location.Coords, target.Coords).ToString("0.###", CultureInfo.InvariantCulture);
        }
        catch
        {
            return "error";
        }
    }

    private static string BoolText(bool value)
    {
        return value ? "true" : "false";
    }

    private static string FormatNullableBool(bool? value)
    {
        return value.HasValue ? BoolText(value.Value) : string.Empty;
    }

    private static string FormatNullableInt(int? value)
    {
        return value.HasValue ? value.Value.ToString(CultureInfo.InvariantCulture) : string.Empty;
    }

    private static string FormatNullableFloat(float? value)
    {
        return value.HasValue ? value.Value.ToString("0.###", CultureInfo.InvariantCulture) : string.Empty;
    }

    private static void LogAiBoundaryDiagnosticFailure(ManualLogSource logger, string phase, Exception ex)
    {
        if (_aiBoundaryDiagnosticFailureLogged)
        {
            return;
        }

        _aiBoundaryDiagnosticFailureLogged = true;
        logger.LogWarning($"Companion AI boundary diagnostic {phase} failed: {ex.GetType().Name}: {ex.Message}");
    }

    private static void LogAiProfileAuditFailure(ManualLogSource logger, string phase, Exception ex)
    {
        if (_aiProfileAuditFailureLogged)
        {
            return;
        }

        _aiProfileAuditFailureLogged = true;
        logger.LogWarning($"Companion AI profile audit {phase} failed: {ex.GetType().Name}: {ex.Message}");
    }

    private static void BlockCandidateCommand(ManualLogSource logger, PetRosterEntry selected, string command)
    {
        SetStatus(logger, $"{selected.DisplayName} {command} blocked: {selected.ReviewId}", warning: true);
        WriteCompanionCommandLog(logger, command, affectedCount: 0, blocked: true, reason: selected.BlockReason);
        logger.LogWarning(
            $"Candidate command blocked. Command={command}; Candidate={selected.DisplayName}/{selected.TemplateName}[{selected.Guid}]; "
            + $"Review={selected.ReviewId}; Reason={selected.BlockReason} No actor spawn, movement, faction, targeting, persistence, or save state was changed.");
    }

    private static void SetPanelVisible(bool visible, ManualLogSource? logger)
    {
        if (_panelVisible == visible)
        {
            return;
        }

        _panelVisible = visible;
        SetControllerCursorScopeActive(IsCompanionUiVisible);
        if (visible)
        {
            CaptureCursorForPanel();
            CapturePanelInputState();
            EnsurePanelPlacement(force: !_panelInitialized);
        }
        else if (!IsCompanionUiVisible)
        {
            RestorePanelInputState();
            RestoreCursorState();
        }

        if (logger != null)
        {
            SetStatus(logger, visible ? "Panel opened" : "Panel closed");
        }

        UpdateCursor();
    }

    private static void SetDialogueVisible(bool visible, ManualLogSource? logger, Location? location = null)
    {
        if (visible && location != null)
        {
            _dialogueLocation = location;
        }

        if (_dialogueVisible == visible)
        {
            if (visible)
            {
                CaptureCursorForPanel();
                EnsureDialogueUiHost(logger);
            }

            return;
        }

        _dialogueVisible = visible;
        if (!visible)
        {
            _dialogueLocation = null;
        }

        SetControllerCursorScopeActive(IsCompanionUiVisible);
        if (visible)
        {
            CaptureCursorForPanel();
            EnsureDialogueUiHost(logger);
        }
        else
        {
            DestroyDialogueUiHost();
            if (!IsCompanionUiVisible)
            {
                RestorePanelInputState();
                RestoreCursorState();
            }
        }

        if (logger != null)
        {
            SetStatus(logger, visible ? "Companion menu opened" : "Companion menu closed");
        }

        UpdateCursor();
    }

    private static void SetControllerCursorScopeActive(bool active)
    {
        if (_controllerCursorScopeActive == active)
        {
            return;
        }

        if (active)
        {
            _taintedInterfaceScopeActive = TaintedInterfaceBridge.BeginCustomUiScope(Plugin.PluginGuid, freezeWorld: true);
            if (!_taintedInterfaceScopeActive)
            {
                FoAModManagerBridge.SetCustomUiScope(Plugin.PluginGuid, active: true, freezeWorld: true);
            }

            _controllerCursorScopeActive = true;
            return;
        }

        if (_taintedInterfaceScopeActive)
        {
            TaintedInterfaceBridge.EndCustomUiScope(Plugin.PluginGuid);
            _taintedInterfaceScopeActive = false;
        }
        else
        {
            FoAModManagerBridge.SetCustomUiScope(Plugin.PluginGuid, active: false, freezeWorld: true);
        }

        _controllerCursorScopeActive = false;
    }

    private static void UpdateCursor()
    {
        if (IsCompanionUiVisible)
        {
            EnsureCursorForPanel();
            return;
        }

        RestoreCursorState();
    }

    private static void CaptureCursorForPanel()
    {
        if (!_cursorCaptured)
        {
            _cursorCaptured = true;
            _cursorWasVisible = Cursor.visible;
            _cursorWasLocked = Cursor.lockState;
        }

        EnsureCursorForPanel();
    }

    private static void EnsureCursorForPanel()
    {
        if (Cursor.lockState != CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.None;
        }

        if (!Cursor.visible)
        {
            Cursor.visible = true;
        }

        TaintedInterfaceBridge.EnsureInteractiveCursor();
    }

    private static void RestoreCursorState()
    {
        if (!_cursorCaptured)
        {
            return;
        }

        Cursor.visible = _cursorWasVisible;
        Cursor.lockState = _cursorWasLocked;
        _cursorCaptured = false;
    }

    private static void CapturePanelInputState()
    {
        if (_inputModulesCaptured)
        {
            EnsurePanelInputBlocked();
            return;
        }

        InputModuleStates.Clear();
        foreach (BaseInputModule module in UnityEngine.Object.FindObjectsByType<BaseInputModule>(FindObjectsSortMode.None))
        {
            InputModuleStates.Add(new InputModuleState(module, module.enabled));
            if (module.enabled)
            {
                module.enabled = false;
            }
        }

        _inputModulesCaptured = true;
        Input.ResetInputAxes();
    }

    private static void EnsurePanelInputBlocked()
    {
        if (!_inputModulesCaptured)
        {
            return;
        }

        for (int i = InputModuleStates.Count - 1; i >= 0; i--)
        {
            BaseInputModule? module = InputModuleStates[i].Module;
            if (module == null)
            {
                InputModuleStates.RemoveAt(i);
                continue;
            }

            if (module.enabled)
            {
                module.enabled = false;
            }
        }
    }

    private static void RestorePanelInputState()
    {
        if (!_inputModulesCaptured)
        {
            return;
        }

        foreach (InputModuleState state in InputModuleStates)
        {
            if (state.Module != null)
            {
                state.Module.enabled = state.WasEnabled;
            }
        }

        InputModuleStates.Clear();
        _inputModulesCaptured = false;
    }

    private static void TickCompanionLifecycleGuard(ManualLogSource logger)
    {
        if (!Plugin.EnableLifecycleSafetyGuard.Value && !Plugin.WriteCompanionLifecycleDump.Value)
        {
            return;
        }

        Hero hero = Hero.Current;
        if (hero == null)
        {
            return;
        }

        string sceneName = GetActiveSceneName();
        Vector3 heroCoords = hero.Coords;
        bool firstSample = !_hasLifecycleHeroCoords;
        bool sceneChanged = !firstSample && !string.Equals(_lastLifecycleScene, sceneName, StringComparison.Ordinal);
        bool longHeroMove = !firstSample && Vector3.Distance(_lastLifecycleHeroCoords, heroCoords) >= LifecycleLongHeroMoveMeters;
        bool periodic = Time.realtimeSinceStartup >= _nextLifecycleSafetyTick;

        if (!firstSample && !sceneChanged && !longHeroMove && !periodic)
        {
            return;
        }

        string reason = sceneChanged
            ? "scene-change"
            : longHeroMove
                ? "long-hero-move"
                : firstSample
                    ? "runtime-ready"
                    : "periodic";

        _hasLifecycleHeroCoords = true;
        _lastLifecycleHeroCoords = heroCoords;
        _lastLifecycleScene = sceneName;
        _nextLifecycleSafetyTick = Time.realtimeSinceStartup + LifecycleSafetyTickSeconds;

        RunCompanionLifecycleSafetyPass(logger, reason, forceDump: sceneChanged || longHeroMove);
    }

    private static void RunCompanionLifecycleSafetyPass(ManualLogSource logger, string reason, bool forceDump)
    {
        try
        {
            RemoveDiscardedCreatureCandidates();

            foreach (Location location in CreatureCandidateLocations.ToArray())
            {
                if (location != null && !location.HasBeenDiscarded)
                {
                    location.MarkedNotSaved = true;
                }
            }

            List<Location> activeRoster = GetPetLocations().Where(IsRosterPet).ToList();
            foreach (Location location in CollectRosterHeroPetAllyLocations(logger))
            {
                AddLocation(activeRoster, location);
            }

            Dictionary<Location, string> notes = new Dictionary<Location, string>();
            int untrackedRosterAllyCount = 0;
            int untrackedDiscarded = 0;
            int duplicatesDiscarded = 0;

            foreach (Location location in activeRoster.ToArray())
            {
                if (location == null || location.HasBeenDiscarded)
                {
                    continue;
                }

                location.MarkedNotSaved = true;

                if (IsCreatureCandidate(location)
                    && HasNativeHeroPetAlly(location)
                    && !CreatureCandidateLocations.Contains(location))
                {
                    untrackedRosterAllyCount++;
                    AddLifecycleNote(notes, location, "untracked one-session roster ally");
                    RemoveNativeCommandAction(location);

                    if (Plugin.EnableLifecycleSafetyGuard.Value)
                    {
                        location.MarkedNotSaved = true;
                        location.Discard();
                        untrackedDiscarded++;
                        AddLifecycleNote(notes, location, "discarded by lifecycle guard");
                        logger.LogWarning(
                            "Lifecycle guard discarded an untracked one-session roster ally "
                            + $"{location.Template?.name}[{location.Template?.GUID}] during {reason}. "
                            + "This prevents a reload/transition orphan from becoming persistent.");
                    }
                }
            }

            RemoveDiscardedCreatureCandidates();
            List<Location> remainingRoster = activeRoster
                .Where(location => location != null && !location.HasBeenDiscarded)
                .ToList();

            Location? keep = SelectLifecycleKeeper(remainingRoster);
            if (keep != null)
            {
                keep.MarkedNotSaved = true;
                AddLifecycleNote(notes, keep, "kept active roster actor");
            }

            foreach (Location location in remainingRoster)
            {
                if (keep == null || ReferenceEquals(location, keep))
                {
                    continue;
                }

                AddLifecycleNote(notes, location, "excess active roster actor");
                RemoveNativeCommandAction(location);
                location.MarkedNotSaved = true;

                if (Plugin.EnableLifecycleSafetyGuard.Value)
                {
                    location.Discard();
                    duplicatesDiscarded++;
                    AddLifecycleNote(notes, location, "discarded by lifecycle guard");
                    logger.LogWarning(
                        "Lifecycle guard discarded an excess active roster actor "
                        + $"{location.Template?.name}[{location.Template?.GUID}] during {reason}. "
                        + "Avalon Companions supports one active roster companion and does not persist squads.");
                }
            }

            RemoveDiscardedCreatureCandidates();

            bool shouldWriteDump = Plugin.WriteCompanionLifecycleDump.Value
                && (forceDump || activeRoster.Count > 0 || untrackedRosterAllyCount > 0 || duplicatesDiscarded > 0 || untrackedDiscarded > 0);
            UpdateLifecycleSummary(
                reason,
                activeRoster,
                untrackedRosterAllyCount,
                duplicatesDiscarded,
                untrackedDiscarded,
                shouldWriteDump);

            if (shouldWriteDump)
            {
                WriteCompanionLifecycleSnapshot(
                    logger,
                    reason,
                    activeRoster,
                    notes,
                    untrackedRosterAllyCount,
                    duplicatesDiscarded,
                    untrackedDiscarded);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning($"Companion lifecycle safety pass failed during {reason}: {ex.GetType().Name}: {ex.Message}");
        }
    }

    private static void UpdateLifecycleSummary(
        string reason,
        List<Location> activeRoster,
        int untrackedRosterAllyCount,
        int duplicatesDiscarded,
        int untrackedDiscarded,
        bool wroteDump)
    {
        int activeCount = activeRoster.Count(location => location != null && !location.HasBeenDiscarded);
        int trackedCount = CreatureCandidateLocations.Count(location => location != null && !location.HasBeenDiscarded);
        _lastLifecycleSummary =
            $"Lifecycle: {reason}; scene={GetActiveSceneName()}; active={activeCount}; tracked={trackedCount}; "
            + $"untracked={untrackedRosterAllyCount}; dupesDiscarded={duplicatesDiscarded}; untrackedDiscarded={untrackedDiscarded}; "
            + $"csv={(wroteDump ? "yes" : "no")}";
    }

    private static Location? SelectLifecycleKeeper(List<Location> activeRoster)
    {
        if (activeRoster.Count == 0)
        {
            return null;
        }

        PetRosterEntry selected = GetSelectedPet();
        return activeRoster.FirstOrDefault(location => IsTemplate(location, selected.Guid))
            ?? activeRoster.FirstOrDefault(location => CreatureCandidateLocations.Contains(location))
            ?? activeRoster.FirstOrDefault(location => location.TryGetElement(out PetElement _))
            ?? activeRoster.FirstOrDefault(location => location.TryGetElement(out PetVariantBase _))
            ?? activeRoster[0];
    }

    private static List<Location> CollectRosterHeroPetAllyLocations(ManualLogSource logger)
    {
        List<Location> locations = new List<Location>();
        foreach (NpcHeroPetAlly petAlly in World.All<NpcHeroPetAlly>().ToArraySlow())
        {
            if (petAlly == null || petAlly.HasBeenDiscarded)
            {
                continue;
            }

            Location? location = ParentLocation(petAlly, logger);
            if (location != null && !location.HasBeenDiscarded && IsRosterPet(location))
            {
                AddLocation(locations, location);
            }
        }

        return locations;
    }

    private static Location? ParentLocation(object value, ManualLogSource logger)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        string[] propertyNames =
        {
            "ParentModel",
            "GenericParentModel",
            "Parent",
            "Location",
        };

        Type type = value.GetType();
        foreach (string propertyName in propertyNames)
        {
            try
            {
                PropertyInfo? property = type.GetProperty(propertyName, flags);
                if (property?.GetValue(value) is Location location)
                {
                    return location;
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning($"Companion lifecycle guard could not read {type.FullName}.{propertyName}: {ex.GetType().Name}: {ex.Message}");
            }
        }

        return null;
    }

    private static bool HasNativeHeroPetAlly(Location location)
    {
        if (location == null || location.HasBeenDiscarded)
        {
            return false;
        }

        if (!location.TryGetElement(out NpcElement npcElement) || npcElement == null)
        {
            return false;
        }

        NpcHeroPetAlly petAlly = npcElement.TryGetElement<NpcHeroPetAlly>();
        return petAlly != null && !petAlly.HasBeenDiscarded;
    }

    private static void AddLifecycleNote(Dictionary<Location, string> notes, Location location, string note)
    {
        if (notes.TryGetValue(location, out string existing) && !string.IsNullOrWhiteSpace(existing))
        {
            notes[location] = existing + "|" + note;
            return;
        }

        notes[location] = note;
    }

    private static void WriteCompanionLifecycleSnapshot(
        ManualLogSource logger,
        string reason,
        List<Location> activeRoster,
        Dictionary<Location, string> notes,
        int untrackedRosterAllyCount,
        int duplicatesDiscarded,
        int untrackedDiscarded)
    {
        try
        {
            string folder = Path.Combine(Paths.ConfigPath, Plugin.PluginGuid);
            Directory.CreateDirectory(folder);
            string path = Path.Combine(folder, "companion-lifecycle.csv");
            bool writeHeader = !File.Exists(path) || new FileInfo(path).Length == 0;

            StringBuilder builder = new StringBuilder();
            if (writeHeader)
            {
                builder.AppendLine("localTime,frame,reason,scene,heroCoords,activeRosterCount,trackedCreatureCount,untrackedRosterAllyCount,duplicatesDiscarded,untrackedDiscarded,templateGuid,templateName,displayName,locationId,debugName,coords,markedNotSaved,hasPetElement,hasPetVariant,hasNpcHeroPetAlly,isTrackedCreature,hasCommandAction,notes");
            }

            string localTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
            string frame = Time.frameCount.ToString(CultureInfo.InvariantCulture);
            string scene = GetActiveSceneName();
            string heroCoords = Hero.Current == null ? string.Empty : FormatVector(Hero.Current.Coords);
            string activeRosterCount = activeRoster.Count.ToString(CultureInfo.InvariantCulture);
            string trackedCreatureCount = CreatureCandidateLocations.Count.ToString(CultureInfo.InvariantCulture);
            string untrackedRosterAllyCountText = untrackedRosterAllyCount.ToString(CultureInfo.InvariantCulture);
            string duplicatesDiscardedText = duplicatesDiscarded.ToString(CultureInfo.InvariantCulture);
            string untrackedDiscardedText = untrackedDiscarded.ToString(CultureInfo.InvariantCulture);

            if (activeRoster.Count == 0)
            {
                AppendLifecycleCsvRow(
                    builder,
                    localTime,
                    frame,
                    reason,
                    scene,
                    heroCoords,
                    activeRosterCount,
                    trackedCreatureCount,
                    untrackedRosterAllyCountText,
                    duplicatesDiscardedText,
                    untrackedDiscardedText,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    "no active roster actors");
            }
            else
            {
                foreach (Location location in activeRoster)
                {
                    notes.TryGetValue(location, out string note);
                    bool discarded = location.HasBeenDiscarded;
                    if (discarded)
                    {
                        note = string.IsNullOrWhiteSpace(note) ? "discarded=true" : note + "|discarded=true";
                    }

                    AppendLifecycleCsvRow(
                        builder,
                        localTime,
                        frame,
                        reason,
                        scene,
                        heroCoords,
                        activeRosterCount,
                        trackedCreatureCount,
                        untrackedRosterAllyCountText,
                        duplicatesDiscardedText,
                        untrackedDiscardedText,
                        location.Template?.GUID ?? string.Empty,
                        location.Template?.name ?? string.Empty,
                        GetRosterEntry(location).DisplayName,
                        location.ID ?? string.Empty,
                        location.DebugName ?? string.Empty,
                        FormatVector(location.Coords),
                        location.MarkedNotSaved ? "true" : "false",
                        !discarded && location.TryGetElement(out PetElement _) ? "true" : "false",
                        !discarded && location.TryGetElement(out PetVariantBase _) ? "true" : "false",
                        !discarded && HasNativeHeroPetAlly(location) ? "true" : "false",
                        !discarded && CreatureCandidateLocations.Contains(location) ? "true" : "false",
                        !discarded && location.HasElement<AvalonCompanionCommandAction>() ? "true" : "false",
                        note ?? string.Empty);
                }
            }

            File.AppendAllText(path, builder.ToString(), Encoding.UTF8);
        }
        catch (Exception ex)
        {
            if (!_lifecycleDumpFailureLogged)
            {
                _lifecycleDumpFailureLogged = true;
                logger.LogWarning($"Companion lifecycle CSV write failed: {ex.GetType().Name}: {ex.Message}");
            }
        }
    }

    private static void AppendLifecycleCsvRow(StringBuilder builder, params string[] values)
    {
        foreach (string value in values)
        {
            AppendCsv(builder, value);
        }

        builder.Length--;
        builder.AppendLine();
    }

    private static void AppendCsv(StringBuilder builder, string value)
    {
        value = (value ?? string.Empty).Replace("\r", " ").Replace("\n", " ");
        builder.Append('"');
        builder.Append(value.Replace("\"", "\"\""));
        builder.Append('"');
        builder.Append(',');
    }

    private static string GetActiveSceneName()
    {
        try
        {
            Scene scene = SceneManager.GetActiveScene();
            if (scene.IsValid())
            {
                return string.IsNullOrWhiteSpace(scene.name) ? "<unnamed-scene>" : scene.name;
            }
        }
        catch
        {
            return "<scene-error>";
        }

        return "<unknown-scene>";
    }

    private static string FormatVector(Vector3 value)
    {
        return string.Format(CultureInfo.InvariantCulture, "{0:0.###}|{1:0.###}|{2:0.###}", value.x, value.y, value.z);
    }

    private static List<Location> GetPetLocations()
    {
        List<Location> locations = new List<Location>();
        RemoveDiscardedCreatureCandidates();

        foreach (PetElement petElement in World.All<PetElement>().ToArraySlow())
        {
            AddLocation(locations, petElement.ParentModel);
        }

        foreach (PetVariantBase petVariant in World.All<PetVariantBase>().ToArraySlow())
        {
            AddLocation(locations, petVariant.ParentModel);
        }

        foreach (Location candidate in CreatureCandidateLocations)
        {
            AddLocation(locations, candidate);
        }

        return locations;
    }

    private static List<Location> GetActiveManagedRosterLocations(ManualLogSource logger)
    {
        List<Location> locations = GetPetLocations().Where(IsRosterPet).ToList();
        foreach (Location location in CollectRosterHeroPetAllyLocations(logger))
        {
            AddLocation(locations, location);
        }

        return locations
            .Where(location => location != null && !location.HasBeenDiscarded && IsRosterPet(location))
            .ToList();
    }

    private static void AddLocation(List<Location> locations, Location? location)
    {
        if (location != null && !location.HasBeenDiscarded && !locations.Contains(location))
        {
            locations.Add(location);
        }
    }

    private static void AddCreatureCandidateLocation(Location location, ManualLogSource logger)
    {
        if (!CreatureCandidateLocations.Contains(location))
        {
            CreatureCandidateLocations.Add(location);
        }

        NextCreatureCandidateDefendPromptAt.Remove(location);
        EnsureNativeCommandAction(location, logger);
    }

    private static void RemoveDiscardedCreatureCandidates()
    {
        CreatureCandidateLocations.RemoveAll(location => location == null || location.HasBeenDiscarded);
        foreach (Location location in NextCreatureCandidateDefendPromptAt.Keys.ToList())
        {
            if (location == null)
            {
                continue;
            }

            if (location.HasBeenDiscarded || !CreatureCandidateLocations.Contains(location))
            {
                NextCreatureCandidateDefendPromptAt.Remove(location);
                NativeCommandSurfaceKeys.Remove(location);
            }
        }

        foreach (Location? location in NativeCommandSurfaceKeys.Keys.ToList())
        {
            if (location == null)
            {
                continue;
            }

            if (location.HasBeenDiscarded || !CreatureCandidateLocations.Contains(location))
            {
                NativeCommandSurfaceKeys.Remove(location);
            }
        }
    }

    private static void EnsureNativeCommandActions(ManualLogSource logger)
    {
        RemoveDiscardedCreatureCandidates();
        foreach (Location location in CreatureCandidateLocations.ToArray())
        {
            if (IsManagedNativeInteractionCompanion(location))
            {
                EnsureNativeCommandAction(location, logger);
            }
            else
            {
                RemoveNativeCommandAction(location);
                CreatureCandidateLocations.Remove(location);
            }
        }
    }

    private static void EnsureNativeCommandAction(Location location, ManualLogSource? logger)
    {
        if (!IsManagedNativeInteractionCompanion(location))
        {
            return;
        }

        bool useMenu = ShouldUseNativeCommandMenu();
        bool useQuickCommands = ShouldUseNativeQuickCommands();
        string mode = useMenu ? "Dialogue" : useQuickCommands ? "QuickCommands" : "None";
        bool changed = false;

        if (useMenu)
        {
            changed |= RemoveNativeQuickCommandActions(location);
            if (!location.HasElement<AvalonCompanionCommandAction>())
            {
                AvalonCompanionCommandAction action = location.AddElement(new AvalonCompanionCommandAction());
                action.MarkedNotSaved = true;
                changed = true;
            }
        }
        else if (useQuickCommands)
        {
            if (location.HasElement<AvalonCompanionCommandAction>())
            {
                location.RemoveElementsOfType<AvalonCompanionCommandAction>();
                changed = true;
            }

            changed |= EnsureNativeQuickCommandActions(location);
        }
        else
        {
            if (location.HasElement<AvalonCompanionCommandAction>())
            {
                location.RemoveElementsOfType<AvalonCompanionCommandAction>();
                changed = true;
            }

            changed |= RemoveNativeQuickCommandActions(location);
        }

        location.MarkedNotSaved = true;
        string surfaceKey = $"{location.Template?.GUID ?? string.Empty}|{mode}";
        bool firstSeen = !NativeCommandSurfaceKeys.TryGetValue(location, out string existingSurfaceKey)
            || !string.Equals(existingSurfaceKey, surfaceKey, StringComparison.Ordinal);
        NativeCommandSurfaceKeys[location] = surfaceKey;
        if (changed || firstSeen)
        {
            logger?.LogInfo($"Native companion command actions synchronized for {location.Template?.name}[{location.Template?.GUID}]. Runtime-only; not saved. Mode={mode}; changed={changed}.");
        }
    }

    private static bool EnsureNativeQuickCommandActions(Location location)
    {
        bool changed = false;
        if (!location.HasElement<AvalonCompanionFollowAction>())
        {
            location.AddElement(new AvalonCompanionFollowAction()).MarkedNotSaved = true;
            changed = true;
        }

        if (!location.HasElement<AvalonCompanionStayAction>())
        {
            location.AddElement(new AvalonCompanionStayAction()).MarkedNotSaved = true;
            changed = true;
        }

        if (!location.HasElement<AvalonCompanionDefendAction>())
        {
            location.AddElement(new AvalonCompanionDefendAction()).MarkedNotSaved = true;
            changed = true;
        }

        if (!location.HasElement<AvalonCompanionComeCloseAction>())
        {
            location.AddElement(new AvalonCompanionComeCloseAction()).MarkedNotSaved = true;
            changed = true;
        }

        if (!location.HasElement<AvalonCompanionRecallAction>())
        {
            location.AddElement(new AvalonCompanionRecallAction()).MarkedNotSaved = true;
            changed = true;
        }

        if (!location.HasElement<AvalonCompanionRecoverAction>())
        {
            location.AddElement(new AvalonCompanionRecoverAction()).MarkedNotSaved = true;
            changed = true;
        }

        if (!location.HasElement<AvalonCompanionDismissAction>())
        {
            location.AddElement(new AvalonCompanionDismissAction()).MarkedNotSaved = true;
            changed = true;
        }

        return changed;
    }

    private static void RemoveNativeCommandAction(Location location)
    {
        if (location == null || location.HasBeenDiscarded)
        {
            return;
        }

        location.RemoveElementsOfType<AvalonCompanionCommandAction>();
        RemoveNativeQuickCommandActions(location);
        NativeCommandSurfaceKeys.Remove(location);
    }

    private static bool RemoveNativeQuickCommandActions(Location location)
    {
        if (location == null || location.HasBeenDiscarded)
        {
            return false;
        }

        bool changed = false;
        if (location.HasElement<AvalonCompanionFollowAction>())
        {
            location.RemoveElementsOfType<AvalonCompanionFollowAction>();
            changed = true;
        }

        if (location.HasElement<AvalonCompanionStayAction>())
        {
            location.RemoveElementsOfType<AvalonCompanionStayAction>();
            changed = true;
        }

        if (location.HasElement<AvalonCompanionDefendAction>())
        {
            location.RemoveElementsOfType<AvalonCompanionDefendAction>();
            changed = true;
        }

        if (location.HasElement<AvalonCompanionComeCloseAction>())
        {
            location.RemoveElementsOfType<AvalonCompanionComeCloseAction>();
            changed = true;
        }

        if (location.HasElement<AvalonCompanionRecallAction>())
        {
            location.RemoveElementsOfType<AvalonCompanionRecallAction>();
            changed = true;
        }

        if (location.HasElement<AvalonCompanionRecoverAction>())
        {
            location.RemoveElementsOfType<AvalonCompanionRecoverAction>();
            changed = true;
        }

        if (location.HasElement<AvalonCompanionDismissAction>())
        {
            location.RemoveElementsOfType<AvalonCompanionDismissAction>();
            changed = true;
        }

        return changed;
    }

    private static void RemoveAllNativeCommandActions()
    {
        foreach (Location location in CreatureCandidateLocations.ToArray())
        {
            RemoveNativeCommandAction(location);
        }

        NativeCommandSurfaceKeys.Clear();
    }

    private static bool IsManagedNativeInteractionCompanion(Location location)
    {
        if (location == null || location.HasBeenDiscarded || !CreatureCandidateLocations.Contains(location) || !IsCreatureCandidate(location))
        {
            return false;
        }

        if (!location.TryGetElement(out NpcElement npcElement) || npcElement == null)
        {
            return false;
        }

        NpcHeroPetAlly petAlly = npcElement.TryGetElement<NpcHeroPetAlly>();
        return petAlly != null && !petAlly.HasBeenDiscarded;
    }

    private static bool IsRosterPet(Location location)
    {
        return Roster.Any(entry => entry.RuntimeApproved && IsTemplate(location, entry.Guid));
    }

    private static bool IsCreatureCandidate(Location location)
    {
        return Roster.Any(entry => entry.RuntimeApproved && !entry.RequiresPetComponent && IsTemplate(location, entry.Guid));
    }

    private static bool IsTemplate(Location location, string guid)
    {
        return string.Equals(location.Template?.GUID, guid, StringComparison.OrdinalIgnoreCase);
    }

    private static string GetActiveRosterPetName()
    {
        List<Location> rosterPets = GetPetLocations().Where(IsRosterPet).ToList();
        if (rosterPets.Count == 0)
        {
            return "None";
        }

        List<string> names = rosterPets
            .Select(location => Roster.FirstOrDefault(entry => IsTemplate(location, entry.Guid)).DisplayName)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (names.Count == 0)
        {
            return $"{rosterPets.Count} roster pet(s)";
        }

        return string.Join(", ", names);
    }

    private static string GetLifecycleStatusLine(bool runtimeReady)
    {
        if (!runtimeReady)
        {
            return "Lifecycle: unavailable until runtime is ready";
        }

        return _lastLifecycleSummary;
    }

    private static void RunManualLifecycleCheck(ManualLogSource logger)
    {
        RunCompanionLifecycleSafetyPass(logger, "panel-lifecycle-check", forceDump: true);
        int activeCount = GetPetLocations().Count(IsRosterPet);
        SetStatus(logger, "Lifecycle check complete; snapshot written.");
        WriteCompanionCommandLog(logger, "lifecycle-check", affectedCount: activeCount, blocked: false, reason: _lastLifecycleSummary);
        WriteCommandPolicyAudit(
            logger,
            GetSelectedPet(),
            EvaluateCommandPolicy(GetSelectedPet(), CompanionCommandPolicyAction.LifecycleCheck),
            "lifecycle-check",
            "debug-panel",
            activeCount);
        TouchCoreRuntimeProfiles(
            logger,
            GetPetLocations().Where(IsRosterPet),
            "lifecycle-check",
            _lastLifecycleSummary + "; runtime profiles remain one-session only",
            CompanionRuntimeProfileState.Active);
        QueueResponsiveNativeRefresh();
    }

    private static void RunManualAiBoundaryCheck(ManualLogSource logger)
    {
        if (!Plugin.WriteCompanionAiBoundaryDiagnostics.Value)
        {
            const string reason = "Diagnostics.WriteCompanionAiBoundaryDiagnostics=false";
            SetStatus(logger, "AI boundary check unavailable: enable diagnostic config.", warning: true);
            WriteCompanionCommandLog(logger, "ai-boundary-check", affectedCount: 0, blocked: true, reason: reason);
            return;
        }

        List<Location> activeRoster = CollectAiBoundaryDiagnosticLocations(logger);
        if (activeRoster.Count == 0)
        {
            const string reason = "no active managed companion";
            SetStatus(logger, "AI boundary check skipped: no active managed companion.", warning: true);
            WriteCompanionCommandLog(logger, "ai-boundary-check", affectedCount: 0, blocked: true, reason: reason);
            return;
        }

        WriteCompanionAiBoundarySnapshot(logger, "panel-ai-boundary-check", activeRoster);
        string message = $"AI boundary check wrote {activeRoster.Count} row(s).";
        SetStatus(logger, message);
        WriteCompanionCommandLog(
            logger,
            "ai-boundary-check",
            affectedCount: activeRoster.Count,
            blocked: false,
            reason: "manual read-only AI boundary snapshot; touchesCommands=false; touchesMovement=false; touchesTargeting=false; touchesPersistence=false; coreExecuted=false");
    }

    private static void RunManualAiProfileCheck(ManualLogSource logger)
    {
        if (!Plugin.WriteCompanionAiProfileAudit.Value)
        {
            const string reason = "Diagnostics.WriteCompanionAiProfileAudit=false";
            SetStatus(logger, "AI profile check unavailable: enable audit config.", warning: true);
            WriteCompanionCommandLog(logger, "ai-profile-check", affectedCount: 0, blocked: true, reason: reason);
            return;
        }

        List<Location> activeRoster = CollectAiBoundaryDiagnosticLocations(logger);
        WriteCompanionAiProfileSnapshot(logger, "panel-ai-profile-check", activeRoster);

        int rowCount = Math.Max(1, activeRoster.Count);
        string message = activeRoster.Count == 0
            ? "AI profile check wrote 1 NoActiveCompanion row."
            : $"AI profile check wrote {activeRoster.Count} row(s).";
        SetStatus(logger, message);
        WriteCompanionCommandLog(
            logger,
            "ai-profile-check",
            affectedCount: activeRoster.Count,
            blocked: false,
            reason: $"manual read-only AI profile snapshot; rows={rowCount}; touchesCommands=false; touchesMovement=false; touchesTargeting=false; touchesPersistence=false; coreExecuted=false");
    }

    private static void RunManualProgressionPersistenceGateCheck(ManualLogSource logger)
    {
        PetRosterEntry selected = GetSelectedPet();
        CompanionCoreRuntimeProfile profile = GetOrCreateCoreRuntimeProfile(selected);
        CompanionTrustProfile trust = profile.TrustProfile;
        CompanionBondPolicyDecision policy = GetCompanionBondPolicy(profile);
        int activeCount = GetPetLocations().Count(IsRosterPet);
        string reason =
            "gate=progression-persistence; decision=profile-data-only; schemaVersion=1; "
            + $"storeEnabled={Plugin.EnableCompanionProfileStore.Value.ToString().ToLowerInvariant()}; futureStoreFile={CompanionProfileStoreFileName}; "
            + $"selected={selected.DisplayName}[{selected.Guid}]; reviewId={selected.ReviewId}; activeRosterCount={activeCount}; "
            + $"trustRuntimeEnabled={Plugin.EnableCompanionTrustRuntime.Value.ToString().ToLowerInvariant()}; "
            + $"bondPolicyEnabled={Plugin.EnableCompanionBondPolicyRuntime.Value.ToString().ToLowerInvariant()}; "
            + $"sampleTrust={trust.TrustScore}({trust.TrustLevel}); sampleLoyalty={trust.LoyaltyScore}({trust.LoyaltyLevel}); "
            + $"sampleBond={policy.BondLevel}; sampleLastEvent={trust.LastEvent}; "
            + "allowedFields=profileSchemaVersion,templateGuid,templateName,displayName,reviewId,requiresPetComponent,runtimeApproved,"
            + "trustScore,loyaltyScore,trustLevel,loyaltyLevel,lastTrustEvent,lastKnownMode,lastKnownFollowRange,effectiveBondLevel; "
            + "blockedFields=locationId,actorInstanceId,scene,coords,health,npcState,nativeActions,targetState,saveOwnedActor,"
            + "autoRespawn,reloadRestoration,actorReAdoption; "
            + "noActorRestore=true; noSaveOwnership=true; noAutoRespawn=true; "
            + "touchesCommands=false; touchesMovement=false; touchesTargeting=false; touchesPersistence=false; coreExecuted=false";
        SetStatus(logger, "Progression gate logged: profile data only; no actor restore.");
        WriteCompanionCommandLog(logger, "progression-persistence-gate", affectedCount: activeCount, blocked: false, reason: reason);
    }

    private static void RunManualTamingEligibilityGateCheck(ManualLogSource logger)
    {
        PetRosterEntry selected = GetSelectedPet();
        CompanionTamingProfile selectedProfile = BuildCompanionTamingProfile(selected);
        List<CompanionTamingProfile> profiles = Roster.Select(BuildCompanionTamingProfile).ToList();
        string reason =
            "gate=taming-eligibility; decision=read-only-profile-definitions; "
            + $"selected={selectedProfile.DisplayName}[{selectedProfile.TemplateGuid}]; reviewId={selectedProfile.ReviewId}; "
            + $"selectedEligibility={GetTamingEligibilityLabel(selectedProfile.Eligibility)}; "
            + $"selectedRole={selectedProfile.CombatRole}; selectedTemperament={selectedProfile.Temperament}; "
            + $"selectedReason={selectedProfile.Reason}; rosterCount={profiles.Count}; counts={FormatTamingEligibilityCounts(profiles)}; "
            + "classes=tameable candidate,already-companion,unsafe hostile,undead/blocked,passive-only,evidence insufficient; "
            + "noCapture=true; noWildActorAdoption=true; noActorPersistence=true; noActorRestore=true; noAutoRespawn=true; "
            + "noCommandBehaviorChange=true; noSaveOwnership=true; noLocationHealthAiTargetState=true; "
            + "touchesCommands=false; touchesMovement=false; touchesTargeting=false; touchesPersistence=false; coreExecuted=false";
        SetStatus(logger, $"Taming gate logged: {GetTamingEligibilityLabel(selectedProfile.Eligibility)}.");
        WriteCompanionCommandLog(logger, "taming-eligibility-gate", affectedCount: profiles.Count, blocked: false, reason: reason);
    }

    private static void RunManualTamingEncounterProbe(ManualLogSource logger)
    {
        try
        {
            List<Location> candidates = CollectTamingEncounterCandidates();
            WriteTamingEncounterSnapshot(logger, "panel-taming-encounter-probe", candidates);
            string reason =
                "gate=taming-encounter-evidence; decision=read-only-live-actor-probe; "
                + $"scanRadius={GetTamingEncounterProbeRadius():0.###}; candidateCount={candidates.Count}; "
                + "fields=templateIdentity,hostility,targetState,factionHints,ownershipHints,aiState,saveBehavior; "
                + "noCapture=true; noWildActorAdoption=true; noFactionChange=true; noNpcHeroPetAllyAttachment=true; "
                + "noSpawn=true; noDismiss=true; noActorPersistence=true; noCommandBehaviorChange=true; "
                + "touchesCommands=false; touchesMovement=false; touchesTargeting=false; touchesPersistence=false; coreExecuted=false";
            string message = candidates.Count == 0
                ? "Encounter probe wrote 1 no-candidate row."
                : $"Encounter probe wrote {candidates.Count} candidate row(s).";
            SetStatus(logger, message);
            WriteCompanionCommandLog(logger, "taming-encounter-probe", affectedCount: candidates.Count, blocked: false, reason: reason);
        }
        catch (Exception ex)
        {
            const string reason = "taming encounter probe failed before writing complete evidence";
            SetStatus(logger, "Encounter probe failed; see BepInEx log.", warning: true);
            WriteCompanionCommandLog(logger, "taming-encounter-probe", affectedCount: 0, blocked: true, reason: reason);
            if (!_tamingEncounterProbeFailureLogged)
            {
                _tamingEncounterProbeFailureLogged = true;
                logger.LogWarning($"Taming encounter probe failed: {ex.GetType().Name}: {ex.Message}");
            }
        }
    }

    private static PetRosterEntry GetRosterEntry(Location location)
    {
        return Roster.FirstOrDefault(entry => IsTemplate(location, entry.Guid));
    }

    private static bool TryGetRosterEntry(Location location, out PetRosterEntry entry)
    {
        foreach (PetRosterEntry candidate in Roster)
        {
            if (IsTemplate(location, candidate.Guid))
            {
                entry = candidate;
                return true;
            }
        }

        entry = default;
        return false;
    }

    private static int IndexOfRosterEntry(string guid)
    {
        for (int i = 0; i < Roster.Length; i++)
        {
            if (string.Equals(Roster[i].Guid, guid, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        return -1;
    }

    private static PetRosterEntry GetSelectedPet()
    {
        int index = NormalizeIndex(Plugin.SelectedPetIndex.Value);
        if (Plugin.SelectedPetIndex.Value != index)
        {
            Plugin.SelectedPetIndex.Value = index;
        }

        return Roster[index];
    }

    private static int NormalizeIndex(int index)
    {
        int count = Roster.Length;
        return ((index % count) + count) % count;
    }

    private static void EnsurePanelPlacement(bool force)
    {
        if (!force && _panelInitialized && _lastPanelScreenWidth == Screen.width && _lastPanelScreenHeight == Screen.height)
        {
            _panelRect = ClampPanelToScreen(_panelRect);
            return;
        }

        float maxWidth = GetMaximumPanelWidth();
        float maxHeight = GetMaximumPanelHeight();
        float width = Mathf.Min(maxWidth, Mathf.Max(MinimumPanelWidth, Screen.width * 0.23f));
        float height = Mathf.Min(maxHeight, Mathf.Max(MinimumPanelHeight, Screen.height * 0.30f));
        float rightInset = Mathf.Max(PanelScreenMargin, Screen.width * 0.035f);
        float topInset = Mathf.Max(96f, Screen.height * 0.09f);

        _panelRect = ClampPanelToScreen(new Rect(
            Screen.width - width - rightInset,
            topInset,
            width,
            height));

        _lastPanelScreenWidth = Screen.width;
        _lastPanelScreenHeight = Screen.height;
        _panelInitialized = true;
    }

    private static void EnsureDialoguePlacement(bool force)
    {
        if (!force && _dialogueInitialized && _lastDialogueScreenWidth == Screen.width && _lastDialogueScreenHeight == Screen.height)
        {
            _dialogueRect = ClampDialogueToScreen(_dialogueRect);
            return;
        }

        float maxWidth = GetMaximumDialogueWidth();
        float maxHeight = GetMaximumDialogueHeight();
        float width = Mathf.Min(maxWidth, Mathf.Clamp(Screen.width * 0.42f, MinimumDialogueWidth, 860f));
        float height = Mathf.Min(maxHeight, Mathf.Clamp(Screen.height * 0.52f, MinimumDialogueHeight, 680f));
        float bottomInset = Mathf.Max(96f, Screen.height * 0.10f);

        _dialogueRect = ClampDialogueToScreen(new Rect(
            (Screen.width - width) * 0.5f,
            Screen.height - height - bottomInset,
            width,
            height));

        _lastDialogueScreenWidth = Screen.width;
        _lastDialogueScreenHeight = Screen.height;
        _dialogueInitialized = true;
    }

    private static Rect ClampPanelToScreen(Rect panel)
    {
        float maxWidth = GetMaximumPanelWidth();
        float maxHeight = GetMaximumPanelHeight();
        float width = Mathf.Clamp(panel.width, MinimumPanelWidth, maxWidth);
        float height = Mathf.Clamp(panel.height, MinimumPanelHeight, maxHeight);
        float maxX = Mathf.Max(PanelScreenMargin, Screen.width - width - PanelScreenMargin);
        float maxY = Mathf.Max(PanelScreenMargin, Screen.height - height - PanelScreenMargin);
        float x = Mathf.Clamp(panel.x, PanelScreenMargin, maxX);
        float y = Mathf.Clamp(panel.y, PanelScreenMargin, maxY);

        return new Rect(x, y, width, height);
    }

    private static Rect ClampDialogueToScreen(Rect panel)
    {
        float maxWidth = GetMaximumDialogueWidth();
        float maxHeight = GetMaximumDialogueHeight();
        float width = Mathf.Clamp(panel.width, MinimumDialogueWidth, maxWidth);
        float height = Mathf.Clamp(panel.height, MinimumDialogueHeight, maxHeight);
        float maxX = Mathf.Max(PanelScreenMargin, Screen.width - width - PanelScreenMargin);
        float maxY = Mathf.Max(PanelScreenMargin, Screen.height - height - PanelScreenMargin);
        float x = Mathf.Clamp(panel.x, PanelScreenMargin, maxX);
        float y = Mathf.Clamp(panel.y, PanelScreenMargin, maxY);

        return new Rect(x, y, width, height);
    }

    private static float GetMaximumPanelWidth()
    {
        return Mathf.Max(MinimumPanelWidth, Screen.width - PanelScreenMargin * 2f);
    }

    private static float GetMaximumPanelHeight()
    {
        return Mathf.Max(MinimumPanelHeight, Screen.height - PanelScreenMargin * 2f);
    }

    private static float GetMaximumDialogueWidth()
    {
        return Mathf.Max(MinimumDialogueWidth, Screen.width - PanelScreenMargin * 2f);
    }

    private static float GetMaximumDialogueHeight()
    {
        return Mathf.Max(MinimumDialogueHeight, Screen.height - PanelScreenMargin * 2f);
    }

    private static void EnsureStyles()
    {
        if (_panelTexture != null)
        {
            return;
        }

        if (TaintedInterfaceBridge.TryGetStyles(out TaintedInterfaceBridge.SharedStyles? sharedStyles) && sharedStyles != null)
        {
            ApplySharedStyles(sharedStyles);
            if (!_sharedStylesLogged && _logger != null)
            {
                _sharedStylesLogged = true;
                _logger.LogInfo("Avalon Companions using Tainted Interface shared styles and scope bridge.");
            }

            return;
        }

        SharedPanelTextures.Clear();

        _panelTexture = MakeTexture(new Color(0.055f, 0.065f, 0.072f, 0.94f));
        _panelAltTexture = MakeTexture(new Color(0.10f, 0.12f, 0.13f, 0.92f));
        _rowTexture = MakeTexture(new Color(0.07f, 0.09f, 0.10f, 0.95f));
        _buttonTexture = MakeTexture(new Color(0.18f, 0.25f, 0.25f, 0.98f));
        _buttonHoverTexture = MakeTexture(new Color(0.25f, 0.34f, 0.33f, 0.98f));
        _selectedButtonTexture = MakeTexture(new Color(0.40f, 0.31f, 0.16f, 0.98f));
        _dangerButtonTexture = MakeTexture(new Color(0.34f, 0.14f, 0.12f, 0.98f));
        _dangerHoverTexture = MakeTexture(new Color(0.46f, 0.18f, 0.15f, 0.98f));

        _windowStyle = new GUIStyle(GUI.skin.window)
        {
            padding = new RectOffset(18, 18, 18, 18),
            border = new RectOffset(8, 8, 8, 8),
        };
        ApplyBackground(_windowStyle, _panelTexture!, new Color(0.92f, 0.90f, 0.82f, 1f));

        _headerStyle = new GUIStyle(GUI.skin.box)
        {
            padding = new RectOffset(16, 16, 12, 12),
            margin = new RectOffset(0, 0, 0, 0),
        };
        ApplyBackground(_headerStyle, _panelAltTexture!, Color.white);

        _sectionStyle = new GUIStyle(GUI.skin.box)
        {
            padding = new RectOffset(16, 16, 12, 12),
            margin = new RectOffset(0, 0, 0, 0),
        };
        ApplyBackground(_sectionStyle, _panelAltTexture!, Color.white);

        _footerStyle = new GUIStyle(GUI.skin.box)
        {
            padding = new RectOffset(14, 14, 10, 10),
            margin = new RectOffset(0, 0, 0, 0),
        };
        ApplyBackground(_footerStyle, _rowTexture!, Color.white);

        _titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 25,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,
            wordWrap = false,
        };
        _titleStyle.normal.textColor = new Color(0.96f, 0.91f, 0.78f, 1f);

        _labelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 18,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,
            wordWrap = false,
            clipping = TextClipping.Clip
        };
        _labelStyle.normal.textColor = Color.white;

        _mutedStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,
            wordWrap = false,
            clipping = TextClipping.Clip
        };
        _mutedStyle.normal.textColor = new Color(0.78f, 0.86f, 0.84f, 1f);

        _statusStyle = new GUIStyle(_mutedStyle)
        {
            fontSize = 16,
            wordWrap = true,
        };
        _statusStyle.normal.textColor = new Color(0.90f, 0.88f, 0.78f, 1f);

        _buttonStyle = BuildButtonStyle(_buttonTexture, _buttonHoverTexture);
        _selectedButtonStyle = BuildButtonStyle(_selectedButtonTexture, _buttonHoverTexture);
        _dangerButtonStyle = BuildButtonStyle(_dangerButtonTexture, _dangerHoverTexture);
    }

    private static void ApplySharedStyles(TaintedInterfaceBridge.SharedStyles sharedStyles)
    {
        SharedPanelTextures.Clear();

        _panelTexture = SharedBackground(sharedStyles.Window) ?? MakeTexture(new Color(0.055f, 0.065f, 0.072f, 0.94f));
        _panelAltTexture = SharedBackground(sharedStyles.Panel) ?? SharedBackground(sharedStyles.Card) ?? _panelTexture;
        _rowTexture = SharedBackground(sharedStyles.Card) ?? _panelAltTexture;
        _buttonTexture = SharedBackground(sharedStyles.Button) ?? MakeTexture(new Color(0.18f, 0.25f, 0.25f, 0.98f));
        _buttonHoverTexture = RegisterSharedTexture(sharedStyles.Button.hover.background) ?? _buttonTexture;
        _selectedButtonTexture = SharedBackground(sharedStyles.SecondaryButton) ?? _buttonHoverTexture;
        _dangerButtonTexture = MakeTexture(new Color(0.34f, 0.14f, 0.12f, 0.98f));
        _dangerHoverTexture = MakeTexture(new Color(0.46f, 0.18f, 0.15f, 0.98f));

        _windowStyle = new GUIStyle(sharedStyles.Window)
        {
            padding = new RectOffset(18, 18, 18, 18),
            border = new RectOffset(8, 8, 8, 8),
        };
        ApplyBackground(_windowStyle, _panelTexture!, new Color(0.92f, 0.90f, 0.82f, 1f));

        _headerStyle = new GUIStyle(sharedStyles.Header)
        {
            padding = new RectOffset(16, 16, 12, 12),
            margin = new RectOffset(0, 0, 0, 0),
        };
        ApplyBackground(_headerStyle, SharedBackground(sharedStyles.Header) ?? _panelAltTexture!, Color.white);

        _sectionStyle = new GUIStyle(sharedStyles.Panel)
        {
            padding = new RectOffset(16, 16, 12, 12),
            margin = new RectOffset(0, 0, 0, 0),
        };
        ApplyBackground(_sectionStyle, _panelAltTexture!, Color.white);

        _footerStyle = new GUIStyle(sharedStyles.Card)
        {
            padding = new RectOffset(14, 14, 10, 10),
            margin = new RectOffset(0, 0, 0, 0),
        };
        ApplyBackground(_footerStyle, _rowTexture!, Color.white);

        _titleStyle = new GUIStyle(sharedStyles.Title)
        {
            fontSize = 25,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,
            wordWrap = false,
        };

        _labelStyle = new GUIStyle(sharedStyles.Label)
        {
            fontSize = 18,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,
            wordWrap = false,
            clipping = TextClipping.Clip,
        };

        _mutedStyle = new GUIStyle(sharedStyles.MutedLabel)
        {
            fontSize = 14,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,
            wordWrap = false,
            clipping = TextClipping.Clip,
        };

        _statusStyle = new GUIStyle(_mutedStyle)
        {
            fontSize = 16,
            wordWrap = true,
        };

        _buttonStyle = BuildSharedButtonStyle(sharedStyles.Button, Color.white);
        _selectedButtonStyle = BuildSharedButtonStyle(sharedStyles.SecondaryButton, new Color(1f, 0.92f, 0.68f, 1f));
        _dangerButtonStyle = BuildButtonStyle(_dangerButtonTexture, _dangerHoverTexture);
    }

    private static GUIStyle GetModeButtonStyle(CompanionMode mode)
    {
        return _companionMode == mode ? _selectedButtonStyle! : _buttonStyle!;
    }

    private static GUIStyle GetRangeButtonStyle(FollowRangeProfile range)
    {
        return GetFollowRangeProfile() == NormalizeFollowRange(range) ? _selectedButtonStyle! : _buttonStyle!;
    }

    private static GUIStyle BuildButtonStyle(Texture2D normal, Texture2D hover)
    {
        GUIStyle style = new GUIStyle(GUI.skin.button)
        {
            fontSize = 13,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            clipping = TextClipping.Clip,
            border = new RectOffset(2, 2, 2, 2),
            padding = new RectOffset(6, 6, 3, 4),
        };
        style.normal.background = normal;
        style.hover.background = hover;
        style.active.background = hover;
        style.focused.background = normal;
        style.onNormal.background = normal;
        style.onHover.background = hover;
        style.onActive.background = hover;
        style.onFocused.background = normal;
        style.normal.textColor = Color.white;
        style.hover.textColor = Color.white;
        style.active.textColor = Color.white;
        style.focused.textColor = Color.white;
        style.onNormal.textColor = Color.white;
        style.onHover.textColor = Color.white;
        style.onActive.textColor = Color.white;
        style.onFocused.textColor = Color.white;
        return style;
    }

    private static GUIStyle BuildSharedButtonStyle(GUIStyle source, Color textColor)
    {
        GUIStyle style = new GUIStyle(source)
        {
            fontSize = 13,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            wordWrap = true,
            clipping = TextClipping.Clip,
            padding = new RectOffset(6, 6, 3, 4),
        };
        style.normal.textColor = textColor;
        style.hover.textColor = textColor;
        style.active.textColor = textColor;
        style.focused.textColor = textColor;
        style.onNormal.textColor = textColor;
        style.onHover.textColor = textColor;
        style.onActive.textColor = textColor;
        style.onFocused.textColor = textColor;
        return style;
    }

    private static void ApplyBackground(GUIStyle style, Texture2D background, Color textColor)
    {
        style.normal.background = background;
        style.hover.background = background;
        style.active.background = background;
        style.focused.background = background;
        style.onNormal.background = background;
        style.onHover.background = background;
        style.onActive.background = background;
        style.onFocused.background = background;
        style.normal.textColor = textColor;
        style.hover.textColor = textColor;
        style.active.textColor = textColor;
        style.focused.textColor = textColor;
        style.onNormal.textColor = textColor;
        style.onHover.textColor = textColor;
        style.onActive.textColor = textColor;
        style.onFocused.textColor = textColor;
    }

    private static Texture2D MakeTexture(Color color)
    {
        Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        texture.hideFlags = HideFlags.HideAndDontSave;
        return texture;
    }

    private static Texture2D? SharedBackground(GUIStyle style)
    {
        return RegisterSharedTexture(style.normal.background ?? style.hover.background ?? style.active.background ?? style.focused.background);
    }

    private static Texture2D? RegisterSharedTexture(Texture2D? texture)
    {
        if (texture != null)
        {
            SharedPanelTextures.Add(texture);
        }

        return texture;
    }

    private readonly struct CompanionProgressionStoreRecord : IEquatable<CompanionProgressionStoreRecord>
    {
        private CompanionProgressionStoreRecord(
            int profileSchemaVersion,
            string templateGuid,
            string templateName,
            string displayName,
            string reviewId,
            bool requiresPetComponent,
            bool runtimeApproved,
            int trustScore,
            int loyaltyScore,
            CompanionBondLevel trustLevel,
            CompanionBondLevel loyaltyLevel,
            CompanionTrustEvent lastTrustEvent,
            CompanionMode lastKnownMode,
            FollowRangeProfile lastKnownFollowRange,
            CompanionBondLevel effectiveBondLevel)
        {
            ProfileSchemaVersion = profileSchemaVersion;
            TemplateGuid = templateGuid;
            TemplateName = templateName;
            DisplayName = displayName;
            ReviewId = reviewId;
            RequiresPetComponent = requiresPetComponent;
            RuntimeApproved = runtimeApproved;
            TrustScore = trustScore;
            LoyaltyScore = loyaltyScore;
            TrustLevel = trustLevel;
            LoyaltyLevel = loyaltyLevel;
            LastTrustEvent = lastTrustEvent;
            LastKnownMode = lastKnownMode;
            LastKnownFollowRange = lastKnownFollowRange;
            EffectiveBondLevel = effectiveBondLevel;
        }

        internal int ProfileSchemaVersion { get; }

        internal string TemplateGuid { get; }

        internal string TemplateName { get; }

        internal string DisplayName { get; }

        internal string ReviewId { get; }

        internal bool RequiresPetComponent { get; }

        internal bool RuntimeApproved { get; }

        internal int TrustScore { get; }

        internal int LoyaltyScore { get; }

        internal CompanionBondLevel TrustLevel { get; }

        internal CompanionBondLevel LoyaltyLevel { get; }

        internal CompanionTrustEvent LastTrustEvent { get; }

        internal CompanionMode LastKnownMode { get; }

        internal FollowRangeProfile LastKnownFollowRange { get; }

        internal CompanionBondLevel EffectiveBondLevel { get; }

        internal static CompanionProgressionStoreRecord FromProfile(
            CompanionCoreRuntimeProfile profile,
            CompanionBondPolicyDecision policy)
        {
            CompanionTrustProfile trust = profile.TrustProfile;
            return new CompanionProgressionStoreRecord(
                CompanionProfileStoreSchemaVersion,
                profile.TemplateGuid,
                profile.TemplateName,
                profile.DisplayName,
                profile.ReviewId,
                profile.RequiresPetComponent,
                profile.RuntimeApproved,
                trust.TrustScore,
                trust.LoyaltyScore,
                trust.TrustLevel,
                trust.LoyaltyLevel,
                trust.LastEvent,
                profile.LastKnownMode,
                NormalizeFollowRange(profile.LastKnownFollowRange),
                policy.BondLevel);
        }

        internal static bool TryParse(string[] fields, out CompanionProgressionStoreRecord record)
        {
            record = default;
            if (fields.Length < 15)
            {
                return false;
            }

            if (!int.TryParse(fields[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int schemaVersion)
                || schemaVersion != CompanionProfileStoreSchemaVersion
                || string.IsNullOrWhiteSpace(fields[1])
                || string.IsNullOrWhiteSpace(fields[4])
                || !bool.TryParse(fields[5], out bool requiresPetComponent)
                || !bool.TryParse(fields[6], out bool runtimeApproved)
                || !int.TryParse(fields[7], NumberStyles.Integer, CultureInfo.InvariantCulture, out int trustScore)
                || !int.TryParse(fields[8], NumberStyles.Integer, CultureInfo.InvariantCulture, out int loyaltyScore)
                || !Enum.TryParse(fields[9], ignoreCase: true, out CompanionBondLevel trustLevel)
                || !Enum.TryParse(fields[10], ignoreCase: true, out CompanionBondLevel loyaltyLevel)
                || !Enum.TryParse(fields[11], ignoreCase: true, out CompanionTrustEvent lastTrustEvent)
                || !Enum.TryParse(fields[12], ignoreCase: true, out CompanionMode lastKnownMode)
                || !Enum.TryParse(fields[13], ignoreCase: true, out FollowRangeProfile lastKnownFollowRange)
                || !Enum.TryParse(fields[14], ignoreCase: true, out CompanionBondLevel effectiveBondLevel))
            {
                return false;
            }

            record = new CompanionProgressionStoreRecord(
                schemaVersion,
                fields[1],
                fields[2],
                fields[3],
                fields[4],
                requiresPetComponent,
                runtimeApproved,
                trustScore,
                loyaltyScore,
                trustLevel,
                loyaltyLevel,
                lastTrustEvent,
                lastKnownMode,
                NormalizeFollowRange(lastKnownFollowRange),
                effectiveBondLevel);
            return true;
        }

        internal string ToTsvLine()
        {
            return string.Join(
                "\t",
                new[]
                {
                    ProfileSchemaVersion.ToString(CultureInfo.InvariantCulture),
                    SanitizeProgressionStoreField(TemplateGuid),
                    SanitizeProgressionStoreField(TemplateName),
                    SanitizeProgressionStoreField(DisplayName),
                    SanitizeProgressionStoreField(ReviewId),
                    RequiresPetComponent.ToString().ToLowerInvariant(),
                    RuntimeApproved.ToString().ToLowerInvariant(),
                    TrustScore.ToString(CultureInfo.InvariantCulture),
                    LoyaltyScore.ToString(CultureInfo.InvariantCulture),
                    TrustLevel.ToString(),
                    LoyaltyLevel.ToString(),
                    LastTrustEvent.ToString(),
                    LastKnownMode.ToString(),
                    NormalizeFollowRange(LastKnownFollowRange).ToString(),
                    EffectiveBondLevel.ToString(),
                });
        }

        public bool Equals(CompanionProgressionStoreRecord other)
        {
            return ProfileSchemaVersion == other.ProfileSchemaVersion
                && StringComparer.OrdinalIgnoreCase.Equals(TemplateGuid, other.TemplateGuid)
                && StringComparer.OrdinalIgnoreCase.Equals(TemplateName, other.TemplateName)
                && StringComparer.OrdinalIgnoreCase.Equals(DisplayName, other.DisplayName)
                && StringComparer.OrdinalIgnoreCase.Equals(ReviewId, other.ReviewId)
                && RequiresPetComponent == other.RequiresPetComponent
                && RuntimeApproved == other.RuntimeApproved
                && TrustScore == other.TrustScore
                && LoyaltyScore == other.LoyaltyScore
                && TrustLevel == other.TrustLevel
                && LoyaltyLevel == other.LoyaltyLevel
                && LastTrustEvent == other.LastTrustEvent
                && LastKnownMode == other.LastKnownMode
                && LastKnownFollowRange == other.LastKnownFollowRange
                && EffectiveBondLevel == other.EffectiveBondLevel;
        }

        public override bool Equals(object? obj)
        {
            return obj is CompanionProgressionStoreRecord other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = ProfileSchemaVersion;
                hash = (hash * 397) ^ StringComparer.OrdinalIgnoreCase.GetHashCode(TemplateGuid ?? string.Empty);
                hash = (hash * 397) ^ StringComparer.OrdinalIgnoreCase.GetHashCode(ReviewId ?? string.Empty);
                hash = (hash * 397) ^ TrustScore;
                hash = (hash * 397) ^ LoyaltyScore;
                hash = (hash * 397) ^ (int)LastTrustEvent;
                return hash;
            }
        }
    }

    private readonly struct DefendTickResult
    {
        internal DefendTickResult(int prompted, int available, int skippedCooldown, int attackerCount)
        {
            Prompted = prompted;
            Available = available;
            SkippedCooldown = skippedCooldown;
            AttackerCount = attackerCount;
        }

        internal int Prompted { get; }

        internal int Available { get; }

        internal int SkippedCooldown { get; }

        internal int AttackerCount { get; }

        internal static DefendTickResult Empty(int attackerCount)
        {
            return new DefendTickResult(0, 0, 0, attackerCount);
        }
    }

    private sealed class InputModuleState
    {
        internal InputModuleState(BaseInputModule module, bool wasEnabled)
        {
            Module = module;
            WasEnabled = wasEnabled;
        }

        internal BaseInputModule Module { get; }

        internal bool WasEnabled { get; }
    }
}
