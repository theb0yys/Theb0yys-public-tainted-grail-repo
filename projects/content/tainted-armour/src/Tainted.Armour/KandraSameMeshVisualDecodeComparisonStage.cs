using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Tainted.Armour;

public sealed class KandraSameMeshVisualDecodeComparisonCandidate
{
    public KandraSameMeshVisualDecodeComparisonCandidate(
        string variantId,
        string variantLabel,
        bool runtimeRegistrationSucceeded,
        string kandraIsRegisteredStatus,
        string kandraTryGetMeshMemoryStatus,
        bool sourceMeshPayloadPreservedExactly,
        bool sourceIndicesPayloadPreservedExactly,
        bool onlyCompressedVertexPlus16Changed,
        int changedNonPlus16ByteCount,
        int changedPlus16FieldCount,
        bool visualEvidenceCaptured,
        bool visualEvidenceReviewed,
        bool visuallyAccepted,
        string visualEvidenceArtifactId = "")
    {
        VariantId = ContractValues.RequireText(variantId, nameof(variantId));
        VariantLabel = variantLabel ?? string.Empty;
        RuntimeRegistrationSucceeded = runtimeRegistrationSucceeded;
        KandraIsRegisteredStatus = kandraIsRegisteredStatus ?? string.Empty;
        KandraTryGetMeshMemoryStatus = kandraTryGetMeshMemoryStatus ?? string.Empty;
        SourceMeshPayloadPreservedExactly = sourceMeshPayloadPreservedExactly;
        SourceIndicesPayloadPreservedExactly = sourceIndicesPayloadPreservedExactly;
        OnlyCompressedVertexPlus16Changed = onlyCompressedVertexPlus16Changed;
        ChangedNonPlus16ByteCount = changedNonPlus16ByteCount;
        ChangedPlus16FieldCount = changedPlus16FieldCount;
        VisualEvidenceCaptured = visualEvidenceCaptured;
        VisualEvidenceReviewed = visualEvidenceReviewed;
        VisuallyAccepted = visuallyAccepted;
        VisualEvidenceArtifactId = visualEvidenceArtifactId ?? string.Empty;
    }

    public string VariantId { get; }

    public string VariantLabel { get; }

    public bool RuntimeRegistrationSucceeded { get; }

    public string KandraIsRegisteredStatus { get; }

    public string KandraTryGetMeshMemoryStatus { get; }

    public bool SourceMeshPayloadPreservedExactly { get; }

    public bool SourceIndicesPayloadPreservedExactly { get; }

    public bool OnlyCompressedVertexPlus16Changed { get; }

    public int ChangedNonPlus16ByteCount { get; }

    public int ChangedPlus16FieldCount { get; }

    public bool VisualEvidenceCaptured { get; }

    public bool VisualEvidenceReviewed { get; }

    public bool VisuallyAccepted { get; }

    public string VisualEvidenceArtifactId { get; }

    public int ExactPlus16FieldMatchCount(int vertexCount) =>
        Math.Max(0, vertexCount - Math.Max(0, ChangedPlus16FieldCount));

    public double ExactPlus16FieldMatchRatio(int vertexCount) =>
        vertexCount <= 0 ? 0.0d : ExactPlus16FieldMatchCount(vertexCount) / (double)vertexCount;
}

public sealed class KandraSameMeshVisualDecodeComparisonRequest
{
    private readonly KandraSameMeshVisualDecodeComparisonCandidate[] candidates;

    public KandraSameMeshVisualDecodeComparisonRequest(
        string requestId,
        string sourceReceiptPath,
        string sourceReceiptRequestId,
        string sourceMeshName,
        int vertexCount,
        bool runtimeProofSucceeded,
        int receiptVariantCount,
        bool nonRegistrationDownstreamBoundaryFalse,
        int roundtripChangedPlus16FieldTolerance,
        IEnumerable<KandraSameMeshVisualDecodeComparisonCandidate> candidates)
    {
        RequestId = ContractValues.RequireText(requestId, nameof(requestId));
        SourceReceiptPath = ContractValues.RequireText(sourceReceiptPath, nameof(sourceReceiptPath));
        SourceReceiptRequestId = ContractValues.RequireText(sourceReceiptRequestId, nameof(sourceReceiptRequestId));
        SourceMeshName = ContractValues.RequireText(sourceMeshName, nameof(sourceMeshName));
        VertexCount = vertexCount;
        RuntimeProofSucceeded = runtimeProofSucceeded;
        ReceiptVariantCount = receiptVariantCount;
        NonRegistrationDownstreamBoundaryFalse = nonRegistrationDownstreamBoundaryFalse;
        RoundtripChangedPlus16FieldTolerance = roundtripChangedPlus16FieldTolerance;
        this.candidates = (candidates ?? Enumerable.Empty<KandraSameMeshVisualDecodeComparisonCandidate>()).ToArray();
    }

    public string RequestId { get; }

    public string SourceReceiptPath { get; }

    public string SourceReceiptRequestId { get; }

    public string SourceMeshName { get; }

    public int VertexCount { get; }

    public bool RuntimeProofSucceeded { get; }

    public int ReceiptVariantCount { get; }

    public bool NonRegistrationDownstreamBoundaryFalse { get; }

    public int RoundtripChangedPlus16FieldTolerance { get; }

    public IReadOnlyList<KandraSameMeshVisualDecodeComparisonCandidate> Candidates => candidates;
}

public sealed class KandraSameMeshVisualDecodeComparisonResult
{
    private readonly KandraSameMeshVisualDecodeComparisonCandidate[] candidates;
    private readonly string[] blockers;
    private readonly string[] warnings;

    public KandraSameMeshVisualDecodeComparisonResult(
        string stageVersion,
        string requestId,
        string sourceReceiptPath,
        string sourceReceiptRequestId,
        string sourceMeshName,
        int vertexCount,
        bool hostRegistrationProofAccepted,
        bool decodeComparisonAccepted,
        bool visualEvidenceAccepted,
        bool visualEvidenceRejectedAllCandidates,
        string selectedEncoderVariantId,
        string decisionStatus,
        IEnumerable<KandraSameMeshVisualDecodeComparisonCandidate> candidates,
        IEnumerable<string> blockers,
        IEnumerable<string> warnings,
        bool candidateMapApplicationExecuted,
        bool conversionExecuted,
        bool itemEquipMutationExecuted,
        bool saveMutationExecuted,
        bool nativeGameWriteExecuted,
        bool nonRegistrationDownstreamWritesExecuted)
    {
        StageVersion = ContractValues.RequireText(stageVersion, nameof(stageVersion));
        RequestId = ContractValues.RequireText(requestId, nameof(requestId));
        SourceReceiptPath = ContractValues.RequireText(sourceReceiptPath, nameof(sourceReceiptPath));
        SourceReceiptRequestId = ContractValues.RequireText(sourceReceiptRequestId, nameof(sourceReceiptRequestId));
        SourceMeshName = ContractValues.RequireText(sourceMeshName, nameof(sourceMeshName));
        VertexCount = vertexCount;
        HostRegistrationProofAccepted = hostRegistrationProofAccepted;
        DecodeComparisonAccepted = decodeComparisonAccepted;
        VisualEvidenceAccepted = visualEvidenceAccepted;
        VisualEvidenceRejectedAllCandidates = visualEvidenceRejectedAllCandidates;
        SelectedEncoderVariantId = selectedEncoderVariantId ?? string.Empty;
        DecisionStatus = ContractValues.RequireText(decisionStatus, nameof(decisionStatus));
        this.candidates = (candidates ?? Enumerable.Empty<KandraSameMeshVisualDecodeComparisonCandidate>()).ToArray();
        this.blockers = (blockers ?? Enumerable.Empty<string>()).ToArray();
        this.warnings = (warnings ?? Enumerable.Empty<string>()).ToArray();
        CandidateMapApplicationExecuted = candidateMapApplicationExecuted;
        ConversionExecuted = conversionExecuted;
        ItemEquipMutationExecuted = itemEquipMutationExecuted;
        SaveMutationExecuted = saveMutationExecuted;
        NativeGameWriteExecuted = nativeGameWriteExecuted;
        NonRegistrationDownstreamWritesExecuted = nonRegistrationDownstreamWritesExecuted;
    }

    public string StageVersion { get; }

    public string RequestId { get; }

    public string SourceReceiptPath { get; }

    public string SourceReceiptRequestId { get; }

    public string SourceMeshName { get; }

    public int VertexCount { get; }

    public bool HostRegistrationProofAccepted { get; }

    public bool DecodeComparisonAccepted { get; }

    public bool VisualEvidenceAccepted { get; }

    public bool VisualEvidenceRejectedAllCandidates { get; }

    public string SelectedEncoderVariantId { get; }

    public string DecisionStatus { get; }

    public IReadOnlyList<KandraSameMeshVisualDecodeComparisonCandidate> Candidates => candidates;

    public IReadOnlyList<string> Blockers => blockers;

    public IReadOnlyList<string> Warnings => warnings;

    public bool CandidateMapApplicationExecuted { get; }

    public bool ConversionExecuted { get; }

    public bool ItemEquipMutationExecuted { get; }

    public bool SaveMutationExecuted { get; }

    public bool NativeGameWriteExecuted { get; }

    public bool NonRegistrationDownstreamWritesExecuted { get; }

    public bool NonRegistrationDownstreamBoundaryFalse =>
        !CandidateMapApplicationExecuted &&
        !ConversionExecuted &&
        !ItemEquipMutationExecuted &&
        !SaveMutationExecuted &&
        !NativeGameWriteExecuted &&
        !NonRegistrationDownstreamWritesExecuted;

    public bool ProductionEncoderDecisionAccepted =>
        HostRegistrationProofAccepted &&
        DecodeComparisonAccepted &&
        VisualEvidenceAccepted &&
        !string.IsNullOrWhiteSpace(SelectedEncoderVariantId) &&
        blockers.Length == 0 &&
        NonRegistrationDownstreamBoundaryFalse;
}

public sealed class KandraSameMeshVisualDecodeComparisonStage
{
    public const string StageVersion = "tainted-armour.kandra-same-mesh-visual-decode-comparison.v1";
    public const string BlockedVisualEvidenceMissing = "blocked:visual_evidence_missing";
    public const string BlockedVisualEvidenceAmbiguous = "blocked:visual_evidence_ambiguous";
    public const string RejectedAllCandidates = "rejected:all_non_original_visual_candidates";
    public const string AcceptedPrefix = "accepted:";

    private static readonly string[] RequiredVariants =
    {
        KandraSameMeshAbProofStage.OriginalBytesVariantId,
        KandraSameMeshAbProofStage.BarGeometricPlus16VariantId,
        KandraSameMeshAbProofStage.DuplicateNormalPlus16VariantId,
        KandraSameMeshAbProofStage.RecoveredOctahedralRoundtripPlus16VariantId,
    };

    public KandraSameMeshVisualDecodeComparisonResult Evaluate(KandraSameMeshVisualDecodeComparisonRequest request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        var blockers = new List<string>();
        var warnings = new List<string>();

        if (request.VertexCount <= 0)
        {
            blockers.Add("kandra_same_mesh_visual_decode_vertex_count_missing");
        }

        bool hostRegistrationProofAccepted = request.RuntimeProofSucceeded &&
                                             request.ReceiptVariantCount == RequiredVariants.Length &&
                                             request.NonRegistrationDownstreamBoundaryFalse;
        if (!request.RuntimeProofSucceeded)
        {
            blockers.Add("kandra_same_mesh_visual_decode_runtime_proof_not_succeeded");
        }

        if (request.ReceiptVariantCount != RequiredVariants.Length)
        {
            blockers.Add("kandra_same_mesh_visual_decode_variant_count_mismatch:" + request.ReceiptVariantCount.ToString(CultureInfo.InvariantCulture));
        }

        if (!request.NonRegistrationDownstreamBoundaryFalse)
        {
            blockers.Add("kandra_same_mesh_visual_decode_non_registration_boundary_not_false");
        }

        Dictionary<string, KandraSameMeshVisualDecodeComparisonCandidate> byVariant = request.Candidates
            .GroupBy(candidate => candidate.VariantId, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);

        foreach (string required in RequiredVariants)
        {
            if (!byVariant.ContainsKey(required))
            {
                blockers.Add("kandra_same_mesh_visual_decode_required_variant_missing:" + required);
            }
        }

        foreach (KandraSameMeshVisualDecodeComparisonCandidate candidate in request.Candidates)
        {
            if (!candidate.RuntimeRegistrationSucceeded)
            {
                blockers.Add("kandra_same_mesh_visual_decode_variant_registration_not_succeeded:" + candidate.VariantId);
            }

            if (!IsObservedTrue(candidate.KandraIsRegisteredStatus))
            {
                blockers.Add("kandra_same_mesh_visual_decode_variant_is_registered_not_observed_true:" + candidate.VariantId);
            }

            if (!IsObservedTrue(candidate.KandraTryGetMeshMemoryStatus))
            {
                blockers.Add("kandra_same_mesh_visual_decode_variant_mesh_memory_not_observed_true:" + candidate.VariantId);
            }

            if (!candidate.SourceIndicesPayloadPreservedExactly)
            {
                blockers.Add("kandra_same_mesh_visual_decode_variant_indices_not_preserved:" + candidate.VariantId);
            }

            if (candidate.ChangedNonPlus16ByteCount != 0)
            {
                blockers.Add("kandra_same_mesh_visual_decode_variant_non_plus16_bytes_changed:" + candidate.VariantId);
            }

            if (!candidate.OnlyCompressedVertexPlus16Changed)
            {
                blockers.Add("kandra_same_mesh_visual_decode_variant_not_plus16_only:" + candidate.VariantId);
            }
        }

        bool decodeComparisonAccepted = false;
        if (byVariant.TryGetValue(KandraSameMeshAbProofStage.OriginalBytesVariantId, out KandraSameMeshVisualDecodeComparisonCandidate? original) &&
            byVariant.TryGetValue(KandraSameMeshAbProofStage.RecoveredOctahedralRoundtripPlus16VariantId, out KandraSameMeshVisualDecodeComparisonCandidate? recovered) &&
            request.VertexCount > 0)
        {
            if (!original.SourceMeshPayloadPreservedExactly || original.ChangedPlus16FieldCount != 0)
            {
                blockers.Add("kandra_same_mesh_visual_decode_original_not_exact");
            }

            if (recovered.ChangedPlus16FieldCount > request.RoundtripChangedPlus16FieldTolerance)
            {
                blockers.Add(
                    "kandra_same_mesh_visual_decode_roundtrip_exceeds_tolerance:" +
                    recovered.ChangedPlus16FieldCount.ToString(CultureInfo.InvariantCulture) +
                    ">" +
                    request.RoundtripChangedPlus16FieldTolerance.ToString(CultureInfo.InvariantCulture));
            }

            decodeComparisonAccepted = original.SourceMeshPayloadPreservedExactly &&
                                       original.ChangedPlus16FieldCount == 0 &&
                                       recovered.ChangedPlus16FieldCount <= request.RoundtripChangedPlus16FieldTolerance;
            warnings.Add(
                "kandra_same_mesh_visual_decode_recovered_roundtrip_exact_match:" +
                recovered.ExactPlus16FieldMatchCount(request.VertexCount).ToString(CultureInfo.InvariantCulture) +
                "/" +
                request.VertexCount.ToString(CultureInfo.InvariantCulture));
        }

        bool visualEvidenceComplete = RequiredVariants.All(required =>
            byVariant.TryGetValue(required, out KandraSameMeshVisualDecodeComparisonCandidate? candidate) &&
            candidate.VisualEvidenceCaptured &&
            candidate.VisualEvidenceReviewed &&
            !string.IsNullOrWhiteSpace(candidate.VisualEvidenceArtifactId));
        KandraSameMeshVisualDecodeComparisonCandidate[] visuallyAccepted = request.Candidates
            .Where(candidate => candidate.VisualEvidenceCaptured && candidate.VisualEvidenceReviewed && candidate.VisuallyAccepted)
            .ToArray();
        KandraSameMeshVisualDecodeComparisonCandidate[] visuallyAcceptedNonOriginal = visuallyAccepted
            .Where(candidate => !string.Equals(candidate.VariantId, KandraSameMeshAbProofStage.OriginalBytesVariantId, StringComparison.Ordinal))
            .ToArray();
        bool originalAccepted = visuallyAccepted.Any(candidate => string.Equals(candidate.VariantId, KandraSameMeshAbProofStage.OriginalBytesVariantId, StringComparison.Ordinal));
        bool visualEvidenceAccepted = visualEvidenceComplete &&
                                      !originalAccepted &&
                                      visuallyAcceptedNonOriginal.Length == 1 &&
                                      visuallyAccepted.Length == 1;
        bool visualEvidenceRejectedAllCandidates = visualEvidenceComplete &&
                                                   visuallyAccepted.Length == 0;
        string selected = visualEvidenceAccepted ? visuallyAccepted[0].VariantId : string.Empty;
        string decisionStatus;

        if (blockers.Count == 0 && hostRegistrationProofAccepted && decodeComparisonAccepted && visualEvidenceAccepted)
        {
            decisionStatus = AcceptedPrefix + selected;
        }
        else if (blockers.Count == 0 && hostRegistrationProofAccepted && decodeComparisonAccepted && visualEvidenceRejectedAllCandidates)
        {
            decisionStatus = RejectedAllCandidates;
        }
        else if (blockers.Count == 0 && hostRegistrationProofAccepted && decodeComparisonAccepted && visualEvidenceComplete)
        {
            blockers.Add("kandra_same_mesh_visual_decode_visual_evidence_ambiguous");
            decisionStatus = BlockedVisualEvidenceAmbiguous;
        }
        else if (blockers.Count == 0 && hostRegistrationProofAccepted && decodeComparisonAccepted)
        {
            blockers.Add("kandra_same_mesh_visual_decode_visual_evidence_missing");
            decisionStatus = BlockedVisualEvidenceMissing;
        }
        else
        {
            decisionStatus = "blocked:comparison_inputs_not_accepted";
        }

        return new KandraSameMeshVisualDecodeComparisonResult(
            StageVersion,
            request.RequestId,
            request.SourceReceiptPath,
            request.SourceReceiptRequestId,
            request.SourceMeshName,
            request.VertexCount,
            hostRegistrationProofAccepted,
            decodeComparisonAccepted,
            visualEvidenceAccepted,
            visualEvidenceRejectedAllCandidates,
            selected,
            decisionStatus,
            request.Candidates,
            blockers,
            warnings,
            candidateMapApplicationExecuted: false,
            conversionExecuted: false,
            itemEquipMutationExecuted: false,
            saveMutationExecuted: false,
            nativeGameWriteExecuted: false,
            nonRegistrationDownstreamWritesExecuted: false);
    }

    private static bool IsObservedTrue(string value) =>
        !string.IsNullOrWhiteSpace(value) &&
        value.StartsWith("observed:True", StringComparison.OrdinalIgnoreCase);
}
