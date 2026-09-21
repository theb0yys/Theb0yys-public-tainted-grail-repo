using System;
using System.Collections.Generic;
using System.Linq;
using Tainted.Armour.Canonical;
using Tainted.Armour.Serialization;

namespace Tainted.Armour.Compatibility;

public enum CompatibilityGateState
{
    Pass,
    Fail,
    Indeterminate,
}

public enum SourceTargetCompatibilityClassification
{
    Exact,
    SemanticEquivalent,
    StructuralEquivalent,
    TransferRequired,
    Unresolved,
    Incompatible,
}

public sealed class SourceCompatibilityProfileV1
{
    public SourceCompatibilityProfileV1(
        string profileId,
        CanonicalArmorManifestV1 manifest,
        CanonicalManifestArtifact manifestArtifact)
    {
        ProfileId = CompatibilityContractText.Require(profileId, nameof(profileId));
        Manifest = manifest ?? throw new ArgumentNullException(nameof(manifest));
        ManifestArtifact = manifestArtifact ?? throw new ArgumentNullException(nameof(manifestArtifact));
    }

    public string ProfileId { get; }
    public CanonicalArmorManifestV1 Manifest { get; }
    public CanonicalManifestArtifact ManifestArtifact { get; }
    public ArtifactId ManifestArtifactId => ManifestArtifact.ArtifactId;
}

public sealed class TargetCompatibilityProfileV1
{
    public TargetCompatibilityProfileV1(
        string targetProfileId,
        ArtifactId targetProfileFingerprint,
        CanonicalArmorManifestV1 manifest,
        CanonicalManifestArtifact manifestArtifact)
    {
        TargetProfileId = CompatibilityContractText.Require(targetProfileId, nameof(targetProfileId));
        TargetProfileFingerprint = targetProfileFingerprint;
        Manifest = manifest ?? throw new ArgumentNullException(nameof(manifest));
        ManifestArtifact = manifestArtifact ?? throw new ArgumentNullException(nameof(manifestArtifact));
    }

    public string TargetProfileId { get; }
    public ArtifactId TargetProfileFingerprint { get; }
    public CanonicalArmorManifestV1 Manifest { get; }
    public CanonicalManifestArtifact ManifestArtifact { get; }
    public ArtifactId ManifestArtifactId => ManifestArtifact.ArtifactId;
}

public sealed class SourceTargetCompatibilityPolicyV1
{
    public const string DefaultContractVersion = "tainted-armour.source-target-compatibility-policy/1";

    public SourceTargetCompatibilityPolicyV1(
        bool acceptSemanticEquivalence = true,
        bool acceptStructuralEquivalence = true)
    {
        AcceptSemanticEquivalence = acceptSemanticEquivalence;
        AcceptStructuralEquivalence = acceptStructuralEquivalence;
        Fingerprint = CompatibilityIdentity.HashCanonicalJson(new Dictionary<string, object?>
        {
            ["contractVersion"] = DefaultContractVersion,
            ["acceptSemanticEquivalence"] = acceptSemanticEquivalence,
            ["acceptStructuralEquivalence"] = acceptStructuralEquivalence,
        });
    }

    public string ContractVersion => DefaultContractVersion;
    public bool AcceptSemanticEquivalence { get; }
    public bool AcceptStructuralEquivalence { get; }
    public ArtifactId Fingerprint { get; }

    public static SourceTargetCompatibilityPolicyV1 CreateReportOnly() => new SourceTargetCompatibilityPolicyV1();
}

public sealed class SourceTargetCompatibilityRequestV1
{
    public SourceTargetCompatibilityRequestV1(
        SourceCompatibilityProfileV1 source,
        TargetCompatibilityProfileV1 target,
        SourceTargetCompatibilityPolicyV1? policy = null,
        bool candidateMapApplicationAllowed = false,
        bool candidateMapApplicationExecuted = false,
        bool conversionAllowed = false,
        bool conversionExecuted = false,
        bool itemEquipMutationAllowed = false,
        bool itemEquipMutationExecuted = false,
        bool saveMutationAllowed = false,
        bool saveMutationExecuted = false,
        bool downstreamWritesAllowed = false,
        bool downstreamWritesExecuted = false)
    {
        Source = source ?? throw new ArgumentNullException(nameof(source));
        Target = target ?? throw new ArgumentNullException(nameof(target));
        Policy = policy ?? SourceTargetCompatibilityPolicyV1.CreateReportOnly();
        CandidateMapApplicationAllowed = candidateMapApplicationAllowed;
        CandidateMapApplicationExecuted = candidateMapApplicationExecuted;
        ConversionAllowed = conversionAllowed;
        ConversionExecuted = conversionExecuted;
        ItemEquipMutationAllowed = itemEquipMutationAllowed;
        ItemEquipMutationExecuted = itemEquipMutationExecuted;
        SaveMutationAllowed = saveMutationAllowed;
        SaveMutationExecuted = saveMutationExecuted;
        DownstreamWritesAllowed = downstreamWritesAllowed;
        DownstreamWritesExecuted = downstreamWritesExecuted;
    }

    public SourceCompatibilityProfileV1 Source { get; }
    public TargetCompatibilityProfileV1 Target { get; }
    public SourceTargetCompatibilityPolicyV1 Policy { get; }
    public bool CandidateMapApplicationAllowed { get; }
    public bool CandidateMapApplicationExecuted { get; }
    public bool ConversionAllowed { get; }
    public bool ConversionExecuted { get; }
    public bool ItemEquipMutationAllowed { get; }
    public bool ItemEquipMutationExecuted { get; }
    public bool SaveMutationAllowed { get; }
    public bool SaveMutationExecuted { get; }
    public bool DownstreamWritesAllowed { get; }
    public bool DownstreamWritesExecuted { get; }
}

public sealed class SourceTargetCompatibilityReceiptV1
{
    public SourceTargetCompatibilityReceiptV1(
        ArtifactId receiptId,
        string stageContractVersion,
        CompatibilityGateState gate,
        SourceTargetCompatibilityClassification classification,
        ArtifactId sourceCanonicalArtifactId,
        ArtifactId targetCanonicalArtifactId,
        ArtifactId targetProfileFingerprint,
        ArtifactId policyFingerprint,
        IEnumerable<string> assertions,
        IEnumerable<string> diagnostics)
    {
        ReceiptId = receiptId;
        StageContractVersion = CompatibilityContractText.Require(stageContractVersion, nameof(stageContractVersion));
        Gate = gate;
        Classification = classification;
        SourceCanonicalArtifactId = sourceCanonicalArtifactId;
        TargetCanonicalArtifactId = targetCanonicalArtifactId;
        TargetProfileFingerprint = targetProfileFingerprint;
        PolicyFingerprint = policyFingerprint;
        this.assertions = CompatibilityContractText.Copy(assertions);
        this.diagnostics = CompatibilityContractText.Copy(diagnostics);
    }

    private readonly string[] assertions;
    private readonly string[] diagnostics;

    public ArtifactId ReceiptId { get; }
    public string StageContractVersion { get; }
    public CompatibilityGateState Gate { get; }
    public SourceTargetCompatibilityClassification Classification { get; }
    public ArtifactId SourceCanonicalArtifactId { get; }
    public ArtifactId TargetCanonicalArtifactId { get; }
    public ArtifactId TargetProfileFingerprint { get; }
    public ArtifactId PolicyFingerprint { get; }
    public IReadOnlyList<string> Assertions => assertions;
    public IReadOnlyList<string> Diagnostics => diagnostics;
}

public sealed class SourceTargetCompatibilityResultV1
{
    public SourceTargetCompatibilityResultV1(
        string stageContractVersion,
        CompatibilityGateState gate,
        SourceTargetCompatibilityClassification classification,
        ArtifactId cacheKey,
        SourceTargetCompatibilityReceiptV1 receipt,
        IEnumerable<string> blockers,
        IEnumerable<string> diagnostics)
    {
        StageContractVersion = CompatibilityContractText.Require(stageContractVersion, nameof(stageContractVersion));
        Gate = gate;
        Classification = classification;
        CacheKey = cacheKey;
        Receipt = receipt ?? throw new ArgumentNullException(nameof(receipt));
        this.blockers = CompatibilityContractText.Copy(blockers);
        this.diagnostics = CompatibilityContractText.Copy(diagnostics);
    }

    private readonly string[] blockers;
    private readonly string[] diagnostics;

    public string StageContractVersion { get; }
    public CompatibilityGateState Gate { get; }
    public SourceTargetCompatibilityClassification Classification { get; }
    public ArtifactId CacheKey { get; }
    public SourceTargetCompatibilityReceiptV1 Receipt { get; }
    public IReadOnlyList<string> Blockers => blockers;
    public IReadOnlyList<string> Diagnostics => diagnostics;
    public bool CompatibilityAccepted => Gate == CompatibilityGateState.Pass;
    public bool CandidateMapApplicationAllowed => false;
    public bool CandidateMapApplicationExecuted => false;
    public bool ConversionAllowed => false;
    public bool ConversionExecuted => false;
    public bool ItemEquipMutationAllowed => false;
    public bool ItemEquipMutationExecuted => false;
    public bool SaveMutationAllowed => false;
    public bool SaveMutationExecuted => false;
    public bool DownstreamWritesAllowed => false;
    public bool DownstreamWritesExecuted => false;
}

internal static class CompatibilityContractText
{
    public static string Require(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Value is required.", parameterName);
        return value.Trim();
    }

    public static string[] Copy(IEnumerable<string> values) =>
        values?.Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value.Trim()).ToArray()
        ?? Array.Empty<string>();
}
