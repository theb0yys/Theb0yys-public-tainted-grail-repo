using System;
using System.Globalization;
using BepInEx.Configuration;
using BepInEx.Logging;
using UnityEngine;

namespace TaintedMusic;

internal sealed class MusicMenuScreen
{
    private const string OwnerId = Plugin.PluginGuid + ".menu";
    private const float Margin = 42f;
    private const float HeaderHeight = 104f;
    private const float TabHeight = 52f;
    private const float FooterHeight = 58f;

    private readonly ConfigFile _config;
    private readonly Func<MusicMenuSnapshot> _snapshotProvider;
    private readonly ManualLogSource _logger;

    private TaintedInterfaceBridge.SharedStyles? _styles;
    private Vector2 _scroll;
    private Page _page;
    private bool _visible;
    private bool _scopeActive;
    private bool _closeAfterDraw;
    private string _status = "Ready.";

    internal MusicMenuScreen(
        ConfigFile config,
        Func<MusicMenuSnapshot> snapshotProvider,
        ManualLogSource logger)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _snapshotProvider = snapshotProvider ?? throw new ArgumentNullException(nameof(snapshotProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    internal bool Show(string trigger)
    {
        if (_visible)
        {
            TaintedInterfaceBridge.EnsureInteractiveCursor();
            return true;
        }

        if (!TaintedInterfaceBridge.IsAvailable() || !TaintedInterfaceBridge.BeginCustomUiScope(OwnerId, freezeWorld: true))
        {
            _logger.LogWarning($"{Plugin.PluginName} separate menu could not open. Trigger={trigger}; reason=tainted-interface-scope-unavailable.");
            return false;
        }

        if (!TaintedInterfaceBridge.TryGetStyles(out _styles) || _styles == null)
        {
            TaintedInterfaceBridge.EndCustomUiScope(OwnerId);
            _logger.LogWarning($"{Plugin.PluginName} separate menu could not open. Trigger={trigger}; reason=tainted-interface-styles-unavailable.");
            return false;
        }

        _visible = true;
        _scopeActive = true;
        _page = Page.Preview;
        _scroll = Vector2.zero;
        _status = "Tainted Interface screen ready.";
        _logger.LogInfo($"{Plugin.PluginName} separate menu opened. Trigger={trigger}; Owner={OwnerId}; Renderer=Tainted Interface; FreezeWorld=true.");
        return true;
    }

    internal void Update()
    {
        if (_visible && Input.GetKeyDown(KeyCode.Escape))
        {
            Close("escape");
        }
    }

    internal void OnGUI()
    {
        if (!_visible || _styles == null)
        {
            return;
        }

        TaintedInterfaceBridge.EnsureInteractiveCursor();
        float scale = Mathf.Clamp(Screen.height / 1200f, 1f, 1.5f);
        float width = Screen.width / scale;
        float height = Screen.height / scale;
        Matrix4x4 previousMatrix = GUI.matrix;
        int previousDepth = GUI.depth;
        try
        {
            GUI.depth = -1000;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
            Rect screen = new Rect(0f, 0f, width, height);
            GUI.Box(screen, GUIContent.none, _styles.Window);
            GUILayout.BeginArea(new Rect(Margin, Margin, width - (Margin * 2f), height - (Margin * 2f)));
            DrawScreen(width - (Margin * 2f), height - (Margin * 2f));
            GUILayout.EndArea();
        }
        finally
        {
            GUI.matrix = previousMatrix;
            GUI.depth = previousDepth;
        }

        if (_closeAfterDraw)
        {
            Close("close-button");
        }
    }

    internal void Close(string trigger)
    {
        if (!_visible && !_scopeActive)
        {
            return;
        }

        if (_scopeActive)
        {
            TaintedInterfaceBridge.EndCustomUiScope(OwnerId);
        }

        _visible = false;
        _scopeActive = false;
        _closeAfterDraw = false;
        _logger.LogInfo($"{Plugin.PluginName} separate menu closed. Trigger={trigger}; Owner={OwnerId}; Renderer=Tainted Interface.");
    }

    private void DrawScreen(float width, float height)
    {
        TaintedInterfaceBridge.SharedStyles styles = _styles!;
        MusicMenuSnapshot snapshot = _snapshotProvider();

        GUILayout.BeginVertical(styles.Header, GUILayout.Height(HeaderHeight));
        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();
        GUILayout.Label("Tainted Music", styles.Title);
        GUILayout.Label("Live music preview and settings", styles.MutedLabel);
        GUILayout.EndVertical();
        GUILayout.FlexibleSpace();
        GUILayout.Label(snapshot.Summary, styles.Label, GUILayout.MaxWidth(Mathf.Max(360f, width * 0.44f)));
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();

        GUILayout.Space(10f);
        DrawTabs(styles);
        GUILayout.Space(10f);

        float contentHeight = Mathf.Max(260f, height - HeaderHeight - TabHeight - FooterHeight - 52f);
        _scroll = GUILayout.BeginScrollView(_scroll, false, true, GUILayout.Height(contentHeight));
        switch (_page)
        {
            case Page.Playback:
                DrawPlaybackSettings(styles);
                break;
            case Page.Mix:
                DrawMusicMixSettings(styles);
                break;
            case Page.Dynamics:
                DrawDynamicsSettings(styles);
                break;
            default:
                DrawPreview(snapshot, styles);
                break;
        }

        GUILayout.EndScrollView();
        GUILayout.Space(10f);
        DrawFooter(styles);
    }

    private void DrawTabs(TaintedInterfaceBridge.SharedStyles styles)
    {
        GUILayout.BeginHorizontal(styles.Panel, GUILayout.Height(TabHeight));
        DrawTab("Preview", Page.Preview, styles);
        DrawTab("Playback", Page.Playback, styles);
        DrawTab("Music Mix", Page.Mix, styles);
        DrawTab("Dynamics & Native Audio", Page.Dynamics, styles);
        GUILayout.EndHorizontal();
    }

    private void DrawTab(string label, Page page, TaintedInterfaceBridge.SharedStyles styles)
    {
        if (GUILayout.Button(label, _page == page ? styles.Button : styles.SecondaryButton, GUILayout.Height(38f)))
        {
            _page = page;
            _scroll = Vector2.zero;
            _status = label + " page selected.";
        }
    }

    private static void DrawPreview(MusicMenuSnapshot snapshot, TaintedInterfaceBridge.SharedStyles styles)
    {
        GUILayout.BeginVertical(styles.Panel);
        GUILayout.Label("Now Playing", styles.Title);
        GUILayout.Label(snapshot.Detail, styles.MutedLabel);
        GUILayout.Space(10f);

        foreach (string line in snapshot.Lines)
        {
            GUILayout.BeginHorizontal(styles.Card, GUILayout.MinHeight(44f));
            GUILayout.Label(line, styles.Label);
            GUILayout.EndHorizontal();
            GUILayout.Space(5f);
        }

        GUILayout.EndVertical();
    }

    private void DrawPlaybackSettings(TaintedInterfaceBridge.SharedStyles styles)
    {
        BeginSettingsGroup("Playback", "Master controls for the Tainted Music system.", styles);
        DrawToggle("General", "Enabled", "Tainted Music", styles);
        DrawFloat("Audio", "GlobalVolume", "Master Volume", 0f, 1f, 0.05f, ValueFormat.Percent, styles);
        DrawFloat("Audio", "FadeSeconds", "Lane Crossfade", 0.1f, 30f, 0.5f, ValueFormat.Seconds, styles);
        GUILayout.EndVertical();
    }

    private void DrawMusicMixSettings(TaintedInterfaceBridge.SharedStyles styles)
    {
        BeginSettingsGroup("Music Mix", "Tune or mute each environment lane independently.", styles);
        DrawLane("Wyrdness", "WyrdnessVolume", "WyrdnessMuted", styles);
        DrawLane("Day Open World", "DayOpenWorldVolume", "DayOpenWorldMuted", styles);
        DrawLane("Interior", "InteriorVolume", "InteriorMuted", styles);
        DrawLane("Settlement", "SettlementVolume", "SettlementMuted", styles);
        DrawLane("Scary Place", "ScaryPlaceVolume", "ScaryPlaceMuted", styles);
        GUILayout.EndVertical();
    }

    private void DrawLane(
        string label,
        string volumeKey,
        string muteKey,
        TaintedInterfaceBridge.SharedStyles styles)
    {
        DrawFloat("Audio", volumeKey, label + " Volume", 0f, 1f, 0.05f, ValueFormat.Percent, styles);
        DrawToggle("Audio", muteKey, "Mute " + label, styles);
    }

    private void DrawDynamicsSettings(TaintedInterfaceBridge.SharedStyles styles)
    {
        BeginSettingsGroup("Dynamics & Native Audio", "Control dialogue/combat ducking and native-audio arbitration.", styles);
        DrawToggle("Native Suppression", "NativeMusicSuppressionEnabled", "Suppress Native Music", styles);
        DrawToggle("Native Suppression", "AsylumAmbientSuppressionEnabled", "Suppress Asylum Ambience", styles);
        DrawToggle("Context Ducking", "Enabled", "Context Ducking", styles);
        DrawToggle("Context Ducking", "DialogueDuckingEnabled", "Duck During Dialogue", styles);
        DrawFloat("Context Ducking", "DialogueVolumeMultiplier", "Dialogue Music Level", 0f, 1f, 0.05f, ValueFormat.Percent, styles);
        DrawToggle("Context Ducking", "CombatDuckingEnabled", "Duck During Combat", styles);
        DrawFloat("Context Ducking", "CombatVolumeMultiplier", "Combat Music Level", 0f, 1f, 0.05f, ValueFormat.Percent, styles);
        DrawFloat("Context Ducking", "CombatHoldSeconds", "Combat Duck Duration", 0f, 30f, 0.5f, ValueFormat.Seconds, styles);
        GUILayout.EndVertical();
    }

    private static void BeginSettingsGroup(
        string title,
        string description,
        TaintedInterfaceBridge.SharedStyles styles)
    {
        GUILayout.BeginVertical(styles.Panel);
        GUILayout.Label(title, styles.Title);
        GUILayout.Label(description, styles.MutedLabel);
        GUILayout.Space(10f);
    }

    private void DrawToggle(
        string section,
        string key,
        string label,
        TaintedInterfaceBridge.SharedStyles styles)
    {
        if (!_config.TryGetEntry(section, key, out ConfigEntry<bool> entry))
        {
            return;
        }

        GUILayout.BeginHorizontal(styles.Card, GUILayout.MinHeight(48f));
        GUILayout.Label(label, styles.Label);
        GUILayout.FlexibleSpace();
        string state = entry.Value ? "Enabled" : "Disabled";
        if (GUILayout.Button(state, entry.Value ? styles.Button : styles.SecondaryButton, GUILayout.Width(150f), GUILayout.Height(34f)))
        {
            entry.Value = !entry.Value;
            SaveSetting(label, entry.Value ? "Enabled" : "Disabled");
        }

        GUILayout.EndHorizontal();
        GUILayout.Space(5f);
    }

    private void DrawFloat(
        string section,
        string key,
        string label,
        float minimum,
        float maximum,
        float step,
        ValueFormat format,
        TaintedInterfaceBridge.SharedStyles styles)
    {
        if (!_config.TryGetEntry(section, key, out ConfigEntry<float> entry))
        {
            return;
        }

        GUILayout.BeginHorizontal(styles.Card, GUILayout.MinHeight(48f));
        GUILayout.Label(label, styles.Label);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("-", styles.SecondaryButton, GUILayout.Width(58f), GUILayout.Height(34f)))
        {
            entry.Value = Mathf.Clamp(entry.Value - step, minimum, maximum);
            SaveSetting(label, FormatValue(entry.Value, format));
        }

        GUILayout.Label(FormatValue(entry.Value, format), styles.Label, GUILayout.Width(110f));
        if (GUILayout.Button("+", styles.Button, GUILayout.Width(58f), GUILayout.Height(34f)))
        {
            entry.Value = Mathf.Clamp(entry.Value + step, minimum, maximum);
            SaveSetting(label, FormatValue(entry.Value, format));
        }

        GUILayout.EndHorizontal();
        GUILayout.Space(5f);
    }

    private void SaveSetting(string label, string value)
    {
        _config.Save();
        _status = label + ": " + value;
    }

    private static string FormatValue(float value, ValueFormat format)
    {
        return format == ValueFormat.Percent
            ? Math.Round(value * 100f).ToString(CultureInfo.InvariantCulture) + "%"
            : value.ToString("0.0", CultureInfo.InvariantCulture) + " s";
    }

    private void DrawFooter(TaintedInterfaceBridge.SharedStyles styles)
    {
        GUILayout.BeginHorizontal(styles.Panel, GUILayout.Height(FooterHeight));
        GUILayout.Label(_status, styles.MutedLabel);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Close  Esc", styles.SecondaryButton, GUILayout.Width(160f), GUILayout.Height(40f)))
        {
            _closeAfterDraw = true;
        }

        GUILayout.EndHorizontal();
    }

    private enum Page
    {
        Preview,
        Playback,
        Mix,
        Dynamics
    }

    private enum ValueFormat
    {
        Percent,
        Seconds
    }
}
