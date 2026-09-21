using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Tainted.Armour.Canonical;
using Tainted.Armour.Serialization;

namespace Tainted.Armour.Compatibility;

public sealed class SourceTargetCompatibilityStage
{
    public const string StageContractVersion = "tainted-armour.source-target-compatibility.v1";

    public SourceTargetCompatibilityResultV1 Evaluate(SourceTargetCompatibilityRequestV1 request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        var assertions = new List<string>();
        var diagnostics = new List<string>();
        var blockers = new List<string>();

        VerifyR2Artifact("source", request.Source.Manifest, request.Source.ManifestArtifactId, assertions, blockers);
        VerifyR2Artifact("target", request.Target.Manifest, request.Target.ManifestArtifactId, assertions, blockers);
        VerifyNoWriteBoundary(request, assertions, blockers);

        SourceTargetCompatibilityClassification classification = blockers.Count == 0
            ? Classify(request, diagnostics)
            : SourceTargetCompatibilityClassification.Incompatible;

        CompatibilityGateState gate = DetermineGate(classification, request.Policy, blockers, diagnostics);
        ArtifactId cacheKey = CreateCacheKey(request);
        ArtifactId receiptId = CreateReceiptId(request, gate, classification, assertions, diagnostics, blockers);
        var receipt = new SourceTargetCompatibilityReceiptV1(
            receiptId,
            StageContractVersion,
            gate,
            classification,
            request.Source.ManifestArtifactId,
            request.Target.ManifestArtifactId,
            request.Target.TargetProfileFingerprint,
            request.Policy.Fingerprint,
            assertions,
            diagnostics);

        return new SourceTargetCompatibilityResultV1(
            StageContractVersion,
            gate,
            classification,
            cacheKey,
            receipt,
            blockers,
            diagnostics);
    }

    private static void VerifyR2Artifact(
        string role,
        CanonicalArmorManifestV1 manifest,
        ArtifactId supplied,
        ICollection<string> assertions,
        ICollection<string> blockers)
    {
        CanonicalManifestArtifact observed = CanonicalManifestWriter.Write(manifest);
        if (observed.ArtifactId == supplied)
        {
            assertions.Add($"r2_{role}_manifest_artifact_matches_current_canonical_bytes");
            return;
        }

        blockers.Add($"r2_{role}_manifest_artifact_mismatch:{supplied}!={observed.ArtifactId}");
    }

    private static void VerifyNoWriteBoundary(
        SourceTargetCompatibilityRequestV1 request,
        ICollection<string> assertions,
        ICollection<string> blockers)
    {
        if (!request.CandidateMapApplicationAllowed
            && !request.CandidateMapApplicationExecuted
            && !request.ConversionAllowed
            && !request.ConversionExecuted
            && !request.ItemEquipMutationAllowed
            && !request.ItemEquipMutationExecuted
            && !request.SaveMutationAllowed
            && !request.SaveMutationExecuted
            && !request.DownstreamWritesAllowed
            && !request.DownstreamWritesExecuted)
        {
            assertions.Add("candidate_map_conversion_item_equip_save_and_downstream_write_boundaries_false");
            return;
        }

        if (request.CandidateMapApplicationAllowed) blockers.Add("candidate_map_application_allowed_not_false");
        if (request.CandidateMapApplicationExecuted) blockers.Add("candidate_map_application_executed_not_false");
        if (request.ConversionAllowed) blockers.Add("conversion_allowed_not_false");
        if (request.ConversionExecuted) blockers.Add("conversion_executed_not_false");
        if (request.ItemEquipMutationAllowed) blockers.Add("item_equip_mutation_allowed_not_false");
        if (request.ItemEquipMutationExecuted) blockers.Add("item_equip_mutation_executed_not_false");
        if (request.SaveMutationAllowed) blockers.Add("save_mutation_allowed_not_false");
        if (request.SaveMutationExecuted) blockers.Add("save_mutation_executed_not_false");
        if (request.DownstreamWritesAllowed) blockers.Add("downstream_writes_allowed_not_false");
        if (request.DownstreamWritesExecuted) blockers.Add("downstream_writes_executed_not_false");
    }

    private static SourceTargetCompatibilityClassification Classify(
        SourceTargetCompatibilityRequestV1 request,
        ICollection<string> diagnostics)
    {
        if (request.Source.ManifestArtifactId == request.Target.ManifestArtifactId)
        {
            diagnostics.Add("classification_exact:same_canonical_manifest_artifact");
            return SourceTargetCompatibilityClassification.Exact;
        }

        MeshProfileMatch source = MeshProfileMatch.TryCreate(request.Source.Manifest);
        MeshProfileMatch target = MeshProfileMatch.TryCreate(request.Target.Manifest);
        if (!source.Accepted || !target.Accepted)
        {
            diagnostics.Add("classification_unresolved:" + source.Reason + ":" + target.Reason);
            return SourceTargetCompatibilityClassification.Unresolved;
        }

        MeshProfileComparison comparison = MeshProfileMatch.Compare(source, target);
        diagnostics.Add(comparison.Diagnostic);
        if (comparison.Equivalent)
        {
            return SourceTargetCompatibilityClassification.StructuralEquivalent;
        }

        return comparison.TransferRequired
            ? SourceTargetCompatibilityClassification.TransferRequired
            : SourceTargetCompatibilityClassification.Unresolved;
    }

    private static CompatibilityGateState DetermineGate(
        SourceTargetCompatibilityClassification classification,
        SourceTargetCompatibilityPolicyV1 policy,
        ICollection<string> blockers,
        ICollection<string> diagnostics)
    {
        if (blockers.Count != 0)
        {
            diagnostics.Add("gate_fail:blockers_present");
            return CompatibilityGateState.Fail;
        }

        switch (classification)
        {
            case SourceTargetCompatibilityClassification.Exact:
                diagnostics.Add("gate_pass:exact");
                return CompatibilityGateState.Pass;
            case SourceTargetCompatibilityClassification.SemanticEquivalent:
                if (policy.AcceptSemanticEquivalence)
                {
                    diagnostics.Add("gate_pass:semantic_equivalence_accepted_by_policy");
                    return CompatibilityGateState.Pass;
                }

                blockers.Add("semantic_equivalence_not_accepted_by_policy");
                diagnostics.Add("gate_indeterminate:semantic_equivalence_not_policy_accepted");
                return CompatibilityGateState.Indeterminate;
            case SourceTargetCompatibilityClassification.StructuralEquivalent:
                if (policy.AcceptStructuralEquivalence)
                {
                    diagnostics.Add("gate_pass:structural_equivalence_accepted_by_policy");
                    return CompatibilityGateState.Pass;
                }

                blockers.Add("structural_equivalence_not_accepted_by_policy");
                diagnostics.Add("gate_indeterminate:structural_equivalence_not_policy_accepted");
                return CompatibilityGateState.Indeterminate;
            case SourceTargetCompatibilityClassification.TransferRequired:
                blockers.Add("adaptation_plan_required_before_mutation");
                diagnostics.Add("gate_indeterminate:transfer_required_without_adaptation_plan");
                return CompatibilityGateState.Indeterminate;
            case SourceTargetCompatibilityClassification.Unresolved:
                blockers.Add("source_target_mapping_unresolved");
                diagnostics.Add("gate_indeterminate:source_target_mapping_unresolved");
                return CompatibilityGateState.Indeterminate;
            default:
                blockers.Add("source_target_incompatible");
                diagnostics.Add("gate_fail:source_target_incompatible");
                return CompatibilityGateState.Fail;
        }
    }

    private static ArtifactId CreateCacheKey(SourceTargetCompatibilityRequestV1 request) =>
        CompatibilityIdentity.HashCanonicalJson(new Dictionary<string, object?>
        {
            ["schemaVersion"] = "tainted-armour.source-target-compatibility-cache-key/1",
            ["stageContractVersion"] = StageContractVersion,
            ["sourceCanonicalArtifact"] = request.Source.ManifestArtifactId.ToString(),
            ["targetCanonicalArtifact"] = request.Target.ManifestArtifactId.ToString(),
            ["targetProfileFingerprint"] = request.Target.TargetProfileFingerprint.ToString(),
            ["policyFingerprint"] = request.Policy.Fingerprint.ToString(),
        });

    private static ArtifactId CreateReceiptId(
        SourceTargetCompatibilityRequestV1 request,
        CompatibilityGateState gate,
        SourceTargetCompatibilityClassification classification,
        IReadOnlyList<string> assertions,
        IReadOnlyList<string> diagnostics,
        IReadOnlyList<string> blockers) =>
        CompatibilityIdentity.HashCanonicalJson(new Dictionary<string, object?>
        {
            ["schemaVersion"] = "tainted-armour.source-target-compatibility-receipt/1",
            ["stageContractVersion"] = StageContractVersion,
            ["sourceCanonicalArtifact"] = request.Source.ManifestArtifactId.ToString(),
            ["targetCanonicalArtifact"] = request.Target.ManifestArtifactId.ToString(),
            ["targetProfileFingerprint"] = request.Target.TargetProfileFingerprint.ToString(),
            ["policyFingerprint"] = request.Policy.Fingerprint.ToString(),
            ["gate"] = GateText(gate),
            ["classification"] = ClassificationText(classification),
            ["assertions"] = assertions.ToArray(),
            ["diagnostics"] = diagnostics.ToArray(),
            ["blockers"] = blockers.ToArray(),
        });

    private static string GateText(CompatibilityGateState value) => value switch
    {
        CompatibilityGateState.Pass => "PASS",
        CompatibilityGateState.Fail => "FAIL",
        CompatibilityGateState.Indeterminate => "INDETERMINATE",
        _ => throw new ArgumentOutOfRangeException(nameof(value)),
    };

    private static string ClassificationText(SourceTargetCompatibilityClassification value) => value switch
    {
        SourceTargetCompatibilityClassification.Exact => "EXACT",
        SourceTargetCompatibilityClassification.SemanticEquivalent => "SEMANTIC_EQUIVALENT",
        SourceTargetCompatibilityClassification.StructuralEquivalent => "STRUCTURAL_EQUIVALENT",
        SourceTargetCompatibilityClassification.TransferRequired => "TRANSFER_REQUIRED",
        SourceTargetCompatibilityClassification.Unresolved => "UNRESOLVED",
        SourceTargetCompatibilityClassification.Incompatible => "INCOMPATIBLE",
        _ => throw new ArgumentOutOfRangeException(nameof(value)),
    };

    private sealed class MeshProfileMatch
    {
        private MeshProfileMatch(bool accepted, string reason, IReadOnlyDictionary<string, MeshShape> meshes)
        {
            Accepted = accepted;
            Reason = reason;
            Meshes = meshes;
        }

        public bool Accepted { get; }
        public string Reason { get; }
        public IReadOnlyDictionary<string, MeshShape> Meshes { get; }

        public static MeshProfileMatch TryCreate(CanonicalArmorManifestV1 manifest)
        {
            var meshes = new Dictionary<string, MeshShape>(StringComparer.Ordinal);
            foreach (CanonicalMeshV1 mesh in manifest.Meshes.Values)
            {
                if (string.IsNullOrWhiteSpace(mesh.Name))
                {
                    return new MeshProfileMatch(false, "mesh_name_missing", meshes);
                }

                string name = mesh.Name.Trim();
                if (meshes.ContainsKey(name))
                {
                    return new MeshProfileMatch(false, "mesh_name_not_unique:" + name, meshes);
                }

                CanonicalMeshBindingV1 binding = manifest.MeshBindings.Values.Single(value => value.MeshId == mesh.Id);
                meshes.Add(name, new MeshShape(
                    mesh.VertexCount,
                    mesh.Attributes.Keys.OrderBy(value => value, StringComparer.Ordinal).ToArray(),
                    mesh.Primitives.Count,
                    mesh.Primitives.Count(value => value.MaterialId.HasValue),
                    binding.SkinId.HasValue));
            }

            return new MeshProfileMatch(true, "mesh_names_unique", meshes);
        }

        public static MeshProfileComparison Compare(MeshProfileMatch source, MeshProfileMatch target)
        {
            string[] missingInTarget = source.Meshes.Keys.Except(target.Meshes.Keys, StringComparer.Ordinal).OrderBy(value => value, StringComparer.Ordinal).ToArray();
            string[] missingInSource = target.Meshes.Keys.Except(source.Meshes.Keys, StringComparer.Ordinal).OrderBy(value => value, StringComparer.Ordinal).ToArray();
            if (missingInTarget.Length != 0 || missingInSource.Length != 0)
            {
                return new MeshProfileComparison(
                    equivalent: false,
                    transferRequired: false,
                    "classification_unresolved:mesh_name_sets_differ");
            }

            bool transferRequired = false;
            foreach (string meshName in source.Meshes.Keys.OrderBy(value => value, StringComparer.Ordinal))
            {
                MeshShape sourceShape = source.Meshes[meshName];
                MeshShape targetShape = target.Meshes[meshName];
                if (sourceShape.StructurallyEquals(targetShape)) continue;

                transferRequired = true;
            }

            return transferRequired
                ? new MeshProfileComparison(false, true, "classification_transfer_required:matched_mesh_names_with_different_structure")
                : new MeshProfileComparison(true, false, "classification_structural_equivalent:matched_mesh_names_and_structure");
        }
    }

    private sealed class MeshShape
    {
        public MeshShape(
            long vertexCount,
            IReadOnlyList<string> attributeNames,
            int primitiveCount,
            int materialBoundPrimitiveCount,
            bool hasSkin)
        {
            VertexCount = vertexCount;
            AttributeNames = attributeNames;
            PrimitiveCount = primitiveCount;
            MaterialBoundPrimitiveCount = materialBoundPrimitiveCount;
            HasSkin = hasSkin;
        }

        public long VertexCount { get; }
        public IReadOnlyList<string> AttributeNames { get; }
        public int PrimitiveCount { get; }
        public int MaterialBoundPrimitiveCount { get; }
        public bool HasSkin { get; }

        public bool StructurallyEquals(MeshShape other) =>
            VertexCount == other.VertexCount
            && PrimitiveCount == other.PrimitiveCount
            && MaterialBoundPrimitiveCount == other.MaterialBoundPrimitiveCount
            && HasSkin == other.HasSkin
            && AttributeNames.SequenceEqual(other.AttributeNames, StringComparer.Ordinal);
    }

    private readonly struct MeshProfileComparison
    {
        public MeshProfileComparison(bool equivalent, bool transferRequired, string diagnostic)
        {
            Equivalent = equivalent;
            TransferRequired = transferRequired;
            Diagnostic = diagnostic;
        }

        public bool Equivalent { get; }
        public bool TransferRequired { get; }
        public string Diagnostic { get; }
    }
}

internal static class CompatibilityIdentity
{
    public static ArtifactId HashCanonicalJson(IReadOnlyDictionary<string, object?> values)
    {
        byte[] ordinaryJson = JsonSerializer.SerializeToUtf8Bytes(values);
        byte[] canonical = Rfc8785CanonicalJson.Canonicalize(ordinaryJson);
        return ArtifactHasher.ComputeSha256(canonical);
    }
}
