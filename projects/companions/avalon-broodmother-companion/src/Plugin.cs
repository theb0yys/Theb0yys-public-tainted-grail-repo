using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Awaken.TG.Assets;
using Awaken.TG.Main.AI.SummonsAndAllies;
using Awaken.TG.Main.Character;
using Awaken.TG.Main.Fights;
using Awaken.TG.Main.Fights.DamageInfo;
using Awaken.TG.Main.Fights.Factions;
using Awaken.TG.Main.Fights.NPCs;
using Awaken.TG.Main.Heroes;
using Awaken.TG.Main.Heroes.Interactions;
using Awaken.TG.Main.Locations;
using Awaken.TG.Main.Locations.Actions;
using Awaken.TG.Main.Locations.Attachments.Elements;
using Awaken.TG.Main.Locations.Attachments.Elements.DeathBehaviours;
using Awaken.TG.Main.Locations.Setup;
using Awaken.TG.Main.Templates;
using Awaken.TG.MVC.Events;
using Awaken.TG.MVC.Elements;
using AvalonAI.Contracts.V2;
using AvalonAI.Execution.PlayMaker;
using AvalonAI.FoAHost.Mono.V2;
using AvalonAI.Runtime.V2;
using AvalonBroodmotherCompanion.AI.Package.V1;
using AvalonBroodmotherCompanion.AI.Package.V2;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace AvalonBroodmotherCompanion;

internal enum BroodmotherDialogueCommand
{
    Follow,
    HoldPosition,
    Defend,
    RangeClose,
    RangeNormal,
    RangeFar,
    ComeClose,
    Recall,
    Recover,
    Dismiss,
}

internal enum BroodmotherFollowRange
{
    Close,
    Normal,
    Far,
}

internal enum BroodmotherSpiderRuntimeAttackKind
{
    None,
    Bite,
    Leap,
    Spit,
}

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    private const int PanelWindowId = 8506421;
    private const float PanelWidth = 420f;
    private const float PanelHeight = 260f;
    private const string TargetTemplateName = "Spec_Broodmother_CI4";
    private const string TargetDisplayName = "Broodmother";
    internal const string TargetTemplateGuid = "527d142b35e81a648b6e84ea921ac543";
    private const string TargetNpcTemplateGuid = "7623d4729979e814ea64992d9a399e17";
    private const string TargetVisualAddress = "avalon-awakened/creatures/spider/broodmother/skin1-1/visual-v1";
    private const string OverlayRootName = "AvalonBroodmotherCompanionVisualOverlay_CI4";
    private const float BroodmotherVisualScaleMultiplier = 5.5f;
    private const string ResolverTypeName = "AvalonAwakened.AvalonCreatureWorldApi, AvalonAwakened";
    private const string ResolverMethodName = "TryResolveWorldBroodmotherTemplate";
    private const float BroodmotherDamageBridgeShortRangeMeters = BroodmotherCompanionAiV1Contract.ShortBiteDistanceMeters + 0.75f;
    private const float BroodmotherDamageBridgeLeapRangeMeters = BroodmotherCompanionAiV1Contract.ReleaseWindowMaxDistanceMeters;
    private const float BroodmotherDamageBridgeSpitRangeMeters = BroodmotherCompanionAiV1Contract.ReleaseWindowMaxDistanceMeters + 3f;
    private const float BroodmotherDamageBridgeLeapMultiplier = 1.25f;
    private const float BroodmotherDamageBridgeSpitMultiplier = 0.85f;
    private static readonly FieldInfo? PlayAnimationAlivePrefabField =
        typeof(PostponedRagdollBehaviourBase).GetField("_alivePrefab", BindingFlags.Instance | BindingFlags.NonPublic);

    public const string PluginGuid = "kane.tgfoa.avalon-broodmother-companion";
    public const string PluginName = "Avalon Broodmother Companion";
    public const string PluginVersion = "0.1.34";
    public const string UiOwnerId = PluginGuid + ".panel";
    internal const string CompanionIconId = "companion.creature";
    internal const string CompanionHudBackgroundIconId = "hud.companion-icon-background";
    internal const string KnownMerchantShopGuid = "75a071140bc819d4ab6e9e37abfdfa59";

    private ConfigEntry<bool> _enabled = null!;
    private ConfigEntry<bool> _allowRuntimeSummon = null!;
    private ConfigEntry<bool> _registerBroodmotherCallSpell = null!;
    private ConfigEntry<bool> _grantBroodmotherCallOnLoad = null!;
    private ConfigEntry<bool> _enableBroodmotherCallSummonTarget = null!;
    private ConfigEntry<bool> _enableBroodmotherCallCompanionRoute = null!;
    private ConfigEntry<bool> _merchantStockEnabled = null!;
    private ConfigEntry<string> _merchantStockTargetShopGuid = null!;
    private ConfigEntry<int> _merchantStockQuantity = null!;
    private ConfigEntry<KeyCode> _togglePanelHotkey = null!;
    private ConfigEntry<KeyCode> _summonOrSwapHotkey = null!;
    private ConfigEntry<KeyCode> _recallHotkey = null!;
    private ConfigEntry<KeyCode> _dismissHotkey = null!;
    private ConfigEntry<KeyCode> _defendToggleHotkey = null!;
    private ConfigEntry<float> _summonDistance = null!;
    private ConfigEntry<float> _summonRightOffset = null!;
    private ConfigEntry<bool> _enableFollowCatchUp = null!;
    private ConfigEntry<float> _followCatchUpDistance = null!;
    private ConfigEntry<bool> _enableNativeDefendAssist = null!;
    private ConfigEntry<bool> _enableAdvancedSpiderCompanionAi = null!;
    private ConfigEntry<bool> _enableAdvancedSpiderLeapRequests = null!;
    private ConfigEntry<float> _advancedSpiderCombatPromptSeconds = null!;
    private ConfigEntry<bool> _enableImmediateTargetPressure = null!;
    private ConfigEntry<float> _immediateTargetPressureSeconds = null!;
    private ConfigEntry<bool> _enableNativeCompanionPrompt = null!;
    private ConfigEntry<bool> _enableNativeCompanionInteractFallback = null!;
    private ConfigEntry<float> _nativeCompanionInteractFallbackDistance = null!;
    private ConfigEntry<float> _nativeCompanionInteractFallbackAngle = null!;
    private ConfigEntry<bool> _showCompanionHud = null!;
    private ConfigEntry<bool> _enableEmbeddedSpiderAudio = null!;
    private ConfigEntry<float> _embeddedSpiderAudioVolume = null!;
    private ConfigEntry<float> _embeddedSpiderAudioMinDistance = null!;
    private ConfigEntry<float> _embeddedSpiderAudioMaxDistance = null!;
    private ConfigEntry<float> _embeddedSpiderCallCooldownSeconds = null!;
    private ConfigEntry<float> _embeddedSpiderAttackCooldownSeconds = null!;
    private ConfigEntry<float> _embeddedSpiderWalkCooldownSeconds = null!;
    private ConfigEntry<float> _tickSeconds = null!;
    private ConfigEntry<bool> _writeCommandLog = null!;

    private Location? _activeBroodmother;
    private AsyncOperationHandle<GameObject> _broodmotherOverlayHandle;
    private GameObject? _broodmotherOverlayInstance;
    private readonly HashSet<Renderer> _broodmotherRuntimeOverlayRenderers = new();
    private readonly HashSet<Renderer> _broodmotherRuntimeOverlayStrippedRenderers = new();
    private readonly HashSet<Component> _broodmotherRuntimeOverlayKandraRenderers = new();
    private readonly HashSet<Component> _broodmotherRuntimeOverlayStrippedKandraRenderers = new();
    private readonly List<Material> _broodmotherOverlayRuntimeMaterials = new();
    private readonly List<Material> _broodmotherRetainedDeathRuntimeMaterials = new();
    private readonly List<AsyncOperationHandle<GameObject>> _broodmotherRetainedDeathHandles = new();
    private Harmony? _harmony;
    private bool _defendMode;
    private bool _holdPositionMode;
    private BroodmotherFollowRange _followRange = BroodmotherFollowRange.Normal;
    private bool _panelVisible;
    private bool _broodmotherOverlayRequested;
    private bool _broodmotherOverlayReady;
    private bool _broodmotherOverlayFailureLogged;
    private int _broodmotherOverlayRuntimeMaterialCount;
    private int _broodmotherOverlayMaterialSlotCount;
    private string _broodmotherOverlaySourceShaders = "none";
    private string _broodmotherOverlayTargetShaders = "none";
    private string _broodmotherOverlayBaseTextureSources = "none";
    private string _broodmotherOverlayNormalTextureSources = "none";
    private Transform? _broodmotherNativeVisualScaleRoot;
    private Vector3 _broodmotherNativeVisualBaseScale = Vector3.one;
    private bool _broodmotherNativeVisualScaleCaptured;
    private Rect _panelRect;
    private string _panelStatus = "Ready.";
    private float _nextTickTime;
    private float _nextDefendLogTime;
    private float _nextAdvancedSpiderCombatPromptTime;
    private float _nextAdvancedSpiderAiLogTime;
    private float _nextImmediateTargetPressureTime;
    private float _nextImmediateTargetPressureLogTime;
    private float _nextNativeCompanionInteractFallbackLogTime;
    private int _advancedSpiderWaveSeed;
    private bool _advancedSpiderTelegraphReady;
    private bool _broodmotherLifecycleReadyLogged;
    private bool _broodmotherLifecycleWarningLogged;
    private string _broodmotherLifecycleWarningReason = string.Empty;
    private bool _broodmotherAnimatedDeathVisibilityLogged;
    private bool _broodmotherNativeDeathAcceptedLogged;
    private bool _broodmotherNativeDeathPendingLogged;
    private BroodmotherCompanionDialogueHost? _dialogueHost;
    private BroodmotherEmbeddedAudioRuntime? _embeddedAudioRuntime;
    private readonly Dictionary<Location, string> _nativeCommandSurfaceKeys = new();
    private readonly Dictionary<Location, LocationInteractability> _nativePromptOriginalInteractability = new();
    private readonly List<Collider> _broodmotherRuntimeHealthHitboxes = new();
    private readonly Dictionary<Collider, int> _broodmotherRuntimeHitboxLayers = new();
    private SpiderFamilyCompanionDefinition? _activeDefinition;
    private float _nextBroodmotherDamageBridgeTime;
    private float _nextBroodmotherDamageBridgeLogTime;
    private AvalonFoASpiderBlazeCommandBridge? _spiderBlazeBridge;
    private AvalonPlayMakerSpiderRunner? _spiderProcedureRunner;
    private ICharacter? _spiderProcedureTarget;
    private bool _spiderDamageWindowOpen;
    private BroodmotherSpiderRuntimeAttackKind _spiderActiveAttackKind = BroodmotherSpiderRuntimeAttackKind.None;
    private string _spiderActiveActionIdValue = string.Empty;
    private bool _spiderLeapRepositionRequired;
    private string _spiderLeapRepositionTargetId = string.Empty;
    private ActorLease? _spiderActorLease;
    private string _spiderBridgeActorId = string.Empty;
    private string _spiderBridgeTargetId = string.Empty;
    private int _spiderLeaseGeneration;
    private int _spiderObservationRevision;
    private int _spiderReservationGeneration;
    private readonly Dictionary<string, GameObject> _spiderRuntimeVfxPrefabs = new(StringComparer.Ordinal);
    private readonly List<Material> _spiderRuntimeVfxMaterials = new();

    internal static Plugin? Instance { get; private set; }
    internal ManualLogSource ModLogger => Logger;
    internal bool RegisterBroodmotherCallSpell => _registerBroodmotherCallSpell.Value;
    internal bool GrantBroodmotherCallOnLoad => _grantBroodmotherCallOnLoad.Value;
    internal bool EnableBroodmotherCallSummonTarget => _enableBroodmotherCallSummonTarget.Value;
    internal bool EnableBroodmotherCallCompanionRoute => _enableBroodmotherCallCompanionRoute.Value;
    internal bool MerchantStockEnabled => _merchantStockEnabled.Value;
    internal string MerchantStockTargetShopGuid => _merchantStockTargetShopGuid.Value ?? string.Empty;
    internal int MerchantStockQuantity => Mathf.Clamp(_merchantStockQuantity.Value, 1, 99);
    internal string ActiveCompanionDisplayName => ActiveDefinition.DisplayName;
    internal BroodmotherIconVariant? ActiveCompanionIconVariant => ActiveDefinition.IconVariant;

    private SpiderFamilyCompanionDefinition ActiveDefinition =>
        _activeDefinition ?? SpiderFamilyCompanionDefinitions.BroodmotherSkin1;

    internal static bool IsNativeBroodmotherCommandMenuAvailable(
        Location location,
        Hero? hero,
        IInteractableWithHero interactable)
    {
        return Instance != null && Instance.IsNativeBroodmotherCommandMenuAvailableInstance(location, hero, interactable);
    }

    internal static void OpenNativeBroodmotherCommandMenu(Location location)
    {
        Instance?.OpenNativeBroodmotherCommandMenuInstance(location);
    }

    internal static bool IsNativeBroodmotherQuickCommandAvailable(
        Location location,
        Hero? hero,
        IInteractableWithHero interactable,
        BroodmotherDialogueCommand command)
    {
        return Instance != null &&
            Instance.IsNativeBroodmotherQuickCommandAvailableInstance(location, hero, interactable, command);
    }

    internal static void RunNativeBroodmotherQuickCommand(Location location, BroodmotherDialogueCommand command)
    {
        Instance?.RunNativeBroodmotherQuickCommandInstance(location, command);
    }

    internal static bool IsCompanionUiVisibleForInputLock => Instance?.IsCompanionUiVisibleInstance == true;

    internal static bool IsCompanionDialogueVisibleForInputLock => Instance?.IsCompanionDialogueVisible == true;

    private void Awake()
    {
        Instance = this;

        _enabled = Config.Bind(
            "General",
            "Enabled",
            true,
            ManagerSetting("Enable Avalon Broodmother Companion.", "General", "Enabled", 0, 0));
        _allowRuntimeSummon = Config.Bind(
            "Safety",
            "AllowRuntimeSummon",
            false,
            ManagerSetting("Allow explicit runtime Broodmother summon commands. Keep disabled until testing on a throwaway save.", "Safety", "Allow Runtime Summon", 5, 0));
        _registerBroodmotherCallSpell = Config.Bind(
            "Spell",
            "RegisterBroodmotherCallSpell",
            true,
            ManagerSetting("Register the mod-owned Broodmother's Call spell item cloned from Wolf's Call.", "Spell", "Register Broodmother's Call", 8, 0));
        _enableBroodmotherCallSummonTarget = Config.Bind(
            "Spell",
            "EnableBroodmotherCallSummonTarget",
            false,
            ManagerSetting("Legacy native Magic_Summon_Ally target rewrite for Broodmother's Call. Leave disabled; the companion route avoids the native SkillSpawnLocation crash path.", "Spell", "Legacy Native Target", 8, 10));
        _enableBroodmotherCallCompanionRoute = Config.Bind(
            "Spell",
            "EnableBroodmotherCallCompanionRoute",
            true,
            ManagerSetting("Intercept Broodmother's Call casts and use the guarded one-session Broodmother companion route instead of native SkillSpawnLocation.", "Spell", "Companion Cast Route", 8, 15));
        _grantBroodmotherCallOnLoad = Config.Bind(
            "Spell",
            "GrantBroodmotherCallOnLoad",
            false,
            ManagerSetting("Grant one Broodmother's Call spell item to the current hero when templates and inventory are ready. This is save-visible; keep disabled until testing on a throwaway save.", "Spell", "Grant On Load", 8, 20));
        _merchantStockEnabled = Config.Bind(
            "MerchantStock",
            "Enabled",
            true,
            ManagerSetting("Add Broodmother's Call to the known Tier 1 vendor when that shop opens.", "Merchant Stock", "Enabled", 9, 0));
        _merchantStockTargetShopGuid = Config.Bind(
            "MerchantStock",
            "TargetShopGuid",
            KnownMerchantShopGuid,
            ManagerSetting("Exact ShopTemplate GUID to stock. Default is the known Shop_Vendor_Tier1 merchant.", "Merchant Stock", "Target Shop GUID", 9, 10));
        _merchantStockQuantity = Config.Bind(
            "MerchantStock",
            "Quantity",
            1,
            ManagerSetting("Quantity of Broodmother's Call to add to the opened merchant stock.", "Merchant Stock", "Quantity", 9, 20, new AcceptableValueRange<int>(1, 99)));
        _togglePanelHotkey = Config.Bind(
            "Panel",
            "TogglePanelHotkey",
            KeyCode.None,
            ManagerSetting("Open or close the Broodmother command panel.", "Screen", "Open Command Panel", 10, 0));
        _summonOrSwapHotkey = Config.Bind(
            "Input",
            "SummonOrSwapHotkey",
            KeyCode.None,
            ManagerSetting("Summon the Broodmother companion. If one is already active, replace it near the hero.", "Controls", "Summon / Swap Hotkey", 20, 0));
        _recallHotkey = Config.Bind(
            "Input",
            "RecallHotkey",
            KeyCode.None,
            ManagerSetting("Recall the active Broodmother companion near the hero.", "Controls", "Recall Hotkey", 20, 10));
        _dismissHotkey = Config.Bind(
            "Input",
            "DismissHotkey",
            KeyCode.None,
            ManagerSetting("Dismiss the active runtime Broodmother companion.", "Controls", "Dismiss Hotkey", 20, 20));
        _defendToggleHotkey = Config.Bind(
            "Input",
            "DefendToggleHotkey",
            KeyCode.None,
            ManagerSetting("Toggle native defend prompts for the active Broodmother companion.", "Controls", "Defend Toggle Hotkey", 20, 30));
        _summonDistance = Config.Bind(
            "Summon",
            "DistanceMeters",
            5.5f,
            ManagerSetting("Distance from the hero for summon and recall.", "Summon", "Distance", 30, 0, new AcceptableValueRange<float>(2f, 20f)));
        _summonRightOffset = Config.Bind(
            "Summon",
            "RightOffsetMeters",
            2.2f,
            ManagerSetting("Right-side offset from the hero for summon and recall.", "Summon", "Right Offset", 30, 10, new AcceptableValueRange<float>(-8f, 8f)));
        _enableFollowCatchUp = Config.Bind(
            "Follow",
            "EnableCatchUpRecall",
            true,
            ManagerSetting("Allow follow mode to recall the Broodmother when it falls behind.", "Follow", "Catch-up Recall", 40, 0));
        _followCatchUpDistance = Config.Bind(
            "Follow",
            "CatchUpDistanceMeters",
            18.0f,
            ManagerSetting("Distance before follow mode recalls the Broodmother near the hero.", "Follow", "Catch-up Distance", 40, 10, new AcceptableValueRange<float>(6f, 80f)));
        _enableNativeDefendAssist = Config.Bind(
            "Defend",
            "EnableNativeDefendAssist",
            false,
            ManagerSetting("Allow native NpcHeroPetAlly.EnterCombat prompts while the hero has live attackers.", "Defend", "Native Defend Assist", 50, 0));
        _enableAdvancedSpiderCompanionAi = Config.Bind(
            "CompanionAI",
            "EnableAdvancedSpiderAI",
            true,
            ManagerSetting("Enable bounded spider companion decisions for native-recognized live combat candidates. Uses native target combat handoff only; no save writes or random spawning.", "Companion AI", "Advanced Spider AI", 55, 0));
        _enableAdvancedSpiderLeapRequests = Config.Bind(
            "CompanionAI",
            "EnableLeapAttackRequests",
            true,
            ManagerSetting("Allow the Broodmother companion AI to classify leap opportunities through the proven spider short-range policy. This does not force an unproven native jump clip.", "Companion AI", "Leap Requests", 55, 10));
        _advancedSpiderCombatPromptSeconds = Config.Bind(
            "CompanionAI",
            "CombatPromptSeconds",
            0.35f,
            ManagerSetting("Minimum seconds between advanced spider combat handoff prompts.", "Companion AI", "Combat Prompt Seconds", 55, 20, new AcceptableValueRange<float>(0.2f, 8f)));
        _enableImmediateTargetPressure = Config.Bind(
            "CompanionAI",
            "EnableImmediateTargetPressure",
            true,
            ManagerSetting("Immediately push native target combat handoff while the hero or Broodmother has a native-recognized live combat candidate, even before the spider package attack commit window opens.", "Companion AI", "Immediate Target Pressure", 55, 30));
        _immediateTargetPressureSeconds = Config.Bind(
            "CompanionAI",
            "ImmediateTargetPressureSeconds",
            0.2f,
            ManagerSetting("Minimum seconds between immediate native target pressure prompts.", "Companion AI", "Target Pressure Seconds", 55, 40, new AcceptableValueRange<float>(0.1f, 2f)));
        _enableNativeCompanionPrompt = Config.Bind(
            "Dialogue",
            "EnableNativeCompanionPrompt",
            true,
            ManagerSetting("Attach one runtime-only native Companion prompt to the active one-session Broodmother. Opens this plugin's Unity UI companion menu; no Avalon Companions dependency, story graph, template edit, or save-owned interaction.", "Dialogue", "Native Companion Prompt", 60, 0));
        _enableNativeCompanionInteractFallback = Config.Bind(
            "Dialogue",
            "EnableNativeCompanionInteractFallback",
            true,
            ManagerSetting("Open the same Broodmother native Companion menu from the E interaction key when FoA has the runtime action attached but does not start it. Scoped to the active looked-at Broodmother only.", "Dialogue", "Native Interact Recovery", 60, 5));
        _nativeCompanionInteractFallbackDistance = Config.Bind(
            "Dialogue",
            "NativeCompanionInteractFallbackDistanceMeters",
            8.0f,
            ManagerSetting("Maximum distance for the active Broodmother E interaction recovery path.", "Dialogue", "Native Recovery Distance", 60, 6, new AcceptableValueRange<float>(2f, 18f)));
        _nativeCompanionInteractFallbackAngle = Config.Bind(
            "Dialogue",
            "NativeCompanionInteractFallbackAngleDegrees",
            42.0f,
            ManagerSetting("Maximum camera angle from the active Broodmother for the E interaction recovery path.", "Dialogue", "Native Recovery Angle", 60, 7, new AcceptableValueRange<float>(8f, 90f)));
        _showCompanionHud = Config.Bind(
            "Dialogue",
            "ShowCompanionHud",
            true,
            ManagerSetting("Show a Broodmother-owned companion HUD with name, health, mode, threat count, and native Companion prompt status.", "Dialogue", "Companion HUD", 60, 10));
        _enableEmbeddedSpiderAudio = Config.Bind(
            "Audio",
            "EnableEmbeddedSpiderAudio",
            true,
            ManagerSetting("Enable Broodmother-owned embedded spider audio cues. Sounds are compiled into this plugin as validated PCM WAV resources and played through FoA FMOD Core.", "Audio", "Embedded Spider Audio", 65, 0));
        _embeddedSpiderAudioVolume = Config.Bind(
            "Audio",
            "EmbeddedSpiderAudioVolume",
            0.72f,
            ManagerSetting("Volume multiplier for Broodmother embedded spider audio cues.", "Audio", "Embedded Spider Volume", 65, 10, new AcceptableValueRange<float>(0f, 1f)));
        _embeddedSpiderAudioMinDistance = Config.Bind(
            "Audio",
            "EmbeddedSpiderAudioMinDistance",
            1.0f,
            ManagerSetting("Minimum 3D attenuation distance for Broodmother embedded spider audio cues.", "Audio", "Embedded Spider Min Distance", 65, 20, new AcceptableValueRange<float>(0.1f, 12f)));
        _embeddedSpiderAudioMaxDistance = Config.Bind(
            "Audio",
            "EmbeddedSpiderAudioMaxDistance",
            28.0f,
            ManagerSetting("Maximum 3D attenuation distance for Broodmother embedded spider audio cues.", "Audio", "Embedded Spider Max Distance", 65, 30, new AcceptableValueRange<float>(4f, 80f)));
        _embeddedSpiderCallCooldownSeconds = Config.Bind(
            "Audio",
            "EmbeddedSpiderCallCooldownSeconds",
            60.0f,
            ManagerSetting("Minimum seconds between Broodmother embedded call cues.", "Audio", "Embedded Spider Call Cooldown", 65, 40, new AcceptableValueRange<float>(1f, 60f)));
        _embeddedSpiderAttackCooldownSeconds = Config.Bind(
            "Audio",
            "EmbeddedSpiderAttackCooldownSeconds",
            1.2f,
            ManagerSetting("Minimum seconds between Broodmother embedded attack cues.", "Audio", "Embedded Spider Attack Cooldown", 65, 50, new AcceptableValueRange<float>(0.2f, 10f)));
        _embeddedSpiderWalkCooldownSeconds = Config.Bind(
            "Audio",
            "EmbeddedSpiderWalkCooldownSeconds",
            4.0f,
            ManagerSetting("Minimum seconds between Broodmother embedded walking cues while following.", "Audio", "Embedded Spider Walk Cooldown", 65, 60, new AcceptableValueRange<float>(1f, 20f)));
        _tickSeconds = Config.Bind(
            "Runtime",
            "TickSeconds",
            0.35f,
            ManagerSetting("Seconds between Broodmother companion maintenance ticks.", "Advanced", "Runtime Tick Seconds", 90, 0, new AcceptableValueRange<float>(0.2f, 5f)));
        _writeCommandLog = Config.Bind(
            "Diagnostics",
            "WriteCommandLog",
            true,
            ManagerSetting("Append plugin-owned Broodmother command decisions to broodmother-command-log.csv.", "Diagnostics", "Write Command Log", 95, 0));

        MigrateFloatConfigDefault(_advancedSpiderCombatPromptSeconds, 1.25f, 0.35f);
        MigrateFloatConfigDefault(_tickSeconds, 0.75f, 0.35f);

        _panelRect = new Rect(18f, 18f, PanelWidth, PanelHeight);
        _dialogueHost = new BroodmotherCompanionDialogueHost(this);
        _embeddedAudioRuntime = new BroodmotherEmbeddedAudioRuntime(Logger);
        _spiderBlazeBridge = new AvalonFoASpiderBlazeCommandBridge(new BroodmotherSpiderBlazeCommandExecutor(this));
        SyncSpiderBlazeBridgeState();
        _harmony = new Harmony(PluginGuid);
        BroodmotherCompanionInputLockPatch.Apply(_harmony, Logger);
        CustomBroodmotherSpellFeature.Apply(_harmony, Logger);
        BroodmotherCallItemIconPatch.Apply(_harmony, Logger);
        CustomBroodmotherMerchantStockPatch.Apply(_harmony, Logger);
        BroodmotherSummonFriendlyFirePatch.Apply(_harmony, Logger);
        Logger.LogInfo($"{PluginName} {PluginVersion} loaded. RuntimeSummon={_allowRuntimeSummon.Value}, SpellCompanionRoute={EnableBroodmotherCallCompanionRoute}, LegacyNativeTarget={EnableBroodmotherCallSummonTarget}, Panel={_togglePanelHotkey.Value}, Summon={_summonOrSwapHotkey.Value}, Recall={_recallHotkey.Value}, Defend={_defendToggleHotkey.Value}, Dismiss={_dismissHotkey.Value}, MerchantStock={MerchantStockEnabled}, MerchantShop={MerchantStockTargetShopGuid}, MerchantQuantity={MerchantStockQuantity}, AiPackage={BroodmotherCompanionAiV1Contract.PackageIdValue}, AiPackageV2={BroodmotherCompanionAiV2Contract.PackageId}, AdvancedSpiderAI={_enableAdvancedSpiderCompanionAi.Value}, LeapRequests={_enableAdvancedSpiderLeapRequests.Value}, CombatPromptSeconds={_advancedSpiderCombatPromptSeconds.Value}, ImmediateTargetPressure={_enableImmediateTargetPressure.Value}, NativePrompt={_enableNativeCompanionPrompt.Value}, NativeInteractFallback={_enableNativeCompanionInteractFallback.Value}, NativeInteractFallbackDistance={_nativeCompanionInteractFallbackDistance.Value}, NativeInteractFallbackAngle={_nativeCompanionInteractFallbackAngle.Value}, CompanionHud={_showCompanionHud.Value}, EmbeddedSpiderAudio={_enableEmbeddedSpiderAudio.Value}, TickSeconds={_tickSeconds.Value}, aiDependencies=foahost-mono-v2, embeddedAudio=game-shipped-FMODUnity.");
    }

    private void Update()
    {
        SyncSpiderBlazeBridgeState();
        _spiderBlazeBridge?.Tick(DateTimeOffset.UtcNow);
        if (!_enabled.Value)
        {
            _panelVisible = false;
            RemoveAllNativeCompanionPrompts();
            _dialogueHost?.Close("general-disabled");
            return;
        }

        _dialogueHost?.Tick();
        if (_enableEmbeddedSpiderAudio.Value)
        {
            _embeddedAudioRuntime?.Tick();
        }
        else
        {
            _embeddedAudioRuntime?.StopAllVoices();
        }
        bool dialogueVisible = IsCompanionDialogueVisible;

        if (IsHotkeyPressed(_togglePanelHotkey))
        {
            _panelVisible = !dialogueVisible && !_panelVisible;
        }

        if (_panelVisible && !dialogueVisible && Input.GetKeyDown(KeyCode.Escape))
        {
            _panelVisible = false;
        }

        if (!dialogueVisible && !_panelVisible && IsHotkeyPressed(_summonOrSwapHotkey))
        {
            TrySummonOrSwap("hotkey");
        }

        if (!dialogueVisible && !_panelVisible && IsHotkeyPressed(_recallHotkey))
        {
            RecallCompanion("hotkey");
        }

        if (!dialogueVisible && !_panelVisible && IsHotkeyPressed(_defendToggleHotkey))
        {
            ToggleDefendMode("hotkey");
        }

        if (!dialogueVisible && !_panelVisible && IsHotkeyPressed(_dismissHotkey))
        {
            DismissCompanion("hotkey");
        }

        if (!dialogueVisible && !_panelVisible)
        {
            TryOpenNativeCompanionMenuFromInteractFallback();
        }

        if (!dialogueVisible)
        {
            TickCompanion();
        }

        CustomBroodmotherSpellFeature.Tick(Logger);
    }

    private void OnGUI()
    {
        if (!_enabled.Value)
        {
            return;
        }

        bool dialogueVisible = IsCompanionDialogueVisible;
        if (dialogueVisible)
        {
            _dialogueHost?.MaintainFromGuiPass();
        }

        if (_showCompanionHud.Value && !dialogueVisible)
        {
            DrawCompanionHud();
        }

        if (!_panelVisible || dialogueVisible)
        {
            return;
        }

        _panelRect = GUI.Window(PanelWindowId, ClampPanelToScreen(_panelRect), DrawPanel, PluginName);
    }

    private void OnDestroy()
    {
        _dialogueHost?.Destroy();
        _embeddedAudioRuntime?.Dispose();
        _embeddedAudioRuntime = null;
        _spiderBlazeBridge?.Dispose();
        _spiderBlazeBridge = null;
        RemoveAllNativeCompanionPrompts();
        DismissCompanion("plugin shutdown");
        ReleaseBroodmotherRetainedDeathVisualReferences();
        BroodmotherCallItemIconPatch.Release();
        _harmony?.UnpatchSelf();
        if (Instance == this)
        {
            Instance = null;
        }
    }

    internal void TrySummonOrSwapFromSpell()
    {
        TrySummonOrSwap(
            SpiderFamilyCompanionDefinitions.BroodmotherSkin1,
            "broodmother-call-spell",
            requireRuntimeSummon: false);
    }

    private void TrySummonOrSwap(string source, bool requireRuntimeSummon = true)
    {
        TrySummonOrSwap(SpiderFamilyCompanionDefinitions.BroodmotherSkin1, source, requireRuntimeSummon);
    }

    internal void TrySummonOrSwapFromSpell(SpiderFamilyCompanionDefinition definition)
    {
        TrySummonOrSwap(definition, "spider-family-call-spell:" + definition.Id, requireRuntimeSummon: false);
    }

    private void TrySummonOrSwap(
        SpiderFamilyCompanionDefinition definition,
        string source,
        bool requireRuntimeSummon = true)
    {
        if (!_enabled.Value)
        {
            SetPanelStatus("Summon blocked by General.Enabled.");
            LogCommand("summon-or-swap", "blocked", "general-enabled-false");
            return;
        }

        if (requireRuntimeSummon && !_allowRuntimeSummon.Value)
        {
            SetPanelStatus("Summon blocked by Safety.AllowRuntimeSummon.");
            LogCommand("summon-or-swap", "blocked", "allow-runtime-summon-false");
            return;
        }

        Hero? hero = Hero.Current;
        if (hero == null)
        {
            SetPanelStatus("Summon blocked: Hero.Current is null.");
            LogCommand("summon-or-swap", "blocked", "hero-current-null");
            return;
        }

        if (!TryResolveCompanionTemplate(definition, out LocationTemplate? template, out string reason) || template == null)
        {
            SetPanelStatus("Summon blocked: " + reason);
            Logger.LogWarning($"{PluginName} summon blocked: {reason}");
            LogCommand("summon-or-swap", "blocked", reason);
            return;
        }

        if (!TryApplyCompanionVisualTemplateSelection(template, definition, out reason))
        {
            SetPanelStatus("Summon blocked: " + reason);
            Logger.LogWarning($"{PluginName} summon blocked: {reason}");
            LogCommand("summon-or-swap", "blocked", reason);
            return;
        }

        if (HasActiveCompanion())
        {
            DismissCompanion("summon swap");
        }

        Vector3 spawnPosition = hero.Coords + hero.Rotation * GetCompanionOffset();
        Location location;
        try
        {
            location = template.SpawnLocation(
                spawnPosition,
                hero.Rotation,
                null,
                null,
                definition.DisplayName,
                hero.ParentTransform.gameObject.scene);
            location.MarkedNotSaved = true;
        }
        catch (Exception ex)
        {
            reason = "spawn-location-failed-" + ex.GetType().Name;
            SetPanelStatus("Summon failed during SpawnLocation.");
            Logger.LogWarning($"{PluginName} summon failed during {source}: {ex.GetType().Name}: {ex.Message}");
            LogCommand("summon-or-swap", "failed", reason);
            return;
        }

        if (!TryValidateSpawnedLocation(location, definition, out NpcElement? npcElement, out reason) || npcElement == null)
        {
            FailSpawnedLocation(location, "summon-or-swap", reason);
            return;
        }

        if (!TryApplyNativeAllyMarker(location, npcElement, hero, out reason))
        {
            FailSpawnedLocation(location, "summon-or-swap", reason);
            return;
        }

        _broodmotherLifecycleReadyLogged = false;
        _broodmotherLifecycleWarningLogged = false;
        _broodmotherLifecycleWarningReason = string.Empty;
        _broodmotherAnimatedDeathVisibilityLogged = false;
        _broodmotherNativeDeathAcceptedLogged = false;
        _broodmotherNativeDeathPendingLogged = false;
        if (!TryMaintainBroodmotherLifecycleSurface(
                location,
                npcElement,
                "summon-or-swap",
                failIfControllerIncomplete: false,
                out reason,
                definition))
        {
            FailSpawnedLocation(location, "summon-or-swap", reason);
            return;
        }

        _activeBroodmother = location;
        _activeDefinition = definition;
        _defendMode = false;
        _holdPositionMode = false;
        _followRange = BroodmotherFollowRange.Normal;
        _broodmotherOverlayFailureLogged = false;
        _broodmotherLifecycleReadyLogged = false;
        _broodmotherLifecycleWarningLogged = false;
        _broodmotherLifecycleWarningReason = string.Empty;
        _broodmotherAnimatedDeathVisibilityLogged = false;
        _broodmotherNativeDeathAcceptedLogged = false;
        _broodmotherNativeDeathPendingLogged = false;
        _nextAdvancedSpiderCombatPromptTime = 0f;
        _nextAdvancedSpiderAiLogTime = 0f;
        _advancedSpiderWaveSeed = unchecked(_advancedSpiderWaveSeed + 1);
        _advancedSpiderTelegraphReady = false;
        _nextTickTime = Time.unscaledTime + Math.Max(0.2f, _tickSeconds.Value);
        location.MarkedNotSaved = true;
        EnsureNativeCompanionPrompt(location);
        if (!TryEnsureBroodmotherNativeVisual(npcElement, definition, out string visualReason) && !_broodmotherOverlayFailureLogged)
        {
            _broodmotherOverlayFailureLogged = true;
            Logger.LogWarning($"{PluginName} native visual failed after {source}: {visualReason}");
        }

        SetPanelStatus(definition.DisplayName + " summoned.");
        Logger.LogInfo($"{PluginName} summoned {definition.DisplayName} at {FormatVector(spawnPosition)}. template={definition.LocationTemplateName}[{definition.LocationTemplateGuid}], npcTemplate={definition.NpcTemplateGuid}, visual={definition.VisualAddress}, templateVisual={definition.TemplateVisualAddress}, runtimeOverlay={definition.RuntimeOverlayVisualAddress ?? "none"}, variant={definition.Id}, nativeVisual={visualReason}, visualScaleMultiplier={definition.VisualScaleMultiplier:0.###}, notSaved=1, ally=1.");
        LogCommand("summon-or-swap", "summoned", source, location, npcElement);
        TryPlayEmbeddedSpiderAudio(BroodmotherEmbeddedAudioCue.Call, location, "summon-or-swap:" + source);
    }

    private bool TryResolveBroodmotherTemplate([NotNullWhen(true)] out LocationTemplate? template, out string reason)
    {
        return TryResolveCompanionTemplate(
            SpiderFamilyCompanionDefinitions.BroodmotherSkin1,
            out template,
            out reason);
    }

    private bool TryResolveCompanionTemplate(
        SpiderFamilyCompanionDefinition definition,
        [NotNullWhen(true)] out LocationTemplate? template,
        out string reason)
    {
        template = null;

        Type? apiType = Type.GetType(ResolverTypeName, throwOnError: false);
        if (apiType == null)
        {
            reason = "Avalon Awakened resolver type was not found.";
            return false;
        }

        MethodInfo? resolver = apiType.GetMethod(definition.ResolverMethodName, BindingFlags.Public | BindingFlags.Static);
        if (resolver == null)
        {
            reason = $"Avalon Awakened {definition.DisplayName} resolver method was not found.";
            return false;
        }

        ParameterInfo[] parameters = resolver.GetParameters();
        if (parameters.Length != 2 ||
            !parameters[0].ParameterType.IsByRef ||
            !parameters[1].ParameterType.IsByRef)
        {
            reason = $"Avalon Awakened {definition.DisplayName} resolver signature changed.";
            return false;
        }

        object?[] args = { null, string.Empty };
        try
        {
            object? invokeResult = resolver.Invoke(null, args);
            if (invokeResult is not bool resolved || !resolved)
            {
                reason = args[1] as string ?? $"Avalon Awakened {definition.DisplayName} resolver returned false.";
                return false;
            }
        }
        catch (TargetInvocationException ex)
        {
            Exception inner = ex.InnerException ?? ex;
            reason = $"Avalon Awakened {definition.DisplayName} resolver threw " + inner.GetType().Name + ": " + inner.Message;
            return false;
        }
        catch (Exception ex)
        {
            reason = $"Avalon Awakened {definition.DisplayName} resolver failed: " + ex.GetType().Name + ": " + ex.Message;
            return false;
        }

        template = args[0] as LocationTemplate;
        if (template == null)
        {
            reason = $"Avalon Awakened {definition.DisplayName} resolver returned null template.";
            return false;
        }

        return ValidateResolvedTemplate(template, definition, out reason);
    }

    private static bool ValidateResolvedTemplate(
        LocationTemplate template,
        SpiderFamilyCompanionDefinition definition,
        out string reason)
    {
        if (!string.Equals(template.name, definition.LocationTemplateName, StringComparison.Ordinal))
        {
            reason = $"{definition.DisplayName} LocationTemplate name changed.";
            return false;
        }

        if (!string.Equals(template.GUID, definition.LocationTemplateGuid, StringComparison.OrdinalIgnoreCase))
        {
            reason = $"{definition.DisplayName} LocationTemplate GUID changed.";
            return false;
        }

        NpcAttachment? npcAttachment = template.GetComponent<NpcAttachment>();
        if (npcAttachment == null)
        {
            reason = $"{definition.DisplayName} LocationTemplate has no NpcAttachment.";
            return false;
        }

        if (npcAttachment.IsUnique)
        {
            reason = $"{definition.DisplayName} LocationTemplate is unique and cannot be used for one-session companion spawning.";
            return false;
        }

        RepetitiveNpcAttachment? repetitiveAttachment = template.GetComponent<RepetitiveNpcAttachment>();
        if (repetitiveAttachment == null || repetitiveAttachment.NpcTemplate == null)
        {
            reason = $"{definition.DisplayName} LocationTemplate has no repetitive NPC template.";
            return false;
        }

        if (!string.Equals(repetitiveAttachment.NpcTemplate.GUID, definition.NpcTemplateGuid, StringComparison.OrdinalIgnoreCase))
        {
            reason = $"{definition.DisplayName} linked NpcTemplate GUID changed.";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    private static bool TryApplyCompanionVisualTemplateSelection(
        LocationTemplate template,
        SpiderFamilyCompanionDefinition definition,
        out string reason)
    {
        RepetitiveNpcAttachment? repetitiveAttachment = template.GetComponent<RepetitiveNpcAttachment>();
        if (repetitiveAttachment == null)
        {
            reason = $"{definition.DisplayName} LocationTemplate has no RepetitiveNpcAttachment.";
            return false;
        }

        NpcTemplate npcTemplate = repetitiveAttachment.NpcTemplate;
        if (npcTemplate == null ||
            !string.Equals(npcTemplate.GUID, definition.NpcTemplateGuid, StringComparison.OrdinalIgnoreCase))
        {
            reason = $"{definition.DisplayName} NpcTemplate identity changed before companion spawn.";
            return false;
        }

        repetitiveAttachment.Setup(npcTemplate, new ARAssetReference(definition.TemplateVisualAddress));
        ARAssetReference visual = repetitiveAttachment.VisualPrefab();
        if (visual == null ||
            !string.Equals(visual.Address, definition.TemplateVisualAddress, StringComparison.Ordinal))
        {
            reason = $"{definition.DisplayName} visual address did not read back before companion spawn.";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    private static bool TryValidateSpawnedLocation(
        Location location,
        SpiderFamilyCompanionDefinition definition,
        [NotNullWhen(true)] out NpcElement? npcElement,
        out string reason)
    {
        npcElement = null;
        location.MarkedNotSaved = true;

        if (!string.Equals(location.Template?.name, definition.LocationTemplateName, StringComparison.Ordinal) ||
            !string.Equals(location.Template?.GUID, definition.LocationTemplateGuid, StringComparison.OrdinalIgnoreCase))
        {
            reason = "spawned location template identity changed.";
            return false;
        }

        if (!string.Equals(location.DisplayName, definition.DisplayName, StringComparison.Ordinal))
        {
            reason = $"spawned display name changed (actual={location.DisplayName}).";
            return false;
        }

        if (!location.TryGetElement(out NpcElement npc) || npc == null)
        {
            reason = "spawned location has no NpcElement.";
            return false;
        }

        if (npc.IsUnique)
        {
            reason = "spawned NpcElement is unique.";
            return false;
        }

        if (!string.Equals(npc.Template?.GUID, definition.NpcTemplateGuid, StringComparison.OrdinalIgnoreCase))
        {
            reason = "spawned NpcTemplate GUID changed.";
            return false;
        }

        npcElement = npc;
        reason = string.Empty;
        return true;
    }

    private bool TryApplyNativeAllyMarker(Location location, NpcElement npcElement, Hero hero, out string reason)
    {
        try
        {
            location.MarkedNotSaved = true;
            npcElement.OverrideFaction(hero.GetFactionTemplateForSummon(), FactionOverrideContext.Summon);
            if (!npcElement.HasElement<NpcHeroPetAlly>())
            {
                npcElement.AddElement(new NpcHeroPetAlly(hero));
            }

            location.MarkedNotSaved = true;
            NpcHeroPetAlly marker = npcElement.TryGetElement<NpcHeroPetAlly>();
            if (marker == null || marker.HasBeenDiscarded)
            {
                reason = "native ally marker readback failed.";
                return false;
            }

            reason = string.Empty;
            return true;
        }
        catch (Exception ex)
        {
            reason = "native ally marker setup failed: " + ex.GetType().Name + ": " + ex.Message;
            Logger.LogWarning($"{PluginName} {reason}");
            return false;
        }
    }

    private bool TryMaintainBroodmotherLifecycleSurface(
        Location location,
        NpcElement npcElement,
        string source,
        bool failIfControllerIncomplete,
        out string reason,
        SpiderFamilyCompanionDefinition? definition = null)
    {
        reason = string.Empty;
        SpiderFamilyCompanionDefinition companionDefinition =
            definition ?? ResolveCompanionDefinition(location, npcElement);
        try
        {
            if (location == null || location.HasBeenDiscarded)
            {
                reason = "location-unavailable";
                return false;
            }

            if (npcElement == null || npcElement.HasBeenDiscarded)
            {
                reason = "npc-element-unavailable";
                return false;
            }

            if (!npcElement.IsAlive)
            {
                if (TryAcceptBroodmotherNativeDeathHandoff(
                        location,
                        "npc-not-alive:" + source,
                        companionDefinition))
                {
                    reason = "native-death-accepted";
                    return true;
                }

                reason = "npc-not-alive-before-native-death-handoff";
                return false;
            }

            location.MarkedNotSaved = true;
            npcElement.KeepCorpseAfterDeath = true;
            if (!npcElement.KeepCorpseAfterDeath)
            {
                reason = "native same-session corpse-retention flag did not read back true.";
                return false;
            }

            if (!npcElement.TryGetElement(out DeathElement death) || death == null)
            {
                reason = "native DeathElement missing.";
                return false;
            }

            bool deathKeepBody = death.KeepBody;
            bool hideHealthBarRemoved = RemoveBroodmotherHiddenHealthBar(location);
            bool summonInvisibilityRemoved = RemoveBroodmotherSummonInvisibility(npcElement);
            bool healthReadable = TryReadCompanionHealthPercent(npcElement, out float healthPercent);

            if (!TryEnsureBroodmotherHitSurface(
                    location,
                    npcElement,
                    death,
                    failIfControllerIncomplete,
                    out bool controllerReady,
                    out int colliders,
                    out int enabledColliders,
                    out int colliderChanges,
                    out int healthHitboxes,
                    out int healthHitboxAdds,
                    out int hitboxLayerChanges,
                    out int hitboxLayer,
                    out int nonTriggerColliders,
                    out int enabledNonTriggerColliders,
                    out bool animatedDeathReady,
                    out int deathBehaviours,
                    out int initializedDeathBehaviours,
                    out bool deathBehaviourLinked,
                    out bool animatedDeathVisibilityReady,
                    out bool alivePrefabSuppressionCleared,
                    out string hitSurfaceReason))
            {
                reason = hitSurfaceReason;
                return false;
            }

            bool lifecycleReady = controllerReady && animatedDeathReady;
            if (lifecycleReady && !_broodmotherLifecycleReadyLogged)
            {
                _broodmotherLifecycleReadyLogged = true;
                _broodmotherLifecycleWarningLogged = false;
                _broodmotherLifecycleWarningReason = string.Empty;
                Logger.LogInfo(
                    "BROODMOTHER_COMPANION_LIFECYCLE_READY " +
                    $"source={FormatMarkerValue(source)}, template={companionDefinition.LocationTemplateName}[{companionDefinition.LocationTemplateGuid}], npcTemplate={companionDefinition.NpcTemplateGuid}, variant={companionDefinition.Id}, " +
                    $"keepCorpse={BoolText(npcElement.KeepCorpseAfterDeath)}, deathElement=1, deathKeepBody={BoolText(deathKeepBody)}, animatedDeath={BoolText(animatedDeathReady)}, " +
                    $"deathBehaviours={deathBehaviours}, initializedDeathBehaviours={initializedDeathBehaviours}, deathBehaviourLinked={BoolText(deathBehaviourLinked)}, " +
                    $"animatedDeathVisibilityReady={BoolText(animatedDeathVisibilityReady)}, alivePrefabSuppressionCleared={BoolText(alivePrefabSuppressionCleared)}, " +
                    $"healthReadable={BoolText(healthReadable)}, healthPercent={(healthReadable ? (healthPercent * 100f).ToString("0") : "unknown")}, " +
                    $"hideHealthBarRemoved={BoolText(hideHealthBarRemoved)}, summonInvisibilityRemoved={BoolText(summonInvisibilityRemoved)}, " +
                    $"colliders={colliders}, enabledColliders={enabledColliders}, colliderChanges={colliderChanges}, " +
                    $"healthHitboxes={healthHitboxes}, healthHitboxAdds={healthHitboxAdds}, hitboxLayerChanges={hitboxLayerChanges}, hitboxLayer={(hitboxLayer >= 0 ? hitboxLayer.ToString() : "missing")}, " +
                    $"nonTriggerColliders={nonTriggerColliders}, enabledNonTriggerColliders={enabledNonTriggerColliders}, heroFriendlyFireBypass=broodmother-only, " +
                    "nativeDummyCorpseRequired=1, nativeDeathOwner=DeathElement, persistence=false.");
            }
            else if (!lifecycleReady &&
                (!_broodmotherLifecycleWarningLogged ||
                    !string.Equals(_broodmotherLifecycleWarningReason, hitSurfaceReason, StringComparison.Ordinal)))
            {
                _broodmotherLifecycleWarningLogged = true;
                _broodmotherLifecycleWarningReason = hitSurfaceReason;
                Logger.LogInfo(
                    "BROODMOTHER_COMPANION_LIFECYCLE_WAITING " +
                    $"source={FormatMarkerValue(source)}, reason={FormatMarkerValue(hitSurfaceReason)}, keepCorpse={BoolText(npcElement.KeepCorpseAfterDeath)}, " +
                    $"deathElement=1, deathKeepBody={BoolText(deathKeepBody)}, animatedDeath={BoolText(animatedDeathReady)}, " +
                    $"deathBehaviours={deathBehaviours}, initializedDeathBehaviours={initializedDeathBehaviours}, deathBehaviourLinked={BoolText(deathBehaviourLinked)}, " +
                    $"animatedDeathVisibilityReady={BoolText(animatedDeathVisibilityReady)}, alivePrefabSuppressionCleared={BoolText(alivePrefabSuppressionCleared)}, " +
                    $"hideHealthBarRemoved={BoolText(hideHealthBarRemoved)}, summonInvisibilityRemoved={BoolText(summonInvisibilityRemoved)}, persistence=false.");
            }

            reason = lifecycleReady ? "lifecycle-ready" : hitSurfaceReason;
            return true;
        }
        catch (Exception ex)
        {
            reason = "lifecycle setup failed: " + ex.GetType().Name + ": " + ex.Message;
            Logger.LogWarning($"{PluginName} {reason}");
            return false;
        }
    }

    private static bool RemoveBroodmotherHiddenHealthBar(Location location)
    {
        if (!location.HasElement<HideHealthBar>())
        {
            return false;
        }

        location.RemoveElementsOfType<HideHealthBar>();
        location.MarkedNotSaved = true;
        return true;
    }

    private static bool RemoveBroodmotherSummonInvisibility(NpcElement npcElement)
    {
        HeroSummonInvisibility invisibility = npcElement.TryGetElement<HeroSummonInvisibility>();
        if (invisibility == null || invisibility.HasBeenDiscarded)
        {
            return false;
        }

        invisibility.Discard();
        return true;
    }

    private bool TryEnsureBroodmotherHitSurface(
        Location location,
        NpcElement npcElement,
        DeathElement death,
        bool failIfControllerIncomplete,
        out bool controllerReady,
        out int colliders,
        out int enabledColliders,
        out int colliderChanges,
        out int healthHitboxes,
        out int healthHitboxAdds,
        out int hitboxLayerChanges,
        out int hitboxLayer,
        out int nonTriggerColliders,
        out int enabledNonTriggerColliders,
        out bool animatedDeathReady,
        out int deathBehaviours,
        out int initializedDeathBehaviours,
        out bool deathBehaviourLinked,
        out bool animatedDeathVisibilityReady,
        out bool alivePrefabSuppressionCleared,
        out string reason)
    {
        controllerReady = false;
        colliders = 0;
        enabledColliders = 0;
        colliderChanges = 0;
        healthHitboxes = 0;
        healthHitboxAdds = 0;
        hitboxLayerChanges = 0;
        hitboxLayer = LayerMask.NameToLayer("Hitboxes");
        nonTriggerColliders = 0;
        enabledNonTriggerColliders = 0;
        animatedDeathReady = false;
        deathBehaviours = 0;
        initializedDeathBehaviours = 0;
        deathBehaviourLinked = false;
        animatedDeathVisibilityReady = false;
        alivePrefabSuppressionCleared = false;

        if (npcElement.Controller == null)
        {
            reason = "waiting-for-npc-controller";
            return !failIfControllerIncomplete;
        }

        GameObject root = npcElement.Controller.AlivePrefab != null
            ? npcElement.Controller.AlivePrefab
            : npcElement.Controller.gameObject;
        if (root == null)
        {
            reason = "npc-controller-alive-prefab-unavailable";
            return !failIfControllerIncomplete;
        }

        controllerReady = true;
        HealthElement healthElement = npcElement.HealthElement;
        if (healthElement == null || healthElement.HasBeenDiscarded)
        {
            reason = "Broodmother native actor HealthElement unavailable for runtime hitbox registration.";
            return false;
        }

        Collider[] actorColliders = root.GetComponentsInChildren<Collider>(true);
        colliders = actorColliders.Length;
        foreach (Collider collider in actorColliders)
        {
            if (collider == null)
            {
                continue;
            }

            if (!collider.enabled)
            {
                collider.enabled = true;
                colliderChanges++;
            }

            if (collider.enabled)
            {
                enabledColliders++;
                EnsureBroodmotherRuntimeHealthHitbox(
                    location,
                    healthElement,
                    collider,
                    hitboxLayer,
                    ref healthHitboxes,
                    ref healthHitboxAdds,
                    ref hitboxLayerChanges);
            }

            if (!collider.isTrigger)
            {
                nonTriggerColliders++;
                if (collider.enabled)
                {
                    enabledNonTriggerColliders++;
                }
            }
        }

        PlayAnimationDeathBehaviour[] visualDeathBehaviours =
            root.GetComponentsInChildren<PlayAnimationDeathBehaviour>(true);
        deathBehaviours = visualDeathBehaviours.Length;
        PlayAnimationDeathBehaviour? animationDeath = death.GetBehaviour<PlayAnimationDeathBehaviour>();
        for (int i = 0; i < visualDeathBehaviours.Length; i++)
        {
            PlayAnimationDeathBehaviour candidate = visualDeathBehaviours[i];
            if (candidate != null && candidate.IsVisualInitialized)
            {
                initializedDeathBehaviours++;
            }

            if (animationDeath != null && ReferenceEquals(candidate, animationDeath))
            {
                deathBehaviourLinked = true;
            }
        }

        animatedDeathReady = animationDeath != null &&
            deathBehaviourLinked &&
            animationDeath.IsVisualInitialized &&
            death.KeepBody;

        if (colliders <= 0)
        {
            reason = "Broodmother native actor has no colliders under AlivePrefab.";
            return false;
        }

        if (enabledColliders <= 0)
        {
            reason = "Broodmother native actor colliders remained disabled after lifecycle surface reassertion.";
            return false;
        }

        if (healthHitboxes <= 0)
        {
            reason = "Broodmother native actor colliders could not be registered with HealthElement hitboxes.";
            return false;
        }

        if (!animatedDeathReady)
        {
            reason = "waiting-for-native-animated-death-behaviour";
            return !failIfControllerIncomplete;
        }

        if (!TryCorrectBroodmotherAnimatedDeathVisibility(
                death,
                animationDeath!,
                out alivePrefabSuppressionCleared,
                out reason))
        {
            return false;
        }

        animatedDeathVisibilityReady = true;
        reason = "hit-surface-ready";
        return true;
    }

    private bool TryCorrectBroodmotherAnimatedDeathVisibility(
        DeathElement death,
        PlayAnimationDeathBehaviour initializedBehaviour,
        out bool alivePrefabSuppressionCleared,
        out string reason)
    {
        alivePrefabSuppressionCleared = false;
        if (death == null || death.HasBeenDiscarded)
        {
            reason = "Broodmother native DeathElement became unavailable before death visibility correction.";
            return false;
        }

        if (initializedBehaviour == null || !initializedBehaviour.IsVisualInitialized)
        {
            reason = "Broodmother native animated-death behavior is missing or not visually initialized.";
            return false;
        }

        PlayAnimationDeathBehaviour? animationDeath = death.GetBehaviour<PlayAnimationDeathBehaviour>();
        if (animationDeath == null ||
            !ReferenceEquals(animationDeath, initializedBehaviour) ||
            !animationDeath.IsVisualInitialized)
        {
            reason = "Broodmother native animated-death behavior is missing, mismatched, or not visually initialized.";
            return false;
        }

        if (!death.KeepBody)
        {
            reason = "Broodmother native CustomDeathController no longer retains its body.";
            return false;
        }

        FieldInfo? alivePrefabField = PlayAnimationAlivePrefabField;
        if (alivePrefabField == null || alivePrefabField.FieldType != typeof(GameObject))
        {
            reason = "the reviewed PlayAnimationDeathBehaviour AlivePrefab suppression field changed.";
            return false;
        }

        bool suppressionTargetWasPresent = alivePrefabField.GetValue(animationDeath) is GameObject;
        alivePrefabField.SetValue(animationDeath, null);
        if (alivePrefabField.GetValue(animationDeath) != null)
        {
            reason = "the animated-death AlivePrefab suppression target did not clear.";
            return false;
        }

        alivePrefabSuppressionCleared = true;
        if (!_broodmotherAnimatedDeathVisibilityLogged)
        {
            _broodmotherAnimatedDeathVisibilityLogged = true;
            Logger.LogWarning(
                "BROODMOTHER_COMPANION_ANIMATED_DEATH_VISIBILITY_READY " +
                $"suppressionTargetWasPresent={BoolText(suppressionTargetWasPresent)}, alivePrefabSuppressionCleared=1, " +
                "deathState=44, keepBody=true, ragdoll=false, persistence=false.");
        }

        reason = string.Empty;
        return true;
    }

    private void EnsureBroodmotherRuntimeHealthHitbox(
        Location location,
        HealthElement healthElement,
        Collider collider,
        int hitboxLayer,
        ref int healthHitboxes,
        ref int healthHitboxAdds,
        ref int hitboxLayerChanges)
    {
        if (location == null || location.HasBeenDiscarded || collider == null || !collider.enabled)
        {
            return;
        }

        GameObject hitboxObject = collider.gameObject;
        if (hitboxLayer >= 0 && hitboxObject != null && hitboxObject.layer != hitboxLayer)
        {
            if (!_broodmotherRuntimeHitboxLayers.ContainsKey(collider))
            {
                _broodmotherRuntimeHitboxLayers.Add(collider, hitboxObject.layer);
            }

            hitboxObject.layer = hitboxLayer;
            hitboxLayerChanges++;
        }

        if (!_broodmotherRuntimeHealthHitboxes.Contains(collider))
        {
            HitboxData hitboxData = HitboxData.Default;
            hitboxData.ValidateAndCorrectCanBeHit();
            healthElement.AddHitbox(collider, in hitboxData);
            _broodmotherRuntimeHealthHitboxes.Add(collider);
            healthHitboxAdds++;
        }

        if (healthElement.HasHitbox(collider))
        {
            healthHitboxes++;
        }
    }

    private void ReleaseBroodmotherRuntimeHitSurface(Location? location)
    {
        try
        {
            HealthElement? healthElement = null;
            if (location != null &&
                !location.HasBeenDiscarded &&
                location.TryGetElement(out NpcElement npcElement) &&
                npcElement != null &&
                !npcElement.HasBeenDiscarded)
            {
                healthElement = npcElement.HealthElement;
            }

            if (healthElement != null && !healthElement.HasBeenDiscarded)
            {
                foreach (Collider collider in _broodmotherRuntimeHealthHitboxes)
                {
                    if (collider != null)
                    {
                        healthElement.RemoveHitbox(collider);
                    }
                }
            }

            foreach (KeyValuePair<Collider, int> entry in _broodmotherRuntimeHitboxLayers)
            {
                Collider collider = entry.Key;
                if (collider != null && collider.gameObject != null)
                {
                    collider.gameObject.layer = entry.Value;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"{PluginName} runtime hit surface cleanup failed: {ex.GetType().Name}: {ex.Message}");
        }
        finally
        {
            _broodmotherRuntimeHealthHitboxes.Clear();
            _broodmotherRuntimeHitboxLayers.Clear();
        }
    }

    private bool TryAcceptBroodmotherNativeDeathHandoff(
        Location location,
        string source,
        SpiderFamilyCompanionDefinition? definition = null)
    {
        SpiderFamilyCompanionDefinition companionDefinition =
            definition ?? ResolveCompanionDefinition(location);
        if (!HasAcceptedBroodmotherNativeDeathTransition(location, companionDefinition, out NpcDummy? dummy) ||
            dummy == null)
        {
            return false;
        }

        if (!_broodmotherNativeDeathAcceptedLogged)
        {
            _broodmotherNativeDeathAcceptedLogged = true;
            SetPanelStatus("Broodmother died.");
            Logger.LogWarning(
                "BROODMOTHER_COMPANION_NATIVE_DEATH_ACCEPTED " +
                $"source={FormatMarkerValue(source)}, location={FormatMarkerValue(location.ID ?? string.Empty)}, " +
                $"npcTemplate={dummy.Template?.GUID ?? "unknown"}, variant={companionDefinition.Id}, npcDummy=true, corpse=true, dummyHasDied={BoolText(dummy.HasDied)}, " +
                $"markedNotSaved={BoolText(location.MarkedNotSaved)}, isNotSaved={BoolText(location.IsNotSaved)}, persistence=false.");
            LogCommand(
                "native-death",
                "accepted",
                $"source={source};npcTemplate={dummy.Template?.GUID ?? "unknown"};npcDummy=1;corpse=1;dummyHasDied={BoolText(dummy.HasDied)}",
                location);
        }

        return true;
    }

    private static bool HasAcceptedBroodmotherNativeDeathTransition(
        Location location,
        SpiderFamilyCompanionDefinition definition,
        [NotNullWhen(true)] out NpcDummy? dummy)
    {
        dummy = null;
        if (location == null || location.HasBeenDiscarded)
        {
            return false;
        }

        if (!location.TryGetElement(out NpcDummy candidate) || candidate == null ||
            !location.TryGetElement(out Corpse corpse) || corpse == null ||
            candidate.HasDied)
        {
            return false;
        }

        string dummyTemplateGuid = candidate.Template?.GUID ?? string.Empty;
        if (!string.Equals(dummyTemplateGuid, definition.NpcTemplateGuid, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        dummy = candidate;
        return true;
    }

    private void TickCompanion()
    {
        if (Time.unscaledTime < _nextTickTime)
        {
            return;
        }

        _nextTickTime = Time.unscaledTime + Math.Max(0.2f, _tickSeconds.Value);
        if (!TryGetActiveCompanion(out Location? location, out NpcElement? npcElement))
        {
            return;
        }

        SpiderFamilyCompanionDefinition definition = ActiveDefinition;
        if (!TryEnsureBroodmotherNativeVisual(npcElement, definition, out string visualReason) && !_broodmotherOverlayFailureLogged)
        {
            _broodmotherOverlayFailureLogged = true;
            SetPanelStatus(definition.DisplayName + " visual failed: " + visualReason);
            Logger.LogWarning($"{PluginName} native visual failed during tick: {visualReason}");
        }

        if (!TryMaintainBroodmotherLifecycleSurface(
                location,
                npcElement,
                "tick",
                failIfControllerIncomplete: false,
                out string lifecycleReason,
                definition))
        {
            if (!_broodmotherLifecycleWarningLogged)
            {
                _broodmotherLifecycleWarningLogged = true;
                Logger.LogWarning($"{PluginName} {definition.DisplayName} lifecycle surface failed during tick: {lifecycleReason}");
            }

            SetPanelStatus(definition.DisplayName + " lifecycle warning: " + lifecycleReason);
        }

        location.MarkedNotSaved = true;
        EnsureNativeCompanionPrompt(location);
        Hero? hero = Hero.Current;
        if (hero == null)
        {
            return;
        }

        bool advancedAiHandledCombat = ActiveDefinition.UsesSpiderCombatPackage &&
            _enableAdvancedSpiderCompanionAi.Value &&
            AdvanceAdvancedSpiderCompanionAi(location, npcElement, hero);
        bool immediatePressureHandled = !advancedAiHandledCombat &&
            !ActiveDefinition.UsesSpiderCombatPackage &&
            _enableImmediateTargetPressure.Value &&
            TryApplyImmediateTargetPressure(location, npcElement, hero);

        if (!_defendMode && !_holdPositionMode && !advancedAiHandledCombat && !immediatePressureHandled)
        {
            float distance = Vector3.Distance(location.Coords, hero.Coords);
            if (distance > 4f)
            {
                TryPlayEmbeddedSpiderAudio(BroodmotherEmbeddedAudioCue.Walk, location, "follow-distance");
            }

            if (_enableFollowCatchUp.Value && distance > GetEffectiveFollowCatchUpDistance())
            {
                RecallCompanion("follow catch-up");
            }
        }

        if (_enableNativeDefendAssist.Value && _defendMode && !advancedAiHandledCombat && !immediatePressureHandled)
        {
            int attackers = CountLiveHeroAttackers(hero);
            if (attackers > 0)
            {
                TriggerDefendAssist(location, npcElement, attackers, "defend mode");
            }
        }
    }

    private bool TryGetActiveCompanion([NotNullWhen(true)] out Location? location, [NotNullWhen(true)] out NpcElement? npcElement)
    {
        location = null;
        npcElement = null;
        if (_activeBroodmother == null)
        {
            return false;
        }

        if (_activeBroodmother.HasBeenDiscarded)
        {
            ClearActiveCompanion();
            return false;
        }

        if (!_activeBroodmother.TryGetElement(out NpcElement npc) || npc == null || npc.HasBeenDiscarded)
        {
            HandleBroodmotherNativeDeathPending(_activeBroodmother, "npc-element-missing-or-discarded");
            return false;
        }

        if (!npc.IsAlive)
        {
            HandleBroodmotherNativeDeathPending(_activeBroodmother, "npc-not-alive");
            return false;
        }

        NpcHeroPetAlly marker = npc.TryGetElement<NpcHeroPetAlly>();
        if (marker == null || marker.HasBeenDiscarded)
        {
            RemoveNativeCompanionPrompt(_activeBroodmother);
            ClearActiveCompanion();
            return false;
        }

        _activeBroodmother.MarkedNotSaved = true;
        location = _activeBroodmother;
        npcElement = npc;
        return true;
    }

    private void HandleBroodmotherNativeDeathPending(Location location, string source)
    {
        if (location == null || location.HasBeenDiscarded)
        {
            ClearActiveCompanion();
            return;
        }

        location.MarkedNotSaved = true;
        RemoveNativeCompanionPrompt(location);
        _dialogueHost?.Close("native-death-pending", preserveStatus: true);
        _embeddedAudioRuntime?.StopAllVoices();
        SpiderFamilyCompanionDefinition definition = ResolveCompanionDefinition(location);

        if (TryAcceptBroodmotherNativeDeathHandoff(location, source, definition))
        {
            ClearActiveCompanion(retainDeathVisual: true);
            return;
        }

        SetPanelStatus(definition.DisplayName + " death handoff pending.");
        if (!_broodmotherNativeDeathPendingLogged)
        {
            _broodmotherNativeDeathPendingLogged = true;
            Logger.LogWarning(
                "BROODMOTHER_COMPANION_NATIVE_DEATH_PENDING " +
                $"source={FormatMarkerValue(source)}, location={FormatMarkerValue(location.ID ?? string.Empty)}, " +
                $"variant={definition.Id}, waitingForNativeDummyCorpse=1, activeLocationRetained=1, visualMaterialsRetained=1, persistence=false.");
            LogCommand("native-death", "pending", $"source={source};waitingForNativeDummyCorpse=1", location);
        }
    }

    private bool HasActiveCompanion()
    {
        return TryGetActiveCompanion(out _, out _);
    }

    private SpiderFamilyCompanionDefinition ResolveCompanionDefinition(
        Location? location,
        NpcElement? npcElement = null)
    {
        if (_activeDefinition != null)
        {
            return _activeDefinition;
        }

        string locationGuid = location?.Template?.GUID ?? string.Empty;
        string npcGuid = npcElement?.Template?.GUID ?? string.Empty;
        if (string.Equals(locationGuid, SpiderFamilyCompanionDefinitions.SpiderLocationTemplateGuid, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(npcGuid, SpiderFamilyCompanionDefinitions.SpiderNpcTemplateGuid, StringComparison.OrdinalIgnoreCase))
        {
            return SpiderFamilyCompanionDefinitions.SpiderSkin1;
        }

        return SpiderFamilyCompanionDefinitions.BroodmotherSkin1;
    }

    private bool IsNativeBroodmotherCommandMenuAvailableInstance(
        Location location,
        Hero? hero,
        IInteractableWithHero interactable)
    {
        return IsNativeBroodmotherActionBaseAvailableInstance(location, hero, interactable);
    }

    private bool IsNativeBroodmotherQuickCommandAvailableInstance(
        Location location,
        Hero? hero,
        IInteractableWithHero interactable,
        BroodmotherDialogueCommand command)
    {
        return IsNativeBroodmotherActionBaseAvailableInstance(location, hero, interactable) &&
            IsBroodmotherQuickCommandRunnable(command);
    }

    private bool IsNativeBroodmotherActionBaseAvailableInstance(
        Location location,
        Hero? hero,
        IInteractableWithHero interactable)
    {
        if (!_enabled.Value || !_enableNativeCompanionPrompt.Value || hero == null)
        {
            return false;
        }

        if (location == null || location.HasBeenDiscarded || !ReferenceEquals(interactable, location))
        {
            return false;
        }

        if (!location.Interactable)
        {
            return false;
        }

        return IsActiveBroodmotherCompanionLocation(location);
    }

    private static bool IsBroodmotherQuickCommandRunnable(BroodmotherDialogueCommand command)
    {
        return command == BroodmotherDialogueCommand.Follow ||
            command == BroodmotherDialogueCommand.HoldPosition ||
            command == BroodmotherDialogueCommand.Defend ||
            command == BroodmotherDialogueCommand.ComeClose ||
            command == BroodmotherDialogueCommand.Recall ||
            command == BroodmotherDialogueCommand.Recover ||
            command == BroodmotherDialogueCommand.Dismiss;
    }

    private void OpenNativeBroodmotherCommandMenuInstance(Location location)
    {
        if (!IsNativeBroodmotherCommandMenuAvailableInstance(location, Hero.Current, location))
        {
            Logger.LogWarning($"{PluginName} native Companion prompt blocked: active Broodmother dialogue host unavailable for this actor.");
            return;
        }

        if (!TryGetActiveCompanion(out Location? activeLocation, out NpcElement? npcElement) ||
            !ReferenceEquals(activeLocation, location))
        {
            Logger.LogWarning($"{PluginName} native Companion prompt blocked: active Broodmother changed before opening.");
            return;
        }

        location.MarkedNotSaved = true;
        _panelVisible = false;
        _dialogueHost?.Open(location);
        SetPanelStatus("Companion dialogue opened.");
        Logger.LogInfo($"{PluginName} native Companion prompt opened plugin-owned Unity UI companion dialogue host. template={location.Template?.name}[{location.Template?.GUID}], dependency=none, storyGraph=0, saveWrite=0.");
        LogCommand("native-companion-prompt", "opened", "runtime-only-unity-ui-dialogue-host-no-dependency", location, npcElement);
    }

    private void TryOpenNativeCompanionMenuFromInteractFallback()
    {
        if (!_enableNativeCompanionPrompt.Value ||
            !_enableNativeCompanionInteractFallback.Value ||
            !Input.GetKeyDown(KeyCode.E))
        {
            return;
        }

        if (!TryGetActiveCompanion(out Location? location, out NpcElement? npcElement))
        {
            return;
        }

        if (!location.HasElement<BroodmotherCompanionCommandAction>() || !location.Interactable)
        {
            LogNativeInteractFallbackBlocked(location, npcElement, "native-action-unavailable");
            return;
        }

        Hero? hero = Hero.Current;
        if (hero == null)
        {
            LogNativeInteractFallbackBlocked(location, npcElement, "hero-current-null");
            return;
        }

        bool focused = IsBroodmotherInNativeInteractFocus(
            location,
            hero,
            out float distance,
            out float angle,
            out string focusReason);
        if (!focused)
        {
            LogNativeInteractFallbackBlocked(
                location,
                npcElement,
                "focus-failed;" + focusReason + ";distance=" + FormatDistance(distance, true) + ";angle=" + FormatAngle(angle, true));
            return;
        }

        LogCommand(
            "native-companion-prompt",
            "fallback-opened",
            "native-action-attached-focused-interact-key;distance=" + FormatDistance(distance, true) + ";angle=" + FormatAngle(angle, true),
            location,
            npcElement);
        OpenNativeBroodmotherCommandMenuInstance(location);
    }

    private void LogNativeInteractFallbackBlocked(Location location, NpcElement npcElement, string reason)
    {
        if (Time.unscaledTime < _nextNativeCompanionInteractFallbackLogTime)
        {
            return;
        }

        _nextNativeCompanionInteractFallbackLogTime = Time.unscaledTime + 1.0f;
        LogCommand("native-companion-prompt", "fallback-blocked", reason, location, npcElement);
    }

    private bool IsBroodmotherInNativeInteractFocus(
        Location location,
        Hero hero,
        out float distance,
        out float angle,
        out string reason)
    {
        distance = Vector3.Distance(hero.Coords, location.Coords);
        float maxDistance = Mathf.Clamp(_nativeCompanionInteractFallbackDistance.Value, 2f, 18f);
        if (distance > maxDistance)
        {
            angle = 180f;
            reason = "too-far";
            return false;
        }

        Vector3 origin = hero.Coords;
        Vector3 forward = hero.Rotation * Vector3.forward;
        Camera camera = Camera.main;
        if (camera != null)
        {
            origin = camera.transform.position;
            forward = camera.transform.forward;
        }

        Vector3 toBroodmother = location.Coords - origin;
        if (toBroodmother.sqrMagnitude <= 0.001f || forward.sqrMagnitude <= 0.001f)
        {
            angle = 0f;
            reason = "overlapping";
            return true;
        }

        angle = Vector3.Angle(forward.normalized, toBroodmother.normalized);
        float maxAngle = Mathf.Clamp(_nativeCompanionInteractFallbackAngle.Value, 8f, 90f);
        if (angle > maxAngle)
        {
            reason = "not-looking-at-broodmother";
            return false;
        }

        reason = "focused";
        return true;
    }

    private void RunNativeBroodmotherQuickCommandInstance(Location location, BroodmotherDialogueCommand command)
    {
        if (!IsNativeBroodmotherQuickCommandAvailableInstance(location, Hero.Current, location, command))
        {
            Logger.LogWarning($"{PluginName} native Broodmother quick command blocked: command={command}; active actor unavailable.");
            LogCommand("native-companion-quick-command", "blocked", command.ToString().ToLowerInvariant(), location);
            return;
        }

        if (!TryGetActiveCompanion(out Location? activeLocation, out NpcElement? npcElement) ||
            !ReferenceEquals(activeLocation, location))
        {
            Logger.LogWarning($"{PluginName} native Broodmother quick command blocked: active Broodmother changed before command={command}.");
            LogCommand("native-companion-quick-command", "blocked", "active-broodmother-changed;" + command, location);
            return;
        }

        location.MarkedNotSaved = true;
        LogCommand(
            "native-companion-quick-command",
            command.ToString().ToLowerInvariant(),
            "runtime-only-direct-abstract-location-action",
            location,
            npcElement);
        RunDialogueCommand(command, "native quick command");
    }

    private bool IsCompanionDialogueVisible => _dialogueHost?.Visible == true;

    private bool IsCompanionUiVisibleInstance => _panelVisible || IsCompanionDialogueVisible;

    private void EnsureNativeCompanionPrompt(Location location)
    {
        if (!_enableNativeCompanionPrompt.Value || !IsActiveBroodmotherCompanionLocation(location))
        {
            RemoveNativeCompanionPrompt(location);
            return;
        }

        bool changed = false;
        changed |= EnsureNativeCompanionAction<BroodmotherCompanionCommandAction>(location);
        changed |= RemoveNativeQuickCommandActions(location);
        changed |= EnsureNativePromptInteractability(location, out string interactabilityReason);

        location.MarkedNotSaved = true;
        string surfaceKey = $"{location.Template?.GUID ?? string.Empty}|Dialogue";
        bool firstSeen = !_nativeCommandSurfaceKeys.TryGetValue(location, out string existingSurfaceKey) ||
            !string.Equals(existingSurfaceKey, surfaceKey, StringComparison.Ordinal);
        _nativeCommandSurfaceKeys[location] = surfaceKey;
        if (changed || firstSeen)
        {
            Logger.LogInfo($"{PluginName} native Companion prompt synchronized for {location.Template?.name}[{location.Template?.GUID}]. Runtime-only; not saved. Mode=Dialogue; changed={changed}; interactable={(location.Interactable ? "1" : "0")}; interactability={FormatInteractability(location.Interactability)}; dependency=none; storyGraph=0; saveWrite=0; quickCommands=0.");
            LogCommand("native-companion-prompt", "synchronized", $"runtime-only-direct-abstract-location-action;mode=Dialogue;changed={changed};interactable={(location.Interactable ? "1" : "0")};interactability={FormatInteractability(location.Interactability)};interactabilityReason={interactabilityReason};dependency=none;quickCommands=0", location);
        }
    }

    private bool EnsureNativePromptInteractability(Location location, out string reason)
    {
        reason = "already-active";
        if (location == null || location.HasBeenDiscarded)
        {
            reason = "location-unavailable";
            return false;
        }

        try
        {
            LocationInteractability current = location.Interactability;
            if (!_nativePromptOriginalInteractability.ContainsKey(location))
            {
                _nativePromptOriginalInteractability[location] = current;
            }

            if (current == LocationInteractability.Active && location.Interactable)
            {
                return false;
            }

            location.MarkedNotSaved = true;
            location.SetInteractability(LocationInteractability.Active);
            location.MarkedNotSaved = true;
            reason = "set-active-from-" + FormatInteractability(current);
            return true;
        }
        catch (Exception ex)
        {
            reason = "failed-" + ex.GetType().Name;
            Logger.LogWarning($"{PluginName} native Companion prompt interactability activation failed: {ex.GetType().Name}: {ex.Message}");
            return false;
        }
    }

    private static bool EnsureNativeCompanionAction<TAction>(Location location)
        where TAction : AbstractLocationAction, new()
    {
        if (location.HasElement<TAction>())
        {
            return false;
        }

        TAction action = location.AddElement(new TAction());
        action.MarkedNotSaved = true;
        return true;
    }

    private static bool EnsureNativeQuickCommandActions(Location location)
    {
        bool changed = false;
        changed |= EnsureNativeCompanionAction<BroodmotherCompanionFollowAction>(location);
        changed |= EnsureNativeCompanionAction<BroodmotherCompanionHoldPositionAction>(location);
        changed |= EnsureNativeCompanionAction<BroodmotherCompanionDefendAction>(location);
        changed |= EnsureNativeCompanionAction<BroodmotherCompanionComeCloseAction>(location);
        changed |= EnsureNativeCompanionAction<BroodmotherCompanionRecallAction>(location);
        changed |= EnsureNativeCompanionAction<BroodmotherCompanionRecoverAction>(location);
        changed |= EnsureNativeCompanionAction<BroodmotherCompanionDismissAction>(location);
        return changed;
    }

    private static bool RemoveNativeQuickCommandActions(Location location)
    {
        bool hadQuickCommands =
            location.HasElement<BroodmotherCompanionFollowAction>() ||
            location.HasElement<BroodmotherCompanionHoldPositionAction>() ||
            location.HasElement<BroodmotherCompanionDefendAction>() ||
            location.HasElement<BroodmotherCompanionComeCloseAction>() ||
            location.HasElement<BroodmotherCompanionRecallAction>() ||
            location.HasElement<BroodmotherCompanionRecoverAction>() ||
            location.HasElement<BroodmotherCompanionDismissAction>();
        if (!hadQuickCommands)
        {
            return false;
        }

        location.RemoveElementsOfType<BroodmotherCompanionFollowAction>();
        location.RemoveElementsOfType<BroodmotherCompanionHoldPositionAction>();
        location.RemoveElementsOfType<BroodmotherCompanionDefendAction>();
        location.RemoveElementsOfType<BroodmotherCompanionComeCloseAction>();
        location.RemoveElementsOfType<BroodmotherCompanionRecallAction>();
        location.RemoveElementsOfType<BroodmotherCompanionRecoverAction>();
        location.RemoveElementsOfType<BroodmotherCompanionDismissAction>();
        return true;
    }

    private void RemoveNativeCompanionPrompt(Location? location)
    {
        if (location == null)
        {
            return;
        }

        if (location.HasBeenDiscarded)
        {
            _nativeCommandSurfaceKeys.Remove(location);
            _nativePromptOriginalInteractability.Remove(location);
            return;
        }

        if (!HasNativeCompanionPrompt(location))
        {
            _nativeCommandSurfaceKeys.Remove(location);
            RestoreNativePromptInteractability(location);
            return;
        }

        RemoveNativeCompanionPromptActions(location);
        location.MarkedNotSaved = true;
        _nativeCommandSurfaceKeys.Remove(location);
        string restoreReason = RestoreNativePromptInteractability(location);
        LogCommand("native-companion-prompt", "removed", "runtime-only-action-cleanup;" + restoreReason, location);
    }

    private void RemoveAllNativeCompanionPrompts()
    {
        if (_activeBroodmother != null)
        {
            RemoveNativeCompanionPrompt(_activeBroodmother);
        }

        foreach (Location location in new List<Location>(_nativeCommandSurfaceKeys.Keys))
        {
            RemoveNativeCompanionPrompt(location);
        }

        _nativeCommandSurfaceKeys.Clear();
        _nativePromptOriginalInteractability.Clear();
    }

    private string RestoreNativePromptInteractability(Location location)
    {
        if (!_nativePromptOriginalInteractability.TryGetValue(location, out LocationInteractability original))
        {
            return "interactability-restore=not-tracked";
        }

        _nativePromptOriginalInteractability.Remove(location);
        if (location == null || location.HasBeenDiscarded)
        {
            return "interactability-restore=discarded";
        }

        try
        {
            LocationInteractability current = location.Interactability;
            if (current == original)
            {
                return "interactability-restore=unchanged";
            }

            location.MarkedNotSaved = true;
            location.SetInteractability(original);
            location.MarkedNotSaved = true;
            return "interactability-restore=" + FormatInteractability(original);
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"{PluginName} native Companion prompt interactability restore failed: {ex.GetType().Name}: {ex.Message}");
            return "interactability-restore=failed-" + ex.GetType().Name;
        }
    }

    private static bool HasNativeCompanionPrompt(Location? location)
    {
        return location != null &&
            !location.HasBeenDiscarded &&
            (location.HasElement<BroodmotherCompanionCommandAction>() ||
             location.HasElement<BroodmotherCompanionFollowAction>() ||
             location.HasElement<BroodmotherCompanionHoldPositionAction>() ||
             location.HasElement<BroodmotherCompanionDefendAction>() ||
             location.HasElement<BroodmotherCompanionComeCloseAction>() ||
             location.HasElement<BroodmotherCompanionRecallAction>() ||
             location.HasElement<BroodmotherCompanionRecoverAction>() ||
             location.HasElement<BroodmotherCompanionDismissAction>());
    }

    private static void RemoveNativeCompanionPromptActions(Location location)
    {
        location.RemoveElementsOfType<BroodmotherCompanionCommandAction>();
        location.RemoveElementsOfType<BroodmotherCompanionFollowAction>();
        location.RemoveElementsOfType<BroodmotherCompanionHoldPositionAction>();
        location.RemoveElementsOfType<BroodmotherCompanionDefendAction>();
        location.RemoveElementsOfType<BroodmotherCompanionComeCloseAction>();
        location.RemoveElementsOfType<BroodmotherCompanionRecallAction>();
        location.RemoveElementsOfType<BroodmotherCompanionRecoverAction>();
        location.RemoveElementsOfType<BroodmotherCompanionDismissAction>();
    }

    private bool IsActiveBroodmotherCompanionLocation(Location location)
    {
        if (location == null || location.HasBeenDiscarded || !ReferenceEquals(_activeBroodmother, location))
        {
            return false;
        }

        SpiderFamilyCompanionDefinition definition = ActiveDefinition;
        if (!string.Equals(location.Template?.name, definition.LocationTemplateName, StringComparison.Ordinal) ||
            !string.Equals(location.Template?.GUID, definition.LocationTemplateGuid, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!location.TryGetElement(out NpcElement npcElement) || npcElement == null || npcElement.HasBeenDiscarded)
        {
            return false;
        }

        if (!string.Equals(npcElement.Template?.GUID, definition.NpcTemplateGuid, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        NpcHeroPetAlly marker = npcElement.TryGetElement<NpcHeroPetAlly>();
        return marker != null && !marker.HasBeenDiscarded;
    }

    private void RecallCompanion(string source)
    {
        if (!TryGetActiveCompanion(out Location? location, out NpcElement? npcElement))
        {
            SetPanelStatus("Recall ignored: no active companion.");
            LogCommand("recall", "ignored", "no-active-companion");
            return;
        }

        string displayName = ActiveCompanionDisplayName;
        Hero? hero = Hero.Current;
        if (hero == null)
        {
            SetPanelStatus("Recall blocked: Hero.Current is null.");
            LogCommand("recall", "blocked", "hero-current-null", location, npcElement);
            return;
        }

        Vector3 target = hero.Coords + hero.Rotation * GetCompanionOffset();
        try
        {
            location.MarkedNotSaved = true;
            location.MoveAndRotateTo(target, hero.Rotation, teleport: true);
            location.MarkedNotSaved = true;
            SetPanelStatus(displayName + " recalled.");
            Logger.LogInfo($"{PluginName} recalled {displayName} during {source} to {FormatVector(target)}.");
            LogCommand("recall", "recalled", source, location, npcElement);
            TryPlayEmbeddedSpiderAudio(BroodmotherEmbeddedAudioCue.Call, location, "recall:" + source);
        }
        catch (Exception ex)
        {
            SetPanelStatus("Recall failed.");
            Logger.LogWarning($"{PluginName} recall failed during {source}: {ex.GetType().Name}: {ex.Message}");
            LogCommand("recall", "failed", ex.GetType().Name, location, npcElement);
        }
    }

    private void ToggleDefendMode(string source)
    {
        if (!TryGetActiveCompanion(out Location? location, out NpcElement? npcElement))
        {
            SetPanelStatus("Defend toggle ignored: no active companion.");
            LogCommand("defend-toggle", "ignored", "no-active-companion");
            return;
        }

        _defendMode = !_defendMode;
        _holdPositionMode = false;
        location.MarkedNotSaved = true;
        SetPanelStatus("Mode set to " + (_defendMode ? "Defend." : "Follow."));
        Logger.LogInfo($"{PluginName} mode={(_defendMode ? "Defend" : "Follow")} during {source}.");
        LogCommand("defend-toggle", _defendMode ? "defend" : "follow", source, location, npcElement);

        Hero? hero = Hero.Current;
        if (_defendMode && _enableNativeDefendAssist.Value && hero != null)
        {
            int attackers = CountLiveHeroAttackers(hero);
            if (attackers > 0)
            {
                TriggerDefendAssist(location, npcElement, attackers, "defend toggle");
            }
        }
    }

    private void TriggerDefendAssist(Location location, NpcElement npcElement, int attackerCount, string source)
    {
        try
        {
            NpcHeroPetAlly marker = npcElement.TryGetElement<NpcHeroPetAlly>();
            if (marker == null || marker.HasBeenDiscarded)
            {
                return;
            }

            location.MarkedNotSaved = true;
            marker.EnterCombat();
            if (Time.unscaledTime >= _nextDefendLogTime)
            {
                Logger.LogInfo($"{PluginName} prompted native Broodmother defend during {source}. attackers={attackerCount}");
                _nextDefendLogTime = Time.unscaledTime + 4f;
            }

            LogCommand("defend-assist", "prompted", source, location, npcElement);
            TryPlayEmbeddedSpiderAudio(BroodmotherEmbeddedAudioCue.Attack, location, "defend-assist:" + source);
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"{PluginName} defend prompt failed during {source}: {ex.GetType().Name}: {ex.Message}");
            LogCommand("defend-assist", "failed", ex.GetType().Name, location, npcElement);
        }
    }

    private bool TryApplyImmediateTargetPressure(Location location, NpcElement npcElement, Hero hero)
    {
        if (Time.unscaledTime < _nextImmediateTargetPressureTime)
        {
            return true;
        }

        if (!TrySelectPrimaryCompanionCombatTarget(
                hero,
                npcElement,
                location.Coords,
                npcElement.Rotation,
                out ICharacter? target,
                out int combatCandidateCount,
                out float distanceMeters,
                out float angleDegrees,
                out bool hasSpatialTarget,
                out string targetSource))
        {
            return false;
        }

        _nextImmediateTargetPressureTime =
            Time.unscaledTime + Math.Max(0.1f, _immediateTargetPressureSeconds.Value);

        bool targetedCombat = false;
        string targetedCombatReason = string.Empty;
        try
        {
            if (npcElement.NpcAI == null)
            {
                targetedCombatReason = "npc-ai-null";
            }
            else
            {
                location.MarkedNotSaved = true;
                _ = TryRegisterNativeCombatTarget(npcElement, target, out string targetRegistrationReason);
                npcElement.NpcAI.EnterCombatWith(target, forceChange: true);
                targetedCombat = true;
                targetedCombatReason = targetRegistrationReason + ReadNativeCombatHandoffState(npcElement, target);
            }
        }
        catch (Exception ex)
        {
            targetedCombatReason = ex.GetType().Name + ": " + ex.Message;
        }

        bool petPrompted = false;
        string petPromptReason = string.Empty;
        try
        {
            NpcHeroPetAlly marker = npcElement.TryGetElement<NpcHeroPetAlly>();
            if (marker == null || marker.HasBeenDiscarded)
            {
                petPromptReason = "native-pet-marker-missing";
            }
            else
            {
                location.MarkedNotSaved = true;
                marker.EnterCombat();
                petPrompted = true;
            }
        }
        catch (Exception ex)
        {
            petPromptReason = ex.GetType().Name + ": " + ex.Message;
        }

        string targetName = string.IsNullOrWhiteSpace(target.Name) ? target.GetType().Name : target.Name.Trim();
        SetPanelStatus("Broodmother targeting: " + targetName + ".");
        string reason =
            "live-native-combat-pressure" +
            ";target=" + targetName +
            ";targetSource=" + targetSource +
            ";candidates=" + combatCandidateCount +
            ";distance=" + FormatDistance(distanceMeters, hasSpatialTarget) +
            ";angle=" + FormatAngle(angleDegrees, hasSpatialTarget) +
            ";nativeTargeted=" + (targetedCombat ? "1" : "0") +
            ";petPrompted=" + (petPrompted ? "1" : "0") +
            ";targetReason=" + targetedCombatReason +
            ";petReason=" + petPromptReason;

        if (Time.unscaledTime >= _nextImmediateTargetPressureLogTime)
        {
            LogCommand(
                "immediate-target-pressure",
                targetedCombat || petPrompted ? "prompted" : "failed",
                reason,
                location,
                npcElement);
            _nextImmediateTargetPressureLogTime = Time.unscaledTime + 1f;
        }

        if (targetedCombat || petPrompted)
        {
            TryPlayEmbeddedSpiderAudio(BroodmotherEmbeddedAudioCue.Attack, location, "immediate-target-pressure");
        }

        return targetedCombat || petPrompted;
    }

    private bool AdvanceAdvancedSpiderCompanionAi(Location location, NpcElement npcElement, Hero hero)
    {
        if (!TrySelectPrimaryCompanionCombatTarget(
            hero,
            npcElement,
            location.Coords,
            npcElement.Rotation,
            out ICharacter? target,
            out int combatCandidateCount,
            out float distanceMeters,
            out float angleDegrees,
            out bool hasSpatialTarget,
            out string targetSource))
        {
            StopActiveSpiderProcedure("no-live-combat-target");
            return false;
        }

        SpiderFamilyCompanionDefinition definition = ActiveDefinition;
        string targetName = string.IsNullOrWhiteSpace(target.Name) ? target.GetType().Name : target.Name.Trim();
        if (!hasSpatialTarget || !TryGetCharacterCoords(target, out Vector3 targetCoords))
        {
            StopActiveSpiderProcedure("target-spatial-unavailable");
            LogCommand(
                "advanced-spider-ai-v2",
                "fail-closed",
                "target-spatial-unavailable;package=" + BroodmotherCompanionAiV2Contract.PackageIdValue +
                ";target=" + FormatMarkerValue(targetName) +
                ";targetSource=" + targetSource,
                location,
                npcElement);
            return true;
        }

        ActorRoleId roleId = ResolveBroodmotherSpiderV2Role(definition);
        int roleCode = ResolveBroodmotherSpiderV2RoleCode(roleId);
        if (roleCode == 0)
        {
            StopActiveSpiderProcedure("unsupported-v2-role");
            LogCommand(
                "advanced-spider-ai-v2",
                "fail-closed",
                "unsupported-v2-role;role=" + FormatMarkerValue(definition.AiActorRoleId) +
                ";package=" + BroodmotherCompanionAiV2Contract.PackageIdValue,
                location,
                npcElement);
            return true;
        }

        string requestedTargetId = ResolveSpiderRuntimeTargetId(target);
        if (_spiderProcedureRunner != null &&
            !string.Equals(_spiderBridgeTargetId, requestedTargetId, StringComparison.Ordinal))
        {
            StopActiveSpiderProcedure("target-changed");
            StopSpiderRuntimeHost("target-changed");
        }

        if (!TryEnsureSpiderRuntimeHost(
                location,
                npcElement,
                target,
                out string runtimeHostReason))
        {
            LogCommand(
                "advanced-spider-ai-v2",
                "bridge-unavailable",
                runtimeHostReason +
                ";package=" + BroodmotherCompanionAiV2Contract.PackageIdValue +
                ";role=" + roleId.Value +
                ";target=" + FormatMarkerValue(targetName),
                location,
                npcElement);
            return true;
        }

        if (_spiderProcedureRunner != null)
        {
            ActionPollResult poll = _spiderProcedureRunner.Tick();
            if (!poll.Status.IsTerminal())
            {
                return true;
            }

            string finalStage = _spiderProcedureRunner.FinalStage;
            string runnerDiagnostics = FormatSpiderRunnerDiagnostics(_spiderProcedureRunner, string.Empty);
            string pollReason =
                "reason=" + FormatMarkerValue(poll.Reason) +
                ";finalStage=" + FormatMarkerValue(finalStage) +
                runnerDiagnostics +
                ";package=" + BroodmotherCompanionAiV2Contract.PackageIdValue +
                ";role=" + roleId.Value +
                ";target=" + FormatMarkerValue(targetName) +
                ";distance=" + FormatDistance(distanceMeters, hasSpatialTarget) +
                ";angle=" + FormatAngle(angleDegrees, hasSpatialTarget) +
                ";targetSource=" + targetSource +
                ";candidates=" + combatCandidateCount +
                ";bridge=ForSpawnedUnityActor;nativeTargeted=0;petPrompted=0;staticEvaluator=0";
            LogCommand("advanced-spider-ai-v2-poll", poll.Status.ToString(), pollReason, location, npcElement);

            if (Time.unscaledTime >= _nextAdvancedSpiderAiLogTime)
            {
                Logger.LogInfo(
                    "BROODMOTHER_COMPANION_ADVANCED_SPIDER_AI_V2_POLL " +
                    $"package={BroodmotherCompanionAiV2Contract.PackageIdValue}, role={roleId.Value}, target={FormatMarkerValue(targetName)}, " +
                    $"status={poll.Status}, reason={FormatMarkerValue(poll.Reason)}, finalStage={FormatMarkerValue(finalStage)}, " +
                    $"lastCommandStage={FormatMarkerValue(_spiderProcedureRunner.LastCommandStage)}, lastCommandAction={FormatMarkerValue(_spiderProcedureRunner.LastCommandAction)}, " +
                    $"lastCommandPhase={FormatMarkerValue(_spiderProcedureRunner.LastCommandPhase)}, lastCommandOutcome={FormatMarkerValue(_spiderProcedureRunner.LastCommandOutcome)}, " +
                    $"lastCommandReason={FormatMarkerValue(_spiderProcedureRunner.LastCommandReason)}, lastCommandPolls={_spiderProcedureRunner.LastCommandPollCount.ToString(CultureInfo.InvariantCulture)}, " +
                    $"lastTickCompletedStages={_spiderProcedureRunner.LastTickCompletedStages.ToString(CultureInfo.InvariantCulture)}, bridge=ForSpawnedUnityActor, nativeTargeted=0, petPrompted=0, staticEvaluator=0, " +
                    $"distance={FormatDistance(distanceMeters, hasSpatialTarget)}, angle={FormatAngle(angleDegrees, hasSpatialTarget)}, " +
                    $"targetSource={targetSource}, candidates={combatCandidateCount}, dependency=FoAHost.PlayMaker, saveWrite=0, randomSpawn=0.");
                _nextAdvancedSpiderAiLogTime = Time.unscaledTime + 3f;
            }

            _spiderProcedureRunner = null;
            _spiderProcedureTarget = null;
            _spiderDamageWindowOpen = false;
            _advancedSpiderTelegraphReady = false;
            UpdateSpiderV2PostProcedureState(poll.Status, requestedTargetId);
            if (poll.Status == ActionStatus.Succeeded)
            {
                _advancedSpiderWaveSeed = unchecked(_advancedSpiderWaveSeed + 1);
            }

            return true;
        }

        if (Time.unscaledTime < _nextAdvancedSpiderCombatPromptTime)
        {
            return true;
        }

        int distanceBand = ResolveBroodmotherSpiderV2DistanceBand(distanceMeters, hasSpatialTarget);
        int angleBand = ResolveBroodmotherSpiderV2AngleBand(angleDegrees, hasSpatialTarget);
        bool rangedLaneClear = hasSpatialTarget;
        bool forceLeapReposition = ShouldForceSpiderV2LeapReposition(roleId, requestedTargetId);
        bool leapEnabledForSelection = _enableAdvancedSpiderLeapRequests.Value && !forceLeapReposition;
        if (!TrySelectBroodmotherSpiderV2Action(
                roleId,
                distanceBand,
                angleBand,
                rangedLaneClear,
                leapEnabledForSelection,
                out AvalonActionDefinition? actionDefinition,
                out AvalonProcedureReference? procedureReference,
                out BroodmotherSpiderRuntimeAttackKind attackKind,
                out string actionReason))
        {
            LogCommand(
                "advanced-spider-ai-v2",
                "no-action",
                actionReason +
                ";package=" + BroodmotherCompanionAiV2Contract.PackageIdValue +
                ";role=" + roleId.Value +
                ";target=" + FormatMarkerValue(targetName),
                location,
                npcElement);
            return true;
        }

        if (forceLeapReposition)
        {
            actionReason += ";leapRepositionRequired=1";
        }

        AvalonPosition actorPosition = ToAvalonPosition(npcElement.Coords);
        AvalonPosition targetPosition = ToAvalonPosition(targetCoords);
        TargetDescriptor targetDescriptor = TargetDescriptor.ForActor(new TargetId(_spiderBridgeTargetId));
        PlanningTarget[] targets =
        {
            new PlanningTarget(
                BroodmotherCompanionAiV2Contract.CurrentTarget,
                targetDescriptor,
                targetPosition),
        };
        PlanningFact[] facts = BuildBroodmotherSpiderV2Facts(
            roleCode,
            distanceBand,
            angleBand,
            attackKind,
            rangedLaneClear,
            atRoleBand: IsAtBroodmotherSpiderV2RoleBand(roleId, distanceBand),
            atFlank: angleBand != BroodmotherCompanionAiV2Contract.AngleBandFront);

        if (!TryStartBroodmotherSpiderProcedure(
                actionDefinition,
                procedureReference,
                roleId,
                actorPosition,
                targetDescriptor,
                facts,
                targets,
                target,
                out string startReason))
        {
            LogCommand(
                "advanced-spider-ai-v2",
                "start-failed",
                startReason +
                ";package=" + BroodmotherCompanionAiV2Contract.PackageIdValue +
                ";role=" + roleId.Value +
                ";action=" + actionDefinition.Id.Value +
                ";procedure=" + procedureReference.Id.Value,
                location,
                npcElement);
            return true;
        }

        _spiderActiveAttackKind = attackKind;
        _spiderActiveActionIdValue = actionDefinition.Id.Value;
        _nextAdvancedSpiderCombatPromptTime =
            Time.unscaledTime + Math.Max(0.2f, _advancedSpiderCombatPromptSeconds.Value);
        _advancedSpiderTelegraphReady = attackKind != BroodmotherSpiderRuntimeAttackKind.None;
        AvalonPlayMakerSpiderRunner initialRunner = _spiderProcedureRunner!;
        ActionPollResult initialPoll = initialRunner.Tick();
        string initialRunnerDiagnostics = FormatSpiderRunnerDiagnostics(initialRunner, "initial");
        string commandReason =
            actionReason +
            ";package=" + BroodmotherCompanionAiV2Contract.PackageIdValue +
            ";role=" + roleId.Value +
            ";action=" + actionDefinition.Id.Value +
            ";procedure=" + procedureReference.Id.Value +
            ";attack=" + attackKind +
            ";initialStatus=" + initialPoll.Status +
            ";initialReason=" + FormatMarkerValue(initialPoll.Reason) +
            ";initialFinalStage=" + FormatMarkerValue(initialRunner.FinalStage) +
            initialRunnerDiagnostics +
            ";target=" + FormatMarkerValue(targetName) +
            ";targetSource=" + targetSource +
            ";candidates=" + combatCandidateCount +
            ";distance=" + FormatDistance(distanceMeters, hasSpatialTarget) +
            ";distanceBand=" + distanceBand +
            ";angle=" + FormatAngle(angleDegrees, hasSpatialTarget) +
            ";angleBand=" + angleBand +
            ";bridge=ForSpawnedUnityActor" +
            ";nativeTargeted=0;petPrompted=0;staticEvaluator=0";

        SetPanelStatus(definition.DisplayName + " executing: " + GetRuntimeAttackDisplayName(attackKind) + ".");
        LogCommand("advanced-spider-ai-v2", procedureReference.Id.Value, commandReason, location, npcElement);
        if (attackKind != BroodmotherSpiderRuntimeAttackKind.None)
        {
            TryPlayEmbeddedSpiderAudio(BroodmotherEmbeddedAudioCue.Attack, location, "advanced-spider-ai-v2:" + attackKind);
        }

        if (initialPoll.Status.IsTerminal())
        {
            _spiderProcedureRunner = null;
            _spiderProcedureTarget = null;
            _spiderDamageWindowOpen = false;
            UpdateSpiderV2PostProcedureState(initialPoll.Status, requestedTargetId);
        }

        if (Time.unscaledTime >= _nextAdvancedSpiderAiLogTime)
        {
            Logger.LogInfo(
                "BROODMOTHER_COMPANION_ADVANCED_SPIDER_AI_V2 " +
                $"package={BroodmotherCompanionAiV2Contract.PackageIdValue}, role={roleId.Value}, target={FormatMarkerValue(targetName)}, " +
                $"action={actionDefinition.Id.Value}, procedure={procedureReference.Id.Value}, attack={attackKind}, initialStatus={initialPoll.Status}, initialReason={FormatMarkerValue(initialPoll.Reason)}, " +
                $"initialFinalStage={FormatMarkerValue(initialRunner.FinalStage)}, initialLastCommandStage={FormatMarkerValue(initialRunner.LastCommandStage)}, " +
                $"initialLastCommandAction={FormatMarkerValue(initialRunner.LastCommandAction)}, initialLastCommandPhase={FormatMarkerValue(initialRunner.LastCommandPhase)}, " +
                $"initialLastCommandOutcome={FormatMarkerValue(initialRunner.LastCommandOutcome)}, initialLastCommandReason={FormatMarkerValue(initialRunner.LastCommandReason)}, " +
                $"initialLastCommandPolls={initialRunner.LastCommandPollCount.ToString(CultureInfo.InvariantCulture)}, initialLastTickCompletedStages={initialRunner.LastTickCompletedStages.ToString(CultureInfo.InvariantCulture)}, " +
                $"distance={FormatDistance(distanceMeters, hasSpatialTarget)}, angle={FormatAngle(angleDegrees, hasSpatialTarget)}, " +
                $"targetSource={targetSource}, candidates={combatCandidateCount}, bridge=ForSpawnedUnityActor, nativeTargeted=0, petPrompted=0, " +
                "staticEvaluator=0, dependency=FoAHost.PlayMaker, saveWrite=0, randomSpawn=0.");
            _nextAdvancedSpiderAiLogTime = Time.unscaledTime + 3f;
        }

        return true;
    }

    private bool TryStartBroodmotherSpiderProcedure(
        AvalonActionDefinition actionDefinition,
        AvalonProcedureReference procedureReference,
        ActorRoleId roleId,
        AvalonPosition actorPosition,
        TargetDescriptor targetDescriptor,
        IReadOnlyList<PlanningFact> facts,
        IReadOnlyList<PlanningTarget> targets,
        ICharacter target,
        out string reason)
    {
        reason = "not-started";
        if (_spiderBlazeBridge == null || !_spiderActorLease.HasValue)
        {
            reason = "spider-runtime-host-not-ready";
            return false;
        }

        AvalonPlayMakerSpiderProcedure? procedure = AvalonPlayMakerSpiderProcedures.Find(procedureReference.Id);
        if (procedure == null)
        {
            reason = "playmaker-spider-procedure-missing:" + procedureReference.Id.Value;
            return false;
        }

        DateTimeOffset now = DateTimeOffset.UtcNow;
        DateTimeOffset deadline = now.Add(procedureReference.Timeout);
        int observationRevision = NextPositiveRuntimeCounter(ref _spiderObservationRevision);
        int reservationGeneration = NextPositiveRuntimeCounter(ref _spiderReservationGeneration);
        string executionSuffix =
            observationRevision.ToString(CultureInfo.InvariantCulture) +
            "." +
            Time.frameCount.ToString(CultureInfo.InvariantCulture);
        ActionExecutionId executionId = new(PluginGuid + ".spider.execution." + executionSuffix);
        ActionInvocationId invocationId = new(PluginGuid + ".spider.invocation." + executionSuffix);
        ActorLease actorLease = _spiderActorLease.Value;
        AvalonActionExecutionLease executionLease = new(
            executionId,
            actorLease,
            AvalonPlayMakerSpiderProcedures.PackageId,
            actionDefinition.Id,
            invocationId,
            actionDefinition,
            targetDescriptor,
            observationRevision,
            now,
            deadline,
            procedure.Capabilities);
        AvalonProcedureInputSnapshot input = new(
            executionId,
            invocationId,
            actorLease,
            AvalonPlayMakerSpiderProcedures.PackageId,
            actionDefinition.Id,
            procedureReference,
            observationRevision,
            actorPosition,
            targetDescriptor,
            facts,
            targets,
            new[]
            {
                new AvalonProcedureParameter("role.id", AvalonProcedureValue.ForStableId(roleId.Value)),
                new AvalonProcedureParameter("reservation.generation", AvalonProcedureValue.ForInteger(reservationGeneration)),
            },
            now,
            deadline);

        AvalonPlayMakerSpiderRunner runner = new(_spiderBlazeBridge, SystemAvalonRuntimeClock.Instance);
        if (!runner.Start(executionLease, input, procedure))
        {
            reason = "spider-runner-start-rejected";
            return false;
        }

        _spiderProcedureRunner = runner;
        _spiderProcedureTarget = target;
        _spiderDamageWindowOpen = false;
        reason = "started;procedure=" + procedure.Name + ";reservation=" + reservationGeneration.ToString(CultureInfo.InvariantCulture);
        return true;
    }

    private bool TryEnsureSpiderRuntimeHost(
        Location location,
        NpcElement npcElement,
        ICharacter target,
        out string reason)
    {
        reason = "not-created";
        if (!TryResolveSpiderRuntimeRoots(
                npcElement,
                target,
                out GameObject? actorRoot,
                out Transform? motionRoot,
                out Transform? targetRoot,
                out Animator? animator,
                out reason))
        {
            return false;
        }

        string actorId = ResolveSpiderRuntimeActorId(location);
        string targetId = ResolveSpiderRuntimeTargetId(target);
        if (_spiderBlazeBridge != null &&
            _spiderActorLease.HasValue &&
            string.Equals(_spiderBridgeActorId, actorId, StringComparison.Ordinal) &&
            string.Equals(_spiderBridgeTargetId, targetId, StringComparison.Ordinal))
        {
            reason = "spider-runtime-host-current";
            return true;
        }

        StopSpiderRuntimeHost("runtime-host-rebind");
        int leaseGeneration = NextPositiveRuntimeCounter(ref _spiderLeaseGeneration);
        ActorLease actorLease = new(
            new ActorId(actorId),
            new OwnershipLeaseId(actorId + ".lease." + leaseGeneration.ToString(CultureInfo.InvariantCulture)),
            ActorExecutionMode.BlazeOwned,
            leaseGeneration);
        AvalonFoABroodmotherSpiderRuntimeHostRequest request = new()
        {
            ActorId = actorId,
            TargetId = targetId,
            ActorRoot = actorRoot,
            MotionRoot = motionRoot,
            TargetRoot = targetRoot,
            Animator = animator,
            AnimationStateByAssetHandle = CreateSpiderAnimationStateMap(),
            VfxPrefabByAssetHandle = EnsureSpiderRuntimeVfxPrefabMap(),
            CombatReadyAssetHandles = AvalonFoABroodmotherSpiderRuntimeHostComposition.RequiredCombatAssetHandles,
            OpenDamageWindow = OpenBroodmotherSpiderDamageWindow,
            CommitDamageWindow = CommitBroodmotherSpiderDamageWindow,
            CloseDamageWindow = CloseBroodmotherSpiderDamageWindow,
            LaunchSpitProjectile = LaunchBroodmotherSpiderSpitProjectile,
            ReleaseReservation = ReleaseBroodmotherSpiderReservation,
            Cleanup = CleanupBroodmotherSpiderProcedure,
            EnableBridge = true,
            PackageEnabled = _enableAdvancedSpiderCompanionAi.Value,
            CapabilityGranted = true,
        };

        AvalonFoABroodmotherSpiderRuntimeHostComposition composition = new();
        if (!composition.TryCreate(request, out AvalonFoASpiderBlazeCommandBridge? bridge, out reason) || bridge == null)
        {
            _spiderActorLease = null;
            _spiderBridgeActorId = string.Empty;
            _spiderBridgeTargetId = string.Empty;
            return false;
        }

        _spiderBlazeBridge = bridge;
        _spiderActorLease = actorLease;
        _spiderBridgeActorId = actorId;
        _spiderBridgeTargetId = targetId;
        reason = "spider-runtime-host-created;actor=" + actorId + ";target=" + targetId + ";leaseGeneration=" + leaseGeneration.ToString(CultureInfo.InvariantCulture);
        return true;
    }

    private void StopSpiderRuntimeHost(string reason)
    {
        StopActiveSpiderProcedure(reason);
        if (_spiderBlazeBridge != null)
        {
            _spiderBlazeBridge.Dispose();
            _spiderBlazeBridge = null;
        }

        _spiderActorLease = null;
        _spiderBridgeActorId = string.Empty;
        _spiderBridgeTargetId = string.Empty;
        _spiderDamageWindowOpen = false;
        ReleaseSpiderRuntimeVfxPrefabs();
    }

    private void StopActiveSpiderProcedure(string reason)
    {
        if (_spiderProcedureRunner != null)
        {
            _ = _spiderProcedureRunner.Interrupt(reason);
            _spiderProcedureRunner = null;
        }

        _spiderProcedureTarget = null;
        _spiderDamageWindowOpen = false;
        ClearSpiderV2ActiveProcedureState();
    }

    private bool ShouldForceSpiderV2LeapReposition(ActorRoleId roleId, string targetId)
    {
        if (!_spiderLeapRepositionRequired ||
            !string.Equals(_spiderLeapRepositionTargetId, targetId, StringComparison.Ordinal))
        {
            return false;
        }

        return roleId == BroodmotherCompanionAiV2Contract.PalePouncerRole ||
               roleId == BroodmotherCompanionAiV2Contract.PaleAmbusherRole;
    }

    private void UpdateSpiderV2PostProcedureState(ActionStatus status, string targetId)
    {
        if (status == ActionStatus.Succeeded)
        {
            if (_spiderActiveAttackKind == BroodmotherSpiderRuntimeAttackKind.Leap)
            {
                _spiderLeapRepositionRequired = true;
                _spiderLeapRepositionTargetId = targetId;
            }
            else if (IsSpiderV2LeapRepositionAction(_spiderActiveActionIdValue))
            {
                _spiderLeapRepositionRequired = false;
                _spiderLeapRepositionTargetId = string.Empty;
            }
        }

        ClearSpiderV2ActiveProcedureState();
    }

    private static bool IsSpiderV2LeapRepositionAction(string actionIdValue)
    {
        return string.Equals(actionIdValue, BroodmotherCompanionAiV2Contract.ActionPouncerFlank.Value, StringComparison.Ordinal) ||
               string.Equals(actionIdValue, BroodmotherCompanionAiV2Contract.ActionAmbusherWideFlank.Value, StringComparison.Ordinal);
    }

    private void ClearSpiderV2ActiveProcedureState()
    {
        _spiderActiveAttackKind = BroodmotherSpiderRuntimeAttackKind.None;
        _spiderActiveActionIdValue = string.Empty;
    }

    private bool OpenBroodmotherSpiderDamageWindow(AvalonFoASpiderActorControllerCommand command)
    {
        _ = command;
        _spiderDamageWindowOpen = true;
        return true;
    }

    private bool CommitBroodmotherSpiderDamageWindow(AvalonFoASpiderActorControllerCommand command)
    {
        if (!TryGetActiveCompanion(out Location? location, out NpcElement? npcElement) ||
            _spiderProcedureTarget == null)
        {
            return false;
        }

        BroodmotherSpiderRuntimeAttackKind attackKind = ResolveRuntimeAttackKind(command);
        if (attackKind == BroodmotherSpiderRuntimeAttackKind.None)
        {
            return true;
        }

        if (attackKind == BroodmotherSpiderRuntimeAttackKind.Bite && !_spiderDamageWindowOpen)
        {
            return false;
        }

        bool hasSpatialTarget = TryGetCharacterCoords(_spiderProcedureTarget, out Vector3 targetCoords);
        float distanceMeters = hasSpatialTarget
            ? Vector3.Distance(npcElement.Coords, targetCoords)
            : 0f;
        return TryApplyBroodmotherDamageBridge(
            location,
            npcElement,
            _spiderProcedureTarget,
            attackKind,
            distanceMeters,
            hasSpatialTarget,
            out _);
    }

    private bool CloseBroodmotherSpiderDamageWindow(AvalonFoASpiderActorControllerCommand command)
    {
        _ = command;
        _spiderDamageWindowOpen = false;
        return true;
    }

    private bool LaunchBroodmotherSpiderSpitProjectile(AvalonFoASpiderActorControllerCommand command)
    {
        if (!command.Command.TargetPosition.HasValue)
        {
            return false;
        }

        Vector3 from = ToUnityPosition(command.Command.ActorPosition) + Vector3.up * 1.25f;
        Vector3 to = ToUnityPosition(command.Command.TargetPosition.Value) + Vector3.up * 1.05f;
        BroodmotherRangedAttackVfxRuntime.Spawn(from, to, Logger);
        return true;
    }

    private bool ReleaseBroodmotherSpiderReservation(AvalonFoASpiderActorControllerCommand command)
    {
        _ = command;
        _spiderDamageWindowOpen = false;
        return true;
    }

    private void CleanupBroodmotherSpiderProcedure(AvalonProcedureInputSnapshot input)
    {
        _ = input;
        _spiderDamageWindowOpen = false;
    }

    private static ActorRoleId ResolveBroodmotherSpiderV2Role(SpiderFamilyCompanionDefinition definition)
    {
        ActorRoleId configured = new(definition.AiActorRoleId);
        for (int index = 0; index < BroodmotherCompanionAiV2Contract.SupportedActorRoles.Count; index++)
        {
            if (BroodmotherCompanionAiV2Contract.SupportedActorRoles[index] == configured)
            {
                return configured;
            }
        }

        return definition.Tier == SpiderFamilyCompanionTier.Broodmother
            ? BroodmotherCompanionAiV2Contract.CrimsonVanguardRole
            : BroodmotherCompanionAiV2Contract.CrimsonHarrierRole;
    }

    private static int ResolveBroodmotherSpiderV2RoleCode(ActorRoleId roleId)
    {
        if (roleId == BroodmotherCompanionAiV2Contract.CrimsonVanguardRole) return BroodmotherCompanionAiV2Contract.CrimsonVanguardRoleCode;
        if (roleId == BroodmotherCompanionAiV2Contract.PalePouncerRole) return BroodmotherCompanionAiV2Contract.PalePouncerRoleCode;
        if (roleId == BroodmotherCompanionAiV2Contract.GildedSpitterRole) return BroodmotherCompanionAiV2Contract.GildedSpitterRoleCode;
        if (roleId == BroodmotherCompanionAiV2Contract.CrimsonHarrierRole) return BroodmotherCompanionAiV2Contract.CrimsonHarrierRoleCode;
        if (roleId == BroodmotherCompanionAiV2Contract.PaleAmbusherRole) return BroodmotherCompanionAiV2Contract.PaleAmbusherRoleCode;
        if (roleId == BroodmotherCompanionAiV2Contract.GildedFinisherRole) return BroodmotherCompanionAiV2Contract.GildedFinisherRoleCode;
        return 0;
    }

    private bool TrySelectBroodmotherSpiderV2Action(
        ActorRoleId roleId,
        int distanceBand,
        int angleBand,
        bool rangedLaneClear,
        bool leapEnabled,
        [NotNullWhen(true)] out AvalonActionDefinition? actionDefinition,
        [NotNullWhen(true)] out AvalonProcedureReference? procedureReference,
        out BroodmotherSpiderRuntimeAttackKind attackKind,
        out string reason)
    {
        actionDefinition = null;
        procedureReference = null;
        attackKind = BroodmotherSpiderRuntimeAttackKind.None;
        ActionId actionId;
        if (roleId == BroodmotherCompanionAiV2Contract.CrimsonVanguardRole)
        {
            actionId = distanceBand == BroodmotherCompanionAiV2Contract.DistanceBandBite
                ? BroodmotherCompanionAiV2Contract.ActionVanguardBite
                : BroodmotherCompanionAiV2Contract.ActionVanguardClose;
        }
        else if (roleId == BroodmotherCompanionAiV2Contract.PalePouncerRole)
        {
            actionId = leapEnabled && distanceBand == BroodmotherCompanionAiV2Contract.DistanceBandLeap
                ? BroodmotherCompanionAiV2Contract.ActionPouncerLeap
                : BroodmotherCompanionAiV2Contract.ActionPouncerFlank;
        }
        else if (roleId == BroodmotherCompanionAiV2Contract.GildedSpitterRole)
        {
            actionId = rangedLaneClear && distanceBand == BroodmotherCompanionAiV2Contract.DistanceBandSpit
                ? BroodmotherCompanionAiV2Contract.ActionSpitterSpit
                : BroodmotherCompanionAiV2Contract.ActionSpitterReposition;
        }
        else if (roleId == BroodmotherCompanionAiV2Contract.CrimsonHarrierRole)
        {
            actionId = distanceBand == BroodmotherCompanionAiV2Contract.DistanceBandBite &&
                       angleBand != BroodmotherCompanionAiV2Contract.AngleBandUnknown
                ? BroodmotherCompanionAiV2Contract.ActionHarrierBite
                : BroodmotherCompanionAiV2Contract.ActionHarrierReposition;
        }
        else if (roleId == BroodmotherCompanionAiV2Contract.PaleAmbusherRole)
        {
            actionId = leapEnabled && distanceBand == BroodmotherCompanionAiV2Contract.DistanceBandLeap
                ? BroodmotherCompanionAiV2Contract.ActionAmbusherLeap
                : BroodmotherCompanionAiV2Contract.ActionAmbusherWideFlank;
        }
        else if (roleId == BroodmotherCompanionAiV2Contract.GildedFinisherRole)
        {
            actionId = distanceBand == BroodmotherCompanionAiV2Contract.DistanceBandBite
                ? BroodmotherCompanionAiV2Contract.ActionFinisherBite
                : BroodmotherCompanionAiV2Contract.ActionFinisherClaimPosition;
        }
        else
        {
            reason = "unsupported-v2-role:" + roleId.Value;
            return false;
        }

        if (!TryGetBroodmotherSpiderV2ActionDefinition(actionId, out actionDefinition) ||
            actionDefinition.ExecutionProfile.Procedure == null)
        {
            reason = "v2-action-definition-missing:" + actionId.Value;
            return false;
        }

        procedureReference = actionDefinition.ExecutionProfile.Procedure;
        attackKind = ResolveRuntimeAttackKind(procedureReference.Id);
        reason =
            "selected-v2-action;distanceBand=" + distanceBand.ToString(CultureInfo.InvariantCulture) +
            ";angleBand=" + angleBand.ToString(CultureInfo.InvariantCulture) +
            ";rangedLaneClear=" + BoolText(rangedLaneClear) +
            ";leapEnabled=" + BoolText(leapEnabled);
        return true;
    }

    private static bool TryGetBroodmotherSpiderV2ActionDefinition(
        ActionId actionId,
        [NotNullWhen(true)] out AvalonActionDefinition? actionDefinition)
    {
        for (int index = 0; index < BroodmotherCompanionAiV2Contract.ActionDefinitions.Count; index++)
        {
            if (BroodmotherCompanionAiV2Contract.ActionDefinitions[index].Id == actionId)
            {
                actionDefinition = BroodmotherCompanionAiV2Contract.ActionDefinitions[index];
                return true;
            }
        }

        actionDefinition = null;
        return false;
    }

    private static BroodmotherSpiderRuntimeAttackKind ResolveRuntimeAttackKind(AvalonProcedureId procedureId)
    {
        if (procedureId == BroodmotherCompanionAiV2Contract.BiteProcedure.Id) return BroodmotherSpiderRuntimeAttackKind.Bite;
        if (procedureId == BroodmotherCompanionAiV2Contract.LeapProcedure.Id) return BroodmotherSpiderRuntimeAttackKind.Leap;
        if (procedureId == BroodmotherCompanionAiV2Contract.SpitProcedure.Id) return BroodmotherSpiderRuntimeAttackKind.Spit;
        return BroodmotherSpiderRuntimeAttackKind.None;
    }

    private static BroodmotherSpiderRuntimeAttackKind ResolveRuntimeAttackKind(
        AvalonFoASpiderActorControllerCommand command)
    {
        if (string.Equals(command.Asset.AssetHandle, BroodmotherCompanionAiV2Contract.BiteDamageWindowHandle, StringComparison.Ordinal))
        {
            return BroodmotherSpiderRuntimeAttackKind.Bite;
        }

        if (string.Equals(command.Asset.AssetHandle, BroodmotherCompanionAiV2Contract.LeapContactDamageWindowHandle, StringComparison.Ordinal))
        {
            return BroodmotherSpiderRuntimeAttackKind.Leap;
        }

        if (string.Equals(command.Asset.AssetHandle, BroodmotherCompanionAiV2Contract.SpitDamageWindowHandle, StringComparison.Ordinal))
        {
            return BroodmotherSpiderRuntimeAttackKind.Spit;
        }

        return BroodmotherSpiderRuntimeAttackKind.None;
    }

    private static int ResolveBroodmotherSpiderV2DistanceBand(float distanceMeters, bool hasSpatialTarget)
    {
        if (!hasSpatialTarget) return BroodmotherCompanionAiV2Contract.DistanceBandUnknown;
        if (distanceMeters <= BroodmotherDamageBridgeShortRangeMeters) return BroodmotherCompanionAiV2Contract.DistanceBandBite;
        if (distanceMeters <= BroodmotherDamageBridgeLeapRangeMeters) return BroodmotherCompanionAiV2Contract.DistanceBandLeap;
        if (distanceMeters <= BroodmotherDamageBridgeSpitRangeMeters) return BroodmotherCompanionAiV2Contract.DistanceBandSpit;
        return BroodmotherCompanionAiV2Contract.DistanceBandFar;
    }

    private static int ResolveBroodmotherSpiderV2AngleBand(float angleDegrees, bool hasSpatialTarget)
    {
        if (!hasSpatialTarget) return BroodmotherCompanionAiV2Contract.AngleBandUnknown;
        if (angleDegrees <= BroodmotherCompanionAiV1Contract.ShortBiteAngleDegrees) return BroodmotherCompanionAiV2Contract.AngleBandFront;
        if (angleDegrees <= BroodmotherCompanionAiV1Contract.FlankerLeapAngleDegrees) return BroodmotherCompanionAiV2Contract.AngleBandSide;
        return BroodmotherCompanionAiV2Contract.AngleBandRear;
    }

    private static bool IsAtBroodmotherSpiderV2RoleBand(ActorRoleId roleId, int distanceBand)
    {
        if (roleId == BroodmotherCompanionAiV2Contract.CrimsonVanguardRole ||
            roleId == BroodmotherCompanionAiV2Contract.CrimsonHarrierRole ||
            roleId == BroodmotherCompanionAiV2Contract.GildedFinisherRole)
        {
            return distanceBand == BroodmotherCompanionAiV2Contract.DistanceBandBite;
        }

        if (roleId == BroodmotherCompanionAiV2Contract.PalePouncerRole ||
            roleId == BroodmotherCompanionAiV2Contract.PaleAmbusherRole)
        {
            return distanceBand == BroodmotherCompanionAiV2Contract.DistanceBandLeap;
        }

        if (roleId == BroodmotherCompanionAiV2Contract.GildedSpitterRole)
        {
            return distanceBand == BroodmotherCompanionAiV2Contract.DistanceBandSpit;
        }

        return false;
    }

    private static PlanningFact[] BuildBroodmotherSpiderV2Facts(
        int roleCode,
        int distanceBand,
        int angleBand,
        BroodmotherSpiderRuntimeAttackKind attackKind,
        bool rangedLaneClear,
        bool atRoleBand,
        bool atFlank)
    {
        int slotKind = attackKind switch
        {
            BroodmotherSpiderRuntimeAttackKind.Bite => BroodmotherCompanionAiV2Contract.SlotKindBite,
            BroodmotherSpiderRuntimeAttackKind.Leap => BroodmotherCompanionAiV2Contract.SlotKindLeap,
            BroodmotherSpiderRuntimeAttackKind.Spit => BroodmotherCompanionAiV2Contract.SlotKindSpit,
            _ => BroodmotherCompanionAiV2Contract.SlotKindNone,
        };
        return new[]
        {
            new PlanningFact(BroodmotherCompanionAiV2Contract.FactRoleCode, roleCode),
            new PlanningFact(BroodmotherCompanionAiV2Contract.FactMustStop, 0),
            new PlanningFact(BroodmotherCompanionAiV2Contract.FactMustRetreat, 0),
            new PlanningFact(BroodmotherCompanionAiV2Contract.FactTargetValid, 1),
            new PlanningFact(BroodmotherCompanionAiV2Contract.FactTargetWeak, 0),
            new PlanningFact(BroodmotherCompanionAiV2Contract.FactDistanceBand, distanceBand),
            new PlanningFact(BroodmotherCompanionAiV2Contract.FactAngleBand, angleBand),
            new PlanningFact(BroodmotherCompanionAiV2Contract.FactSlotGranted, slotKind == BroodmotherCompanionAiV2Contract.SlotKindNone ? 0 : 1),
            new PlanningFact(BroodmotherCompanionAiV2Contract.FactSlotKind, slotKind),
            new PlanningFact(BroodmotherCompanionAiV2Contract.FactRangedLaneClear, rangedLaneClear ? 1 : 0),
            new PlanningFact(BroodmotherCompanionAiV2Contract.FactRecoveryRequired, 0),
            new PlanningFact(BroodmotherCompanionAiV2Contract.FactAtFlank, atFlank ? 1 : 0),
            new PlanningFact(BroodmotherCompanionAiV2Contract.FactAtRoleBand, atRoleBand ? 1 : 0),
            new PlanningFact(BroodmotherCompanionAiV2Contract.FactActionCompleted, 0),
        };
    }

    private static IReadOnlyDictionary<string, string> CreateSpiderAnimationStateMap()
    {
        return new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [BroodmotherCompanionAiV2Contract.BiteTelegraphClipHandle] = BroodmotherCompanionAiV1Contract.ShortRangeSecondaryClipName,
            [BroodmotherCompanionAiV2Contract.BiteAttackClipHandle] = BroodmotherCompanionAiV1Contract.ShortRangePrimaryClipName,
            [BroodmotherCompanionAiV2Contract.LeapTelegraphClipHandle] = BroodmotherCompanionAiV1Contract.JumpClipName,
            [BroodmotherCompanionAiV2Contract.LeapAttackClipHandle] = BroodmotherCompanionAiV1Contract.JumpV2ClipName,
            [BroodmotherCompanionAiV2Contract.MovementRunClipHandle] = "Spider_Run_CI4",
            [BroodmotherCompanionAiV2Contract.GildedSpitTelegraphClipHandle] = BroodmotherCompanionAiV1Contract.ShortRangeSecondaryClipName,
            [BroodmotherCompanionAiV2Contract.SafeStopHandle] = "Spider_Tail_Idle_CI4",
            [BroodmotherCompanionAiV2Contract.RecoveryClipHandle] = BroodmotherCompanionAiV1Contract.ShortRangeSecondaryClipName,
            [BroodmotherCompanionAiV2Contract.GroundedControlReadyHandle] = "Spider_Tail_Idle_CI4",
        };
    }

    private IReadOnlyDictionary<string, GameObject> EnsureSpiderRuntimeVfxPrefabMap()
    {
        EnsureSpiderRuntimeVfxPrefab(
            BroodmotherCompanionAiV2Contract.GildedSpitVfxHandle,
            "AvalonBroodmotherCompanion.GildedSpitVfxPrefab",
            new Color(1f, 0.68f, 0.12f, 0.92f),
            new Color(0.86f, 0.28f, 1f, 0.74f));
        EnsureSpiderRuntimeVfxPrefab(
            BroodmotherCompanionAiV2Contract.LeapArcVfxHandle,
            "AvalonBroodmotherCompanion.GildedLeapArcVfxPrefab",
            new Color(1f, 0.76f, 0.2f, 0.90f),
            new Color(1f, 0.42f, 0.08f, 0.68f));
        return _spiderRuntimeVfxPrefabs;
    }

    private void EnsureSpiderRuntimeVfxPrefab(
        string assetHandle,
        string prefabName,
        Color startColor,
        Color endColor)
    {
        if (_spiderRuntimeVfxPrefabs.ContainsKey(assetHandle))
        {
            return;
        }

        GameObject root = new(prefabName);
        root.hideFlags = HideFlags.HideAndDontSave;
        Material material = CreateSpiderRuntimeVfxMaterial(startColor);
        LineRenderer line = root.AddComponent<LineRenderer>();
        line.useWorldSpace = false;
        line.positionCount = 3;
        line.numCapVertices = 4;
        line.numCornerVertices = 2;
        line.startWidth = 0.24f;
        line.endWidth = 0.06f;
        line.startColor = startColor;
        line.endColor = endColor;
        line.material = material;
        line.SetPosition(0, new Vector3(-0.42f, 0f, -0.16f));
        line.SetPosition(1, new Vector3(0f, 0.18f, 0f));
        line.SetPosition(2, new Vector3(0.42f, 0f, 0.16f));
        AddSpiderRuntimeVfxPulse(root.transform, Vector3.zero, 0.18f, material);
        root.SetActive(false);
        _spiderRuntimeVfxPrefabs[assetHandle] = root;
    }

    private Material CreateSpiderRuntimeVfxMaterial(Color color)
    {
        Shader shader = Shader.Find("Sprites/Default") ?? Shader.Find("Hidden/Internal-Colored");
        Material material = new(shader);
        material.hideFlags = HideFlags.HideAndDontSave;
        if (material.HasProperty("_Color"))
        {
            material.color = color;
        }

        _spiderRuntimeVfxMaterials.Add(material);
        return material;
    }

    private static void AddSpiderRuntimeVfxPulse(
        Transform parent,
        Vector3 localPosition,
        float size,
        Material material)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = "AvalonBroodmotherCompanion.GildedSpiderPrefabPulse";
        sphere.hideFlags = HideFlags.HideAndDontSave;
        sphere.transform.SetParent(parent, worldPositionStays: false);
        sphere.transform.localPosition = localPosition;
        sphere.transform.localScale = Vector3.one * size;
        Collider? collider = sphere.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }

        Renderer? renderer = sphere.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = material;
        }
    }

    private void ReleaseSpiderRuntimeVfxPrefabs()
    {
        foreach (GameObject prefab in _spiderRuntimeVfxPrefabs.Values)
        {
            if (prefab != null)
            {
                Destroy(prefab);
            }
        }

        _spiderRuntimeVfxPrefabs.Clear();
        for (int index = 0; index < _spiderRuntimeVfxMaterials.Count; index++)
        {
            if (_spiderRuntimeVfxMaterials[index] != null)
            {
                Destroy(_spiderRuntimeVfxMaterials[index]);
            }
        }

        _spiderRuntimeVfxMaterials.Clear();
    }

    private static bool TryResolveSpiderRuntimeRoots(
        NpcElement npcElement,
        ICharacter target,
        [NotNullWhen(true)] out GameObject? actorRoot,
        [NotNullWhen(true)] out Transform? motionRoot,
        [NotNullWhen(true)] out Transform? targetRoot,
        [NotNullWhen(true)] out Animator? animator,
        out string reason)
    {
        actorRoot = null;
        motionRoot = null;
        targetRoot = null;
        animator = null;
        if (npcElement.Controller == null)
        {
            reason = "spider-runtime-npc-controller-missing";
            return false;
        }

        actorRoot = npcElement.Controller.AlivePrefab != null
            ? npcElement.Controller.AlivePrefab
            : npcElement.Controller.gameObject;
        if (actorRoot == null)
        {
            reason = "spider-runtime-actor-root-missing";
            return false;
        }

        motionRoot = npcElement.Controller.transform != null
            ? npcElement.Controller.transform
            : actorRoot.transform;
        animator = actorRoot.GetComponentInChildren<Animator>(true);
        if (animator == null && npcElement.Controller.gameObject != actorRoot)
        {
            animator = npcElement.Controller.gameObject.GetComponentInChildren<Animator>(true);
        }

        if (animator == null)
        {
            reason = "spider-runtime-animator-missing";
            return false;
        }

        if (!TryResolveCharacterTransform(target, out targetRoot))
        {
            reason = "spider-runtime-target-root-missing";
            return false;
        }

        reason = "spider-runtime-roots-ready";
        return true;
    }

    private static bool TryResolveCharacterTransform(
        ICharacter character,
        [NotNullWhen(true)] out Transform? transform)
    {
        transform = null;
        if (character is NpcElement npcElement)
        {
            if (npcElement.ParentTransform != null)
            {
                transform = npcElement.ParentTransform;
                return true;
            }

            if (npcElement.Controller != null)
            {
                transform = npcElement.Controller.transform;
                return transform != null;
            }
        }

        if (character is Hero hero && hero.ParentTransform != null)
        {
            transform = hero.ParentTransform;
            return true;
        }

        PropertyInfo? parentTransform = character.GetType().GetProperty(
            "ParentTransform",
            BindingFlags.Public | BindingFlags.Instance);
        if (parentTransform?.PropertyType == typeof(Transform) &&
            parentTransform.GetValue(character) is Transform reflectedTransform)
        {
            transform = reflectedTransform;
            return true;
        }

        return false;
    }

    private static string ResolveSpiderRuntimeActorId(Location location)
    {
        string locationId = string.IsNullOrWhiteSpace(location.ID)
            ? RuntimeHelpers.GetHashCode(location).ToString(CultureInfo.InvariantCulture)
            : location.ID.Trim();
        return PluginGuid + ".runtime.actor." + SanitizeStableIdSegment(locationId);
    }

    private static string ResolveSpiderRuntimeTargetId(ICharacter target)
    {
        string prefix = target is Hero ? "hero" : target is NpcElement ? "npc" : "character";
        return PluginGuid +
               ".runtime.target." +
               prefix +
               "." +
               RuntimeHelpers.GetHashCode(target).ToString(CultureInfo.InvariantCulture);
    }

    private static string SanitizeStableIdSegment(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "unknown";
        }

        StringBuilder builder = new(value.Length);
        for (int index = 0; index < value.Length; index++)
        {
            char candidate = value[index];
            builder.Append(char.IsLetterOrDigit(candidate) || candidate == '-' || candidate == '_' || candidate == '.'
                ? candidate
                : '_');
        }

        return builder.Length == 0 ? "unknown" : builder.ToString();
    }

    private static AvalonPosition ToAvalonPosition(Vector3 value) =>
        new(value.x, value.y, value.z);

    private static Vector3 ToUnityPosition(AvalonPosition value) =>
        new(value.X, value.Y, value.Z);

    private static int NextPositiveRuntimeCounter(ref int value)
    {
        value = unchecked(value + 1);
        if (value <= 0)
        {
            value = 1;
        }

        return value;
    }

    private static string GetRuntimeAttackDisplayName(BroodmotherSpiderRuntimeAttackKind attack)
    {
        return attack switch
        {
            BroodmotherSpiderRuntimeAttackKind.Bite => "bite",
            BroodmotherSpiderRuntimeAttackKind.Leap => "leap",
            BroodmotherSpiderRuntimeAttackKind.Spit => "gilded spit",
            _ => "positioning",
        };
    }

    private bool TryApplyBroodmotherDamageBridge(
        Location location,
        NpcElement npcElement,
        ICharacter target,
        BroodmotherSpiderRuntimeAttackKind attackKind,
        float distanceMeters,
        bool hasSpatialTarget,
        out string reason)
    {
        reason = "not-applied";
        try
        {
            if (attackKind != BroodmotherSpiderRuntimeAttackKind.Bite &&
                attackKind != BroodmotherSpiderRuntimeAttackKind.Leap &&
                attackKind != BroodmotherSpiderRuntimeAttackKind.Spit)
            {
                reason = "attack-kind-no-damage-bridge";
                return false;
            }

            if (Time.unscaledTime < _nextBroodmotherDamageBridgeTime)
            {
                reason = "cooldown";
                return false;
            }

            if (!IsActiveBroodmotherCompanionLocation(location))
            {
                reason = "active-broodmother-mismatch";
                return false;
            }

            if (npcElement == null || npcElement.HasBeenDiscarded || !npcElement.IsAlive)
            {
                reason = "broodmother-not-live";
                return false;
            }

            if (target == null || target.HasBeenDiscarded || !target.IsAlive || target == Hero.Current)
            {
                reason = "target-not-valid-hostile";
                return false;
            }

            if (!hasSpatialTarget)
            {
                reason = "target-spatial-unavailable";
                return false;
            }

            float maxRange = attackKind switch
            {
                BroodmotherSpiderRuntimeAttackKind.Leap => BroodmotherDamageBridgeLeapRangeMeters,
                BroodmotherSpiderRuntimeAttackKind.Spit => BroodmotherDamageBridgeSpitRangeMeters,
                _ => BroodmotherDamageBridgeShortRangeMeters,
            };
            if (distanceMeters > maxRange)
            {
                reason = "target-out-of-bridge-range:" + FormatDistance(distanceMeters, true) + ">" + maxRange.ToString("0.###");
                return false;
            }

            HealthElement targetHealth = target.HealthElement;
            if (targetHealth == null || targetHealth.HasBeenDiscarded)
            {
                reason = "target-health-unavailable";
                return false;
            }

            Damage.GetNpcDmgBasedOnDamageType(
                npcElement,
                DamageType.PhysicalHitSource,
                isRanged: attackKind == BroodmotherSpiderRuntimeAttackKind.Spit,
                out float nativeMeleeDamage);

            float bridgeDamage = Mathf.Max(
                1f,
                nativeMeleeDamage *
                (attackKind == BroodmotherSpiderRuntimeAttackKind.Leap
                    ? BroodmotherDamageBridgeLeapMultiplier
                    : attackKind == BroodmotherSpiderRuntimeAttackKind.Spit
                        ? BroodmotherDamageBridgeSpitMultiplier
                        : 1f));
            string targetName = GetCharacterLogName(target);
            Vector3 dealerCoords = npcElement.Coords;
            if (!TryGetCharacterCoords(target, out Vector3 targetCoords))
            {
                reason = "target-coords-unavailable";
                return false;
            }

            Vector3 direction = targetCoords - dealerCoords;
            if (direction.sqrMagnitude > 0.0001f)
            {
                direction.Normalize();
            }
            else
            {
                direction = npcElement.ParentTransform != null ? npcElement.ParentTransform.forward : Vector3.forward;
            }

            DamageParameters parameters = DamageParameters.Default;
            parameters.Position = targetCoords;
            parameters.DealerPosition = dealerCoords;
            parameters.Direction = direction;
            parameters.ForceDirection = direction;
            parameters.PoiseDamage = attackKind == BroodmotherSpiderRuntimeAttackKind.Leap ? 2f : 1f;
            parameters.ForceDamage = attackKind == BroodmotherSpiderRuntimeAttackKind.Leap ? 2f : 1f;

            Damage damage = new Damage(
                parameters,
                npcElement,
                target,
                new RawDamageData(bridgeDamage));
            targetHealth.TakeDamage(damage);
            if (attackKind == BroodmotherSpiderRuntimeAttackKind.Leap)
            {
                BroodmotherRangedAttackVfxRuntime.Spawn(
                    dealerCoords + Vector3.up * 1.25f,
                    targetCoords + Vector3.up * 1.05f,
                    Logger);
            }

            _nextBroodmotherDamageBridgeTime =
                Time.unscaledTime + Math.Max(0.4f, _advancedSpiderCombatPromptSeconds.Value);

            reason =
                "applied:" + bridgeDamage.ToString("0.##") +
                ";attack=" + attackKind +
                ";target=" + targetName +
                ";nativeMelee=" + nativeMeleeDamage.ToString("0.##");
            if (Time.unscaledTime >= _nextBroodmotherDamageBridgeLogTime)
            {
                Logger.LogInfo(
                    "BROODMOTHER_COMPANION_DAMAGE_BRIDGE " +
                    $"attack={attackKind}, target={FormatMarkerValue(targetName)}, damage={bridgeDamage:0.##}, nativeMelee={nativeMeleeDamage:0.##}, " +
                    $"distance={FormatDistance(distanceMeters, true)}, maxRange={maxRange:0.###}, source=advanced-spider-ai-commit, nativeEntry=HealthElement.TakeDamage.");
                _nextBroodmotherDamageBridgeLogTime = Time.unscaledTime + 3f;
            }

            return true;
        }
        catch (Exception ex)
        {
            reason = ex.GetType().Name + ": " + ex.Message;
            Logger.LogWarning($"{PluginName} Broodmother damage bridge failed: {reason}");
            return false;
        }
    }

    private bool TrySelectPrimaryCompanionCombatTarget(
        Hero hero,
        NpcElement npcElement,
        Vector3 companionCoords,
        Quaternion companionRotation,
        [NotNullWhen(true)] out ICharacter? primaryTarget,
        out int combatCandidateCount,
        out float primaryDistanceMeters,
        out float primaryAngleDegrees,
        out bool primaryHasSpatialTarget,
        out string targetSource)
    {
        primaryTarget = null;
        combatCandidateCount = 0;
        primaryDistanceMeters = 0f;
        primaryAngleDegrees = 0f;
        primaryHasSpatialTarget = false;
        targetSource = "none";
        float bestDistance = float.PositiveInfinity;
        int bestPriority = int.MaxValue;
        List<ICharacter> seenCandidates = new();

        ConsiderCombatCandidates(
            hero.PossibleAttackers,
            hero,
            npcElement,
            "hero.possible-attackers",
            priority: 0,
            companionCoords,
            companionRotation,
            seenCandidates,
            ref primaryTarget,
            ref combatCandidateCount,
            ref bestPriority,
            ref bestDistance,
            ref primaryDistanceMeters,
            ref primaryAngleDegrees,
            ref primaryHasSpatialTarget,
            ref targetSource);
        ConsiderCombatCandidates(
            npcElement.PossibleAttackers,
            hero,
            npcElement,
            "broodmother.possible-attackers",
            priority: 1,
            companionCoords,
            companionRotation,
            seenCandidates,
            ref primaryTarget,
            ref combatCandidateCount,
            ref bestPriority,
            ref bestDistance,
            ref primaryDistanceMeters,
            ref primaryAngleDegrees,
            ref primaryHasSpatialTarget,
            ref targetSource);
        ConsiderCombatCandidates(
            npcElement.PossibleTargets,
            hero,
            npcElement,
            "broodmother.possible-targets",
            priority: 2,
            companionCoords,
            companionRotation,
            seenCandidates,
            ref primaryTarget,
            ref combatCandidateCount,
            ref bestPriority,
            ref bestDistance,
            ref primaryDistanceMeters,
            ref primaryAngleDegrees,
            ref primaryHasSpatialTarget,
            ref targetSource);

        if (primaryTarget == null)
        {
            return false;
        }

        return true;
    }

    private static void ConsiderCombatCandidates(
        IEnumerable<ICharacter> candidates,
        Hero hero,
        NpcElement npcElement,
        string source,
        int priority,
        Vector3 companionCoords,
        Quaternion companionRotation,
        List<ICharacter> seenCandidates,
        ref ICharacter? primaryTarget,
        ref int combatCandidateCount,
        ref int bestPriority,
        ref float bestDistance,
        ref float primaryDistanceMeters,
        ref float primaryAngleDegrees,
        ref bool primaryHasSpatialTarget,
        ref string targetSource)
    {
        try
        {
            foreach (ICharacter candidate in candidates)
            {
                if (!IsLiveCombatCandidate(candidate, hero, npcElement) ||
                    HasSeenCombatCandidate(seenCandidates, candidate))
                {
                    continue;
                }

                seenCandidates.Add(candidate);
                combatCandidateCount++;
                bool hasCoords = TryGetCharacterCoords(candidate, out Vector3 candidateCoords);
                float distance = hasCoords ? Vector3.Distance(companionCoords, candidateCoords) : float.PositiveInfinity;
                bool better =
                    primaryTarget == null ||
                    priority < bestPriority ||
                    (priority == bestPriority && hasCoords && (!primaryHasSpatialTarget || distance < bestDistance));

                if (!better)
                {
                    continue;
                }

                bestPriority = priority;
                bestDistance = distance;
                targetSource = source;
                primaryTarget = candidate;
                if (hasCoords)
                {
                    primaryDistanceMeters = distance;
                    primaryAngleDegrees = ComputeTargetAngleDegrees(
                        companionCoords,
                        companionRotation,
                        candidateCoords);
                    primaryHasSpatialTarget = true;
                }
                else
                {
                    primaryDistanceMeters = 0f;
                    primaryAngleDegrees = 0f;
                    primaryHasSpatialTarget = false;
                }
            }
        }
        catch
        {
            // Native possible-target stores can be transient during combat ticks; skip the source.
        }
    }

    private static bool TryRegisterNativeCombatTarget(NpcElement npcElement, ICharacter target, out string reason)
    {
        reason = "targetRegistered=0";
        try
        {
            bool alreadyRegistered = npcElement.PossibleTargets.Contains(target);
            bool added = npcElement.TryAddPossibleCombatTarget(target);
            reason =
                "targetRegistered=" + (alreadyRegistered || added ? "1" : "0") +
                ";targetRegisteredChanged=" + (added ? "1" : "0");
            return alreadyRegistered || added;
        }
        catch (Exception ex)
        {
            reason = "targetRegistered=0;targetRegisterReason=" + ex.GetType().Name;
            return false;
        }
    }

    private static string ReadNativeCombatHandoffState(NpcElement npcElement, ICharacter expectedTarget)
    {
        try
        {
            ICharacter? currentTarget = npcElement.GetCurrentTarget();
            return
                ";npcInCombat=" + (npcElement.NpcAI?.InCombat == true ? "1" : "0") +
                ";currentTarget=" + FormatMarkerValue(GetCharacterLogName(currentTarget)) +
                ";currentTargetMatched=" + (ReferenceEquals(currentTarget, expectedTarget) ? "1" : "0");
        }
        catch (Exception ex)
        {
            return ";combatReadback=failed-" + ex.GetType().Name;
        }
    }

    private static string GetCharacterLogName(ICharacter? character)
    {
        if (character == null)
        {
            return "none";
        }

        try
        {
            string name = character.Name;
            return string.IsNullOrWhiteSpace(name)
                ? character.GetType().Name
                : name.Trim();
        }
        catch
        {
            return character.GetType().Name;
        }
    }

    private static bool IsLiveCombatCandidate(ICharacter? candidate, Hero hero, NpcElement npcElement)
    {
        return candidate != null &&
            !candidate.HasBeenDiscarded &&
            candidate.IsAlive &&
            !ReferenceEquals(candidate, hero) &&
            !ReferenceEquals(candidate, npcElement);
    }

    private static bool HasSeenCombatCandidate(List<ICharacter> seenCandidates, ICharacter candidate)
    {
        for (int i = 0; i < seenCandidates.Count; i++)
        {
            if (ReferenceEquals(seenCandidates[i], candidate))
            {
                return true;
            }
        }

        return false;
    }

    private static bool TryGetCharacterCoords(ICharacter character, out Vector3 coords)
    {
        if (character is NpcElement npcElement)
        {
            coords = npcElement.Coords;
            return true;
        }

        if (character is Hero hero)
        {
            coords = hero.Coords;
            return true;
        }

        PropertyInfo? coordsProperty = character.GetType().GetProperty(
            "Coords",
            BindingFlags.Public | BindingFlags.Instance);
        if (coordsProperty?.PropertyType == typeof(Vector3) &&
            coordsProperty.GetValue(character) is Vector3 reflectedCoords)
        {
            coords = reflectedCoords;
            return true;
        }

        coords = Vector3.zero;
        return false;
    }

    private static float ComputeTargetAngleDegrees(
        Vector3 companionCoords,
        Quaternion companionRotation,
        Vector3 targetCoords)
    {
        Vector3 toTarget = targetCoords - companionCoords;
        toTarget.y = 0f;
        if (toTarget.sqrMagnitude <= 0.001f)
        {
            return 0f;
        }

        Vector3 forward = companionRotation * Vector3.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude <= 0.001f)
        {
            return 0f;
        }

        return Vector3.Angle(forward.normalized, toTarget.normalized);
    }

    private int CountLiveHeroAttackers(Hero hero)
    {
        int count = 0;
        try
        {
            foreach (ICharacter attacker in hero.PossibleAttackers)
            {
                if (attacker == null || attacker.HasBeenDiscarded || !attacker.IsAlive)
                {
                    continue;
                }

                count++;
            }
        }
        catch
        {
            return 0;
        }

        return count;
    }

    private int CountLiveCompanionCombatCandidates(Hero hero, NpcElement npcElement)
    {
        return TrySelectPrimaryCompanionCombatTarget(
            hero,
            npcElement,
            npcElement.Coords,
            npcElement.Rotation,
            out _,
            out int combatCandidateCount,
            out _,
            out _,
            out _,
            out _)
            ? combatCandidateCount
            : 0;
    }

    private void DismissCompanion(string source)
    {
        Location? location = _activeBroodmother;
        try
        {
            if (location != null && !location.HasBeenDiscarded)
            {
                string displayName = ActiveCompanionDisplayName;
                ReleaseBroodmotherRuntimeHitSurface(location);
                RemoveNativeCompanionPrompt(location);
                location.MarkedNotSaved = true;
                location.Discard();
                SetPanelStatus(displayName + " dismissed.");
                Logger.LogInfo($"{PluginName} dismissed {displayName} during {source}.");
                LogCommand("dismiss", "dismissed", source, location);
            }
            else
            {
                SetPanelStatus("Dismiss ignored: no active companion.");
                LogCommand("dismiss", "ignored", "no-active-companion");
            }
        }
        catch (Exception ex)
        {
            SetPanelStatus("Dismiss failed.");
            Logger.LogWarning($"{PluginName} dismiss failed during {source}: {ex.GetType().Name}: {ex.Message}");
            LogCommand("dismiss", "failed", ex.GetType().Name, location);
        }
        finally
        {
            ClearActiveCompanion();
        }
    }

    private void FailSpawnedLocation(Location location, string command, string reason)
    {
        ReleaseBroodmotherOverlay();
        try
        {
            ReleaseBroodmotherRuntimeHitSurface(location);
            RemoveNativeCompanionPrompt(location);
            location.MarkedNotSaved = true;
            location.Discard();
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"{PluginName} failed cleanup after {command}: {ex.GetType().Name}: {ex.Message}");
        }

        SetPanelStatus("Summon discarded: " + reason);
        Logger.LogWarning($"{PluginName} summon discarded: {reason}");
        LogCommand(command, "discarded", reason, location);
    }

    private void ClearActiveCompanion(bool retainDeathVisual = false)
    {
        StopSpiderRuntimeHost("active-companion-cleared");
        ReleaseBroodmotherRuntimeHitSurface(_activeBroodmother);
        _dialogueHost?.Close("active-companion-cleared", preserveStatus: true);
        _embeddedAudioRuntime?.StopAllVoices();
        if (retainDeathVisual)
        {
            RetainBroodmotherNativeDeathVisualReferences();
        }
        else
        {
            ReleaseBroodmotherOverlay();
        }

        _activeBroodmother = null;
        _activeDefinition = null;
        _defendMode = false;
        _holdPositionMode = false;
        _followRange = BroodmotherFollowRange.Normal;
        _broodmotherOverlayFailureLogged = false;
        _broodmotherNativeDeathPendingLogged = false;
        _broodmotherLifecycleReadyLogged = false;
        _broodmotherLifecycleWarningLogged = false;
        _broodmotherLifecycleWarningReason = string.Empty;
        _broodmotherAnimatedDeathVisibilityLogged = false;
        _nextAdvancedSpiderCombatPromptTime = 0f;
        _nextAdvancedSpiderAiLogTime = 0f;
        _advancedSpiderTelegraphReady = false;
        _nextBroodmotherDamageBridgeTime = 0f;
        _nextBroodmotherDamageBridgeLogTime = 0f;
    }

    private Vector3 GetCompanionOffset()
    {
        return (Vector3.forward * _summonDistance.Value) + (Vector3.right * _summonRightOffset.Value);
    }

    private bool TryEnsureBroodmotherNativeVisual(
        NpcElement npcElement,
        SpiderFamilyCompanionDefinition definition,
        out string reason)
    {
        if (!string.IsNullOrWhiteSpace(definition.RuntimeOverlayVisualAddress))
        {
            return TryEnsureCompanionRuntimeOverlay(npcElement, definition, out reason);
        }

        if (npcElement.Controller == null)
        {
            reason = "waiting-for-npc-controller";
            return true;
        }

        if (_broodmotherOverlayInstance != null)
        {
            UnityEngine.Object.Destroy(_broodmotherOverlayInstance);
            _broodmotherOverlayInstance = null;
        }

        Renderer[] renderers = npcElement.Controller.gameObject.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length <= 0)
        {
            reason = "Broodmother native visual has no Unity renderers under NpcController.";
            return false;
        }

        Transform scaleRoot = ResolveBroodmotherNativeVisualScaleRoot(npcElement);
        CaptureBroodmotherNativeVisualBaseScale(scaleRoot);
        ApplyBroodmotherNativeVisualScale(scaleRoot, definition);

        if (!_broodmotherOverlayReady && !TryApplyBroodmotherNativeVisualHdrpMaterials(renderers, out reason))
        {
            return false;
        }

        _broodmotherOverlayReady = true;
        ReassertBroodmotherNativeVisual(npcElement, definition);
        int enabledRenderers = CountEnabledRenderers(renderers);
        if (enabledRenderers <= 0)
        {
            reason = "Broodmother native visual renderers remained disabled.";
            return false;
        }

        if (!_broodmotherOverlayRequested)
        {
            _broodmotherOverlayRequested = true;
            Logger.LogInfo(
                $"BROODMOTHER_COMPANION_NATIVE_VISUAL_READY visual={definition.VisualAddress}, variant={definition.Id}, " +
                $"renderers={renderers.Length}, enabledRenderers={enabledRenderers}, " +
                $"runtimeMaterials={_broodmotherOverlayRuntimeMaterialCount}, materialSlots={_broodmotherOverlayMaterialSlotCount}, " +
                $"sourceShaders={FormatMarkerValue(_broodmotherOverlaySourceShaders)}, targetShaders={FormatMarkerValue(_broodmotherOverlayTargetShaders)}, " +
                $"baseTextureSources={FormatMarkerValue(_broodmotherOverlayBaseTextureSources)}, normalTextureSources={FormatMarkerValue(_broodmotherOverlayNormalTextureSources)}, maskTextureSources=disabled, " +
                $"scaleRoot={FormatMarkerValue(scaleRoot.name)}, baseScale={FormatMarkerValue(FormatVector(_broodmotherNativeVisualBaseScale))}, " +
                $"appliedScale={FormatMarkerValue(FormatVector(scaleRoot.localScale))}, scaleMultiplier={definition.VisualScaleMultiplier:0.###}, " +
                $"animators={CountComponentsByTypeName(npcElement.Controller.gameObject, "Animator")}, " +
                $"arNpcAnimancers={CountComponentsByTypeName(npcElement.Controller.gameObject, "ARNpcAnimancer")}, " +
                $"rootMotions={CountComponentsByTypeName(npcElement.Controller.gameObject, "RootMotion")}, " +
                $"animatorClipPlayers={CountComponentsByTypeName(npcElement.Controller.gameObject, "AnimatorClipPlayer")}, " +
                "overlay=false, nativeRenderersHidden=false, sourceMaterialMutation=false, saveWrite=false.");
        }

        reason = "ready";
        return true;
    }

    private bool TryEnsureCompanionRuntimeOverlay(
        NpcElement npcElement,
        SpiderFamilyCompanionDefinition definition,
        out string reason)
    {
        if (npcElement.Controller == null)
        {
            reason = "waiting-for-npc-controller";
            return true;
        }

        if (_broodmotherOverlayReady && _broodmotherOverlayInstance != null)
        {
            ReassertCompanionRuntimeOverlayVisibility(npcElement);
            reason = "ready";
            return true;
        }

        string overlayAddress = definition.RuntimeOverlayVisualAddress ?? string.Empty;
        if (string.IsNullOrWhiteSpace(overlayAddress))
        {
            reason = "runtime-overlay-address-missing";
            return false;
        }

        if (!_broodmotherOverlayRequested)
        {
            if (!TryFindUniqueRuntimeOverlayLocation(overlayAddress, out IResourceLocation? overlayLocation, out string locationReason))
            {
                reason = $"{definition.DisplayName} runtime overlay visual failed to resolve from Addressables: {locationReason}.";
                return false;
            }

            _broodmotherOverlayRequested = true;
            _broodmotherOverlayHandle = Addressables.LoadAssetAsync<GameObject>(overlayLocation);
            Logger.LogInfo(
                $"{PluginName} runtime overlay native locator selected; variant={definition.Id}, overlayVisual={overlayAddress}, {locationReason}, saveWrite=false.");
            reason = "waiting-for-runtime-overlay-load";
            return true;
        }

        if (!_broodmotherOverlayHandle.IsDone)
        {
            reason = "waiting-for-runtime-overlay-load";
            return true;
        }

        if (_broodmotherOverlayHandle.Status != AsyncOperationStatus.Succeeded ||
            _broodmotherOverlayHandle.Result == null)
        {
            Exception? operationException = _broodmotherOverlayHandle.OperationException;
            string exceptionText = operationException == null
                ? "none"
                : operationException.GetType().Name + ":" + operationException.Message;
            reason = $"{definition.DisplayName} runtime overlay visual failed to load from Addressables; status={_broodmotherOverlayHandle.Status}; exception={exceptionText}.";
            return false;
        }

        Transform parent = npcElement.Controller.AlivePrefab != null
            ? npcElement.Controller.AlivePrefab.transform
            : npcElement.Controller.transform;
        _broodmotherOverlayInstance = UnityEngine.Object.Instantiate(_broodmotherOverlayHandle.Result, parent, false);
        _broodmotherOverlayInstance.name = "AvalonBroodmotherCompanion." + definition.Id + ".RuntimeOverlay";
        _broodmotherOverlayInstance.transform.localPosition = Vector3.zero;
        _broodmotherOverlayInstance.transform.localRotation = Quaternion.identity;
        _broodmotherOverlayInstance.transform.localScale = Vector3.one;

        Renderer[] overlayRenderers = _broodmotherOverlayInstance.GetComponentsInChildren<Renderer>(true);
        if (overlayRenderers.Length <= 0)
        {
            reason = $"{definition.DisplayName} runtime overlay has no Unity renderers.";
            ReleaseBroodmotherOverlay();
            return false;
        }

        if (!TryApplyBroodmotherNativeVisualHdrpMaterials(overlayRenderers, out reason))
        {
            ReleaseBroodmotherOverlay();
            return false;
        }

        CaptureCompanionRuntimeOverlaySurfaces(_broodmotherOverlayInstance);
        StripCompanionRuntimeOverlayMountedProps(_broodmotherOverlayInstance);
        _broodmotherOverlayInstance.SetActive(true);
        _broodmotherOverlayReady = true;
        ReassertCompanionRuntimeOverlayVisibility(npcElement);

        Logger.LogInfo(
            "BROODMOTHER_COMPANION_RUNTIME_OVERLAY_READY " +
            $"target={definition.LocationTemplateName}[{definition.LocationTemplateGuid}], npcTemplate={definition.NpcTemplateGuid}, " +
            $"variant={definition.Id}, templateVisual={definition.TemplateVisualAddress}, overlayVisual={overlayAddress}, " +
            $"overlayRenderers={_broodmotherRuntimeOverlayRenderers.Count}, strippedOverlayRenderers={_broodmotherRuntimeOverlayStrippedRenderers.Count}, " +
            $"overlayKandraRenderers={_broodmotherRuntimeOverlayKandraRenderers.Count}, strippedOverlayKandraRenderers={_broodmotherRuntimeOverlayStrippedKandraRenderers.Count}, " +
            $"runtimeMaterials={_broodmotherOverlayRuntimeMaterialCount}, materialSlots={_broodmotherOverlayMaterialSlotCount}, " +
            $"sourceShaders={FormatMarkerValue(_broodmotherOverlaySourceShaders)}, targetShaders={FormatMarkerValue(_broodmotherOverlayTargetShaders)}, " +
            "nativeBootstrapRenderersHidden=true, sourceMaterialMutation=false, saveWrite=false.");

        reason = "ready";
        return true;
    }

    private static bool TryFindUniqueRuntimeOverlayLocation(
        string address,
        out IResourceLocation? location,
        out string reason)
    {
        location = null;
        var matches = new List<IResourceLocation>();
        var distinctLocations = new List<IResourceLocation>();
        var uniqueLocators = new List<IResourceLocator>();
        int duplicateLocatorReferences = 0;
        int duplicateLocationReferences = 0;
        int duplicateLocationIdentities = 0;

        foreach (IResourceLocator locator in Addressables.ResourceLocators)
        {
            if (uniqueLocators.Any(existing => ReferenceEquals(existing, locator)))
            {
                duplicateLocatorReferences++;
                continue;
            }

            uniqueLocators.Add(locator);
            if (!locator.Locate(address, typeof(GameObject), out IList<IResourceLocation> locations) ||
                locations == null ||
                locations.Count <= 0)
            {
                continue;
            }

            foreach (IResourceLocation candidate in locations)
            {
                if (candidate == null)
                {
                    continue;
                }

                matches.Add(candidate);
                if (distinctLocations.Any(existing => ReferenceEquals(existing, candidate)))
                {
                    duplicateLocationReferences++;
                    continue;
                }

                if (distinctLocations.Any(existing => SameRuntimeOverlayLocationIdentity(existing, candidate)))
                {
                    duplicateLocationIdentities++;
                    continue;
                }

                distinctLocations.Add(candidate);
            }
        }

        if (matches.Count == 0)
        {
            reason = "missing-runtime-overlay-location";
            return false;
        }

        if (distinctLocations.Count != 1)
        {
            reason =
                "ambiguous-runtime-overlay-location; rawMatches=" + matches.Count +
                "; distinctLocations=" + distinctLocations.Count +
                "; duplicateLocatorReferences=" + duplicateLocatorReferences +
                "; duplicateLocationReferences=" + duplicateLocationReferences +
                "; duplicateLocationIdentities=" + duplicateLocationIdentities;
            return false;
        }

        if (!typeof(GameObject).IsAssignableFrom(distinctLocations[0].ResourceType))
        {
            reason = "wrong-runtime-overlay-resource-type:" + distinctLocations[0].ResourceType.FullName;
            return false;
        }

        location = distinctLocations[0];
        reason =
            "located-runtime-overlay-location; rawMatches=" + matches.Count +
            "; distinctLocations=1" +
            "; duplicateLocatorReferences=" + duplicateLocatorReferences +
            "; duplicateLocationReferences=" + duplicateLocationReferences +
            "; duplicateLocationIdentities=" + duplicateLocationIdentities;
        return true;
    }

    private static bool SameRuntimeOverlayLocationIdentity(IResourceLocation left, IResourceLocation right)
    {
        return string.Equals(left.PrimaryKey, right.PrimaryKey, StringComparison.Ordinal) &&
               string.Equals(left.InternalId, right.InternalId, StringComparison.Ordinal) &&
               left.ResourceType == right.ResourceType;
    }

    private void CaptureCompanionRuntimeOverlaySurfaces(GameObject overlayInstance)
    {
        _broodmotherRuntimeOverlayRenderers.Clear();
        _broodmotherRuntimeOverlayStrippedRenderers.Clear();
        _broodmotherRuntimeOverlayKandraRenderers.Clear();
        _broodmotherRuntimeOverlayStrippedKandraRenderers.Clear();

        foreach (Renderer renderer in overlayInstance.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer != null)
            {
                _broodmotherRuntimeOverlayRenderers.Add(renderer);
            }
        }

        foreach (Component component in overlayInstance.GetComponentsInChildren<Component>(true))
        {
            if (IsKandraRendererComponent(component))
            {
                _broodmotherRuntimeOverlayKandraRenderers.Add(component);
            }
        }
    }

    private void StripCompanionRuntimeOverlayMountedProps(GameObject overlayInstance)
    {
        Transform overlayRoot = overlayInstance.transform;
        foreach (Renderer renderer in overlayInstance.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer == null || !IsMountedProp(renderer.transform, overlayRoot))
            {
                continue;
            }

            renderer.enabled = false;
            _broodmotherRuntimeOverlayStrippedRenderers.Add(renderer);
        }

        foreach (Component component in overlayInstance.GetComponentsInChildren<Component>(true))
        {
            if (!IsKandraRendererComponent(component) || !IsMountedProp(component.transform, overlayRoot))
            {
                continue;
            }

            SetKandraRendererEnabled(component, false);
            _broodmotherRuntimeOverlayStrippedKandraRenderers.Add(component);
        }
    }

    private void ReassertCompanionRuntimeOverlayVisibility(NpcElement npcElement)
    {
        if (npcElement.Controller == null)
        {
            return;
        }

        foreach (Renderer renderer in npcElement.Controller.gameObject.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer == null)
            {
                continue;
            }

            renderer.enabled =
                _broodmotherRuntimeOverlayRenderers.Contains(renderer) &&
                !_broodmotherRuntimeOverlayStrippedRenderers.Contains(renderer);
            if (renderer is SkinnedMeshRenderer skinned)
            {
                skinned.updateWhenOffscreen = true;
            }
        }

        foreach (Component component in npcElement.Controller.gameObject.GetComponentsInChildren<Component>(true))
        {
            if (!IsKandraRendererComponent(component))
            {
                continue;
            }

            SetKandraRendererEnabled(
                component,
                _broodmotherRuntimeOverlayKandraRenderers.Contains(component) &&
                !_broodmotherRuntimeOverlayStrippedKandraRenderers.Contains(component));
        }
    }

    private static bool IsMountedProp(Transform transform, Transform overlayRoot)
    {
        Transform? cursor = transform;
        while (cursor != null)
        {
            string name = cursor.name ?? string.Empty;
            if (string.Equals(name, "L_w", StringComparison.Ordinal) ||
                string.Equals(name, "R_w", StringComparison.Ordinal) ||
                name.IndexOf("Placeholder", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            if (cursor == overlayRoot)
            {
                break;
            }

            cursor = cursor.parent;
        }

        return false;
    }

    private static bool IsKandraRendererComponent(Component? component)
    {
        return component != null &&
            string.Equals(component.GetType().Name, "KandraRenderer", StringComparison.Ordinal);
    }

    private static void SetKandraRendererEnabled(Component component, bool enabled)
    {
        try
        {
            PropertyInfo? property = component.GetType().GetProperty(
                "enabled",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (property != null && property.PropertyType == typeof(bool) && property.CanWrite)
            {
                property.SetValue(component, enabled);
            }
        }
        catch (Exception)
        {
            // Kandra visibility is best-effort here; Unity Renderer visibility remains authoritative.
        }
    }

    private void ReassertBroodmotherNativeVisual(
        NpcElement npcElement,
        SpiderFamilyCompanionDefinition definition)
    {
        if (npcElement.Controller == null)
        {
            return;
        }

        if (_broodmotherNativeVisualScaleRoot != null)
        {
            ApplyBroodmotherNativeVisualScale(_broodmotherNativeVisualScaleRoot, definition);
        }

        foreach (Renderer renderer in npcElement.Controller.gameObject.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer == null)
            {
                continue;
            }

            renderer.enabled = true;
            if (renderer is SkinnedMeshRenderer skinned)
            {
                skinned.updateWhenOffscreen = true;
            }
        }
    }

    private bool TryApplyBroodmotherNativeVisualHdrpMaterials(Renderer[] renderers, out string reason)
    {
        ReleaseBroodmotherOverlayRuntimeMaterials();
        _broodmotherOverlayRuntimeMaterialCount = 0;
        _broodmotherOverlayMaterialSlotCount = 0;
        _broodmotherOverlaySourceShaders = "none";
        _broodmotherOverlayBaseTextureSources = "none";
        _broodmotherOverlayNormalTextureSources = "none";

        Shader? hdrpLit = Shader.Find("HDRP/Lit");

        Dictionary<Material, Material> convertedBySource = new();
        List<string> sourceShaders = new();
        List<string> targetShaders = new();
        List<string> baseTextureSources = new();
        List<string> normalTextureSources = new();
        foreach (Renderer renderer in renderers)
        {
            if (renderer == null)
            {
                continue;
            }

            Material[] materials = renderer.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
            {
                _broodmotherOverlayMaterialSlotCount++;
                Material source = materials[i];
                if (source == null || source.shader == null)
                {
                    reason = $"Broodmother native visual material setup failed because renderer {renderer.name} has a null material or shader at slot {i}.";
                    return false;
                }

                string sourceShaderName = source.shader.name ?? "unknown";
                if (!ContainsString(sourceShaders, sourceShaderName))
                {
                    sourceShaders.Add(sourceShaderName);
                }

                if (!convertedBySource.TryGetValue(source, out Material converted))
                {
                    if (!TryCreateBroodmotherRuntimeMaterial(
                            source,
                            hdrpLit,
                            out converted,
                            out string targetShaderName,
                            out string baseTextureSource,
                            out string normalTextureSource,
                            out reason))
                    {
                        return false;
                    }

                    if (!ContainsString(targetShaders, targetShaderName))
                    {
                        targetShaders.Add(targetShaderName);
                    }

                    if (!ContainsString(baseTextureSources, baseTextureSource))
                    {
                        baseTextureSources.Add(baseTextureSource);
                    }

                    if (!ContainsString(normalTextureSources, normalTextureSource))
                    {
                        normalTextureSources.Add(normalTextureSource);
                    }

                    convertedBySource[source] = converted;
                    _broodmotherOverlayRuntimeMaterials.Add(converted);
                }

                materials[i] = converted;
            }

            renderer.sharedMaterials = materials;
            renderer.enabled = true;
            if (renderer is SkinnedMeshRenderer skinned)
            {
                skinned.updateWhenOffscreen = true;
            }
        }

        _broodmotherOverlayRuntimeMaterialCount = convertedBySource.Count;
        _broodmotherOverlaySourceShaders = sourceShaders.Count == 0 ? "none" : string.Join("|", sourceShaders);
        _broodmotherOverlayTargetShaders = targetShaders.Count == 0 ? "none" : string.Join("|", targetShaders);
        _broodmotherOverlayBaseTextureSources = baseTextureSources.Count == 0 ? "none" : string.Join("|", baseTextureSources);
        _broodmotherOverlayNormalTextureSources = normalTextureSources.Count == 0 ? "none" : string.Join("|", normalTextureSources);
        reason = string.Empty;
        return true;
    }

    private static bool TryCreateBroodmotherRuntimeMaterial(
        Material source,
        Shader? hdrpLit,
        out Material material,
        out string targetShaderName,
        out string baseTextureSource,
        out string normalTextureSource,
        out string reason)
    {
        if (hdrpLit != null && hdrpLit.isSupported)
        {
            material = CreateHdrpMaterial(source, hdrpLit, out baseTextureSource, out normalTextureSource);
            targetShaderName = "HDRP/Lit";
            reason = string.Empty;
            return true;
        }

        Shader sourceShader = source.shader;
        if (sourceShader != null && sourceShader.isSupported)
        {
            material = new Material(source)
            {
                name = source.name + "_Broodmother_Companion_SourceShaderFallback",
            };
            targetShaderName = sourceShader.name ?? "source-shader";
            baseTextureSource = "source-shader-fallback";
            normalTextureSource = "source-shader-fallback";
            reason = string.Empty;
            return true;
        }

        material = null!;
        targetShaderName = "none";
        baseTextureSource = "none";
        normalTextureSource = "none";
        reason = "Broodmother native visual material setup failed because the source shader is unsupported and HDRP/Lit is unavailable.";
        return false;
    }

    private static Transform ResolveBroodmotherNativeVisualScaleRoot(NpcElement npcElement)
    {
        if (npcElement.Controller == null)
        {
            throw new InvalidOperationException("Broodmother native visual scale root cannot be resolved without an NpcController.");
        }

        Component[] components = npcElement.Controller.gameObject.GetComponentsInChildren<Component>(true);
        for (int i = 0; i < components.Length; i++)
        {
            Component component = components[i];
            if (component != null && string.Equals(component.GetType().Name, "Animator", StringComparison.Ordinal))
            {
                return component.transform;
            }
        }

        return npcElement.Controller.transform;
    }

    private void CaptureBroodmotherNativeVisualBaseScale(Transform scaleRoot)
    {
        if (_broodmotherNativeVisualScaleCaptured && ReferenceEquals(_broodmotherNativeVisualScaleRoot, scaleRoot))
        {
            return;
        }

        _broodmotherNativeVisualScaleRoot = scaleRoot;
        _broodmotherNativeVisualBaseScale = IsFinitePositiveScale(scaleRoot.localScale)
            ? scaleRoot.localScale
            : Vector3.one;
        _broodmotherNativeVisualScaleCaptured = true;
    }

    private void ApplyBroodmotherNativeVisualScale(
        Transform scaleRoot,
        SpiderFamilyCompanionDefinition definition)
    {
        Vector3 baseScale = IsFinitePositiveScale(_broodmotherNativeVisualBaseScale)
            ? _broodmotherNativeVisualBaseScale
            : Vector3.one;
        scaleRoot.localScale = baseScale * definition.VisualScaleMultiplier;
    }

    private static bool IsFinitePositiveScale(Vector3 scale)
    {
        return IsFinitePositive(scale.x) && IsFinitePositive(scale.y) && IsFinitePositive(scale.z);
    }

    private static bool IsFinitePositive(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value) && value > 0f;
    }

    private static int CountComponentsByTypeName(GameObject root, string typeName)
    {
        int count = 0;
        foreach (Component component in root.GetComponentsInChildren<Component>(true))
        {
            if (component != null && string.Equals(component.GetType().Name, typeName, StringComparison.Ordinal))
            {
                count++;
            }
        }

        return count;
    }

    private static Material CreateHdrpMaterial(
        Material source,
        Shader hdrpLit,
        out string baseTextureSource,
        out string normalTextureSource)
    {
        Material material = new(hdrpLit)
        {
            name = source.name + "_Broodmother_Companion_HDRP",
            renderQueue = -1,
        };
        material.SetOverrideTag("RenderType", "Opaque");
        material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.DisableKeyword("_ALPHATEST_ON");
        material.DisableKeyword("_MASKMAP");
        material.DisableKeyword("_METALLICSPECGLOSSMAP");

        Color baseColor = ReadColor(source, "_BaseColor", ReadColor(source, "_Color", Color.white));
        baseColor.a = 1f;
        SetColorIfPresent(material, "_BaseColor", baseColor);
        SetFloatIfPresent(material, "_Metallic", 0f);
        SetFloatIfPresent(material, "_MetallicRemapMin", 0f);
        SetFloatIfPresent(material, "_MetallicRemapMax", 0f);
        SetFloatIfPresent(material, "_Smoothness", 0.035f);
        SetFloatIfPresent(material, "_SmoothnessRemapMin", 0f);
        SetFloatIfPresent(material, "_SmoothnessRemapMax", 0.07f);
        SetFloatIfPresent(material, "_CoatMask", 0f);
        SetFloatIfPresent(material, "_CoatSmoothness", 0f);
        SetFloatIfPresent(material, "_AlphaCutoffEnable", 0f);
        SetFloatIfPresent(material, "_SurfaceType", 0f);
        SetFloatIfPresent(material, "_BlendMode", 0f);
        SetFloatIfPresent(material, "_SrcBlend", 1f);
        SetFloatIfPresent(material, "_DstBlend", 0f);
        SetFloatIfPresent(material, "_AlphaSrcBlend", 1f);
        SetFloatIfPresent(material, "_AlphaDstBlend", 0f);
        SetFloatIfPresent(material, "_ZWrite", 1f);
        SetFloatIfPresent(material, "_DoubleSidedEnable", 1f);
        SetFloatIfPresent(material, "_CullMode", 0f);
        SetFloatIfPresent(material, "_CullModeForward", 0f);
        SetFloatIfPresent(material, "_TransparentCullMode", 0f);
        SetColorIfPresent(material, "_EmissiveColor", Color.black);
        SetColorIfPresent(material, "_SpecularColor", Color.black);

        if (!CopyTextureIfPresent(out baseTextureSource, source, material, "_BaseColorMap", "_BaseColorMap", "_BaseMap", "_MainTex", "_Albedo", "_AlbedoMap", "_DiffuseMap", "_ColorMap", "_BaseTexture", "_AlbedoTexture", "_DiffuseTexture"))
        {
            CopyBestTextureByKeyword(out baseTextureSource, source, material, "_BaseColorMap", "albedo", "base", "diffuse", "color", "main");
        }

        if (CopyTextureIfPresent(out normalTextureSource, source, material, "_NormalMap", "_NormalMap", "_BumpMap", "_Normal", "_NormalTexture") ||
            CopyBestTextureByKeyword(out normalTextureSource, source, material, "_NormalMap", "normal", "bump"))
        {
            material.EnableKeyword("_NORMALMAP");
        }

        return material;
    }

    private void ReleaseBroodmotherOverlay()
    {
        if (_broodmotherOverlayInstance != null)
        {
            UnityEngine.Object.Destroy(_broodmotherOverlayInstance);
            _broodmotherOverlayInstance = null;
        }

        ReleaseBroodmotherOverlayRuntimeMaterials();

        if (_broodmotherOverlayHandle.IsValid())
        {
            Addressables.Release(_broodmotherOverlayHandle);
        }

        _broodmotherOverlayRequested = false;
        _broodmotherOverlayReady = false;
        _broodmotherOverlayRuntimeMaterialCount = 0;
        _broodmotherOverlayMaterialSlotCount = 0;
        _broodmotherOverlaySourceShaders = "none";
        _broodmotherOverlayTargetShaders = "none";
        _broodmotherOverlayBaseTextureSources = "none";
        _broodmotherOverlayNormalTextureSources = "none";
        _broodmotherNativeVisualScaleRoot = null;
        _broodmotherNativeVisualBaseScale = Vector3.one;
        _broodmotherNativeVisualScaleCaptured = false;
        _broodmotherRuntimeOverlayRenderers.Clear();
        _broodmotherRuntimeOverlayStrippedRenderers.Clear();
        _broodmotherRuntimeOverlayKandraRenderers.Clear();
        _broodmotherRuntimeOverlayStrippedKandraRenderers.Clear();
    }

    private void RetainBroodmotherNativeDeathVisualReferences()
    {
        if (_broodmotherOverlayInstance != null)
        {
            _broodmotherOverlayInstance = null;
        }

        foreach (Material material in _broodmotherOverlayRuntimeMaterials)
        {
            if (material != null && !_broodmotherRetainedDeathRuntimeMaterials.Contains(material))
            {
                _broodmotherRetainedDeathRuntimeMaterials.Add(material);
            }
        }

        _broodmotherOverlayRuntimeMaterials.Clear();
        if (_broodmotherOverlayHandle.IsValid())
        {
            _broodmotherRetainedDeathHandles.Add(_broodmotherOverlayHandle);
            _broodmotherOverlayHandle = default;
        }

        _broodmotherOverlayRequested = false;
        _broodmotherOverlayReady = false;
        _broodmotherOverlayRuntimeMaterialCount = 0;
        _broodmotherOverlayMaterialSlotCount = 0;
        _broodmotherOverlaySourceShaders = "none";
        _broodmotherOverlayTargetShaders = "none";
        _broodmotherOverlayBaseTextureSources = "none";
        _broodmotherOverlayNormalTextureSources = "none";
        _broodmotherNativeVisualScaleRoot = null;
        _broodmotherNativeVisualBaseScale = Vector3.one;
        _broodmotherNativeVisualScaleCaptured = false;
        _broodmotherRuntimeOverlayRenderers.Clear();
        _broodmotherRuntimeOverlayStrippedRenderers.Clear();
        _broodmotherRuntimeOverlayKandraRenderers.Clear();
        _broodmotherRuntimeOverlayStrippedKandraRenderers.Clear();
    }

    private void ReleaseBroodmotherOverlayRuntimeMaterials()
    {
        for (int i = 0; i < _broodmotherOverlayRuntimeMaterials.Count; i++)
        {
            Material material = _broodmotherOverlayRuntimeMaterials[i];
            if (material != null)
            {
                UnityEngine.Object.Destroy(material);
            }
        }

        _broodmotherOverlayRuntimeMaterials.Clear();
    }

    private void ReleaseBroodmotherRetainedDeathVisualReferences()
    {
        for (int i = 0; i < _broodmotherRetainedDeathRuntimeMaterials.Count; i++)
        {
            Material material = _broodmotherRetainedDeathRuntimeMaterials[i];
            if (material != null)
            {
                UnityEngine.Object.Destroy(material);
            }
        }

        _broodmotherRetainedDeathRuntimeMaterials.Clear();
        for (int i = 0; i < _broodmotherRetainedDeathHandles.Count; i++)
        {
            AsyncOperationHandle<GameObject> handle = _broodmotherRetainedDeathHandles[i];
            if (handle.IsValid())
            {
                Addressables.Release(handle);
            }
        }

        _broodmotherRetainedDeathHandles.Clear();
    }

    private static bool CopyTextureIfPresent(out string copiedSourceProperty, Material source, Material target, string targetProperty, params string[] sourceProperties)
    {
        copiedSourceProperty = "none";
        if (!target.HasProperty(targetProperty))
        {
            return false;
        }

        for (int i = 0; i < sourceProperties.Length; i++)
        {
            string sourceProperty = sourceProperties[i];
            if (!source.HasProperty(sourceProperty))
            {
                continue;
            }

            Texture texture = source.GetTexture(sourceProperty);
            if (texture == null)
            {
                continue;
            }

            target.SetTexture(targetProperty, texture);
            copiedSourceProperty = sourceProperty;
            TryCopyTextureTransform(source, target, sourceProperty, targetProperty);

            return true;
        }

        return false;
    }

    private static bool CopyBestTextureByKeyword(out string copiedSourceProperty, Material source, Material target, string targetProperty, params string[] keywords)
    {
        copiedSourceProperty = "none";
        if (!target.HasProperty(targetProperty))
        {
            return false;
        }

        string[] sourceProperties;
        try
        {
            sourceProperties = source.GetTexturePropertyNames();
        }
        catch (Exception)
        {
            return false;
        }

        for (int i = 0; i < sourceProperties.Length; i++)
        {
            string sourceProperty = sourceProperties[i];
            if (!ContainsAnyKeyword(sourceProperty, keywords))
            {
                continue;
            }

            Texture texture = source.GetTexture(sourceProperty);
            if (texture == null)
            {
                continue;
            }

            target.SetTexture(targetProperty, texture);
            copiedSourceProperty = sourceProperty;
            TryCopyTextureTransform(source, target, sourceProperty, targetProperty);
            return true;
        }

        return false;
    }

    private static bool ContainsAnyKeyword(string value, string[] keywords)
    {
        for (int i = 0; i < keywords.Length; i++)
        {
            if (value.IndexOf(keywords[i], StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }
        }

        return false;
    }

    private static void TryCopyTextureTransform(Material source, Material target, string sourceProperty, string targetProperty)
    {
        try
        {
            target.SetTextureScale(targetProperty, source.GetTextureScale(sourceProperty));
            target.SetTextureOffset(targetProperty, source.GetTextureOffset(sourceProperty));
        }
        catch (Exception)
        {
            // Shader families do not always expose texture transform metadata.
        }
    }

    private static int CountEnabledRenderers(Renderer[] renderers)
    {
        int count = 0;
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null && renderers[i].enabled)
            {
                count++;
            }
        }

        return count;
    }

    private static Color ReadColor(Material material, string propertyName, Color fallback)
    {
        return material.HasProperty(propertyName) ? material.GetColor(propertyName) : fallback;
    }

    private static float ReadFloat(Material material, string propertyName, float fallback)
    {
        return material.HasProperty(propertyName) ? material.GetFloat(propertyName) : fallback;
    }

    private static void SetColorIfPresent(Material material, string propertyName, Color value)
    {
        if (material.HasProperty(propertyName))
        {
            material.SetColor(propertyName, value);
        }
    }

    private static void SetFloatIfPresent(Material material, string propertyName, float value)
    {
        if (material.HasProperty(propertyName))
        {
            material.SetFloat(propertyName, value);
        }
    }

    private static bool ContainsString(List<string> items, string value)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (string.Equals(items[i], value, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private void DrawPanel(int windowId)
    {
        GUILayout.Label("Status: " + _panelStatus);
        GUILayout.Label("Runtime summon: " + (_allowRuntimeSummon.Value ? "enabled" : "disabled"));
        GUILayout.Label("Mode: " + GetCompanionModeLabel() + " | Range: " + GetFollowRangeLabel(_followRange));
        GUILayout.Space(8f);

        if (GUILayout.Button("Summon / Swap"))
        {
            TrySummonOrSwap("panel");
        }

        using (new GUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Recall"))
            {
                RecallCompanion("panel");
            }

            if (GUILayout.Button("Dismiss"))
            {
                DismissCompanion("panel");
            }
        }

        using (new GUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Follow"))
            {
                SetDefendMode(false, "panel");
            }

            if (GUILayout.Button("Hold"))
            {
                SetHoldPositionMode("panel");
            }

            if (GUILayout.Button("Defend"))
            {
                SetDefendMode(true, "panel");
            }
        }

        using (new GUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Close"))
            {
                SetFollowRange(BroodmotherFollowRange.Close, "panel", recallActive: true);
            }

            if (GUILayout.Button("Pace"))
            {
                SetFollowRange(BroodmotherFollowRange.Normal, "panel", recallActive: true);
            }

            if (GUILayout.Button("Far"))
            {
                SetFollowRange(BroodmotherFollowRange.Far, "panel", recallActive: true);
            }
        }

        GUI.DragWindow();
    }

    private void DrawCompanionHud()
    {
        if (!TryGetActiveCompanion(out Location? location, out NpcElement? npcElement))
        {
            return;
        }

        float width = Mathf.Min(560f, Math.Max(380f, Screen.width - 44f));
        Rect rect = new(22f, 72f, width, 134f);
        Color previousColor = GUI.color;
        Color previousContentColor = GUI.contentColor;

        GUI.color = new Color(0f, 0f, 0f, 0.82f);
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = previousColor;

        bool hasHudIcon = DrawCompanionHudIcon(new Rect(rect.x + 14f, rect.y + 18f, 88f, 88f));
        float contentX = rect.x + (hasHudIcon ? 116f : 16f);
        float contentWidth = rect.width - (hasHudIcon ? 132f : 32f);

        GUIStyle titleStyle = HudLabelStyle(18, FontStyle.Bold, TextAnchor.MiddleLeft, Color.white);
        GUIStyle valueStyle = HudLabelStyle(13, FontStyle.Normal, TextAnchor.MiddleLeft, new Color(0.88f, 0.88f, 0.88f, 1f));
        GUIStyle promptStyle = HudLabelStyle(14, FontStyle.Bold, TextAnchor.MiddleLeft, new Color(1f, 0.82f, 0.48f, 1f));
        GUIStyle barTextStyle = HudLabelStyle(13, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
        GUIStyle statusStyle = HudLabelStyle(12, FontStyle.Normal, TextAnchor.UpperLeft, new Color(0.78f, 0.78f, 0.78f, 1f), wordWrap: true);

        string displayName = string.IsNullOrWhiteSpace(location.DisplayName)
            ? ActiveCompanionDisplayName
            : location.DisplayName.Trim();
        GUI.Label(new Rect(contentX, rect.y + 10f, contentWidth, 26f), displayName, titleStyle);

        bool hasHealth = TryReadCompanionHealthPercent(npcElement, out float healthPercent);
        Rect healthBar = new(contentX, rect.y + 42f, contentWidth, 24f);
        DrawCompanionHealthBar(healthBar, hasHealth, healthPercent);
        GUI.Label(
            healthBar,
            hasHealth ? $"Health {healthPercent * 100f:0}%" : "Health unknown",
            barTextStyle);

        Hero? hero = Hero.Current;
        int threats = hero == null ? 0 : CountLiveCompanionCombatCandidates(hero, npcElement);
        GUI.Label(
            new Rect(contentX, rect.y + 74f, contentWidth * 0.48f, 20f),
            $"Mode: {GetCompanionModeLabel()} | Threats: {threats}",
            valueStyle);
        GUI.Label(
            new Rect(contentX + contentWidth * 0.52f, rect.y + 74f, contentWidth * 0.44f, 20f),
            GetNativePromptStatusLine(location),
            promptStyle);
        GUI.Label(
            new Rect(contentX, rect.y + 99f, contentWidth, 28f),
            "Status: " + _panelStatus,
            statusStyle);

        GUI.contentColor = previousContentColor;
        GUI.color = previousColor;
    }

    private bool DrawCompanionHudIcon(Rect frameRect)
    {
        Texture2D? icon = TryGetDefinitionIconTexture(ActiveDefinition, out _) ??
            TaintedInterfaceReflection.GetIcon(CompanionIconId);
        if (icon == null)
        {
            return false;
        }

        Texture2D? background = TaintedInterfaceReflection.GetIcon(CompanionHudBackgroundIconId);
        Color previousColor = GUI.color;
        try
        {
            if (background != null)
            {
                GUI.color = new Color(1f, 1f, 1f, 0.92f);
                GUI.DrawTexture(frameRect, background, ScaleMode.ScaleToFit, true);
            }

            float iconSize = Mathf.Clamp(frameRect.width * 0.54f, 32f, frameRect.width - 18f);
            Rect iconRect = new(
                frameRect.x + (frameRect.width - iconSize) * 0.5f,
                frameRect.y + (frameRect.height - iconSize) * 0.5f,
                iconSize,
                iconSize);
            GUI.color = new Color(1f, 1f, 1f, 0.96f);
            GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit, true);
        }
        finally
        {
            GUI.color = previousColor;
        }

        return true;
    }

    private static Texture2D? TryGetDefinitionIconTexture(
        SpiderFamilyCompanionDefinition definition,
        out string source)
    {
        if (definition.IconVariant.HasValue)
        {
            return BroodmotherEmbeddedIconRuntime.GetIconTexture(definition.IconVariant.Value, out source);
        }

        source = string.Empty;
        return null;
    }

    private void SetDefendMode(bool defend, string source)
    {
        if (!TryGetActiveCompanion(out Location? location, out NpcElement? npcElement))
        {
            SetPanelStatus("Mode change ignored: no active companion.");
            LogCommand("mode", "ignored", "no-active-companion");
            return;
        }

        _defendMode = defend;
        _holdPositionMode = false;
        location.MarkedNotSaved = true;
        SetPanelStatus("Mode set to " + (defend ? "Defend." : "Follow."));
        LogCommand("mode", defend ? "defend" : "follow", source, location, npcElement);

        Hero? hero = Hero.Current;
        if (defend && _enableNativeDefendAssist.Value && hero != null)
        {
            int attackers = CountLiveHeroAttackers(hero);
            if (attackers > 0)
            {
                TriggerDefendAssist(location, npcElement, attackers, "panel defend");
            }
        }
    }

    private void SetHoldPositionMode(string source)
    {
        if (!TryGetActiveCompanion(out Location? location, out NpcElement? npcElement))
        {
            SetPanelStatus("Mode change ignored: no active companion.");
            LogCommand("mode", "ignored", "no-active-companion");
            return;
        }

        _defendMode = false;
        _holdPositionMode = true;
        location.MarkedNotSaved = true;
        SetPanelStatus("Mode set to Hold Position.");
        LogCommand("mode", "hold-position", source, location, npcElement);
    }

    private void SetFollowRange(BroodmotherFollowRange range, string source, bool recallActive)
    {
        BroodmotherFollowRange normalized = NormalizeFollowRange(range);
        if (!TryGetActiveCompanion(out Location? location, out NpcElement? npcElement))
        {
            SetPanelStatus("Range change ignored: no active companion.");
            LogCommand("range", "ignored", "no-active-companion");
            return;
        }

        _followRange = normalized;
        _defendMode = false;
        _holdPositionMode = false;
        location.MarkedNotSaved = true;

        if (recallActive)
        {
            RecallCompanion(source + " range-" + GetFollowRangeLabel(normalized).ToLowerInvariant());
        }

        SetPanelStatus("Distance: " + GetFollowRangeLabel(normalized) + ".");
        LogCommand("range", GetFollowRangeLabel(normalized).ToLowerInvariant(), source, location, npcElement);
    }

    private void ComeCloseCompanion(string source)
    {
        _followRange = BroodmotherFollowRange.Close;
        _defendMode = false;
        _holdPositionMode = false;
        RecallCompanion(source + " come-close");
        SetPanelStatus("Come Close: companion called to you.");
    }

    private void RecoverCompanion(string source)
    {
        if (!TryGetActiveCompanion(out Location? location, out NpcElement? npcElement))
        {
            SetPanelStatus("Recover checked: no active companion.");
            LogCommand("recover", "ignored", "no-active-companion");
            return;
        }

        string displayName = ActiveCompanionDisplayName;
        Hero? hero = Hero.Current;
        if (hero == null)
        {
            SetPanelStatus("Recover blocked: Hero.Current is null.");
            LogCommand("recover", "blocked", "hero-current-null", location, npcElement);
            return;
        }

        float distance = Vector3.Distance(location.Coords, hero.Coords);
        if (distance >= GetRecoveryOutOfRangeDistance())
        {
            RecallCompanion(source + " recover");
            SetPanelStatus("Recover complete: " + displayName + " recalled.");
            LogCommand("recover", "recalled", $"distance={distance:0.##}; threshold={GetRecoveryOutOfRangeDistance():0.##}", location, npcElement);
            return;
        }

        location.MarkedNotSaved = true;
        EnsureNativeCompanionPrompt(location);
        SetPanelStatus("Recover checked; no action needed.");
        LogCommand("recover", "checked", $"distance={distance:0.##}; threshold={GetRecoveryOutOfRangeDistance():0.##}", location, npcElement);
    }

    internal bool IsDialogueDefendMode => _defendMode;

    internal bool IsDialogueHoldPositionMode => _holdPositionMode;

    internal BroodmotherFollowRange DialogueFollowRange => _followRange;

    internal string DialogueStatus => _panelStatus;

    internal int DialogueHeroAttackerCount
    {
        get
        {
            Hero? hero = Hero.Current;
            if (hero == null || !TryGetActiveCompanion(out _, out NpcElement? npcElement))
            {
                return 0;
            }

            return CountLiveCompanionCombatCandidates(hero, npcElement);
        }
    }

    internal bool TryGetActiveBroodmotherForDialogue([NotNullWhen(true)] out Location? location, [NotNullWhen(true)] out NpcElement? npcElement)
    {
        return TryGetActiveCompanion(out location, out npcElement);
    }

    internal void RunDialogueCommand(BroodmotherDialogueCommand command, string source = "companion dialogue")
    {
        switch (command)
        {
            case BroodmotherDialogueCommand.Follow:
                SetDefendMode(false, source);
                break;
            case BroodmotherDialogueCommand.HoldPosition:
                SetHoldPositionMode(source);
                break;
            case BroodmotherDialogueCommand.Defend:
                SetDefendMode(true, source);
                break;
            case BroodmotherDialogueCommand.RangeClose:
                SetFollowRange(BroodmotherFollowRange.Close, source, recallActive: false);
                break;
            case BroodmotherDialogueCommand.RangeNormal:
                SetFollowRange(BroodmotherFollowRange.Normal, source, recallActive: false);
                break;
            case BroodmotherDialogueCommand.RangeFar:
                SetFollowRange(BroodmotherFollowRange.Far, source, recallActive: false);
                break;
            case BroodmotherDialogueCommand.ComeClose:
                ComeCloseCompanion(source);
                break;
            case BroodmotherDialogueCommand.Recall:
                RecallCompanion(source);
                break;
            case BroodmotherDialogueCommand.Recover:
                RecoverCompanion(source);
                break;
            case BroodmotherDialogueCommand.Dismiss:
                DismissCompanion(source);
                break;
        }
    }

    internal string DialogueStatusLine
    {
        get
        {
            if (_defendMode)
            {
                int threats = DialogueHeroAttackerCount;
                return threats > 0
                    ? $"Defending you. Threats nearby: {threats}."
                    : "Defending you. Waiting for a threat.";
            }

            if (_holdPositionMode)
            {
                return "Holding position. Recall brings your companion back to you.";
            }

            return "Following. Your companion will " + GetDialogueRangePhrase(_followRange) + ".";
        }
    }

    private string GetCompanionModeLabel()
    {
        if (_defendMode)
        {
            return "Defend";
        }

        return _holdPositionMode ? "Hold" : "Follow";
    }

    private float GetEffectiveFollowCatchUpDistance()
    {
        return NormalizeFollowRange(_followRange) switch
        {
            BroodmotherFollowRange.Close => 14f,
            BroodmotherFollowRange.Far => 34f,
            _ => _followCatchUpDistance.Value,
        };
    }

    private static float GetRecoveryOutOfRangeDistance()
    {
        return 34f;
    }

    internal static BroodmotherFollowRange NormalizeFollowRange(BroodmotherFollowRange range)
    {
        return range switch
        {
            BroodmotherFollowRange.Close => BroodmotherFollowRange.Close,
            BroodmotherFollowRange.Far => BroodmotherFollowRange.Far,
            _ => BroodmotherFollowRange.Normal,
        };
    }

    internal static string GetFollowRangeLabel(BroodmotherFollowRange range)
    {
        return NormalizeFollowRange(range) switch
        {
            BroodmotherFollowRange.Close => "Close",
            BroodmotherFollowRange.Far => "Far",
            _ => "Normal",
        };
    }

    private static string GetDialogueRangePhrase(BroodmotherFollowRange range)
    {
        return NormalizeFollowRange(range) switch
        {
            BroodmotherFollowRange.Close => "stay close",
            BroodmotherFollowRange.Far => "keep some distance",
            _ => "keep a steady pace",
        };
    }

    internal void LogDialogueHostEvent(string command, string result, string reason, Location? location = null, NpcElement? npcElement = null)
    {
        LogCommand(command, result, reason, location, npcElement);
    }

    private void TryPlayEmbeddedSpiderAudio(BroodmotherEmbeddedAudioCue cue, Location? location, string source)
    {
        if (!ActiveDefinition.UsesSpiderCombatPackage ||
            !_enableEmbeddedSpiderAudio.Value ||
            _embeddedAudioRuntime == null)
        {
            return;
        }

        Vector3 position = Vector3.zero;
        if (location != null && !location.HasBeenDiscarded)
        {
            position = location.Coords;
        }
        else if (Hero.Current != null)
        {
            position = Hero.Current.Coords;
        }

        float cooldownSeconds = cue switch
        {
            BroodmotherEmbeddedAudioCue.Attack => _embeddedSpiderAttackCooldownSeconds.Value,
            BroodmotherEmbeddedAudioCue.Walk => _embeddedSpiderWalkCooldownSeconds.Value,
            _ => _embeddedSpiderCallCooldownSeconds.Value,
        };
        float minDistance = Math.Max(0.1f, _embeddedSpiderAudioMinDistance.Value);
        float maxDistance = Math.Max(minDistance + 0.1f, _embeddedSpiderAudioMaxDistance.Value);

        _embeddedAudioRuntime.Play(
            cue,
            null,
            position,
            _embeddedSpiderAudioVolume.Value,
            minDistance,
            maxDistance,
            cooldownSeconds,
            source);
    }

    private void LogCommand(string command, string result, string reason, Location? location = null, NpcElement? npcElement = null)
    {
        if (!_writeCommandLog.Value)
        {
            return;
        }

        try
        {
            string folder = Path.Combine(Paths.ConfigPath, PluginGuid);
            Directory.CreateDirectory(folder);
            string path = Path.Combine(folder, "broodmother-command-log.csv");
            bool writeHeader = !File.Exists(path);

            StringBuilder builder = new();
            if (writeHeader)
            {
                builder.AppendLine("utc,frame,command,result,reason,locationId,templateGuid,npcTemplateGuid,markedNotSaved,defendMode,holdPositionMode,followRange");
            }

            builder
                .Append(Csv(DateTimeOffset.UtcNow.ToString("O"))).Append(',')
                .Append(Time.frameCount).Append(',')
                .Append(Csv(command)).Append(',')
                .Append(Csv(result)).Append(',')
                .Append(Csv(reason)).Append(',')
                .Append(Csv(location?.ID ?? string.Empty)).Append(',')
                .Append(Csv(location?.Template?.GUID ?? string.Empty)).Append(',')
                .Append(Csv(npcElement?.Template?.GUID ?? string.Empty)).Append(',')
                .Append(location?.MarkedNotSaved == true ? "true" : "false").Append(',')
                .Append(_defendMode ? "true" : "false").Append(',')
                .Append(_holdPositionMode ? "true" : "false").Append(',')
                .Append(Csv(GetFollowRangeLabel(_followRange)))
                .AppendLine();

            File.AppendAllText(path, builder.ToString(), Encoding.UTF8);
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"{PluginName} command log write failed: {ex.GetType().Name}: {ex.Message}");
        }
    }

    private static bool IsHotkeyPressed(ConfigEntry<KeyCode> hotkey)
    {
        return hotkey.Value != KeyCode.None && Input.GetKeyDown(hotkey.Value);
    }

    private static void MigrateFloatConfigDefault(ConfigEntry<float> entry, float oldDefault, float newDefault)
    {
        if (Math.Abs(entry.Value - oldDefault) <= 0.0001f)
        {
            entry.Value = newDefault;
        }
    }

    private static bool TryReadCompanionHealthPercent(NpcElement npcElement, out float percent)
    {
        percent = 0f;
        try
        {
            percent = Mathf.Clamp01(npcElement.AliveStats.Health.Percentage);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static void DrawCompanionHealthBar(Rect rect, bool hasHealth, float percent)
    {
        Color previousColor = GUI.color;
        GUI.color = new Color(0.1f, 0.1f, 0.1f, 0.92f);
        GUI.DrawTexture(rect, Texture2D.whiteTexture);

        if (hasHealth)
        {
            Rect fill = new(rect.x, rect.y, rect.width * Mathf.Clamp01(percent), rect.height);
            GUI.color = new Color(0.72f, 0.08f, 0.06f, 0.95f);
            GUI.DrawTexture(fill, Texture2D.whiteTexture);
        }

        GUI.color = previousColor;
        GUI.Box(rect, GUIContent.none);
    }

    private string GetNativePromptStatusLine(Location location)
    {
        if (!_enableNativeCompanionPrompt.Value)
        {
            return "Native menu: off";
        }

        if (location == null || location.HasBeenDiscarded)
        {
            return "Native menu: missing";
        }

        if (!location.HasElement<BroodmotherCompanionCommandAction>())
        {
            return "Native menu: syncing";
        }

        return location.Interactable
            ? "Native menu: Companion active"
            : "Native menu: action attached, not interactable";
    }

    private static GUIStyle HudLabelStyle(
        int fontSize,
        FontStyle fontStyle,
        TextAnchor alignment,
        Color textColor,
        bool wordWrap = false)
    {
        GUIStyle style = new(GUI.skin.label)
        {
            fontSize = fontSize,
            fontStyle = fontStyle,
            alignment = alignment,
            wordWrap = wordWrap,
            clipping = wordWrap ? TextClipping.Clip : TextClipping.Overflow,
        };
        style.normal.textColor = textColor;
        return style;
    }

    private static ConfigDescription ManagerSetting(
        string description,
        string displaySection,
        string displayName,
        int sectionOrder,
        int order,
        AcceptableValueBase? acceptableValues = null)
    {
        return new ConfigDescription(
            description,
            acceptableValues,
            new SettingUiMetadata(displaySection, displayName, sectionOrder, order));
    }

    private static Rect ClampPanelToScreen(Rect rect)
    {
        float maxX = Math.Max(0f, Screen.width - rect.width);
        float maxY = Math.Max(0f, Screen.height - rect.height);
        rect.x = Mathf.Clamp(rect.x, 0f, maxX);
        rect.y = Mathf.Clamp(rect.y, 0f, maxY);
        return rect;
    }

    private static string FormatVector(Vector3 value)
    {
        return $"({value.x:0.###}, {value.y:0.###}, {value.z:0.###})";
    }

    private static string FormatSpiderRunnerDiagnostics(AvalonPlayMakerSpiderRunner runner, string prefix)
    {
        string normalizedPrefix = string.IsNullOrEmpty(prefix) ? string.Empty : prefix;
        return
            ";" + normalizedPrefix + "lastCommandStage=" + FormatMarkerValue(runner.LastCommandStage) +
            ";" + normalizedPrefix + "lastCommandAction=" + FormatMarkerValue(runner.LastCommandAction) +
            ";" + normalizedPrefix + "lastCommandPhase=" + FormatMarkerValue(runner.LastCommandPhase) +
            ";" + normalizedPrefix + "lastCommandOutcome=" + FormatMarkerValue(runner.LastCommandOutcome) +
            ";" + normalizedPrefix + "lastCommandReason=" + FormatMarkerValue(runner.LastCommandReason) +
            ";" + normalizedPrefix + "lastCommandPolls=" + runner.LastCommandPollCount.ToString(CultureInfo.InvariantCulture) +
            ";" + normalizedPrefix + "lastTickCompletedStages=" + runner.LastTickCompletedStages.ToString(CultureInfo.InvariantCulture);
    }

    private static string FormatDistance(float value, bool available)
    {
        return available ? value.ToString("0.###") : "unknown";
    }

    private static string FormatAngle(float value, bool available)
    {
        return available ? value.ToString("0.#") : "unknown";
    }

    private static string GetAttackDisplayName(BroodmotherSpiderAttackKind attack)
    {
        switch (attack)
        {
            case BroodmotherSpiderAttackKind.ShortBite:
                return "short bite";
            case BroodmotherSpiderAttackKind.LeapJump:
                return "leap attack";
            case BroodmotherSpiderAttackKind.Feint:
                return "feint";
            case BroodmotherSpiderAttackKind.Reposition:
                return "reposition";
            case BroodmotherSpiderAttackKind.TargetedNativeCombat:
                return "targeted native combat";
            case BroodmotherSpiderAttackKind.Hold:
            default:
                return "hold";
        }
    }

    private static string FormatMarkerValue(string value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? "none"
            : value.Replace(",", "_").Replace(" ", "_");
    }

    private static string BoolText(bool value)
    {
        return value ? "1" : "0";
    }

    private static string FormatInteractability(LocationInteractability interactability)
    {
        if (interactability == LocationInteractability.Active)
        {
            return "Active";
        }

        if (interactability == LocationInteractability.Inactive)
        {
            return "Inactive";
        }

        return interactability == LocationInteractability.Hidden ? "Hidden" : FormatMarkerValue(interactability.ToString());
    }

    private static string Csv(string value)
    {
        return "\"" + (value ?? string.Empty).Replace("\"", "\"\"") + "\"";
    }

    internal void SetPanelStatus(string status)
    {
        _panelStatus = status;
    }

    internal bool ShouldAllowHeroDamageToActiveBroodmother(
        NpcHeroSummon summon,
        HookResult<HealthElement, Damage> hook)
    {
        try
        {
            if (summon == null || hook.Value.DamageDealerPure != Hero.Current)
            {
                return false;
            }

            Location location = summon.Location;
            return location != null && IsActiveBroodmotherCompanionLocation(location);
        }
        catch
        {
            return false;
        }
    }

    private void SyncSpiderBlazeBridgeState()
    {
        if (_spiderBlazeBridge == null)
        {
            return;
        }

        bool pluginEnabled = _enabled?.Value == true;
        _spiderBlazeBridge.Enabled = pluginEnabled && _enableAdvancedSpiderCompanionAi.Value;
        _spiderBlazeBridge.PackageEnabled = pluginEnabled;
        _spiderBlazeBridge.CapabilityGranted = _enableAdvancedSpiderLeapRequests.Value;
        _spiderBlazeBridge.KillSwitchActive = !pluginEnabled;
    }

    private sealed class BroodmotherSpiderBlazeCommandExecutor : IAvalonFoASpiderBlazeCommandExecutor
    {
        private readonly Plugin owner;

        internal BroodmotherSpiderBlazeCommandExecutor(Plugin owner)
        {
            this.owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        public bool ActorExists => owner._activeBroodmother != null;

        public bool ActorFakeNull => owner._activeBroodmother == null;

        public bool ActorSceneLoaded => owner._activeBroodmother != null && !owner._activeBroodmother.HasBeenDiscarded;

        public bool ActorOwnershipCurrent => ActorSceneLoaded;

        public bool TargetExists => Hero.Current != null;

        public bool TargetFakeNull => Hero.Current == null;

        public bool TargetDestroyed => false;

        public string ActorId => owner.ActiveDefinition.Id;

        public string TargetId => Hero.Current != null ? "hero.current" : string.Empty;

        public bool Supports(AvalonFoASpiderBlazeCommand command)
        {
            _ = command;
            return false;
        }

        public AvalonFoASpiderBlazeCommandBridgeDispatch Dispatch(AvalonFoASpiderBlazeCommand command)
        {
            _ = command;
            return AvalonFoASpiderBlazeCommandBridgeDispatch.Rejected(
                AvalonFoASpiderBlazeCommandBridgeReasonCodes.ExecutorUnsupported);
        }

        public AvalonFoASpiderBlazeCommandBridgeDispatch Poll(
            AvalonFoASpiderBlazeCommand command,
            DateTimeOffset requestedAtUtc)
        {
            _ = command;
            _ = requestedAtUtc;
            return AvalonFoASpiderBlazeCommandBridgeDispatch.Rejected(
                AvalonFoASpiderBlazeCommandBridgeReasonCodes.ExecutorUnsupported);
        }

        public void Tick(DateTimeOffset nowUtc)
        {
            _ = nowUtc;
        }

        public AvalonFoASpiderBlazeCommandBridgeDispatch Cancel(AvalonFoASpiderBlazeCommand command)
        {
            _ = command;
            return AvalonFoASpiderBlazeCommandBridgeDispatch.Interrupted(
                AvalonFoASpiderBlazeCommandBridgeReasonCodes.Cancelled);
        }

        public void Cleanup(AvalonProcedureInputSnapshot input)
        {
            _ = input;
        }
    }
}

internal sealed class SettingUiMetadata
{
    internal SettingUiMetadata(string displaySection, string displayName, int sectionOrder, int order)
    {
        DisplaySection = displaySection;
        DisplayName = displayName;
        SectionOrder = sectionOrder;
        Order = order;
    }

    public string DisplaySection { get; }
    public string DisplayName { get; }
    public int SectionOrder { get; }
    public int Order { get; }
}
