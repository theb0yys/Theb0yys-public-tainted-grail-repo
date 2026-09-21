using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using UnityEngine;

namespace Tainted.Armour.FoA;

internal sealed class FoaKandraSameMeshAbProofRunner
{
    public const string StageVersion = "tainted-armour.foa.kandra-same-mesh-ab-proof-runner.v1";

    private readonly Action<string> logInfo;
    private readonly Action<string> logWarning;

    public FoaKandraSameMeshAbProofRunner(Action<string> logInfo, Action<string> logWarning)
    {
        this.logInfo = logInfo ?? (_ => { });
        this.logWarning = logWarning ?? (_ => { });
    }

    public PendingProof Begin(
        string trigger,
        string templateRendererPathContains,
        string modDirectory,
        string meshNamePrefix,
        int maxPollFrames)
    {
        string trimmedSelector = templateRendererPathContains?.Trim() ?? string.Empty;
        var blockers = new List<string>();
        var warnings = new List<string>();
        string requestId = "foa.same-mesh-ab." + DateTime.UtcNow.ToString("yyyyMMddTHHmmssfffZ", CultureInfo.InvariantCulture);

        FoaKandraRuntimeRegistrationInvoker.RuntimeSurface? surface = FoaKandraRuntimeRegistrationInvoker.RuntimeSurface.TryResolve(blockers);
        if (surface == null || blockers.Count != 0)
        {
            return PendingProof.Completed(CreateBlockedReceipt(trigger, requestId, trimmedSelector, blockers, warnings));
        }

        Component? template = FindTemplateRenderer(surface, trimmedSelector, blockers, warnings);
        if (template == null || blockers.Count != 0)
        {
            return PendingProof.Completed(CreateBlockedReceipt(trigger, requestId, trimmedSelector, blockers, warnings));
        }

        object? templateRendererData = surface.RendererDataField.GetValue(template);
        object? sourceMesh = templateRendererData == null ? null : surface.RendererDataMeshField.GetValue(templateRendererData);
        if (sourceMesh == null)
        {
            blockers.Add("foa_kandra_same_mesh_ab_template_mesh_missing");
            return PendingProof.Completed(CreateBlockedReceipt(trigger, requestId, trimmedSelector, blockers, warnings));
        }

        string modDirectoryRoot = ResolveModDirectoryRoot(blockers);
        if (blockers.Count != 0)
        {
            return PendingProof.Completed(CreateBlockedReceipt(trigger, requestId, trimmedSelector, blockers, warnings));
        }

        A2KT34LiveObserver.KandraRegistrationContractDiagnosticPacket diagnostic = A2KT34LiveObserver.KandraRegistrationContractDiagnosticPacket.Capture();
        KandraRuntimeRegistrationContractFingerprint fingerprint = ToCoreFingerprint(diagnostic);
        KandraSameMeshAbProofSourceSnapshot? sourceSnapshot = CaptureSourceSnapshot(surface, sourceMesh, blockers, warnings);
        if (sourceSnapshot == null || blockers.Count != 0)
        {
            return PendingProof.Completed(CreateBlockedReceipt(trigger, requestId, trimmedSelector, blockers, warnings, diagnostic));
        }

        KandraSameMeshAbProofResult build = new KandraSameMeshAbProofStage().Build(
            new KandraSameMeshAbProofRequest(
                requestId,
                modDirectoryRoot,
                modDirectory,
                meshNamePrefix,
                "config-enabled-same-mesh-ab-proof",
                sourceSnapshot,
                fingerprint));

        if (!build.ProofPackageSetAccepted)
        {
            blockers.AddRange(build.Blockers.Select(blocker => "core:" + blocker));
            warnings.AddRange(build.Warnings.Select(warning => "core:" + warning));
            return PendingProof.Completed(CreateBlockedReceipt(trigger, requestId, trimmedSelector, blockers, warnings, diagnostic, build));
        }

        logInfo("Tainted Armour built Kandra same-mesh A/B package set. RequestId=" + requestId + "; SourceMesh=" + sourceSnapshot.SourceMeshName + "; VariantCount=" + build.Variants.Count.ToString(CultureInfo.InvariantCulture));
        return new PendingProof(
            trigger,
            trimmedSelector,
            diagnostic,
            build,
            new FoaKandraRuntimeRegistrationInvoker(logInfo, logWarning),
            Math.Max(1, Math.Min(3600, maxPollFrames)),
            logInfo,
            logWarning);
    }

    private static KandraSameMeshAbProofSourceSnapshot? CaptureSourceSnapshot(
        FoaKandraRuntimeRegistrationInvoker.RuntimeSurface surface,
        object sourceMesh,
        List<string> blockers,
        List<string> warnings)
    {
        int vertexCount = ReadIntMember(sourceMesh, "vertexCount", blockers);
        int indicesCount = ReadIntMember(sourceMesh, "indicesCount", blockers);
        int bindposesCount = ReadIntMember(sourceMesh, "bindposesCount", blockers);
        string sourceMeshName = ReadMeshName(sourceMesh);
        string[] blendshapeNames = ReadStringArrayMember(sourceMesh, "blendshapesNames");

        object? streamingManager = ResolveStreamingManager(surface, blockers);
        if (streamingManager == null)
        {
            return null;
        }

        byte[] meshData = LoadRuntimeArray<byte>(streamingManager, "LoadMeshData", surface.MeshType, sourceMesh, blockers);
        ushort[] indices = LoadRuntimeArray<ushort>(streamingManager, "LoadIndicesData", surface.MeshType, sourceMesh, blockers);
        if (blockers.Count != 0)
        {
            return null;
        }

        if (indices.Length != indicesCount)
        {
            blockers.Add(
                "foa_kandra_same_mesh_ab_indices_count_mismatch:metadata=" +
                indicesCount.ToString(CultureInfo.InvariantCulture) +
                ":loaded=" +
                indices.Length.ToString(CultureInfo.InvariantCulture));
            return null;
        }

        warnings.Add(
            "foa_kandra_same_mesh_ab_source_snapshot:" +
            sourceMeshName +
            ":vertices=" +
            vertexCount.ToString(CultureInfo.InvariantCulture) +
            ":indices=" +
            indices.Length.ToString(CultureInfo.InvariantCulture) +
            ":bindposes=" +
            bindposesCount.ToString(CultureInfo.InvariantCulture) +
            ":blendshapes=" +
            blendshapeNames.Length.ToString(CultureInfo.InvariantCulture));

        return new KandraSameMeshAbProofSourceSnapshot(
            sourceMeshName,
            vertexCount,
            bindposesCount,
            blendshapeNames,
            meshData,
            indices);
    }

    private static Component? FindTemplateRenderer(
        FoaKandraRuntimeRegistrationInvoker.RuntimeSurface surface,
        string selector,
        List<string> blockers,
        List<string> warnings)
    {
        if (string.IsNullOrWhiteSpace(selector))
        {
            blockers.Add("foa_kandra_same_mesh_ab_template_selector_required");
            return null;
        }

        UnityEngine.Object[] objects = Resources.FindObjectsOfTypeAll(surface.RendererType);
        var candidates = new List<Component>();
        foreach (UnityEngine.Object obj in objects)
        {
            if (obj is not Component component || !component)
            {
                continue;
            }

            GameObject gameObject = component.gameObject;
            if (!gameObject || !gameObject.scene.IsValid() || !gameObject.activeInHierarchy)
            {
                continue;
            }

            if (gameObject.name.StartsWith(FoaKandraRuntimeRegistrationInvoker.ProbeGameObjectPrefix, StringComparison.Ordinal))
            {
                continue;
            }

            if (!HasUsableRendererData(surface, component))
            {
                continue;
            }

            string path = BuildPath(component.transform);
            if (path.IndexOf(selector, StringComparison.OrdinalIgnoreCase) >= 0 ||
                gameObject.name.IndexOf(selector, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                candidates.Add(component);
            }
        }

        if (candidates.Count == 1)
        {
            warnings.Add("foa_kandra_same_mesh_ab_template_renderer:" + BuildPath(candidates[0].transform));
            return candidates[0];
        }

        if (candidates.Count == 0)
        {
            blockers.Add("foa_kandra_same_mesh_ab_template_renderer_not_found:" + selector);
        }
        else
        {
            blockers.Add("foa_kandra_same_mesh_ab_template_renderer_ambiguous:" + selector + ":matches=" + candidates.Count.ToString(CultureInfo.InvariantCulture));
        }

        return null;
    }

    private static bool HasUsableRendererData(FoaKandraRuntimeRegistrationInvoker.RuntimeSurface surface, Component renderer)
    {
        try
        {
            object rendererData = surface.RendererDataField.GetValue(renderer);
            return surface.RendererDataMeshField.GetValue(rendererData) != null &&
                   surface.RendererDataRigField.GetValue(rendererData) != null;
        }
        catch
        {
            return false;
        }
    }

    private static object? ResolveStreamingManager(
        FoaKandraRuntimeRegistrationInvoker.RuntimeSurface surface,
        List<string> blockers)
    {
        PropertyInfo? property = surface.ManagerInstance.GetType().GetProperty(
            "StreamingManager",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        object? streamingManager = property?.GetValue(surface.ManagerInstance);
        if (streamingManager == null)
        {
            blockers.Add("foa_kandra_same_mesh_ab_streaming_manager_missing");
        }

        return streamingManager;
    }

    private static T[] LoadRuntimeArray<T>(
        object streamingManager,
        string methodName,
        Type meshType,
        object sourceMesh,
        List<string> blockers)
        where T : struct
    {
        MethodInfo? method = streamingManager.GetType().GetMethod(
            methodName,
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
            null,
            new[] { meshType },
            null);
        if (method == null)
        {
            blockers.Add("foa_kandra_same_mesh_ab_streaming_method_missing:" + methodName);
            return Array.Empty<T>();
        }

        try
        {
            object? span = method.Invoke(streamingManager, new[] { sourceMesh });
            if (span == null)
            {
                blockers.Add("foa_kandra_same_mesh_ab_streaming_span_missing:" + methodName);
                return Array.Empty<T>();
            }

            MethodInfo? asNativeArray = span.GetType().GetMethod(
                "AsNativeArray",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                Type.EmptyTypes,
                null);
            object? nativeArray = asNativeArray?.Invoke(span, null);
            MethodInfo? toArray = nativeArray?.GetType().GetMethod(
                "ToArray",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                Type.EmptyTypes,
                null);
            object? managedArray = toArray?.Invoke(nativeArray, null);
            if (managedArray is T[] typed)
            {
                return typed;
            }

            blockers.Add("foa_kandra_same_mesh_ab_streaming_span_copy_unavailable:" + methodName + ":" + span.GetType().FullName);
            return Array.Empty<T>();
        }
        catch (Exception exception)
        {
            blockers.Add("foa_kandra_same_mesh_ab_streaming_load_failed:" + methodName + ":" + exception.GetType().Name);
            return Array.Empty<T>();
        }
    }

    private static string ResolveModDirectoryRoot(List<string> blockers)
    {
        Type? modManager = FindRuntimeType("Awaken.Utility.Assets.Modding.ModManager");
        if (modManager == null)
        {
            blockers.Add("foa_kandra_same_mesh_ab_mod_manager_type_missing");
            return string.Empty;
        }

        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
        object? value = modManager.GetProperty("ModDirectoryPath", flags)?.GetValue(null, null)
            ?? modManager.GetField("ModDirectoryPath", flags)?.GetValue(null);
        if (value is string path && !string.IsNullOrWhiteSpace(path))
        {
            return Path.GetFullPath(path);
        }

        blockers.Add("foa_kandra_same_mesh_ab_mod_directory_path_missing");
        return string.Empty;
    }

    private static int ReadIntMember(object instance, string memberName, List<string> blockers)
    {
        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        object? value = instance.GetType().GetField(memberName, flags)?.GetValue(instance)
            ?? instance.GetType().GetProperty(memberName, flags)?.GetValue(instance, null);
        if (value == null)
        {
            blockers.Add("foa_kandra_same_mesh_ab_mesh_member_missing:" + memberName);
            return 0;
        }

        try
        {
            return Convert.ToInt32(value, CultureInfo.InvariantCulture);
        }
        catch
        {
            blockers.Add("foa_kandra_same_mesh_ab_mesh_member_not_int:" + memberName);
            return 0;
        }
    }

    private static string ReadMeshName(object instance)
    {
        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        object? value = instance.GetType().GetProperty("Name", flags)?.GetValue(instance, null)
            ?? instance.GetType().GetField("_name", flags)?.GetValue(instance)
            ?? instance.GetType().GetField("name", flags)?.GetValue(instance);
        if (value is string text && !string.IsNullOrWhiteSpace(text))
        {
            return text.Trim();
        }

        return instance is UnityEngine.Object unityObject && !string.IsNullOrWhiteSpace(unityObject.name)
            ? unityObject.name
            : "unnamed_kandra_mesh";
    }

    private static string[] ReadStringArrayMember(object instance, string memberName)
    {
        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        object? value = instance.GetType().GetField(memberName, flags)?.GetValue(instance)
            ?? instance.GetType().GetProperty(memberName, flags)?.GetValue(instance, null);
        return value is string[] array ? array : Array.Empty<string>();
    }

    private static KandraRuntimeRegistrationContractFingerprint ToCoreFingerprint(A2KT34LiveObserver.KandraRegistrationContractDiagnosticPacket diagnostic) =>
        new KandraRuntimeRegistrationContractFingerprint(
            diagnostic.status,
            diagnostic.assemblyName,
            diagnostic.assemblyLocation,
            diagnostic.assemblySha256,
            diagnostic.managerType,
            diagnostic.managerInstancePresent,
            diagnostic.registrationOwner,
            diagnostic.registrationMethodSignature,
            diagnostic.registrationMethodFingerprint,
            diagnostic.registrationMethodPresent,
            diagnostic.registrationMethodPublic,
            diagnostic.rendererType,
            diagnostic.rendererTypePresent,
            diagnostic.rendererOnEnableMethodPresent,
            diagnostic.rendererOnDisableMethodPresent,
            diagnostic.rendererDataFieldPresent,
            diagnostic.rendererRenderingIdPropertyPresent,
            diagnostic.requiredRendererDataMembersPresent,
            diagnostic.requiredKandraMeshMembersPresent,
            diagnostic.managerDependencyMethodsPresent,
            diagnostic.finalizeRegistrationMethodPresent,
            diagnostic.onEarlyUpdateBeginMethodPresent,
            diagnostic.onBeginRenderingMethodPresent,
            diagnostic.queueStateFieldsPresent,
            diagnostic.diagnosticBoundary,
            diagnostic.registrationMethodInvoked,
            diagnostic.canRegisterMethodsInvoked,
            diagnostic.createdOrActivatedKandraObject,
            diagnostic.runtimeRegistrationInvocationAllowed,
            diagnostic.runtimeRegistrationExecuted,
            diagnostic.candidateMapApplicationExecuted,
            diagnostic.conversionExecuted,
            diagnostic.itemEquipMutationExecuted,
            diagnostic.saveMutationExecuted,
            diagnostic.nativeGameWriteExecuted,
            diagnostic.downstreamWritesExecuted);

    private static Type? FindRuntimeType(string typeName) =>
        AppDomain.CurrentDomain.GetAssemblies()
            .Select(assembly => assembly.GetType(typeName, throwOnError: false))
            .FirstOrDefault(type => type != null);

    private static KandraSameMeshAbProofHostReceiptPacket CreateBlockedReceipt(
        string trigger,
        string requestId,
        string templateRendererPathContains,
        IEnumerable<string> blockers,
        IEnumerable<string> warnings,
        A2KT34LiveObserver.KandraRegistrationContractDiagnosticPacket? diagnostic = null,
        KandraSameMeshAbProofResult? buildResult = null) =>
        new KandraSameMeshAbProofHostReceiptPacket
        {
            WrittenAtUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture),
            Trigger = trigger ?? string.Empty,
            RequestId = requestId,
            TemplateRendererPathContains = templateRendererPathContains ?? string.Empty,
            RegistrationContractDiagnostic = diagnostic,
            BuildResult = buildResult,
            VariantReceipts = Array.Empty<KandraSameMeshAbProofVariantReceiptPacket>(),
            CandidateMapApplicationExecuted = false,
            ConversionExecuted = false,
            ItemEquipMutationExecuted = false,
            SaveMutationExecuted = false,
            NativeGameWriteExecuted = false,
            NonRegistrationDownstreamWritesExecuted = false,
            Blockers = blockers.Where(value => !string.IsNullOrWhiteSpace(value)).ToArray(),
            Warnings = warnings.Where(value => !string.IsNullOrWhiteSpace(value)).ToArray(),
        };

    private static string BuildPath(Transform transform)
    {
        var names = new Stack<string>();
        Transform? current = transform;
        while (current != null)
        {
            names.Push(current.name);
            current = current.parent;
        }

        return "/" + string.Join("/", names.ToArray());
    }

    internal sealed class PendingProof : IDisposable
    {
        private readonly string trigger;
        private readonly string templateRendererPathContains;
        private readonly A2KT34LiveObserver.KandraRegistrationContractDiagnosticPacket? registrationContractDiagnostic;
        private readonly KandraSameMeshAbProofResult? buildResult;
        private readonly FoaKandraRuntimeRegistrationInvoker? invoker;
        private readonly int maxPollFrames;
        private readonly Action<string> logInfo;
        private readonly Action<string> logWarning;
        private readonly KandraSameMeshAbProofHostReceiptPacket? immediateReceipt;
        private readonly List<KandraSameMeshAbProofVariantReceiptPacket> variantReceipts = new List<KandraSameMeshAbProofVariantReceiptPacket>();
        private FoaKandraRuntimeRegistrationInvoker.PendingInvocation? pendingVariant;
        private KandraSameMeshAbProofVariantResult? pendingVariantResult;
        private int nextVariantIndex;
        private bool disposed;

        internal PendingProof(
            string trigger,
            string templateRendererPathContains,
            A2KT34LiveObserver.KandraRegistrationContractDiagnosticPacket registrationContractDiagnostic,
            KandraSameMeshAbProofResult buildResult,
            FoaKandraRuntimeRegistrationInvoker invoker,
            int maxPollFrames,
            Action<string> logInfo,
            Action<string> logWarning)
        {
            this.trigger = trigger ?? string.Empty;
            this.templateRendererPathContains = templateRendererPathContains ?? string.Empty;
            this.registrationContractDiagnostic = registrationContractDiagnostic;
            this.buildResult = buildResult;
            this.invoker = invoker;
            this.maxPollFrames = maxPollFrames;
            this.logInfo = logInfo ?? (_ => { });
            this.logWarning = logWarning ?? (_ => { });
        }

        private PendingProof(KandraSameMeshAbProofHostReceiptPacket immediateReceipt)
        {
            this.immediateReceipt = immediateReceipt;
            trigger = immediateReceipt.Trigger;
            templateRendererPathContains = immediateReceipt.TemplateRendererPathContains;
            maxPollFrames = 1;
            logInfo = _ => { };
            logWarning = _ => { };
        }

        public static PendingProof Completed(KandraSameMeshAbProofHostReceiptPacket receipt) =>
            new PendingProof(receipt);

        public bool TryComplete(out KandraSameMeshAbProofHostReceiptPacket? receipt)
        {
            receipt = null;
            if (immediateReceipt != null)
            {
                receipt = immediateReceipt;
                return true;
            }

            if (disposed || buildResult == null || invoker == null)
            {
                return false;
            }

            if (pendingVariant == null)
            {
                StartNextVariant();
            }

            if (pendingVariant == null || pendingVariantResult == null)
            {
                receipt = CreateFinalReceipt();
                return true;
            }

            if (!pendingVariant.TryComplete(out KandraRuntimeRegistrationInvocationAttempt? attempt) || attempt == null)
            {
                return false;
            }

            variantReceipts.Add(KandraSameMeshAbProofVariantReceiptPacket.From(pendingVariantResult, attempt));
            logInfo("Tainted Armour same-mesh A/B variant completed. Variant=" + pendingVariantResult.VariantId + "; RuntimeRegistrationSucceeded=" + attempt.RuntimeRegistrationSucceeded.ToString(CultureInfo.InvariantCulture) + "; IsRegistered=" + attempt.KandraIsRegisteredStatus + "; TryGetMeshMemory=" + attempt.KandraTryGetMeshMemoryStatus);
            pendingVariant.Dispose();
            pendingVariant = null;
            pendingVariantResult = null;

            if (nextVariantIndex < buildResult.Variants.Count)
            {
                StartNextVariant();
                return false;
            }

            receipt = CreateFinalReceipt();
            return true;
        }

        private void StartNextVariant()
        {
            if (buildResult == null || invoker == null || nextVariantIndex >= buildResult.Variants.Count)
            {
                return;
            }

            KandraSameMeshAbProofVariantResult variant = buildResult.Variants[nextVariantIndex++];
            try
            {
                pendingVariant = invoker.Begin(variant.InvocationContext, templateRendererPathContains, maxPollFrames);
                pendingVariantResult = variant;
                logInfo("Tainted Armour started same-mesh A/B runtime registration variant. Variant=" + variant.VariantId + "; Mesh=" + variant.MeshName);
            }
            catch (Exception exception)
            {
                logWarning("Tainted Armour same-mesh A/B variant failed to start. Variant=" + variant.VariantId + "; Exception=" + exception.GetType().Name + "; Message=" + exception.Message);
                variantReceipts.Add(KandraSameMeshAbProofVariantReceiptPacket.From(
                    variant,
                    new KandraRuntimeRegistrationInvocationAttempt(
                        registrationMethodInvoked: false,
                        canRegisterMethodsInvoked: false,
                        createdOrActivatedKandraObject: false,
                        runtimeRegistrationExecuted: false,
                        runtimeRegistrationSucceeded: false,
                        kandraIsRegisteredStatus: "blocked:start_exception",
                        kandraTryGetMeshMemoryStatus: "blocked:start_exception",
                        registeredRendererIdentity: string.Empty,
                        registeredMeshMemoryIdentity: string.Empty,
                        blockers: new[] { "foa_kandra_same_mesh_ab_variant_start_exception:" + exception.GetType().Name },
                        warnings: new[] { exception.Message })));
            }
        }

        private KandraSameMeshAbProofHostReceiptPacket CreateFinalReceipt()
        {
            KandraSameMeshAbProofVariantReceiptPacket[] variants = variantReceipts.ToArray();
            bool allVariantsAttempted = buildResult != null && variants.Length == buildResult.Variants.Count;
            bool allVariantsSucceeded = allVariantsAttempted && variants.All(variant => variant.Attempt != null && variant.Attempt.RuntimeRegistrationSucceeded);
            var blockers = new List<string>();
            if (!allVariantsAttempted)
            {
                blockers.Add("foa_kandra_same_mesh_ab_not_all_variants_attempted");
            }

            if (!allVariantsSucceeded)
            {
                blockers.Add("foa_kandra_same_mesh_ab_not_all_variants_succeeded");
            }

            return new KandraSameMeshAbProofHostReceiptPacket
            {
                WrittenAtUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture),
                Trigger = trigger,
                RequestId = buildResult?.RequestId ?? string.Empty,
                TemplateRendererPathContains = templateRendererPathContains,
                RegistrationContractDiagnostic = registrationContractDiagnostic,
                BuildResult = buildResult,
                VariantReceipts = variants,
                CandidateMapApplicationExecuted = false,
                ConversionExecuted = false,
                ItemEquipMutationExecuted = false,
                SaveMutationExecuted = false,
                NativeGameWriteExecuted = false,
                NonRegistrationDownstreamWritesExecuted = false,
                Blockers = blockers.ToArray(),
                Warnings = Array.Empty<string>(),
            };
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            pendingVariant?.Dispose();
            pendingVariant = null;
            pendingVariantResult = null;
        }
    }
}

internal sealed class KandraSameMeshAbProofHostReceiptPacket
{
    public string StageVersion { get; set; } = FoaKandraSameMeshAbProofRunner.StageVersion;

    public string WrittenAtUtc { get; set; } = string.Empty;

    public string Trigger { get; set; } = string.Empty;

    public string RequestId { get; set; } = string.Empty;

    public string TemplateRendererPathContains { get; set; } = string.Empty;

    public A2KT34LiveObserver.KandraRegistrationContractDiagnosticPacket? RegistrationContractDiagnostic { get; set; }

    public KandraSameMeshAbProofResult? BuildResult { get; set; }

    public KandraSameMeshAbProofVariantReceiptPacket[] VariantReceipts { get; set; } = Array.Empty<KandraSameMeshAbProofVariantReceiptPacket>();

    public bool CandidateMapApplicationExecuted { get; set; }

    public bool ConversionExecuted { get; set; }

    public bool ItemEquipMutationExecuted { get; set; }

    public bool SaveMutationExecuted { get; set; }

    public bool NativeGameWriteExecuted { get; set; }

    public bool NonRegistrationDownstreamWritesExecuted { get; set; }

    public string[] Blockers { get; set; } = Array.Empty<string>();

    public string[] Warnings { get; set; } = Array.Empty<string>();

    public bool RuntimeProofSucceeded =>
        Blockers.Length == 0 &&
        VariantReceipts.Length == 4 &&
        VariantReceipts.All(variant => variant.Attempt != null && variant.Attempt.RuntimeRegistrationSucceeded);

    public bool NonRegistrationDownstreamBoundaryFalse =>
        !CandidateMapApplicationExecuted &&
        !ConversionExecuted &&
        !ItemEquipMutationExecuted &&
        !SaveMutationExecuted &&
        !NativeGameWriteExecuted &&
        !NonRegistrationDownstreamWritesExecuted;

    public string ToJson() =>
        JsonConvert.SerializeObject(this, Formatting.Indented);
}

internal sealed class KandraSameMeshAbProofVariantReceiptPacket
{
    public string VariantId { get; set; } = string.Empty;

    public string VariantLabel { get; set; } = string.Empty;

    public string MeshName { get; set; } = string.Empty;

    public string MeshDataSha256 { get; set; } = string.Empty;

    public string IndicesDataSha256 { get; set; } = string.Empty;

    public bool SourceMeshPayloadPreservedExactly { get; set; }

    public bool SourceIndicesPayloadPreservedExactly { get; set; }

    public bool OnlyCompressedVertexPlus16Changed { get; set; }

    public int ChangedByteCount { get; set; }

    public int ChangedCompressedVertexPlus16ByteCount { get; set; }

    public int ChangedNonPlus16ByteCount { get; set; }

    public int ChangedPlus16FieldCount { get; set; }

    public KandraRuntimeRegistrationInvocationContextPacket Context { get; set; } = new KandraRuntimeRegistrationInvocationContextPacket();

    public KandraRuntimeRegistrationInvocationAttempt? Attempt { get; set; }

    public static KandraSameMeshAbProofVariantReceiptPacket From(
        KandraSameMeshAbProofVariantResult variant,
        KandraRuntimeRegistrationInvocationAttempt attempt) =>
        new KandraSameMeshAbProofVariantReceiptPacket
        {
            VariantId = variant.VariantId,
            VariantLabel = variant.VariantLabel,
            MeshName = variant.MeshName,
            MeshDataSha256 = variant.WriterResult.MeshDataSha256,
            IndicesDataSha256 = variant.WriterResult.IndicesDataSha256,
            SourceMeshPayloadPreservedExactly = variant.SourceMeshPayloadPreservedExactly,
            SourceIndicesPayloadPreservedExactly = variant.SourceIndicesPayloadPreservedExactly,
            OnlyCompressedVertexPlus16Changed = variant.OnlyCompressedVertexPlus16Changed,
            ChangedByteCount = variant.ChangedByteCount,
            ChangedCompressedVertexPlus16ByteCount = variant.ChangedCompressedVertexPlus16ByteCount,
            ChangedNonPlus16ByteCount = variant.ChangedNonPlus16ByteCount,
            ChangedPlus16FieldCount = variant.ChangedPlus16FieldCount,
            Context = KandraRuntimeRegistrationInvocationContextPacket.From(variant.InvocationContext),
            Attempt = attempt,
        };
}
