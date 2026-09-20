using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using FoAModManager.Patches;
using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FoAModManager;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
[DefaultExecutionOrder(-32000)]
public sealed class Plugin : BaseUnityPlugin
{
    private const int ManagerWindowId = 10974204;
    private const float WindowScreenMargin = 24f;
    private const float MinimumWindowWidth = 1040f;
    private const float MinimumWindowHeight = 430f;
    private const float HeaderDragHeight = 72f;
    private const float HeaderDragRightReserve = 500f;
    private const float HeaderTitleMinimumWidth = 280f;
    private const float ResizeHandleSize = 28f;
    private const float PanelContentInset = 38f;
    private const float ToolbarContentLeftInset = 24f;
    private const float MainPaneHeaderTextTopInset = 8f;
    private const float MainPaneHeaderTextLeftInset = 24f;
    private const float SelectedPluginHeaderTextLeftInset = 40f;
    private const float WarningSummaryTextLeftInset = 20f;
    private const float PluginListItemTextLeftInset = 28f;
    private const float PluginListItemTextRightInset = 16f;
    private const float PluginListItemNameTopInset = 20f;
    private const float PluginListItemNameHeight = 28f;
    private const float PluginListItemMetaTopInset = 50f;
    private const float PluginListItemMetaHeight = 24f;
    private const float SidebarTitleHeight = 28f;
    private const float SelectedPluginTitleHeight = 30f;
    private const float SelectedPluginVersionHeight = 20f;
    private const string ReadabilityProfileStandard = "Standard";
    private const string ReadabilityProfileLarge = "Large";
    private const string TaintedInterfaceCoreThemePackId = "dark-fantasy-rpg-ui-toolkit";
    private const string TaintedInterfaceFantasyRpgGuiPackId = "fantasy-rpg-gui";
    private const string TaintedInterfaceHeatCompleteModernUiPackId = "heat-complete-modern-ui";
    private const string TaintedInterfaceFantasyNouveauUiPackId = "fantasy-nouveau-ui";
    private const string TaintedInterfaceTribalUiSetPackId = "tribal-ui-set";
    private const string TaintedInterfaceGameUiEmptyBoxSet4kPackId = "game-ui-empty-box-set-4k";
    private const string TaintedInterfaceFantasyRpgUiKitPackId = "fantasy-rpg-ui-kit";
    private const string DefaultTaintedInterfacePackId = TaintedInterfaceCoreThemePackId;
    private const string TaintedInterfaceApiTypeName = "TaintedInterface.TaintedInterfaceApi";
    private const float StandardReadabilityScale = 1.0f;
    private const float LargeReadabilityScale = 1.65f;
#if FOA_MOD_MANAGER_LARGE_FONT
    private const string DefaultReadabilityProfile = ReadabilityProfileLarge;
#else
    private const string DefaultReadabilityProfile = ReadabilityProfileStandard;
#endif
    private const float DefaultInstalledModsColumnWidth = 390f;
    private const float MinimumInstalledModsColumnWidth = 260f;
    private const float MaximumInstalledModsColumnWidth = 560f;
    private const string DefaultProfileName = "default";
    private const string ProfileDirectoryName = "FoAModManager";
    private const string ProfileFileExtension = ".foa-settings.tsv";
    private const string ReportDirectoryName = "reports";
    private const string ReportFilePrefix = "foa-mod-manager-warnings-";
    private const int StatusProviderLineLimit = 8;
    private const int StatusProviderTextLimit = 360;
    private const string DefaultDarkFantasyAssetRoot = "assets/dark-fantasy-ui";
    private const string EmbeddedDarkFantasyResourcePrefix = "FoAModManager.Assets.DarkFantasy.";
    private const string TaintedInterfaceFullDarkFantasyResourcePrefix = "TaintedInterface.Assets.FullUserInterface.StructureKits.Dark Fantasy UI asset\\";
    private const string LegacyAvalonModManagerGuid = "com.avalonmodmanager";
    private const string LegacyAvalonModManagerName = "Avalon Mod Manager";
    private const int ControllerProbeSummaryEntryLimit = 16;
    private const int ControllerProbePhysicalSourceEntryLimit = 4;
    private const int ControllerProbeSuppressedSourceLogInterval = 25;
    private const float ControllerCursorDefaultSpeed = 900f;
    private const float ControllerCursorDefaultDeadzone = 0.22f;
    private const float ControllerChordProbeDefaultTargetScanIntervalSeconds = 1.0f;
    private const string ControllerChordProbeDefaultModifierActions = "JoystickButton9";
    private const string ControllerChordProbeDefaultTargetActions = "UI_A,UI_Y,DPad_Up,DPad_Down,DPad_Left,DPad_Right,Next,ModDown";
    private const string ControllerActionLayerDefaultTargetAction = "JoystickButton0";
    private const string ControllerActionLayerDefaultActionId = "manager.toggle";
    private const string ControllerActionLayerDefaultTestBinding = ControllerActionLayerDefaultTargetAction + "=" + ControllerActionLayerDefaultActionId;
    private const float ControllerActionLayerDefaultRepeatSuppressSeconds = 0.25f;
    private const string ControllerActionLayerDefaultModifierActions = "JoystickButton9";
    private const uint MouseEventLeftDown = 0x0002;
    private const uint MouseEventLeftUp = 0x0004;
    private static readonly ProfilePreset[] ProfilePresets =
    {
        new("default", "Baseline"),
        new("stable", "Stable"),
        new("testing", "Testing"),
        new("streaming", "Quiet"),
        new("experimental", "Experimental")
    };
    private static readonly ControllerHotkeyChoice[] ControllerHotkeyChoices =
    {
        new(KeyCode.JoystickButton0, "A"),
        new(KeyCode.JoystickButton1, "B"),
        new(KeyCode.JoystickButton2, "X"),
        new(KeyCode.JoystickButton3, "Y"),
        new(KeyCode.JoystickButton4, "LB"),
        new(KeyCode.JoystickButton5, "RB"),
        new(KeyCode.JoystickButton6, "View"),
        new(KeyCode.JoystickButton7, "Menu"),
        new(KeyCode.JoystickButton9, "R3")
    };
    private static readonly ControllerActionTargetChoice[] ControllerActionLayerTargetChoices =
    {
        new("JoystickButton0", "A"),
        new("JoystickButton1", "B"),
        new("JoystickButton2", "X"),
        new("JoystickButton3", "Y"),
        new("JoystickButton4", "LB"),
        new("JoystickButton5", "RB"),
        new("Block", "LT"),
        new("Attack", "RT"),
        new("JoystickButton6", "View"),
        new("JoystickButton7", "Menu"),
        new("JoystickButton9", "R3"),
        new("DPad_Up", "D-Pad Up"),
        new("DPad_Down", "D-Pad Down"),
        new("DPad_Left", "D-Pad Left"),
        new("DPad_Right", "D-Pad Right"),
        new("Next", "Next"),
        new("ModDown", "ModDown")
    };
    private static readonly ControllerActionTargetChoice[] ControllerActionLayerModifierChoices =
    {
        new("JoystickButton8", "L3"),
        new("JoystickButton9", "R3"),
        new("JoystickButton4", "LB"),
        new("JoystickButton5", "RB"),
        new("JoystickButton6", "View"),
        new("JoystickButton7", "Menu"),
        new("JoystickButton3", "Y"),
        new("JoystickButton2", "X"),
        new("JoystickButton1", "B"),
        new("JoystickButton0", "A"),
        new("DPad_Up", "D-Pad Up"),
        new("DPad_Down", "D-Pad Down"),
        new("DPad_Left", "D-Pad Left"),
        new("DPad_Right", "D-Pad Right")
    };

    public const string PluginGuid = "kane.tgfoa.mod-manager";
    public const string PluginName = "FoA Mod Manager";
    public const string PluginVersion = "0.6.62";
    private static readonly TaintedInterfacePackChoice[] TaintedInterfacePackChoices =
    {
        new(TaintedInterfaceCoreThemePackId, "Dark Fantasy", "Dark Fantasy"),
        new(TaintedInterfaceFantasyRpgGuiPackId, "Fantasy RPG GUI", "Fantasy RPG"),
        new(TaintedInterfaceHeatCompleteModernUiPackId, "Heat - Complete Modern UI", "Heat"),
        new(TaintedInterfaceFantasyNouveauUiPackId, "Fantasy Nouveau UI", "Nouveau"),
        new(TaintedInterfaceTribalUiSetPackId, "Tribal UI Set", "Tribal"),
        new(TaintedInterfaceGameUiEmptyBoxSet4kPackId, "Game UI Empty Box Set 4k", "Empty Box"),
        new(TaintedInterfaceFantasyRpgUiKitPackId, "Fantasy RPG UI Kit", "RPG UI Kit")
    };

    private static readonly TaintedInterfaceManagerTextureBinding[] TaintedInterfaceManagerTextureBindings =
    {
        new("window", "panel.modal"),
        new("header", "panel.title"),
        new("panel", "panel.modal"),
        new("card", "field.backing"),
        new("field", "field.backing"),
        new("button", "button.primary"),
        new("button-hover", "button.confirm"),
        new("secondary-button", "button.secondary"),
        new("selected-button", "button.confirm"),
        new("pill", "field.backing-active"),
        new("slider-track", "progress.track"),
        new("slider-thumb", "slider.thumb")
    };

    private static readonly TaintedInterfaceManagerTextureBinding[] LineArtTaintedInterfaceManagerTextureBindings =
    {
        new("window", "panel.modal"),
        new("header", "panel.title"),
        new("panel", "panel.modal"),
        new("card", "panel.modal"),
        new("field", "field.backing"),
        new("button", "button.primary"),
        new("button-hover", "button.confirm"),
        new("secondary-button", "button.secondary"),
        new("selected-button", "button.confirm"),
        new("pill", "field.backing-active"),
        new("slider-track", "progress.track"),
        new("slider-thumb", "slider.thumb")
    };

    private static ConfigEntry<KeyCode>? _openHotkey;
    private static ConfigEntry<bool>? _showManagerConfig;
    private static ConfigEntry<bool>? _darkFantasyAssetsEnabled;
    private static ConfigEntry<string>? _darkFantasyAssetRoot;
    private static ConfigEntry<string>? _readabilityProfile;
    private static ConfigEntry<string>? _taintedInterfacePack;
    private static ConfigEntry<float>? _installedModsColumnWidth;
    private static ConfigEntry<bool>? _controllerCursorEnabled;
    private static ConfigEntry<float>? _controllerCursorSpeed;
    private static ConfigEntry<float>? _controllerCursorDeadzone;
    private static ConfigEntry<string>? _controllerCursorLeftActions;
    private static ConfigEntry<string>? _controllerCursorRightActions;
    private static ConfigEntry<string>? _controllerCursorUpActions;
    private static ConfigEntry<string>? _controllerCursorDownActions;
    private static ConfigEntry<string>? _controllerCursorPrimaryClickActions;
    private static ConfigEntry<bool>? _controllerChordProbeEnabled;
    private static ConfigEntry<string>? _controllerChordProbeModifierActions;
    private static ConfigEntry<string>? _controllerChordProbeTargetActions;
    private static ConfigEntry<int>? _controllerChordProbeMaxLogsPerSession;
    private static ConfigEntry<float>? _controllerChordProbeRepeatSuppressSeconds;
    private static ConfigEntry<float>? _controllerChordProbeTargetScanIntervalSeconds;
    private static ConfigEntry<bool>? _controllerChordProbeControllerSourcesOnly;
    private static ConfigEntry<bool>? _controllerActionLayerEnabled;
    private static ConfigEntry<bool>? _controllerActionLayerDryRun;
    private static ConfigEntry<bool>? _controllerActionLayerDispatchEnabled;
    private static ConfigEntry<string>? _controllerActionLayerModifierActions;
    private static ConfigEntry<string>? _controllerActionLayerBindings;
    private static ConfigEntry<int>? _controllerActionLayerMaxLogsPerSession;
    private static ConfigEntry<float>? _controllerActionLayerRepeatSuppressSeconds;
    private static ConfigEntry<bool>? _controllerActionLayerControllerSourcesOnly;
    private static ConfigEntry<bool>? _controllerProbeEnabled;
    private static ConfigEntry<int>? _controllerProbeMaxLogsPerSession;
    private static ConfigEntry<float>? _controllerProbeRepeatSuppressSeconds;
    private static ConfigEntry<bool>? _controllerProbeDumpActionMapOnFirstCapture;
    private static ConfigEntry<int>? _controllerProbeMaxActionMapRows;
    private static ConfigEntry<int>? _controllerProbeSummaryEveryRows;
    private static ConfigEntry<bool>? _controllerProbeControllerSourcesOnly;
    private static ConfigEntry<bool>? _controllerProbeLogFirstSeenPhysicalElementsAfterCap;
    private static Plugin? _instance;

    private readonly List<ManagedPlugin> _plugins = new();
    private readonly List<Texture2D> _ownedTextures = new();
    private readonly List<InputModuleState> _inputModuleStates = new();
    private readonly Dictionary<string, float> _controllerProbeLastLogTimes = new(StringComparer.Ordinal);
    private readonly Dictionary<string, ControllerActionRegistration> _controllerActionRegistrations = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, StatusProviderRegistration> _statusProviderRegistrations = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, float> _controllerActionLayerLastLogTimes = new(StringComparer.Ordinal);
    private readonly HashSet<string> _controllerActionLayerSuppressedActionNames = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<int> _controllerActionLayerSuppressedActionIds = new();
    private readonly Dictionary<int, ControllerProbeActionInfo> _controllerProbeActionsById = new();
    private readonly Dictionary<string, ControllerProbeActionInfo> _controllerProbeActionsByName = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, ControllerProbeCaptureSummary> _controllerProbeSummaries = new(StringComparer.Ordinal);
    private readonly HashSet<string> _controllerProbeSeenPhysicalElementKeys = new(StringComparer.Ordinal);
    private readonly Dictionary<string, float> _controllerChordProbeLastLogTimes = new(StringComparer.Ordinal);
    private readonly HashSet<string> _controllerCursorScopes = new(StringComparer.Ordinal);
    private readonly HashSet<string> _customUiScopes = new(StringComparer.Ordinal);
    private readonly HashSet<string> _worldFreezeScopes = new(StringComparer.Ordinal);
    private readonly HashSet<string> _expandedSettingSections = new(StringComparer.Ordinal);
    private readonly HashSet<KeyCode> _controllerHotkeyLayerKeys = new();

    private Harmony? _harmony;
    private UiStyles? _styles;
    private bool _stylesDirty;
    private string _lastManagerSkinPackId = DefaultTaintedInterfacePackId;
    private string _lastManagerSkinSource = "Generated";
    private int _lastManagerSkinTextureCount;
    private int _lastManagerSkinMissingCount;
    private Rect _window;
    private Vector2 _pluginScroll;
    private Vector2 _settingsScroll;
    private string? _selectedPluginKey;
    private string? _captureSettingId;
    private string _pluginSearch = string.Empty;
    private string _settingSearch = string.Empty;
    private string _profileName = DefaultProfileName;
    private readonly List<string> _profileNames = new();
    private string _dashboardProfileCoverageCacheKey = string.Empty;
    private DashboardProfileCoverage _dashboardProfileCoverage = DashboardProfileCoverage.Empty;
    private Vector2 _subPanelScroll;
    private ManagerSubPanel _activeSubPanel = ManagerSubPanel.None;
    private string _controllerActionLayerBindingDraft = string.Empty;
    private string _controllerActionLayerTargetDraft = ControllerActionLayerDefaultTargetAction;
    private string _controllerActionLayerActionDraft = ControllerActionLayerDefaultActionId;
    private bool _controllerActionLayerBindingDraftInitialized;
    private string _status = string.Empty;
    private bool _visible;
    private bool _selectedPluginIssuesExpanded;
    private bool _windowInitialized;
    private int _lastScreenWidth;
    private int _lastScreenHeight;
    private CursorLockMode _previousCursorLockState;
    private bool _previousCursorVisible;
    private float _previousTimeScale = 1f;
    private bool _cursorStateCaptured;
    private bool _inputModulesCaptured;
    private bool _worldTimeScaleCaptured;
    private bool _resizingWindow;
    private bool _windowFrameChangedThisFrame;
    private Vector2 _resizeStartMouse;
    private Rect _resizeStartWindow;
    private int _controllerProbeLogCount;
    private int _controllerProbeLastSummaryLogCount;
    private bool _controllerProbeActionMapLoaded;
    private bool _controllerProbeActionMapDumped;
    private float _controllerProbeNextActionMapLoadAttemptAt;
    private int _controllerProbeSuppressedNonControllerSourceCount;
    private bool _controllerProbeMaxLogWarningLogged;
    private object? _controllerCursorPlayer;
    private float _controllerCursorNextPlayerResolveAt;
    private bool _controllerCursorPlayerUnavailableLogged;
    private int _controllerCursorInputReadDepth;
    private int _controllerChordProbeInputReadDepth;
    private int _controllerActionLayerInputReadDepth;
    private int _controllerChordProbeLogCount;
    private int _controllerActionLayerLogCount;
    private int _controllerActionLayerModifierHeldFrame = -1;
    private bool _controllerChordProbeModifierWasHeld;
    private bool _controllerChordProbeMaxLogWarningLogged;
    private bool _controllerActionLayerMaxLogWarningLogged;
    private bool _controllerActionLayerDispatchGateWarningLogged;
    private float _controllerChordProbeNextTargetScanAt;
    private string _controllerChordProbeLastModifierAction = "<none>";
    private int _controllerActionLayerSuppressionFrame = -1;
    private Vector2 _controllerCursorPosition;
    private bool _controllerCursorPositionInitialized;
    private bool _controllerCursorMouseButtonDown;
    private static float _readabilityScale = GetReadabilityScale(DefaultReadabilityProfile);
    private static string _appliedReadabilityProfile = DefaultReadabilityProfile;

    private static KeyCode OpenHotkey => _openHotkey?.Value ?? KeyCode.F10;

    private static string BuildProfileDisplayName => GetReadabilityProfileDisplayName(_appliedReadabilityProfile);

    private static float Px(float value)
    {
        return Mathf.Round(value * _readabilityScale);
    }

    private static int FontPx(int value)
    {
        return Mathf.Max(1, Mathf.RoundToInt(value * _readabilityScale));
    }

    private static RectOffset Pad(int left, int right, int top, int bottom)
    {
        return new RectOffset(FontPx(left), FontPx(right), FontPx(top), FontPx(bottom));
    }

    private static void ApplyStartupReadabilityProfile(string? profile)
    {
        _appliedReadabilityProfile = NormalizeReadabilityProfile(profile);
        _readabilityScale = GetReadabilityScale(_appliedReadabilityProfile);
    }

    private static string NormalizeReadabilityProfile(string? profile)
    {
        if (string.Equals(profile, ReadabilityProfileStandard, StringComparison.OrdinalIgnoreCase))
        {
            return ReadabilityProfileStandard;
        }

        if (string.Equals(profile, ReadabilityProfileLarge, StringComparison.OrdinalIgnoreCase))
        {
            return ReadabilityProfileLarge;
        }

        return DefaultReadabilityProfile;
    }

    private static float GetReadabilityScale(string profile)
    {
        return string.Equals(profile, ReadabilityProfileLarge, StringComparison.OrdinalIgnoreCase)
            ? LargeReadabilityScale
            : StandardReadabilityScale;
    }

    private static string GetReadabilityProfileDisplayName(string profile)
    {
        return string.Equals(profile, ReadabilityProfileLarge, StringComparison.OrdinalIgnoreCase)
            ? "larger-font accessibility"
            : "standard-size";
    }

    internal static void ShowManager()
    {
        _instance?.OpenManager();
    }

    internal static void HideManager()
    {
        _instance?.CloseManager();
    }

    internal static void RefreshManager()
    {
        _instance?.RefreshCatalog();
    }

    internal static bool IsManagerVisible => _instance?._visible == true;

    internal static bool IsModUiInputOwned => _instance?.HasModUiInputContext == true;

    internal static bool IsCustomUiScopeActive => _instance?._customUiScopes.Count > 0;

    internal static bool IsInternalControllerInputReadActive => _instance?._controllerCursorInputReadDepth > 0 ||
        _instance?._controllerChordProbeInputReadDepth > 0 ||
        _instance?._controllerActionLayerInputReadDepth > 0;

    internal static bool IsControllerCursorInputReadActive => IsInternalControllerInputReadActive;

    internal static bool IsControllerCursorActive => _instance?.IsControllerCursorActiveInstance == true;

    internal static string GetTaintedInterfaceUiPackId()
    {
        return _instance?.GetTaintedInterfaceUiPackIdInstance() ?? DefaultTaintedInterfacePackId;
    }

    internal static bool ShouldSuppressControllerActionLayerButton(string actionName)
    {
        return _instance?.ShouldSuppressControllerActionLayerButtonInstance(actionName) == true;
    }

    internal static bool ShouldSuppressControllerActionLayerButton(int actionId)
    {
        return _instance?.ShouldSuppressControllerActionLayerButtonInstance(actionId) == true;
    }

    internal static bool ShouldBlockControllerActionLayerButton(string actionName)
    {
        return _instance?.ShouldBlockControllerActionLayerButtonInstance(actionName) == true;
    }

    internal static bool ShouldBlockControllerActionLayerButton(int actionId)
    {
        return _instance?.ShouldBlockControllerActionLayerButtonInstance(actionId) == true;
    }

    internal static bool ShouldBlockControllerHotkeyKey(KeyCode keyCode)
    {
        return _instance?.ShouldBlockControllerHotkeyKeyInstance(keyCode) == true;
    }

    internal static void SetControllerCursorScope(string ownerId, bool active)
    {
        _instance?.SetControllerCursorScopeInstance(ownerId, active);
    }

    internal static void SetCustomUiScope(string ownerId, bool active, bool freezeWorld)
    {
        _instance?.SetCustomUiScopeInstance(ownerId, active, freezeWorld);
    }

    internal static bool RegisterControllerAction(string actionId, string displayName, Action callback)
    {
        return RegisterControllerAction(actionId, displayName, "General", string.Empty, callback);
    }

    internal static bool RegisterControllerAction(string actionId, string displayName, string category, string description, Action callback)
    {
        return _instance?.RegisterControllerActionInstance(actionId, displayName, category, description, callback) == true;
    }

    internal static bool UnregisterControllerAction(string actionId)
    {
        return _instance?.UnregisterControllerActionInstance(actionId) == true;
    }

    internal static bool RegisterStatusProvider(string providerId, string displayName, Func<FoAModStatusSnapshot> snapshotProvider)
    {
        return RegisterStatusProvider(providerId, displayName, "General", string.Empty, snapshotProvider);
    }

    internal static bool RegisterStatusProvider(string providerId, string displayName, string category, string description, Func<FoAModStatusSnapshot> snapshotProvider)
    {
        return _instance?.RegisterStatusProviderInstance(providerId, displayName, category, description, snapshotProvider) == true;
    }

    internal static bool UnregisterStatusProvider(string providerId)
    {
        return _instance?.UnregisterStatusProviderInstance(providerId) == true;
    }

    internal static void RecordControllerProbeButtonResult(string bindingKind, string bindingValue, string methodName, object? playerInstance)
    {
        _instance?.RecordControllerProbeButtonResultInstance(bindingKind, bindingValue, methodName, playerInstance);
    }

    private void Awake()
    {
        _instance = this;

        _openHotkey = Config.Bind("General", "OpenHotkey", KeyCode.F10, "Open or close the FoA Mod Manager.");
        _showManagerConfig = Config.Bind("General", "ShowManagerConfig", false, "Show the FoA Mod Manager's own settings in the manager.");
        _darkFantasyAssetsEnabled = Config.Bind(
            "Display",
            "UseDarkFantasyUiAssets",
            true,
            "Load curated dark-fantasy PNG UI textures from Display.DarkFantasyAssetRoot when present. Missing assets fall back to generated manager textures.");
        _darkFantasyAssetRoot = Config.Bind(
            "Display",
            "DarkFantasyAssetRoot",
            DefaultDarkFantasyAssetRoot,
            "Relative path under this plugin folder containing the approved curated dark-fantasy UI PNG subset. Absolute paths and parent traversal are refused.");
        _readabilityProfile = Config.Bind(
            "Display",
            "ReadabilityProfile",
            DefaultReadabilityProfile,
            new ConfigDescription(
                "Fixed manager UI size profile. Standard uses the lower-font layout; Large uses the accessibility layout. Changes apply after restarting the game.",
                new AcceptableValueList<string>(ReadabilityProfileStandard, ReadabilityProfileLarge)));
        ApplyStartupReadabilityProfile(_readabilityProfile.Value);
        _taintedInterfacePack = Config.Bind(
            "Custom UI",
            "TaintedInterfacePack",
            DefaultTaintedInterfacePackId,
            new ConfigDescription(
                "Selected Tainted Interface UI pack ID for downstream mods that use FoA Mod Manager as their pack setting owner.",
                new AcceptableValueList<string>(TaintedInterfacePackChoices.Select(choice => choice.Id).ToArray())));
        _installedModsColumnWidth = Config.Bind(
            "Display",
            "InstalledModsColumnWidth",
            DefaultInstalledModsColumnWidth,
            new ConfigDescription(
                "Width in pixels for the left Installed Mods column. Increase this when mod names wrap or feel cramped.",
                new AcceptableValueRange<float>(MinimumInstalledModsColumnWidth, MaximumInstalledModsColumnWidth)));
        _controllerCursorEnabled = Config.Bind(
            "ControllerCursor",
            "Enabled",
            true,
            "Allow controller input to move and click the mouse cursor while FoA Mod Manager or an opted-in custom mod screen owns UI input.");
        _controllerCursorSpeed = Config.Bind(
            "ControllerCursor",
            "Speed",
            ControllerCursorDefaultSpeed,
            new ConfigDescription(
                "Controller cursor speed in screen pixels per second.",
                new AcceptableValueRange<float>(120f, 2400f)));
        _controllerCursorDeadzone = Config.Bind(
            "ControllerCursor",
            "Deadzone",
            ControllerCursorDefaultDeadzone,
            new ConfigDescription(
                "Ignore controller cursor axis values below this amount.",
                new AcceptableValueRange<float>(0f, 0.75f)));
        _controllerCursorLeftActions = Config.Bind(
            "ControllerCursor",
            "LeftActions",
            "Navi_Left,UI_Left,Left",
            "Comma-separated Rewired action names that move the UI cursor left.");
        _controllerCursorRightActions = Config.Bind(
            "ControllerCursor",
            "RightActions",
            "Navi_Right,UI_Right,Right",
            "Comma-separated Rewired action names that move the UI cursor right.");
        _controllerCursorUpActions = Config.Bind(
            "ControllerCursor",
            "UpActions",
            "Navi_Up,UI_Up,Up",
            "Comma-separated Rewired action names that move the UI cursor up.");
        _controllerCursorDownActions = Config.Bind(
            "ControllerCursor",
            "DownActions",
            "Navi_Down,UI_Down,Down",
            "Comma-separated Rewired action names that move the UI cursor down.");
        _controllerCursorPrimaryClickActions = Config.Bind(
            "ControllerCursor",
            "PrimaryClickActions",
            "UI_A,Submit,Confirm,Interact",
            "Comma-separated Rewired action names that hold the left mouse button for UI cursor clicks and dragging. Unity JoystickButton0 is also accepted as a fallback.");
        _controllerChordProbeEnabled = Config.Bind(
            "ControllerChordProbe",
            "Enabled",
            false,
            "Enable diagnostic-only logging of controller modifier + target button chord states. This does not dispatch mod actions or suppress vanilla input.");
        _controllerChordProbeModifierActions = Config.Bind(
            "ControllerChordProbe",
            "ModifierActions",
            ControllerChordProbeDefaultModifierActions,
            "Comma-separated Rewired action names or Unity KeyCode names treated as the diagnostic modifier. Default is R3 / JoystickButton9.");
        _controllerChordProbeTargetActions = Config.Bind(
            "ControllerChordProbe",
            "TargetActions",
            ControllerChordProbeDefaultTargetActions,
            "Comma-separated Rewired action names to test while the modifier is held. These are diagnostic candidates only and do not trigger actions.");
        _controllerChordProbeMaxLogsPerSession = Config.Bind(
            "ControllerChordProbe",
            "MaxLogsPerSession",
            80,
            new ConfigDescription(
                "Maximum controller chord probe rows per game session. Set to 0 to keep the probe armed but silent.",
                new AcceptableValueRange<int>(0, 1000)));
        _controllerChordProbeRepeatSuppressSeconds = Config.Bind(
            "ControllerChordProbe",
            "RepeatSuppressSeconds",
            0.5f,
            new ConfigDescription(
                "Suppress repeated chord probe rows for this many seconds.",
                new AcceptableValueRange<float>(0f, 10f)));
        _controllerChordProbeTargetScanIntervalSeconds = Config.Bind(
            "ControllerChordProbe",
            "TargetScanIntervalSeconds",
            ControllerChordProbeDefaultTargetScanIntervalSeconds,
            new ConfigDescription(
                "While the diagnostic modifier is held, log compact target-state scan rows at this interval. Press/down states still log immediately.",
                new AcceptableValueRange<float>(0.1f, 10f)));
        _controllerChordProbeControllerSourcesOnly = Config.Bind(
            "ControllerChordProbe",
            "ControllerSourcesOnly",
            true,
            "Suppress chord probe rows unless the sampled action resolves to a controller-like source or last-active controller.");
        _controllerActionLayerEnabled = Config.Bind(
            "ControllerActionLayer",
            "Enabled",
            false,
            "Enable the default-off controller mod-action shortcut layer. No shortcuts are shipped by default; users can add their own bindings in FoA Mod Manager.");
        _controllerActionLayerDryRun = Config.Bind(
            "ControllerActionLayer",
            "DryRun",
            true,
            "Log controller shortcut routes without invoking actions. User-facing shortcuts require this to be false and DispatchEnabled=true.");
        _controllerActionLayerDispatchEnabled = Config.Bind(
            "ControllerActionLayer",
            "DispatchEnabled",
            false,
            "Allow registered controller shortcut actions to run. Requires Enabled=true and DryRun=false; only exact mapped target inputs are consumed.");
        _controllerActionLayerModifierActions = Config.Bind(
            "ControllerActionLayer",
            "ModifierActions",
            ControllerActionLayerDefaultModifierActions,
            "Comma-separated Rewired action names or Unity KeyCode names treated as controller shortcut modifiers. Default starts as R3 / JoystickButton9, and users can change it in the Controller panel.");
        _controllerActionLayerBindings = Config.Bind(
            "ControllerActionLayer",
            "Bindings",
            string.Empty,
            "Semicolon-separated targetAction=registeredActionId rows, for example UI_A=manager.toggle. Default empty means no public chord bindings.");
        _controllerActionLayerMaxLogsPerSession = Config.Bind(
            "ControllerActionLayer",
            "MaxLogsPerSession",
            80,
            new ConfigDescription(
                "Maximum controller action-layer route rows per game session. Set to 0 to keep the layer armed but silent.",
                new AcceptableValueRange<int>(0, 1000)));
        _controllerActionLayerRepeatSuppressSeconds = Config.Bind(
            "ControllerActionLayer",
            "RepeatSuppressSeconds",
            ControllerActionLayerDefaultRepeatSuppressSeconds,
            new ConfigDescription(
                "Suppress repeated action-layer route rows for this many seconds.",
                new AcceptableValueRange<float>(0f, 10f)));
        _controllerActionLayerControllerSourcesOnly = Config.Bind(
            "ControllerActionLayer",
            "ControllerSourcesOnly",
            true,
            "Suppress action-layer route rows unless the sampled action resolves to a controller-like source or last-active controller.");
        _controllerProbeEnabled = Config.Bind(
            "ControllerProbe",
            "Enabled",
            false,
            "Enable diagnostic-only logging of Rewired button actions that return true. This does not remap controls or trigger mod actions.");
        _controllerProbeMaxLogsPerSession = Config.Bind(
            "ControllerProbe",
            "MaxLogsPerSession",
            120,
            new ConfigDescription(
                "Maximum controller probe log rows per game session. Set to 0 to keep the probe armed but silent.",
                new AcceptableValueRange<int>(0, 1000)));
        _controllerProbeRepeatSuppressSeconds = Config.Bind(
            "ControllerProbe",
            "RepeatSuppressSeconds",
            0.75f,
            new ConfigDescription(
                "Suppress repeated probe rows for the same Rewired method/action for this many seconds.",
                new AcceptableValueRange<float>(0f, 10f)));
        _controllerProbeDumpActionMapOnFirstCapture = Config.Bind(
            "ControllerProbe",
            "DumpActionMapOnFirstCapture",
            true,
            "Dump Rewired action ID/name rows once when the controller probe first captures input. This is diagnostic-only and does not remap controls.");
        _controllerProbeMaxActionMapRows = Config.Bind(
            "ControllerProbe",
            "MaxActionMapRows",
            400,
            new ConfigDescription(
                "Maximum Rewired action ID/name rows to dump when DumpActionMapOnFirstCapture is enabled. Set to 0 to skip full action-map rows.",
                new AcceptableValueRange<int>(0, 1000)));
        _controllerProbeSummaryEveryRows = Config.Bind(
            "ControllerProbe",
            "SummaryEveryRows",
            40,
            new ConfigDescription(
                "Write a compact controller probe summary after this many captured rows. Set to 0 to only summarize when the cap is reached.",
                new AcceptableValueRange<int>(0, 250)));
        _controllerProbeControllerSourcesOnly = Config.Bind(
            "ControllerProbe",
            "ControllerSourcesOnly",
            true,
            "Suppress Mouse/Keyboard controller-probe rows so the capped diagnostics are reserved for joystick/custom-controller source proof.");
        _controllerProbeLogFirstSeenPhysicalElementsAfterCap = Config.Bind(
            "ControllerProbe",
            "LogFirstSeenPhysicalElementsAfterCap",
            true,
            "After MaxLogsPerSession is reached, still log the first row for each newly seen controller physical element. This is diagnostic-only and helps complete controller coverage without raising existing configs.");

        MigrateControllerLayerModifierDefaults();
        RebuildControllerHotkeyLayerKeysFromConfigFiles();
        RegisterManagerControllerActions();

        _harmony = new Harmony(PluginGuid);
        MenuButtonPatch.Apply(_harmony, Logger);
        GameInputPatch.Apply(_harmony, Logger);
        NativeAudioSettingsPatch.Apply(_harmony, Logger);

        Logger.LogInfo($"{PluginName} {PluginVersion} ({BuildProfileDisplayName}) loaded. Press {OpenHotkey} or use the {PluginName} menu button. taintedInterfacePack={GetTaintedInterfaceUiPackId()}");
    }

    private void Update()
    {
        NativeAudioSettingsPatch.Tick();

        KeyCode hotkey = OpenHotkey;
        if (hotkey != KeyCode.None && Input.GetKeyDown(hotkey))
        {
            ToggleManager();
        }

        if (HasModUiInputContext)
        {
            EnsureModUiInputCapture();
            UpdateControllerCursor();
            Input.ResetInputAxes();
        }
        else
        {
            UpdateControllerChordProbe();
            UpdateControllerActionLayer();
        }
    }

    private void LateUpdate()
    {
        if (HasModUiInputContext)
        {
            EnsureModUiInputCapture();
            Input.ResetInputAxes();
        }
    }

    private void OpenManager()
    {
        _visible = true;
        _activeSubPanel = ManagerSubPanel.None;
        _selectedPluginIssuesExpanded = false;
        _subPanelScroll = Vector2.zero;
        CaptureCursorForManager();
        CaptureGameInputForManager();
        RefreshCatalog();
        RefreshProfileNames();
        EnsureWindowPlacement(force: !_windowInitialized);
    }

    private void ToggleManager()
    {
        if (_visible)
        {
            CloseManager();
            return;
        }

        OpenManager();
    }

    private void OnGUI()
    {
        if (!_visible)
        {
            return;
        }

        EnsureCursorForManager();
        EnsureGameInputBlocked();
        EnsureStyles();
        EnsureWindowPlacement(force: false);

        GUI.depth = -1001;
        _windowFrameChangedThisFrame = false;
        Rect drawnWindow = GUI.Window(ManagerWindowId, _window, DrawWindow, GUIContent.none, _styles!.Window);
        _window = ClampWindowToScreen(_windowFrameChangedThisFrame ? _window : drawnWindow);
    }

    private void DrawWindow(int windowId)
    {
        HandleKeyCaptureEvent();
        HandleWindowShortcuts();

        GUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        DrawHeader();
        DrawManagerToolbar();
        DrawActiveSubPanel();

        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        DrawPluginList();
        DrawSelectedPluginSettings();
        GUILayout.EndHorizontal();

        DrawFooter();
        GUILayout.EndVertical();

        DrawWindowFrameControls();

        Event current = Event.current;
        if (current.type == EventType.MouseDown || current.type == EventType.MouseDrag || current.type == EventType.MouseUp)
        {
            current.Use();
        }
    }

    private void DrawHeader()
    {
        int optionCount = _plugins.Sum(plugin => plugin.Settings.Count);

        GUILayout.BeginHorizontal(_styles!.Header, GUILayout.ExpandWidth(true));
        GUILayout.BeginVertical(GUILayout.MinWidth(Px(HeaderTitleMinimumWidth)), GUILayout.ExpandWidth(true));
        GUILayout.Label(PluginName, _styles.Title);
        GUILayout.Label("Tune installed mods without leaving the game.", _styles.Subtitle);
        GUILayout.EndVertical();

        GUILayout.FlexibleSpace();
        GUILayout.Label($"{FormatCount(_plugins.Count, "mod")}  |  {FormatCount(optionCount, "option")}", _styles.HeaderMeta, GUILayout.Width(Px(180f)));
        GUILayout.Label(OpenHotkey.ToString(), _styles.HotkeyPill, GUILayout.Width(Px(84f)), GUILayout.Height(Px(34f)));

        if (GUILayout.Button("Tall", _styles.HeaderButton, GUILayout.Width(Px(78f)), GUILayout.Height(Px(34f))))
        {
            GrowWindowHeight();
        }

        if (GUILayout.Button("Custom UI", _styles.HeaderButton, GUILayout.Width(Px(128f)), GUILayout.Height(Px(34f))))
        {
            ToggleSubPanel(ManagerSubPanel.CustomUi);
        }

        if (GUILayout.Button("Refresh", _styles.HeaderButton, GUILayout.Width(Px(104f)), GUILayout.Height(Px(34f))))
        {
            RefreshCatalog();
        }

        if (GUILayout.Button("Close", _styles.HeaderButton, GUILayout.Width(Px(88f)), GUILayout.Height(Px(34f))))
        {
            CloseManager();
        }

        GUILayout.EndHorizontal();
        GUILayout.Space(Px(10f));
    }

    private void DrawManagerToolbar()
    {
        int issueCount = _plugins.Sum(plugin => plugin.Issues.Count);

        GUILayout.BeginHorizontal(_styles!.Footer, GUILayout.ExpandWidth(true));
        GUILayout.Space(Px(ToolbarContentLeftInset));
        GUILayout.Label("Profile", _styles.FooterText, GUILayout.Width(Px(58f)), GUILayout.Height(Px(30f)));
        _profileName = GUILayout.TextField(_profileName, _styles.ValueField, GUILayout.Width(Px(150f)), GUILayout.Height(Px(30f)));

        if (GUILayout.Button("Save", _styles.SecondaryButton, GUILayout.Width(Px(62f)), GUILayout.Height(Px(30f))))
        {
            SaveSelectedProfile();
        }

        if (GUILayout.Button("Load", _styles.SecondaryButton, GUILayout.Width(Px(62f)), GUILayout.Height(Px(30f))))
        {
            LoadSelectedProfile();
        }

        DrawSubPanelButton("Dashboard", ManagerSubPanel.Dashboard, 106f);
        DrawSubPanelButton("Saved", ManagerSubPanel.Saved, 68f);
        DrawSubPanelButton("Presets", ManagerSubPanel.Presets, 78f);
        DrawSubPanelButton("Profiles", ManagerSubPanel.Profiles, 78f);
        DrawSubPanelButton("Controller", ManagerSubPanel.Commands, 108f);
        DrawSubPanelButton("Custom UI", ManagerSubPanel.CustomUi, 108f);
        DrawSubPanelButton("Status", ManagerSubPanel.Status, 86f);
        DrawSubPanelButton(issueCount == 0 ? "Warnings" : $"Warnings {issueCount}", ManagerSubPanel.Warnings, 112f);

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Space(Px(10f));
    }

    private void DrawSubPanelButton(string label, ManagerSubPanel panel, float width)
    {
        GUIStyle style = _activeSubPanel == panel ? _styles!.ActionButton : _styles!.SecondaryButton;
        if (GUILayout.Button(label, style, GUILayout.Width(Px(width)), GUILayout.Height(Px(30f))))
        {
            ToggleSubPanel(panel);
        }
    }

    private void ToggleSubPanel(ManagerSubPanel panel)
    {
        _activeSubPanel = _activeSubPanel == panel ? ManagerSubPanel.None : panel;
        _subPanelScroll = Vector2.zero;

        if (_activeSubPanel == ManagerSubPanel.Saved ||
            _activeSubPanel == ManagerSubPanel.Presets ||
            _activeSubPanel == ManagerSubPanel.Profiles)
        {
            RefreshProfileNames();
        }

        if (_activeSubPanel == ManagerSubPanel.Commands)
        {
            ResetControllerCommandDrafts();
        }
    }

    private void DrawActiveSubPanel()
    {
        if (_activeSubPanel == ManagerSubPanel.None)
        {
            return;
        }

        bool tallPanel = _activeSubPanel == ManagerSubPanel.Commands ||
            _activeSubPanel == ManagerSubPanel.Dashboard ||
            _activeSubPanel == ManagerSubPanel.CustomUi ||
            _activeSubPanel == ManagerSubPanel.Status;
        float panelMaxHeight = tallPanel ? 320f : 260f;
        float scrollHeight = tallPanel ? 245f : 185f;
        GUILayout.BeginVertical(_styles!.SettingCard, GUILayout.MinHeight(Px(128f)), GUILayout.MaxHeight(Px(panelMaxHeight)), GUILayout.ExpandWidth(true));
        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
        GUILayout.Space(Px(PanelContentInset));
        GUILayout.Label(GetSubPanelTitle(_activeSubPanel), _styles.PanelTitle);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Close Panel", _styles.SecondaryButton, GUILayout.Width(Px(120f)), GUILayout.Height(Px(30f))))
        {
            _activeSubPanel = ManagerSubPanel.None;
        }

        GUILayout.EndHorizontal();
        GUILayout.Space(Px(6f));

        _subPanelScroll = GUILayout.BeginScrollView(_subPanelScroll, GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.Height(Px(scrollHeight)), GUILayout.ExpandWidth(true));
        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
        GUILayout.Space(Px(PanelContentInset));
        GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
        switch (_activeSubPanel)
        {
            case ManagerSubPanel.Dashboard:
                DrawDashboardPanel();
                break;
            case ManagerSubPanel.Saved:
                DrawSavedProfilesPanel();
                break;
            case ManagerSubPanel.Presets:
                DrawPresetsPanel();
                break;
            case ManagerSubPanel.Profiles:
                DrawProfilesPanel();
                break;
            case ManagerSubPanel.Commands:
                DrawCommandsPanel();
                break;
            case ManagerSubPanel.CustomUi:
                DrawCustomUiPanel();
                break;
            case ManagerSubPanel.Status:
                DrawStatusPanel();
                break;
            case ManagerSubPanel.Warnings:
                DrawWarningsPanel();
                break;
        }

        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
        GUILayout.EndScrollView();
        GUILayout.EndVertical();
        GUILayout.Space(Px(10f));
    }

    private static string GetSubPanelTitle(ManagerSubPanel panel)
    {
        return panel switch
        {
            ManagerSubPanel.Dashboard => "Selected Mod Dashboard",
            ManagerSubPanel.Saved => "Saved Profiles",
            ManagerSubPanel.Presets => "Preset Slots",
            ManagerSubPanel.Profiles => "Profile Tools",
            ManagerSubPanel.Commands => "Controller Shortcuts",
            ManagerSubPanel.CustomUi => "Custom UI",
            ManagerSubPanel.Status => "Status Providers",
            ManagerSubPanel.Warnings => "Warnings",
            _ => string.Empty
        };
    }

    private void DrawDashboardPanel()
    {
        UiStyles styles = _styles!;
        ManagedPlugin? selected = GetSelectedPlugin();
        if (selected == null)
        {
            GUILayout.Label("No adjustable mods found.", styles.EmptyState, GUILayout.MinHeight(Px(72f)));
            return;
        }

        ControllerActionRegistration[] actions = GetDashboardControllerActions(selected);
        StatusProviderRegistration[] providers = GetDashboardStatusProviders(selected);
        DashboardCustomUiOwner[] customUiOwners = GetDashboardCustomUiOwners(selected);
        DashboardProfileCoverage coverage = GetDashboardProfileCoverage(selected);
        int problemCount = selected.Issues.Count(issue => issue.IsBlocking);
        int warningCount = selected.Issues.Count - problemCount;
        int categoryCount = selected.Settings
            .Select(setting => setting.DisplaySection)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();

        GUILayout.Label(selected.Name, styles.SectionLabel);
        GUILayout.Label($"v{selected.Version}  |  {selected.Guid}", styles.PluginDetail);
        GUILayout.Space(Px(8f));

        GUILayout.BeginHorizontal();
        DrawDashboardStatusCell("Options", selected.Settings.Count.ToString(CultureInfo.InvariantCulture), selected.Settings.Count > 0);
        DrawDashboardStatusCell("Categories", categoryCount.ToString(CultureInfo.InvariantCulture), categoryCount > 0);
        DrawDashboardStatusCell("Warnings", $"{problemCount.ToString(CultureInfo.InvariantCulture)} / {warningCount.ToString(CultureInfo.InvariantCulture)}", selected.Issues.Count > 0);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        DrawDashboardStatusCell("Actions", actions.Length.ToString(CultureInfo.InvariantCulture), actions.Length > 0);
        DrawDashboardStatusCell("Status", providers.Length.ToString(CultureInfo.InvariantCulture), providers.Length > 0);
        DrawDashboardStatusCell("UI Owners", customUiOwners.Length.ToString(CultureInfo.InvariantCulture), customUiOwners.Length > 0);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.Space(Px(12f));
        DrawDashboardWarnings(selected);
        GUILayout.Space(Px(12f));
        DrawDashboardActions(actions);
        GUILayout.Space(Px(12f));
        DrawDashboardStatusSnapshots(providers);
        GUILayout.Space(Px(12f));
        DrawDashboardCustomUiOwnership(customUiOwners);
        GUILayout.Space(Px(12f));
        DrawDashboardProfileCoverage(coverage);
        GUILayout.Space(Px(12f));
        DrawDashboardConfigCounts(selected);
    }

    private ManagedPlugin? GetSelectedPlugin()
    {
        return _plugins.FirstOrDefault(plugin => plugin.SelectionKey == _selectedPluginKey);
    }

    private void DrawDashboardWarnings(ManagedPlugin selected)
    {
        UiStyles styles = _styles!;
        GUILayout.BeginHorizontal();
        GUILayout.Label("Warnings", styles.SectionLabel);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Open Warnings", styles.SecondaryButton, GUILayout.Width(Px(132f)), GUILayout.Height(Px(28f))))
        {
            ToggleSubPanel(ManagerSubPanel.Warnings);
        }

        GUILayout.EndHorizontal();

        if (selected.Issues.Count == 0)
        {
            GUILayout.Label("No warnings are attached to this mod.", styles.SettingDescription);
            return;
        }

        foreach (ManagerIssue issue in selected.Issues
            .OrderBy(issue => issue.IsBlocking ? 0 : 1)
            .ThenBy(issue => issue.Category, StringComparer.OrdinalIgnoreCase)
            .ThenBy(issue => issue.Message, StringComparer.OrdinalIgnoreCase)
            .Take(4))
        {
            GUILayout.Label($"{issue.Severity}: {issue.Message}", issue.IsBlocking ? styles.DisabledLabel : styles.FooterText);
            if (!string.IsNullOrWhiteSpace(issue.Detail))
            {
                GUILayout.Label(issue.Detail, styles.SettingDescription);
            }
        }

        if (selected.Issues.Count > 4)
        {
            GUILayout.Label($"{selected.Issues.Count - 4} more warning rows in the Warnings panel.", styles.PluginDetail);
        }
    }

    private void DrawDashboardActions(ControllerActionRegistration[] actions)
    {
        UiStyles styles = _styles!;
        GUILayout.BeginHorizontal();
        GUILayout.Label("Registered Actions", styles.SectionLabel);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Open Controller", styles.SecondaryButton, GUILayout.Width(Px(138f)), GUILayout.Height(Px(28f))))
        {
            ToggleSubPanel(ManagerSubPanel.Commands);
        }

        GUILayout.EndHorizontal();

        if (actions.Length == 0)
        {
            GUILayout.Label("No registered controller actions matched this mod.", styles.SettingDescription);
            return;
        }

        foreach (ControllerActionRegistration action in actions.Take(5))
        {
            GUILayout.BeginVertical();
            GUILayout.Label(action.DisplayName, styles.SettingName);
            GUILayout.Label($"{action.Category}  |  {action.ActionId}", styles.PluginDetail);
            if (action.Description.Length > 0)
            {
                GUILayout.Label(action.Description, styles.SettingDescription);
            }

            GUILayout.EndVertical();
        }

        if (actions.Length > 5)
        {
            GUILayout.Label($"{actions.Length - 5} more matched actions in the Controller panel.", styles.PluginDetail);
        }
    }

    private void DrawDashboardStatusSnapshots(StatusProviderRegistration[] providers)
    {
        UiStyles styles = _styles!;
        GUILayout.BeginHorizontal();
        GUILayout.Label("Status Snapshot", styles.SectionLabel);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Open Status", styles.SecondaryButton, GUILayout.Width(Px(120f)), GUILayout.Height(Px(28f))))
        {
            ToggleSubPanel(ManagerSubPanel.Status);
        }

        GUILayout.EndHorizontal();

        if (providers.Length == 0)
        {
            GUILayout.Label("No status providers matched this mod.", styles.SettingDescription);
            return;
        }

        foreach (StatusProviderRegistration provider in providers.Take(3))
        {
            DrawStatusProviderRow(provider);
            GUILayout.Space(Px(8f));
        }

        if (providers.Length > 3)
        {
            GUILayout.Label($"{providers.Length - 3} more matched providers in the Status panel.", styles.PluginDetail);
        }
    }

    private void DrawDashboardCustomUiOwnership(DashboardCustomUiOwner[] owners)
    {
        UiStyles styles = _styles!;
        GUILayout.BeginHorizontal();
        GUILayout.Label("Custom UI Ownership", styles.SectionLabel);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Open Custom UI", styles.SecondaryButton, GUILayout.Width(Px(140f)), GUILayout.Height(Px(28f))))
        {
            ToggleSubPanel(ManagerSubPanel.CustomUi);
        }

        GUILayout.EndHorizontal();

        if (owners.Length == 0)
        {
            GUILayout.Label("No active custom UI or controller cursor scope matched this mod.", styles.SettingDescription);
            return;
        }

        foreach (DashboardCustomUiOwner owner in owners)
        {
            DrawCustomUiScopeRow(owner.OwnerId, owner.ScopeType, owner.Mode);
        }
    }

    private void DrawDashboardProfileCoverage(DashboardProfileCoverage coverage)
    {
        UiStyles styles = _styles!;
        GUILayout.BeginHorizontal();
        GUILayout.Label("Profile Coverage", styles.SectionLabel);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Open Profiles", styles.SecondaryButton, GUILayout.Width(Px(126f)), GUILayout.Height(Px(28f))))
        {
            ToggleSubPanel(ManagerSubPanel.Profiles);
        }

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label(coverage.ProfileName, styles.ValuePill, GUILayout.Width(Px(170f)), GUILayout.Height(Px(28f)));
        GUILayout.Label(coverage.Exists ? "Saved profile" : "Not saved", coverage.Exists ? styles.SavedPill : styles.TypePill, GUILayout.Width(Px(132f)), GUILayout.Height(Px(28f)));
        GUILayout.Label($"{coverage.MatchedSettings.ToString(CultureInfo.InvariantCulture)} / {coverage.TotalSettings.ToString(CultureInfo.InvariantCulture)} options", styles.PluginDetail, GUILayout.Width(Px(190f)));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        if (coverage.Error.Length > 0)
        {
            GUILayout.Label(coverage.Error, styles.DisabledLabel);
            return;
        }

        GUILayout.Label($"File: {coverage.ProfileFileName}", styles.SettingDescription);
        GUILayout.Label($"Rows for this mod: {coverage.OwnerRows.ToString(CultureInfo.InvariantCulture)}  |  Unknown rows: {coverage.UnknownRows.ToString(CultureInfo.InvariantCulture)}  |  Invalid rows: {coverage.InvalidRows.ToString(CultureInfo.InvariantCulture)}", styles.PluginDetail);
    }

    private void DrawDashboardConfigCounts(ManagedPlugin selected)
    {
        UiStyles styles = _styles!;
        int liveCount = selected.Settings.Count(setting => setting.Entry != null);
        int fileBackedCount = selected.Settings.Count(setting => setting.FileEntry != null);

        GUILayout.Label("Config Counts", styles.SectionLabel);
        GUILayout.Label($"Config: {Path.GetFileName(selected.ConfigPath)}", styles.SettingDescription);
        GUILayout.Label($"Live entries: {liveCount.ToString(CultureInfo.InvariantCulture)}  |  File-backed entries: {fileBackedCount.ToString(CultureInfo.InvariantCulture)}", styles.PluginDetail);

        foreach (IGrouping<string, ManagedSetting> group in selected.Settings
            .GroupBy(GetFriendlyKind, StringComparer.OrdinalIgnoreCase)
            .OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase))
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(group.Key, styles.ValuePill, GUILayout.Width(Px(118f)), GUILayout.Height(Px(28f)));
            GUILayout.Label(FormatCount(group.Count(), "option"), styles.PluginDetail, GUILayout.MinWidth(Px(160f)));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
    }

    private void DrawDashboardStatusCell(string label, string value, bool highlighted)
    {
        UiStyles styles = _styles!;
        GUILayout.BeginVertical(GUILayout.Width(Px(150f)));
        GUILayout.Label(label, styles.FooterText);
        GUILayout.Label(value, highlighted ? styles.EnabledLabel : styles.DisabledLabel, GUILayout.Height(Px(24f)));
        GUILayout.EndVertical();
    }

    private ControllerActionRegistration[] GetDashboardControllerActions(ManagedPlugin selected)
    {
        return GetControllerActionRegistrationsForUi()
            .Where(action => DashboardRegistrationMatchesPlugin(selected, action.ActionId, action.Category, action.DisplayName))
            .ToArray();
    }

    private StatusProviderRegistration[] GetDashboardStatusProviders(ManagedPlugin selected)
    {
        return GetStatusProviderRegistrationsForUi()
            .Where(provider => DashboardRegistrationMatchesPlugin(selected, provider.ProviderId, provider.Category, provider.DisplayName))
            .ToArray();
    }

    private DashboardCustomUiOwner[] GetDashboardCustomUiOwners(ManagedPlugin selected)
    {
        List<DashboardCustomUiOwner> owners = new();
        foreach (string ownerId in _customUiScopes
            .Where(ownerId => DashboardTokenMatchesPlugin(selected, ownerId))
            .OrderBy(ownerId => ownerId, StringComparer.OrdinalIgnoreCase))
        {
            owners.Add(new DashboardCustomUiOwner(ownerId, "Custom UI", _worldFreezeScopes.Contains(ownerId) ? "Freeze" : "Overlay"));
        }

        foreach (string ownerId in _controllerCursorScopes
            .Where(ownerId => !_customUiScopes.Contains(ownerId) && DashboardTokenMatchesPlugin(selected, ownerId))
            .OrderBy(ownerId => ownerId, StringComparer.OrdinalIgnoreCase))
        {
            owners.Add(new DashboardCustomUiOwner(ownerId, "Cursor", "No freeze"));
        }

        return owners.ToArray();
    }

    private DashboardProfileCoverage GetDashboardProfileCoverage(ManagedPlugin selected)
    {
        string profileName = SanitizeProfileName(_profileName);
        string profilePath;
        try
        {
            profilePath = GetProfilePath(profileName);
        }
        catch (Exception ex)
        {
            return new DashboardProfileCoverage(
                profileName,
                "(invalid profile path)",
                exists: false,
                matchedSettings: 0,
                totalSettings: selected.Settings.Count,
                ownerRows: 0,
                unknownRows: 0,
                invalidRows: 0,
                error: "Invalid profile path: " + ex.Message);
        }

        bool exists = File.Exists(profilePath);
        long lastWriteTicks = exists ? File.GetLastWriteTimeUtc(profilePath).Ticks : -1L;
        string cacheKey = string.Join("\n",
            selected.SelectionKey,
            profileName,
            profilePath,
            lastWriteTicks.ToString(CultureInfo.InvariantCulture),
            selected.Settings.Count.ToString(CultureInfo.InvariantCulture));
        if (string.Equals(_dashboardProfileCoverageCacheKey, cacheKey, StringComparison.Ordinal))
        {
            return _dashboardProfileCoverage;
        }

        _dashboardProfileCoverageCacheKey = cacheKey;
        _dashboardProfileCoverage = BuildDashboardProfileCoverage(selected, profileName, profilePath, exists);
        return _dashboardProfileCoverage;
    }

    private static DashboardProfileCoverage BuildDashboardProfileCoverage(ManagedPlugin selected, string profileName, string profilePath, bool exists)
    {
        string profileFileName = Path.GetFileName(profilePath);
        if (!exists)
        {
            return new DashboardProfileCoverage(
                profileName,
                profileFileName,
                exists: false,
                matchedSettings: 0,
                totalSettings: selected.Settings.Count,
                ownerRows: 0,
                unknownRows: 0,
                invalidRows: 0,
                error: string.Empty);
        }

        try
        {
            HashSet<string> selectedSettingKeys = new(StringComparer.Ordinal);
            foreach (ManagedSetting setting in selected.Settings)
            {
                selectedSettingKeys.Add(BuildProfileSettingKey(setting));
            }

            HashSet<string> matchedSettings = new(StringComparer.Ordinal);
            int ownerRows = 0;
            int unknownRows = 0;
            int invalidRows = 0;

            foreach (string line in File.ReadAllLines(profilePath))
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#", StringComparison.Ordinal))
                {
                    continue;
                }

                string[] parts = line.Split('\t');
                if (parts.Length != 4 ||
                    !TryDecodeProfileField(parts[0], out string ownerGuid) ||
                    !TryDecodeProfileField(parts[1], out string section) ||
                    !TryDecodeProfileField(parts[2], out string key) ||
                    !TryDecodeProfileField(parts[3], out _))
                {
                    invalidRows++;
                    continue;
                }

                if (!string.Equals(ownerGuid, selected.Guid, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                ownerRows++;
                string profileSettingKey = BuildProfileSettingKey(ownerGuid, section, key);
                if (selectedSettingKeys.Contains(profileSettingKey))
                {
                    matchedSettings.Add(profileSettingKey);
                }
                else
                {
                    unknownRows++;
                }
            }

            return new DashboardProfileCoverage(
                profileName,
                profileFileName,
                exists: true,
                matchedSettings: matchedSettings.Count,
                totalSettings: selected.Settings.Count,
                ownerRows,
                unknownRows,
                invalidRows,
                error: string.Empty);
        }
        catch (Exception ex)
        {
            return new DashboardProfileCoverage(
                profileName,
                profileFileName,
                exists: true,
                matchedSettings: 0,
                totalSettings: selected.Settings.Count,
                ownerRows: 0,
                unknownRows: 0,
                invalidRows: 0,
                error: "Could not read profile coverage: " + ex.Message);
        }
    }

    private static bool DashboardRegistrationMatchesPlugin(ManagedPlugin selected, string id, string category, string displayName)
    {
        if (string.Equals(selected.Guid, PluginGuid, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(category, "Manager", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return DashboardTokenMatchesPlugin(selected, id) ||
            DashboardTokenMatchesPlugin(selected, category) ||
            DashboardTokenMatchesPlugin(selected, displayName);
    }

    private static bool DashboardTokenMatchesPlugin(ManagedPlugin selected, string token)
    {
        string cleaned = token.Trim();
        if (cleaned.Length == 0)
        {
            return false;
        }

        return DashboardTokenMatches(cleaned, selected.Guid) ||
            DashboardTokenMatches(cleaned, selected.Name);
    }

    private static bool DashboardTokenMatches(string token, string target)
    {
        string cleanedTarget = target.Trim();
        if (cleanedTarget.Length == 0)
        {
            return false;
        }

        if (string.Equals(token, cleanedTarget, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (token.Length <= cleanedTarget.Length ||
            !token.StartsWith(cleanedTarget, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        char separator = token[cleanedTarget.Length];
        return separator == '.' ||
            separator == ':' ||
            separator == '/' ||
            separator == '\\' ||
            separator == '_' ||
            separator == ' ';
    }

    private void DrawSavedProfilesPanel()
    {
        RefreshProfileNames();
        if (_profileNames.Count == 0)
        {
            GUILayout.Label("No saved profiles found.", _styles!.EmptyState, GUILayout.MinHeight(Px(72f)));
            return;
        }

        foreach (string profileName in _profileNames)
        {
            DrawProfileRow(profileName, GetProfileStatus(profileName));
        }
    }

    private void DrawPresetsPanel()
    {
        foreach (ProfilePreset preset in ProfilePresets)
        {
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            GUILayout.Label(preset.Name, _styles!.SettingName);
            GUILayout.Label($"{preset.Label} slot  |  {GetProfileStatus(preset.Name)}", _styles.SettingDescription);
            GUILayout.EndVertical();

            if (GUILayout.Button("Select", _styles.SecondaryButton, GUILayout.Width(Px(82f)), GUILayout.Height(Px(30f))))
            {
                SelectProfile(preset.Name);
            }

            if (GUILayout.Button("Save", _styles.SecondaryButton, GUILayout.Width(Px(72f)), GUILayout.Height(Px(30f))))
            {
                _profileName = preset.Name;
                SaveSelectedProfile();
            }

            if (GUILayout.Button("Load", _styles.SecondaryButton, GUILayout.Width(Px(72f)), GUILayout.Height(Px(30f))))
            {
                _profileName = preset.Name;
                LoadSelectedProfile();
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(Px(8f));
        }
    }

    private void DrawProfilesPanel()
    {
        GUILayout.Label("Current Profile", _styles!.SectionLabel);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Name", _styles.FooterText, GUILayout.Width(Px(58f)));
        _profileName = GUILayout.TextField(_profileName, _styles.ValueField, GUILayout.Width(Px(220f)), GUILayout.Height(Px(34f)));

        if (GUILayout.Button("Previous", _styles.SecondaryButton, GUILayout.Width(Px(92f)), GUILayout.Height(Px(34f))))
        {
            SelectAdjacentProfile(-1);
        }

        if (GUILayout.Button("Next", _styles.SecondaryButton, GUILayout.Width(Px(72f)), GUILayout.Height(Px(34f))))
        {
            SelectAdjacentProfile(1);
        }

        if (GUILayout.Button("Save", _styles.ActionButton, GUILayout.Width(Px(72f)), GUILayout.Height(Px(34f))))
        {
            SaveSelectedProfile();
        }

        if (GUILayout.Button("Load", _styles.SecondaryButton, GUILayout.Width(Px(72f)), GUILayout.Height(Px(34f))))
        {
            LoadSelectedProfile();
        }

        GUILayout.EndHorizontal();
        GUILayout.Space(Px(8f));
        GUILayout.Label($"File: {Path.GetFileName(GetProfilePath(_profileName))}", _styles.SettingDescription);
        GUILayout.Label($"Folder: {GetProfileRootPath()}", _styles.SettingDescription);
    }

    private void DrawCommandsPanel()
    {
        if (!_controllerActionLayerBindingDraftInitialized)
        {
            ResetControllerCommandDrafts();
        }

        UiStyles styles = _styles!;

        bool shortcutsEnabled = AreControllerShortcutsEnabled;
        ControllerActionBinding[] bindings = ParseControllerActionLayerBindings(_controllerActionLayerBindings?.Value);

        GUILayout.Label("Controller Shortcuts", styles.SectionLabel);
        GUILayout.BeginHorizontal();
        GUILayout.Label(shortcutsEnabled ? "On" : "Off", shortcutsEnabled ? styles.EnabledLabel : styles.DisabledLabel, GUILayout.Width(Px(86f)));
        GUILayout.Label($"Hold {GetControllerModifierDisplayName()}", styles.ValuePill, GUILayout.Width(Px(190f)), GUILayout.Height(Px(30f)));
        GUILayout.Label($"{bindings.Length.ToString(CultureInfo.InvariantCulture)} shortcut{(bindings.Length == 1 ? string.Empty : "s")}", styles.PluginDetail, GUILayout.Width(Px(132f)));
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Enable", styles.ActionButton, GUILayout.Width(Px(90f)), GUILayout.Height(Px(30f))))
        {
            string currentBindings = _controllerActionLayerBindings?.Value ?? string.Empty;
            SaveControllerShortcutSettings(enabled: true, bindings.Length == 0 ? ControllerActionLayerDefaultTestBinding : currentBindings);
        }

        if (GUILayout.Button("Disable", styles.SecondaryButton, GUILayout.Width(Px(96f)), GUILayout.Height(Px(30f))))
        {
            SaveControllerShortcutSettings(enabled: false, _controllerActionLayerBindings?.Value ?? string.Empty);
        }

        GUILayout.EndHorizontal();
        GUILayout.Space(Px(6f));
        GUILayout.Label("Shortcuts run only while the modifier is held and no mod UI already owns input.", styles.SettingDescription);
        GUILayout.Space(Px(10f));

        GUILayout.Label("Modifier", styles.SectionLabel);
        DrawControllerModifierPicker(styles);
        GUILayout.Space(Px(10f));

        GUILayout.Label("Add Shortcut", styles.SectionLabel);
        DrawControllerTargetPicker(styles);
        DrawControllerActionPicker(styles);
        DrawControllerActionRunButton(styles);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Save Shortcut", styles.ActionButton, GUILayout.Width(Px(150f)), GUILayout.Height(Px(34f))))
        {
            SaveControllerShortcutBinding(_controllerActionLayerTargetDraft, _controllerActionLayerActionDraft);
        }

        if (GUILayout.Button("Reset Choice", styles.SecondaryButton, GUILayout.Width(Px(130f)), GUILayout.Height(Px(34f))))
        {
            _controllerActionLayerTargetDraft = ControllerActionLayerDefaultTargetAction;
            _controllerActionLayerActionDraft = ControllerActionLayerDefaultActionId;
        }

        GUILayout.EndHorizontal();
        GUILayout.Space(Px(12f));

        GUILayout.Label("Current Shortcuts", styles.SectionLabel);
        if (bindings.Length == 0)
        {
            GUILayout.Label("No controller shortcuts are bound.", styles.SettingDescription);
        }
        else
        {
            foreach (ControllerActionBinding binding in bindings)
            {
                DrawControllerShortcutRow(binding, bindings);
            }
        }

        GUILayout.Space(Px(8f));
        GUILayout.Label("Advanced", styles.SectionLabel);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Raw", styles.FooterText, GUILayout.Width(Px(58f)));
        _controllerActionLayerBindingDraft = GUILayout.TextField(_controllerActionLayerBindingDraft, styles.ValueField, GUILayout.MinWidth(Px(250f)), GUILayout.Height(Px(34f)));

        if (GUILayout.Button("Save Raw", styles.SecondaryButton, GUILayout.Width(Px(110f)), GUILayout.Height(Px(34f))))
        {
            SaveControllerShortcutSettings(shortcutsEnabled, _controllerActionLayerBindingDraft);
        }

        GUILayout.EndHorizontal();
    }

    private void DrawCustomUiPanel()
    {
        UiStyles styles = _styles!;
        bool darkFantasyEnabled = _darkFantasyAssetsEnabled?.Value ?? true;
        string selectedTaintedInterfacePack = GetTaintedInterfaceUiPackIdInstance();
        bool managerSkinTexturesLoaded = darkFantasyEnabled && _lastManagerSkinTextureCount > 0;
        bool cursorEnabled = _controllerCursorEnabled?.Value ?? true;
        bool cursorActive = IsControllerCursorActiveInstance;
        float cursorSpeed = Mathf.Clamp(_controllerCursorSpeed?.Value ?? ControllerCursorDefaultSpeed, 120f, 2400f);
        float cursorDeadzone = Mathf.Clamp(_controllerCursorDeadzone?.Value ?? ControllerCursorDefaultDeadzone, 0f, 0.75f);

        GUILayout.Label("Manager Skin", styles.SectionLabel);
        GUILayout.BeginHorizontal();
        DrawCustomUiStatusCell("Style", darkFantasyEnabled ? GetTaintedInterfacePackDisplayName(selectedTaintedInterfacePack) : "Fallback", darkFantasyEnabled);
        DrawCustomUiStatusCell("Assets", managerSkinTexturesLoaded ? $"{_lastManagerSkinTextureCount} loaded" : "Missing", managerSkinTexturesLoaded);
        DrawCustomUiStatusCell("Source", _lastManagerSkinSource, managerSkinTexturesLoaded);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Embedded", darkFantasyEnabled ? styles.ActionButton : styles.SecondaryButton, GUILayout.Width(Px(118f)), GUILayout.Height(Px(30f))))
        {
            SaveManagerSkin(true);
        }

        if (GUILayout.Button("Fallback", darkFantasyEnabled ? styles.SecondaryButton : styles.ActionButton, GUILayout.Width(Px(112f)), GUILayout.Height(Px(30f))))
        {
            SaveManagerSkin(false);
        }

        GUILayout.EndHorizontal();
        GUILayout.Space(Px(8f));

        string savedReadabilityProfile = NormalizeReadabilityProfile(_readabilityProfile?.Value);
        GUILayout.BeginHorizontal();
        DrawCustomUiStatusCell("Font", savedReadabilityProfile, string.Equals(savedReadabilityProfile, _appliedReadabilityProfile, StringComparison.OrdinalIgnoreCase));
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Standard", string.Equals(savedReadabilityProfile, ReadabilityProfileStandard, StringComparison.OrdinalIgnoreCase) ? styles.ActionButton : styles.SecondaryButton, GUILayout.Width(Px(118f)), GUILayout.Height(Px(30f))))
        {
            SaveReadabilityProfile(ReadabilityProfileStandard);
        }

        if (GUILayout.Button("Large", string.Equals(savedReadabilityProfile, ReadabilityProfileLarge, StringComparison.OrdinalIgnoreCase) ? styles.ActionButton : styles.SecondaryButton, GUILayout.Width(Px(92f)), GUILayout.Height(Px(30f))))
        {
            SaveReadabilityProfile(ReadabilityProfileLarge);
        }

        GUILayout.EndHorizontal();
        if (!string.Equals(savedReadabilityProfile, _appliedReadabilityProfile, StringComparison.OrdinalIgnoreCase))
        {
            GUILayout.Label("Restart required to apply the saved font profile.", styles.PluginDetail);
        }

        GUILayout.Space(Px(12f));

        GUILayout.Label("Tainted Interface Pack", styles.SectionLabel);
        GUILayout.BeginHorizontal();
        DrawCustomUiStatusCell("Selected", GetTaintedInterfacePackDisplayName(selectedTaintedInterfacePack), true);
        DrawCustomUiStatusCell("Owner", "FoA Manager", true);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Space(Px(4f));
        DrawTaintedInterfacePackButtons(selectedTaintedInterfacePack, styles);
        GUILayout.Space(Px(12f));

        GUILayout.Label("Shared UI Scope", styles.SectionLabel);
        GUILayout.BeginHorizontal();
        DrawCustomUiStatusCell("Input", HasModUiInputContext ? "Owned" : "Free", HasModUiInputContext);
        DrawCustomUiStatusCell("Scopes", _customUiScopes.Count.ToString(CultureInfo.InvariantCulture), _customUiScopes.Count > 0);
        DrawCustomUiStatusCell("World", HasWorldFreezeContext ? "Frozen" : "Running", HasWorldFreezeContext);
        DrawCustomUiStatusCell("Cursor", cursorActive ? "Active" : "Idle", cursorActive);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.Space(Px(10f));
        GUILayout.Label("Active Owners", styles.SectionLabel);
        if (_customUiScopes.Count == 0 && _controllerCursorScopes.Count == 0)
        {
            GUILayout.Label("No custom UI scopes are active.", styles.SettingDescription);
        }
        else
        {
            foreach (string ownerId in _customUiScopes.OrderBy(owner => owner, StringComparer.OrdinalIgnoreCase).ToArray())
            {
                DrawCustomUiScopeRow(ownerId, "Custom UI", _worldFreezeScopes.Contains(ownerId) ? "Freeze" : "Overlay");
            }

            foreach (string ownerId in _controllerCursorScopes
                         .Where(ownerId => !_customUiScopes.Contains(ownerId))
                         .OrderBy(owner => owner, StringComparer.OrdinalIgnoreCase)
                         .ToArray())
            {
                DrawCustomUiScopeRow(ownerId, "Cursor", "No freeze");
            }
        }

        GUILayout.Space(Px(12f));
        GUILayout.Label("Controller Cursor", styles.SectionLabel);
        GUILayout.BeginHorizontal();
        GUILayout.Label(cursorEnabled ? "On" : "Off", cursorEnabled ? styles.EnabledLabel : styles.DisabledLabel, GUILayout.Width(Px(86f)));
        if (GUILayout.Button("Enable", styles.ActionButton, GUILayout.Width(Px(90f)), GUILayout.Height(Px(30f))))
        {
            SaveControllerCursorEnabled(true);
        }

        if (GUILayout.Button("Disable", styles.SecondaryButton, GUILayout.Width(Px(96f)), GUILayout.Height(Px(30f))))
        {
            SaveControllerCursorEnabled(false);
        }

        GUILayout.Label($"Speed {cursorSpeed.ToString("0", CultureInfo.InvariantCulture)}", styles.ValuePill, GUILayout.Width(Px(128f)), GUILayout.Height(Px(30f)));
        if (GUILayout.Button("Slow", styles.SecondaryButton, GUILayout.Width(Px(72f)), GUILayout.Height(Px(30f))))
        {
            SaveControllerCursorSpeed(600f);
        }

        if (GUILayout.Button("Default", styles.SecondaryButton, GUILayout.Width(Px(92f)), GUILayout.Height(Px(30f))))
        {
            SaveControllerCursorSpeed(ControllerCursorDefaultSpeed);
        }

        if (GUILayout.Button("Fast", styles.SecondaryButton, GUILayout.Width(Px(72f)), GUILayout.Height(Px(30f))))
        {
            SaveControllerCursorSpeed(1200f);
        }

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Deadzone", styles.FooterText, GUILayout.Width(Px(86f)));
        GUILayout.Label(cursorDeadzone.ToString("0.00", CultureInfo.InvariantCulture), styles.ValuePill, GUILayout.Width(Px(92f)), GUILayout.Height(Px(30f)));
        if (GUILayout.Button("Low", styles.SecondaryButton, GUILayout.Width(Px(72f)), GUILayout.Height(Px(30f))))
        {
            SaveControllerCursorDeadzone(0.15f);
        }

        if (GUILayout.Button("Default", styles.SecondaryButton, GUILayout.Width(Px(92f)), GUILayout.Height(Px(30f))))
        {
            SaveControllerCursorDeadzone(ControllerCursorDefaultDeadzone);
        }

        if (GUILayout.Button("High", styles.SecondaryButton, GUILayout.Width(Px(76f)), GUILayout.Height(Px(30f))))
        {
            SaveControllerCursorDeadzone(0.35f);
        }

        GUILayout.EndHorizontal();
    }

    private void DrawTaintedInterfacePackButtons(string selectedPackId, UiStyles styles)
    {
        const int packsPerRow = 4;
        for (int index = 0; index < TaintedInterfacePackChoices.Length; index++)
        {
            if (index % packsPerRow == 0)
            {
                GUILayout.BeginHorizontal();
            }

            TaintedInterfacePackChoice choice = TaintedInterfacePackChoices[index];
            GUIStyle buttonStyle = string.Equals(selectedPackId, choice.Id, StringComparison.OrdinalIgnoreCase)
                ? styles.ActionButton
                : styles.SecondaryButton;
            if (GUILayout.Button(choice.ButtonLabel, buttonStyle, GUILayout.Width(Px(126f)), GUILayout.Height(Px(30f))))
            {
                SaveTaintedInterfacePack(choice.Id);
            }

            if (index % packsPerRow == packsPerRow - 1 || index == TaintedInterfacePackChoices.Length - 1)
            {
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                if (index != TaintedInterfacePackChoices.Length - 1)
                {
                    GUILayout.Space(Px(4f));
                }
            }
        }
    }

    private void DrawCustomUiStatusCell(string label, string value, bool highlighted)
    {
        UiStyles styles = _styles!;
        GUILayout.BeginVertical(GUILayout.Width(Px(150f)));
        GUILayout.Label(label, styles.FooterText);
        GUILayout.Label(value, highlighted ? styles.EnabledLabel : styles.DisabledLabel, GUILayout.Height(Px(24f)));
        GUILayout.EndVertical();
    }

    private void DrawCustomUiScopeRow(string ownerId, string scopeType, string mode)
    {
        UiStyles styles = _styles!;
        GUILayout.BeginHorizontal();
        GUILayout.Label(scopeType, styles.ValuePill, GUILayout.Width(Px(118f)), GUILayout.Height(Px(30f)));
        GUILayout.Label(ownerId, styles.SettingDescription, GUILayout.MinWidth(Px(320f)));
        GUILayout.FlexibleSpace();
        GUILayout.Label(mode, styles.PluginDetail, GUILayout.Width(Px(110f)));
        GUILayout.EndHorizontal();
    }

    private void DrawStatusPanel()
    {
        UiStyles styles = _styles!;
        StatusProviderRegistration[] providers = GetStatusProviderRegistrationsForUi();

        GUILayout.BeginHorizontal();
        GUILayout.Label(FormatCount(providers.Length, "provider"), styles.PluginDetail);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Refresh", styles.SecondaryButton, GUILayout.Width(Px(100f)), GUILayout.Height(Px(30f))))
        {
            _status = providers.Length == 0
                ? "No status providers are registered."
                : $"Refreshed {FormatCount(providers.Length, "status provider")}.";
        }

        GUILayout.EndHorizontal();
        GUILayout.Space(Px(8f));

        if (providers.Length == 0)
        {
            GUILayout.Label("No status providers are registered.", styles.EmptyState, GUILayout.MinHeight(Px(72f)));
            return;
        }

        foreach (StatusProviderRegistration provider in providers)
        {
            DrawStatusProviderRow(provider);
            GUILayout.Space(Px(10f));
        }
    }

    private void DrawStatusProviderRow(StatusProviderRegistration provider)
    {
        UiStyles styles = _styles!;
        StatusProviderSnapshotView snapshot = ReadStatusSnapshot(provider);

        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
        GUILayout.Label(GetStatusLevelLabel(snapshot.Level), GetStatusLevelStyle(snapshot.Level, styles), GUILayout.Width(Px(104f)), GUILayout.Height(Px(28f)));
        GUILayout.BeginVertical();
        GUILayout.Label(provider.DisplayName, styles.SettingName);
        GUILayout.Label(GetStatusProviderMeta(provider, snapshot), styles.PluginDetail);
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        if (provider.Description.Length > 0)
        {
            GUILayout.Label(provider.Description, styles.SettingDescription);
        }

        if (snapshot.Summary.Length > 0)
        {
            GUILayout.Label(snapshot.Summary, styles.SettingDescription);
        }

        if (snapshot.Detail.Length > 0)
        {
            GUILayout.Label(snapshot.Detail, styles.SettingDescription);
        }

        foreach (string line in snapshot.Lines)
        {
            GUILayout.Label(line, styles.SettingDescription);
        }

        if (snapshot.LinesClipped)
        {
            GUILayout.Label("Additional status lines were clipped.", styles.PluginDetail);
        }

        GUILayout.EndVertical();
    }

    private StatusProviderSnapshotView ReadStatusSnapshot(StatusProviderRegistration provider)
    {
        try
        {
            FoAModStatusSnapshot? snapshot = provider.SnapshotProvider();
            if (snapshot == null)
            {
                return new StatusProviderSnapshotView(
                    FoAModStatusLevel.Unknown,
                    "Provider returned no status snapshot.",
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    Array.Empty<string>(),
                    linesClipped: false);
            }

            return CreateStatusSnapshotView(snapshot);
        }
        catch (Exception ex)
        {
            return new StatusProviderSnapshotView(
                FoAModStatusLevel.Problem,
                "Status provider failed.",
                CleanStatusText(ex.GetType().Name + ": " + ex.Message, StatusProviderTextLimit),
                string.Empty,
                string.Empty,
                Array.Empty<string>(),
                linesClipped: false);
        }
    }

    private static StatusProviderSnapshotView CreateStatusSnapshotView(FoAModStatusSnapshot snapshot)
    {
        string[] rawLines = snapshot.Lines ?? Array.Empty<string>();
        string[] lines = rawLines
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Take(StatusProviderLineLimit)
            .Select(line => CleanStatusText(line, StatusProviderTextLimit))
            .ToArray();

        return new StatusProviderSnapshotView(
            snapshot.Level,
            CleanStatusText(snapshot.Summary, StatusProviderTextLimit),
            CleanStatusText(snapshot.Detail, StatusProviderTextLimit),
            CleanStatusText(snapshot.Schema, 96),
            CleanStatusText(snapshot.UpdatedUtc, 96),
            lines,
            rawLines.Count(line => !string.IsNullOrWhiteSpace(line)) > StatusProviderLineLimit);
    }

    private static string GetStatusProviderMeta(StatusProviderRegistration provider, StatusProviderSnapshotView snapshot)
    {
        List<string> parts = new()
        {
            provider.Category,
            provider.ProviderId
        };

        if (snapshot.Schema.Length > 0)
        {
            parts.Add("schema " + snapshot.Schema);
        }

        if (snapshot.UpdatedUtc.Length > 0)
        {
            parts.Add("updated " + snapshot.UpdatedUtc);
        }

        return string.Join("  |  ", parts);
    }

    private static string GetStatusLevelLabel(FoAModStatusLevel level)
    {
        return level switch
        {
            FoAModStatusLevel.Ok => "OK",
            FoAModStatusLevel.Info => "Info",
            FoAModStatusLevel.Warning => "Warning",
            FoAModStatusLevel.Problem => "Problem",
            _ => "Unknown"
        };
    }

    private static GUIStyle GetStatusLevelStyle(FoAModStatusLevel level, UiStyles styles)
    {
        return level switch
        {
            FoAModStatusLevel.Ok => styles.SavedPill,
            FoAModStatusLevel.Warning => styles.DirtyPill,
            FoAModStatusLevel.Problem => styles.DisabledLabel,
            FoAModStatusLevel.Info => styles.ValuePill,
            _ => styles.TypePill
        };
    }

    private void SaveManagerSkin(bool useEmbeddedSkin)
    {
        if (!TrySaveManagerConfigValue(_darkFantasyAssetsEnabled, useEmbeddedSkin, out string message))
        {
            _status = message;
            return;
        }

        _stylesDirty = true;
        _status = useEmbeddedSkin
            ? "Embedded manager UI enabled."
            : "Fallback manager UI enabled.";
    }

    private void SaveReadabilityProfile(string profile)
    {
        string normalizedProfile = NormalizeReadabilityProfile(profile);
        if (!TrySaveManagerConfigValue(_readabilityProfile, normalizedProfile, out string message))
        {
            _status = message;
            return;
        }

        _status = string.Equals(normalizedProfile, _appliedReadabilityProfile, StringComparison.OrdinalIgnoreCase)
            ? $"Font profile is already {normalizedProfile}."
            : $"Font profile saved: {normalizedProfile}. Restart the game to apply it.";
    }

    private void SaveTaintedInterfacePack(string packId)
    {
        string normalizedPackId = NormalizeTaintedInterfacePackId(packId);
        if (!TrySaveManagerConfigValue(_taintedInterfacePack, normalizedPackId, out string message))
        {
            _status = message;
            return;
        }

        _stylesDirty = true;
        _status = $"Tainted Interface pack set to {GetTaintedInterfacePackDisplayName(normalizedPackId)}.";
    }

    private string GetTaintedInterfaceUiPackIdInstance()
    {
        return NormalizeTaintedInterfacePackId(_taintedInterfacePack?.Value);
    }

    private static string NormalizeTaintedInterfacePackId(string? packId)
    {
        string normalizedPackId = packId?.Trim() ?? string.Empty;
        foreach (TaintedInterfacePackChoice choice in TaintedInterfacePackChoices)
        {
            if (string.Equals(normalizedPackId, choice.Id, StringComparison.OrdinalIgnoreCase))
            {
                return choice.Id;
            }
        }

        return TaintedInterfaceCoreThemePackId;
    }

    private static string GetTaintedInterfacePackDisplayName(string packId)
    {
        foreach (TaintedInterfacePackChoice choice in TaintedInterfacePackChoices)
        {
            if (string.Equals(packId, choice.Id, StringComparison.OrdinalIgnoreCase))
            {
                return choice.DisplayName;
            }
        }

        return "Dark Fantasy";
    }

    private void SaveControllerCursorEnabled(bool enabled)
    {
        if (!TrySaveManagerConfigValue(_controllerCursorEnabled, enabled, out string message))
        {
            _status = message;
            return;
        }

        _status = enabled
            ? "Controller cursor enabled."
            : "Controller cursor disabled.";
    }

    private void SaveControllerCursorSpeed(float speed)
    {
        float clampedSpeed = Mathf.Clamp(speed, 120f, 2400f);
        if (!TrySaveManagerConfigValue(_controllerCursorSpeed, clampedSpeed, out string message))
        {
            _status = message;
            return;
        }

        _status = $"Controller cursor speed set to {clampedSpeed.ToString("0", CultureInfo.InvariantCulture)}.";
    }

    private void SaveControllerCursorDeadzone(float deadzone)
    {
        float clampedDeadzone = Mathf.Clamp(deadzone, 0f, 0.75f);
        if (!TrySaveManagerConfigValue(_controllerCursorDeadzone, clampedDeadzone, out string message))
        {
            _status = message;
            return;
        }

        _status = $"Controller cursor deadzone set to {clampedDeadzone.ToString("0.00", CultureInfo.InvariantCulture)}.";
    }

    private bool AreControllerShortcutsEnabled =>
        IsControllerActionLayerGateArmed &&
        (HasControllerActionLayerBindings || _controllerHotkeyLayerKeys.Count > 0);

    private bool IsControllerActionLayerGateArmed =>
        _controllerActionLayerEnabled?.Value == true &&
        _controllerActionLayerDryRun?.Value == false &&
        _controllerActionLayerDispatchEnabled?.Value == true;

    private bool HasControllerActionLayerBindings =>
        !string.IsNullOrWhiteSpace(_controllerActionLayerBindings?.Value);

    private void DrawControllerModifierPicker(UiStyles styles)
    {
        int buttonsPerRow = GetControllerModifierButtonsPerRow();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Hold", styles.FooterText, GUILayout.Width(Px(74f)));
        for (int i = 0; i < ControllerActionLayerModifierChoices.Length; i++)
        {
            ControllerActionTargetChoice choice = ControllerActionLayerModifierChoices[i];
            bool selected = IsControllerActionLayerModifierTarget(choice.ActionName);
            if (GUILayout.Button(choice.Label, selected ? styles.ActionButton : styles.SecondaryButton, GUILayout.Width(Px(104f)), GUILayout.Height(Px(30f))))
            {
                SaveControllerModifierChoice(choice);
            }

            if ((i + 1) % buttonsPerRow == 0 && i < ControllerActionLayerModifierChoices.Length - 1)
            {
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Space(Px(74f));
            }
        }

        GUILayout.EndHorizontal();
    }

    private int GetControllerModifierButtonsPerRow()
    {
        float availableWidth = Mathf.Max(Px(520f), _window.width - Px(220f));
        int calculated = Mathf.FloorToInt(availableWidth / Px(104f));
        return Mathf.Clamp(calculated, 4, 8);
    }

    private void DrawControllerTargetPicker(UiStyles styles)
    {
        int buttonsPerRow = GetControllerTargetButtonsPerRow();
        ControllerActionTargetChoice[] choices = ControllerActionLayerTargetChoices
            .Where(choice => !IsControllerActionLayerModifierTarget(choice.ActionName))
            .ToArray();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Button", styles.FooterText, GUILayout.Width(Px(74f)));
        for (int i = 0; i < choices.Length; i++)
        {
            ControllerActionTargetChoice choice = choices[i];
            bool selected = string.Equals(_controllerActionLayerTargetDraft, choice.ActionName, StringComparison.OrdinalIgnoreCase);
            if (GUILayout.Button(choice.Label, selected ? styles.ActionButton : styles.SecondaryButton, GUILayout.Width(Px(104f)), GUILayout.Height(Px(30f))))
            {
                _controllerActionLayerTargetDraft = choice.ActionName;
            }

            if ((i + 1) % buttonsPerRow == 0 && i < choices.Length - 1)
            {
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Space(Px(74f));
            }
        }

        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Custom", styles.FooterText, GUILayout.Width(Px(74f)));
        _controllerActionLayerTargetDraft = GUILayout.TextField(_controllerActionLayerTargetDraft, styles.ValueField, GUILayout.MinWidth(Px(220f)), GUILayout.Height(Px(32f)));
        GUILayout.Label(GetControllerShortcutChordLabel(_controllerActionLayerTargetDraft), styles.PluginDetail, GUILayout.Width(Px(260f)));
        GUILayout.EndHorizontal();
    }

    private int GetControllerTargetButtonsPerRow()
    {
        float availableWidth = Mathf.Max(Px(520f), _window.width - Px(220f));
        int calculated = Mathf.FloorToInt(availableWidth / Px(104f));
        return Mathf.Clamp(calculated, 4, 8);
    }

    private void DrawControllerActionPicker(UiStyles styles)
    {
        ControllerActionRegistration[] actions = GetControllerActionRegistrationsForUi();
        if (actions.Length == 0)
        {
            GUILayout.Label("No controller actions are registered yet.", styles.SettingDescription);
            return;
        }

        if (!actions.Any(action => string.Equals(action.ActionId, _controllerActionLayerActionDraft, StringComparison.OrdinalIgnoreCase)))
        {
            _controllerActionLayerActionDraft = actions[0].ActionId;
        }

        GUILayout.BeginHorizontal();
        GUILayout.Label("Action", styles.FooterText, GUILayout.Width(Px(74f)));
        for (int i = 0; i < actions.Length; i++)
        {
            ControllerActionRegistration action = actions[i];
            bool selected = string.Equals(_controllerActionLayerActionDraft, action.ActionId, StringComparison.OrdinalIgnoreCase);
            if (GUILayout.Button(action.DisplayName, selected ? styles.ActionButton : styles.SecondaryButton, GUILayout.Width(Px(210f)), GUILayout.Height(Px(30f))))
            {
                _controllerActionLayerActionDraft = action.ActionId;
            }

            if ((i + 1) % 3 == 0 && i < actions.Length - 1)
            {
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Space(Px(74f));
            }
        }

        GUILayout.EndHorizontal();

        ControllerActionRegistration? selectedAction = actions
            .FirstOrDefault(action => string.Equals(action.ActionId, _controllerActionLayerActionDraft, StringComparison.OrdinalIgnoreCase));
        if (selectedAction != null)
        {
            string detail = selectedAction.Description.Length > 0
                ? $"{selectedAction.Category} - {selectedAction.Description}"
                : selectedAction.Category;
            GUILayout.BeginHorizontal();
            GUILayout.Space(Px(74f));
            GUILayout.Label(detail, styles.PluginDetail, GUILayout.MinWidth(Px(360f)), GUILayout.Height(Px(22f)));
            GUILayout.EndHorizontal();
        }
    }

    private void DrawControllerActionRunButton(UiStyles styles)
    {
        ControllerActionRegistration? selectedAction = GetControllerActionRegistration(_controllerActionLayerActionDraft);
        GUILayout.BeginHorizontal();
        GUILayout.Space(Px(74f));
        GUILayout.FlexibleSpace();
        bool canRun = selectedAction != null;
        bool previousEnabled = GUI.enabled;
        GUI.enabled = previousEnabled && canRun;
        if (GUILayout.Button("Run Action Now", styles.SecondaryButton, GUILayout.Width(Px(150f)), GUILayout.Height(Px(34f))) && selectedAction != null)
        {
            RunControllerActionNow(selectedAction);
        }

        GUI.enabled = previousEnabled;
        GUILayout.EndHorizontal();
        if (selectedAction == null)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(Px(74f));
            GUILayout.Label("Select a registered action to run it from the manager.", styles.SettingDescription);
            GUILayout.EndHorizontal();
        }
    }

    private void RunControllerActionNow(ControllerActionRegistration registration)
    {
        try
        {
            registration.Callback();
            Logger.LogInfo($"Controller action run from manager: registeredAction={registration.ActionId}; displayName={registration.DisplayName}; source=manager-button.");
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"Controller action run from manager failed for {registration.ActionId}: {ex}");
        }
    }

    private void DrawControllerShortcutRow(ControllerActionBinding binding, ControllerActionBinding[] allBindings)
    {
        UiStyles styles = _styles!;
        GUILayout.BeginHorizontal();
        GUILayout.Label(GetControllerShortcutChordLabel(binding.TargetAction), styles.ValuePill, GUILayout.Width(Px(230f)), GUILayout.Height(Px(30f)));
        GUILayout.Label(GetControllerActionDisplayName(binding.ActionId), styles.SettingDescription, GUILayout.MinWidth(Px(220f)));
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Change", styles.SecondaryButton, GUILayout.Width(Px(92f)), GUILayout.Height(Px(30f))))
        {
            _controllerActionLayerTargetDraft = binding.TargetAction;
            _controllerActionLayerActionDraft = binding.ActionId;
        }

        if (GUILayout.Button("Remove", styles.SecondaryButton, GUILayout.Width(Px(96f)), GUILayout.Height(Px(30f))))
        {
            RemoveControllerShortcutBinding(binding, allBindings);
        }

        GUILayout.EndHorizontal();
    }

    private void ResetControllerCommandDrafts()
    {
        string savedBinding = _controllerActionLayerBindings?.Value ?? string.Empty;
        _controllerActionLayerBindingDraft = string.IsNullOrWhiteSpace(savedBinding)
            ? ControllerActionLayerDefaultTestBinding
            : savedBinding;
        ControllerActionBinding[] bindings = ParseControllerActionLayerBindings(_controllerActionLayerBindingDraft);
        if (bindings.Length > 0)
        {
            _controllerActionLayerTargetDraft = bindings[0].TargetAction;
            _controllerActionLayerActionDraft = bindings[0].ActionId;
        }
        else
        {
            _controllerActionLayerTargetDraft = ControllerActionLayerDefaultTargetAction;
            _controllerActionLayerActionDraft = ControllerActionLayerDefaultActionId;
        }

        _controllerActionLayerBindingDraftInitialized = true;
    }

    private void SaveControllerShortcutBinding(string targetAction, string actionId)
    {
        string cleanedTargetAction = CleanControllerShortcutTarget(targetAction);
        string cleanedActionId = NormalizeControllerActionId(actionId);
        if (cleanedTargetAction.Length == 0)
        {
            _status = "Choose a controller button before saving the shortcut.";
            return;
        }

        if (IsControllerActionLayerReservedModifierTarget(cleanedTargetAction))
        {
            _status = $"{GetControllerModifierDisplayName()} is reserved as the controller shortcut modifier.";
            return;
        }

        if (!_controllerActionRegistrations.ContainsKey(cleanedActionId))
        {
            _status = $"Controller action is not registered: {cleanedActionId}.";
            return;
        }

        List<ControllerActionBinding> bindings = ParseControllerActionLayerBindings(_controllerActionLayerBindings?.Value).ToList();
        bindings.RemoveAll(binding => string.Equals(binding.TargetAction, cleanedTargetAction, StringComparison.OrdinalIgnoreCase));
        bindings.Add(new ControllerActionBinding(cleanedTargetAction, cleanedActionId));

        string serializedBindings = SerializeControllerActionLayerBindings(bindings);
        if (SaveControllerShortcutSettings(enabled: true, serializedBindings))
        {
            _controllerActionLayerTargetDraft = cleanedTargetAction;
            _controllerActionLayerActionDraft = cleanedActionId;
            _status = $"Controller shortcut saved: {GetControllerShortcutChordLabel(cleanedTargetAction)} -> {GetControllerActionDisplayName(cleanedActionId)}.";
        }
    }

    private void RemoveControllerShortcutBinding(ControllerActionBinding bindingToRemove, ControllerActionBinding[] allBindings)
    {
        List<ControllerActionBinding> bindings = allBindings
            .Where(binding =>
                !string.Equals(binding.TargetAction, bindingToRemove.TargetAction, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(binding.ActionId, bindingToRemove.ActionId, StringComparison.OrdinalIgnoreCase))
            .ToList();

        bool keepEnabled = bindings.Count > 0 && AreControllerShortcutsEnabled;
        if (SaveControllerShortcutSettings(keepEnabled, SerializeControllerActionLayerBindings(bindings)))
        {
            _status = bindings.Count == 0
                ? "Controller shortcut removed. Shortcuts are disabled because no bindings remain."
                : "Controller shortcut removed.";
        }
    }

    private bool SaveControllerShortcutSettings(bool enabled, string bindings)
    {
        string modifierActions = GetControllerModifierActionsForSave();
        ControllerActionBinding[] parsedBindings = ParseControllerActionLayerBindings(bindings);
        if (parsedBindings.Any(binding => IsControllerActionLayerReservedModifierTarget(binding.TargetAction)))
        {
            _status = $"{GetControllerModifierDisplayName()} is reserved as the controller shortcut modifier.";
            return false;
        }

        string cleanedBindings = SerializeControllerActionLayerBindings(parsedBindings);
        if (enabled && parsedBindings.Length == 0)
        {
            _status = "Add at least one controller shortcut before enabling shortcuts.";
            return false;
        }

        if (!TrySaveManagerConfigValue(_controllerActionLayerEnabled, enabled, out string message) ||
            !TrySaveManagerConfigValue(_controllerActionLayerDryRun, !enabled, out message) ||
            !TrySaveManagerConfigValue(_controllerActionLayerDispatchEnabled, enabled, out message) ||
            !TrySaveManagerConfigValue(_controllerActionLayerModifierActions, modifierActions, out message) ||
            !TrySaveManagerConfigValue(_controllerActionLayerBindings, cleanedBindings, out message) ||
            !TrySaveManagerConfigValue(_controllerActionLayerControllerSourcesOnly, true, out message))
        {
            _status = message;
            return false;
        }

        _controllerActionLayerBindingDraft = string.IsNullOrWhiteSpace(cleanedBindings)
            ? ControllerActionLayerDefaultTestBinding
            : cleanedBindings;
        _controllerActionLayerBindingDraftInitialized = true;
        ResetControllerActionLayerLogState();
        _status = enabled
            ? "Controller shortcuts enabled."
            : "Controller shortcuts disabled.";
        return true;
    }

    private string GetControllerModifierActionsForSave()
    {
        string current = string.Join(",", SplitControllerCursorActions(_controllerActionLayerModifierActions?.Value));
        return current.Length == 0 ? ControllerActionLayerDefaultModifierActions : current;
    }

    private void SaveControllerModifierChoice(ControllerActionTargetChoice choice)
    {
        string modifierAction = CleanControllerShortcutTarget(choice.ActionName);
        if (modifierAction.Length == 0)
        {
            _status = "Choose a controller modifier before saving.";
            return;
        }

        ControllerActionBinding[] bindings = ParseControllerActionLayerBindings(_controllerActionLayerBindings?.Value);
        ControllerActionBinding? conflictingBinding = bindings.FirstOrDefault(binding => AreControllerActionTokensEquivalent(modifierAction, binding.TargetAction));
        if (conflictingBinding != null)
        {
            _status = $"Remove {GetControllerTargetDisplayName(conflictingBinding.TargetAction)} shortcut before using it as the modifier.";
            return;
        }

        if (TryParseControllerActionKeyCode(modifierAction, out KeyCode modifierKeyCode) &&
            _controllerHotkeyLayerKeys.Contains(modifierKeyCode))
        {
            _status = $"Rebind existing {GetControllerHotkeyDisplayName(modifierKeyCode)} hotkeys before using it as the modifier.";
            return;
        }

        if (!TrySaveManagerConfigValue(_controllerActionLayerModifierActions, modifierAction, out string message))
        {
            _status = message;
            return;
        }

        ResetControllerActionLayerLogState();
        RebuildControllerHotkeyLayerKeys();
        _status = $"Controller modifier set to {choice.Label}.";
    }

    private void MigrateControllerLayerModifierDefaults()
    {
        MigrateControllerModifierDefault(_controllerActionLayerModifierActions);
        MigrateControllerModifierDefault(_controllerChordProbeModifierActions);
    }

    private void MigrateControllerModifierDefault(ConfigEntry<string>? entry)
    {
        if (entry == null ||
            !string.Equals(entry.Value?.Trim(), "ModUp", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        try
        {
            entry.Value = ControllerActionLayerDefaultModifierActions;
            entry.ConfigFile.Save();
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"Could not migrate {entry.Definition.Section}.{entry.Definition.Key} from ModUp to {ControllerActionLayerDefaultModifierActions}: {ex}");
        }
    }

    private ControllerActionRegistration[] GetControllerActionRegistrationsForUi()
    {
        return _controllerActionRegistrations.Values
            .OrderBy(action => action.Category, StringComparer.OrdinalIgnoreCase)
            .ThenBy(action => action.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(action => action.ActionId, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private string GetControllerActionDisplayName(string actionId)
    {
        string normalizedActionId = NormalizeControllerActionId(actionId);
        return _controllerActionRegistrations.TryGetValue(normalizedActionId, out ControllerActionRegistration registration)
            ? registration.DisplayName
            : $"{normalizedActionId} (missing)";
    }

    private ControllerActionRegistration? GetControllerActionRegistration(string actionId)
    {
        string normalizedActionId = NormalizeControllerActionId(actionId);
        return _controllerActionRegistrations.TryGetValue(normalizedActionId, out ControllerActionRegistration registration)
            ? registration
            : null;
    }

    private string GetControllerShortcutChordLabel(string targetAction)
    {
        return $"{GetControllerModifierDisplayName()} + {GetControllerTargetDisplayName(targetAction)}";
    }

    private string GetControllerModifierDisplayName()
    {
        string[] modifierActions = SplitControllerCursorActions(_controllerActionLayerModifierActions?.Value);
        string modifierAction = modifierActions.Length == 0 ? ControllerActionLayerDefaultModifierActions : modifierActions[0];
        return GetControllerModifierDisplayName(modifierAction);
    }

    private static string GetControllerModifierDisplayName(string modifierAction)
    {
        string cleanedModifierAction = CleanControllerShortcutTarget(modifierAction);
        ControllerActionTargetChoice? choice = ControllerActionLayerModifierChoices
            .FirstOrDefault(candidate => AreControllerActionTokensEquivalent(candidate.ActionName, cleanedModifierAction));
        return choice?.Label ?? cleanedModifierAction;
    }

    private static string GetControllerTargetDisplayName(string targetAction)
    {
        string cleanedTargetAction = CleanControllerShortcutTarget(targetAction);
        ControllerActionTargetChoice? choice = ControllerActionLayerTargetChoices
            .FirstOrDefault(candidate => string.Equals(candidate.ActionName, cleanedTargetAction, StringComparison.OrdinalIgnoreCase));
        return choice?.Label ?? cleanedTargetAction;
    }

    private static string CleanControllerShortcutTarget(string targetAction)
    {
        if (string.IsNullOrWhiteSpace(targetAction))
        {
            return string.Empty;
        }

        return CleanProbeValue(targetAction).Trim();
    }

    private bool IsControllerActionLayerReservedModifierTarget(string targetAction)
    {
        return IsControllerActionLayerModifierTarget(targetAction);
    }

    private bool IsControllerActionLayerModifierTarget(string targetAction)
    {
        string[] modifierActions = SplitControllerCursorActions(_controllerActionLayerModifierActions?.Value);
        if (modifierActions.Length == 0)
        {
            modifierActions = SplitControllerCursorActions(ControllerActionLayerDefaultModifierActions);
        }

        foreach (string modifierAction in modifierActions)
        {
            if (AreControllerActionTokensEquivalent(modifierAction, targetAction))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsControllerActionLayerModifierKeyCode(KeyCode keyCode)
    {
        string[] modifierActions = SplitControllerCursorActions(_controllerActionLayerModifierActions?.Value);
        if (modifierActions.Length == 0)
        {
            modifierActions = SplitControllerCursorActions(ControllerActionLayerDefaultModifierActions);
        }

        foreach (string modifierAction in modifierActions)
        {
            if (TryParseControllerActionKeyCode(modifierAction, out KeyCode modifierKeyCode) &&
                modifierKeyCode == keyCode)
            {
                return true;
            }
        }

        return false;
    }

    private static bool AreControllerActionTokensEquivalent(string left, string right)
    {
        string cleanedLeft = CleanControllerShortcutTarget(left);
        string cleanedRight = CleanControllerShortcutTarget(right);
        if (cleanedLeft.Length == 0 || cleanedRight.Length == 0)
        {
            return false;
        }

        if (string.Equals(cleanedLeft, cleanedRight, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return TryParseControllerActionKeyCode(cleanedLeft, out KeyCode leftKeyCode) &&
            TryParseControllerActionKeyCode(cleanedRight, out KeyCode rightKeyCode) &&
            leftKeyCode == rightKeyCode;
    }

    private static string SerializeControllerActionLayerBindings(IEnumerable<ControllerActionBinding> bindings)
    {
        return string.Join(
            ";",
            bindings
                .Where(binding => binding.TargetAction.Length > 0 && binding.ActionId.Length > 0)
                .GroupBy(binding => binding.TargetAction, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.Last())
                .Select(binding => $"{binding.TargetAction}={binding.ActionId}"));
    }

    private void SaveControllerActionRouteTest(bool enabled, string binding)
    {
        string cleanedBinding = string.IsNullOrWhiteSpace(binding)
            ? ControllerActionLayerDefaultTestBinding
            : binding.Trim();
        string modifierActions = GetControllerModifierActionsForSave();

        if (!TrySaveManagerConfigValue(_controllerActionLayerEnabled, enabled, out string message) ||
            !TrySaveManagerConfigValue(_controllerActionLayerDryRun, true, out message) ||
            !TrySaveManagerConfigValue(_controllerActionLayerDispatchEnabled, false, out message) ||
            !TrySaveManagerConfigValue(_controllerActionLayerModifierActions, modifierActions, out message) ||
            !TrySaveManagerConfigValue(_controllerActionLayerBindings, cleanedBinding, out message) ||
            !TrySaveManagerConfigValue(_controllerActionLayerControllerSourcesOnly, true, out message))
        {
            _status = message;
            return;
        }

        _controllerActionLayerBindingDraft = cleanedBinding;
        _controllerActionLayerBindingDraftInitialized = true;
        ResetControllerActionLayerLogState();
        _status = enabled
            ? $"Controller route test started: {cleanedBinding}."
            : "Controller route test stopped.";
    }

    private void SaveControllerActionDispatchTest(bool enabled, string binding)
    {
        string cleanedBinding = string.IsNullOrWhiteSpace(binding)
            ? ControllerActionLayerDefaultTestBinding
            : binding.Trim();
        string modifierActions = GetControllerModifierActionsForSave();

        if (!TrySaveManagerConfigValue(_controllerActionLayerEnabled, true, out string message) ||
            !TrySaveManagerConfigValue(_controllerActionLayerDryRun, !enabled, out message) ||
            !TrySaveManagerConfigValue(_controllerActionLayerDispatchEnabled, enabled, out message) ||
            !TrySaveManagerConfigValue(_controllerActionLayerModifierActions, modifierActions, out message) ||
            !TrySaveManagerConfigValue(_controllerActionLayerBindings, cleanedBinding, out message) ||
            !TrySaveManagerConfigValue(_controllerActionLayerControllerSourcesOnly, true, out message))
        {
            _status = message;
            return;
        }

        _controllerActionLayerBindingDraft = cleanedBinding;
        _controllerActionLayerBindingDraftInitialized = true;
        ResetControllerActionLayerLogState();
        _status = enabled
            ? $"Controller dispatch armed for {cleanedBinding}."
            : "Controller dispatch disarmed; route logging remains available.";
    }

    private void SaveControllerChordScan(bool enabled)
    {
        if (!TrySaveManagerConfigValue(_controllerChordProbeEnabled, enabled, out string message) ||
            !TrySaveManagerConfigValue(_controllerChordProbeModifierActions, ControllerChordProbeDefaultModifierActions, out message) ||
            !TrySaveManagerConfigValue(_controllerChordProbeTargetActions, ControllerChordProbeDefaultTargetActions, out message) ||
            !TrySaveManagerConfigValue(_controllerChordProbeTargetScanIntervalSeconds, ControllerChordProbeDefaultTargetScanIntervalSeconds, out message) ||
            !TrySaveManagerConfigValue(_controllerChordProbeControllerSourcesOnly, true, out message))
        {
            _status = message;
            return;
        }

        ResetControllerChordProbeLogState();
        _status = enabled
            ? "Controller chord scan started."
            : "Controller chord scan stopped.";
    }

    private bool TrySaveManagerConfigValue<T>(ConfigEntry<T>? entry, T value, out string message)
    {
        if (entry == null)
        {
            message = "Controller shortcut setting is not available.";
            return false;
        }

        try
        {
            entry.Value = value;
            entry.ConfigFile.Save();
            message = string.Empty;
            return true;
        }
        catch (Exception ex)
        {
            message = "Could not save controller shortcut setting.";
            Logger.LogWarning($"Could not save {entry.Definition.Section}.{entry.Definition.Key}: {ex}");
            return false;
        }
    }

    private void ResetControllerCommandLogState()
    {
        ResetControllerActionLayerLogState();
        ResetControllerChordProbeLogState();
    }

    private void ResetControllerActionLayerLogState()
    {
        _controllerActionLayerLogCount = 0;
        _controllerActionLayerLastLogTimes.Clear();
        _controllerActionLayerSuppressedActionNames.Clear();
        _controllerActionLayerSuppressedActionIds.Clear();
        _controllerActionLayerModifierHeldFrame = -1;
        _controllerActionLayerSuppressionFrame = -1;
        _controllerActionLayerMaxLogWarningLogged = false;
        _controllerActionLayerDispatchGateWarningLogged = false;
    }

    private void ResetControllerChordProbeLogState()
    {
        _controllerChordProbeLogCount = 0;
        _controllerChordProbeLastLogTimes.Clear();
        _controllerChordProbeModifierWasHeld = false;
        _controllerChordProbeMaxLogWarningLogged = false;
        _controllerChordProbeNextTargetScanAt = 0f;
        _controllerChordProbeLastModifierAction = "<none>";
    }

    private void DrawWarningsPanel()
    {
        List<(ManagedPlugin Plugin, ManagerIssue Issue)> issues = _plugins
            .SelectMany(plugin => plugin.Issues.Select(issue => (Plugin: plugin, Issue: issue)))
            .OrderBy(row => row.Issue.IsBlocking ? 0 : 1)
            .ThenBy(row => row.Issue.Category, StringComparer.OrdinalIgnoreCase)
            .ThenBy(row => row.Plugin.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(row => row.Issue.Message, StringComparer.OrdinalIgnoreCase)
            .ToList();

        int problemCount = issues.Count(row => row.Issue.IsBlocking);
        GUILayout.BeginHorizontal();
        GUILayout.Label($"{FormatCount(problemCount, "problem")}  |  {FormatCount(issues.Count - problemCount, "warning")}", _styles!.PluginDetail);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Export Report", _styles.ActionButton, GUILayout.Width(Px(132f)), GUILayout.Height(Px(30f))))
        {
            ExportDiagnosticReport();
        }

        GUILayout.EndHorizontal();
        GUILayout.Space(Px(8f));

        if (issues.Count == 0)
        {
            GUILayout.Label("No warnings found.", _styles.EmptyState, GUILayout.MinHeight(Px(72f)));
            return;
        }

        foreach ((ManagedPlugin plugin, ManagerIssue issue) in issues)
        {
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            GUILayout.Label($"{issue.Severity}: {issue.Message}", issue.IsBlocking ? _styles!.DisabledLabel : _styles!.FooterText);
            GUILayout.Label($"{issue.Category}  |  {plugin.Name}  |  {plugin.Guid}", _styles.PluginDetail);
            if (issue.HasSettingTarget)
            {
                GUILayout.Label($"Setting: {issue.SettingDisplayName}", _styles.SettingDescription);
            }

            if (!string.IsNullOrWhiteSpace(issue.Detail))
            {
                GUILayout.Label(issue.Detail, _styles.SettingDescription);
            }

            GUILayout.EndVertical();
            GUILayout.BeginVertical(GUILayout.Width(Px(118f)));
            if (GUILayout.Button("Select Mod", _styles.SecondaryButton, GUILayout.Height(Px(30f))))
            {
                FocusIssue(plugin, issue, closePanel: false);
            }

            if (issue.HasSettingTarget)
            {
                if (GUILayout.Button("Find Setting", _styles.SecondaryButton, GUILayout.Height(Px(30f))))
                {
                    FocusIssue(plugin, issue, closePanel: true);
                }

                if (GUILayout.Button("Rebind", _styles.ActionButton, GUILayout.Height(Px(30f))))
                {
                    BeginIssueRebind(plugin, issue);
                }
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.Space(Px(10f));
        }
    }

    private void DrawProfileRow(string profileName, string status)
    {
        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();
        GUILayout.Label(profileName, _styles!.SettingName);
        GUILayout.Label(status, _styles.SettingDescription);
        GUILayout.EndVertical();

        if (GUILayout.Button("Select", _styles.SecondaryButton, GUILayout.Width(Px(82f)), GUILayout.Height(Px(30f))))
        {
            SelectProfile(profileName);
        }

        if (GUILayout.Button("Load", _styles.SecondaryButton, GUILayout.Width(Px(72f)), GUILayout.Height(Px(30f))))
        {
            _profileName = profileName;
            LoadSelectedProfile();
        }

        GUILayout.EndHorizontal();
        GUILayout.Space(Px(8f));
    }

    private void SelectProfile(string profileName)
    {
        _profileName = SanitizeProfileName(profileName);
        _status = $"Selected profile `{_profileName}`.";
    }

    private void DrawPluginList()
    {
        GUILayout.BeginVertical(_styles!.Sidebar, GUILayout.Width(Px(GetInstalledModsColumnWidth())), GUILayout.ExpandHeight(true));
        GUILayout.Space(Px(MainPaneHeaderTextTopInset));
        GUILayout.BeginHorizontal();
        GUILayout.Space(Px(MainPaneHeaderTextLeftInset));
        GUILayout.Label("Installed Mods", _styles.PanelTitle, GUILayout.Height(Px(SidebarTitleHeight)));
        GUILayout.EndHorizontal();
        DrawSearchBox(ref _pluginSearch, "Find a mod");
        GUILayout.Space(Px(8f));

        _pluginScroll = GUILayout.BeginScrollView(_pluginScroll, GUIStyle.none, GUI.skin.verticalScrollbar);
        List<ManagedPlugin> plugins = FilterPlugins().ToList();
        if (plugins.Count == 0)
        {
            GUILayout.Label("No matching mods.", _styles.EmptyState, GUILayout.MinHeight(Px(96f)));
        }

        foreach (ManagedPlugin plugin in plugins)
        {
            DrawPluginListItem(plugin);
        }

        GUILayout.EndScrollView();
        GUILayout.EndVertical();
        GUILayout.Space(Px(12f));
    }

    private static float GetInstalledModsColumnWidth()
    {
        float width = _installedModsColumnWidth?.Value ?? DefaultInstalledModsColumnWidth;
        return Mathf.Clamp(width, MinimumInstalledModsColumnWidth, MaximumInstalledModsColumnWidth);
    }

    private void DrawPluginListItem(ManagedPlugin plugin)
    {
        bool selected = plugin.SelectionKey == _selectedPluginKey;
        GUIStyle style = selected ? _styles!.PluginButtonSelected : _styles!.PluginButton;
        float itemHeight = Px(84f);

        GUILayout.BeginVertical(style, GUILayout.MinHeight(itemHeight));
        if (GUILayout.Button(GUIContent.none, _styles.InvisibleButton, GUILayout.ExpandWidth(true), GUILayout.Height(1f)) && !selected)
        {
            SelectPlugin(plugin.SelectionKey);
        }

        Rect itemRect = GUILayoutUtility.GetLastRect();
        itemRect.height = itemHeight;
        if (Event.current.type == EventType.MouseDown && itemRect.Contains(Event.current.mousePosition))
        {
            SelectPlugin(plugin.SelectionKey);
            Event.current.Use();
        }

        if (selected)
        {
            GUI.DrawTexture(new Rect(itemRect.x, itemRect.y, Px(4f), itemRect.height), _styles.SelectedStrip);
        }

        float textX = itemRect.x + Px(PluginListItemTextLeftInset);
        float textWidth = itemRect.width - Px(PluginListItemTextLeftInset + PluginListItemTextRightInset);
        GUI.Label(new Rect(textX, itemRect.y + Px(PluginListItemNameTopInset), textWidth, Px(PluginListItemNameHeight)), plugin.Name, selected ? _styles.PluginNameSelected : _styles.PluginName);
        string pluginMeta = $"{FormatCount(plugin.Settings.Count, "option")}  |  v{plugin.Version}";
        if (plugin.Issues.Count > 0)
        {
            pluginMeta += $"  |  {FormatCount(plugin.Issues.Count, "warning")}";
        }

        GUI.Label(new Rect(textX, itemRect.y + Px(PluginListItemMetaTopInset), textWidth, Px(PluginListItemMetaHeight)), pluginMeta, selected ? _styles.PluginMetaSelected : _styles.PluginMeta);
        GUILayout.Space(itemHeight - 1f);
        GUILayout.EndVertical();
        GUILayout.Space(Px(6f));
    }

    private void DrawSelectedPluginSettings()
    {
        GUILayout.BeginVertical(_styles!.Content, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        ManagedPlugin? selected = _plugins.FirstOrDefault(plugin => plugin.SelectionKey == _selectedPluginKey);
        if (selected == null)
        {
            GUILayout.Label("No adjustable mods found.", _styles.EmptyState);
            GUILayout.EndVertical();
            return;
        }

        GUILayout.BeginHorizontal();
        GUILayout.Space(Px(SelectedPluginHeaderTextLeftInset));
        GUILayout.BeginVertical();
        GUILayout.Space(Px(MainPaneHeaderTextTopInset));
        GUILayout.Label(selected.Name, _styles.PluginTitle, GUILayout.MinHeight(Px(SelectedPluginTitleHeight)));
        GUILayout.Label($"Version {selected.Version}", _styles.PluginDetail, GUILayout.MinHeight(Px(SelectedPluginVersionHeight)));
        GUILayout.EndVertical();
        GUILayout.FlexibleSpace();
        GUILayout.BeginVertical(GUILayout.Width(Px(128f)));
        GUILayout.Space(Px(MainPaneHeaderTextTopInset));
        GUILayout.Label(FormatCount(selected.Settings.Count, "option"), _styles.CountPill, GUILayout.Width(Px(128f)), GUILayout.Height(Px(30f)));
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        GUILayout.Space(Px(8f));
        DrawSearchBox(ref _settingSearch, "Find an option");
        GUILayout.Space(Px(10f));
        DrawPluginIssues(selected);

        List<ManagedSetting> settings = FilterSettings(selected).ToList();
        if (settings.Count == 0)
        {
            GUILayout.Label("No matching options.", _styles.EmptyState);
            GUILayout.EndVertical();
            return;
        }

        List<IGrouping<string, ManagedSetting>> sections = settings.GroupBy(setting => setting.DisplaySection).ToList();
        DrawSettingCategoryToolbar(selected, sections, settings.Count);

        bool searchActive = !string.IsNullOrWhiteSpace(_settingSearch);
        _settingsScroll = GUILayout.BeginScrollView(_settingsScroll, GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        foreach (IGrouping<string, ManagedSetting> section in sections)
        {
            List<ManagedSetting> sectionSettings = section.ToList();
            string sectionKey = BuildSettingSectionStateKey(selected.Guid, section.Key);
            bool expanded = searchActive || _expandedSettingSections.Contains(sectionKey);
            bool headerClicked = DrawSettingCategoryHeader(section.Key, sectionSettings.Count, expanded, searchActive);
            if (!searchActive && headerClicked)
            {
                if (expanded)
                {
                    _expandedSettingSections.Remove(sectionKey);
                }
                else
                {
                    _expandedSettingSections.Add(sectionKey);
                }

                expanded = !expanded;
            }

            if (expanded)
            {
                foreach (ManagedSetting setting in sectionSettings)
                {
                    DrawSetting(setting);
                }
            }

            GUILayout.Space(Px(8f));
        }

        GUILayout.EndScrollView();
        GUILayout.EndVertical();
    }

    private void DrawSettingCategoryToolbar(ManagedPlugin plugin, IReadOnlyCollection<IGrouping<string, ManagedSetting>> sections, int visibleSettingCount)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(FormatCount(sections.Count, "category"), _styles!.CountPill, GUILayout.Width(Px(144f)), GUILayout.Height(Px(28f)));
        GUILayout.Label(FormatCount(visibleSettingCount, "option"), _styles.CountPill, GUILayout.Width(Px(128f)), GUILayout.Height(Px(28f)));
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Open All", _styles.SecondaryButton, GUILayout.Width(Px(112f)), GUILayout.Height(Px(28f))))
        {
            foreach (IGrouping<string, ManagedSetting> section in sections)
            {
                _expandedSettingSections.Add(BuildSettingSectionStateKey(plugin.Guid, section.Key));
            }
        }

        if (GUILayout.Button("Close All", _styles.SecondaryButton, GUILayout.Width(Px(112f)), GUILayout.Height(Px(28f))))
        {
            foreach (IGrouping<string, ManagedSetting> section in sections)
            {
                _expandedSettingSections.Remove(BuildSettingSectionStateKey(plugin.Guid, section.Key));
            }
        }

        GUILayout.EndHorizontal();
        GUILayout.Space(Px(8f));
    }

    private bool DrawSettingCategoryHeader(string sectionName, int settingCount, bool expanded, bool searchActive)
    {
        string marker = expanded ? "- " : "+ ";
        string suffix = searchActive ? $"  |  {FormatCount(settingCount, "match")}" : $"  |  {FormatCount(settingCount, "option")}";
        return GUILayout.Button(marker + sectionName + suffix, _styles!.SectionButton, GUILayout.ExpandWidth(true), GUILayout.Height(Px(36f)));
    }

    private void DrawPluginIssues(ManagedPlugin plugin)
    {
        if (plugin.Issues.Count == 0)
        {
            return;
        }

        GUILayout.BeginVertical(_styles!.SettingCard, GUILayout.ExpandWidth(true));
        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
        GUILayout.Space(Px(WarningSummaryTextLeftInset));
        GUILayout.Label($"Warnings ({plugin.Issues.Count.ToString(CultureInfo.InvariantCulture)})", _styles.SectionLabel);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button(_selectedPluginIssuesExpanded ? "Hide" : "Show", _styles.SecondaryButton, GUILayout.Width(Px(82f)), GUILayout.Height(Px(28f))))
        {
            _selectedPluginIssuesExpanded = !_selectedPluginIssuesExpanded;
        }

        if (GUILayout.Button("Open Warnings", _styles.SecondaryButton, GUILayout.Width(Px(132f)), GUILayout.Height(Px(28f))))
        {
            ToggleSubPanel(ManagerSubPanel.Warnings);
        }

        GUILayout.EndHorizontal();
        if (_selectedPluginIssuesExpanded)
        {
            GUILayout.Space(Px(6f));
            foreach (ManagerIssue issue in plugin.Issues)
            {
                GUILayout.Label($"{issue.Severity}: {issue.Message}", issue.IsBlocking ? _styles.DisabledLabel : _styles.FooterText);
                if (!string.IsNullOrWhiteSpace(issue.Detail))
                {
                    GUILayout.Label(issue.Detail, _styles.SettingDescription);
                }
            }
        }

        GUILayout.EndVertical();
        GUILayout.Space(Px(8f));
    }

    private void DrawSetting(ManagedSetting setting)
    {
        GUILayout.BeginVertical(_styles!.SettingCard, GUILayout.ExpandWidth(true));
        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
        GUILayout.Space(Px(PanelContentInset));
        GUILayout.BeginVertical(GUILayout.ExpandWidth(true));

        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
        GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
        GUILayout.Label(setting.DisplayName, _styles.SettingName);
        if (!string.IsNullOrWhiteSpace(setting.Description))
        {
            GUILayout.Label(setting.Description, _styles.SettingDescription);
        }

        GUILayout.EndVertical();
        GUILayout.Label(GetFriendlyKind(setting), _styles.TypePill, GUILayout.Width(Px(96f)), GUILayout.Height(Px(28f)));
        if (GUILayout.Button("Restore", _styles.SecondaryButton, GUILayout.Width(Px(96f)), GUILayout.Height(Px(28f))))
        {
            RestoreDefault(setting);
        }

        GUILayout.EndHorizontal();
        GUILayout.Space(Px(8f));

        if (setting.SettingType == typeof(bool))
        {
            DrawBoolSetting(setting);
        }
        else if (setting.SettingType == typeof(KeyCode))
        {
            DrawKeyCodeSetting(setting);
        }
        else if (setting.HasChoiceValues)
        {
            DrawChoiceSetting(setting);
        }
        else if (setting.NumericRange != null)
        {
            DrawRangedNumberSetting(setting);
        }
        else
        {
            DrawSerializedSetting(setting);
        }

        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        GUILayout.Space(Px(8f));
    }

    private void DrawBoolSetting(ManagedSetting setting)
    {
        UiStyles styles = _styles!;
        bool current = setting.TryReadBool(out bool value) && value;
        GUILayout.BeginHorizontal();
        GUILayout.Label(current ? "On" : "Off", current ? styles.EnabledLabel : styles.DisabledLabel, GUILayout.Width(Px(100f)));
        GUILayout.FlexibleSpace();
        if (GUILayout.Button(current ? "Turn Off" : "Turn On", styles.ActionButton, GUILayout.Width(Px(118f)), GUILayout.Height(Px(34f))))
        {
            SaveBoxedValue(setting, !current);
        }

        GUILayout.EndHorizontal();
    }

    private void DrawChoiceSetting(ManagedSetting setting)
    {
        UiStyles styles = _styles!;
        string currentValue = setting.GetSerializedValue();

        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
        GUILayout.Label($"Current: {setting.GetChoiceLabel(currentValue)}", styles.ValuePill, GUILayout.MinWidth(Px(196f)), GUILayout.Height(Px(32f)));
        GUILayout.Label("Saved", styles.SavedPill, GUILayout.Width(Px(84f)), GUILayout.Height(Px(32f)));
        GUILayout.EndHorizontal();
        GUILayout.Space(Px(6f));

        DrawChoiceButtons(setting);
        GUILayout.EndVertical();
    }

    private void DrawChoiceButtons(ManagedSetting setting)
    {
        UiStyles styles = _styles!;
        IReadOnlyList<string> choices = setting.ChoiceValues;
        if (choices.Count == 0)
        {
            return;
        }

        bool hasLongLabels = choices.Any(choice => setting.GetChoiceLabel(choice).Length > 22);
        int columns = Math.Max(1, Math.Min(hasLongLabels ? 1 : 2, choices.Count));
        for (int i = 0; i < choices.Count; i++)
        {
            if (i % columns == 0)
            {
                GUILayout.BeginHorizontal();
            }

            string option = choices[i];
            string label = setting.GetChoiceLabel(option);
            bool selected = string.Equals(option, setting.DraftValue, StringComparison.OrdinalIgnoreCase);
            if (GUILayout.Button(
                    label,
                    selected ? styles.ActionButton : styles.SecondaryButton,
                    GUILayout.MinWidth(Px(hasLongLabels ? 196f : 160f)),
                    GUILayout.Height(Px(hasLongLabels ? 46f : 40f))))
            {
                setting.DraftValue = option;
                SaveSerializedValue(setting);
            }

            if (i % columns == columns - 1 || i == choices.Count - 1)
            {
                GUILayout.EndHorizontal();
            }
        }
    }

    private void DrawSearchBox(ref string search, string label)
    {
        UiStyles styles = _styles!;
        GUILayout.Label(label, styles.SearchLabel);
        GUILayout.BeginHorizontal();
        search = GUILayout.TextField(search, styles.SearchField, GUILayout.Height(Px(34f)));
        if (GUILayout.Button("Clear", styles.SecondaryButton, GUILayout.Width(Px(78f)), GUILayout.Height(Px(34f))))
        {
            search = string.Empty;
        }

        GUILayout.EndHorizontal();
    }

    private void DrawKeyCodeSetting(ManagedSetting setting)
    {
        UiStyles styles = _styles!;
        bool capturing = _captureSettingId == setting.Id;
        GUILayout.BeginHorizontal();
        GUILayout.Label(setting.GetSerializedValue(), styles.ValuePill, GUILayout.Width(Px(134f)), GUILayout.Height(Px(32f)));
        GUILayout.FlexibleSpace();

        if (GUILayout.Button(capturing ? "Press Key" : "Change", capturing ? styles.CaptureButton : styles.ActionButton, GUILayout.Width(Px(116f)), GUILayout.Height(Px(34f))))
        {
            _captureSettingId = capturing ? null : setting.Id;
            _status = capturing ? "Key capture cancelled." : $"Press a key for {setting.DisplayName}.";
        }

        if (GUILayout.Button("Clear", styles.SecondaryButton, GUILayout.Width(Px(88f)), GUILayout.Height(Px(34f))))
        {
            SaveBoxedValue(setting, KeyCode.None);
            if (capturing)
            {
                _captureSettingId = null;
            }
        }

        GUILayout.EndHorizontal();
        DrawControllerHotkeyButtons(setting);
    }

    private void DrawControllerHotkeyButtons(ManagedSetting setting)
    {
        UiStyles styles = _styles!;
        string currentValue = setting.GetSerializedValue();
        string modifierLabel = GetControllerModifierDisplayName();
        ControllerHotkeyChoice[] choices = ControllerHotkeyChoices
            .Where(choice => !IsControllerActionLayerModifierKeyCode(choice.KeyCode))
            .ToArray();
        GUILayout.Space(Px(6f));
        for (int i = 0; i < choices.Length; i++)
        {
            if (i % 6 == 0)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(i == 0 ? "Controller" : string.Empty, styles.FooterText, GUILayout.Width(Px(96f)));
            }

            ControllerHotkeyChoice choice = choices[i];
            string serializedKey = choice.KeyCode.ToString();
            bool selected = string.Equals(currentValue, serializedKey, StringComparison.OrdinalIgnoreCase);
            if (GUILayout.Button(modifierLabel + "+" + choice.Label, selected ? styles.ActionButton : styles.SecondaryButton, GUILayout.Width(Px(112f)), GUILayout.Height(Px(30f))))
            {
                SaveControllerHotkeyChoice(setting, choice);
            }

            if (i % 6 == 5 || i == choices.Length - 1)
            {
                GUILayout.EndHorizontal();
            }
        }
    }

    private void SaveControllerHotkeyChoice(ManagedSetting setting, ControllerHotkeyChoice choice)
    {
        _captureSettingId = null;
        if (IsControllerActionLayerModifierKeyCode(choice.KeyCode))
        {
            _status = $"{GetControllerModifierDisplayName()} is reserved as the controller shortcut modifier.";
            return;
        }

        if (!SaveBoxedValue(setting, choice.KeyCode))
        {
            return;
        }

        if (!EnableControllerHotkeyLayer())
        {
            return;
        }

        RebuildControllerHotkeyLayerKeys();
        _status = $"Saved {setting.DisplayName} to {GetControllerModifierDisplayName()}+{choice.Label}.";
    }

    private bool EnableControllerHotkeyLayer()
    {
        string modifierActions = GetControllerModifierActionsForSave();
        if (!TrySaveManagerConfigValue(_controllerActionLayerEnabled, true, out string message) ||
            !TrySaveManagerConfigValue(_controllerActionLayerDryRun, false, out message) ||
            !TrySaveManagerConfigValue(_controllerActionLayerDispatchEnabled, true, out message) ||
            !TrySaveManagerConfigValue(_controllerActionLayerModifierActions, modifierActions, out message) ||
            !TrySaveManagerConfigValue(_controllerActionLayerControllerSourcesOnly, true, out message))
        {
            _status = message;
            return false;
        }

        ResetControllerActionLayerLogState();
        return true;
    }

    private static string GetControllerHotkeyDisplayName(KeyCode keyCode)
    {
        return ControllerHotkeyChoices.FirstOrDefault(choice => choice.KeyCode == keyCode)?.Label ?? keyCode.ToString();
    }

    private void DrawSerializedSetting(ManagedSetting setting)
    {
        UiStyles styles = _styles!;
        string currentValue = setting.GetSerializedValue();
        bool dirty = setting.DraftValue != currentValue;

        GUILayout.BeginHorizontal();
        setting.DraftValue = GUILayout.TextField(setting.DraftValue, styles.ValueField, GUILayout.MinWidth(Px(176f)), GUILayout.Height(Px(34f)));
        GUILayout.Label(dirty ? "Unsaved" : "Saved", dirty ? styles.DirtyPill : styles.SavedPill, GUILayout.Width(Px(84f)), GUILayout.Height(Px(34f)));
        if (GUILayout.Button("Apply", dirty ? styles.ActionButton : styles.SecondaryButton, GUILayout.Width(Px(92f)), GUILayout.Height(Px(34f))))
        {
            SaveSerializedValue(setting);
        }

        if (GUILayout.Button("Undo", styles.SecondaryButton, GUILayout.Width(Px(92f)), GUILayout.Height(Px(34f))))
        {
            setting.DraftValue = currentValue;
            _status = $"Reverted {setting.DisplayName}.";
        }

        GUILayout.EndHorizontal();
    }

    private void DrawRangedNumberSetting(ManagedSetting setting)
    {
        UiStyles styles = _styles!;
        if (setting.NumericRange is not NumericRange range)
        {
            DrawSerializedSetting(setting);
            return;
        }

        string currentValue = setting.GetSerializedValue();
        bool dirty = setting.DraftValue != currentValue;

        double draft = TryParseNumber(setting.DraftValue, out double parsedDraft)
            ? Clamp(parsedDraft, range.Minimum, range.Maximum)
            : ReadCurrentNumber(setting, range);

        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
        if (ShouldShowMultiplierPresets(range))
        {
            DrawRangePresetButton(setting, range, "0", 0d);
            DrawRangePresetButton(setting, range, "0.5", 0.5d);
            DrawRangePresetButton(setting, range, "1", 1d);
        }

        GUILayout.FlexibleSpace();
        GUILayout.Label("Value", styles.RangeEndpoint, GUILayout.Width(Px(60f)), GUILayout.Height(Px(34f)));
        string editedValue = GUILayout.TextField(setting.DraftValue, styles.ValueField, GUILayout.Width(Px(96f)), GUILayout.Height(Px(34f)));
        if (editedValue != setting.DraftValue)
        {
            setting.DraftValue = editedValue;
        }

        dirty = setting.DraftValue != currentValue;
        GUILayout.Label(dirty ? "Unsaved" : "Saved", dirty ? styles.DirtyPill : styles.SavedPill, GUILayout.Width(Px(84f)), GUILayout.Height(Px(34f)));
        if (GUILayout.Button("Apply", dirty ? styles.ActionButton : styles.SecondaryButton, GUILayout.Width(Px(92f)), GUILayout.Height(Px(34f))))
        {
            SaveSerializedValue(setting);
        }

        if (GUILayout.Button("Undo", styles.SecondaryButton, GUILayout.Width(Px(92f)), GUILayout.Height(Px(34f))))
        {
            setting.DraftValue = currentValue;
            _status = $"Reverted {setting.DisplayName}.";
        }

        GUILayout.EndHorizontal();
        GUILayout.Space(Px(4f));

        GUILayout.BeginHorizontal();
        GUILayout.Label(FormatRangeEndpoint(range.Minimum, range.IsInteger), styles.RangeEndpoint, GUILayout.Width(Px(52f)), GUILayout.Height(Px(28f)));
        float sliderValue = (float)draft;
        float moved = GUILayout.HorizontalSlider(
            sliderValue,
            (float)range.Minimum,
            (float)range.Maximum,
            styles.RangeSlider,
            styles.RangeSliderThumb,
            GUILayout.MinWidth(Px(240f)),
            GUILayout.Height(Px(28f)));

        double movedValue = range.IsInteger ? Math.Round(moved) : moved;
        if (Math.Abs(movedValue - draft) > 0.0001d)
        {
            setting.DraftValue = FormatNumber(movedValue, range.IsInteger);
            dirty = setting.DraftValue != currentValue;
        }

        GUILayout.Label(FormatRangeEndpoint(range.Maximum, range.IsInteger), styles.RangeEndpoint, GUILayout.Width(Px(52f)), GUILayout.Height(Px(28f)));
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }

    private void DrawFooter()
    {
        GUILayout.Space(Px(10f));
        GUILayout.BeginHorizontal(_styles!.Footer, GUILayout.ExpandWidth(true));
        GUILayout.Label(string.IsNullOrWhiteSpace(_status) ? "Ready. Changes save when applied." : _status, _styles.FooterText);
        GUILayout.FlexibleSpace();
        if (!string.IsNullOrEmpty(_captureSettingId))
        {
            GUILayout.Label("Esc cancels key capture", _styles.FooterText, GUILayout.Width(Px(180f)));
        }

        GUILayout.EndHorizontal();
    }

    private void DrawWindowFrameControls()
    {
        HandleResizeEvents();

        Rect resizeRect = GetResizeHandleRect();
        GUI.Label(resizeRect, "///", _styles!.ResizeHandle);

        if (!_resizingWindow)
        {
            float dragWidth = Mathf.Max(Px(180f), _window.width - Px(HeaderDragRightReserve));
            GUI.DragWindow(new Rect(0f, 0f, dragWidth, Px(HeaderDragHeight)));
        }
    }

    private void HandleResizeEvents()
    {
        Event current = Event.current;
        int controlId = GUIUtility.GetControlID(FocusType.Passive);
        EventType eventType = current.GetTypeForControl(controlId);
        Rect resizeRect = GetResizeHandleRect();

        if (eventType == EventType.MouseDown && current.button == 0 && resizeRect.Contains(current.mousePosition))
        {
            GUIUtility.hotControl = controlId;
            _resizingWindow = true;
            _resizeStartMouse = GetScreenMousePosition();
            _resizeStartWindow = _window;
            _windowFrameChangedThisFrame = true;
            current.Use();
            return;
        }

        if (GUIUtility.hotControl != controlId || !_resizingWindow)
        {
            return;
        }

        if (eventType == EventType.MouseDrag)
        {
            Vector2 currentMouse = GetScreenMousePosition();
            Vector2 delta = currentMouse - _resizeStartMouse;
            float maxWidth = GetMaximumWindowWidth();
            float maxHeight = GetMaximumWindowHeight();

            _window = new Rect(
                _resizeStartWindow.x,
                _resizeStartWindow.y,
                Mathf.Clamp(_resizeStartWindow.width + delta.x, GetMinimumWindowWidth(), maxWidth),
                Mathf.Clamp(_resizeStartWindow.height + delta.y, GetMinimumWindowHeight(), maxHeight));

            _window = ClampWindowToScreen(_window);
            _windowFrameChangedThisFrame = true;
            current.Use();
            return;
        }

        if (eventType == EventType.MouseUp)
        {
            GUIUtility.hotControl = 0;
            _resizingWindow = false;
            _windowFrameChangedThisFrame = true;
            current.Use();
        }
    }

    private Rect GetResizeHandleRect()
    {
        float handleSize = Px(ResizeHandleSize);
        float inset = Px(8f);
        return new Rect(_window.width - handleSize - inset, _window.height - handleSize - inset, handleSize, handleSize);
    }

    private void GrowWindowHeight()
    {
        float maxHeight = GetMaximumWindowHeight();
        float targetHeight = Mathf.Min(maxHeight, Mathf.Max(_window.height + Px(180f), Screen.height * 0.86f));
        float bottom = _window.y + _window.height;
        _window = ClampWindowToScreen(new Rect(
            _window.x,
            bottom - targetHeight,
            _window.width,
            targetHeight));
        _windowFrameChangedThisFrame = true;
    }

    private static Vector2 GetScreenMousePosition()
    {
        Vector3 mouse = Input.mousePosition;
        return new Vector2(mouse.x, Screen.height - mouse.y);
    }

    private void HandleKeyCaptureEvent()
    {
        if (string.IsNullOrEmpty(_captureSettingId))
        {
            return;
        }

        Event current = Event.current;
        if (current.type != EventType.KeyDown)
        {
            return;
        }

        if (current.keyCode == KeyCode.Escape)
        {
            _captureSettingId = null;
            _status = "Key capture cancelled.";
            current.Use();
            return;
        }

        if (current.keyCode == KeyCode.None)
        {
            return;
        }

        ManagedSetting? setting = FindSetting(_captureSettingId);
        if (setting != null)
        {
            if (IsUnityJoystickKeyCode(current.keyCode) && IsControllerActionLayerModifierKeyCode(current.keyCode))
            {
                _status = $"{GetControllerModifierDisplayName()} is reserved as the controller shortcut modifier.";
                _captureSettingId = null;
                current.Use();
                return;
            }

            if (SaveBoxedValue(setting, current.keyCode) && IsControllerHotkeyLayerKey(current.keyCode))
            {
                EnableControllerHotkeyLayer();
                _status = $"Saved {setting.DisplayName} to {GetControllerModifierDisplayName()}+{GetControllerHotkeyDisplayName(current.keyCode)}.";
            }
        }

        _captureSettingId = null;
        current.Use();
    }

    private void HandleWindowShortcuts()
    {
        if (!string.IsNullOrEmpty(_captureSettingId))
        {
            return;
        }

        Event current = Event.current;
        if (current.type == EventType.KeyDown && current.keyCode == KeyCode.Escape)
        {
            CloseManager();
            current.Use();
        }
    }

    private void CloseManager()
    {
        _visible = false;
        _captureSettingId = null;
        _resizingWindow = false;
        RefreshModUiInputCapture();
    }

    private bool HasModUiInputContext => _visible || _controllerCursorScopes.Count > 0 || _customUiScopes.Count > 0;

    private bool HasWorldFreezeContext => _worldFreezeScopes.Count > 0;

    private bool IsControllerCursorActiveInstance => HasModUiInputContext && _controllerCursorEnabled?.Value == true;

    private void SetControllerCursorScopeInstance(string ownerId, bool active)
    {
        string cleanOwnerId = NormalizeUiScopeOwnerId(ownerId);
        if (active)
        {
            _controllerCursorScopes.Add(cleanOwnerId);
        }
        else
        {
            _controllerCursorScopes.Remove(cleanOwnerId);
        }

        RefreshModUiInputCapture();
    }

    private void SetCustomUiScopeInstance(string ownerId, bool active, bool freezeWorld)
    {
        string cleanOwnerId = NormalizeUiScopeOwnerId(ownerId);
        if (active)
        {
            _customUiScopes.Add(cleanOwnerId);
            if (freezeWorld)
            {
                _worldFreezeScopes.Add(cleanOwnerId);
            }
            else
            {
                _worldFreezeScopes.Remove(cleanOwnerId);
            }
        }
        else
        {
            _customUiScopes.Remove(cleanOwnerId);
            _worldFreezeScopes.Remove(cleanOwnerId);
        }

        RefreshModUiInputCapture();
    }

    private static string NormalizeUiScopeOwnerId(string ownerId)
    {
        return string.IsNullOrWhiteSpace(ownerId) ? "<unknown>" : ownerId.Trim();
    }

    private void EnsureModUiInputCapture()
    {
        if (!HasModUiInputContext)
        {
            return;
        }

        RefreshWorldFreeze();
        CaptureCursorForManager();
        CaptureGameInputForManager();
    }

    private void RefreshModUiInputCapture()
    {
        RefreshWorldFreeze();
        if (HasModUiInputContext)
        {
            CaptureCursorForManager();
            CaptureGameInputForManager();
            return;
        }

        ReleaseControllerCursorMouseButton();
        _controllerCursorPositionInitialized = false;
        RestoreGameInputState();
        RestoreCursorState();
    }

    private void RefreshWorldFreeze()
    {
        if (HasWorldFreezeContext)
        {
            if (!_worldTimeScaleCaptured)
            {
                _previousTimeScale = Time.timeScale;
                _worldTimeScaleCaptured = true;
            }

            if (!Mathf.Approximately(Time.timeScale, 0f))
            {
                Time.timeScale = 0f;
            }

            return;
        }

        RestoreWorldTimeScale();
    }

    private void RestoreWorldTimeScale()
    {
        if (!_worldTimeScaleCaptured)
        {
            return;
        }

        Time.timeScale = _previousTimeScale;
        _previousTimeScale = 1f;
        _worldTimeScaleCaptured = false;
    }

    private void CaptureCursorForManager()
    {
        if (!_cursorStateCaptured)
        {
            _previousCursorLockState = Cursor.lockState;
            _previousCursorVisible = Cursor.visible;
            _cursorStateCaptured = true;
        }

        EnsureCursorForManager();
    }

    private static void EnsureCursorForManager()
    {
        if (Cursor.lockState != CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.None;
        }

        if (!Cursor.visible)
        {
            Cursor.visible = true;
        }
    }

    private void RestoreCursorState()
    {
        if (!_cursorStateCaptured)
        {
            return;
        }

        Cursor.lockState = _previousCursorLockState;
        Cursor.visible = _previousCursorVisible;
        _cursorStateCaptured = false;
    }

    private void CaptureGameInputForManager()
    {
        if (_inputModulesCaptured)
        {
            EnsureGameInputBlocked();
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
    }

    private void EnsureGameInputBlocked()
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

    private void RegisterManagerControllerActions()
    {
        RegisterControllerActionInstance("manager.open", "Open FoA Mod Manager", "Manager", "Open the shared mod manager panel.", OpenManager);
        RegisterControllerActionInstance("manager.close", "Close FoA Mod Manager", "Manager", "Close the shared mod manager panel.", CloseManager);
        RegisterControllerActionInstance("manager.toggle", "Toggle FoA Mod Manager", "Manager", "Open or close the shared mod manager panel.", ToggleManager);
        RegisterControllerActionInstance("manager.refresh", "Refresh FoA Mod Manager", "Manager", "Refresh discovered mods, settings, profiles, and warnings.", RefreshCatalog);
    }

    private bool RegisterControllerActionInstance(string actionId, string displayName, Action callback)
    {
        return RegisterControllerActionInstance(actionId, displayName, "General", string.Empty, callback);
    }

    private bool RegisterControllerActionInstance(string actionId, string displayName, string category, string description, Action callback)
    {
        if (callback == null)
        {
            return false;
        }

        string normalizedActionId = NormalizeControllerActionId(actionId);
        if (normalizedActionId.Length == 0)
        {
            return false;
        }

        string cleanedDisplayName = string.IsNullOrWhiteSpace(displayName)
            ? normalizedActionId
            : CleanProbeValue(displayName);
        string cleanedCategory = string.IsNullOrWhiteSpace(category)
            ? "General"
            : CleanProbeValue(category);
        string cleanedDescription = string.IsNullOrWhiteSpace(description)
            ? string.Empty
            : CleanProbeValue(description);
        _controllerActionRegistrations[normalizedActionId] = new ControllerActionRegistration(
            normalizedActionId,
            cleanedDisplayName,
            cleanedCategory,
            cleanedDescription,
            callback);
        return true;
    }

    private bool UnregisterControllerActionInstance(string actionId)
    {
        string normalizedActionId = NormalizeControllerActionId(actionId);
        return normalizedActionId.Length > 0 && _controllerActionRegistrations.Remove(normalizedActionId);
    }

    private StatusProviderRegistration[] GetStatusProviderRegistrationsForUi()
    {
        return _statusProviderRegistrations.Values
            .OrderBy(provider => provider.Category, StringComparer.OrdinalIgnoreCase)
            .ThenBy(provider => provider.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(provider => provider.ProviderId, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private bool RegisterStatusProviderInstance(string providerId, string displayName, Func<FoAModStatusSnapshot> snapshotProvider)
    {
        return RegisterStatusProviderInstance(providerId, displayName, "General", string.Empty, snapshotProvider);
    }

    private bool RegisterStatusProviderInstance(string providerId, string displayName, string category, string description, Func<FoAModStatusSnapshot> snapshotProvider)
    {
        if (snapshotProvider == null)
        {
            return false;
        }

        string normalizedProviderId = NormalizeStatusProviderId(providerId);
        if (normalizedProviderId.Length == 0)
        {
            return false;
        }

        string cleanedDisplayName = string.IsNullOrWhiteSpace(displayName)
            ? normalizedProviderId
            : CleanStatusText(displayName, 96);
        string cleanedCategory = string.IsNullOrWhiteSpace(category)
            ? "General"
            : CleanStatusText(category, 96);
        string cleanedDescription = string.IsNullOrWhiteSpace(description)
            ? string.Empty
            : CleanStatusText(description, StatusProviderTextLimit);
        _statusProviderRegistrations[normalizedProviderId] = new StatusProviderRegistration(
            normalizedProviderId,
            cleanedDisplayName,
            cleanedCategory,
            cleanedDescription,
            snapshotProvider);
        return true;
    }

    private bool UnregisterStatusProviderInstance(string providerId)
    {
        string normalizedProviderId = NormalizeStatusProviderId(providerId);
        return normalizedProviderId.Length > 0 && _statusProviderRegistrations.Remove(normalizedProviderId);
    }

    private void UpdateControllerActionLayer()
    {
        if (_controllerActionLayerEnabled?.Value != true)
        {
            return;
        }

        int maxLogs = Mathf.Clamp(_controllerActionLayerMaxLogsPerSession?.Value ?? 80, 0, 1000);
        ControllerActionBinding[] bindings = ParseControllerActionLayerBindings(_controllerActionLayerBindings?.Value);
        if (bindings.Length == 0)
        {
            return;
        }

        string[] modifierActions = SplitControllerCursorActions(_controllerActionLayerModifierActions?.Value);
        if (modifierActions.Length == 0)
        {
            return;
        }

        object? player = ResolveControllerInputPlayer("Controller action layer");
        if (player == null)
        {
            return;
        }

        bool modifierHeld;
        string modifierAction;
        ControllerProbePhysicalSourceInfo modifierSource;
        _controllerActionLayerInputReadDepth++;
        try
        {
            modifierHeld = TryReadFirstHeldControllerAction(player, modifierActions, out modifierAction);
            modifierSource = modifierHeld
                ? ResolveControllerChordProbeSource(player, modifierAction)
                : ControllerProbePhysicalSourceInfo.Unknown("<modifier-not-held>");
        }
        finally
        {
            _controllerActionLayerInputReadDepth--;
        }

        if (!modifierHeld)
        {
            return;
        }

        if (_controllerActionLayerControllerSourcesOnly?.Value == true && !modifierSource.HasControllerSource)
        {
            return;
        }

        _controllerActionLayerModifierHeldFrame = Time.frameCount;
        _controllerActionLayerInputReadDepth++;
        try
        {
            foreach (ControllerActionBinding binding in bindings)
            {
                ControllerChordProbeTargetState targetState = ReadControllerChordProbeTargetState(player, binding.TargetAction);
                if (!targetState.IsDown)
                {
                    continue;
                }

                ControllerProbePhysicalSourceInfo targetSource = ResolveControllerChordProbeSource(player, binding.TargetAction);
                if (_controllerActionLayerControllerSourcesOnly?.Value == true && !targetSource.HasControllerSource)
                {
                    continue;
                }

                _controllerActionRegistrations.TryGetValue(binding.ActionId, out ControllerActionRegistration registration);
                RecordControllerActionLayerRoute(
                    modifierAction,
                    binding,
                    targetState.IsHeld,
                    targetSource,
                    registration,
                    maxLogs);
            }
        }
        finally
        {
            _controllerActionLayerInputReadDepth--;
        }
    }

    private void RecordControllerActionLayerRoute(
        string modifierAction,
        ControllerActionBinding binding,
        bool targetHeld,
        ControllerProbePhysicalSourceInfo targetSource,
        ControllerActionRegistration? registration,
        int maxLogs)
    {
        string logKey = "action-layer\n" + modifierAction + "\n" + binding.TargetAction + "\n" + binding.ActionId;
        bool canLog = TryReserveControllerActionLayerLog(logKey, maxLogs);

        bool dryRun = _controllerActionLayerDryRun?.Value != false;
        bool dispatchEnabled = _controllerActionLayerDispatchEnabled?.Value == true;
        if (!dryRun && !dispatchEnabled && !_controllerActionLayerDispatchGateWarningLogged)
        {
            _controllerActionLayerDispatchGateWarningLogged = true;
            Logger.LogWarning("Controller action layer DryRun=false was requested, but DispatchEnabled=false. Route rows will be logged without dispatch or input suppression.");
        }

        bool registrationFound = registration != null;
        string registeredActionId = registration?.ActionId ?? binding.ActionId;
        string displayName = registration?.DisplayName ?? "<missing>";
        bool hasCallback = registration?.Callback != null;
        bool contextAllowed = IsControllerActionLayerDispatchContextAllowed(registration);
        bool shouldDispatch = !dryRun && dispatchEnabled && registrationFound && hasCallback && contextAllowed;
        bool dispatchBlockedByGate = dryRun || !dispatchEnabled || !registrationFound || !hasCallback;
        bool dispatchBlockedByContext = !contextAllowed;
        if (canLog)
        {
            Logger.LogInfo(
                "Controller action layer route: " +
                $"modifierAction={CleanProbeValue(modifierAction)}; modifierHeld=true; " +
                $"targetAction={CleanProbeValue(binding.TargetAction)}; targetPressed=true; targetHeld={(targetHeld ? "true" : "false")}; " +
                $"registeredAction={CleanProbeValue(registeredActionId)}; registeredActionFound={(registrationFound ? "true" : "false")}; " +
                $"displayName={displayName}; hasCallback={(hasCallback ? "true" : "false")}; dryRun={(dryRun ? "true" : "false")}; " +
                $"dispatchEnabled={(dispatchEnabled ? "true" : "false")}; contextAllowed={(contextAllowed ? "true" : "false")}; " +
                targetSource.ToLogSegment() + "; " +
                $"source=rewired-direct-sample; dispatch={(shouldDispatch ? "true" : "false")}; " +
                $"dispatchBlockedByGate={(dispatchBlockedByGate ? "true" : "false")}; " +
                $"dispatchBlockedByContext={(dispatchBlockedByContext ? "true" : "false")}; " +
                $"vanillaInputSuppressed={(shouldDispatch ? "true" : "false")}; " +
                $"count={_controllerActionLayerLogCount}/{maxLogs}.");
        }

        if (shouldDispatch && registration != null)
        {
            DispatchControllerActionLayerRoute(modifierAction, binding, registration);
        }
    }

    private bool IsControllerActionLayerDispatchContextAllowed(ControllerActionRegistration? registration)
    {
        return registration != null && !HasModUiInputContext;
    }

    private void DispatchControllerActionLayerRoute(
        string modifierAction,
        ControllerActionBinding binding,
        ControllerActionRegistration registration)
    {
        try
        {
            ArmControllerActionLayerTargetSuppression(binding);
            registration.Callback();
            Logger.LogInfo(
                "Controller action layer dispatched: " +
                $"modifierAction={CleanProbeValue(modifierAction)}; targetAction={CleanProbeValue(binding.TargetAction)}; " +
                $"registeredAction={CleanProbeValue(registration.ActionId)}; displayName={registration.DisplayName}; " +
                "source=rewired-direct-sample; dispatch=true; vanillaInputSuppressed=true.");
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"Controller action layer dispatch failed for {registration.ActionId}: {ex}");
        }
    }

    private void ArmControllerActionLayerTargetSuppression(ControllerActionBinding binding)
    {
        _controllerActionLayerSuppressedActionNames.Clear();
        _controllerActionLayerSuppressedActionIds.Clear();

        string targetAction = CleanProbeValue(binding.TargetAction);
        if (targetAction.Length > 0)
        {
            _controllerActionLayerSuppressedActionNames.Add(targetAction);
        }

        if (int.TryParse(targetAction, NumberStyles.Integer, CultureInfo.InvariantCulture, out int targetActionId))
        {
            _controllerActionLayerSuppressedActionIds.Add(targetActionId);
        }

        ControllerProbeActionInfo? actionInfo = ResolveControllerChordProbeActionInfo(targetAction);
        if (actionInfo != null)
        {
            _controllerActionLayerSuppressedActionIds.Add(actionInfo.Id);
            _controllerActionLayerSuppressedActionNames.Add(actionInfo.Name);
        }

        _controllerActionLayerSuppressionFrame = Time.frameCount;
    }

    private bool ShouldSuppressControllerActionLayerButtonInstance(string actionName)
    {
        if (IsInternalControllerInputReadActive || HasModUiInputContext)
        {
            return false;
        }

        ClearExpiredControllerActionLayerSuppression();
        return _controllerActionLayerSuppressedActionNames.Contains(CleanProbeValue(actionName));
    }

    private bool ShouldSuppressControllerActionLayerButtonInstance(int actionId)
    {
        if (IsInternalControllerInputReadActive || HasModUiInputContext)
        {
            return false;
        }

        ClearExpiredControllerActionLayerSuppression();
        return _controllerActionLayerSuppressedActionIds.Contains(actionId);
    }

    private bool ShouldBlockControllerActionLayerButtonInstance(string actionName)
    {
        return IsControllerActionLayerModifierLockActive() &&
            !IsControllerActionLayerModifierToken(actionName);
    }

    private bool ShouldBlockControllerActionLayerButtonInstance(int actionId)
    {
        return IsControllerActionLayerModifierLockActive() &&
            !IsControllerActionLayerModifierToken(actionId);
    }

    private bool IsControllerActionLayerModifierLockActive()
    {
        if (!AreControllerShortcutsEnabled || IsInternalControllerInputReadActive || HasModUiInputContext)
        {
            return false;
        }

        string[] modifierActions = SplitControllerCursorActions(_controllerActionLayerModifierActions?.Value);
        foreach (string modifierAction in modifierActions)
        {
            if (TryReadUnityKeyButton("GetButton", modifierAction, out bool held) && held)
            {
                return true;
            }
        }

        return _controllerActionLayerModifierHeldFrame == Time.frameCount;
    }

    private bool ShouldBlockControllerHotkeyKeyInstance(KeyCode keyCode)
    {
        if (IsControllerActionLayerModifierKeyCode(keyCode) ||
            !IsControllerHotkeyLayerKey(keyCode) ||
            IsInternalControllerInputReadActive ||
            HasModUiInputContext ||
            !_controllerHotkeyLayerKeys.Contains(keyCode) ||
            !IsControllerActionLayerGateArmed)
        {
            return false;
        }

        return !IsControllerActionLayerModifierLockActive();
    }

    private bool IsControllerActionLayerModifierToken(string actionName)
    {
        return IsControllerActionLayerModifierTarget(actionName);
    }

    private bool IsControllerActionLayerModifierToken(int actionId)
    {
        string[] modifierActions = SplitControllerCursorActions(_controllerActionLayerModifierActions?.Value);
        if (modifierActions.Length == 0)
        {
            modifierActions = SplitControllerCursorActions(ControllerActionLayerDefaultModifierActions);
        }

        foreach (string modifierAction in modifierActions)
        {
            if (int.TryParse(modifierAction, NumberStyles.Integer, CultureInfo.InvariantCulture, out int modifierActionId) &&
                modifierActionId == actionId)
            {
                return true;
            }

            if (_controllerProbeActionsByName.TryGetValue(modifierAction, out ControllerProbeActionInfo actionInfo) &&
                actionInfo.Id == actionId)
            {
                return true;
            }
        }

        return false;
    }

    private void ClearExpiredControllerActionLayerSuppression()
    {
        if (_controllerActionLayerSuppressionFrame == Time.frameCount)
        {
            return;
        }

        _controllerActionLayerSuppressedActionNames.Clear();
        _controllerActionLayerSuppressedActionIds.Clear();
        _controllerActionLayerSuppressionFrame = -1;
    }

    private bool TryReserveControllerActionLayerLog(string logKey, int maxLogs)
    {
        if (maxLogs <= 0)
        {
            return false;
        }

        if (_controllerActionLayerLogCount >= maxLogs)
        {
            if (!_controllerActionLayerMaxLogWarningLogged)
            {
                _controllerActionLayerMaxLogWarningLogged = true;
                Logger.LogWarning("Controller action layer reached MaxLogsPerSession. Disable and re-enable the plugin or raise ControllerActionLayer.MaxLogsPerSession for more rows.");
            }

            return false;
        }

        float now = Time.unscaledTime;
        float repeatSuppressSeconds = Mathf.Max(
            0f,
            _controllerActionLayerRepeatSuppressSeconds?.Value ?? ControllerActionLayerDefaultRepeatSuppressSeconds);
        if (repeatSuppressSeconds > 0f &&
            _controllerActionLayerLastLogTimes.TryGetValue(logKey, out float lastLoggedAt) &&
            now - lastLoggedAt < repeatSuppressSeconds)
        {
            return false;
        }

        _controllerActionLayerLastLogTimes[logKey] = now;
        _controllerActionLayerLogCount++;
        return true;
    }

    private void UpdateControllerChordProbe()
    {
        if (_controllerChordProbeEnabled?.Value != true)
        {
            _controllerChordProbeModifierWasHeld = false;
            _controllerChordProbeNextTargetScanAt = 0f;
            return;
        }

        int maxLogs = Mathf.Clamp(_controllerChordProbeMaxLogsPerSession?.Value ?? 80, 0, 1000);
        if (maxLogs == 0)
        {
            return;
        }

        object? player = ResolveControllerInputPlayer("Controller chord probe");
        if (player == null)
        {
            return;
        }

        string[] modifierActions = SplitControllerCursorActions(_controllerChordProbeModifierActions?.Value);
        if (modifierActions.Length == 0)
        {
            return;
        }

        string[] targetActions = SplitControllerCursorActions(_controllerChordProbeTargetActions?.Value);
        bool modifierHeld;
        string modifierAction;
        ControllerProbePhysicalSourceInfo modifierSource;
        _controllerChordProbeInputReadDepth++;
        try
        {
            modifierHeld = TryReadFirstHeldControllerAction(player, modifierActions, out modifierAction);
            modifierSource = modifierHeld
                ? ResolveControllerChordProbeSource(player, modifierAction)
                : ControllerProbePhysicalSourceInfo.Unknown("<modifier-not-held>");
        }
        finally
        {
            _controllerChordProbeInputReadDepth--;
        }

        if (modifierHeld)
        {
            _controllerChordProbeLastModifierAction = modifierAction;
        }
        else if (_controllerChordProbeModifierWasHeld)
        {
            modifierAction = _controllerChordProbeLastModifierAction;
        }

        bool modifierJustChanged = modifierHeld != _controllerChordProbeModifierWasHeld;
        bool modifierJustPressed = modifierHeld && !_controllerChordProbeModifierWasHeld;
        _controllerChordProbeModifierWasHeld = modifierHeld;

        if (modifierJustChanged)
        {
            RecordControllerChordProbeModifierState(modifierHeld, modifierAction, modifierSource, maxLogs);
        }

        if (!modifierHeld || targetActions.Length == 0)
        {
            if (!modifierHeld)
            {
                _controllerChordProbeNextTargetScanAt = 0f;
            }

            return;
        }

        _controllerChordProbeInputReadDepth++;
        try
        {
            List<ControllerChordProbeTargetState> targetStates = new();
            foreach (string targetAction in targetActions)
            {
                ControllerChordProbeTargetState targetState = ReadControllerChordProbeTargetState(player, targetAction);
                targetStates.Add(targetState);

                bool targetHeld = targetState.IsHeld;
                bool targetPressed = targetState.IsDown ||
                    (modifierJustPressed && targetHeld);
                if (!targetPressed)
                {
                    continue;
                }

                ControllerProbePhysicalSourceInfo targetSource = ResolveControllerChordProbeSource(player, targetAction);
                RecordControllerChordProbeTargetState(
                    modifierAction,
                    targetAction,
                    targetHeld,
                    targetSource,
                    maxLogs);
            }

            RecordControllerChordProbeTargetScanState(
                modifierAction,
                targetStates,
                modifierJustPressed,
                maxLogs);
        }
        finally
        {
            _controllerChordProbeInputReadDepth--;
        }
    }

    private static bool TryReadFirstHeldControllerAction(object player, string[] actionNames, out string actionName)
    {
        foreach (string candidate in actionNames)
        {
            if (ReadControllerCursorButton(player, "GetButton", candidate))
            {
                actionName = candidate;
                return true;
            }
        }

        actionName = "<none>";
        return false;
    }

    private ControllerProbePhysicalSourceInfo ResolveControllerChordProbeSource(object player, string actionName)
    {
        string cleanedActionName = CleanProbeValue(actionName);
        if (TryParseControllerActionKeyCode(cleanedActionName, out KeyCode keyCode) && IsUnityJoystickKeyCode(keyCode))
        {
            return ControllerProbePhysicalSourceInfo.UnityJoystick(keyCode);
        }

        ControllerProbeActionInfo? actionInfo = ResolveControllerChordProbeActionInfo(cleanedActionName);
        return ResolveControllerProbePhysicalSourceInfo(player, "name", cleanedActionName, actionInfo);
    }

    private ControllerProbeActionInfo? ResolveControllerChordProbeActionInfo(string actionName)
    {
        EnsureControllerProbeActionMapLoaded();
        return ResolveControllerProbeActionInfo("name", CleanProbeValue(actionName));
    }

    private ControllerChordProbeTargetState ReadControllerChordProbeTargetState(object player, string targetAction)
    {
        ControllerProbeActionInfo? actionInfo = ResolveControllerChordProbeActionInfo(targetAction);
        bool nameHeld = ReadControllerCursorButton(player, "GetButton", targetAction);
        bool nameDown = ReadControllerCursorButton(player, "GetButtonDown", targetAction);
        bool idHeld = actionInfo != null && ReadControllerCursorButton(player, "GetButton", actionInfo.Id);
        bool idDown = actionInfo != null && ReadControllerCursorButton(player, "GetButtonDown", actionInfo.Id);

        return new ControllerChordProbeTargetState(targetAction, actionInfo, nameHeld, nameDown, idHeld, idDown);
    }

    private void RecordControllerChordProbeModifierState(
        bool modifierHeld,
        string modifierAction,
        ControllerProbePhysicalSourceInfo modifierSource,
        int maxLogs)
    {
        if (_controllerChordProbeControllerSourcesOnly?.Value == true &&
            modifierHeld &&
            !modifierSource.HasControllerSource)
        {
            return;
        }

        string state = modifierHeld ? "pressed" : "released";
        string logKey = "modifier\n" + state + "\n" + modifierAction;
        if (!TryReserveControllerChordProbeLog(logKey, maxLogs))
        {
            return;
        }

        Logger.LogInfo(
            "Controller chord probe modifier: " +
            $"modifierAction={CleanProbeValue(modifierAction)}; modifierHeld={(modifierHeld ? "true" : "false")}; " +
            modifierSource.ToFilterLogSegment() + "; " +
            "source=rewired-direct-sample; diagnosticOnly=true; dispatch=false; vanillaInputSuppressed=false; " +
            $"count={_controllerChordProbeLogCount}/{maxLogs}.");
    }

    private void RecordControllerChordProbeTargetState(
        string modifierAction,
        string targetAction,
        bool targetHeld,
        ControllerProbePhysicalSourceInfo targetSource,
        int maxLogs)
    {
        if (_controllerChordProbeControllerSourcesOnly?.Value == true && !targetSource.HasControllerSource)
        {
            return;
        }

        string logKey = "target\n" + modifierAction + "\n" + targetAction;
        if (!TryReserveControllerChordProbeLog(logKey, maxLogs))
        {
            return;
        }

        Logger.LogInfo(
            "Controller chord probe target: " +
            $"modifierAction={CleanProbeValue(modifierAction)}; modifierHeld=true; " +
            $"targetAction={CleanProbeValue(targetAction)}; targetPressed=true; targetHeld={(targetHeld ? "true" : "false")}; " +
            targetSource.ToLogSegment() + "; " +
            "source=rewired-direct-sample; diagnosticOnly=true; dispatch=false; vanillaInputSuppressed=false; " +
            $"count={_controllerChordProbeLogCount}/{maxLogs}.");
    }

    private void RecordControllerChordProbeTargetScanState(
        string modifierAction,
        IReadOnlyList<ControllerChordProbeTargetState> targetStates,
        bool modifierJustPressed,
        int maxLogs)
    {
        if (targetStates.Count == 0)
        {
            return;
        }

        float now = Time.unscaledTime;
        bool anyDown = targetStates.Any(state => state.IsDown);
        if (!modifierJustPressed && !anyDown && now < _controllerChordProbeNextTargetScanAt)
        {
            return;
        }

        float intervalSeconds = Mathf.Clamp(
            _controllerChordProbeTargetScanIntervalSeconds?.Value ?? ControllerChordProbeDefaultTargetScanIntervalSeconds,
            0.1f,
            10f);
        string targetStateSegment = string.Join(" | ", targetStates.Select(state => state.ToScanEntry()));
        string logKey = "target-scan\n" + modifierAction + "\n" + targetStateSegment;
        if (!TryReserveControllerChordProbeLog(logKey, maxLogs))
        {
            return;
        }

        _controllerChordProbeNextTargetScanAt = now + intervalSeconds;
        Logger.LogInfo(
            "Controller chord probe target scan: " +
            $"modifierAction={CleanProbeValue(modifierAction)}; modifierHeld=true; " +
            $"targetStates=[{targetStateSegment}]; " +
            "source=rewired-direct-sample; diagnosticOnly=true; dispatch=false; vanillaInputSuppressed=false; " +
            $"count={_controllerChordProbeLogCount}/{maxLogs}.");
    }

    private bool TryReserveControllerChordProbeLog(string logKey, int maxLogs)
    {
        if (_controllerChordProbeLogCount >= maxLogs)
        {
            if (!_controllerChordProbeMaxLogWarningLogged)
            {
                _controllerChordProbeMaxLogWarningLogged = true;
                Logger.LogWarning("Controller chord probe reached MaxLogsPerSession. Disable and re-enable the plugin or raise ControllerChordProbe.MaxLogsPerSession for more rows.");
            }

            return false;
        }

        float now = Time.unscaledTime;
        float repeatSuppressSeconds = Mathf.Max(0f, _controllerChordProbeRepeatSuppressSeconds?.Value ?? 0.5f);
        if (repeatSuppressSeconds > 0f &&
            _controllerChordProbeLastLogTimes.TryGetValue(logKey, out float lastLoggedAt) &&
            now - lastLoggedAt < repeatSuppressSeconds)
        {
            return false;
        }

        _controllerChordProbeLastLogTimes[logKey] = now;
        _controllerChordProbeLogCount++;
        return true;
    }

    private void UpdateControllerCursor()
    {
        if (!IsControllerCursorActiveInstance)
        {
            ReleaseControllerCursorMouseButton();
            return;
        }

        if (!IsWindowsRuntime())
        {
            ReleaseControllerCursorMouseButton();
            return;
        }

        EnsureCursorForManager();
        if (!_controllerCursorPositionInitialized)
        {
            _controllerCursorPosition = ClampControllerCursorPosition(GetScreenMousePosition());
            _controllerCursorPositionInitialized = true;
        }

        object? player = ResolveControllerInputPlayer("Controller cursor");
        Vector2 movement = Vector2.zero;
        bool primaryClickHeld = Input.GetKey(KeyCode.JoystickButton0);

        _controllerCursorInputReadDepth++;
        try
        {
            if (player != null)
            {
                float moveX = ReadControllerCursorSignedAxis(
                    player,
                    SplitControllerCursorActions(_controllerCursorRightActions?.Value),
                    SplitControllerCursorActions(_controllerCursorLeftActions?.Value));
                float moveY = ReadControllerCursorSignedAxis(
                    player,
                    SplitControllerCursorActions(_controllerCursorDownActions?.Value),
                    SplitControllerCursorActions(_controllerCursorUpActions?.Value));

                movement += new Vector2(moveX, moveY);
                primaryClickHeld |= ReadAnyControllerCursorButton(
                    player,
                    SplitControllerCursorActions(_controllerCursorPrimaryClickActions?.Value));
            }
        }
        finally
        {
            _controllerCursorInputReadDepth--;
        }

        movement += ReadUnityControllerCursorFallback();
        float deadzone = Mathf.Clamp(_controllerCursorDeadzone?.Value ?? ControllerCursorDefaultDeadzone, 0f, 0.75f);
        if (Mathf.Abs(movement.x) < deadzone)
        {
            movement.x = 0f;
        }

        if (Mathf.Abs(movement.y) < deadzone)
        {
            movement.y = 0f;
        }

        if (movement.sqrMagnitude > 1f)
        {
            movement.Normalize();
        }

        if (movement.sqrMagnitude > 0f)
        {
            float speed = Mathf.Clamp(_controllerCursorSpeed?.Value ?? ControllerCursorDefaultSpeed, 120f, 2400f);
            float deltaTime = Mathf.Clamp(Time.unscaledDeltaTime, 0f, 0.05f);
            _controllerCursorPosition = ClampControllerCursorPosition(_controllerCursorPosition + movement * speed * deltaTime);
            TrySetSystemCursorPosition(_controllerCursorPosition);
        }

        SetControllerCursorMouseButton(primaryClickHeld);
    }

    private object? ResolveControllerInputPlayer(string featureName)
    {
        if (_controllerCursorPlayer != null)
        {
            return _controllerCursorPlayer;
        }

        float now = Time.unscaledTime;
        if (now < _controllerCursorNextPlayerResolveAt)
        {
            return null;
        }

        _controllerCursorNextPlayerResolveAt = now + 5f;
        try
        {
            Type? reInputType = AccessTools.TypeByName("Rewired.ReInput");
            if (reInputType == null)
            {
                LogControllerInputPlayerUnavailable(featureName, "Rewired.ReInput type was not found.");
                return null;
            }

            PropertyInfo? isReadyProperty = reInputType.GetProperty("isReady", BindingFlags.Public | BindingFlags.Static);
            if (isReadyProperty?.GetValue(null) is bool isReady && !isReady)
            {
                return null;
            }

            object? players = reInputType.GetProperty("players", BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
            if (players == null)
            {
                LogControllerInputPlayerUnavailable(featureName, "Rewired players service was not available.");
                return null;
            }

            MethodInfo? getPlayer = players.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(method =>
                {
                    if (!string.Equals(method.Name, "GetPlayer", StringComparison.Ordinal))
                    {
                        return false;
                    }

                    ParameterInfo[] parameters = method.GetParameters();
                    return parameters.Length == 1 && parameters[0].ParameterType == typeof(int);
                });

            _controllerCursorPlayer = getPlayer?.Invoke(players, new object[] { 0 });
            if (_controllerCursorPlayer == null)
            {
                LogControllerInputPlayerUnavailable(featureName, "Rewired player 0 was not available.");
            }

            return _controllerCursorPlayer;
        }
        catch (Exception ex)
        {
            LogControllerInputPlayerUnavailable(featureName, $"{ex.GetType().Name}: {CleanProbeValue(ex.Message)}");
            return null;
        }
    }

    private void LogControllerInputPlayerUnavailable(string featureName, string reason)
    {
        if (_controllerCursorPlayerUnavailableLogged)
        {
            return;
        }

        _controllerCursorPlayerUnavailableLogged = true;
        Logger.LogWarning($"{featureName} Rewired input unavailable: {reason} Unity joystick button/axis fallback remains active.");
    }

    private static float ReadControllerCursorSignedAxis(object player, string[] positiveActions, string[] negativeActions)
    {
        float positive = ReadControllerCursorActionAmount(player, positiveActions);
        float negative = ReadControllerCursorActionAmount(player, negativeActions);
        return Mathf.Clamp(positive - negative, -1f, 1f);
    }

    private static float ReadControllerCursorActionAmount(object player, string[] actionNames)
    {
        float amount = 0f;
        foreach (string actionName in actionNames)
        {
            amount = Mathf.Max(amount, Mathf.Abs(ReadControllerCursorAxisRaw(player, actionName)));
            if (ReadControllerCursorButton(player, "GetButton", actionName))
            {
                amount = 1f;
            }
        }

        return Mathf.Clamp01(amount);
    }

    private static bool ReadAnyControllerCursorButton(object player, string[] actionNames)
    {
        foreach (string actionName in actionNames)
        {
            if (ReadControllerCursorButton(player, "GetButton", actionName))
            {
                return true;
            }
        }

        return false;
    }

    private static float ReadControllerCursorAxisRaw(object player, string actionName)
    {
        try
        {
            object? value = InvokeRewiredPlayerActionMethod(player, "GetAxisRaw", actionName);
            return value == null ? 0f : Convert.ToSingle(value, CultureInfo.InvariantCulture);
        }
        catch
        {
            return 0f;
        }
    }

    private static bool ReadControllerCursorButton(object player, string methodName, string actionName)
    {
        if (TryReadUnityKeyButton(methodName, actionName, out bool unityPressed))
        {
            return unityPressed;
        }

        try
        {
            object? value = InvokeRewiredPlayerActionMethod(player, methodName, actionName);
            return value is bool pressed && pressed;
        }
        catch
        {
            return false;
        }
    }

    private static bool TryReadUnityKeyButton(string methodName, string actionName, out bool pressed)
    {
        pressed = false;
        if (!TryParseControllerActionKeyCode(actionName, out KeyCode keyCode))
        {
            return false;
        }

        pressed = methodName switch
        {
            "GetButtonDown" or "GetButtonDoublePressDown" or "GetButtonSinglePressDown" or "GetButtonTimedPressDown" or "GetAnyButtonDown" => Input.GetKeyDown(keyCode),
            "GetButtonUp" or "GetButtonTimedPressUp" or "GetAnyButtonUp" => Input.GetKeyUp(keyCode),
            _ => Input.GetKey(keyCode)
        };
        return true;
    }

    private static bool ReadControllerChordProbeButton(
        object player,
        string methodName,
        string actionName,
        ControllerProbeActionInfo? actionInfo)
    {
        if (ReadControllerCursorButton(player, methodName, actionName))
        {
            return true;
        }

        return actionInfo != null && ReadControllerCursorButton(player, methodName, actionInfo.Id);
    }

    private static bool ReadControllerCursorButton(object player, string methodName, int actionId)
    {
        try
        {
            object? value = InvokeRewiredPlayerActionMethod(player, methodName, actionId);
            return value is bool pressed && pressed;
        }
        catch
        {
            return false;
        }
    }

    private static object? InvokeRewiredPlayerActionMethod(object player, string methodName, string actionName)
    {
        MethodInfo? method = player.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(candidate =>
            {
                if (!string.Equals(candidate.Name, methodName, StringComparison.Ordinal))
                {
                    return false;
                }

                ParameterInfo[] parameters = candidate.GetParameters();
                return parameters.Length == 1 && parameters[0].ParameterType == typeof(string);
            });

        return method?.Invoke(player, new object[] { actionName });
    }

    private static object? InvokeRewiredPlayerActionMethod(object player, string methodName, int actionId)
    {
        MethodInfo? method = player.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(candidate =>
            {
                if (!string.Equals(candidate.Name, methodName, StringComparison.Ordinal))
                {
                    return false;
                }

                ParameterInfo[] parameters = candidate.GetParameters();
                return parameters.Length == 1 && parameters[0].ParameterType == typeof(int);
            });

        return method?.Invoke(player, new object[] { actionId });
    }

    private static ControllerActionBinding[] ParseControllerActionLayerBindings(string? rawBindings)
    {
        if (string.IsNullOrWhiteSpace(rawBindings))
        {
            return Array.Empty<ControllerActionBinding>();
        }

        Dictionary<string, ControllerActionBinding> bindings = new(StringComparer.OrdinalIgnoreCase);
        string[] rows = rawBindings.Split(new[] { ';', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (string row in rows)
        {
            string trimmed = row.Trim();
            int separatorIndex = trimmed.IndexOf('=');
            if (separatorIndex < 0)
            {
                separatorIndex = trimmed.IndexOf(':');
            }

            if (separatorIndex <= 0 || separatorIndex >= trimmed.Length - 1)
            {
                continue;
            }

            string targetAction = trimmed.Substring(0, separatorIndex).Trim();
            string actionId = NormalizeControllerActionId(trimmed.Substring(separatorIndex + 1));
            if (targetAction.Length == 0 || actionId.Length == 0)
            {
                continue;
            }

            bindings[targetAction] = new ControllerActionBinding(targetAction, actionId);
        }

        return bindings.Values.ToArray();
    }

    private static string NormalizeControllerActionId(string? actionId)
    {
        return string.IsNullOrWhiteSpace(actionId) ? string.Empty : actionId.Trim();
    }

    private static string NormalizeStatusProviderId(string? providerId)
    {
        return string.IsNullOrWhiteSpace(providerId) ? string.Empty : CleanStatusText(providerId, 128);
    }

    private static string[] SplitControllerCursorActions(string? actions)
    {
        if (string.IsNullOrWhiteSpace(actions))
        {
            return Array.Empty<string>();
        }

        return actions.Split(new[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(action => action.Trim())
            .Where(action => action.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static Vector2 ReadUnityControllerCursorFallback()
    {
        Vector2 movement = Vector2.zero;
        movement.x += ReadUnityAxisRaw("Horizontal");
        movement.y -= ReadUnityAxisRaw("Vertical");
        return movement;
    }

    private static float ReadUnityAxisRaw(string axisName)
    {
        try
        {
            return Input.GetAxisRaw(axisName);
        }
        catch
        {
            return 0f;
        }
    }

    private static Vector2 ClampControllerCursorPosition(Vector2 position)
    {
        return new Vector2(
            Mathf.Clamp(position.x, 0f, Mathf.Max(0f, Screen.width - 1f)),
            Mathf.Clamp(position.y, 0f, Mathf.Max(0f, Screen.height - 1f)));
    }

    private static bool TrySetSystemCursorPosition(Vector2 screenPosition)
    {
        if (!TryGetForegroundGameClient(out WinPoint clientOrigin, out int clientWidth, out int clientHeight))
        {
            return false;
        }

        float widthScale = Screen.width <= 0 ? 1f : clientWidth / (float)Screen.width;
        float heightScale = Screen.height <= 0 ? 1f : clientHeight / (float)Screen.height;
        int desktopX = clientOrigin.X + Mathf.RoundToInt(screenPosition.x * widthScale);
        int desktopY = clientOrigin.Y + Mathf.RoundToInt(screenPosition.y * heightScale);
        return SetCursorPos(desktopX, desktopY);
    }

    private void SetControllerCursorMouseButton(bool pressed)
    {
        if (pressed == _controllerCursorMouseButtonDown)
        {
            return;
        }

        if (pressed && !TryIsForegroundGameWindow())
        {
            return;
        }

        MouseEvent(pressed ? MouseEventLeftDown : MouseEventLeftUp);
        _controllerCursorMouseButtonDown = pressed;
    }

    private void ReleaseControllerCursorMouseButton()
    {
        if (!_controllerCursorMouseButtonDown)
        {
            return;
        }

        MouseEvent(MouseEventLeftUp);
        _controllerCursorMouseButtonDown = false;
    }

    private static void MouseEvent(uint eventFlags)
    {
        if (IsWindowsRuntime())
        {
            mouse_event(eventFlags, 0, 0, 0, UIntPtr.Zero);
        }
    }

    private static bool TryGetForegroundGameClient(out WinPoint clientOrigin, out int clientWidth, out int clientHeight)
    {
        clientOrigin = default;
        clientWidth = 0;
        clientHeight = 0;

        IntPtr window = GetForegroundWindow();
        if (!IsWindowOwnedByCurrentProcess(window) || !GetClientRect(window, out WinRect rect))
        {
            return false;
        }

        WinPoint origin = new() { X = rect.Left, Y = rect.Top };
        if (!ClientToScreen(window, ref origin))
        {
            return false;
        }

        clientOrigin = origin;
        clientWidth = Math.Max(1, rect.Right - rect.Left);
        clientHeight = Math.Max(1, rect.Bottom - rect.Top);
        return true;
    }

    private static bool TryIsForegroundGameWindow()
    {
        return IsWindowOwnedByCurrentProcess(GetForegroundWindow());
    }

    private static bool IsWindowOwnedByCurrentProcess(IntPtr window)
    {
        if (window == IntPtr.Zero)
        {
            return false;
        }

        GetWindowThreadProcessId(window, out uint processId);
        return processId == (uint)System.Diagnostics.Process.GetCurrentProcess().Id;
    }

    private static bool IsWindowsRuntime()
    {
        return Application.platform == RuntimePlatform.WindowsPlayer ||
            Application.platform == RuntimePlatform.WindowsEditor;
    }

    private void RestoreGameInputState()
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

    private IEnumerable<ManagedPlugin> FilterPlugins()
    {
        if (string.IsNullOrWhiteSpace(_pluginSearch))
        {
            return _plugins;
        }

        return _plugins.Where(plugin =>
            Contains(plugin.Name, _pluginSearch) ||
            Contains(plugin.Guid, _pluginSearch));
    }

    private IEnumerable<ManagedSetting> FilterSettings(ManagedPlugin plugin)
    {
        if (string.IsNullOrWhiteSpace(_settingSearch))
        {
            return plugin.Settings;
        }

        return plugin.Settings.Where(setting =>
            Contains(setting.Section, _settingSearch) ||
            Contains(setting.DisplaySection, _settingSearch) ||
            Contains(setting.Key, _settingSearch) ||
            Contains(setting.DisplayName, _settingSearch) ||
            Contains(setting.Description, _settingSearch));
    }

    private void SelectPlugin(string selectionKey)
    {
        _selectedPluginKey = selectionKey;
        _settingsScroll = Vector2.zero;
        _captureSettingId = null;
        _selectedPluginIssuesExpanded = false;
        _dashboardProfileCoverageCacheKey = string.Empty;
    }

    private void FocusIssue(ManagedPlugin plugin, ManagerIssue issue, bool closePanel)
    {
        SelectPlugin(plugin.SelectionKey);
        if (issue.HasSettingTarget)
        {
            ManagedSetting? setting = FindSetting(issue.SettingId);
            _settingSearch = setting?.DisplayName ?? issue.SettingDisplayName;
            _status = string.IsNullOrWhiteSpace(_settingSearch)
                ? $"Selected {plugin.Name}."
                : $"Focused {plugin.Name} / {_settingSearch}.";
        }
        else
        {
            _status = $"Selected {plugin.Name}.";
        }

        if (closePanel)
        {
            _activeSubPanel = ManagerSubPanel.None;
        }
    }

    private void BeginIssueRebind(ManagedPlugin plugin, ManagerIssue issue)
    {
        if (!issue.HasSettingTarget)
        {
            FocusIssue(plugin, issue, closePanel: false);
            return;
        }

        SelectPlugin(plugin.SelectionKey);
        ManagedSetting? setting = FindSetting(issue.SettingId);
        if (setting == null || setting.SettingType != typeof(KeyCode))
        {
            _status = "Could not find that hotkey setting.";
            return;
        }

        _settingSearch = setting.DisplayName;
        _settingsScroll = Vector2.zero;
        _activeSubPanel = ManagerSubPanel.None;
        _captureSettingId = setting.Id;
        _status = $"Press a key for {setting.DisplayName}.";
    }

    private ManagedSetting? FindSetting(string id)
    {
        return _plugins.SelectMany(plugin => plugin.Settings).FirstOrDefault(setting => setting.Id == id);
    }

    private bool SaveBoxedValue(ManagedSetting setting, object value)
    {
        try
        {
            if (setting.Entry != null)
            {
                setting.Entry.BoxedValue = value;
                setting.Entry.ConfigFile.Save();
                setting.DraftValue = setting.Entry.GetSerializedValue();
            }
            else
            {
                SaveFileBackedValue(setting, Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty);
            }

            _status = $"Saved {setting.DisplayName}.";
            if (setting.SettingType == typeof(KeyCode))
            {
                RebuildControllerHotkeyLayerKeys();
            }

            return true;
        }
        catch (Exception ex)
        {
            _status = $"Could not save {setting.DisplayName}. Check the value and try again.";
            Logger.LogWarning($"Could not save {setting.Section}.{setting.Key}: {ex}");
            return false;
        }
    }

    private void SaveSerializedValue(ManagedSetting setting)
    {
        if (TrySaveSerializedValue(setting, setting.DraftValue, out string message))
        {
            _status = $"Saved {setting.DisplayName}.";
            if (setting.SettingType == typeof(KeyCode))
            {
                RebuildControllerHotkeyLayerKeys();
            }
        }
        else
        {
            _status = message;
        }
    }

    private bool TrySaveSerializedValue(ManagedSetting setting, string value, out string message)
    {
        try
        {
            if (!TryValidateSerializedValue(setting, value, out string validationMessage))
            {
                message = validationMessage;
                return false;
            }

            setting.DraftValue = value;
            if (setting.Entry != null)
            {
                setting.Entry.SetSerializedValue(value);
                setting.Entry.ConfigFile.Save();
                setting.DraftValue = setting.Entry.GetSerializedValue();
            }
            else
            {
                SaveFileBackedValue(setting, value);
            }

            message = $"Saved {setting.DisplayName}.";
            return true;
        }
        catch (Exception ex)
        {
            message = $"Could not save {setting.DisplayName}. Check the value and try again.";
            Logger.LogWarning($"Could not save {setting.Section}.{setting.Key}: {ex}");
            return false;
        }
    }

    private void RestoreDefault(ManagedSetting setting)
    {
        object? defaultValue = setting.GetDefaultValue();
        if (defaultValue == null)
        {
            _status = $"No default value is available for {setting.DisplayName}.";
            return;
        }

        if (setting.Entry != null)
        {
            SaveBoxedValue(setting, defaultValue);
            return;
        }

        setting.DraftValue = Convert.ToString(defaultValue, CultureInfo.InvariantCulture) ?? string.Empty;
        SaveSerializedValue(setting);
    }

    private void SaveSelectedProfile()
    {
        _profileName = SanitizeProfileName(_profileName);
        ExportProfile(_profileName);
        RefreshProfileNames();
    }

    private void LoadSelectedProfile()
    {
        _profileName = SanitizeProfileName(_profileName);
        ImportProfile(_profileName);
        RefreshProfileNames();
    }

    private void SelectAdjacentProfile(int direction)
    {
        RefreshProfileNames();
        if (_profileNames.Count == 0)
        {
            _profileName = DefaultProfileName;
            _status = "No saved profiles found.";
            return;
        }

        string safeName = SanitizeProfileName(_profileName);
        int index = _profileNames.FindIndex(name => string.Equals(name, safeName, StringComparison.OrdinalIgnoreCase));
        index = index < 0
            ? 0
            : (index + direction + _profileNames.Count) % _profileNames.Count;

        _profileName = _profileNames[index];
        _status = $"Selected profile `{_profileName}`.";
    }

    private void RefreshProfileNames()
    {
        _profileNames.Clear();
        _profileNames.AddRange(DiscoverProfileNames());
        if (!_profileNames.Contains(DefaultProfileName, StringComparer.OrdinalIgnoreCase))
        {
            _profileNames.Insert(0, DefaultProfileName);
        }

        _profileNames.Sort((left, right) =>
        {
            bool leftDefault = string.Equals(left, DefaultProfileName, StringComparison.OrdinalIgnoreCase);
            bool rightDefault = string.Equals(right, DefaultProfileName, StringComparison.OrdinalIgnoreCase);
            if (leftDefault && !rightDefault)
            {
                return -1;
            }

            if (!leftDefault && rightDefault)
            {
                return 1;
            }

            return string.Compare(left, right, StringComparison.OrdinalIgnoreCase);
        });
    }

    private void ExportProfile(string profileName)
    {
        try
        {
            string profilePath = GetProfilePath(profileName);
            Directory.CreateDirectory(Path.GetDirectoryName(profilePath)!);

            List<string> lines = new()
            {
                "# FoA Mod Manager settings profile v1",
                "# ownerGuid\tsection\tkey\tvalue"
            };

            foreach (ManagedSetting setting in _plugins
                .SelectMany(plugin => plugin.Settings)
                .OrderBy(setting => setting.OwnerGuid, StringComparer.OrdinalIgnoreCase)
                .ThenBy(setting => setting.Section, StringComparer.OrdinalIgnoreCase)
                .ThenBy(setting => setting.Key, StringComparer.OrdinalIgnoreCase))
            {
                lines.Add(string.Join("\t",
                    EncodeProfileField(setting.OwnerGuid),
                    EncodeProfileField(setting.Section),
                    EncodeProfileField(setting.Key),
                    EncodeProfileField(setting.GetSerializedValue())));
            }

            File.WriteAllLines(profilePath, lines);
            _status = $"Exported {FormatCount(lines.Count - 2, "option")} to {Path.GetFileName(profilePath)}.";
        }
        catch (Exception ex)
        {
            _status = "Could not export profile. Check the BepInEx config folder.";
            Logger.LogWarning($"Could not export FoA Mod Manager profile '{profileName}': {ex}");
        }
    }

    private void ImportProfile(string profileName)
    {
        try
        {
            string profilePath = GetProfilePath(profileName);
            if (!File.Exists(profilePath))
            {
                _status = $"No profile file found: {Path.GetFileName(profilePath)}.";
                return;
            }

            Dictionary<string, ManagedSetting> settingsByKey = _plugins
                .SelectMany(plugin => plugin.Settings)
                .GroupBy(BuildProfileSettingKey, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);

            int imported = 0;
            int skipped = 0;
            int failed = 0;

            foreach (string line in File.ReadAllLines(profilePath))
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#", StringComparison.Ordinal))
                {
                    continue;
                }

                string[] parts = line.Split('\t');
                if (parts.Length != 4 ||
                    !TryDecodeProfileField(parts[0], out string ownerGuid) ||
                    !TryDecodeProfileField(parts[1], out string section) ||
                    !TryDecodeProfileField(parts[2], out string key) ||
                    !TryDecodeProfileField(parts[3], out string value))
                {
                    failed++;
                    continue;
                }

                if (!settingsByKey.TryGetValue(BuildProfileSettingKey(ownerGuid, section, key), out ManagedSetting setting))
                {
                    skipped++;
                    continue;
                }

                if (TrySaveSerializedValue(setting, value, out _))
                {
                    imported++;
                }
                else
                {
                    failed++;
                }
            }

            RefreshCatalog();
            _status = $"Imported {FormatCount(imported, "option")}; skipped {skipped}; failed {failed}.";
        }
        catch (Exception ex)
        {
            _status = "Could not import profile. Check the profile file.";
            Logger.LogWarning($"Could not import FoA Mod Manager profile '{profileName}': {ex}");
        }
    }

    private void ExportDiagnosticReport()
    {
        try
        {
            string reportPath = GetDiagnosticReportPath();
            Directory.CreateDirectory(Path.GetDirectoryName(reportPath)!);
            File.WriteAllLines(reportPath, BuildDiagnosticReport(), Encoding.UTF8);
            _status = $"Exported warning report to {Path.GetFileName(reportPath)}.";
        }
        catch (Exception ex)
        {
            _status = "Could not export warning report. Check the BepInEx config folder.";
            Logger.LogWarning($"Could not export FoA Mod Manager warning report: {ex}");
        }
    }

    private IEnumerable<string> BuildDiagnosticReport()
    {
        DateTime generatedUtc = DateTime.UtcNow;
        int issueCount = _plugins.Sum(plugin => plugin.Issues.Count);
        int problemCount = _plugins.Sum(plugin => plugin.Issues.Count(issue => issue.IsBlocking));
        int optionCount = _plugins.Sum(plugin => plugin.Settings.Count);

        List<string> lines = new()
        {
            "# FoA Mod Manager Warning Report",
            string.Empty,
            $"Generated UTC: {generatedUtc:yyyy-MM-dd HH:mm:ss}Z",
            string.Empty,
            "## Summary",
            string.Empty,
            $"- Manager: {PluginName} {PluginVersion} ({BuildProfileDisplayName})",
            $"- Managed mods: {_plugins.Count}",
            $"- Adjustable options: {optionCount}",
            $"- Problems: {problemCount}",
            $"- Warnings: {issueCount - problemCount}",
            $"- BepInEx config folder: {Path.GetFileName(Path.GetFullPath(Paths.ConfigPath))}",
            string.Empty,
            "## Issues",
            string.Empty
        };

        List<(ManagedPlugin Plugin, ManagerIssue Issue)> issues = _plugins
            .SelectMany(plugin => plugin.Issues.Select(issue => (Plugin: plugin, Issue: issue)))
            .OrderBy(row => row.Issue.IsBlocking ? 0 : 1)
            .ThenBy(row => row.Issue.Category, StringComparer.OrdinalIgnoreCase)
            .ThenBy(row => row.Plugin.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(row => row.Issue.Message, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (issues.Count == 0)
        {
            lines.Add("- No warning issues were found.");
            lines.Add(string.Empty);
        }
        else
        {
            foreach ((ManagedPlugin plugin, ManagerIssue issue) in issues)
            {
                lines.Add($"### {issue.Severity}: {issue.Message}");
                lines.Add(string.Empty);
                lines.Add($"- Category: {issue.Category}");
                lines.Add($"- Mod: {plugin.Name} v{plugin.Version}");
                lines.Add($"- GUID: `{plugin.Guid}`");
                lines.Add($"- Config file: `{FormatReportFileName(plugin.ConfigPath)}`");
                if (issue.HasSettingTarget)
                {
                    ManagedSetting? setting = plugin.Settings.FirstOrDefault(candidate => candidate.Id == issue.SettingId);
                    string settingLabel = setting == null
                        ? issue.SettingDisplayName
                        : $"{setting.DisplaySection} / {setting.DisplayName} (`{setting.Section}.{setting.Key}`)";
                    lines.Add($"- Setting: {settingLabel}");
                }

                AddReportDetail(lines, "Detail", issue.Detail);
                lines.Add(string.Empty);
            }
        }

        lines.Add("## Managed Mods");
        lines.Add(string.Empty);
        foreach (ManagedPlugin plugin in _plugins.OrderBy(plugin => plugin.Name, StringComparer.OrdinalIgnoreCase))
        {
            lines.Add($"- {plugin.Name} v{plugin.Version} (`{plugin.Guid}`): {FormatCount(plugin.Settings.Count, "option")}, {FormatCount(plugin.Issues.Count, "warning")}, config `{FormatReportFileName(plugin.ConfigPath)}`");
        }

        lines.Add(string.Empty);
        lines.Add("## Dependency Snapshot");
        lines.Add(string.Empty);
        AppendDependencySnapshot(lines);

        lines.Add(string.Empty);
        lines.Add("## Hotkey Snapshot");
        lines.Add(string.Empty);
        AppendHotkeySnapshot(lines);

        lines.Add(string.Empty);
        lines.Add("## Config Conflict Snapshot");
        lines.Add(string.Empty);
        AppendConfigConflictSnapshot(lines);

        return lines;
    }

    private static void AppendDependencySnapshot(List<string> lines)
    {
        Dictionary<string, PluginInfo> loadedByGuid = Chainloader.PluginInfos.Values
            .GroupBy(info => info.Metadata.GUID, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

        bool wroteAny = false;
        foreach (PluginInfo info in Chainloader.PluginInfos.Values.OrderBy(info => info.Metadata.Name, StringComparer.OrdinalIgnoreCase))
        {
            if (!info.Dependencies.Any() && !info.Incompatibilities.Any())
            {
                continue;
            }

            wroteAny = true;
            lines.Add($"### {info.Metadata.Name} v{info.Metadata.Version}");
            lines.Add(string.Empty);
            lines.Add($"- GUID: `{info.Metadata.GUID}`");
            lines.Add($"- Assembly: `{FormatReportFileName(info.Location)}`");

            foreach (BepInDependency dependency in info.Dependencies.OrderBy(dependency => dependency.DependencyGUID, StringComparer.OrdinalIgnoreCase))
            {
                bool soft = (dependency.Flags & BepInDependency.DependencyFlags.SoftDependency) != 0;
                loadedByGuid.TryGetValue(dependency.DependencyGUID, out PluginInfo dependencyInfo);
                string loadedText = dependencyInfo == null
                    ? "missing"
                    : $"loaded {dependencyInfo.Metadata.Name} v{dependencyInfo.Metadata.Version}";
                string minimum = dependency.MinimumVersion == null ? "any version" : dependency.MinimumVersion.ToString();
                lines.Add($"- {(soft ? "Optional" : "Required")} dependency `{dependency.DependencyGUID}` ({minimum}): {loadedText}");
            }

            foreach (BepInIncompatibility incompatibility in info.Incompatibilities.OrderBy(incompatibility => incompatibility.IncompatibilityGUID, StringComparer.OrdinalIgnoreCase))
            {
                loadedByGuid.TryGetValue(incompatibility.IncompatibilityGUID, out PluginInfo incompatibleInfo);
                string loadedText = incompatibleInfo == null
                    ? "not loaded"
                    : $"loaded {incompatibleInfo.Metadata.Name} v{incompatibleInfo.Metadata.Version}";
                lines.Add($"- Incompatible with `{incompatibility.IncompatibilityGUID}`: {loadedText}");
            }

            lines.Add(string.Empty);
        }

        if (!wroteAny)
        {
            lines.Add("- No loaded plugins declared BepInEx dependencies or incompatibilities.");
        }
    }

    private void AppendHotkeySnapshot(List<string> lines)
    {
        var hotkeys = _plugins
            .SelectMany(plugin => plugin.Settings
                .Where(setting => setting.SettingType == typeof(KeyCode))
                .Select(setting => new
                {
                    Plugin = plugin,
                    Setting = setting,
                    Key = TryParseKeyCode(setting.GetSerializedValue(), out KeyCode key) ? key : KeyCode.None
                }))
            .Where(row => row.Key != KeyCode.None)
            .OrderBy(row => row.Key.ToString(), StringComparer.OrdinalIgnoreCase)
            .ThenBy(row => row.Plugin.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (hotkeys.Count == 0)
        {
            lines.Add("- No non-empty KeyCode settings were discovered.");
            return;
        }

        foreach (var group in hotkeys.GroupBy(row => row.Key))
        {
            string marker = group.Select(row => row.Plugin.Guid + "::" + row.Setting.Id).Distinct(StringComparer.Ordinal).Count() > 1
                ? "conflict"
                : "single";
            lines.Add($"- `{group.Key}` ({marker})");
            foreach (var row in group.OrderBy(row => row.Plugin.Name, StringComparer.OrdinalIgnoreCase))
            {
                lines.Add($"  - {row.Plugin.Name} / {row.Setting.DisplayName} (`{row.Plugin.Guid}`)");
            }
        }
    }

    private void AppendConfigConflictSnapshot(List<string> lines)
    {
        var sharedFiles = _plugins
            .Where(plugin => !string.IsNullOrWhiteSpace(plugin.ConfigPath))
            .GroupBy(plugin => NormalizePath(plugin.ConfigPath), StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Select(plugin => plugin.Guid).Distinct(StringComparer.OrdinalIgnoreCase).Count() > 1)
            .ToList();

        var sharedEntries = _plugins
            .Where(plugin => !string.IsNullOrWhiteSpace(plugin.ConfigPath))
            .SelectMany(plugin => plugin.Settings.Select(setting => new
            {
                Plugin = plugin,
                Setting = setting,
                ConfigPath = NormalizePath(plugin.ConfigPath),
                setting.Section,
                setting.Key
            }))
            .Where(row => !string.IsNullOrWhiteSpace(row.ConfigPath))
            .GroupBy(row => row.ConfigPath + "\n" + row.Section + "\n" + row.Key, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Select(row => row.Plugin.Guid).Distinct(StringComparer.OrdinalIgnoreCase).Count() > 1)
            .ToList();

        if (sharedFiles.Count == 0 && sharedEntries.Count == 0)
        {
            lines.Add("- No shared config files or shared config entries were found.");
            return;
        }

        foreach (IGrouping<string, ManagedPlugin> group in sharedFiles)
        {
            lines.Add($"- Shared file `{FormatReportFileName(group.Key)}`");
            foreach (ManagedPlugin plugin in group.OrderBy(plugin => plugin.Name, StringComparer.OrdinalIgnoreCase))
            {
                lines.Add($"  - {plugin.Name} v{plugin.Version} (`{plugin.Guid}`)");
            }
        }

        foreach (var group in sharedEntries)
        {
            var first = group.First();
            lines.Add($"- Shared entry `{FormatReportFileName(first.ConfigPath)}` [{first.Section}] {first.Key}");
            foreach (var row in group.OrderBy(row => row.Plugin.Name, StringComparer.OrdinalIgnoreCase))
            {
                lines.Add($"  - {row.Plugin.Name} / {row.Setting.DisplayName} (`{row.Plugin.Guid}`)");
            }
        }
    }

    private static void AddReportDetail(List<string> lines, string label, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        string[] detailLines = value.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        if (detailLines.Length == 1)
        {
            lines.Add($"- {label}: {detailLines[0]}");
            return;
        }

        lines.Add($"- {label}:");
        foreach (string detailLine in detailLines.Where(line => !string.IsNullOrWhiteSpace(line)))
        {
            lines.Add($"  - {detailLine.Trim()}");
        }
    }

    private static string FormatReportFileName(string path)
    {
        return string.IsNullOrWhiteSpace(path) ? "(none)" : Path.GetFileName(path);
    }

    private void SaveFileBackedValue(ManagedSetting setting, string value)
    {
        FileConfigEntry? fileEntry = setting.FileEntry;
        if (fileEntry == null)
        {
            throw new InvalidOperationException("Setting is not backed by a config file.");
        }

        string configRoot = Path.GetFullPath(Paths.ConfigPath);
        string configPath = Path.GetFullPath(fileEntry.ConfigPath);
        if (!IsPathInside(configPath, configRoot))
        {
            throw new InvalidOperationException($"Refusing to write config outside BepInEx config directory: {configPath}");
        }

        string[] lines = File.ReadAllLines(configPath);
        int lineIndex = FindConfigEntryLine(lines, fileEntry.Section, fileEntry.Key);
        if (lineIndex < 0)
        {
            throw new InvalidOperationException($"Could not find {fileEntry.Section}.{fileEntry.Key} in {configPath}");
        }

        lines[lineIndex] = ReplaceConfigEntryValue(lines[lineIndex], value);
        File.WriteAllLines(configPath, lines);
        fileEntry.UpdateValue(value, lineIndex);
        setting.DraftValue = value;
    }

    private static bool TryValidateSerializedValue(ManagedSetting setting, string value, out string message)
    {
        message = string.Empty;

        if (setting.SettingType == typeof(bool) && !bool.TryParse(value, out _))
        {
            message = $"{setting.DisplayName} needs true or false.";
            return false;
        }

        if (setting.SettingType == typeof(KeyCode) && !Enum.TryParse(value, ignoreCase: true, out KeyCode _))
        {
            message = $"{setting.DisplayName} needs a valid key name.";
            return false;
        }

        if (IsNumericSetting(setting.SettingType))
        {
            if (!TryParseNumber(value, out double parsed))
            {
                message = $"{setting.DisplayName} needs a number.";
                return false;
            }

            if (setting.NumericRange is NumericRange range && (parsed < range.Minimum || parsed > range.Maximum))
            {
                message = $"{setting.DisplayName} must be between {FormatRangeEndpoint(range.Minimum, range.IsInteger)} and {FormatRangeEndpoint(range.Maximum, range.IsInteger)}.";
                return false;
            }
        }

        return true;
    }

    private void RefreshCatalog()
    {
        _plugins.Clear();
        List<ManagedPlugin> discovered = DiscoverPlugins().ToList();
        AnalyzePluginIssues(discovered);
        _plugins.AddRange(discovered);
        RebuildControllerHotkeyLayerKeys();
        _dashboardProfileCoverageCacheKey = string.Empty;

        if (_plugins.All(plugin => plugin.SelectionKey != _selectedPluginKey))
        {
            _selectedPluginKey = _plugins.FirstOrDefault()?.SelectionKey;
        }

        int issueCount = _plugins.Sum(plugin => plugin.Issues.Count);
        _status = issueCount == 0
            ? $"Found {FormatCount(_plugins.Count, "adjustable mod")}."
            : $"Found {FormatCount(_plugins.Count, "adjustable mod")} and {FormatCount(issueCount, "warning")}.";
    }

    private static string FormatDependencyDetail(ManagedPlugin plugin, BepInDependency dependency, PluginInfo? dependencyInfo, bool soft)
    {
        string minimum = dependency.MinimumVersion == null ? "any version" : dependency.MinimumVersion.ToString();
        string installed = dependencyInfo == null
            ? "not loaded"
            : $"{dependencyInfo.Metadata.Name} v{dependencyInfo.Metadata.Version} (`{dependencyInfo.Metadata.GUID}`)";

        return string.Join("\n",
            $"Declared by: {plugin.Name} v{plugin.Version}",
            $"Dependency GUID: `{dependency.DependencyGUID}`",
            $"Requirement: {(soft ? "Optional" : "Required")} / {minimum}",
            $"Installed: {installed}");
    }

    private static string FormatIncompatibilityDetail(ManagedPlugin plugin, BepInIncompatibility incompatibility, PluginInfo incompatibleInfo)
    {
        return string.Join("\n",
            $"Declared by: {plugin.Name} v{plugin.Version}",
            $"Incompatible GUID: `{incompatibility.IncompatibilityGUID}`",
            $"Installed: {incompatibleInfo.Metadata.Name} v{incompatibleInfo.Metadata.Version} (`{incompatibleInfo.Metadata.GUID}`)");
    }

    private static void AnalyzePluginIssues(List<ManagedPlugin> plugins)
    {
        Dictionary<string, ManagedPlugin> pluginsByGuid = plugins
            .GroupBy(plugin => plugin.Guid, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);
        Dictionary<string, PluginInfo> loadedByGuid = Chainloader.PluginInfos.Values
            .GroupBy(info => info.Metadata.GUID, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

        foreach (IGrouping<string, ManagedPlugin> group in plugins
            .Where(plugin => !string.IsNullOrWhiteSpace(plugin.Guid))
            .GroupBy(plugin => plugin.Guid, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Select(plugin => plugin.SelectionKey).Distinct(StringComparer.Ordinal).Count() > 1))
        {
            string identities = string.Join(", ", group
                .Select(plugin => $"{plugin.Name} v{plugin.Version} ({FormatReportFileName(plugin.ConfigPath)})")
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(identity => identity, StringComparer.OrdinalIgnoreCase));

            foreach (ManagedPlugin plugin in group)
            {
                plugin.Issues.Add(ManagerIssue.Warning(
                    "Duplicate plugin GUID",
                    $"Multiple detected mod records use GUID `{group.Key}`: {identities}. This is usually a stale copied cfg file or an unusual shared-config layout; use config filenames when comparing support reports.",
                    category: "Identity conflict"));
            }
        }

        foreach (PluginInfo info in Chainloader.PluginInfos.Values)
        {
            if (!pluginsByGuid.TryGetValue(info.Metadata.GUID, out ManagedPlugin plugin))
            {
                continue;
            }

            foreach (BepInDependency dependency in info.Dependencies)
            {
                bool soft = (dependency.Flags & BepInDependency.DependencyFlags.SoftDependency) != 0;
                if (!loadedByGuid.TryGetValue(dependency.DependencyGUID, out PluginInfo dependencyInfo))
                {
                    if (soft)
                    {
                        plugin.Issues.Add(ManagerIssue.Warning(
                            "Optional dependency not loaded",
                            FormatDependencyDetail(plugin, dependency, dependencyInfo, soft),
                            category: "Dependency"));
                    }
                    else
                    {
                        plugin.Issues.Add(ManagerIssue.Blocking(
                            "Missing dependency",
                            FormatDependencyDetail(plugin, dependency, dependencyInfo, soft),
                            category: "Dependency"));
                    }

                    continue;
                }

                if (dependency.MinimumVersion != null && dependencyInfo.Metadata.Version < dependency.MinimumVersion)
                {
                    plugin.Issues.Add(ManagerIssue.Blocking(
                        "Dependency version too low",
                        FormatDependencyDetail(plugin, dependency, dependencyInfo, soft),
                        category: "Dependency"));
                }
            }

            foreach (BepInIncompatibility incompatibility in info.Incompatibilities)
            {
                if (loadedByGuid.ContainsKey(incompatibility.IncompatibilityGUID))
                {
                    PluginInfo incompatibleInfo = loadedByGuid[incompatibility.IncompatibilityGUID];
                    plugin.Issues.Add(ManagerIssue.Blocking(
                        "Declared incompatibility loaded",
                        FormatIncompatibilityDetail(plugin, incompatibility, incompatibleInfo),
                        category: "Dependency"));
                }
            }
        }

        foreach (IGrouping<string, ManagedPlugin> group in plugins
            .Where(plugin => !string.IsNullOrWhiteSpace(plugin.Name))
            .GroupBy(plugin => plugin.Name.Trim(), StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Select(plugin => plugin.Guid).Distinct(StringComparer.OrdinalIgnoreCase).Count() > 1))
        {
            string identities = string.Join(", ", group
                .Select(plugin => $"{plugin.Name} v{plugin.Version} (`{plugin.Guid}`)")
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase));

            foreach (ManagedPlugin plugin in group)
            {
                plugin.Issues.Add(ManagerIssue.Warning(
                    "Duplicate mod name",
                    $"Multiple loaded mods use the display name `{group.Key}`: {identities}. Use GUIDs when comparing profiles or support reports.",
                    category: "Identity conflict"));
            }
        }

        foreach (IGrouping<string, ManagedPlugin> group in plugins
            .Where(plugin => !string.IsNullOrWhiteSpace(plugin.ConfigPath))
            .GroupBy(plugin => NormalizePath(plugin.ConfigPath), StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Select(plugin => plugin.Guid).Distinct(StringComparer.OrdinalIgnoreCase).Count() > 1))
        {
            string names = string.Join(", ", group.Select(plugin => plugin.Name).OrderBy(name => name, StringComparer.OrdinalIgnoreCase));
            foreach (ManagedPlugin plugin in group)
            {
                plugin.Issues.Add(ManagerIssue.Warning(
                    "Shared config file",
                    $"This mod shares `{Path.GetFileName(group.Key)}` with: {names}. Check imported profiles carefully.",
                    category: "Config conflict"));
            }
        }

        var sharedConfigEntryGroups = plugins
            .Where(plugin => !string.IsNullOrWhiteSpace(plugin.ConfigPath))
            .SelectMany(plugin => plugin.Settings.Select(setting => new
            {
                Plugin = plugin,
                Setting = setting,
                ConfigPath = NormalizePath(plugin.ConfigPath),
                setting.Section,
                setting.Key
            }))
            .Where(row => !string.IsNullOrWhiteSpace(row.ConfigPath))
            .GroupBy(row => row.ConfigPath + "\n" + row.Section + "\n" + row.Key, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Select(row => row.Plugin.Guid).Distinct(StringComparer.OrdinalIgnoreCase).Count() > 1);

        foreach (var group in sharedConfigEntryGroups)
        {
            var first = group.First();
            string owners = string.Join(", ", group
                .Select(row => $"{row.Plugin.Name} / {row.Setting.DisplayName}")
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase));

            foreach (var row in group)
            {
                row.Plugin.Issues.Add(ManagerIssue.Warning(
                    "Shared config option",
                    $"`{Path.GetFileName(first.ConfigPath)}` entry [{first.Section}] {first.Key} is also exposed by: {owners}. Editing this option or importing profiles may affect the same saved value.",
                    row.Setting.Id,
                    row.Setting.DisplayName,
                    "Config conflict"));
            }
        }

        var hotkeyGroups = plugins
            .SelectMany(plugin => plugin.Settings
                .Where(setting => setting.SettingType == typeof(KeyCode))
                .Select(setting => new { Plugin = plugin, Setting = setting, Key = TryParseKeyCode(setting.GetSerializedValue(), out KeyCode key) ? key : KeyCode.None }))
            .Where(row => row.Key != KeyCode.None)
            .GroupBy(row => row.Key)
            .Where(group => group.Select(row => row.Plugin.Guid + "::" + row.Setting.Id).Distinct(StringComparer.Ordinal).Count() > 1);

        foreach (var group in hotkeyGroups)
        {
            string owners = string.Join(", ", group
                .Select(row => $"{row.Plugin.Name} / {row.Setting.DisplayName}")
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase));

            foreach (var row in group)
            {
                row.Plugin.Issues.Add(ManagerIssue.Warning(
                    "Hotkey conflict",
                    $"{group.Key} is also used by: {owners}.",
                    row.Setting.Id,
                    row.Setting.DisplayName,
                    "Hotkey conflict"));
            }
        }
    }

    private IEnumerable<ManagedPlugin> DiscoverPlugins()
    {
        Dictionary<string, FileConfigCatalog> configFiles = DiscoverConfigFiles();
        HashSet<string> liveConfigPathsWithEntries = new(StringComparer.OrdinalIgnoreCase);

        foreach (PluginInfo info in Chainloader.PluginInfos.Values.OrderBy(plugin => plugin.Metadata.Name))
        {
            if (info.Instance is not BaseUnityPlugin plugin)
            {
                continue;
            }

            if (_showManagerConfig?.Value != true && info.Metadata.GUID == PluginGuid)
            {
                continue;
            }

            if (IsLegacyAvalonModManager(info.Metadata.GUID, info.Metadata.Name))
            {
                continue;
            }

            string configPathKey = NormalizePath(plugin.Config.ConfigFilePath);
            configFiles.TryGetValue(configPathKey, out FileConfigCatalog? fileCatalog);
            ConfigEntryBase[] entries = ((IDictionary<ConfigDefinition, ConfigEntryBase>)plugin.Config).Values.ToArray();
            if (entries.Length == 0)
            {
                continue;
            }

            liveConfigPathsWithEntries.Add(configPathKey);
            List<ManagedSetting> settings = entries
                .Select(entry => new ManagedSetting(info.Metadata.GUID, entry, fileCatalog?.FindEntry(entry.Definition.Section, entry.Definition.Key)))
                .Where(setting => !setting.Hidden)
                .OrderBy(setting => setting.SectionOrder)
                .ThenBy(setting => setting.DisplaySection, StringComparer.OrdinalIgnoreCase)
                .ThenBy(setting => setting.Order)
                .ThenBy(setting => setting.DisplayName, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (settings.Count == 0)
            {
                continue;
            }

            yield return new ManagedPlugin(
                info.Metadata.GUID,
                info.Metadata.Name,
                info.Metadata.Version.ToString(),
                plugin.Config.ConfigFilePath,
                settings);
        }

        foreach (FileConfigCatalog fileCatalog in configFiles.Values.OrderBy(file => file.Name, StringComparer.OrdinalIgnoreCase))
        {
            string configPathKey = NormalizePath(fileCatalog.ConfigPath);
            if (liveConfigPathsWithEntries.Contains(configPathKey))
            {
                continue;
            }

            if (_showManagerConfig?.Value != true && fileCatalog.Guid == PluginGuid)
            {
                continue;
            }

            if (IsLegacyAvalonModManager(fileCatalog.Guid, fileCatalog.Name))
            {
                continue;
            }

            List<ManagedSetting> settings = fileCatalog.Entries
                .Select(entry => new ManagedSetting(fileCatalog.Guid, entry))
                .OrderBy(setting => setting.SectionOrder)
                .ThenBy(setting => setting.DisplaySection, StringComparer.OrdinalIgnoreCase)
                .ThenBy(setting => setting.Order)
                .ThenBy(setting => setting.DisplayName, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (settings.Count == 0)
            {
                continue;
            }

            yield return new ManagedPlugin(
                fileCatalog.Guid,
                fileCatalog.Name,
                fileCatalog.Version,
                fileCatalog.ConfigPath,
                settings);
        }
    }

    private Dictionary<string, FileConfigCatalog> DiscoverConfigFiles()
    {
        Dictionary<string, FileConfigCatalog> result = new(StringComparer.OrdinalIgnoreCase);

        string configPath;
        try
        {
            configPath = Paths.ConfigPath;
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"Could not resolve BepInEx config path for config-file discovery: {ex.Message}");
            return result;
        }

        if (string.IsNullOrWhiteSpace(configPath) || !Directory.Exists(configPath))
        {
            return result;
        }

        string[] filePaths;
        try
        {
            filePaths = Directory.GetFiles(configPath, "*.cfg", SearchOption.TopDirectoryOnly);
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"Could not enumerate BepInEx config files for config-file discovery: {ex.Message}");
            return result;
        }

        foreach (string filePath in filePaths)
        {
            if (string.Equals(Path.GetFileName(filePath), "BepInEx.cfg", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            try
            {
                if (TryReadConfigFile(filePath, out FileConfigCatalog? catalog) && catalog != null)
                {
                    result[NormalizePath(filePath)] = catalog;
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning($"Could not read BepInEx config file {filePath}: {ex.Message}");
            }
        }

        return result;
    }

    private static bool TryReadConfigFile(string configPath, out FileConfigCatalog? catalog)
    {
        catalog = null;
        string[] lines = File.ReadAllLines(configPath);
        string fileName = Path.GetFileNameWithoutExtension(configPath);
        string name = HumanizeIdentifier(fileName);
        string version = "cfg";
        string guid = fileName;
        string currentSection = string.Empty;
        List<FileConfigEntry> entries = new();
        List<string> pendingDescription = new();
        string? pendingSettingType = null;
        string? pendingDefaultValue = null;
        string? pendingAcceptableValues = null;
        string? pendingRange = null;

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            string trimmed = line.Trim();

            if (TryReadPluginCreatedHeader(trimmed, out string? pluginName, out string? pluginVersion))
            {
                name = pluginName;
                version = pluginVersion;
                continue;
            }

            if (TryReadCommentValue(trimmed, "## Plugin GUID:", out string pluginGuid))
            {
                guid = pluginGuid;
                continue;
            }

            if (TryReadSection(trimmed, out string section))
            {
                currentSection = section;
                ClearPendingConfigMetadata(pendingDescription, ref pendingSettingType, ref pendingDefaultValue, ref pendingAcceptableValues, ref pendingRange);
                continue;
            }

            if (trimmed.StartsWith("##", StringComparison.Ordinal))
            {
                if (!string.IsNullOrWhiteSpace(currentSection))
                {
                    string descriptionLine = trimmed.Substring(2).Trim();
                    if (descriptionLine.Length > 0)
                    {
                        pendingDescription.Add(descriptionLine);
                    }
                }

                continue;
            }

            if (TryReadCommentValue(trimmed, "# Setting type:", out string settingType))
            {
                pendingSettingType = settingType;
                continue;
            }

            if (TryReadCommentValue(trimmed, "# Default value:", out string defaultValue))
            {
                pendingDefaultValue = defaultValue;
                continue;
            }

            if (TryReadCommentValue(trimmed, "# Acceptable values:", out string acceptableValues))
            {
                pendingAcceptableValues = acceptableValues;
                continue;
            }

            if (TryReadCommentValue(trimmed, "# Acceptable value range:", out string acceptableRange))
            {
                pendingRange = acceptableRange;
                continue;
            }

            if (TryReadAssignment(line, out string key, out string value))
            {
                Type entryType = ResolveFileSettingType(pendingSettingType, value);
                NumericRange? numericRange = TryCreateNumericRangeFromText(pendingRange, entryType, out NumericRange range)
                    ? range
                    : null;
                IReadOnlyList<string> choices = ParseAcceptableValues(pendingAcceptableValues);
                string description = string.Join(" ", pendingDescription);

                entries.Add(new FileConfigEntry(
                    configPath,
                    i,
                    currentSection,
                    key,
                    value,
                    entryType,
                    pendingDefaultValue,
                    description,
                    choices,
                    numericRange));

                ClearPendingConfigMetadata(pendingDescription, ref pendingSettingType, ref pendingDefaultValue, ref pendingAcceptableValues, ref pendingRange);
            }
        }

        if (entries.Count == 0)
        {
            return false;
        }

        catalog = new FileConfigCatalog(guid, name, version, configPath, entries);
        return true;
    }

    private static void ClearPendingConfigMetadata(List<string> pendingDescription, ref string? settingType, ref string? defaultValue, ref string? acceptableValues, ref string? range)
    {
        pendingDescription.Clear();
        settingType = null;
        defaultValue = null;
        acceptableValues = null;
        range = null;
    }

    private static bool TryReadPluginCreatedHeader(string line, out string name, out string version)
    {
        const string prefix = "## Settings file was created by plugin ";
        name = string.Empty;
        version = "cfg";
        if (!line.StartsWith(prefix, StringComparison.Ordinal))
        {
            return false;
        }

        string body = line.Substring(prefix.Length).Trim();
        int versionIndex = body.LastIndexOf(" v", StringComparison.Ordinal);
        if (versionIndex > 0)
        {
            name = body.Substring(0, versionIndex).Trim();
            version = body.Substring(versionIndex + 2).Trim();
        }
        else
        {
            name = body;
        }

        return !string.IsNullOrWhiteSpace(name);
    }

    private static bool TryReadCommentValue(string line, string prefix, out string value)
    {
        value = string.Empty;
        if (!line.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        value = line.Substring(prefix.Length).Trim();
        return true;
    }

    private static bool TryReadSection(string line, out string section)
    {
        section = string.Empty;
        if (line.Length < 3 || line[0] != '[' || line[line.Length - 1] != ']')
        {
            return false;
        }

        section = line.Substring(1, line.Length - 2).Trim();
        return section.Length > 0;
    }

    private static bool TryReadAssignment(string line, out string key, out string value)
    {
        key = string.Empty;
        value = string.Empty;
        string trimmed = line.TrimStart();
        if (trimmed.StartsWith("#", StringComparison.Ordinal) || trimmed.StartsWith("[", StringComparison.Ordinal))
        {
            return false;
        }

        int equalsIndex = line.IndexOf('=');
        if (equalsIndex <= 0)
        {
            return false;
        }

        key = line.Substring(0, equalsIndex).Trim();
        value = line.Substring(equalsIndex + 1).Trim();
        return key.Length > 0;
    }

    private static Type ResolveFileSettingType(string? settingType, string value)
    {
        string normalized = (settingType ?? string.Empty).Trim().ToLowerInvariant();
        if (normalized.StartsWith("system.", StringComparison.Ordinal))
        {
            normalized = normalized.Substring("system.".Length);
        }

        return normalized switch
        {
            "boolean" or "bool" => typeof(bool),
            "keycode" or "unityengine.keycode" => typeof(KeyCode),
            "byte" => typeof(byte),
            "sbyte" => typeof(sbyte),
            "int16" or "short" => typeof(short),
            "uint16" or "ushort" => typeof(ushort),
            "int32" or "int" or "integer" => typeof(int),
            "uint32" or "uint" => typeof(uint),
            "int64" or "long" => typeof(long),
            "uint64" or "ulong" => typeof(ulong),
            "single" or "float" => typeof(float),
            "double" => typeof(double),
            "decimal" => typeof(decimal),
            "string" => typeof(string),
            _ => InferFileSettingType(value)
        };
    }

    private static Type InferFileSettingType(string value)
    {
        if (bool.TryParse(value, out _))
        {
            return typeof(bool);
        }

        if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
        {
            return typeof(int);
        }

        if (TryParseNumber(value, out _))
        {
            return typeof(float);
        }

        if (Enum.TryParse(value, ignoreCase: true, out KeyCode _))
        {
            return typeof(KeyCode);
        }

        return typeof(string);
    }

    private static IReadOnlyList<string> ParseAcceptableValues(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Array.Empty<string>();
        }

        return value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(part => part.Trim())
            .Where(part => part.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static bool TryCreateNumericRangeFromText(string? value, Type settingType, out NumericRange range)
    {
        range = default;
        if (string.IsNullOrWhiteSpace(value) || !IsNumericSetting(settingType))
        {
            return false;
        }

        string text = value.Trim();
        int fromIndex = text.IndexOf("From ", StringComparison.OrdinalIgnoreCase);
        int toIndex = text.IndexOf(" to ", StringComparison.OrdinalIgnoreCase);
        if (fromIndex < 0 || toIndex <= fromIndex)
        {
            return false;
        }

        string minimumText = text.Substring(fromIndex + "From ".Length, toIndex - (fromIndex + "From ".Length)).Trim();
        string maximumText = text.Substring(toIndex + " to ".Length).Trim();
        if (!TryParseNumber(minimumText, out double minimum) || !TryParseNumber(maximumText, out double maximum))
        {
            return false;
        }

        if (!IsFinite(minimum) || !IsFinite(maximum) || minimum >= maximum)
        {
            return false;
        }

        range = new NumericRange(minimum, maximum, IsIntegerSetting(settingType));
        return true;
    }

    private void EnsureWindowPlacement(bool force)
    {
        if (!force && _windowInitialized && _lastScreenWidth == Screen.width && _lastScreenHeight == Screen.height)
        {
            _window = ClampWindowToScreen(_window);
            return;
        }

        float maxWidth = GetMaximumWindowWidth();
        float maxHeight = GetMaximumWindowHeight();
        float width = Mathf.Min(maxWidth, Mathf.Max(Px(960f), Screen.width * 0.82f));
        float height = Mathf.Min(maxHeight, Mathf.Max(Px(620f), Screen.height * 0.86f));

        _window = ClampWindowToScreen(new Rect(
            Mathf.Max(WindowScreenMargin, (Screen.width - width) * 0.5f),
            Mathf.Max(WindowScreenMargin, (Screen.height - height) * 0.48f),
            width,
            height));

        _lastScreenWidth = Screen.width;
        _lastScreenHeight = Screen.height;
        _windowInitialized = true;
    }

    private static Rect ClampWindowToScreen(Rect window)
    {
        float maxWidth = GetMaximumWindowWidth();
        float maxHeight = GetMaximumWindowHeight();
        float width = Mathf.Clamp(window.width, GetMinimumWindowWidth(), maxWidth);
        float height = Mathf.Clamp(window.height, GetMinimumWindowHeight(), maxHeight);
        float maxX = Mathf.Max(WindowScreenMargin, Screen.width - width - WindowScreenMargin);
        float maxY = Mathf.Max(WindowScreenMargin, Screen.height - height - WindowScreenMargin);
        float x = Mathf.Clamp(window.x, WindowScreenMargin, maxX);
        float y = Mathf.Clamp(window.y, WindowScreenMargin, maxY);

        return new Rect(x, y, width, height);
    }

    private static float GetMinimumWindowWidth()
    {
        return Mathf.Min(Px(MinimumWindowWidth), GetAvailableWindowWidth());
    }

    private static float GetMinimumWindowHeight()
    {
        return Mathf.Min(Px(MinimumWindowHeight), GetAvailableWindowHeight());
    }

    private static float GetMaximumWindowWidth()
    {
        return Mathf.Max(GetMinimumWindowWidth(), GetAvailableWindowWidth());
    }

    private static float GetMaximumWindowHeight()
    {
        return Mathf.Max(GetMinimumWindowHeight(), GetAvailableWindowHeight());
    }

    private static float GetAvailableWindowWidth()
    {
        return Mathf.Max(320f, Screen.width - WindowScreenMargin * 2f);
    }

    private static float GetAvailableWindowHeight()
    {
        return Mathf.Max(320f, Screen.height - WindowScreenMargin * 2f);
    }

    private void EnsureStyles()
    {
        if (_stylesDirty)
        {
            _styles = null;
            ReleaseStyleTextures();
            _stylesDirty = false;
        }

        _styles ??= new UiStyles(this);
    }

    private UiTextureSet LoadManagerTextureSet()
    {
        UiTextureSet textures = new();
        _lastManagerSkinPackId = GetTaintedInterfaceUiPackIdInstance();
        _lastManagerSkinTextureCount = 0;
        _lastManagerSkinMissingCount = 0;
        _lastManagerSkinSource = _darkFantasyAssetsEnabled?.Value == true ? "Generated" : "Fallback";
        if (_darkFantasyAssetsEnabled?.Value != true)
        {
            return textures;
        }

        if (!string.Equals(_lastManagerSkinPackId, TaintedInterfaceCoreThemePackId, StringComparison.OrdinalIgnoreCase))
        {
            textures = LoadTaintedInterfaceManagerTextureSet(_lastManagerSkinPackId);
            _lastManagerSkinTextureCount = textures.LoadedCount;
            _lastManagerSkinMissingCount = textures.MissingCount;
            _lastManagerSkinSource = textures.LoadedCount > 0 ? "Tainted Interface" : _lastManagerSkinSource;
            return textures;
        }

        textures = LoadDarkFantasyTextureSet();
        bool loadedFromTaintedInterfaceFull = false;
        if (textures.LoadedCount == 0)
        {
            UiTextureSet taintedInterfaceFullTextures = LoadTaintedInterfaceFullDarkFantasyTextureSet();
            if (taintedInterfaceFullTextures.LoadedCount > 0 || taintedInterfaceFullTextures.MissingCount > 0)
            {
                textures = taintedInterfaceFullTextures;
                loadedFromTaintedInterfaceFull = taintedInterfaceFullTextures.LoadedCount > 0;
            }
        }

        _lastManagerSkinTextureCount = textures.LoadedCount;
        _lastManagerSkinMissingCount = textures.MissingCount;
        _lastManagerSkinSource = textures.LoadedCount > 0
            ? loadedFromTaintedInterfaceFull ? "Tainted Interface" : textures.EmbeddedCount > 0 ? "Built In" : "Loose"
            : "Generated";
        return textures;
    }

    private UiTextureSet LoadTaintedInterfaceManagerTextureSet(string packId)
    {
        UiTextureSet textures = new();
        Type? apiType = ResolveTaintedInterfaceApiType();
        MethodInfo? getUiTextureMethod = apiType?.GetMethod(
            "GetUiTexture",
            BindingFlags.Public | BindingFlags.Static,
            null,
            new[] { typeof(string) },
            null);
        if (getUiTextureMethod == null)
        {
            textures.MissingCount = GetTaintedInterfaceManagerTextureBindings(packId).Length;
            _lastManagerSkinSource = "Missing API";
            Logger.LogWarning($"FoA Mod Manager could not load Tainted Interface manager skin for pack={packId}: {TaintedInterfaceApiTypeName}.GetUiTexture(string) is unavailable; generated style fallback active.");
            return textures;
        }

        foreach (TaintedInterfaceManagerTextureBinding binding in GetTaintedInterfaceManagerTextureBindings(packId))
        {
            Texture2D? texture = LoadTaintedInterfaceManagerTexture(getUiTextureMethod, BuildTaintedInterfaceTextureId(packId, binding.TextureIdSuffix));
            textures.Assign(binding.Label, texture);
            if (texture != null)
            {
                textures.LoadedCount++;
                textures.EmbeddedCount++;
            }
            else
            {
                textures.MissingCount++;
            }
        }

        if (textures.LoadedCount > 0)
        {
            Logger.LogInfo($"FoA Mod Manager Tainted Interface manager skin loaded. pack={packId}; loaded={textures.LoadedCount}; missing={textures.MissingCount}; source=Tainted Interface API");
        }
        else
        {
            _lastManagerSkinSource = "Missing assets";
            Logger.LogWarning($"FoA Mod Manager Tainted Interface manager skin loaded no textures. pack={packId}; missing={textures.MissingCount}; generated style fallback active.");
        }

        return textures;
    }

    private static TaintedInterfaceManagerTextureBinding[] GetTaintedInterfaceManagerTextureBindings(string packId)
    {
        return UsesLineArtPresetPolish(packId)
            ? LineArtTaintedInterfaceManagerTextureBindings
            : TaintedInterfaceManagerTextureBindings;
    }

    private static bool UsesLineArtPresetPolish(string packId)
    {
        return string.Equals(packId, TaintedInterfaceFantasyNouveauUiPackId, StringComparison.OrdinalIgnoreCase)
            || string.Equals(packId, TaintedInterfaceTribalUiSetPackId, StringComparison.OrdinalIgnoreCase);
    }

    private static string BuildTaintedInterfaceTextureId(string packId, string textureIdSuffix)
    {
        return "ui." + packId + "." + textureIdSuffix;
    }

    private Texture2D? LoadTaintedInterfaceManagerTexture(MethodInfo getUiTextureMethod, string textureId)
    {
        try
        {
            return getUiTextureMethod.Invoke(null, new object[] { textureId }) as Texture2D;
        }
        catch (TargetInvocationException ex)
        {
            Exception inner = ex.InnerException ?? ex;
            Logger.LogWarning($"FoA Mod Manager Tainted Interface texture request failed for {textureId}: {inner.GetType().Name}: {inner.Message}");
            return null;
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"FoA Mod Manager Tainted Interface texture request failed for {textureId}: {ex.GetType().Name}: {ex.Message}");
            return null;
        }
    }

    private UiTextureSet LoadTaintedInterfaceFullDarkFantasyTextureSet()
    {
        UiTextureSet textures = new();
        Type? apiType = ResolveTaintedInterfaceApiType();
        MethodInfo? getBytesMethod = apiType?.GetMethod(
            "GetFullEmbeddedAssetBytes",
            BindingFlags.Public | BindingFlags.Static,
            null,
            new[] { typeof(string) },
            null);
        if (getBytesMethod == null)
        {
            return textures;
        }

        textures.Window = LoadTaintedInterfaceFullDarkFantasyTexture(getBytesMethod, "window", "PNG/1080p/panels & dialogs/panel - medium.png", textures);
        textures.Header = LoadTaintedInterfaceFullDarkFantasyTexture(getBytesMethod, "header", "PNG/1080p/panels & dialogs/item header.png", textures);
        textures.Panel = LoadTaintedInterfaceFullDarkFantasyTexture(getBytesMethod, "panel", "PNG/1080p/panels & dialogs/panel - small.png", textures);
        textures.Card = LoadTaintedInterfaceFullDarkFantasyTexture(getBytesMethod, "card", "PNG/1080p/panels & dialogs/card.png", textures);
        textures.Field = LoadTaintedInterfaceFullDarkFantasyTexture(getBytesMethod, "field", "PNG/1080p/Inputs/input box.png", textures);
        textures.Button = LoadTaintedInterfaceFullDarkFantasyTexture(getBytesMethod, "button", "PNG/1080p/Inputs/Btn.png", textures);
        textures.ButtonHover = LoadTaintedInterfaceFullDarkFantasyTexture(getBytesMethod, "button-hover", "PNG/1080p/Inputs/Btn - Hovered.png", textures);
        textures.Secondary = LoadTaintedInterfaceFullDarkFantasyTexture(getBytesMethod, "secondary-button", "PNG/1080p/Inputs/list - button.png", textures);
        textures.Selected = LoadTaintedInterfaceFullDarkFantasyTexture(getBytesMethod, "selected-button", "PNG/1080p/Inputs/list - button (hovered).png", textures);
        textures.Pill = LoadTaintedInterfaceFullDarkFantasyTexture(getBytesMethod, "pill", "PNG/1080p/Inputs/Btn - keyboard tip.png", textures);
        textures.RangeTrack = LoadTaintedInterfaceFullDarkFantasyTexture(getBytesMethod, "slider-track", "PNG/1080p/Inputs/slider - BG.png", textures);
        textures.RangeThumb = LoadTaintedInterfaceFullDarkFantasyTexture(getBytesMethod, "slider-thumb", "PNG/1080p/Inputs/slider - Grabber.png", textures);

        if (textures.LoadedCount > 0)
        {
            Logger.LogInfo($"FoA Mod Manager core Dark Fantasy manager skin loaded from Tainted Interface full embedded assets. loaded={textures.LoadedCount}; missing={textures.MissingCount}");
        }
        else if (textures.MissingCount > 0)
        {
            Logger.LogWarning($"FoA Mod Manager core Dark Fantasy manager skin found Tainted Interface full asset API but no expected textures loaded. missing={textures.MissingCount}; generated style fallback active.");
        }

        return textures;
    }

    private Texture2D? LoadTaintedInterfaceFullDarkFantasyTexture(MethodInfo getBytesMethod, string label, string relativePath, UiTextureSet textures)
    {
        string resourceName = TaintedInterfaceFullDarkFantasyResourcePrefix + relativePath.Replace('/', '\\');
        try
        {
            if (getBytesMethod.Invoke(null, new object[] { resourceName }) is byte[] bytes && bytes.Length > 0)
            {
                Texture2D? texture = CreateDarkFantasyTexture(label, bytes, resourceName);
                if (texture != null)
                {
                    textures.LoadedCount++;
                    textures.EmbeddedCount++;
                    return texture;
                }
            }
        }
        catch (TargetInvocationException ex)
        {
            Exception inner = ex.InnerException ?? ex;
            Logger.LogWarning($"FoA Mod Manager Tainted Interface full asset request failed for {resourceName}: {inner.GetType().Name}: {inner.Message}");
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"FoA Mod Manager Tainted Interface full asset request failed for {resourceName}: {ex.GetType().Name}: {ex.Message}");
        }

        textures.MissingCount++;
        return null;
    }

    private static Type? ResolveTaintedInterfaceApiType()
    {
        return AppDomain.CurrentDomain
            .GetAssemblies()
            .Select(assembly => assembly.GetType(TaintedInterfaceApiTypeName, throwOnError: false))
            .FirstOrDefault(type => type != null);
    }

    private UiTextureSet LoadDarkFantasyTextureSet()
    {
        UiTextureSet textures = new();
        if (_darkFantasyAssetsEnabled?.Value != true)
        {
            return textures;
        }

        bool hasLooseRoot = TryResolveDarkFantasyAssetRoot(out string rootPath);
        bool hasEmbeddedTextures = HasEmbeddedDarkFantasyTextures();
        if (!hasLooseRoot && !hasEmbeddedTextures)
        {
            return textures;
        }

        textures.Window = LoadDarkFantasyTexture(hasLooseRoot, rootPath, "window", "PNG/1080p/panels & dialogs/panel - medium.png", "Window.png", hasEmbeddedTextures, textures);
        textures.Header = LoadDarkFantasyTexture(hasLooseRoot, rootPath, "header", "PNG/1080p/panels & dialogs/item header.png", "Header.png", hasEmbeddedTextures, textures);
        textures.Panel = LoadDarkFantasyTexture(hasLooseRoot, rootPath, "panel", "PNG/1080p/panels & dialogs/panel - small.png", "Panel.png", hasEmbeddedTextures, textures);
        textures.Card = LoadDarkFantasyTexture(hasLooseRoot, rootPath, "card", "PNG/1080p/panels & dialogs/card.png", "Card.png", hasEmbeddedTextures, textures);
        textures.Field = LoadDarkFantasyTexture(hasLooseRoot, rootPath, "field", "PNG/1080p/Inputs/input box.png", "Field.png", hasEmbeddedTextures, textures);
        textures.Button = LoadDarkFantasyTexture(hasLooseRoot, rootPath, "button", "PNG/1080p/Inputs/Btn.png", "Button.png", hasEmbeddedTextures, textures);
        textures.ButtonHover = LoadDarkFantasyTexture(hasLooseRoot, rootPath, "button-hover", "PNG/1080p/Inputs/Btn - Hovered.png", "ButtonHover.png", hasEmbeddedTextures, textures);
        textures.Secondary = LoadDarkFantasyTexture(hasLooseRoot, rootPath, "secondary-button", "PNG/1080p/Inputs/list - button.png", "Secondary.png", hasEmbeddedTextures, textures);
        textures.Selected = LoadDarkFantasyTexture(hasLooseRoot, rootPath, "selected-button", "PNG/1080p/Inputs/list - button (hovered).png", "Selected.png", hasEmbeddedTextures, textures);
        textures.Pill = LoadDarkFantasyTexture(hasLooseRoot, rootPath, "pill", "PNG/1080p/Inputs/Btn - keyboard tip.png", "Pill.png", hasEmbeddedTextures, textures);
        textures.RangeTrack = LoadDarkFantasyTexture(hasLooseRoot, rootPath, "slider-track", "PNG/1080p/Inputs/slider - BG.png", "RangeTrack.png", hasEmbeddedTextures, textures);
        textures.RangeThumb = LoadDarkFantasyTexture(hasLooseRoot, rootPath, "slider-thumb", "PNG/1080p/Inputs/slider - Grabber.png", "RangeThumb.png", hasEmbeddedTextures, textures);

        if (textures.LoadedCount > 0)
        {
            string rootSummary = hasLooseRoot ? rootPath : "(none)";
            Logger.LogInfo($"Dark-fantasy manager UI textures loaded. loaded={textures.LoadedCount}; loose={textures.LooseCount}; embedded={textures.EmbeddedCount}; missing={textures.MissingCount}; root={rootSummary}");
        }
        else if (textures.MissingCount > 0)
        {
            string rootSummary = hasLooseRoot ? rootPath : "(none)";
            Logger.LogWarning($"Dark-fantasy manager UI asset path was available but no expected textures loaded. missing={textures.MissingCount}; root={rootSummary}; generated style fallback active.");
        }

        return textures;
    }

    private static bool HasEmbeddedDarkFantasyTextures()
    {
        try
        {
            return typeof(Plugin).Assembly.GetManifestResourceNames().Any(name => name.StartsWith(EmbeddedDarkFantasyResourcePrefix, StringComparison.Ordinal));
        }
        catch
        {
            return false;
        }
    }

    private bool TryResolveDarkFantasyAssetRoot(out string rootPath)
    {
        rootPath = string.Empty;
        string configuredPath = _darkFantasyAssetRoot?.Value ?? DefaultDarkFantasyAssetRoot;
        if (string.IsNullOrWhiteSpace(configuredPath))
        {
            Logger.LogWarning("Display.DarkFantasyAssetRoot is blank; embedded dark-fantasy assets will be used when present, otherwise generated style fallback stays active.");
            return false;
        }

        if (Uri.TryCreate(configuredPath, UriKind.Absolute, out _))
        {
            Logger.LogWarning("Display.DarkFantasyAssetRoot must be relative to the FoA Mod Manager plugin folder, not an absolute URI; embedded dark-fantasy assets will be used when present.");
            return false;
        }

        string relativePath = configuredPath.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);
        if (Path.IsPathRooted(relativePath))
        {
            Logger.LogWarning("Display.DarkFantasyAssetRoot must be relative to the FoA Mod Manager plugin folder; embedded dark-fantasy assets will be used when present.");
            return false;
        }

        foreach (string segment in relativePath.Split(Path.DirectorySeparatorChar))
        {
            if (string.IsNullOrWhiteSpace(segment) || segment == "." || segment == "..")
            {
                Logger.LogWarning("Display.DarkFantasyAssetRoot contains an unsafe path segment; embedded dark-fantasy assets will be used when present.");
                return false;
            }
        }

        string pluginRoot = Path.GetFullPath(Path.GetDirectoryName(Info.Location) ?? Paths.PluginPath);
        string candidateRoot;
        try
        {
            candidateRoot = Path.GetFullPath(Path.Combine(pluginRoot, relativePath));
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"Display.DarkFantasyAssetRoot could not be resolved: {ex.GetType().Name}: {ex.Message}; embedded dark-fantasy assets will be used when present.");
            return false;
        }

        if (!IsPathInside(candidateRoot, pluginRoot))
        {
            Logger.LogWarning("Display.DarkFantasyAssetRoot resolved outside the FoA Mod Manager plugin folder; embedded dark-fantasy assets will be used when present.");
            return false;
        }

        if (!Directory.Exists(candidateRoot))
        {
            Logger.LogInfo($"Dark-fantasy manager UI asset root not found at {candidateRoot}; embedded dark-fantasy assets will be used when present, otherwise generated style fallback stays active.");
            return false;
        }

        rootPath = candidateRoot;
        return true;
    }

    private Texture2D? LoadDarkFantasyTexture(bool hasLooseRoot, string rootPath, string label, string relativePath, string embeddedName, bool hasEmbeddedTextures, UiTextureSet textures)
    {
        if (hasEmbeddedTextures)
        {
            Texture2D? embeddedTexture = LoadEmbeddedDarkFantasyTexture(label, EmbeddedDarkFantasyResourcePrefix + embeddedName);
            if (embeddedTexture != null)
            {
                textures.LoadedCount++;
                textures.EmbeddedCount++;
                return embeddedTexture;
            }
        }

        if (hasLooseRoot)
        {
            Texture2D? looseTexture = LoadLooseDarkFantasyTexture(rootPath, label, relativePath);
            if (looseTexture != null)
            {
                textures.LoadedCount++;
                textures.LooseCount++;
                return looseTexture;
            }
        }

        textures.MissingCount++;
        return null;
    }

    private Texture2D? LoadLooseDarkFantasyTexture(string rootPath, string label, string relativePath)
    {
        string normalizedRelativePath = relativePath.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);
        string filePath;
        try
        {
            filePath = Path.GetFullPath(Path.Combine(rootPath, normalizedRelativePath));
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"Dark-fantasy UI texture path for {label} could not be resolved: {ex.GetType().Name}: {ex.Message}");
            return null;
        }

        if (!IsPathInside(filePath, rootPath))
        {
            Logger.LogWarning($"Dark-fantasy UI texture path for {label} resolved outside the asset root and was refused.");
            return null;
        }

        if (!File.Exists(filePath))
        {
            return null;
        }

        try
        {
            byte[] bytes = File.ReadAllBytes(filePath);
            return CreateDarkFantasyTexture(label, bytes, filePath);
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"Dark-fantasy UI texture load failed for {label}: {ex.GetType().Name}: {ex.Message}");
            return null;
        }
    }

    private Texture2D? LoadEmbeddedDarkFantasyTexture(string label, string resourceName)
    {
        try
        {
            Stream? stream = typeof(Plugin).Assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                return null;
            }

            using (stream)
            {
                using MemoryStream buffer = new();
                stream.CopyTo(buffer);
                return CreateDarkFantasyTexture(label, buffer.ToArray(), resourceName);
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"Embedded dark-fantasy UI texture load failed for {label}: {ex.GetType().Name}: {ex.Message}");
            return null;
        }
    }

    private Texture2D? CreateDarkFantasyTexture(string label, byte[] bytes, string source)
    {
        Texture2D texture = new(2, 2, TextureFormat.RGBA32, false)
        {
            name = "FoAModManager:" + label,
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };

        if (!ImageConversion.LoadImage(texture, bytes, markNonReadable: false))
        {
            Destroy(texture);
            Logger.LogWarning($"Dark-fantasy UI texture for {label} could not be decoded as PNG: {source}");
            return null;
        }

        _ownedTextures.Add(texture);
        return texture;
    }

    private Texture2D CreateTexture(Color color)
    {
        Texture2D texture = new(1, 1, TextureFormat.RGBA32, false);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        _ownedTextures.Add(texture);
        return texture;
    }

    private static bool Contains(string value, string search)
    {
        return value.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static string NormalizePath(string path)
    {
        return string.IsNullOrWhiteSpace(path) ? string.Empty : Path.GetFullPath(path);
    }

    private static string BuildPluginSelectionKey(string guid, string configPath)
    {
        string normalizedGuid = string.IsNullOrWhiteSpace(guid) ? "(unknown)" : guid.Trim();
        string normalizedPath = string.IsNullOrWhiteSpace(configPath) ? string.Empty : NormalizePath(configPath);
        return normalizedGuid + "\n" + normalizedPath;
    }

    private static string GetProfileRootPath()
    {
        string configRoot = Path.GetFullPath(Paths.ConfigPath);
        return Path.GetFullPath(Path.Combine(configRoot, ProfileDirectoryName));
    }

    private static string GetProfilePath(string profileName)
    {
        string profileRoot = GetProfileRootPath();
        string safeName = SanitizeProfileName(profileName);
        string profilePath = Path.GetFullPath(Path.Combine(profileRoot, safeName + ProfileFileExtension));
        if (!IsPathInside(profilePath, profileRoot))
        {
            throw new InvalidOperationException($"Refusing to use profile path outside {profileRoot}: {profilePath}");
        }

        return profilePath;
    }

    private static string GetReportRootPath()
    {
        return Path.GetFullPath(Path.Combine(GetProfileRootPath(), ReportDirectoryName));
    }

    private static string GetDiagnosticReportPath()
    {
        string reportRoot = GetReportRootPath();
        string fileName = ReportFilePrefix + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss'Z'", CultureInfo.InvariantCulture) + ".md";
        string reportPath = Path.GetFullPath(Path.Combine(reportRoot, fileName));
        if (!IsPathInside(reportPath, reportRoot))
        {
            throw new InvalidOperationException($"Refusing to use report path outside {reportRoot}: {reportPath}");
        }

        return reportPath;
    }

    private static IEnumerable<string> DiscoverProfileNames()
    {
        string profileRoot = GetProfileRootPath();
        if (!Directory.Exists(profileRoot))
        {
            yield break;
        }

        foreach (string filePath in Directory.GetFiles(profileRoot, "*" + ProfileFileExtension, SearchOption.TopDirectoryOnly))
        {
            string profilePath = Path.GetFullPath(filePath);
            if (!IsPathInside(profilePath, profileRoot))
            {
                continue;
            }

            string fileName = Path.GetFileName(profilePath);
            if (!fileName.EndsWith(ProfileFileExtension, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            string profileName = fileName.Substring(0, fileName.Length - ProfileFileExtension.Length);
            if (profileName.Length == 0 || !string.Equals(profileName, SanitizeProfileName(profileName), StringComparison.Ordinal))
            {
                continue;
            }

            yield return profileName;
        }
    }

    private static string GetProfileStatus(string profileName)
    {
        try
        {
            string profilePath = GetProfilePath(profileName);
            FileInfo file = new(profilePath);
            if (!file.Exists)
            {
                return "Not saved yet";
            }

            return $"Saved {file.LastWriteTime:g}  |  {file.Length:N0} bytes";
        }
        catch
        {
            return "Invalid profile path";
        }
    }

    private static string SanitizeProfileName(string profileName)
    {
        string name = string.IsNullOrWhiteSpace(profileName) ? DefaultProfileName : profileName.Trim();
        StringBuilder builder = new(name.Length);
        foreach (char c in name)
        {
            builder.Append(char.IsLetterOrDigit(c) || c == '-' || c == '_' ? c : '_');
        }

        return builder.Length == 0 ? DefaultProfileName : builder.ToString();
    }

    private static string EncodeProfileField(string value)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(value ?? string.Empty));
    }

    private static bool TryDecodeProfileField(string text, out string value)
    {
        try
        {
            value = Encoding.UTF8.GetString(Convert.FromBase64String(text));
            return true;
        }
        catch
        {
            value = string.Empty;
            return false;
        }
    }

    private static string BuildProfileSettingKey(ManagedSetting setting)
    {
        return BuildProfileSettingKey(setting.OwnerGuid, setting.Section, setting.Key);
    }

    private static string BuildProfileSettingKey(string ownerGuid, string section, string key)
    {
        return ownerGuid + "\n" + section + "\n" + key;
    }

    private static string BuildSettingSectionStateKey(string ownerGuid, string displaySection)
    {
        return ownerGuid + "\n" + displaySection;
    }

    private static bool IsLegacyAvalonModManager(string guid, string name)
    {
        return string.Equals(guid, LegacyAvalonModManagerGuid, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(name, LegacyAvalonModManagerName, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsPathInside(string path, string root)
    {
        string normalizedRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        string normalizedPath = Path.GetFullPath(path);
        return string.Equals(normalizedPath, normalizedRoot, StringComparison.OrdinalIgnoreCase) ||
            normalizedPath.StartsWith(normalizedRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
            normalizedPath.StartsWith(normalizedRoot + Path.AltDirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryParseKeyCode(string value, out KeyCode key)
    {
        return Enum.TryParse(value, ignoreCase: true, out key);
    }

    private static bool TryParseControllerActionKeyCode(string value, out KeyCode key)
    {
        key = KeyCode.None;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        string cleaned = value.Trim();
        string normalized = cleaned
            .Replace(" ", string.Empty)
            .Replace("_", string.Empty)
            .Replace("-", string.Empty);
        if (string.Equals(normalized, "A", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(normalized, "Cross", StringComparison.OrdinalIgnoreCase))
        {
            key = KeyCode.JoystickButton0;
            return true;
        }

        if (string.Equals(normalized, "B", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(normalized, "Circle", StringComparison.OrdinalIgnoreCase))
        {
            key = KeyCode.JoystickButton1;
            return true;
        }

        if (string.Equals(normalized, "X", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(normalized, "Square", StringComparison.OrdinalIgnoreCase))
        {
            key = KeyCode.JoystickButton2;
            return true;
        }

        if (string.Equals(normalized, "Y", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(normalized, "Triangle", StringComparison.OrdinalIgnoreCase))
        {
            key = KeyCode.JoystickButton3;
            return true;
        }

        if (string.Equals(normalized, "LB", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(normalized, "LeftBumper", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(normalized, "LeftShoulder", StringComparison.OrdinalIgnoreCase))
        {
            key = KeyCode.JoystickButton4;
            return true;
        }

        if (string.Equals(normalized, "RB", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(normalized, "RightBumper", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(normalized, "RightShoulder", StringComparison.OrdinalIgnoreCase))
        {
            key = KeyCode.JoystickButton5;
            return true;
        }

        if (string.Equals(normalized, "View", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(normalized, "Back", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(normalized, "Select", StringComparison.OrdinalIgnoreCase))
        {
            key = KeyCode.JoystickButton6;
            return true;
        }

        if (string.Equals(normalized, "Menu", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(normalized, "Start", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(normalized, "Options", StringComparison.OrdinalIgnoreCase))
        {
            key = KeyCode.JoystickButton7;
            return true;
        }

        if (string.Equals(normalized, "L3", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(normalized, "LeftStick", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(normalized, "LeftStickClick", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(normalized, "LeftStickButton", StringComparison.OrdinalIgnoreCase))
        {
            key = KeyCode.JoystickButton8;
            return true;
        }

        if (string.Equals(normalized, "R3", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(normalized, "RightStick", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(normalized, "RightStickClick", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(normalized, "RightStickButton", StringComparison.OrdinalIgnoreCase))
        {
            key = KeyCode.JoystickButton9;
            return true;
        }

        return Enum.TryParse(cleaned, ignoreCase: true, out key);
    }

    private static bool IsUnityJoystickKeyCode(KeyCode keyCode)
    {
        int rawKeyCode = (int)keyCode;
        return rawKeyCode >= (int)KeyCode.JoystickButton0 && rawKeyCode <= (int)KeyCode.JoystickButton19;
    }

    private bool IsControllerHotkeyLayerKey(KeyCode keyCode)
    {
        return IsUnityJoystickKeyCode(keyCode) && !IsControllerActionLayerModifierKeyCode(keyCode);
    }

    private void RebuildControllerHotkeyLayerKeys()
    {
        _controllerHotkeyLayerKeys.Clear();
        foreach (ManagedSetting setting in _plugins.SelectMany(plugin => plugin.Settings))
        {
            if (setting.SettingType != typeof(KeyCode) ||
                !TryParseKeyCode(setting.GetSerializedValue(), out KeyCode keyCode) ||
                !IsControllerHotkeyLayerKey(keyCode))
            {
                continue;
            }

            _controllerHotkeyLayerKeys.Add(keyCode);
        }
    }

    private void RebuildControllerHotkeyLayerKeysFromConfigFiles()
    {
        try
        {
            Dictionary<string, FileConfigCatalog> configFiles = DiscoverConfigFiles();
            foreach (FileConfigEntry entry in configFiles.Values.SelectMany(catalog => catalog.Entries))
            {
                if (entry.SettingType != typeof(KeyCode) ||
                    !TryParseKeyCode(entry.Value, out KeyCode keyCode) ||
                    !IsControllerHotkeyLayerKey(keyCode))
                {
                    continue;
                }

                _controllerHotkeyLayerKeys.Add(keyCode);
            }

            if (_controllerHotkeyLayerKeys.Count > 0)
            {
                string keys = string.Join(", ", _controllerHotkeyLayerKeys
                    .Select(key => key.ToString())
                    .OrderBy(key => key, StringComparer.OrdinalIgnoreCase));
                Logger.LogInfo($"Controller hotkey layer startup cache: keys={keys}.");
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"Could not build controller hotkey startup cache from config files: {ex}");
        }
    }

    private void RecordControllerProbeButtonResultInstance(string bindingKind, string bindingValue, string methodName, object? playerInstance)
    {
        if (_controllerProbeEnabled?.Value != true)
        {
            return;
        }

        int maxLogs = Mathf.Clamp(_controllerProbeMaxLogsPerSession?.Value ?? 120, 0, 1000);
        if (maxLogs == 0)
        {
            return;
        }

        string cleanedKind = CleanProbeValue(bindingKind);
        string cleanedValue = CleanProbeValue(bindingValue);
        string cleanedMethod = CleanProbeValue(methodName);
        EnsureControllerProbeActionMapLoaded();
        ControllerProbeActionInfo? actionInfo = ResolveControllerProbeActionInfo(cleanedKind, cleanedValue);
        ControllerProbePhysicalSourceInfo physicalSourceInfo = ResolveControllerProbePhysicalSourceInfo(
            playerInstance,
            cleanedKind,
            cleanedValue,
            actionInfo);
        if (_controllerProbeControllerSourcesOnly?.Value == true && !physicalSourceInfo.HasControllerSource)
        {
            RecordControllerProbeSuppressedNonControllerSource(physicalSourceInfo);
            return;
        }

        string physicalElementKey = physicalSourceInfo.PhysicalElementKey;
        bool firstSeenPhysicalElement = physicalSourceInfo.HasControllerSource &&
            !string.IsNullOrWhiteSpace(physicalElementKey) &&
            _controllerProbeSeenPhysicalElementKeys.Add(physicalElementKey);
        bool capBypass = false;
        if (_controllerProbeLogCount >= maxLogs)
        {
            if (_controllerProbeLogFirstSeenPhysicalElementsAfterCap?.Value == true && firstSeenPhysicalElement)
            {
                capBypass = true;
            }
            else
            {
                return;
            }
        }

        string logKey = cleanedMethod + "\n" + cleanedKind + "\n" + cleanedValue;
        float now = Time.unscaledTime;
        float repeatSuppressSeconds = Mathf.Max(0f, _controllerProbeRepeatSuppressSeconds?.Value ?? 0.75f);
        if (!firstSeenPhysicalElement &&
            repeatSuppressSeconds > 0f &&
            _controllerProbeLastLogTimes.TryGetValue(logKey, out float lastLoggedAt) &&
            now - lastLoggedAt < repeatSuppressSeconds)
        {
            return;
        }

        _controllerProbeLastLogTimes[logKey] = now;
        _controllerProbeLogCount++;
        RecordControllerProbeSummary(cleanedKind, cleanedValue, cleanedMethod, actionInfo);
        Logger.LogInfo(
            "Controller probe button result: " +
            $"method={cleanedMethod}; bindingKind={cleanedKind}; binding={cleanedValue}; " +
            BuildControllerProbeActionSegment(actionInfo) + "; " +
            physicalSourceInfo.ToLogSegment() + "; " +
            $"source=rewired-action-result; diagnosticOnly=true; vanillaInputSuppressed=false; " +
            $"firstSeenPhysicalElement={(firstSeenPhysicalElement ? "true" : "false")}; " +
            $"capBypass={(capBypass ? "true" : "false")}; " +
            $"count={_controllerProbeLogCount}/{maxLogs}.");
        MaybeLogControllerProbeSummary(force: false);

        if (_controllerProbeLogCount >= maxLogs && !_controllerProbeMaxLogWarningLogged)
        {
            _controllerProbeMaxLogWarningLogged = true;
            Logger.LogWarning("Controller probe reached MaxLogsPerSession. Disable and re-enable the plugin or raise ControllerProbe.MaxLogsPerSession for more rows.");
            MaybeLogControllerProbeSummary(force: true);
        }
    }

    private void RecordControllerProbeSuppressedNonControllerSource(ControllerProbePhysicalSourceInfo physicalSourceInfo)
    {
        _controllerProbeSuppressedNonControllerSourceCount++;
        if (_controllerProbeSuppressedNonControllerSourceCount != 1 &&
            _controllerProbeSuppressedNonControllerSourceCount % ControllerProbeSuppressedSourceLogInterval != 0)
        {
            return;
        }

        Logger.LogInfo(
            "Controller probe suppressed non-controller source: " +
            $"suppressed={_controllerProbeSuppressedNonControllerSourceCount.ToString(CultureInfo.InvariantCulture)}; " +
            physicalSourceInfo.ToFilterLogSegment() + "; " +
            "controllerSourcesOnly=true; diagnosticOnly=true; vanillaInputSuppressed=false.");
    }

    private void EnsureControllerProbeActionMapLoaded()
    {
        if (_controllerProbeActionMapLoaded)
        {
            return;
        }

        float now = Time.unscaledTime;
        if (now < _controllerProbeNextActionMapLoadAttemptAt)
        {
            return;
        }

        _controllerProbeNextActionMapLoadAttemptAt = now + 5f;

        try
        {
            Type? reInputType = AccessTools.TypeByName("Rewired.ReInput");
            if (reInputType == null)
            {
                Logger.LogWarning("Controller probe action map unavailable: Rewired.ReInput type was not found.");
                _controllerProbeActionMapLoaded = true;
                return;
            }

            PropertyInfo? isReadyProperty = reInputType.GetProperty("isReady", BindingFlags.Public | BindingFlags.Static);
            if (isReadyProperty?.GetValue(null) is bool isReady && !isReady)
            {
                return;
            }

            object? mapping = reInputType.GetProperty("mapping", BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
            object? actionsValue = mapping?.GetType().GetProperty("Actions", BindingFlags.Public | BindingFlags.Instance)?.GetValue(mapping);
            if (actionsValue is not IEnumerable actions)
            {
                Logger.LogWarning("Controller probe action map unavailable: Rewired mapping.Actions could not be read.");
                _controllerProbeActionMapLoaded = true;
                return;
            }

            foreach (object action in actions)
            {
                int actionId = ReadIntProperty(action, "id", -1);
                if (actionId < 0)
                {
                    continue;
                }

                string actionName = CleanProbeValue(ReadStringProperty(action, "name", "<unnamed>"));
                ControllerProbeActionInfo info = new(
                    actionId,
                    actionName,
                    CleanProbeValue(ReadStringProperty(action, "descriptiveName", "<empty>")),
                    CleanProbeValue(ReadStringProperty(action, "type", "<unknown>")),
                    ReadIntProperty(action, "categoryId", -1),
                    ReadBoolProperty(action, "userAssignable"));

                _controllerProbeActionsById[actionId] = info;
                if (!string.IsNullOrWhiteSpace(actionName) && !string.Equals(actionName, "<empty>", StringComparison.Ordinal))
                {
                    _controllerProbeActionsByName[actionName] = info;
                }
            }

            _controllerProbeActionMapLoaded = true;
            Logger.LogInfo(
                "Controller probe action map loaded: " +
                $"actions={_controllerProbeActionsById.Count}; diagnosticOnly=true; vanillaInputSuppressed=false.");
            DumpControllerProbeActionMapIfNeeded();
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"Controller probe action map unavailable: {ex.GetType().Name}: {CleanProbeValue(ex.Message)}");
            _controllerProbeActionMapLoaded = true;
        }
    }

    private void DumpControllerProbeActionMapIfNeeded()
    {
        if (_controllerProbeActionMapDumped ||
            _controllerProbeDumpActionMapOnFirstCapture?.Value != true ||
            _controllerProbeActionsById.Count == 0)
        {
            return;
        }

        _controllerProbeActionMapDumped = true;
        int maxRows = Mathf.Clamp(_controllerProbeMaxActionMapRows?.Value ?? 400, 0, 1000);
        if (maxRows == 0)
        {
            Logger.LogInfo("Controller probe action map dump skipped because ControllerProbe.MaxActionMapRows is 0.");
            return;
        }

        int emitted = 0;
        foreach (ControllerProbeActionInfo action in _controllerProbeActionsById.Values.OrderBy(info => info.Id))
        {
            if (emitted >= maxRows)
            {
                break;
            }

            Logger.LogInfo(
                "Controller probe action map: " +
                BuildControllerProbeActionSegment(action) + "; diagnosticOnly=true; vanillaInputSuppressed=false.");
            emitted++;
        }

        if (emitted < _controllerProbeActionsById.Count)
        {
            Logger.LogWarning(
                "Controller probe action map dump clipped: " +
                $"emitted={emitted}; total={_controllerProbeActionsById.Count}; raise ControllerProbe.MaxActionMapRows for the full map.");
        }
    }

    private ControllerProbeActionInfo? ResolveControllerProbeActionInfo(string bindingKind, string bindingValue)
    {
        if (string.Equals(bindingKind, "id", StringComparison.OrdinalIgnoreCase) &&
            int.TryParse(bindingValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out int actionId) &&
            _controllerProbeActionsById.TryGetValue(actionId, out ControllerProbeActionInfo idInfo))
        {
            return idInfo;
        }

        if (string.Equals(bindingKind, "name", StringComparison.OrdinalIgnoreCase) &&
            _controllerProbeActionsByName.TryGetValue(bindingValue, out ControllerProbeActionInfo nameInfo))
        {
            return nameInfo;
        }

        return null;
    }

    private ControllerProbePhysicalSourceInfo ResolveControllerProbePhysicalSourceInfo(
        object? playerInstance,
        string bindingKind,
        string bindingValue,
        ControllerProbeActionInfo? actionInfo)
    {
        if (playerInstance == null)
        {
            return ControllerProbePhysicalSourceInfo.Unknown("<no-player-instance>");
        }

        try
        {
            int actionId = actionInfo?.Id ?? ResolveControllerProbeActionId(bindingKind, bindingValue);
            int playerId = ReadIntMember(playerInstance, "id", -1);
            string playerName = CleanProbeValue(ReadStringMember(playerInstance, "name", "<unknown>"));
            string playerDescriptiveName = CleanProbeValue(ReadStringMember(playerInstance, "descriptiveName", "<unknown>"));
            object? activeController = ResolveLastActiveController(playerInstance);
            ControllerProbeControllerInfo activeControllerInfo = ControllerProbeControllerInfo.FromController(activeController);
            bool sourcesClipped = false;
            List<ControllerProbeInputSourceInfo> sourceInfos = actionId >= 0
                ? ResolveCurrentInputSources(playerInstance, actionId, out sourcesClipped)
                : new List<ControllerProbeInputSourceInfo>();

            return new ControllerProbePhysicalSourceInfo(
                playerId,
                playerName,
                playerDescriptiveName,
                activeControllerInfo,
                sourceInfos,
                sourcesClipped);
        }
        catch (Exception ex)
        {
            return ControllerProbePhysicalSourceInfo.Unknown(ex.GetType().Name + ":" + CleanProbeValue(ex.Message));
        }
    }

    private static int ResolveControllerProbeActionId(string bindingKind, string bindingValue)
    {
        return string.Equals(bindingKind, "id", StringComparison.OrdinalIgnoreCase) &&
            int.TryParse(bindingValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out int actionId)
            ? actionId
            : -1;
    }

    private static object? ResolveLastActiveController(object playerInstance)
    {
        object? controllers = ReadMemberValue(playerInstance, "controllers");
        return InvokeNoArgumentMethod(controllers, "GetLastActiveController");
    }

    private static List<ControllerProbeInputSourceInfo> ResolveCurrentInputSources(object playerInstance, int actionId, out bool sourcesClipped)
    {
        sourcesClipped = false;
        List<ControllerProbeInputSourceInfo> sourceInfos = new();
        MethodInfo? getSources = playerInstance.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(method =>
            {
                if (!string.Equals(method.Name, "GetCurrentInputSources", StringComparison.Ordinal))
                {
                    return false;
                }

                ParameterInfo[] parameters = method.GetParameters();
                return parameters.Length == 1 && parameters[0].ParameterType == typeof(int);
            });

        if (getSources?.Invoke(playerInstance, new object[] { actionId }) is not IEnumerable sources)
        {
            return sourceInfos;
        }

        foreach (object source in sources)
        {
            if (sourceInfos.Count >= ControllerProbePhysicalSourceEntryLimit)
            {
                sourcesClipped = true;
                break;
            }

            sourceInfos.Add(ControllerProbeInputSourceInfo.FromSource(source));
        }

        return sourceInfos;
    }

    private void RecordControllerProbeSummary(
        string bindingKind,
        string bindingValue,
        string methodName,
        ControllerProbeActionInfo? actionInfo)
    {
        string summaryKey = bindingKind + "\n" + bindingValue;
        if (!_controllerProbeSummaries.TryGetValue(summaryKey, out ControllerProbeCaptureSummary summary))
        {
            summary = new ControllerProbeCaptureSummary(bindingKind, bindingValue);
            _controllerProbeSummaries[summaryKey] = summary;
        }

        summary.Record(methodName, actionInfo);
    }

    private void MaybeLogControllerProbeSummary(bool force)
    {
        if (_controllerProbeSummaries.Count == 0)
        {
            return;
        }

        int summaryEveryRows = Mathf.Clamp(_controllerProbeSummaryEveryRows?.Value ?? 40, 0, 250);
        if (!force)
        {
            if (summaryEveryRows == 0 || _controllerProbeLogCount - _controllerProbeLastSummaryLogCount < summaryEveryRows)
            {
                return;
            }

            _controllerProbeLastSummaryLogCount = _controllerProbeLogCount;
        }
        else if (_controllerProbeLastSummaryLogCount == _controllerProbeLogCount)
        {
            return;
        }
        else
        {
            _controllerProbeLastSummaryLogCount = _controllerProbeLogCount;
        }

        string[] entries = _controllerProbeSummaries.Values
            .OrderByDescending(summary => summary.Total)
            .ThenBy(summary => summary.BindingValue, StringComparer.Ordinal)
            .Take(ControllerProbeSummaryEntryLimit)
            .Select(summary => summary.ToLogEntry())
            .ToArray();

        Logger.LogInfo(
            "Controller probe summary: " +
            $"capturedRows={_controllerProbeLogCount}; uniqueBindings={_controllerProbeSummaries.Count}; " +
            $"actionMapLoaded={_controllerProbeActionMapLoaded}; top={string.Join(" | ", entries)}.");
    }

    private static string BuildControllerProbeActionSegment(ControllerProbeActionInfo? actionInfo)
    {
        if (actionInfo == null)
        {
            return "actionId=<unknown>; actionName=<unknown>; actionDescriptiveName=<unknown>; actionType=<unknown>; actionCategoryId=<unknown>; userAssignable=<unknown>";
        }

        return
            $"actionId={actionInfo.Id.ToString(CultureInfo.InvariantCulture)}; " +
            $"actionName={actionInfo.Name}; " +
            $"actionDescriptiveName={actionInfo.DescriptiveName}; " +
            $"actionType={actionInfo.ActionType}; " +
            $"actionCategoryId={actionInfo.CategoryId.ToString(CultureInfo.InvariantCulture)}; " +
            $"userAssignable={(actionInfo.UserAssignable ? "true" : "false")}";
    }

    private static int ReadIntProperty(object target, string propertyName, int fallback)
    {
        object? value = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance)?.GetValue(target);
        try
        {
            return value == null ? fallback : Convert.ToInt32(value, CultureInfo.InvariantCulture);
        }
        catch
        {
            return fallback;
        }
    }

    private static bool ReadBoolProperty(object target, string propertyName)
    {
        return target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance)?.GetValue(target) is bool value && value;
    }

    private static string ReadStringProperty(object target, string propertyName, string fallback)
    {
        object? value = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance)?.GetValue(target);
        return value == null ? fallback : Convert.ToString(value, CultureInfo.InvariantCulture) ?? fallback;
    }

    private static object? ReadMemberValue(object? target, string memberName)
    {
        if (target == null)
        {
            return null;
        }

        Type type = target.GetType();
        PropertyInfo? property = type.GetProperty(memberName, BindingFlags.Public | BindingFlags.Instance);
        if (property != null)
        {
            return property.GetValue(target);
        }

        FieldInfo? field = type.GetField(memberName, BindingFlags.Public | BindingFlags.Instance);
        return field?.GetValue(target);
    }

    private static int ReadIntMember(object? target, string memberName, int fallback)
    {
        object? value = ReadMemberValue(target, memberName);
        try
        {
            return value == null ? fallback : Convert.ToInt32(value, CultureInfo.InvariantCulture);
        }
        catch
        {
            return fallback;
        }
    }

    private static string ReadStringMember(object? target, string memberName, string fallback)
    {
        object? value = ReadMemberValue(target, memberName);
        return value == null ? fallback : Convert.ToString(value, CultureInfo.InvariantCulture) ?? fallback;
    }

    private static bool ReadBoolMember(object? target, string memberName)
    {
        return ReadMemberValue(target, memberName) is bool value && value;
    }

    private static object? InvokeNoArgumentMethod(object? target, string methodName)
    {
        if (target == null)
        {
            return null;
        }

        MethodInfo? method = target.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(candidate =>
                string.Equals(candidate.Name, methodName, StringComparison.Ordinal) &&
                candidate.GetParameters().Length == 0);
        return method?.Invoke(target, null);
    }

    private static string CleanProbeValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "<empty>";
        }

        return value.Replace('\r', ' ').Replace('\n', ' ').Replace(';', ',').Replace('|', '/').Trim();
    }

    private static string CleanStatusText(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        string cleaned = value.Replace('\r', ' ').Replace('\n', ' ').Trim();
        if (maxLength <= 0 || cleaned.Length <= maxLength)
        {
            return cleaned;
        }

        return cleaned.Substring(0, Math.Max(0, maxLength - 3)).TrimEnd() + "...";
    }

    private static bool IsControllerSourceType(string controllerType)
    {
        return !string.IsNullOrWhiteSpace(controllerType) &&
            !string.Equals(controllerType, "<unknown>", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(controllerType, "<none>", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(controllerType, "Mouse", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(controllerType, "Keyboard", StringComparison.OrdinalIgnoreCase);
    }

    private sealed class ControllerProbePhysicalSourceInfo
    {
        private readonly int _playerId;
        private readonly string _playerName;
        private readonly string _playerDescriptiveName;
        private readonly ControllerProbeControllerInfo _activeController;
        private readonly List<ControllerProbeInputSourceInfo> _sources;
        private readonly bool _sourcesClipped;
        private readonly string _error;
        private readonly string _fallbackSourceType;
        private readonly string _fallbackElementName;

        internal ControllerProbePhysicalSourceInfo(
            int playerId,
            string playerName,
            string playerDescriptiveName,
            ControllerProbeControllerInfo activeController,
            List<ControllerProbeInputSourceInfo> sources,
            bool sourcesClipped)
        {
            _playerId = playerId;
            _playerName = playerName;
            _playerDescriptiveName = playerDescriptiveName;
            _activeController = activeController;
            _sources = sources;
            _sourcesClipped = sourcesClipped;
            _error = "<none>";
            _fallbackSourceType = string.Empty;
            _fallbackElementName = string.Empty;
        }

        private ControllerProbePhysicalSourceInfo(string error)
        {
            _playerId = -1;
            _playerName = "<unknown>";
            _playerDescriptiveName = "<unknown>";
            _activeController = ControllerProbeControllerInfo.Unknown;
            _sources = new List<ControllerProbeInputSourceInfo>();
            _sourcesClipped = false;
            _error = CleanProbeValue(error);
            _fallbackSourceType = string.Empty;
            _fallbackElementName = string.Empty;
        }

        private ControllerProbePhysicalSourceInfo(KeyCode keyCode)
        {
            _playerId = -1;
            _playerName = "Unity";
            _playerDescriptiveName = "Unity KeyCode fallback";
            _activeController = ControllerProbeControllerInfo.Unknown;
            _sources = new List<ControllerProbeInputSourceInfo>();
            _sourcesClipped = false;
            _error = "<none>";
            _fallbackSourceType = "Joystick";
            _fallbackElementName = keyCode.ToString();
        }

        internal static ControllerProbePhysicalSourceInfo Unknown(string error)
        {
            return new ControllerProbePhysicalSourceInfo(error);
        }

        internal static ControllerProbePhysicalSourceInfo UnityJoystick(KeyCode keyCode)
        {
            return new ControllerProbePhysicalSourceInfo(keyCode);
        }

        internal bool HasControllerSource => _sources.Count > 0
            ? _sources.Any(source => source.IsControllerLike)
            : _activeController.IsControllerLike || _fallbackSourceType.Length > 0;

        internal string PhysicalElementKey
        {
            get
            {
                if (_fallbackSourceType.Length > 0)
                {
                    return _fallbackSourceType + ":" + _fallbackElementName;
                }

                if (_sources.Count == 0)
                {
                    return _activeController.ControllerType + ":<no-current-source>";
                }

                return string.Join(
                    " / ",
                    _sources.Select(source => source.ElementKey)
                        .Distinct(StringComparer.Ordinal)
                        .OrderBy(value => value, StringComparer.Ordinal)
                        .ToArray());
            }
        }

        internal string ToLogSegment()
        {
            string sourceSummary = _fallbackSourceType.Length > 0
                ? $"sourceControllerType={_fallbackSourceType}, sourceElementIdentifierName={_fallbackElementName}, source=unity-keycode"
                : _sources.Count == 0
                ? "<none>"
                : string.Join(" / ", _sources.Select(source => source.ToLogEntry()).ToArray());
            return
                $"playerId={_playerId.ToString(CultureInfo.InvariantCulture)}; " +
                $"playerName={_playerName}; " +
                $"playerDescriptiveName={_playerDescriptiveName}; " +
                $"activeController={_activeController.ToLogEntry()}; " +
                $"physicalSourceCount={_sources.Count.ToString(CultureInfo.InvariantCulture)}; " +
                $"physicalSourcesClipped={(_sourcesClipped ? "true" : "false")}; " +
                $"physicalSources={sourceSummary}; " +
                $"physicalSourceError={_error}";
        }

        internal string ToFilterLogSegment()
        {
            string sourceTypes = _fallbackSourceType.Length > 0
                ? _fallbackSourceType
                : _sources.Count == 0
                ? "<none>"
                : string.Join(",", _sources.Select(source => source.SourceControllerType).Distinct(StringComparer.Ordinal).ToArray());
            return
                $"playerId={_playerId.ToString(CultureInfo.InvariantCulture)}; " +
                $"activeControllerType={_activeController.ControllerType}; " +
                $"physicalSourceCount={_sources.Count.ToString(CultureInfo.InvariantCulture)}; " +
                $"physicalSourceTypes={sourceTypes}; " +
                $"physicalSourceError={_error}";
        }
    }

    private sealed class ControllerProbeControllerInfo
    {
        internal static readonly ControllerProbeControllerInfo Unknown = new(null);

        private readonly int _id;
        private readonly string _type;
        private readonly string _name;
        private readonly string _hardwareName;
        private readonly string _hardwareIdentifier;
        private readonly string _mapTypeString;
        private readonly bool _connected;
        private readonly bool _enabled;

        private ControllerProbeControllerInfo(object? controller)
        {
            _id = ReadIntMember(controller, "id", -1);
            _type = CleanProbeValue(ReadStringMember(controller, "type", "<unknown>"));
            _name = CleanProbeValue(ReadStringMember(controller, "name", "<unknown>"));
            _hardwareName = CleanProbeValue(ReadStringMember(controller, "hardwareName", "<unknown>"));
            _hardwareIdentifier = CleanProbeValue(ReadStringMember(controller, "hardwareIdentifier", "<unknown>"));
            _mapTypeString = CleanProbeValue(ReadStringMember(controller, "mapTypeString", "<unknown>"));
            _connected = ReadBoolMember(controller, "isConnected");
            _enabled = ReadBoolMember(controller, "enabled");
        }

        internal static ControllerProbeControllerInfo FromController(object? controller)
        {
            return new ControllerProbeControllerInfo(controller);
        }

        internal string ControllerType => _type;

        internal bool IsControllerLike => IsControllerSourceType(_type);

        internal string ToLogEntry()
        {
            return
                $"type={_type}, id={_id.ToString(CultureInfo.InvariantCulture)}, name={_name}, " +
                $"hardwareName={_hardwareName}, hardwareIdentifier={_hardwareIdentifier}, " +
                $"mapType={_mapTypeString}, connected={(_connected ? "true" : "false")}, enabled={(_enabled ? "true" : "false")}";
        }
    }

    private sealed class ControllerProbeInputSourceInfo
    {
        private readonly ControllerProbeControllerInfo _controller;
        private readonly ControllerProbeControllerMapInfo _controllerMap;
        private readonly ControllerProbeActionElementMapInfo _actionElementMap;
        private readonly string _sourceControllerType;
        private readonly string _sourceElementIdentifierName;

        private ControllerProbeInputSourceInfo(object source)
        {
            object? controller = ReadMemberValue(source, "controller");
            object? controllerMap = ReadMemberValue(source, "controllerMap");
            object? actionElementMap = ReadMemberValue(source, "actionElementMap");
            _controller = ControllerProbeControllerInfo.FromController(controller);
            _controllerMap = ControllerProbeControllerMapInfo.FromControllerMap(controllerMap);
            _actionElementMap = ControllerProbeActionElementMapInfo.FromActionElementMap(actionElementMap);
            _sourceControllerType = CleanProbeValue(ReadStringMember(source, "controllerType", "<unknown>"));
            _sourceElementIdentifierName = CleanProbeValue(ReadStringMember(source, "elementIdentifierName", "<unknown>"));
        }

        internal static ControllerProbeInputSourceInfo FromSource(object source)
        {
            return new ControllerProbeInputSourceInfo(source);
        }

        internal string SourceControllerType => _sourceControllerType;

        internal bool IsControllerLike => IsControllerSourceType(_sourceControllerType);

        internal string ElementKey => _sourceControllerType + ":" + _sourceElementIdentifierName;

        internal string ToLogEntry()
        {
            return
                $"sourceControllerType={_sourceControllerType}, sourceElementIdentifierName={_sourceElementIdentifierName}, " +
                $"controller=({_controller.ToLogEntry()}), map=({_controllerMap.ToLogEntry()}), element=({_actionElementMap.ToLogEntry()})";
        }
    }

    private sealed class ControllerProbeControllerMapInfo
    {
        private readonly int _id;
        private readonly int _controllerId;
        private readonly int _playerId;
        private readonly int _categoryId;
        private readonly int _layoutId;
        private readonly string _name;
        private readonly string _controllerType;
        private readonly bool _enabled;

        private ControllerProbeControllerMapInfo(object? controllerMap)
        {
            _id = ReadIntMember(controllerMap, "id", -1);
            _controllerId = ReadIntMember(controllerMap, "controllerId", -1);
            _playerId = ReadIntMember(controllerMap, "playerId", -1);
            _categoryId = ReadIntMember(controllerMap, "categoryId", -1);
            _layoutId = ReadIntMember(controllerMap, "layoutId", -1);
            _name = CleanProbeValue(ReadStringMember(controllerMap, "name", "<unknown>"));
            _controllerType = CleanProbeValue(ReadStringMember(controllerMap, "controllerType", "<unknown>"));
            _enabled = ReadBoolMember(controllerMap, "enabled");
        }

        internal static ControllerProbeControllerMapInfo FromControllerMap(object? controllerMap)
        {
            return new ControllerProbeControllerMapInfo(controllerMap);
        }

        internal string ToLogEntry()
        {
            return
                $"id={_id.ToString(CultureInfo.InvariantCulture)}, name={_name}, controllerType={_controllerType}, " +
                $"controllerId={_controllerId.ToString(CultureInfo.InvariantCulture)}, playerId={_playerId.ToString(CultureInfo.InvariantCulture)}, " +
                $"categoryId={_categoryId.ToString(CultureInfo.InvariantCulture)}, layoutId={_layoutId.ToString(CultureInfo.InvariantCulture)}, " +
                $"enabled={(_enabled ? "true" : "false")}";
        }
    }

    private sealed class ControllerProbeActionElementMapInfo
    {
        private readonly int _id;
        private readonly int _actionId;
        private readonly int _elementIdentifierId;
        private readonly int _elementIndex;
        private readonly string _elementIdentifierName;
        private readonly string _elementType;
        private readonly string _axisRange;
        private readonly string _axisContribution;
        private readonly string _keyCode;
        private readonly bool _enabled;

        private ControllerProbeActionElementMapInfo(object? actionElementMap)
        {
            _id = ReadIntMember(actionElementMap, "id", -1);
            _actionId = ReadIntMember(actionElementMap, "actionId", -1);
            _elementIdentifierId = ReadIntMember(actionElementMap, "elementIdentifierId", -1);
            _elementIndex = ReadIntMember(actionElementMap, "elementIndex", -1);
            _elementIdentifierName = CleanProbeValue(ReadStringMember(actionElementMap, "elementIdentifierName", "<unknown>"));
            _elementType = CleanProbeValue(ReadStringMember(actionElementMap, "elementType", "<unknown>"));
            _axisRange = CleanProbeValue(ReadStringMember(actionElementMap, "axisRange", "<unknown>"));
            _axisContribution = CleanProbeValue(ReadStringMember(actionElementMap, "axisContribution", "<unknown>"));
            _keyCode = CleanProbeValue(ReadStringMember(actionElementMap, "keyCode", "<unknown>"));
            _enabled = ReadBoolMember(actionElementMap, "enabled");
        }

        internal static ControllerProbeActionElementMapInfo FromActionElementMap(object? actionElementMap)
        {
            return new ControllerProbeActionElementMapInfo(actionElementMap);
        }

        internal string ToLogEntry()
        {
            return
                $"id={_id.ToString(CultureInfo.InvariantCulture)}, actionId={_actionId.ToString(CultureInfo.InvariantCulture)}, " +
                $"elementIdentifierId={_elementIdentifierId.ToString(CultureInfo.InvariantCulture)}, elementIdentifierName={_elementIdentifierName}, " +
                $"elementIndex={_elementIndex.ToString(CultureInfo.InvariantCulture)}, elementType={_elementType}, " +
                $"axisRange={_axisRange}, axisContribution={_axisContribution}, keyCode={_keyCode}, enabled={(_enabled ? "true" : "false")}";
        }
    }

    private sealed class ControllerActionRegistration
    {
        internal ControllerActionRegistration(string actionId, string displayName, string category, string description, Action callback)
        {
            ActionId = actionId;
            DisplayName = displayName;
            Category = category;
            Description = description;
            Callback = callback;
        }

        internal string ActionId { get; }

        internal string DisplayName { get; }

        internal string Category { get; }

        internal string Description { get; }

        internal Action Callback { get; }
    }

    private sealed class StatusProviderRegistration
    {
        internal StatusProviderRegistration(string providerId, string displayName, string category, string description, Func<FoAModStatusSnapshot> snapshotProvider)
        {
            ProviderId = providerId;
            DisplayName = displayName;
            Category = category;
            Description = description;
            SnapshotProvider = snapshotProvider;
        }

        internal string ProviderId { get; }

        internal string DisplayName { get; }

        internal string Category { get; }

        internal string Description { get; }

        internal Func<FoAModStatusSnapshot> SnapshotProvider { get; }
    }

    private sealed class StatusProviderSnapshotView
    {
        internal StatusProviderSnapshotView(
            FoAModStatusLevel level,
            string summary,
            string detail,
            string schema,
            string updatedUtc,
            string[] lines,
            bool linesClipped)
        {
            Level = level;
            Summary = summary;
            Detail = detail;
            Schema = schema;
            UpdatedUtc = updatedUtc;
            Lines = lines;
            LinesClipped = linesClipped;
        }

        internal FoAModStatusLevel Level { get; }

        internal string Summary { get; }

        internal string Detail { get; }

        internal string Schema { get; }

        internal string UpdatedUtc { get; }

        internal string[] Lines { get; }

        internal bool LinesClipped { get; }
    }

    private sealed class DashboardCustomUiOwner
    {
        internal DashboardCustomUiOwner(string ownerId, string scopeType, string mode)
        {
            OwnerId = ownerId;
            ScopeType = scopeType;
            Mode = mode;
        }

        internal string OwnerId { get; }

        internal string ScopeType { get; }

        internal string Mode { get; }
    }

    private sealed class DashboardProfileCoverage
    {
        internal DashboardProfileCoverage(
            string profileName,
            string profileFileName,
            bool exists,
            int matchedSettings,
            int totalSettings,
            int ownerRows,
            int unknownRows,
            int invalidRows,
            string error)
        {
            ProfileName = profileName;
            ProfileFileName = profileFileName;
            Exists = exists;
            MatchedSettings = matchedSettings;
            TotalSettings = totalSettings;
            OwnerRows = ownerRows;
            UnknownRows = unknownRows;
            InvalidRows = invalidRows;
            Error = error;
        }

        internal static DashboardProfileCoverage Empty { get; } = new(
            DefaultProfileName,
            DefaultProfileName + ProfileFileExtension,
            exists: false,
            matchedSettings: 0,
            totalSettings: 0,
            ownerRows: 0,
            unknownRows: 0,
            invalidRows: 0,
            error: string.Empty);

        internal string ProfileName { get; }

        internal string ProfileFileName { get; }

        internal bool Exists { get; }

        internal int MatchedSettings { get; }

        internal int TotalSettings { get; }

        internal int OwnerRows { get; }

        internal int UnknownRows { get; }

        internal int InvalidRows { get; }

        internal string Error { get; }
    }

    private sealed class ControllerActionBinding
    {
        internal ControllerActionBinding(string targetAction, string actionId)
        {
            TargetAction = CleanProbeValue(targetAction);
            ActionId = NormalizeControllerActionId(actionId);
        }

        internal string TargetAction { get; }

        internal string ActionId { get; }
    }

    private sealed class ControllerChordProbeTargetState
    {
        internal ControllerChordProbeTargetState(
            string actionName,
            ControllerProbeActionInfo? actionInfo,
            bool nameHeld,
            bool nameDown,
            bool idHeld,
            bool idDown)
        {
            ActionName = CleanProbeValue(actionName);
            ActionInfo = actionInfo;
            NameHeld = nameHeld;
            NameDown = nameDown;
            IdHeld = idHeld;
            IdDown = idDown;
        }

        private string ActionName { get; }

        private ControllerProbeActionInfo? ActionInfo { get; }

        private bool NameHeld { get; }

        private bool NameDown { get; }

        private bool IdHeld { get; }

        private bool IdDown { get; }

        internal bool IsHeld => NameHeld || IdHeld;

        internal bool IsDown => NameDown || IdDown;

        internal string ToScanEntry()
        {
            string actionId = ActionInfo?.Id.ToString(CultureInfo.InvariantCulture) ?? "<missing>";
            return
                $"{ActionName}(id={actionId},nameHeld={(NameHeld ? "true" : "false")}," +
                $"nameDown={(NameDown ? "true" : "false")},idHeld={(IdHeld ? "true" : "false")}," +
                $"idDown={(IdDown ? "true" : "false")})";
        }
    }

    private sealed class ControllerProbeActionInfo
    {
        internal ControllerProbeActionInfo(
            int id,
            string name,
            string descriptiveName,
            string actionType,
            int categoryId,
            bool userAssignable)
        {
            Id = id;
            Name = name;
            DescriptiveName = descriptiveName;
            ActionType = actionType;
            CategoryId = categoryId;
            UserAssignable = userAssignable;
        }

        internal int Id { get; }

        internal string Name { get; }

        internal string DescriptiveName { get; }

        internal string ActionType { get; }

        internal int CategoryId { get; }

        internal bool UserAssignable { get; }
    }

    private sealed class ControllerProbeCaptureSummary
    {
        internal ControllerProbeCaptureSummary(string bindingKind, string bindingValue)
        {
            BindingKind = bindingKind;
            BindingValue = bindingValue;
        }

        internal string BindingKind { get; }

        internal string BindingValue { get; }

        internal ControllerProbeActionInfo? ActionInfo { get; private set; }

        internal int Total { get; private set; }

        private int DownCount { get; set; }

        private int HoldCount { get; set; }

        private int UpCount { get; set; }

        private int OtherCount { get; set; }

        internal void Record(string methodName, ControllerProbeActionInfo? actionInfo)
        {
            ActionInfo ??= actionInfo;
            Total++;

            if (methodName.EndsWith("Down", StringComparison.Ordinal))
            {
                DownCount++;
            }
            else if (methodName.EndsWith("Up", StringComparison.Ordinal))
            {
                UpCount++;
            }
            else if (string.Equals(methodName, "GetButton", StringComparison.Ordinal))
            {
                HoldCount++;
            }
            else
            {
                OtherCount++;
            }
        }

        internal string ToLogEntry()
        {
            string actionName = ActionInfo?.Name ?? "<unknown>";
            string descriptiveName = ActionInfo?.DescriptiveName ?? "<unknown>";
            return
                $"[{BindingKind}={BindingValue}, action={actionName}, desc={descriptiveName}, " +
                $"total={Total.ToString(CultureInfo.InvariantCulture)}, down={DownCount.ToString(CultureInfo.InvariantCulture)}, " +
                $"hold={HoldCount.ToString(CultureInfo.InvariantCulture)}, up={UpCount.ToString(CultureInfo.InvariantCulture)}, " +
                $"other={OtherCount.ToString(CultureInfo.InvariantCulture)}]";
        }
    }

    private static int FindConfigEntryLine(string[] lines, string section, string key)
    {
        string currentSection = string.Empty;
        for (int i = 0; i < lines.Length; i++)
        {
            string trimmed = lines[i].Trim();
            if (TryReadSection(trimmed, out string parsedSection))
            {
                currentSection = parsedSection;
                continue;
            }

            if (!string.Equals(currentSection, section, StringComparison.Ordinal))
            {
                continue;
            }

            if (TryReadAssignment(lines[i], out string parsedKey, out _) &&
                string.Equals(parsedKey, key, StringComparison.Ordinal))
            {
                return i;
            }
        }

        return -1;
    }

    private static string ReplaceConfigEntryValue(string line, string value)
    {
        int equalsIndex = line.IndexOf('=');
        if (equalsIndex < 0)
        {
            return line;
        }

        int valueStart = equalsIndex + 1;
        while (valueStart < line.Length && char.IsWhiteSpace(line[valueStart]))
        {
            valueStart++;
        }

        return line.Substring(0, valueStart) + value;
    }

    private static string FormatCount(int count, string singular)
    {
        return count == 1 ? $"1 {singular}" : $"{count} {singular}s";
    }

    private static string GetFriendlyKind(ManagedSetting setting)
    {
        Type settingType = setting.SettingType;

        if (settingType == typeof(bool))
        {
            return "Toggle";
        }

        if (settingType == typeof(KeyCode))
        {
            return "Hotkey";
        }

        if (setting.HasChoiceValues || settingType.IsEnum)
        {
            return "Choice";
        }

        if (settingType == typeof(byte) ||
            settingType == typeof(sbyte) ||
            settingType == typeof(short) ||
            settingType == typeof(ushort) ||
            settingType == typeof(int) ||
            settingType == typeof(uint) ||
            settingType == typeof(long) ||
            settingType == typeof(ulong) ||
            settingType == typeof(float) ||
            settingType == typeof(double) ||
            settingType == typeof(decimal))
        {
            return setting.NumericRange == null ? "Number" : "Slider";
        }

        if (settingType == typeof(string))
        {
            return "Text";
        }

        return "Value";
    }

    private static bool TryCreateNumericRange(ConfigEntryBase entry, out NumericRange range)
    {
        range = default;

        if (!IsNumericSetting(entry.SettingType))
        {
            return false;
        }

        AcceptableValueBase? acceptableValues = entry.Description.AcceptableValues;
        if (acceptableValues == null)
        {
            return false;
        }

        Type acceptableType = acceptableValues.GetType();
        if (!acceptableType.IsGenericType || acceptableType.GetGenericTypeDefinition() != typeof(AcceptableValueRange<>))
        {
            return false;
        }

        PropertyInfo? minProperty = acceptableType.GetProperty("MinValue");
        PropertyInfo? maxProperty = acceptableType.GetProperty("MaxValue");
        object? minValue = minProperty?.GetValue(acceptableValues);
        object? maxValue = maxProperty?.GetValue(acceptableValues);
        if (minValue == null || maxValue == null)
        {
            return false;
        }

        double minimum = Convert.ToDouble(minValue, CultureInfo.InvariantCulture);
        double maximum = Convert.ToDouble(maxValue, CultureInfo.InvariantCulture);
        if (!IsFinite(minimum) || !IsFinite(maximum) || minimum >= maximum)
        {
            return false;
        }

        range = new NumericRange(minimum, maximum, IsIntegerSetting(entry.SettingType));
        return true;
    }

    private static IReadOnlyList<string> CreateChoiceValues(ConfigEntryBase entry, FileConfigEntry? fileMetadata)
    {
        if (entry.SettingType.IsEnum)
        {
            return Enum.GetNames(entry.SettingType);
        }

        if (TryCreateAcceptableValueList(entry.Description.AcceptableValues, out IReadOnlyList<string> metadataChoices))
        {
            return metadataChoices;
        }

        return fileMetadata?.ChoiceValues ?? Array.Empty<string>();
    }

    private static bool TryCreateAcceptableValueList(AcceptableValueBase? acceptableValues, out IReadOnlyList<string> choices)
    {
        choices = Array.Empty<string>();
        if (acceptableValues == null)
        {
            return false;
        }

        Type acceptableType = acceptableValues.GetType();
        if (!acceptableType.IsGenericType || acceptableType.GetGenericTypeDefinition() != typeof(AcceptableValueList<>))
        {
            return false;
        }

        PropertyInfo? valuesProperty = acceptableType.GetProperty("AcceptableValues", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        object? rawValues = valuesProperty?.GetValue(acceptableValues);
        if (rawValues is not System.Collections.IEnumerable enumerable)
        {
            return false;
        }

        choices = enumerable
            .Cast<object?>()
            .Select(value => Convert.ToString(value, CultureInfo.InvariantCulture))
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return choices.Count > 0;
    }

    private static bool IsNumericSetting(Type settingType)
    {
        return settingType == typeof(byte) ||
            settingType == typeof(sbyte) ||
            settingType == typeof(short) ||
            settingType == typeof(ushort) ||
            settingType == typeof(int) ||
            settingType == typeof(uint) ||
            settingType == typeof(long) ||
            settingType == typeof(ulong) ||
            settingType == typeof(float) ||
            settingType == typeof(double) ||
            settingType == typeof(decimal);
    }

    private static bool IsIntegerSetting(Type settingType)
    {
        return settingType == typeof(byte) ||
            settingType == typeof(sbyte) ||
            settingType == typeof(short) ||
            settingType == typeof(ushort) ||
            settingType == typeof(int) ||
            settingType == typeof(uint) ||
            settingType == typeof(long) ||
            settingType == typeof(ulong);
    }

    private static double ReadCurrentNumber(ManagedSetting setting, NumericRange range)
    {
        try
        {
            return Clamp(Convert.ToDouble(setting.GetSerializedValue(), CultureInfo.InvariantCulture), range.Minimum, range.Maximum);
        }
        catch
        {
            return range.Minimum;
        }
    }

    private static bool TryParseNumber(string value, out double result)
    {
        return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result) && IsFinite(result);
    }

    private static bool IsFinite(double value)
    {
        return !double.IsNaN(value) && !double.IsInfinity(value);
    }

    private static double Clamp(double value, double minimum, double maximum)
    {
        if (value < minimum)
        {
            return minimum;
        }

        return value > maximum ? maximum : value;
    }

    private static string FormatNumber(double value, bool integer)
    {
        return integer
            ? Math.Round(value).ToString("0", CultureInfo.InvariantCulture)
            : value.ToString("0.###", CultureInfo.InvariantCulture);
    }

    private static string FormatRangeEndpoint(double value, bool integer)
    {
        return integer
            ? value.ToString("0", CultureInfo.InvariantCulture)
            : value.ToString("0.##", CultureInfo.InvariantCulture);
    }

    private static bool ShouldShowMultiplierPresets(NumericRange range)
    {
        return !range.IsInteger && range.Minimum <= 0d && range.Maximum >= 1d && range.Maximum <= 10d;
    }

    private void DrawRangePresetButton(ManagedSetting setting, NumericRange range, string label, double value)
    {
        if (value < range.Minimum || value > range.Maximum)
        {
            return;
        }

        if (GUILayout.Button(label, _styles!.SecondaryButton, GUILayout.Width(Px(54f)), GUILayout.Height(Px(34f))))
        {
            setting.DraftValue = FormatNumber(value, range.IsInteger);
        }
    }

    private static string HumanizeIdentifier(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "General";
        }

        string normalized = value.Replace('_', ' ').Replace('-', ' ').Replace('.', ' ');
        StringBuilder builder = new(normalized.Length + 8);
        char previous = '\0';

        for (int i = 0; i < normalized.Length; i++)
        {
            char current = normalized[i];
            char next = i + 1 < normalized.Length ? normalized[i + 1] : '\0';

            if (i > 0 &&
                current != ' ' &&
                previous != ' ' &&
                ((char.IsUpper(current) && (char.IsLower(previous) || char.IsDigit(previous))) ||
                 (char.IsUpper(current) && char.IsUpper(previous) && char.IsLower(next)) ||
                 (char.IsDigit(current) && char.IsLetter(previous))))
            {
                builder.Append(' ');
            }

            builder.Append(current);
            previous = current;
        }

        return builder.ToString().Trim();
    }

    private void OnDestroy()
    {
        Config.Save();
        _harmony?.UnpatchSelf();
        ReleaseControllerCursorMouseButton();
        RestoreWorldTimeScale();
        RestoreGameInputState();
        RestoreCursorState();
        if (_instance == this)
        {
            _instance = null;
        }

        ReleaseStyleTextures();
    }

    private void ReleaseStyleTextures()
    {
        foreach (Texture2D texture in _ownedTextures)
        {
            Destroy(texture);
        }

        _ownedTextures.Clear();
    }

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern bool GetClientRect(IntPtr hWnd, out WinRect lpRect);

    [DllImport("user32.dll")]
    private static extern bool ClientToScreen(IntPtr hWnd, ref WinPoint lpPoint);

    [DllImport("user32.dll")]
    private static extern bool SetCursorPos(int x, int y);

    [DllImport("user32.dll")]
    private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

    [StructLayout(LayoutKind.Sequential)]
    private struct WinPoint
    {
        internal int X;
        internal int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct WinRect
    {
        internal int Left;
        internal int Top;
        internal int Right;
        internal int Bottom;
    }

    private sealed class FileConfigCatalog
    {
        private readonly Dictionary<string, FileConfigEntry> _entriesByKey;

        internal FileConfigCatalog(string guid, string name, string version, string configPath, List<FileConfigEntry> entries)
        {
            Guid = guid;
            Name = name;
            Version = version;
            ConfigPath = configPath;
            Entries = entries;
            _entriesByKey = entries
                .GroupBy(entry => BuildConfigEntryKey(entry.Section, entry.Key), StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
        }

        internal string Guid { get; }
        internal string Name { get; }
        internal string Version { get; }
        internal string ConfigPath { get; }
        internal List<FileConfigEntry> Entries { get; }

        internal FileConfigEntry? FindEntry(string section, string key)
        {
            return _entriesByKey.TryGetValue(BuildConfigEntryKey(section, key), out FileConfigEntry entry) ? entry : null;
        }

        private static string BuildConfigEntryKey(string section, string key)
        {
            return section + "\n" + key;
        }
    }

    private sealed class FileConfigEntry
    {
        internal FileConfigEntry(
            string configPath,
            int lineIndex,
            string section,
            string key,
            string value,
            Type settingType,
            string? defaultValue,
            string description,
            IReadOnlyList<string> choiceValues,
            NumericRange? numericRange)
        {
            ConfigPath = configPath;
            LineIndex = lineIndex;
            Section = section;
            Key = key;
            Value = value;
            SettingType = settingType;
            DefaultValue = defaultValue;
            Description = description;
            ChoiceValues = choiceValues;
            NumericRange = numericRange;
        }

        internal string ConfigPath { get; }
        internal int LineIndex { get; private set; }
        internal string Section { get; }
        internal string Key { get; }
        internal string Value { get; private set; }
        internal Type SettingType { get; }
        internal string? DefaultValue { get; }
        internal string Description { get; }
        internal IReadOnlyList<string> ChoiceValues { get; }
        internal NumericRange? NumericRange { get; }

        internal void UpdateValue(string value, int lineIndex)
        {
            Value = value;
            LineIndex = lineIndex;
        }
    }

    private sealed class ManagedPlugin
    {
        internal ManagedPlugin(string guid, string name, string version, string configPath, List<ManagedSetting> settings)
        {
            Guid = guid;
            SelectionKey = BuildPluginSelectionKey(guid, configPath);
            Name = name;
            Version = version;
            ConfigPath = configPath;
            Settings = settings;
            Issues = new List<ManagerIssue>();
        }

        internal string Guid { get; }
        internal string SelectionKey { get; }
        internal string Name { get; }
        internal string Version { get; }
        internal string ConfigPath { get; }
        internal List<ManagedSetting> Settings { get; }
        internal List<ManagerIssue> Issues { get; }
    }

    private sealed class ManagerIssue
    {
        private ManagerIssue(string severity, string message, string detail, bool isBlocking, string settingId, string settingDisplayName, string category)
        {
            Severity = severity;
            Message = message;
            Detail = detail;
            IsBlocking = isBlocking;
            SettingId = settingId;
            SettingDisplayName = settingDisplayName;
            Category = string.IsNullOrWhiteSpace(category) ? "General" : category;
        }

        internal string Severity { get; }
        internal string Message { get; }
        internal string Detail { get; }
        internal bool IsBlocking { get; }
        internal string SettingId { get; }
        internal string SettingDisplayName { get; }
        internal string Category { get; }
        internal bool HasSettingTarget => !string.IsNullOrWhiteSpace(SettingId);

        internal static ManagerIssue Warning(string message, string detail, string settingId = "", string settingDisplayName = "", string category = "General")
        {
            return new ManagerIssue("Warning", message, detail, isBlocking: false, settingId, settingDisplayName, category);
        }

        internal static ManagerIssue Blocking(string message, string detail, string settingId = "", string settingDisplayName = "", string category = "General")
        {
            return new ManagerIssue("Problem", message, detail, isBlocking: true, settingId, settingDisplayName, category);
        }
    }

    private enum ManagerSubPanel
    {
        None,
        Dashboard,
        Saved,
        Presets,
        Profiles,
        Commands,
        CustomUi,
        Status,
        Warnings
    }

    private sealed class ProfilePreset
    {
        internal ProfilePreset(string name, string label)
        {
            Name = name;
            Label = label;
        }

        internal string Name { get; }
        internal string Label { get; }
    }

    private sealed class ControllerActionTargetChoice
    {
        internal ControllerActionTargetChoice(string actionName, string label)
        {
            ActionName = actionName;
            Label = label;
        }

        internal string ActionName { get; }

        internal string Label { get; }
    }

    private sealed class ControllerHotkeyChoice
    {
        internal ControllerHotkeyChoice(KeyCode keyCode, string label)
        {
            KeyCode = keyCode;
            Label = label;
        }

        internal KeyCode KeyCode { get; }

        internal string Label { get; }
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

    private sealed class ManagedSetting
    {
        private const int MaxChoiceButtonCount = 48;

        internal ManagedSetting(string ownerGuid, ConfigEntryBase entry, FileConfigEntry? fileMetadata)
        {
            SettingUiMetadata ui = SettingUiMetadata.From(entry.Description.Tags);
            OwnerGuid = ownerGuid;
            Entry = entry;
            FileEntry = fileMetadata;
            Section = entry.Definition.Section;
            Key = entry.Definition.Key;
            SettingType = entry.SettingType;
            DisplaySection = string.IsNullOrWhiteSpace(ui.DisplaySection) ? HumanizeIdentifier(Section) : ui.DisplaySection;
            DisplayName = string.IsNullOrWhiteSpace(ui.DisplayName) ? HumanizeIdentifier(Key) : ui.DisplayName;
            Description = string.IsNullOrWhiteSpace(entry.Description.Description) ? fileMetadata?.Description ?? string.Empty : entry.Description.Description;
            NumericRange = TryCreateNumericRange(entry, out NumericRange range) ? range : fileMetadata?.NumericRange;
            ChoiceValues = CreateChoiceValues(entry, fileMetadata);
            ChoiceLabels = ui.ChoiceLabels;
            DraftValue = entry.GetSerializedValue();
            Id = $"{ownerGuid}::{Section}::{Key}";
            SectionOrder = ui.SectionOrder;
            Order = ui.Order;
            Hidden = ui.Hidden;
        }

        internal ManagedSetting(string ownerGuid, FileConfigEntry fileEntry)
        {
            OwnerGuid = ownerGuid;
            Entry = null;
            FileEntry = fileEntry;
            Section = fileEntry.Section;
            Key = fileEntry.Key;
            SettingType = fileEntry.SettingType;
            DisplaySection = HumanizeIdentifier(Section);
            DisplayName = HumanizeIdentifier(Key);
            Description = fileEntry.Description;
            NumericRange = fileEntry.NumericRange;
            ChoiceValues = fileEntry.ChoiceValues;
            ChoiceLabels = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            DraftValue = fileEntry.Value;
            Id = $"{ownerGuid}::{Section}::{Key}";
            SectionOrder = int.MaxValue;
            Order = int.MaxValue;
            Hidden = false;
        }

        internal string OwnerGuid { get; }
        internal ConfigEntryBase? Entry { get; }
        internal FileConfigEntry? FileEntry { get; }
        internal string Id { get; }
        internal string Section { get; }
        internal string Key { get; }
        internal Type SettingType { get; }
        internal string DisplaySection { get; }
        internal string DisplayName { get; }
        internal string Description { get; }
        internal NumericRange? NumericRange { get; }
        internal IReadOnlyList<string> ChoiceValues { get; }
        internal IReadOnlyDictionary<string, string> ChoiceLabels { get; }
        internal string DraftValue { get; set; }
        internal int SectionOrder { get; }
        internal int Order { get; }
        internal bool Hidden { get; }

        internal bool HasChoiceValues => SettingType != typeof(KeyCode) && ChoiceValues.Count > 0 && ChoiceValues.Count <= MaxChoiceButtonCount;

        internal string GetChoiceLabel(string value)
        {
            return ChoiceLabels.TryGetValue(value, out string label) ? label : HumanizeIdentifier(value);
        }

        internal string GetSerializedValue()
        {
            return Entry?.GetSerializedValue() ?? FileEntry?.Value ?? string.Empty;
        }

        internal object? GetDefaultValue()
        {
            return Entry?.DefaultValue ?? FileEntry?.DefaultValue;
        }

        internal bool TryReadBool(out bool value)
        {
            if (Entry?.BoxedValue is bool boolValue)
            {
                value = boolValue;
                return true;
            }

            return bool.TryParse(GetSerializedValue(), out value);
        }
    }

    private sealed class SettingUiMetadata
    {
        internal string? DisplaySection { get; private set; }
        internal string? DisplayName { get; private set; }
        internal IReadOnlyDictionary<string, string> ChoiceLabels { get; private set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        internal int SectionOrder { get; private set; } = int.MaxValue;
        internal int Order { get; private set; } = int.MaxValue;
        internal bool Hidden { get; private set; }

        internal static SettingUiMetadata From(IEnumerable<object>? tags)
        {
            SettingUiMetadata metadata = new();
            if (tags == null)
            {
                return metadata;
            }

            foreach (object tag in tags)
            {
                if (tag == null)
                {
                    continue;
                }

                if (TryReadString(tag, "DisplaySection", out string? displaySection) ||
                    TryReadString(tag, "Section", out displaySection))
                {
                    metadata.DisplaySection = displaySection;
                }

                if (TryReadString(tag, "DisplayName", out string? displayName) ||
                    TryReadString(tag, "DispName", out displayName))
                {
                    metadata.DisplayName = displayName;
                }

                if (TryReadString(tag, "ChoiceLabels", out string? choiceLabels) ||
                    TryReadString(tag, "ChoiceDisplayNames", out choiceLabels))
                {
                    metadata.ChoiceLabels = ParseChoiceLabels(choiceLabels ?? string.Empty);
                }

                if (TryReadInt(tag, "SectionOrder", out int sectionOrder))
                {
                    metadata.SectionOrder = sectionOrder;
                }

                if (TryReadInt(tag, "Order", out int order) ||
                    TryReadInt(tag, "SortOrder", out order))
                {
                    metadata.Order = order;
                }

                if ((TryReadBool(tag, "Hidden", out bool hidden) && hidden) ||
                    (TryReadBool(tag, "Browsable", out bool browsable) && !browsable))
                {
                    metadata.Hidden = true;
                }
            }

            return metadata;
        }

        private static IReadOnlyDictionary<string, string> ParseChoiceLabels(string text)
        {
            Dictionary<string, string> labels = new(StringComparer.OrdinalIgnoreCase);
            foreach (string part in text.Split(new[] { ';', '|' }, StringSplitOptions.RemoveEmptyEntries))
            {
                int separatorIndex = part.IndexOf('=');
                if (separatorIndex <= 0 || separatorIndex >= part.Length - 1)
                {
                    continue;
                }

                string value = part.Substring(0, separatorIndex).Trim();
                string label = part.Substring(separatorIndex + 1).Trim();
                if (value.Length > 0 && label.Length > 0)
                {
                    labels[value] = label;
                }
            }

            return labels;
        }

        private static bool TryReadString(object source, string name, out string? value)
        {
            value = ReadMember(source, name) as string;
            return !string.IsNullOrWhiteSpace(value);
        }

        private static bool TryReadInt(object source, string name, out int value)
        {
            object? raw = ReadMember(source, name);
            if (raw is int intValue)
            {
                value = intValue;
                return true;
            }

            if (raw != null && int.TryParse(Convert.ToString(raw, CultureInfo.InvariantCulture), NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
            {
                return true;
            }

            value = 0;
            return false;
        }

        private static bool TryReadBool(object source, string name, out bool value)
        {
            object? raw = ReadMember(source, name);
            if (raw is bool boolValue)
            {
                value = boolValue;
                return true;
            }

            if (raw != null && bool.TryParse(Convert.ToString(raw, CultureInfo.InvariantCulture), out value))
            {
                return true;
            }

            value = false;
            return false;
        }

        private static object? ReadMember(object source, string name)
        {
            Type type = source.GetType();
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase;

            PropertyInfo? property = type.GetProperty(name, flags);
            if (property != null && property.GetIndexParameters().Length == 0)
            {
                return property.GetValue(source);
            }

            FieldInfo? field = type.GetField(name, flags);
            return field?.GetValue(source);
        }
    }

    private readonly struct NumericRange
    {
        internal NumericRange(double minimum, double maximum, bool isInteger)
        {
            Minimum = minimum;
            Maximum = maximum;
            IsInteger = isInteger;
        }

        internal double Minimum { get; }
        internal double Maximum { get; }
        internal bool IsInteger { get; }
    }

    private sealed class UiTextureSet
    {
        internal int LoadedCount { get; set; }
        internal int LooseCount { get; set; }
        internal int EmbeddedCount { get; set; }
        internal int MissingCount { get; set; }
        internal Texture2D? Window { get; set; }
        internal Texture2D? Header { get; set; }
        internal Texture2D? Panel { get; set; }
        internal Texture2D? Card { get; set; }
        internal Texture2D? Field { get; set; }
        internal Texture2D? Button { get; set; }
        internal Texture2D? ButtonHover { get; set; }
        internal Texture2D? Secondary { get; set; }
        internal Texture2D? Selected { get; set; }
        internal Texture2D? Pill { get; set; }
        internal Texture2D? RangeTrack { get; set; }
        internal Texture2D? RangeThumb { get; set; }

        internal void Assign(string label, Texture2D? texture)
        {
            switch (label)
            {
                case "window":
                    Window = texture;
                    break;
                case "header":
                    Header = texture;
                    break;
                case "panel":
                    Panel = texture;
                    break;
                case "card":
                    Card = texture;
                    break;
                case "field":
                    Field = texture;
                    break;
                case "button":
                    Button = texture;
                    break;
                case "button-hover":
                    ButtonHover = texture;
                    break;
                case "secondary-button":
                    Secondary = texture;
                    break;
                case "selected-button":
                    Selected = texture;
                    break;
                case "pill":
                    Pill = texture;
                    break;
                case "slider-track":
                    RangeTrack = texture;
                    break;
                case "slider-thumb":
                    RangeThumb = texture;
                    break;
            }
        }
    }

    private readonly struct TaintedInterfaceManagerTextureBinding
    {
        internal TaintedInterfaceManagerTextureBinding(string label, string textureIdSuffix)
        {
            Label = label;
            TextureIdSuffix = textureIdSuffix;
        }

        internal string Label { get; }
        internal string TextureIdSuffix { get; }
    }

    private readonly struct TaintedInterfacePackChoice
    {
        internal TaintedInterfacePackChoice(string id, string displayName, string buttonLabel)
        {
            Id = id;
            DisplayName = displayName;
            ButtonLabel = buttonLabel;
        }

        internal string Id { get; }
        internal string DisplayName { get; }
        internal string ButtonLabel { get; }
    }

    private sealed class UiStyles
    {
        internal UiStyles(Plugin owner)
        {
            UiTextureSet darkFantasy = owner.LoadManagerTextureSet();
            bool lineArtPresetPolish = UsesLineArtPresetPolish(owner._lastManagerSkinPackId);
            RectOffset? lineArtPanelBorder = lineArtPresetPolish ? new RectOffset(36, 36, 40, 40) : null;
            RectOffset? lineArtHeaderBorder = lineArtPresetPolish ? new RectOffset(36, 36, 18, 18) : null;
            RectOffset? lineArtControlBorder = lineArtPresetPolish ? new RectOffset(24, 24, 14, 14) : null;
            RectOffset? lineArtPillBorder = lineArtPresetPolish ? new RectOffset(18, 18, 10, 10) : null;
            Texture2D window = darkFantasy.Window ?? owner.CreateTexture(new Color(0.115f, 0.135f, 0.145f, 0.84f));
            Texture2D header = darkFantasy.Header ?? owner.CreateTexture(new Color(0.165f, 0.200f, 0.210f, 0.92f));
            Texture2D panel = darkFantasy.Panel ?? owner.CreateTexture(new Color(0.125f, 0.150f, 0.160f, 0.84f));
            Texture2D card = darkFantasy.Card ?? owner.CreateTexture(new Color(0.155f, 0.180f, 0.188f, 0.88f));
            Texture2D field = darkFantasy.Field ?? owner.CreateTexture(new Color(0.075f, 0.092f, 0.098f, 0.96f));
            Texture2D button = darkFantasy.Button ?? owner.CreateTexture(new Color(0.125f, 0.405f, 0.430f, 0.92f));
            Texture2D buttonHover = darkFantasy.ButtonHover ?? owner.CreateTexture(new Color(0.180f, 0.525f, 0.550f, 0.96f));
            Texture2D selected = darkFantasy.Selected ?? owner.CreateTexture(new Color(0.120f, 0.395f, 0.425f, 0.96f));
            Texture2D secondary = darkFantasy.Secondary ?? owner.CreateTexture(new Color(0.105f, 0.128f, 0.137f, 0.92f));
            Texture2D capture = darkFantasy.ButtonHover ?? owner.CreateTexture(new Color(0.520f, 0.185f, 0.140f, 0.98f));
            Texture2D pill = darkFantasy.Pill ?? owner.CreateTexture(new Color(0.080f, 0.100f, 0.108f, 0.92f));
            Texture2D dirty = owner.CreateTexture(new Color(0.570f, 0.380f, 0.105f, 0.95f));
            Texture2D saved = owner.CreateTexture(new Color(0.100f, 0.395f, 0.260f, 0.94f));
            Texture2D rangeTrack = darkFantasy.RangeTrack ?? owner.CreateTexture(new Color(0.065f, 0.090f, 0.098f, 0.96f));
            Texture2D rangeThumb = darkFantasy.RangeThumb ?? owner.CreateTexture(new Color(0.790f, 0.560f, 0.240f, 0.96f));
            if (lineArtPresetPolish)
            {
                card = owner.CreateTexture(new Color(0.135f, 0.118f, 0.082f, 0.90f));
                field = owner.CreateTexture(new Color(0.052f, 0.054f, 0.046f, 0.97f));
                button = owner.CreateTexture(new Color(0.205f, 0.164f, 0.074f, 0.95f));
                buttonHover = owner.CreateTexture(new Color(0.300f, 0.238f, 0.105f, 0.97f));
                selected = owner.CreateTexture(new Color(0.245f, 0.192f, 0.082f, 0.97f));
                secondary = owner.CreateTexture(new Color(0.110f, 0.098f, 0.070f, 0.94f));
                pill = owner.CreateTexture(new Color(0.082f, 0.078f, 0.062f, 0.95f));
                rangeTrack = owner.CreateTexture(new Color(0.035f, 0.047f, 0.050f, 0.98f));
                rangeThumb = owner.CreateTexture(new Color(0.690f, 0.555f, 0.265f, 0.98f));
            }

            SelectedStrip = owner.CreateTexture(new Color(0.860f, 0.610f, 0.250f, 0.98f));

            Window = new GUIStyle(GUI.skin.window)
            {
                normal = { background = window, textColor = new Color(0.90f, 0.88f, 0.82f) },
                hover = { background = window, textColor = new Color(0.90f, 0.88f, 0.82f) },
                active = { background = window, textColor = new Color(0.90f, 0.88f, 0.82f) },
                focused = { background = window, textColor = new Color(0.90f, 0.88f, 0.82f) },
                onNormal = { background = window, textColor = new Color(0.90f, 0.88f, 0.82f) },
                onHover = { background = window, textColor = new Color(0.90f, 0.88f, 0.82f) },
                onActive = { background = window, textColor = new Color(0.90f, 0.88f, 0.82f) },
                onFocused = { background = window, textColor = new Color(0.90f, 0.88f, 0.82f) },
                padding = Pad(14, 14, 14, 14),
                border = lineArtPanelBorder ?? Pad(8, 8, 8, 8)
            };

            Header = new GUIStyle(GUI.skin.box)
            {
                normal = { background = header },
                hover = { background = header },
                active = { background = header },
                focused = { background = header },
                onNormal = { background = header },
                onHover = { background = header },
                onActive = { background = header },
                onFocused = { background = header },
                padding = Pad(14, 14, 10, 10),
                margin = Pad(0, 0, 0, 0)
            };
            ApplyOptionalBorder(Header, lineArtHeaderBorder);

            Sidebar = new GUIStyle(GUI.skin.box)
            {
                normal = { background = panel },
                hover = { background = panel },
                active = { background = panel },
                focused = { background = panel },
                onNormal = { background = panel },
                onHover = { background = panel },
                onActive = { background = panel },
                onFocused = { background = panel },
                padding = Pad(12, 12, 12, 12),
                margin = Pad(0, 0, 0, 0)
            };
            ApplyOptionalBorder(Sidebar, lineArtPanelBorder);

            Content = new GUIStyle(Sidebar)
            {
                padding = Pad(16, 16, 14, 14)
            };
            ApplyOptionalBorder(Content, lineArtPanelBorder);

            SettingCard = new GUIStyle(GUI.skin.box)
            {
                normal = { background = card },
                hover = { background = card },
                active = { background = card },
                focused = { background = card },
                onNormal = { background = card },
                onHover = { background = card },
                onActive = { background = card },
                onFocused = { background = card },
                padding = Pad(34, 34, 20, 20),
                margin = Pad(0, 4, 0, 0)
            };
            ApplyOptionalBorder(SettingCard, lineArtPanelBorder);

            Title = Label(28, FontStyle.Bold, new Color(1.00f, 0.93f, 0.72f));
            Title.wordWrap = false;
            Title.clipping = TextClipping.Clip;
            Subtitle = Label(14, FontStyle.Bold, new Color(0.78f, 0.86f, 0.86f));
            Subtitle.wordWrap = false;
            Subtitle.clipping = TextClipping.Clip;
            HeaderMeta = Label(14, FontStyle.Bold, new Color(0.82f, 0.92f, 0.92f), TextAnchor.MiddleCenter);
            PanelTitle = Label(18, FontStyle.Bold, new Color(1.00f, 0.88f, 0.62f));
            SearchLabel = Label(14, FontStyle.Bold, new Color(0.82f, 0.90f, 0.90f));
            PluginTitle = Label(22, FontStyle.Bold, new Color(0.95f, 0.91f, 0.78f));
            PluginDetail = Label(14, FontStyle.Bold, new Color(0.76f, 0.84f, 0.84f));
            SectionLabel = Label(17, FontStyle.Bold, new Color(1.00f, 0.68f, 0.34f));
            SettingName = Label(17, FontStyle.Bold, new Color(0.97f, 0.94f, 0.86f));
            SettingDescription = Label(14, FontStyle.Bold, new Color(0.78f, 0.83f, 0.80f));
            EmptyState = Label(16, FontStyle.Bold, new Color(0.80f, 0.86f, 0.84f), TextAnchor.MiddleCenter);
            FooterText = Label(14, FontStyle.Bold, new Color(0.78f, 0.83f, 0.80f));
            EnabledLabel = Label(15, FontStyle.Bold, new Color(0.54f, 0.88f, 0.66f));
            DisabledLabel = Label(15, FontStyle.Bold, new Color(0.88f, 0.55f, 0.48f));
            PluginName = Label(16, FontStyle.Bold, new Color(0.91f, 0.92f, 0.84f));
            PluginNameSelected = Label(16, FontStyle.Bold, new Color(1.00f, 0.94f, 0.68f));
            PluginMeta = Label(13, FontStyle.Bold, new Color(0.70f, 0.78f, 0.78f));
            PluginMetaSelected = Label(13, FontStyle.Bold, new Color(0.86f, 0.98f, 0.95f));

            SearchField = new GUIStyle(GUI.skin.textField)
            {
                normal = { background = field, textColor = new Color(0.98f, 0.95f, 0.86f) },
                focused = { background = field, textColor = Color.white },
                active = { background = field, textColor = Color.white },
                hover = { background = field, textColor = Color.white },
                padding = Pad(10, 10, 6, 6),
                fontSize = FontPx(15),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };
            ApplyOptionalBorder(SearchField, lineArtControlBorder);

            ValueField = new GUIStyle(SearchField)
            {
                alignment = TextAnchor.MiddleLeft
            };
            ApplyOptionalBorder(ValueField, lineArtControlBorder);

            RangeEndpoint = Label(14, FontStyle.Bold, new Color(0.82f, 0.90f, 0.90f), TextAnchor.MiddleCenter);
            RangeSlider = new GUIStyle(GUI.skin.horizontalSlider)
            {
                normal = { background = rangeTrack },
                hover = { background = rangeTrack },
                active = { background = rangeTrack },
                focused = { background = rangeTrack },
                fixedHeight = Px(9f),
                margin = Pad(6, 6, 11, 0)
            };
            RangeSliderThumb = new GUIStyle(GUI.skin.horizontalSliderThumb)
            {
                normal = { background = rangeThumb },
                hover = { background = buttonHover },
                active = { background = buttonHover },
                focused = { background = rangeThumb },
                fixedWidth = Px(18f),
                fixedHeight = Px(24f)
            };

            HeaderButton = Button(button, buttonHover, 14, lineArtControlBorder);
            ActionButton = Button(button, buttonHover, 14, lineArtControlBorder);
            SecondaryButton = Button(secondary, buttonHover, 14, lineArtControlBorder);
            SectionButton = new GUIStyle(SecondaryButton)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = FontPx(16),
                padding = Pad(36, 18, 6, 6),
                margin = Pad(0, 0, 0, 0)
            };
            ApplyOptionalBorder(SectionButton, lineArtControlBorder);
            CaptureButton = Button(capture, buttonHover, 14, lineArtControlBorder);
            ResizeHandle = new GUIStyle(SecondaryButton)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = FontPx(14),
                fontStyle = FontStyle.Bold,
                margin = Pad(0, 0, 0, 0),
                padding = Pad(0, 0, 0, 0)
            };

            PluginButton = new GUIStyle(GUI.skin.box)
            {
                normal = { background = secondary },
                hover = { background = button },
                active = { background = buttonHover },
                focused = { background = secondary },
                onNormal = { background = secondary },
                onHover = { background = button },
                onActive = { background = buttonHover },
                onFocused = { background = secondary },
                padding = Pad(0, 0, 0, 0),
                margin = Pad(0, 0, 0, 0)
            };
            ApplyOptionalBorder(PluginButton, lineArtControlBorder);

            PluginButtonSelected = new GUIStyle(PluginButton)
            {
                normal = { background = selected },
                hover = { background = selected },
                active = { background = selected },
                focused = { background = selected },
                onNormal = { background = selected },
                onHover = { background = selected },
                onActive = { background = selected },
                onFocused = { background = selected }
            };
            ApplyOptionalBorder(PluginButtonSelected, lineArtControlBorder);

            InvisibleButton = new GUIStyle(GUI.skin.button)
            {
                normal = { background = null, textColor = Color.clear },
                hover = { background = null, textColor = Color.clear },
                active = { background = null, textColor = Color.clear },
                focused = { background = null, textColor = Color.clear },
                onNormal = { background = null, textColor = Color.clear },
                onHover = { background = null, textColor = Color.clear },
                onActive = { background = null, textColor = Color.clear },
                onFocused = { background = null, textColor = Color.clear },
                border = Pad(0, 0, 0, 0),
                margin = Pad(0, 0, 0, 0),
                padding = Pad(0, 0, 0, 0)
            };

            TypePill = Pill(pill, new Color(0.74f, 0.82f, 0.82f), lineArtPillBorder);
            ValuePill = Pill(pill, new Color(0.95f, 0.87f, 0.68f), lineArtPillBorder);
            CountPill = Pill(pill, new Color(0.95f, 0.87f, 0.68f), lineArtPillBorder);
            HotkeyPill = Pill(button, new Color(0.96f, 0.94f, 0.86f), lineArtControlBorder);
            DirtyPill = Pill(dirty, new Color(1.00f, 0.92f, 0.72f), lineArtPillBorder);
            SavedPill = Pill(saved, new Color(0.72f, 0.96f, 0.80f), lineArtPillBorder);
            Footer = new GUIStyle(GUI.skin.box)
            {
                normal = { background = panel },
                hover = { background = panel },
                active = { background = panel },
                focused = { background = panel },
                onNormal = { background = panel },
                onHover = { background = panel },
                onActive = { background = panel },
                onFocused = { background = panel },
                padding = Pad(12, 12, 7, 7),
                margin = Pad(0, 0, 0, 0)
            };
            ApplyOptionalBorder(Footer, lineArtHeaderBorder);
        }

        internal Texture2D SelectedStrip { get; }
        internal GUIStyle Window { get; }
        internal GUIStyle Header { get; }
        internal GUIStyle Sidebar { get; }
        internal GUIStyle Content { get; }
        internal GUIStyle SettingCard { get; }
        internal GUIStyle Title { get; }
        internal GUIStyle Subtitle { get; }
        internal GUIStyle HeaderMeta { get; }
        internal GUIStyle HotkeyPill { get; }
        internal GUIStyle HeaderButton { get; }
        internal GUIStyle PanelTitle { get; }
        internal GUIStyle SearchLabel { get; }
        internal GUIStyle SearchField { get; }
        internal GUIStyle PluginButton { get; }
        internal GUIStyle PluginButtonSelected { get; }
        internal GUIStyle PluginName { get; }
        internal GUIStyle PluginNameSelected { get; }
        internal GUIStyle PluginMeta { get; }
        internal GUIStyle PluginMetaSelected { get; }
        internal GUIStyle InvisibleButton { get; }
        internal GUIStyle PluginTitle { get; }
        internal GUIStyle PluginDetail { get; }
        internal GUIStyle CountPill { get; }
        internal GUIStyle SectionLabel { get; }
        internal GUIStyle SettingName { get; }
        internal GUIStyle SettingDescription { get; }
        internal GUIStyle TypePill { get; }
        internal GUIStyle ValuePill { get; }
        internal GUIStyle DirtyPill { get; }
        internal GUIStyle SavedPill { get; }
        internal GUIStyle ValueField { get; }
        internal GUIStyle RangeEndpoint { get; }
        internal GUIStyle RangeSlider { get; }
        internal GUIStyle RangeSliderThumb { get; }
        internal GUIStyle ActionButton { get; }
        internal GUIStyle SecondaryButton { get; }
        internal GUIStyle SectionButton { get; }
        internal GUIStyle CaptureButton { get; }
        internal GUIStyle ResizeHandle { get; }
        internal GUIStyle EnabledLabel { get; }
        internal GUIStyle DisabledLabel { get; }
        internal GUIStyle EmptyState { get; }
        internal GUIStyle Footer { get; }
        internal GUIStyle FooterText { get; }

        private static GUIStyle Label(int size, FontStyle fontStyle, Color color, TextAnchor alignment = TextAnchor.MiddleLeft)
        {
            return new GUIStyle(GUI.skin.label)
            {
                fontSize = FontPx(size),
                fontStyle = fontStyle,
                alignment = alignment,
                wordWrap = true,
                normal = { textColor = color }
            };
        }

        private static GUIStyle Button(Texture2D normal, Texture2D hover, int fontSize, RectOffset? border = null)
        {
            GUIStyle style = new(GUI.skin.button)
            {
                normal = { background = normal, textColor = new Color(0.98f, 0.96f, 0.88f) },
                hover = { background = hover, textColor = Color.white },
                active = { background = hover, textColor = Color.white },
                focused = { background = normal, textColor = new Color(0.98f, 0.96f, 0.88f) },
                onNormal = { background = normal, textColor = new Color(0.98f, 0.96f, 0.88f) },
                onHover = { background = hover, textColor = Color.white },
                onActive = { background = hover, textColor = Color.white },
                onFocused = { background = normal, textColor = new Color(0.98f, 0.96f, 0.88f) },
                alignment = TextAnchor.MiddleCenter,
                fontSize = FontPx(fontSize),
                fontStyle = FontStyle.Bold,
                wordWrap = true,
                padding = Pad(10, 10, 7, 7)
            };
            ApplyOptionalBorder(style, border);
            return style;
        }

        private static GUIStyle Pill(Texture2D background, Color textColor, RectOffset? border = null)
        {
            GUIStyle style = new(GUI.skin.label)
            {
                normal = { background = background, textColor = textColor },
                alignment = TextAnchor.MiddleCenter,
                fontSize = FontPx(14),
                fontStyle = FontStyle.Bold,
                padding = Pad(8, 8, 3, 3)
            };
            ApplyOptionalBorder(style, border);
            return style;
        }

        private static void ApplyOptionalBorder(GUIStyle style, RectOffset? border)
        {
            if (border != null)
            {
                style.border = border;
            }
        }
    }
}
