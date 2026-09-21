using System;
using System.Collections.Generic;
using AvalonHumanCompanions.Patches;
using BepInEx;
using BepInEx.Configuration;
using Awaken.TG.Main.AI.SummonsAndAllies;
using Awaken.TG.Main.Character;
using Awaken.TG.Main.Fights.Factions;
using Awaken.TG.Main.Fights.NPCs;
using Awaken.TG.Main.Heroes;
using Awaken.TG.Main.Heroes.Interactions;
using Awaken.TG.Main.Locations;
using Awaken.TG.Main.Locations.Setup;
using Awaken.TG.Main.Templates;
using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AvalonHumanCompanions;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed partial class Plugin : BaseUnityPlugin
{
    private const int PanelWindowId = 7620419;
    private const float PanelWidth = 900f;
    private const float PanelHeight = 780f;
    private const float PanelMinWidth = 760f;
    private const float PanelMinHeight = 640f;
    private const float PanelMargin = 32f;
    private const float PanelViewportWidthFraction = 0.42f;
    private const float PanelViewportHeightFraction = 0.38f;
    private static bool UseCompanionStyleCommandSurface => true;

    public const string PluginGuid = "kane.tgfoa.avalon-human-companions";
    public const string PluginName = "Avalon Human Companions";
    public const string PluginVersion = "0.6.2";

    private const string GateStatus = "BlockedUntilHumanNpcResearch";
    private const string HumanIconRoot = "AvatarIconsMegapack/CharacterIcons/Characters_WithBackground/";
    private const string HumanIconDefault = HumanIconRoot + "Human_01.png";
    private const string HumanIconArcher = HumanIconRoot + "Human_01_archer.png";
    private const string HumanIconVeteranArcher = HumanIconRoot + "Human_17_archer.png";
    private const string HumanIconCrossbowman = HumanIconRoot + "Crossbowman.png";
    private const string HumanIconFootman = HumanIconRoot + "Footman.png";
    private const string HumanIconHeavy = HumanIconRoot + "BoldWarrior.png";
    private const string HumanIconBarbarian = HumanIconRoot + "Human_25_barbarian.png";
    private const string HumanIconKnight = HumanIconRoot + "Human_04_knight.png";
    private const string HumanIconSlayer = HumanIconRoot + "Human_36_Slayer_of_knights.png";
    private const string HumanIconLancer = HumanIconRoot + "Lancer.png";
    private const string HumanIconHalberdier = HumanIconRoot + "Human_35_Halberdier.png";
    private const string HumanIconRogue = HumanIconRoot + "Human_23_rogue.png";
    private const string HumanIconRobber = HumanIconRoot + "Robber.png";
    private const string HumanIconRobber2 = HumanIconRoot + "Robber2.png";
    private const string HumanIconThug = HumanIconRoot + "Human_28_thug.png";
    private const string HumanIconBackground = "Dragon/1 (10).png";
    private const float HumanIconPortraitScale = 0.66f;
    private const float HumanHudIconPortraitScale = 0.54f;
    private const float CommandTickSeconds = 0.75f;
    private const float DefendTickSeconds = 0.35f;
    private const float FollowModeCatchUpDistance = 8f;
    private const float CompanionBrainMinimumTickSeconds = 0.5f;
    private const float CompanionBrainDefendPromptCooldownSeconds = 2.5f;
    private const float CompanionBrainLogSeconds = 4f;
    private const float AutoRecallDuplicateWindowSeconds = 2.5f;
    private const float RecallTargetDuplicateMeters = 0.5f;
    private const float AutoRecallTriggerBufferMeters = 2.0f;
    private const float FollowAutoRecallCooldownSeconds = 3.0f;
    private const float CombatAutoRecallCooldownSeconds = 2.0f;
    private const float EmergencyAutoRecallCooldownSeconds = 5.0f;
    private const float ResponsiveBrainTickSeconds = 0.5f;
    private const float ResponsiveFollowCatchUpDistance = 14f;
    private const float ResponsiveCombatRecallDistance = 12f;
    private const float ResponsiveFollowRecallGraceSeconds = 2.25f;
    private const float ResponsiveFollowAutoRecallCooldownSeconds = 1.75f;
    private const float ResponsiveCombatAutoRecallCooldownSeconds = 1.4f;
    private const float ResponsiveEmergencyAutoRecallCooldownSeconds = 3.0f;
    private const float ResponsiveDefendPromptCooldownSeconds = 1.4f;
    private const int DefaultHumanCandidateIndex = 8;
    private static readonly StarterCandidateRosterEntry[] StarterCandidateRoster =
    {
        new(8, "One-handed melee"),
        new(9, "Heavy melee"),
        new(10, "Spear and shield"),
        new(11, "Ranged support"),
        new(12, "Ranged support"),
        new(13, "One-handed melee"),
        new(14, "One-handed melee"),
        new(20, "Heavy melee"),
        new(26, "Ranged support"),
        new(32, "Armored melee"),
        new(33, "Ranged support"),
        new(36, "Armored melee"),
        new(37, "Heavy melee"),
    };

    private static readonly HumanCompanionRosterEntry[] ProofCandidateRoster =
    {
        new("Dal Riata Berserker", "Spec_Enemy_DalRiata_Tier4_DalRiataBerserker", "7ec0daa350cec3046bab5964e0337443", "HUMAN-REVIEW-001", "guard-faction-risk|hostile-risk"),
        new("Dal Riata Shrine Guard Berserker", "Spec_Enemy_DalRiata_Tier4_DalRiataBerserker_ShrineOnly_Guard", "e5b3797632ead0a44aabad268e76206e", "HUMAN-REVIEW-002", "guard-faction-risk|hostile-risk"),
        new("Desperate Peasant 1H", "Spec_Enemy_Generic_Tier0_DesperatePeasant_1h", "58810bba8bd2b2d49b5215b7e8fbdf1a", "HUMAN-REVIEW-003", "civilian-risk|hostile-risk"),
        new("Desperate Peasant 2H", "Spec_Enemy_Generic_Tier0_DesperatePeasant_2h", "e77dee7d8c4a1184da9fac445aff6826", "HUMAN-REVIEW-004", "civilian-risk|hostile-risk"),
        new("Outcast Peasant 1H", "Spec_Enemy_Generic_Tier1_OutcastPeasant_1H", "b4ebd8b6838b5064383679bd0e94f217", "HUMAN-REVIEW-005", "civilian-risk|hostile-risk"),
        new("Outcast Peasant 2H", "Spec_Enemy_Generic_Tier1_OutcastPeasant_2H", "238093c06bde73348bf46c0aa553e5a7", "HUMAN-REVIEW-006", "civilian-risk|hostile-risk"),
        new("Outcast Peasant Archer", "Spec_Enemy_Generic_Tier1_OutcastPeasant_Archer", "a42fd11d19d8de64c95a40002410ef4f", "HUMAN-REVIEW-007", "civilian-risk|hostile-risk"),
        new("Outcast Villager 2H", "Spec_Enemy_Generic_Tier1_OutcastVillager_2H", "d9fb7728fde8f384888700dea16db3f9", "HUMAN-REVIEW-008", "civilian-risk|hostile-risk"),
        new("Outlaw 1H", "Spec_Enemy_Generic_Tier1_Outlaw_1H", "2bd34a05d1e1fb94f9770b9ee7f23be2", "HUMAN-REVIEW-009", "hostile-risk"),
        new("Outlaw 2H", "Spec_Enemy_Generic_Tier1_Outlaw_2H", "e243d273de865d141bb168edf78a51d7", "HUMAN-REVIEW-010", "hostile-risk"),
        new("Outlaw Spear Shield", "Spec_Enemy_Generic_Tier1_Outlaw_SpearShield", "d25c05b988c561a4f9492cc21d17974b", "HUMAN-REVIEW-011", "hostile-risk"),
        new("Outlaw Archer", "Spec_Enemy_Generic_Tier1_OutlawArcher", "40a14d83ee1efba4ea728ac8aff659a8", "HUMAN-REVIEW-012", "hostile-risk"),
        new("Deranged Archer", "Spec_Enemy_Generic_Tier2_DerangedArcher", "31ee3f09a8ad8cd4584e0f530a3c3383", "HUMAN-REVIEW-013", "hostile-risk"),
        new("Deranged Infantryman 1H", "Spec_Enemy_Generic_Tier2_DerangedInfantryman_1H", "047f7c2bc50157147906bb587993d505", "HUMAN-REVIEW-014", "hostile-risk"),
        new("Highwayman 1H", "Spec_Enemy_Generic_Tier2_Highwayman_1H", "c7b737af7d133f64f9c088fcf74242c2", "HUMAN-REVIEW-015", "hostile-risk"),
        new("Highwayman 1H Abandoned Village", "Spec_Enemy_Generic_Tier2_Highwayman_1H_PostHOSAbandonedVillage", "630906930a4566949b51ab9b10bcc877", "HUMAN-REVIEW-016", "hostile-risk"),
        new("Highwayman 1H Gallows", "Spec_Enemy_Generic_Tier2_Highwayman_1H_PostHOSGallows", "55c7ec1d65df1aa4da4d316fcd35ad68", "HUMAN-REVIEW-017", "hostile-risk"),
        new("Highwayman 1H Kings Road", "Spec_Enemy_Generic_Tier2_Highwayman_1H_PostHOSKingsRoad", "770bb6c1999f0074d89fd8632e2669b5", "HUMAN-REVIEW-018", "hostile-risk"),
        new("Highwayman 1H Merch Ruins", "Spec_Enemy_Generic_Tier2_Highwayman_1H_PostHOSMerchRuins", "5f72cbae576486841999a9c6cab8399d", "HUMAN-REVIEW-019", "hostile-risk"),
        new("Highwayman 1H Self Ambush", "Spec_Enemy_Generic_Tier2_Highwayman_1H_SelfAmbushTeam", "6d472d6f7ffff3c439ce7c1d49cc2744", "HUMAN-REVIEW-020", "hostile-risk"),
        new("Highwayman 2H", "Spec_Enemy_Generic_Tier2_Highwayman_2H", "649bd846e1dede24ab7f9e877dd8d6eb", "HUMAN-REVIEW-021", "hostile-risk"),
        new("Highwayman 2H Abandoned Village", "Spec_Enemy_Generic_Tier2_Highwayman_2H_PostHOSAbandonedVillage", "3110daae817dfb440825e080dd072afe", "HUMAN-REVIEW-022", "hostile-risk"),
        new("Highwayman 2H Gallows", "Spec_Enemy_Generic_Tier2_Highwayman_2H_PostHOSGallows", "0779d655ef859984397f8844f94ee452", "HUMAN-REVIEW-023", "hostile-risk"),
        new("Highwayman 2H Kings Road", "Spec_Enemy_Generic_Tier2_Highwayman_2H_PostHOSKingsRoad", "3df98dfc52c1b624fb904368f635a079", "HUMAN-REVIEW-024", "hostile-risk"),
        new("Highwayman 2H Merch Ruins", "Spec_Enemy_Generic_Tier2_Highwayman_2H_PostHOSMerchRuins", "a156db91306851944b6fb5aca456f94b", "HUMAN-REVIEW-025", "hostile-risk"),
        new("Highwayman 2H Self Ambush", "Spec_Enemy_Generic_Tier2_Highwayman_2H_SelfAmbushTeam", "ec7edb091f4387f4d8806f32c07f00c1", "HUMAN-REVIEW-026", "hostile-risk"),
        new("Highwayman Archer", "Spec_Enemy_Generic_Tier2_HighwaymanArcher", "b61b246e7fd10f444916ca90b845aeab", "HUMAN-REVIEW-027", "hostile-risk"),
        new("Highwayman Archer Abandoned Village", "Spec_Enemy_Generic_Tier2_HighwaymanArcher_PostHOSAbandonedVillage", "5ae3eb0d344e1174ab20279f3612a422", "HUMAN-REVIEW-028", "hostile-risk"),
        new("Highwayman Archer Gallows", "Spec_Enemy_Generic_Tier2_HighwaymanArcher_PostHOSGallows", "52b98fb3a1e9b8a48bc1a5541b6490dc", "HUMAN-REVIEW-029", "hostile-risk"),
        new("Highwayman Archer Kings Road", "Spec_Enemy_Generic_Tier2_HighwaymanArcher_PostHOSKingsRoad", "e7907931432682942abfdb0572541d46", "HUMAN-REVIEW-030", "hostile-risk"),
        new("Highwayman Archer Merch Ruins", "Spec_Enemy_Generic_Tier2_HighwaymanArcher_PostHOSMerchRuins", "08202c42be62235429b809f9a73c3de7", "HUMAN-REVIEW-031", "hostile-risk"),
        new("Highwayman Archer Self Ambush", "Spec_Enemy_Generic_Tier2_HighwaymanArcher_SelfAmbushTeam", "716a3e21eada79e4793b70fa457e6a5e", "HUMAN-REVIEW-032", "hostile-risk"),
        new("Outlawed Knight 1H", "Spec_Enemy_Generic_Tier2_OutlawedKnight_1H", "b732840fb6f53004b92bb3f28fd10312", "HUMAN-REVIEW-033", "hostile-risk"),
        new("Desperate Archer", "Spec_Enemy_Generic_Tier3_DesperateArcher", "b849e9256457bf64d845a031526d9c69", "HUMAN-REVIEW-034", "hostile-risk"),
        new("Desperate Archer Galahad Variant", "Spec_Enemy_Generic_Tier3_DesperateArcher_GalahadVariant", "b33e55e03d0c269419190a003083c6f8", "HUMAN-REVIEW-035", "hostile-risk"),
        new("Desperate Archer Galahad Stand", "Spec_Enemy_Generic_Tier3_DesperateArcher_GalahadVariant_Stand", "a557c366fea47ff45a7c9f79405a6ebd", "HUMAN-REVIEW-036", "hostile-risk"),
        new("Disgraced Knight 1H", "Spec_Enemy_Generic_Tier3_DisgracedKnight_1H", "ecaebf529944057499ca2baf8ea4a86f", "HUMAN-REVIEW-037", "hostile-risk"),
        new("Watchtower Highwayman 2H", "Spec_Enemy_Special_Tier2_Highwayman_2H_Watchtower", "563ca6c7bf56b0745a09c1d01ec2ee8c", "HUMAN-REVIEW-038", "hostile-risk"),
        new("Hollow Druid", "Spec_NPC_BloodLake_HollowDruid", "c7428097ae0578340bc974595422c432", "HUMAN-REVIEW-039", "none"),
        new("HoS Guard", "Spec_NPC_HoS_HoSGuard", "315b1dd2db8446f44b29352d10c574c1", "HUMAN-REVIEW-040", "guard-faction-risk"),
        new("HoS Guard 03", "Spec_NPC_HoSGeneric_Guard_03", "e840d90476ebd91488ae4467e07e329c", "HUMAN-REVIEW-041", "guard-faction-risk"),
        new("Galahad Squire Repetitive", "Spec_NPC_Special_GalahadSquire_Repetetive", "a13a2abd2f5e61d438f322360035ea9a", "HUMAN-REVIEW-042", "none"),
    };

    private enum HumanProofMode
    {
        Follow,
        Hold,
        Defend
    }

    private readonly struct StarterCandidateRosterEntry
    {
        internal StarterCandidateRosterEntry(int rosterIndex, string roleLabel)
        {
            RosterIndex = rosterIndex;
            RoleLabel = roleLabel;
        }

        internal int RosterIndex { get; }

        internal string RoleLabel { get; }
    }

    private readonly struct HumanCompanionBrainProfile
    {
        internal HumanCompanionBrainProfile(
            string roleLabel,
            float followBack,
            float followRight,
            float combatBack,
            float combatRight)
        {
            RoleLabel = roleLabel;
            FollowBack = followBack;
            FollowRight = followRight;
            CombatBack = combatBack;
            CombatRight = combatRight;
        }

        internal string RoleLabel { get; }

        internal float FollowBack { get; }

        internal float FollowRight { get; }

        internal float CombatBack { get; }

        internal float CombatRight { get; }
    }

    private ConfigEntry<bool> _enabled = null!;
    private ConfigEntry<bool> _researchModeOnly = null!;
    private ConfigEntry<string> _templateName = null!;
    private ConfigEntry<string> _templateGuid = null!;
    private ConfigEntry<bool> _logGateOnLoad = null!;
    private ConfigEntry<bool> _enableDisabledRecruitmentTest = null!;
    private ConfigEntry<KeyCode> _disabledRecruitmentTestHotkey = null!;
    private ConfigEntry<bool> _enableSafeSpawnCommand = null!;
    private ConfigEntry<KeyCode> _safeSpawnHotkey = null!;
    private ConfigEntry<float> _safeSpawnDistance = null!;
    private ConfigEntry<bool> _limitOneProofSpawnPerSession = null!;
    private ConfigEntry<bool> _enableOneSessionAllyProof = null!;
    private ConfigEntry<KeyCode> _allyProofHotkey = null!;
    private ConfigEntry<KeyCode> _dismissAllyProofHotkey = null!;
    private ConfigEntry<float> _allyProofDistance = null!;
    private ConfigEntry<bool> _limitOneAllyProofPerSession = null!;
    private ConfigEntry<bool> _enableNativeCommandActions = null!;
    private ConfigEntry<bool> _enableFollowCatchUp = null!;
    private ConfigEntry<float> _followCatchUpDistance = null!;
    private ConfigEntry<bool> _enableNativeDefendAssist = null!;
    private ConfigEntry<bool> _enableCompanionBrain = null!;
    private ConfigEntry<bool> _enableCompanionBrainResponsiveAssist = null!;
    private ConfigEntry<float> _companionBrainTickSeconds = null!;
    private ConfigEntry<bool> _enableCompanionBrainEmergencyRecall = null!;
    private ConfigEntry<bool> _enableCompanionBrainRolePlacement = null!;
    private ConfigEntry<float> _companionBrainCombatRecallDistance = null!;
    private ConfigEntry<float> _companionBrainEmergencyRecallDistance = null!;
    private ConfigEntry<float> _responsiveBrainTickSeconds = null!;
    private ConfigEntry<float> _responsiveFollowLeashDistance = null!;
    private ConfigEntry<float> _responsiveFollowRecallGraceSeconds = null!;
    private ConfigEntry<float> _responsiveCombatRecallDistance = null!;
    private ConfigEntry<float> _responsiveFollowRecallCooldownSeconds = null!;
    private ConfigEntry<float> _responsiveCombatRecallCooldownSeconds = null!;
    private ConfigEntry<float> _responsiveEmergencyRecallCooldownSeconds = null!;
    private ConfigEntry<float> _responsiveDefendPromptCooldownSeconds = null!;
    private ConfigEntry<bool> _enableProofCommandPanel = null!;
    private ConfigEntry<KeyCode> _togglePanelHotkey = null!;
    private ConfigEntry<bool> _showCompanionHudIcon = null!;
    private ConfigEntry<string> _companionHudIconCorner = null!;
    private ConfigEntry<int> _companionHudIconSize = null!;
    private ConfigEntry<bool> _enableProofCandidateRoster = null!;
    private ConfigEntry<int> _selectedCandidateIndex = null!;
    private ConfigEntry<bool> _enableLifecycleSafetyGuard = null!;
    private ConfigEntry<bool> _writeLifecycleDiagnostics = null!;
    private ConfigEntry<float> _lifecycleTickSeconds = null!;
    private ConfigEntry<bool> _enableDisabledActorScanner = null!;
    private ConfigEntry<KeyCode> _actorScannerHotkey = null!;
    private ConfigEntry<float> _actorScannerRadius = null!;
    private ConfigEntry<string> _actorScannerSelectedTarget = null!;

    private Location? _lastProofSpawn;
    private Location? _lastAllyProof;
    private bool _proofSpawnedThisSession;
    private bool _allyProofSpawnedThisSession;
    private HumanProofMode _allyProofMode = HumanProofMode.Follow;
    private float _nextCommandTick;
    private float _nextDefendTick;
    private float _nextCompanionBrainTick;
    private float _nextCompanionBrainDefendPromptAt;
    private float _nextCompanionBrainLogAt;
    private float _lastAllyRecallAt;
    private float _nextFollowAutoRecallAllowedAt;
    private float _nextCombatAutoRecallAllowedAt;
    private float _nextEmergencyAutoRecallAllowedAt;
    private float _followLeashExceededSince;
    private bool _companionBrainLastHadAttackers;
    private int _companionBrainLastAttackerCount;
    private string _companionBrainLastEvent = "Idle.";
    private string _lastAllyRecallReason = string.Empty;
    private string _hudCompanionIconPath = string.Empty;
    private Texture2D? _hudCompanionIconTexture;
    private Texture2D? _hudCompanionBackgroundTexture;
    private float _nextHudBackgroundLookupAt;
    private bool _hasLastAllyRecallTarget;
    private Vector3 _lastAllyRecallTarget;
    private bool _hasHoldAnchor;
    private Vector3 _holdAnchor;
    private bool _panelVisible;
    private bool _dialogueVisible;
    private bool _panelInitialized;
    private Vector2 _panelScrollPosition;
    private float _nextLifecycleTick;
    private int _lastPanelScreenWidth;
    private int _lastPanelScreenHeight;
    private bool _cursorCaptured;
    private bool _controllerCursorScopeActive;
    private bool _taintedInterfaceScopeActive;
    private bool _previousCursorVisible;
    private bool _inputModulesCaptured;
    private CursorLockMode _previousCursorLockState;
    private Rect _panelRect;
    private string _panelStatus = "Ready.";
    private readonly List<InputModuleState> _inputModuleStates = new();
    private readonly HashSet<Texture2D> _sharedPanelTextures = new();
    private Harmony? _harmony;
    private Texture2D? _panelTexture;
    private Texture2D? _sectionTexture;
    private Texture2D? _buttonTexture;
    private Texture2D? _buttonHoverTexture;
    private Texture2D? _selectedButtonTexture;
    private Texture2D? _dangerButtonTexture;
    private Texture2D? _dangerHoverTexture;
    private GUIStyle? _panelWindowStyle;
    private GUIStyle? _headerStyle;
    private GUIStyle? _sectionStyle;
    private GUIStyle? _titleStyle;
    private GUIStyle? _labelStyle;
    private GUIStyle? _mutedStyle;
    private GUIStyle? _buttonStyle;
    private GUIStyle? _selectedButtonStyle;
    private GUIStyle? _dangerButtonStyle;
    private GUIStyle? _footerStyle;

    internal static Plugin? Instance { get; private set; }

    internal static bool IsPanelInputActive => Instance is { } instance && (instance._panelVisible || instance._dialogueVisible);
    internal static bool IsDialogueVisible => Instance?._dialogueVisible == true;

    private void Awake()
    {
        Instance = this;

        _enabled = Config.Bind("General", "Enabled", true, "Enable this diagnostic scaffold.");
        _researchModeOnly = Config.Bind("Research", "ResearchModeOnly", true, "Keep human NPC companion runtime behavior blocked until actor, faction, dialogue, quest, transition, and save/load research pass.");
        _templateName = Config.Bind("Target", "TemplateName", string.Empty, "Optional reviewed human NPC LocationTemplate name. Diagnostics only; empty means no target is approved.");
        _templateGuid = Config.Bind("Target", "TemplateGuid", string.Empty, "Optional reviewed human NPC LocationTemplate GUID. Diagnostics only; empty means no target is approved.");
        _logGateOnLoad = Config.Bind("Diagnostics", "LogGateOnLoad", true, "Log the human companion research gate when the scaffold loads.");
        _enableDisabledRecruitmentTest = Config.Bind("Prototype", "EnableDisabledRecruitmentTest", false, "Diagnostic gate only. When enabled with a hotkey, logs that recruitment behavior is still blocked. It never touches NPCs.");
        _disabledRecruitmentTestHotkey = Config.Bind("Prototype", "DisabledRecruitmentTestHotkey", KeyCode.None, "Optional hotkey for the disabled recruitment-test log. Default None avoids hotkey conflicts.");
        _enableSafeSpawnCommand = Config.Bind("ProofSpawn", "EnableSafeSpawnCommand", false, "Enable one explicit proof spawn command. Requires Target.TemplateGuid, blocks unique NPC templates, marks the spawned location not saved, and does not apply companion behavior.");
        _safeSpawnHotkey = Config.Bind("ProofSpawn", "SafeSpawnHotkey", KeyCode.None, "Optional hotkey for the proof spawn command. Default None prevents accidental spawns.");
        _safeSpawnDistance = Config.Bind("ProofSpawn", "SafeSpawnDistance", 4f, new ConfigDescription("Meters behind the hero for the proof spawn.", new AcceptableValueRange<float>(2f, 12f)));
        _limitOneProofSpawnPerSession = Config.Bind("ProofSpawn", "LimitOneProofSpawnPerSession", true, "Allow only one proof spawn per game session.");
        _enableOneSessionAllyProof = Config.Bind("HumanAllyProof", "EnableOneSessionAllyProof", false, "Enable one explicit human native-ally proof spawn. Requires ResearchModeOnly=false, Target.TemplateGuid, non-unique NpcAttachment, and applies the same summon-faction plus NpcHeroPetAlly path used by Avalon Companions creature candidates.");
        _allyProofHotkey = Config.Bind("HumanAllyProof", "AllyProofHotkey", KeyCode.None, "Optional hotkey for the human native-ally proof spawn. Default None prevents accidental spawns.");
        _dismissAllyProofHotkey = Config.Bind("HumanAllyProof", "DismissAllyProofHotkey", KeyCode.None, "Optional hotkey to discard the active one-session human ally proof actor.");
        _allyProofDistance = Config.Bind("HumanAllyProof", "AllyProofDistance", 5f, new ConfigDescription("Meters behind and slightly right of the hero for the human native-ally proof spawn.", new AcceptableValueRange<float>(2f, 12f)));
        _limitOneAllyProofPerSession = Config.Bind("HumanAllyProof", "LimitOneAllyProofPerSession", true, "Allow only one human native-ally proof spawn per game session.");
        _enableNativeCommandActions = Config.Bind("HumanCommands", "EnableNativeCommandActions", true, "Attach one runtime-only native Companion prompt to the active one-session human ally proof actor. With the proof panel enabled, the prompt opens the existing plugin-owned command panel.");
        _enableFollowCatchUp = Config.Bind("HumanCommands", "EnableFollowCatchUp", true, "When native command actions are enabled, allow Follow and Defend modes to recall the active proof actor if it falls too far behind.");
        _followCatchUpDistance = Config.Bind("HumanCommands", "FollowCatchUpDistance", 18f, new ConfigDescription("Meters before Defend recalls the active proof actor near the hero. Follow mode clamps this to the closer proof-follow range.", new AcceptableValueRange<float>(8f, 60f)));
        _enableNativeDefendAssist = Config.Bind("HumanCommands", "EnableNativeDefendAssist", false, "Allow native NpcHeroPetAlly.EnterCombat when the hero already has live attackers. The native ally path chooses the target; this does not add custom targeting or attack commands.");
        _enableCompanionBrain = Config.Bind("HumanBrain", "EnableCompanionBrain", true, "Enable the runtime-only companion brain for the active one-session human ally proof actor. It only reuses bounded recall and native NpcHeroPetAlly.EnterCombat; it does not add custom targeting, pathing, recruitment, or save data.");
        _enableCompanionBrainResponsiveAssist = Config.Bind("HumanBrain", "EnableResponsiveNativeAssist", true, "Allow the companion brain to evaluate more often, keep Follow responsive with a bounded leash and grace window, and proactively prompt native NpcHeroPetAlly.EnterCombat when the hero already has live attackers. Uses only bounded recall and native defend; no custom targeting, pathing, recruitment, persistence, or save data.");
        _companionBrainTickSeconds = Config.Bind("HumanBrain", "BrainTickSeconds", 0.75f, new ConfigDescription("Minimum seconds between companion brain evaluations. Values below 0.5 are clamped.", new AcceptableValueRange<float>(0.5f, 5f)));
        _enableCompanionBrainEmergencyRecall = Config.Bind("HumanBrain", "EnableEmergencyRecall", true, "Allow the companion brain to recover the active proof actor with the existing recall path if it is extremely far away. Hold mode is not moved.");
        _enableCompanionBrainRolePlacement = Config.Bind("HumanBrain", "EnableRoleAwarePlacement", true, "Allow the companion brain to use the selected starter role for bounded recall placement. This changes only recall offsets around the hero; it does not add custom pathing or targets.");
        _companionBrainCombatRecallDistance = Config.Bind("HumanBrain", "CombatRecallDistance", 12f, new ConfigDescription("Meters before Defend mode recalls the active proof actor closer before native combat prompting.", new AcceptableValueRange<float>(6f, 40f)));
        _companionBrainEmergencyRecallDistance = Config.Bind("HumanBrain", "EmergencyRecallDistance", 30f, new ConfigDescription("Meters before the companion brain emergency recall runs for Follow or Defend when enabled.", new AcceptableValueRange<float>(12f, 120f)));
        _responsiveBrainTickSeconds = Config.Bind("HumanBrainTuning", "ResponsiveBrainTickSeconds", ResponsiveBrainTickSeconds, new ConfigDescription("Seconds between responsive companion brain evaluations. Lower values react faster; higher values reduce update pressure.", new AcceptableValueRange<float>(0.5f, 5f)));
        _responsiveFollowLeashDistance = Config.Bind("HumanBrainTuning", "ResponsiveFollowLeashDistance", ResponsiveFollowCatchUpDistance, new ConfigDescription("Meters before responsive Follow starts considering automatic catch-up recall. Larger values let the companion move freely longer before teleport recall.", new AcceptableValueRange<float>(8f, 60f)));
        _responsiveFollowRecallGraceSeconds = Config.Bind("HumanBrainTuning", "ResponsiveFollowRecallGraceSeconds", ResponsiveFollowRecallGraceSeconds, new ConfigDescription("Seconds the companion may remain beyond the responsive Follow leash before automatic catch-up recall fires. Set to 0 for immediate recall after crossing the leash.", new AcceptableValueRange<float>(0f, 10f)));
        _responsiveCombatRecallDistance = Config.Bind("HumanBrainTuning", "ResponsiveCombatRecallDistance", ResponsiveCombatRecallDistance, new ConfigDescription("Meters before responsive combat protection may recall the companion closer before native NpcHeroPetAlly.EnterCombat prompting.", new AcceptableValueRange<float>(6f, 60f)));
        _responsiveFollowRecallCooldownSeconds = Config.Bind("HumanBrainTuning", "ResponsiveFollowRecallCooldownSeconds", ResponsiveFollowAutoRecallCooldownSeconds, new ConfigDescription("Minimum seconds between automatic responsive Follow catch-up recalls.", new AcceptableValueRange<float>(0.5f, 15f)));
        _responsiveCombatRecallCooldownSeconds = Config.Bind("HumanBrainTuning", "ResponsiveCombatRecallCooldownSeconds", ResponsiveCombatAutoRecallCooldownSeconds, new ConfigDescription("Minimum seconds between automatic responsive combat catch-up recalls.", new AcceptableValueRange<float>(0.5f, 15f)));
        _responsiveEmergencyRecallCooldownSeconds = Config.Bind("HumanBrainTuning", "ResponsiveEmergencyRecallCooldownSeconds", ResponsiveEmergencyAutoRecallCooldownSeconds, new ConfigDescription("Minimum seconds between automatic responsive emergency recalls.", new AcceptableValueRange<float>(1f, 30f)));
        _responsiveDefendPromptCooldownSeconds = Config.Bind("HumanBrainTuning", "ResponsiveDefendPromptCooldownSeconds", ResponsiveDefendPromptCooldownSeconds, new ConfigDescription("Minimum seconds between responsive native defend prompts while the hero has live attackers.", new AcceptableValueRange<float>(0.5f, 15f)));
        _enableProofCommandPanel = Config.Bind("HumanCommandPanel", "EnableProofCommandPanel", true, "Enable a proof-only styled command panel for the active one-session human ally proof actor.");
        _togglePanelHotkey = Config.Bind("HumanCommandPanel", "TogglePanelHotkey", KeyCode.End, "Open or close the proof-only human companion command panel.");
        _showCompanionHudIcon = Config.Bind("HumanHud", "ShowCompanionHudIcon", true, "Show a passive gameplay HUD badge for the active one-session human companion. Hidden while the panel or dialogue is open.");
        _companionHudIconCorner = Config.Bind("HumanHud", "CompanionHudIconCorner", "TopLeft", "HUD badge corner. Supported values: TopLeft, BottomRight.");
        _companionHudIconSize = Config.Bind("HumanHud", "CompanionHudIconSize", 88, new ConfigDescription("HUD badge size in pixels.", new AcceptableValueRange<int>(56, 128)));
        _enableProofCandidateRoster = Config.Bind("HumanRoster", "EnableProofCandidateRoster", true, "Enable the registered human proof-candidate roster in the debug panel. Candidates are one-session proof targets only, not recruitment or persistence approval.");
        _selectedCandidateIndex = Config.Bind("HumanRoster", "SelectedCandidateIndex", DefaultHumanCandidateIndex, "Selected human proof-candidate roster index. Prev/Next in the panel changes this.");
        _enableLifecycleSafetyGuard = Config.Bind("HumanLifecycle", "EnableLifecycleSafetyGuard", true, "Keep the active one-session proof actor marked not saved and discard it if it loses the expected managed proof marker.");
        _writeLifecycleDiagnostics = Config.Bind("HumanLifecycle", "WriteLifecycleDiagnostics", true, "Write human-proof-lifecycle.csv event rows for spawn, dismiss, recall, guard ticks, and cleanup. Evidence only.");
        _lifecycleTickSeconds = Config.Bind("HumanLifecycle", "LifecycleTickSeconds", 5f, new ConfigDescription("Seconds between lifecycle guard evidence ticks while a proof actor is active.", new AcceptableValueRange<float>(1f, 60f)));
        _enableDisabledActorScanner = Config.Bind("ActorScanner", "EnableDisabledActorScanner", false, "Enable the disabled human NPC scanner. It writes CSV evidence only and never commands, factions, recruits, persists, or edits actors.");
        _actorScannerHotkey = Config.Bind("ActorScanner", "ScannerHotkey", KeyCode.None, "Optional hotkey for the disabled scanner. Default None prevents accidental dumps.");
        _actorScannerRadius = Config.Bind("ActorScanner", "ScanRadius", 35f, new ConfigDescription("Meters around the hero to scan for live actor-like locations.", new AcceptableValueRange<float>(5f, 120f)));
        _actorScannerSelectedTarget = Config.Bind("ActorScanner", "SelectedTarget", string.Empty, "Optional reviewed template GUID or live location ID for dry-run command classification.");

        _harmony = new Harmony(PluginGuid);
        HumanPanelInputLockPatch.Apply(_harmony, Logger);

        Logger.LogInfo($"{PluginName} {PluginVersion} loaded. Enabled={_enabled.Value}; ResearchModeOnly={_researchModeOnly.Value}; ProofCandidateRoster={_enableProofCandidateRoster.Value}; CompanionBrain={_enableCompanionBrain.Value}; ResponsiveNativeAssist={_enableCompanionBrainResponsiveAssist.Value}; Target={GetTargetLabel()}.");
        Logger.LogInfo($"{PluginName} responsive tuning: {GetResponsiveTuningSummary()}");

        if (_logGateOnLoad.Value)
        {
            Logger.LogInfo(
                $"{PluginName} is research/proof only. Status={GateStatus}; general companion behavior is blocked; "
                + "only explicitly enabled throwaway-save proof gates can run. Human NPC recruitment, conversion, command panel, custom control, and persistence are not implemented.");
        }
    }

    private void Update()
    {
        if (!_enabled.Value)
        {
            if (_panelVisible)
            {
                SetPanelVisible(false, "plugin disabled");
            }

            if (_dialogueVisible)
            {
                SetDialogueVisible(false, "plugin disabled");
            }

            TryDiscardProofSpawn("plugin disabled");
            TryDiscardAllyProof("plugin disabled");
            UpdateCursor();
            return;
        }

        if (_dialogueVisible && Input.GetKeyDown(KeyCode.Escape))
        {
            SetDialogueVisible(false, "escape");
            return;
        }

        if (_enableProofCommandPanel.Value
            && _togglePanelHotkey.Value != KeyCode.None
            && Input.GetKeyDown(_togglePanelHotkey.Value))
        {
            if (!_panelVisible && _dialogueVisible)
            {
                SetDialogueVisible(false, "panel hotkey");
            }

            SetPanelVisible(!_panelVisible, "panel hotkey");
        }

        if (_panelVisible && Input.GetKeyDown(KeyCode.Escape))
        {
            SetPanelVisible(false, "escape");
        }

        if (_enableDisabledRecruitmentTest.Value
            && _disabledRecruitmentTestHotkey.Value != KeyCode.None
            && Input.GetKeyDown(_disabledRecruitmentTestHotkey.Value))
        {
            Logger.LogWarning($"{PluginName} disabled recruitment test requested for {GetTargetLabel()}, but behavior is blocked. This scaffold does not recruit, spawn, clone, convert, move, command, persist, or alter human NPCs.");
        }

        if (_enableDisabledActorScanner.Value
            && _actorScannerHotkey.Value != KeyCode.None
            && Input.GetKeyDown(_actorScannerHotkey.Value))
        {
            HumanActorScanner.WriteSnapshot(
                Logger,
                _actorScannerRadius.Value,
                GetScannerTargetGuid());
        }

        if (_enableOneSessionAllyProof.Value
            && _dismissAllyProofHotkey.Value != KeyCode.None
            && Input.GetKeyDown(_dismissAllyProofHotkey.Value))
        {
            TryDiscardAllyProof("dismiss hotkey");
        }

        TickAllyProofCommands();
        TickAllyProofLifecycle();

        if (_panelVisible)
        {
            Input.ResetInputAxes();
            EnsurePanelInputBlocked();
        }
        else if (_dialogueVisible)
        {
            EnsureHumanCompanionDialogueUiHost();
        }

        UpdateCursor();
    }

    private void LateUpdate()
    {
        if (!_enabled.Value)
        {
            return;
        }

        if (_panelVisible)
        {
            Input.ResetInputAxes();
            EnsurePanelInputBlocked();
        }
        else if (_dialogueVisible)
        {
            EnsureHumanCompanionDialogueUiHost();
        }

        if (_panelVisible || _dialogueVisible)
        {
            EnsureCursorForPanel();
        }

        if (_enableSafeSpawnCommand.Value
            && _safeSpawnHotkey.Value != KeyCode.None
            && Input.GetKeyDown(_safeSpawnHotkey.Value))
        {
            TrySafeProofSpawn();
        }

        if (_enableOneSessionAllyProof.Value
            && _allyProofHotkey.Value != KeyCode.None
            && Input.GetKeyDown(_allyProofHotkey.Value))
        {
            TryOneSessionAllyProofSpawn();
        }
    }

    private void OnDestroy()
    {
        SetPanelVisible(false, "plugin unload");
        SetDialogueVisible(false, "plugin unload");
        TryDiscardProofSpawn("plugin unload");
        TryDiscardAllyProof("plugin unload");
        DestroyHumanCompanionDialogueUiHost();
        DestroyPanelTextures();
        _harmony?.UnpatchSelf();
        _harmony = null;
        ResetAiRuntimeBridgeForPluginUnload();
        Instance = null;
    }

    private void OnGUI()
    {
        if (_dialogueVisible)
        {
            EnsureCursorForPanel();
            EnsureHumanCompanionDialogueUiHost();
        }

        DrawHumanCompanionHudIcon();

        if (!_panelVisible)
        {
            return;
        }

        EnsurePanelStyles();
        EnsurePanelPlacement();
        EnsureCursorForPanel();
        TaintedInterfaceBridge.EnsureInteractiveCursor();
        EnsurePanelInputBlocked();

        GUI.depth = -1000;
        Rect drawnPanel = GUI.Window(PanelWindowId, _panelRect, DrawPanelWindow, GUIContent.none, _panelWindowStyle!);
        _panelRect = ClampPanelToScreen(drawnPanel);
    }

    private void TrySafeProofSpawn()
    {
        if (_limitOneProofSpawnPerSession.Value && _proofSpawnedThisSession)
        {
            Logger.LogWarning($"{PluginName} proof spawn blocked: one proof spawn already ran this session.");
            return;
        }

        string guid = GetActiveTargetGuid();
        if (string.IsNullOrWhiteSpace(guid))
        {
            Logger.LogWarning($"{PluginName} proof spawn blocked: no human proof target selected.");
            return;
        }

        Hero hero = Hero.Current;
        if (hero == null)
        {
            Logger.LogWarning($"{PluginName} proof spawn blocked: Hero.Current is null. Load a save before using the command.");
            return;
        }

        LocationTemplate? template = new TemplateReference(guid).Get<LocationTemplate>();
        if (template == null)
        {
            Logger.LogWarning($"{PluginName} proof spawn blocked: no LocationTemplate found for GUID {guid}.");
            return;
        }

        NpcAttachment? npcAttachment = template.GetComponent<NpcAttachment>();
        if (npcAttachment == null)
        {
            Logger.LogWarning($"{PluginName} proof spawn blocked for {template.name}[{template.GUID}]: template has no NpcAttachment.");
            return;
        }

        if (npcAttachment.IsUnique)
        {
            Logger.LogWarning($"{PluginName} proof spawn blocked for {template.name}[{template.GUID}]: UniqueNpcAttachment or unique NPC template detected.");
            return;
        }

        Vector3 spawnPosition = hero.Coords + hero.Rotation * (Vector3.back * _safeSpawnDistance.Value);
        Location location = template.SpawnLocation(spawnPosition, hero.Rotation);
        location.MarkedNotSaved = true;

        if (!location.TryGetElement(out NpcElement npcElement) || npcElement == null)
        {
            location.MarkedNotSaved = true;
            location.Discard();
            Logger.LogWarning($"{PluginName} proof spawn discarded: {template.name}[{template.GUID}] spawned without NpcElement.");
            return;
        }

        if (npcElement.IsUnique)
        {
            location.MarkedNotSaved = true;
            location.Discard();
            Logger.LogWarning($"{PluginName} proof spawn discarded: {template.name}[{template.GUID}] produced an IsUnique NpcElement.");
            return;
        }

        _lastProofSpawn = location;
        _proofSpawnedThisSession = true;
        WriteLifecycleEvent("proof-spawn-created", "safe proof spawn", location, tracked: false);

        Logger.LogInfo(
            $"{PluginName} proof spawn created {template.name}[{template.GUID}] at {spawnPosition}. "
            + "Location marked not saved. No recruitment, faction edit, ally marker, follow command, dialogue, story, crime, or persistence behavior was applied.");
    }

    private void TryOneSessionAllyProofSpawn()
    {
        if (_researchModeOnly.Value)
        {
            Logger.LogWarning($"{PluginName} one-session ally proof blocked: Research.ResearchModeOnly=true. Set it false only on a throwaway save after reading the human ally proof docs.");
            return;
        }

        if (_limitOneAllyProofPerSession.Value && _allyProofSpawnedThisSession)
        {
            Logger.LogWarning($"{PluginName} one-session ally proof blocked: one ally proof already ran this session.");
            return;
        }

        string guid = GetActiveTargetGuid();
        if (string.IsNullOrWhiteSpace(guid))
        {
            Logger.LogWarning($"{PluginName} one-session ally proof blocked: no human proof target selected.");
            return;
        }

        Hero hero = Hero.Current;
        if (hero == null)
        {
            Logger.LogWarning($"{PluginName} one-session ally proof blocked: Hero.Current is null. Load a save before using the command.");
            return;
        }

        LocationTemplate? template = new TemplateReference(guid).Get<LocationTemplate>();
        if (template == null)
        {
            Logger.LogWarning($"{PluginName} one-session ally proof blocked: no LocationTemplate found for GUID {guid}.");
            return;
        }

        NpcAttachment? npcAttachment = template.GetComponent<NpcAttachment>();
        if (npcAttachment == null)
        {
            Logger.LogWarning($"{PluginName} one-session ally proof blocked for {template.name}[{template.GUID}]: template has no NpcAttachment.");
            return;
        }

        if (npcAttachment.IsUnique)
        {
            Logger.LogWarning($"{PluginName} one-session ally proof blocked for {template.name}[{template.GUID}]: UniqueNpcAttachment or unique NPC template detected.");
            return;
        }

        Vector3 offset = (Vector3.back * _allyProofDistance.Value) + (Vector3.right * 1.6f);
        Vector3 spawnPosition = hero.Coords + hero.Rotation * offset;
        Location location = template.SpawnLocation(spawnPosition, hero.Rotation);
        location.MarkedNotSaved = true;

        if (!location.TryGetElement(out NpcElement npcElement) || npcElement == null)
        {
            location.MarkedNotSaved = true;
            location.Discard();
            Logger.LogWarning($"{PluginName} one-session ally proof discarded: {template.name}[{template.GUID}] spawned without NpcElement.");
            return;
        }

        if (npcElement.IsUnique)
        {
            location.MarkedNotSaved = true;
            location.Discard();
            Logger.LogWarning($"{PluginName} one-session ally proof discarded: {template.name}[{template.GUID}] produced an IsUnique NpcElement.");
            return;
        }

        if (!TryApplyNativeAllyMarker(location, npcElement, hero))
        {
            location.MarkedNotSaved = true;
            location.Discard();
            Logger.LogWarning($"{PluginName} one-session ally proof discarded: native summon-faction plus NpcHeroPetAlly setup failed for {template.name}[{template.GUID}].");
            return;
        }

        _lastAllyProof = location;
        _allyProofSpawnedThisSession = true;
        _allyProofMode = HumanProofMode.Follow;
        ResetCompanionBrainState("Ready.");
        EnsureNativeCommandActions(location);
        WriteLifecycleEvent("ally-proof-created", "one-session ally proof spawn", location, tracked: true);

        Logger.LogInfo(
            $"{PluginName} one-session ally proof created {template.name}[{template.GUID}] at {spawnPosition}. "
            + "Location marked not saved; native summon-faction plus NpcHeroPetAlly ownership applied, mirroring the Avalon Companions creature-candidate path. "
            + "No recruitment, dialogue, story, crime, quest, custom target selection, or persistence behavior was applied.");
    }

    private bool TryApplyNativeAllyMarker(Location location, NpcElement npcElement, Hero hero)
    {
        try
        {
            if (!npcElement.HasElement<NpcHeroPetAlly>())
            {
                npcElement.OverrideFaction(hero.GetFactionTemplateForSummon(), FactionOverrideContext.Summon);
                npcElement.AddElement(new NpcHeroPetAlly(hero));
            }

            location.MarkedNotSaved = true;
            NpcHeroPetAlly marker = npcElement.TryGetElement<NpcHeroPetAlly>();
            if (marker == null || marker.HasBeenDiscarded)
            {
                return false;
            }

            Logger.LogInfo(
                $"{PluginName} native ally setup applied for {location.Template?.name}[{location.Template?.GUID}]. "
                + "Mirrors Avalon Companions creature path: OverrideFaction(hero summon faction, Summon) plus NpcHeroPetAlly.");
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"{PluginName} native ally setup failed for {location.Template?.name}[{location.Template?.GUID}]: {ex.GetType().Name}: {ex.Message}");
            return false;
        }
    }

    internal static bool IsNativeHumanCommandAvailable(Location location, Hero? hero, IInteractableWithHero interactable, NativeHumanCompanionCommand command)
    {
        return Instance != null && Instance.IsNativeHumanCommandAvailableInstance(location, hero, interactable, command);
    }

    internal static void RunNativeHumanCommand(Location location, NativeHumanCompanionCommand command)
    {
        Instance?.RunNativeHumanCommandInstance(location, command);
    }

    internal static bool IsNativeHumanCommandMenuAvailable(Location location, Hero? hero, IInteractableWithHero interactable)
    {
        return Instance != null && Instance.IsNativeHumanCommandMenuAvailableInstance(location, hero, interactable);
    }

    internal static void OpenNativeHumanCommandMenu(Location location)
    {
        Instance?.OpenNativeHumanCommandMenuInstance(location);
    }

    private bool IsNativeHumanCommandMenuAvailableInstance(Location location, Hero? hero, IInteractableWithHero interactable)
    {
        if (!_enableNativeCommandActions.Value || !_enableProofCommandPanel.Value || hero == null)
        {
            return false;
        }

        if (location == null || location.HasBeenDiscarded || !ReferenceEquals(interactable, location))
        {
            return false;
        }

        return _enabled.Value && !_researchModeOnly.Value && IsManagedAllyProof(location);
    }

    private void OpenNativeHumanCommandMenuInstance(Location location)
    {
        if (!IsNativeHumanCommandMenuAvailableInstance(location, Hero.Current, location))
        {
            Logger.LogWarning($"{PluginName} native Companion prompt blocked: proof panel unavailable for this actor.");
            return;
        }

        MarkAllyProofNotSaved();
        _panelStatus = "Companion menu opened.";
        SetDialogueVisible(true, "native Companion prompt");
        WriteLifecycleEvent("native-companion-prompt", "opened companion-style dialogue from native Companion prompt", location, tracked: true);
    }

    private bool IsNativeHumanCommandAvailableInstance(Location location, Hero? hero, IInteractableWithHero interactable, NativeHumanCompanionCommand command)
    {
        if (!_enableNativeCommandActions.Value || _enableProofCommandPanel.Value || hero == null)
        {
            return false;
        }

        if (location == null || location.HasBeenDiscarded || !ReferenceEquals(interactable, location))
        {
            return false;
        }

        return IsProofCommandAvailable(command);
    }

    private void RunNativeHumanCommandInstance(Location location, NativeHumanCompanionCommand command)
    {
        if (!IsNativeHumanCommandAvailableInstance(location, Hero.Current, location, command))
        {
            Logger.LogWarning($"{PluginName} native human command blocked: {command} unavailable for the active proof actor.");
            return;
        }

        _panelStatus = ExecuteProofCommand(command, "native");
    }

    private bool IsProofCommandAvailable(NativeHumanCompanionCommand command)
    {
        if (!_enabled.Value || _researchModeOnly.Value || !IsManagedAllyProof(_lastAllyProof))
        {
            return false;
        }

        return command switch
        {
            NativeHumanCompanionCommand.Follow => true,
            NativeHumanCompanionCommand.Hold => _allyProofMode != HumanProofMode.Hold,
            NativeHumanCompanionCommand.Defend => _allyProofMode != HumanProofMode.Defend,
            NativeHumanCompanionCommand.ComeClose => true,
            NativeHumanCompanionCommand.Recall => true,
            NativeHumanCompanionCommand.Dismiss => true,
            _ => false,
        };
    }

    private string ExecuteProofCommand(NativeHumanCompanionCommand command, string source)
    {
        if (!IsProofCommandAvailable(command))
        {
            return GetHumanCommandBlockedStatusLine(command);
        }

        switch (command)
        {
            case NativeHumanCompanionCommand.Follow:
            {
                bool recalled = ShouldRecallForFollowCommand();
                SetAllyProofMode(HumanProofMode.Follow, recallNow: recalled, $"{source} follow command");
                return recalled ? "I'm coming." : "I'll keep close.";
            }
            case NativeHumanCompanionCommand.Hold:
                SetAllyProofMode(HumanProofMode.Hold, recallNow: false, $"{source} hold command");
                return "I'll wait here.";
            case NativeHumanCompanionCommand.Defend:
            {
                SetAllyProofMode(HumanProofMode.Defend, recallNow: false, $"{source} defend command");
                int prompted = TriggerAllyProofDefend($"{source} defend command");
                if (prompted > 0)
                {
                    DeferCompanionBrainDefendPrompt();
                    _companionBrainLastEvent = "Protecting you.";
                    return "I'll protect you.";
                }

                _companionBrainLastEvent = "Watching for threats.";
                return CountLiveHeroAttackers(Hero.Current) > 0
                    ? "I'm ready to protect you."
                    : "I'll watch for threats.";
            }
            case NativeHumanCompanionCommand.ComeClose:
                SetAllyProofMode(HumanProofMode.Follow, recallNow: true, $"{source} come close command");
                return "I'm coming.";
            case NativeHumanCompanionCommand.Recall:
                return RecallAllyProof($"{source} recall command")
                    ? "I'm coming."
                    : "I can't reach you right now.";
            case NativeHumanCompanionCommand.Dismiss:
                TryDiscardAllyProof($"{source} dismiss command");
                return "We'll part ways.";
            default:
                return "Command sent.";
        }
    }

    private void SetAllyProofMode(HumanProofMode mode, bool recallNow, string reason)
    {
        _allyProofMode = mode;
        ResetFollowLeashGrace();
        ApplyHoldMovementState(mode == HumanProofMode.Hold);
        if (recallNow)
        {
            RecallAllyProof(reason);
        }
        else
        {
            MarkAllyProofNotSaved();
        }

        Logger.LogInfo($"{PluginName} one-session ally proof mode set to {mode} by {reason}. Runtime-only; not saved.");
        WriteLifecycleEvent("mode-set", reason, _lastAllyProof, tracked: IsManagedAllyProof(_lastAllyProof));
        QueueCompanionBrainRefresh();
    }

    private bool ShouldRecallForFollowCommand()
    {
        Location? location = _lastAllyProof;
        Hero hero = Hero.Current;
        return location != null
            && !location.HasBeenDiscarded
            && hero != null
            && Vector3.Distance(location.Coords, hero.Coords) > GetFollowCatchUpDistance(HumanProofMode.Follow);
    }

    private float GetFollowCatchUpDistance(HumanProofMode mode)
    {
        float configuredDistance = _followCatchUpDistance.Value;
        return mode == HumanProofMode.Follow ? Mathf.Min(configuredDistance, FollowModeCatchUpDistance) : configuredDistance;
    }

    private float GetAutomaticCatchUpTriggerDistance(HumanProofMode mode)
    {
        if (IsResponsiveNativeAssistEnabled() && mode == HumanProofMode.Follow)
        {
            return GetResponsiveFollowLeashDistance();
        }

        return GetFollowCatchUpDistance(mode) + AutoRecallTriggerBufferMeters;
    }

    private void TickAllyProofCommands()
    {
        if (!_enableOneSessionAllyProof.Value || _researchModeOnly.Value)
        {
            return;
        }

        if (!_enableCompanionBrain.Value && !_enableNativeCommandActions.Value && !_enableProofCommandPanel.Value)
        {
            return;
        }

        Location? location = _lastAllyProof;
        if (location == null || location.HasBeenDiscarded)
        {
            return;
        }

        if (_enableNativeCommandActions.Value || _enableProofCommandPanel.Value)
        {
            EnsureNativeCommandActions(location);
        }
        else
        {
            RemoveNativeCommandActions(location);
        }

        if (IsAiRuntimeOwned)
        {
            return;
        }

        if (_enableCompanionBrain.Value)
        {
            TickCompanionBrain(location);
            return;
        }

        if (_enableFollowCatchUp.Value && _allyProofMode != HumanProofMode.Hold && Time.realtimeSinceStartup >= _nextCommandTick)
        {
            _nextCommandTick = Time.realtimeSinceStartup + CommandTickSeconds;
            Hero hero = Hero.Current;
            if (hero != null)
            {
                float distance = Vector3.Distance(location.Coords, hero.Coords);
                float triggerDistance = GetAutomaticCatchUpTriggerDistance(_allyProofMode);
                if (distance > triggerDistance && ShouldRunFollowRecallAfterGrace(distance, triggerDistance, Time.realtimeSinceStartup))
                {
                    RecallAllyProof("follow catch-up");
                }
            }
        }

        if (_enableNativeDefendAssist.Value && _allyProofMode == HumanProofMode.Defend && Time.realtimeSinceStartup >= _nextDefendTick)
        {
            _nextDefendTick = Time.realtimeSinceStartup + DefendTickSeconds;
            TriggerAllyProofDefend("defend mode");
        }
    }

    private void TickCompanionBrain(Location location)
    {
        float now = Time.realtimeSinceStartup;
        if (now < _nextCompanionBrainTick)
        {
            return;
        }

        _nextCompanionBrainTick = now + GetCompanionBrainTickInterval();
        if (!IsManagedAllyProof(location))
        {
            return;
        }

        Hero hero = Hero.Current;
        if (hero == null)
        {
            _companionBrainLastEvent = "Waiting for hero.";
            return;
        }

        location.MarkedNotSaved = true;
        int attackerCount = CountLiveHeroAttackers(hero);
        TrackCompanionBrainThreatState(attackerCount);

        if (_allyProofMode == HumanProofMode.Hold)
        {
            _companionBrainLastEvent = attackerCount > 0
                ? $"Holding this spot; threats {attackerCount}."
                : _hasHoldAnchor ? "Holding this spot." : "Holding position.";
            return;
        }

        float distance = Vector3.Distance(location.Coords, hero.Coords);
        bool recalled = false;
        if (_enableCompanionBrainEmergencyRecall.Value
            && distance > GetCompanionBrainEmergencyRecallDistance())
        {
            bool didRecall = RecallAllyProof("companion brain emergency recall");
            _nextCommandTick = now + CommandTickSeconds;
            _companionBrainLastEvent = didRecall
                ? $"Regrouping from {distance:0.#}m."
                : $"Staying ready at {distance:0.#}m.";
            recalled = true;
        }

        if (!recalled
            && ShouldCompanionBrainProtect(attackerCount)
            && distance > GetCompanionBrainCombatRecallDistance() + AutoRecallTriggerBufferMeters)
        {
            string reason = _allyProofMode == HumanProofMode.Defend
                ? "companion brain combat recall"
                : "companion brain proactive combat recall";
            bool didRecall = RecallAllyProof(reason);
            _nextCommandTick = now + CommandTickSeconds;
            _companionBrainLastEvent = didRecall
                ? $"Moving into guard position; threats {attackerCount}."
                : $"Holding guard position; threats {attackerCount}.";
            recalled = true;
        }

        float followTriggerDistance = GetAutomaticCatchUpTriggerDistance(_allyProofMode);
        if (!recalled && _enableFollowCatchUp.Value && distance > followTriggerDistance)
        {
            string reason = _allyProofMode == HumanProofMode.Defend
                ? "companion brain guard catch-up"
                : "companion brain follow catch-up";
            if (!ShouldRunFollowRecallAfterGrace(distance, followTriggerDistance, now))
            {
                _companionBrainLastEvent = $"Catching up from {distance:0.#}m.";
                return;
            }

            bool didRecall = RecallAllyProof(reason);
            _nextCommandTick = now + CommandTickSeconds;
            _companionBrainLastEvent = didRecall
                ? $"Closing distance from {distance:0.#}m."
                : $"Keeping pace at {distance:0.#}m.";
            recalled = true;
        }

        if (ShouldCompanionBrainProtect(attackerCount))
        {
            string reason = _allyProofMode == HumanProofMode.Defend
                ? "companion brain defend"
                : "companion brain proactive defend";
            TryCompanionBrainDefend(attackerCount, now, reason);
        }
        else if (!recalled && attackerCount <= 0)
        {
            ResetFollowLeashGrace();
            _companionBrainLastEvent = _allyProofMode == HumanProofMode.Defend
                ? "Watching for threats."
                : "Keeping close.";
        }
    }

    private bool ShouldRunFollowRecallAfterGrace(float distance, float triggerDistance, float now)
    {
        if (_allyProofMode != HumanProofMode.Follow || !IsResponsiveNativeAssistEnabled())
        {
            return true;
        }

        if (distance <= triggerDistance)
        {
            ResetFollowLeashGrace();
            return false;
        }

        float graceSeconds = GetResponsiveFollowRecallGraceSeconds();
        if (graceSeconds <= 0f)
        {
            return true;
        }

        if (_followLeashExceededSince <= 0f)
        {
            _followLeashExceededSince = now;
            return false;
        }

        return now - _followLeashExceededSince >= graceSeconds;
    }

    private void ResetFollowLeashGrace()
    {
        _followLeashExceededSince = 0f;
    }

    private bool ShouldCompanionBrainProtect(int attackerCount)
    {
        if (attackerCount <= 0 || _allyProofMode == HumanProofMode.Hold)
        {
            return false;
        }

        return _allyProofMode == HumanProofMode.Defend
            || IsResponsiveNativeAssistEnabled()
            || _enableNativeDefendAssist.Value;
    }

    private bool IsResponsiveNativeAssistEnabled()
    {
        return _enableCompanionBrain.Value && _enableCompanionBrainResponsiveAssist.Value;
    }

    private float GetResponsiveFollowLeashDistance()
    {
        return GetBoundedConfigValue(_responsiveFollowLeashDistance, ResponsiveFollowCatchUpDistance, 8f, 60f);
    }

    private float GetResponsiveBrainTickSeconds()
    {
        return GetBoundedConfigValue(_responsiveBrainTickSeconds, ResponsiveBrainTickSeconds, 0.5f, 5f);
    }

    private float GetResponsiveFollowRecallGraceSeconds()
    {
        return GetBoundedConfigValue(_responsiveFollowRecallGraceSeconds, ResponsiveFollowRecallGraceSeconds, 0f, 10f);
    }

    private float GetResponsiveCombatRecallDistance()
    {
        return GetBoundedConfigValue(_responsiveCombatRecallDistance, ResponsiveCombatRecallDistance, 6f, 60f);
    }

    private float GetResponsiveFollowRecallCooldown()
    {
        return GetBoundedConfigValue(_responsiveFollowRecallCooldownSeconds, ResponsiveFollowAutoRecallCooldownSeconds, 0.5f, 15f);
    }

    private float GetResponsiveCombatRecallCooldown()
    {
        return GetBoundedConfigValue(_responsiveCombatRecallCooldownSeconds, ResponsiveCombatAutoRecallCooldownSeconds, 0.5f, 15f);
    }

    private float GetResponsiveEmergencyRecallCooldown()
    {
        return GetBoundedConfigValue(_responsiveEmergencyRecallCooldownSeconds, ResponsiveEmergencyAutoRecallCooldownSeconds, 1f, 30f);
    }

    private float GetResponsiveDefendPromptCooldown()
    {
        return GetBoundedConfigValue(_responsiveDefendPromptCooldownSeconds, ResponsiveDefendPromptCooldownSeconds, 0.5f, 15f);
    }

    private static float GetBoundedConfigValue(ConfigEntry<float> entry, float fallback, float min, float max)
    {
        float value = entry.Value;
        if (float.IsNaN(value))
        {
            value = fallback;
        }

        return Mathf.Clamp(value, min, max);
    }

    private void TryCompanionBrainDefend(int attackerCount, float now, string reason)
    {
        if (now < _nextCompanionBrainDefendPromptAt)
        {
            _companionBrainLastEvent = $"Protecting you; threats {attackerCount}.";
            return;
        }

        float cooldown = GetCompanionBrainDefendPromptCooldown();
        int prompted = TriggerAllyProofDefend(reason);
        _nextCompanionBrainDefendPromptAt = now + cooldown;
        if (prompted <= 0)
        {
            _companionBrainLastEvent = $"Ready to protect you; threats {attackerCount}.";
            return;
        }

        _nextDefendTick = now + DefendTickSeconds;
        _companionBrainLastEvent = $"Protecting you; threats {attackerCount}.";
        if (now >= _nextCompanionBrainLogAt)
        {
            _nextCompanionBrainLogAt = now + CompanionBrainLogSeconds;
            Logger.LogInfo($"{PluginName} companion brain prompted native defend. Reason={reason}; Attackers={attackerCount}; cooldown={cooldown:0.##}s; touchesTargeting=false; touchesPersistence=false.");
        }

        WriteLifecycleEvent("brain-defend", $"attackers={attackerCount}; reason={reason}; native NpcHeroPetAlly.EnterCombat path only", _lastAllyProof, tracked: IsManagedAllyProof(_lastAllyProof));
    }

    private void TrackCompanionBrainThreatState(int attackerCount)
    {
        bool hasAttackers = attackerCount > 0;
        if (hasAttackers)
        {
            if (!_companionBrainLastHadAttackers || _companionBrainLastAttackerCount != attackerCount)
            {
                _companionBrainLastEvent = $"Threats nearby: {attackerCount}.";
                Logger.LogInfo($"{PluginName} companion brain detected {attackerCount} live hero attacker(s). Native-safe defend prompting is armed.");
                WriteLifecycleEvent("brain-threat-detected", $"attackers={attackerCount}; native-safe observation only", _lastAllyProof, tracked: IsManagedAllyProof(_lastAllyProof));
            }
        }
        else if (_companionBrainLastHadAttackers)
        {
            _nextCompanionBrainDefendPromptAt = 0f;
            _companionBrainLastEvent = "Threats cleared; staying close.";
            Logger.LogInfo($"{PluginName} companion brain cleared threat state because the hero has no live attackers.");
            WriteLifecycleEvent("brain-threat-cleared", "hero has no live attackers", _lastAllyProof, tracked: IsManagedAllyProof(_lastAllyProof));
        }

        _companionBrainLastHadAttackers = hasAttackers;
        _companionBrainLastAttackerCount = attackerCount;
    }

    private float GetCompanionBrainTickInterval()
    {
        float seconds = _companionBrainTickSeconds.Value;
        if (float.IsNaN(seconds) || seconds < CompanionBrainMinimumTickSeconds)
        {
            return CompanionBrainMinimumTickSeconds;
        }

        return IsResponsiveNativeAssistEnabled()
            ? GetResponsiveBrainTickSeconds()
            : seconds;
    }

    private float GetCompanionBrainCombatRecallDistance()
    {
        float distance = _companionBrainCombatRecallDistance.Value;
        distance = float.IsNaN(distance) || distance < 6f ? 6f : distance;
        return IsResponsiveNativeAssistEnabled()
            ? Mathf.Min(distance, GetResponsiveCombatRecallDistance())
            : distance;
    }

    private float GetCompanionBrainEmergencyRecallDistance()
    {
        float distance = _companionBrainEmergencyRecallDistance.Value;
        return float.IsNaN(distance) || distance < 12f ? 12f : distance;
    }

    private HumanCompanionBrainProfile GetCompanionBrainProfile()
    {
        if (!_enableCompanionBrainRolePlacement.Value)
        {
            return GetDefaultCompanionBrainProfile();
        }

        return GetSelectedRoleLabel() switch
        {
            "One-handed melee" => new HumanCompanionBrainProfile("One-handed melee", followBack: 3.0f, followRight: 1.8f, combatBack: 2.8f, combatRight: 1.6f),
            "Heavy melee" => new HumanCompanionBrainProfile("Heavy melee", followBack: 3.6f, followRight: -1.8f, combatBack: 3.2f, combatRight: -2.0f),
            "Spear and shield" => new HumanCompanionBrainProfile("Spear and shield", followBack: 3.2f, followRight: 0.4f, combatBack: 2.6f, combatRight: 0.6f),
            "Ranged support" => new HumanCompanionBrainProfile("Ranged support", followBack: 5.4f, followRight: -2.4f, combatBack: 7.2f, combatRight: -3.0f),
            "Armored melee" => new HumanCompanionBrainProfile("Armored melee", followBack: 3.4f, followRight: 0.8f, combatBack: 3.0f, combatRight: 1.2f),
            _ => GetDefaultCompanionBrainProfile(),
        };
    }

    private static HumanCompanionBrainProfile GetDefaultCompanionBrainProfile()
    {
        return new HumanCompanionBrainProfile("Standard", followBack: 3.2f, followRight: 1.8f, combatBack: 3.2f, combatRight: 1.8f);
    }

    private void QueueCompanionBrainRefresh()
    {
        _nextCompanionBrainTick = 0f;
    }

    private void DeferCompanionBrainDefendPrompt()
    {
        _nextCompanionBrainDefendPromptAt = Mathf.Max(
            _nextCompanionBrainDefendPromptAt,
            Time.realtimeSinceStartup + GetCompanionBrainDefendPromptCooldown());
    }

    private float GetCompanionBrainDefendPromptCooldown()
    {
        return IsResponsiveNativeAssistEnabled()
            ? GetResponsiveDefendPromptCooldown()
            : CompanionBrainDefendPromptCooldownSeconds;
    }

    private void ResetCompanionBrainState(string lastEvent = "Idle.")
    {
        _nextCompanionBrainTick = 0f;
        _nextCompanionBrainDefendPromptAt = 0f;
        _nextCompanionBrainLogAt = 0f;
        _lastAllyRecallAt = 0f;
        _nextFollowAutoRecallAllowedAt = 0f;
        _nextCombatAutoRecallAllowedAt = 0f;
        _nextEmergencyAutoRecallAllowedAt = 0f;
        ResetFollowLeashGrace();
        _companionBrainLastHadAttackers = false;
        _companionBrainLastAttackerCount = 0;
        _companionBrainLastEvent = lastEvent;
        _lastAllyRecallReason = string.Empty;
        _hasLastAllyRecallTarget = false;
        _lastAllyRecallTarget = default;
        ClearHoldAnchor();
    }

    private void TickAllyProofLifecycle()
    {
        Location? location = _lastAllyProof;
        if (location == null)
        {
            return;
        }

        if (location.HasBeenDiscarded)
        {
            WriteLifecycleEvent("tracked-proof-discarded", "tracked proof reference was already discarded", location, tracked: false);
            _lastAllyProof = null;
            _allyProofMode = HumanProofMode.Follow;
            ResetCompanionBrainState();
            return;
        }

        if (_researchModeOnly.Value)
        {
            TryDiscardAllyProof("research mode re-enabled");
            return;
        }

        if (_enableLifecycleSafetyGuard.Value)
        {
            location.MarkedNotSaved = true;
            if (!IsManagedAllyProof(location))
            {
                RemoveNativeCommandActions(location);
                RemoveHoldMovementBlock(location);
                location.MarkedNotSaved = true;
                WriteLifecycleEvent("guard-discard-invalid", "active proof actor lost expected managed ally marker", location, tracked: false);
                location.Discard();
                _lastAllyProof = null;
                _allyProofMode = HumanProofMode.Follow;
                ResetCompanionBrainState();
                Logger.LogWarning($"{PluginName} lifecycle guard discarded the active proof actor because it lost the expected managed ally marker.");
                return;
            }
        }

        if (_writeLifecycleDiagnostics.Value && Time.realtimeSinceStartup >= _nextLifecycleTick)
        {
            _nextLifecycleTick = Time.realtimeSinceStartup + Math.Max(1f, _lifecycleTickSeconds.Value);
            WriteLifecycleEvent("guard-tick", "periodic lifecycle guard", location, tracked: true);
        }
    }

    private bool RecallAllyProof(string reason)
    {
        Location? location = _lastAllyProof;
        Hero hero = Hero.Current;
        if (location == null || location.HasBeenDiscarded || hero == null)
        {
            Logger.LogWarning($"{PluginName} one-session ally proof recall skipped during {reason}: no active proof actor or hero.");
            return false;
        }

        HumanCompanionBrainProfile profile = GetCompanionBrainProfile();
        Vector3 offset = GetCompanionRecallOffset(reason, profile);
        Vector3 target = hero.Coords + hero.Rotation * offset;
        float now = Time.realtimeSinceStartup;
        if (IsAutomaticRecallThrottled(reason, now))
        {
            location.MarkedNotSaved = true;
            DeferCompanionBrainAfterAutomaticRecall(now);
            return false;
        }

        if (IsDuplicateAutomaticRecall(reason, target, now))
        {
            location.MarkedNotSaved = true;
            DeferCompanionBrainAfterAutomaticRecall(now);
            return false;
        }

        location.MarkedNotSaved = true;
        location.MoveAndRotateTo(target, hero.Rotation, teleport: true);
        ResetFollowLeashGrace();
        RecordAllyRecall(reason, target, now);
        if (_allyProofMode == HumanProofMode.Hold)
        {
            SetHoldAnchor(location);
        }

        Logger.LogInfo($"{PluginName} one-session ally proof recalled during {reason}. Role={profile.RoleLabel}; Offset={offset}; Location remains marked not saved.");
        WriteLifecycleEvent("recall", reason, location, tracked: true);
        if (IsAutomaticRecallReason(reason))
        {
            MarkAutomaticRecallCooldown(reason, now);
            DeferCompanionBrainAfterAutomaticRecall(now);
        }
        else
        {
            QueueCompanionBrainRefresh();
        }

        return true;
    }

    private bool IsAutomaticRecallThrottled(string reason, float now)
    {
        if (!IsAutomaticRecallReason(reason))
        {
            return false;
        }

        if (IsEmergencyRecallReason(reason))
        {
            return now < _nextEmergencyAutoRecallAllowedAt;
        }

        if (IsCombatRecallReason(reason))
        {
            return now < _nextCombatAutoRecallAllowedAt;
        }

        return now < _nextFollowAutoRecallAllowedAt;
    }

    private void MarkAutomaticRecallCooldown(string reason, float now)
    {
        if (!IsAutomaticRecallReason(reason))
        {
            return;
        }

        if (IsEmergencyRecallReason(reason))
        {
            _nextEmergencyAutoRecallAllowedAt = now + GetEmergencyAutoRecallCooldown();
            return;
        }

        if (IsCombatRecallReason(reason))
        {
            _nextCombatAutoRecallAllowedAt = now + GetCombatAutoRecallCooldown();
            return;
        }

        _nextFollowAutoRecallAllowedAt = now + GetFollowAutoRecallCooldown();
    }

    private float GetFollowAutoRecallCooldown()
    {
        return IsResponsiveNativeAssistEnabled()
            ? GetResponsiveFollowRecallCooldown()
            : FollowAutoRecallCooldownSeconds;
    }

    private float GetCombatAutoRecallCooldown()
    {
        return IsResponsiveNativeAssistEnabled()
            ? GetResponsiveCombatRecallCooldown()
            : CombatAutoRecallCooldownSeconds;
    }

    private float GetEmergencyAutoRecallCooldown()
    {
        return IsResponsiveNativeAssistEnabled()
            ? GetResponsiveEmergencyRecallCooldown()
            : EmergencyAutoRecallCooldownSeconds;
    }

    private bool IsDuplicateAutomaticRecall(string reason, Vector3 target, float now)
    {
        return IsAutomaticRecallReason(reason)
            && _hasLastAllyRecallTarget
            && now - _lastAllyRecallAt <= AutoRecallDuplicateWindowSeconds
            && string.Equals(_lastAllyRecallReason, reason, StringComparison.OrdinalIgnoreCase)
            && Vector3.Distance(_lastAllyRecallTarget, target) <= RecallTargetDuplicateMeters;
    }

    private void RecordAllyRecall(string reason, Vector3 target, float now)
    {
        _lastAllyRecallReason = reason;
        _lastAllyRecallTarget = target;
        _lastAllyRecallAt = now;
        _hasLastAllyRecallTarget = true;
    }

    private void DeferCompanionBrainAfterAutomaticRecall(float now)
    {
        _nextCompanionBrainTick = Mathf.Max(_nextCompanionBrainTick, now + GetCompanionBrainTickInterval());
    }

    private static bool IsAutomaticRecallReason(string reason)
    {
        return reason.IndexOf("catch-up", StringComparison.OrdinalIgnoreCase) >= 0
            || reason.IndexOf("companion brain", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static bool IsEmergencyRecallReason(string reason)
    {
        return reason.IndexOf("emergency", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private Vector3 GetCompanionRecallOffset(string reason, HumanCompanionBrainProfile profile)
    {
        if (IsCloseRecallReason(reason))
        {
            return (Vector3.back * 3.2f) + (Vector3.right * 1.8f);
        }

        bool combatPlacement = IsCombatRecallReason(reason);
        float back = combatPlacement ? profile.CombatBack : profile.FollowBack;
        float right = combatPlacement ? profile.CombatRight : profile.FollowRight;
        return (Vector3.back * back) + (Vector3.right * right);
    }

    private static bool IsCloseRecallReason(string reason)
    {
        return reason.IndexOf("come close", StringComparison.OrdinalIgnoreCase) >= 0
            || reason.IndexOf("recall command", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private bool IsCombatRecallReason(string reason)
    {
        return reason.IndexOf("combat", StringComparison.OrdinalIgnoreCase) >= 0
            || (_allyProofMode == HumanProofMode.Defend && CountLiveHeroAttackers(Hero.Current) > 0);
    }

    private int TriggerAllyProofDefend(string reason)
    {
        Location? location = _lastAllyProof;
        Hero hero = Hero.Current;
        if (location == null || location.HasBeenDiscarded || hero == null || !HeroHasAttackers(hero))
        {
            return 0;
        }

        if (!location.TryGetElement(out NpcElement npcElement) || npcElement == null)
        {
            return 0;
        }

        NpcHeroPetAlly marker = npcElement.TryGetElement<NpcHeroPetAlly>();
        if (marker == null || marker.HasBeenDiscarded)
        {
            return 0;
        }

        location.MarkedNotSaved = true;
        marker.EnterCombat();
        Logger.LogInfo($"{PluginName} one-session ally proof defend prompted native NpcHeroPetAlly.EnterCombat during {reason}.");
        return 1;
    }

    private static bool HeroHasAttackers(Hero hero)
    {
        return CountLiveHeroAttackers(hero) > 0;
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

    private void EnsureNativeCommandActions(Location location)
    {
        if (!_enableNativeCommandActions.Value || !IsManagedAllyProof(location))
        {
            RemoveNativeCommandActions(location);
            return;
        }

        if (_enableProofCommandPanel.Value)
        {
            RemoveNativeQuickCommandActions(location);
            bool promptAttached = EnsureNativeCommandMenuAction(location);
            if (promptAttached)
            {
                location.MarkedNotSaved = true;
                Logger.LogInfo($"{PluginName} runtime-only native Companion prompt attached to {location.Template?.name}[{location.Template?.GUID}].");
            }

            return;
        }

        if (location.HasElement<HumanCompanionCommandAction>())
        {
            location.RemoveElementsOfType<HumanCompanionCommandAction>();
        }

        bool attached = false;
        attached |= EnsureNativeCommandAction<HumanCompanionFollowAction>(location);
        attached |= EnsureNativeCommandAction<HumanCompanionHoldAction>(location);
        attached |= EnsureNativeCommandAction<HumanCompanionDefendAction>(location);
        attached |= EnsureNativeCommandAction<HumanCompanionComeCloseAction>(location);
        attached |= EnsureNativeCommandAction<HumanCompanionRecallAction>(location);
        attached |= EnsureNativeCommandAction<HumanCompanionDismissAction>(location);

        if (attached)
        {
            location.MarkedNotSaved = true;
            Logger.LogInfo($"{PluginName} runtime-only native human command actions attached to {location.Template?.name}[{location.Template?.GUID}].");
        }
    }

    private static bool EnsureNativeCommandMenuAction(Location location)
    {
        if (location.HasElement<HumanCompanionCommandAction>())
        {
            return false;
        }

        HumanCompanionCommandAction action = location.AddElement(new HumanCompanionCommandAction());
        action.MarkedNotSaved = true;
        return true;
    }

    private static bool EnsureNativeCommandAction<TAction>(Location location)
        where TAction : HumanCompanionQuickCommandAction, new()
    {
        if (location.HasElement<TAction>())
        {
            return false;
        }

        TAction action = location.AddElement(new TAction());
        action.MarkedNotSaved = true;
        return true;
    }

    private static void RemoveNativeCommandActions(Location location)
    {
        if (location == null || location.HasBeenDiscarded)
        {
            return;
        }

        location.RemoveElementsOfType<HumanCompanionCommandAction>();
        RemoveNativeQuickCommandActions(location);
    }

    private static bool RemoveNativeQuickCommandActions(Location location)
    {
        if (location == null || location.HasBeenDiscarded)
        {
            return false;
        }

        bool changed = false;
        if (location.HasElement<HumanCompanionFollowAction>())
        {
            location.RemoveElementsOfType<HumanCompanionFollowAction>();
            changed = true;
        }

        if (location.HasElement<HumanCompanionHoldAction>())
        {
            location.RemoveElementsOfType<HumanCompanionHoldAction>();
            changed = true;
        }

        if (location.HasElement<HumanCompanionDefendAction>())
        {
            location.RemoveElementsOfType<HumanCompanionDefendAction>();
            changed = true;
        }

        if (location.HasElement<HumanCompanionComeCloseAction>())
        {
            location.RemoveElementsOfType<HumanCompanionComeCloseAction>();
            changed = true;
        }

        if (location.HasElement<HumanCompanionRecallAction>())
        {
            location.RemoveElementsOfType<HumanCompanionRecallAction>();
            changed = true;
        }

        if (location.HasElement<HumanCompanionDismissAction>())
        {
            location.RemoveElementsOfType<HumanCompanionDismissAction>();
            changed = true;
        }

        return changed;
    }

    private void ApplyHoldMovementState(bool hold)
    {
        Location? location = _lastAllyProof;
        if (!IsManagedAllyProof(location) || !location!.TryGetElement(out NpcElement npcElement) || npcElement == null)
        {
            if (!hold)
            {
                ClearHoldAnchor();
            }

            return;
        }

        if (hold)
        {
            SetHoldAnchor(location);
            if (!npcElement.HasElement<HumanCompanionHoldMovementBlock>())
            {
                HumanCompanionHoldMovementBlock blocker = npcElement.AddElement(new HumanCompanionHoldMovementBlock());
                blocker.MarkedNotSaved = true;
                location.MarkedNotSaved = true;
                Logger.LogInfo($"{PluginName} one-session ally proof hold movement block applied. Runtime-only; not saved.");
            }

            return;
        }

        RemoveHoldMovementBlock(location);
        ClearHoldAnchor();
    }

    private void SetHoldAnchor(Location location)
    {
        if (location == null || location.HasBeenDiscarded)
        {
            ClearHoldAnchor();
            return;
        }

        _holdAnchor = location.Coords;
        _hasHoldAnchor = true;
    }

    private void ClearHoldAnchor()
    {
        _holdAnchor = default;
        _hasHoldAnchor = false;
    }

    private static void RemoveHoldMovementBlock(Location location)
    {
        if (location == null || location.HasBeenDiscarded)
        {
            return;
        }

        if (location.TryGetElement(out NpcElement npcElement) && npcElement != null)
        {
            npcElement.RemoveElementsOfType<HumanCompanionHoldMovementBlock>();
        }
    }

    private bool IsManagedAllyProof(Location? location)
    {
        if (location == null || location.HasBeenDiscarded || _lastAllyProof == null || !ReferenceEquals(location, _lastAllyProof))
        {
            return false;
        }

        if (!location.TryGetElement(out NpcElement npcElement) || npcElement == null)
        {
            return false;
        }

        NpcHeroPetAlly marker = npcElement.TryGetElement<NpcHeroPetAlly>();
        return marker != null && !marker.HasBeenDiscarded;
    }

    private void MarkAllyProofNotSaved()
    {
        if (_lastAllyProof != null && !_lastAllyProof.HasBeenDiscarded)
        {
            _lastAllyProof.MarkedNotSaved = true;
        }
    }

    private void DrawPanelWindow(int windowId)
    {
        GUILayout.BeginVertical();

        GUILayout.BeginHorizontal(_headerStyle!, GUILayout.Height(66f));
        GUILayout.BeginVertical();
        GUILayout.Label("Avalon Human Companions", _titleStyle!);
        GUILayout.Label($"{_togglePanelHotkey.Value}: companion panel   Esc: close", _mutedStyle!);
        GUILayout.EndVertical();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Close", _dangerButtonStyle!, GUILayout.Width(88f), GUILayout.Height(38f)))
        {
            SetPanelVisible(false, "close button");
            ClearPanelControlFocus();
        }

        GUILayout.EndHorizontal();

        GUILayout.Space(10f);
        _panelScrollPosition = GUILayout.BeginScrollView(_panelScrollPosition, false, true);
        GUILayout.BeginVertical(_sectionStyle!, GUILayout.MinHeight(236f));
        GUILayout.BeginHorizontal();
        bool drewIcon = DrawSelectedHumanIcon(104f);
        if (drewIcon)
        {
            GUILayout.Space(14f);
        }

        GUILayout.BeginVertical();
        GUILayout.Label($"Selected: {GetTargetNameLabel()}", _labelStyle!);
        GUILayout.Label($"Role: {GetSelectedRoleLabel()}", _mutedStyle!);
        GUILayout.Label($"Available: {StarterCandidateRoster.Length} companions", _mutedStyle!);
        GUILayout.Label($"Active: {GetActiveProofNameLabel()}", _mutedStyle!);
        GUILayout.Label($"Stance: {GetCompanionStanceLabel()} - {GetModeFeedbackLine()}", _mutedStyle!);
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
        GUILayout.Space(8f);
        foreach (string line in GetActiveProofStatusLines())
        {
            GUILayout.Label(line, _mutedStyle!);
        }

        GUILayout.EndVertical();

        GUILayout.Space(10f);
        GUILayout.BeginHorizontal();
        DrawPanelSelectButton("Prev", -1);
        DrawPanelSpawnButton();
        DrawPanelSelectButton("Next", 1);
        DrawPanelCommandButton("Recall", NativeHumanCompanionCommand.Recall, selected: false);
        DrawPanelCommandButton("Come Close", NativeHumanCompanionCommand.ComeClose, selected: false);
        GUILayout.EndHorizontal();

        GUILayout.Space(8f);
        GUILayout.BeginHorizontal();
        DrawPanelCommandButton("Follow", NativeHumanCompanionCommand.Follow, _allyProofMode == HumanProofMode.Follow);
        DrawPanelCommandButton("Hold", NativeHumanCompanionCommand.Hold, _allyProofMode == HumanProofMode.Hold);
        DrawPanelCommandButton("Defend", NativeHumanCompanionCommand.Defend, _allyProofMode == HumanProofMode.Defend);
        GUILayout.EndHorizontal();

        GUILayout.Space(8f);
        GUILayout.BeginHorizontal();
        DrawPanelCommandButton("Dismiss", NativeHumanCompanionCommand.Dismiss, selected: false, danger: true);
        GUILayout.EndHorizontal();

        GUILayout.Space(8f);
        DrawPanelBrainTuningControls();

        GUILayout.BeginVertical(_footerStyle!, GUILayout.MinHeight(58f));
        GUILayout.Label("Status: " + _panelStatus, _mutedStyle!);
        GUILayout.EndVertical();
        GUILayout.EndScrollView();
        GUILayout.EndVertical();

        GUI.DragWindow(new Rect(0f, 0f, Mathf.Max(180f, _panelRect.width - 104f), 72f));

        Event current = Event.current;
        if (current.type == EventType.MouseDown || current.type == EventType.MouseDrag || current.type == EventType.MouseUp)
        {
            current.Use();
        }
    }

    private void DrawPanelBrainTuningControls()
    {
        GUILayout.BeginVertical(_sectionStyle!);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Live Tuning", _labelStyle!);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Save Config", _buttonStyle!, GUILayout.Width(150f), GUILayout.Height(34f)))
        {
            SaveResponsiveTuningConfig();
            ClearPanelControlFocus();
        }

        if (GUILayout.Button("Reset Defaults", _dangerButtonStyle!, GUILayout.Width(170f), GUILayout.Height(34f)))
        {
            ResetResponsiveTuningDefaults();
            ClearPanelControlFocus();
        }

        GUILayout.EndHorizontal();
        GUILayout.Space(6f);
        DrawTuningValueRow("Brain tick", _responsiveBrainTickSeconds, ResponsiveBrainTickSeconds, 0.1f, 0.5f, 5f, "s");
        DrawTuningValueRow("Follow leash", _responsiveFollowLeashDistance, ResponsiveFollowCatchUpDistance, 1f, 8f, 60f, "m");
        DrawTuningValueRow("Follow grace", _responsiveFollowRecallGraceSeconds, ResponsiveFollowRecallGraceSeconds, 0.25f, 0f, 10f, "s");
        DrawTuningValueRow("Combat recall", _responsiveCombatRecallDistance, ResponsiveCombatRecallDistance, 1f, 6f, 60f, "m");
        DrawTuningValueRow("Follow cooldown", _responsiveFollowRecallCooldownSeconds, ResponsiveFollowAutoRecallCooldownSeconds, 0.25f, 0.5f, 15f, "s");
        DrawTuningValueRow("Combat cooldown", _responsiveCombatRecallCooldownSeconds, ResponsiveCombatAutoRecallCooldownSeconds, 0.25f, 0.5f, 15f, "s");
        DrawTuningValueRow("Emergency cooldown", _responsiveEmergencyRecallCooldownSeconds, ResponsiveEmergencyAutoRecallCooldownSeconds, 0.5f, 1f, 30f, "s");
        DrawTuningValueRow("Defend prompt", _responsiveDefendPromptCooldownSeconds, ResponsiveDefendPromptCooldownSeconds, 0.25f, 0.5f, 15f, "s");
        GUILayout.EndVertical();
    }

    private void DrawTuningValueRow(
        string label,
        ConfigEntry<float> entry,
        float fallback,
        float step,
        float min,
        float max,
        string suffix)
    {
        float value = GetBoundedConfigValue(entry, fallback, min, max);
        GUILayout.BeginHorizontal();
        GUILayout.Label(label, _mutedStyle!, GUILayout.Width(210f));
        GUILayout.Label($"{value:0.##}{suffix}", _labelStyle!, GUILayout.Width(86f));
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("-", _buttonStyle!, GUILayout.Width(42f), GUILayout.Height(30f)))
        {
            SetResponsiveTuningValue(label, entry, value - step, fallback, min, max, suffix);
            ClearPanelControlFocus();
        }

        if (GUILayout.Button("+", _buttonStyle!, GUILayout.Width(42f), GUILayout.Height(30f)))
        {
            SetResponsiveTuningValue(label, entry, value + step, fallback, min, max, suffix);
            ClearPanelControlFocus();
        }

        GUILayout.EndHorizontal();
    }

    private void SetResponsiveTuningValue(
        string label,
        ConfigEntry<float> entry,
        float requestedValue,
        float fallback,
        float min,
        float max,
        string suffix)
    {
        float value = requestedValue;
        if (float.IsNaN(value))
        {
            value = fallback;
        }

        value = Mathf.Clamp(value, min, max);
        entry.Value = value;
        ResetFollowLeashGrace();
        QueueCompanionBrainRefresh();
        _panelStatus = $"{label}: {value:0.##}{suffix} live.";
    }

    private void SaveResponsiveTuningConfig()
    {
        ClampResponsiveTuningValues();
        Config.Save();
        Logger.LogInfo($"{PluginName} saved responsive tuning. {GetResponsiveTuningSummary()}");
        _panelStatus = "Responsive tuning saved.";
    }

    private void ResetResponsiveTuningDefaults()
    {
        _responsiveBrainTickSeconds.Value = ResponsiveBrainTickSeconds;
        _responsiveFollowLeashDistance.Value = ResponsiveFollowCatchUpDistance;
        _responsiveFollowRecallGraceSeconds.Value = ResponsiveFollowRecallGraceSeconds;
        _responsiveCombatRecallDistance.Value = ResponsiveCombatRecallDistance;
        _responsiveFollowRecallCooldownSeconds.Value = ResponsiveFollowAutoRecallCooldownSeconds;
        _responsiveCombatRecallCooldownSeconds.Value = ResponsiveCombatAutoRecallCooldownSeconds;
        _responsiveEmergencyRecallCooldownSeconds.Value = ResponsiveEmergencyAutoRecallCooldownSeconds;
        _responsiveDefendPromptCooldownSeconds.Value = ResponsiveDefendPromptCooldownSeconds;
        ResetFollowLeashGrace();
        QueueCompanionBrainRefresh();
        Config.Save();
        Logger.LogInfo($"{PluginName} reset responsive tuning defaults. {GetResponsiveTuningSummary()}");
        _panelStatus = "Responsive tuning reset and saved.";
    }

    private void ClampResponsiveTuningValues()
    {
        _responsiveBrainTickSeconds.Value = GetResponsiveBrainTickSeconds();
        _responsiveFollowLeashDistance.Value = GetResponsiveFollowLeashDistance();
        _responsiveFollowRecallGraceSeconds.Value = GetResponsiveFollowRecallGraceSeconds();
        _responsiveCombatRecallDistance.Value = GetResponsiveCombatRecallDistance();
        _responsiveFollowRecallCooldownSeconds.Value = GetResponsiveFollowRecallCooldown();
        _responsiveCombatRecallCooldownSeconds.Value = GetResponsiveCombatRecallCooldown();
        _responsiveEmergencyRecallCooldownSeconds.Value = GetResponsiveEmergencyRecallCooldown();
        _responsiveDefendPromptCooldownSeconds.Value = GetResponsiveDefendPromptCooldown();
    }

    private string GetResponsiveTuningSummary()
    {
        return $"BrainTick={GetResponsiveBrainTickSeconds():0.##}s; FollowLeash={GetResponsiveFollowLeashDistance():0.##}m; FollowGrace={GetResponsiveFollowRecallGraceSeconds():0.##}s; CombatRecall={GetResponsiveCombatRecallDistance():0.##}m; FollowCooldown={GetResponsiveFollowRecallCooldown():0.##}s; CombatCooldown={GetResponsiveCombatRecallCooldown():0.##}s; EmergencyCooldown={GetResponsiveEmergencyRecallCooldown():0.##}s; DefendPromptCooldown={GetResponsiveDefendPromptCooldown():0.##}s.";
    }

    private void DrawPanelSelectButton(string label, int delta)
    {
        bool available = UseStarterCandidateSelector() && StarterCandidateRoster.Length > 1;
        bool previousEnabled = GUI.enabled;
        GUI.enabled = previousEnabled && available;

        if (GUILayout.Button(label, _buttonStyle!, GUILayout.Height(42f)))
        {
            SelectRelativeHumanCandidate(delta);
            ClearPanelControlFocus();
        }

        GUI.enabled = previousEnabled;
    }

    private bool DrawSelectedHumanIcon(float size)
    {
        Texture2D? icon = TaintedInterfaceBridge.GetFullEmbeddedIcon(GetSelectedHumanIconRelativePath());
        if (icon == null)
        {
            return false;
        }

        Rect rect = GUILayoutUtility.GetRect(size, size, GUILayout.Width(size), GUILayout.Height(size));
        Texture2D? background = TaintedInterfaceBridge.GetFullEmbeddedIconAndDialogueBackground(HumanIconBackground);
        if (background == null)
        {
            GUI.DrawTexture(rect, icon, ScaleMode.ScaleToFit, alphaBlend: true);
            return true;
        }

        GUI.DrawTexture(rect, background, ScaleMode.ScaleToFit, alphaBlend: true);
        float portraitSize = size * HumanIconPortraitScale;
        Rect portraitRect = new(
            rect.x + (size - portraitSize) * 0.5f,
            rect.y + (size - portraitSize) * 0.5f,
            portraitSize,
            portraitSize);
        GUI.DrawTexture(portraitRect, icon, ScaleMode.ScaleToFit, alphaBlend: true);
        return true;
    }

    private void DrawHumanCompanionHudIcon()
    {
        Event current = Event.current;
        if (current == null
            || current.type != EventType.Repaint
            || !_showCompanionHudIcon.Value
            || !_enabled.Value
            || _researchModeOnly.Value
            || _panelVisible
            || _dialogueVisible
            || !IsManagedAllyProof(_lastAllyProof))
        {
            return;
        }

        string iconPath = GetActiveHumanIconRelativePath();
        Texture2D? icon = GetCachedHumanHudIcon(iconPath);
        if (icon == null)
        {
            return;
        }

        float size = Mathf.Clamp(_companionHudIconSize.Value, 56, 128);
        Rect frameRect = GetHumanHudIconRect(size);
        Texture2D? background = GetCachedHumanHudBackground();
        float iconSize = background == null
            ? size
            : Mathf.Clamp(size * HumanHudIconPortraitScale, 32f, size - 18f);
        Rect iconRect = new(
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
            GUI.DrawTexture(frameRect, background, ScaleMode.ScaleToFit, alphaBlend: true);
        }

        GUI.color = new Color(1f, 1f, 1f, 0.96f);
        GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit, alphaBlend: true);
        GUI.color = previousColor;
        GUI.depth = previousDepth;
    }

    private Texture2D? GetCachedHumanHudIcon(string iconPath)
    {
        if (string.Equals(_hudCompanionIconPath, iconPath, StringComparison.OrdinalIgnoreCase) && _hudCompanionIconTexture != null)
        {
            return _hudCompanionIconTexture;
        }

        _hudCompanionIconPath = iconPath;
        _hudCompanionIconTexture = TaintedInterfaceBridge.GetFullEmbeddedIcon(iconPath);
        return _hudCompanionIconTexture;
    }

    private Texture2D? GetCachedHumanHudBackground()
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
        _hudCompanionBackgroundTexture = TaintedInterfaceBridge.GetFullEmbeddedIconAndDialogueBackground(HumanIconBackground);
        return _hudCompanionBackgroundTexture;
    }

    private Rect GetHumanHudIconRect(float size)
    {
        string corner = _companionHudIconCorner.Value?.Trim() ?? string.Empty;
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

    private void DrawPanelSpawnButton()
    {
        bool available = CanPanelSpawnProof();
        bool previousEnabled = GUI.enabled;
        GUI.enabled = previousEnabled && available;

        if (GUILayout.Button("Call / Swap", _buttonStyle!, GUILayout.Height(42f)))
        {
            TryPanelSpawnOrSwap();
            if (!IsManagedAllyProof(_lastAllyProof))
            {
                _panelStatus = "Call requested; no companion appeared. Check the log if this was unexpected.";
            }

            ClearPanelControlFocus();
        }

        GUI.enabled = previousEnabled;
    }

    private void SelectRelativeHumanCandidate(int delta)
    {
        int position = GetSelectedStarterCandidatePosition();
        int nextPosition = NormalizeStarterCandidatePosition(position + delta);
        StarterCandidateRosterEntry starter = StarterCandidateRoster[nextPosition];
        int index = starter.RosterIndex;
        _selectedCandidateIndex.Value = index;

        HumanCompanionRosterEntry selected = ProofCandidateRoster[index];
        _panelStatus = $"Selected {nextPosition + 1}/{StarterCandidateRoster.Length}: {selected.DisplayName} - {starter.RoleLabel}.";
        Logger.LogInfo($"{PluginName} selected starter human proof candidate {nextPosition + 1}/{StarterCandidateRoster.Length}; role={starter.RoleLabel}; reviewedRow={index + 1}/{ProofCandidateRoster.Length}: {selected.TemplateName}[{selected.Guid}] ({selected.ReviewId}).");

        if (IsManagedAllyProof(_lastAllyProof))
        {
            TryPanelSpawnOrSwap();
        }
    }

    private void TryPanelSpawnOrSwap()
    {
        if (!CanPanelSpawnProof())
        {
            Logger.LogWarning($"{PluginName} panel spawn/swap blocked: proof gates are not open or no human proof target is selected.");
            return;
        }

        bool hadActiveProof = IsManagedAllyProof(_lastAllyProof);
        if (hadActiveProof)
        {
            TryDiscardAllyProof("panel spawn/swap");
            _allyProofSpawnedThisSession = false;
        }

        TryOneSessionAllyProofSpawn();
        if (IsManagedAllyProof(_lastAllyProof))
        {
            _panelStatus = hadActiveProof ? "Companion swapped." : "Companion joined you.";
        }
    }

    private void DrawPanelCommandButton(string label, NativeHumanCompanionCommand command, bool selected, bool danger = false)
    {
        bool available = IsProofCommandAvailable(command);
        bool previousEnabled = GUI.enabled;
        GUI.enabled = previousEnabled && available;

        GUIStyle style = danger ? _dangerButtonStyle! : selected ? _selectedButtonStyle! : _buttonStyle!;
        if (GUILayout.Button(label, style, GUILayout.Height(42f)))
        {
            _panelStatus = ExecuteProofCommand(command, "panel");
            ClearPanelControlFocus();
        }

        GUI.enabled = previousEnabled;
    }

    private bool CanPanelSpawnProof()
    {
        if (!_enableProofCommandPanel.Value || !_enableOneSessionAllyProof.Value || _researchModeOnly.Value)
        {
            return false;
        }

        if (!HasActiveTarget())
        {
            return false;
        }

        if (!IsManagedAllyProof(_lastAllyProof)
            && _limitOneAllyProofPerSession.Value
            && _allyProofSpawnedThisSession)
        {
            return false;
        }

        return true;
    }

    private string GetTargetNameLabel()
    {
        if (UseProofCandidateRoster())
        {
            int position = GetSelectedStarterCandidatePosition();
            HumanCompanionRosterEntry selected = GetSelectedHumanCandidate();
            return $"{position + 1}/{StarterCandidateRoster.Length} {selected.DisplayName}";
        }

        return string.IsNullOrWhiteSpace(_templateName.Value)
            ? "No reviewed target selected"
            : HumanizeTemplateName(_templateName.Value);
    }

    private string GetSelectedRoleLabel()
    {
        if (!UseStarterCandidateSelector())
        {
            return "Companion";
        }

        return StarterCandidateRoster[GetSelectedStarterCandidatePosition()].RoleLabel;
    }

    private string GetCompanionStanceLabel()
    {
        return _allyProofMode switch
        {
            HumanProofMode.Follow => "Follow",
            HumanProofMode.Hold => "Hold",
            HumanProofMode.Defend => "Defend",
            _ => "Ready",
        };
    }

    private string GetSelectedHumanIconRelativePath()
    {
        if (UseProofCandidateRoster())
        {
            HumanCompanionRosterEntry selected = GetSelectedHumanCandidate();
            return GetHumanIconRelativePath(selected.DisplayName, selected.TemplateName, GetSelectedRoleLabel());
        }

        return GetHumanIconRelativePath(_templateName.Value, _templateName.Value, "Companion");
    }

    private string GetActiveHumanIconRelativePath()
    {
        Location? location = _lastAllyProof;
        string? guid = location?.Template?.GUID;
        if (!string.IsNullOrWhiteSpace(guid))
        {
            for (int i = 0; i < ProofCandidateRoster.Length; i++)
            {
                HumanCompanionRosterEntry entry = ProofCandidateRoster[i];
                if (string.Equals(entry.Guid, guid, StringComparison.OrdinalIgnoreCase))
                {
                    return GetHumanIconRelativePath(entry.DisplayName, entry.TemplateName, GetStarterRoleLabelForRosterIndex(i));
                }
            }
        }

        return GetHumanIconRelativePath(GetCompanionDisplayName(location), location?.Template?.name ?? string.Empty, GetSelectedRoleLabel());
    }

    private static string GetStarterRoleLabelForRosterIndex(int rosterIndex)
    {
        for (int i = 0; i < StarterCandidateRoster.Length; i++)
        {
            if (StarterCandidateRoster[i].RosterIndex == rosterIndex)
            {
                return StarterCandidateRoster[i].RoleLabel;
            }
        }

        return "Companion";
    }

    private static string GetHumanIconRelativePath(string displayName, string templateName, string roleLabel)
    {
        string key = displayName + " " + templateName + " " + roleLabel;
        if (ContainsIconKey(key, "Outlawed Knight") || ContainsIconKey(key, "Disgraced Knight"))
        {
            return HumanIconSlayer;
        }

        if (ContainsIconKey(key, "Armored"))
        {
            return HumanIconKnight;
        }

        if (ContainsIconKey(key, "SpearShield") || ContainsIconKey(key, "Spear and shield"))
        {
            return HumanIconHalberdier;
        }

        if (ContainsIconKey(key, "Spear") || ContainsIconKey(key, "Lancer"))
        {
            return HumanIconLancer;
        }

        if (ContainsIconKey(key, "HighwaymanArcher"))
        {
            return HumanIconCrossbowman;
        }

        if (ContainsIconKey(key, "DerangedArcher") || ContainsIconKey(key, "DesperateArcher"))
        {
            return HumanIconVeteranArcher;
        }

        if (ContainsIconKey(key, "Archer") || ContainsIconKey(key, "Ranged"))
        {
            return HumanIconArcher;
        }

        if (ContainsIconKey(key, "Highwayman"))
        {
            return ContainsIconKey(key, "2H") || ContainsIconKey(key, "Heavy")
                ? HumanIconRobber
                : HumanIconRobber2;
        }

        if (ContainsIconKey(key, "Outlaw"))
        {
            return ContainsIconKey(key, "2H") || ContainsIconKey(key, "Heavy")
                ? HumanIconBarbarian
                : HumanIconRogue;
        }

        if (ContainsIconKey(key, "Deranged"))
        {
            return HumanIconRobber;
        }

        if (ContainsIconKey(key, "Berserker") || ContainsIconKey(key, "Barbarian"))
        {
            return HumanIconBarbarian;
        }

        if (ContainsIconKey(key, "2H") || ContainsIconKey(key, "Heavy"))
        {
            return HumanIconHeavy;
        }

        if (ContainsIconKey(key, "Thug"))
        {
            return HumanIconThug;
        }

        if (ContainsIconKey(key, "One-handed") || ContainsIconKey(key, "1H"))
        {
            return HumanIconFootman;
        }

        return HumanIconDefault;
    }

    private static bool ContainsIconKey(string value, string token)
    {
        return value.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private string GetTargetTemplateLabel()
    {
        if (UseProofCandidateRoster())
        {
            return GetSelectedHumanCandidate().TemplateName;
        }

        return string.IsNullOrWhiteSpace(_templateName.Value)
            ? "manual target"
            : _templateName.Value.Trim();
    }

    private string GetActiveTargetGuidLabel()
    {
        string guid = GetActiveTargetGuid();
        return string.IsNullOrWhiteSpace(guid) ? "no-guid" : guid;
    }

    private string GetScannerTargetGuid()
    {
        string configured = _actorScannerSelectedTarget.Value.Trim();
        return string.IsNullOrWhiteSpace(configured) ? GetActiveTargetGuid() : configured;
    }

    private bool HasActiveTarget()
    {
        return !string.IsNullOrWhiteSpace(GetActiveTargetGuid());
    }

    private string GetActiveTargetGuid()
    {
        return UseProofCandidateRoster()
            ? GetSelectedHumanCandidate().Guid
            : _templateGuid.Value.Trim();
    }

    private bool UseProofCandidateRoster()
    {
        return _enableProofCandidateRoster.Value && ProofCandidateRoster.Length > 0;
    }

    private bool UseStarterCandidateSelector()
    {
        return UseProofCandidateRoster() && StarterCandidateRoster.Length > 0;
    }

    private HumanCompanionRosterEntry GetSelectedHumanCandidate()
    {
        return ProofCandidateRoster[GetSelectedHumanCandidateRosterIndex()];
    }

    private int GetSelectedHumanCandidateRosterIndex()
    {
        if (UseStarterCandidateSelector())
        {
            int position = GetSelectedStarterCandidatePosition();
            return StarterCandidateRoster[position].RosterIndex;
        }

        int index = NormalizeCandidateIndex(_selectedCandidateIndex.Value);
        if (_selectedCandidateIndex.Value != index)
        {
            _selectedCandidateIndex.Value = index;
        }

        return index;
    }

    private int GetSelectedStarterCandidatePosition()
    {
        int normalizedIndex = NormalizeCandidateIndex(_selectedCandidateIndex.Value);
        int position = IndexOfStarterCandidate(normalizedIndex);
        if (position >= 0)
        {
            if (_selectedCandidateIndex.Value != normalizedIndex)
            {
                _selectedCandidateIndex.Value = normalizedIndex;
            }

            return position;
        }

        _selectedCandidateIndex.Value = DefaultHumanCandidateIndex;
        int defaultPosition = IndexOfStarterCandidate(DefaultHumanCandidateIndex);
        return defaultPosition >= 0 ? defaultPosition : 0;
    }

    private static int IndexOfStarterCandidate(int rosterIndex)
    {
        for (int i = 0; i < StarterCandidateRoster.Length; i++)
        {
            if (StarterCandidateRoster[i].RosterIndex == rosterIndex)
            {
                return i;
            }
        }

        return -1;
    }

    private static int NormalizeCandidateIndex(int index)
    {
        int count = ProofCandidateRoster.Length;
        return ((index % count) + count) % count;
    }

    private static int NormalizeStarterCandidatePosition(int position)
    {
        int count = StarterCandidateRoster.Length;
        return ((position % count) + count) % count;
    }

    private string GetModeFeedbackLine()
    {
        if (_researchModeOnly.Value)
        {
            return "Companion commands are unavailable.";
        }

        if (!IsManagedAllyProof(_lastAllyProof))
        {
            return "Call a companion before sending commands.";
        }

        int attackerCount = CountLiveHeroAttackers(Hero.Current);
        return _allyProofMode switch
        {
            HumanProofMode.Follow => attackerCount > 0 && ShouldCompanionBrainProtect(attackerCount) ? "Protecting you." : "Keeping close.",
            HumanProofMode.Hold => "Holding this spot.",
            HumanProofMode.Defend => attackerCount > 0 ? "Protecting you." : "Watching for threats.",
            _ => "I'm ready.",
        };
    }

    private string GetHumanCommandBlockedStatusLine(NativeHumanCompanionCommand command)
    {
        if (_researchModeOnly.Value)
        {
            return "Companion commands are unavailable right now.";
        }

        if (!IsManagedAllyProof(_lastAllyProof))
        {
            return "No companion is currently with you.";
        }

        return command switch
        {
            NativeHumanCompanionCommand.Hold when _allyProofMode == HumanProofMode.Hold => "Already holding this spot.",
            NativeHumanCompanionCommand.Defend when _allyProofMode == HumanProofMode.Defend => "Already watching for threats.",
            _ => "Command is unavailable right now.",
        };
    }

    private List<string> GetActiveProofStatusLines()
    {
        List<string> lines = new List<string>();
        Location? location = _lastAllyProof;
        Hero hero = Hero.Current;

        if (!IsManagedAllyProof(location))
        {
            lines.Add("No companion is currently with you.");
            lines.Add("Choose a companion, then Call / Swap.");
            return lines;
        }

        float? distance = hero == null ? null : Vector3.Distance(location!.Coords, hero.Coords);
        int attackerCount = CountLiveHeroAttackers(hero);
        lines.Add(distance.HasValue ? $"Distance: {distance.Value:0.#}m away" : "Distance: unavailable");
        if (_allyProofMode == HumanProofMode.Hold)
        {
            float drift = _hasHoldAnchor ? Vector3.Distance(location!.Coords, _holdAnchor) : 0f;
            lines.Add(drift > 1f ? "Position: holding the new spot." : "Position: holding this spot.");
        }

        lines.Add($"Awareness: {GetCompanionAwarenessLabel(attackerCount)}");
        return lines;
    }

    private string GetCompanionAwarenessLabel(int attackerCount)
    {
        if (!_enableCompanionBrain.Value)
        {
            return "Manual commands.";
        }

        float cooldown = Mathf.Max(0f, _nextCompanionBrainDefendPromptAt - Time.realtimeSinceStartup);
        if (attackerCount > 0)
        {
            if (ShouldCompanionBrainProtect(attackerCount))
            {
                return cooldown > 0f
                    ? "Protecting you."
                    : "Moving to protect you.";
            }

            return cooldown > 0f
                ? "Threats nearby; holding formation."
                : "Threats nearby.";
        }

        return _allyProofMode switch
        {
            HumanProofMode.Follow => "Keeping close.",
            HumanProofMode.Hold => "Holding position.",
            HumanProofMode.Defend => "Watching for threats.",
            _ => "Ready.",
        };
    }

    private string GetActiveProofNameLabel()
    {
        if (!IsManagedAllyProof(_lastAllyProof))
        {
            return "No companion";
        }

        return GetCompanionDisplayName(_lastAllyProof);
    }

    private static string GetCompanionDisplayName(Location? location)
    {
        string? guid = location?.Template?.GUID;
        if (!string.IsNullOrWhiteSpace(guid))
        {
            foreach (HumanCompanionRosterEntry entry in ProofCandidateRoster)
            {
                if (string.Equals(entry.Guid, guid, StringComparison.OrdinalIgnoreCase))
                {
                    return entry.DisplayName;
                }
            }
        }

        return HumanizeTemplateName(location?.Template?.name);
    }

    private static string HumanizeTemplateName(string? templateName)
    {
        if (string.IsNullOrWhiteSpace(templateName))
        {
            return "Companion";
        }

        string value = templateName.Trim();
        string[] prefixes =
        {
            "Spec_Enemy_Generic_",
            "Spec_Enemy_",
            "Spec_NPC_",
            "Spec_",
        };

        foreach (string prefix in prefixes)
        {
            if (value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                value = value.Substring(prefix.Length);
                break;
            }
        }

        value = value
            .Replace("_Tier0_", " ")
            .Replace("_Tier1_", " ")
            .Replace("_Tier2_", " ")
            .Replace("_Tier3_", " ")
            .Replace("_Tier4_", " ")
            .Replace("Tier0_", string.Empty)
            .Replace("Tier1_", string.Empty)
            .Replace("Tier2_", string.Empty)
            .Replace("Tier3_", string.Empty)
            .Replace("Tier4_", string.Empty)
            .Replace('_', ' ')
            .Trim();

        return string.IsNullOrWhiteSpace(value) ? "Companion" : value;
    }

    private void SetDialogueVisible(bool visible, string reason)
    {
        if (_dialogueVisible == visible)
        {
            if (visible)
            {
                CaptureCursorForPanel();
                EnsureHumanCompanionDialogueUiHost();
            }

            return;
        }

        _dialogueVisible = visible;
        SetControllerCursorScopeActive(_panelVisible || _dialogueVisible);

        if (visible)
        {
            CaptureCursorForPanel();
            EnsureHumanCompanionDialogueUiHost();
        }
        else
        {
            DestroyHumanCompanionDialogueUiHost();
            if (!_panelVisible && !_dialogueVisible)
            {
                RestorePanelInputState();
                RestoreCursorState();
            }
        }

        Logger.LogInfo($"{PluginName} companion-style dialogue {(visible ? "opened" : "closed")} by {reason}.");
        UpdateCursor();
    }

    private void SetPanelVisible(bool visible, string reason)
    {
        if (_panelVisible == visible)
        {
            return;
        }

        _panelVisible = visible;
        _panelStatus = visible ? "Panel opened." : "Panel closed.";
        SetControllerCursorScopeActive(_panelVisible || _dialogueVisible);

        if (visible)
        {
            CaptureCursorForPanel();
            CapturePanelInputForPanel();
            EnsurePanelPlacement();
        }
        else if (!_panelVisible && !_dialogueVisible)
        {
            RestorePanelInputState();
            RestoreCursorState();
        }

        Logger.LogInfo($"{PluginName} proof debug panel {(visible ? "opened" : "closed")} by {reason}.");
        UpdateCursor();
    }

    private void SetControllerCursorScopeActive(bool active)
    {
        if (_controllerCursorScopeActive == active)
        {
            return;
        }

        if (active)
        {
            _taintedInterfaceScopeActive = TaintedInterfaceBridge.BeginCustomUiScope(PluginGuid, freezeWorld: true);
            if (!_taintedInterfaceScopeActive)
            {
                FoAModManagerBridge.SetCustomUiScope(PluginGuid, active: true, freezeWorld: true);
            }

            _controllerCursorScopeActive = true;
            return;
        }

        if (_taintedInterfaceScopeActive)
        {
            TaintedInterfaceBridge.EndCustomUiScope(PluginGuid);
            _taintedInterfaceScopeActive = false;
        }
        else
        {
            FoAModManagerBridge.SetCustomUiScope(PluginGuid, active: false, freezeWorld: true);
        }

        _controllerCursorScopeActive = false;
    }

    private void UpdateCursor()
    {
        if (_panelVisible || _dialogueVisible)
        {
            EnsureCursorForPanel();
            return;
        }

        RestoreCursorState();
    }

    private void EnsurePanelPlacement()
    {
        if (_panelInitialized && _lastPanelScreenWidth == Screen.width && _lastPanelScreenHeight == Screen.height)
        {
            return;
        }

        float width = GetPanelWidth();
        float height = GetPanelHeight();
        _panelRect = new Rect(
            Mathf.Max(PanelMargin, (Screen.width - width) * 0.5f),
            Mathf.Max(PanelMargin, (Screen.height - height) * 0.5f),
            width,
            height);
        _panelInitialized = true;
        _lastPanelScreenWidth = Screen.width;
        _lastPanelScreenHeight = Screen.height;
    }

    private static Rect ClampPanelToScreen(Rect rect)
    {
        float width = Mathf.Clamp(rect.width, GetPanelMinimumWidth(), GetPanelMaximumWidth());
        float height = Mathf.Clamp(rect.height, GetPanelMinimumHeight(), GetPanelMaximumHeight());
        float x = Mathf.Clamp(rect.x, PanelMargin, Mathf.Max(PanelMargin, Screen.width - width - PanelMargin));
        float y = Mathf.Clamp(rect.y, PanelMargin, Mathf.Max(PanelMargin, Screen.height - height - PanelMargin));
        return new Rect(x, y, width, height);
    }

    private static float GetPanelWidth()
    {
        float responsiveWidth = Mathf.Max(PanelWidth, Screen.width * PanelViewportWidthFraction);
        return Mathf.Clamp(responsiveWidth, GetPanelMinimumWidth(), GetPanelMaximumWidth());
    }

    private static float GetPanelHeight()
    {
        float responsiveHeight = Mathf.Max(PanelHeight, Screen.height * PanelViewportHeightFraction);
        return Mathf.Clamp(responsiveHeight, GetPanelMinimumHeight(), GetPanelMaximumHeight());
    }

    private static float GetPanelMinimumWidth()
    {
        return Mathf.Min(PanelMinWidth, GetPanelMaximumWidth());
    }

    private static float GetPanelMinimumHeight()
    {
        return Mathf.Min(PanelMinHeight, GetPanelMaximumHeight());
    }

    private static float GetPanelMaximumWidth()
    {
        return Mathf.Max(360f, Screen.width - (PanelMargin * 2f));
    }

    private static float GetPanelMaximumHeight()
    {
        return Mathf.Max(360f, Screen.height - (PanelMargin * 2f));
    }

    private static void ClearPanelControlFocus()
    {
        GUIUtility.keyboardControl = 0;
        GUIUtility.hotControl = 0;
    }

    private void CaptureCursorForPanel()
    {
        if (!_cursorCaptured)
        {
            _previousCursorVisible = Cursor.visible;
            _previousCursorLockState = Cursor.lockState;
            _cursorCaptured = true;
        }

        EnsureCursorForPanel();
        TaintedInterfaceBridge.EnsureInteractiveCursor();
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

    private void RestoreCursorState()
    {
        if (!_cursorCaptured)
        {
            return;
        }

        Cursor.visible = _previousCursorVisible;
        Cursor.lockState = _previousCursorLockState;
        _cursorCaptured = false;
    }

    private void CapturePanelInputForPanel()
    {
        if (_inputModulesCaptured)
        {
            EnsurePanelInputBlocked();
            return;
        }

        _inputModuleStates.Clear();
        foreach (BaseInputModule module in FindObjectsByType<BaseInputModule>(FindObjectsSortMode.None))
        {
            _inputModuleStates.Add(new InputModuleState(module, module.enabled));
            if (module.enabled)
            {
                module.enabled = false;
            }
        }

        _inputModulesCaptured = true;
        Input.ResetInputAxes();
    }

    private void EnsurePanelInputBlocked()
    {
        if (!_inputModulesCaptured)
        {
            return;
        }

        for (int i = _inputModuleStates.Count - 1; i >= 0; i--)
        {
            BaseInputModule? module = _inputModuleStates[i].Module;
            if (module == null)
            {
                _inputModuleStates.RemoveAt(i);
                continue;
            }

            if (module.enabled)
            {
                module.enabled = false;
            }
        }
    }

    private void RestorePanelInputState()
    {
        if (!_inputModulesCaptured)
        {
            return;
        }

        foreach (InputModuleState state in _inputModuleStates)
        {
            if (state.Module != null)
            {
                state.Module.enabled = state.WasEnabled;
            }
        }

        _inputModuleStates.Clear();
        _inputModulesCaptured = false;
    }

    private void EnsurePanelStyles()
    {
        if (_panelWindowStyle != null)
        {
            return;
        }

        if (TaintedInterfaceBridge.TryGetStyles(out TaintedInterfaceBridge.SharedStyles? sharedStyles) && sharedStyles != null)
        {
            ApplySharedPanelStyles(sharedStyles);
            Logger.LogInfo($"{PluginName} using Tainted Interface shared styles and scope bridge.");
            return;
        }

        _sharedPanelTextures.Clear();
        _panelTexture = BuildTexture(new Color(0.045f, 0.052f, 0.05f, 0.97f));
        _sectionTexture = BuildTexture(new Color(0.082f, 0.098f, 0.094f, 0.95f));
        _buttonTexture = BuildTexture(new Color(0.12f, 0.29f, 0.29f, 0.98f));
        _buttonHoverTexture = BuildTexture(new Color(0.18f, 0.39f, 0.37f, 1f));
        _selectedButtonTexture = BuildTexture(new Color(0.17f, 0.34f, 0.31f, 1f));
        _dangerButtonTexture = BuildTexture(new Color(0.32f, 0.12f, 0.1f, 0.98f));
        _dangerHoverTexture = BuildTexture(new Color(0.44f, 0.16f, 0.12f, 1f));

        _panelWindowStyle = new GUIStyle(GUI.skin.window)
        {
            normal = { background = _panelTexture },
            padding = new RectOffset(18, 18, 18, 18),
            border = new RectOffset(8, 8, 8, 8)
        };

        _headerStyle = new GUIStyle(GUI.skin.box)
        {
            normal = { background = _sectionTexture },
            padding = new RectOffset(18, 18, 12, 12)
        };

        _sectionStyle = new GUIStyle(_headerStyle)
        {
            padding = new RectOffset(18, 18, 16, 16)
        };

        _footerStyle = new GUIStyle(_sectionStyle)
        {
            padding = new RectOffset(18, 18, 12, 12)
        };

        _titleStyle = BuildLabel(28, FontStyle.Bold, new Color(0.94f, 0.76f, 0.42f, 1f));
        _labelStyle = BuildLabel(18, FontStyle.Bold, new Color(0.91f, 0.9f, 0.84f, 1f));
        _mutedStyle = BuildLabel(15, FontStyle.Normal, new Color(0.71f, 0.76f, 0.72f, 1f));
        _buttonStyle = BuildButton(_buttonTexture, _buttonHoverTexture, new Color(0.92f, 0.96f, 0.92f, 1f));
        _selectedButtonStyle = BuildButton(_selectedButtonTexture, _selectedButtonTexture, new Color(1f, 0.92f, 0.68f, 1f));
        _dangerButtonStyle = BuildButton(_dangerButtonTexture, _dangerHoverTexture, new Color(1f, 0.88f, 0.82f, 1f));
    }

    private void ApplySharedPanelStyles(TaintedInterfaceBridge.SharedStyles sharedStyles)
    {
        _sharedPanelTextures.Clear();

        _panelTexture = SharedBackground(sharedStyles.Window) ?? BuildTexture(new Color(0.045f, 0.052f, 0.05f, 0.97f));
        _sectionTexture = SharedBackground(sharedStyles.Panel) ?? SharedBackground(sharedStyles.Card) ?? _panelTexture;
        _buttonTexture = SharedBackground(sharedStyles.Button) ?? BuildTexture(new Color(0.12f, 0.29f, 0.29f, 0.98f));
        _buttonHoverTexture = RegisterSharedTexture(sharedStyles.Button.hover.background) ?? _buttonTexture;
        _selectedButtonTexture = SharedBackground(sharedStyles.SecondaryButton) ?? _buttonHoverTexture;
        _dangerButtonTexture = BuildTexture(new Color(0.32f, 0.12f, 0.1f, 0.98f));
        _dangerHoverTexture = BuildTexture(new Color(0.44f, 0.16f, 0.12f, 1f));

        _panelWindowStyle = new GUIStyle(sharedStyles.Window)
        {
            normal = { background = _panelTexture },
            padding = new RectOffset(18, 18, 18, 18),
            border = new RectOffset(8, 8, 8, 8)
        };

        _headerStyle = new GUIStyle(sharedStyles.Header)
        {
            normal = { background = _sectionTexture },
            padding = new RectOffset(18, 18, 12, 12)
        };

        _sectionStyle = new GUIStyle(sharedStyles.Panel)
        {
            normal = { background = _sectionTexture },
            padding = new RectOffset(18, 18, 16, 16)
        };

        _footerStyle = new GUIStyle(sharedStyles.Card)
        {
            padding = new RectOffset(18, 18, 12, 12)
        };

        _titleStyle = new GUIStyle(sharedStyles.Title)
        {
            fontSize = 28,
            fontStyle = FontStyle.Bold,
            wordWrap = true,
            alignment = TextAnchor.MiddleLeft
        };

        _labelStyle = new GUIStyle(sharedStyles.Label)
        {
            fontSize = 18,
            fontStyle = FontStyle.Bold,
            wordWrap = true,
            alignment = TextAnchor.MiddleLeft
        };

        _mutedStyle = new GUIStyle(sharedStyles.MutedLabel)
        {
            fontSize = 15,
            fontStyle = FontStyle.Normal,
            wordWrap = true,
            alignment = TextAnchor.MiddleLeft
        };

        _buttonStyle = BuildSharedButton(sharedStyles.Button, new Color(0.92f, 0.96f, 0.92f, 1f));
        _selectedButtonStyle = BuildSharedButton(sharedStyles.SecondaryButton, new Color(1f, 0.92f, 0.68f, 1f));
        _dangerButtonStyle = BuildButton(_dangerButtonTexture, _dangerHoverTexture, new Color(1f, 0.88f, 0.82f, 1f));
    }

    private static GUIStyle BuildLabel(int fontSize, FontStyle fontStyle, Color color)
    {
        return new GUIStyle(GUI.skin.label)
        {
            fontSize = fontSize,
            fontStyle = fontStyle,
            normal = { textColor = color },
            wordWrap = true,
            alignment = TextAnchor.MiddleLeft
        };
    }

    private static GUIStyle BuildButton(Texture2D normal, Texture2D hover, Color textColor)
    {
        return new GUIStyle(GUI.skin.button)
        {
            fontSize = 17,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            wordWrap = true,
            normal = { background = normal, textColor = textColor },
            hover = { background = hover, textColor = textColor },
            active = { background = normal, textColor = textColor },
            focused = { background = normal, textColor = textColor },
            onNormal = { background = normal, textColor = textColor },
            onHover = { background = hover, textColor = textColor },
            onActive = { background = normal, textColor = textColor },
            onFocused = { background = normal, textColor = textColor },
            padding = new RectOffset(12, 12, 8, 8)
        };
    }

    private static GUIStyle BuildSharedButton(GUIStyle source, Color textColor)
    {
        GUIStyle style = new GUIStyle(source)
        {
            fontSize = 17,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            wordWrap = true,
            padding = new RectOffset(12, 12, 8, 8)
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

    private Texture2D? SharedBackground(GUIStyle style)
    {
        return RegisterSharedTexture(style.normal.background ?? style.hover.background ?? style.active.background ?? style.focused.background);
    }

    private Texture2D? RegisterSharedTexture(Texture2D? texture)
    {
        if (texture != null)
        {
            _sharedPanelTextures.Add(texture);
        }

        return texture;
    }

    private static Texture2D BuildTexture(Color color)
    {
        Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        texture.hideFlags = HideFlags.HideAndDontSave;
        return texture;
    }

    private void DestroyPanelTextures()
    {
        DestroyTexture(ref _panelTexture);
        DestroyTexture(ref _sectionTexture);
        DestroyTexture(ref _buttonTexture);
        DestroyTexture(ref _buttonHoverTexture);
        DestroyTexture(ref _selectedButtonTexture);
        DestroyTexture(ref _dangerButtonTexture);
        DestroyTexture(ref _dangerHoverTexture);
        _sharedPanelTextures.Clear();
    }

    private void DestroyTexture(ref Texture2D? texture)
    {
        if (texture == null)
        {
            return;
        }

        if (!_sharedPanelTextures.Contains(texture))
        {
            Destroy(texture);
        }

        texture = null;
    }

    private void TryDiscardProofSpawn(string reason)
    {
        try
        {
            if (_lastProofSpawn == null || _lastProofSpawn.HasBeenDiscarded)
            {
                _lastProofSpawn = null;
                return;
            }

            _lastProofSpawn.MarkedNotSaved = true;
            WriteLifecycleEvent("proof-spawn-dismiss-requested", reason, _lastProofSpawn, tracked: false);
            _lastProofSpawn.Discard();
            Logger.LogInfo($"{PluginName} discarded proof spawn during {reason}.");
            WriteLifecycleEvent("proof-spawn-discarded", reason, _lastProofSpawn, tracked: false);
            _lastProofSpawn = null;
        }
        catch
        {
            // Best-effort cleanup only. Never hide behavior errors elsewhere.
        }
    }

    private void TryDiscardAllyProof(string reason)
    {
        try
        {
            if (_lastAllyProof == null || _lastAllyProof.HasBeenDiscarded)
            {
                _lastAllyProof = null;
                _allyProofMode = HumanProofMode.Follow;
                ResetCompanionBrainState();
                return;
            }

            RemoveNativeCommandActions(_lastAllyProof);
            RemoveHoldMovementBlock(_lastAllyProof);
            _lastAllyProof.MarkedNotSaved = true;
            WriteLifecycleEvent("ally-proof-dismiss-requested", reason, _lastAllyProof, tracked: true);
            _lastAllyProof.Discard();
            Logger.LogInfo($"{PluginName} discarded one-session ally proof during {reason}.");
            WriteLifecycleEvent("ally-proof-discarded", reason, _lastAllyProof, tracked: false);
            _lastAllyProof = null;
            _allyProofMode = HumanProofMode.Follow;
            ResetCompanionBrainState();
        }
        catch
        {
            // Best-effort cleanup only. Never hide behavior errors elsewhere.
        }
    }

    private string GetTargetLabel()
    {
        string name = GetTargetTemplateLabel();
        string guid = GetActiveTargetGuidLabel();
        return $"{name}[{guid}]";
    }

    private void WriteLifecycleEvent(string eventName, string reason, Location? location, bool tracked)
    {
        if (!_writeLifecycleDiagnostics.Value)
        {
            return;
        }

        HumanProofLifecycleDiagnostics.WriteEvent(Logger, eventName, reason, location, tracked, _allyProofMode.ToString());
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
