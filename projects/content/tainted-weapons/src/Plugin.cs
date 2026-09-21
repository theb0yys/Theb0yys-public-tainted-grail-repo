using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Awaken.ECS.DrakeRenderer.Authoring;
using Awaken.TG.Assets;
using Awaken.TG.Graphics;
using Awaken.TG.Graphics.Cutscenes;
using Awaken.TG.Main.Heroes.Combat;
using Awaken.TG.Main.Heroes.Items.Attachments;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace TaintedWeapons;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "kane.tgfoa.tainted-weapons";
    public const string PluginName = "Tainted Weapons";
    public const string PluginVersion = "0.3.34";

    private const string SafetyFields =
        "probeMutatesRuntime=false,equipmentMutation=registered-weapon-visual-reference-and-runtime-view-only,inventoryWrites=false,itemGrants=false,combatGameplay=false,animationMutation=false,rendererMutation=framework-owned-prototype-authoring-and-registered-spawned-presentation-activation-and-stored-linked-offset-plus-visible-vanilla-fpp-placement-layer-ecs-completion-and-retention-and-visual-proof-plus-registered-inventory-preview-owner-layer-mask-repair-and-vanilla-equipped-comparison-receipt-readiness-only-plus-evil-equipped-placement-transform-bounds-update-and-compact-placement-post-receipt-plus-registered-equipped-retrofit-after-vanilla-fpp-contract-plus-front-face-visible-depth-placement-plus-pending-registered-fpp-placement-contract-lifecycle-plus-global-hand-observer-fpp-gated,materialMutation=framework-owned-prototype-reference-only,textureMutation=false,sharedMaterialMutation=false,probeAssetBundleLoad=false,frameworkAssetBundleLoad=definition-and-prototype-authoring,saveWrites=false,persistence=false,online=false";
    private const string FrameworkStage = "drake-framework-mapping";
    private const string FrameworkRoute = "template-clone-plus-drake-prototype-rebinding";
    private const float AutoRetrySeconds = 10f;
    private const float LifecycleValidationConfigReloadSeconds = 1f;
    private const int MaxAutoRunAttempts = 30;

    private ConfigEntry<bool> _probeEnabled = null!;
    private ConfigEntry<KeyCode> _probeTriggerKey = null!;
    private ConfigEntry<bool> _autoRunOnce = null!;
    private ConfigEntry<float> _initialDelaySeconds = null!;
    private ConfigEntry<string> _targetFilters = null!;
    private ConfigEntry<string> _targetPriorityFilters = null!;
    private ConfigEntry<int> _maxRows = null!;
    private ConfigEntry<int> _maxMemberValues = null!;
    private ConfigEntry<bool> _lifecycleValidationEnabled = null!;
    private ConfigEntry<int> _lifecycleValidationRequestId = null!;
    private ConfigEntry<bool> _lifecycleValidationWriteOnSceneChange = null!;
    private ConfigEntry<bool> _lifecycleValidationWriteOnShutdown = null!;

    internal static Plugin? Instance { get; private set; }

    internal ManualLogSource ModLogger => Logger;

    internal TaintedWeaponsCapabilityReceipt? CapabilityReceipt => _framework?.CapabilityReceipt;

    internal TaintedWeaponNativeItemRegistrarStatus? NativeItemRegistrarStatus => _framework?.NativeItemRegistrarStatus;

    private bool _autoRunComplete;
    private bool _shutdownLifecycleReportWritten;
    private int _autoRunAttempts;
    private int _lastLifecycleValidationRequestId;
    private float _autoRunAtTime;
    private float _nextNativeItemRegistrarQueueAt;
    private float _nextLifecycleValidationConfigReloadAt;
    private long _lastLifecycleValidationConfigWriteTicks;
    private string _lastActiveSceneName = string.Empty;
    private TaintedWeaponFramework _framework = null!;
    private Harmony? _frameworkHarmony;
    private bool _importerLifecycleOpen;

    private int MaxRows => Mathf.Clamp(_maxRows.Value, 1, 1000);

    private int MaxMemberValues => Mathf.Clamp(_maxMemberValues.Value, 1, 200);

    private void Awake()
    {
        Instance = this;
        _probeEnabled = Config.Bind(
            "WeaponMaterialProbe",
            "Enabled",
            false,
            "Enable the read-only weapon material probe. Default is off because broad runtime scans are development-only.");

        _probeTriggerKey = Config.Bind(
            "WeaponMaterialProbe",
            "TriggerKey",
            KeyCode.F9,
            "When the probe is enabled, pressing this key writes one read-only weapon material report.");

        _autoRunOnce = Config.Bind(
            "WeaponMaterialProbe",
            "AutoRunOnce",
            false,
            "When enabled, write one read-only report after InitialDelaySeconds. Default is off; use the hotkey for bounded evidence captures.");

        _initialDelaySeconds = Config.Bind(
            "WeaponMaterialProbe",
            "InitialDelaySeconds",
            8f,
            new ConfigDescription(
                "Delay before the startup report. Gives gameplay scenes time to load.",
                new AcceptableValueRange<float>(0f, 60f)));

        _targetFilters = Config.Bind(
            "WeaponMaterialProbe",
            "TargetFilters",
            "Equipable_Weapon|Weapon_",
            "Case-insensitive filters separated by |. A renderer or component is included when its path, material name, or texture name contains any filter.");

        _targetPriorityFilters = Config.Bind(
            "WeaponMaterialProbe",
            "TargetPriorityFilters",
            "Longsword|LongSword|Long_Sword|DullLongsword|Dull Longsword|Dull_Longsword|Medium_Tier2_Longsword|Medium_Tier1_DullLongsword",
            "Case-insensitive high-priority target filters separated by |. Priority matches are written before broad weapon rows, and auto-run only completes when a priority match is found.");

        _maxRows = Config.Bind(
            "WeaponMaterialProbe",
            "MaxRows",
            160,
            new ConfigDescription(
                "Maximum renderer/material/texture rows and component rows written per report.",
                new AcceptableValueRange<int>(1, 1000)));

        _maxMemberValues = Config.Bind(
            "WeaponMaterialProbe",
            "MaxMemberValues",
            24,
            new ConfigDescription(
                "Maximum reflected member values written for each component row.",
                new AcceptableValueRange<int>(1, 200)));

        _lifecycleValidationEnabled = Config.Bind(
            "LifecycleValidation",
            "Enabled",
            false,
            "Enable the default-off W7-W10/W14-W17 lifecycle validation receipt harness. It observes and reports framework state; it does not equip, unequip, grant items, or drive gameplay.");

        _lifecycleValidationRequestId = Config.Bind(
            "LifecycleValidation",
            "RequestId",
            0,
            "Increment this value while LifecycleValidation.Enabled is true to write one lifecycle validation report.");

        _lifecycleValidationWriteOnSceneChange = Config.Bind(
            "LifecycleValidation",
            "WriteOnSceneChange",
            false,
            "When enabled, write a lifecycle validation report when the active scene changes.");

        _lifecycleValidationWriteOnShutdown = Config.Bind(
            "LifecycleValidation",
            "WriteOnShutdown",
            false,
            "When enabled, write a before/after cleanup lifecycle report before the native title-screen exit kills the process, with plugin destroy retained as a fallback.");
        _lastLifecycleValidationRequestId = _lifecycleValidationRequestId.Value;
        _lastLifecycleValidationConfigWriteTicks = ReadConfigFileWriteTicks();

        Scene activeScene = SceneManager.GetActiveScene();
        _lastActiveSceneName = activeScene.IsValid() ? activeScene.name : string.Empty;
        ScheduleAutoRun();
        _framework = new TaintedWeaponFramework(Logger);
        _frameworkHarmony = new Harmony(PluginGuid);
        PatchGameQuitLifecycleHandler();
        (bool registrarHookAvailable, string registrarHookReason) = TaintedWeaponNativeItemRegistrarPatch.Apply(_frameworkHarmony, Logger);
        _framework.SetNativeItemRegistrarReadinessHookStatus(registrarHookAvailable, registrarHookReason);
        TaintedWeaponsCapabilityReceipt capabilityReceipt = TaintedWeaponEquipReferencePatch.Apply(_frameworkHarmony, Logger);
        _framework.SetCapabilityReceipt(capabilityReceipt);
        foreach (TaintedWeaponsCapability capability in capabilityReceipt.Capabilities)
        {
            Logger.LogInfo($"{PluginName} capability; {capability}");
        }

        foreach (TaintedWeaponsCapabilityState state in capabilityReceipt.States)
        {
            Logger.LogInfo($"{PluginName} capability state; {state}");
        }

        if (!capabilityReceipt.PresentationReady)
        {
            Logger.LogError($"{PluginName} framework presentation capability blocked; denialReasons={capabilityReceipt.PresentationDenialReasons}");
        }

        if (!capabilityReceipt.ImporterReady)
        {
            Logger.LogWarning($"{PluginName} framework importer capability claims blocked; stateDenialReasons={capabilityReceipt.StateDenialReasons}");
        }

        foreach (string row in _framework.DescribeStageRows())
        {
            Logger.LogInfo($"{PluginName} framework stage; {row}");
        }

        _importerLifecycleOpen = true;
        TaintedWeaponsApi.FlushPendingDefinitions(this);
        TaintedWeaponsApi.FlushPendingNativeItemRegistrations(this);

        Logger.LogInfo($"{PluginName} {PluginVersion} loaded. FrameworkStage={FrameworkStage}, FrameworkRoute={FrameworkRoute}, NativeItemRegistrar={_framework.NativeItemRegistrarStatus}, NativeItemRegistrationAutoInvoke=false, DirectUnityRendererFallback=false, SyntheticDrakeEntityConstruction=false, WeaponMaterialProbe.Enabled={_probeEnabled.Value}, TriggerKey={_probeTriggerKey.Value}, AutoRunOnce={_autoRunOnce.Value}, InitialDelaySeconds={FormatFloat(_initialDelaySeconds.Value)}, TargetFilters={Clean(_targetFilters.Value)}, TargetPriorityFilters={Clean(_targetPriorityFilters.Value)}, LifecycleValidation.Enabled={_lifecycleValidationEnabled.Value}, LifecycleValidation.RequestId={_lifecycleValidationRequestId.Value.ToString(CultureInfo.InvariantCulture)}, LifecycleValidation.WriteOnSceneChange={_lifecycleValidationWriteOnSceneChange.Value}, LifecycleValidation.WriteOnShutdown={_lifecycleValidationWriteOnShutdown.Value}, Safety={SafetyFields}.");
    }

    private void PatchGameQuitLifecycleHandler()
    {
        try
        {
            Type? titleScreenType = AccessTools.TypeByName("Awaken.TG.Main.UI.TitleScreen.TitleScreenUI");
            MethodInfo? exitMethod = titleScreenType == null
                ? null
                : AccessTools.Method(titleScreenType, "Exit", Type.EmptyTypes);
            if (exitMethod == null || _frameworkHarmony == null)
            {
                Logger.LogWarning($"{PluginName} could not patch native game quit lifecycle handler. TitleScreenUI.Exit was not found; plugin-destroy fallback remains active.");
                return;
            }

            MethodInfo prefix = AccessTools.Method(typeof(Plugin), nameof(BeforeNativeGameQuit));
            _frameworkHarmony.Patch(exitMethod, prefix: new HarmonyMethod(prefix));
            Logger.LogInfo($"{PluginName} patched native game quit lifecycle handler. target={exitMethod.DeclaringType?.FullName}.{exitMethod.Name}, originalPreserved=true, pluginDestroyFallback=true");
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"{PluginName} could not patch native game quit lifecycle handler. {ex.GetType().Name}: {ex.Message}; plugin-destroy fallback remains active.");
        }
    }

    private static void BeforeNativeGameQuit()
    {
        Instance?.TryWriteShutdownLifecycleValidationReport("native-title-screen-exit");
    }

    private bool TryWriteShutdownLifecycleValidationReport(string reason)
    {
        if (_shutdownLifecycleReportWritten)
        {
            return true;
        }

        if (_framework == null ||
            _lifecycleValidationEnabled == null ||
            _lifecycleValidationWriteOnShutdown == null ||
            !_lifecycleValidationEnabled.Value ||
            !_lifecycleValidationWriteOnShutdown.Value)
        {
            return false;
        }

        _shutdownLifecycleReportWritten = WriteLifecycleValidationReport(reason, disposeAfterSnapshot: true);
        return _shutdownLifecycleReportWritten;
    }

    private void OnDestroy()
    {
        bool frameworkDisposedByReport = TryWriteShutdownLifecycleValidationReport("plugin-destroy");

        _importerLifecycleOpen = false;
        _frameworkHarmony?.UnpatchSelf();
        _frameworkHarmony = null;
        if (!frameworkDisposedByReport)
        {
            _framework?.Dispose();
        }

        _framework = null!;
        if (ReferenceEquals(Instance, this))
        {
            Instance = null;
        }
    }

    internal bool ImporterLifecycleOpen => _importerLifecycleOpen && _framework != null;

    internal bool RegisterWeaponDefinition(TaintedWeaponDefinition definition, out string message)
    {
        if (_framework == null)
        {
            message = "framework-not-initialized";
            return false;
        }

        TaintedWeaponRegistrationResult result = _framework.RegisterDefinition(definition);
        message = result.ToString();
        return result.Accepted;
    }

    internal bool RegisterNativeWeaponTemplate(
        TaintedWeaponNativeItemRegistrationRequest request,
        out TaintedWeaponNativeItemRegistrationReceipt receipt)
    {
        if (_framework == null)
        {
            receipt = TaintedWeaponNativeItemRegistrationReceipt.DeniedWithoutDefinition(
                request,
                "framework-not-initialized",
                "The Tainted Weapons framework is not initialized.");
            return false;
        }

        receipt = _framework.RegisterNativeWeaponTemplate(request);
        return receipt.Accepted;
    }

    internal bool TryGetNativeItemRegistrationReceipt(
        string registryKey,
        out TaintedWeaponNativeItemRegistrationReceipt? receipt)
    {
        receipt = null;
        return _framework?.TryGetNativeItemRegistrationReceipt(registryKey, out receipt) == true;
    }

    internal void ProcessNativeItemRegistrarQueue()
    {
        _framework?.ProcessNativeItemRegistrarQueue();
    }

    internal bool TryRedirectEquippedVisual(ItemEquip itemEquip, ARAssetReference? sourceReference, out ARAssetReference? replacement, out string message)
    {
        replacement = null;
        if (_framework == null)
        {
            message = "framework-not-initialized";
            return false;
        }

        return _framework.TryRedirectEquippedVisual(itemEquip, sourceReference, out replacement, out message);
    }

    internal bool TryCreateEquippedPrototypeHandle(ARAssetReference reference, out ARAsyncOperationHandle<GameObject> handle, out string message)
    {
        handle = default;
        if (_framework == null)
        {
            message = "framework-not-initialized";
            return false;
        }

        return _framework.TryCreateEquippedPrototypeHandle(reference, out handle, out message);
    }

    internal void RecordDrakeLifecycleEvent(TaintedWeaponDrakeLoadingEvent eventData)
    {
        _framework?.RecordDrakeLoadingEvent(eventData);
    }

    internal void ObserveEquippedRuntimeView(ItemEquip itemEquip, CharacterHandBase? handBase, string route)
    {
        TaintedWeaponFramework? framework = _framework;
        if (framework == null || !framework.ShouldObserveEquippedRuntime(itemEquip, handBase))
        {
            return;
        }

        framework?.ObserveEquippedRuntimeView(itemEquip, handBase, route);
        if (framework != null && handBase != null)
        {
            StartCoroutine(framework.ObserveEquippedRuntimeEcsReadiness(itemEquip, handBase, route));
        }
    }

    internal void ObserveCharacterHandMounted(CharacterHandBase handBase, string route)
    {
        TaintedWeaponFramework? framework = _framework;
        if (framework == null || !framework.ShouldObserveCharacterHandMounted(handBase))
        {
            return;
        }

        framework?.ObserveCharacterHandMounted(handBase, route);
        if (framework != null)
        {
            StartCoroutine(framework.ObserveCharacterHandRuntimeEcsReadiness(handBase, route));
        }
    }

    internal void ObserveInventoryPreviewRuntimeView(ItemEquip itemEquip, CharacterHandBase? handBase, CustomHeroClothes? previewOwner, string route)
    {
        TaintedWeaponFramework? framework = _framework;
        framework?.ObserveInventoryPreviewRuntimeView(itemEquip, handBase, previewOwner, route);
        if (framework != null && handBase != null)
        {
            StartCoroutine(framework.ObserveInventoryPreviewRuntimeEcsReadiness(itemEquip, handBase, previewOwner, route));
        }
    }

    private void Update()
    {
        if (_framework?.HasPendingNativeItemRegistrations == true &&
            Time.realtimeSinceStartup >= _nextNativeItemRegistrarQueueAt)
        {
            _nextNativeItemRegistrarQueueAt = Time.realtimeSinceStartup + 0.5f;
            _framework.ProcessNativeItemRegistrarQueue();
        }

        ReloadLifecycleValidationConfigIfChanged();
        RefreshSceneTracking();
        WriteRequestedLifecycleValidationReport();

        if (!_probeEnabled.Value)
        {
            return;
        }

        if (_autoRunOnce.Value &&
            !_autoRunComplete &&
            Time.realtimeSinceStartup >= _autoRunAtTime &&
            IsAutoRunSceneReady(SceneManager.GetActiveScene()))
        {
            _autoRunAttempts++;
            bool foundRequiredEvidence = WriteWeaponMaterialProbe("auto-run");
            if (foundRequiredEvidence)
            {
                _autoRunComplete = true;
            }
            else if (_autoRunAttempts >= MaxAutoRunAttempts)
            {
                _autoRunComplete = true;
                Logger.LogWarning($"{PluginName} auto-run stopped after {_autoRunAttempts.ToString(CultureInfo.InvariantCulture)} attempts. The probe did not find required target evidence with TargetPriorityFilters={Clean(_targetPriorityFilters.Value)} and TargetFilters={Clean(_targetFilters.Value)}.");
            }
            else
            {
                _autoRunAtTime = Time.realtimeSinceStartup + AutoRetrySeconds;
                Logger.LogInfo($"{PluginName} auto-run did not find required target evidence; retry={_autoRunAttempts.ToString(CultureInfo.InvariantCulture)}/{MaxAutoRunAttempts.ToString(CultureInfo.InvariantCulture)}, nextSeconds={FormatFloat(AutoRetrySeconds)}, scene={Clean(SceneManager.GetActiveScene().name)}.");
            }
        }

        if (Input.GetKeyDown(_probeTriggerKey.Value))
        {
            WriteWeaponMaterialProbe("hotkey");
        }
    }

    private void ReloadLifecycleValidationConfigIfChanged()
    {
        if (Time.realtimeSinceStartup < _nextLifecycleValidationConfigReloadAt)
        {
            return;
        }

        _nextLifecycleValidationConfigReloadAt = Time.realtimeSinceStartup + LifecycleValidationConfigReloadSeconds;

        long currentWriteTicks = ReadConfigFileWriteTicks();
        if (currentWriteTicks <= 0 || currentWriteTicks == _lastLifecycleValidationConfigWriteTicks)
        {
            return;
        }

        try
        {
            Config.Reload();
            _lastLifecycleValidationConfigWriteTicks = ReadConfigFileWriteTicks();
            Logger.LogInfo($"{PluginName} lifecycle validation config reloaded; path={Clean(Config.ConfigFilePath)}");
        }
        catch (Exception ex)
        {
            _lastLifecycleValidationConfigWriteTicks = currentWriteTicks;
            Logger.LogWarning($"{PluginName} lifecycle validation config reload failed: {ex.GetType().Name}: {ex.Message}");
        }
    }

    private long ReadConfigFileWriteTicks()
    {
        try
        {
            string path = Config.ConfigFilePath;
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return 0;
            }

            return File.GetLastWriteTimeUtc(path).Ticks;
        }
        catch
        {
            return 0;
        }
    }

    private void RefreshSceneTracking()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        string sceneName = activeScene.IsValid() ? activeScene.name : string.Empty;
        if (string.Equals(sceneName, _lastActiveSceneName, StringComparison.Ordinal))
        {
            return;
        }

        string previousSceneName = _lastActiveSceneName;
        _lastActiveSceneName = sceneName;
        _autoRunAttempts = 0;
        ScheduleAutoRun();
        _framework?.RecordLifecycleSceneTransition(previousSceneName, sceneName);
        if (_lifecycleValidationEnabled.Value && _lifecycleValidationWriteOnSceneChange.Value)
        {
            WriteLifecycleValidationReport("scene-change:" + previousSceneName + "->" + sceneName, disposeAfterSnapshot: false);
        }
    }

    private void ScheduleAutoRun()
    {
        _autoRunAtTime = Time.realtimeSinceStartup + Mathf.Clamp(_initialDelaySeconds.Value, 0f, 60f);
    }

    private bool WriteWeaponMaterialProbe(string reason)
    {
        string[] filters = ParseFilters(_targetFilters.Value);
        string[] priorityFilters = ParseFilters(_targetPriorityFilters.Value);
        Scene scene = SceneManager.GetActiveScene();
        string outputPath = BuildOutputPath();
        ProbeReport report = CollectReport(filters, priorityFilters);

        try
        {
            WriteReport(outputPath, reason, scene, filters, priorityFilters, report);
            Logger.LogInfo($"{PluginName} weapon material probe wrote {Clean(outputPath)}; reason={Clean(reason)}, rendererRows={report.RendererRows.Count}, drakeMaterialRows={report.DrakeMaterialRows.Count}, componentRows={report.ComponentRows.Count}, priorityRows={report.PriorityRows}, candidateRoots={report.CandidateRoots.Count}, Safety={SafetyFields}.");
            return report.HasRequiredEvidence(priorityFilters);
        }
        catch (Exception ex)
        {
            Logger.LogError($"{PluginName} weapon material probe failed: {ex.GetType().Name}: {ex.Message}");
            return false;
        }
    }

    private void WriteRequestedLifecycleValidationReport()
    {
        if (!_lifecycleValidationEnabled.Value)
        {
            _lastLifecycleValidationRequestId = _lifecycleValidationRequestId.Value;
            return;
        }

        int requestId = _lifecycleValidationRequestId.Value;
        if (requestId == _lastLifecycleValidationRequestId)
        {
            return;
        }

        _lastLifecycleValidationRequestId = requestId;
        WriteLifecycleValidationReport("request-id-" + requestId.ToString(CultureInfo.InvariantCulture), disposeAfterSnapshot: false);
    }

    private bool WriteLifecycleValidationReport(string reason, bool disposeAfterSnapshot)
    {
        if (_framework == null)
        {
            return false;
        }

        Scene scene = SceneManager.GetActiveScene();
        string outputPath = BuildLifecycleOutputPath();
        try
        {
            if (_framework.TryWriteLifecycleValidationReport(outputPath, reason, scene.IsValid() ? scene.name : string.Empty, disposeAfterSnapshot, out string message))
            {
                Logger.LogInfo($"{PluginName} lifecycle validation wrote {Clean(outputPath)}; reason={Clean(reason)}; {message}");
                return disposeAfterSnapshot;
            }

            Logger.LogWarning($"{PluginName} lifecycle validation failed; reason={Clean(reason)}; message={Clean(message)}");
            return false;
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"{PluginName} lifecycle validation failed; reason={Clean(reason)}; error={ex.GetType().Name}: {ex.Message}");
            return false;
        }
    }

    private ProbeReport CollectReport(IReadOnlyList<string> filters, IReadOnlyList<string> priorityFilters)
    {
        var report = new ProbeReport();
        var roots = new HashSet<string>(StringComparer.Ordinal);
        var seenRenderers = new HashSet<int>();
        var seenDrakeRenderers = new HashSet<int>();
        var seenComponents = new HashSet<int>();

        AddRendererMatches(report, roots, priorityFilters, true, seenRenderers);
        AddRendererMatches(report, roots, filters, false, seenRenderers);
        AddDrakeMaterialRows(report, roots, priorityFilters, true, seenDrakeRenderers);
        AddDrakeMaterialRows(report, roots, filters, false, seenDrakeRenderers);
        AddComponentMatches(report, roots, priorityFilters, true, seenComponents);
        AddComponentMatches(report, roots, filters, false, seenComponents);

        report.CandidateRoots.AddRange(roots.OrderBy(root => root, StringComparer.Ordinal));
        return report;
    }

    private void AddRendererMatches(ProbeReport report, HashSet<string> roots, IReadOnlyList<string> filters, bool priorityMatch, HashSet<int> seenRenderers)
    {
        if (filters.Count == 0)
        {
            return;
        }

        foreach (Renderer renderer in Resources.FindObjectsOfTypeAll<Renderer>())
        {
            if (report.RendererRows.Count >= MaxRows)
            {
                break;
            }

            if (!IsLoadedSceneObject(renderer))
            {
                continue;
            }

            int instanceId = renderer.GetInstanceID();
            if (seenRenderers.Contains(instanceId))
            {
                continue;
            }

            string path = BuildPath(renderer.transform);
            Material[] materials = SafeMaterials(renderer);
            if (!MatchesRenderer(path, filters, materials))
            {
                continue;
            }

            seenRenderers.Add(instanceId);
            roots.Add(FindCandidateRootPath(renderer.transform, filters, path));
            int beforeCount = report.RendererRows.Count;
            AddRendererRows(report.RendererRows, renderer, path, materials);
            AddPriorityRows(report, priorityMatch, report.RendererRows.Count - beforeCount);
        }
    }

    private void AddComponentMatches(ProbeReport report, HashSet<string> roots, IReadOnlyList<string> filters, bool priorityMatch, HashSet<int> seenComponents)
    {
        if (filters.Count == 0)
        {
            return;
        }

        foreach (MonoBehaviour component in Resources.FindObjectsOfTypeAll<MonoBehaviour>())
        {
            if (report.ComponentRows.Count >= MaxRows)
            {
                break;
            }

            if (!IsLoadedSceneObject(component))
            {
                continue;
            }

            int instanceId = component.GetInstanceID();
            if (seenComponents.Contains(instanceId))
            {
                continue;
            }

            string path = BuildPath(component.transform);
            if (!MatchesAny(path, filters) && !IsInterestingComponent(component))
            {
                continue;
            }

            if (!MatchesAny(path, filters) && !ComponentMentionsFilter(component, filters))
            {
                continue;
            }

            seenComponents.Add(instanceId);
            roots.Add(FindCandidateRootPath(component.transform, filters, path));
            report.ComponentRows.Add(BuildComponentRow(component, path));
            AddPriorityRows(report, priorityMatch, 1);
        }
    }

    private void AddDrakeMaterialRows(ProbeReport report, HashSet<string> roots, IReadOnlyList<string> filters, bool priorityMatch, HashSet<int> seenDrakeRenderers)
    {
        if (filters.Count == 0)
        {
            return;
        }

        Dictionary<string, Material> loadedMaterials = LoadedMaterialsTracker.Instance?.MaterialKeyToLoadedMaterialMap ?? new Dictionary<string, Material>();

        foreach (DrakeMeshRenderer drake in Resources.FindObjectsOfTypeAll<DrakeMeshRenderer>())
        {
            if (report.DrakeMaterialRows.Count >= MaxRows)
            {
                break;
            }

            if (!IsLoadedSceneObject(drake))
            {
                continue;
            }

            int instanceId = drake.GetInstanceID();
            if (seenDrakeRenderers.Contains(instanceId))
            {
                continue;
            }

            string path = BuildPath(drake.transform);
            if (!DrakeMatches(path, drake, filters, loadedMaterials))
            {
                continue;
            }

            seenDrakeRenderers.Add(instanceId);
            roots.Add(FindCandidateRootPath(drake.transform, filters, path));
            int beforeCount = report.DrakeMaterialRows.Count;
            AddDrakeRows(report.DrakeMaterialRows, drake, path, loadedMaterials);
            AddPriorityRows(report, priorityMatch, report.DrakeMaterialRows.Count - beforeCount);
        }
    }

    private static void AddPriorityRows(ProbeReport report, bool priorityMatch, int addedRows)
    {
        if (priorityMatch && addedRows > 0)
        {
            report.PriorityRows += addedRows;
        }
    }

    private void AddDrakeRows(List<DrakeMaterialRow> rows, DrakeMeshRenderer drake, string path, IReadOnlyDictionary<string, Material> loadedMaterials)
    {
        string meshGuid = SafeMeshGuid(drake);
        string meshSubObject = SafeMeshSubObject(drake);
        Material[] overrides = SafeRuntimeOverrideMaterials(drake);
        var references = SafeMaterialReferences(drake);
        if (references.Length == 0)
        {
            rows.Add(DrakeMaterialRow.ForMissingReference(drake, path, meshGuid, meshSubObject, "drake-material-reference-array-empty"));
            return;
        }

        for (int materialSlot = 0; materialSlot < references.Length && rows.Count < MaxRows; materialSlot++)
        {
            string materialKey = SafeRuntimeKey(references[materialSlot]);
            bool runtimeOverride = materialSlot < overrides.Length && overrides[materialSlot] != null;
            Material? material = runtimeOverride
                ? overrides[materialSlot]
                : (loadedMaterials.TryGetValue(materialKey, out Material? loadedMaterial) ? loadedMaterial : null);

            if (material == null)
            {
                rows.Add(DrakeMaterialRow.ForMissingMaterial(drake, path, meshGuid, meshSubObject, materialSlot, materialKey, runtimeOverride));
                continue;
            }

            string[] propertyNames = SafeTexturePropertyNames(material);
            if (propertyNames.Length == 0)
            {
                rows.Add(DrakeMaterialRow.ForMaterialWithoutTextures(drake, path, meshGuid, meshSubObject, materialSlot, materialKey, runtimeOverride, material));
                continue;
            }

            foreach (string propertyName in propertyNames)
            {
                if (rows.Count >= MaxRows)
                {
                    break;
                }

                Texture? texture = SafeGetTexture(material, propertyName);
                rows.Add(DrakeMaterialRow.ForTexture(drake, path, meshGuid, meshSubObject, materialSlot, materialKey, runtimeOverride, material, propertyName, texture));
            }
        }
    }

    private void AddRendererRows(List<RendererRow> rows, Renderer renderer, string path, Material[] materials)
    {
        if (materials.Length == 0)
        {
            rows.Add(RendererRow.ForEmptyRenderer(renderer, path));
            return;
        }

        for (int materialSlot = 0; materialSlot < materials.Length && rows.Count < MaxRows; materialSlot++)
        {
            Material? material = materials[materialSlot];
            if (material == null)
            {
                rows.Add(RendererRow.ForNullMaterial(renderer, path, materialSlot));
                continue;
            }

            string[] propertyNames = SafeTexturePropertyNames(material);
            if (propertyNames.Length == 0)
            {
                rows.Add(RendererRow.ForMaterialWithoutTextures(renderer, path, materialSlot, material));
                continue;
            }

            foreach (string propertyName in propertyNames)
            {
                if (rows.Count >= MaxRows)
                {
                    break;
                }

                Texture? texture = SafeGetTexture(material, propertyName);
                rows.Add(RendererRow.ForTexture(renderer, path, materialSlot, material, propertyName, texture));
            }
        }
    }

    private static bool MatchesRenderer(string path, IReadOnlyList<string> filters, IReadOnlyList<Material> materials)
    {
        if (MatchesAny(path, filters))
        {
            return true;
        }

        foreach (Material? material in materials)
        {
            if (material == null)
            {
                continue;
            }

            if (MatchesAny(material.name, filters) || (material.shader != null && MatchesAny(material.shader.name, filters)))
            {
                return true;
            }

            foreach (string propertyName in SafeTexturePropertyNames(material))
            {
                Texture? texture = SafeGetTexture(material, propertyName);
                if (texture != null && MatchesAny(texture.name, filters))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool DrakeMatches(string path, DrakeMeshRenderer drake, IReadOnlyList<string> filters, IReadOnlyDictionary<string, Material> loadedMaterials)
    {
        if (MatchesAny(path, filters) ||
            MatchesAny(SafeMeshGuid(drake), filters) ||
            MatchesAny(SafeMeshSubObject(drake), filters))
        {
            return true;
        }

        foreach (object? reference in SafeMaterialReferences(drake))
        {
            string materialKey = SafeRuntimeKey(reference);
            if (MatchesAny(materialKey, filters))
            {
                return true;
            }

            if (loadedMaterials.TryGetValue(materialKey, out Material? material) && material != null)
            {
                if (MatchesAny(material.name, filters) || (material.shader != null && MatchesAny(material.shader.name, filters)))
                {
                    return true;
                }

                foreach (string propertyName in SafeTexturePropertyNames(material))
                {
                    Texture? texture = SafeGetTexture(material, propertyName);
                    if (texture != null && MatchesAny(texture.name, filters))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private bool ComponentMentionsFilter(MonoBehaviour component, IReadOnlyList<string> filters)
    {
        foreach (MemberInfo member in EnumerateInterestingMembers(component.GetType()))
        {
            if (!TryReadMember(component, member, out object? value))
            {
                continue;
            }

            string description = DescribeValue(value);
            if (MatchesAny(description, filters))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsInterestingComponent(MonoBehaviour component)
    {
        string typeName = component.GetType().FullName ?? component.GetType().Name;
        return
            typeName.IndexOf("Weapon", StringComparison.OrdinalIgnoreCase) >= 0 ||
            typeName.IndexOf("Kandra", StringComparison.OrdinalIgnoreCase) >= 0 ||
            typeName.IndexOf("Drake", StringComparison.OrdinalIgnoreCase) >= 0 ||
            typeName.IndexOf("Renderer", StringComparison.OrdinalIgnoreCase) >= 0 ||
            typeName.IndexOf("Visual", StringComparison.OrdinalIgnoreCase) >= 0 ||
            typeName.IndexOf("VFX", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private ComponentRow BuildComponentRow(MonoBehaviour component, string path)
    {
        Type type = component.GetType();
        var values = new List<string>();

        foreach (MemberInfo member in EnumerateInterestingMembers(type))
        {
            if (values.Count >= MaxMemberValues)
            {
                break;
            }

            if (!TryReadMember(component, member, out object? value))
            {
                continue;
            }

            values.Add(member.Name + "=" + DescribeValue(value));
        }

        return new ComponentRow(
            path,
            type.FullName ?? type.Name,
            component.enabled,
            component.gameObject.activeSelf,
            component.gameObject.activeInHierarchy,
            string.Join(";", values));
    }

    private static IEnumerable<MemberInfo> EnumerateInterestingMembers(Type type)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        var seen = new HashSet<string>(StringComparer.Ordinal);

        for (Type? current = type; current != null; current = current.BaseType)
        {
            foreach (FieldInfo field in current.GetFields(flags))
            {
                if (seen.Add("F:" + field.Name) && IsInterestingMember(field.Name, field.FieldType))
                {
                    yield return field;
                }
            }

        }
    }

    private static bool IsInterestingMember(string name, Type type)
    {
        string haystack = name + "|" + type.FullName + "|" + type.Name;
        return
            haystack.IndexOf("weapon", StringComparison.OrdinalIgnoreCase) >= 0 ||
            haystack.IndexOf("material", StringComparison.OrdinalIgnoreCase) >= 0 ||
            haystack.IndexOf("texture", StringComparison.OrdinalIgnoreCase) >= 0 ||
            haystack.IndexOf("renderer", StringComparison.OrdinalIgnoreCase) >= 0 ||
            haystack.IndexOf("mesh", StringComparison.OrdinalIgnoreCase) >= 0 ||
            haystack.IndexOf("visual", StringComparison.OrdinalIgnoreCase) >= 0 ||
            haystack.IndexOf("vfx", StringComparison.OrdinalIgnoreCase) >= 0 ||
            haystack.IndexOf("kandra", StringComparison.OrdinalIgnoreCase) >= 0 ||
            haystack.IndexOf("drake", StringComparison.OrdinalIgnoreCase) >= 0 ||
            typeof(UnityEngine.Object).IsAssignableFrom(type);
    }

    private static bool TryReadMember(object instance, MemberInfo member, out object? value)
    {
        try
        {
            switch (member)
            {
                case FieldInfo field:
                    value = field.GetValue(instance);
                    return true;
                default:
                    value = null;
                    return false;
            }
        }
        catch
        {
            value = null;
            return false;
        }
    }

    private static Material[] SafeMaterials(Renderer renderer)
    {
        try
        {
            return renderer.sharedMaterials ?? Array.Empty<Material>();
        }
        catch
        {
            return Array.Empty<Material>();
        }
    }

    private static string[] SafeTexturePropertyNames(Material material)
    {
        try
        {
            return material.GetTexturePropertyNames() ?? Array.Empty<string>();
        }
        catch
        {
            return Array.Empty<string>();
        }
    }

    private static Texture? SafeGetTexture(Material material, string propertyName)
    {
        try
        {
            return material.HasProperty(propertyName) ? material.GetTexture(propertyName) : null;
        }
        catch
        {
            return null;
        }
    }

    private static object?[] SafeMaterialReferences(DrakeMeshRenderer drake)
    {
        try
        {
            return drake.MaterialReferences?.Cast<object?>().ToArray() ?? Array.Empty<object?>();
        }
        catch
        {
            return Array.Empty<object?>();
        }
    }

    private static Material[] SafeRuntimeOverrideMaterials(DrakeMeshRenderer drake)
    {
        try
        {
            return drake.RuntimeOverrideMaterials ?? Array.Empty<Material>();
        }
        catch
        {
            return Array.Empty<Material>();
        }
    }

    private static string SafeRuntimeKey(object? assetReference)
    {
        try
        {
            object? runtimeKey = assetReference?.GetType()
                .GetProperty("RuntimeKey", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                ?.GetValue(assetReference);
            return runtimeKey?.ToString() ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string SafeMeshGuid(DrakeMeshRenderer drake)
    {
        try
        {
            return drake.MeshReferenceData.Item1 ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string SafeMeshSubObject(DrakeMeshRenderer drake)
    {
        try
        {
            return drake.MeshReferenceData.Item2 ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string SafeHasEntitiesAccess(DrakeMeshRenderer drake)
    {
        try
        {
            return drake.HasEntitiesAccess ? "true" : "false";
        }
        catch
        {
            return "unknown";
        }
    }

    private static bool IsLoadedSceneObject(Component? component)
    {
        if (component == null || component.gameObject == null)
        {
            return false;
        }

        Scene scene = component.gameObject.scene;
        return scene.IsValid() && scene.isLoaded;
    }

    private static bool IsAutoRunSceneReady(Scene scene)
    {
        return scene.IsValid() &&
               scene.isLoaded &&
               !string.Equals(scene.name, "BuildInitialScene", StringComparison.Ordinal) &&
               !string.Equals(scene.name, "TitleScreen", StringComparison.Ordinal);
    }

    private static string FindCandidateRootPath(Transform transform, IReadOnlyList<string> filters, string fallbackPath)
    {
        var chain = new Stack<Transform>();
        for (Transform? current = transform; current != null; current = current.parent)
        {
            chain.Push(current);
        }

        var partial = new List<string>();
        foreach (Transform current in chain)
        {
            partial.Add(current.name);
            if (MatchesAny(current.name, filters))
            {
                return "/" + string.Join("/", partial);
            }
        }

        return fallbackPath;
    }

    private static string BuildPath(Transform transform)
    {
        var names = new Stack<string>();
        for (Transform? current = transform; current != null; current = current.parent)
        {
            names.Push(current.name);
        }

        return "/" + string.Join("/", names);
    }

    private static string[] ParseFilters(string value)
    {
        return value
            .Split(new[] { '|', ';' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(filter => filter.Trim())
            .Where(filter => filter.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static bool MatchesAny(string value, IReadOnlyList<string> filters)
    {
        if (filters.Count == 0)
        {
            return false;
        }

        foreach (string filter in filters)
        {
            if (value.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }
        }

        return false;
    }

    private static string BuildOutputPath()
    {
        string directory = Path.Combine(Paths.ConfigPath, PluginGuid);
        Directory.CreateDirectory(directory);
        string timestamp = DateTimeOffset.Now.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
        return Path.Combine(directory, "weapon-material-probe-" + timestamp + ".tsv");
    }

    private static string BuildLifecycleOutputPath()
    {
        string directory = Path.Combine(Paths.ConfigPath, PluginGuid);
        Directory.CreateDirectory(directory);
        string timestamp = DateTimeOffset.Now.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
        return Path.Combine(directory, "lifecycle-validation-" + timestamp + ".tsv");
    }

    private static void WriteReport(string outputPath, string reason, Scene scene, IReadOnlyList<string> filters, IReadOnlyList<string> priorityFilters, ProbeReport report)
    {
        using var writer = new StreamWriter(outputPath, append: false, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        writer.WriteLine("# plugin\t" + Tsv(PluginName));
        writer.WriteLine("# version\t" + Tsv(PluginVersion));
        writer.WriteLine("# reason\t" + Tsv(reason));
        writer.WriteLine("# time\t" + Tsv(DateTimeOffset.Now.ToString("O", CultureInfo.InvariantCulture)));
        writer.WriteLine("# frame\t" + Time.frameCount.ToString(CultureInfo.InvariantCulture));
        writer.WriteLine("# unityVersion\t" + Tsv(Application.unityVersion));
        writer.WriteLine("# product\t" + Tsv(Application.productName));
        writer.WriteLine("# applicationVersion\t" + Tsv(Application.version));
        writer.WriteLine("# scene\t" + Tsv(scene.name));
        writer.WriteLine("# sceneLoaded\t" + scene.isLoaded);
        writer.WriteLine("# filters\t" + Tsv(string.Join("|", filters)));
        writer.WriteLine("# priorityFilters\t" + Tsv(string.Join("|", priorityFilters)));
        writer.WriteLine("# safety\t" + Tsv(SafetyFields));
        writer.WriteLine("# rendererRows\t" + report.RendererRows.Count.ToString(CultureInfo.InvariantCulture));
        writer.WriteLine("# drakeMaterialRows\t" + report.DrakeMaterialRows.Count.ToString(CultureInfo.InvariantCulture));
        writer.WriteLine("# componentRows\t" + report.ComponentRows.Count.ToString(CultureInfo.InvariantCulture));
        writer.WriteLine("# priorityRows\t" + report.PriorityRows.ToString(CultureInfo.InvariantCulture));
        writer.WriteLine("# candidateRoots\t" + report.CandidateRoots.Count.ToString(CultureInfo.InvariantCulture));
        writer.WriteLine();

        writer.WriteLine("[candidate_roots]");
        writer.WriteLine("rootPath");
        foreach (string root in report.CandidateRoots)
        {
            writer.WriteLine(Tsv(root));
        }

        writer.WriteLine();
        writer.WriteLine("[renderer_textures]");
        writer.WriteLine(string.Join("\t", RendererRow.Headers));
        foreach (RendererRow row in report.RendererRows)
        {
            writer.WriteLine(row.ToTsv());
        }

        writer.WriteLine();
        writer.WriteLine("[drake_materials]");
        writer.WriteLine(string.Join("\t", DrakeMaterialRow.Headers));
        foreach (DrakeMaterialRow row in report.DrakeMaterialRows)
        {
            writer.WriteLine(row.ToTsv());
        }

        writer.WriteLine();
        writer.WriteLine("[components]");
        writer.WriteLine(string.Join("\t", ComponentRow.Headers));
        foreach (ComponentRow row in report.ComponentRows)
        {
            writer.WriteLine(row.ToTsv());
        }
    }

    private static string DescribeValue(object? value)
    {
        if (value == null)
        {
            return "null";
        }

        if (value is Object unityObject && unityObject == null)
        {
            return "DestroyedUnityObject:" + Clean(value.GetType().FullName ?? value.GetType().Name);
        }

        if (value is Material material)
        {
            return SafeDescribeUnityObject(
                material,
                target => "Material:" + Clean(target.name) + ":shader=" + Clean(target.shader != null ? target.shader.name : "null"));
        }

        if (value is Texture texture)
        {
            return DescribeTexture(texture);
        }

        if (value is Renderer renderer)
        {
            return SafeDescribeUnityObject(
                renderer,
                target => "Renderer:" + Clean(target.GetType().FullName ?? target.GetType().Name) + ":" + Clean(BuildPath(target.transform)));
        }

        if (value is Mesh mesh)
        {
            return SafeDescribeUnityObject(
                mesh,
                target => "Mesh:" + Clean(target.name) + ":vertices=" + target.vertexCount.ToString(CultureInfo.InvariantCulture));
        }

        if (value is Component component)
        {
            return SafeDescribeUnityObject(
                component,
                target => Clean(target.GetType().FullName ?? target.GetType().Name) + ":" + Clean(BuildPath(target.transform)));
        }

        if (value is GameObject gameObject)
        {
            return SafeDescribeUnityObject(
                gameObject,
                target => "GameObject:" + Clean(BuildPath(target.transform)) + ":active=" + target.activeInHierarchy);
        }

        if (value is Transform transform)
        {
            return SafeDescribeUnityObject(
                transform,
                target => "Transform:" + Clean(BuildPath(target)));
        }

        if (value is Object remainingUnityObject)
        {
            return SafeDescribeUnityObject(
                remainingUnityObject,
                target => Clean(target.GetType().FullName ?? target.GetType().Name) + ":" + Clean(target.name));
        }

        if (value is ICollection collection)
        {
            return Clean(value.GetType().Name) + ":count=" + collection.Count.ToString(CultureInfo.InvariantCulture);
        }

        Type valueType = value.GetType();
        if (valueType.IsPrimitive || value is string || value is decimal || value is Enum)
        {
            try
            {
                return Clean(Convert.ToString(value, CultureInfo.InvariantCulture) ?? "unknown");
            }
            catch (Exception ex)
            {
                return Clean(valueType.FullName ?? valueType.Name) + ":stringify-failed=" + ex.GetType().Name;
            }
        }

        return Clean(valueType.FullName ?? valueType.Name);
    }

    private static string SafeDescribeUnityObject<T>(T value, Func<T, string> describe)
        where T : Object
    {
        try
        {
            if (value == null)
            {
                return "DestroyedUnityObject:" + Clean(typeof(T).FullName ?? typeof(T).Name);
            }

            return describe(value);
        }
        catch (Exception ex) when (ex is NullReferenceException || ex is MissingReferenceException || ex is InvalidOperationException)
        {
            return "UnityObjectUnavailable:" + Clean(value.GetType().FullName ?? value.GetType().Name) + ":" + ex.GetType().Name;
        }
    }

    private static string DescribeTexture(Texture texture)
    {
        return SafeDescribeUnityObject(
            texture,
            target =>
            {
                string details = "Texture:" + Clean(target.GetType().FullName ?? target.GetType().Name) + ":" + Clean(target.name) +
                                 ":width=" + target.width.ToString(CultureInfo.InvariantCulture) +
                                 ":height=" + target.height.ToString(CultureInfo.InvariantCulture);

                if (target is Texture2D texture2D)
                {
                    details +=
                        ":format=" + texture2D.format +
                        ":mipmaps=" + texture2D.mipmapCount.ToString(CultureInfo.InvariantCulture) +
                        ":readable=" + SafeReadable(texture2D);
                }

                return details;
            });
    }

    private static string SafeReadable(Texture2D texture)
    {
        try
        {
            return texture.isReadable ? "true" : "false";
        }
        catch
        {
            return "unknown";
        }
    }

    private static string FormatFloat(float value)
    {
        return value.ToString("0.###", CultureInfo.InvariantCulture);
    }

    private static string Tsv(string value)
    {
        return Clean(value).Replace('\t', ' ');
    }

    private static string Clean(string value)
    {
        string cleaned = value
            .Replace('\r', ' ')
            .Replace('\n', ' ')
            .Replace('\t', ' ');

        return cleaned.Length <= 480 ? cleaned : cleaned.Substring(0, 480) + "...";
    }

    private sealed class ProbeReport
    {
        internal List<string> CandidateRoots { get; } = new();

        internal List<RendererRow> RendererRows { get; } = new();

        internal List<DrakeMaterialRow> DrakeMaterialRows { get; } = new();

        internal List<ComponentRow> ComponentRows { get; } = new();

        internal int PriorityRows { get; set; }

        internal bool HasRows => RendererRows.Count > 0 || DrakeMaterialRows.Count > 0 || ComponentRows.Count > 0 || CandidateRoots.Count > 0;

        internal bool HasRequiredEvidence(IReadOnlyList<string> priorityFilters)
        {
            return priorityFilters.Count == 0 ? HasRows : PriorityRows > 0;
        }
    }

    private sealed class RendererRow
    {
        internal static readonly string[] Headers =
        {
            "objectPath",
            "rendererType",
            "rendererName",
            "enabled",
            "activeSelf",
            "activeInHierarchy",
            "materialSlot",
            "materialName",
            "materialInstanceId",
            "shaderName",
            "textureProperty",
            "textureName",
            "textureType",
            "width",
            "height",
            "format",
            "mipmaps",
            "readable"
        };

        private readonly string[] _values;

        private RendererRow(params string[] values)
        {
            _values = values;
        }

        internal static RendererRow ForEmptyRenderer(Renderer renderer, string path)
        {
            return Create(renderer, path, -1, null, "none", null);
        }

        internal static RendererRow ForNullMaterial(Renderer renderer, string path, int materialSlot)
        {
            return Create(renderer, path, materialSlot, null, "null-material", null);
        }

        internal static RendererRow ForMaterialWithoutTextures(Renderer renderer, string path, int materialSlot, Material material)
        {
            return Create(renderer, path, materialSlot, material, "none", null);
        }

        internal static RendererRow ForTexture(Renderer renderer, string path, int materialSlot, Material material, string textureProperty, Texture? texture)
        {
            return Create(renderer, path, materialSlot, material, textureProperty, texture);
        }

        internal string ToTsv()
        {
            return string.Join("\t", _values.Select(Tsv));
        }

        private static RendererRow Create(Renderer renderer, string path, int materialSlot, Material? material, string textureProperty, Texture? texture)
        {
            string format = "";
            string mipmaps = "";
            string readable = "";
            if (texture is Texture2D texture2D)
            {
                format = texture2D.format.ToString();
                mipmaps = texture2D.mipmapCount.ToString(CultureInfo.InvariantCulture);
                readable = SafeReadable(texture2D);
            }

            return new RendererRow(
                path,
                renderer.GetType().FullName ?? renderer.GetType().Name,
                renderer.name,
                renderer.enabled.ToString(CultureInfo.InvariantCulture),
                renderer.gameObject.activeSelf.ToString(CultureInfo.InvariantCulture),
                renderer.gameObject.activeInHierarchy.ToString(CultureInfo.InvariantCulture),
                materialSlot.ToString(CultureInfo.InvariantCulture),
                material == null ? "" : material.name,
                material == null ? "" : material.GetInstanceID().ToString(CultureInfo.InvariantCulture),
                material?.shader == null ? "" : material.shader.name,
                textureProperty,
                texture == null ? "" : texture.name,
                texture == null ? "" : texture.GetType().FullName ?? texture.GetType().Name,
                texture == null ? "" : texture.width.ToString(CultureInfo.InvariantCulture),
                texture == null ? "" : texture.height.ToString(CultureInfo.InvariantCulture),
                format,
                mipmaps,
                readable);
        }
    }

    private sealed class DrakeMaterialRow
    {
        internal static readonly string[] Headers =
        {
            "objectPath",
            "componentType",
            "enabled",
            "activeSelf",
            "activeInHierarchy",
            "hasEntitiesAccess",
            "meshGuid",
            "meshSubObject",
            "materialSlot",
            "materialRuntimeKey",
            "runtimeOverrideMaterial",
            "materialLoaded",
            "materialName",
            "materialInstanceId",
            "shaderName",
            "textureProperty",
            "textureName",
            "textureType",
            "width",
            "height",
            "format",
            "mipmaps",
            "readable",
            "notes"
        };

        private readonly string[] _values;

        private DrakeMaterialRow(params string[] values)
        {
            _values = values;
        }

        internal static DrakeMaterialRow ForMissingReference(DrakeMeshRenderer drake, string path, string meshGuid, string meshSubObject, string notes)
        {
            return Create(drake, path, meshGuid, meshSubObject, -1, string.Empty, false, null, "none", null, notes);
        }

        internal static DrakeMaterialRow ForMissingMaterial(DrakeMeshRenderer drake, string path, string meshGuid, string meshSubObject, int materialSlot, string materialKey, bool runtimeOverride)
        {
            return Create(drake, path, meshGuid, meshSubObject, materialSlot, materialKey, runtimeOverride, null, "none", null, "drake-material-not-loaded");
        }

        internal static DrakeMaterialRow ForMaterialWithoutTextures(DrakeMeshRenderer drake, string path, string meshGuid, string meshSubObject, int materialSlot, string materialKey, bool runtimeOverride, Material material)
        {
            return Create(drake, path, meshGuid, meshSubObject, materialSlot, materialKey, runtimeOverride, material, "none", null, "drake-material-has-no-texture-properties");
        }

        internal static DrakeMaterialRow ForTexture(DrakeMeshRenderer drake, string path, string meshGuid, string meshSubObject, int materialSlot, string materialKey, bool runtimeOverride, Material material, string textureProperty, Texture? texture)
        {
            return Create(drake, path, meshGuid, meshSubObject, materialSlot, materialKey, runtimeOverride, material, textureProperty, texture, texture == null ? "drake-texture-slot-empty" : "drake-read-only-slot-observation");
        }

        internal string ToTsv()
        {
            return string.Join("\t", _values.Select(Tsv));
        }

        private static DrakeMaterialRow Create(DrakeMeshRenderer drake, string path, string meshGuid, string meshSubObject, int materialSlot, string materialKey, bool runtimeOverride, Material? material, string textureProperty, Texture? texture, string notes)
        {
            string format = "";
            string mipmaps = "";
            string readable = "";
            if (texture is Texture2D texture2D)
            {
                format = texture2D.format.ToString();
                mipmaps = texture2D.mipmapCount.ToString(CultureInfo.InvariantCulture);
                readable = SafeReadable(texture2D);
            }

            return new DrakeMaterialRow(
                path,
                drake.GetType().FullName ?? drake.GetType().Name,
                drake.enabled.ToString(CultureInfo.InvariantCulture),
                drake.gameObject.activeSelf.ToString(CultureInfo.InvariantCulture),
                drake.gameObject.activeInHierarchy.ToString(CultureInfo.InvariantCulture),
                SafeHasEntitiesAccess(drake),
                meshGuid,
                meshSubObject,
                materialSlot.ToString(CultureInfo.InvariantCulture),
                materialKey,
                runtimeOverride.ToString(CultureInfo.InvariantCulture),
                (material != null).ToString(CultureInfo.InvariantCulture),
                material == null ? "" : material.name,
                material == null ? "" : material.GetInstanceID().ToString(CultureInfo.InvariantCulture),
                material?.shader == null ? "" : material.shader.name,
                textureProperty,
                texture == null ? "" : texture.name,
                texture == null ? "" : texture.GetType().FullName ?? texture.GetType().Name,
                texture == null ? "" : texture.width.ToString(CultureInfo.InvariantCulture),
                texture == null ? "" : texture.height.ToString(CultureInfo.InvariantCulture),
                format,
                mipmaps,
                readable,
                notes);
        }
    }

    private sealed class ComponentRow
    {
        internal static readonly string[] Headers =
        {
            "objectPath",
            "componentType",
            "enabled",
            "activeSelf",
            "activeInHierarchy",
            "memberValues"
        };

        private readonly string[] _values;

        internal ComponentRow(string path, string componentType, bool enabled, bool activeSelf, bool activeInHierarchy, string memberValues)
        {
            _values = new[]
            {
                path,
                componentType,
                enabled.ToString(CultureInfo.InvariantCulture),
                activeSelf.ToString(CultureInfo.InvariantCulture),
                activeInHierarchy.ToString(CultureInfo.InvariantCulture),
                memberValues
            };
        }

        internal string ToTsv()
        {
            return string.Join("\t", _values.Select(Tsv));
        }
    }
}
