using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Tainted.Armour.Canonical;

namespace Tainted.Armour.Compatibility;

public sealed class SourceTargetCompatibilityReceiptArtifactV1
{
    public const string SchemaVersionValue = "tainted-armour.source-target-compatibility-output/1";
    public const string ReceiptContractVersionValue = "tainted-armour.source-target-compatibility-receipt/1";

    public SourceTargetCompatibilityReceiptArtifactV1(
        string schemaVersion,
        string stageContractVersion,
        string receiptContractVersion,
        SourceTargetCompatibilityReceiptSourceV1 source,
        SourceTargetCompatibilityReceiptTargetV1 target,
        ArtifactId policyFingerprint,
        ArtifactId cacheKey,
        ArtifactId receiptId,
        string gate,
        string classification,
        bool compatibilityAccepted,
        IEnumerable<string> assertions,
        IEnumerable<string> diagnostics,
        IEnumerable<string> blockers,
        CompatibilityWriteBoundaryV1 boundaries)
    {
        SchemaVersion = CompatibilityContractText.Require(schemaVersion, nameof(schemaVersion));
        StageContractVersion = CompatibilityContractText.Require(stageContractVersion, nameof(stageContractVersion));
        ReceiptContractVersion = CompatibilityContractText.Require(receiptContractVersion, nameof(receiptContractVersion));
        Source = source ?? throw new ArgumentNullException(nameof(source));
        Target = target ?? throw new ArgumentNullException(nameof(target));
        PolicyFingerprint = policyFingerprint;
        CacheKey = cacheKey;
        ReceiptId = receiptId;
        Gate = CompatibilityContractText.Require(gate, nameof(gate));
        Classification = CompatibilityContractText.Require(classification, nameof(classification));
        CompatibilityAccepted = compatibilityAccepted;
        this.assertions = CompatibilityContractText.Copy(assertions);
        this.diagnostics = CompatibilityContractText.Copy(diagnostics);
        this.blockers = CompatibilityContractText.Copy(blockers);
        Boundaries = boundaries ?? throw new ArgumentNullException(nameof(boundaries));
    }

    private readonly string[] assertions;
    private readonly string[] diagnostics;
    private readonly string[] blockers;

    public string SchemaVersion { get; }
    public string StageContractVersion { get; }
    public string ReceiptContractVersion { get; }
    public SourceTargetCompatibilityReceiptSourceV1 Source { get; }
    public SourceTargetCompatibilityReceiptTargetV1 Target { get; }
    public ArtifactId PolicyFingerprint { get; }
    public ArtifactId CacheKey { get; }
    public ArtifactId ReceiptId { get; }
    public string Gate { get; }
    public string Classification { get; }
    public bool CompatibilityAccepted { get; }
    public IReadOnlyList<string> Assertions => assertions;
    public IReadOnlyList<string> Diagnostics => diagnostics;
    public IReadOnlyList<string> Blockers => blockers;
    public CompatibilityWriteBoundaryV1 Boundaries { get; }
}

public sealed class SourceTargetCompatibilityReceiptSourceV1
{
    public SourceTargetCompatibilityReceiptSourceV1(
        string artifactContractVersion,
        string profileId,
        string canonicalPackage,
        string sourceReceipt,
        ArtifactId canonicalArtifact)
    {
        ArtifactContractVersion = CompatibilityContractText.Require(artifactContractVersion, nameof(artifactContractVersion));
        ProfileId = CompatibilityContractText.Require(profileId, nameof(profileId));
        CanonicalPackage = CompatibilityContractText.Require(canonicalPackage, nameof(canonicalPackage));
        SourceReceipt = CompatibilityContractText.Require(sourceReceipt, nameof(sourceReceipt));
        CanonicalArtifact = canonicalArtifact;
    }

    public string ArtifactContractVersion { get; }
    public string ProfileId { get; }
    public string CanonicalPackage { get; }
    public string SourceReceipt { get; }
    public ArtifactId CanonicalArtifact { get; }
}

public sealed class SourceTargetCompatibilityReceiptTargetV1
{
    public SourceTargetCompatibilityReceiptTargetV1(
        string targetProfileContractVersion,
        string targetProfileId,
        ArtifactId targetProfileFingerprint,
        string canonicalPackage,
        ArtifactId canonicalArtifact)
    {
        TargetProfileContractVersion = CompatibilityContractText.Require(targetProfileContractVersion, nameof(targetProfileContractVersion));
        TargetProfileId = CompatibilityContractText.Require(targetProfileId, nameof(targetProfileId));
        TargetProfileFingerprint = targetProfileFingerprint;
        CanonicalPackage = CompatibilityContractText.Require(canonicalPackage, nameof(canonicalPackage));
        CanonicalArtifact = canonicalArtifact;
    }

    public string TargetProfileContractVersion { get; }
    public string TargetProfileId { get; }
    public ArtifactId TargetProfileFingerprint { get; }
    public string CanonicalPackage { get; }
    public ArtifactId CanonicalArtifact { get; }
}

public sealed class CompatibilityWriteBoundaryV1
{
    public CompatibilityWriteBoundaryV1(
        bool candidateMapApplicationAllowed,
        bool candidateMapApplicationExecuted,
        bool conversionAllowed,
        bool conversionExecuted,
        bool itemEquipMutationAllowed,
        bool itemEquipMutationExecuted,
        bool saveMutationAllowed,
        bool saveMutationExecuted,
        bool downstreamWritesAllowed,
        bool downstreamWritesExecuted)
    {
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
    public bool NoWriteBoundary =>
        !CandidateMapApplicationAllowed
        && !CandidateMapApplicationExecuted
        && !ConversionAllowed
        && !ConversionExecuted
        && !ItemEquipMutationAllowed
        && !ItemEquipMutationExecuted
        && !SaveMutationAllowed
        && !SaveMutationExecuted
        && !DownstreamWritesAllowed
        && !DownstreamWritesExecuted;

    public static CompatibilityWriteBoundaryV1 NoWrite() =>
        new CompatibilityWriteBoundaryV1(
            candidateMapApplicationAllowed: false,
            candidateMapApplicationExecuted: false,
            conversionAllowed: false,
            conversionExecuted: false,
            itemEquipMutationAllowed: false,
            itemEquipMutationExecuted: false,
            saveMutationAllowed: false,
            saveMutationExecuted: false,
            downstreamWritesAllowed: false,
            downstreamWritesExecuted: false);
}

public sealed class SourceTargetCompatibilityReceiptArtifactReader
{
    public SourceTargetCompatibilityReceiptArtifactV1 ReadFile(string receiptPath)
    {
        if (string.IsNullOrWhiteSpace(receiptPath))
        {
            throw new ArgumentException("Receipt path is required.", nameof(receiptPath));
        }

        string fullPath = Path.GetFullPath(receiptPath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException("Compatibility receipt artifact was not found.", fullPath);
        }

        return Read(File.ReadAllBytes(fullPath));
    }

    public SourceTargetCompatibilityReceiptArtifactV1 Read(ReadOnlySpan<byte> receiptJson)
    {
        using JsonDocument document = JsonDocument.Parse(receiptJson.ToArray(), new JsonDocumentOptions
        {
            AllowTrailingCommas = false,
            CommentHandling = JsonCommentHandling.Disallow,
            MaxDepth = 64,
        });

        JsonElement root = document.RootElement;
        JsonElement source = root.GetProperty("source");
        JsonElement target = root.GetProperty("target");
        JsonElement boundaries = root.GetProperty("boundaries");

        return new SourceTargetCompatibilityReceiptArtifactV1(
            RequireString(root, "schemaVersion"),
            RequireString(root, "stageContractVersion"),
            RequireString(root, "receiptContractVersion"),
            new SourceTargetCompatibilityReceiptSourceV1(
                RequireString(source, "artifactContractVersion"),
                RequireString(source, "profileId"),
                RequireString(source, "canonicalPackage"),
                RequireString(source, "sourceReceipt"),
                ReadArtifactId(source, "canonicalArtifact")),
            new SourceTargetCompatibilityReceiptTargetV1(
                RequireString(target, "targetProfileContractVersion"),
                RequireString(target, "targetProfileId"),
                ReadArtifactId(target, "targetProfileFingerprint"),
                RequireString(target, "canonicalPackage"),
                ReadArtifactId(target, "canonicalArtifact")),
            ReadArtifactId(root, "policyFingerprint"),
            ReadArtifactId(root, "cacheKey"),
            ReadArtifactId(root, "receiptId"),
            RequireString(root, "gate"),
            RequireString(root, "classification"),
            root.GetProperty("compatibilityAccepted").GetBoolean(),
            ReadStringArray(root, "assertions"),
            ReadStringArray(root, "diagnostics"),
            ReadStringArray(root, "blockers"),
            new CompatibilityWriteBoundaryV1(
                boundaries.GetProperty("candidateMapApplicationAllowed").GetBoolean(),
                boundaries.GetProperty("candidateMapApplicationExecuted").GetBoolean(),
                boundaries.GetProperty("conversionAllowed").GetBoolean(),
                boundaries.GetProperty("conversionExecuted").GetBoolean(),
                boundaries.GetProperty("itemEquipMutationAllowed").GetBoolean(),
                boundaries.GetProperty("itemEquipMutationExecuted").GetBoolean(),
                boundaries.GetProperty("saveMutationAllowed").GetBoolean(),
                boundaries.GetProperty("saveMutationExecuted").GetBoolean(),
                boundaries.GetProperty("downstreamWritesAllowed").GetBoolean(),
                boundaries.GetProperty("downstreamWritesExecuted").GetBoolean()));
    }

    private static ArtifactId ReadArtifactId(JsonElement element, string name) =>
        ArtifactId.Parse(RequireString(element, name));

    private static string[] ReadStringArray(JsonElement element, string name) =>
        element.GetProperty(name).EnumerateArray()
            .Select(value => value.GetString() ?? string.Empty)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .ToArray();

    private static string RequireString(JsonElement element, string name)
    {
        string? value = element.GetProperty(name).GetString();
        if (string.IsNullOrWhiteSpace(value)) throw new InvalidDataException($"String property '{name}' is required.");
        return value.Trim();
    }
}

public sealed class CompatibilityReceiptNoWriteConsumer
{
    public CompatibilityReceiptConsumerDecisionV1 Evaluate(SourceTargetCompatibilityReceiptArtifactV1 receipt)
    {
        if (receipt == null) throw new ArgumentNullException(nameof(receipt));

        var blockers = new List<string>();
        if (receipt.SchemaVersion != SourceTargetCompatibilityReceiptArtifactV1.SchemaVersionValue) blockers.Add("unsupported_receipt_schema");
        if (receipt.StageContractVersion != SourceTargetCompatibilityStage.StageContractVersion) blockers.Add("unsupported_stage_contract");
        if (receipt.ReceiptContractVersion != SourceTargetCompatibilityReceiptArtifactV1.ReceiptContractVersionValue) blockers.Add("unsupported_receipt_contract");
        if (receipt.Gate != "PASS") blockers.Add("receipt_gate_not_pass");
        if (receipt.Classification != "EXACT") blockers.Add("receipt_classification_not_exact");
        if (!receipt.CompatibilityAccepted) blockers.Add("receipt_not_accepted");
        if (receipt.Blockers.Count != 0) blockers.Add("receipt_blockers_present");
        if (!receipt.Boundaries.NoWriteBoundary) blockers.Add("receipt_no_write_boundary_not_false");

        string decision = blockers.Count == 0
            ? CompatibilityReceiptConsumerDecisionV1.AcceptNoWriteDecision
            : CompatibilityReceiptConsumerDecisionV1.BlockNoWriteDecision;
        string[] diagnostics = blockers.Count == 0
            ? new[] { "receipt_consumer_ready:exact_pass_no_write" }
            : blockers.Select(value => "receipt_consumer_blocked:" + value).ToArray();

        return new CompatibilityReceiptConsumerDecisionV1(
            receipt.ReceiptId,
            receipt.CacheKey,
            decision,
            diagnostics,
            blockers);
    }
}

public sealed class CompatibilityReceiptConsumerDecisionV1
{
    public const string ContractVersionValue = "tainted-armour.compatibility-receipt-consumer.no-write/1";
    public const string AcceptNoWriteDecision = "ACCEPT_NO_WRITE_COMPATIBILITY_RECEIPT";
    public const string BlockNoWriteDecision = "BLOCK_NO_WRITE_COMPATIBILITY_RECEIPT";

    public CompatibilityReceiptConsumerDecisionV1(
        ArtifactId sourceReceiptId,
        ArtifactId sourceCacheKey,
        string decision,
        IEnumerable<string> diagnostics,
        IEnumerable<string> blockers)
    {
        SourceReceiptId = sourceReceiptId;
        SourceCacheKey = sourceCacheKey;
        Decision = CompatibilityContractText.Require(decision, nameof(decision));
        this.diagnostics = CompatibilityContractText.Copy(diagnostics);
        this.blockers = CompatibilityContractText.Copy(blockers);
    }

    private readonly string[] diagnostics;
    private readonly string[] blockers;

    public string ConsumerContractVersion => ContractVersionValue;
    public ArtifactId SourceReceiptId { get; }
    public ArtifactId SourceCacheKey { get; }
    public string Decision { get; }
    public bool Accepted => Decision == AcceptNoWriteDecision;
    public IReadOnlyList<string> Diagnostics => diagnostics;
    public IReadOnlyList<string> Blockers => blockers;
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
