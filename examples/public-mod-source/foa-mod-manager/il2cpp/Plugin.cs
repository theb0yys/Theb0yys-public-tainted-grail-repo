using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using UnityEngine;

namespace FoAModManager;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BasePlugin
{
    public const string PluginGuid = "kane.tgfoa.mod-manager";
    public const string PluginName = "FoA Mod Manager";
    public const string PluginVersion = "0.7.6";

    private static readonly bool ForceIl2CppRuntimePatchQuarantine = true;
    private const float LifecycleLogPollIntervalSeconds = 2f;
    private const long MaxLifecycleReadBytes = 32768;
    private const int StatusProviderLineLimit = 12;
    private const int StatusProviderTextLimit = 360;

    private static readonly Dictionary<string, StatusProviderRegistration> StatusProviders =
        new(StringComparer.OrdinalIgnoreCase);

    private static readonly HashSet<string> ControllerCursorScopes = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, bool> CustomUiScopes = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, ControllerActionRegistration> ControllerActions =
        new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, string> SettingDraftValues =
        new(StringComparer.OrdinalIgnoreCase);
    private static readonly HashSet<string> SuppressedControllerActionNames =
        new(StringComparer.OrdinalIgnoreCase);
    private static readonly HashSet<int> SuppressedControllerActionIds = new();

    private static Plugin _instance;
    private static Harmony _harmony;
    private static ConfigEntry<KeyCode> _openHotkey;
    private static ConfigEntry<bool> _showOnStartup;
    private static ConfigEntry<bool> _runtimeQuarantine;
    private static ConfigEntry<bool> _allowConfigWritesAfterGameplayReady;
    private static ConfigEntry<bool> _autoArmGameplayReadyFromFoALog;
    private static ConfigEntry<bool> _freezeWorldForCustomUiScopes;
    private static ConfigEntry<bool> _enableMenuButtons;
    private static ConfigEntry<bool> _enableInputFreezePatches;
    private static ConfigEntry<bool> _controllerActionLayerEnabled;
    private static ConfigEntry<bool> _controllerActionLayerDispatchEnabled;
    private static ConfigEntry<KeyCode> _controllerActionLayerModifierKey;
    private static ConfigEntry<KeyCode> _controllerActionLayerTargetKey;
    private static Rect _windowRect = new(80f, 80f, 760f, 560f);
    private static Vector2 _scrollPosition;
    private static ConfigPluginSnapshot[] _configPlugins = Array.Empty<ConfigPluginSnapshot>();
    private static string _selectedConfigPluginKey = string.Empty;
    private static string _configCatalogStatus = "Config catalog has not been refreshed yet.";
    private static string _managerStatus = "Ready.";
    private static DateTime _configCatalogUpdatedUtc = DateTime.MinValue;
    private static bool _isManagerVisible;
    private static bool _cursorStateCaptured;
    private static bool _lastInputOwned;
    private static bool _menuPatchAttempted;
    private static bool _menuPatchApplied;
    private static bool _inputPatchAttempted;
    private static bool _inputPatchApplied;
    private static bool _manualGameplayReady;
    private static bool _lifecycleLoadStartedSeen;
    private static bool _lifecycleLoadingCompletedSeen;
    private static bool _lifecycleLoadingFullyDiscardedSeen;
    private static bool _wasCustomUiScopeActive;
    private static float _nextLifecycleLogPollAt;
    private static float _capturedTimeScale = 1f;
    private static long _lifecycleLogPosition;
    private static DateTime _pluginLoadedUtc;
    private static CursorLockMode _capturedCursorLockMode = CursorLockMode.None;
    private static bool _capturedCursorVisible = true;
    private static string _gameplayReadyGateSource = "blocked";
    private static string _lifecycleLogPath = string.Empty;
    private static string _lifecycleStatus = "FoA lifecycle log has not been scanned yet.";
    private static int _suppressedControllerActionFrame = -1;

    public override void Load()
    {
        _instance = this;
        _pluginLoadedUtc = DateTime.UtcNow;

        _openHotkey = Config.Bind(
            "General",
            "OpenHotkey",
            KeyCode.F10,
            "Hotkey used to open or close the IL2CPP FoA Mod Manager shell.");

        _showOnStartup = Config.Bind(
            "General",
            "ShowOnStartup",
            false,
            "Open the IL2CPP manager shell automatically after plugin load.");

        _runtimeQuarantine = Config.Bind(
            "Safety",
            "RuntimeQuarantine",
            true,
            "Disable IL2CPP Harmony menu/input patches, controller dispatch, and time-scale changes while save-load stability is under investigation.");

        _allowConfigWritesAfterGameplayReady = Config.Bind(
            "Safety",
            "AllowConfigWritesAfterGameplayReady",
            true,
            "Allow manager config saves only after the in-session gameplay-ready gate is armed from FoA log markers or manually after load. Runtime patches remain quarantined by this recovery build.");

        _autoArmGameplayReadyFromFoALog = Config.Bind(
            "Safety",
            "AutoArmGameplayReadyFromFoALog",
            true,
            "Read FoA's own LocalLow log and arm the gameplay-ready gate only after Loading: Completed and Loading: Fully discarded are both observed for a load transition.");

        _freezeWorldForCustomUiScopes = Config.Bind(
            "Custom UI",
            "FreezeWorldForCustomUiScopes",
            true,
            "Pause Unity time while any IL2CPP custom UI scope that requests world freeze is active.");

        _enableMenuButtons = Config.Bind(
            "Menu",
            "EnableMenuButtons",
            true,
            "Add a FoA Mod Manager button to supported IL2CPP title/pause menu screens when their UI types are available.");

        _enableInputFreezePatches = Config.Bind(
            "Input",
            "EnableInputFreezePatches",
            true,
            "Patch IL2CPP game/Rewired input reads so gameplay/menu input is frozen while FoA Mod Manager or a custom UI scope owns input.");

        _controllerActionLayerEnabled = Config.Bind(
            "ControllerActionLayer",
            "Enabled",
            false,
            "Enable the IL2CPP controller action layer. Default is off until live controller validation is confirmed on this branch.");

        _controllerActionLayerDispatchEnabled = Config.Bind(
            "ControllerActionLayer",
            "DispatchEnabled",
            true,
            "Allow the IL2CPP controller action layer to dispatch registered manager/mod actions when Enabled is true.");

        _controllerActionLayerModifierKey = Config.Bind(
            "ControllerActionLayer",
            "ModifierKey",
            KeyCode.JoystickButton9,
            "Controller modifier key for the IL2CPP action layer. Default is R3 / JoystickButton9.");

        _controllerActionLayerTargetKey = Config.Bind(
            "ControllerActionLayer",
            "ManagerToggleTargetKey",
            KeyCode.JoystickButton0,
            "Controller target key that toggles FoA Mod Manager while the modifier is held. Default is A / JoystickButton0.");

        RegisterBuiltInStatusProviders();
        RegisterBuiltInControllerActions();
        RegisterTaintedFrameworkStatusProvider();
        RefreshConfigCatalog();
        ApplyRuntimePatches();
        AddComponent<ManagerBehaviour>();

        _isManagerVisible = _showOnStartup.Value;
        Log.LogInfo(
            $"{PluginName} {PluginVersion} IL2CPP safe-functionality layer loaded. " +
            $"openHotkey={_openHotkey.Value}; " +
            "branch=IL2CPP; " +
            "bepInEx=BepInEx 6; " +
            "statusProviderSurface=True; " +
            $"runtimePatchQuarantine={IsRuntimeQuarantineEnabled()}; " +
            $"runtimePatchQuarantineForced={ForceIl2CppRuntimePatchQuarantine}; " +
            "configEditingPorted=True; " +
            $"configWritesArmed={AreConfigWritesAllowed()}; " +
            $"gameplayReadyGate={_manualGameplayReady}; " +
            $"gameplayReadyAutoLog={_autoArmGameplayReadyFromFoALog.Value}; " +
            "cursorOwnershipPorted=True; " +
            $"menuPatchesPorted={_menuPatchApplied}; " +
            $"inputFreezePorted={_inputPatchApplied}; " +
            $"controllerDispatchPorted={!IsRuntimeQuarantineEnabled()}; " +
            "taintedStatusAdapter=True.");
    }

    public override bool Unload()
    {
        _isManagerVisible = false;
        ControllerCursorScopes.Clear();
        CustomUiScopes.Clear();
        ControllerActions.Clear();
        StatusProviders.Clear();
        ClearGameplayReadyGate("Plugin unloading.", resetLifecycleMarkers: true);
        RestoreCustomUiTimeScaleIfNeeded(force: true);
        RestoreInputOwnershipState(force: true);
        _harmony?.UnpatchSelf();
        _harmony = null;
        _instance = null;
        return true;
    }

    internal static bool IsManagerVisible => _isManagerVisible;

    internal static bool IsControllerCursorActive =>
        _isManagerVisible || ControllerCursorScopes.Count > 0 || CustomUiScopes.Count > 0;

    internal static bool IsInternalControllerInputReadActive { get; private set; }

    internal static bool IsControllerCursorInputReadActive => IsInternalControllerInputReadActive;

    internal static bool IsModUiInputOwned => _isManagerVisible || CustomUiScopes.Count > 0;

    internal static bool IsCustomUiScopeActive => CustomUiScopes.Count > 0;

    internal static bool ShouldSuppressControllerActionLayerButton(string actionName)
    {
        if (IsInternalControllerInputReadActive || IsModUiInputOwned)
        {
            return false;
        }

        ClearExpiredControllerActionSuppression();
        return SuppressedControllerActionNames.Contains(CleanText(actionName, 160, string.Empty));
    }

    internal static bool ShouldSuppressControllerActionLayerButton(int actionId)
    {
        if (IsInternalControllerInputReadActive || IsModUiInputOwned)
        {
            return false;
        }

        ClearExpiredControllerActionSuppression();
        return SuppressedControllerActionIds.Contains(actionId);
    }

    internal static void ShowManager()
    {
        _isManagerVisible = true;
    }

    internal static void HideManager()
    {
        _isManagerVisible = false;
    }

    internal static void RefreshManager()
    {
        RefreshConfigCatalog();

        if (_instance != null)
        {
            _instance.Log.LogInfo(
                $"{PluginName} IL2CPP refresh requested. " +
                $"loadedPlugins={GetLoadedPluginCount()}; " +
                $"configPlugins={_configPlugins.Length}; " +
                $"configEntries={GetDiscoveredConfigEntryCount()}; " +
                $"statusProviders={StatusProviders.Count}; " +
                "configEditingPorted=True; " +
                $"configWritesArmed={AreConfigWritesAllowed()}.");
        }
    }

    internal static void SetControllerCursorScope(string ownerId, bool active)
    {
        string normalizedOwnerId = NormalizeOwnerId(ownerId);
        if (normalizedOwnerId.Length == 0)
        {
            return;
        }

        if (active)
        {
            ControllerCursorScopes.Add(normalizedOwnerId);
        }
        else
        {
            ControllerCursorScopes.Remove(normalizedOwnerId);
        }
    }

    internal static void SetCustomUiScope(string ownerId, bool active, bool freezeWorld)
    {
        string normalizedOwnerId = NormalizeOwnerId(ownerId);
        if (normalizedOwnerId.Length == 0)
        {
            return;
        }

        if (active)
        {
            CustomUiScopes[normalizedOwnerId] = freezeWorld;
        }
        else
        {
            CustomUiScopes.Remove(normalizedOwnerId);
        }

        if (!IsRuntimeQuarantineEnabled())
        {
            ApplyCustomUiTimeScale();
        }
    }

    internal static bool RegisterControllerAction(string actionId, string displayName, Action callback)
    {
        return RegisterControllerAction(actionId, displayName, "General", string.Empty, callback);
    }

    internal static bool RegisterControllerAction(
        string actionId,
        string displayName,
        string category,
        string description,
        Action callback)
    {
        string normalizedActionId = NormalizeProviderId(actionId);
        if (normalizedActionId.Length == 0 || callback == null)
        {
            return false;
        }

        ControllerActions[normalizedActionId] = new ControllerActionRegistration(
            normalizedActionId,
            CleanText(displayName, 96, normalizedActionId),
            CleanText(category, 96, "General"),
            CleanText(description, StatusProviderTextLimit, string.Empty),
            callback);
        return true;
    }

    internal static bool UnregisterControllerAction(string actionId)
    {
        string normalizedActionId = NormalizeProviderId(actionId);
        return normalizedActionId.Length > 0 && ControllerActions.Remove(normalizedActionId);
    }

    internal static bool RegisterStatusProvider(
        string providerId,
        string displayName,
        Func<FoAModStatusSnapshot> snapshotProvider)
    {
        return RegisterStatusProvider(providerId, displayName, "General", string.Empty, snapshotProvider);
    }

    internal static bool RegisterStatusProvider(
        string providerId,
        string displayName,
        string category,
        string description,
        Func<FoAModStatusSnapshot> snapshotProvider)
    {
        string normalizedProviderId = NormalizeProviderId(providerId);
        if (normalizedProviderId.Length == 0 || snapshotProvider == null)
        {
            return false;
        }

        StatusProviders[normalizedProviderId] = new StatusProviderRegistration(
            normalizedProviderId,
            CleanText(displayName, 96, normalizedProviderId),
            CleanText(category, 96, "General"),
            CleanText(description, StatusProviderTextLimit, string.Empty),
            snapshotProvider);
        return true;
    }

    internal static bool UnregisterStatusProvider(string providerId)
    {
        string normalizedProviderId = NormalizeProviderId(providerId);
        return normalizedProviderId.Length > 0 && StatusProviders.Remove(normalizedProviderId);
    }

    internal static void UpdateFromUnity()
    {
        if (_openHotkey != null && Input.GetKeyDown(_openHotkey.Value))
        {
            _isManagerVisible = !_isManagerVisible;
        }

        if (_isManagerVisible && Input.GetKeyDown(KeyCode.Escape))
        {
            _isManagerVisible = false;
        }

        UpdateGameplayReadyGateFromFoALog();
        ApplyInputOwnershipState();

        if (!IsRuntimeQuarantineEnabled())
        {
            UpdateControllerActionLayer();
            ApplyCustomUiTimeScale();
            return;
        }

        ClearExpiredControllerActionSuppression();
        RestoreCustomUiTimeScaleIfNeeded(force: true);
    }

    internal static void DrawFromUnity()
    {
        if (!_isManagerVisible)
        {
            return;
        }

        _windowRect = ClampWindowRect(_windowRect);
        _windowRect = GUILayout.Window(
            0xF0A0712,
            _windowRect,
            (GUI.WindowFunction)DrawWindow,
            PluginName + " IL2CPP");

        ConsumeCurrentGuiInputEvent();
    }

    private static void DrawWindow(int windowId)
    {
        GUILayout.BeginVertical();
        GUILayout.Label("IL2CPP branch manager");
        GUILayout.Label(IsRuntimeQuarantineEnabled()
            ? "Runtime patch quarantine is active. F10 overlay, cursor ownership, status, and gameplay-ready-gated config saves are available; Harmony menu/input/controller hooks are blocked."
            : "Config editing, input ownership, controller action storage, menu patch attempts, and status adapters are active for this branch build.");
        GUILayout.Label(_managerStatus);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Refresh", GUILayout.Width(120f)))
        {
            RefreshManager();
        }

        if (GUILayout.Button("Close", GUILayout.Width(120f)))
        {
            HideManager();
        }

        GUILayout.EndHorizontal();

        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
        DrawRuntimeSection();
        DrawConfigDiscoverySection();
        DrawStatusSection();
        DrawApiSection();
        GUILayout.EndScrollView();

        GUI.DragWindow(new Rect(0f, 0f, 10000f, 24f));
        GUILayout.EndVertical();
    }

    private static void DrawRuntimeSection()
    {
        GUILayout.Space(8f);
        GUILayout.Label("Runtime");
        DrawLine("Version", PluginVersion);
        DrawLine("Branch", "IL2CPP / BepInEx 6");
        DrawLine("Process", Paths.ProcessName ?? "unknown");
        DrawLine("Game version", Application.version ?? "unknown");
        DrawLine("Unity", Application.unityVersion ?? "unknown");
        DrawLine("Loaded BepInEx plugins", GetLoadedPluginCount().ToString(CultureInfo.InvariantCulture));
        DrawLine("Discovered config plugins", _configPlugins.Length.ToString(CultureInfo.InvariantCulture));
        DrawLine("Discovered config entries", GetDiscoveredConfigEntryCount().ToString(CultureInfo.InvariantCulture));
        DrawLine("Config path", Paths.ConfigPath ?? "unknown");
        DrawLine("Runtime patch quarantine", IsRuntimeQuarantineEnabled().ToString(CultureInfo.InvariantCulture));
        DrawLine("Quarantine forced", ForceIl2CppRuntimePatchQuarantine.ToString(CultureInfo.InvariantCulture));
        DrawLine("Gameplay-ready gate", _manualGameplayReady ? "Armed (" + _gameplayReadyGateSource + ")" : "Blocked");
        DrawLine("Lifecycle log gate", _autoArmGameplayReadyFromFoALog?.Value == true ? _lifecycleStatus : "Disabled");
        DrawLine("Config writes", AreConfigWritesAllowed() ? "Allowed" : GetConfigWriteBlockReason());
        DrawLine("Menu patch", _menuPatchApplied ? "Applied" : (_menuPatchAttempted ? "Attempted" : "Disabled"));
        DrawLine("Input freeze patch", _inputPatchApplied ? "Applied" : (_inputPatchAttempted ? "Attempted" : "Disabled"));

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Mark Gameplay Ready", GUILayout.Width(170f)))
        {
            ArmGameplayReadyGate("manual", "Gameplay-ready gate armed manually for this session. Config saves are allowed while Safety.AllowConfigWritesAfterGameplayReady remains true.");
        }

        if (GUILayout.Button("Clear Gameplay Gate", GUILayout.Width(170f)))
        {
            ClearGameplayReadyGate("Gameplay-ready gate cleared. Config saves are blocked.", resetLifecycleMarkers: true);
        }

        GUILayout.EndHorizontal();
    }

    private static void DrawConfigDiscoverySection()
    {
        GUILayout.Space(8f);
        GUILayout.Label("Config");
        GUILayout.Label(AreConfigWritesAllowed()
            ? "Loaded BepInEx 6 config entries can be edited here. Saves call SetSerializedValue and the owning ConfigFile.Save()."
            : "Loaded BepInEx 6 config entries are inspectable. Saves are blocked until the gameplay-ready gate is armed.");
        GUILayout.Label(_configCatalogStatus);

        if (_configCatalogUpdatedUtc != DateTime.MinValue)
        {
            DrawLine("Last refresh UTC", _configCatalogUpdatedUtc.ToString("O", CultureInfo.InvariantCulture));
        }

        if (_configPlugins.Length == 0)
        {
            GUILayout.Label("No loaded plugin config entries discovered.");
            return;
        }

        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(260f));
        GUILayout.Label("Loaded Mods");

        foreach (ConfigPluginSnapshot plugin in _configPlugins)
        {
            string label = plugin.DisplayName + " (" + plugin.SettingCount.ToString(CultureInfo.InvariantCulture) + ")";
            if (GUILayout.Button(label))
            {
                _selectedConfigPluginKey = plugin.SelectionKey;
            }
        }

        GUILayout.EndVertical();

        ConfigPluginSnapshot selected = GetSelectedConfigPlugin();
        GUILayout.BeginVertical(GUI.skin.box);

        if (selected == null)
        {
            GUILayout.Label("Select a mod to inspect its config entries.");
        }
        else
        {
            DrawSelectedConfigPlugin(selected);
        }

        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
    }

    private static void DrawSelectedConfigPlugin(ConfigPluginSnapshot plugin)
    {
        GUILayout.Label(plugin.DisplayName);
        DrawLine("GUID", plugin.Guid);
        DrawLine("Version", plugin.Version);
        DrawLine("Settings", plugin.SettingCount.ToString(CultureInfo.InvariantCulture));

        if (plugin.ConfigPath.Length > 0)
        {
            DrawLine("Config file", System.IO.Path.GetFileName(plugin.ConfigPath));
        }

        if (plugin.Settings.Length == 0)
        {
            GUILayout.Label("This loaded plugin has no registered config entries yet.");
            return;
        }

        int displayed = 0;
        foreach (ConfigSettingSnapshot setting in plugin.Settings)
        {
            if (displayed >= 80)
            {
                GUILayout.Label("Additional settings hidden in this build-only shell.");
                break;
            }

            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label(setting.Section + " / " + setting.Key);
            DrawLine("Type", setting.SettingType);
            DrawLine("Current", setting.SerializedValue);
            DrawLine("Default", setting.DefaultValue);

            if (setting.Description.Length > 0)
            {
                GUILayout.Label(setting.Description);
            }

            DrawConfigSettingEditor(setting);
            GUILayout.EndVertical();
            displayed++;
        }
    }

    private static void DrawConfigSettingEditor(ConfigSettingSnapshot setting)
    {
        if (!CanWriteConfigSetting(setting, out string blockReason))
        {
            GUILayout.Label(blockReason);
            return;
        }

        string draft = SettingDraftValues.TryGetValue(setting.Id, out string existingDraft)
            ? existingDraft
            : setting.SerializedValue;

        GUILayout.BeginHorizontal();
        GUILayout.Label("Edit", GUILayout.Width(190f));
        string updatedDraft = GUILayout.TextField(draft ?? string.Empty);
        if (!string.Equals(updatedDraft, draft, StringComparison.Ordinal))
        {
            SettingDraftValues[setting.Id] = updatedDraft;
        }

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Apply", GUILayout.Width(100f)))
        {
            SaveSerializedSetting(setting, SettingDraftValues.TryGetValue(setting.Id, out string value) ? value : setting.SerializedValue);
        }

        if (GUILayout.Button("Reset", GUILayout.Width(100f)))
        {
            SettingDraftValues[setting.Id] = setting.SerializedValue;
            _managerStatus = $"Reset draft for {setting.Section}/{setting.Key}.";
        }

        if (GUILayout.Button("Default", GUILayout.Width(100f)))
        {
            RestoreDefaultSetting(setting);
        }

        if (IsBooleanSetting(setting) && GUILayout.Button("Toggle", GUILayout.Width(100f)))
        {
            ToggleBooleanSetting(setting);
        }

        GUILayout.EndHorizontal();
    }

    private static void DrawStatusSection()
    {
        GUILayout.Space(8f);
        GUILayout.Label("Status Providers");

        StatusProviderRegistration[] providers = StatusProviders.Values
            .OrderBy(provider => provider.Category, StringComparer.OrdinalIgnoreCase)
            .ThenBy(provider => provider.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (providers.Length == 0)
        {
            GUILayout.Label("No status providers registered.");
            return;
        }

        foreach (StatusProviderRegistration provider in providers)
        {
            DrawStatusProvider(provider);
        }
    }

    private static void DrawStatusProvider(StatusProviderRegistration provider)
    {
        StatusProviderSnapshotView snapshot = ReadStatusSnapshot(provider);
        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label(provider.DisplayName + " [" + snapshot.Level + "]");
        GUILayout.Label(provider.Category + " - " + provider.ProviderId);

        if (snapshot.Summary.Length > 0)
        {
            GUILayout.Label(snapshot.Summary);
        }

        if (snapshot.Detail.Length > 0)
        {
            GUILayout.Label(snapshot.Detail);
        }

        foreach (string line in snapshot.Lines)
        {
            GUILayout.Label("- " + line);
        }

        if (snapshot.LinesWereTruncated)
        {
            GUILayout.Label("- additional lines hidden");
        }

        GUILayout.EndVertical();
    }

    private static void DrawApiSection()
    {
        GUILayout.Space(8f);
        GUILayout.Label("API State");
        DrawLine("Status providers", StatusProviders.Count.ToString(CultureInfo.InvariantCulture));
        DrawLine("Config plugins", _configPlugins.Length.ToString(CultureInfo.InvariantCulture));
        DrawLine("Config entries", GetDiscoveredConfigEntryCount().ToString(CultureInfo.InvariantCulture));
        DrawLine("Runtime patch quarantine", IsRuntimeQuarantineEnabled().ToString(CultureInfo.InvariantCulture));
        DrawLine("Config writes", AreConfigWritesAllowed() ? "Allowed" : GetConfigWriteBlockReason());
        DrawLine("Registered controller actions", ControllerActions.Count.ToString(CultureInfo.InvariantCulture));
        DrawLine("Controller cursor scopes", ControllerCursorScopes.Count.ToString(CultureInfo.InvariantCulture));
        DrawLine("Custom UI scopes", CustomUiScopes.Count.ToString(CultureInfo.InvariantCulture));
        DrawLine("World freeze active", IsWorldFreezeRequested().ToString(CultureInfo.InvariantCulture));
    }

    private static void DrawLine(string label, string value)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(label, GUILayout.Width(190f));
        GUILayout.Label(value);
        GUILayout.EndHorizontal();
    }

    private static StatusProviderSnapshotView ReadStatusSnapshot(StatusProviderRegistration provider)
    {
        try
        {
            return CreateStatusSnapshotView(provider.SnapshotProvider());
        }
        catch (Exception ex)
        {
            return new StatusProviderSnapshotView(
                FoAModStatusLevel.Problem,
                "Provider failed",
                CleanText(ex.GetType().Name + ": " + ex.Message, StatusProviderTextLimit, string.Empty),
                string.Empty,
                Array.Empty<string>(),
                linesWereTruncated: false);
        }
    }

    private static StatusProviderSnapshotView CreateStatusSnapshotView(FoAModStatusSnapshot snapshot)
    {
        if (snapshot == null)
        {
            return new StatusProviderSnapshotView(
                FoAModStatusLevel.Unknown,
                "Provider returned no snapshot",
                string.Empty,
                string.Empty,
                Array.Empty<string>(),
                linesWereTruncated: false);
        }

        string[] lines = (snapshot.Lines ?? Array.Empty<string>())
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => CleanText(line, StatusProviderTextLimit, string.Empty))
            .Take(StatusProviderLineLimit)
            .ToArray();

        bool truncated = (snapshot.Lines ?? Array.Empty<string>())
            .Count(line => !string.IsNullOrWhiteSpace(line)) > StatusProviderLineLimit;

        return new StatusProviderSnapshotView(
            snapshot.Level,
            CleanText(snapshot.Summary, StatusProviderTextLimit, string.Empty),
            CleanText(snapshot.Detail, StatusProviderTextLimit, string.Empty),
            CleanText(snapshot.UpdatedUtc, 96, string.Empty),
            lines,
            truncated);
    }

    private static void RegisterBuiltInStatusProviders()
    {
        RegisterStatusProvider(
            "foa.mod-manager.il2cpp",
            "FoA Mod Manager IL2CPP",
            "Manager",
            "Branch and port status for the IL2CPP manager shell.",
            CreateManagerStatusSnapshot);
    }

    private static FoAModStatusSnapshot CreateManagerStatusSnapshot()
    {
        bool quarantined = IsRuntimeQuarantineEnabled();
        bool configWritesAllowed = AreConfigWritesAllowed();
        return new FoAModStatusSnapshot
        {
            Level = quarantined ? FoAModStatusLevel.Warning : FoAModStatusLevel.Info,
            Summary = quarantined
                ? "IL2CPP runtime patches are quarantined; safe manager functions remain available."
                : "IL2CPP manager parity layer is loaded on BepInEx 6.",
            Detail = quarantined
                ? "Harmony menu/input patches, controller dispatch, and time-scale mutation are disabled while save-load stability is under investigation. F10 overlay, cursor ownership, status providers, and gameplay-ready-gated config saves remain available."
                : "Config edit/save, custom UI ownership, controller action storage, menu patch attempts, input freeze patches, and status adapters are available in this branch build.",
            Schema = "foa-mod-manager-il2cpp-status/1",
            UpdatedUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture),
            Lines = new[]
            {
                "branch=IL2CPP",
                "bepInEx=BepInEx 6",
                "runtimePatchQuarantine=" + quarantined.ToString(CultureInfo.InvariantCulture),
                "runtimePatchQuarantineForced=" + ForceIl2CppRuntimePatchQuarantine.ToString(CultureInfo.InvariantCulture),
                "api.statusProvider=True",
                "api.customUiScope=True",
                "configEditingPorted=True",
                "configWritesArmed=" + configWritesAllowed.ToString(CultureInfo.InvariantCulture),
                "gameplayReadyGate=" + _manualGameplayReady.ToString(CultureInfo.InvariantCulture),
                "gameplayReadyGateSource=" + _gameplayReadyGateSource,
                "gameplayReadyAutoLog=" + (_autoArmGameplayReadyFromFoALog?.Value == true).ToString(CultureInfo.InvariantCulture),
                "lifecycleLog=" + _lifecycleStatus,
                "cursorOwnershipPorted=True",
                "configPlugins=" + _configPlugins.Length.ToString(CultureInfo.InvariantCulture),
                "configEntries=" + GetDiscoveredConfigEntryCount().ToString(CultureInfo.InvariantCulture),
                "menuPatchesPorted=" + _menuPatchApplied.ToString(CultureInfo.InvariantCulture),
                "inputFreezePorted=" + _inputPatchApplied.ToString(CultureInfo.InvariantCulture),
                "controllerDispatchPorted=" + (!quarantined).ToString(CultureInfo.InvariantCulture),
                "taintedStatusAdapter=True"
            }
        };
    }

    private static void RegisterBuiltInControllerActions()
    {
        RegisterControllerAction(
            "manager.toggle",
            "Toggle FoA Mod Manager",
            "Manager",
            "Open or close the IL2CPP FoA Mod Manager overlay.",
            () => _isManagerVisible = !_isManagerVisible);
    }

    private static void RegisterTaintedFrameworkStatusProvider()
    {
        RegisterStatusProvider(
            "tainted-framework.il2cpp",
            "Tainted Framework",
            "Framework",
            "Reflection-only status adapter for the live Tainted Framework plugin.",
            CreateTaintedFrameworkStatusSnapshot);
    }

    private static FoAModStatusSnapshot CreateTaintedFrameworkStatusSnapshot()
    {
        PluginInfo info = FindLoadedPlugin("kane.tgfoa.tainted-framework");
        if (info == null)
        {
            return new FoAModStatusSnapshot
            {
                Level = FoAModStatusLevel.Warning,
                Summary = "Tainted Framework is not loaded.",
                Detail = "No loaded BepInEx plugin with GUID kane.tgfoa.tainted-framework was found.",
                Schema = "foa-mod-manager-tainted-status/1",
                UpdatedUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture),
                Lines = new[] { "loaded=False", "adapter=reflection-only" }
            };
        }

        string version = info.Metadata?.Version == null ? "unknown" : info.Metadata.Version.ToString();
        return new FoAModStatusSnapshot
        {
            Level = FoAModStatusLevel.Info,
            Summary = "Tainted Framework is loaded.",
            Detail = "The manager sees the live framework plugin without taking a compile-time Tainted dependency.",
            Schema = "foa-mod-manager-tainted-status/1",
            UpdatedUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture),
            Lines = new[]
            {
                "loaded=True",
                "guid=" + CleanText(info.Metadata?.GUID, 160, "unknown"),
                "name=" + CleanText(info.Metadata?.Name, 160, "unknown"),
                "version=" + CleanText(version, 80, "unknown"),
                "adapter=reflection-only"
            }
        };
    }

    private static PluginInfo FindLoadedPlugin(string guid)
    {
        try
        {
            if (IL2CPPChainloader.Instance?.Plugins == null)
            {
                return null;
            }

            return IL2CPPChainloader.Instance.Plugins.Values.FirstOrDefault(
                plugin => string.Equals(plugin.Metadata?.GUID, guid, StringComparison.OrdinalIgnoreCase));
        }
        catch
        {
            return null;
        }
    }

    private static int GetLoadedPluginCount()
    {
        try
        {
            return IL2CPPChainloader.Instance?.Plugins?.Count ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    private static int GetDiscoveredConfigEntryCount()
    {
        return _configPlugins.Sum(plugin => plugin.SettingCount);
    }

    private static ConfigPluginSnapshot GetSelectedConfigPlugin()
    {
        if (_configPlugins.Length == 0)
        {
            return null;
        }

        ConfigPluginSnapshot selected = _configPlugins.FirstOrDefault(
            plugin => string.Equals(plugin.SelectionKey, _selectedConfigPluginKey, StringComparison.OrdinalIgnoreCase));

        return selected ?? _configPlugins[0];
    }

    private static void RefreshConfigCatalog()
    {
        try
        {
            List<ConfigPluginSnapshot> plugins = new();
            IEnumerable<PluginInfo> loadedPlugins = IL2CPPChainloader.Instance == null || IL2CPPChainloader.Instance.Plugins == null
                ? Array.Empty<PluginInfo>()
                : IL2CPPChainloader.Instance.Plugins.Values;

            foreach (PluginInfo info in loadedPlugins.OrderBy(plugin => plugin.Metadata.Name, StringComparer.OrdinalIgnoreCase))
            {
                BasePlugin pluginInstance = info.Instance as BasePlugin;
                string guid = CleanText(info.Metadata.GUID, 160, "unknown");
                string displayName = CleanText(info.Metadata.Name, 160, guid);
                string version = info.Metadata.Version == null
                    ? "unknown"
                    : CleanText(info.Metadata.Version.ToString(), 80, "unknown");
                string location = CleanText(info.Location, 260, string.Empty);
                string configPath = string.Empty;
                ConfigSettingSnapshot[] settings = Array.Empty<ConfigSettingSnapshot>();

                if (pluginInstance != null && pluginInstance.Config != null)
                {
                    string selectionKey = CreateConfigPluginSelectionKey(guid, location);
                    configPath = CleanText(pluginInstance.Config.ConfigFilePath, 260, string.Empty);
                    settings = ((IDictionary<ConfigDefinition, ConfigEntryBase>)pluginInstance.Config)
                        .Values
                        .OrderBy(entry => entry.Definition.Section, StringComparer.OrdinalIgnoreCase)
                        .ThenBy(entry => entry.Definition.Key, StringComparer.OrdinalIgnoreCase)
                        .Select(entry => CreateConfigSettingSnapshot(selectionKey, entry))
                        .ToArray();
                }

                plugins.Add(new ConfigPluginSnapshot(
                    CreateConfigPluginSelectionKey(guid, location),
                    guid,
                    displayName,
                    version,
                    location,
                    configPath,
                    settings));
            }

            _configPlugins = plugins.ToArray();
            if (_configPlugins.Length > 0 &&
                !_configPlugins.Any(plugin => string.Equals(plugin.SelectionKey, _selectedConfigPluginKey, StringComparison.OrdinalIgnoreCase)))
            {
                _selectedConfigPluginKey = _configPlugins[0].SelectionKey;
            }

            _configCatalogUpdatedUtc = DateTime.UtcNow;
            _configCatalogStatus =
                "Discovered " +
                _configPlugins.Length.ToString(CultureInfo.InvariantCulture) +
                " loaded plugins and " +
                GetDiscoveredConfigEntryCount().ToString(CultureInfo.InvariantCulture) +
                " config entries. Edit/save enabled.";
        }
        catch (Exception ex)
        {
            _configPlugins = Array.Empty<ConfigPluginSnapshot>();
            _configCatalogStatus = "Config discovery failed: " + CleanText(ex.GetType().Name + ": " + ex.Message, 220, "unknown error");
            _configCatalogUpdatedUtc = DateTime.UtcNow;
        }
    }

    private static ConfigSettingSnapshot CreateConfigSettingSnapshot(string pluginSelectionKey, ConfigEntryBase entry)
    {
        string section = CleanText(entry.Definition.Section, 120, "General");
        string key = CleanText(entry.Definition.Key, 120, "Setting");
        string id = pluginSelectionKey + "|" + section + "|" + key;
        string serializedValue = CleanText(ReadSerializedValue(entry), 220, string.Empty);
        if (!SettingDraftValues.ContainsKey(id))
        {
            SettingDraftValues[id] = serializedValue;
        }

        return new ConfigSettingSnapshot(
            id,
            section,
            key,
            entry.SettingType == null ? "unknown" : CleanText(entry.SettingType.Name, 80, "unknown"),
            serializedValue,
            CleanText(ReadDefaultValue(entry), 220, string.Empty),
            entry.Description == null ? string.Empty : CleanText(entry.Description.Description, 260, string.Empty),
            entry);
    }

    private static string ReadSerializedValue(ConfigEntryBase entry)
    {
        try
        {
            return entry.GetSerializedValue();
        }
        catch (Exception ex)
        {
            return "unreadable:" + ex.GetType().Name;
        }
    }

    private static string ReadDefaultValue(ConfigEntryBase entry)
    {
        try
        {
            return entry.DefaultValue == null ? string.Empty : entry.DefaultValue.ToString();
        }
        catch (Exception ex)
        {
            return "unreadable:" + ex.GetType().Name;
        }
    }

    private static void SaveSerializedSetting(ConfigSettingSnapshot setting, string value)
    {
        if (!CanWriteConfigSetting(setting, out string blockReason))
        {
            _managerStatus = blockReason;
            return;
        }

        try
        {
            setting.Entry.SetSerializedValue(value ?? string.Empty);
            setting.Entry.ConfigFile.Save();
            SettingDraftValues[setting.Id] = ReadSerializedValue(setting.Entry);
            _managerStatus = $"Saved {setting.Section}/{setting.Key}.";
            RefreshConfigCatalog();
        }
        catch (Exception ex)
        {
            _managerStatus = $"Could not save {setting.Section}/{setting.Key}: {ex.GetType().Name}.";
            _instance?.Log.LogWarning($"Could not save {setting.Section}.{setting.Key}: {ex}");
        }
    }

    private static void RestoreDefaultSetting(ConfigSettingSnapshot setting)
    {
        if (!CanWriteConfigSetting(setting, out string blockReason))
        {
            _managerStatus = blockReason;
            return;
        }

        try
        {
            if (setting.Entry.DefaultValue == null)
            {
                _managerStatus = $"No default value is available for {setting.Section}/{setting.Key}.";
                return;
            }

            setting.Entry.BoxedValue = setting.Entry.DefaultValue;
            setting.Entry.ConfigFile.Save();
            SettingDraftValues[setting.Id] = ReadSerializedValue(setting.Entry);
            _managerStatus = $"Restored default for {setting.Section}/{setting.Key}.";
            RefreshConfigCatalog();
        }
        catch (Exception ex)
        {
            _managerStatus = $"Could not restore {setting.Section}/{setting.Key}: {ex.GetType().Name}.";
            _instance?.Log.LogWarning($"Could not restore {setting.Section}.{setting.Key}: {ex}");
        }
    }

    private static bool IsBooleanSetting(ConfigSettingSnapshot setting)
    {
        return setting.Entry?.SettingType == typeof(bool) ||
            string.Equals(setting.SettingType, "Boolean", StringComparison.OrdinalIgnoreCase);
    }

    private static void ToggleBooleanSetting(ConfigSettingSnapshot setting)
    {
        if (!CanWriteConfigSetting(setting, out string blockReason))
        {
            _managerStatus = blockReason;
            return;
        }

        try
        {
            bool current = false;
            if (setting.Entry.BoxedValue is bool boxed)
            {
                current = boxed;
            }
            else
            {
                bool.TryParse(setting.SerializedValue, out current);
            }

            setting.Entry.BoxedValue = !current;
            setting.Entry.ConfigFile.Save();
            SettingDraftValues[setting.Id] = ReadSerializedValue(setting.Entry);
            _managerStatus = $"Toggled {setting.Section}/{setting.Key}.";
            RefreshConfigCatalog();
        }
        catch (Exception ex)
        {
            _managerStatus = $"Could not toggle {setting.Section}/{setting.Key}: {ex.GetType().Name}.";
            _instance?.Log.LogWarning($"Could not toggle {setting.Section}.{setting.Key}: {ex}");
        }
    }

    private static string CreateConfigPluginSelectionKey(string guid, string location)
    {
        return guid + "|" + location;
    }

    private static Rect ClampWindowRect(Rect rect)
    {
        float maxWidth = Mathf.Max(360f, Screen.width - 32f);
        float maxHeight = Mathf.Max(260f, Screen.height - 32f);
        float width = Mathf.Clamp(rect.width, 360f, maxWidth);
        float height = Mathf.Clamp(rect.height, 260f, maxHeight);
        float x = Mathf.Clamp(rect.x, 8f, Mathf.Max(8f, Screen.width - width - 8f));
        float y = Mathf.Clamp(rect.y, 8f, Mathf.Max(8f, Screen.height - height - 8f));
        return new Rect(x, y, width, height);
    }

    private static void ApplyRuntimePatches()
    {
        if (IsRuntimeQuarantineEnabled())
        {
            _managerStatus = "Runtime patch quarantine active: Harmony patches, controller dispatch, and time-scale mutation are disabled.";
            _instance?.Log.LogWarning("FoA Mod Manager IL2CPP runtime patch quarantine is active. Menu patches, input freeze patches, controller dispatch, and time-scale mutation are disabled; F10 overlay, cursor ownership, status providers, and gameplay-ready-gated config saves remain available.");
            return;
        }

        _harmony = new Harmony(PluginGuid + ".il2cpp");

        if (_enableMenuButtons?.Value == true)
        {
            _menuPatchAttempted = true;
            _menuPatchApplied = Il2CppRuntimePatches.PatchMenuButtons(_harmony, _instance.Log);
        }

        if (_enableInputFreezePatches?.Value == true)
        {
            _inputPatchAttempted = true;
            _inputPatchApplied = Il2CppRuntimePatches.PatchInputFreeze(_harmony, _instance.Log);
        }
    }

    private static void ApplyInputOwnershipState()
    {
        bool inputOwned = IsModUiInputOwned;
        if (inputOwned)
        {
            if (!_cursorStateCaptured)
            {
                _capturedCursorLockMode = Cursor.lockState;
                _capturedCursorVisible = Cursor.visible;
                _cursorStateCaptured = true;
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            _lastInputOwned = true;
            return;
        }

        if (_lastInputOwned)
        {
            RestoreInputOwnershipState(force: false);
        }
    }

    private static void RestoreInputOwnershipState(bool force)
    {
        if (!_cursorStateCaptured && !force)
        {
            _lastInputOwned = false;
            return;
        }

        if (_cursorStateCaptured)
        {
            Cursor.lockState = _capturedCursorLockMode;
            Cursor.visible = _capturedCursorVisible;
        }

        _cursorStateCaptured = false;
        _lastInputOwned = false;
    }

    private static void ConsumeCurrentGuiInputEvent()
    {
        Event current = Event.current;
        if (current == null || !IsModUiInputOwned)
        {
            return;
        }

        if (current.type == EventType.MouseDown ||
            current.type == EventType.MouseUp ||
            current.type == EventType.MouseDrag ||
            current.type == EventType.ScrollWheel ||
            current.type == EventType.KeyDown ||
            current.type == EventType.KeyUp)
        {
            current.Use();
        }
    }

    private static void UpdateControllerActionLayer()
    {
        if (_controllerActionLayerEnabled?.Value != true ||
            _controllerActionLayerDispatchEnabled?.Value != true ||
            IsModUiInputOwned)
        {
            ClearExpiredControllerActionSuppression();
            return;
        }

        KeyCode modifier = _controllerActionLayerModifierKey?.Value ?? KeyCode.JoystickButton9;
        KeyCode target = _controllerActionLayerTargetKey?.Value ?? KeyCode.JoystickButton0;
        if (modifier == KeyCode.None || target == KeyCode.None)
        {
            return;
        }

        IsInternalControllerInputReadActive = true;
        try
        {
            if (!Input.GetKey(modifier) || !Input.GetKeyDown(target))
            {
                return;
            }
        }
        finally
        {
            IsInternalControllerInputReadActive = false;
        }

        if (!ControllerActions.TryGetValue("manager.toggle", out ControllerActionRegistration registration))
        {
            return;
        }

        try
        {
            registration.Callback();
            ArmControllerActionSuppression(target);
            _instance?.Log.LogInfo(
                "Controller action layer dispatched: " +
                $"modifierKey={modifier}; targetKey={target}; registeredAction={registration.ActionId}; " +
                "source=unity-keycode; dispatch=true; vanillaInputSuppressed=true.");
        }
        catch (Exception ex)
        {
            _instance?.Log.LogWarning($"Controller action layer dispatch failed for {registration.ActionId}: {ex}");
        }
    }

    private static void ArmControllerActionSuppression(KeyCode target)
    {
        SuppressedControllerActionNames.Clear();
        SuppressedControllerActionIds.Clear();
        SuppressedControllerActionNames.Add(target.ToString());
        _suppressedControllerActionFrame = Time.frameCount;
    }

    private static void ClearExpiredControllerActionSuppression()
    {
        if (_suppressedControllerActionFrame == Time.frameCount)
        {
            return;
        }

        SuppressedControllerActionNames.Clear();
        SuppressedControllerActionIds.Clear();
        _suppressedControllerActionFrame = -1;
    }

    private static void ApplyCustomUiTimeScale()
    {
        if (IsRuntimeQuarantineEnabled())
        {
            RestoreCustomUiTimeScaleIfNeeded(force: true);
            return;
        }

        bool shouldFreeze = IsWorldFreezeRequested();
        if (shouldFreeze && !_wasCustomUiScopeActive)
        {
            _capturedTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            _wasCustomUiScopeActive = true;
            return;
        }

        if (!shouldFreeze)
        {
            RestoreCustomUiTimeScaleIfNeeded(force: false);
        }
    }

    private static void RestoreCustomUiTimeScaleIfNeeded(bool force)
    {
        if (!_wasCustomUiScopeActive && !force)
        {
            return;
        }

        if (_wasCustomUiScopeActive || force)
        {
            Time.timeScale = _capturedTimeScale <= 0f ? 1f : _capturedTimeScale;
            _wasCustomUiScopeActive = false;
        }
    }

    private static bool IsWorldFreezeRequested()
    {
        return !IsRuntimeQuarantineEnabled() &&
            _freezeWorldForCustomUiScopes?.Value == true &&
            CustomUiScopes.Values.Any(freezeWorld => freezeWorld);
    }

    private static void UpdateGameplayReadyGateFromFoALog()
    {
        if (_autoArmGameplayReadyFromFoALog?.Value != true)
        {
            return;
        }

        float now = Time.unscaledTime;
        if (now < _nextLifecycleLogPollAt)
        {
            return;
        }

        _nextLifecycleLogPollAt = now + LifecycleLogPollIntervalSeconds;

        try
        {
            string logPath = FindLatestFoAGameLogPath();
            if (logPath.Length == 0)
            {
                _lifecycleStatus = "No current FoA game log found.";
                return;
            }

            FileInfo info = new(logPath);
            long length = info.Exists ? info.Length : 0L;
            if (length <= 0L)
            {
                _lifecycleStatus = "FoA game log is empty.";
                return;
            }

            if (!string.Equals(_lifecycleLogPath, logPath, StringComparison.OrdinalIgnoreCase) ||
                length < _lifecycleLogPosition)
            {
                _lifecycleLogPath = logPath;
                _lifecycleLogPosition = length;
                _lifecycleLoadStartedSeen = false;
                _lifecycleLoadingCompletedSeen = false;
                _lifecycleLoadingFullyDiscardedSeen = false;
                ClearGameplayReadyGate("New FoA lifecycle log observed. Config saves are blocked until load completion markers are seen.", resetLifecycleMarkers: false);
            }

            if (length == _lifecycleLogPosition)
            {
                _lifecycleStatus = BuildLifecycleStatus();
                return;
            }

            long readStart = Math.Max(_lifecycleLogPosition, length - MaxLifecycleReadBytes);
            using FileStream stream = new(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            stream.Seek(readStart, SeekOrigin.Begin);
            using StreamReader reader = new(stream);
            string text = reader.ReadToEnd();
            _lifecycleLogPosition = stream.Position;

            ProcessFoALifecycleLogText(text);
        }
        catch (Exception ex)
        {
            _lifecycleStatus = "FoA lifecycle log scan failed: " + ex.GetType().Name;
        }
    }

    private static string FindLatestFoAGameLogPath()
    {
        try
        {
            string persistentRoot = Application.persistentDataPath ?? string.Empty;
            if (persistentRoot.Length == 0)
            {
                return string.Empty;
            }

            string logDirectory = Path.Combine(persistentRoot, "Logs");
            if (!Directory.Exists(logDirectory))
            {
                return string.Empty;
            }

            DateTime oldestAccepted = _pluginLoadedUtc == DateTime.MinValue
                ? DateTime.UtcNow.AddMinutes(-5)
                : _pluginLoadedUtc.AddMinutes(-2);

            FileInfo latest = new DirectoryInfo(logDirectory)
                .EnumerateFiles("*.txt", SearchOption.TopDirectoryOnly)
                .Where(file => file.LastWriteTimeUtc >= oldestAccepted)
                .OrderByDescending(file => file.LastWriteTimeUtc)
                .FirstOrDefault();

            return latest?.FullName ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static void ProcessFoALifecycleLogText(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            _lifecycleStatus = BuildLifecycleStatus();
            return;
        }

        string[] lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (string rawLine in lines)
        {
            if (ContainsOrdinalIgnoreCase(rawLine, "Loading '") ||
                ContainsOrdinalIgnoreCase(rawLine, "Dropping TitleScreen") ||
                ContainsOrdinalIgnoreCase(rawLine, "Waiting for templates to load"))
            {
                _lifecycleLoadStartedSeen = true;
                _lifecycleLoadingCompletedSeen = false;
                _lifecycleLoadingFullyDiscardedSeen = false;
                ClearGameplayReadyGate("FoA load transition observed. Config saves are blocked until load completion markers are seen.", resetLifecycleMarkers: false);
                continue;
            }

            if (ContainsOrdinalIgnoreCase(rawLine, "Loading: Completed"))
            {
                _lifecycleLoadingCompletedSeen = true;
            }

            if (ContainsOrdinalIgnoreCase(rawLine, "Loading: Fully discarded"))
            {
                _lifecycleLoadingFullyDiscardedSeen = true;
            }
        }

        if (_lifecycleLoadStartedSeen &&
            _lifecycleLoadingCompletedSeen &&
            _lifecycleLoadingFullyDiscardedSeen)
        {
            ArmGameplayReadyGate("foa-log", "Gameplay-ready gate auto-armed from FoA log markers: Loading: Completed + Loading: Fully discarded.");
            _lifecycleStatus = "Armed from FoA log markers.";
            return;
        }

        _lifecycleStatus = BuildLifecycleStatus();
    }

    private static void ArmGameplayReadyGate(string source, string status)
    {
        bool changed = !_manualGameplayReady ||
            !string.Equals(_gameplayReadyGateSource, source, StringComparison.OrdinalIgnoreCase);

        _manualGameplayReady = true;
        _gameplayReadyGateSource = source;
        _managerStatus = status;

        if (changed)
        {
            _instance?.Log.LogInfo(status);
        }
    }

    private static void ClearGameplayReadyGate(string status, bool resetLifecycleMarkers)
    {
        if (resetLifecycleMarkers)
        {
            _lifecycleLoadStartedSeen = false;
            _lifecycleLoadingCompletedSeen = false;
            _lifecycleLoadingFullyDiscardedSeen = false;
        }

        if (_manualGameplayReady)
        {
            _instance?.Log.LogInfo(status);
        }

        _manualGameplayReady = false;
        _gameplayReadyGateSource = "blocked";
        _managerStatus = status;
        _lifecycleStatus = BuildLifecycleStatus();
    }

    private static string BuildLifecycleStatus()
    {
        return "loadStarted=" + _lifecycleLoadStartedSeen.ToString(CultureInfo.InvariantCulture) +
            "; completed=" + _lifecycleLoadingCompletedSeen.ToString(CultureInfo.InvariantCulture) +
            "; fullyDiscarded=" + _lifecycleLoadingFullyDiscardedSeen.ToString(CultureInfo.InvariantCulture);
    }

    private static bool ContainsOrdinalIgnoreCase(string value, string pattern)
    {
        return value?.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static bool AreConfigWritesAllowed()
    {
        return _allowConfigWritesAfterGameplayReady?.Value == true && _manualGameplayReady;
    }

    private static bool CanWriteConfigSetting(ConfigSettingSnapshot setting, out string blockReason)
    {
        if (setting?.Entry == null)
        {
            blockReason = "No live config entry is available for saving.";
            return false;
        }

        if (_allowConfigWritesAfterGameplayReady?.Value != true)
        {
            blockReason = "Config saves are disabled by Safety.AllowConfigWritesAfterGameplayReady.";
            return false;
        }

        if (!_manualGameplayReady)
        {
            blockReason = "Config saves are blocked until FoA log load-completion markers are observed or Mark Gameplay Ready is pressed after the current load has fully completed.";
            return false;
        }

        blockReason = string.Empty;
        return true;
    }

    private static string GetConfigWriteBlockReason()
    {
        if (_allowConfigWritesAfterGameplayReady?.Value != true)
        {
            return "Blocked by Safety.AllowConfigWritesAfterGameplayReady";
        }

        return _manualGameplayReady ? "Allowed" : "Blocked until gameplay-ready gate is armed";
    }

    private static bool IsRuntimeQuarantineEnabled()
    {
        return ForceIl2CppRuntimePatchQuarantine || _runtimeQuarantine?.Value != false;
    }

    private static string NormalizeOwnerId(string ownerId)
    {
        return CleanText(ownerId, 160, string.Empty);
    }

    private static string NormalizeProviderId(string providerId)
    {
        return CleanText(providerId, 160, string.Empty).ToLowerInvariant();
    }

    private static string CleanText(string value, int limit, string fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }

        string trimmed = value.Trim();
        return trimmed.Length <= limit ? trimmed : trimmed.Substring(0, limit);
    }

    private static class Il2CppRuntimePatches
    {
        private static readonly string[] AxisMethodNames =
        {
            "GetAxis",
            "GetAxisRaw",
            "GetAxisPrev",
            "GetAxisRawPrev"
        };

        private static readonly string[] ButtonMethodNames =
        {
            "GetButton",
            "GetButtonDown",
            "GetButtonUp",
            "GetButtonPrev",
            "GetButtonDoublePressDown",
            "GetButtonDoublePressHold",
            "GetButtonSinglePressDown",
            "GetButtonSinglePressHold",
            "GetButtonTimedPress",
            "GetButtonTimedPressDown",
            "GetButtonTimedPressUp",
            "GetAnyButton",
            "GetAnyButtonDown",
            "GetAnyButtonUp",
            "GetAnyButtonPrev"
        };

        private static FieldInfo _playerMoveInputField;
        private static FieldInfo _playerMountMoveInputField;
        private static FieldInfo _playerLookInputField;

        internal static bool PatchMenuButtons(Harmony harmony, ManualLogSource logger)
        {
            bool menuPatched = PatchMenuType(
                harmony,
                logger,
                "Awaken.TG.Main.UI.Menu.VMenuUI",
                nameof(MenuInitializePostfix));
            bool titlePatched = PatchMenuType(
                harmony,
                logger,
                "Awaken.TG.Main.UI.TitleScreen.VTitleScreenUI",
                nameof(MenuInitializePostfix));
            return menuPatched || titlePatched;
        }

        internal static bool PatchInputFreeze(Harmony harmony, ManualLogSource logger)
        {
            int patched = 0;

            if (PatchNoArgPrefix(
                harmony,
                logger,
                "Awaken.TG.MVC.UI.GameUI",
                "UpdateMousePosition",
                nameof(BlockWhenManagerOwnsInputPrefix)))
            {
                patched++;
            }

            Type playerInputType = SafeTypeByName("Awaken.TG.Main.Heroes.PlayerInput");
            if (playerInputType != null)
            {
                _playerMoveInputField = AccessTools.Field(playerInputType, "<MoveInput>k__BackingField");
                _playerMountMoveInputField = AccessTools.Field(playerInputType, "<MountMoveInput>k__BackingField");
                _playerLookInputField = AccessTools.Field(playerInputType, "<LookInput>k__BackingField");
                if (PatchNoArgPrefix(
                    harmony,
                    logger,
                    playerInputType,
                    "ProcessLateUpdate",
                    nameof(PlayerInputProcessLateUpdatePrefix)))
                {
                    patched++;
                }
            }
            else
            {
                logger.LogWarning("Could not find Awaken.TG.Main.Heroes.PlayerInput. Player input freeze patch was not applied.");
            }

            patched += PatchRewiredAxes(harmony, logger);
            patched += PatchRewiredButtons(harmony, logger);
            patched += PatchUnityKeyCodes(harmony, logger);
            return patched > 0;
        }

        private static bool PatchMenuType(Harmony harmony, ManualLogSource logger, string typeName, string postfixName)
        {
            Type viewType = SafeTypeByName(typeName);
            MethodInfo target = viewType == null ? null : AccessTools.Method(viewType, "OnInitialize");
            MethodInfo postfix = AccessTools.Method(typeof(Il2CppRuntimePatches), postfixName);
            if (viewType == null || target == null || postfix == null)
            {
                logger.LogWarning($"Could not patch {typeName}.OnInitialize for FoA Mod Manager menu button.");
                return false;
            }

            try
            {
                harmony.Patch(target, postfix: new HarmonyMethod(postfix));
                logger.LogInfo($"Patched {typeName}.OnInitialize for FoA Mod Manager menu button.");
                return true;
            }
            catch (Exception ex)
            {
                logger.LogWarning($"Could not patch {typeName}.OnInitialize: {ex.GetType().Name}: {ex.Message}");
                return false;
            }
        }

        private static bool PatchNoArgPrefix(
            Harmony harmony,
            ManualLogSource logger,
            string typeName,
            string methodName,
            string prefixName)
        {
            Type type = SafeTypeByName(typeName);
            return type != null && PatchNoArgPrefix(harmony, logger, type, methodName, prefixName);
        }

        private static bool PatchNoArgPrefix(
            Harmony harmony,
            ManualLogSource logger,
            Type type,
            string methodName,
            string prefixName)
        {
            MethodInfo target = AccessTools.Method(type, methodName);
            MethodInfo prefix = AccessTools.Method(typeof(Il2CppRuntimePatches), prefixName);
            if (target == null || prefix == null)
            {
                logger.LogWarning($"Could not patch {type.FullName}.{methodName}.");
                return false;
            }

            try
            {
                harmony.Patch(target, prefix: new HarmonyMethod(prefix));
                logger.LogInfo($"Patched {type.Name}.{methodName} for FoA Mod Manager input freeze.");
                return true;
            }
            catch (Exception ex)
            {
                logger.LogWarning($"Could not patch {type.FullName}.{methodName}: {ex.GetType().Name}: {ex.Message}");
                return false;
            }
        }

        private static int PatchRewiredAxes(Harmony harmony, ManualLogSource logger)
        {
            Type rewiredPlayerType = SafeTypeByName("Rewired.Player");
            MethodInfo prefix = AccessTools.Method(typeof(Il2CppRuntimePatches), nameof(AxisPrefix));
            if (rewiredPlayerType == null || prefix == null)
            {
                logger.LogWarning("Could not find Rewired.Player axis methods for FoA Mod Manager input freeze.");
                return 0;
            }

            int patched = 0;
            foreach (MethodInfo method in rewiredPlayerType.GetMethods(BindingFlags.Instance | BindingFlags.Public))
            {
                if (!AxisMethodNames.Contains(method.Name, StringComparer.Ordinal))
                {
                    continue;
                }

                ParameterInfo[] parameters = method.GetParameters();
                if (method.ReturnType != typeof(float) ||
                    parameters.Length != 1 ||
                    (parameters[0].ParameterType != typeof(string) && parameters[0].ParameterType != typeof(int)))
                {
                    continue;
                }

                try
                {
                    harmony.Patch(method, prefix: new HarmonyMethod(prefix));
                    patched++;
                }
                catch (Exception ex)
                {
                    logger.LogWarning($"Could not patch Rewired.Player.{method.Name}: {ex.GetType().Name}: {ex.Message}");
                }
            }

            if (patched == 0)
            {
                logger.LogWarning("No Rewired.Player axis overloads were patched for FoA Mod Manager input freeze.");
            }
            else
            {
                logger.LogInfo($"Patched {patched} Rewired.Player axis overloads for FoA Mod Manager input freeze.");
            }

            return patched;
        }

        private static int PatchRewiredButtons(Harmony harmony, ManualLogSource logger)
        {
            Type rewiredPlayerType = SafeTypeByName("Rewired.Player");
            MethodInfo fallbackPrefix = AccessTools.Method(typeof(Il2CppRuntimePatches), nameof(ButtonPrefix));
            MethodInfo stringPrefix = AccessTools.Method(typeof(Il2CppRuntimePatches), nameof(ButtonStringPrefix));
            MethodInfo intPrefix = AccessTools.Method(typeof(Il2CppRuntimePatches), nameof(ButtonIntPrefix));
            if (rewiredPlayerType == null || fallbackPrefix == null)
            {
                logger.LogWarning("Could not find Rewired.Player button methods for FoA Mod Manager input freeze.");
                return 0;
            }

            int patched = 0;
            foreach (MethodInfo method in rewiredPlayerType.GetMethods(BindingFlags.Instance | BindingFlags.Public))
            {
                if (!ButtonMethodNames.Contains(method.Name, StringComparer.Ordinal) || method.ReturnType != typeof(bool))
                {
                    continue;
                }

                HarmonyMethod prefix = CreateButtonPrefix(method, stringPrefix, intPrefix, fallbackPrefix);
                try
                {
                    harmony.Patch(method, prefix: prefix);
                    patched++;
                }
                catch (Exception ex)
                {
                    logger.LogWarning($"Could not patch Rewired.Player.{method.Name}: {ex.GetType().Name}: {ex.Message}");
                }
            }

            if (patched == 0)
            {
                logger.LogWarning("No Rewired.Player button overloads were patched for FoA Mod Manager input freeze.");
            }
            else
            {
                logger.LogInfo($"Patched {patched} Rewired.Player button overloads for FoA Mod Manager input freeze.");
            }

            return patched;
        }

        private static int PatchUnityKeyCodes(Harmony harmony, ManualLogSource logger)
        {
            MethodInfo prefix = AccessTools.Method(typeof(Il2CppRuntimePatches), nameof(UnityKeyCodePrefix));
            if (prefix == null)
            {
                return 0;
            }

            int patched = 0;
            foreach (string methodName in new[] { "GetKey", "GetKeyDown", "GetKeyUp" })
            {
                MethodInfo target = typeof(Input)
                    .GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .FirstOrDefault(method =>
                    {
                        ParameterInfo[] parameters = method.GetParameters();
                        return string.Equals(method.Name, methodName, StringComparison.Ordinal) &&
                            method.ReturnType == typeof(bool) &&
                            parameters.Length == 1 &&
                            parameters[0].ParameterType == typeof(KeyCode);
                    });

                if (target == null)
                {
                    continue;
                }

                try
                {
                    harmony.Patch(target, prefix: new HarmonyMethod(prefix));
                    patched++;
                }
                catch (Exception ex)
                {
                    logger.LogWarning($"Could not patch Unity Input.{methodName}(KeyCode): {ex.GetType().Name}: {ex.Message}");
                }
            }

            if (patched > 0)
            {
                logger.LogInfo($"Patched {patched} Unity Input KeyCode methods for FoA Mod Manager controller layer gating.");
            }

            return patched;
        }

        private static HarmonyMethod CreateButtonPrefix(
            MethodInfo method,
            MethodInfo stringPrefix,
            MethodInfo intPrefix,
            MethodInfo fallbackPrefix)
        {
            ParameterInfo[] parameters = method.GetParameters();
            if (parameters.Length > 0 && parameters[0].ParameterType == typeof(string) && stringPrefix != null)
            {
                return new HarmonyMethod(stringPrefix);
            }

            if (parameters.Length > 0 && parameters[0].ParameterType == typeof(int) && intPrefix != null)
            {
                return new HarmonyMethod(intPrefix);
            }

            return new HarmonyMethod(fallbackPrefix);
        }

        private static Type SafeTypeByName(string typeName)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    Type type = assembly.GetType(typeName, throwOnError: false, ignoreCase: false);
                    if (type != null)
                    {
                        return type;
                    }
                }
                catch (Exception ex) when (ex is TypeLoadException || ex is ReflectionTypeLoadException || ex is BadImageFormatException)
                {
                    // Some generated IL2CPP assemblies contain invalid metadata for unrelated types.
                    // Avoid Harmony's assembly-wide type scan so those types do not create startup warnings.
                }
            }

            return null;
        }

        private static void MenuInitializePostfix(object __instance)
        {
            AddManagerButton(__instance);
        }

        private static void AddManagerButton(object view)
        {
            try
            {
                if (view == null)
                {
                    return;
                }

                Type viewType = view.GetType();
                Component optionsComponent = FindMenuButtonComponent(view, viewType);
                if (optionsComponent == null || optionsComponent.gameObject == null)
                {
                    _instance?.Log.LogWarning($"{viewType.Name} options/settings button was not available. FoA Mod Manager button was not added.");
                    return;
                }

                Transform parent = optionsComponent.transform.parent;
                if (parent == null)
                {
                    _instance?.Log.LogWarning($"{viewType.Name} options button has no parent. FoA Mod Manager button was not added.");
                    return;
                }

                if (parent.Find("FoAModManagerButton") != null)
                {
                    return;
                }

                GameObject managerButtonObject = UnityEngine.Object.Instantiate(optionsComponent.gameObject, parent);
                managerButtonObject.name = "FoAModManagerButton";
                managerButtonObject.transform.SetSiblingIndex(optionsComponent.transform.GetSiblingIndex() + 1);

                Component managerButton = managerButtonObject.GetComponent(optionsComponent.GetIl2CppType()) as Component;
                if (managerButton == null)
                {
                    UnityEngine.Object.Destroy(managerButtonObject);
                    _instance?.Log.LogWarning($"{viewType.Name} cloned menu button is missing the expected button component.");
                    return;
                }

                ClearButtonEvents(managerButton);
                InvokeInitializeButton(managerButton);
            }
            catch (Exception ex)
            {
                _instance?.Log.LogWarning($"Failed to add FoA Mod Manager menu button: {ex}");
            }
        }

        private static Component FindMenuButtonComponent(object view, Type viewType)
        {
            foreach (string fieldName in new[] { "options", "settings", "optionsButton", "settingsButton" })
            {
                Component exact = ReadFieldComponent(view, viewType, fieldName);
                if (IsUsableButtonComponent(exact))
                {
                    return exact;
                }
            }

            foreach (MemberInfo member in EnumerateViewMembers(viewType, preferOptionLikeNames: true))
            {
                Component component = ReadMemberComponent(view, member);
                if (IsUsableButtonComponent(component))
                {
                    return component;
                }
            }

            foreach (MemberInfo member in EnumerateViewMembers(viewType, preferOptionLikeNames: false))
            {
                Component component = ReadMemberComponent(view, member);
                if (IsUsableButtonComponent(component))
                {
                    return component;
                }
            }

            return null;
        }

        private static Component ReadFieldComponent(object view, Type viewType, string fieldName)
        {
            try
            {
                FieldInfo field = viewType.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                return field?.GetValue(view) as Component;
            }
            catch
            {
                return null;
            }
        }

        private static IEnumerable<MemberInfo> EnumerateViewMembers(Type viewType, bool preferOptionLikeNames)
        {
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            for (Type current = viewType; current != null; current = current.BaseType)
            {
                FieldInfo[] fields;
                PropertyInfo[] properties;
                try
                {
                    fields = current.GetFields(flags | BindingFlags.DeclaredOnly);
                    properties = current.GetProperties(flags | BindingFlags.DeclaredOnly);
                }
                catch
                {
                    continue;
                }

                foreach (FieldInfo field in fields)
                {
                    if (IsCandidateButtonMember(field.Name, field.FieldType, preferOptionLikeNames))
                    {
                        yield return field;
                    }
                }

                foreach (PropertyInfo property in properties)
                {
                    if (property.GetIndexParameters().Length == 0 &&
                        IsCandidateButtonMember(property.Name, property.PropertyType, preferOptionLikeNames))
                    {
                        yield return property;
                    }
                }
            }
        }

        private static bool IsCandidateButtonMember(string memberName, Type memberType, bool preferOptionLikeNames)
        {
            string name = memberName ?? string.Empty;
            string typeName = memberType?.FullName ?? string.Empty;
            bool optionLikeName =
                name.IndexOf("option", StringComparison.OrdinalIgnoreCase) >= 0 ||
                name.IndexOf("setting", StringComparison.OrdinalIgnoreCase) >= 0;
            bool buttonLikeType =
                typeName.IndexOf("ButtonConfig", StringComparison.OrdinalIgnoreCase) >= 0 ||
                (memberType != null && typeof(Component).IsAssignableFrom(memberType));
            return preferOptionLikeNames ? optionLikeName && buttonLikeType : buttonLikeType;
        }

        private static Component ReadMemberComponent(object view, MemberInfo member)
        {
            try
            {
                return member switch
                {
                    FieldInfo field => field.GetValue(view) as Component,
                    PropertyInfo property => property.GetValue(view) as Component,
                    _ => null
                };
            }
            catch
            {
                return null;
            }
        }

        private static bool IsUsableButtonComponent(Component component)
        {
            return component != null &&
                component.gameObject != null &&
                HasInitializeButton(component);
        }

        private static bool HasInitializeButton(Component component)
        {
            return component.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Any(method =>
                {
                    if (!string.Equals(method.Name, "InitializeButton", StringComparison.Ordinal))
                    {
                        return false;
                    }

                    ParameterInfo[] parameters = method.GetParameters();
                    return parameters.Length == 2 && parameters[1].ParameterType == typeof(string);
                });
        }

        private static void ClearButtonEvents(Component managerButton)
        {
            FieldInfo buttonField = managerButton.GetType().GetField("button", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            object button = buttonField?.GetValue(managerButton);
            MethodInfo clearMethod = button == null ? null : button.GetType().GetMethod(
                "ClearAllOnClickEvents",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            clearMethod?.Invoke(button, Array.Empty<object>());
        }

        private static void InvokeInitializeButton(Component managerButton)
        {
            MethodInfo initialize = managerButton.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .FirstOrDefault(method =>
                {
                    if (!string.Equals(method.Name, "InitializeButton", StringComparison.Ordinal))
                    {
                        return false;
                    }

                    ParameterInfo[] parameters = method.GetParameters();
                    return parameters.Length == 2 && parameters[1].ParameterType == typeof(string);
                });

            if (initialize == null)
            {
                _instance?.Log.LogWarning("Cloned menu button did not expose InitializeButton(Action,string).");
                return;
            }

            initialize.Invoke(managerButton, new object[] { (Action)ShowManager, PluginName });
        }

        private static bool BlockWhenManagerOwnsInputPrefix()
        {
            return !IsModUiInputOwned;
        }

        private static bool PlayerInputProcessLateUpdatePrefix(object __instance)
        {
            if (!IsModUiInputOwned)
            {
                return true;
            }

            _playerMoveInputField?.SetValue(__instance, Vector2.zero);
            _playerMountMoveInputField?.SetValue(__instance, Vector2.zero);
            _playerLookInputField?.SetValue(__instance, Vector2.zero);
            return false;
        }

        private static bool AxisPrefix(ref float __result)
        {
            if (!IsModUiInputOwned || IsInternalControllerInputReadActive)
            {
                return true;
            }

            __result = 0f;
            return false;
        }

        private static bool ButtonPrefix(ref bool __result)
        {
            if (!IsModUiInputOwned || IsInternalControllerInputReadActive)
            {
                return true;
            }

            __result = false;
            return false;
        }

        private static bool ButtonStringPrefix(string __0, ref bool __result)
        {
            if (TryBlockOwnedUiButton(ref __result))
            {
                return false;
            }

            if (ShouldSuppressControllerActionLayerButton(__0))
            {
                __result = false;
                return false;
            }

            return true;
        }

        private static bool ButtonIntPrefix(int __0, ref bool __result)
        {
            if (TryBlockOwnedUiButton(ref __result))
            {
                return false;
            }

            if (ShouldSuppressControllerActionLayerButton(__0))
            {
                __result = false;
                return false;
            }

            return true;
        }

        private static bool UnityKeyCodePrefix(KeyCode __0, ref bool __result)
        {
            if (IsModUiInputOwned || IsInternalControllerInputReadActive)
            {
                return true;
            }

            KeyCode target = _controllerActionLayerTargetKey?.Value ?? KeyCode.None;
            KeyCode modifier = _controllerActionLayerModifierKey?.Value ?? KeyCode.None;
            if (_controllerActionLayerEnabled?.Value == true &&
                target != KeyCode.None &&
                __0 == target &&
                modifier != KeyCode.None &&
                !Input.GetKey(modifier))
            {
                __result = false;
                return false;
            }

            return true;
        }

        private static bool TryBlockOwnedUiButton(ref bool result)
        {
            if (!IsModUiInputOwned || IsInternalControllerInputReadActive)
            {
                return false;
            }

            result = false;
            return true;
        }
    }

    private sealed class StatusProviderRegistration
    {
        internal StatusProviderRegistration(
            string providerId,
            string displayName,
            string category,
            string description,
            Func<FoAModStatusSnapshot> snapshotProvider)
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
            string updatedUtc,
            string[] lines,
            bool linesWereTruncated)
        {
            Level = level;
            Summary = summary;
            Detail = detail;
            UpdatedUtc = updatedUtc;
            Lines = lines;
            LinesWereTruncated = linesWereTruncated;
        }

        internal FoAModStatusLevel Level { get; }
        internal string Summary { get; }
        internal string Detail { get; }
        internal string UpdatedUtc { get; }
        internal string[] Lines { get; }
        internal bool LinesWereTruncated { get; }
    }

    private sealed class ControllerActionRegistration
    {
        internal ControllerActionRegistration(
            string actionId,
            string displayName,
            string category,
            string description,
            Action callback)
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

    private sealed class ConfigPluginSnapshot
    {
        internal ConfigPluginSnapshot(
            string selectionKey,
            string guid,
            string displayName,
            string version,
            string location,
            string configPath,
            ConfigSettingSnapshot[] settings)
        {
            SelectionKey = selectionKey;
            Guid = guid;
            DisplayName = displayName;
            Version = version;
            Location = location;
            ConfigPath = configPath;
            Settings = settings;
        }

        internal string SelectionKey { get; }
        internal string Guid { get; }
        internal string DisplayName { get; }
        internal string Version { get; }
        internal string Location { get; }
        internal string ConfigPath { get; }
        internal ConfigSettingSnapshot[] Settings { get; }
        internal int SettingCount => Settings.Length;
    }

    private sealed class ConfigSettingSnapshot
    {
        internal ConfigSettingSnapshot(
            string id,
            string section,
            string key,
            string settingType,
            string serializedValue,
            string defaultValue,
            string description,
            ConfigEntryBase entry)
        {
            Id = id;
            Section = section;
            Key = key;
            SettingType = settingType;
            SerializedValue = serializedValue;
            DefaultValue = defaultValue;
            Description = description;
            Entry = entry;
        }

        internal string Id { get; }
        internal string Section { get; }
        internal string Key { get; }
        internal string SettingType { get; }
        internal string SerializedValue { get; }
        internal string DefaultValue { get; }
        internal string Description { get; }
        internal ConfigEntryBase Entry { get; }
    }
}

public sealed class ManagerBehaviour : MonoBehaviour
{
    public ManagerBehaviour(IntPtr pointer)
        : base(pointer)
    {
    }

    private void Update()
    {
        Plugin.UpdateFromUnity();
    }

    private void OnGUI()
    {
        Plugin.DrawFromUnity();
    }
}
