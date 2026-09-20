using System;
using System.Globalization;
using BepInEx.Logging;
using UnityEngine;

namespace TaintedPerformance;

internal sealed class PerformanceOverviewScreen
{
    private const string ScreenId = Plugin.PluginGuid + ".overview";
    private const int WindowId = 9913417;
    private const float Margin = 32f;
    private const float MinimumWidth = 1080f;
    private const float MinimumHeight = 700f;
    private const float MaximumWidth = 2020f;
    private const float MaximumHeight = 1260f;
    private const float ContentPadding = 24f;
    private const float HeaderHeight = 78f;
    private const float TabBarHeight = 46f;
    private const float FooterHeight = 56f;
    private const float Gap = 12f;
    private const float PanelPadding = 16f;
    private const float DecorativeTextInset = 28f;
    private const float FullPageDecorativeTextInset = 52f;
    private const float PanelHeaderHeight = 42f;
    private const float RowHeight = 40f;
    private const float TallRowHeight = 54f;
    private const float MetricHeight = 112f;
    private const float TimelineHeight = 226f;

    private readonly TaintedPerformanceConfig _config;
    private readonly RuntimePerformanceWatcher _watcher;
    private readonly ManualLogSource _logger;

    private Rect _rect;
    private Vector2 _scroll;
    private Page _page;
    private bool _visible;
    private bool _taintedInterfaceScopeActive;
    private bool _managerScopeActive;
    private bool _scopeLogged;
    private bool _closeAfterDraw;
    private string _status = "Ready.";
    private Styles? _styles;
    private float _drawWidth;
    private float _drawHeight;

    internal PerformanceOverviewScreen(
        TaintedPerformanceConfig config,
        RuntimePerformanceWatcher watcher,
        ManualLogSource logger)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _watcher = watcher ?? throw new ArgumentNullException(nameof(watcher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    internal void Show()
    {
        if (!_config.OverviewPanelEnabled.Value)
        {
            _status = "Overview screen is disabled in config.";
            return;
        }

        _visible = true;
        EnsurePlacement();
        AcquireScope();
    }

    internal void Update()
    {
        if (!_config.OverviewPanelEnabled.Value)
        {
            Close();
            return;
        }

        if (Input.GetKeyDown(_config.OverviewPanelHotkey.Value))
        {
            if (_visible)
            {
                Close();
            }
            else
            {
                Show();
            }
        }

        if (_visible && Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
        }
    }

    internal void OnGUI()
    {
        if (!_visible)
        {
            return;
        }

        EnsurePlacement();
        AcquireScope();
        TaintedInterfaceBridge.EnsureInteractiveCursor();
        Styles styles = EnsureStyles();
        float uiScale = CalculateUiScale();
        Rect logicalRect = new Rect(
            _rect.x / uiScale,
            _rect.y / uiScale,
            _rect.width / uiScale,
            _rect.height / uiScale);
        _drawWidth = logicalRect.width;
        _drawHeight = logicalRect.height;

        Matrix4x4 previousMatrix = GUI.matrix;
        try
        {
            GUI.matrix = Matrix4x4.Scale(new Vector3(uiScale, uiScale, 1f));
            logicalRect = GUI.Window(WindowId, logicalRect, (GUI.WindowFunction)DrawWindow, string.Empty, styles.Window);
        }
        finally
        {
            GUI.matrix = previousMatrix;
        }

        _rect = new Rect(
            logicalRect.x * uiScale,
            logicalRect.y * uiScale,
            logicalRect.width * uiScale,
            logicalRect.height * uiScale);
        if (_closeAfterDraw)
        {
            Close();
        }
    }

    internal void Close()
    {
        if (!_visible && !_taintedInterfaceScopeActive && !_managerScopeActive)
        {
            return;
        }

        if (_taintedInterfaceScopeActive || TaintedInterfaceBridge.IsScreenScopeOpen(ScreenId))
        {
            TaintedInterfaceBridge.CloseScreenScope(ScreenId);
        }

        if (_managerScopeActive)
        {
            FoAModManagerBridge.SetCustomUiScope(ScreenId, active: false, _config.FreezeWorldWhileOverviewOpen.Value);
        }

        _visible = false;
        _taintedInterfaceScopeActive = false;
        _managerScopeActive = false;
        _closeAfterDraw = false;
    }

    private void DrawWindow(int id)
    {
        PerformanceRuntimeOverview overview = _watcher.GetOverview();
        Styles styles = EnsureStyles();

        GUI.Box(new Rect(0f, 0f, _drawWidth, _drawHeight), string.Empty, styles.Window);
        DrawHeader(overview, styles, _drawWidth);

        float tabsY = HeaderHeight + 8f;
        DrawTabs(styles, _drawWidth, tabsY);

        float contentTop = tabsY + TabBarHeight + 14f;
        float footerTop = _drawHeight - FooterHeight - 14f;
        Rect viewport = new Rect(
            ContentPadding,
            contentTop,
            _drawWidth - (ContentPadding * 2f),
            Mathf.Max(180f, footerTop - contentTop - 10f));
        float viewWidth = Mathf.Max(480f, viewport.width - 20f);
        float viewHeight = Mathf.Max(viewport.height, MeasurePageHeight(overview, viewWidth));

        _scroll = GUI.BeginScrollView(viewport, _scroll, new Rect(0f, 0f, viewWidth, viewHeight));
        DrawPage(overview, styles, viewWidth, viewHeight);
        GUI.EndScrollView();

        DrawFooter(new Rect(ContentPadding, footerTop, _drawWidth - (ContentPadding * 2f), FooterHeight), styles);
        GUI.DragWindow(new Rect(0f, 0f, _drawWidth, HeaderHeight));
    }

    private void DrawHeader(PerformanceRuntimeOverview overview, Styles styles, float width)
    {
        GUI.Box(new Rect(0f, 0f, width, HeaderHeight), string.Empty, styles.Header);

        float badgeWidth = 150f;
        Rect titleRect = new Rect(ContentPadding, 12f, width - (ContentPadding * 2f) - badgeWidth - 18f, 34f);
        Rect badgeRect = new Rect(width - ContentPadding - badgeWidth, 16f, badgeWidth, 30f);
        GUI.Label(titleRect, "Tainted Performance", styles.Title);
        GUI.Box(badgeRect, string.Empty, overview.CurrentLowFpsWarning ? styles.BadgeWarningBox : styles.BadgeGoodBox);
        GUI.Label(badgeRect, overview.StatusLevel, overview.CurrentLowFpsWarning ? styles.Warning : styles.Good);
        GUI.Label(new Rect(ContentPadding, 48f, width - (ContentPadding * 2f), 24f), overview.Summary, styles.HeaderMeta);
        GUI.Box(new Rect(ContentPadding, HeaderHeight - 5f, width - (ContentPadding * 2f), 4f), string.Empty, styles.Divider);
    }

    private void DrawTabs(Styles styles, float width, float y)
    {
        Rect bar = new Rect(ContentPadding, y, width - (ContentPadding * 2f), TabBarHeight);
        GUI.Box(bar, string.Empty, styles.TabBar);

        const float tabGap = 6f;
        float tabWidth = (bar.width - 16f - (tabGap * 5f)) / 6f;
        float tabX = bar.x + 8f;
        float tabY = bar.y + 6f;
        DrawTab(new Rect(tabX, tabY, tabWidth, 34f), "Overview", Page.Overview, styles);
        tabX += tabWidth + tabGap;
        DrawTab(new Rect(tabX, tabY, tabWidth, 34f), "Timing", Page.Timing, styles);
        tabX += tabWidth + tabGap;
        DrawTab(new Rect(tabX, tabY, tabWidth, 34f), "Memory", Page.Memory, styles);
        tabX += tabWidth + tabGap;
        DrawTab(new Rect(tabX, tabY, tabWidth, 34f), "System", Page.Native, styles);
        tabX += tabWidth + tabGap;
        DrawTab(new Rect(tabX, tabY, tabWidth, 34f), "Mod Stack", Page.Mods, styles);
        tabX += tabWidth + tabGap;
        DrawTab(new Rect(tabX, tabY, tabWidth, 34f), "Reports", Page.Reports, styles);
    }

    private void DrawTab(Rect rect, string label, Page page, Styles styles)
    {
        if (GUI.Button(rect, label, _page == page ? styles.TabActive : styles.Tab))
        {
            _page = page;
            _scroll = new Vector2(0f, 0f);
        }
    }

    private void DrawPage(PerformanceRuntimeOverview overview, Styles styles, float width, float availableHeight)
    {
        switch (_page)
        {
            case Page.Timing:
                DrawTiming(overview, styles, width, availableHeight);
                break;
            case Page.Memory:
                DrawMemory(overview, styles, width, availableHeight);
                break;
            case Page.Native:
                DrawNative(overview, styles, width, availableHeight);
                break;
            case Page.Mods:
                DrawMods(overview, styles, width, availableHeight);
                break;
            case Page.Reports:
                DrawReports(overview, styles, width, availableHeight);
                break;
            default:
                DrawOverview(overview, styles, width, availableHeight);
                break;
        }
    }

    private float DrawOverview(PerformanceRuntimeOverview overview, Styles styles, float width, float availableHeight)
    {
        float y = 0f;
        DrawMetricStrip(overview, styles, width, ref y);
        DrawTimeline(overview, styles, width, ref y);

        float columnWidth = (width - Gap) * 0.5f;
        float panelHeight = Mathf.Max(MeasureOverviewPanelHeight(overview, columnWidth), availableHeight - y);
        DrawFrameWindowPanel(overview, styles, new Rect(0f, y, columnWidth, panelHeight));
        DrawAssessmentPanel(overview, styles, new Rect(columnWidth + Gap, y, columnWidth, panelHeight));
        return y + panelHeight;
    }

    private void DrawMetricStrip(PerformanceRuntimeOverview overview, Styles styles, float width, ref float y)
    {
        float tileWidth = (width - (Gap * 4f)) * 0.2f;
        DrawMetric(
            new Rect(0f, y, tileWidth, MetricHeight),
            "Average FPS",
            RuntimePerformanceWatcher.FormatNumber(overview.AverageFramesPerSecond),
            "current window",
            styles);
        DrawMetric(
            new Rect(tileWidth + Gap, y, tileWidth, MetricHeight),
            "1% Low FPS",
            RuntimePerformanceWatcher.FormatNumber(overview.OnePercentLowFramesPerSecond),
            "slowest one percent",
            styles);
        DrawMetric(
            new Rect((tileWidth + Gap) * 2f, y, tileWidth, MetricHeight),
            "Frame p95",
            RuntimePerformanceWatcher.FormatNumber(overview.P95FrameMilliseconds) + " ms",
            "slow-frame shape",
            styles);
        DrawMetric(
            new Rect((tileWidth + Gap) * 3f, y, tileWidth, MetricHeight),
            "Frame p99",
            RuntimePerformanceWatcher.FormatNumber(overview.P99FrameMilliseconds) + " ms",
            "tail latency",
            styles);
        DrawMetric(
            new Rect((tileWidth + Gap) * 4f, y, tileWidth, MetricHeight),
            "Worst Frame",
            RuntimePerformanceWatcher.FormatNumber(overview.MaximumFrameMilliseconds) + " ms",
            "largest sample",
            styles);
        y += MetricHeight + Gap;
    }

    private void DrawTimeline(
        PerformanceRuntimeOverview overview,
        Styles styles,
        float width,
        ref float y)
    {
        Rect panel = new Rect(0f, y, width, TimelineHeight);
        float contentY = DrawPanelShell(panel, "Live Frame-Time History", styles);
        Rect graph = new Rect(
            panel.x + PanelPadding,
            contentY + 2f,
            panel.width - (PanelPadding * 2f),
            128f);
        GUI.Box(graph, string.Empty, styles.GraphBackground);

        double[] timeline = overview.FrameTimelineMilliseconds;
        if (timeline.Length == 0)
        {
            GUI.Label(graph, "Collecting frame samples...", styles.Body);
        }
        else
        {
            double graphMaximum = Math.Max(
                33.333d,
                Math.Min(200d, Math.Max(overview.MaximumFrameMilliseconds, overview.ReferenceFrameMilliseconds * 2d)));
            float barWidth = Mathf.Max(1f, graph.width / timeline.Length);
            for (int i = 0; i < timeline.Length; i++)
            {
                double frameMilliseconds = Math.Min(graphMaximum, Math.Max(0d, timeline[i]));
                float barHeight = (float)(frameMilliseconds / graphMaximum * (graph.height - 4f));
                GUIStyle barStyle = frameMilliseconds >= 100d
                    ? styles.GraphSevere
                    : frameMilliseconds >= 50d ||
                      (overview.ReferenceFrameMilliseconds > 0d && frameMilliseconds > overview.ReferenceFrameMilliseconds)
                        ? styles.GraphSpike
                        : styles.GraphBar;
                GUI.Box(
                    new Rect(
                        graph.x + (i * barWidth),
                        graph.y + graph.height - barHeight - 2f,
                        Mathf.Max(1f, barWidth - 1f),
                        Mathf.Max(1f, barHeight)),
                    string.Empty,
                    barStyle);
            }

            if (overview.ReferenceFrameMilliseconds > 0d)
            {
                float budgetY = graph.y + graph.height - (float)Math.Min(
                    graph.height - 2f,
                    overview.ReferenceFrameMilliseconds / graphMaximum * graph.height);
                GUI.Box(new Rect(graph.x, budgetY, graph.width, 2f), string.Empty, styles.GraphBudget);
            }
        }

        string caption = string.Format(
            CultureInfo.InvariantCulture,
            "{0} samples / {1:0.###} s     reference {2:0.###} FPS ({3:0.###} ms)     misses {4} ({5:0.###}%)     stutters {6} / severe {7}",
            overview.SampleCount,
            overview.WindowDurationSeconds,
            overview.ReferenceFramesPerSecond,
            overview.ReferenceFrameMilliseconds,
            overview.ReferenceBudgetMissCount,
            overview.ReferenceBudgetMissPercentage,
            overview.StutterCount,
            overview.SevereStutterCount);
        GUI.Label(
            new Rect(panel.x + PanelPadding, graph.y + graph.height + 8f, graph.width, 28f),
            caption,
            styles.MetricCaption);
        y += TimelineHeight + Gap;
    }

    private void DrawMetric(Rect rect, string label, string value, string caption, Styles styles)
    {
        GUI.Box(rect, string.Empty, styles.Card);
        GUI.Label(new Rect(rect.x + DecorativeTextInset, rect.y + 12f, rect.width - (DecorativeTextInset * 2f), 22f), label, styles.MetricLabel);
        GUI.Label(new Rect(rect.x + DecorativeTextInset, rect.y + 38f, rect.width - (DecorativeTextInset * 2f), 36f), DisplayValue(value), styles.MetricValue);
        GUI.Label(new Rect(rect.x + DecorativeTextInset, rect.y + 78f, rect.width - (DecorativeTextInset * 2f), 24f), caption, styles.MetricCaption);
    }

    private void DrawFrameWindowPanel(PerformanceRuntimeOverview overview, Styles styles, Rect rect)
    {
        float y = DrawPanelShell(rect, "Frame Window", styles);
        float rowWidth = rect.width - (PanelPadding * 2f);
        float x = rect.x + PanelPadding;
        DrawRow(ref y, x, rowWidth, "Window", RuntimePerformanceWatcher.FormatNumber(overview.WindowDurationSeconds) + " s", styles);
        DrawRow(ref y, x, rowWidth, "CPU avg", RuntimePerformanceWatcher.FormatOptionalMilliseconds(overview.AverageCpuFrameMilliseconds), styles);
        DrawRow(ref y, x, rowWidth, "GPU avg", RuntimePerformanceWatcher.FormatOptionalMilliseconds(overview.AverageGpuFrameMilliseconds), styles);
        DrawRow(ref y, x, rowWidth, "GC collections", overview.Gen0Collections + " / " + overview.Gen1Collections + " / " + overview.Gen2Collections, styles);
        DrawRow(ref y, x, rowWidth, "Alloc total / max", FormatBytes(overview.TotalManagedMainThreadAllocationBytes) + " / " + FormatBytes(overview.MaximumManagedMainThreadAllocationBytes), styles, TallRowHeight, wrapValue: true);
    }

    private void DrawAssessmentPanel(PerformanceRuntimeOverview overview, Styles styles, Rect rect)
    {
        float y = DrawPanelShell(rect, "Assessment", styles);
        float rowWidth = rect.width - (PanelPadding * 2f);
        float x = rect.x + PanelPadding;
        DrawParagraph(ref y, x, rowWidth, overview.Detail, styles);
        DrawRow(ref y, x, rowWidth, "Symptom", overview.SymptomId, styles, TallRowHeight, wrapValue: true);
        DrawRow(ref y, x, rowWidth, "Confidence", overview.AssessmentConfidence, styles);
        DrawParagraph(ref y, x, rowWidth, overview.BestNextCheck, styles, minimumHeight: 68f, maximumHeight: 126f);
    }

    private float DrawNative(PerformanceRuntimeOverview overview, Styles styles, float width, float availableHeight)
    {
        Rect panel = new Rect(0f, 0f, width, Mathf.Max(MeasureNativePanelHeight(overview, width), availableHeight));
        float y = DrawPanelShell(panel, "Game Settings", styles);
        float rowWidth = panel.width - (PanelPadding * 2f);
        float x = panel.x + PanelPadding;
        DrawRow(ref y, x, rowWidth, "Scene", overview.SceneName, styles);
        DrawRow(ref y, x, rowWidth, "Game / Unity", overview.GameVersion + " / " + overview.UnityVersion, styles, TallRowHeight, wrapValue: true);
        DrawRow(ref y, x, rowWidth, "Display", overview.ScreenDescription, styles, TallRowHeight, wrapValue: true);
        DrawRow(ref y, x, rowWidth, "Quality", overview.QualityLevel, styles);
        DrawRow(ref y, x, rowWidth, "VSync / target FPS", overview.VSyncCount + " / " + overview.TargetFrameRate, styles);
        DrawRow(ref y, x, rowWidth, "Frame timing", overview.FrameTimingStatus, styles, TallRowHeight, wrapValue: true);
        DrawParagraph(ref y, x, rowWidth, overview.NativeInvestigation, styles, minimumHeight: 72f, maximumHeight: 132f);
        DrawParagraph(ref y, x, rowWidth, overview.GameSettingsRecommendation, styles, minimumHeight: 62f, maximumHeight: 112f);
        return panel.height;
    }

    private float DrawTiming(PerformanceRuntimeOverview overview, Styles styles, float width, float availableHeight)
    {
        Rect panel = new Rect(0f, 0f, width, Mathf.Max(596f, availableHeight));
        float y = DrawPanelShell(panel, "Frame Pacing & CPU/GPU Timing", styles);
        float rowWidth = panel.width - (PanelPadding * 2f);
        float x = panel.x + PanelPadding;
        DrawRow(ref y, x, rowWidth, "Reference budget", RuntimePerformanceWatcher.FormatNumber(overview.ReferenceFramesPerSecond) + " FPS / " + RuntimePerformanceWatcher.FormatNumber(overview.ReferenceFrameMilliseconds) + " ms", styles);
        DrawRow(ref y, x, rowWidth, "Median / p95 / p99", RuntimePerformanceWatcher.FormatNumber(overview.MedianFrameMilliseconds) + " / " + RuntimePerformanceWatcher.FormatNumber(overview.P95FrameMilliseconds) + " / " + RuntimePerformanceWatcher.FormatNumber(overview.P99FrameMilliseconds) + " ms", styles, TallRowHeight, wrapValue: true);
        DrawRow(ref y, x, rowWidth, "Budget misses", overview.ReferenceBudgetMissCount + " (" + RuntimePerformanceWatcher.FormatNumber(overview.ReferenceBudgetMissPercentage) + "%)", styles);
        DrawRow(ref y, x, rowWidth, "CPU avg / max", RuntimePerformanceWatcher.FormatOptionalMilliseconds(overview.AverageCpuFrameMilliseconds) + " / " + RuntimePerformanceWatcher.FormatOptionalMilliseconds(overview.MaximumCpuFrameMilliseconds), styles, TallRowHeight, wrapValue: true);
        DrawRow(ref y, x, rowWidth, "CPU timing coverage", overview.CpuTimingSampleCount + " / " + overview.SampleCount + " samples", styles);
        DrawRow(ref y, x, rowWidth, "GPU avg / max", RuntimePerformanceWatcher.FormatOptionalMilliseconds(overview.AverageGpuFrameMilliseconds) + " / " + RuntimePerformanceWatcher.FormatOptionalMilliseconds(overview.MaximumGpuFrameMilliseconds), styles, TallRowHeight, wrapValue: true);
        DrawRow(ref y, x, rowWidth, "GPU timing coverage", overview.GpuTimingSampleCount + " / " + overview.SampleCount + " samples", styles);
        DrawParagraph(ref y, x, rowWidth, "CPU/GPU timings are broad Unity counters. They help separate correlated frame pressure, but they do not identify an exact game subsystem, method, asset, or mod.", styles, minimumHeight: 72f, maximumHeight: 96f);
        return panel.height;
    }

    private float DrawMemory(PerformanceRuntimeOverview overview, Styles styles, float width, float availableHeight)
    {
        Rect panel = new Rect(0f, 0f, width, Mathf.Max(530f, availableHeight));
        float y = DrawPanelShell(panel, "Managed Memory & Stability", styles);
        float rowWidth = panel.width - (PanelPadding * 2f);
        float x = panel.x + PanelPadding;
        DrawRow(ref y, x, rowWidth, "Allocation counter", overview.AllocationCounterStatus, styles);
        DrawRow(ref y, x, rowWidth, "Main-thread allocation rate", FormatByteRate(overview.ManagedMainThreadAllocationBytesPerSecond), styles);
        DrawRow(ref y, x, rowWidth, "Window allocation", FormatBytes(overview.TotalManagedMainThreadAllocationBytes), styles);
        DrawRow(ref y, x, rowWidth, "Largest frame allocation", FormatBytes(overview.MaximumManagedMainThreadAllocationBytes), styles);
        DrawRow(ref y, x, rowWidth, "Allocation coverage", overview.AllocationSampleCount + " / " + overview.SampleCount + " samples", styles);
        DrawRow(ref y, x, rowWidth, "GC collections (0 / 1 / 2)", overview.Gen0Collections + " / " + overview.Gen1Collections + " / " + overview.Gen2Collections, styles);
        DrawRow(ref y, x, rowWidth, "Stutters >=50 / >=100 ms", overview.StutterCount + " / " + overview.SevereStutterCount + " (" + RuntimePerformanceWatcher.FormatNumber(overview.StutterPercentage) + "% >=50 ms)", styles, TallRowHeight, wrapValue: true);
        DrawParagraph(ref y, x, rowWidth, "Allocation and GC activity are correlations inside this sample window. Use a controlled comparison and a profiler before assigning cause.", styles, minimumHeight: 62f, maximumHeight: 86f);
        return panel.height;
    }

    private float DrawMods(PerformanceRuntimeOverview overview, Styles styles, float width, float availableHeight)
    {
        Rect panel = new Rect(0f, 0f, width, Mathf.Max(MeasureModsPanelHeight(overview, width), availableHeight));
        float y = DrawPanelShell(panel, "Mod Context", styles);
        float rowWidth = panel.width - (PanelPadding * 2f);
        float x = panel.x + PanelPadding;
        DrawRow(ref y, x, rowWidth, "Loaded plugins", overview.LoadedPluginCount.ToString(CultureInfo.InvariantCulture), styles);
        DrawRow(ref y, x, rowWidth, "Other plugins", overview.OtherLoadedPluginCount.ToString(CultureInfo.InvariantCulture), styles);
        DrawParagraph(ref y, x, rowWidth, overview.ModStackInvestigation, styles, minimumHeight: 72f, maximumHeight: 132f);
        DrawParagraph(ref y, x, rowWidth, overview.ModManagerRecommendation, styles, minimumHeight: 62f, maximumHeight: 112f);

        Rect buttonRect = new Rect(x, y + 4f, 236f, 42f);
        if (GUI.Button(buttonRect, "Open FoA Mod Manager", styles.SecondaryButton))
        {
            OpenManager();
        }

        return panel.height;
    }

    private float DrawReports(PerformanceRuntimeOverview overview, Styles styles, float width, float availableHeight)
    {
        Rect panel = new Rect(0f, 0f, width, Mathf.Max(MeasureReportsPanelHeight(overview, width), availableHeight));
        float y = DrawPanelShell(panel, "Report Output", styles);
        float rowWidth = panel.width - (PanelPadding * 2f);
        float x = panel.x + PanelPadding;
        DrawRow(ref y, x, rowWidth, "Last status", overview.LastReportStatus, styles, TallRowHeight, wrapValue: true);
        DrawRow(ref y, x, rowWidth, "Last report", string.IsNullOrWhiteSpace(overview.LastReportId) ? "none" : overview.LastReportId, styles);
        DrawRow(ref y, x, rowWidth, "Trigger", string.IsNullOrWhiteSpace(overview.LastReportTriggerKind) ? "none" : overview.LastReportTriggerKind, styles);
        DrawRow(ref y, x, rowWidth, "Folder", string.IsNullOrWhiteSpace(overview.LastReportFolder) ? "none" : overview.LastReportFolder, styles, TallRowHeight, wrapValue: true);

        Rect buttonRect = new Rect(x, y + 4f, 196f, 42f);
        if (GUI.Button(buttonRect, "Write Report Now", styles.SecondaryButton))
        {
            _status = _watcher.WriteManualReport() ? "Manual report written." : "Manual report failed. Check BepInEx log.";
        }

        return panel.height;
    }

    private float DrawPanelShell(Rect rect, string title, Styles styles)
    {
        float titleInset = _page == Page.Overview ? PanelPadding : FullPageDecorativeTextInset;
        GUI.Box(rect, string.Empty, styles.Panel);
        GUI.Label(new Rect(rect.x + titleInset, rect.y + 10f, rect.width - (titleInset * 2f), 24f), title, styles.Section);
        GUI.Box(new Rect(rect.x + PanelPadding, rect.y + PanelHeaderHeight - 5f, rect.width - (PanelPadding * 2f), 2f), string.Empty, styles.Divider);
        return rect.y + PanelHeaderHeight;
    }

    private void DrawRow(
        ref float y,
        float x,
        float width,
        string label,
        string value,
        Styles styles,
        float height = RowHeight,
        bool wrapValue = false)
    {
        Rect row = new Rect(x, y, width, height);
        GUI.Box(row, string.Empty, styles.Row);
        float textInset = _page == Page.Overview ? DecorativeTextInset : FullPageDecorativeTextInset;
        float labelWidth = Mathf.Min(230f, width * 0.38f);
        float labelTextWidth = Mathf.Max(80f, labelWidth - (textInset - 12f));
        GUI.Label(new Rect(row.x + textInset, row.y, labelTextWidth, row.height), label, styles.Label);
        GUI.Label(
            new Rect(row.x + labelWidth + 18f, row.y + 2f, row.width - labelWidth - 18f - textInset, row.height - 4f),
            DisplayValue(value),
            wrapValue ? styles.ValueWrap : styles.Value);
        y += height + 6f;
    }

    private void DrawParagraph(
        ref float y,
        float x,
        float width,
        string value,
        Styles styles,
        float minimumHeight = 58f,
        float maximumHeight = 116f)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        float textInset = _page == Page.Overview ? DecorativeTextInset : FullPageDecorativeTextInset;
        float textWidth = Mathf.Max(120f, width - (textInset * 2f));
        float height = ParagraphHeight(value, textWidth, minimumHeight, maximumHeight);
        Rect note = new Rect(x, y, width, height);
        GUI.Box(note, string.Empty, styles.Note);
        GUI.Label(new Rect(note.x + textInset, note.y + 10f, textWidth, note.height - 20f), value, styles.Body);
        y += height + 8f;
    }

    private void DrawFooter(Rect rect, Styles styles)
    {
        GUI.Box(rect, string.Empty, styles.Footer);

        float buttonY = rect.y + 8f;
        float closeWidth = 116f;
        float managerWidth = 188f;
        float reportWidth = 178f;
        float buttonGap = 10f;
        float closeX = rect.x + rect.width - closeWidth - 10f;
        float managerX = closeX - buttonGap - managerWidth;
        float reportX = managerX - buttonGap - reportWidth;
        float statusWidth = Mathf.Max(180f, reportX - rect.x - 20f);

        GUI.Label(
            new Rect(rect.x + DecorativeTextInset, rect.y + 8f, Mathf.Max(80f, statusWidth - (DecorativeTextInset - 14f)), rect.height - 16f),
            _status,
            styles.FooterStatus);
        if (GUI.Button(new Rect(reportX, buttonY, reportWidth, 40f), "Capture Report  F9", styles.SecondaryButton))
        {
            _status = _watcher.WriteManualReport() ? "Manual report written." : "Manual report failed. Check BepInEx log.";
        }

        if (GUI.Button(new Rect(managerX, buttonY, managerWidth, 40f), "FoA Mod Manager", styles.SecondaryButton))
        {
            OpenManager();
        }

        if (GUI.Button(new Rect(closeX, buttonY, closeWidth, 40f), "Close  Esc", styles.DangerButton))
        {
            _closeAfterDraw = true;
        }
    }

    private float MeasurePageHeight(PerformanceRuntimeOverview overview, float width)
    {
        switch (_page)
        {
            case Page.Timing:
                return 596f;
            case Page.Memory:
                return 530f;
            case Page.Native:
                return MeasureNativePanelHeight(overview, width);
            case Page.Mods:
                return MeasureModsPanelHeight(overview, width);
            case Page.Reports:
                return MeasureReportsPanelHeight(overview, width);
            default:
                float columnWidth = (width - Gap) * 0.5f;
                return MetricHeight + Gap + TimelineHeight + Gap + MeasureOverviewPanelHeight(overview, columnWidth) + 4f;
        }
    }

    private float MeasureOverviewPanelHeight(PerformanceRuntimeOverview overview, float width)
    {
        float rowWidth = width - (PanelPadding * 2f);
        float assessment = PanelHeaderHeight
            + ParagraphHeight(overview.Detail, rowWidth, 58f, 116f) + 8f
            + TallRowHeight + 6f
            + RowHeight + 6f
            + ParagraphHeight(overview.BestNextCheck, rowWidth, 68f, 126f) + 8f
            + PanelPadding;
        float frame = PanelHeaderHeight + ((RowHeight + 6f) * 4f) + TallRowHeight + 6f + PanelPadding;
        return Mathf.Max(300f, Mathf.Max(frame, assessment));
    }

    private float MeasureNativePanelHeight(PerformanceRuntimeOverview overview, float width)
    {
        float rowWidth = width - (PanelPadding * 2f);
        return PanelHeaderHeight
            + RowHeight + 6f
            + TallRowHeight + 6f
            + TallRowHeight + 6f
            + RowHeight + 6f
            + RowHeight + 6f
            + TallRowHeight + 6f
            + ParagraphHeight(overview.NativeInvestigation, rowWidth, 72f, 132f) + 8f
            + ParagraphHeight(overview.GameSettingsRecommendation, rowWidth, 62f, 112f) + 8f
            + PanelPadding;
    }

    private float MeasureModsPanelHeight(PerformanceRuntimeOverview overview, float width)
    {
        float rowWidth = width - (PanelPadding * 2f);
        return PanelHeaderHeight
            + ((RowHeight + 6f) * 2f)
            + ParagraphHeight(overview.ModStackInvestigation, rowWidth, 72f, 132f) + 8f
            + ParagraphHeight(overview.ModManagerRecommendation, rowWidth, 62f, 112f) + 8f
            + 54f
            + PanelPadding;
    }

    private float MeasureReportsPanelHeight(PerformanceRuntimeOverview overview, float width)
    {
        return PanelHeaderHeight
            + TallRowHeight + 6f
            + RowHeight + 6f
            + RowHeight + 6f
            + TallRowHeight + 6f
            + 54f
            + PanelPadding;
    }

    private static float ParagraphHeight(string value, float width, float minimumHeight, float maximumHeight)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return 0f;
        }

        int charactersPerLine = Math.Max(32, (int)(width / 9.5f));
        int lines = Math.Max(1, (value.Length + charactersPerLine - 1) / charactersPerLine);
        return Mathf.Clamp(22f + (lines * 22f), minimumHeight, maximumHeight);
    }

    private static string DisplayValue(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? "unavailable" : value;
    }

    private static string FormatBytes(double bytes)
    {
        if (bytes >= 1024d * 1024d)
        {
            return RuntimePerformanceWatcher.FormatNumber(bytes / (1024d * 1024d)) + " MiB";
        }

        if (bytes >= 1024d)
        {
            return RuntimePerformanceWatcher.FormatNumber(bytes / 1024d) + " KiB";
        }

        return RuntimePerformanceWatcher.FormatNumber(bytes) + " B";
    }

    private static string FormatByteRate(double bytesPerSecond)
    {
        return FormatBytes(bytesPerSecond) + "/s";
    }

    private void OpenManager()
    {
        bool opened = FoAModManagerBridge.OpenForPerformanceMod(Plugin.PluginGuid);
        _status = opened
            ? "FoA Mod Manager opened. Use Tainted Performance status/settings to compare mods."
            : "FoA Mod Manager is not available.";
        if (opened)
        {
            _closeAfterDraw = true;
        }
    }

    private void AcquireScope()
    {
        if (_taintedInterfaceScopeActive)
        {
            TaintedInterfaceBridge.FocusScreenScope(ScreenId);
            return;
        }

        if (_managerScopeActive)
        {
            return;
        }

        bool freezeWorld = _config.FreezeWorldWhileOverviewOpen.Value;
        _taintedInterfaceScopeActive = TaintedInterfaceBridge.OpenScreenScope(ScreenId, freezeWorld);
        if (_taintedInterfaceScopeActive)
        {
            TaintedInterfaceBridge.FocusScreenScope(ScreenId);
            LogScope("Tainted Interface screen scope", "tainted-performance-screen-scope-service");
            return;
        }

        FoAModManagerBridge.SetCustomUiScope(ScreenId, active: true, freezeWorld);
        _managerScopeActive = true;
        LogScope("FoA Mod Manager custom UI fallback", "tainted-performance-manager-scope-fallback");
    }

    private void LogScope(string scope, string marker)
    {
        if (_scopeLogged)
        {
            return;
        }

        _scopeLogged = true;
        _logger.LogInfo($"{Plugin.PluginName} overview using {scope}. screenId={ScreenId}; marker={marker}");
    }

    private void EnsurePlacement()
    {
        float width = Mathf.Min(
            Mathf.Max(MinimumWidth, Screen.width * 0.5f),
            Mathf.Min(MaximumWidth, Mathf.Max(700f, Screen.width - (Margin * 2f))));
        float height = Mathf.Min(
            Mathf.Max(MinimumHeight, Screen.height * 0.58f),
            Mathf.Min(MaximumHeight, Mathf.Max(460f, Screen.height - (Margin * 2f))));
        if (_rect.width <= 0f || _rect.height <= 0f)
        {
            _rect = new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);
        }

        _rect.width = width;
        _rect.height = height;
        _rect.x = Mathf.Clamp(_rect.x, Margin, Mathf.Max(Margin, Screen.width - _rect.width - Margin));
        _rect.y = Mathf.Clamp(_rect.y, Margin, Mathf.Max(Margin, Screen.height - _rect.height - Margin));
    }

    private static float CalculateUiScale()
    {
        return Mathf.Clamp(Screen.height / 1200f, 1f, 1.5f);
    }

    private Styles EnsureStyles()
    {
        if (_styles != null)
        {
            return _styles;
        }

        if (TaintedInterfaceBridge.TryGetStyles(out TaintedInterfaceBridge.SharedStyles? shared) && shared != null)
        {
            _styles = Styles.FromShared(shared);
            _logger.LogInfo($"{Plugin.PluginName} overview using Tainted Interface dark-fantasy styles and semantic assets. marker=tainted-performance-dark-fantasy-monitor");
            return _styles;
        }

        _styles = Styles.Local();
        return _styles;
    }

    private enum Page
    {
        Overview,
        Timing,
        Memory,
        Native,
        Mods,
        Reports
    }

    private sealed class Styles
    {
        internal GUIStyle Window { get; private set; } = null!;
        internal GUIStyle Header { get; private set; } = null!;
        internal GUIStyle HeaderMeta { get; private set; } = null!;
        internal GUIStyle TabBar { get; private set; } = null!;
        internal GUIStyle Tab { get; private set; } = null!;
        internal GUIStyle TabActive { get; private set; } = null!;
        internal GUIStyle Panel { get; private set; } = null!;
        internal GUIStyle Card { get; private set; } = null!;
        internal GUIStyle Row { get; private set; } = null!;
        internal GUIStyle Note { get; private set; } = null!;
        internal GUIStyle Footer { get; private set; } = null!;
        internal GUIStyle SecondaryButton { get; private set; } = null!;
        internal GUIStyle DangerButton { get; private set; } = null!;
        internal GUIStyle BadgeWarningBox { get; private set; } = null!;
        internal GUIStyle BadgeGoodBox { get; private set; } = null!;
        internal GUIStyle Title { get; private set; } = null!;
        internal GUIStyle Section { get; private set; } = null!;
        internal GUIStyle Label { get; private set; } = null!;
        internal GUIStyle Body { get; private set; } = null!;
        internal GUIStyle Value { get; private set; } = null!;
        internal GUIStyle ValueWrap { get; private set; } = null!;
        internal GUIStyle MetricLabel { get; private set; } = null!;
        internal GUIStyle MetricValue { get; private set; } = null!;
        internal GUIStyle MetricCaption { get; private set; } = null!;
        internal GUIStyle FooterStatus { get; private set; } = null!;
        internal GUIStyle Warning { get; private set; } = null!;
        internal GUIStyle Good { get; private set; } = null!;
        internal GUIStyle Divider { get; private set; } = null!;
        internal GUIStyle GraphBackground { get; private set; } = null!;
        internal GUIStyle GraphBar { get; private set; } = null!;
        internal GUIStyle GraphSpike { get; private set; } = null!;
        internal GUIStyle GraphSevere { get; private set; } = null!;
        internal GUIStyle GraphBudget { get; private set; } = null!;
        internal bool UsesTaintedInterfaceTheme { get; private set; }

        internal static Styles FromShared(TaintedInterfaceBridge.SharedStyles shared)
        {
            return new Styles
            {
                UsesTaintedInterfaceTheme = true,
                Window = CloneBoxStyle(shared.Window, 0, 0, 0, 0),
                Header = CloneBoxStyle(shared.Header, 20, 20, 12, 12),
                HeaderMeta = TextStyle(shared.MutedLabel, 15, FontStyle.Bold, TextAnchor.MiddleLeft, wordWrap: false, Palette.Muted),
                TabBar = CloneBoxStyle(shared.Panel, 8, 8, 6, 6),
                Tab = CloneButtonStyle(shared.SecondaryButton, Palette.Text),
                TabActive = CloneButtonStyle(shared.Button, Palette.Accent),
                Panel = CloneBoxStyle(shared.Panel, 16, 16, 14, 14),
                Card = CloneBoxStyle(shared.Card, 14, 14, 12, 12),
                Row = CloneBoxStyle(shared.Card, 10, 10, 7, 7),
                Note = CloneBoxStyle(shared.Panel, 12, 12, 10, 10),
                Footer = CloneBoxStyle(shared.Panel, 10, 10, 8, 8),
                SecondaryButton = CloneButtonStyle(shared.SecondaryButton, Palette.Text),
                DangerButton = CloneButtonStyle(shared.Button, Palette.Warning),
                BadgeWarningBox = BoxStyle(shared.Card, Palette.BadgeWarning, 8, 8, 4, 4),
                BadgeGoodBox = BoxStyle(shared.Card, Palette.BadgeGood, 8, 8, 4, 4),
                Title = TextStyle(shared.Title, 30, FontStyle.Bold, TextAnchor.MiddleLeft, wordWrap: false, Palette.Accent),
                Section = TextStyle(shared.Label, 18, FontStyle.Bold, TextAnchor.MiddleLeft, wordWrap: false, Palette.Section),
                Label = TextStyle(shared.Label, 16, FontStyle.Bold, TextAnchor.MiddleLeft, wordWrap: false, Palette.Muted),
                Body = TextStyle(shared.Label, 16, FontStyle.Normal, TextAnchor.UpperLeft, wordWrap: true, Palette.Text),
                Value = TextStyle(shared.Label, 17, FontStyle.Bold, TextAnchor.MiddleRight, wordWrap: false, Palette.Text),
                ValueWrap = TextStyle(shared.Label, 16, FontStyle.Bold, TextAnchor.MiddleRight, wordWrap: true, Palette.Text),
                MetricLabel = TextStyle(shared.MutedLabel, 14, FontStyle.Bold, TextAnchor.MiddleLeft, wordWrap: false, Palette.Muted),
                MetricValue = TextStyle(shared.Label, 28, FontStyle.Bold, TextAnchor.MiddleLeft, wordWrap: false, Palette.Text),
                MetricCaption = TextStyle(shared.MutedLabel, 14, FontStyle.Normal, TextAnchor.MiddleLeft, wordWrap: true, Palette.Dim),
                FooterStatus = TextStyle(shared.MutedLabel, 14, FontStyle.Normal, TextAnchor.MiddleLeft, wordWrap: true, Palette.Muted),
                Warning = TextStyle(shared.Label, 15, FontStyle.Bold, TextAnchor.MiddleCenter, wordWrap: false, Palette.Warning),
                Good = TextStyle(shared.Label, 15, FontStyle.Bold, TextAnchor.MiddleCenter, wordWrap: false, Palette.Good),
                Divider = shared.DividerTexture != null
                    ? TexturedBoxStyle(shared.Panel, shared.DividerTexture)
                    : BoxStyle(shared.Panel, Palette.Section, 0, 0, 0, 0),
                GraphBackground = shared.ProgressTrackTexture != null
                    ? TexturedBoxStyle(shared.Card, shared.ProgressTrackTexture)
                    : CloneBoxStyle(shared.Card, 0, 0, 0, 0),
                GraphBar = BoxStyle(shared.Card, Palette.GraphBar, 0, 0, 0, 0),
                GraphSpike = BoxStyle(shared.Card, Palette.GraphSpike, 0, 0, 0, 0),
                GraphSevere = BoxStyle(shared.Card, Palette.GraphSevere, 0, 0, 0, 0),
                GraphBudget = BoxStyle(shared.Card, Palette.GraphBudget, 0, 0, 0, 0)
            };
        }

        internal static Styles Local()
        {
            return Create(
                GUI.skin.window,
                GUI.skin.box,
                GUI.skin.box,
                GUI.skin.box,
                GUI.skin.button,
                GUI.skin.button,
                GUI.skin.label,
                GUI.skin.label,
                GUI.skin.label);
        }

        private static Styles Create(
            GUIStyle windowBase,
            GUIStyle headerBase,
            GUIStyle panelBase,
            GUIStyle cardBase,
            GUIStyle buttonBase,
            GUIStyle secondaryButtonBase,
            GUIStyle titleBase,
            GUIStyle labelBase,
            GUIStyle mutedBase)
        {
            return new Styles
            {
                UsesTaintedInterfaceTheme = false,
                Window = BoxStyle(windowBase, Palette.Window, 0, 0, 0, 0),
                Header = BoxStyle(headerBase, Palette.Header, 20, 20, 12, 12),
                HeaderMeta = TextStyle(mutedBase, 15, FontStyle.Bold, TextAnchor.MiddleLeft, wordWrap: false, Palette.Muted),
                TabBar = BoxStyle(panelBase, Palette.TabBar, 8, 8, 6, 6),
                Tab = ButtonStyle(buttonBase, Palette.Tab, Palette.TabHover, Palette.Text),
                TabActive = ButtonStyle(buttonBase, Palette.TabActive, Palette.TabActiveHover, Palette.Accent),
                Panel = BoxStyle(panelBase, Palette.Panel, 16, 16, 14, 14),
                Card = BoxStyle(cardBase, Palette.Card, 14, 14, 12, 12),
                Row = BoxStyle(cardBase, Palette.Row, 10, 10, 7, 7),
                Note = BoxStyle(cardBase, Palette.Note, 12, 12, 10, 10),
                Footer = BoxStyle(panelBase, Palette.Footer, 10, 10, 8, 8),
                SecondaryButton = ButtonStyle(secondaryButtonBase, Palette.Button, Palette.ButtonHover, Palette.Text),
                DangerButton = ButtonStyle(buttonBase, Palette.DangerButton, Palette.DangerHover, Palette.Warning),
                BadgeWarningBox = BoxStyle(cardBase, Palette.BadgeWarning, 8, 8, 4, 4),
                BadgeGoodBox = BoxStyle(cardBase, Palette.BadgeGood, 8, 8, 4, 4),
                Title = TextStyle(titleBase, 30, FontStyle.Bold, TextAnchor.MiddleLeft, wordWrap: false, Palette.Accent),
                Section = TextStyle(labelBase, 18, FontStyle.Bold, TextAnchor.MiddleLeft, wordWrap: false, Palette.Section),
                Label = TextStyle(labelBase, 16, FontStyle.Bold, TextAnchor.MiddleLeft, wordWrap: false, Palette.Muted),
                Body = TextStyle(labelBase, 16, FontStyle.Normal, TextAnchor.UpperLeft, wordWrap: true, Palette.Text),
                Value = TextStyle(labelBase, 17, FontStyle.Bold, TextAnchor.MiddleRight, wordWrap: false, Palette.Text),
                ValueWrap = TextStyle(labelBase, 16, FontStyle.Bold, TextAnchor.MiddleRight, wordWrap: true, Palette.Text),
                MetricLabel = TextStyle(mutedBase, 14, FontStyle.Bold, TextAnchor.MiddleLeft, wordWrap: false, Palette.Muted),
                MetricValue = TextStyle(labelBase, 28, FontStyle.Bold, TextAnchor.MiddleLeft, wordWrap: false, Palette.Text),
                MetricCaption = TextStyle(mutedBase, 14, FontStyle.Normal, TextAnchor.MiddleLeft, wordWrap: true, Palette.Dim),
                FooterStatus = TextStyle(mutedBase, 14, FontStyle.Normal, TextAnchor.MiddleLeft, wordWrap: true, Palette.Muted),
                Warning = TextStyle(labelBase, 15, FontStyle.Bold, TextAnchor.MiddleCenter, wordWrap: false, Palette.Warning),
                Good = TextStyle(labelBase, 15, FontStyle.Bold, TextAnchor.MiddleCenter, wordWrap: false, Palette.Good),
                Divider = BoxStyle(panelBase, Palette.Section, 0, 0, 0, 0),
                GraphBackground = BoxStyle(cardBase, Palette.GraphBackground, 0, 0, 0, 0),
                GraphBar = BoxStyle(cardBase, Palette.GraphBar, 0, 0, 0, 0),
                GraphSpike = BoxStyle(cardBase, Palette.GraphSpike, 0, 0, 0, 0),
                GraphSevere = BoxStyle(cardBase, Palette.GraphSevere, 0, 0, 0, 0),
                GraphBudget = BoxStyle(cardBase, Palette.GraphBudget, 0, 0, 0, 0)
            };
        }

        private static GUIStyle CloneBoxStyle(
            GUIStyle source,
            int left,
            int right,
            int top,
            int bottom)
        {
            return new GUIStyle(source)
            {
                padding = new RectOffset(left, right, top, bottom),
                margin = new RectOffset(0, 0, 0, 0),
                wordWrap = false
            };
        }

        private static GUIStyle CloneButtonStyle(GUIStyle source, Color textColor)
        {
            GUIStyle style = new GUIStyle(source)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = false,
                padding = new RectOffset(10, 10, 6, 6),
                margin = new RectOffset(0, 0, 0, 0)
            };
            style.normal.textColor = textColor;
            style.hover.textColor = textColor;
            style.active.textColor = Palette.Accent;
            style.focused.textColor = textColor;
            return style;
        }

        private static GUIStyle TexturedBoxStyle(GUIStyle source, Texture2D texture)
        {
            GUIStyle style = CloneBoxStyle(source, 0, 0, 0, 0);
            style.normal.background = texture;
            style.hover.background = texture;
            style.active.background = texture;
            style.focused.background = texture;
            return style;
        }

        private static GUIStyle BoxStyle(GUIStyle source, Color background, int left, int right, int top, int bottom)
        {
            GUIStyle style = new GUIStyle(source)
            {
                padding = new RectOffset(left, right, top, bottom),
                margin = new RectOffset(0, 0, 0, 0),
                wordWrap = false
            };
            Texture2D texture = Texture(background);
            style.normal = State(Palette.Text, texture);
            style.hover = State(Palette.Text, texture);
            style.active = State(Palette.Text, texture);
            style.focused = State(Palette.Text, texture);
            return style;
        }

        private static GUIStyle ButtonStyle(GUIStyle source, Color background, Color hoverBackground, Color textColor)
        {
            GUIStyle style = TextStyle(source, 15, FontStyle.Bold, TextAnchor.MiddleCenter, wordWrap: false, textColor);
            style.padding = new RectOffset(10, 10, 6, 6);
            style.margin = new RectOffset(0, 0, 0, 0);
            style.normal = State(textColor, Texture(background));
            style.hover = State(textColor, Texture(hoverBackground));
            style.active = State(Palette.Accent, Texture(Palette.ButtonActive));
            style.focused = State(textColor, Texture(hoverBackground));
            return style;
        }

        private static GUIStyle TextStyle(
            GUIStyle source,
            int fontSize,
            FontStyle fontStyle,
            TextAnchor alignment,
            bool wordWrap,
            Color textColor)
        {
            GUIStyle style = new GUIStyle(source)
            {
                fontSize = fontSize,
                fontStyle = fontStyle,
                alignment = alignment,
                wordWrap = wordWrap,
                padding = new RectOffset(0, 0, 0, 0),
                margin = new RectOffset(0, 0, 0, 0)
            };
            style.normal = State(textColor);
            style.hover = State(textColor);
            style.active = State(textColor);
            style.focused = State(textColor);
            return style;
        }

        private static GUIStyleState State(Color textColor, Texture2D? background = null)
        {
            return new GUIStyleState
            {
                textColor = textColor,
                background = background
            };
        }

        private static Texture2D Texture(Color color)
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }

        private static class Palette
        {
            internal static readonly Color Window = new Color { r = 0.018f, g = 0.023f, b = 0.024f, a = 0.94f };
            internal static readonly Color Header = new Color { r = 0.19f, g = 0.025f, b = 0.04f, a = 0.96f };
            internal static readonly Color TabBar = new Color { r = 0.035f, g = 0.041f, b = 0.043f, a = 0.96f };
            internal static readonly Color Tab = new Color { r = 0.07f, g = 0.079f, b = 0.083f, a = 0.98f };
            internal static readonly Color TabHover = new Color { r = 0.105f, g = 0.115f, b = 0.12f, a = 0.98f };
            internal static readonly Color TabActive = new Color { r = 0.18f, g = 0.045f, b = 0.055f, a = 0.98f };
            internal static readonly Color TabActiveHover = new Color { r = 0.23f, g = 0.06f, b = 0.07f, a = 0.98f };
            internal static readonly Color Panel = new Color { r = 0.032f, g = 0.04f, b = 0.042f, a = 0.94f };
            internal static readonly Color Card = new Color { r = 0.055f, g = 0.062f, b = 0.066f, a = 0.98f };
            internal static readonly Color Row = new Color { r = 0.07f, g = 0.079f, b = 0.082f, a = 0.97f };
            internal static readonly Color Note = new Color { r = 0.062f, g = 0.069f, b = 0.073f, a = 0.96f };
            internal static readonly Color Footer = new Color { r = 0.026f, g = 0.032f, b = 0.034f, a = 0.96f };
            internal static readonly Color Button = new Color { r = 0.105f, g = 0.115f, b = 0.12f, a = 0.98f };
            internal static readonly Color ButtonHover = new Color { r = 0.14f, g = 0.15f, b = 0.155f, a = 0.98f };
            internal static readonly Color ButtonActive = new Color { r = 0.21f, g = 0.065f, b = 0.07f, a = 0.98f };
            internal static readonly Color DangerButton = new Color { r = 0.25f, g = 0.052f, b = 0.052f, a = 0.98f };
            internal static readonly Color DangerHover = new Color { r = 0.33f, g = 0.07f, b = 0.06f, a = 0.98f };
            internal static readonly Color BadgeWarning = new Color { r = 0.24f, g = 0.06f, b = 0.045f, a = 0.98f };
            internal static readonly Color BadgeGood = new Color { r = 0.07f, g = 0.12f, b = 0.075f, a = 0.98f };
            internal static readonly Color Text = new Color { r = 0.97f, g = 0.93f, b = 0.84f, a = 1f };
            internal static readonly Color Muted = new Color { r = 0.85f, g = 0.79f, b = 0.68f, a = 1f };
            internal static readonly Color Dim = new Color { r = 0.70f, g = 0.65f, b = 0.56f, a = 1f };
            internal static readonly Color Section = new Color { r = 1f, g = 0.42f, b = 0.12f, a = 1f };
            internal static readonly Color Accent = new Color { r = 1f, g = 0.58f, b = 0.16f, a = 1f };
            internal static readonly Color Good = new Color { r = 0.72f, g = 0.86f, b = 0.56f, a = 1f };
            internal static readonly Color Warning = new Color { r = 1f, g = 0.42f, b = 0.34f, a = 1f };
            internal static readonly Color GraphBackground = new Color { r = 0.018f, g = 0.023f, b = 0.024f, a = 0.98f };
            internal static readonly Color GraphBar = new Color { r = 0.36f, g = 0.55f, b = 0.31f, a = 0.98f };
            internal static readonly Color GraphSpike = new Color { r = 0.94f, g = 0.48f, b = 0.12f, a = 0.98f };
            internal static readonly Color GraphSevere = new Color { r = 0.78f, g = 0.10f, b = 0.08f, a = 0.98f };
            internal static readonly Color GraphBudget = new Color { r = 0.92f, g = 0.70f, b = 0.36f, a = 0.90f };
        }
    }
}
