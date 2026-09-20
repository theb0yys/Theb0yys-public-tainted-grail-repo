using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Tainted.Abstractions.Runtime;
using UnityEngine;
using UnityEngine.SceneManagement;

#if IL2CPP
using BepInEx.Unity.IL2CPP;
#endif

namespace AvalonExceptions;

#if !IL2CPP
[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
[BepInDependency(TaintedFrameworkGuid, TaintedFrameworkMinimumVersion)]
[BepInDependency(TaintedInterfaceGuid, BepInDependency.DependencyFlags.SoftDependency)]
public sealed class Plugin : BaseUnityPlugin
#else
public sealed class Plugin : MonoBehaviour
#endif
{
    public const string PluginGuid = "tgfoa.the-last-menhir";
    public const string PluginName = "The Last Menhir";
    public const string PluginVersion = "0.3.0";
    public const string TaintedFrameworkGuid = "kane.tgfoa.tainted-framework";
    public const string TaintedFrameworkMinimumVersion = "0.1.38";

    private static readonly string[] LegacyPluginGuids =
    {
        "tgfoa.avalon-exceptions",
        "kane.tgfoa.avalon-exceptions"
    };
    private const string TaintedInterfaceGuid = "kane.tgfoa.tainted-interface";
    private const string IncidentWindowUiOwnerId = PluginGuid + ".incident-window";
    private const int BreadcrumbCapacity = 100;
    private const string GameQuitPatchId = PluginGuid + ".game-quit-clean-shutdown";

    private static Plugin? s_instance;

    private ConfigEntry<bool> _enabled = null!;
    private ConfigEntry<bool> _generateIncidentForUncleanPreviousSession = null!;
    private ConfigEntry<bool> _captureUnityExceptions = null!;
    private ConfigEntry<int> _maxIncidentsPerSession = null!;
    private ConfigEntry<int> _maxLogExcerptLines = null!;
    private ConfigEntry<bool> _includeUnityPlayerLogExcerpt = null!;
    private ConfigEntry<bool> _generateSupportBundle = null!;
    private ConfigEntry<bool> _showIncidentWindow = null!;
    private ConfigEntry<bool> _captureMainThreadStalls = null!;
    private ConfigEntry<int> _mainThreadStallSeconds = null!;
    private ConfigEntry<int> _heartbeatWriteIntervalSeconds = null!;
    private ConfigEntry<bool> _showNativeHangWindow = null!;
    private ConfigEntry<string> _outputFolder = null!;
    private ConfigEntry<bool> _collectModDllHashes = null!;
    private ConfigEntry<bool> _writeSyntheticManagedExceptionOnStartup = null!;
    private ConfigEntry<bool> _forceLocalIncidentWindowUi = null!;

    private readonly Queue<Breadcrumb> _breadcrumbs = new Queue<Breadcrumb>();
    private readonly List<PendingIncident> _pendingIncidents = new List<PendingIncident>();
    private readonly Queue<IncidentWindowSnapshot> _incidentWindows = new Queue<IncidentWindowSnapshot>();
    private readonly HashSet<string> _incidentFingerprints = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    private BepInExLogIncidentListener? _bepInExLogIncidentListener;
    private Harmony? _harmony;
    private ReportRedactor? _redactor;
    private SessionRecord? _session;
    private string _rootFolder = string.Empty;
    private string _sessionStatePath = string.Empty;
    private string _watchdogStatePath = string.Empty;
    private Timer? _watchdogTimer;
    private long _lastMainThreadHeartbeatUtcTicks;
    private long _lastHeartbeatSessionWriteUtcTicks;
    private string _lastActiveScene = string.Empty;
    private PluginRecord[] _lastPluginSnapshot = Array.Empty<PluginRecord>();
    private LocalModPreflightRow[] _lastLocalModPreflightRows = Array.Empty<LocalModPreflightRow>();
    private IncidentWindowSnapshot? _activeIncidentWindow;
    private Vector2 _incidentWindowScroll;
    private int _incidentsWrittenThisSession;
    private bool _reportingAvailable;
    private bool _shutdownMarked;
    private bool _startupBepInExLogScanPending;
    private bool _stallIncidentQueuedThisSession;
    private volatile bool _watchdogStateWrittenForCurrentStall;
    private volatile bool _nativeHangWindowShownForCurrentStall;
    private GUIStyle? _incidentTitleStyle;
    private GUIStyle? _incidentSubtitleStyle;
    private GUIStyle? _incidentSectionStyle;
    private GUIStyle? _incidentBodyStyle;
    private GUIStyle? _incidentMutedStyle;
    private GUIStyle? _incidentButtonStyle;
    private GUIStyle? _incidentPreStyle;
    private GUIStyle? _incidentWindowStyle;
    private Texture2D? _incidentButtonTexture;
    private Texture2D? _incidentButtonHoverTexture;
    private Texture2D? _incidentPreTexture;
    private Color _incidentAccentColor = new Color(0.48f, 0.78f, 1f, 1f);
    private float _incidentStyleScale;
    private bool _incidentStyleUsesTaintedInterface;
    private bool _incidentWindowScopeActive;
    private bool _incidentWindowSharedUiLogged;
    private bool _incidentWindowLocalUiLogged;

#if IL2CPP
    private ConfigFile? _il2CppConfig;
    private ManualLogSource? _il2CppLogger;
    private Il2CppSystem.Func<bool>? _il2CppWantsToQuit;
    private Il2CppSystem.Action? _il2CppQuitting;
    private Application.LogCallback? _il2CppLogCallback;

    public Plugin(IntPtr pointer)
        : base(pointer)
    {
    }

    private ConfigFile Config =>
        _il2CppConfig ?? throw new InvalidOperationException(
            "The Last Menhir config was not initialized.");

    private ManualLogSource Logger =>
        _il2CppLogger ?? throw new InvalidOperationException(
            "The Last Menhir logger was not initialized.");

    internal void Initialize(ConfigFile config, ManualLogSource logger)
    {
        _il2CppConfig = config;
        _il2CppLogger = logger;
        Awake();
    }
#endif

    private void Awake()
    {
        TryMigrateLegacyConfig();
        BindConfig();

        if (!_enabled.Value)
        {
            Logger.LogInfo($"{PluginName} {PluginVersion} disabled by config.");
            return;
        }

        s_instance = this;

        try
        {
            TryMigrateLegacyOutputFolder();
            _rootFolder = ResolveOutputFolder();
            Directory.CreateDirectory(_rootFolder);
            Directory.CreateDirectory(Path.Combine(_rootFolder, "incidents"));
            _sessionStatePath = Path.Combine(_rootFolder, "session-state.json");
            _watchdogStatePath = Path.Combine(_rootFolder, "watchdog-state.json");
            _redactor = CreateRedactor(_rootFolder);
            DateTime initialHeartbeatUtc = DateTime.UtcNow;
            _lastActiveScene = SafeActiveSceneName();
            Volatile.Write(ref _lastMainThreadHeartbeatUtcTicks, initialHeartbeatUtc.Ticks);
            Volatile.Write(ref _lastHeartbeatSessionWriteUtcTicks, initialHeartbeatUtc.Ticks);
            _session = SessionRecord.Create(initialHeartbeatUtc, SafeFrameCount(), _lastActiveScene);
            _reportingAvailable = true;
            _lastLocalModPreflightRows = LocalModPreflight.Build(GetLocalModsPath());
            if (_lastLocalModPreflightRows.Length > 0)
            {
                AddBreadcrumb(
                    "local-mod-preflight",
                    "startup_preflight_completed",
                    $"localMods={_lastLocalModPreflightRows.Length.ToString(CultureInfo.InvariantCulture)}; issues={_lastLocalModPreflightRows.Count(row => row.HasIssue).ToString(CultureInfo.InvariantCulture)}");
            }

            SessionRecord? previousSession = SessionRecord.TryRead(_sessionStatePath);
            WatchdogState? previousWatchdog = WatchdogState.TryRead(_watchdogStatePath);
            AddBreadcrumb("session", "session_started", $"sessionId={_session.SessionId}");

            if (_generateIncidentForUncleanPreviousSession.Value && previousSession != null && !previousSession.CleanShutdown)
            {
                TryQueueIncident(CreatePreviousSessionUncleanIncident(previousSession));
            }

            if (_captureMainThreadStalls.Value
                && previousSession != null
                && !previousSession.CleanShutdown
                && previousWatchdog != null
                && string.Equals(previousWatchdog.SessionId, previousSession.SessionId, StringComparison.OrdinalIgnoreCase))
            {
                TryQueueIncident(CreatePreviousWatchdogIncident(previousWatchdog));
            }

            if (_writeSyntheticManagedExceptionOnStartup.Value)
            {
                TryQueueIncident(CreateSyntheticManagedExceptionIncident());
                AddBreadcrumb("validation", "synthetic_managed_exception_queued", "A disabled-by-default validation incident was queued by config.");
            }

            FlushPendingIncidents();
            WriteSessionState(cleanShutdown: false);

#if IL2CPP
            _il2CppWantsToQuit =
                (Il2CppSystem.Func<bool>)new Func<bool>(OnApplicationWantsToQuit);
            _il2CppQuitting =
                (Il2CppSystem.Action)new Action(OnApplicationQuitting);
            _il2CppLogCallback =
                (Application.LogCallback)new Action<string, string, LogType>(
                    OnUnityLogMessageReceived);
            Application.add_wantsToQuit(_il2CppWantsToQuit);
            Application.add_quitting(_il2CppQuitting);
            Application.add_logMessageReceived(_il2CppLogCallback);
#else
            Application.wantsToQuit += OnApplicationWantsToQuit;
            Application.quitting += OnApplicationQuitting;
            Application.logMessageReceived += OnUnityLogMessageReceived;
#endif
            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
            PatchGameQuitHandlers();
            RegisterBepInExLogListener();
            StartMainThreadWatchdog();
            _startupBepInExLogScanPending = true;
            Logger.LogInfo($"{PluginName} {PluginVersion} loaded. Runtime={RuntimeKind}; Framework=Tainted.Abstractions; OutputFolder={_rootFolder}; CaptureUnityExceptions={_captureUnityExceptions.Value}; BepInExLogExceptionScan=true; ShowIncidentWindow={_showIncidentWindow.Value}; CaptureMainThreadStalls={_captureMainThreadStalls.Value}; MainThreadStallSeconds={_mainThreadStallSeconds.Value}; ShowNativeHangWindow={_showNativeHangWindow.Value}; GenerateSupportBundle={_generateSupportBundle.Value}; SyntheticManagedExceptionOnStartup={_writeSyntheticManagedExceptionOnStartup.Value}; MaxLogExcerptLines={_maxLogExcerptLines.Value}");
        }
        catch (Exception ex)
        {
            _reportingAvailable = false;
            Logger.LogWarning($"{PluginName} reporting disabled for this session. {ex.GetType().Name}: {ex.Message}");
        }
    }

    private static TaintedRuntimeKind RuntimeKind
    {
        get
        {
#if IL2CPP
            return TaintedRuntimeKind.Il2Cpp;
#else
            return TaintedRuntimeKind.Mono;
#endif
        }
    }

    private void Update()
    {
        if (!_reportingAvailable || _session == null || _redactor == null)
        {
            return;
        }

        RecordMainThreadHeartbeat();

        if (_startupBepInExLogScanPending)
        {
            _startupBepInExLogScanPending = false;
            TryQueueStartupBepInExDependencyErrorIncident();
            TryQueueStartupBepInExLogExceptionIncident();
        }

        FlushPendingIncidents();
    }

    private void OnApplicationQuit()
    {
        MarkCleanShutdown();
    }

    private bool OnApplicationWantsToQuit()
    {
        MarkCleanShutdown();
        return true;
    }

    private void OnApplicationQuitting()
    {
        MarkCleanShutdown();
    }

    private void OnProcessExit(object? sender, EventArgs eventArgs)
    {
        MarkCleanShutdown(refreshUnityState: false);
    }

    private void OnDisable()
    {
        MarkCleanShutdown();
    }

    private void OnDestroy()
    {
        MarkCleanShutdown();
        UnpatchGameQuitHandlers();
#if IL2CPP
        if (_il2CppWantsToQuit != null)
        {
            Application.remove_wantsToQuit(_il2CppWantsToQuit);
            _il2CppWantsToQuit = null;
        }

        if (_il2CppQuitting != null)
        {
            Application.remove_quitting(_il2CppQuitting);
            _il2CppQuitting = null;
        }

        if (_il2CppLogCallback != null)
        {
            Application.remove_logMessageReceived(_il2CppLogCallback);
            _il2CppLogCallback = null;
        }
#else
        Application.wantsToQuit -= OnApplicationWantsToQuit;
        Application.quitting -= OnApplicationQuitting;
        Application.logMessageReceived -= OnUnityLogMessageReceived;
#endif
        AppDomain.CurrentDomain.ProcessExit -= OnProcessExit;
        UnregisterBepInExLogListener();
        StopMainThreadWatchdog();
        EndIncidentWindowScope();
        DestroyIncidentWindowTextures();
        if (ReferenceEquals(s_instance, this))
        {
            s_instance = null;
        }
    }

    private void OnGUI()
    {
        if (!_reportingAvailable || _showIncidentWindow == null || !_showIncidentWindow.Value)
        {
            EndIncidentWindowScope();
            return;
        }

        if (_activeIncidentWindow == null && _incidentWindows.Count > 0)
        {
            _activeIncidentWindow = _incidentWindows.Dequeue();
            _incidentWindowScroll = Vector2.zero;
            BeginIncidentWindowScope();
        }

        if (_activeIncidentWindow == null)
        {
            EndIncidentWindowScope();
            return;
        }

        BeginIncidentWindowScope();

        Event currentEvent = Event.current;
        if (currentEvent.type == EventType.KeyDown && currentEvent.keyCode == KeyCode.Escape)
        {
            DismissIncidentWindow();
            currentEvent.Use();
            return;
        }

        DrawIncidentWindow(_activeIncidentWindow);
    }

    private void BindConfig()
    {
        _enabled = Config.Bind("General", "Enabled", true, "Enable The Last Menhir.");
        _generateIncidentForUncleanPreviousSession = Config.Bind("Reports", "GenerateIncidentForUncleanPreviousSession", true, "Write an incident report on startup when the previous session did not mark clean shutdown.");
        _captureUnityExceptions = Config.Bind("Reports", "CaptureUnityExceptions", true, "Capture Unity managed exception events and exception-looking BepInEx Error/Fatal log events as local incidents.");
        _maxIncidentsPerSession = Config.Bind("Reports", "MaxIncidentsPerSession", 3, new ConfigDescription("Maximum live exception incidents to write per session.", new AcceptableValueRange<int>(0, 20)));
        _maxLogExcerptLines = Config.Bind("Reports", "MaxLogExcerptLines", 200, new ConfigDescription("Maximum lines copied from each source log into an incident.", new AcceptableValueRange<int>(0, 2000)));
        _includeUnityPlayerLogExcerpt = Config.Bind("Reports", "IncludeUnityPlayerLogExcerpt", true, "Include a bounded sanitized Unity Player log excerpt when the path is available.");
        _generateSupportBundle = Config.Bind("Reports", "GenerateSupportBundle", true, "Create a local support_bundle.zip from the fixed allowed incident files after redaction safety checks pass.");
        _showIncidentWindow = Config.Bind("Reports", "ShowIncidentWindow", true, "Show an in-game incident review window when The Last Menhir writes an incident and the Unity UI thread is responsive.");
        _captureMainThreadStalls = Config.Bind("Watchdog", "CaptureMainThreadStalls", true, "Capture recovered Unity main-thread stalls and previous-session watchdog evidence as possible hang/freeze incidents. This can help with frozen or infinite loading screens, but it does not prove loading-screen state.");
        _mainThreadStallSeconds = Config.Bind("Watchdog", "MainThreadStallSeconds", 60, new ConfigDescription("Seconds without a Unity Update heartbeat before The Last Menhir records possible hang/freeze evidence.", new AcceptableValueRange<int>(10, 600)));
        _heartbeatWriteIntervalSeconds = Config.Bind("Watchdog", "HeartbeatWriteIntervalSeconds", 10, new ConfigDescription("Minimum seconds between session-state heartbeat writes while the game is running.", new AcceptableValueRange<int>(1, 120)));
        _showNativeHangWindow = Config.Bind("Watchdog", "ShowNativeHangWindow", false, "Show a small native Windows warning if the watchdog sees the Unity main thread stop advancing. Full details are written after recovery or on next launch.");
        _outputFolder = Config.Bind("Reports", "OutputFolder", string.Empty, "Folder where The Last Menhir reports are written. Values outside BepInEx/config are ignored; empty values use this plugin's default config folder.");
        _collectModDllHashes = Config.Bind("Privacy", "CollectModDllHashes", false, "Compute SHA-256 hashes for loaded plugin DLLs. Disabled by default to minimize installed-file reads.");
        _writeSyntheticManagedExceptionOnStartup = Config.Bind("Validation", "WriteSyntheticManagedExceptionOnStartup", false, "Write one controlled synthetic managed-exception incident on startup for validation. This does not throw, crash, patch gameplay, or upload data.");
        _forceLocalIncidentWindowUi = Config.Bind("Validation", "ForceLocalIncidentWindowUi", false, "Force the local IMGUI incident-window fallback even when Tainted Interface is available. Intended only for release validation.");
    }

    private void TryMigrateLegacyConfig()
    {
        string currentPath = Path.Combine(Paths.ConfigPath, PluginGuid + ".cfg");
        if (File.Exists(currentPath))
        {
            return;
        }

        foreach (string legacyPluginGuid in LegacyPluginGuids)
        {
            string legacyPath = Path.Combine(Paths.ConfigPath, legacyPluginGuid + ".cfg");
            if (!File.Exists(legacyPath))
            {
                continue;
            }

            try
            {
                File.Move(legacyPath, currentPath);
                Config.Reload();
                Logger.LogInfo($"{PluginName} migrated the legacy config from {legacyPluginGuid}.cfg to {PluginGuid}.cfg.");
                return;
            }
            catch (Exception ex)
            {
                Logger.LogWarning($"{PluginName} could not migrate the legacy config from {legacyPluginGuid}.cfg. Defaults will be used unless {PluginGuid}.cfg already exists. The legacy config was left in place when possible. {ex.GetType().Name}: {ex.Message}");
            }
        }
    }

    private void TryMigrateLegacyOutputFolder()
    {
        if (!string.IsNullOrWhiteSpace(_outputFolder.Value))
        {
            return;
        }

        string currentFolder = Path.Combine(Paths.ConfigPath, PluginGuid);
        if (Directory.Exists(currentFolder))
        {
            return;
        }

        foreach (string legacyPluginGuid in LegacyPluginGuids)
        {
            string legacyFolder = Path.Combine(Paths.ConfigPath, legacyPluginGuid);
            if (!Directory.Exists(legacyFolder))
            {
                continue;
            }

            try
            {
                Directory.Move(legacyFolder, currentFolder);
                Logger.LogInfo($"{PluginName} migrated the legacy report folder from {legacyPluginGuid} to {PluginGuid}.");
                return;
            }
            catch (Exception ex)
            {
                Logger.LogWarning($"{PluginName} could not migrate the legacy report folder from {legacyPluginGuid}. Existing reports remain at the old location. {ex.GetType().Name}: {ex.Message}");
            }
        }
    }

    private void DrawIncidentWindow(IncidentWindowSnapshot snapshot)
    {
        float scale = IncidentUiScale();
        EnsureIncidentWindowStyles(scale);
        Rect screen = new Rect(0f, 0f, Screen.width, Screen.height);
        DrawSolidRect(screen, new Color(0f, 0f, 0f, 0.66f));

        float margin = 48f * scale;
        float width = Mathf.Clamp(Screen.width * 0.72f, 760f * scale, Screen.width - (margin * 2f));
        float height = Mathf.Clamp(Screen.height * 0.82f, 560f * scale, Screen.height - (margin * 2f));
        Rect panel = new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);
        if (_incidentWindowStyle == null)
        {
            DrawSolidRect(panel, new Color(0.07f, 0.08f, 0.09f, 0.98f));
        }
        else
        {
            GUI.Box(panel, GUIContent.none, _incidentWindowStyle);
        }

        DrawSolidRect(new Rect(panel.x, panel.y, panel.width, 5f * scale), _incidentAccentColor);

        float paddingX = 32f * scale;
        float paddingTop = 26f * scale;
        float paddingBottom = 24f * scale;
        GUILayout.BeginArea(new Rect(panel.x + paddingX, panel.y + paddingTop, panel.width - (paddingX * 2f), panel.height - paddingTop - paddingBottom));
        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();
        GUILayout.Label(PluginName, _incidentTitleStyle!);
        GUILayout.Label(snapshot.Title, _incidentSubtitleStyle!);
        GUILayout.EndVertical();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Dismiss", _incidentButtonStyle!, GUILayout.Width(132f * scale), GUILayout.Height(42f * scale)))
        {
            DismissIncidentWindow();
        }

        GUILayout.EndHorizontal();
        GUILayout.Space(18f * scale);

        _incidentWindowScroll = GUILayout.BeginScrollView(_incidentWindowScroll);
        GUILayout.Label("What happened", _incidentSectionStyle!);
        DrawIncidentRow("Incident", $"{snapshot.Kind} / {snapshot.Severity}");
        DrawIncidentRow("Route", $"{snapshot.RouteLabel} ({snapshot.RouteConfidence})");
        DrawIncidentRow("Likely cause", $"{snapshot.LikelyCause} ({snapshot.Confidence})");
        DrawIncidentRow("Mod suspect", snapshot.ModSuspectStatus);
        DrawIncidentRow("Game", $"{snapshot.GameVersion} / Unity {snapshot.UnityVersion}");
        DrawIncidentRow("Report folder", snapshot.ReportFolder);
        DrawIncidentRow("Support bundle", string.IsNullOrWhiteSpace(snapshot.SupportBundlePath) ? "not created or unavailable" : snapshot.SupportBundlePath);
        GUILayout.Space(14f * scale);

        GUILayout.Label("Recommended action", _incidentSectionStyle!);
        GUILayout.Label(snapshot.RecommendedAction, _incidentBodyStyle!);
        GUILayout.Space(14f * scale);

        GUILayout.Label("What this does not prove", _incidentSectionStyle!);
        GUILayout.Label(snapshot.DoesNotProve, _incidentMutedStyle!);
        GUILayout.Space(14f * scale);

        GUILayout.Label("Best next check", _incidentSectionStyle!);
        GUILayout.Label(snapshot.BestNextCheck, _incidentBodyStyle!);
        GUILayout.Space(14f * scale);

        GUILayout.Label("Call stack or freeze evidence", _incidentSectionStyle!);
        GUILayout.TextArea(string.IsNullOrWhiteSpace(snapshot.ManagedStackPreview) ? "No managed call stack was captured for this incident." : snapshot.ManagedStackPreview, _incidentPreStyle!, GUILayout.MinHeight(220f * scale));
        GUILayout.Space(14f * scale);

        GUILayout.Label($"Loaded mods ({snapshot.LoadedPluginCount})", _incidentSectionStyle!);
        GUILayout.TextArea(snapshot.LoadedPluginsPreview, _incidentPreStyle!, GUILayout.MinHeight(180f * scale));
        GUILayout.Space(14f * scale);

        GUILayout.Label("Mod-owner summary", _incidentSectionStyle!);
        GUILayout.TextArea(snapshot.OwnerSummary, _incidentPreStyle!, GUILayout.MinHeight(220f * scale));
        GUILayout.Space(14f * scale);

        GUILayout.Label("Evidence boundary", _incidentSectionStyle!);
        GUILayout.Label(snapshot.EvidenceBoundary, _incidentMutedStyle!);
        GUILayout.EndScrollView();

        GUILayout.Space(16f * scale);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Copy Summary", _incidentButtonStyle!, GUILayout.Height(44f * scale)))
        {
            GUIUtility.systemCopyBuffer = snapshot.OwnerSummary;
        }

        if (GUILayout.Button("Copy Report Path", _incidentButtonStyle!, GUILayout.Height(44f * scale)))
        {
            GUIUtility.systemCopyBuffer = snapshot.ReportFolder;
        }

        if (!string.IsNullOrWhiteSpace(snapshot.SupportBundlePath)
            && GUILayout.Button("Copy Bundle Path", _incidentButtonStyle!, GUILayout.Height(44f * scale)))
        {
            GUIUtility.systemCopyBuffer = snapshot.SupportBundlePath;
        }

        if (_incidentWindows.Count > 0 && GUILayout.Button($"Next ({_incidentWindows.Count})", _incidentButtonStyle!, GUILayout.Height(44f * scale)))
        {
            _activeIncidentWindow = _incidentWindows.Dequeue();
            _incidentWindowScroll = Vector2.zero;
        }

        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }

    private void DrawIncidentRow(string label, string value)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(label, _incidentMutedStyle!, GUILayout.Width(190f * _incidentStyleScale));
        GUILayout.Label(string.IsNullOrWhiteSpace(value) ? "unavailable" : value, _incidentBodyStyle!);
        GUILayout.EndHorizontal();
    }

    private void DismissIncidentWindow()
    {
        _activeIncidentWindow = null;
        _incidentWindowScroll = Vector2.zero;
        EndIncidentWindowScope();
    }

    private void BeginIncidentWindowScope()
    {
        if (_forceLocalIncidentWindowUi.Value)
        {
            EndIncidentWindowScope();
            return;
        }

        if (_incidentWindowScopeActive)
        {
            TaintedInterfaceBridge.EnsureInteractiveCursor();
            return;
        }

        if (TaintedInterfaceBridge.BeginCustomUiScope(IncidentWindowUiOwnerId, freezeWorld: false))
        {
            _incidentWindowScopeActive = true;
            AddBreadcrumb("ui", "tainted_interface_scope_acquired", "incident-window");
        }
    }

    private void EndIncidentWindowScope()
    {
        if (!_incidentWindowScopeActive)
        {
            return;
        }

        TaintedInterfaceBridge.EndCustomUiScope(IncidentWindowUiOwnerId);
        _incidentWindowScopeActive = false;
    }

    private void EnsureIncidentWindowStyles(float scale)
    {
        TaintedInterfaceBridge.SharedStyles? sharedStyles = null;
        bool useSharedStyles = !_forceLocalIncidentWindowUi.Value
            && TaintedInterfaceBridge.TryGetStyles(out sharedStyles)
            && sharedStyles != null;
        if (_incidentTitleStyle != null
            && Mathf.Abs(_incidentStyleScale - scale) < 0.01f
            && _incidentStyleUsesTaintedInterface == useSharedStyles)
        {
            return;
        }

        DestroyIncidentWindowTextures();
        _incidentStyleScale = scale;
        _incidentStyleUsesTaintedInterface = useSharedStyles;
        _incidentAccentColor = useSharedStyles
            ? sharedStyles!.Title.normal.textColor
            : new Color(0.48f, 0.78f, 1f, 1f);

        if (useSharedStyles)
        {
            EnsureSharedIncidentWindowStyles(scale, sharedStyles!);
            if (!_incidentWindowSharedUiLogged)
            {
                _incidentWindowSharedUiLogged = true;
                Logger.LogInfo($"{PluginName} incident review window using Tainted Interface shared UI styles.");
            }

            return;
        }

        _incidentButtonTexture = CreateSolidTexture(new Color(0.12f, 0.28f, 0.36f, 1f));
        _incidentButtonHoverTexture = CreateSolidTexture(new Color(0.17f, 0.39f, 0.5f, 1f));
        _incidentPreTexture = CreateSolidTexture(new Color(0.04f, 0.05f, 0.06f, 0.96f));
        _incidentWindowStyle = null;

        _incidentTitleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = ScaledFont(34, scale),
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(0.92f, 0.96f, 1f, 1f) }
        };
        _incidentSubtitleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = ScaledFont(18, scale),
            wordWrap = true,
            normal = { textColor = new Color(0.76f, 0.84f, 0.9f, 1f) }
        };
        _incidentSectionStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = ScaledFont(20, scale),
            fontStyle = FontStyle.Bold,
            margin = new RectOffset(0, 0, Mathf.RoundToInt(10f * scale), Mathf.RoundToInt(6f * scale)),
            normal = { textColor = new Color(0.5f, 0.82f, 1f, 1f) }
        };
        _incidentBodyStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = ScaledFont(17, scale),
            wordWrap = true,
            normal = { textColor = new Color(0.9f, 0.93f, 0.96f, 1f) }
        };
        _incidentMutedStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = ScaledFont(16, scale),
            wordWrap = true,
            normal = { textColor = new Color(0.66f, 0.71f, 0.77f, 1f) }
        };
        _incidentButtonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = ScaledFont(16, scale),
            fontStyle = FontStyle.Bold,
            padding = new RectOffset(
                Mathf.RoundToInt(16f * scale),
                Mathf.RoundToInt(16f * scale),
                Mathf.RoundToInt(8f * scale),
                Mathf.RoundToInt(8f * scale)),
            normal = { textColor = Color.white, background = _incidentButtonTexture! },
            hover = { textColor = Color.white, background = _incidentButtonHoverTexture! },
            active = { textColor = Color.white, background = _incidentButtonHoverTexture! }
        };
        _incidentPreStyle = new GUIStyle(GUI.skin.textArea)
        {
            fontSize = ScaledFont(15, scale),
            wordWrap = true,
            normal = { textColor = new Color(0.9f, 0.94f, 0.97f, 1f), background = _incidentPreTexture! },
            focused = { textColor = new Color(0.9f, 0.94f, 0.97f, 1f), background = _incidentPreTexture! },
            hover = { textColor = new Color(0.9f, 0.94f, 0.97f, 1f), background = _incidentPreTexture! }
        };

        if (!_incidentWindowLocalUiLogged)
        {
            _incidentWindowLocalUiLogged = true;
            Logger.LogInfo($"{PluginName} incident review window using local fallback UI styles.");
        }
    }

    private void EnsureSharedIncidentWindowStyles(float scale, TaintedInterfaceBridge.SharedStyles sharedStyles)
    {
        _incidentWindowStyle = new GUIStyle(sharedStyles.Window)
        {
            padding = new RectOffset(
                Mathf.RoundToInt(18f * scale),
                Mathf.RoundToInt(18f * scale),
                Mathf.RoundToInt(18f * scale),
                Mathf.RoundToInt(18f * scale))
        };
        _incidentTitleStyle = ScaleStyle(sharedStyles.Title, 34, scale, wordWrap: false);
        _incidentSubtitleStyle = ScaleStyle(sharedStyles.MutedLabel, 18, scale, wordWrap: true);
        _incidentSectionStyle = ScaleStyle(sharedStyles.Title, 20, scale, wordWrap: true);
        _incidentSectionStyle.margin = new RectOffset(0, 0, Mathf.RoundToInt(10f * scale), Mathf.RoundToInt(6f * scale));
        _incidentBodyStyle = ScaleStyle(sharedStyles.Label, 17, scale, wordWrap: true);
        _incidentMutedStyle = ScaleStyle(sharedStyles.MutedLabel, 16, scale, wordWrap: true);
        _incidentButtonStyle = ScaleStyle(sharedStyles.Button, 16, scale, wordWrap: false);
        _incidentButtonStyle.fontStyle = FontStyle.Bold;
        _incidentButtonStyle.padding = new RectOffset(
            Mathf.RoundToInt(16f * scale),
            Mathf.RoundToInt(16f * scale),
            Mathf.RoundToInt(8f * scale),
            Mathf.RoundToInt(8f * scale));
        _incidentPreStyle = ScaleStyle(sharedStyles.Field, 15, scale, wordWrap: true);
        _incidentPreStyle.normal.textColor = new Color(0.9f, 0.94f, 0.97f, 1f);
        _incidentPreStyle.focused.textColor = new Color(0.9f, 0.94f, 0.97f, 1f);
        _incidentPreStyle.hover.textColor = new Color(0.9f, 0.94f, 0.97f, 1f);
    }

    private static GUIStyle ScaleStyle(GUIStyle source, int baseSize, float scale, bool wordWrap)
    {
        return new GUIStyle(source)
        {
            fontSize = ScaledFont(baseSize, scale),
            wordWrap = wordWrap
        };
    }

    private static float IncidentUiScale()
    {
        return Mathf.Clamp(Screen.height / 1080f, 1f, 1.45f);
    }

    private static int ScaledFont(int baseSize, float scale)
    {
        return Mathf.RoundToInt(baseSize * scale);
    }

    private static void DrawSolidRect(Rect rect, Color color)
    {
        Color previous = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = previous;
    }

    private static Texture2D CreateSolidTexture(Color color)
    {
        Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, mipChain: false)
        {
            hideFlags = HideFlags.HideAndDontSave
        };
        texture.SetPixel(0, 0, color);
        texture.Apply(updateMipmaps: false, makeNoLongerReadable: true);
        return texture;
    }

    private void DestroyIncidentWindowTextures()
    {
        DestroyTexture(ref _incidentButtonTexture);
        DestroyTexture(ref _incidentButtonHoverTexture);
        DestroyTexture(ref _incidentPreTexture);
    }

    private static void DestroyTexture(ref Texture2D? texture)
    {
        if (texture == null)
        {
            return;
        }

        UnityEngine.Object.Destroy(texture);
        texture = null;
    }

    private string ResolveOutputFolder()
    {
        string defaultFolder = Path.Combine(Paths.ConfigPath, PluginGuid);
        if (!string.IsNullOrWhiteSpace(_outputFolder.Value))
        {
            try
            {
                string candidate = Path.GetFullPath(_outputFolder.Value);
                string configRoot = Path.GetFullPath(Paths.ConfigPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
                if (candidate.StartsWith(configRoot, StringComparison.OrdinalIgnoreCase))
                {
                    return candidate;
                }

                Logger.LogWarning($"{PluginName} ignored Reports.OutputFolder because it is outside BepInEx/config. Using default output folder.");
            }
            catch (Exception ex)
            {
                Logger.LogWarning($"{PluginName} ignored invalid Reports.OutputFolder. {ex.GetType().Name}: {ex.Message}");
            }
        }

        return defaultFolder;
    }

    private static ReportRedactor CreateRedactor(string outputRoot)
    {
        return ReportRedactor.Create(new[]
        {
            (Paths.ConfigPath, "<config>"),
            (Paths.PluginPath, "<plugins>"),
            (outputRoot, "<the-last-menhir>"),
            (Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "<user-profile>"),
            (AppDomain.CurrentDomain.BaseDirectory, "<game-root>")
        });
    }

    private void RecordMainThreadHeartbeat()
    {
        if (!_captureMainThreadStalls.Value || _session == null)
        {
            return;
        }

        DateTime nowUtc = DateTime.UtcNow;
        long previousTicks = Volatile.Read(ref _lastMainThreadHeartbeatUtcTicks);
        DateTime? previousHeartbeatUtc = previousTicks > 0
            ? new DateTime(previousTicks, DateTimeKind.Utc)
            : null;

        string activeScene = SafeActiveSceneName();
        _lastActiveScene = activeScene;
        Volatile.Write(ref _lastMainThreadHeartbeatUtcTicks, nowUtc.Ticks);
        UpdateSessionHeartbeat(nowUtc, activeScene);
        if (_lastPluginSnapshot.Length == 0)
        {
            _lastPluginSnapshot = BuildPluginInventory(includeHash: false);
        }

        int thresholdSeconds = MainThreadStallThresholdSeconds();
        if (previousHeartbeatUtc.HasValue)
        {
            double elapsedSeconds = (nowUtc - previousHeartbeatUtc.Value).TotalSeconds;
            if (!_stallIncidentQueuedThisSession && elapsedSeconds >= thresholdSeconds)
            {
                _stallIncidentQueuedThisSession = true;
                _watchdogStateWrittenForCurrentStall = false;
                TryQueueIncident(CreateRecoveredStallIncident(previousHeartbeatUtc.Value, nowUtc, elapsedSeconds, activeScene));
            }
            else if (elapsedSeconds < thresholdSeconds)
            {
                _watchdogStateWrittenForCurrentStall = false;
                _nativeHangWindowShownForCurrentStall = false;
            }
        }

        long lastWriteTicks = Volatile.Read(ref _lastHeartbeatSessionWriteUtcTicks);
        DateTime lastWriteUtc = lastWriteTicks > 0
            ? new DateTime(lastWriteTicks, DateTimeKind.Utc)
            : DateTime.MinValue;
        if ((nowUtc - lastWriteUtc).TotalSeconds >= HeartbeatWriteIntervalSeconds())
        {
            Volatile.Write(ref _lastHeartbeatSessionWriteUtcTicks, nowUtc.Ticks);
            WriteSessionState(cleanShutdown: false);
        }
    }

    private void UpdateSessionHeartbeat(DateTime heartbeatUtc, string activeScene)
    {
        if (_session == null)
        {
            return;
        }

        _session = _session.WithHeartbeat(heartbeatUtc, SafeFrameCount(), activeScene);
    }

    private void StartMainThreadWatchdog()
    {
        if (!_captureMainThreadStalls.Value || _session == null)
        {
            return;
        }

        int thresholdSeconds = MainThreadStallThresholdSeconds();
        int periodSeconds = Math.Max(2, Math.Min(10, thresholdSeconds / 3));
        _watchdogTimer = new Timer(CheckMainThreadWatchdog, null, TimeSpan.FromSeconds(periodSeconds), TimeSpan.FromSeconds(periodSeconds));
    }

    private void StopMainThreadWatchdog()
    {
        Timer? timer = Interlocked.Exchange(ref _watchdogTimer, null);
        timer?.Dispose();
    }

    private void CheckMainThreadWatchdog(object? state)
    {
        if (_shutdownMarked || !_reportingAvailable || _session == null || string.IsNullOrWhiteSpace(_watchdogStatePath))
        {
            return;
        }

        long lastHeartbeatTicks = Volatile.Read(ref _lastMainThreadHeartbeatUtcTicks);
        if (lastHeartbeatTicks <= 0)
        {
            return;
        }

        DateTime nowUtc = DateTime.UtcNow;
        DateTime lastHeartbeatUtc = new DateTime(lastHeartbeatTicks, DateTimeKind.Utc);
        double staleSeconds = (nowUtc - lastHeartbeatUtc).TotalSeconds;
        if (staleSeconds < MainThreadStallThresholdSeconds())
        {
            _watchdogStateWrittenForCurrentStall = false;
            return;
        }

        if (_watchdogStateWrittenForCurrentStall)
        {
            return;
        }

        _watchdogStateWrittenForCurrentStall = true;
        WatchdogState stateRecord = new WatchdogState(
            _session.SessionId,
            _session.StartedAtUtc,
            lastHeartbeatUtc,
            nowUtc,
            (int)Math.Round(staleSeconds),
            _lastActiveScene,
            "Unity Update heartbeat stale; possible main-thread freeze or infinite loading screen.");
        stateRecord.TryWrite(_watchdogStatePath);
        TryShowNativeHangWindow(stateRecord);
    }

    private void TryShowNativeHangWindow(WatchdogState stateRecord)
    {
        if (_showNativeHangWindow == null || !_showNativeHangWindow.Value || _nativeHangWindowShownForCurrentStall)
        {
            return;
        }

        _nativeHangWindowShownForCurrentStall = true;
        try
        {
            NativeMessageBox.Show(
                $"{PluginName} - possible freeze",
                BuildNativeHangWindowText(stateRecord));
        }
        catch
        {
            // A native notice is optional; watchdog evidence still stays on disk.
        }
    }

    private string BuildNativeHangWindowText(WatchdogState stateRecord)
    {
        PluginRecord[] plugins = _lastPluginSnapshot;
        string pluginPreview = plugins.Length == 0
            ? "Plugin inventory was not captured yet."
            : string.Join(Environment.NewLine, plugins
                .OrderBy(plugin => plugin.Name, StringComparer.OrdinalIgnoreCase)
                .Take(12)
                .Select(plugin => $"- {plugin.Name} {plugin.Version} ({plugin.Guid})"));

        if (plugins.Length > 12)
        {
            pluginPreview += Environment.NewLine + $"- ...and {plugins.Length - 12} more loaded plugins.";
        }

        return string.Join(Environment.NewLine, new[]
        {
            "The Last Menhir detected that the Unity main thread stopped advancing.",
            "",
            $"Stale heartbeat: about {stateRecord.StaleSeconds.ToString(CultureInfo.InvariantCulture)} seconds",
            $"Last active scene: {stateRecord.ActiveScene}",
            $"Loaded plugins: {plugins.Length.ToString(CultureInfo.InvariantCulture)}",
            "",
            "Likely cause: unknown. A main-thread freeze does not produce a managed exception call stack.",
            "If this is an infinite loading screen or unrecoverable session, force-close the game and launch it again.",
            "The Last Menhir will open a full incident review window on next launch if the session marker is still present.",
            "",
            "Loaded mod preview:",
            pluginPreview,
            "",
            "Report root:",
            _rootFolder
        });
    }

    private int MainThreadStallThresholdSeconds()
    {
        return Math.Max(10, _mainThreadStallSeconds.Value);
    }

    private int HeartbeatWriteIntervalSeconds()
    {
        return Math.Max(1, _heartbeatWriteIntervalSeconds.Value);
    }

    private PendingIncident CreateRecoveredStallIncident(DateTime lastHeartbeatUtc, DateTime recoveredAtUtc, double elapsedSeconds, string activeScene)
    {
        string roundedSeconds = Math.Round(elapsedSeconds).ToString(CultureInfo.InvariantCulture);
        string evidence = string.Join(Environment.NewLine, new[]
        {
            $"lastMainThreadHeartbeatUtc={lastHeartbeatUtc:o}",
            $"recoveredAtUtc={recoveredAtUtc:o}",
            $"elapsedSeconds={roundedSeconds}",
            $"activeScene={activeScene}",
            $"frameCount={SafeFrameCount().ToString(CultureInfo.InvariantCulture)}",
            "boundary=No game-specific loading-screen hook is active; this is heartbeat-stall evidence only."
        });

        return new PendingIncident(
            "possible_hang_or_freeze",
            "warning",
            "Possible freeze or infinite loading screen recovered.",
            $"Unity Update did not advance for about {roundedSeconds} seconds and then recovered. If the player was on a loading screen, treat this as possible infinite-loading evidence; The Last Menhir cannot prove loading-screen state from v0.2.0 evidence alone.",
            evidence);
    }

    private static PendingIncident CreatePreviousWatchdogIncident(WatchdogState watchdog)
    {
        string evidence = string.Join(Environment.NewLine, new[]
        {
            $"previousSessionId={watchdog.SessionId}",
            $"previousSessionStartedAtUtc={watchdog.SessionStartedAtUtc:o}",
            $"lastMainThreadHeartbeatUtc={watchdog.LastHeartbeatAtUtc:o}",
            $"watchdogDetectedAtUtc={watchdog.DetectedAtUtc:o}",
            $"staleSeconds={watchdog.StaleSeconds.ToString(CultureInfo.InvariantCulture)}",
            $"activeScene={watchdog.ActiveScene}",
            $"watchdogReason={watchdog.Reason}",
            "boundary=No native crash dump or game-specific loading-screen hook was collected."
        });

        return new PendingIncident(
            "possible_hang_or_freeze",
            "warning",
            "Possible freeze or infinite loading screen before forced exit.",
            $"The previous session did not shut down cleanly after The Last Menhir recorded a stale Unity Update heartbeat for about {watchdog.StaleSeconds.ToString(CultureInfo.InvariantCulture)} seconds. If the player force-quit from a loading screen, treat this as possible infinite-loading evidence; it does not prove a native crash.",
            evidence,
            PreviousSessionEvidence.From(watchdog));
    }

    private static PendingIncident CreatePreviousSessionUncleanIncident(SessionRecord previousSession)
    {
        string evidence = string.Join(Environment.NewLine, new[]
        {
            $"previousSessionId={previousSession.SessionId}",
            $"previousSessionStartedAtUtc={previousSession.StartedAtUtc:o}",
            $"previousSessionLastHeartbeatAtUtc={previousSession.LastHeartbeatAtUtc?.ToString("o", CultureInfo.InvariantCulture) ?? "unavailable"}",
            $"previousSessionLastHeartbeatFrame={previousSession.LastHeartbeatFrame.ToString(CultureInfo.InvariantCulture)}",
            $"previousSessionLastActiveScene={previousSession.LastActiveScene}",
            "boundary=The previous session marker was not clean. This is hard-crash or force-close evidence, not native crash capture."
        });

        return new PendingIncident(
            "previous_session_unclean_exit",
            "warning",
            "Previous session did not mark clean shutdown.",
            $"Previous session {previousSession.SessionId} started at {previousSession.StartedAtUtc:o} did not write a clean shutdown marker. This can happen after a hard crash, force-close, or process termination before The Last Menhir can shut down cleanly.",
            evidence,
            PreviousSessionEvidence.From(previousSession));
    }

    private void TryDeleteCurrentWatchdogState()
    {
        try
        {
            WatchdogState? state = WatchdogState.TryRead(_watchdogStatePath);
            if (state == null || _session == null || !string.Equals(state.SessionId, _session.SessionId, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            File.Delete(_watchdogStatePath);
        }
        catch
        {
            // Best-effort cleanup only; stale markers are session-id checked on next launch.
        }
    }

    private static int SafeFrameCount()
    {
        try
        {
            return Time.frameCount;
        }
        catch
        {
            return 0;
        }
    }

    private static string SafeActiveSceneName()
    {
        try
        {
            Scene scene = SceneManager.GetActiveScene();
            return string.IsNullOrWhiteSpace(scene.name) ? "(unnamed)" : scene.name;
        }
        catch
        {
            return string.Empty;
        }
    }

    private void OnUnityLogMessageReceived(string condition, string stackTrace, LogType type)
    {
        if (!_reportingAvailable || !_captureUnityExceptions.Value)
        {
            return;
        }

        if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
        {
            AddBreadcrumb("unity-log", type.ToString(), condition);
        }

        if (!ShouldCaptureUnityLogAsManagedIncident(condition, stackTrace, type))
        {
            return;
        }

        string title = FirstNonEmptyLine(condition);
        string stack = BuildManagedStack(condition, stackTrace);
        TryQueueIncident(new PendingIncident(
            "managed_exception",
            "error",
            string.IsNullOrWhiteSpace(title) ? "Unity managed exception." : title,
            string.IsNullOrWhiteSpace(condition) ? "Unity managed exception event captured." : condition,
            stack));
    }

    private static bool ShouldCaptureUnityLogAsManagedIncident(string condition, string stackTrace, LogType type)
    {
        if (type == LogType.Exception)
        {
            return true;
        }

        if (type != LogType.Error && type != LogType.Assert)
        {
            return false;
        }

        return ExceptionLogDetector.LooksLikeManagedException(condition)
            || ExceptionLogDetector.LooksLikeManagedException(stackTrace);
    }

    private void RegisterBepInExLogListener()
    {
        if (_bepInExLogIncidentListener != null)
        {
            return;
        }

        _bepInExLogIncidentListener = new BepInExLogIncidentListener(this);
        BepInEx.Logging.Logger.Listeners.Add(_bepInExLogIncidentListener);
    }

    private void UnregisterBepInExLogListener()
    {
        if (_bepInExLogIncidentListener == null)
        {
            return;
        }

        BepInEx.Logging.Logger.Listeners.Remove(_bepInExLogIncidentListener);
        _bepInExLogIncidentListener.Dispose();
        _bepInExLogIncidentListener = null;
    }

    private void OnBepInExLogEvent(LogEventArgs eventArgs)
    {
        if (!_reportingAvailable || !_captureUnityExceptions.Value || !IsErrorOrFatal(eventArgs.Level))
        {
            return;
        }

        string sourceName = eventArgs.Source?.SourceName ?? string.Empty;
        if (string.Equals(sourceName, PluginName, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        string data = eventArgs.Data?.ToString() ?? string.Empty;
        if (!ExceptionLogDetector.LooksLikeManagedException(data))
        {
            return;
        }

        string title = FirstNonEmptyLine(data);
        AddBreadcrumb("bepinex-log", eventArgs.Level.ToString(), $"{sourceName}: {title}");
        TryQueueIncident(new PendingIncident(
            "managed_exception",
            "error",
            string.IsNullOrWhiteSpace(title) ? "BepInEx managed exception log event." : title,
            string.IsNullOrWhiteSpace(sourceName)
                ? "BepInEx Error/Fatal log event contained managed-exception evidence."
                : $"BepInEx Error/Fatal log event from {sourceName} contained managed-exception evidence.",
            data));
    }

    private void TryQueueStartupBepInExLogExceptionIncident()
    {
        if (!_captureUnityExceptions.Value || _redactor == null)
        {
            return;
        }

        LogExcerpt excerpt = ReadLogExcerpt(GetBepInExLogPath(), _maxLogExcerptLines.Value);
        if (!excerpt.Available || !ExceptionLogDetector.TryExtractFirstExceptionBlock(excerpt.Lines, out string title, out string stackTrace))
        {
            return;
        }

        AddBreadcrumb("bepinex-log", "startup_exception_scan", title);
        TryQueueIncident(new PendingIncident(
            "managed_exception",
            "error",
            string.IsNullOrWhiteSpace(title) ? "BepInEx managed exception log evidence." : title,
            "Exception-looking BepInEx log evidence was found during the bounded startup scan.",
            stackTrace));
    }

    private void TryQueueStartupBepInExDependencyErrorIncident()
    {
        if (!_captureUnityExceptions.Value)
        {
            return;
        }

        BepInExDependencyErrorFinding[] findings = BepInExDependencyErrorProbe.Build();
        if (findings.Length == 0)
        {
            return;
        }

        string summary = findings.Length == 1
            ? "BepInEx loader dependency error."
            : $"BepInEx reported {findings.Length.ToString(CultureInfo.InvariantCulture)} loader dependency errors.";
        string message = findings.Length == 1
            ? findings[0].Describe()
            : $"BepInEx reported {findings.Length.ToString(CultureInfo.InvariantCulture)} plugin loader dependency errors. First: {findings[0].Describe()}";
        string stackTrace = string.Join(Environment.NewLine, findings
            .Select(finding => finding.Describe())
            .Distinct(StringComparer.OrdinalIgnoreCase));

        AddBreadcrumb("bepinex-log", "startup_dependency_error_scan", message);
        TryQueueIncident(new PendingIncident(
            "bepinex_loader_dependency_error",
            "warning",
            summary,
            message,
            stackTrace));
    }

    private static bool IsErrorOrFatal(LogLevel level)
    {
        return (level & (LogLevel.Error | LogLevel.Fatal)) != 0;
    }

    private bool TryQueueIncident(PendingIncident incident)
    {
        IncidentQueueEntry incomingEntry = ToIncidentQueueEntry(incident);
        string incidentText = incomingEntry.Text;
        string groupFingerprint = IncidentFingerprint.BuildGroup(incident.Kind, incident.Title, incident.Message, incident.StackTrace);
        bool incomingAddressablesInvalidPath = AddressablesInvalidPathEvidence.TryExtract(incidentText, out _);
        bool bypassManagedIncidentLimit = IncidentQueueGrouping.ShouldBypassManagedIncidentLimit(incomingEntry);
        bool incomingAddressablesWrapper = !incomingAddressablesInvalidPath
            && AddressablesInvalidPathEvidence.LooksLikeCascadeWrapper(incidentText);

        if (incomingAddressablesInvalidPath)
        {
            AddressablesQueueSweepResult sweep = IncidentQueueGrouping.FindPendingPathlessAddressablesWrappers(
                _pendingIncidents.Select(ToIncidentQueueEntry).ToArray(),
                incomingEntry);
            for (int i = 0; i < sweep.PendingGroupFingerprints.Length; i++)
            {
                _incidentFingerprints.Remove(sweep.PendingGroupFingerprints[i]);
                _pendingIncidents.RemoveAt(sweep.PendingIndices[i]);
            }

            if (sweep.AbsorbedDuplicateCount > 0)
            {
                incident = incident.WithDuplicateCount(incident.DuplicateCount + sweep.AbsorbedDuplicateCount);
                AddBreadcrumb("incident", "incident_duplicate_grouped", $"fingerprint={groupFingerprint}; absorbedPathlessWrappers={sweep.AbsorbedDuplicateCount.ToString(CultureInfo.InvariantCulture)}");
            }
        }

        for (int i = 0; i < _pendingIncidents.Count; i++)
        {
            PendingIncident pending = _pendingIncidents[i];
            string pendingGroup = IncidentFingerprint.BuildGroup(pending.Kind, pending.Title, pending.Message, pending.StackTrace);
            if (string.Equals(
                pendingGroup,
                groupFingerprint,
                StringComparison.OrdinalIgnoreCase))
            {
                _pendingIncidents[i] = pending.WithDuplicateCount(pending.DuplicateCount + incident.DuplicateCount);
                AddBreadcrumb("incident", "incident_duplicate_grouped", $"fingerprint={groupFingerprint}; seen={_pendingIncidents[i].DuplicateCount.ToString(CultureInfo.InvariantCulture)}");
                return false;
            }

            if (incomingAddressablesWrapper && IncidentQueueGrouping.IsAddressablesInvalidPathGroup(pendingGroup))
            {
                _pendingIncidents[i] = pending.WithDuplicateCount(pending.DuplicateCount + incident.DuplicateCount);
                AddBreadcrumb("incident", "incident_duplicate_grouped", $"fingerprint={pendingGroup}; seen={_pendingIncidents[i].DuplicateCount.ToString(CultureInfo.InvariantCulture)}");
                return false;
            }
        }

        if (incomingAddressablesWrapper && _incidentFingerprints.Any(IncidentQueueGrouping.IsAddressablesInvalidPathGroup))
        {
            AddBreadcrumb("incident", "incident_duplicate_grouped", "fingerprint=addressables-assetbundle-invalid-path; pathless wrapper suppressed after path-specific incident.");
            return false;
        }

        if (!bypassManagedIncidentLimit
            && string.Equals(incident.Kind, "managed_exception", StringComparison.OrdinalIgnoreCase)
            && _incidentsWrittenThisSession + _pendingIncidents.Count(pending => string.Equals(pending.Kind, "managed_exception", StringComparison.OrdinalIgnoreCase)) >= _maxIncidentsPerSession.Value)
        {
            return false;
        }

        if (!_incidentFingerprints.Add(groupFingerprint))
        {
            return false;
        }

        _pendingIncidents.Add(incident);
        return true;
    }

    private static IncidentQueueEntry ToIncidentQueueEntry(PendingIncident incident)
    {
        return new IncidentQueueEntry(
            incident.Kind,
            incident.Title,
            incident.Message,
            incident.StackTrace,
            incident.DuplicateCount);
    }

    private void FlushPendingIncidents()
    {
        while (_pendingIncidents.Count > 0)
        {
            PendingIncident incident = _pendingIncidents[0];
            _pendingIncidents.RemoveAt(0);
            TryWriteIncident(incident);
        }
    }

    private static string BuildManagedStack(string condition, string stackTrace)
    {
        if (string.IsNullOrWhiteSpace(condition))
        {
            return stackTrace ?? string.Empty;
        }

        if (string.IsNullOrWhiteSpace(stackTrace))
        {
            return condition;
        }

        return condition + Environment.NewLine + stackTrace;
    }

    private static string FirstNonEmptyLine(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        foreach (string line in value.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None))
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                return line.Trim();
            }
        }

        return string.Empty;
    }

    private static PendingIncident CreateSyntheticManagedExceptionIncident()
    {
        string pluginPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BepInEx", "plugins", "AvalonExceptions", "AvalonExceptions.dll");
        string stackTrace = string.Join(Environment.NewLine, new[]
        {
            @"System.InvalidOperationException: Synthetic The Last Menhir validation incident. This is not a real crash.",
            @"  at ExampleValidationPlugin.SyntheticCrashProbe.Run() in C:\Users\TestPlayer\AppData\LocalLow\Awaken Realms\Tainted Grail FoA\ValidationProbe.cs:line 42",
            $"  at AvalonExceptions.ValidationHarness.QueueSyntheticIncident() in {pluginPath}:line 1"
        });

        return new PendingIncident(
            "managed_exception",
            "error",
            "Synthetic validation managed exception.",
            "Synthetic validation incident generated by The Last Menhir config. No real exception was thrown and no crash occurred.",
            stackTrace);
    }

    private void AddBreadcrumb(string category, string eventName, string detail)
    {
        if (_redactor != null)
        {
            detail = _redactor.Redact(detail);
        }

        while (_breadcrumbs.Count >= BreadcrumbCapacity)
        {
            _breadcrumbs.Dequeue();
        }

        _breadcrumbs.Enqueue(new Breadcrumb(DateTime.UtcNow, category, eventName, detail));
    }

    private bool TryWriteIncident(PendingIncident pending)
    {
        if (_session == null || _redactor == null)
        {
            return false;
        }

        try
        {
            string shortId = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture).Substring(0, 8);
            string incidentId = $"{DateTime.UtcNow:yyyyMMdd-HHmmssZ}-{shortId}";
            string incidentFolder = Path.Combine(_rootFolder, "incidents", incidentId);
            Directory.CreateDirectory(incidentFolder);

            AddBreadcrumb("incident", "incident_started", $"kind={pending.Kind}; incidentId={incidentId}");

            LogExcerpt bepinexLog = ReadLogExcerpt(GetBepInExLogPath(), _maxLogExcerptLines.Value);
            LogExcerpt playerLog = _includeUnityPlayerLogExcerpt.Value
                ? ReadLogExcerpt(GetUnityPlayerLogPath(), _maxLogExcerptLines.Value)
                : LogExcerpt.Disabled("Unity Player log excerpt disabled by config.");

            PluginRecord[] plugins = BuildPluginInventory(_collectModDllHashes.Value);
            _lastPluginSnapshot = plugins;
            BepInExDependencyErrorFinding[] bepinExDependencyErrors = BepInExDependencyErrorProbe.Build();
            DependencyFinding[] dependencyFindings = BuildDependencyFindings(bepinexLog, playerLog);
            DependencyApiFinding[] dependencyApiFindings = DependencyApiProbe.Build(
                pending.Kind,
                pending.Title,
                pending.Message,
                pending.StackTrace);
            HarmonyPatchFinding[] harmonyPatchFindings = HarmonyPatchChainProbe.Build(
                pending.Kind,
                pending.Title,
                pending.Message,
                pending.StackTrace);
            LocalModPreflightRow[] localModPreflightRows = _lastLocalModPreflightRows.Length == 0
                ? LocalModPreflight.Build(GetLocalModsPath())
                : _lastLocalModPreflightRows;
            Breadcrumb[] breadcrumbs = _breadcrumbs.ToArray();
            IncidentRecord incident = IncidentRecord.Create(
                incidentId,
                _session,
                pending,
                plugins,
                bepinExDependencyErrors,
                dependencyFindings,
                dependencyApiFindings,
                harmonyPatchFindings,
                localModPreflightRows,
                breadcrumbs,
                bepinexLog,
                playerLog,
                _redactor);

            string reportMarkdown = incident.ToMarkdown();
            File.WriteAllText(Path.Combine(incidentFolder, "incident.json"), incident.ToJson(), Encoding.UTF8);
            File.WriteAllText(Path.Combine(incidentFolder, "report.html"), incident.ToHtml(), Encoding.UTF8);
            File.WriteAllText(Path.Combine(incidentFolder, "report.md"), reportMarkdown, Encoding.UTF8);
            File.WriteAllText(Path.Combine(incidentFolder, "session.json"), _session.ToJson(_redactor), Encoding.UTF8);
            File.WriteAllText(Path.Combine(incidentFolder, "mods.csv"), PluginRecord.ToCsv(plugins), Encoding.UTF8);
            File.WriteAllText(Path.Combine(incidentFolder, "bepinex_dependency_errors.csv"), BepInExDependencyErrorFinding.ToCsv(bepinExDependencyErrors), Encoding.UTF8);
            File.WriteAllText(Path.Combine(incidentFolder, "dependency_findings.csv"), DependencyFinding.ToCsv(dependencyFindings), Encoding.UTF8);
            File.WriteAllText(Path.Combine(incidentFolder, "dependency_api_findings.csv"), DependencyApiFinding.ToCsv(dependencyApiFindings), Encoding.UTF8);
            File.WriteAllText(Path.Combine(incidentFolder, "harmony_patch_findings.csv"), HarmonyPatchFinding.ToCsv(harmonyPatchFindings), Encoding.UTF8);
            File.WriteAllText(Path.Combine(incidentFolder, "local_mod_preflight.csv"), LocalModPreflight.ToCsv(localModPreflightRows), Encoding.UTF8);
            File.WriteAllText(Path.Combine(incidentFolder, "breadcrumbs.csv"), Breadcrumb.ToCsv(breadcrumbs), Encoding.UTF8);
            File.WriteAllText(Path.Combine(incidentFolder, "log_excerpt_bepinex.txt"), bepinexLog.ToText(), Encoding.UTF8);
            File.WriteAllText(Path.Combine(incidentFolder, "log_excerpt_player.txt"), playerLog.ToText(), Encoding.UTF8);
            File.WriteAllText(Path.Combine(incidentFolder, "fix-first.txt"), incident.ToFixFirstText(), Encoding.UTF8);
            File.WriteAllText(Path.Combine(incidentFolder, "copyable-summary.txt"), incident.ToCopyableSummary(), Encoding.UTF8);
            File.WriteAllText(Path.Combine(incidentFolder, "redaction_report.txt"), _redactor.BuildReport(), Encoding.UTF8);
            TryWriteSupportBundle(incidentFolder);
            string supportBundlePath = Path.Combine(incidentFolder, "support_bundle.zip");
            if (_showIncidentWindow.Value)
            {
                _incidentWindows.Enqueue(incident.ToWindowSnapshot(
                    incidentFolder,
                    File.Exists(supportBundlePath) ? supportBundlePath : string.Empty,
                    reportMarkdown));
            }

            _incidentsWrittenThisSession++;
            TryWriteSessionSummary();
            if (string.Equals(pending.Kind, "possible_hang_or_freeze", StringComparison.OrdinalIgnoreCase))
            {
                TryDeleteCurrentWatchdogState();
            }

            Logger.LogInfo($"{PluginName} wrote incident report: {incidentFolder}");
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"{PluginName} failed to write incident report. {ex.GetType().Name}: {ex.Message}");
            return false;
        }
    }

    private void TryWriteSupportBundle(string incidentFolder)
    {
        if (!_generateSupportBundle.Value || _redactor == null)
        {
            return;
        }

        SupportBundleResult result = SupportBundleWriter.TryCreate(incidentFolder, _redactor);
        if (result.Success)
        {
            Logger.LogInfo($"{PluginName} wrote support bundle: {result.BundlePath}");
            return;
        }

        string notePath = Path.Combine(incidentFolder, "support_bundle_error.txt");
        File.WriteAllText(notePath, $"supportBundleCreated=false{Environment.NewLine}reason={result.Error}{Environment.NewLine}", Encoding.UTF8);
        Logger.LogWarning($"{PluginName} did not create support bundle. {result.Error}");
    }

    private void TryWriteSessionSummary()
    {
        if (_session == null || string.IsNullOrWhiteSpace(_rootFolder))
        {
            return;
        }

        try
        {
            IncidentSummaryEntry[] entries = ReadIncidentSummaryEntries();
            IncidentSummaryEntry[] currentSession = entries
                .Where(entry => string.Equals(entry.SessionId, _session.SessionId, StringComparison.OrdinalIgnoreCase))
                .OrderBy(entry => entry.CapturedAtUtc)
                .ToArray();
            if (currentSession.Length == 0)
            {
                return;
            }

            IncidentSummaryEntry primary = currentSession
                .OrderByDescending(entry => RoutePriority(entry.RouteId))
                .ThenByDescending(entry => entry.CapturedAtUtc)
                .First();
            IncidentSummaryEntry[] followOns = currentSession
                .Where(entry => !string.Equals(entry.IncidentId, primary.IncidentId, StringComparison.OrdinalIgnoreCase))
                .OrderBy(entry => entry.CapturedAtUtc)
                .ToArray();

            int recentLaunchesScanned = 0;
            int seenAcrossRecentLaunches = CountSeenAcrossRecentLaunches(entries, primary, out recentLaunchesScanned);
            int groupedOccurrenceCount = currentSession.Sum(entry => Math.Max(1, entry.DuplicateCount));
            SessionSummary summary = new SessionSummary(
                _session.SessionId,
                DateTime.UtcNow,
                currentSession.Length,
                groupedOccurrenceCount,
                primary,
                followOns,
                recentLaunchesScanned,
                seenAcrossRecentLaunches);

            File.WriteAllText(Path.Combine(_rootFolder, "session-summary.json"), summary.ToJson(), Encoding.UTF8);
            File.WriteAllText(Path.Combine(_rootFolder, "session-summary.md"), summary.ToMarkdown(), Encoding.UTF8);
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"{PluginName} could not write session summary. {ex.GetType().Name}: {ex.Message}");
        }
    }

    private IncidentSummaryEntry[] ReadIncidentSummaryEntries()
    {
        string incidentRoot = Path.Combine(_rootFolder, "incidents");
        if (!Directory.Exists(incidentRoot))
        {
            return Array.Empty<IncidentSummaryEntry>();
        }

        List<IncidentSummaryEntry> entries = new List<IncidentSummaryEntry>();
        foreach (string incidentFolder in Directory.GetDirectories(incidentRoot))
        {
            string incidentJsonPath = Path.Combine(incidentFolder, "incident.json");
            if (!File.Exists(incidentJsonPath))
            {
                continue;
            }

            try
            {
                string json = File.ReadAllText(incidentJsonPath, Encoding.UTF8);
                if (IncidentSummaryEntry.TryRead(incidentFolder, json, out IncidentSummaryEntry entry))
                {
                    entries.Add(entry);
                }
            }
            catch
            {
                // Session summaries should never block incident reporting.
            }
        }

        return entries
            .OrderByDescending(entry => entry.CapturedAtUtc)
            .Take(120)
            .ToArray();
    }

    private static int CountSeenAcrossRecentLaunches(IncidentSummaryEntry[] entries, IncidentSummaryEntry primary, out int recentLaunchesScanned)
    {
        string[] recentSessions = entries
            .OrderByDescending(entry => entry.CapturedAtUtc)
            .Select(entry => entry.SessionId)
            .Where(sessionId => !string.IsNullOrWhiteSpace(sessionId))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(10)
            .ToArray();
        recentLaunchesScanned = recentSessions.Length;
        if (recentSessions.Length == 0)
        {
            return 0;
        }

        HashSet<string> recentSessionSet = new HashSet<string>(recentSessions, StringComparer.OrdinalIgnoreCase);
        return entries
            .Where(entry => recentSessionSet.Contains(entry.SessionId)
                && string.Equals(entry.Signature, primary.Signature, StringComparison.OrdinalIgnoreCase))
            .Select(entry => entry.SessionId)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();
    }

    private static int RoutePriority(string routeId)
    {
        if (string.Equals(routeId, "addressables-assetbundle-invalid-path", StringComparison.OrdinalIgnoreCase)
            || string.Equals(routeId, AddressablesCatalogCorruptEvidence.RouteId, StringComparison.OrdinalIgnoreCase))
        {
            return 100;
        }

        if (string.Equals(routeId, "bepinex-loader-dependency-error", StringComparison.OrdinalIgnoreCase)
            || string.Equals(routeId, "managed-dependency-mismatch", StringComparison.OrdinalIgnoreCase))
        {
            return 90;
        }

        if (string.Equals(routeId, "possible-hang-freeze", StringComparison.OrdinalIgnoreCase)
            || string.Equals(routeId, "previous-session-unclean-exit", StringComparison.OrdinalIgnoreCase))
        {
            return 75;
        }

        if (string.Equals(routeId, ManagedDependencyCascadeEvidence.FollowOnRouteId, StringComparison.OrdinalIgnoreCase))
        {
            return 50;
        }

        return 10;
    }

    private void PatchGameQuitHandlers()
    {
        try
        {
            Type? titleScreenType = AccessTools.TypeByName("Awaken.TG.Main.UI.TitleScreen.TitleScreenUI");
            MethodInfo? exitMethod = titleScreenType == null
                ? null
                : AccessTools.Method(titleScreenType, "Exit", Type.EmptyTypes);
            if (exitMethod == null)
            {
                Logger.LogWarning($"{PluginName} could not patch game quit handler. TitleScreenUI.Exit was not found.");
                return;
            }

            _harmony = new Harmony(GameQuitPatchId);
            MethodInfo prefix = AccessTools.Method(typeof(Plugin), nameof(BeforeGameQuit));
            _harmony.Patch(exitMethod, prefix: new HarmonyMethod(prefix));
            Logger.LogInfo($"{PluginName} patched game quit handler for clean-shutdown marker. target={exitMethod.DeclaringType?.FullName}.{exitMethod.Name}");
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"{PluginName} could not patch game quit handler. {ex.GetType().Name}: {ex.Message}");
        }
    }

    private void UnpatchGameQuitHandlers()
    {
        try
        {
            _harmony?.UnpatchSelf();
            _harmony = null;
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"{PluginName} could not unpatch game quit handler. {ex.GetType().Name}: {ex.Message}");
        }
    }

    private static void BeforeGameQuit()
    {
        s_instance?.MarkCleanShutdown(refreshUnityState: false);
    }

    private void MarkCleanShutdown(bool refreshUnityState = true)
    {
        if (_shutdownMarked || !_reportingAvailable || _session == null)
        {
            return;
        }

        try
        {
            StopMainThreadWatchdog();
            if (ShouldPreserveUncleanSessionForWatchdog())
            {
                _shutdownMarked = true;
                Logger.LogWarning($"{PluginName} preserved stale watchdog evidence and left the session unclean for next-launch freeze reporting. sessionId={_session.SessionId}");
                return;
            }

            UpdateSessionHeartbeat(DateTime.UtcNow, refreshUnityState ? SafeActiveSceneName() : _lastActiveScene);
            WriteSessionState(cleanShutdown: true);
            TryDeleteCurrentWatchdogState();
            _shutdownMarked = true;
            Logger.LogInfo($"{PluginName} marked session clean. sessionId={_session.SessionId}; refreshUnityState={refreshUnityState}");
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"{PluginName} could not mark clean shutdown. {ex.GetType().Name}: {ex.Message}");
        }
    }

    private bool ShouldPreserveUncleanSessionForWatchdog()
    {
        WatchdogState? state = WatchdogState.TryRead(_watchdogStatePath);
        return state != null
            && _session != null
            && WatchdogShutdownPolicy.ShouldPreserveUncleanSession(_session.SessionId, state.SessionId);
    }

    private void WriteSessionState(bool cleanShutdown)
    {
        if (_session == null || _redactor == null)
        {
            return;
        }

        _session = _session.WithCleanShutdown(cleanShutdown, DateTime.UtcNow);
        File.WriteAllText(_sessionStatePath, _session.ToJson(_redactor), Encoding.UTF8);
    }

    private static string GetBepInExLogPath()
    {
        DirectoryInfo? configDirectory = Directory.GetParent(Paths.ConfigPath);
        string bepinexRoot = configDirectory?.FullName ?? Paths.ConfigPath;
        return Path.Combine(bepinexRoot, "LogOutput.log");
    }

    private static string GetUnityPlayerLogPath()
    {
        try
        {
            PropertyInfo? consoleLogPath = typeof(Application).GetProperty("consoleLogPath", BindingFlags.Public | BindingFlags.Static);
            string? value = consoleLogPath?.GetValue(null, null) as string;
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }
        catch
        {
            // Fallback below.
        }

        string persistentDataPath = Application.persistentDataPath;
        return string.IsNullOrWhiteSpace(persistentDataPath)
            ? string.Empty
            : Path.Combine(persistentDataPath, "Player.log");
    }

    private static string GetLocalModsPath()
    {
        string persistentDataPath = Application.persistentDataPath;
        return string.IsNullOrWhiteSpace(persistentDataPath)
            ? string.Empty
            : Path.Combine(persistentDataPath, "Mods");
    }

    private LogExcerpt ReadLogExcerpt(string path, int maxLines)
    {
        if (_redactor == null)
        {
            return LogExcerpt.Unavailable(path, "redactor unavailable");
        }

        if (string.IsNullOrWhiteSpace(path))
        {
            return LogExcerpt.Unavailable(path, "log path unavailable");
        }

        if (!File.Exists(path))
        {
            return LogExcerpt.Unavailable(path, "log file not found");
        }

        try
        {
            int limit = Math.Max(0, maxLines);
            if (limit == 0)
            {
                return LogExcerpt.FromLines(path, Array.Empty<string>(), "log excerpt disabled by MaxLogExcerptLines=0");
            }

            Queue<string> lines = new Queue<string>();
            foreach (string line in File.ReadLines(path))
            {
                while (lines.Count >= limit)
                {
                    lines.Dequeue();
                }

                lines.Enqueue(_redactor.Redact(line));
            }

            return LogExcerpt.FromLines(path, lines.ToArray(), string.Empty);
        }
        catch (Exception ex)
        {
            return LogExcerpt.Unavailable(path, _redactor.Redact($"{ex.GetType().Name}: {ex.Message}"));
        }
    }

    private PluginRecord[] BuildPluginInventory(bool includeHash)
    {
        if (_redactor == null)
        {
            return Array.Empty<PluginRecord>();
        }

        List<PluginRecord> records = new List<PluginRecord>();
#if IL2CPP
        IEnumerable<KeyValuePair<string, BepInEx.PluginInfo>> pluginInfos =
            IL2CPPChainloader.Instance.Plugins;
#else
        IEnumerable<KeyValuePair<string, BepInEx.PluginInfo>> pluginInfos =
            Chainloader.PluginInfos;
#endif
        foreach (KeyValuePair<string, BepInEx.PluginInfo> entry in pluginInfos.OrderBy(item => item.Key, StringComparer.OrdinalIgnoreCase))
        {
            BepInEx.PluginInfo info = entry.Value;
            string guid = info.Metadata?.GUID ?? entry.Key;
            string name = info.Metadata?.Name ?? string.Empty;
            string version = info.Metadata?.Version?.ToString() ?? string.Empty;
            string location = TryReadPluginLocation(info);
            Assembly? assembly = TryReadPluginAssembly(info);
            AssemblyName? assemblyName = assembly?.GetName();
            string dll = string.IsNullOrWhiteSpace(location) ? string.Empty : Path.GetFileName(location);
            string assemblyFullName = assembly?.FullName ?? string.Empty;
            string assemblyVersion = assemblyName?.Version?.ToString() ?? string.Empty;
            string fileVersion = TryReadFileVersion(location);
            long fileSizeBytes = TryReadFileSizeBytes(location);
            DateTime? lastWriteUtc = TryReadLastWriteUtc(location);
            string hash = includeHash ? TryHashPluginDll(location) : string.Empty;
            records.Add(new PluginRecord(
                guid,
                name,
                version,
                dll,
                assemblyFullName,
                assemblyVersion,
                fileVersion,
                fileSizeBytes,
                lastWriteUtc,
                hash));
        }

        return records.ToArray();
    }

    private static Assembly? TryReadPluginAssembly(BepInEx.PluginInfo info)
    {
        try
        {
            PropertyInfo? instanceProperty = info.GetType().GetProperty("Instance", BindingFlags.Public | BindingFlags.Instance);
            object? instance = instanceProperty?.GetValue(info, null);
            if (instance != null)
            {
                return instance.GetType().Assembly;
            }

            PropertyInfo? assemblyProperty = info.GetType().GetProperty("Assembly", BindingFlags.Public | BindingFlags.Instance);
            return assemblyProperty?.GetValue(info, null) as Assembly;
        }
        catch
        {
            return null;
        }
    }

    private static string TryReadPluginLocation(BepInEx.PluginInfo info)
    {
        try
        {
            PropertyInfo? property = info.GetType().GetProperty("Location", BindingFlags.Public | BindingFlags.Instance);
            return property?.GetValue(info, null) as string ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string TryReadFileVersion(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            return string.Empty;
        }

        try
        {
            return FileVersionInfo.GetVersionInfo(path).FileVersion ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static long TryReadFileSizeBytes(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            return 0L;
        }

        try
        {
            return new FileInfo(path).Length;
        }
        catch
        {
            return 0L;
        }
    }

    private static DateTime? TryReadLastWriteUtc(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            return null;
        }

        try
        {
            return File.GetLastWriteTimeUtc(path);
        }
        catch
        {
            return null;
        }
    }

    private string TryHashPluginDll(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            return string.Empty;
        }

        try
        {
            string pluginRoot = Path.GetFullPath(Paths.PluginPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
            string fullPath = Path.GetFullPath(path);
            if (!fullPath.StartsWith(pluginRoot, StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty;
            }

            using FileStream stream = File.OpenRead(fullPath);
            using SHA256 sha = SHA256.Create();
            return ToHex(sha.ComputeHash(stream));
        }
        catch
        {
            return string.Empty;
        }
    }

    private static DependencyFinding[] BuildDependencyFindings(params LogExcerpt[] excerpts)
    {
        List<DependencyFinding> findings = new List<DependencyFinding>();
        string[] keywords = { "dependency", "missing", "could not load", "failed to load", "incompatib" };
        foreach (LogExcerpt excerpt in excerpts)
        {
            foreach (string line in excerpt.Lines)
            {
                if (keywords.Any(keyword => line.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    findings.Add(new DependencyFinding(excerpt.SourceLabel, line));
                }
            }
        }

        return findings.Take(100).ToArray();
    }

    private static string ToHex(byte[] bytes)
    {
        StringBuilder builder = new StringBuilder(bytes.Length * 2);
        foreach (byte value in bytes)
        {
            builder.Append(value.ToString("x2", CultureInfo.InvariantCulture));
        }

        return builder.ToString();
    }

    private sealed class PreviousSessionEvidence
    {
        private PreviousSessionEvidence(
            string sessionId,
            DateTime startedAtUtc,
            DateTime? lastHeartbeatAtUtc,
            int? lastHeartbeatFrame,
            string lastActiveScene,
            DateTime? watchdogDetectedAtUtc,
            int? watchdogStaleSeconds)
        {
            SessionId = sessionId;
            StartedAtUtc = startedAtUtc;
            LastHeartbeatAtUtc = lastHeartbeatAtUtc;
            LastHeartbeatFrame = lastHeartbeatFrame;
            LastActiveScene = lastActiveScene;
            WatchdogDetectedAtUtc = watchdogDetectedAtUtc;
            WatchdogStaleSeconds = watchdogStaleSeconds;
        }

        internal string SessionId { get; }
        internal DateTime StartedAtUtc { get; }
        internal DateTime? LastHeartbeatAtUtc { get; }
        internal int? LastHeartbeatFrame { get; }
        internal string LastActiveScene { get; }
        internal DateTime? WatchdogDetectedAtUtc { get; }
        internal int? WatchdogStaleSeconds { get; }

        internal static PreviousSessionEvidence From(SessionRecord previousSession)
        {
            return new PreviousSessionEvidence(
                previousSession.SessionId,
                previousSession.StartedAtUtc,
                previousSession.LastHeartbeatAtUtc,
                previousSession.LastHeartbeatFrame,
                previousSession.LastActiveScene,
                null,
                null);
        }

        internal static PreviousSessionEvidence From(WatchdogState watchdog)
        {
            return new PreviousSessionEvidence(
                watchdog.SessionId,
                watchdog.SessionStartedAtUtc,
                watchdog.LastHeartbeatAtUtc,
                null,
                watchdog.ActiveScene,
                watchdog.DetectedAtUtc,
                watchdog.StaleSeconds);
        }
    }

    private readonly struct PendingIncident
    {
        internal PendingIncident(
            string kind,
            string severity,
            string title,
            string message,
            string stackTrace,
            PreviousSessionEvidence? previousSessionEvidence = null,
            int duplicateCount = 1)
        {
            Kind = kind;
            Severity = severity;
            Title = title;
            Message = message;
            StackTrace = stackTrace;
            PreviousSessionEvidence = previousSessionEvidence;
            DuplicateCount = Math.Max(1, duplicateCount);
        }

        internal string Kind { get; }
        internal string Severity { get; }
        internal string Title { get; }
        internal string Message { get; }
        internal string StackTrace { get; }
        internal PreviousSessionEvidence? PreviousSessionEvidence { get; }
        internal int DuplicateCount { get; }

        internal PendingIncident WithDuplicateCount(int duplicateCount)
        {
            return new PendingIncident(
                Kind,
                Severity,
                Title,
                Message,
                StackTrace,
                PreviousSessionEvidence,
                duplicateCount);
        }
    }

    private sealed class BepInExLogIncidentListener : ILogListener
    {
        private readonly Plugin _plugin;

        internal BepInExLogIncidentListener(Plugin plugin)
        {
            _plugin = plugin;
        }

#if IL2CPP
        public LogLevel LogLevelFilter => LogLevel.Error | LogLevel.Fatal;
#endif

        public void LogEvent(object sender, LogEventArgs eventArgs)
        {
            _plugin.OnBepInExLogEvent(eventArgs);
        }

        public void Dispose()
        {
        }
    }

    private readonly struct Breadcrumb
    {
        internal Breadcrumb(DateTime timestampUtc, string category, string eventName, string detail)
        {
            TimestampUtc = timestampUtc;
            Category = category;
            EventName = eventName;
            Detail = detail;
        }

        internal DateTime TimestampUtc { get; }
        internal string Category { get; }
        internal string EventName { get; }
        internal string Detail { get; }

        internal static string ToCsv(IEnumerable<Breadcrumb> breadcrumbs)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("timestampUtc,category,event,detail");
            foreach (Breadcrumb breadcrumb in breadcrumbs)
            {
                builder.AppendCsvRow(
                    breadcrumb.TimestampUtc.ToString("o", CultureInfo.InvariantCulture),
                    breadcrumb.Category,
                    breadcrumb.EventName,
                    breadcrumb.Detail);
            }

            return builder.ToString();
        }
    }

    private readonly struct PluginRecord
    {
        internal PluginRecord(
            string guid,
            string name,
            string version,
            string dll,
            string assemblyFullName,
            string assemblyVersion,
            string fileVersion,
            long fileSizeBytes,
            DateTime? lastWriteUtc,
            string sha256)
        {
            Guid = guid;
            Name = name;
            Version = version;
            Dll = dll;
            AssemblyFullName = assemblyFullName;
            AssemblyVersion = assemblyVersion;
            FileVersion = fileVersion;
            FileSizeBytes = fileSizeBytes;
            LastWriteUtc = lastWriteUtc;
            Sha256 = sha256;
        }

        internal string Guid { get; }
        internal string Name { get; }
        internal string Version { get; }
        internal string Dll { get; }
        internal string AssemblyFullName { get; }
        internal string AssemblyVersion { get; }
        internal string FileVersion { get; }
        internal long FileSizeBytes { get; }
        internal DateTime? LastWriteUtc { get; }
        internal string Sha256 { get; }

        internal static string ToCsv(IEnumerable<PluginRecord> records)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("guid,name,version,dll,assemblyFullName,assemblyVersion,fileVersion,fileSizeBytes,lastWriteUtc,sha256");
            foreach (PluginRecord record in records)
            {
                builder.AppendCsvRow(
                    record.Guid,
                    record.Name,
                    record.Version,
                    record.Dll,
                    record.AssemblyFullName,
                    record.AssemblyVersion,
                    record.FileVersion,
                    record.FileSizeBytes <= 0 ? string.Empty : record.FileSizeBytes.ToString(CultureInfo.InvariantCulture),
                    record.LastWriteUtc?.ToString("o", CultureInfo.InvariantCulture) ?? string.Empty,
                    record.Sha256);
            }

            return builder.ToString();
        }
    }

    private readonly struct DependencyFinding
    {
        internal DependencyFinding(string source, string line)
        {
            Source = source;
            Line = line;
        }

        internal string Source { get; }
        internal string Line { get; }

        internal static string ToCsv(IEnumerable<DependencyFinding> findings)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("source,line");
            foreach (DependencyFinding finding in findings)
            {
                builder.AppendCsvRow(finding.Source, finding.Line);
            }

            return builder.ToString();
        }
    }

    private sealed class LogExcerpt
    {
        private LogExcerpt(string sourcePath, string sourceLabel, string[] lines, string note, bool available)
        {
            SourcePath = sourcePath;
            SourceLabel = sourceLabel;
            Lines = lines;
            Note = note;
            Available = available;
        }

        internal string SourcePath { get; }
        internal string SourceLabel { get; }
        internal string[] Lines { get; }
        internal string Note { get; }
        internal bool Available { get; }

        internal static LogExcerpt FromLines(string sourcePath, string[] lines, string note)
        {
            return new LogExcerpt(sourcePath, Path.GetFileName(sourcePath), lines, note, available: true);
        }

        internal static LogExcerpt Unavailable(string sourcePath, string note)
        {
            string label = string.IsNullOrWhiteSpace(sourcePath) ? "unavailable" : Path.GetFileName(sourcePath);
            return new LogExcerpt(sourcePath, label, Array.Empty<string>(), note, available: false);
        }

        internal static LogExcerpt Disabled(string note)
        {
            return new LogExcerpt(string.Empty, "disabled", Array.Empty<string>(), note, available: false);
        }

        internal string ToText()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"available={Available}");
            builder.AppendLine($"source={SourceLabel}");
            builder.AppendLine($"note={Note}");
            builder.AppendLine();
            foreach (string line in Lines)
            {
                builder.AppendLine(line);
            }

            return builder.ToString();
        }
    }

    private sealed class SessionRecord
    {
        private SessionRecord(
            string sessionId,
            DateTime startedAtUtc,
            DateTime? endedAtUtc,
            bool cleanShutdown,
            string gameVersion,
            string unityVersion,
            string platform,
            DateTime? lastHeartbeatAtUtc,
            int lastHeartbeatFrame,
            string lastActiveScene)
        {
            SessionId = sessionId;
            StartedAtUtc = startedAtUtc;
            EndedAtUtc = endedAtUtc;
            CleanShutdown = cleanShutdown;
            GameVersion = gameVersion;
            UnityVersion = unityVersion;
            Platform = platform;
            LastHeartbeatAtUtc = lastHeartbeatAtUtc;
            LastHeartbeatFrame = lastHeartbeatFrame;
            LastActiveScene = lastActiveScene;
        }

        internal string SessionId { get; }
        internal DateTime StartedAtUtc { get; }
        internal DateTime? EndedAtUtc { get; }
        internal bool CleanShutdown { get; }
        internal string GameVersion { get; }
        internal string UnityVersion { get; }
        internal string Platform { get; }
        internal DateTime? LastHeartbeatAtUtc { get; }
        internal int LastHeartbeatFrame { get; }
        internal string LastActiveScene { get; }

        internal static SessionRecord Create(DateTime heartbeatUtc, int frameCount, string activeScene)
        {
            return new SessionRecord(
                Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture),
                DateTime.UtcNow,
                null,
                cleanShutdown: false,
                SafeApplicationValue(() => Application.version),
                SafeApplicationValue(() => Application.unityVersion),
                SafeApplicationValue(() => Application.platform.ToString()),
                heartbeatUtc,
                frameCount,
                activeScene);
        }

        internal SessionRecord WithCleanShutdown(bool cleanShutdown, DateTime timestampUtc)
        {
            return new SessionRecord(
                SessionId,
                StartedAtUtc,
                cleanShutdown ? timestampUtc : EndedAtUtc,
                cleanShutdown,
                GameVersion,
                UnityVersion,
                Platform,
                LastHeartbeatAtUtc,
                LastHeartbeatFrame,
                LastActiveScene);
        }

        internal SessionRecord WithHeartbeat(DateTime heartbeatUtc, int frameCount, string activeScene)
        {
            return new SessionRecord(
                SessionId,
                StartedAtUtc,
                EndedAtUtc,
                CleanShutdown,
                GameVersion,
                UnityVersion,
                Platform,
                heartbeatUtc,
                frameCount,
                activeScene);
        }

        internal string ToJson(ReportRedactor redactor)
        {
            JsonWriter writer = new JsonWriter();
            writer.BeginObject();
            writer.Property("sessionId", SessionId);
            writer.Property("startedAtUtc", StartedAtUtc.ToString("o", CultureInfo.InvariantCulture));
            writer.Property("endedAtUtc", EndedAtUtc?.ToString("o", CultureInfo.InvariantCulture) ?? string.Empty);
            writer.Property("cleanShutdown", CleanShutdown);
            writer.Property("pluginName", PluginName);
            writer.Property("pluginGuid", PluginGuid);
            writer.Property("pluginVersion", PluginVersion);
            writer.Property("gameVersion", GameVersion);
            writer.Property("unityVersion", UnityVersion);
            writer.Property("platform", Platform);
            writer.Property("lastMainThreadHeartbeatAtUtc", LastHeartbeatAtUtc?.ToString("o", CultureInfo.InvariantCulture) ?? string.Empty);
            writer.Property("lastMainThreadHeartbeatFrame", LastHeartbeatFrame);
            writer.Property("lastActiveScene", LastActiveScene);
            writer.Property("configRoot", redactor.Redact(Paths.ConfigPath));
            writer.EndObject();
            return writer.ToString();
        }

        internal static SessionRecord? TryRead(string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }

            try
            {
                string text = File.ReadAllText(path);
                string sessionId = ReadJsonString(text, "sessionId");
                string startedAt = ReadJsonString(text, "startedAtUtc");
                string heartbeatAt = ReadJsonString(text, "lastMainThreadHeartbeatAtUtc");
                bool cleanShutdown = ReadJsonBool(text, "cleanShutdown");
                if (string.IsNullOrWhiteSpace(sessionId) || !DateTime.TryParse(startedAt, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime started))
                {
                    return null;
                }

                DateTime? lastHeartbeat = DateTime.TryParse(heartbeatAt, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime parsedHeartbeat)
                    ? parsedHeartbeat.ToUniversalTime()
                    : null;
                return new SessionRecord(
                    sessionId,
                    started.ToUniversalTime(),
                    null,
                    cleanShutdown,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    lastHeartbeat,
                    ReadJsonInt(text, "lastMainThreadHeartbeatFrame"),
                    ReadJsonString(text, "lastActiveScene"));
            }
            catch
            {
                return null;
            }
        }
    }

    private sealed class WatchdogState
    {
        internal WatchdogState(
            string sessionId,
            DateTime sessionStartedAtUtc,
            DateTime lastHeartbeatAtUtc,
            DateTime detectedAtUtc,
            int staleSeconds,
            string activeScene,
            string reason)
        {
            SessionId = sessionId;
            SessionStartedAtUtc = sessionStartedAtUtc;
            LastHeartbeatAtUtc = lastHeartbeatAtUtc;
            DetectedAtUtc = detectedAtUtc;
            StaleSeconds = staleSeconds;
            ActiveScene = activeScene;
            Reason = reason;
        }

        internal string SessionId { get; }
        internal DateTime SessionStartedAtUtc { get; }
        internal DateTime LastHeartbeatAtUtc { get; }
        internal DateTime DetectedAtUtc { get; }
        internal int StaleSeconds { get; }
        internal string ActiveScene { get; }
        internal string Reason { get; }

        internal void TryWrite(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            try
            {
                JsonWriter writer = new JsonWriter();
                writer.BeginObject();
                writer.Property("sessionId", SessionId);
                writer.Property("sessionStartedAtUtc", SessionStartedAtUtc.ToString("o", CultureInfo.InvariantCulture));
                writer.Property("lastMainThreadHeartbeatAtUtc", LastHeartbeatAtUtc.ToString("o", CultureInfo.InvariantCulture));
                writer.Property("watchdogDetectedAtUtc", DetectedAtUtc.ToString("o", CultureInfo.InvariantCulture));
                writer.Property("staleSeconds", StaleSeconds);
                writer.Property("activeScene", ActiveScene);
                writer.Property("reason", Reason);
                writer.EndObject();
                File.WriteAllText(path, writer.ToString(), Encoding.UTF8);
            }
            catch
            {
                // Watchdog marker writes are best-effort; they must never worsen a freeze.
            }
        }

        internal static WatchdogState? TryRead(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return null;
            }

            try
            {
                string text = File.ReadAllText(path);
                string sessionId = ReadJsonString(text, "sessionId");
                string startedAt = ReadJsonString(text, "sessionStartedAtUtc");
                string heartbeatAt = ReadJsonString(text, "lastMainThreadHeartbeatAtUtc");
                string detectedAt = ReadJsonString(text, "watchdogDetectedAtUtc");
                if (string.IsNullOrWhiteSpace(sessionId)
                    || !DateTime.TryParse(startedAt, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime started)
                    || !DateTime.TryParse(heartbeatAt, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime heartbeat)
                    || !DateTime.TryParse(detectedAt, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime detected))
                {
                    return null;
                }

                return new WatchdogState(
                    sessionId,
                    started.ToUniversalTime(),
                    heartbeat.ToUniversalTime(),
                    detected.ToUniversalTime(),
                    ReadJsonInt(text, "staleSeconds"),
                    ReadJsonString(text, "activeScene"),
                    ReadJsonString(text, "reason"));
            }
            catch
            {
                return null;
            }
        }
    }

    private sealed class IncidentWindowSnapshot
    {
        internal IncidentWindowSnapshot(
            string title,
            string kind,
            string severity,
            string routeLabel,
            string routeConfidence,
            string modSuspectStatus,
            string likelyCause,
            string confidence,
            string recommendedAction,
            string doesNotProve,
            string bestNextCheck,
            string gameVersion,
            string unityVersion,
            int loadedPluginCount,
            string reportFolder,
            string supportBundlePath,
            string managedStackPreview,
            string loadedPluginsPreview,
            string ownerSummary,
            string evidenceBoundary)
        {
            Title = title;
            Kind = kind;
            Severity = severity;
            RouteLabel = routeLabel;
            RouteConfidence = routeConfidence;
            ModSuspectStatus = modSuspectStatus;
            LikelyCause = likelyCause;
            Confidence = confidence;
            RecommendedAction = recommendedAction;
            DoesNotProve = doesNotProve;
            BestNextCheck = bestNextCheck;
            GameVersion = gameVersion;
            UnityVersion = unityVersion;
            LoadedPluginCount = loadedPluginCount;
            ReportFolder = reportFolder;
            SupportBundlePath = supportBundlePath;
            ManagedStackPreview = managedStackPreview;
            LoadedPluginsPreview = loadedPluginsPreview;
            OwnerSummary = ownerSummary;
            EvidenceBoundary = evidenceBoundary;
        }

        internal string Title { get; }
        internal string Kind { get; }
        internal string Severity { get; }
        internal string RouteLabel { get; }
        internal string RouteConfidence { get; }
        internal string ModSuspectStatus { get; }
        internal string LikelyCause { get; }
        internal string Confidence { get; }
        internal string RecommendedAction { get; }
        internal string DoesNotProve { get; }
        internal string BestNextCheck { get; }
        internal string GameVersion { get; }
        internal string UnityVersion { get; }
        internal int LoadedPluginCount { get; }
        internal string ReportFolder { get; }
        internal string SupportBundlePath { get; }
        internal string ManagedStackPreview { get; }
        internal string LoadedPluginsPreview { get; }
        internal string OwnerSummary { get; }
        internal string EvidenceBoundary { get; }
    }

    private sealed class IncidentRecord
    {
        private IncidentRecord(
            string incidentId,
            SessionRecord session,
            PendingIncident pending,
            PluginRecord[] plugins,
            BepInExDependencyErrorFinding[] bepinExDependencyErrors,
            DependencyFinding[] dependencyFindings,
            DependencyApiFinding[] dependencyApiFindings,
            HarmonyPatchFinding[] harmonyPatchFindings,
            LocalModPreflightRow[] localModPreflightRows,
            Breadcrumb[] breadcrumbs,
            LogExcerpt bepinexLog,
            LogExcerpt playerLog,
            ReportRedactor redactor)
        {
            IncidentId = incidentId;
            Session = session;
            Pending = pending;
            Plugins = plugins;
            BepInExDependencyErrors = bepinExDependencyErrors;
            DependencyFindings = dependencyFindings;
            DependencyApiFindings = dependencyApiFindings;
            HarmonyPatchFindings = harmonyPatchFindings;
            LocalModPreflightRows = localModPreflightRows;
            Breadcrumbs = breadcrumbs;
            BepInExLog = bepinexLog;
            PlayerLog = playerLog;
            Redactor = redactor;
            CapturedAtUtc = DateTime.UtcNow;
        }

        private string IncidentId { get; }
        private SessionRecord Session { get; }
        private PendingIncident Pending { get; }
        private PluginRecord[] Plugins { get; }
        private BepInExDependencyErrorFinding[] BepInExDependencyErrors { get; }
        private DependencyFinding[] DependencyFindings { get; }
        private DependencyApiFinding[] DependencyApiFindings { get; }
        private HarmonyPatchFinding[] HarmonyPatchFindings { get; }
        private LocalModPreflightRow[] LocalModPreflightRows { get; }
        private Breadcrumb[] Breadcrumbs { get; }
        private LogExcerpt BepInExLog { get; }
        private LogExcerpt PlayerLog { get; }
        private ReportRedactor Redactor { get; }
        private DateTime CapturedAtUtc { get; }

        internal static IncidentRecord Create(
            string incidentId,
            SessionRecord session,
            PendingIncident pending,
            PluginRecord[] plugins,
            BepInExDependencyErrorFinding[] bepinExDependencyErrors,
            DependencyFinding[] dependencyFindings,
            DependencyApiFinding[] dependencyApiFindings,
            HarmonyPatchFinding[] harmonyPatchFindings,
            LocalModPreflightRow[] localModPreflightRows,
            Breadcrumb[] breadcrumbs,
            LogExcerpt bepinexLog,
            LogExcerpt playerLog,
            ReportRedactor redactor)
        {
            return new IncidentRecord(incidentId, session, pending, plugins, bepinExDependencyErrors, dependencyFindings, dependencyApiFindings, harmonyPatchFindings, localModPreflightRows, breadcrumbs, bepinexLog, playerLog, redactor);
        }

        internal string ToJson()
        {
            string[] suspects = BuildSuspectRows();
            IncidentClassification classification = BuildClassification(suspects);
            JsonWriter writer = new JsonWriter();
            writer.BeginObject();
            writer.Property("schemaVersion", "0.1.0");
            writer.Property("incidentId", IncidentId);
            writer.Property("sessionId", Session.SessionId);
            writer.Property("capturedAtUtc", CapturedAtUtc.ToString("o", CultureInfo.InvariantCulture));
            writer.Property("kind", Pending.Kind);
            writer.Property("severity", Pending.Severity);
            writer.Property("duplicateCount", Pending.DuplicateCount);
            writer.Property("summary", Redactor.Redact(Pending.Title));
            writer.Property("message", Redactor.Redact(Pending.Message));
            writer.Property("managedStack", Redactor.Redact(Pending.StackTrace));
            writer.Property("pluginName", PluginName);
            writer.Property("pluginGuid", PluginGuid);
            writer.Property("pluginVersion", PluginVersion);
            writer.Property("gameVersion", Session.GameVersion);
            writer.Property("unityVersion", Session.UnityVersion);
            writer.Property("platform", Session.Platform);
            writer.Property("routeId", classification.RouteId);
            writer.Property("routeLabel", classification.RouteLabel);
            writer.Property("routeConfidence", classification.RouteConfidence);
            writer.Property("routeEvidence", classification.RouteEvidence);
            writer.Property("modSuspectStatus", classification.ModSuspectStatus);
            writer.Property("likelyCause", classification.LikelyCauseLabel);
            writer.Property("confidence", classification.ConfidenceLabel);
            writer.Property("recommendedAction", classification.RecommendedAction);
            writer.Property("doesNotProve", classification.DoesNotProve);
            writer.Property("bestNextCheck", classification.BestNextCheck);
            if (TryGetAddressablesLocalModFinding(out AddressablesInvalidPathFinding addressablesFinding))
            {
                writer.Property("localModId", addressablesFinding.ModId);
                writer.Property("bundleRelativePath", addressablesFinding.BundlePath);
                writer.Property("localModFolderHint", addressablesFinding.LocalModFolderHint);
            }
            else if (TryGetAddressablesCatalogCorruptFinding(out AddressablesCatalogCorruptFinding addressablesCatalogFinding))
            {
                writer.Property("localModId", addressablesCatalogFinding.ModId);
                writer.Property("catalogRelativePath", addressablesCatalogFinding.CatalogRelativePath);
                writer.Property("localModFolderHint", addressablesCatalogFinding.LocalModFolderHint);
            }

            writer.Property("loadedPluginCount", Plugins.Length);
            writer.Property("bepinexDependencyErrorCount", BepInExDependencyErrors.Length);
            writer.Property("dependencyFindingCount", DependencyFindings.Length);
            writer.Property("dependencyApiFindingCount", DependencyApiFindings.Length);
            writer.Property("harmonyPatchFindingCount", HarmonyPatchFindings.Length);
            writer.Property("localModPreflightCount", LocalModPreflightRows.Length);
            writer.Property("localModPreflightIssueCount", LocalModPreflightRows.Count(row => row.HasIssue));
            if (string.Equals(classification.RouteId, ManagedDependencyCascadeEvidence.FollowOnRouteId, StringComparison.OrdinalIgnoreCase))
            {
                writer.Property("cascadeRole", "follow-on");
                writer.Property("fixFirstHint", "Fix the primary local-mod, loader dependency, or managed dependency/API incident in session-summary.md first.");
            }

            if (Pending.PreviousSessionEvidence != null)
            {
                writer.Property("previousSessionId", Pending.PreviousSessionEvidence.SessionId);
                writer.Property("previousSessionStartedAtUtc", Pending.PreviousSessionEvidence.StartedAtUtc.ToString("o", CultureInfo.InvariantCulture));
                writer.Property("previousSessionLastHeartbeatAtUtc", Pending.PreviousSessionEvidence.LastHeartbeatAtUtc?.ToString("o", CultureInfo.InvariantCulture) ?? string.Empty);
                writer.Property("previousSessionLastHeartbeatFrame", Pending.PreviousSessionEvidence.LastHeartbeatFrame?.ToString(CultureInfo.InvariantCulture) ?? string.Empty);
                writer.Property("previousSessionLastActiveScene", Pending.PreviousSessionEvidence.LastActiveScene);
                writer.Property("previousSessionWatchdogDetectedAtUtc", Pending.PreviousSessionEvidence.WatchdogDetectedAtUtc?.ToString("o", CultureInfo.InvariantCulture) ?? string.Empty);
                writer.Property("previousSessionWatchdogStaleSeconds", Pending.PreviousSessionEvidence.WatchdogStaleSeconds?.ToString(CultureInfo.InvariantCulture) ?? string.Empty);
            }

            writer.Property("breadcrumbCount", Breadcrumbs.Length);
            writer.Property("bepinexLogAvailable", BepInExLog.Available);
            writer.Property("playerLogAvailable", PlayerLog.Available);
            writer.Property("nativeCrashCapture", "not-collected-v0.1");
            writer.Property("uploadConsent", false);
            writer.Property("redactionApplied", true);
            writer.ArrayProperty("suspects", suspects);
            writer.EndObject();
            return writer.ToString();
        }

        internal string ToHtml()
        {
            string summary = Redactor.Redact(Pending.Title);
            string message = Redactor.Redact(Pending.Message);
            string stack = Redactor.Redact(Pending.StackTrace);
            StringBuilder builder = new StringBuilder();
            string[] suspects = BuildSuspectRows();
            IncidentClassification classification = BuildClassification(suspects);
            string primarySuspect = classification.LikelyCauseLabel;
            string confidence = classification.ConfidenceLabel;
            string firstStackLine = FirstNonEmptyLine(stack);
            string reportHeadline = BuildReportHeadline(classification);

            builder.AppendLine("<!doctype html>");
            builder.AppendLine("<html lang=\"en-GB\">");
            builder.AppendLine("<head>");
            builder.AppendLine("  <meta charset=\"utf-8\">");
            builder.AppendLine("  <meta name=\"viewport\" content=\"width=device-width,initial-scale=1\">");
            builder.AppendLine($"  <title>{Html(PluginName)} report</title>");
            AppendReportStyles(builder);
            builder.AppendLine("</head>");
            builder.AppendLine("<body>");
            builder.AppendLine("  <div class=\"report-shell\">");
            builder.AppendLine("    <header class=\"report-header\">");
            builder.AppendLine("      <div>");
            builder.AppendLine("        <p class=\"eyebrow\">Local crash evidence</p>");
            builder.AppendLine($"        <h1>{Html(reportHeadline)}</h1>");
            builder.AppendLine($"        <p class=\"summary\">{Html(summary)}</p>");
            builder.AppendLine("      </div>");
            builder.AppendLine("      <div class=\"header-metrics\">");
            AppendMetric(builder, "Incident", Pending.Kind);
            AppendMetric(builder, "Route", classification.RouteLabel);
            AppendMetric(builder, "Mod suspect", classification.ModSuspectStatus);
            AppendMetric(builder, "Confidence", confidence);
            AppendMetric(builder, "Captured", CapturedAtUtc.ToString("u", CultureInfo.InvariantCulture));
            builder.AppendLine("      </div>");
            builder.AppendLine("    </header>");
            builder.AppendLine("    <main class=\"tab-workspace\">");
            builder.AppendLine("      <input class=\"tab-input\" type=\"radio\" name=\"report-tab\" id=\"tab-overview\" checked>");
            builder.AppendLine("      <input class=\"tab-input\" type=\"radio\" name=\"report-tab\" id=\"tab-crash-chain\">");
            builder.AppendLine("      <input class=\"tab-input\" type=\"radio\" name=\"report-tab\" id=\"tab-dlls\">");
            builder.AppendLine("      <input class=\"tab-input\" type=\"radio\" name=\"report-tab\" id=\"tab-logs\">");
            builder.AppendLine("      <input class=\"tab-input\" type=\"radio\" name=\"report-tab\" id=\"tab-privacy\">");
            builder.AppendLine("      <nav class=\"tabs\" aria-label=\"Report sections\">");
            builder.AppendLine("        <label for=\"tab-overview\">Overview</label>");
            builder.AppendLine("        <label for=\"tab-crash-chain\">Crash Chain</label>");
            builder.AppendLine("        <label for=\"tab-dlls\">DLLs</label>");
            builder.AppendLine("        <label for=\"tab-logs\">Logs</label>");
            builder.AppendLine("        <label for=\"tab-privacy\">Privacy</label>");
            builder.AppendLine("      </nav>");
            builder.AppendLine("      <div class=\"panels\">");

            builder.AppendLine("        <section class=\"tab-panel\" id=\"panel-overview\">");
            AppendSectionTitle(builder, "Overview", "What happened, what looks most likely, and what to do next.");
            builder.AppendLine("          <div class=\"overview-grid\">");
            AppendCallout(builder, "Crash route", classification.RouteLabel, classification.RouteConfidence);
            AppendCallout(builder, "Likely cause", primarySuspect, confidence);
            AppendCallout(builder, "Evidence boundary", "This report is local evidence, not absolute mod blame. Native crash capture and upload are not enabled in v0.1.", "Policy");
            builder.AppendLine("          </div>");
            builder.AppendLine("          <table class=\"kv-table\"><tbody>");
            AppendRow(builder, "Incident ID", IncidentId);
            AppendRow(builder, "Kind", Pending.Kind);
            AppendRow(builder, "Severity", Pending.Severity);
            AppendRow(builder, "Seen this session", Pending.DuplicateCount.ToString(CultureInfo.InvariantCulture));
            AppendRow(builder, "Route", classification.RouteLabel);
            AppendRow(builder, "Route confidence", classification.RouteConfidence);
            AppendRow(builder, "Route evidence", classification.RouteEvidence);
            AppendAddressablesLocalModRows(builder);
            AppendRow(builder, "Mod suspect status", classification.ModSuspectStatus);
            AppendRow(builder, "Recommended action", classification.RecommendedAction);
            AppendRow(builder, "What this does not prove", classification.DoesNotProve);
            AppendRow(builder, "Best next check", classification.BestNextCheck);
            AppendRow(builder, "Message", message);
            AppendRow(builder, "Game", Session.GameVersion);
            AppendRow(builder, "Unity", Session.UnityVersion);
            AppendRow(builder, "Platform", Session.Platform);
            AppendRow(builder, "Loaded plugin DLLs", Plugins.Length.ToString(CultureInfo.InvariantCulture));
            AppendRow(builder, "BepInEx dependency errors", BepInExDependencyErrors.Length.ToString(CultureInfo.InvariantCulture));
            AppendRow(builder, "Dependency findings", DependencyFindings.Length.ToString(CultureInfo.InvariantCulture));
            AppendRow(builder, "Dependency/API probes", DependencyApiFindings.Length.ToString(CultureInfo.InvariantCulture));
            AppendRow(builder, "Harmony patch-chain findings", HarmonyPatchFindings.Length.ToString(CultureInfo.InvariantCulture));
            AppendPreviousSessionEvidenceRows(builder);
            AppendRow(builder, "Native crash capture", "not-collected-v0.1");
            AppendRow(builder, "Upload consent", "false");
            builder.AppendLine("          </tbody></table>");
            AppendOrderedList(builder, new[]
            {
                classification.RecommendedAction,
                classification.BestNextCheck,
                "Review the Crash Chain tab before disabling unrelated mods.",
                "Check dependency findings and loader errors first.",
                "If confidence is medium, low, or unknown, reproduce with a smaller mod list before assigning blame."
            });
            builder.AppendLine("        </section>");

            builder.AppendLine("        <section class=\"tab-panel\" id=\"panel-crash-chain\">");
            AppendSectionTitle(builder, "Crash Chain", "Exception summary, likely suspect evidence, stack trace, and recent breadcrumbs.");
            builder.AppendLine("          <table class=\"kv-table\"><tbody>");
            AppendRow(builder, "Exception or first stack line", string.IsNullOrWhiteSpace(firstStackLine) ? "No managed stack captured." : firstStackLine);
            AppendRow(builder, "Summary", summary);
            AppendRow(builder, "Message", message);
            AppendRow(builder, "Seen this session", Pending.DuplicateCount.ToString(CultureInfo.InvariantCulture));
            AppendRow(builder, "Route", classification.RouteLabel);
            AppendRow(builder, "Route confidence", classification.RouteConfidence);
            AppendRow(builder, "Route evidence", classification.RouteEvidence);
            AppendAddressablesLocalModRows(builder);
            AppendRow(builder, "Mod suspect status", classification.ModSuspectStatus);
            AppendRow(builder, "Likely cause", primarySuspect);
            AppendRow(builder, "Confidence", confidence);
            AppendRow(builder, "Recommended action", classification.RecommendedAction);
            AppendRow(builder, "What this does not prove", classification.DoesNotProve);
            AppendRow(builder, "Best next check", classification.BestNextCheck);
            AppendPreviousSessionEvidenceRows(builder);
            builder.AppendLine("          </tbody></table>");
            AppendSuspects(builder, suspects);
            AppendPre(builder, "Managed stack", string.IsNullOrWhiteSpace(stack) ? "No managed stack captured." : stack);
            AppendBreadcrumbTable(builder);
            builder.AppendLine("        </section>");

            builder.AppendLine("        <section class=\"tab-panel\" id=\"panel-dlls\">");
            AppendSectionTitle(builder, "DLLs", "Loaded BepInEx plugin inventory. Game DLLs and BepInEx binaries are not included.");
            AppendPluginTable(builder);
            builder.AppendLine("        </section>");

            builder.AppendLine("        <section class=\"tab-panel\" id=\"panel-logs\">");
            AppendSectionTitle(builder, "Logs", "Bounded, sanitized excerpts and dependency/load findings around the incident.");
            AppendBepInExDependencyErrorTable(builder);
            AppendDependencyTable(builder);
            AppendDependencyApiTable(builder);
            AppendHarmonyPatchTable(builder);
            AppendLocalModPreflightTable(builder);
            AppendLogExcerpt(builder, "BepInEx log excerpt", BepInExLog);
            AppendLogExcerpt(builder, "Unity Player log excerpt", PlayerLog);
            builder.AppendLine("        </section>");

            builder.AppendLine("        <section class=\"tab-panel\" id=\"panel-privacy\">");
            AppendSectionTitle(builder, "Privacy", "What was redacted, what is excluded, and what the support bundle may contain.");
            builder.AppendLine("          <table class=\"kv-table\"><tbody>");
            AppendRow(builder, "Redaction applied", "true");
            AppendRow(builder, "Upload enabled", "false");
            AppendRow(builder, "Support bundle file", "support_bundle.zip, only after redaction safety checks pass");
            AppendRow(builder, "Support bundle contents", "incident.json, report.html, report.md, session.json, mods.csv, bepinex_dependency_errors.csv, dependency_findings.csv, dependency_api_findings.csv, harmony_patch_findings.csv, local_mod_preflight.csv, breadcrumbs.csv, log excerpts, redaction_report.txt, fix-first.txt, copyable-summary.txt");
            AppendRow(builder, "Excluded by default", "saves, game DLLs, BepInEx binaries, full plugin folders, full config folders, raw minidumps, memory dumps, network upload");
            builder.AppendLine("          </tbody></table>");
            AppendPre(builder, "Redaction profile", Redactor.BuildReport());
            builder.AppendLine("        </section>");

            builder.AppendLine("      </div>");
            builder.AppendLine("    </main>");
            builder.AppendLine("  </div>");
            builder.AppendLine("</body>");
            builder.AppendLine("</html>");
            return builder.ToString();
        }

        internal string ToMarkdown()
        {
            string[] suspects = BuildSuspectRows();
            IncidentClassification classification = BuildClassification(suspects);
            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"**{BuildReportHeadline(classification)}**");
            builder.AppendLine($"- Incident: `{Pending.Kind}`");
            builder.AppendLine($"- Severity: `{Pending.Severity}`");
            builder.AppendLine($"- Seen this session: `{Pending.DuplicateCount.ToString(CultureInfo.InvariantCulture)}`");
            builder.AppendLine($"- Summary: {Redactor.Redact(Pending.Title)}");
            builder.AppendLine($"- Game: `{Session.GameVersion}` / Unity `{Session.UnityVersion}`");
            builder.AppendLine($"- Loaded plugins: `{Plugins.Length}`");
            builder.AppendLine($"- BepInEx dependency errors: `{BepInExDependencyErrors.Length}`");
            builder.AppendLine($"- Dependency findings: `{DependencyFindings.Length}`");
            builder.AppendLine($"- Dependency/API probes: `{DependencyApiFindings.Length}`");
            builder.AppendLine($"- Harmony patch-chain findings: `{HarmonyPatchFindings.Length}`");
            builder.AppendLine($"- Local mod preflight issues: `{LocalModPreflightRows.Count(row => row.HasIssue)}` of `{LocalModPreflightRows.Length}` checked local mod folders");
            builder.AppendLine("- Native crash capture: `not-collected-v0.1`");
            AppendPreviousSessionEvidenceMarkdown(builder);
            builder.AppendLine($"- Report headline: {BuildReportHeadline(classification)}");
            builder.AppendLine($"- Route: {classification.RouteLabel}");
            builder.AppendLine($"- Route confidence: {classification.RouteConfidence}");
            builder.AppendLine($"- Route evidence: {classification.RouteEvidence}");
            AppendAddressablesLocalModMarkdown(builder);
            builder.AppendLine($"- Mod suspect status: {classification.ModSuspectStatus}");
            builder.AppendLine($"- Likely cause: {classification.LikelyCauseLabel}");
            builder.AppendLine($"- What this does not prove: {classification.DoesNotProve}");
            builder.AppendLine($"- Best next check: {classification.BestNextCheck}");
            builder.AppendLine(suspects.Length == 0
                ? "- Mod suspects: no high-confidence suspect from v0.1 evidence"
                : $"- Mod suspects: {string.Join("; ", suspects)}");
            builder.AppendLine($"- Suggested action: {classification.RecommendedAction}");
            return builder.ToString();
        }

        internal string ToFixFirstText()
        {
            string[] suspects = BuildSuspectRows();
            IncidentClassification classification = BuildClassification(suspects);
            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"Fix first: {classification.RecommendedAction}");
            builder.AppendLine($"Route: {classification.RouteId} / {classification.RouteLabel}");
            builder.AppendLine($"Likely cause: {classification.LikelyCauseLabel}");
            builder.AppendLine($"Best next check: {classification.BestNextCheck}");
            builder.AppendLine($"Seen this session: {Pending.DuplicateCount.ToString(CultureInfo.InvariantCulture)}");
            builder.AppendLine($"Evidence boundary: {classification.DoesNotProve}");
            return Redactor.Redact(builder.ToString());
        }

        internal string ToCopyableSummary()
        {
            string[] suspects = BuildSuspectRows();
            IncidentClassification classification = BuildClassification(suspects);
            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"{PluginName} incident summary");
            builder.AppendLine($"Incident: {IncidentId}");
            builder.AppendLine($"Route: {classification.RouteId} ({classification.RouteLabel}, {classification.RouteConfidence})");
            builder.AppendLine($"Likely cause: {classification.LikelyCauseLabel}");
            builder.AppendLine($"Action: {classification.RecommendedAction}");
            builder.AppendLine($"Best next check: {classification.BestNextCheck}");
            builder.AppendLine($"Seen this session: {Pending.DuplicateCount.ToString(CultureInfo.InvariantCulture)}");
            builder.AppendLine($"Evidence boundary: {classification.DoesNotProve}");
            if (suspects.Length > 0)
            {
                builder.AppendLine($"Suspects: {string.Join("; ", suspects)}");
            }

            return Redactor.Redact(builder.ToString());
        }

        internal IncidentWindowSnapshot ToWindowSnapshot(string reportFolder, string supportBundlePath, string ownerSummary)
        {
            string summary = Redactor.Redact(Pending.Title);
            string stack = Redactor.Redact(Pending.StackTrace);
            string[] suspects = BuildSuspectRows();
            IncidentClassification classification = BuildClassification(suspects);
            string loadedPlugins = Plugins.Length == 0
                ? "No loaded BepInEx plugin records were captured."
                : string.Join(Environment.NewLine, Plugins
                    .OrderBy(plugin => plugin.Name, StringComparer.OrdinalIgnoreCase)
                    .Select(plugin => $"{plugin.Name} {plugin.Version} | {plugin.Guid} | {plugin.Dll} | file={plugin.FileVersion} | bytes={FormatPluginBytes(plugin.FileSizeBytes)} | lastWriteUtc={plugin.LastWriteUtc?.ToString("o", CultureInfo.InvariantCulture) ?? "unavailable"}"));

            string evidenceBoundary = string.Equals(Pending.Kind, "possible_hang_or_freeze", StringComparison.OrdinalIgnoreCase)
                ? "This is watchdog heartbeat evidence. It can support freeze or infinite-loading triage, but it does not prove native crash capture or confirmed loading-screen state."
                : "This is managed exception, BepInEx log, or previous-session evidence. Native minidumps are not collected and suspect labels are evidence hints, not absolute blame.";

            StringBuilder copySummary = new StringBuilder(ownerSummary.Trim());
            if (Pending.DuplicateCount > 1)
            {
                copySummary.AppendLine();
                copySummary.AppendLine($"Seen this session: {Pending.DuplicateCount.ToString(CultureInfo.InvariantCulture)} grouped occurrences.");
            }

            copySummary.AppendLine();
            copySummary.AppendLine();
            copySummary.AppendLine("Attach `support_bundle.zip` from the incident folder if it exists.");
            copySummary.AppendLine("Do not attach saves, full logs, game DLLs, BepInEx binaries, or full plugin folders unless a mod owner explicitly asks for them.");

            return new IncidentWindowSnapshot(
                BuildWindowTitle(summary, classification),
                Pending.Kind,
                Pending.Severity,
                classification.RouteLabel,
                classification.RouteConfidence,
                classification.ModSuspectStatus,
                classification.LikelyCauseLabel,
                classification.ConfidenceLabel,
                classification.RecommendedAction,
                classification.DoesNotProve,
                classification.BestNextCheck,
                Session.GameVersion,
                Session.UnityVersion,
                Plugins.Length,
                reportFolder,
                supportBundlePath,
                LimitLines(string.IsNullOrWhiteSpace(stack) ? "No managed call stack was captured for this incident." : stack, 140),
                LimitLines(loadedPlugins, 240),
                copySummary.ToString(),
                evidenceBoundary);
        }

        private static void AppendReportStyles(StringBuilder builder)
        {
            builder.AppendLine("  <style>");
            builder.AppendLine("    :root{color-scheme:dark;--bg:#111315;--panel:#1b1f23;--panel-2:#22272d;--line:#343b43;--text:#eceff2;--muted:#a9b1ba;--accent:#7ec7ff;--warn:#ffc46b;--bad:#ff8a8a;--good:#91d18b}");
            builder.AppendLine("    *{box-sizing:border-box}");
            builder.AppendLine("    body{margin:0;min-height:100vh;background:var(--bg);color:var(--text);font-family:Segoe UI,Arial,sans-serif;line-height:1.45}");
            builder.AppendLine("    .report-shell{min-height:100vh;display:flex;flex-direction:column}");
            builder.AppendLine("    .report-header{display:flex;justify-content:space-between;gap:24px;padding:24px 32px;border-bottom:1px solid var(--line);background:#171a1e}");
            builder.AppendLine("    .eyebrow{margin:0 0 4px;color:var(--accent);font-size:12px;font-weight:700;text-transform:uppercase;letter-spacing:0}");
            builder.AppendLine("    h1{margin:0;font-size:34px;line-height:1.1}");
            builder.AppendLine("    .summary{margin:10px 0 0;max-width:880px;color:var(--muted)}");
            builder.AppendLine("    .header-metrics{display:grid;grid-template-columns:repeat(3,minmax(120px,1fr));gap:10px;min-width:420px}");
            builder.AppendLine("    .metric{border:1px solid var(--line);background:var(--panel);padding:10px}");
            builder.AppendLine("    .metric span{display:block;color:var(--muted);font-size:12px}");
            builder.AppendLine("    .metric strong{display:block;margin-top:4px;font-size:14px;word-break:break-word}");
            builder.AppendLine("    .tab-workspace{flex:1;display:flex;flex-direction:column;min-height:0}");
            builder.AppendLine("    .tab-input{position:absolute;opacity:0;pointer-events:none}");
            builder.AppendLine("    .tabs{display:flex;gap:1px;padding:0 32px;background:#15181c;border-bottom:1px solid var(--line);position:sticky;top:0;z-index:2}");
            builder.AppendLine("    .tabs label{padding:14px 18px;color:var(--muted);cursor:pointer;border-left:1px solid transparent;border-right:1px solid transparent}");
            builder.AppendLine("    .tabs label:hover{color:var(--text);background:var(--panel)}");
            builder.AppendLine("    #tab-overview:checked~.tabs label[for=tab-overview],#tab-crash-chain:checked~.tabs label[for=tab-crash-chain],#tab-dlls:checked~.tabs label[for=tab-dlls],#tab-logs:checked~.tabs label[for=tab-logs],#tab-privacy:checked~.tabs label[for=tab-privacy]{color:var(--text);background:var(--panel-2);border-color:var(--line);box-shadow:inset 0 -3px 0 var(--accent)}");
            builder.AppendLine("    .panels{flex:1;padding:24px 32px;overflow:auto}");
            builder.AppendLine("    .tab-panel{display:none;max-width:1500px;margin:0 auto}");
            builder.AppendLine("    #tab-overview:checked~.panels #panel-overview,#tab-crash-chain:checked~.panels #panel-crash-chain,#tab-dlls:checked~.panels #panel-dlls,#tab-logs:checked~.panels #panel-logs,#tab-privacy:checked~.panels #panel-privacy{display:block}");
            builder.AppendLine("    .section-title{margin:0 0 18px}.section-title h2{margin:0;font-size:24px}.section-title p{margin:6px 0 0;color:var(--muted)}");
            builder.AppendLine("    .overview-grid{display:grid;grid-template-columns:repeat(2,minmax(0,1fr));gap:16px;margin-bottom:18px}");
            builder.AppendLine("    .callout,.table-wrap,.pre-block{border:1px solid var(--line);background:var(--panel);padding:16px;margin:16px 0}");
            builder.AppendLine("    .callout .label{color:var(--muted);font-size:12px;text-transform:uppercase;letter-spacing:0}.callout h3{margin:6px 0 8px;font-size:18px}.callout .badge{display:inline-block;margin-top:6px;padding:3px 8px;border:1px solid var(--line);background:#111820;color:var(--accent)}");
            builder.AppendLine("    table{width:100%;border-collapse:collapse;background:var(--panel)}th,td{border:1px solid var(--line);padding:8px 10px;text-align:left;vertical-align:top}th{background:var(--panel-2);color:#d9dee4}td{word-break:break-word}.kv-table th{width:220px;color:var(--muted)}");
            builder.AppendLine("    code,pre{font-family:Consolas,Menlo,monospace}.pre-block pre{margin:0;white-space:pre-wrap;word-break:break-word;color:#e8edf2}");
            builder.AppendLine("    .empty{color:var(--muted);font-style:italic}.support-list{margin:16px 0;padding-left:22px}.support-list li{margin:6px 0}");
            builder.AppendLine("    @media(max-width:900px){.report-header{display:block;padding:18px}.header-metrics{grid-template-columns:1fr;min-width:0;margin-top:16px}.tabs{padding:0 12px;overflow:auto}.tabs label{white-space:nowrap}.panels{padding:16px}.overview-grid{grid-template-columns:1fr}.kv-table th{width:auto}}");
            builder.AppendLine("  </style>");
        }

        private static void AppendMetric(StringBuilder builder, string label, string value)
        {
            builder.AppendLine("        <div class=\"metric\">");
            builder.AppendLine($"          <span>{Html(label)}</span>");
            builder.AppendLine($"          <strong>{Html(value)}</strong>");
            builder.AppendLine("        </div>");
        }

        private static void AppendSectionTitle(StringBuilder builder, string title, string description)
        {
            builder.AppendLine("          <div class=\"section-title\">");
            builder.AppendLine($"            <h2>{Html(title)}</h2>");
            builder.AppendLine($"            <p>{Html(description)}</p>");
            builder.AppendLine("          </div>");
        }

        private static void AppendCallout(StringBuilder builder, string label, string text, string badge)
        {
            builder.AppendLine("            <div class=\"callout\">");
            builder.AppendLine($"              <div class=\"label\">{Html(label)}</div>");
            builder.AppendLine($"              <h3>{Html(text)}</h3>");
            builder.AppendLine($"              <span class=\"badge\">{Html(badge)}</span>");
            builder.AppendLine("            </div>");
        }

        private static void AppendRow(StringBuilder builder, string label, string value)
        {
            builder.AppendLine($"            <tr><th>{Html(label)}</th><td>{Html(string.IsNullOrWhiteSpace(value) ? "unavailable" : value)}</td></tr>");
        }

        private void AppendPreviousSessionEvidenceRows(StringBuilder builder)
        {
            if (Pending.PreviousSessionEvidence == null)
            {
                return;
            }

            PreviousSessionEvidence evidence = Pending.PreviousSessionEvidence;
            AppendRow(builder, "Previous session ID", evidence.SessionId);
            AppendRow(builder, "Previous session started UTC", evidence.StartedAtUtc.ToString("o", CultureInfo.InvariantCulture));
            AppendRow(builder, "Previous session heartbeat UTC", evidence.LastHeartbeatAtUtc?.ToString("o", CultureInfo.InvariantCulture) ?? string.Empty);
            AppendRow(builder, "Previous session heartbeat frame", evidence.LastHeartbeatFrame?.ToString(CultureInfo.InvariantCulture) ?? string.Empty);
            AppendRow(builder, "Previous session active scene", evidence.LastActiveScene);
            AppendRow(builder, "Previous watchdog detected UTC", evidence.WatchdogDetectedAtUtc?.ToString("o", CultureInfo.InvariantCulture) ?? string.Empty);
            AppendRow(builder, "Previous watchdog stale seconds", evidence.WatchdogStaleSeconds?.ToString(CultureInfo.InvariantCulture) ?? string.Empty);
        }

        private void AppendAddressablesLocalModRows(StringBuilder builder)
        {
            if (TryGetAddressablesLocalModFinding(out AddressablesInvalidPathFinding finding))
            {
                AppendRow(builder, "Local mod ID", finding.ModId);
                AppendRow(builder, "Bundle relative path", finding.BundlePath);
                AppendRow(builder, "Local mod folder hint", finding.LocalModFolderHint);
                return;
            }

            if (TryGetAddressablesCatalogCorruptFinding(out AddressablesCatalogCorruptFinding catalogFinding))
            {
                AppendRow(builder, "Local mod ID", catalogFinding.ModId);
                AppendRow(builder, "Catalog relative path", catalogFinding.CatalogRelativePath);
                AppendRow(builder, "Local mod folder hint", catalogFinding.LocalModFolderHint);
            }
        }

        private void AppendPreviousSessionEvidenceMarkdown(StringBuilder builder)
        {
            if (Pending.PreviousSessionEvidence == null)
            {
                return;
            }

            PreviousSessionEvidence evidence = Pending.PreviousSessionEvidence;
            builder.AppendLine($"- Previous session ID: `{evidence.SessionId}`");
            builder.AppendLine($"- Previous session started UTC: `{evidence.StartedAtUtc.ToString("o", CultureInfo.InvariantCulture)}`");
            builder.AppendLine($"- Previous session heartbeat UTC: `{(evidence.LastHeartbeatAtUtc?.ToString("o", CultureInfo.InvariantCulture) ?? "unavailable")}`");
            builder.AppendLine($"- Previous session heartbeat frame: `{(evidence.LastHeartbeatFrame?.ToString(CultureInfo.InvariantCulture) ?? "unavailable")}`");
            builder.AppendLine($"- Previous session active scene: `{(string.IsNullOrWhiteSpace(evidence.LastActiveScene) ? "unavailable" : evidence.LastActiveScene)}`");
            if (evidence.WatchdogDetectedAtUtc.HasValue)
            {
                builder.AppendLine($"- Previous watchdog detected UTC: `{evidence.WatchdogDetectedAtUtc.Value.ToString("o", CultureInfo.InvariantCulture)}`");
            }

            if (evidence.WatchdogStaleSeconds.HasValue)
            {
                builder.AppendLine($"- Previous watchdog stale seconds: `{evidence.WatchdogStaleSeconds.Value.ToString(CultureInfo.InvariantCulture)}`");
            }
        }

        private void AppendAddressablesLocalModMarkdown(StringBuilder builder)
        {
            if (TryGetAddressablesLocalModFinding(out AddressablesInvalidPathFinding finding))
            {
                builder.AppendLine($"- Local mod ID: `{finding.ModId}`");
                builder.AppendLine($"- Bundle relative path: `{finding.BundlePath}`");
                builder.AppendLine($"- Local mod folder hint: `{finding.LocalModFolderHint}`");
                return;
            }

            if (TryGetAddressablesCatalogCorruptFinding(out AddressablesCatalogCorruptFinding catalogFinding))
            {
                builder.AppendLine($"- Local mod ID: `{catalogFinding.ModId}`");
                builder.AppendLine($"- Catalog relative path: `{catalogFinding.CatalogRelativePath}`");
                builder.AppendLine($"- Local mod folder hint: `{catalogFinding.LocalModFolderHint}`");
            }
        }

        private static void AppendOrderedList(StringBuilder builder, IEnumerable<string> items)
        {
            builder.AppendLine("          <ol class=\"support-list\">");
            foreach (string item in items)
            {
                builder.AppendLine($"            <li>{Html(item)}</li>");
            }

            builder.AppendLine("          </ol>");
        }

        private static void AppendPre(StringBuilder builder, string title, string text)
        {
            builder.AppendLine("          <div class=\"pre-block\">");
            builder.AppendLine($"            <h3>{Html(title)}</h3>");
            builder.AppendLine($"            <pre>{Html(text)}</pre>");
            builder.AppendLine("          </div>");
        }

        private void AppendSuspects(StringBuilder builder, string[] suspects)
        {
            builder.AppendLine("          <div class=\"table-wrap\">");
            builder.AppendLine("            <h3>Mod suspect evidence</h3>");
            if (suspects.Length == 0)
            {
                builder.AppendLine("            <p class=\"empty\">No high-confidence mod assembly suspect was identified from v0.1 evidence.</p>");
            }
            else
            {
                builder.AppendLine("            <table><thead><tr><th>Evidence</th></tr></thead><tbody>");
                foreach (string suspect in suspects)
                {
                    builder.AppendLine($"              <tr><td>{Html(suspect)}</td></tr>");
                }

                builder.AppendLine("            </tbody></table>");
            }

            builder.AppendLine("          </div>");
        }

        private void AppendPluginTable(StringBuilder builder)
        {
            builder.AppendLine("          <div class=\"table-wrap\">");
            if (Plugins.Length == 0)
            {
                builder.AppendLine("            <p class=\"empty\">No loaded BepInEx plugin records were captured.</p>");
            }
            else
            {
                builder.AppendLine("            <table><thead><tr><th>DLL</th><th>Name</th><th>GUID</th><th>Plugin version</th><th>Assembly</th><th>Assembly version</th><th>File version</th><th>File size</th><th>Last write UTC</th><th>SHA-256</th><th>Flags</th></tr></thead><tbody>");
                foreach (PluginRecord plugin in Plugins.OrderBy(item => item.Dll, StringComparer.OrdinalIgnoreCase))
                {
                    builder.AppendLine(
                        $"              <tr><td>{Html(plugin.Dll)}</td><td>{Html(plugin.Name)}</td><td>{Html(plugin.Guid)}</td><td>{Html(plugin.Version)}</td><td>{Html(plugin.AssemblyFullName)}</td><td>{Html(plugin.AssemblyVersion)}</td><td>{Html(plugin.FileVersion)}</td><td>{Html(FormatBytes(plugin.FileSizeBytes))}</td><td>{Html(plugin.LastWriteUtc?.ToString("o", CultureInfo.InvariantCulture) ?? string.Empty)}</td><td>{Html(string.IsNullOrWhiteSpace(plugin.Sha256) ? "disabled" : plugin.Sha256)}</td><td>{Html(BuildPluginFlags(plugin))}</td></tr>");
                }

                builder.AppendLine("            </tbody></table>");
            }

            builder.AppendLine("          </div>");
        }

        private void AppendBepInExDependencyErrorTable(StringBuilder builder)
        {
            builder.AppendLine("          <div class=\"table-wrap\">");
            builder.AppendLine("            <h3>BepInEx dependency errors</h3>");
            if (BepInExDependencyErrors.Length == 0)
            {
                builder.AppendLine("            <p class=\"empty\">No structured BepInEx Chainloader.DependencyErrors rows were captured.</p>");
            }
            else
            {
                builder.AppendLine("            <table><thead><tr><th>Plugin GUID</th><th>Dependent plugin</th><th>Version</th><th>Missing dependency</th><th>Message</th><th>Status</th><th>Source</th></tr></thead><tbody>");
                foreach (BepInExDependencyErrorFinding finding in BepInExDependencyErrors)
                {
                    builder.AppendLine($"              <tr><td>{Html(finding.PluginGuid)}</td><td>{Html(finding.DependentPluginName)}</td><td>{Html(finding.DependentPluginVersion)}</td><td>{Html(finding.MissingDependencyGuid)}</td><td>{Html(finding.Message)}</td><td>{Html(finding.Status)}</td><td>{Html(finding.Source)}</td></tr>");
                }

                builder.AppendLine("            </tbody></table>");
            }

            builder.AppendLine("          </div>");
        }

        private void AppendDependencyTable(StringBuilder builder)
        {
            builder.AppendLine("          <div class=\"table-wrap\">");
            builder.AppendLine("            <h3>Dependency and load findings</h3>");
            if (DependencyFindings.Length == 0)
            {
                builder.AppendLine("            <p class=\"empty\">No dependency, missing, failed-load, or incompatibility lines were found in bounded log excerpts.</p>");
            }
            else
            {
                builder.AppendLine("            <table><thead><tr><th>Source</th><th>Line</th></tr></thead><tbody>");
                foreach (DependencyFinding finding in DependencyFindings)
                {
                    builder.AppendLine($"              <tr><td>{Html(finding.Source)}</td><td>{Html(finding.Line)}</td></tr>");
                }

                builder.AppendLine("            </tbody></table>");
            }

            builder.AppendLine("          </div>");
        }

        private void AppendDependencyApiTable(StringBuilder builder)
        {
            builder.AppendLine("          <div class=\"table-wrap\">");
            builder.AppendLine("            <h3>Dependency/API probes</h3>");
            if (DependencyApiFindings.Length == 0)
            {
                builder.AppendLine("            <p class=\"empty\">No missing type, missing member, or dependency API probe evidence was extracted from the captured incident.</p>");
            }
            else
            {
                builder.AppendLine("            <table><thead><tr><th>Kind</th><th>Requested assembly</th><th>Requested symbol</th><th>Requesting member</th><th>Status</th><th>Loaded assembly</th><th>Evidence</th></tr></thead><tbody>");
                foreach (DependencyApiFinding finding in DependencyApiFindings)
                {
                    builder.AppendLine($"              <tr><td>{Html(finding.Kind)}</td><td>{Html(finding.RequestedAssembly)}</td><td>{Html(finding.RequestedSymbol)}</td><td>{Html(finding.RequestingMember)}</td><td>{Html(finding.Status)}</td><td>{Html(finding.LoadedAssembly)}</td><td>{Html(finding.Evidence)}</td></tr>");
                }

                builder.AppendLine("            </tbody></table>");
            }

            builder.AppendLine("          </div>");
        }

        private void AppendHarmonyPatchTable(StringBuilder builder)
        {
            builder.AppendLine("          <div class=\"table-wrap\">");
            builder.AppendLine("            <h3>Harmony patch-chain findings</h3>");
            if (HarmonyPatchFindings.Length == 0)
            {
                builder.AppendLine("            <p class=\"empty\">No Harmony patch-chain evidence was extracted from the captured incident.</p>");
            }
            else
            {
                builder.AppendLine("            <table><thead><tr><th>Status</th><th>Original method</th><th>Patch kind</th><th>Owner</th><th>Priority</th><th>Before</th><th>After</th><th>Patch method</th><th>Evidence</th></tr></thead><tbody>");
                foreach (HarmonyPatchFinding finding in HarmonyPatchFindings)
                {
                    builder.AppendLine($"              <tr><td>{Html(finding.Status)}</td><td>{Html(finding.OriginalMethod)}</td><td>{Html(finding.PatchKind)}</td><td>{Html(finding.Owner)}</td><td>{Html(finding.Priority)}</td><td>{Html(finding.Before)}</td><td>{Html(finding.After)}</td><td>{Html(finding.PatchMethod)}</td><td>{Html(finding.Evidence)}</td></tr>");
                }

                builder.AppendLine("            </tbody></table>");
            }

            builder.AppendLine("          </div>");
        }

        private void AppendLocalModPreflightTable(StringBuilder builder)
        {
            builder.AppendLine("          <div class=\"table-wrap\">");
            builder.AppendLine("            <h3>Local mod preflight</h3>");
            if (LocalModPreflightRows.Length == 0)
            {
                builder.AppendLine("            <p class=\"empty\">No local mod folders were found or the local Mods folder was unavailable.</p>");
            }
            else
            {
                builder.AppendLine("            <table><thead><tr><th>Local mod</th><th>Catalog</th><th>Status</th><th>Bundles</th><th>Missing bundles</th><th>Invalid base64 fields</th><th>Evidence</th></tr></thead><tbody>");
                foreach (LocalModPreflightRow row in LocalModPreflightRows)
                {
                    builder.AppendLine($"              <tr><td>{Html(row.LocalModId)}</td><td>{Html(row.CatalogRelativePath)}</td><td>{Html(row.Status)}</td><td>{Html(row.ReferencedBundleCount.ToString(CultureInfo.InvariantCulture))}</td><td>{Html(row.MissingBundleCount.ToString(CultureInfo.InvariantCulture))}</td><td>{Html(row.InvalidBase64FieldCount.ToString(CultureInfo.InvariantCulture))}</td><td>{Html(row.Evidence)}</td></tr>");
                }

                builder.AppendLine("            </tbody></table>");
            }

            builder.AppendLine("          </div>");
        }

        private void AppendBreadcrumbTable(StringBuilder builder)
        {
            builder.AppendLine("          <div class=\"table-wrap\">");
            builder.AppendLine("            <h3>Recent breadcrumbs</h3>");
            if (Breadcrumbs.Length == 0)
            {
                builder.AppendLine("            <p class=\"empty\">No breadcrumbs were captured.</p>");
            }
            else
            {
                builder.AppendLine("            <table><thead><tr><th>UTC time</th><th>Category</th><th>Event</th><th>Detail</th></tr></thead><tbody>");
                foreach (Breadcrumb breadcrumb in Breadcrumbs.OrderByDescending(item => item.TimestampUtc).Take(50))
                {
                    builder.AppendLine($"              <tr><td>{Html(breadcrumb.TimestampUtc.ToString("u", CultureInfo.InvariantCulture))}</td><td>{Html(breadcrumb.Category)}</td><td>{Html(breadcrumb.EventName)}</td><td>{Html(breadcrumb.Detail)}</td></tr>");
                }

                builder.AppendLine("            </tbody></table>");
            }

            builder.AppendLine("          </div>");
        }

        private static void AppendLogExcerpt(StringBuilder builder, string title, LogExcerpt excerpt)
        {
            StringBuilder text = new StringBuilder();
            text.AppendLine($"available={excerpt.Available}");
            text.AppendLine($"source={excerpt.SourceLabel}");
            text.AppendLine($"note={excerpt.Note}");
            text.AppendLine();
            foreach (string line in excerpt.Lines)
            {
                text.AppendLine(line);
            }

            AppendPre(builder, title, text.ToString());
        }

        private static string FirstNonEmptyLine(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            foreach (string line in value.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None))
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    return line.Trim();
                }
            }

            return string.Empty;
        }

        private static string LimitLines(string value, int maxLines)
        {
            if (string.IsNullOrWhiteSpace(value) || maxLines <= 0)
            {
                return string.Empty;
            }

            string[] lines = value.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            if (lines.Length <= maxLines)
            {
                return value;
            }

            return string.Join(Environment.NewLine, lines.Take(maxLines))
                + Environment.NewLine
                + $"...truncated {lines.Length - maxLines} additional lines; open report.html or incident.json for the full saved evidence.";
        }

        private static string FormatPluginBytes(long bytes)
        {
            return bytes <= 0
                ? "unavailable"
                : bytes.ToString(CultureInfo.InvariantCulture);
        }

        private static string BuildConfidenceLabel(string[] suspects)
        {
            if (suspects.Any(item => item.IndexOf("high confidence", StringComparison.OrdinalIgnoreCase) >= 0))
            {
                return "High";
            }

            if (suspects.Any(item => item.IndexOf("medium confidence", StringComparison.OrdinalIgnoreCase) >= 0))
            {
                return "Medium";
            }

            if (suspects.Any(item => item.IndexOf("low confidence", StringComparison.OrdinalIgnoreCase) >= 0))
            {
                return "Low";
            }

            return "Unknown";
        }

        private static string BuildPluginFlags(PluginRecord plugin)
        {
            List<string> flags = new List<string>();
            if (string.IsNullOrWhiteSpace(plugin.Dll))
            {
                flags.Add("missing dll filename");
            }

            if (string.IsNullOrWhiteSpace(plugin.Version))
            {
                flags.Add("missing version");
            }

            if (string.IsNullOrWhiteSpace(plugin.AssemblyFullName))
            {
                flags.Add("missing assembly name");
            }

            if (string.IsNullOrWhiteSpace(plugin.FileVersion))
            {
                flags.Add("missing file version");
            }

            if (plugin.FileSizeBytes <= 0)
            {
                flags.Add("missing file size");
            }

            if (!plugin.LastWriteUtc.HasValue)
            {
                flags.Add("missing last write time");
            }

            if (string.IsNullOrWhiteSpace(plugin.Guid))
            {
                flags.Add("missing guid");
            }

            return flags.Count == 0 ? "none" : string.Join("; ", flags);
        }

        private static string FormatBytes(long bytes)
        {
            return bytes <= 0
                ? string.Empty
                : bytes.ToString(CultureInfo.InvariantCulture);
        }

        private string[] BuildSuspectRows()
        {
            List<string> suspects = new List<string>();
            string stack = Pending.StackTrace ?? string.Empty;
            bool possibleHang = string.Equals(Pending.Kind, "possible_hang_or_freeze", StringComparison.OrdinalIgnoreCase);
            bool loaderDependencyIncident = string.Equals(Pending.Kind, "bepinex_loader_dependency_error", StringComparison.OrdinalIgnoreCase);

            if (!possibleHang)
            {
                foreach (string apiMismatchSuspect in BuildDependencyApiMismatchSuspectRows().Take(5))
                {
                    suspects.Add(apiMismatchSuspect);
                }
            }

            foreach (PluginRecord plugin in Plugins)
            {
                if (!string.IsNullOrWhiteSpace(plugin.Dll) && stack.IndexOf(Path.GetFileNameWithoutExtension(plugin.Dll), StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    suspects.Add($"{plugin.Name} ({plugin.Guid}) - high confidence: assembly appears in managed stack");
                }
            }

            if (!possibleHang)
            {
                foreach (BepInExDependencyErrorFinding finding in BepInExDependencyErrors.Take(5))
                {
                    if (loaderDependencyIncident || DependencyErrorMatchesPendingEvidence(finding))
                    {
                        suspects.Add(finding.ToSuspectRow());
                    }
                }

                foreach (DependencyApiFinding finding in DependencyApiFindings.Take(5))
                {
                    if (string.Equals(finding.Status, "dependency-present-api-missing", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(finding.Status, "dependency-assembly-not-loaded", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(finding.Status, "dependency-present-declaring-type-missing", StringComparison.OrdinalIgnoreCase))
                    {
                        suspects.Add($"dependency/API probe - high confidence: {finding.Status}; {finding.RequestedAssembly}; {finding.RequestedSymbol}");
                    }
                }

                foreach (HarmonyPatchFinding finding in HarmonyPatchFindings.Take(10))
                {
                    if (!string.Equals(finding.Status, "patch-row", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(finding.PatchMethod)
                        && stack.IndexOf(finding.PatchMethod, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        suspects.Add($"Harmony patch-chain - high confidence: {finding.PatchKind}; owner={finding.Owner}; patch={finding.PatchMethod}; original={finding.OriginalMethod}");
                    }
                    else if (string.Equals(finding.PatchKind, "transpiler", StringComparison.OrdinalIgnoreCase))
                    {
                        suspects.Add($"Harmony patch-chain - medium confidence: same-target transpiler owner={finding.Owner}; patch={finding.PatchMethod}; original={finding.OriginalMethod}");
                    }
                }

                foreach (DependencyFinding finding in DependencyFindings.Take(5))
                {
                    suspects.Add($"dependency/log finding - medium confidence: {finding.Line}");
                }
            }

            return suspects.Distinct(StringComparer.OrdinalIgnoreCase).Take(10).ToArray();
        }

        private IEnumerable<string> BuildDependencyApiMismatchSuspectRows()
        {
            foreach (DependencyApiFinding finding in DependencyApiFindings)
            {
                if (!IsMissingDependencyApiFinding(finding))
                {
                    continue;
                }

                string consumer = HumanizeAssemblyName(ExtractRequestingAssemblyName(finding.RequestingMember));
                string provider = HumanizeAssemblyName(finding.RequestedAssembly);
                if (string.IsNullOrWhiteSpace(consumer) || string.IsNullOrWhiteSpace(provider))
                {
                    continue;
                }

                string missingApi = string.IsNullOrWhiteSpace(finding.RequestedSymbol)
                    ? string.Empty
                    : $" Missing API: {finding.RequestedSymbol}.";
                yield return $"Live DLL mismatch - high confidence: {consumer} and {provider} live DLLs do not match; redeploy a matching pair.{missingApi}";
            }
        }

        private bool DependencyErrorMatchesPendingEvidence(BepInExDependencyErrorFinding finding)
        {
            string text = string.Join("\n", new[]
            {
                Pending.Title ?? string.Empty,
                Pending.Message ?? string.Empty,
                Pending.StackTrace ?? string.Empty
            });
            return ContainsToken(text, finding.PluginGuid)
                || ContainsToken(text, finding.DependentPluginName)
                || ContainsToken(text, finding.MissingDependencyGuid);
        }

        private static bool IsMissingDependencyApiFinding(DependencyApiFinding finding)
        {
            return string.Equals(finding.Status, "dependency-present-api-missing", StringComparison.OrdinalIgnoreCase)
                || string.Equals(finding.Status, "dependency-assembly-not-loaded", StringComparison.OrdinalIgnoreCase)
                || string.Equals(finding.Status, "dependency-present-declaring-type-missing", StringComparison.OrdinalIgnoreCase);
        }

        private static bool ContainsToken(string text, string token)
        {
            return !string.IsNullOrWhiteSpace(text)
                && !string.IsNullOrWhiteSpace(token)
                && text.IndexOf(token.Trim(), StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string ExtractRequestingAssemblyName(string requestingMember)
        {
            if (string.IsNullOrWhiteSpace(requestingMember))
            {
                return string.Empty;
            }

            string value = requestingMember.Trim();
            int dot = value.IndexOf('.');
            int colon = value.IndexOf(':');
            int end = dot >= 0 && colon >= 0
                ? Math.Min(dot, colon)
                : Math.Max(dot, colon);
            return end > 0 ? value.Substring(0, end) : value;
        }

        private static string HumanizeAssemblyName(string assemblyName)
        {
            string value = AssemblySimpleName(assemblyName);
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            value = Regex.Replace(value, @"(?<=[a-z0-9])(?=[A-Z])", " ", RegexOptions.CultureInvariant);
            value = Regex.Replace(value, @"[_\-]+", " ", RegexOptions.CultureInvariant);
            return Regex.Replace(value.Trim(), @"\s+", " ", RegexOptions.CultureInvariant);
        }

        private static string AssemblySimpleName(string assemblyName)
        {
            string value = (assemblyName ?? string.Empty).Trim();
            int comma = value.IndexOf(',');
            return comma >= 0 ? value.Substring(0, comma).Trim() : value;
        }

        private IncidentClassification BuildClassification(string[] suspects)
        {
            AddressablesCatalogCorruptFinding? catalogFinding = TryGetAddressablesCatalogCorruptFinding(out AddressablesCatalogCorruptFinding finding)
                ? finding
                : null;
            return IncidentClassifier.Classify(Pending.Kind, Pending.Title, Pending.Message, Pending.StackTrace, suspects, catalogFinding);
        }

        private bool TryGetAddressablesLocalModFinding(out AddressablesInvalidPathFinding finding)
        {
            IncidentQueueEntry pending = new IncidentQueueEntry(
                Pending.Kind,
                Pending.Title,
                Pending.Message,
                Pending.StackTrace,
                Pending.DuplicateCount);
            return AddressablesInvalidPathEvidence.TryExtract(pending.Text, out finding);
        }

        private bool TryGetAddressablesCatalogCorruptFinding(out AddressablesCatalogCorruptFinding finding)
        {
            IncidentQueueEntry pending = new IncidentQueueEntry(
                Pending.Kind,
                Pending.Title,
                Pending.Message,
                Pending.StackTrace,
                Pending.DuplicateCount);
            return AddressablesCatalogCorruptEvidence.TryBuildFinding(
                pending.Text,
                BuildCatalogBearingLocalModIds(),
                out finding);
        }

        private static string[] BuildCatalogBearingLocalModIds()
        {
            string modsPath = GetLocalModsPath();
            if (string.IsNullOrWhiteSpace(modsPath) || !Directory.Exists(modsPath))
            {
                return Array.Empty<string>();
            }

            try
            {
                List<string> candidates = new List<string>();
                foreach (string directory in Directory.GetDirectories(modsPath))
                {
                    if (!File.Exists(Path.Combine(directory, "catalog.json")))
                    {
                        continue;
                    }

                    string? modId = Path.GetFileName(directory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
                    if (!string.IsNullOrWhiteSpace(modId)
                        && !candidates.Contains(modId, StringComparer.OrdinalIgnoreCase))
                    {
                        candidates.Add(modId);
                    }

                    if (candidates.Count > 1)
                    {
                        break;
                    }
                }

                return candidates.ToArray();
            }
            catch
            {
                return Array.Empty<string>();
            }
        }

        private static string BuildReportHeadline(IncidentClassification classification)
        {
            if (string.Equals(classification.RouteId, "addressables-assetbundle-invalid-path", StringComparison.OrdinalIgnoreCase)
                || string.Equals(classification.RouteId, AddressablesCatalogCorruptEvidence.RouteId, StringComparison.OrdinalIgnoreCase)
                || string.Equals(classification.RouteId, "bepinex-loader-dependency-error", StringComparison.OrdinalIgnoreCase)
                || string.Equals(classification.RouteId, "managed-dependency-mismatch", StringComparison.OrdinalIgnoreCase)
                || string.Equals(classification.RouteId, ManagedDependencyCascadeEvidence.FollowOnRouteId, StringComparison.OrdinalIgnoreCase))
            {
                return classification.RouteLabel;
            }

            return PluginName;
        }

        private static string BuildWindowTitle(string summary, IncidentClassification classification)
        {
            if (string.Equals(classification.RouteId, "addressables-assetbundle-invalid-path", StringComparison.OrdinalIgnoreCase)
                || string.Equals(classification.RouteId, AddressablesCatalogCorruptEvidence.RouteId, StringComparison.OrdinalIgnoreCase)
                || string.Equals(classification.RouteId, "bepinex-loader-dependency-error", StringComparison.OrdinalIgnoreCase)
                || string.Equals(classification.RouteId, "managed-dependency-mismatch", StringComparison.OrdinalIgnoreCase)
                || string.Equals(classification.RouteId, ManagedDependencyCascadeEvidence.FollowOnRouteId, StringComparison.OrdinalIgnoreCase))
            {
                return classification.RouteLabel;
            }

            return string.IsNullOrWhiteSpace(summary) ? $"{PluginName} incident" : summary;
        }
    }

    private readonly struct SessionSummary
    {
        internal SessionSummary(
            string sessionId,
            DateTime generatedAtUtc,
            int incidentCount,
            int groupedOccurrenceCount,
            IncidentSummaryEntry primary,
            IncidentSummaryEntry[] followOns,
            int recentLaunchesScanned,
            int seenAcrossRecentLaunches)
        {
            SessionId = sessionId ?? string.Empty;
            GeneratedAtUtc = generatedAtUtc;
            IncidentCount = incidentCount;
            GroupedOccurrenceCount = groupedOccurrenceCount;
            Primary = primary;
            FollowOns = followOns ?? Array.Empty<IncidentSummaryEntry>();
            RecentLaunchesScanned = recentLaunchesScanned;
            SeenAcrossRecentLaunches = seenAcrossRecentLaunches;
        }

        private string SessionId { get; }
        private DateTime GeneratedAtUtc { get; }
        private int IncidentCount { get; }
        private int GroupedOccurrenceCount { get; }
        private IncidentSummaryEntry Primary { get; }
        private IncidentSummaryEntry[] FollowOns { get; }
        private int RecentLaunchesScanned { get; }
        private int SeenAcrossRecentLaunches { get; }

        internal string ToJson()
        {
            JsonWriter writer = new JsonWriter();
            writer.BeginObject();
            writer.Property("schemaVersion", "0.1.0");
            writer.Property("pluginName", PluginName);
            writer.Property("pluginGuid", PluginGuid);
            writer.Property("pluginVersion", PluginVersion);
            writer.Property("sessionId", SessionId);
            writer.Property("generatedAtUtc", GeneratedAtUtc.ToString("o", CultureInfo.InvariantCulture));
            writer.Property("incidentCount", IncidentCount);
            writer.Property("groupedOccurrenceCount", GroupedOccurrenceCount);
            writer.Property("primaryIncidentId", Primary.IncidentId);
            writer.Property("primaryIncidentFolder", Primary.IncidentFolderName);
            writer.Property("primaryRouteId", Primary.RouteId);
            writer.Property("primaryRouteLabel", Primary.RouteLabel);
            writer.Property("primaryLikelyCause", Primary.LikelyCause);
            writer.Property("fixFirst", Primary.RecommendedAction);
            writer.Property("primaryDuplicateCount", Primary.DuplicateCount);
            writer.Property("localModId", Primary.LocalModId);
            writer.Property("bundleRelativePath", Primary.BundleRelativePath);
            writer.Property("catalogRelativePath", Primary.CatalogRelativePath);
            writer.Property("followOnCount", FollowOns.Length);
            writer.ArrayProperty("followOnIncidentIds", FollowOns.Select(entry => entry.IncidentId));
            writer.ArrayProperty("followOnRouteLabels", FollowOns.Select(entry => entry.RouteLabel));
            writer.Property("recentLaunchesScanned", RecentLaunchesScanned);
            writer.Property("seenAcrossRecentLaunches", SeenAcrossRecentLaunches);
            writer.Property("seenAcrossRecentLaunchesLabel", BuildSeenAcrossRecentLaunchesLabel());
            writer.Property("bestNextCheck", Primary.BestNextCheck);
            writer.Property("evidenceBoundary", Primary.DoesNotProve);
            writer.EndObject();
            return writer.ToString();
        }

        internal string ToMarkdown()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("**Session Summary**");
            builder.AppendLine($"- Session: `{SessionId}`");
            builder.AppendLine($"- Primary incident: `{Primary.IncidentId}` / {Primary.RouteLabel}");
            builder.AppendLine($"- Fix first: {Primary.RecommendedAction}");
            builder.AppendLine($"- Likely cause: {Primary.LikelyCause}");
            builder.AppendLine($"- Follow-ons: `{FollowOns.Length.ToString(CultureInfo.InvariantCulture)}`");
            if (FollowOns.Length > 0)
            {
                builder.AppendLine($"- Follow-on incidents: {string.Join("; ", FollowOns.Select(entry => $"`{entry.IncidentId}` ({entry.RouteLabel})"))}");
            }

            builder.AppendLine($"- Repeated count: primary seen `{Primary.DuplicateCount.ToString(CultureInfo.InvariantCulture)}` time(s); session grouped occurrences `{GroupedOccurrenceCount.ToString(CultureInfo.InvariantCulture)}`");
            builder.AppendLine($"- Seen across recent launches: {BuildSeenAcrossRecentLaunchesLabel()}");
            builder.AppendLine($"- Best next check: {Primary.BestNextCheck}");
            builder.AppendLine($"- Evidence boundary: {Primary.DoesNotProve}");
            return builder.ToString();
        }

        private string BuildSeenAcrossRecentLaunchesLabel()
        {
            return RecentLaunchesScanned <= 0
                ? "no recent launch index available"
                : $"seen across {SeenAcrossRecentLaunches.ToString(CultureInfo.InvariantCulture)} of the last {RecentLaunchesScanned.ToString(CultureInfo.InvariantCulture)} launch(es)";
        }
    }

    private readonly struct IncidentSummaryEntry
    {
        private IncidentSummaryEntry(
            string incidentFolderName,
            string incidentId,
            string sessionId,
            DateTime capturedAtUtc,
            string routeId,
            string routeLabel,
            string likelyCause,
            string recommendedAction,
            string bestNextCheck,
            string doesNotProve,
            string localModId,
            string bundleRelativePath,
            string catalogRelativePath,
            int duplicateCount)
        {
            IncidentFolderName = incidentFolderName ?? string.Empty;
            IncidentId = incidentId ?? string.Empty;
            SessionId = sessionId ?? string.Empty;
            CapturedAtUtc = capturedAtUtc;
            RouteId = routeId ?? string.Empty;
            RouteLabel = routeLabel ?? string.Empty;
            LikelyCause = likelyCause ?? string.Empty;
            RecommendedAction = recommendedAction ?? string.Empty;
            BestNextCheck = bestNextCheck ?? string.Empty;
            DoesNotProve = doesNotProve ?? string.Empty;
            LocalModId = localModId ?? string.Empty;
            BundleRelativePath = bundleRelativePath ?? string.Empty;
            CatalogRelativePath = catalogRelativePath ?? string.Empty;
            DuplicateCount = Math.Max(1, duplicateCount);
            Signature = BuildSignature(RouteId, LikelyCause, LocalModId, BundleRelativePath, CatalogRelativePath);
        }

        internal string IncidentFolderName { get; }
        internal string IncidentId { get; }
        internal string SessionId { get; }
        internal DateTime CapturedAtUtc { get; }
        internal string RouteId { get; }
        internal string RouteLabel { get; }
        internal string LikelyCause { get; }
        internal string RecommendedAction { get; }
        internal string BestNextCheck { get; }
        internal string DoesNotProve { get; }
        internal string LocalModId { get; }
        internal string BundleRelativePath { get; }
        internal string CatalogRelativePath { get; }
        internal int DuplicateCount { get; }
        internal string Signature { get; }

        internal static bool TryRead(string incidentFolder, string json, out IncidentSummaryEntry entry)
        {
            string incidentId = ReadJsonString(json, "incidentId");
            string sessionId = ReadJsonString(json, "sessionId");
            string routeId = ReadJsonString(json, "routeId");
            if (string.IsNullOrWhiteSpace(incidentId)
                || string.IsNullOrWhiteSpace(sessionId)
                || string.IsNullOrWhiteSpace(routeId))
            {
                entry = default;
                return false;
            }

            DateTime capturedAtUtc = DateTime.MinValue;
            DateTime.TryParse(
                ReadJsonString(json, "capturedAtUtc"),
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out capturedAtUtc);

            entry = new IncidentSummaryEntry(
                Path.GetFileName((incidentFolder ?? string.Empty).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)) ?? string.Empty,
                incidentId,
                sessionId,
                capturedAtUtc,
                routeId,
                ReadJsonString(json, "routeLabel"),
                ReadJsonString(json, "likelyCause"),
                ReadJsonString(json, "recommendedAction"),
                ReadJsonString(json, "bestNextCheck"),
                ReadJsonString(json, "doesNotProve"),
                ReadJsonString(json, "localModId"),
                ReadJsonString(json, "bundleRelativePath"),
                ReadJsonString(json, "catalogRelativePath"),
                ReadJsonInt(json, "duplicateCount"));
            return true;
        }

        private static string BuildSignature(string routeId, string likelyCause, string localModId, string bundleRelativePath, string catalogRelativePath)
        {
            return string.Join(
                "|",
                Normalize(routeId),
                Normalize(localModId),
                Normalize(bundleRelativePath),
                Normalize(catalogRelativePath),
                Normalize(likelyCause));
        }

        private static string Normalize(string value)
        {
            return Regex.Replace((value ?? string.Empty).Trim().ToLowerInvariant(), @"\s+", " ", RegexOptions.CultureInvariant);
        }
    }

    private sealed class JsonWriter
    {
        private readonly StringBuilder _builder = new StringBuilder();
        private readonly Stack<bool> _firstStack = new Stack<bool>();
        private bool _first = true;

        internal void BeginObject()
        {
            _builder.Append('{');
            _firstStack.Push(_first);
            _first = true;
        }

        internal void EndObject()
        {
            _builder.AppendLine();
            _builder.Append('}');
            _first = _firstStack.Count > 0 ? _firstStack.Pop() : false;
        }

        internal void Property(string name, string value)
        {
            WritePrefix(name);
            _builder.Append('"').Append(Json(value)).Append('"');
        }

        internal void Property(string name, bool value)
        {
            WritePrefix(name);
            _builder.Append(value ? "true" : "false");
        }

        internal void Property(string name, int value)
        {
            WritePrefix(name);
            _builder.Append(value.ToString(CultureInfo.InvariantCulture));
        }

        internal void ArrayProperty(string name, IEnumerable<string> values)
        {
            WritePrefix(name);
            _builder.Append('[');
            bool first = true;
            foreach (string value in values)
            {
                if (!first)
                {
                    _builder.Append(',');
                }

                _builder.Append('"').Append(Json(value)).Append('"');
                first = false;
            }

            _builder.Append(']');
        }

        public override string ToString()
        {
            return _builder.ToString();
        }

        private void WritePrefix(string name)
        {
            if (!_first)
            {
                _builder.Append(',');
            }

            _builder.AppendLine();
            _builder.Append("  \"").Append(Json(name)).Append("\": ");
            _first = false;
        }
    }

    private static string SafeApplicationValue(Func<string> getter)
    {
        try
        {
            return getter() ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string ReadJsonString(string json, string key)
    {
        Match match = Regex.Match(json, $"\"{Regex.Escape(key)}\"\\s*:\\s*\"(?<value>(?:\\\\.|[^\"])*)\"", RegexOptions.CultureInvariant);
        return match.Success ? match.Groups["value"].Value.Replace("\\\"", "\"") : string.Empty;
    }

    private static bool ReadJsonBool(string json, string key)
    {
        Match match = Regex.Match(json, $"\"{Regex.Escape(key)}\"\\s*:\\s*(?<value>true|false)", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
        return match.Success && bool.TryParse(match.Groups["value"].Value, out bool value) && value;
    }

    private static int ReadJsonInt(string json, string key)
    {
        Match match = Regex.Match(json, $"\"{Regex.Escape(key)}\"\\s*:\\s*(?<value>-?\\d+)", RegexOptions.CultureInvariant);
        return match.Success && int.TryParse(match.Groups["value"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value)
            ? value
            : 0;
    }

    private static string Json(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        StringBuilder builder = new StringBuilder(value.Length + 16);
        foreach (char ch in value)
        {
            switch (ch)
            {
                case '\\':
                    builder.Append("\\\\");
                    break;
                case '"':
                    builder.Append("\\\"");
                    break;
                case '\r':
                    builder.Append("\\r");
                    break;
                case '\n':
                    builder.Append("\\n");
                    break;
                case '\t':
                    builder.Append("\\t");
                    break;
                default:
                    if (char.IsControl(ch))
                    {
                        builder.Append("\\u").Append(((int)ch).ToString("x4", CultureInfo.InvariantCulture));
                    }
                    else
                    {
                        builder.Append(ch);
                    }

                    break;
            }
        }

        return builder.ToString();
    }

    private static string Html(string value)
    {
        return (value ?? string.Empty)
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;");
    }
}

internal static class NativeMessageBox
{
    private const uint Ok = 0x00000000;
    private const uint IconWarning = 0x00000030;
    private const uint SetForeground = 0x00010000;
    private const uint TopMost = 0x00040000;

    internal static void Show(string title, string message)
    {
        if (Environment.OSVersion.Platform != PlatformID.Win32NT)
        {
            return;
        }

        MessageBox(IntPtr.Zero, message, title, Ok | IconWarning | SetForeground | TopMost);
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);
}

internal static class CsvExtensions
{
    internal static void AppendCsvRow(this StringBuilder builder, params string[] values)
    {
        for (int i = 0; i < values.Length; i++)
        {
            if (i > 0)
            {
                builder.Append(',');
            }

            builder.Append(Csv(values[i]));
        }

        builder.AppendLine();
    }

    private static string Csv(string value)
    {
        value ??= string.Empty;
        bool quote = value.IndexOfAny(new[] { ',', '"', '\r', '\n' }) >= 0;
        if (!quote)
        {
            return value;
        }

        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }
}
