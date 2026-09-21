using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Tainted.Armour.Canonical;
using Tainted.Armour.Compatibility;
using Tainted.Armour.Serialization;

namespace Tainted.Armour.Compatibility.Fixtures;

internal static class Program
{
    private const string AdapterContract = "com.tainted.armour.compatibility-fixture/1";
    private const string CanonicalT56RealSampleReceipt = "t56-dragon-knight-iron-no-cape-full-geometry-import.json";
    private const string CanonicalT56SourceReceiptPath = @"..\..\" + CanonicalT56RealSampleReceipt;
    private const string CompatibilityReceiptArtifact = "compatibility.receipt.json";
    private const string RetiredT56NonFullReceipt = "t56-dragon-knight-iron-no-cape-import.json";
    private const string RealSampleImportMarker = "TAINTED_ARMOUR_REAL_SAMPLE_IMPORT_EXECUTED_DEFORMATION_BLOCKED_DOWNSTREAM_FALSE";
    private const string FullGeometryCaptureMarker = "TAINTED_ARMOUR_REAL_SAMPLE_FULL_GEOMETRY_CAPTURE_READY_DOWNSTREAM_FALSE";

    private static int Main()
    {
        var tests = new (string Name, Action Execute)[]
        {
            ("exact canonical artifacts pass without downstream authorisation", ExactCanonicalArtifactsPassWithoutDownstreamAuthorisation),
            ("structural equivalent artifacts pass without conversion", StructuralEquivalentArtifactsPassWithoutConversion),
            ("transfer required is indeterminate and no-write", TransferRequiredIsIndeterminateAndNoWrite),
            ("manifest artifact mismatch fails closed", ManifestArtifactMismatchFailsClosed),
            ("downstream boundary request fails closed", DownstreamBoundaryRequestFailsClosed),
            ("cache and receipt identity repeat deterministically", CacheAndReceiptIdentityRepeatDeterministically),
            ("input adapter creates request from real R2 artifact and fingerprinted target profile", InputAdapterCreatesRequest),
            ("input adapter rejects non-R2 artifact", InputAdapterRejectsNonR2Artifact),
            ("target profile fingerprint affects compatibility cache identity", TargetProfileFingerprintAffectsCacheIdentity),
            ("T56 target profile uses canonical refreshed full-geometry receipt", T56TargetProfileUsesCanonicalFullGeometryReceipt),
            ("persisted real-armour R2 package feeds adapter and compatibility stage", PersistedRealArmourPackageFeedsAdapterAndStage),
            ("persisted real-armour compatibility receipt matches production result", PersistedRealArmourCompatibilityReceiptMatchesProductionResult),
            ("compatibility receipt consumer preserves no-write decision", CompatibilityReceiptConsumerPreservesNoWriteDecision),
            ("compatibility receipt consumer blocks non-no-write receipt", CompatibilityReceiptConsumerBlocksNonNoWriteReceipt),
        };

        var failures = new List<string>();
        foreach ((string name, Action execute) in tests)
        {
            try
            {
                execute();
                Console.WriteLine("PASS " + name);
            }
            catch (Exception exception)
            {
                failures.Add(name + ": " + exception.Message);
                Console.WriteLine("FAIL " + name + ": " + exception.Message);
            }
        }

        if (failures.Count != 0)
        {
            Console.WriteLine("TAINTED_ARMOUR_COMPATIBILITY_FIXTURES_FAILED count=" + failures.Count);
            return 1;
        }

        Console.WriteLine("TAINTED_ARMOUR_COMPATIBILITY_FIXTURES_PASS count=" + tests.Length);
        return 0;
    }

    private static void ExactCanonicalArtifactsPassWithoutDownstreamAuthorisation()
    {
        FixtureManifest source = BuildManifest("asset/exact", "body", 3, boundsMaximumY: 1d);
        SourceTargetCompatibilityResultV1 result = Evaluate(source, source);

        Check(result.Gate == CompatibilityGateState.Pass, "Exact source/target artifact should pass the compatibility-report gate.");
        Check(result.Classification == SourceTargetCompatibilityClassification.Exact, "Same canonical artifact should classify as EXACT.");
        Check(result.Blockers.Count == 0, "Exact source/target artifact should not emit blockers.");
        CheckNoDownstreamAuthorisation(result);
    }

    private static void StructuralEquivalentArtifactsPassWithoutConversion()
    {
        FixtureManifest source = BuildManifest("asset/structural-source", "body", 3, boundsMaximumY: 1d);
        FixtureManifest target = BuildManifest("asset/structural-target", "body", 3, boundsMaximumY: 2d);
        Check(source.Artifact.ArtifactId != target.Artifact.ArtifactId, "Fixture must exercise different canonical artifact identities.");

        SourceTargetCompatibilityResultV1 result = Evaluate(source, target);

        Check(result.Gate == CompatibilityGateState.Pass, "Structurally equivalent source/target profiles should pass the report gate.");
        Check(result.Classification == SourceTargetCompatibilityClassification.StructuralEquivalent, "Expected STRUCTURAL_EQUIVALENT classification.");
        Check(result.Diagnostics.Contains("classification_structural_equivalent:matched_mesh_names_and_structure"), "Structural evidence diagnostic was not retained.");
        CheckNoDownstreamAuthorisation(result);
    }

    private static void TransferRequiredIsIndeterminateAndNoWrite()
    {
        FixtureManifest source = BuildManifest("asset/transfer-source", "body", 3, boundsMaximumY: 1d);
        FixtureManifest target = BuildManifest("asset/transfer-target", "body", 4, boundsMaximumY: 1d);

        SourceTargetCompatibilityResultV1 result = Evaluate(source, target);

        Check(result.Gate == CompatibilityGateState.Indeterminate, "Different mesh structure should require an adaptation plan before mutation.");
        Check(result.Classification == SourceTargetCompatibilityClassification.TransferRequired, "Expected TRANSFER_REQUIRED classification.");
        Check(result.Blockers.Contains("adaptation_plan_required_before_mutation"), "Missing adaptation-plan blocker.");
        CheckNoDownstreamAuthorisation(result);
    }

    private static void ManifestArtifactMismatchFailsClosed()
    {
        FixtureManifest source = BuildManifest("asset/mismatch-source", "body", 3, boundsMaximumY: 1d);
        FixtureManifest target = BuildManifest("asset/mismatch-target", "body", 3, boundsMaximumY: 1d);
        CanonicalManifestArtifact wrongSourceArtifact = new CanonicalManifestArtifact(
            source.Artifact.Bytes,
            ArtifactHasher.ComputeSha256(Encoding.UTF8.GetBytes("not the source manifest")));

        var request = new SourceTargetCompatibilityRequestV1(
            new SourceCompatibilityProfileV1("source.mismatch", source.Manifest, wrongSourceArtifact),
            CreateTarget(target));
        SourceTargetCompatibilityResultV1 result = new SourceTargetCompatibilityStage().Evaluate(request);

        Check(result.Gate == CompatibilityGateState.Fail, "Mismatched R2 artifact identity must fail closed.");
        Check(result.Blockers.Any(value => value.StartsWith("r2_source_manifest_artifact_mismatch:", StringComparison.Ordinal)), "Missing R2 artifact mismatch blocker.");
        CheckNoDownstreamAuthorisation(result);
    }

    private static void DownstreamBoundaryRequestFailsClosed()
    {
        FixtureManifest source = BuildManifest("asset/boundary-source", "body", 3, boundsMaximumY: 1d);
        FixtureManifest target = BuildManifest("asset/boundary-target", "body", 3, boundsMaximumY: 1d);
        var request = new SourceTargetCompatibilityRequestV1(
            CreateSource(source),
            CreateTarget(target),
            conversionAllowed: true);

        SourceTargetCompatibilityResultV1 result = new SourceTargetCompatibilityStage().Evaluate(request);

        Check(result.Gate == CompatibilityGateState.Fail, "Requested conversion allowance must fail the no-write boundary.");
        Check(result.Blockers.Contains("conversion_allowed_not_false"), "Missing conversion boundary blocker.");
        CheckNoDownstreamAuthorisation(result);
    }

    private static void CacheAndReceiptIdentityRepeatDeterministically()
    {
        FixtureManifest source = BuildManifest("asset/repeat-source", "body", 3, boundsMaximumY: 1d);
        FixtureManifest target = BuildManifest("asset/repeat-target", "body", 3, boundsMaximumY: 2d);

        SourceTargetCompatibilityResultV1 first = Evaluate(source, target);
        SourceTargetCompatibilityResultV1 second = Evaluate(source, target);

        Check(first.CacheKey == second.CacheKey, "Repeated compatibility evaluation changed cache identity.");
        Check(first.Receipt.ReceiptId == second.Receipt.ReceiptId, "Repeated compatibility evaluation changed receipt identity.");
    }

    private static void InputAdapterCreatesRequest()
    {
        FixtureManifest source = BuildManifest("asset/adapter-source", "body", 3, boundsMaximumY: 1d);
        FixtureManifest target = BuildManifest("asset/adapter-target", "body", 3, boundsMaximumY: 2d);
        ArtifactId targetProfileFingerprint = ArtifactHasher.ComputeSha256(Encoding.UTF8.GetBytes("target-profile:adapter-target:v1"));
        var adapter = new CompatibilityInputAdapter();

        SourceTargetCompatibilityRequestV1 request = adapter.CreateRequest(
            new R2CanonicalArmorArtifactV1("source.adapter", source.Manifest, source.Artifact),
            new FingerprintedTargetProfileV1(
                "target.adapter",
                targetProfileFingerprint,
                new R2CanonicalArmorArtifactV1("target.adapter.canonical", target.Manifest, target.Artifact)));
        SourceTargetCompatibilityResultV1 result = new SourceTargetCompatibilityStage().Evaluate(request);

        Check(request.Source.ProfileId == "source.adapter", "Adapter did not preserve the source profile ID.");
        Check(request.Target.TargetProfileId == "target.adapter", "Adapter did not preserve the target profile ID.");
        Check(request.Target.TargetProfileFingerprint == targetProfileFingerprint, "Adapter did not preserve the target profile fingerprint.");
        Check(!request.CandidateMapApplicationAllowed && !request.CandidateMapApplicationExecuted, "Adapter must keep candidate-map application false.");
        Check(!request.ConversionAllowed && !request.ConversionExecuted, "Adapter must keep conversion false.");
        Check(!request.ItemEquipMutationAllowed && !request.ItemEquipMutationExecuted, "Adapter must keep item/equip mutation false.");
        Check(!request.SaveMutationAllowed && !request.SaveMutationExecuted, "Adapter must keep save mutation false.");
        Check(!request.DownstreamWritesAllowed && !request.DownstreamWritesExecuted, "Adapter must keep downstream writes false.");
        Check(result.Gate == CompatibilityGateState.Pass, "Adapter-created request should reach the compatibility stage.");
        CheckNoDownstreamAuthorisation(result);
    }

    private static void InputAdapterRejectsNonR2Artifact()
    {
        FixtureManifest source = BuildManifest("asset/adapter-reject-source", "body", 3, boundsMaximumY: 1d);
        CanonicalManifestArtifact mismatched = new CanonicalManifestArtifact(
            source.Artifact.Bytes,
            ArtifactHasher.ComputeSha256(Encoding.UTF8.GetBytes("mismatched-r2-artifact")));

        CheckThrows<ArgumentException>(
            () => new R2CanonicalArmorArtifactV1("source.bad", source.Manifest, mismatched),
            "Adapter accepted a canonical artifact whose identity does not match R2 canonical bytes.");
    }

    private static void TargetProfileFingerprintAffectsCacheIdentity()
    {
        FixtureManifest source = BuildManifest("asset/fingerprint-source", "body", 3, boundsMaximumY: 1d);
        FixtureManifest target = BuildManifest("asset/fingerprint-target", "body", 3, boundsMaximumY: 2d);
        var adapter = new CompatibilityInputAdapter();
        var sourceArtifact = new R2CanonicalArmorArtifactV1("source.fingerprint", source.Manifest, source.Artifact);
        var targetArtifact = new R2CanonicalArmorArtifactV1("target.fingerprint.canonical", target.Manifest, target.Artifact);
        SourceTargetCompatibilityRequestV1 firstRequest = adapter.CreateRequest(
            sourceArtifact,
            new FingerprintedTargetProfileV1(
                "target.fingerprint",
                ArtifactHasher.ComputeSha256(Encoding.UTF8.GetBytes("target-profile:fingerprint:a")),
                targetArtifact));
        SourceTargetCompatibilityRequestV1 secondRequest = adapter.CreateRequest(
            sourceArtifact,
            new FingerprintedTargetProfileV1(
                "target.fingerprint",
                ArtifactHasher.ComputeSha256(Encoding.UTF8.GetBytes("target-profile:fingerprint:b")),
                targetArtifact));

        SourceTargetCompatibilityResultV1 first = new SourceTargetCompatibilityStage().Evaluate(firstRequest);
        SourceTargetCompatibilityResultV1 second = new SourceTargetCompatibilityStage().Evaluate(secondRequest);

        Check(first.CacheKey != second.CacheKey, "Target profile fingerprint must participate in compatibility cache identity.");
        Check(first.Receipt.ReceiptId != second.Receipt.ReceiptId, "Target profile fingerprint must participate in receipt identity.");
    }

    private static void T56TargetProfileUsesCanonicalFullGeometryReceipt()
    {
        string packageRoot = ResolveFixturePath(
            "mods",
            "tainted-armour",
            "tests",
            "real-samples",
            "r2-canonical",
            "t56-dragon-knight-iron-no-cape");
        string profilePath = Path.Combine(packageRoot, "target-profile.json");
        using JsonDocument profileDocument = JsonDocument.Parse(File.ReadAllBytes(profilePath));
        string sourceReceipt = profileDocument.RootElement.GetProperty("sourceReceipt").GetString() ?? string.Empty;
        string normalizedReceipt = NormalizeFixturePath(sourceReceipt);

        Check(
            normalizedReceipt == CanonicalT56SourceReceiptPath,
            "T56 target profile must consume the refreshed full-geometry receipt.");
        Check(
            !normalizedReceipt.EndsWith(RetiredT56NonFullReceipt, StringComparison.Ordinal),
            "T56 target profile must not consume the retired non-full receipt.");

        string receiptPath = Path.GetFullPath(Path.Combine(packageRoot, sourceReceipt));
        using JsonDocument receiptDocument = JsonDocument.Parse(File.ReadAllBytes(receiptPath));
        JsonElement root = receiptDocument.RootElement;
        Check(root.GetProperty("marker").GetString() == RealSampleImportMarker, "Canonical T56 receipt marker changed.");
        Check(root.GetProperty("fullGeometryCaptureWritten").GetBoolean(), "Canonical T56 receipt must include full-geometry capture.");
        Check(root.GetProperty("fullGeometryCaptureMarker").GetString() == FullGeometryCaptureMarker, "Canonical T56 full-geometry marker changed.");

        JsonElement rows = root.GetProperty("rows");
        Check(rows.GetArrayLength() == 6, "Canonical T56 receipt must retain six real-sample rows.");
        foreach (JsonElement row in rows.EnumerateArray())
        {
            Check(row.GetProperty("controlsPassed").GetBoolean(), "Canonical T56 row controls must pass.");
            Check(row.GetProperty("controlFailureCount").GetInt32() == 0, "Canonical T56 row control failure count must be zero.");
            Check(row.GetProperty("outOfPlaneRigidRotationDiagnosticFailed").GetBoolean(), "Canonical T56 row must retain out-of-plane diagnostic warning state.");
            Check(!row.GetProperty("metricCalibrationFailed").GetBoolean(), "Canonical T56 row must not hard-fail metric calibration.");
            Check(!row.GetProperty("visualRuntimeRequested").GetBoolean(), "Canonical T56 Unity receipt must not fabricate an A2K-T34 visual-runtime request.");
            CheckReceiptRowNoDownstreamWrites(row);
        }

        JsonElement summary = root.GetProperty("summary");
        Check(summary.GetProperty("importInvocationCount").GetInt32() == 6, "Canonical T56 receipt import count changed.");
        Check(summary.GetProperty("totalOrientationReversedTriangles").GetInt32() == 444, "Canonical T56 receipt reversed-triangle total changed.");
        Check(summary.GetProperty("totalCollapsedTriangles").GetInt32() == 0, "Canonical T56 receipt collapsed-triangle total changed.");
        Check(summary.GetProperty("rowParityWithT56").GetBoolean(), "Canonical T56 receipt lost T56 row parity.");
        Check(summary.GetProperty("productionImporterFlowVerified").GetBoolean(), "Canonical T56 production importer flow is not verified.");
        Check(summary.GetProperty("metricCalibrationBoundaryVerified").GetBoolean(), "Canonical T56 metric calibration boundary is not verified.");
        Check(summary.GetProperty("candidateSupplementalMetricBoundaryVerified").GetBoolean(), "Canonical T56 candidate supplemental metric boundary is not verified.");
        Check(summary.GetProperty("visualRuntimeObserverBoundaryVerified").GetBoolean(), "Canonical T56 visual-runtime observer boundary is not verified.");
        Check(summary.GetProperty("candidateMapAndDownstreamResultsFalse").GetBoolean(), "Canonical T56 candidate-map/downstream result boundary is not false.");
        Check(summary.GetProperty("everyDownstreamWriteFalse").GetBoolean(), "Canonical T56 downstream write boundary is not false.");
        Check(root.GetProperty("blockers").GetArrayLength() == 0, "Canonical T56 receipt-level blockers changed.");
        Check(root.GetProperty("errors").GetArrayLength() == 0, "Canonical T56 receipt-level errors changed.");
        CheckReceiptDownstreamNoWrites(root.GetProperty("downstream"));
    }

    private static void PersistedRealArmourPackageFeedsAdapterAndStage()
    {
        string packageRoot = ResolveFixturePath(
            "mods",
            "tainted-armour",
            "tests",
            "real-samples",
            "r2-canonical",
            "t56-dragon-knight-iron-no-cape");
        PersistedCompatibilityEvaluation evaluation = EvaluatePersistedRealArmourCompatibility(packageRoot);
        R2CanonicalArmorArtifactV1 sourceArtifact = evaluation.SourceArtifact;
        SourceTargetCompatibilityRequestV1 request = evaluation.Request;
        SourceTargetCompatibilityResultV1 result = evaluation.Result;
        CanonicalMeshV1 mesh = sourceArtifact.Manifest.Meshes.Values.Single();
        CanonicalAccessorV1 positionAccessor = sourceArtifact.Manifest.Accessors.Values.Single(value =>
            value.ComponentType == CanonicalComponentType.F32 && value.ElementShape == CanonicalElementShape.Vec3);
        CanonicalAccessorV1 indexAccessor = sourceArtifact.Manifest.Accessors.Values.Single(value => value.ComponentType == CanonicalComponentType.U32);
        CanonicalAccessorV1 matrixAccessor = sourceArtifact.Manifest.Accessors.Values.Single(value => value.ElementShape == CanonicalElementShape.Mat4);
        long[] bufferLengths = sourceArtifact.Manifest.Buffers.Values.Select(value => value.ByteLength).OrderBy(value => value).ToArray();

        Check(sourceArtifact.ManifestArtifactId.ToString() == "sha256:b7b8c898cbce4ed250e920c37a241000c55dc94f094bdd53ea5c147fd809e49f", "Persisted package manifest identity changed.");
        Check(sourceArtifact.Manifest.Buffers.Count == 3, "Persisted package must contain position, index, and bind-matrix buffers.");
        Check(sourceArtifact.Manifest.BufferViews.Count == 3, "Persisted package must contain position, index, and bind-matrix buffer views.");
        Check(sourceArtifact.Manifest.Accessors.Values.All(value => value.BufferViewId.HasValue), "Full-geometry accessors must reference dense buffer views.");
        Check(sourceArtifact.Manifest.Accessors.Values.All(value => value.Sparse == null), "Full-geometry package must not use sparse or implicit-zero accessor payloads.");
        Check(bufferLengths.SequenceEqual(new long[] { 64, 307920, 517908 }), "Persisted package blob byte lengths changed.");
        Check(mesh.Name == "UE5_no_cape", "Persisted package did not retain the real sample mesh name.");
        Check(mesh.VertexCount == 43159, "Persisted package did not retain the real sample vertex count.");
        Check(positionAccessor.Count == 43159, "Persisted package did not retain the real sample position accessor count.");
        Check(indexAccessor.Count == 76980, "Persisted package did not retain the real sample submesh index count.");
        Check(matrixAccessor.Count == 1, "Persisted package did not retain one bind-matrix accessor.");
        Check(request.Source.ManifestArtifactId == sourceArtifact.ManifestArtifactId, "Adapter did not preserve persisted source package identity.");
        Check(request.Target.TargetProfileFingerprint.ToString() == "sha256:b8cc6f6aa56d8606726d07292b78dd35d104da050e0c8c0a18b172c8dcb94ab5", "Adapter did not preserve persisted target profile fingerprint.");
        Check(result.Gate == CompatibilityGateState.Pass, "Persisted package should pass exact source/target compatibility reporting.");
        Check(result.Classification == SourceTargetCompatibilityClassification.Exact, "Persisted self-target package should classify as EXACT.");
        CheckNoDownstreamAuthorisation(result);
    }

    private static void PersistedRealArmourCompatibilityReceiptMatchesProductionResult()
    {
        string packageRoot = ResolveFixturePath(
            "mods",
            "tainted-armour",
            "tests",
            "real-samples",
            "r2-canonical",
            "t56-dragon-knight-iron-no-cape");
        PersistedCompatibilityEvaluation evaluation = EvaluatePersistedRealArmourCompatibility(packageRoot);

        CheckPersistedCompatibilityReceipt(
            packageRoot,
            evaluation.SourceArtifact,
            evaluation.TargetProfile,
            evaluation.Result);
    }

    private static void CompatibilityReceiptConsumerPreservesNoWriteDecision()
    {
        string packageRoot = ResolveFixturePath(
            "mods",
            "tainted-armour",
            "tests",
            "real-samples",
            "r2-canonical",
            "t56-dragon-knight-iron-no-cape");
        PersistedCompatibilityEvaluation evaluation = EvaluatePersistedRealArmourCompatibility(packageRoot);
        string receiptPath = Path.Combine(packageRoot, CompatibilityReceiptArtifact);
        SourceTargetCompatibilityReceiptArtifactV1 receipt = new SourceTargetCompatibilityReceiptArtifactReader().ReadFile(receiptPath);
        CompatibilityReceiptConsumerDecisionV1 decision = new CompatibilityReceiptNoWriteConsumer().Evaluate(receipt);

        Check(receipt.ReceiptId == evaluation.Result.Receipt.ReceiptId, "Consumer receipt ID drifted from production result.");
        Check(receipt.CacheKey == evaluation.Result.CacheKey, "Consumer cache key drifted from production result.");
        Check(receipt.Gate == GateText(evaluation.Result.Gate), "Consumer gate drifted from production result.");
        Check(receipt.Classification == ClassificationText(evaluation.Result.Classification), "Consumer classification drifted from production result.");
        Check(receipt.CompatibilityAccepted == evaluation.Result.CompatibilityAccepted, "Consumer acceptance drifted from production result.");
        Check(receipt.Source.CanonicalArtifact == evaluation.SourceArtifact.ManifestArtifactId, "Consumer source artifact drifted.");
        Check(receipt.Target.CanonicalArtifact == evaluation.TargetProfile.CanonicalArtifact.ManifestArtifactId, "Consumer target artifact drifted.");
        Check(receipt.Target.TargetProfileFingerprint == evaluation.TargetProfile.TargetProfileFingerprint, "Consumer target fingerprint drifted.");

        Check(decision.ConsumerContractVersion == CompatibilityReceiptConsumerDecisionV1.ContractVersionValue, "Consumer decision contract changed.");
        Check(decision.SourceReceiptId == receipt.ReceiptId, "Consumer decision did not retain source receipt identity.");
        Check(decision.SourceCacheKey == receipt.CacheKey, "Consumer decision did not retain source cache key.");
        Check(decision.Decision == CompatibilityReceiptConsumerDecisionV1.AcceptNoWriteDecision, "Consumer decision changed.");
        Check(decision.Accepted, "Consumer decision did not accept the exact no-write receipt.");
        Check(decision.CandidateMapApplicationAllowed == false, "Consumer must not allow candidate-map application.");
        Check(decision.CandidateMapApplicationExecuted == false, "Consumer must not execute candidate-map application.");
        Check(decision.ConversionAllowed == false, "Consumer must not allow conversion.");
        Check(decision.ConversionExecuted == false, "Consumer must not execute conversion.");
        Check(decision.ItemEquipMutationAllowed == false, "Consumer must not allow item/equip mutation.");
        Check(decision.ItemEquipMutationExecuted == false, "Consumer must not execute item/equip mutation.");
        Check(decision.SaveMutationAllowed == false, "Consumer must not allow save mutation.");
        Check(decision.SaveMutationExecuted == false, "Consumer must not execute save mutation.");
        Check(decision.DownstreamWritesAllowed == false, "Consumer must not allow downstream writes.");
        Check(decision.DownstreamWritesExecuted == false, "Consumer must not execute downstream writes.");
        Check(decision.Diagnostics.SequenceEqual(new[] { "receipt_consumer_ready:exact_pass_no_write" }), "Consumer diagnostics changed.");
        Check(decision.Blockers.Count == 0, "Consumer must not report blockers for the exact no-write receipt.");
    }

    private static void CompatibilityReceiptConsumerBlocksNonNoWriteReceipt()
    {
        FixtureManifest source = BuildManifest("asset/receipt-consumer-block-source", "body", 3, boundsMaximumY: 1d);
        SourceTargetCompatibilityResultV1 result = Evaluate(source, source);
        var receipt = new SourceTargetCompatibilityReceiptArtifactV1(
            SourceTargetCompatibilityReceiptArtifactV1.SchemaVersionValue,
            result.StageContractVersion,
            SourceTargetCompatibilityReceiptArtifactV1.ReceiptContractVersionValue,
            new SourceTargetCompatibilityReceiptSourceV1(
                R2CanonicalArmorArtifactV1.ContractVersionValue,
                "source.receipt-consumer-block",
                ".",
                CanonicalT56SourceReceiptPath,
                result.Receipt.SourceCanonicalArtifactId),
            new SourceTargetCompatibilityReceiptTargetV1(
                FingerprintedTargetProfileV1.ContractVersionValue,
                "target.receipt-consumer-block",
                result.Receipt.TargetProfileFingerprint,
                ".",
                result.Receipt.TargetCanonicalArtifactId),
            result.Receipt.PolicyFingerprint,
            result.CacheKey,
            result.Receipt.ReceiptId,
            GateText(result.Gate),
            ClassificationText(result.Classification),
            result.CompatibilityAccepted,
            result.Receipt.Assertions,
            result.Diagnostics,
            result.Blockers,
            new CompatibilityWriteBoundaryV1(
                candidateMapApplicationAllowed: false,
                candidateMapApplicationExecuted: false,
                conversionAllowed: true,
                conversionExecuted: false,
                itemEquipMutationAllowed: false,
                itemEquipMutationExecuted: false,
                saveMutationAllowed: false,
                saveMutationExecuted: false,
                downstreamWritesAllowed: false,
                downstreamWritesExecuted: false));

        CompatibilityReceiptConsumerDecisionV1 decision = new CompatibilityReceiptNoWriteConsumer().Evaluate(receipt);

        Check(decision.Decision == CompatibilityReceiptConsumerDecisionV1.BlockNoWriteDecision, "Consumer must block non-no-write receipts.");
        Check(!decision.Accepted, "Consumer must not accept non-no-write receipts.");
        Check(decision.Blockers.SequenceEqual(new[] { "receipt_no_write_boundary_not_false" }), "Consumer did not report the no-write boundary blocker.");
        Check(decision.Diagnostics.SequenceEqual(new[] { "receipt_consumer_blocked:receipt_no_write_boundary_not_false" }), "Consumer block diagnostics changed.");
        Check(decision.CandidateMapApplicationAllowed == false, "Blocked consumer decision must not allow candidate-map application.");
        Check(decision.CandidateMapApplicationExecuted == false, "Blocked consumer decision must not execute candidate-map application.");
        Check(decision.ConversionAllowed == false, "Blocked consumer decision must not allow conversion.");
        Check(decision.ConversionExecuted == false, "Blocked consumer decision must not execute conversion.");
        Check(decision.ItemEquipMutationAllowed == false, "Blocked consumer decision must not allow item/equip mutation.");
        Check(decision.ItemEquipMutationExecuted == false, "Blocked consumer decision must not execute item/equip mutation.");
        Check(decision.SaveMutationAllowed == false, "Blocked consumer decision must not allow save mutation.");
        Check(decision.SaveMutationExecuted == false, "Blocked consumer decision must not execute save mutation.");
        Check(decision.DownstreamWritesAllowed == false, "Blocked consumer decision must not allow downstream writes.");
        Check(decision.DownstreamWritesExecuted == false, "Blocked consumer decision must not execute downstream writes.");
    }

    private static PersistedCompatibilityEvaluation EvaluatePersistedRealArmourCompatibility(string packageRoot)
    {
        var packageReader = new PersistedR2CanonicalPackageReader();
        R2CanonicalArmorArtifactV1 sourceArtifact = packageReader.ReadPackage(packageRoot, "source.t56-dragon-knight-iron-no-cape");
        R2CanonicalArmorArtifactV1 targetArtifact = packageReader.ReadPackage(packageRoot, "target.t56-dragon-knight-iron-no-cape");
        FingerprintedTargetProfileV1 targetProfile = ReadTargetProfile(packageRoot, targetArtifact);
        SourceTargetCompatibilityRequestV1 request = new CompatibilityInputAdapter().CreateRequest(sourceArtifact, targetProfile);
        SourceTargetCompatibilityResultV1 result = new SourceTargetCompatibilityStage().Evaluate(request);

        return new PersistedCompatibilityEvaluation(sourceArtifact, targetProfile, request, result);
    }

    private static SourceTargetCompatibilityResultV1 Evaluate(FixtureManifest source, FixtureManifest target)
    {
        var request = new SourceTargetCompatibilityRequestV1(CreateSource(source), CreateTarget(target));
        return new SourceTargetCompatibilityStage().Evaluate(request);
    }

    private static SourceCompatibilityProfileV1 CreateSource(FixtureManifest fixture) =>
        new SourceCompatibilityProfileV1("source." + fixture.Key, fixture.Manifest, fixture.Artifact);

    private static TargetCompatibilityProfileV1 CreateTarget(FixtureManifest fixture) =>
        new TargetCompatibilityProfileV1(
            "target." + fixture.Key,
            ArtifactHasher.ComputeSha256(Encoding.UTF8.GetBytes("target-profile:" + fixture.Artifact.ArtifactId)),
            fixture.Manifest,
            fixture.Artifact);

    private static FixtureManifest BuildManifest(string key, string meshName, int vertexCount, double boundsMaximumY)
    {
        CanonicalObjectId assetId = Id("asset", key + "/asset");
        CanonicalObjectId positionsBufferId = Id("buffer", key + "/buffer/positions");
        CanonicalObjectId indicesBufferId = Id("buffer", key + "/buffer/indices");
        CanonicalObjectId matrixBufferId = Id("buffer", key + "/buffer/matrix");
        CanonicalObjectId positionsViewId = Id("buffer-view", key + "/view/positions");
        CanonicalObjectId indicesViewId = Id("buffer-view", key + "/view/indices");
        CanonicalObjectId matrixViewId = Id("buffer-view", key + "/view/matrix");
        CanonicalObjectId positionsAccessorId = Id("accessor", key + "/accessor/positions");
        CanonicalObjectId indicesAccessorId = Id("accessor", key + "/accessor/indices");
        CanonicalObjectId matrixAccessorId = Id("accessor", key + "/accessor/matrix");
        CanonicalObjectId meshId = Id("mesh", key + "/mesh/body");
        CanonicalObjectId primitiveId = Id("primitive", key + "/primitive/body");
        CanonicalObjectId bindingId = Id("mesh-binding", key + "/binding/body");

        CanonicalBlob positionsBlob = CanonicalBinaryWriter.WriteFloat32(BuildPositionScalars(vertexCount));
        CanonicalBlob indicesBlob = CanonicalBinaryWriter.WriteUInt32(new uint[] { 0, 1, 2 });
        CanonicalBlob matrixBlob = CanonicalBinaryWriter.WriteMatrix4x4F32(new CanonicalMatrix4x4F32(
            1f, 0f, 0f, 0f,
            0f, 1f, 0f, 0f,
            0f, 0f, 1f, 0f,
            0f, 0f, 0f, 1f));

        var manifest = new CanonicalArmorManifestV1(
            assetId,
            new Dictionary<CanonicalObjectId, CanonicalBufferBlobRefV1>
            {
                [positionsBufferId] = new CanonicalBufferBlobRefV1(positionsBufferId, positionsBlob.Artifact, positionsBlob.Bytes.Length),
                [indicesBufferId] = new CanonicalBufferBlobRefV1(indicesBufferId, indicesBlob.Artifact, indicesBlob.Bytes.Length),
                [matrixBufferId] = new CanonicalBufferBlobRefV1(matrixBufferId, matrixBlob.Artifact, matrixBlob.Bytes.Length),
            },
            new Dictionary<CanonicalObjectId, CanonicalBufferViewV1>
            {
                [positionsViewId] = new CanonicalBufferViewV1(positionsViewId, positionsBufferId, positionsBlob.Bytes.Length),
                [indicesViewId] = new CanonicalBufferViewV1(indicesViewId, indicesBufferId, indicesBlob.Bytes.Length),
                [matrixViewId] = new CanonicalBufferViewV1(matrixViewId, matrixBufferId, matrixBlob.Bytes.Length),
            },
            new Dictionary<CanonicalObjectId, CanonicalAccessorV1>
            {
                [positionsAccessorId] = new CanonicalAccessorV1(positionsAccessorId, positionsViewId, CanonicalComponentType.F32, CanonicalElementShape.Vec3, vertexCount),
                [indicesAccessorId] = new CanonicalAccessorV1(indicesAccessorId, indicesViewId, CanonicalComponentType.U32, CanonicalElementShape.Scalar, 3),
                [matrixAccessorId] = new CanonicalAccessorV1(matrixAccessorId, matrixViewId, CanonicalComponentType.F32, CanonicalElementShape.Mat4, 1),
            },
            new Dictionary<CanonicalObjectId, CanonicalMeshV1>
            {
                [meshId] = new CanonicalMeshV1(
                    meshId,
                    meshName,
                    vertexCount,
                    new Dictionary<string, CanonicalObjectId>(StringComparer.Ordinal)
                    {
                        ["POSITION"] = positionsAccessorId,
                    },
                    new[] { new CanonicalPrimitiveV1(primitiveId, indicesAccessorId) },
                    new CanonicalBoundsV1(0d, 0d, 0d, 1d, boundsMaximumY, 0d)),
            },
            new Dictionary<CanonicalObjectId, CanonicalMeshBindingV1>
            {
                [bindingId] = new CanonicalMeshBindingV1(bindingId, meshId, null, matrixAccessorId),
            });

        return new FixtureManifest(key, manifest, CanonicalManifestWriter.Write(manifest));
    }

    private static float[] BuildPositionScalars(int vertexCount)
    {
        var values = new float[checked(vertexCount * 3)];
        for (int vertex = 0; vertex < vertexCount; vertex++)
        {
            values[(vertex * 3) + 0] = vertex;
            values[(vertex * 3) + 1] = vertex == 2 ? 1f : 0f;
            values[(vertex * 3) + 2] = 0f;
        }

        return values;
    }

    private static CanonicalObjectId Id(string kind, string key) =>
        CanonicalObjectIdDeriver.DeriveSource(kind, AdapterContract, key);

    private static void CheckNoDownstreamAuthorisation(SourceTargetCompatibilityResultV1 result)
    {
        Check(!result.CandidateMapApplicationAllowed, "Candidate-map application must remain disallowed.");
        Check(!result.CandidateMapApplicationExecuted, "Candidate-map application must not execute.");
        Check(!result.ConversionAllowed, "Conversion must remain disallowed.");
        Check(!result.ConversionExecuted, "Conversion must not execute.");
        Check(!result.ItemEquipMutationAllowed, "Item/equip mutation must remain disallowed.");
        Check(!result.ItemEquipMutationExecuted, "Item/equip mutation must not execute.");
        Check(!result.SaveMutationAllowed, "Save mutation must remain disallowed.");
        Check(!result.SaveMutationExecuted, "Save mutation must not execute.");
        Check(!result.DownstreamWritesAllowed, "Downstream writes must remain disallowed.");
        Check(!result.DownstreamWritesExecuted, "Downstream writes must not execute.");
    }

    private static void CheckReceiptRowNoDownstreamWrites(JsonElement row)
    {
        Check(!row.GetProperty("candidateMapApplicationAllowed").GetBoolean(), "Receipt row candidate-map application must remain disallowed.");
        Check(!row.GetProperty("candidateMapApplicationExecuted").GetBoolean(), "Receipt row candidate-map application must not execute.");
        Check(!row.GetProperty("conversionAllowed").GetBoolean(), "Receipt row conversion must remain disallowed.");
        Check(!row.GetProperty("conversionExecuted").GetBoolean(), "Receipt row conversion must not execute.");
        Check(!row.GetProperty("sidecarsGenerated").GetBoolean(), "Receipt row sidecar generation must not execute.");
        Check(!row.GetProperty("sourceFbxMutationExecuted").GetBoolean(), "Receipt row source FBX mutation must not execute.");
        Check(!row.GetProperty("unityProjectAssetMutationExecuted").GetBoolean(), "Receipt row Unity project asset mutation must not execute.");
        Check(!row.GetProperty("runtimeLoaderChanged").GetBoolean(), "Receipt row runtime loader changes must not execute.");
        Check(!row.GetProperty("runtimeRegistrationExecuted").GetBoolean(), "Receipt row runtime registration must not execute.");
        Check(!row.GetProperty("itemRegistrationExecuted").GetBoolean(), "Receipt row item registration must not execute.");
        Check(!row.GetProperty("inventoryEquipSaveMutationExecuted").GetBoolean(), "Receipt row inventory/equip/save mutation must not execute.");
        Check(!row.GetProperty("saveWriteExecuted").GetBoolean(), "Receipt row save write must not execute.");
        Check(!row.GetProperty("nativeGameWriteExecuted").GetBoolean(), "Receipt row native game write must not execute.");
        Check(!row.GetProperty("releaseReady").GetBoolean(), "Receipt row release readiness must remain false.");
    }

    private static void CheckReceiptDownstreamNoWrites(JsonElement downstream)
    {
        foreach (JsonProperty property in downstream.EnumerateObject())
        {
            Check(!property.Value.GetBoolean(), "Receipt downstream field must be false: " + property.Name);
        }
    }

    private static void CheckPersistedCompatibilityReceipt(
        string packageRoot,
        R2CanonicalArmorArtifactV1 sourceArtifact,
        FingerprintedTargetProfileV1 targetProfile,
        SourceTargetCompatibilityResultV1 result)
    {
        string receiptPath = Path.Combine(packageRoot, CompatibilityReceiptArtifact);
        using JsonDocument document = JsonDocument.Parse(File.ReadAllBytes(receiptPath));
        JsonElement root = document.RootElement;
        JsonElement source = root.GetProperty("source");
        JsonElement target = root.GetProperty("target");
        JsonElement boundaries = root.GetProperty("boundaries");

        Check(root.GetProperty("schemaVersion").GetString() == "tainted-armour.source-target-compatibility-output/1", "Compatibility receipt schema changed.");
        Check(root.GetProperty("stageContractVersion").GetString() == result.StageContractVersion, "Compatibility receipt stage contract changed.");
        Check(root.GetProperty("receiptContractVersion").GetString() == "tainted-armour.source-target-compatibility-receipt/1", "Compatibility receipt contract changed.");
        Check(root.GetProperty("policyFingerprint").GetString() == result.Receipt.PolicyFingerprint.ToString(), "Compatibility receipt policy fingerprint changed.");
        Check(root.GetProperty("cacheKey").GetString() == result.CacheKey.ToString(), "Compatibility receipt cache key drifted from production result.");
        Check(root.GetProperty("receiptId").GetString() == result.Receipt.ReceiptId.ToString(), "Compatibility receipt identity drifted from production result.");
        Check(root.GetProperty("gate").GetString() == GateText(result.Gate), "Compatibility receipt gate drifted from production result.");
        Check(root.GetProperty("classification").GetString() == ClassificationText(result.Classification), "Compatibility receipt classification drifted from production result.");
        Check(root.GetProperty("compatibilityAccepted").GetBoolean() == result.CompatibilityAccepted, "Compatibility receipt acceptance drifted from production result.");
        CheckStringArray(root.GetProperty("assertions"), result.Receipt.Assertions, "Compatibility receipt assertions drifted from production result.");
        CheckStringArray(root.GetProperty("diagnostics"), result.Diagnostics, "Compatibility receipt diagnostics drifted from production result.");
        CheckStringArray(root.GetProperty("blockers"), result.Blockers, "Compatibility receipt blockers drifted from production result.");

        Check(source.GetProperty("artifactContractVersion").GetString() == R2CanonicalArmorArtifactV1.ContractVersionValue, "Compatibility receipt source artifact contract changed.");
        Check(source.GetProperty("profileId").GetString() == sourceArtifact.ProfileId, "Compatibility receipt source profile changed.");
        Check(source.GetProperty("canonicalPackage").GetString() == ".", "Compatibility receipt source package pointer changed.");
        string sourceReceipt = source.GetProperty("sourceReceipt").GetString() ?? string.Empty;
        Check(NormalizeFixturePath(sourceReceipt) == CanonicalT56SourceReceiptPath, "Compatibility receipt source receipt must be the canonical full-geometry receipt.");
        Check(!NormalizeFixturePath(sourceReceipt).EndsWith(RetiredT56NonFullReceipt, StringComparison.Ordinal), "Compatibility receipt must not consume the retired non-full T56 receipt.");
        Check(File.Exists(Path.GetFullPath(Path.Combine(packageRoot, sourceReceipt))), "Compatibility receipt source receipt is missing.");
        Check(source.GetProperty("canonicalArtifact").GetString() == sourceArtifact.ManifestArtifactId.ToString(), "Compatibility receipt source canonical artifact changed.");

        Check(target.GetProperty("targetProfileContractVersion").GetString() == FingerprintedTargetProfileV1.ContractVersionValue, "Compatibility receipt target-profile contract changed.");
        Check(target.GetProperty("targetProfileId").GetString() == targetProfile.TargetProfileId, "Compatibility receipt target profile changed.");
        Check(target.GetProperty("targetProfileFingerprint").GetString() == targetProfile.TargetProfileFingerprint.ToString(), "Compatibility receipt target profile fingerprint drifted.");
        Check(target.GetProperty("canonicalPackage").GetString() == ".", "Compatibility receipt target package pointer changed.");
        Check(target.GetProperty("canonicalArtifact").GetString() == targetProfile.CanonicalArtifact.ManifestArtifactId.ToString(), "Compatibility receipt target canonical artifact changed.");

        CheckCompatibilityBoundaryNoWrites(boundaries, result);
    }

    private static void CheckStringArray(JsonElement element, IReadOnlyList<string> expected, string message)
    {
        string[] observed = element.EnumerateArray().Select(value => value.GetString() ?? string.Empty).ToArray();
        Check(observed.SequenceEqual(expected), message);
    }

    private static void CheckCompatibilityBoundaryNoWrites(JsonElement boundaries, SourceTargetCompatibilityResultV1 result)
    {
        Check(boundaries.GetProperty("candidateMapApplicationAllowed").GetBoolean() == result.CandidateMapApplicationAllowed, "Compatibility receipt candidate-map allowance drifted.");
        Check(boundaries.GetProperty("candidateMapApplicationExecuted").GetBoolean() == result.CandidateMapApplicationExecuted, "Compatibility receipt candidate-map execution drifted.");
        Check(boundaries.GetProperty("conversionAllowed").GetBoolean() == result.ConversionAllowed, "Compatibility receipt conversion allowance drifted.");
        Check(boundaries.GetProperty("conversionExecuted").GetBoolean() == result.ConversionExecuted, "Compatibility receipt conversion execution drifted.");
        Check(boundaries.GetProperty("itemEquipMutationAllowed").GetBoolean() == result.ItemEquipMutationAllowed, "Compatibility receipt item/equip allowance drifted.");
        Check(boundaries.GetProperty("itemEquipMutationExecuted").GetBoolean() == result.ItemEquipMutationExecuted, "Compatibility receipt item/equip execution drifted.");
        Check(boundaries.GetProperty("saveMutationAllowed").GetBoolean() == result.SaveMutationAllowed, "Compatibility receipt save allowance drifted.");
        Check(boundaries.GetProperty("saveMutationExecuted").GetBoolean() == result.SaveMutationExecuted, "Compatibility receipt save execution drifted.");
        Check(boundaries.GetProperty("downstreamWritesAllowed").GetBoolean() == result.DownstreamWritesAllowed, "Compatibility receipt downstream write allowance drifted.");
        Check(boundaries.GetProperty("downstreamWritesExecuted").GetBoolean() == result.DownstreamWritesExecuted, "Compatibility receipt downstream write execution drifted.");
        foreach (JsonProperty property in boundaries.EnumerateObject())
        {
            Check(!property.Value.GetBoolean(), "Compatibility receipt boundary field must remain false: " + property.Name);
        }
    }

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

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private static void CheckThrows<TException>(Action action, string message)
        where TException : Exception
    {
        try
        {
            action();
        }
        catch (TException)
        {
            return;
        }

        throw new InvalidOperationException(message);
    }

    private static FingerprintedTargetProfileV1 ReadTargetProfile(string packageRoot, R2CanonicalArmorArtifactV1 canonicalArtifact)
    {
        string profilePath = Path.Combine(packageRoot, "target-profile.json");
        using JsonDocument document = JsonDocument.Parse(File.ReadAllBytes(profilePath));
        JsonElement root = document.RootElement;
        Check(root.GetProperty("schemaVersion").GetString() == "tainted-armour.target-profile-fixture/1", "Target profile fixture schema changed.");
        Check(root.GetProperty("canonicalPackage").GetString() == ".", "Target profile fixture must point at its package directory.");
        string sourceGeometryCapture = root.GetProperty("sourceGeometryCapture").GetString() ?? string.Empty;
        Check(File.Exists(Path.GetFullPath(Path.Combine(packageRoot, sourceGeometryCapture))), "Target profile fixture source geometry capture is missing.");
        string sourceReceipt = root.GetProperty("sourceReceipt").GetString() ?? string.Empty;
        Check(File.Exists(Path.GetFullPath(Path.Combine(packageRoot, sourceReceipt))), "Target profile fixture source receipt is missing.");
        Check(
            !NormalizeFixturePath(sourceReceipt).EndsWith(RetiredT56NonFullReceipt, StringComparison.Ordinal),
            "Target profile fixture must not depend on the retired non-full T56 receipt.");
        return new FingerprintedTargetProfileV1(
            root.GetProperty("targetProfileId").GetString() ?? string.Empty,
            ArtifactId.Parse(root.GetProperty("targetProfileFingerprint").GetString() ?? string.Empty),
            canonicalArtifact);
    }

    private static string NormalizeFixturePath(string path) =>
        path.Replace('/', '\\');

    private static string ResolveFixturePath(params string[] segments)
    {
        string current = Directory.GetCurrentDirectory();
        while (!string.IsNullOrEmpty(current))
        {
            string candidate = Path.Combine(new[] { current }.Concat(segments).ToArray());
            if (Directory.Exists(candidate) || File.Exists(candidate)) return candidate;
            string? parent = Directory.GetParent(current)?.FullName;
            if (StringComparer.OrdinalIgnoreCase.Equals(parent, current)) break;
            current = parent ?? string.Empty;
        }

        throw new DirectoryNotFoundException("Unable to resolve fixture path: " + string.Join("/", segments));
    }

    private sealed class FixtureManifest
    {
        public FixtureManifest(string key, CanonicalArmorManifestV1 manifest, CanonicalManifestArtifact artifact)
        {
            Key = key;
            Manifest = manifest;
            Artifact = artifact;
        }

        public string Key { get; }
        public CanonicalArmorManifestV1 Manifest { get; }
        public CanonicalManifestArtifact Artifact { get; }
    }

    private sealed class PersistedCompatibilityEvaluation
    {
        public PersistedCompatibilityEvaluation(
            R2CanonicalArmorArtifactV1 sourceArtifact,
            FingerprintedTargetProfileV1 targetProfile,
            SourceTargetCompatibilityRequestV1 request,
            SourceTargetCompatibilityResultV1 result)
        {
            SourceArtifact = sourceArtifact;
            TargetProfile = targetProfile;
            Request = request;
            Result = result;
        }

        public R2CanonicalArmorArtifactV1 SourceArtifact { get; }
        public FingerprintedTargetProfileV1 TargetProfile { get; }
        public SourceTargetCompatibilityRequestV1 Request { get; }
        public SourceTargetCompatibilityResultV1 Result { get; }
    }
}
