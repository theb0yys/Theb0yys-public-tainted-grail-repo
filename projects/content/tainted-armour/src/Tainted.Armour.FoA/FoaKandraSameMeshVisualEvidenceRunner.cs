using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Newtonsoft.Json;
using UnityEngine;

namespace Tainted.Armour.FoA;

internal sealed class FoaKandraSameMeshVisualEvidenceRunner
{
    public const string StageVersion = "tainted-armour.foa.kandra-same-mesh-visual-evidence-runner.v1";

    private static readonly string[] RequiredVariants =
    {
        KandraSameMeshAbProofStage.OriginalBytesVariantId,
        KandraSameMeshAbProofStage.BarGeometricPlus16VariantId,
        KandraSameMeshAbProofStage.DuplicateNormalPlus16VariantId,
        KandraSameMeshAbProofStage.RecoveredOctahedralRoundtripPlus16VariantId,
    };

    private readonly Action<string> logInfo;
    private readonly Action<string> logWarning;

    public FoaKandraSameMeshVisualEvidenceRunner(Action<string> logInfo, Action<string> logWarning)
    {
        this.logInfo = logInfo ?? (_ => { });
        this.logWarning = logWarning ?? (_ => { });
    }

    public PendingVisualEvidence Begin(
        string trigger,
        string sourceReceiptPath,
        string templateRendererPathContains,
        string acceptedVariantId,
        bool rejectAllNonOriginalCandidates,
        int maxPollFrames,
        int screenshotWaitFrames,
        string outputFolder)
    {
        string requestId = "foa.same-mesh-visual." + DateTime.UtcNow.ToString("yyyyMMddTHHmmssfffZ", CultureInfo.InvariantCulture);
        string normalizedSourcePath = sourceReceiptPath?.Trim() ?? string.Empty;
        string normalizedTemplateSelector = templateRendererPathContains?.Trim() ?? string.Empty;
        string normalizedAcceptedVariantId = acceptedVariantId?.Trim() ?? string.Empty;
        string normalizedOutputFolder = string.IsNullOrWhiteSpace(outputFolder)
            ? Directory.GetCurrentDirectory()
            : Path.GetFullPath(outputFolder);
        var blockers = new List<string>();
        var warnings = new List<string>();
        var reviewBlockers = new List<string>();

        if (string.IsNullOrWhiteSpace(normalizedSourcePath))
        {
            blockers.Add("foa_kandra_same_mesh_visual_source_receipt_path_required");
        }
        else if (!File.Exists(normalizedSourcePath))
        {
            blockers.Add("foa_kandra_same_mesh_visual_source_receipt_missing:" + normalizedSourcePath);
        }

        if (string.IsNullOrWhiteSpace(normalizedTemplateSelector))
        {
            blockers.Add("foa_kandra_same_mesh_visual_template_selector_required");
        }

        KandraSameMeshVisualEvidenceSourceReceiptPacket? sourceReceipt = null;
        if (blockers.Count == 0)
        {
            try
            {
                string json = File.ReadAllText(normalizedSourcePath);
                sourceReceipt = JsonConvert.DeserializeObject<KandraSameMeshVisualEvidenceSourceReceiptPacket>(json);
                if (sourceReceipt == null)
                {
                    blockers.Add("foa_kandra_same_mesh_visual_source_receipt_json_null");
                }
            }
            catch (Exception exception)
            {
                blockers.Add("foa_kandra_same_mesh_visual_source_receipt_read_failed:" + exception.GetType().Name);
                warnings.Add(exception.Message);
            }
        }

        if (sourceReceipt != null)
        {
            ValidateSourceReceipt(sourceReceipt, blockers);
        }

        bool acceptedVariantConfigured = !string.IsNullOrWhiteSpace(normalizedAcceptedVariantId);
        bool acceptedVariantKnown = RequiredVariants.Any(variant => string.Equals(variant, normalizedAcceptedVariantId, StringComparison.Ordinal));
        bool acceptedVariantIsOriginal = string.Equals(normalizedAcceptedVariantId, KandraSameMeshAbProofStage.OriginalBytesVariantId, StringComparison.Ordinal);
        bool acceptedNonOriginalVariantConfigured = acceptedVariantConfigured && acceptedVariantKnown && !acceptedVariantIsOriginal;
        if (acceptedVariantConfigured && !acceptedVariantKnown)
        {
            reviewBlockers.Add("foa_kandra_same_mesh_visual_accepted_variant_unknown:" + normalizedAcceptedVariantId);
        }

        if (acceptedVariantIsOriginal)
        {
            reviewBlockers.Add("foa_kandra_same_mesh_visual_original_bytes_cannot_be_selected_as_encoder");
        }

        if (acceptedVariantConfigured && rejectAllNonOriginalCandidates)
        {
            reviewBlockers.Add("foa_kandra_same_mesh_visual_review_config_conflict:accepted_variant_and_reject_all");
        }

        bool visualReviewDecisionConfigured = reviewBlockers.Count == 0 &&
                                              (acceptedNonOriginalVariantConfigured || rejectAllNonOriginalCandidates);
        if (!visualReviewDecisionConfigured && reviewBlockers.Count == 0)
        {
            warnings.Add("foa_kandra_same_mesh_visual_review_decision_not_configured");
        }

        if (blockers.Count != 0 || sourceReceipt == null)
        {
            return PendingVisualEvidence.Completed(CreateBlockedReceipt(
                trigger,
                requestId,
                normalizedSourcePath,
                sourceReceipt?.RequestId ?? string.Empty,
                sourceReceipt?.BuildResult?.SourceMeshName ?? string.Empty,
                sourceReceipt?.BuildResult?.VertexCount ?? 0,
                normalizedTemplateSelector,
                normalizedAcceptedVariantId,
                rejectAllNonOriginalCandidates,
                blockers,
                warnings.Concat(reviewBlockers)));
        }

        logInfo(
            "Tainted Armour started same-mesh visual evidence capture. RequestId=" +
            requestId +
            "; SourceReceiptRequestId=" +
            sourceReceipt.RequestId +
            "; VariantCount=" +
            sourceReceipt.VariantReceipts.Length.ToString(CultureInfo.InvariantCulture) +
            "; VisualReviewDecisionConfigured=" +
            visualReviewDecisionConfigured.ToString(CultureInfo.InvariantCulture));
        return new PendingVisualEvidence(
            trigger,
            requestId,
            normalizedSourcePath,
            sourceReceipt,
            normalizedTemplateSelector,
            normalizedAcceptedVariantId,
            rejectAllNonOriginalCandidates,
            visualReviewDecisionConfigured,
            reviewBlockers,
            warnings,
            new FoaKandraRuntimeRegistrationInvoker(logInfo, logWarning),
            Math.Max(1, Math.Min(3600, maxPollFrames)),
            Math.Max(1, Math.Min(600, screenshotWaitFrames)),
            normalizedOutputFolder,
            logInfo,
            logWarning);
    }

    private static void ValidateSourceReceipt(
        KandraSameMeshVisualEvidenceSourceReceiptPacket sourceReceipt,
        List<string> blockers)
    {
        if (string.IsNullOrWhiteSpace(sourceReceipt.RequestId))
        {
            blockers.Add("foa_kandra_same_mesh_visual_source_receipt_request_id_missing");
        }

        if (sourceReceipt.BuildResult == null)
        {
            blockers.Add("foa_kandra_same_mesh_visual_source_build_result_missing");
        }
        else
        {
            if (string.IsNullOrWhiteSpace(sourceReceipt.BuildResult.SourceMeshName))
            {
                blockers.Add("foa_kandra_same_mesh_visual_source_mesh_name_missing");
            }

            if (sourceReceipt.BuildResult.VertexCount <= 0)
            {
                blockers.Add("foa_kandra_same_mesh_visual_source_vertex_count_missing");
            }
        }

        if (!sourceReceipt.RuntimeProofSucceeded)
        {
            blockers.Add("foa_kandra_same_mesh_visual_source_runtime_proof_not_succeeded");
        }

        if (!sourceReceipt.NonRegistrationDownstreamBoundaryFalse)
        {
            blockers.Add("foa_kandra_same_mesh_visual_source_non_registration_boundary_not_false");
        }

        Dictionary<string, KandraSameMeshVisualEvidenceSourceVariantReceiptPacket> byVariant = sourceReceipt.VariantReceipts
            .GroupBy(variant => variant.VariantId, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
        foreach (string requiredVariant in RequiredVariants)
        {
            if (!byVariant.TryGetValue(requiredVariant, out KandraSameMeshVisualEvidenceSourceVariantReceiptPacket? variant))
            {
                blockers.Add("foa_kandra_same_mesh_visual_source_variant_missing:" + requiredVariant);
                continue;
            }

            if (variant.Context == null ||
                string.IsNullOrWhiteSpace(variant.Context.RequestId) ||
                string.IsNullOrWhiteSpace(variant.Context.ModDirectory) ||
                string.IsNullOrWhiteSpace(variant.Context.Name))
            {
                blockers.Add("foa_kandra_same_mesh_visual_source_variant_context_missing:" + requiredVariant);
            }

            if (variant.Attempt == null || !variant.Attempt.RuntimeRegistrationSucceeded)
            {
                blockers.Add("foa_kandra_same_mesh_visual_source_variant_registration_not_succeeded:" + requiredVariant);
            }
        }
    }

    private static KandraSameMeshVisualEvidenceHostReceiptPacket CreateBlockedReceipt(
        string trigger,
        string requestId,
        string sourceReceiptPath,
        string sourceReceiptRequestId,
        string sourceMeshName,
        int vertexCount,
        string templateRendererPathContains,
        string acceptedVariantId,
        bool rejectAllNonOriginalCandidates,
        IEnumerable<string> blockers,
        IEnumerable<string> warnings)
    {
        var blockerArray = blockers.Where(value => !string.IsNullOrWhiteSpace(value)).ToArray();
        KandraSameMeshVisualDecodeComparisonResult comparison = new KandraSameMeshVisualDecodeComparisonStage().Evaluate(
            new KandraSameMeshVisualDecodeComparisonRequest(
                requestId,
                string.IsNullOrWhiteSpace(sourceReceiptPath) ? "missing://source-receipt" : sourceReceiptPath,
                string.IsNullOrWhiteSpace(sourceReceiptRequestId) ? "missing-source-receipt-request-id" : sourceReceiptRequestId,
                string.IsNullOrWhiteSpace(sourceMeshName) ? "missing-source-mesh" : sourceMeshName,
                vertexCount,
                runtimeProofSucceeded: false,
                receiptVariantCount: 0,
                nonRegistrationDownstreamBoundaryFalse: false,
                roundtripChangedPlus16FieldTolerance: 0,
                candidates: Array.Empty<KandraSameMeshVisualDecodeComparisonCandidate>()));
        return new KandraSameMeshVisualEvidenceHostReceiptPacket
        {
            WrittenAtUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture),
            Trigger = trigger ?? string.Empty,
            RequestId = requestId,
            SourceReceiptPath = sourceReceiptPath ?? string.Empty,
            SourceReceiptRequestId = sourceReceiptRequestId ?? string.Empty,
            SourceMeshName = sourceMeshName ?? string.Empty,
            VertexCount = vertexCount,
            TemplateRendererPathContains = templateRendererPathContains ?? string.Empty,
            AcceptedVariantId = acceptedVariantId ?? string.Empty,
            RejectAllNonOriginalCandidates = rejectAllNonOriginalCandidates,
            VisualReviewDecisionConfigured = false,
            Variants = Array.Empty<KandraSameMeshVisualEvidenceVariantPacket>(),
            ComparisonResult = comparison,
            CandidateMapApplicationExecuted = false,
            ConversionExecuted = false,
            ItemEquipMutationExecuted = false,
            SaveMutationExecuted = false,
            NativeGameWriteExecuted = false,
            NonRegistrationDownstreamWritesExecuted = false,
            Blockers = blockerArray,
            Warnings = warnings.Where(value => !string.IsNullOrWhiteSpace(value)).ToArray(),
        };
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

    private static string ComputeSha256(string path)
    {
        using SHA256 sha256 = SHA256.Create();
        using FileStream stream = File.OpenRead(path);
        byte[] hash = sha256.ComputeHash(stream);
        char[] chars = new char[hash.Length * 2];
        for (int i = 0; i < hash.Length; i++)
        {
            byte b = hash[i];
            chars[i * 2] = GetHexValue(b / 16);
            chars[(i * 2) + 1] = GetHexValue(b % 16);
        }

        return new string(chars);
    }

    private static char GetHexValue(int value) =>
        (char)(value < 10 ? value + '0' : value - 10 + 'a');

    internal sealed class PendingVisualEvidence : IDisposable
    {
        private readonly string trigger;
        private readonly string requestId;
        private readonly string sourceReceiptPath;
        private readonly KandraSameMeshVisualEvidenceSourceReceiptPacket sourceReceipt;
        private readonly string templateRendererPathContains;
        private readonly string acceptedVariantId;
        private readonly bool rejectAllNonOriginalCandidates;
        private readonly bool visualReviewDecisionConfigured;
        private readonly string[] reviewBlockers;
        private readonly List<string> warnings;
        private readonly FoaKandraRuntimeRegistrationInvoker? invoker;
        private readonly int maxPollFrames;
        private readonly int screenshotWaitFrames;
        private readonly string outputFolder;
        private readonly Action<string> logInfo;
        private readonly Action<string> logWarning;
        private readonly KandraSameMeshVisualEvidenceHostReceiptPacket? immediateReceipt;
        private readonly List<KandraSameMeshVisualEvidenceVariantPacket> variants = new List<KandraSameMeshVisualEvidenceVariantPacket>();
        private readonly Dictionary<string, KandraSameMeshVisualEvidenceSourceVariantReceiptPacket> sourceVariants;
        private FoaKandraRuntimeRegistrationInvoker.PendingInvocation? pendingVariant;
        private KandraSameMeshVisualEvidenceSourceVariantReceiptPacket? pendingSourceVariant;
        private KandraRuntimeRegistrationInvocationAttempt? pendingAttempt;
        private string pendingScreenshotPath = string.Empty;
        private int nextVariantIndex;
        private int screenshotPollFrames;
        private bool disposed;

        internal PendingVisualEvidence(
            string trigger,
            string requestId,
            string sourceReceiptPath,
            KandraSameMeshVisualEvidenceSourceReceiptPacket sourceReceipt,
            string templateRendererPathContains,
            string acceptedVariantId,
            bool rejectAllNonOriginalCandidates,
            bool visualReviewDecisionConfigured,
            IEnumerable<string> reviewBlockers,
            IEnumerable<string> warnings,
            FoaKandraRuntimeRegistrationInvoker invoker,
            int maxPollFrames,
            int screenshotWaitFrames,
            string outputFolder,
            Action<string> logInfo,
            Action<string> logWarning)
        {
            this.trigger = trigger ?? string.Empty;
            this.requestId = requestId;
            this.sourceReceiptPath = sourceReceiptPath;
            this.sourceReceipt = sourceReceipt;
            this.templateRendererPathContains = templateRendererPathContains ?? string.Empty;
            this.acceptedVariantId = acceptedVariantId ?? string.Empty;
            this.rejectAllNonOriginalCandidates = rejectAllNonOriginalCandidates;
            this.visualReviewDecisionConfigured = visualReviewDecisionConfigured;
            this.reviewBlockers = reviewBlockers.Where(value => !string.IsNullOrWhiteSpace(value)).ToArray();
            this.warnings = warnings.Where(value => !string.IsNullOrWhiteSpace(value)).ToList();
            this.invoker = invoker;
            this.maxPollFrames = maxPollFrames;
            this.screenshotWaitFrames = screenshotWaitFrames;
            this.outputFolder = outputFolder;
            this.logInfo = logInfo ?? (_ => { });
            this.logWarning = logWarning ?? (_ => { });
            sourceVariants = sourceReceipt.VariantReceipts
                .GroupBy(variant => variant.VariantId, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
        }

        private PendingVisualEvidence(KandraSameMeshVisualEvidenceHostReceiptPacket immediateReceipt)
        {
            this.immediateReceipt = immediateReceipt;
            trigger = immediateReceipt.Trigger;
            requestId = immediateReceipt.RequestId;
            sourceReceiptPath = immediateReceipt.SourceReceiptPath;
            sourceReceipt = new KandraSameMeshVisualEvidenceSourceReceiptPacket();
            templateRendererPathContains = immediateReceipt.TemplateRendererPathContains;
            acceptedVariantId = immediateReceipt.AcceptedVariantId;
            maxPollFrames = 1;
            screenshotWaitFrames = 1;
            outputFolder = string.Empty;
            warnings = new List<string>();
            reviewBlockers = Array.Empty<string>();
            sourceVariants = new Dictionary<string, KandraSameMeshVisualEvidenceSourceVariantReceiptPacket>(StringComparer.Ordinal);
            logInfo = _ => { };
            logWarning = _ => { };
        }

        public static PendingVisualEvidence Completed(KandraSameMeshVisualEvidenceHostReceiptPacket receipt) =>
            new PendingVisualEvidence(receipt);

        public bool TryComplete(out KandraSameMeshVisualEvidenceHostReceiptPacket? receipt)
        {
            receipt = null;
            if (immediateReceipt != null)
            {
                receipt = immediateReceipt;
                return true;
            }

            if (disposed || invoker == null)
            {
                return false;
            }

            if (pendingVariant == null && pendingAttempt == null && nextVariantIndex < RequiredVariants.Length)
            {
                StartNextVariant();
            }

            if (pendingVariant == null || pendingSourceVariant == null)
            {
                if (nextVariantIndex >= RequiredVariants.Length)
                {
                    receipt = CreateFinalReceipt();
                    return true;
                }

                return false;
            }

            if (pendingAttempt == null)
            {
                if (!pendingVariant.TryComplete(out KandraRuntimeRegistrationInvocationAttempt? attempt) || attempt == null)
                {
                    return false;
                }

                pendingAttempt = attempt;
                if (!attempt.RuntimeRegistrationSucceeded)
                {
                    RecordCurrentVariant(
                        attempt,
                        visualEvidenceCaptured: false,
                        screenshotPath: string.Empty,
                        screenshotSha256: string.Empty,
                        screenshotByteCount: 0,
                        blockers: attempt.Blockers.Concat(new[] { "foa_kandra_same_mesh_visual_registration_not_succeeded" }),
                        warnings: attempt.Warnings);
                    ClearCurrentVariant();
                    return false;
                }

                try
                {
                    pendingScreenshotPath = BuildScreenshotPath(pendingSourceVariant.VariantId);
                    Directory.CreateDirectory(Path.GetDirectoryName(pendingScreenshotPath) ?? outputFolder);
                    ScreenCapture.CaptureScreenshot(pendingScreenshotPath);
                    screenshotPollFrames = 0;
                    logInfo("Tainted Armour requested same-mesh visual screenshot. Variant=" + pendingSourceVariant.VariantId + "; Path=" + pendingScreenshotPath);
                }
                catch (Exception exception)
                {
                    RecordCurrentVariant(
                        attempt,
                        visualEvidenceCaptured: false,
                        screenshotPath: pendingScreenshotPath,
                        screenshotSha256: string.Empty,
                        screenshotByteCount: 0,
                        blockers: new[] { "foa_kandra_same_mesh_visual_capture_request_failed:" + exception.GetType().Name },
                        warnings: attempt.Warnings.Concat(new[] { exception.Message }));
                    ClearCurrentVariant();
                }

                return false;
            }

            screenshotPollFrames++;
            if (!string.IsNullOrWhiteSpace(pendingScreenshotPath) && File.Exists(pendingScreenshotPath))
            {
                FileInfo file = new FileInfo(pendingScreenshotPath);
                if (file.Length > 0)
                {
                    string sha256 = string.Empty;
                    try
                    {
                        sha256 = ComputeSha256(pendingScreenshotPath);
                    }
                    catch (Exception exception)
                    {
                        logWarning("Tainted Armour screenshot hash failed. Variant=" + pendingSourceVariant.VariantId + "; Exception=" + exception.GetType().Name + "; Message=" + exception.Message);
                    }

                    RecordCurrentVariant(
                        pendingAttempt,
                        visualEvidenceCaptured: true,
                        screenshotPath: pendingScreenshotPath,
                        screenshotSha256: sha256,
                        screenshotByteCount: file.Length,
                        blockers: Array.Empty<string>(),
                        warnings: pendingAttempt.Warnings);
                    logInfo("Tainted Armour captured same-mesh visual screenshot. Variant=" + pendingSourceVariant.VariantId + "; Bytes=" + file.Length.ToString(CultureInfo.InvariantCulture) + "; Sha256=" + sha256);
                    ClearCurrentVariant();
                    return false;
                }
            }

            if (screenshotPollFrames >= screenshotWaitFrames)
            {
                RecordCurrentVariant(
                    pendingAttempt,
                    visualEvidenceCaptured: false,
                    screenshotPath: pendingScreenshotPath,
                    screenshotSha256: string.Empty,
                    screenshotByteCount: 0,
                    blockers: new[] { "foa_kandra_same_mesh_visual_screenshot_timeout:frames=" + screenshotPollFrames.ToString(CultureInfo.InvariantCulture) },
                    warnings: pendingAttempt.Warnings);
                ClearCurrentVariant();
            }

            return false;
        }

        private void StartNextVariant()
        {
            if (nextVariantIndex >= RequiredVariants.Length || invoker == null)
            {
                return;
            }

            string variantId = RequiredVariants[nextVariantIndex++];
            if (!sourceVariants.TryGetValue(variantId, out KandraSameMeshVisualEvidenceSourceVariantReceiptPacket? variant))
            {
                return;
            }

            try
            {
                pendingSourceVariant = variant;
                pendingVariant = invoker.Begin(variant.Context.ToContext(), templateRendererPathContains, maxPollFrames);
                logInfo("Tainted Armour started same-mesh visual runtime variant. Variant=" + variant.VariantId + "; Mesh=" + variant.MeshName);
            }
            catch (Exception exception)
            {
                logWarning("Tainted Armour same-mesh visual variant failed to start. Variant=" + variantId + "; Exception=" + exception.GetType().Name + "; Message=" + exception.Message);
                variants.Add(KandraSameMeshVisualEvidenceVariantPacket.FromSource(
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
                        blockers: new[] { "foa_kandra_same_mesh_visual_variant_start_exception:" + exception.GetType().Name },
                        warnings: new[] { exception.Message }),
                    visualEvidenceCaptured: false,
                    visualEvidenceReviewed: false,
                    visuallyAccepted: false,
                    screenshotPath: string.Empty,
                    screenshotSha256: string.Empty,
                    screenshotByteCount: 0,
                    visualEvidenceArtifactId: string.Empty,
                    blockers: new[] { "foa_kandra_same_mesh_visual_variant_start_exception:" + exception.GetType().Name },
                    warnings: new[] { exception.Message }));
                pendingSourceVariant = null;
                pendingVariant = null;
            }
        }

        private void RecordCurrentVariant(
            KandraRuntimeRegistrationInvocationAttempt attempt,
            bool visualEvidenceCaptured,
            string screenshotPath,
            string screenshotSha256,
            long screenshotByteCount,
            IEnumerable<string> blockers,
            IEnumerable<string> warnings)
        {
            if (pendingSourceVariant == null)
            {
                return;
            }

            bool visualEvidenceReviewed = visualReviewDecisionConfigured && visualEvidenceCaptured;
            bool visuallyAccepted = visualEvidenceReviewed &&
                                    !rejectAllNonOriginalCandidates &&
                                    string.Equals(pendingSourceVariant.VariantId, acceptedVariantId, StringComparison.Ordinal);
            string visualEvidenceArtifactId = visualEvidenceCaptured && !string.IsNullOrWhiteSpace(screenshotSha256)
                ? "sha256:" + screenshotSha256 + ";path=" + screenshotPath
                : string.Empty;
            variants.Add(KandraSameMeshVisualEvidenceVariantPacket.FromSource(
                pendingSourceVariant,
                attempt,
                visualEvidenceCaptured,
                visualEvidenceReviewed,
                visuallyAccepted,
                screenshotPath,
                screenshotSha256,
                screenshotByteCount,
                visualEvidenceArtifactId,
                blockers,
                warnings));
        }

        private KandraSameMeshVisualEvidenceHostReceiptPacket CreateFinalReceipt()
        {
            var blockers = new List<string>(reviewBlockers);
            KandraSameMeshVisualEvidenceVariantPacket[] variantArray = variants.ToArray();
            if (variantArray.Length != RequiredVariants.Length)
            {
                blockers.Add("foa_kandra_same_mesh_visual_not_all_variants_attempted:" + variantArray.Length.ToString(CultureInfo.InvariantCulture));
            }

            foreach (KandraSameMeshVisualEvidenceVariantPacket variant in variantArray)
            {
                if (!variant.VisualEvidenceCaptured)
                {
                    blockers.Add("foa_kandra_same_mesh_visual_variant_capture_missing:" + variant.VariantId);
                }
            }

            int roundtripTolerance = sourceReceipt.BuildResult == null
                ? 0
                : Math.Max(4, (int)Math.Ceiling(sourceReceipt.BuildResult.VertexCount * 0.001d));
            KandraSameMeshVisualDecodeComparisonResult comparison = new KandraSameMeshVisualDecodeComparisonStage().Evaluate(
                new KandraSameMeshVisualDecodeComparisonRequest(
                    requestId,
                    sourceReceiptPath,
                    sourceReceipt.RequestId,
                    sourceReceipt.BuildResult?.SourceMeshName ?? string.Empty,
                    sourceReceipt.BuildResult?.VertexCount ?? 0,
                    sourceReceipt.RuntimeProofSucceeded,
                    sourceReceipt.VariantReceipts.Length,
                    sourceReceipt.NonRegistrationDownstreamBoundaryFalse,
                    roundtripTolerance,
                    variantArray.Select(variant => variant.ToComparisonCandidate())));

            return new KandraSameMeshVisualEvidenceHostReceiptPacket
            {
                WrittenAtUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture),
                Trigger = trigger,
                RequestId = requestId,
                SourceReceiptPath = sourceReceiptPath,
                SourceReceiptRequestId = sourceReceipt.RequestId,
                SourceMeshName = sourceReceipt.BuildResult?.SourceMeshName ?? string.Empty,
                VertexCount = sourceReceipt.BuildResult?.VertexCount ?? 0,
                TemplateRendererPathContains = templateRendererPathContains,
                AcceptedVariantId = acceptedVariantId,
                RejectAllNonOriginalCandidates = rejectAllNonOriginalCandidates,
                VisualReviewDecisionConfigured = visualReviewDecisionConfigured,
                Variants = variantArray,
                ComparisonResult = comparison,
                CandidateMapApplicationExecuted = false,
                ConversionExecuted = false,
                ItemEquipMutationExecuted = false,
                SaveMutationExecuted = false,
                NativeGameWriteExecuted = false,
                NonRegistrationDownstreamWritesExecuted = false,
                Blockers = blockers.Where(value => !string.IsNullOrWhiteSpace(value)).ToArray(),
                Warnings = warnings.Where(value => !string.IsNullOrWhiteSpace(value)).ToArray(),
            };
        }

        private string BuildScreenshotPath(string variantId)
        {
            string sourceId = SanitizeFileName(sourceReceipt.RequestId);
            string captureRoot = Path.Combine(outputFolder, "kandra-same-mesh-visual-evidence-" + sourceId);
            string fileName = SanitizeFileName(requestId + "-" + variantId) + ".png";
            return Path.Combine(captureRoot, fileName);
        }

        private void ClearCurrentVariant()
        {
            pendingVariant?.Dispose();
            pendingVariant = null;
            pendingSourceVariant = null;
            pendingAttempt = null;
            pendingScreenshotPath = string.Empty;
            screenshotPollFrames = 0;
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            ClearCurrentVariant();
        }
    }
}

internal sealed class KandraSameMeshVisualEvidenceSourceReceiptPacket
{
    public string RequestId { get; set; } = string.Empty;

    public KandraSameMeshVisualEvidenceSourceBuildResultPacket? BuildResult { get; set; }

    public KandraSameMeshVisualEvidenceSourceVariantReceiptPacket[] VariantReceipts { get; set; } = Array.Empty<KandraSameMeshVisualEvidenceSourceVariantReceiptPacket>();

    public bool CandidateMapApplicationExecuted { get; set; }

    public bool ConversionExecuted { get; set; }

    public bool ItemEquipMutationExecuted { get; set; }

    public bool SaveMutationExecuted { get; set; }

    public bool NativeGameWriteExecuted { get; set; }

    public bool NonRegistrationDownstreamWritesExecuted { get; set; }

    public string[] Blockers { get; set; } = Array.Empty<string>();

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
}

internal sealed class KandraSameMeshVisualEvidenceSourceBuildResultPacket
{
    public string SourceMeshName { get; set; } = string.Empty;

    public int VertexCount { get; set; }
}

internal sealed class KandraSameMeshVisualEvidenceSourceVariantReceiptPacket
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

    public KandraSameMeshVisualEvidenceSourceAttemptPacket? Attempt { get; set; }
}

internal sealed class KandraSameMeshVisualEvidenceSourceAttemptPacket
{
    public bool RuntimeRegistrationSucceeded { get; set; }
}

internal sealed class KandraSameMeshVisualEvidenceHostReceiptPacket
{
    public string StageVersion { get; set; } = FoaKandraSameMeshVisualEvidenceRunner.StageVersion;

    public string WrittenAtUtc { get; set; } = string.Empty;

    public string Trigger { get; set; } = string.Empty;

    public string RequestId { get; set; } = string.Empty;

    public string SourceReceiptPath { get; set; } = string.Empty;

    public string SourceReceiptRequestId { get; set; } = string.Empty;

    public string SourceMeshName { get; set; } = string.Empty;

    public int VertexCount { get; set; }

    public string TemplateRendererPathContains { get; set; } = string.Empty;

    public string AcceptedVariantId { get; set; } = string.Empty;

    public bool RejectAllNonOriginalCandidates { get; set; }

    public bool VisualReviewDecisionConfigured { get; set; }

    public KandraSameMeshVisualEvidenceVariantPacket[] Variants { get; set; } = Array.Empty<KandraSameMeshVisualEvidenceVariantPacket>();

    public KandraSameMeshVisualDecodeComparisonResult? ComparisonResult { get; set; }

    public bool CandidateMapApplicationExecuted { get; set; }

    public bool ConversionExecuted { get; set; }

    public bool ItemEquipMutationExecuted { get; set; }

    public bool SaveMutationExecuted { get; set; }

    public bool NativeGameWriteExecuted { get; set; }

    public bool NonRegistrationDownstreamWritesExecuted { get; set; }

    public string[] Blockers { get; set; } = Array.Empty<string>();

    public string[] Warnings { get; set; } = Array.Empty<string>();

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

internal sealed class KandraSameMeshVisualEvidenceVariantPacket
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

    public bool VisualEvidenceCaptured { get; set; }

    public bool VisualEvidenceReviewed { get; set; }

    public bool VisuallyAccepted { get; set; }

    public string ScreenshotPath { get; set; } = string.Empty;

    public string ScreenshotSha256 { get; set; } = string.Empty;

    public long ScreenshotByteCount { get; set; }

    public string VisualEvidenceArtifactId { get; set; } = string.Empty;

    public string[] Blockers { get; set; } = Array.Empty<string>();

    public string[] Warnings { get; set; } = Array.Empty<string>();

    public static KandraSameMeshVisualEvidenceVariantPacket FromSource(
        KandraSameMeshVisualEvidenceSourceVariantReceiptPacket source,
        KandraRuntimeRegistrationInvocationAttempt attempt,
        bool visualEvidenceCaptured,
        bool visualEvidenceReviewed,
        bool visuallyAccepted,
        string screenshotPath,
        string screenshotSha256,
        long screenshotByteCount,
        string visualEvidenceArtifactId,
        IEnumerable<string> blockers,
        IEnumerable<string> warnings) =>
        new KandraSameMeshVisualEvidenceVariantPacket
        {
            VariantId = source.VariantId,
            VariantLabel = source.VariantLabel,
            MeshName = source.MeshName,
            MeshDataSha256 = source.MeshDataSha256,
            IndicesDataSha256 = source.IndicesDataSha256,
            SourceMeshPayloadPreservedExactly = source.SourceMeshPayloadPreservedExactly,
            SourceIndicesPayloadPreservedExactly = source.SourceIndicesPayloadPreservedExactly,
            OnlyCompressedVertexPlus16Changed = source.OnlyCompressedVertexPlus16Changed,
            ChangedByteCount = source.ChangedByteCount,
            ChangedCompressedVertexPlus16ByteCount = source.ChangedCompressedVertexPlus16ByteCount,
            ChangedNonPlus16ByteCount = source.ChangedNonPlus16ByteCount,
            ChangedPlus16FieldCount = source.ChangedPlus16FieldCount,
            Context = source.Context,
            Attempt = attempt,
            VisualEvidenceCaptured = visualEvidenceCaptured,
            VisualEvidenceReviewed = visualEvidenceReviewed,
            VisuallyAccepted = visuallyAccepted,
            ScreenshotPath = screenshotPath ?? string.Empty,
            ScreenshotSha256 = screenshotSha256 ?? string.Empty,
            ScreenshotByteCount = screenshotByteCount,
            VisualEvidenceArtifactId = visualEvidenceArtifactId ?? string.Empty,
            Blockers = blockers.Where(value => !string.IsNullOrWhiteSpace(value)).ToArray(),
            Warnings = warnings.Where(value => !string.IsNullOrWhiteSpace(value)).ToArray(),
        };

    public KandraSameMeshVisualDecodeComparisonCandidate ToComparisonCandidate() =>
        new KandraSameMeshVisualDecodeComparisonCandidate(
            VariantId,
            VariantLabel,
            Attempt != null && Attempt.RuntimeRegistrationSucceeded,
            Attempt?.KandraIsRegisteredStatus ?? string.Empty,
            Attempt?.KandraTryGetMeshMemoryStatus ?? string.Empty,
            SourceMeshPayloadPreservedExactly,
            SourceIndicesPayloadPreservedExactly,
            OnlyCompressedVertexPlus16Changed,
            ChangedNonPlus16ByteCount,
            ChangedPlus16FieldCount,
            VisualEvidenceCaptured,
            VisualEvidenceReviewed,
            VisuallyAccepted,
            VisualEvidenceArtifactId);
}
