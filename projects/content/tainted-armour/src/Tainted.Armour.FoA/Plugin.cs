using System;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using Newtonsoft.Json;
using UnityEngine;

namespace Tainted.Armour.FoA;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "theboyyss.tgfoa.tainted-armour.foa-observer";
    public const string PluginName = "Tainted Armour FoA Observer";
    public const string PluginVersion = "0.1.0";

    private ConfigEntry<bool> observerEnabled = null!;
    private ConfigEntry<KeyCode> captureHotkey = null!;
    private ConfigEntry<bool> pollConfigForCaptureRequest = null!;
    private ConfigEntry<bool> captureWhenReady = null!;
    private ConfigEntry<int> captureRequestId = null!;
    private ConfigEntry<float> captureRequestPollSeconds = null!;
    private ConfigEntry<float> captureReadinessTimeoutSeconds = null!;
    private ConfigEntry<int> maxRendererRows = null!;
    private ConfigEntry<string> outputFolder = null!;
    private ConfigEntry<bool> kandraRegistrationEnabled = null!;
    private ConfigEntry<int> kandraRegistrationRequestId = null!;
    private ConfigEntry<string> kandraRegistrationInvocationContextPath = null!;
    private ConfigEntry<string> kandraRegistrationTemplateRendererPathContains = null!;
    private ConfigEntry<int> kandraRegistrationMaxPollFrames = null!;
    private ConfigEntry<bool> kandraSameMeshAbProofEnabled = null!;
    private ConfigEntry<int> kandraSameMeshAbProofRequestId = null!;
    private ConfigEntry<string> kandraSameMeshAbProofTemplateRendererPathContains = null!;
    private ConfigEntry<string> kandraSameMeshAbProofModDirectory = null!;
    private ConfigEntry<string> kandraSameMeshAbProofMeshNamePrefix = null!;
    private ConfigEntry<int> kandraSameMeshAbProofMaxPollFrames = null!;
    private ConfigEntry<bool> kandraSameMeshVisualEvidenceEnabled = null!;
    private ConfigEntry<int> kandraSameMeshVisualEvidenceRequestId = null!;
    private ConfigEntry<string> kandraSameMeshVisualEvidenceSourceReceiptPath = null!;
    private ConfigEntry<string> kandraSameMeshVisualEvidenceTemplateRendererPathContains = null!;
    private ConfigEntry<string> kandraSameMeshVisualEvidenceAcceptedVariantId = null!;
    private ConfigEntry<bool> kandraSameMeshVisualEvidenceRejectAllNonOriginalCandidates = null!;
    private ConfigEntry<int> kandraSameMeshVisualEvidenceMaxPollFrames = null!;
    private ConfigEntry<int> kandraSameMeshVisualEvidenceScreenshotWaitFrames = null!;
    private int lastCaptureRequestId;
    private float nextCaptureRequestPollTime;
    private int pendingCaptureRequestId;
    private string pendingCaptureTrigger = string.Empty;
    private string pendingCaptureLastStatus = string.Empty;
    private float pendingCaptureDeadline;
    private int lastKandraRegistrationRequestId;
    private float nextKandraRegistrationRequestPollTime;
    private FoaKandraRuntimeRegistrationInvoker? kandraRegistrationInvoker;
    private FoaKandraRuntimeRegistrationInvoker.PendingInvocation? pendingKandraRegistration;
    private KandraRuntimeRegistrationInvocationContext? pendingKandraRegistrationContext;
    private string pendingKandraRegistrationTrigger = string.Empty;
    private FoaKandraRuntimeRegistrationInvoker.PendingInvocation? activeKandraRegistration;
    private int lastKandraSameMeshAbProofRequestId;
    private float nextKandraSameMeshAbProofRequestPollTime;
    private FoaKandraSameMeshAbProofRunner? kandraSameMeshAbProofRunner;
    private FoaKandraSameMeshAbProofRunner.PendingProof? pendingKandraSameMeshAbProof;
    private string pendingKandraSameMeshAbProofTrigger = string.Empty;
    private int lastKandraSameMeshVisualEvidenceRequestId;
    private float nextKandraSameMeshVisualEvidenceRequestPollTime;
    private FoaKandraSameMeshVisualEvidenceRunner? kandraSameMeshVisualEvidenceRunner;
    private FoaKandraSameMeshVisualEvidenceRunner.PendingVisualEvidence? pendingKandraSameMeshVisualEvidence;
    private string pendingKandraSameMeshVisualEvidenceTrigger = string.Empty;

    private void Awake()
    {
        observerEnabled = Config.Bind("General", "Enabled", true, "Enable the Tainted Armour no-write observer transport.");
        captureHotkey = Config.Bind("A2K-T34", "CaptureHotkey", KeyCode.F6, "Hotkey that writes one bounded no-write A2K-T34 observer packet.");
        pollConfigForCaptureRequest = Config.Bind("A2K-T34", "PollConfigForCaptureRequest", true, "Reload config on a bounded interval and run one no-write capture when CaptureRequestId increases.");
        captureWhenReady = Config.Bind("A2K-T34", "CaptureWhenReady", true, "For config-triggered captures, wait until the no-write observer sees loaded HoS hero/body/equip context; pose binding is recorded in the emitted packet and may still fail closed.");
        captureRequestId = Config.Bind(
            "A2K-T34",
            "CaptureRequestId",
            0,
            new ConfigDescription("Increment to a new positive integer to request one no-write A2K-T34 observer packet without using the hotkey.", new AcceptableValueRange<int>(0, int.MaxValue)));
        captureRequestPollSeconds = Config.Bind(
            "A2K-T34",
            "CaptureRequestPollSeconds",
            1.0f,
            new ConfigDescription("Seconds between config reload checks for CaptureRequestId.", new AcceptableValueRange<float>(0.5f, 60.0f)));
        captureReadinessTimeoutSeconds = Config.Bind(
            "A2K-T34",
            "CaptureReadinessTimeoutSeconds",
            45.0f,
            new ConfigDescription("Seconds between wait-status warnings while a config-triggered no-write capture waits for live HoS hero/body/equip context.", new AcceptableValueRange<float>(1.0f, 300.0f)));
        maxRendererRows = Config.Bind(
            "A2K-T34",
            "MaxRendererRows",
            160,
            new ConfigDescription("Maximum renderer rows included in one packet.", new AcceptableValueRange<int>(0, 1000)));
        outputFolder = Config.Bind(
            "A2K-T34",
            "OutputFolder",
            Path.Combine(Paths.ConfigPath, PluginGuid),
            "Folder where A2K-T34 observer packets are written. Empty values use this plugin's BepInEx config folder.");
        kandraRegistrationEnabled = Config.Bind(
            "Kandra-Runtime-Registration",
            "Enabled",
            false,
            "Enable the explicit custom Kandra runtime-registration host invoker. This creates one runtime KandraRenderer and calls the recovered KandraRendererManager.Register(KandraRenderer) path; candidate maps, conversion, item/equip/save mutation, and native game writes remain false.");
        kandraRegistrationRequestId = Config.Bind(
            "Kandra-Runtime-Registration",
            "InvocationRequestId",
            0,
            new ConfigDescription("Increment to a new positive integer to request one custom Kandra runtime-registration invocation from InvocationContextPath.", new AcceptableValueRange<int>(0, int.MaxValue)));
        kandraRegistrationInvocationContextPath = Config.Bind(
            "Kandra-Runtime-Registration",
            "InvocationContextPath",
            string.Empty,
            "Path to a JSON KandraRuntimeRegistrationInvocationContext packet produced from the approved framework invocation gate.");
        kandraRegistrationTemplateRendererPathContains = Config.Bind(
            "Kandra-Runtime-Registration",
            "TemplateRendererPathContains",
            string.Empty,
            "Required selector for exactly one active live KandraRenderer path/name to clone rig/material/bone rendererData from. Use an exact substring from a fresh A2K-T34 packet.");
        kandraRegistrationMaxPollFrames = Config.Bind(
            "Kandra-Runtime-Registration",
            "MaxPollFrames",
            300,
            new ConfigDescription("Maximum Unity Update polls to wait for IsRegistered and TryGetMeshMemory proof after Register is queued.", new AcceptableValueRange<int>(1, 3600)));
        kandraSameMeshAbProofEnabled = Config.Bind(
            "Kandra-Same-Mesh-Ab-Proof",
            "Enabled",
            false,
            "Enable the controlled same-mesh Kandra A/B runtime proof route. It snapshots one selected live mesh, writes four loose same-mesh packages, registers each proof mesh, and records IsRegistered/TryGetMeshMemory. Candidate maps, conversion, item/equip/save mutation, and native game writes remain false.");
        kandraSameMeshAbProofRequestId = Config.Bind(
            "Kandra-Same-Mesh-Ab-Proof",
            "RequestId",
            0,
            new ConfigDescription("Increment to a new positive integer to run one controlled same-mesh A/B proof from the selected live KandraRenderer.", new AcceptableValueRange<int>(0, int.MaxValue)));
        kandraSameMeshAbProofTemplateRendererPathContains = Config.Bind(
            "Kandra-Same-Mesh-Ab-Proof",
            "TemplateRendererPathContains",
            string.Empty,
            "Required selector for exactly one active live KandraRenderer path/name. The same renderer supplies original bytes and the rig/material template for all four same-mesh proof variants.");
        kandraSameMeshAbProofModDirectory = Config.Bind(
            "Kandra-Same-Mesh-Ab-Proof",
            "ModDirectory",
            "TaintedArmourSameMeshAbProof",
            "Single modDirectory segment used for the loose proof packages under ModDirectoryPath/<ModDirectory>/Kandra. Reusing this value overwrites the same proof files instead of accumulating stale packages.");
        kandraSameMeshAbProofMeshNamePrefix = Config.Bind(
            "Kandra-Same-Mesh-Ab-Proof",
            "MeshNamePrefix",
            "SameMeshAbProof",
            "File/name prefix for the four controlled variants: original_bytes, bar_geometric_plus16, duplicate_normal_plus16, and recovered_octahedral_roundtrip_plus16.");
        kandraSameMeshAbProofMaxPollFrames = Config.Bind(
            "Kandra-Same-Mesh-Ab-Proof",
            "MaxPollFrames",
            300,
            new ConfigDescription("Maximum Unity Update polls per same-mesh variant to wait for IsRegistered and TryGetMeshMemory proof after registration is queued.", new AcceptableValueRange<int>(1, 3600)));
        kandraSameMeshVisualEvidenceEnabled = Config.Bind(
            "Kandra-Same-Mesh-Visual-Evidence",
            "Enabled",
            false,
            "Enable the controlled same-mesh visual evidence route. It consumes a successful same-mesh A/B proof receipt, registers the four variants sequentially, captures screenshots, and reruns the framework visual/decode comparison. Candidate maps, conversion, item/equip/save mutation, and native game writes remain false.");
        kandraSameMeshVisualEvidenceRequestId = Config.Bind(
            "Kandra-Same-Mesh-Visual-Evidence",
            "RequestId",
            0,
            new ConfigDescription("Increment to a new positive integer to run one controlled same-mesh visual evidence capture from SourceReceiptPath.", new AcceptableValueRange<int>(0, int.MaxValue)));
        kandraSameMeshVisualEvidenceSourceReceiptPath = Config.Bind(
            "Kandra-Same-Mesh-Visual-Evidence",
            "SourceReceiptPath",
            string.Empty,
            "Path to the successful kandra-same-mesh-ab-proof receipt that owns the exact source receipt/test ID for these screenshots.");
        kandraSameMeshVisualEvidenceTemplateRendererPathContains = Config.Bind(
            "Kandra-Same-Mesh-Visual-Evidence",
            "TemplateRendererPathContains",
            string.Empty,
            "Required selector for exactly one active live KandraRenderer path/name to clone rig/material/bone rendererData from during visual evidence capture.");
        kandraSameMeshVisualEvidenceAcceptedVariantId = Config.Bind(
            "Kandra-Same-Mesh-Visual-Evidence",
            "AcceptedVariantId",
            string.Empty,
            "Optional reviewed decision. Set to exactly one non-original variant id to accept that +16 encoder after screenshots are reviewed. Leave empty when only capturing evidence.");
        kandraSameMeshVisualEvidenceRejectAllNonOriginalCandidates = Config.Bind(
            "Kandra-Same-Mesh-Visual-Evidence",
            "RejectAllNonOriginalCandidates",
            false,
            "Optional reviewed decision. Set true only after screenshots show every non-original +16 encoder candidate must be rejected. Do not set with AcceptedVariantId.");
        kandraSameMeshVisualEvidenceMaxPollFrames = Config.Bind(
            "Kandra-Same-Mesh-Visual-Evidence",
            "MaxPollFrames",
            300,
            new ConfigDescription("Maximum Unity Update polls per same-mesh visual variant to wait for IsRegistered and TryGetMeshMemory proof after registration is queued.", new AcceptableValueRange<int>(1, 3600)));
        kandraSameMeshVisualEvidenceScreenshotWaitFrames = Config.Bind(
            "Kandra-Same-Mesh-Visual-Evidence",
            "ScreenshotWaitFrames",
            90,
            new ConfigDescription("Maximum Unity Update polls per variant to wait for ScreenCapture.CaptureScreenshot to materialize the evidence PNG.", new AcceptableValueRange<int>(1, 600)));
        lastCaptureRequestId = captureRequestId.Value;
        lastKandraRegistrationRequestId = kandraRegistrationRequestId.Value;
        lastKandraSameMeshAbProofRequestId = kandraSameMeshAbProofRequestId.Value;
        lastKandraSameMeshVisualEvidenceRequestId = kandraSameMeshVisualEvidenceRequestId.Value;

        Logger.LogInfo($"{PluginName} {PluginVersion} loaded. Enabled={observerEnabled.Value}; CaptureHotkey={captureHotkey.Value}; PollConfigForCaptureRequest={pollConfigForCaptureRequest.Value}; CaptureWhenReady={captureWhenReady.Value}; CaptureRequestId={captureRequestId.Value}; KandraRuntimeRegistrationEnabled={kandraRegistrationEnabled.Value}; KandraRuntimeRegistrationRequestId={kandraRegistrationRequestId.Value}; KandraSameMeshAbProofEnabled={kandraSameMeshAbProofEnabled.Value}; KandraSameMeshAbProofRequestId={kandraSameMeshAbProofRequestId.Value}; KandraSameMeshVisualEvidenceEnabled={kandraSameMeshVisualEvidenceEnabled.Value}; KandraSameMeshVisualEvidenceRequestId={kandraSameMeshVisualEvidenceRequestId.Value}; candidate-map/conversion/item/equip/save/native writes false.");
    }

    private void Update()
    {
        if (!observerEnabled.Value)
        {
            return;
        }

        KeyCode key = captureHotkey.Value;
        if (key != KeyCode.None && Input.GetKeyDown(key))
        {
            Capture("hotkey:" + key);
        }

        PollConfigCaptureRequest();
        PollKandraRegistrationRequest();
        TryCompleteKandraRegistration();
        PollKandraSameMeshAbProofRequest();
        TryCompleteKandraSameMeshAbProof();
        PollKandraSameMeshVisualEvidenceRequest();
        TryCompleteKandraSameMeshVisualEvidence();
    }

    private void PollConfigCaptureRequest()
    {
        if (Time.unscaledTime < nextCaptureRequestPollTime)
        {
            return;
        }

        float pollSeconds = Math.Max(0.5f, Math.Min(60.0f, captureRequestPollSeconds.Value));
        nextCaptureRequestPollTime = Time.unscaledTime + pollSeconds;

        try
        {
            Config.Reload();
        }
        catch (Exception exception)
        {
            Logger.LogWarning($"{PluginName} config A2K-T34 capture request reload failed. Exception={exception.GetType().Name}; Message={exception.Message}; action=none");
            return;
        }

        if (!observerEnabled.Value || !pollConfigForCaptureRequest.Value)
        {
            ClearPendingCapture();
            lastCaptureRequestId = captureRequestId.Value;
            return;
        }

        int requestId = captureRequestId.Value;
        if (requestId > lastCaptureRequestId)
        {
            lastCaptureRequestId = requestId;
            if (captureWhenReady.Value)
            {
                QueueCaptureWhenReady(requestId);
            }
            else
            {
                Capture("config-request:" + requestId);
            }
        }
        else if (requestId < lastCaptureRequestId)
        {
            ClearPendingCapture();
            lastCaptureRequestId = requestId;
            Logger.LogInfo($"{PluginName} config A2K-T34 capture request baseline reset. CaptureRequestId={requestId}; action=read-only");
        }

        TryRunPendingCapture();
    }

    private void QueueCaptureWhenReady(int requestId)
    {
        pendingCaptureRequestId = requestId;
        pendingCaptureTrigger = "config-request:" + requestId;
        pendingCaptureLastStatus = string.Empty;
        float timeoutSeconds = Math.Max(1.0f, Math.Min(300.0f, captureReadinessTimeoutSeconds.Value));
        pendingCaptureDeadline = Time.unscaledTime + timeoutSeconds;
        Logger.LogInfo($"{PluginName} queued A2K-T34 no-write config capture. CaptureRequestId={requestId}; CaptureWhenReady=True; statusIntervalSeconds={timeoutSeconds}; action=wait-for-live-context");
    }

    private void TryRunPendingCapture()
    {
        if (pendingCaptureRequestId <= 0)
        {
            return;
        }

        A2KT34LiveObserver.ObserverReadiness readiness = A2KT34LiveObserver.ProbeReadiness(maxRendererRows.Value);
        if (readiness.Ready)
        {
            string trigger = pendingCaptureTrigger + ";readiness=live-context-observed;" + readiness.TriggerToken;
            int requestId = pendingCaptureRequestId;
            ClearPendingCapture();
            Logger.LogInfo($"{PluginName} A2K-T34 no-write config capture live context observed. CaptureRequestId={requestId}; {readiness.LogSummary}; action=capture");
            Capture(trigger);
            return;
        }

        if (Time.unscaledTime >= pendingCaptureDeadline)
        {
            int requestId = pendingCaptureRequestId;
            float timeoutSeconds = Math.Max(1.0f, Math.Min(300.0f, captureReadinessTimeoutSeconds.Value));
            pendingCaptureDeadline = Time.unscaledTime + timeoutSeconds;
            Logger.LogWarning($"{PluginName} A2K-T34 no-write config capture still waiting for live context. CaptureRequestId={requestId}; {readiness.LogSummary}; action=wait-no-live-context");
            return;
        }

        if (!string.Equals(pendingCaptureLastStatus, readiness.StatusToken, StringComparison.Ordinal))
        {
            pendingCaptureLastStatus = readiness.StatusToken;
            Logger.LogInfo($"{PluginName} A2K-T34 no-write config capture waiting. CaptureRequestId={pendingCaptureRequestId}; {readiness.LogSummary}; action=wait");
        }
    }

    private void ClearPendingCapture()
    {
        pendingCaptureRequestId = 0;
        pendingCaptureTrigger = string.Empty;
        pendingCaptureLastStatus = string.Empty;
        pendingCaptureDeadline = 0f;
    }

    private void Capture(string trigger)
    {
        try
        {
            string root = ResolveOutputFolder();
            string packetPath = A2KT34LiveObserver.Write(Logger, root, trigger, maxRendererRows.Value);
            Logger.LogInfo($"{PluginName} wrote A2K-T34 no-write observer packet: {packetPath}");
        }
        catch (Exception exception)
        {
            Logger.LogError($"{PluginName} A2K-T34 observer capture failed: {exception}");
        }
    }

    private string ResolveOutputFolder()
    {
        string configured = outputFolder.Value;
        return string.IsNullOrWhiteSpace(configured)
            ? Path.Combine(Paths.ConfigPath, PluginGuid)
            : configured;
    }

    private void PollKandraRegistrationRequest()
    {
        if (Time.unscaledTime < nextKandraRegistrationRequestPollTime)
        {
            return;
        }

        float pollSeconds = Math.Max(0.5f, Math.Min(60.0f, captureRequestPollSeconds.Value));
        nextKandraRegistrationRequestPollTime = Time.unscaledTime + pollSeconds;

        try
        {
            Config.Reload();
        }
        catch (Exception exception)
        {
            Logger.LogWarning($"{PluginName} config Kandra runtime-registration request reload failed. Exception={exception.GetType().Name}; Message={exception.Message}; action=none");
            return;
        }

        if (!kandraRegistrationEnabled.Value)
        {
            lastKandraRegistrationRequestId = kandraRegistrationRequestId.Value;
            return;
        }

        int requestId = kandraRegistrationRequestId.Value;
        if (requestId > lastKandraRegistrationRequestId)
        {
            lastKandraRegistrationRequestId = requestId;
            BeginKandraRegistration(requestId);
        }
        else if (requestId < lastKandraRegistrationRequestId)
        {
            lastKandraRegistrationRequestId = requestId;
            ClearPendingKandraRegistration(disposeActive: false);
            Logger.LogInfo($"{PluginName} Kandra runtime-registration request baseline reset. InvocationRequestId={requestId}; action=registration-none");
        }
    }

    private void BeginKandraRegistration(int requestId)
    {
        if (pendingKandraRegistration != null)
        {
            Logger.LogWarning($"{PluginName} Kandra runtime-registration request ignored because another invocation is pending. InvocationRequestId={requestId}; action=none");
            return;
        }

        string contextPath = kandraRegistrationInvocationContextPath.Value?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(contextPath) || !File.Exists(contextPath))
        {
            Logger.LogWarning($"{PluginName} Kandra runtime-registration request has no readable InvocationContextPath. InvocationRequestId={requestId}; InvocationContextPath={contextPath}; action=none");
            return;
        }

        try
        {
            string json = File.ReadAllText(contextPath);
            KandraRuntimeRegistrationInvocationContextPacket? packet = JsonConvert.DeserializeObject<KandraRuntimeRegistrationInvocationContextPacket>(json);
            if (packet == null)
            {
                Logger.LogWarning($"{PluginName} Kandra runtime-registration context JSON returned null. InvocationRequestId={requestId}; InvocationContextPath={contextPath}; action=none");
                return;
            }

            KandraRuntimeRegistrationInvocationContext context = packet.ToContext();
            activeKandraRegistration?.Dispose();
            activeKandraRegistration = null;

            kandraRegistrationInvoker ??= new FoaKandraRuntimeRegistrationInvoker(Logger.LogInfo, Logger.LogWarning);
            pendingKandraRegistration = kandraRegistrationInvoker.Begin(
                context,
                kandraRegistrationTemplateRendererPathContains.Value,
                kandraRegistrationMaxPollFrames.Value);
            pendingKandraRegistrationContext = context;
            pendingKandraRegistrationTrigger = "config-kandra-runtime-registration-request:" + requestId;
            Logger.LogInfo($"{PluginName} started Kandra runtime-registration invocation. InvocationRequestId={requestId}; ContextRequestId={context.RequestId}; action=register-poll");
        }
        catch (Exception exception)
        {
            Logger.LogError($"{PluginName} Kandra runtime-registration invocation failed to start. InvocationRequestId={requestId}; Exception={exception}");
            ClearPendingKandraRegistration(disposeActive: false);
        }
    }

    private void TryCompleteKandraRegistration()
    {
        if (pendingKandraRegistration == null || pendingKandraRegistrationContext == null)
        {
            return;
        }

        if (!pendingKandraRegistration.TryComplete(out KandraRuntimeRegistrationInvocationAttempt? attempt) || attempt == null)
        {
            return;
        }

        try
        {
            string receiptPath = WriteKandraRegistrationReceipt(pendingKandraRegistrationTrigger, pendingKandraRegistrationContext, attempt);
            Logger.LogInfo($"{PluginName} wrote Kandra runtime-registration receipt: {receiptPath}; RuntimeRegistrationSucceeded={attempt.RuntimeRegistrationSucceeded}; IsRegistered={attempt.KandraIsRegisteredStatus}; TryGetMeshMemory={attempt.KandraTryGetMeshMemoryStatus}; candidate-map/conversion/item/equip/save/native writes false");
        }
        catch (Exception exception)
        {
            Logger.LogError($"{PluginName} Kandra runtime-registration receipt write failed: {exception}");
        }

        if (attempt.RuntimeRegistrationSucceeded)
        {
            activeKandraRegistration = pendingKandraRegistration;
            pendingKandraRegistration = null;
        }
        else
        {
            pendingKandraRegistration.Dispose();
            pendingKandraRegistration = null;
        }

        pendingKandraRegistrationContext = null;
        pendingKandraRegistrationTrigger = string.Empty;
    }

    private string WriteKandraRegistrationReceipt(
        string trigger,
        KandraRuntimeRegistrationInvocationContext context,
        KandraRuntimeRegistrationInvocationAttempt attempt)
    {
        string root = ResolveOutputFolder();
        Directory.CreateDirectory(root);
        string timestamp = DateTime.UtcNow.ToString("yyyyMMddTHHmmssZ");
        string fileName = "kandra-runtime-registration-" + SanitizeFileName(context.RequestId) + "-" + timestamp + ".json";
        string path = Path.Combine(root, fileName);
        KandraRuntimeRegistrationHostReceiptPacket receipt = KandraRuntimeRegistrationHostReceiptPacket.From(trigger, context, attempt);
        File.WriteAllText(path, receipt.ToJson());
        return path;
    }

    private void ClearPendingKandraRegistration(bool disposeActive)
    {
        pendingKandraRegistration?.Dispose();
        pendingKandraRegistration = null;
        pendingKandraRegistrationContext = null;
        pendingKandraRegistrationTrigger = string.Empty;

        if (disposeActive)
        {
            activeKandraRegistration?.Dispose();
            activeKandraRegistration = null;
        }
    }

    private void PollKandraSameMeshAbProofRequest()
    {
        if (Time.unscaledTime < nextKandraSameMeshAbProofRequestPollTime)
        {
            return;
        }

        float pollSeconds = Math.Max(0.5f, Math.Min(60.0f, captureRequestPollSeconds.Value));
        nextKandraSameMeshAbProofRequestPollTime = Time.unscaledTime + pollSeconds;

        try
        {
            Config.Reload();
        }
        catch (Exception exception)
        {
            Logger.LogWarning($"{PluginName} config Kandra same-mesh A/B proof request reload failed. Exception={exception.GetType().Name}; Message={exception.Message}; action=none");
            return;
        }

        if (!kandraSameMeshAbProofEnabled.Value)
        {
            lastKandraSameMeshAbProofRequestId = kandraSameMeshAbProofRequestId.Value;
            return;
        }

        int requestId = kandraSameMeshAbProofRequestId.Value;
        if (requestId > lastKandraSameMeshAbProofRequestId)
        {
            lastKandraSameMeshAbProofRequestId = requestId;
            BeginKandraSameMeshAbProof(requestId);
        }
        else if (requestId < lastKandraSameMeshAbProofRequestId)
        {
            lastKandraSameMeshAbProofRequestId = requestId;
            ClearPendingKandraSameMeshAbProof();
            Logger.LogInfo($"{PluginName} Kandra same-mesh A/B proof request baseline reset. RequestId={requestId}; action=registration-none");
        }
    }

    private void BeginKandraSameMeshAbProof(int requestId)
    {
        if (pendingKandraSameMeshAbProof != null)
        {
            Logger.LogWarning($"{PluginName} Kandra same-mesh A/B proof request ignored because another proof is pending. RequestId={requestId}; action=none");
            return;
        }

        try
        {
            kandraSameMeshAbProofRunner ??= new FoaKandraSameMeshAbProofRunner(Logger.LogInfo, Logger.LogWarning);
            pendingKandraSameMeshAbProofTrigger = "config-kandra-same-mesh-ab-proof-request:" + requestId;
            pendingKandraSameMeshAbProof = kandraSameMeshAbProofRunner.Begin(
                pendingKandraSameMeshAbProofTrigger,
                kandraSameMeshAbProofTemplateRendererPathContains.Value,
                kandraSameMeshAbProofModDirectory.Value,
                kandraSameMeshAbProofMeshNamePrefix.Value,
                kandraSameMeshAbProofMaxPollFrames.Value);
            Logger.LogInfo($"{PluginName} started Kandra same-mesh A/B proof. RequestId={requestId}; TemplateRendererPathContains={kandraSameMeshAbProofTemplateRendererPathContains.Value}; action=register-ab-proof-poll");
        }
        catch (Exception exception)
        {
            Logger.LogError($"{PluginName} Kandra same-mesh A/B proof failed to start. RequestId={requestId}; Exception={exception}");
            ClearPendingKandraSameMeshAbProof();
        }
    }

    private void TryCompleteKandraSameMeshAbProof()
    {
        if (pendingKandraSameMeshAbProof == null)
        {
            return;
        }

        if (!pendingKandraSameMeshAbProof.TryComplete(out KandraSameMeshAbProofHostReceiptPacket? receipt) || receipt == null)
        {
            return;
        }

        try
        {
            string receiptPath = WriteKandraSameMeshAbProofReceipt(receipt);
            Logger.LogInfo($"{PluginName} wrote Kandra same-mesh A/B proof receipt: {receiptPath}; RuntimeProofSucceeded={receipt.RuntimeProofSucceeded}; VariantCount={receipt.VariantReceipts.Length}; candidate-map/conversion/item/equip/save/native writes false");
        }
        catch (Exception exception)
        {
            Logger.LogError($"{PluginName} Kandra same-mesh A/B proof receipt write failed: {exception}");
        }

        ClearPendingKandraSameMeshAbProof();
    }

    private string WriteKandraSameMeshAbProofReceipt(KandraSameMeshAbProofHostReceiptPacket receipt)
    {
        string root = ResolveOutputFolder();
        Directory.CreateDirectory(root);
        string timestamp = DateTime.UtcNow.ToString("yyyyMMddTHHmmssZ");
        string fileName = "kandra-same-mesh-ab-proof-" + SanitizeFileName(receipt.RequestId) + "-" + timestamp + ".json";
        string path = Path.Combine(root, fileName);
        File.WriteAllText(path, receipt.ToJson());
        return path;
    }

    private void ClearPendingKandraSameMeshAbProof()
    {
        pendingKandraSameMeshAbProof?.Dispose();
        pendingKandraSameMeshAbProof = null;
        pendingKandraSameMeshAbProofTrigger = string.Empty;
    }

    private void PollKandraSameMeshVisualEvidenceRequest()
    {
        if (Time.unscaledTime < nextKandraSameMeshVisualEvidenceRequestPollTime)
        {
            return;
        }

        float pollSeconds = Math.Max(0.5f, Math.Min(60.0f, captureRequestPollSeconds.Value));
        nextKandraSameMeshVisualEvidenceRequestPollTime = Time.unscaledTime + pollSeconds;

        try
        {
            Config.Reload();
        }
        catch (Exception exception)
        {
            Logger.LogWarning($"{PluginName} config Kandra same-mesh visual evidence request reload failed. Exception={exception.GetType().Name}; Message={exception.Message}; action=none");
            return;
        }

        if (!kandraSameMeshVisualEvidenceEnabled.Value)
        {
            lastKandraSameMeshVisualEvidenceRequestId = kandraSameMeshVisualEvidenceRequestId.Value;
            return;
        }

        int requestId = kandraSameMeshVisualEvidenceRequestId.Value;
        if (requestId > lastKandraSameMeshVisualEvidenceRequestId)
        {
            lastKandraSameMeshVisualEvidenceRequestId = requestId;
            BeginKandraSameMeshVisualEvidence(requestId);
        }
        else if (requestId < lastKandraSameMeshVisualEvidenceRequestId)
        {
            lastKandraSameMeshVisualEvidenceRequestId = requestId;
            ClearPendingKandraSameMeshVisualEvidence();
            Logger.LogInfo($"{PluginName} Kandra same-mesh visual evidence request baseline reset. RequestId={requestId}; action=registration-none");
        }
    }

    private void BeginKandraSameMeshVisualEvidence(int requestId)
    {
        if (pendingKandraSameMeshVisualEvidence != null)
        {
            Logger.LogWarning($"{PluginName} Kandra same-mesh visual evidence request ignored because another capture is pending. RequestId={requestId}; action=none");
            return;
        }

        try
        {
            kandraSameMeshVisualEvidenceRunner ??= new FoaKandraSameMeshVisualEvidenceRunner(Logger.LogInfo, Logger.LogWarning);
            pendingKandraSameMeshVisualEvidenceTrigger = "config-kandra-same-mesh-visual-evidence-request:" + requestId;
            pendingKandraSameMeshVisualEvidence = kandraSameMeshVisualEvidenceRunner.Begin(
                pendingKandraSameMeshVisualEvidenceTrigger,
                kandraSameMeshVisualEvidenceSourceReceiptPath.Value,
                kandraSameMeshVisualEvidenceTemplateRendererPathContains.Value,
                kandraSameMeshVisualEvidenceAcceptedVariantId.Value,
                kandraSameMeshVisualEvidenceRejectAllNonOriginalCandidates.Value,
                kandraSameMeshVisualEvidenceMaxPollFrames.Value,
                kandraSameMeshVisualEvidenceScreenshotWaitFrames.Value,
                ResolveOutputFolder());
            Logger.LogInfo($"{PluginName} started Kandra same-mesh visual evidence capture. RequestId={requestId}; SourceReceiptPath={kandraSameMeshVisualEvidenceSourceReceiptPath.Value}; TemplateRendererPathContains={kandraSameMeshVisualEvidenceTemplateRendererPathContains.Value}; action=register-capture-compare-poll");
        }
        catch (Exception exception)
        {
            Logger.LogError($"{PluginName} Kandra same-mesh visual evidence capture failed to start. RequestId={requestId}; Exception={exception}");
            ClearPendingKandraSameMeshVisualEvidence();
        }
    }

    private void TryCompleteKandraSameMeshVisualEvidence()
    {
        if (pendingKandraSameMeshVisualEvidence == null)
        {
            return;
        }

        if (!pendingKandraSameMeshVisualEvidence.TryComplete(out KandraSameMeshVisualEvidenceHostReceiptPacket? receipt) || receipt == null)
        {
            return;
        }

        try
        {
            string receiptPath = WriteKandraSameMeshVisualEvidenceReceipt(receipt);
            Logger.LogInfo($"{PluginName} wrote Kandra same-mesh visual evidence receipt: {receiptPath}; CapturedVariants={receipt.Variants.Length}; DecisionStatus={receipt.ComparisonResult?.DecisionStatus}; ProductionEncoderDecisionAccepted={receipt.ComparisonResult?.ProductionEncoderDecisionAccepted}; candidate-map/conversion/item/equip/save/native writes false");
        }
        catch (Exception exception)
        {
            Logger.LogError($"{PluginName} Kandra same-mesh visual evidence receipt write failed: {exception}");
        }

        ClearPendingKandraSameMeshVisualEvidence();
    }

    private string WriteKandraSameMeshVisualEvidenceReceipt(KandraSameMeshVisualEvidenceHostReceiptPacket receipt)
    {
        string root = ResolveOutputFolder();
        Directory.CreateDirectory(root);
        string timestamp = DateTime.UtcNow.ToString("yyyyMMddTHHmmssZ");
        string fileName = "kandra-same-mesh-visual-evidence-" + SanitizeFileName(receipt.SourceReceiptRequestId) + "-" + SanitizeFileName(receipt.RequestId) + "-" + timestamp + ".json";
        string path = Path.Combine(root, fileName);
        File.WriteAllText(path, receipt.ToJson());
        return path;
    }

    private void ClearPendingKandraSameMeshVisualEvidence()
    {
        pendingKandraSameMeshVisualEvidence?.Dispose();
        pendingKandraSameMeshVisualEvidence = null;
        pendingKandraSameMeshVisualEvidenceTrigger = string.Empty;
    }

    private void OnDestroy()
    {
        ClearPendingKandraRegistration(disposeActive: true);
        ClearPendingKandraSameMeshAbProof();
        ClearPendingKandraSameMeshVisualEvidence();
    }

    private static string SanitizeFileName(string value)
    {
        char[] invalid = Path.GetInvalidFileNameChars();
        char[] chars = value.ToCharArray();
        for (int i = 0; i < chars.Length; i++)
        {
            if (Array.IndexOf(invalid, chars[i]) >= 0 || char.IsControl(chars[i]))
            {
                chars[i] = '_';
            }
        }

        return new string(chars);
    }
}
