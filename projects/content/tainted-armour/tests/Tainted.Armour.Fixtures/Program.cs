using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Tainted.Armour;

internal static class Program
{
    private static readonly ArmorVector3[] Baseline =
    {
        new ArmorVector3(0d, 0d, 0d),
        new ArmorVector3(1d, 0d, 0d),
        new ArmorVector3(0d, 1d, 0d),
    };

    private static int Main(string[] args)
    {
        if (args.Length == 2 && string.Equals(args[0], "--import-live-packet", StringComparison.Ordinal))
        {
            return ImportLivePacket(args[1]);
        }

        if (args.Length == 2 && string.Equals(args[0], "--validate-kandra-package-with-live-packet", StringComparison.Ordinal))
        {
            return ValidateKandraPackageWithLivePacket(args[1]);
        }

        if (args.Length == 2 && string.Equals(args[0], "--preflight-kandra-registration-with-live-packet", StringComparison.Ordinal))
        {
            return PreflightKandraRegistrationWithLivePacket(args[1]);
        }

        if (args.Length == 2 && string.Equals(args[0], "--build-kandra-registration-candidate-with-live-packet", StringComparison.Ordinal))
        {
            return BuildKandraRegistrationCandidateWithLivePacket(args[1]);
        }

        if (args.Length == 2 && string.Equals(args[0], "--dry-run-kandra-registration-with-live-packet", StringComparison.Ordinal))
        {
            return DryRunKandraRegistrationWithLivePacket(args[1]);
        }

        if (args.Length == 2 && string.Equals(args[0], "--compare-same-mesh-ab-receipt", StringComparison.Ordinal))
        {
            return CompareSameMeshAbReceipt(args[1]);
        }

        var fixtures = new (string Name, Action Run)[]
        {
            ("identity geometry", IdentityGeometry),
            ("rigid rotation", RigidRotation),
            ("local orientation reversal", LocalOrientationReversal),
            ("Unity normalization epsilon", UnityNormalizationEpsilon),
            ("collapsed triangle", CollapsedTriangle),
            ("deterministic repeat", DeterministicRepeat),
            ("adapter contract mapping", AdapterContractMapping),
            ("mismatched pose fails closed", MismatchedPoseFailsClosed),
            ("downstream writes stay false", DownstreamWritesStayFalse),
            ("A2K-T34 visual runtime observer reaches importer result", VisualRuntimeObserverReachesImporterResult),
            ("A2K-T34 blocked diagnostic metrics fail closed", VisualRuntimeBlockedMetricsFailClosed),
            ("Kandra writer emits runtime layout files", KandraWriterEmitsRuntimeLayoutFiles),
            ("Kandra package validation uses live proof and stays no-write", KandraPackageValidationUsesLiveProofAndStaysNoWrite),
            ("Kandra registration preflight compares metadata and stays no-write", KandraRegistrationPreflightComparesMetadataAndStaysNoWrite),
            ("Kandra registration candidate builds request and stays no-write", KandraRegistrationCandidateBuildsRequestAndStaysNoWrite),
            ("Kandra runtime registration dry-run consumes contract and stays no-write", KandraRuntimeRegistrationDryRunConsumesContractAndStaysNoWrite),
            ("Kandra runtime registration invocation requires explicit approval", KandraRuntimeRegistrationInvocationRequiresExplicitApproval),
            ("Kandra host registration proof consumes live receipt artifact", KandraHostRegistrationProofConsumesLiveReceiptArtifact),
            ("Kandra target runtime registration plan refuses proof package reuse and builds target context", KandraTargetRuntimeRegistrationPlanRefusesProofPackageReuseAndBuildsTargetContext),
            ("Kandra same-mesh A/B proof builds controlled package variants", KandraSameMeshAbProofBuildsControlledPackageVariants),
            ("Kandra same-mesh visual/decode comparison blocks encoder without visual evidence", KandraSameMeshVisualDecodeComparisonBlocksWithoutVisualEvidence),
            ("Kandra full-section package generation builds candidate package from imported channels", KandraFullSectionPackageGenerationBuildsCandidatePackageFromImportedChannels),
        };

        var failures = new List<string>();
        foreach (var fixture in fixtures)
        {
            try
            {
                fixture.Run();
                Console.WriteLine("PASS " + fixture.Name);
            }
            catch (Exception exception)
            {
                failures.Add(fixture.Name + ": " + exception.Message);
                Console.WriteLine("FAIL " + fixture.Name + ": " + exception.Message);
            }
        }

        if (failures.Count != 0)
        {
            Console.WriteLine("TAINTED_ARMOUR_FIXTURES_FAILED count=" + failures.Count);
            return 1;
        }

        Console.WriteLine("TAINTED_ARMOUR_FIXTURES_PASS count=" + fixtures.Length);
        return 0;
    }

    private static void IdentityGeometry()
    {
        ArmorImportResult result = Import("identity", Baseline);
        Check(result.Status == ArmorImportStatus.DeformationValidatedConversionBlocked, "Identity must pass deformation and stop at conversion.");
        Check(result.Deformation.DeformationAccepted, "Identity deformation must be accepted.");
        Check(result.Deformation.Controls.Passed, "Production control calibration must pass.");
        Check(!result.Deformation.Controls.OutOfPlaneRigidRotationPassed, "Default calibration must still expose the current metric's out-of-plane limitation.");
        Check(
            result.Deformation.Warnings.Contains("control_out_of_plane_rigid_rotation_diagnostic_failed:t29-t31.normal-dot.v1"),
            "Out-of-plane limitation must flow as a diagnostic warning.");
        Check(
            !result.Deformation.Blockers.Contains("metric_calibration_failed:t29-t31.normal-dot.v1"),
            "Default out-of-plane diagnostic warning must not block active calibration.");
        Check(result.Deformation.CollapsedTriangleCount == 0, "Identity must not collapse.");
        Check(result.Deformation.OrientationReversedTriangleCount == 0, "Identity must preserve winding.");
    }

    private static void RigidRotation()
    {
        ArmorImportResult result = Import(
            "rotation",
            new[]
            {
                new ArmorVector3(0d, 0d, 0d),
                new ArmorVector3(0d, 1d, 0d),
                new ArmorVector3(-1d, 0d, 0d),
            });
        Check(result.Deformation.DeformationAccepted, "Rigid rotation must preserve orientation.");
        Check(result.Deformation.OrientationReversedTriangleCount == 0, "Rigid rotation must not reverse winding.");
    }

    private static void LocalOrientationReversal()
    {
        ArmorImportResult result = Import("reversal", new[] { Baseline[0], Baseline[2], Baseline[1] });
        Check(result.Status == ArmorImportStatus.DeformationBlocked, "Orientation reversal must block deformation.");
        Check(result.Deformation.OrientationReversedTriangleCount == 1, "Exactly one reversal must be reported.");
        TriangleDeformationEvidence record = result.Deformation.Poses.Single().TriangleEvidence.Single();
        Check(record.OrientationReversed, "Per-triangle evidence must retain the reversal.");
        Check(record.NormalizedNormalDot < 0d, "Reversal evidence must retain a negative normalized dot.");
    }

    private static void UnityNormalizationEpsilon()
    {
        var tinyBaseline = new[]
        {
            new ArmorVector3(0d, 0d, 0d),
            new ArmorVector3(0.001d, 0d, 0d),
            new ArmorVector3(0d, 0.001d, 0d),
        };
        var source = new ArmorSourceContract(
            "armor.fixture.tiny",
            "sha256:source-tiny",
            "body-mesh",
            0,
            "body-material",
            "sha256:rig",
            "sha256:bind-pose",
            "sha256:weights",
            3,
            1,
            3,
            4,
            0);
        var target = new ArmorTargetContract(
            "native.body",
            "male",
            "Armor",
            "sha256:target",
            "sha256:candidate-map",
            "candidate_map_ready_not_applied");
        var deformation = new DeformationValidationRequest(
            new PoseGeometrySnapshot("baseline.tiny", "rest", 0d, "sha256:baseline-tiny", tinyBaseline),
            new[]
            {
                new PoseGeometrySnapshot(
                    "pose.tiny-reversal",
                    "clip.fixture",
                    0.5d,
                    "sha256:pose-tiny",
                    new[] { tinyBaseline[0], tinyBaseline[2], tinyBaseline[1] }),
            },
            new[] { new ArmorTriangle(0, 0, 1, 2) },
            new[] { 0, 1, 2 },
            DeformationPolicy.CreateT29T31ZeroTolerance());

        ArmorImportResult result = new ArmorImporter().Import(
            new ArmorImportRequest("request.tiny-normalization", source, target, deformation));
        TriangleDeformationEvidence record = result.Deformation.Poses.Single().TriangleEvidence.Single();
        Check(!record.Collapsed, "The tiny control triangle must remain above the collapse epsilon.");
        Check(!record.OrientationReversed, "Unity's Vector3 normalization epsilon must zero this tiny normal.");
        Check(record.NormalizedNormalDot == 0d, "The tiny normalized normal dot must be zero.");
    }

    private static void CollapsedTriangle()
    {
        ArmorImportResult result = Import(
            "collapse",
            new[]
            {
                new ArmorVector3(0d, 0d, 0d),
                new ArmorVector3(1d, 0d, 0d),
                new ArmorVector3(2d, 0d, 0d),
            });
        Check(result.Status == ArmorImportStatus.DeformationBlocked, "Collapse must block deformation.");
        Check(result.Deformation.CollapsedTriangleCount == 1, "Exactly one collapse must be reported.");
        Check(result.Deformation.Poses.Single().TriangleEvidence.Single().Collapsed, "Per-triangle evidence must retain the collapse.");
    }

    private static void DeterministicRepeat()
    {
        ArmorImportResult first = Import("repeat", new[] { Baseline[0], Baseline[2], Baseline[1] });
        ArmorImportResult second = Import("repeat", new[] { Baseline[0], Baseline[2], Baseline[1] });
        Check(Signature(first) == Signature(second), "Repeated evaluation must return the same typed result.");
    }

    private static void AdapterContractMapping()
    {
        var sampled = new SampledMeshRecord(
            "armor.fixture",
            "sha256:source",
            "body-mesh",
            Baseline,
            new[] { new ArmorTriangle(0, 0, 1, 2) });
        ArmorImportRequest request = sampled.ToImportRequest(Baseline);
        ArmorImportResult result = new ArmorImporter().Import(request);
        Check(result.Deformation.SourceFingerprint == "sha256:source", "Adapter mapping must preserve source identity.");
        Check(result.Deformation.EvaluatedTriangleCount == 1, "Adapter mapping must feed the production stage.");
    }

    private static void DownstreamWritesStayFalse()
    {
        ArmorImportResult result = Import("false-boundary", Baseline);
        Check(!result.CandidateMapApplicationAllowed, "Candidate-map application must remain disallowed.");
        Check(!result.CandidateMapApplicationExecuted, "Candidate-map application must not execute.");
        Check(!result.ConversionAllowed && !result.ConversionExecuted, "Conversion must remain false.");
        Check(!result.SidecarsGenerated, "Sidecars must remain false.");
        Check(!result.RuntimeLoaderChanged && !result.RuntimeRegistrationExecuted, "Runtime mutation must remain false.");
        Check(!result.NativeGameWriteExecuted && !result.ReleaseReady, "Game writes and release must remain false.");
    }

    private static void VisualRuntimeObserverReachesImporterResult()
    {
        ArmorImportRequest request = CreateRequest(
            "visual-runtime",
            Baseline,
            CreateVisualRuntimeObservation("observed", "observed", "observed:fall / Anim_Hero_TPP_Base_Knockdown_Air_Loop", "observed"));
        ArmorImportResult result = new ArmorImporter().Import(request);
        Check(
            result.Status == ArmorImportStatus.DeformationAndVisualRuntimeValidatedConversionBlocked,
            "Accepted A2K-T34 observation must enter the importer result before conversion blocks.");
        Check(result.VisualRuntime.Requested, "Visual runtime stage must record that the observer was supplied.");
        Check(result.VisualRuntime.RowCount == 6, "The six T56/T57 receiver rows must be retained.");
        Check(result.VisualRuntime.RequiredRowsPresent, "Required female/male neck_02, spine_04, and spine_05 rows must be present.");
        Check(result.VisualRuntime.DownstreamBoundaryFalse, "Visual runtime stage must preserve false downstream boundaries.");
        Check(!result.VisualRuntime.CandidateMapApplicationAllowed && !result.VisualRuntime.CandidateMapApplicationExecuted, "Visual runtime candidate-map boundary must stay false.");
        Check(!result.VisualRuntime.ConversionAllowed && !result.VisualRuntime.ConversionExecuted, "Visual runtime conversion boundary must stay false.");
        Check(!result.VisualRuntime.DownstreamWritesExecuted, "Visual runtime downstream writes must stay false.");
    }

    private static void VisualRuntimeBlockedMetricsFailClosed()
    {
        ArmorImportRequest request = CreateRequest(
            "visual-runtime-blocked",
            Baseline,
            CreateVisualRuntimeObservation("observed", "observed", "observed:fall / Anim_Hero_TPP_Base_Knockdown_Air_Loop", "blocked_unobserved"));
        ArmorImportResult result = new ArmorImporter().Import(request);
        Check(result.Status == ArmorImportStatus.VisualRuntimeBlocked, "Blocked visual metrics must block the visual-runtime stage.");
        Check(result.VisualRuntime.BlockedMetricRowCount == 6, "Every required row must retain the blocked metric status.");
        Check(
            result.Blockers.Contains("a2k_t34_visual_metric_rows_blocked:6"),
            "The aggregate blocked metric reason must flow to the import result.");
        Check(!result.CandidateMapApplicationExecuted && !result.ConversionExecuted && !result.NativeGameWriteExecuted, "Blocked visual observation must not execute downstream writes.");
    }

    private static void KandraWriterEmitsRuntimeLayoutFiles()
    {
        string fixtureRoot = Path.Combine(Path.GetTempPath(), "ta-kandra-writer-" + Guid.NewGuid().ToString("N"));
        try
        {
            KandraMeshPayloadSections payload = CreateFixtureKandraPayload();
            KandraWriterResult result = WriteFixtureKandraPackage(fixtureRoot, "FixtureMod", "Mesh_Fixture_Kandra", payload);

            string expectedDirectory = Path.Combine(fixtureRoot, "FixtureMod", "Kandra");
            string expectedMeshPath = Path.Combine(expectedDirectory, "Mesh_Fixture_Kandra.mdkandra");
            string expectedIndexPath = Path.Combine(expectedDirectory, "Mesh_Fixture_Kandra.ixkandra");
            Check(result.KandraDirectory == expectedDirectory, "Writer must target modDirectory/Kandra.");
            Check(result.MeshDataPath == expectedMeshPath, "Writer must use the .mdkandra runtime seam.");
            Check(result.IndicesDataPath == expectedIndexPath, "Writer must use the .ixkandra runtime seam.");
            Check(File.Exists(expectedMeshPath), "Writer must emit the mesh payload file.");
            Check(File.Exists(expectedIndexPath), "Writer must emit the index payload file.");

            byte[] meshBytes = File.ReadAllBytes(expectedMeshPath);
            byte[] indexBytes = File.ReadAllBytes(expectedIndexPath);
            Check(meshBytes.Length == result.MeshDataByteCount, "Result mesh length must match the written file.");
            Check(indexBytes.Length == result.IndicesDataByteCount, "Result index length must match the written file.");
            Check(meshBytes.Length == KandraRuntimePayloadLayout.ExpectedMeshPayloadByteCount(2, 1, 1), "Mesh payload must match the recovered ReadSerializedData layout length.");
            Check(indexBytes.SequenceEqual(new byte[] { 0, 0, 1, 0, 0, 0 }), "Index payload must be raw little-endian ushort data.");
            Check(meshBytes.Take(KandraRuntimePayloadLayout.CompressedVertexByteSize * 2).SequenceEqual(payload.CompressedVertices), "CompressedVertex section must be first.");
            Check(result.MeshDataSha256.StartsWith("sha256:", StringComparison.Ordinal), "Mesh payload hash must be reported.");
            Check(result.IndicesDataSha256.StartsWith("sha256:", StringComparison.Ordinal), "Index payload hash must be reported.");
            Check(!result.CandidateMapApplicationExecuted, "Writer must not apply candidate maps.");
            Check(!result.ConversionExecuted, "Writer must not claim conversion execution.");
            Check(!result.RuntimeRegistrationAllowed && !result.RuntimeRegistrationExecuted, "Writer must not enable runtime registration.");
            Check(!result.NativeGameWriteExecuted && !result.DownstreamWritesExecuted, "Writer fixture must not claim downstream game writes.");
        }
        finally
        {
            CleanupDirectory(fixtureRoot);
        }
    }

    private static void KandraPackageValidationUsesLiveProofAndStaysNoWrite()
    {
        string fixtureRoot = Path.Combine(Path.GetTempPath(), "ta-kandra-package-validation-" + Guid.NewGuid().ToString("N"));
        try
        {
            KandraWriterResult writer = WriteFixtureKandraPackage(fixtureRoot, "FixtureMod", "Mesh_Fixture_Kandra", CreateFixtureKandraPayload());
            ArmorImportResult proof = new ArmorImporter().Import(
                CreateRequest(
                    "kandra-package-live-proof",
                    Baseline,
                    CreateVisualRuntimeObservation(
                        "observed:live FoA hero/body renderer context",
                        "observed:live FoA equip renderer context",
                        "blocked_unproven:tpp-target-animation-clip-binding",
                        "blocked_unobserved",
                        "observed:True:renderingId=1",
                        "observed:True")));

            KandraPackageValidationResult result = new KandraPackageValidationStage().Validate(
                new KandraPackageValidationRequest("fixture.kandra-package-validation", writer, proof));

            Check(result.PackageValidationAccepted, "Kandra package validation must accept a valid loose package plus live Kandra proof.");
            Check(result.RuntimeSeamValid, "Kandra package validation must verify the modDirectory/Kandra seam.");
            Check(result.MeshDataFilePresent && result.IndicesDataFilePresent, "Kandra package validation must see both loose package files.");
            Check(result.MeshDataLengthMatches && result.IndicesDataLengthMatches, "Kandra package validation must verify file lengths.");
            Check(result.MeshDataHashMatches && result.IndicesDataHashMatches, "Kandra package validation must verify file hashes.");
            Check(result.LiveKandraRegistrationProofPresent, "Kandra package validation must retain live IsRegistered proof.");
            Check(result.LiveKandraMeshMemoryProofPresent, "Kandra package validation must retain live TryGetMeshMemory proof.");
            Check(result.LiveProofDownstreamBoundaryFalse && result.WriterDownstreamBoundaryFalse, "Kandra package validation must require false downstream boundaries.");
            Check(!result.CustomArmourRegistrationAllowed && !result.RuntimeRegistrationExecuted, "Kandra package validation must not allow or execute custom registration.");
            Check(!result.CandidateMapApplicationExecuted && !result.ConversionExecuted, "Kandra package validation must not apply candidate maps or conversion.");
            Check(!result.NativeGameWriteExecuted && !result.DownstreamWritesExecuted, "Kandra package validation must remain no-write.");
        }
        finally
        {
            CleanupDirectory(fixtureRoot);
        }
    }

    private static void KandraRegistrationPreflightComparesMetadataAndStaysNoWrite()
    {
        string fixtureRoot = Path.Combine(Path.GetTempPath(), "ta-kandra-registration-preflight-" + Guid.NewGuid().ToString("N"));
        try
        {
            KandraWriterResult writer = WriteFixtureKandraPackage(fixtureRoot, "FixtureMod", "Mesh_Fixture_Kandra", CreateFixtureKandraPayload());
            ArmorImportResult proof = new ArmorImporter().Import(
                CreateRequest(
                    "kandra-registration-preflight-live-proof",
                    Baseline,
                    CreateVisualRuntimeObservation(
                        "observed:live FoA hero/body renderer context",
                        "observed:live FoA equip renderer context",
                        "blocked_unproven:tpp-target-animation-clip-binding",
                        "blocked_unobserved",
                        "observed:True:renderingId=1",
                        "observed:True")));
            KandraPackageValidationResult package = new KandraPackageValidationStage().Validate(
                new KandraPackageValidationRequest("fixture.kandra-registration-preflight.package", writer, proof));
            KandraMeshRegistrationMetadata metadata = CreateFixtureKandraRegistrationMetadata("FixtureMod", "Mesh_Fixture_Kandra");

            KandraRegistrationPreflightResult result = new KandraRegistrationPreflightStage().Validate(
                new KandraRegistrationPreflightRequest("fixture.kandra-registration-preflight", metadata, package));

            Check(result.RegistrationPreflightAccepted, "Registration preflight must accept matching metadata plus a validated loose package.");
            Check(result.PackageValidationAccepted, "Registration preflight must require accepted package validation.");
            Check(result.RuntimeSeamValid, "Registration preflight must retain the runtime seam validation.");
            Check(result.MetadataModDirectoryMatches && result.MetadataNameMatches, "Registration preflight must compare KandraMesh modDirectory/name metadata.");
            Check(result.MetadataLayoutVersionMatches, "Registration preflight must compare the recovered Kandra payload layout version.");
            Check(result.MeshDataByteCountMatches && result.IndicesDataByteCountMatches, "Registration preflight must compare metadata counts against persisted package byte counts.");
            Check(result.ExpectedMeshDataByteCount == KandraRuntimePayloadLayout.ExpectedMeshPayloadByteCount(2, 1, 1), "Registration preflight must expose expected mesh byte count.");
            Check(result.ExpectedIndicesDataByteCount == KandraRuntimePayloadLayout.ExpectedIndexPayloadByteCount(3), "Registration preflight must expose expected index byte count.");
            Check(result.MeshDataHashMatches && result.IndicesDataHashMatches, "Registration preflight must require validated package hashes.");
            Check(result.LiveKandraRegistrationProofPresent && result.LiveKandraMeshMemoryProofPresent, "Registration preflight must retain live Kandra proof.");
            Check(result.PackageDownstreamBoundaryFalse, "Registration preflight must require package downstream boundaries to be false.");
            Check(!result.CustomArmourRegistrationAllowed && !result.RuntimeRegistrationExecuted, "Registration preflight must not allow or execute custom registration.");
            Check(!result.CandidateMapApplicationExecuted && !result.ConversionExecuted, "Registration preflight must not apply candidate maps or conversion.");
            Check(!result.NativeGameWriteExecuted && !result.DownstreamWritesExecuted, "Registration preflight must remain no-write.");

            KandraRegistrationPreflightResult mismatch = new KandraRegistrationPreflightStage().Validate(
                new KandraRegistrationPreflightRequest(
                    "fixture.kandra-registration-preflight.mismatch",
                    CreateFixtureKandraRegistrationMetadata("FixtureMod", "Mesh_Fixture_Kandra_Wrong"),
                    package));
            Check(!mismatch.RegistrationPreflightAccepted, "Registration preflight must block mismatched metadata.");
            Check(
                mismatch.Blockers.Contains("kandra_registration_preflight_name_mismatch"),
                "Registration preflight must expose the metadata-name mismatch blocker.");
            Check(!mismatch.RuntimeRegistrationExecuted && !mismatch.DownstreamWritesExecuted, "Registration preflight mismatch must still remain no-write.");
        }
        finally
        {
            CleanupDirectory(fixtureRoot);
        }
    }

    private static void KandraRegistrationCandidateBuildsRequestAndStaysNoWrite()
    {
        string fixtureRoot = Path.Combine(Path.GetTempPath(), "ta-kandra-registration-candidate-" + Guid.NewGuid().ToString("N"));
        try
        {
            KandraWriterResult writer = WriteFixtureKandraPackage(fixtureRoot, "FixtureMod", "Mesh_Fixture_Kandra", CreateFixtureKandraPayload());
            ArmorImportResult proof = new ArmorImporter().Import(
                CreateRequest(
                    "kandra-registration-candidate-live-proof",
                    Baseline,
                    CreateVisualRuntimeObservation(
                        "observed:live FoA hero/body renderer context",
                        "observed:live FoA equip renderer context",
                        "blocked_unproven:tpp-target-animation-clip-binding",
                        "blocked_unobserved",
                        "observed:True:renderingId=1",
                        "observed:True")));
            KandraPackageValidationResult package = new KandraPackageValidationStage().Validate(
                new KandraPackageValidationRequest("fixture.kandra-registration-candidate.package", writer, proof));
            KandraRegistrationPreflightResult preflight = new KandraRegistrationPreflightStage().Validate(
                new KandraRegistrationPreflightRequest(
                    "fixture.kandra-registration-candidate.preflight",
                    CreateFixtureKandraRegistrationMetadata("FixtureMod", "Mesh_Fixture_Kandra"),
                    package));

            KandraRegistrationCandidateBuildResult result = new KandraRegistrationCandidateStage().Build(
                new KandraRegistrationCandidateBuildRequest("fixture.kandra-registration-candidate", preflight));

            Check(result.CandidateBuildAccepted, "Registration candidate stage must accept an accepted preflight.");
            Check(result.PreflightAccepted, "Registration candidate stage must retain accepted preflight status.");
            Check(result.CandidateRequestConstructed, "Registration candidate stage must construct the candidate request object.");
            Check(result.CandidateRequest != null, "Registration candidate request object must be present.");
            KandraCustomRegistrationCandidateRequest candidate = result.CandidateRequest!;
            Check(candidate.SourcePreflightRequestId == preflight.RequestId, "Registration candidate must cite the source preflight result.");
            Check(candidate.ModDirectory == preflight.ModDirectory && candidate.Name == preflight.Name, "Registration candidate must copy KandraMesh modDirectory/name from preflight.");
            Check(candidate.KandraMeshModDirectory == candidate.ModDirectory && candidate.KandraMeshName == candidate.Name, "Registration candidate must expose KandraMesh metadata aliases.");
            Check(candidate.VertexCount == preflight.VertexCount, "Registration candidate must copy vertex count from preflight.");
            Check(candidate.IndicesCount == preflight.IndicesCount, "Registration candidate must copy index count from preflight.");
            Check(candidate.BindposesCount == preflight.BindposesCount, "Registration candidate must copy bind-pose count from preflight.");
            Check(candidate.BlendshapeCount == preflight.BlendshapeCount, "Registration candidate must copy blend-shape count from preflight.");
            Check(candidate.LayoutVersion == preflight.LayoutVersion, "Registration candidate must copy payload layout version from preflight.");
            Check(candidate.MeshDataPath == preflight.MeshDataPath && candidate.IndicesDataPath == preflight.IndicesDataPath, "Registration candidate must copy validated loose package paths from preflight.");
            Check(candidate.MeshDataByteCount == preflight.ActualMeshDataByteCount, "Registration candidate must copy actual mesh payload byte count from preflight.");
            Check(candidate.IndicesDataByteCount == preflight.ActualIndicesDataByteCount, "Registration candidate must copy actual index payload byte count from preflight.");
            Check(candidate.MeshDataSha256 == preflight.MeshDataSha256 && candidate.IndicesDataSha256 == preflight.IndicesDataSha256, "Registration candidate must copy validated loose package hashes from preflight.");
            Check(!candidate.RuntimeRegistrationCallContractProven && !candidate.RuntimeRegistrationInvocationAllowed && !candidate.RuntimeRegistrationExecuted, "Registration candidate must not allow or execute runtime registration.");
            Check(!result.RuntimeRegistrationCallContractProven && !result.RuntimeRegistrationInvocationAllowed && !result.RuntimeRegistrationExecuted, "Registration candidate result must not allow or execute runtime registration.");
            Check(!candidate.CandidateMapApplicationExecuted && !candidate.ConversionExecuted, "Registration candidate must not apply candidate maps or conversion.");
            Check(!candidate.ItemEquipMutationExecuted && !candidate.SaveMutationExecuted, "Registration candidate must not mutate item/equip/save state.");
            Check(!candidate.NativeGameWriteExecuted && !candidate.DownstreamWritesExecuted, "Registration candidate must remain no-write.");
            Check(!result.CandidateMapApplicationExecuted && !result.ConversionExecuted, "Registration candidate result must not apply candidate maps or conversion.");
            Check(!result.NativeGameWriteExecuted && !result.DownstreamWritesExecuted, "Registration candidate result must remain no-write.");
            Check(result.Warnings.Contains("runtime_registration_call_contract_not_proven"), "Registration candidate result must retain the unproven runtime-call warning.");

            KandraRegistrationPreflightResult mismatchPreflight = new KandraRegistrationPreflightStage().Validate(
                new KandraRegistrationPreflightRequest(
                    "fixture.kandra-registration-candidate.preflight.mismatch",
                    CreateFixtureKandraRegistrationMetadata("FixtureMod", "Mesh_Fixture_Kandra_Wrong"),
                    package));
            KandraRegistrationCandidateBuildResult blocked = new KandraRegistrationCandidateStage().Build(
                new KandraRegistrationCandidateBuildRequest("fixture.kandra-registration-candidate.blocked", mismatchPreflight));
            Check(!blocked.CandidateBuildAccepted, "Registration candidate stage must block non-accepted preflight.");
            Check(!blocked.CandidateRequestConstructed && blocked.CandidateRequest == null, "Registration candidate stage must not construct a request from blocked preflight.");
            Check(blocked.Blockers.Contains("kandra_registration_candidate_preflight_not_accepted"), "Registration candidate stage must expose a blocked-preflight blocker.");
            Check(
                blocked.Blockers.Contains("preflight:kandra_registration_preflight_name_mismatch"),
                "Registration candidate stage must retain the preflight mismatch blocker.");
            Check(!blocked.RuntimeRegistrationInvocationAllowed && !blocked.RuntimeRegistrationExecuted, "Blocked registration candidate result must still remain no-write.");
        }
        finally
        {
            CleanupDirectory(fixtureRoot);
        }
    }

    private static void KandraRuntimeRegistrationDryRunConsumesContractAndStaysNoWrite()
    {
        string fixtureRoot = Path.Combine(Path.GetTempPath(), "ta-kandra-runtime-registration-dry-run-" + Guid.NewGuid().ToString("N"));
        try
        {
            KandraWriterResult writer = WriteFixtureKandraPackage(fixtureRoot, "FixtureMod", "Mesh_Fixture_Kandra", CreateFixtureKandraPayload());
            ArmorImportResult proof = new ArmorImporter().Import(
                CreateRequest(
                    "kandra-runtime-registration-dry-run-live-proof",
                    Baseline,
                    CreateVisualRuntimeObservation(
                        "observed:live FoA hero/body renderer context",
                        "observed:live FoA equip renderer context",
                        "blocked_unproven:tpp-target-animation-clip-binding",
                        "blocked_unobserved",
                        "observed:True:renderingId=1",
                        "observed:True")));
            KandraPackageValidationResult package = new KandraPackageValidationStage().Validate(
                new KandraPackageValidationRequest("fixture.kandra-runtime-registration-dry-run.package", writer, proof));
            KandraRegistrationPreflightResult preflight = new KandraRegistrationPreflightStage().Validate(
                new KandraRegistrationPreflightRequest(
                    "fixture.kandra-runtime-registration-dry-run.preflight",
                    CreateFixtureKandraRegistrationMetadata("FixtureMod", "Mesh_Fixture_Kandra"),
                    package));
            KandraRegistrationCandidateBuildResult candidate = new KandraRegistrationCandidateStage().Build(
                new KandraRegistrationCandidateBuildRequest("fixture.kandra-runtime-registration-dry-run.candidate", preflight));

            KandraRuntimeRegistrationDryRunResult result = new KandraRuntimeRegistrationDryRunStage().Evaluate(
                new KandraRuntimeRegistrationDryRunRequest(
                    "fixture.kandra-runtime-registration-dry-run",
                    candidate,
                    package,
                    CreateFixtureKandraRuntimeRegistrationContractFingerprint()));

            Check(result.RegistrationDryRunAccepted, "Runtime registration dry-run gate must accept validated package, candidate, and contract fingerprint.");
            Check(result.CandidateBuildAccepted && result.CandidateRequestPresent, "Runtime registration dry-run gate must require an accepted candidate request.");
            Check(result.PackageValidationAccepted, "Runtime registration dry-run gate must require accepted package validation.");
            Check(result.CandidateMatchesValidatedPackage, "Runtime registration dry-run gate must compare candidate metadata against the validated loose package.");
            Check(result.LoosePackageStillPresent && result.LoosePackageHashesStillMatch, "Runtime registration dry-run gate must re-check loose package presence and hashes.");
            Check(result.RuntimeRegistrationContractFingerprintAccepted, "Runtime registration dry-run gate must accept the observed live Kandra contract fingerprint.");
            Check(result.RuntimeRegistrationContractSurfacePresent, "Runtime registration dry-run gate must require the live registration member surface.");
            Check(result.RuntimeRegistrationContractNoWriteBoundaryFalse, "Runtime registration dry-run gate must require the contract diagnostic no-write boundary.");
            Check(result.RuntimeRegistrationCallContractProven, "Runtime registration dry-run gate must mark the call contract fingerprint as proven for dry-run planning.");
            Check(!result.ExplicitRuntimeInvocationApprovalPresent, "Runtime registration dry-run gate must not expose invocation approval yet.");
            Check(!result.RuntimeRegistrationInvocationAllowed, "Runtime registration dry-run gate must refuse runtime invocation.");
            Check(!result.RegistrationMethodInvoked && !result.CanRegisterMethodsInvoked, "Runtime registration dry-run gate must not invoke Register or CanRegister.");
            Check(!result.CreatedOrActivatedKandraObject && !result.RuntimeRegistrationExecuted, "Runtime registration dry-run gate must not create, activate, or register runtime objects.");
            Check(!result.CandidateMapApplicationExecuted && !result.ConversionExecuted, "Runtime registration dry-run gate must not apply candidate maps or conversion.");
            Check(!result.ItemEquipMutationExecuted && !result.SaveMutationExecuted, "Runtime registration dry-run gate must not mutate item/equip/save state.");
            Check(!result.NativeGameWriteExecuted && !result.DownstreamWritesExecuted, "Runtime registration dry-run gate must remain no-write.");
            Check(result.RegistrationOwner == KandraRuntimeRegistrationContractFingerprint.ExpectedManagerType, "Runtime registration dry-run gate must retain the live registration owner.");
            Check(result.RegistrationMethodSignature == KandraRuntimeRegistrationContractFingerprint.ExpectedRegistrationMethodSignature, "Runtime registration dry-run gate must retain the live Register(KandraRenderer) signature.");
            Check(result.RegistrationInvocationRefusalReason == "runtime_registration_invocation_approval_not_defined", "Runtime registration dry-run gate must record the approval blocker.");
            Check(result.Warnings.Contains("runtime_registration_invocation_refused"), "Runtime registration dry-run gate must warn that invocation is refused.");

            KandraRuntimeRegistrationDryRunResult invokedDiagnosticBlocked = new KandraRuntimeRegistrationDryRunStage().Evaluate(
                new KandraRuntimeRegistrationDryRunRequest(
                    "fixture.kandra-runtime-registration-dry-run.invoked-diagnostic",
                    candidate,
                    package,
                    CreateFixtureKandraRuntimeRegistrationContractFingerprint(registrationMethodInvoked: true)));
            Check(!invokedDiagnosticBlocked.RegistrationDryRunAccepted, "Runtime registration dry-run gate must block a contract diagnostic that claims Register was invoked.");
            Check(
                invokedDiagnosticBlocked.Blockers.Contains("kandra_runtime_registration_dry_run_contract_downstream_boundary_not_false"),
                "Runtime registration dry-run gate must expose the contract no-write boundary blocker.");
            Check(!invokedDiagnosticBlocked.RuntimeRegistrationInvocationAllowed && !invokedDiagnosticBlocked.RuntimeRegistrationExecuted, "Blocked runtime registration dry-run must still remain no-write.");

            KandraRuntimeRegistrationDryRunResult wrongFingerprintBlocked = new KandraRuntimeRegistrationDryRunStage().Evaluate(
                new KandraRuntimeRegistrationDryRunRequest(
                    "fixture.kandra-runtime-registration-dry-run.wrong-fingerprint",
                    candidate,
                    package,
                    CreateFixtureKandraRuntimeRegistrationContractFingerprint(registrationMethodFingerprint: "sha256:0000000000000000000000000000000000000000000000000000000000000000")));
            Check(!wrongFingerprintBlocked.RegistrationDryRunAccepted, "Runtime registration dry-run gate must block a contract diagnostic with an unknown Register fingerprint.");
            Check(
                wrongFingerprintBlocked.Blockers.Contains("kandra_runtime_registration_dry_run_contract_fingerprint_not_accepted"),
                "Runtime registration dry-run gate must expose the contract fingerprint blocker.");
        }
        finally
        {
            CleanupDirectory(fixtureRoot);
        }
    }

    private static void KandraRuntimeRegistrationInvocationRequiresExplicitApproval()
    {
        string fixtureRoot = Path.Combine(Path.GetTempPath(), "ta-kandra-runtime-registration-invocation-" + Guid.NewGuid().ToString("N"));
        try
        {
            KandraWriterResult writer = WriteFixtureKandraPackage(fixtureRoot, "FixtureMod", "Mesh_Fixture_Kandra", CreateFixtureKandraPayload());
            ArmorImportResult proof = new ArmorImporter().Import(
                CreateRequest(
                    "kandra-runtime-registration-invocation-live-proof",
                    Baseline,
                    CreateVisualRuntimeObservation(
                        "observed:live FoA hero/body renderer context",
                        "observed:live FoA equip renderer context",
                        "blocked_unproven:tpp-target-animation-clip-binding",
                        "blocked_unobserved",
                        "observed:True:renderingId=1",
                        "observed:True")));
            KandraPackageValidationResult package = new KandraPackageValidationStage().Validate(
                new KandraPackageValidationRequest("fixture.kandra-runtime-registration-invocation.package", writer, proof));
            KandraRegistrationPreflightResult preflight = new KandraRegistrationPreflightStage().Validate(
                new KandraRegistrationPreflightRequest(
                    "fixture.kandra-runtime-registration-invocation.preflight",
                    CreateFixtureKandraRegistrationMetadata("FixtureMod", "Mesh_Fixture_Kandra"),
                    package));
            KandraRegistrationCandidateBuildResult candidate = new KandraRegistrationCandidateStage().Build(
                new KandraRegistrationCandidateBuildRequest("fixture.kandra-runtime-registration-invocation.candidate", preflight));
            KandraRuntimeRegistrationDryRunResult dryRun = new KandraRuntimeRegistrationDryRunStage().Evaluate(
                new KandraRuntimeRegistrationDryRunRequest(
                    "fixture.kandra-runtime-registration-invocation.dry-run",
                    candidate,
                    package,
                    CreateFixtureKandraRuntimeRegistrationContractFingerprint()));

            Check(dryRun.RegistrationDryRunAccepted, "Runtime registration invocation fixture must start from an accepted dry-run boundary.");

            var fakeInvoker = new FakeKandraRuntimeRegistrationInvoker();
            KandraRuntimeRegistrationInvocationResult refused = new KandraRuntimeRegistrationInvocationStage().Invoke(
                new KandraRuntimeRegistrationInvocationRequest(
                    "fixture.kandra-runtime-registration-invocation.refused",
                    dryRun,
                    CreateFixtureKandraRuntimeInvocationApproval(dryRun, approved: false),
                    fakeInvoker));

            Check(!refused.RuntimeRegistrationInvocationAllowed, "Runtime registration invocation must refuse missing explicit approval.");
            Check(!refused.RegistrationMethodInvoked && !refused.RuntimeRegistrationExecuted, "Refused runtime registration invocation must not call Register.");
            Check(fakeInvoker.InvocationCount == 0, "Refused runtime registration invocation must not call the host invoker.");
            Check(
                refused.Blockers.Contains("kandra_runtime_registration_invocation_explicit_approval_missing"),
                "Refused runtime registration invocation must expose the missing approval blocker.");

            KandraRuntimeRegistrationInvocationResult approved = new KandraRuntimeRegistrationInvocationStage().Invoke(
                new KandraRuntimeRegistrationInvocationRequest(
                    "fixture.kandra-runtime-registration-invocation.approved",
                    dryRun,
                    CreateFixtureKandraRuntimeInvocationApproval(dryRun, approved: true),
                    fakeInvoker));

            Check(approved.RuntimeRegistrationInvocationAllowed, "Approved runtime registration invocation must pass the guarded invocation gate.");
            Check(approved.ExplicitRuntimeInvocationApprovalPresent, "Approved runtime registration invocation must retain explicit approval.");
            Check(approved.ApprovalMatchesDryRun, "Approved runtime registration invocation must match the dry-run identity.");
            Check(approved.ApprovalKeepsNonRegistrationWritesFalse, "Approved runtime registration invocation must keep non-registration writes false.");
            Check(approved.InvokerPresent, "Approved runtime registration invocation must require a host invoker.");
            Check(fakeInvoker.InvocationCount == 1, "Approved runtime registration invocation must call the host invoker exactly once.");
            Check(approved.RegistrationMethodInvoked, "Approved runtime registration invocation must retain the Register invocation marker.");
            Check(approved.CreatedOrActivatedKandraObject, "Approved runtime registration invocation must retain the host object-creation marker.");
            Check(approved.RuntimeRegistrationExecuted && approved.RuntimeRegistrationSucceeded, "Approved runtime registration invocation must retain runtime registration success.");
            Check(approved.KandraIsRegisteredStatus.StartsWith("observed:True", StringComparison.OrdinalIgnoreCase), "Approved runtime registration invocation must retain IsRegistered proof.");
            Check(approved.KandraTryGetMeshMemoryStatus.StartsWith("observed:True", StringComparison.OrdinalIgnoreCase), "Approved runtime registration invocation must retain TryGetMeshMemory proof.");
            Check(!approved.CandidateMapApplicationExecuted && !approved.ConversionExecuted, "Approved runtime registration invocation must not apply candidate maps or conversion.");
            Check(!approved.ItemEquipMutationExecuted && !approved.SaveMutationExecuted && !approved.NativeGameWriteExecuted, "Approved runtime registration invocation must not mutate item/equip/save/native game files.");
            Check(!approved.NonRegistrationWritesExecuted, "Approved runtime registration invocation must keep non-registration writes false.");

            KandraRuntimeRegistrationInvocationResult mismatchedApproval = new KandraRuntimeRegistrationInvocationStage().Invoke(
                new KandraRuntimeRegistrationInvocationRequest(
                    "fixture.kandra-runtime-registration-invocation.mismatched-approval",
                    dryRun,
                    CreateFixtureKandraRuntimeInvocationApproval(
                        dryRun,
                        approved: true,
                        registrationMethodFingerprint: "sha256:0000000000000000000000000000000000000000000000000000000000000000"),
                    fakeInvoker));

            Check(!mismatchedApproval.RuntimeRegistrationInvocationAllowed, "Runtime registration invocation must refuse mismatched approval fingerprints.");
            Check(fakeInvoker.InvocationCount == 1, "Mismatched runtime registration approval must not call the host invoker.");
            Check(
                mismatchedApproval.Blockers.Contains("kandra_runtime_registration_invocation_approval_mismatch:registration_method_fingerprint"),
                "Mismatched runtime registration approval must expose the fingerprint mismatch blocker.");
        }
        finally
        {
            CleanupDirectory(fixtureRoot);
        }
    }

    private static void KandraHostRegistrationProofConsumesLiveReceiptArtifact()
    {
        KandraRuntimeRegistrationHostProofResult result = ValidateLiveRegistrationHostProofArtifact();

        Check(result.HostInvocationProofAccepted, "Live host-registration receipt must be accepted as the first proven host invocation proof.");
        Check(result.ReceiptStageVersionAccepted, "Host proof must accept the FoA host-invoker receipt stage version.");
        Check(result.RegistrationMethodFingerprintAccepted, "Host proof must retain the recovered Register(KandraRenderer) fingerprint.");
        Check(result.RegistrationAssemblyFingerprintAccepted, "Host proof must retain the Awaken.Kandra assembly fingerprint.");
        Check(result.ContextCountsValid, "Host proof must retain valid Kandra context counts.");
        Check(result.ModDirectory == "AvalonAwakenedProof", "Host proof must retain the live proof modDirectory.");
        Check(result.Name == "RealSkinnedTriangle", "Host proof must retain the live proof Kandra name.");
        Check(result.VertexCount == 3 && result.IndicesCount == 3, "Host proof must retain RealSkinnedTriangle geometry counts.");
        Check(result.BindposesCount == 4 && result.BlendshapeCount == 0, "Host proof must retain RealSkinnedTriangle bind-pose/blend-shape counts.");
        Check(result.RegistrationMethodInvoked, "Host proof must retain the Register invocation marker.");
        Check(!result.CanRegisterMethodsInvoked, "Host proof must retain that CanRegister methods were not invoked.");
        Check(result.CreatedOrActivatedKandraObject, "Host proof must retain the runtime object creation/activation marker.");
        Check(result.HostRuntimeRegistrationExecuted && result.HostRuntimeRegistrationSucceeded, "Host proof must retain live runtime registration success.");
        Check(result.KandraIsRegisteredProofPresent, "Host proof must retain IsRegistered=True evidence.");
        Check(result.KandraMeshMemoryProofPresent, "Host proof must retain TryGetMeshMemory=True evidence.");
        Check(result.AttemptBlockerFree, "Host proof must require a blocker-free host attempt.");
        Check(result.NonRegistrationDownstreamBoundaryFalse, "Host proof must require all non-registration downstream writes false.");
        Check(!result.CandidateMapApplicationExecuted && !result.ConversionExecuted, "Host proof consumer must not apply candidate maps or conversion.");
        Check(!result.ItemEquipMutationExecuted && !result.SaveMutationExecuted, "Host proof consumer must not mutate item/equip/save state.");
        Check(!result.NativeGameWriteExecuted && !result.DownstreamWritesExecuted, "Host proof consumer must not write downstream state.");
    }

    private static void KandraTargetRuntimeRegistrationPlanRefusesProofPackageReuseAndBuildsTargetContext()
    {
        KandraRuntimeRegistrationHostProofResult hostProof = ValidateLiveRegistrationHostProofArtifact();
        string fixtureRoot = Path.Combine(Path.GetTempPath(), "ta-kandra-target-runtime-registration-plan-" + Guid.NewGuid().ToString("N"));
        try
        {
            KandraRuntimeRegistrationDryRunResult targetDryRun = CreateFixtureKandraRuntimeRegistrationDryRun(
                fixtureRoot,
                "TargetArmourProofMod",
                "Mesh_Target_Armour_RuntimePackage",
                "fixture.kandra-target-runtime-registration-plan.target");
            Check(targetDryRun.RegistrationDryRunAccepted, "Target runtime registration plan fixture must start from an accepted target dry-run package.");

            KandraTargetRuntimeRegistrationPlanResult result = new KandraTargetRuntimeRegistrationPlanStage().Plan(
                new KandraTargetRuntimeRegistrationPlanRequest(
                    "fixture.kandra-target-runtime-registration-plan",
                    hostProof,
                    targetDryRun,
                    CreateFixtureKandraRuntimeInvocationApproval(targetDryRun, approved: true)));

            Check(result.TargetRuntimeRegistrationPlanAccepted, "Target runtime registration plan must accept host proof, target dry-run, and matching approval.");
            Check(result.HostInvocationProofAccepted, "Target plan must require accepted host invocation proof.");
            Check(result.TargetDryRunAccepted, "Target plan must require an accepted target dry-run.");
            Check(result.TargetRuntimeRegistrationCallContractProven, "Target plan must require the proven runtime registration contract.");
            Check(result.TargetPackageDiffersFromHostProofIdentity, "Target plan must require a target package identity different from the RealSkinnedTriangle proof.");
            Check(result.TargetPayloadDiffersFromHostProofPayload, "Target plan must require target payload hashes different from the RealSkinnedTriangle proof payload.");
            Check(result.ExplicitRuntimeInvocationApprovalPresent, "Target plan must require explicit runtime invocation approval.");
            Check(result.ApprovalMatchesTargetDryRun, "Target plan must pin approval to the target dry-run identity.");
            Check(result.ApprovalKeepsNonRegistrationWritesFalse, "Target plan approval must keep all non-registration writes false.");
            Check(result.TargetInvocationContextConstructed && result.TargetInvocationContext != null, "Target plan must construct the target invocation context.");
            Check(result.RuntimeRegistrationInvocationAllowed, "Target plan must allow only the prepared target runtime-registration invocation boundary.");
            Check(!result.RegistrationMethodInvoked && !result.RuntimeRegistrationExecuted, "Target plan must not call Register or execute runtime registration.");
            Check(!result.CandidateMapApplicationExecuted && !result.ConversionExecuted, "Target plan must not apply candidate maps or conversion.");
            Check(!result.ItemEquipMutationExecuted && !result.SaveMutationExecuted, "Target plan must not mutate item/equip/save state.");
            Check(!result.NativeGameWriteExecuted && !result.DownstreamWritesExecuted, "Target plan must not write downstream state.");

            KandraRuntimeRegistrationInvocationContext targetContext = result.TargetInvocationContext!;
            Check(targetContext.ModDirectory == targetDryRun.ModDirectory, "Target invocation context must carry the target modDirectory.");
            Check(targetContext.Name == targetDryRun.Name, "Target invocation context must carry the target Kandra name.");
            Check(targetContext.MeshDataSha256 == targetDryRun.MeshDataSha256, "Target invocation context must carry target mesh payload hash.");
            Check(targetContext.IndicesDataSha256 == targetDryRun.IndicesDataSha256, "Target invocation context must carry target index payload hash.");
            Check(targetContext.RegistrationMethodFingerprint == targetDryRun.RegistrationMethodFingerprint, "Target invocation context must carry the recovered Register fingerprint.");

            KandraRuntimeRegistrationDryRunResult proofIdentityDryRun = CreateFixtureKandraRuntimeRegistrationDryRun(
                fixtureRoot,
                "AvalonAwakenedProof",
                "RealSkinnedTriangle",
                "fixture.kandra-target-runtime-registration-plan.proof-identity");
            KandraTargetRuntimeRegistrationPlanResult proofIdentityBlocked = new KandraTargetRuntimeRegistrationPlanStage().Plan(
                new KandraTargetRuntimeRegistrationPlanRequest(
                    "fixture.kandra-target-runtime-registration-plan.proof-identity",
                    hostProof,
                    proofIdentityDryRun,
                    CreateFixtureKandraRuntimeInvocationApproval(proofIdentityDryRun, approved: true)));

            Check(!proofIdentityBlocked.TargetRuntimeRegistrationPlanAccepted, "Target plan must refuse reusing the RealSkinnedTriangle proof package identity.");
            Check(!proofIdentityBlocked.TargetInvocationContextConstructed && proofIdentityBlocked.TargetInvocationContext == null, "Blocked proof-package reuse must not construct a target context.");
            Check(
                proofIdentityBlocked.Blockers.Contains("kandra_target_runtime_registration_plan_reuses_host_proof_package_identity"),
                "Target plan must expose the proof-package identity reuse blocker.");
            Check(!proofIdentityBlocked.RegistrationMethodInvoked && !proofIdentityBlocked.RuntimeRegistrationExecuted, "Blocked proof-package reuse must not execute runtime registration.");
            Check(!proofIdentityBlocked.DownstreamWritesExecuted, "Blocked proof-package reuse must keep downstream writes false.");
        }
        finally
        {
            CleanupDirectory(fixtureRoot);
        }
    }

    private static void KandraSameMeshAbProofBuildsControlledPackageVariants()
    {
        string fixtureRoot = Path.Combine(Path.GetTempPath(), "ta-kandra-same-mesh-ab-proof-" + Guid.NewGuid().ToString("N"));
        try
        {
            byte[] sourceMeshBytes = CreateSameMeshAbSourcePayload();
            var source = new KandraSameMeshAbProofSourceSnapshot(
                "Mesh_Fixture_Source_Kandra",
                4,
                1,
                Array.Empty<string>(),
                sourceMeshBytes,
                new ushort[] { 0, 1, 2, 0, 2, 3 });
            KandraSameMeshAbProofResult result = new KandraSameMeshAbProofStage().Build(
                new KandraSameMeshAbProofRequest(
                    "fixture.kandra-same-mesh-ab-proof",
                    fixtureRoot,
                    "FixtureSameMeshAb",
                    "Mesh_SameMeshAb",
                    "fixture-approval-same-mesh-ab",
                    source,
                    CreateFixtureKandraRuntimeRegistrationContractFingerprint()));

            Check(result.ProofPackageSetAccepted, "Same-mesh A/B stage must accept the four controlled package variants.");
            Check(result.SourcePayloadLengthAccepted, "Same-mesh A/B stage must validate source payload length against recovered Kandra layout.");
            Check(result.ContractFingerprintAccepted && result.ContractSurfacePresent, "Same-mesh A/B stage must require the recovered live registration contract fingerprint.");
            Check(result.ContractNoWriteBoundaryFalse, "Same-mesh A/B stage must require the no-write contract diagnostic boundary.");
            Check(result.Variants.Count == 4, "Same-mesh A/B stage must produce exactly four variants.");
            Check(!result.CandidateMapApplicationExecuted && !result.ConversionExecuted, "Same-mesh A/B stage must not apply candidate maps or conversion.");
            Check(!result.RuntimeRegistrationExecuted && !result.DownstreamWritesExecuted, "Same-mesh A/B stage must not invoke runtime registration or downstream writes.");

            KandraSameMeshAbProofVariantResult original = result.Variants.Single(variant => variant.VariantId == KandraSameMeshAbProofStage.OriginalBytesVariantId);
            KandraSameMeshAbProofVariantResult duplicateNormal = result.Variants.Single(variant => variant.VariantId == KandraSameMeshAbProofStage.DuplicateNormalPlus16VariantId);
            KandraSameMeshAbProofVariantResult bar = result.Variants.Single(variant => variant.VariantId == KandraSameMeshAbProofStage.BarGeometricPlus16VariantId);
            KandraSameMeshAbProofVariantResult recovered = result.Variants.Single(variant => variant.VariantId == KandraSameMeshAbProofStage.RecoveredOctahedralRoundtripPlus16VariantId);

            Check(original.SourceMeshPayloadPreservedExactly, "Original variant must preserve the runtime mesh bytes exactly.");
            Check(original.SourceIndicesPayloadPreservedExactly, "Original variant must preserve the runtime index bytes exactly.");
            Check(original.ChangedByteCount == 0, "Original variant must report zero changed mesh bytes.");
            Check(File.ReadAllBytes(original.WriterResult.MeshDataPath).SequenceEqual(sourceMeshBytes), "Original variant file must equal the input mesh bytes.");

            Check(duplicateNormal.SourceIndicesPayloadPreservedExactly, "Duplicate-normal variant must preserve the index bytes.");
            Check(duplicateNormal.OnlyCompressedVertexPlus16Changed, "Duplicate-normal variant must change only compressed-vertex +16 bytes.");
            Check(duplicateNormal.ChangedNonPlus16ByteCount == 0, "Duplicate-normal variant must report no non-+16 byte changes.");
            Check(duplicateNormal.ChangedPlus16FieldCount == 4, "Duplicate-normal variant must touch the +16 field for every fixture vertex.");
            byte[] duplicateBytes = File.ReadAllBytes(duplicateNormal.WriterResult.MeshDataPath);
            for (int vertex = 0; vertex < 4; vertex++)
            {
                int offset = vertex * KandraRuntimePayloadLayout.CompressedVertexByteSize;
                Check(
                    duplicateBytes.Skip(offset + 16).Take(4).SequenceEqual(sourceMeshBytes.Skip(offset + 12).Take(4)),
                    "Duplicate-normal variant must copy encoded normal +12 into +16.");
            }

            Check(bar.SourceIndicesPayloadPreservedExactly, "BAR geometric variant must preserve the index bytes.");
            Check(bar.OnlyCompressedVertexPlus16Changed, "BAR geometric variant must change only compressed-vertex +16 bytes.");
            Check(bar.ChangedNonPlus16ByteCount == 0, "BAR geometric variant must report no non-+16 byte changes.");
            Check(recovered.SourceIndicesPayloadPreservedExactly, "Recovered roundtrip variant must preserve the index bytes.");
            Check(recovered.OnlyCompressedVertexPlus16Changed, "Recovered roundtrip variant must change only compressed-vertex +16 bytes.");
            Check(recovered.ChangedNonPlus16ByteCount == 0, "Recovered roundtrip variant must report no non-+16 byte changes.");

            foreach (KandraSameMeshAbProofVariantResult variant in result.Variants)
            {
                Check(variant.InvocationContext.ModDirectory == "FixtureSameMeshAb", "Each A/B variant context must carry the requested modDirectory.");
                Check(variant.InvocationContext.Name == variant.MeshName, "Each A/B variant context must carry the variant mesh name.");
                Check(variant.InvocationContext.VertexCount == 4 && variant.InvocationContext.IndicesCount == 6, "Each A/B variant context must carry same-mesh counts.");
                Check(variant.InvocationContext.BindposesCount == 1 && variant.InvocationContext.BlendshapeCount == 0, "Each A/B variant context must carry bindpose/blendshape counts.");
                Check(variant.InvocationContext.RegistrationMethodFingerprint == KandraRuntimeRegistrationContractFingerprint.ExpectedRegistrationMethodFingerprint, "Each A/B variant context must carry the recovered Register fingerprint.");
                Check(!variant.CandidateMapApplicationExecuted && !variant.ConversionExecuted, "A/B variant must not apply candidate maps or conversion.");
                Check(!variant.RuntimeRegistrationExecuted && !variant.DownstreamWritesExecuted, "A/B variant must not invoke registration or downstream writes.");
            }
        }
        finally
        {
            CleanupDirectory(fixtureRoot);
        }
    }

    private static void KandraSameMeshVisualDecodeComparisonBlocksWithoutVisualEvidence()
    {
        string fixtureRoot = Path.Combine(Path.GetTempPath(), "ta-kandra-same-mesh-visual-decode-" + Guid.NewGuid().ToString("N"));
        try
        {
            byte[] sourceMeshBytes = CreateSameMeshAbSourcePayload();
            var source = new KandraSameMeshAbProofSourceSnapshot(
                "Mesh_Fixture_Source_Kandra",
                4,
                1,
                Array.Empty<string>(),
                sourceMeshBytes,
                new ushort[] { 0, 1, 2, 0, 2, 3 });
            KandraSameMeshAbProofResult build = new KandraSameMeshAbProofStage().Build(
                new KandraSameMeshAbProofRequest(
                    "fixture.kandra-same-mesh-visual-decode",
                    fixtureRoot,
                    "FixtureSameMeshVisualDecode",
                    "Mesh_SameMeshVisualDecode",
                    "fixture-approval-same-mesh-visual-decode",
                    source,
                    CreateFixtureKandraRuntimeRegistrationContractFingerprint()));
            KandraSameMeshAbProofVariantResult recovered = build.Variants.Single(variant => variant.VariantId == KandraSameMeshAbProofStage.RecoveredOctahedralRoundtripPlus16VariantId);

            KandraSameMeshVisualDecodeComparisonResult blocked = new KandraSameMeshVisualDecodeComparisonStage().Evaluate(
                new KandraSameMeshVisualDecodeComparisonRequest(
                    "fixture.kandra-same-mesh-visual-decode.blocked",
                    "fixture://same-mesh-ab-proof.receipt.json",
                    build.RequestId,
                    build.SourceMeshName,
                    build.VertexCount,
                    runtimeProofSucceeded: true,
                    receiptVariantCount: build.Variants.Count,
                    nonRegistrationDownstreamBoundaryFalse: true,
                    roundtripChangedPlus16FieldTolerance: recovered.ChangedPlus16FieldCount,
                    candidates: build.Variants.Select(variant => CreateSameMeshComparisonCandidate(variant, selectedVisualVariantId: string.Empty, visualEvidenceReviewed: false))));

            Check(blocked.HostRegistrationProofAccepted, "Comparison must accept the prior same-mesh host registration proof.");
            Check(blocked.DecodeComparisonAccepted, "Comparison must accept byte/decode inputs before visual decision.");
            Check(!blocked.VisualEvidenceAccepted, "Comparison must not accept a production encoder without visual evidence.");
            Check(!blocked.ProductionEncoderDecisionAccepted, "Comparison must block production encoder choice without visual evidence.");
            Check(blocked.DecisionStatus == KandraSameMeshVisualDecodeComparisonStage.BlockedVisualEvidenceMissing, "Comparison must report the exact visual-evidence blocker.");
            Check(blocked.Blockers.Contains("kandra_same_mesh_visual_decode_visual_evidence_missing"), "Comparison must retain visual-evidence blocker.");
            Check(blocked.NonRegistrationDownstreamBoundaryFalse, "Comparison must keep all downstream writes false.");

            KandraSameMeshVisualDecodeComparisonResult accepted = new KandraSameMeshVisualDecodeComparisonStage().Evaluate(
                new KandraSameMeshVisualDecodeComparisonRequest(
                    "fixture.kandra-same-mesh-visual-decode.accepted",
                    "fixture://same-mesh-ab-proof.receipt.json",
                    build.RequestId,
                    build.SourceMeshName,
                    build.VertexCount,
                    runtimeProofSucceeded: true,
                    receiptVariantCount: build.Variants.Count,
                    nonRegistrationDownstreamBoundaryFalse: true,
                    roundtripChangedPlus16FieldTolerance: recovered.ChangedPlus16FieldCount,
                    candidates: build.Variants.Select(variant => CreateSameMeshComparisonCandidate(variant, KandraSameMeshAbProofStage.DuplicateNormalPlus16VariantId, visualEvidenceReviewed: true))));

            Check(accepted.ProductionEncoderDecisionAccepted, "Comparison must accept exactly one visually approved non-original encoder.");
            Check(accepted.SelectedEncoderVariantId == KandraSameMeshAbProofStage.DuplicateNormalPlus16VariantId, "Comparison must retain the visually selected encoder variant.");
            Check(accepted.DecisionStatus == KandraSameMeshVisualDecodeComparisonStage.AcceptedPrefix + KandraSameMeshAbProofStage.DuplicateNormalPlus16VariantId, "Comparison must report accepted encoder status.");
            Check(accepted.NonRegistrationDownstreamBoundaryFalse, "Accepted comparison must still keep downstream writes false.");

            KandraSameMeshVisualDecodeComparisonResult rejected = new KandraSameMeshVisualDecodeComparisonStage().Evaluate(
                new KandraSameMeshVisualDecodeComparisonRequest(
                    "fixture.kandra-same-mesh-visual-decode.rejected",
                    "fixture://same-mesh-ab-proof.receipt.json",
                    build.RequestId,
                    build.SourceMeshName,
                    build.VertexCount,
                    runtimeProofSucceeded: true,
                    receiptVariantCount: build.Variants.Count,
                    nonRegistrationDownstreamBoundaryFalse: true,
                    roundtripChangedPlus16FieldTolerance: recovered.ChangedPlus16FieldCount,
                    candidates: build.Variants.Select(variant => CreateSameMeshComparisonCandidate(variant, selectedVisualVariantId: string.Empty, visualEvidenceReviewed: true))));

            Check(!rejected.ProductionEncoderDecisionAccepted, "Comparison must not accept a production encoder when every visual candidate is rejected.");
            Check(rejected.VisualEvidenceRejectedAllCandidates, "Comparison must explicitly preserve the reviewed all-candidates-rejected visual decision.");
            Check(rejected.DecisionStatus == KandraSameMeshVisualDecodeComparisonStage.RejectedAllCandidates, "Comparison must report the exact all-candidates-rejected status.");
            Check(rejected.NonRegistrationDownstreamBoundaryFalse, "Rejected comparison must still keep downstream writes false.");
        }
        finally
        {
            CleanupDirectory(fixtureRoot);
        }
    }

    private static KandraSameMeshVisualDecodeComparisonCandidate CreateSameMeshComparisonCandidate(
        KandraSameMeshAbProofVariantResult variant,
        string selectedVisualVariantId,
        bool visualEvidenceReviewed)
    {
        bool selected = string.Equals(variant.VariantId, selectedVisualVariantId, StringComparison.Ordinal);
        bool captured = visualEvidenceReviewed;
        return new KandraSameMeshVisualDecodeComparisonCandidate(
            variant.VariantId,
            variant.VariantLabel,
            runtimeRegistrationSucceeded: true,
            kandraIsRegisteredStatus: "observed:True:fixture",
            kandraTryGetMeshMemoryStatus: "observed:True:fixture",
            variant.SourceMeshPayloadPreservedExactly,
            variant.SourceIndicesPayloadPreservedExactly,
            variant.OnlyCompressedVertexPlus16Changed,
            variant.ChangedNonPlus16ByteCount,
            variant.ChangedPlus16FieldCount,
            visualEvidenceCaptured: captured,
            visualEvidenceReviewed: visualEvidenceReviewed,
            visuallyAccepted: selected,
            visualEvidenceArtifactId: captured ? "fixture://same-mesh-visual-evidence/" + variant.VariantId + ".png" : string.Empty);
    }

    private static KandraMeshPayloadSections CreateFixtureKandraPayload() =>
        new KandraMeshPayloadSections(
            Bytes(KandraRuntimePayloadLayout.CompressedVertexByteSize * 2, 1),
            Bytes(KandraRuntimePayloadLayout.AdditionalVertexDataByteSize * 2, 2),
            Bytes(KandraRuntimePayloadLayout.PackedBonesWeightsByteSize * 2, 3),
            Bytes(KandraRuntimePayloadLayout.Float3x4ByteSize, 4),
            new[]
            {
                new KandraBlendshapePayload("fixture_blendshape", Bytes(KandraRuntimePayloadLayout.PackedBlendshapeDatumByteSize * 2, 5)),
            });

    private static KandraMeshRegistrationMetadata CreateFixtureKandraRegistrationMetadata(string modDirectory, string name) =>
        new KandraMeshRegistrationMetadata(
            modDirectory,
            name,
            2,
            3,
            1,
            1,
            KandraRuntimePayloadLayout.LayoutVersion);

    private static KandraRuntimeRegistrationContractFingerprint CreateFixtureKandraRuntimeRegistrationContractFingerprint(
        bool registrationMethodInvoked = false,
        string status = KandraRuntimeRegistrationContractFingerprint.ObservedStatus,
        string registrationMethodSignature = KandraRuntimeRegistrationContractFingerprint.ExpectedRegistrationMethodSignature,
        string registrationMethodFingerprint = KandraRuntimeRegistrationContractFingerprint.ExpectedRegistrationMethodFingerprint,
        string assemblySha256 = KandraRuntimeRegistrationContractFingerprint.ExpectedAssemblySha256) =>
        new KandraRuntimeRegistrationContractFingerprint(
            status,
            KandraRuntimeRegistrationContractFingerprint.ExpectedAssemblyName,
            "fixture://Awaken.Kandra.dll",
            assemblySha256,
            KandraRuntimeRegistrationContractFingerprint.ExpectedManagerType,
            managerInstancePresent: true,
            KandraRuntimeRegistrationContractFingerprint.ExpectedManagerType,
            registrationMethodSignature,
            registrationMethodFingerprint,
            registrationMethodPresent: true,
            registrationMethodPublic: true,
            KandraRuntimeRegistrationContractFingerprint.ExpectedRendererType,
            rendererTypePresent: true,
            rendererOnEnableMethodPresent: true,
            rendererOnDisableMethodPresent: true,
            rendererDataFieldPresent: true,
            rendererRenderingIdPropertyPresent: true,
            requiredRendererDataMembersPresent: true,
            requiredKandraMeshMembersPresent: true,
            managerDependencyMethodsPresent: true,
            finalizeRegistrationMethodPresent: true,
            onEarlyUpdateBeginMethodPresent: true,
            onBeginRenderingMethodPresent: true,
            queueStateFieldsPresent: true,
            "reflectionOnly=true,registerInvoked=false,canRegisterInvoked=false,createdOrActivatedKandraObject=false,candidateMapApplicationExecuted=false,conversionExecuted=false,itemEquipMutationExecuted=false,saveMutationExecuted=false,nativeGameWriteExecuted=false,downstreamWritesExecuted=false",
            registrationMethodInvoked,
            canRegisterMethodsInvoked: false,
            createdOrActivatedKandraObject: false,
            runtimeRegistrationInvocationAllowed: false,
            runtimeRegistrationExecuted: false,
            candidateMapApplicationExecuted: false,
            conversionExecuted: false,
            itemEquipMutationExecuted: false,
            saveMutationExecuted: false,
            nativeGameWriteExecuted: false,
            downstreamWritesExecuted: false);

    private static KandraRuntimeInvocationApproval CreateFixtureKandraRuntimeInvocationApproval(
        KandraRuntimeRegistrationDryRunResult dryRun,
        bool approved,
        string? registrationMethodFingerprint = null) =>
        new KandraRuntimeInvocationApproval(
            "fixture-approval-" + (approved ? "approved" : "refused"),
            "fixture",
            "2026-08-13T00:00:00Z",
            approved,
            dryRun.RequestId,
            dryRun.ModDirectory,
            dryRun.Name,
            dryRun.MeshDataSha256,
            dryRun.IndicesDataSha256,
            registrationMethodFingerprint ?? dryRun.RegistrationMethodFingerprint,
            dryRun.RegistrationAssemblySha256,
            allowRuntimeRegistrationInvocation: approved,
            allowCandidateMapApplication: false,
            allowConversion: false,
            allowItemEquipMutation: false,
            allowSaveMutation: false,
            allowNativeGameWrite: false);

    private static KandraRuntimeRegistrationDryRunResult CreateFixtureKandraRuntimeRegistrationDryRun(
        string fixtureRoot,
        string modDirectory,
        string meshName,
        string requestId)
    {
        KandraWriterResult writer = WriteFixtureKandraPackage(fixtureRoot, modDirectory, meshName, CreateFixtureKandraPayload());
        ArmorImportResult proof = new ArmorImporter().Import(
            CreateRequest(
                requestId + ".live-proof",
                Baseline,
                CreateVisualRuntimeObservation(
                    "observed:live FoA hero/body renderer context",
                    "observed:live FoA equip renderer context",
                    "blocked_unproven:tpp-target-animation-clip-binding",
                    "blocked_unobserved",
                    "observed:True:renderingId=1",
                    "observed:True")));
        KandraPackageValidationResult package = new KandraPackageValidationStage().Validate(
            new KandraPackageValidationRequest(requestId + ".package", writer, proof));
        KandraRegistrationPreflightResult preflight = new KandraRegistrationPreflightStage().Validate(
            new KandraRegistrationPreflightRequest(
                requestId + ".preflight",
                CreateFixtureKandraRegistrationMetadata(modDirectory, meshName),
                package));
        KandraRegistrationCandidateBuildResult candidate = new KandraRegistrationCandidateStage().Build(
            new KandraRegistrationCandidateBuildRequest(requestId + ".candidate", preflight));
        return new KandraRuntimeRegistrationDryRunStage().Evaluate(
            new KandraRuntimeRegistrationDryRunRequest(
                requestId + ".dry-run",
                candidate,
                package,
                CreateFixtureKandraRuntimeRegistrationContractFingerprint()));
    }

    private static KandraWriterResult WriteFixtureKandraPackage(
        string fixtureRoot,
        string modDirectory,
        string meshName,
        KandraMeshPayloadSections payload)
    {
        var request = new KandraWriterRequest(
            "fixture.kandra-writer",
            fixtureRoot,
            modDirectory,
            meshName,
            2,
            1,
            payload,
            new ushort[] { 0, 1, 0 });

        return new KandraWriterStage().Write(request);
    }

    private sealed class FakeKandraRuntimeRegistrationInvoker : IKandraRuntimeRegistrationInvoker
    {
        public int InvocationCount { get; private set; }

        public KandraRuntimeRegistrationInvocationAttempt Invoke(KandraRuntimeRegistrationInvocationContext context)
        {
            InvocationCount++;
            Check(context.ModDirectory == "FixtureMod", "Invocation context must carry the approved modDirectory.");
            Check(context.Name == "Mesh_Fixture_Kandra", "Invocation context must carry the approved Kandra mesh name.");
            Check(context.VertexCount == 2, "Invocation context must carry the accepted vertex count.");
            Check(context.IndicesCount == 3, "Invocation context must carry the accepted index count.");
            Check(context.BindposesCount == 1, "Invocation context must carry the accepted bind-pose count.");
            Check(context.BlendshapeCount == 1, "Invocation context must carry the accepted blend-shape count.");
            Check(
                context.LayoutVersion == KandraRuntimePayloadLayout.LayoutVersion,
                "Invocation context must carry the accepted Kandra payload layout version.");
            Check(
                context.RegistrationMethodFingerprint == KandraRuntimeRegistrationContractFingerprint.ExpectedRegistrationMethodFingerprint,
                "Invocation context must carry the accepted Register fingerprint.");

            return new KandraRuntimeRegistrationInvocationAttempt(
                registrationMethodInvoked: true,
                canRegisterMethodsInvoked: false,
                createdOrActivatedKandraObject: true,
                runtimeRegistrationExecuted: true,
                runtimeRegistrationSucceeded: true,
                kandraIsRegisteredStatus: "observed:True:renderingId=99",
                kandraTryGetMeshMemoryStatus: "observed:True:meshMemory=fixture",
                registeredRendererIdentity: "fixture://kandra-renderer/99",
                registeredMeshMemoryIdentity: "fixture://mesh-memory/Mesh_Fixture_Kandra",
                blockers: Array.Empty<string>(),
                warnings: Array.Empty<string>());
        }
    }

    private static void CleanupDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            return;
        }

        try
        {
            Directory.Delete(path, recursive: true);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
        catch
        {
        }
    }

    private static int ImportLivePacket(string packetPath)
    {
        try
        {
            if (!File.Exists(packetPath))
            {
                throw new FileNotFoundException("Live observer packet not found.", packetPath);
            }

            using JsonDocument document = JsonDocument.Parse(File.ReadAllText(packetPath));
            JsonElement root = document.RootElement;
            JsonElement requestJson = RequiredProperty(root, "visualRuntimeObservationRequest");
            VisualRuntimeObservationRequest visualRuntimeObservation = ReadVisualRuntimeObservation(requestJson);
            ArmorImportResult result = new ArmorImporter().Import(
                CreateRequest("request-23-live-packet", Baseline, visualRuntimeObservation));

            VisualRuntimeObservationRow[] rows = result.VisualRuntime.Rows.ToArray();
            int registeredDiagnosticRows = rows.Count(row => IsObservedTrue(row.KandraIsRegisteredStatus));
            int meshMemoryDiagnosticRows = rows.Count(row => IsObservedTrue(row.KandraTryGetMeshMemoryStatus));

            Check(result.VisualRuntime.Requested, "Live packet import must request the visual runtime stage.");
            Check(result.VisualRuntime.LiveBodyObserved, "Request-23 live body status must remain observed.");
            Check(result.VisualRuntime.LiveEquipObserved, "Request-23 live equip status must remain observed.");
            Check(result.VisualRuntime.DownstreamBoundaryFalse, "Request-23 downstream boundary must remain false.");
            Check(result.VisualRuntime.RequiredRowsPresent, "Request-23 required A2K-T34 rows must remain present.");
            Check(rows.Length == 6, "Request-23 importer result must retain six A2K-T34 rows.");
            Check(registeredDiagnosticRows > 0, "Request-23 importer result must retain Kandra IsRegistered diagnostics.");
            Check(meshMemoryDiagnosticRows > 0, "Request-23 importer result must retain Kandra TryGetMeshMemory diagnostics.");
            Check(!result.CandidateMapApplicationAllowed && !result.CandidateMapApplicationExecuted, "Candidate-map application must remain false.");
            Check(!result.ConversionAllowed && !result.ConversionExecuted, "Conversion must remain false.");
            Check(!result.RuntimeLoaderChanged && !result.RuntimeRegistrationExecuted, "Runtime registration must remain false.");
            Check(!result.ItemRegistrationExecuted && !result.InventoryEquipSaveMutationExecuted, "Item/equip/save mutation must remain false.");
            Check(!result.NativeGameWriteExecuted && !result.SaveWriteExecuted && !result.ReleaseReady, "Native writes, save writes, and release readiness must remain false.");

            Console.WriteLine("TAINTED_ARMOUR_LIVE_PACKET_IMPORT_RESULT");
            Console.WriteLine("packet=" + packetPath);
            Console.WriteLine("marker=" + ReadString(root, "marker"));
            Console.WriteLine("generatedAtUtc=" + ReadString(root, "generatedAtUtc"));
            Console.WriteLine("plugin=" + ReadString(root, "pluginName") + " " + ReadString(root, "pluginVersion"));
            Console.WriteLine("status=" + result.Status);
            Console.WriteLine("visualRuntime.requested=" + result.VisualRuntime.Requested);
            Console.WriteLine("visualRuntime.liveBodyObserved=" + result.VisualRuntime.LiveBodyObserved);
            Console.WriteLine("visualRuntime.liveEquipObserved=" + result.VisualRuntime.LiveEquipObserved);
            Console.WriteLine("visualRuntime.poseBindingAccepted=" + result.VisualRuntime.PoseBindingAccepted);
            Console.WriteLine("visualRuntime.downstreamBoundaryFalse=" + result.VisualRuntime.DownstreamBoundaryFalse);
            Console.WriteLine("visualRuntime.requiredRowsPresent=" + result.VisualRuntime.RequiredRowsPresent);
            Console.WriteLine("visualRuntime.blockedMetricRowCount=" + result.VisualRuntime.BlockedMetricRowCount.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Console.WriteLine("visualRuntime.rowCount=" + result.VisualRuntime.RowCount.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Console.WriteLine("kandra.isRegisteredDiagnosticRows=" + registeredDiagnosticRows.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Console.WriteLine("kandra.tryGetMeshMemoryDiagnosticRows=" + meshMemoryDiagnosticRows.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Console.WriteLine("kandra.firstIsRegisteredStatus=" + rows.First().KandraIsRegisteredStatus);
            Console.WriteLine("kandra.firstTryGetMeshMemoryStatus=" + rows.First().KandraTryGetMeshMemoryStatus);
            Console.WriteLine("candidateMapApplicationAllowed=" + result.CandidateMapApplicationAllowed);
            Console.WriteLine("candidateMapApplicationExecuted=" + result.CandidateMapApplicationExecuted);
            Console.WriteLine("conversionAllowed=" + result.ConversionAllowed);
            Console.WriteLine("conversionExecuted=" + result.ConversionExecuted);
            Console.WriteLine("runtimeLoaderChanged=" + result.RuntimeLoaderChanged);
            Console.WriteLine("runtimeRegistrationExecuted=" + result.RuntimeRegistrationExecuted);
            Console.WriteLine("itemRegistrationExecuted=" + result.ItemRegistrationExecuted);
            Console.WriteLine("inventoryEquipSaveMutationExecuted=" + result.InventoryEquipSaveMutationExecuted);
            Console.WriteLine("nativeGameWriteExecuted=" + result.NativeGameWriteExecuted);
            Console.WriteLine("saveWriteExecuted=" + result.SaveWriteExecuted);
            Console.WriteLine("releaseReady=" + result.ReleaseReady);
            Console.WriteLine("blockers=" + string.Join("|", result.Blockers));
            return 0;
        }
        catch (Exception exception)
        {
            Console.WriteLine("TAINTED_ARMOUR_LIVE_PACKET_IMPORT_FAILED " + exception.Message);
            return 1;
        }
    }

    private static int ValidateKandraPackageWithLivePacket(string packetPath)
    {
        string fixtureRoot = Path.Combine(Path.GetTempPath(), "ta-kandra-package-validation-" + Guid.NewGuid().ToString("N"));
        try
        {
            if (!File.Exists(packetPath))
            {
                throw new FileNotFoundException("Live observer packet not found.", packetPath);
            }

            using JsonDocument document = JsonDocument.Parse(File.ReadAllText(packetPath));
            JsonElement root = document.RootElement;
            JsonElement requestJson = RequiredProperty(root, "visualRuntimeObservationRequest");
            VisualRuntimeObservationRequest visualRuntimeObservation = ReadVisualRuntimeObservation(requestJson);
            ArmorImportResult proof = new ArmorImporter().Import(
                CreateRequest("request-23-live-package-validation", Baseline, visualRuntimeObservation));
            KandraWriterResult writer = WriteFixtureKandraPackage(
                fixtureRoot,
                "Request23ProofMod",
                "Mesh_Request23_LoosePackageProof",
                CreateFixtureKandraPayload());
            KandraPackageValidationResult result = new KandraPackageValidationStage().Validate(
                new KandraPackageValidationRequest("request-23.kandra-package-validation", writer, proof));

            Check(result.PackageValidationAccepted, "Request-23 package validation must accept the loose package plus live proof.");
            Check(result.RuntimeSeamValid, "Request-23 package validation must validate the modDirectory/Kandra seam.");
            Check(result.MeshDataFilePresent && result.IndicesDataFilePresent, "Request-23 package validation must see both package files.");
            Check(result.MeshDataLengthMatches && result.IndicesDataLengthMatches, "Request-23 package validation must verify lengths.");
            Check(result.MeshDataHashMatches && result.IndicesDataHashMatches, "Request-23 package validation must verify hashes.");
            Check(result.LiveKandraRegistrationProofPresent, "Request-23 package validation must retain live IsRegistered proof.");
            Check(result.LiveKandraMeshMemoryProofPresent, "Request-23 package validation must retain live TryGetMeshMemory proof.");
            Check(result.LiveProofDownstreamBoundaryFalse && result.WriterDownstreamBoundaryFalse, "Request-23 package validation must retain false downstream boundaries.");
            Check(!result.CustomArmourRegistrationAllowed && !result.RuntimeRegistrationExecuted, "Request-23 package validation must not allow or execute custom registration.");
            Check(!result.CandidateMapApplicationExecuted && !result.ConversionExecuted, "Request-23 package validation must not apply candidate maps or conversion.");
            Check(!result.NativeGameWriteExecuted && !result.DownstreamWritesExecuted, "Request-23 package validation must remain no-write.");

            VisualRuntimeObservationRow[] rows = proof.VisualRuntime.Rows.ToArray();
            Console.WriteLine("TAINTED_ARMOUR_KANDRA_PACKAGE_VALIDATION_RESULT");
            Console.WriteLine("packet=" + packetPath);
            Console.WriteLine("marker=" + ReadString(root, "marker"));
            Console.WriteLine("generatedAtUtc=" + ReadString(root, "generatedAtUtc"));
            Console.WriteLine("plugin=" + ReadString(root, "pluginName") + " " + ReadString(root, "pluginVersion"));
            Console.WriteLine("importStatus=" + proof.Status);
            Console.WriteLine("packageValidationAccepted=" + result.PackageValidationAccepted);
            Console.WriteLine("runtimeSeamValid=" + result.RuntimeSeamValid);
            Console.WriteLine("kandraDirectory=" + result.KandraDirectory);
            Console.WriteLine("meshDataPath=" + result.MeshDataPath);
            Console.WriteLine("indicesDataPath=" + result.IndicesDataPath);
            Console.WriteLine("meshDataLengthMatches=" + result.MeshDataLengthMatches);
            Console.WriteLine("indicesDataLengthMatches=" + result.IndicesDataLengthMatches);
            Console.WriteLine("meshDataHashMatches=" + result.MeshDataHashMatches);
            Console.WriteLine("indicesDataHashMatches=" + result.IndicesDataHashMatches);
            Console.WriteLine("liveKandraRegistrationProofPresent=" + result.LiveKandraRegistrationProofPresent);
            Console.WriteLine("liveKandraMeshMemoryProofPresent=" + result.LiveKandraMeshMemoryProofPresent);
            Console.WriteLine("liveProofDownstreamBoundaryFalse=" + result.LiveProofDownstreamBoundaryFalse);
            Console.WriteLine("writerDownstreamBoundaryFalse=" + result.WriterDownstreamBoundaryFalse);
            Console.WriteLine("kandra.firstIsRegisteredStatus=" + rows.First().KandraIsRegisteredStatus);
            Console.WriteLine("kandra.firstTryGetMeshMemoryStatus=" + rows.First().KandraTryGetMeshMemoryStatus);
            Console.WriteLine("candidateMapApplicationAllowed=" + result.CandidateMapApplicationAllowed);
            Console.WriteLine("candidateMapApplicationExecuted=" + result.CandidateMapApplicationExecuted);
            Console.WriteLine("conversionAllowed=" + result.ConversionAllowed);
            Console.WriteLine("conversionExecuted=" + result.ConversionExecuted);
            Console.WriteLine("customArmourRegistrationAllowed=" + result.CustomArmourRegistrationAllowed);
            Console.WriteLine("runtimeRegistrationExecuted=" + result.RuntimeRegistrationExecuted);
            Console.WriteLine("nativeGameWriteExecuted=" + result.NativeGameWriteExecuted);
            Console.WriteLine("downstreamWritesExecuted=" + result.DownstreamWritesExecuted);
            Console.WriteLine("blockers=" + string.Join("|", result.Blockers));
            Console.WriteLine("warnings=" + string.Join("|", result.Warnings));
            return 0;
        }
        catch (Exception exception)
        {
            Console.WriteLine("TAINTED_ARMOUR_KANDRA_PACKAGE_VALIDATION_FAILED " + exception.Message);
            return 1;
        }
        finally
        {
            CleanupDirectory(fixtureRoot);
        }
    }

    private static int PreflightKandraRegistrationWithLivePacket(string packetPath)
    {
        string fixtureRoot = Path.Combine(Path.GetTempPath(), "ta-kandra-registration-preflight-" + Guid.NewGuid().ToString("N"));
        try
        {
            if (!File.Exists(packetPath))
            {
                throw new FileNotFoundException("Live observer packet not found.", packetPath);
            }

            using JsonDocument document = JsonDocument.Parse(File.ReadAllText(packetPath));
            JsonElement root = document.RootElement;
            JsonElement requestJson = RequiredProperty(root, "visualRuntimeObservationRequest");
            VisualRuntimeObservationRequest visualRuntimeObservation = ReadVisualRuntimeObservation(requestJson);
            ArmorImportResult proof = new ArmorImporter().Import(
                CreateRequest("request-23-live-registration-preflight", Baseline, visualRuntimeObservation));
            KandraWriterResult writer = WriteFixtureKandraPackage(
                fixtureRoot,
                "Request23ProofMod",
                "Mesh_Request23_LoosePackageProof",
                CreateFixtureKandraPayload());
            KandraPackageValidationResult package = new KandraPackageValidationStage().Validate(
                new KandraPackageValidationRequest("request-23.kandra-registration-preflight.package", writer, proof));
            KandraMeshRegistrationMetadata metadata = CreateFixtureKandraRegistrationMetadata(
                "Request23ProofMod",
                "Mesh_Request23_LoosePackageProof");
            KandraRegistrationPreflightResult result = new KandraRegistrationPreflightStage().Validate(
                new KandraRegistrationPreflightRequest("request-23.kandra-registration-preflight", metadata, package));

            Check(result.RegistrationPreflightAccepted, "Request-23 registration preflight must accept matching metadata plus validated package.");
            Check(result.PackageValidationAccepted, "Request-23 registration preflight must retain accepted package validation.");
            Check(result.RuntimeSeamValid, "Request-23 registration preflight must retain valid runtime seam.");
            Check(result.MetadataModDirectoryMatches && result.MetadataNameMatches, "Request-23 registration preflight must match metadata identity.");
            Check(result.MetadataLayoutVersionMatches, "Request-23 registration preflight must match layout version.");
            Check(result.MeshDataByteCountMatches && result.IndicesDataByteCountMatches, "Request-23 registration preflight must match metadata counts to package byte counts.");
            Check(result.MeshDataHashMatches && result.IndicesDataHashMatches, "Request-23 registration preflight must retain package hash validation.");
            Check(result.LiveKandraRegistrationProofPresent, "Request-23 registration preflight must retain live IsRegistered proof.");
            Check(result.LiveKandraMeshMemoryProofPresent, "Request-23 registration preflight must retain live TryGetMeshMemory proof.");
            Check(result.PackageDownstreamBoundaryFalse, "Request-23 registration preflight must keep package downstream boundary false.");
            Check(!result.CustomArmourRegistrationAllowed && !result.RuntimeRegistrationExecuted, "Request-23 registration preflight must not allow or execute registration.");
            Check(!result.CandidateMapApplicationExecuted && !result.ConversionExecuted, "Request-23 registration preflight must not apply candidate maps or conversion.");
            Check(!result.ItemEquipMutationExecuted && !result.SaveMutationExecuted, "Request-23 registration preflight must not mutate item/equip/save state.");
            Check(!result.NativeGameWriteExecuted && !result.DownstreamWritesExecuted, "Request-23 registration preflight must remain no-write.");

            VisualRuntimeObservationRow[] rows = proof.VisualRuntime.Rows.ToArray();
            Console.WriteLine("TAINTED_ARMOUR_KANDRA_REGISTRATION_PREFLIGHT_RESULT");
            Console.WriteLine("packet=" + packetPath);
            Console.WriteLine("marker=" + ReadString(root, "marker"));
            Console.WriteLine("generatedAtUtc=" + ReadString(root, "generatedAtUtc"));
            Console.WriteLine("plugin=" + ReadString(root, "pluginName") + " " + ReadString(root, "pluginVersion"));
            Console.WriteLine("importStatus=" + proof.Status);
            Console.WriteLine("packageValidationAccepted=" + package.PackageValidationAccepted);
            Console.WriteLine("registrationPreflightAccepted=" + result.RegistrationPreflightAccepted);
            Console.WriteLine("runtimeSeamValid=" + result.RuntimeSeamValid);
            Console.WriteLine("metadata.modDirectory=" + result.ModDirectory);
            Console.WriteLine("metadata.name=" + result.Name);
            Console.WriteLine("metadata.vertexCount=" + result.VertexCount.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Console.WriteLine("metadata.indicesCount=" + result.IndicesCount.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Console.WriteLine("metadata.bindposesCount=" + result.BindposesCount.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Console.WriteLine("metadata.blendshapeCount=" + result.BlendshapeCount.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Console.WriteLine("metadataModDirectoryMatches=" + result.MetadataModDirectoryMatches);
            Console.WriteLine("metadataNameMatches=" + result.MetadataNameMatches);
            Console.WriteLine("metadataLayoutVersionMatches=" + result.MetadataLayoutVersionMatches);
            Console.WriteLine("expectedMeshDataByteCount=" + result.ExpectedMeshDataByteCount.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Console.WriteLine("actualMeshDataByteCount=" + result.ActualMeshDataByteCount.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Console.WriteLine("meshDataByteCountMatches=" + result.MeshDataByteCountMatches);
            Console.WriteLine("expectedIndicesDataByteCount=" + result.ExpectedIndicesDataByteCount.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Console.WriteLine("actualIndicesDataByteCount=" + result.ActualIndicesDataByteCount.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Console.WriteLine("indicesDataByteCountMatches=" + result.IndicesDataByteCountMatches);
            Console.WriteLine("meshDataHashMatches=" + result.MeshDataHashMatches);
            Console.WriteLine("indicesDataHashMatches=" + result.IndicesDataHashMatches);
            Console.WriteLine("liveKandraRegistrationProofPresent=" + result.LiveKandraRegistrationProofPresent);
            Console.WriteLine("liveKandraMeshMemoryProofPresent=" + result.LiveKandraMeshMemoryProofPresent);
            Console.WriteLine("packageDownstreamBoundaryFalse=" + result.PackageDownstreamBoundaryFalse);
            Console.WriteLine("kandra.firstIsRegisteredStatus=" + rows.First().KandraIsRegisteredStatus);
            Console.WriteLine("kandra.firstTryGetMeshMemoryStatus=" + rows.First().KandraTryGetMeshMemoryStatus);
            Console.WriteLine("candidateMapApplicationAllowed=" + result.CandidateMapApplicationAllowed);
            Console.WriteLine("candidateMapApplicationExecuted=" + result.CandidateMapApplicationExecuted);
            Console.WriteLine("conversionAllowed=" + result.ConversionAllowed);
            Console.WriteLine("conversionExecuted=" + result.ConversionExecuted);
            Console.WriteLine("customArmourRegistrationAllowed=" + result.CustomArmourRegistrationAllowed);
            Console.WriteLine("runtimeRegistrationExecuted=" + result.RuntimeRegistrationExecuted);
            Console.WriteLine("itemEquipMutationExecuted=" + result.ItemEquipMutationExecuted);
            Console.WriteLine("saveMutationExecuted=" + result.SaveMutationExecuted);
            Console.WriteLine("nativeGameWriteExecuted=" + result.NativeGameWriteExecuted);
            Console.WriteLine("downstreamWritesExecuted=" + result.DownstreamWritesExecuted);
            Console.WriteLine("blockers=" + string.Join("|", result.Blockers));
            Console.WriteLine("warnings=" + string.Join("|", result.Warnings));
            return 0;
        }
        catch (Exception exception)
        {
            Console.WriteLine("TAINTED_ARMOUR_KANDRA_REGISTRATION_PREFLIGHT_FAILED " + exception.Message);
            return 1;
        }
        finally
        {
            CleanupDirectory(fixtureRoot);
        }
    }

    private static int BuildKandraRegistrationCandidateWithLivePacket(string packetPath)
    {
        string fixtureRoot = Path.Combine(Path.GetTempPath(), "ta-kandra-registration-candidate-" + Guid.NewGuid().ToString("N"));
        try
        {
            if (!File.Exists(packetPath))
            {
                throw new FileNotFoundException("Live observer packet not found.", packetPath);
            }

            using JsonDocument document = JsonDocument.Parse(File.ReadAllText(packetPath));
            JsonElement root = document.RootElement;
            JsonElement requestJson = RequiredProperty(root, "visualRuntimeObservationRequest");
            VisualRuntimeObservationRequest visualRuntimeObservation = ReadVisualRuntimeObservation(requestJson);
            ArmorImportResult proof = new ArmorImporter().Import(
                CreateRequest("request-23-live-registration-candidate", Baseline, visualRuntimeObservation));
            KandraWriterResult writer = WriteFixtureKandraPackage(
                fixtureRoot,
                "Request23ProofMod",
                "Mesh_Request23_LoosePackageProof",
                CreateFixtureKandraPayload());
            KandraPackageValidationResult package = new KandraPackageValidationStage().Validate(
                new KandraPackageValidationRequest("request-23.kandra-registration-candidate.package", writer, proof));
            KandraRegistrationPreflightResult preflight = new KandraRegistrationPreflightStage().Validate(
                new KandraRegistrationPreflightRequest(
                    "request-23.kandra-registration-candidate.preflight",
                    CreateFixtureKandraRegistrationMetadata("Request23ProofMod", "Mesh_Request23_LoosePackageProof"),
                    package));
            KandraRegistrationCandidateBuildResult result = new KandraRegistrationCandidateStage().Build(
                new KandraRegistrationCandidateBuildRequest("request-23.kandra-registration-candidate", preflight));

            Check(result.CandidateBuildAccepted, "Request-23 registration candidate build must accept the accepted preflight.");
            Check(result.PreflightAccepted, "Request-23 registration candidate build must retain accepted preflight status.");
            Check(result.CandidateRequestConstructed && result.CandidateRequest != null, "Request-23 registration candidate build must construct a request object.");
            Check(!result.RuntimeRegistrationCallContractProven && !result.RuntimeRegistrationInvocationAllowed, "Request-23 registration candidate build must keep runtime registration invocation blocked.");
            Check(!result.RuntimeRegistrationExecuted, "Request-23 registration candidate build must not execute runtime registration.");
            Check(!result.CandidateMapApplicationExecuted && !result.ConversionExecuted, "Request-23 registration candidate build must not apply candidate maps or conversion.");
            Check(!result.ItemEquipMutationExecuted && !result.SaveMutationExecuted, "Request-23 registration candidate build must not mutate item/equip/save state.");
            Check(!result.NativeGameWriteExecuted && !result.DownstreamWritesExecuted, "Request-23 registration candidate build must remain no-write.");

            KandraCustomRegistrationCandidateRequest candidate = result.CandidateRequest!;
            Check(candidate.ModDirectory == preflight.ModDirectory && candidate.Name == preflight.Name, "Request-23 candidate must match preflight metadata identity.");
            Check(candidate.MeshDataByteCount == preflight.ActualMeshDataByteCount, "Request-23 candidate must match preflight mesh byte count.");
            Check(candidate.IndicesDataByteCount == preflight.ActualIndicesDataByteCount, "Request-23 candidate must match preflight index byte count.");
            Check(candidate.MeshDataSha256 == preflight.MeshDataSha256 && candidate.IndicesDataSha256 == preflight.IndicesDataSha256, "Request-23 candidate must match preflight hashes.");
            Check(!candidate.RuntimeRegistrationCallContractProven && !candidate.RuntimeRegistrationInvocationAllowed, "Request-23 candidate request must keep runtime registration invocation blocked.");
            Check(!candidate.RuntimeRegistrationExecuted && !candidate.DownstreamWritesExecuted, "Request-23 candidate request must not execute registration or writes.");

            Console.WriteLine("TAINTED_ARMOUR_KANDRA_REGISTRATION_CANDIDATE_RESULT");
            Console.WriteLine("packet=" + packetPath);
            Console.WriteLine("marker=" + ReadString(root, "marker"));
            Console.WriteLine("generatedAtUtc=" + ReadString(root, "generatedAtUtc"));
            Console.WriteLine("plugin=" + ReadString(root, "pluginName") + " " + ReadString(root, "pluginVersion"));
            Console.WriteLine("importStatus=" + proof.Status);
            Console.WriteLine("packageValidationAccepted=" + package.PackageValidationAccepted);
            Console.WriteLine("registrationPreflightAccepted=" + preflight.RegistrationPreflightAccepted);
            Console.WriteLine("candidateBuildAccepted=" + result.CandidateBuildAccepted);
            Console.WriteLine("candidateRequestConstructed=" + result.CandidateRequestConstructed);
            Console.WriteLine("candidate.schemaVersion=" + candidate.SchemaVersion);
            Console.WriteLine("candidate.requestId=" + candidate.RequestId);
            Console.WriteLine("candidate.sourcePreflightRequestId=" + candidate.SourcePreflightRequestId);
            Console.WriteLine("candidate.modDirectory=" + candidate.ModDirectory);
            Console.WriteLine("candidate.name=" + candidate.Name);
            Console.WriteLine("candidate.kandraMeshModDirectory=" + candidate.KandraMeshModDirectory);
            Console.WriteLine("candidate.kandraMeshName=" + candidate.KandraMeshName);
            Console.WriteLine("candidate.vertexCount=" + candidate.VertexCount.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Console.WriteLine("candidate.indicesCount=" + candidate.IndicesCount.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Console.WriteLine("candidate.bindposesCount=" + candidate.BindposesCount.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Console.WriteLine("candidate.blendshapeCount=" + candidate.BlendshapeCount.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Console.WriteLine("candidate.layoutVersion=" + candidate.LayoutVersion);
            Console.WriteLine("candidate.kandraDirectory=" + candidate.KandraDirectory);
            Console.WriteLine("candidate.meshDataPath=" + candidate.MeshDataPath);
            Console.WriteLine("candidate.indicesDataPath=" + candidate.IndicesDataPath);
            Console.WriteLine("candidate.meshDataByteCount=" + candidate.MeshDataByteCount.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Console.WriteLine("candidate.indicesDataByteCount=" + candidate.IndicesDataByteCount.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Console.WriteLine("candidate.meshDataSha256=" + candidate.MeshDataSha256);
            Console.WriteLine("candidate.indicesDataSha256=" + candidate.IndicesDataSha256);
            Console.WriteLine("runtimeRegistrationCallContractStatus=" + result.RuntimeRegistrationCallContractStatus);
            Console.WriteLine("runtimeRegistrationCallContractProven=" + result.RuntimeRegistrationCallContractProven);
            Console.WriteLine("runtimeRegistrationInvocationAllowed=" + result.RuntimeRegistrationInvocationAllowed);
            Console.WriteLine("runtimeRegistrationExecuted=" + result.RuntimeRegistrationExecuted);
            Console.WriteLine("candidateMapApplicationAllowed=" + result.CandidateMapApplicationAllowed);
            Console.WriteLine("candidateMapApplicationExecuted=" + result.CandidateMapApplicationExecuted);
            Console.WriteLine("conversionAllowed=" + result.ConversionAllowed);
            Console.WriteLine("conversionExecuted=" + result.ConversionExecuted);
            Console.WriteLine("itemEquipMutationExecuted=" + result.ItemEquipMutationExecuted);
            Console.WriteLine("saveMutationExecuted=" + result.SaveMutationExecuted);
            Console.WriteLine("nativeGameWriteExecuted=" + result.NativeGameWriteExecuted);
            Console.WriteLine("downstreamWritesExecuted=" + result.DownstreamWritesExecuted);
            Console.WriteLine("blockers=" + string.Join("|", result.Blockers));
            Console.WriteLine("warnings=" + string.Join("|", result.Warnings));
            return 0;
        }
        catch (Exception exception)
        {
            Console.WriteLine("TAINTED_ARMOUR_KANDRA_REGISTRATION_CANDIDATE_FAILED " + exception.Message);
            return 1;
        }
        finally
        {
            CleanupDirectory(fixtureRoot);
        }
    }

    private static int DryRunKandraRegistrationWithLivePacket(string packetPath)
    {
        string fixtureRoot = Path.Combine(Path.GetTempPath(), "ta-kandra-runtime-registration-dry-run-" + Guid.NewGuid().ToString("N"));
        try
        {
            if (!File.Exists(packetPath))
            {
                throw new FileNotFoundException("Live observer packet not found.", packetPath);
            }

            using JsonDocument document = JsonDocument.Parse(File.ReadAllText(packetPath));
            JsonElement root = document.RootElement;
            JsonElement requestJson = RequiredProperty(root, "visualRuntimeObservationRequest");
            VisualRuntimeObservationRequest visualRuntimeObservation = ReadVisualRuntimeObservation(requestJson);
            KandraRuntimeRegistrationContractFingerprint contractFingerprint = ReadKandraRuntimeRegistrationContractFingerprint(root);
            ArmorImportResult proof = new ArmorImporter().Import(
                CreateRequest("live-kandra-runtime-registration-dry-run", Baseline, visualRuntimeObservation));
            KandraWriterResult writer = WriteFixtureKandraPackage(
                fixtureRoot,
                "LiveProofMod",
                "Mesh_Live_LoosePackageProof",
                CreateFixtureKandraPayload());
            KandraPackageValidationResult package = new KandraPackageValidationStage().Validate(
                new KandraPackageValidationRequest("live.kandra-runtime-registration-dry-run.package", writer, proof));
            KandraRegistrationPreflightResult preflight = new KandraRegistrationPreflightStage().Validate(
                new KandraRegistrationPreflightRequest(
                    "live.kandra-runtime-registration-dry-run.preflight",
                    CreateFixtureKandraRegistrationMetadata("LiveProofMod", "Mesh_Live_LoosePackageProof"),
                    package));
            KandraRegistrationCandidateBuildResult candidate = new KandraRegistrationCandidateStage().Build(
                new KandraRegistrationCandidateBuildRequest("live.kandra-runtime-registration-dry-run.candidate", preflight));
            KandraRuntimeRegistrationDryRunResult result = new KandraRuntimeRegistrationDryRunStage().Evaluate(
                new KandraRuntimeRegistrationDryRunRequest(
                    "live.kandra-runtime-registration-dry-run",
                    candidate,
                    package,
                    contractFingerprint));

            Check(package.PackageValidationAccepted, "Live packet package validation must accept the loose package plus live proof.");
            Check(preflight.RegistrationPreflightAccepted, "Live packet registration preflight must accept the validated loose package.");
            Check(candidate.CandidateBuildAccepted && candidate.CandidateRequest != null, "Live packet registration candidate must be constructed.");
            Check(result.RegistrationDryRunAccepted, "Live packet runtime registration dry-run gate must accept the candidate, package, and contract fingerprint.");
            Check(result.RuntimeRegistrationContractFingerprintAccepted, "Live packet dry-run gate must accept the live registration contract fingerprint.");
            Check(result.RuntimeRegistrationContractSurfacePresent, "Live packet dry-run gate must retain the live registration member surface.");
            Check(result.RuntimeRegistrationContractNoWriteBoundaryFalse, "Live packet dry-run gate must retain the contract no-write boundary.");
            Check(result.RuntimeRegistrationCallContractProven, "Live packet dry-run gate must mark the contract fingerprint as proven for dry-run planning.");
            Check(!result.ExplicitRuntimeInvocationApprovalPresent, "Live packet dry-run gate must not have runtime invocation approval.");
            Check(!result.RuntimeRegistrationInvocationAllowed, "Live packet dry-run gate must refuse runtime registration invocation.");
            Check(!result.RegistrationMethodInvoked && !result.CanRegisterMethodsInvoked, "Live packet dry-run gate must not invoke Register or CanRegister.");
            Check(!result.RuntimeRegistrationExecuted, "Live packet dry-run gate must not execute runtime registration.");
            Check(!result.CandidateMapApplicationExecuted && !result.ConversionExecuted, "Live packet dry-run gate must not apply candidate maps or conversion.");
            Check(!result.ItemEquipMutationExecuted && !result.SaveMutationExecuted, "Live packet dry-run gate must not mutate item/equip/save state.");
            Check(!result.NativeGameWriteExecuted && !result.DownstreamWritesExecuted, "Live packet dry-run gate must remain no-write.");

            Console.WriteLine("TAINTED_ARMOUR_KANDRA_RUNTIME_REGISTRATION_DRY_RUN_RESULT");
            Console.WriteLine("packet=" + packetPath);
            Console.WriteLine("marker=" + ReadString(root, "marker"));
            Console.WriteLine("generatedAtUtc=" + ReadString(root, "generatedAtUtc"));
            Console.WriteLine("plugin=" + ReadString(root, "pluginName") + " " + ReadString(root, "pluginVersion"));
            Console.WriteLine("importStatus=" + proof.Status);
            Console.WriteLine("packageValidationAccepted=" + package.PackageValidationAccepted);
            Console.WriteLine("registrationPreflightAccepted=" + preflight.RegistrationPreflightAccepted);
            Console.WriteLine("candidateBuildAccepted=" + candidate.CandidateBuildAccepted);
            Console.WriteLine("registrationDryRunAccepted=" + result.RegistrationDryRunAccepted);
            Console.WriteLine("runtimeRegistrationContractFingerprintAccepted=" + result.RuntimeRegistrationContractFingerprintAccepted);
            Console.WriteLine("runtimeRegistrationContractSurfacePresent=" + result.RuntimeRegistrationContractSurfacePresent);
            Console.WriteLine("runtimeRegistrationContractNoWriteBoundaryFalse=" + result.RuntimeRegistrationContractNoWriteBoundaryFalse);
            Console.WriteLine("runtimeRegistrationCallContractProven=" + result.RuntimeRegistrationCallContractProven);
            Console.WriteLine("explicitRuntimeInvocationApprovalPresent=" + result.ExplicitRuntimeInvocationApprovalPresent);
            Console.WriteLine("runtimeRegistrationInvocationAllowed=" + result.RuntimeRegistrationInvocationAllowed);
            Console.WriteLine("registrationMethodInvoked=" + result.RegistrationMethodInvoked);
            Console.WriteLine("canRegisterMethodsInvoked=" + result.CanRegisterMethodsInvoked);
            Console.WriteLine("runtimeRegistrationExecuted=" + result.RuntimeRegistrationExecuted);
            Console.WriteLine("registrationOwner=" + result.RegistrationOwner);
            Console.WriteLine("registrationMethodSignature=" + result.RegistrationMethodSignature);
            Console.WriteLine("registrationMethodFingerprint=" + result.RegistrationMethodFingerprint);
            Console.WriteLine("registrationAssemblySha256=" + result.RegistrationAssemblySha256);
            Console.WriteLine("candidate.modDirectory=" + result.ModDirectory);
            Console.WriteLine("candidate.name=" + result.Name);
            Console.WriteLine("candidate.kandraDirectory=" + result.KandraDirectory);
            Console.WriteLine("candidate.meshDataSha256=" + result.MeshDataSha256);
            Console.WriteLine("candidate.indicesDataSha256=" + result.IndicesDataSha256);
            Console.WriteLine("candidateMapApplicationAllowed=" + result.CandidateMapApplicationAllowed);
            Console.WriteLine("candidateMapApplicationExecuted=" + result.CandidateMapApplicationExecuted);
            Console.WriteLine("conversionAllowed=" + result.ConversionAllowed);
            Console.WriteLine("conversionExecuted=" + result.ConversionExecuted);
            Console.WriteLine("itemEquipMutationExecuted=" + result.ItemEquipMutationExecuted);
            Console.WriteLine("saveMutationExecuted=" + result.SaveMutationExecuted);
            Console.WriteLine("nativeGameWriteExecuted=" + result.NativeGameWriteExecuted);
            Console.WriteLine("downstreamWritesExecuted=" + result.DownstreamWritesExecuted);
            Console.WriteLine("registrationInvocationRefusalReason=" + result.RegistrationInvocationRefusalReason);
            Console.WriteLine("blockers=" + string.Join("|", result.Blockers));
            Console.WriteLine("warnings=" + string.Join("|", result.Warnings));
            return 0;
        }
        catch (Exception exception)
        {
            Console.WriteLine("TAINTED_ARMOUR_KANDRA_RUNTIME_REGISTRATION_DRY_RUN_FAILED " + exception.Message);
            return 1;
        }
        finally
        {
            CleanupDirectory(fixtureRoot);
        }
    }

    private static int CompareSameMeshAbReceipt(string receiptPath)
    {
        try
        {
            if (!File.Exists(receiptPath))
            {
                throw new FileNotFoundException("Same-mesh A/B receipt not found.", receiptPath);
            }

            using JsonDocument document = JsonDocument.Parse(File.ReadAllText(receiptPath));
            JsonElement root = document.RootElement;
            JsonElement buildResult = RequiredProperty(root, "BuildResult");
            JsonElement variantsJson = RequiredProperty(buildResult, "Variants");
            JsonElement variantReceiptsJson = RequiredProperty(root, "VariantReceipts");
            Dictionary<string, JsonElement> variantReceipts = variantReceiptsJson
                .EnumerateArray()
                .ToDictionary(variant => ReadString(variant, "VariantId"), StringComparer.Ordinal);
            int vertexCount = ReadInt(buildResult, "VertexCount");
            int roundtripTolerance = Math.Max(4, (int)Math.Ceiling(vertexCount * 0.001d));

            var candidates = new List<KandraSameMeshVisualDecodeComparisonCandidate>();
            foreach (JsonElement variantJson in variantsJson.EnumerateArray())
            {
                string variantId = ReadString(variantJson, "VariantId");
                Check(variantReceipts.TryGetValue(variantId, out JsonElement receiptVariant), "Same-mesh receipt missing runtime variant: " + variantId);
                JsonElement attempt = RequiredProperty(receiptVariant, "Attempt");
                candidates.Add(new KandraSameMeshVisualDecodeComparisonCandidate(
                    variantId,
                    ReadString(variantJson, "VariantLabel"),
                    ReadBool(attempt, "RuntimeRegistrationSucceeded"),
                    ReadString(attempt, "KandraIsRegisteredStatus"),
                    ReadString(attempt, "KandraTryGetMeshMemoryStatus"),
                    ReadBool(variantJson, "SourceMeshPayloadPreservedExactly"),
                    ReadBool(variantJson, "SourceIndicesPayloadPreservedExactly"),
                    ReadBool(variantJson, "OnlyCompressedVertexPlus16Changed"),
                    ReadInt(variantJson, "ChangedNonPlus16ByteCount"),
                    ReadInt(variantJson, "ChangedPlus16FieldCount"),
                    visualEvidenceCaptured: false,
                    visualEvidenceReviewed: false,
                    visuallyAccepted: false));
            }

            KandraSameMeshVisualDecodeComparisonResult result = new KandraSameMeshVisualDecodeComparisonStage().Evaluate(
                new KandraSameMeshVisualDecodeComparisonRequest(
                    "live.kandra-same-mesh-visual-decode-comparison",
                    receiptPath,
                    ReadString(root, "RequestId"),
                    ReadString(buildResult, "SourceMeshName"),
                    vertexCount,
                    ReadBool(root, "RuntimeProofSucceeded"),
                    variantReceiptsJson.GetArrayLength(),
                    ReadBool(root, "NonRegistrationDownstreamBoundaryFalse"),
                    roundtripTolerance,
                    candidates));

            Console.WriteLine("TAINTED_ARMOUR_KANDRA_SAME_MESH_VISUAL_DECODE_COMPARISON_RESULT");
            Console.WriteLine("receipt=" + receiptPath);
            Console.WriteLine("sourceReceiptRequestId=" + result.SourceReceiptRequestId);
            Console.WriteLine("sourceMeshName=" + result.SourceMeshName);
            Console.WriteLine("vertexCount=" + result.VertexCount);
            Console.WriteLine("hostRegistrationProofAccepted=" + result.HostRegistrationProofAccepted);
            Console.WriteLine("decodeComparisonAccepted=" + result.DecodeComparisonAccepted);
            Console.WriteLine("visualEvidenceAccepted=" + result.VisualEvidenceAccepted);
            Console.WriteLine("productionEncoderDecisionAccepted=" + result.ProductionEncoderDecisionAccepted);
            Console.WriteLine("selectedEncoderVariantId=" + result.SelectedEncoderVariantId);
            Console.WriteLine("decisionStatus=" + result.DecisionStatus);
            Console.WriteLine("candidateMapApplicationExecuted=" + result.CandidateMapApplicationExecuted);
            Console.WriteLine("conversionExecuted=" + result.ConversionExecuted);
            Console.WriteLine("itemEquipMutationExecuted=" + result.ItemEquipMutationExecuted);
            Console.WriteLine("saveMutationExecuted=" + result.SaveMutationExecuted);
            Console.WriteLine("nativeGameWriteExecuted=" + result.NativeGameWriteExecuted);
            Console.WriteLine("nonRegistrationDownstreamWritesExecuted=" + result.NonRegistrationDownstreamWritesExecuted);
            Console.WriteLine("blockers=" + string.Join("|", result.Blockers));
            Console.WriteLine("warnings=" + string.Join("|", result.Warnings));
            foreach (KandraSameMeshVisualDecodeComparisonCandidate candidate in result.Candidates)
            {
                Console.WriteLine(
                    "variant=" +
                    candidate.VariantId +
                    ";runtimeRegistrationSucceeded=" +
                    candidate.RuntimeRegistrationSucceeded +
                    ";isRegistered=" +
                    candidate.KandraIsRegisteredStatus +
                    ";tryGetMeshMemory=" +
                    candidate.KandraTryGetMeshMemoryStatus +
                    ";exactPlus16Fields=" +
                    candidate.ExactPlus16FieldMatchCount(result.VertexCount) +
                    "/" +
                    result.VertexCount +
                    ";changedPlus16Fields=" +
                    candidate.ChangedPlus16FieldCount +
                    ";changedNonPlus16Bytes=" +
                    candidate.ChangedNonPlus16ByteCount);
            }

            return result.HostRegistrationProofAccepted &&
                   result.DecodeComparisonAccepted &&
                   result.DecisionStatus == KandraSameMeshVisualDecodeComparisonStage.BlockedVisualEvidenceMissing &&
                   result.NonRegistrationDownstreamBoundaryFalse
                ? 0
                : 1;
        }
        catch (Exception exception)
        {
            Console.WriteLine("TAINTED_ARMOUR_KANDRA_SAME_MESH_VISUAL_DECODE_COMPARISON_FAILED " + exception.Message);
            return 1;
        }
    }

    private static void KandraFullSectionPackageGenerationBuildsCandidatePackageFromImportedChannels()
    {
        string fixtureRoot = Path.Combine(Path.GetTempPath(), "ta-kandra-full-section-generation-" + Guid.NewGuid().ToString("N"));
        try
        {
            var source = new KandraFullSectionSourceMesh(
                "Mesh_Custom_Armour_Source",
                new[]
                {
                    new KandraFullSectionVertex(
                        new ArmorVector3(0d, 0d, 0d),
                        new ArmorVector3(0d, 0d, 1d),
                        new ArmorVector3(1d, 0d, 0d),
                        1f,
                        0f,
                        0f,
                        new KandraFullSectionSkinWeights(0, 1, 2, 3, 1f, 0f, 0f, 0f)),
                    new KandraFullSectionVertex(
                        new ArmorVector3(1d, 0d, 0d),
                        new ArmorVector3(0d, 0d, 1d),
                        new ArmorVector3(1d, 0d, 0d),
                        1f,
                        1f,
                        0f,
                        new KandraFullSectionSkinWeights(0, 1, 2, 3, 0.5f, 0.5f, 0f, 0f)),
                    new KandraFullSectionVertex(
                        new ArmorVector3(0d, 1d, 0d),
                        new ArmorVector3(0d, 0d, 1d),
                        new ArmorVector3(1d, 0d, 0d),
                        1f,
                        0f,
                        1f,
                        new KandraFullSectionSkinWeights(0, 1, 2, 3, 0.25f, 0.25f, 0.25f, 0.25f)),
                },
                new ushort[] { 0, 1, 2 },
                new[]
                {
                    new KandraFullSectionBindposeFloat3x4(new[]
                    {
                        1f, 0f, 0f, 0f,
                        0f, 1f, 0f, 0f,
                        0f, 0f, 1f, 0f,
                    }),
                });

            KandraFullSectionPackageGenerationResult result = new KandraFullSectionPackageGenerationStage().Generate(
                new KandraFullSectionPackageGenerationRequest(
                    "fixture.kandra-full-section-package-generation",
                    fixtureRoot,
                    "FixtureFullSection",
                    "Mesh_Custom_Armour_Candidate",
                    KandraFullSectionPackageGenerationStage.AcceptedRecoveredOctahedralRoundtripPlus16EncoderId,
                    source));

            Check(result.SourceChannelsAccepted, "Full-section package generation must accept complete imported source channels.");
            Check(result.FullSectionEncodingExecuted, "Full-section package generation must execute candidate encoding.");
            Check(result.CandidatePackageGenerated && result.WriterResult != null, "Full-section package generation must emit a candidate loose package.");
            Check(result.LayoutVersion == KandraRuntimePayloadLayout.LayoutVersion, "Full-section package generation must expose the recovered Kandra layout version.");
            Check(result.RegistrationMetadataGenerated && result.RegistrationMetadata != null, "Full-section package generation must emit registration metadata.");
            Check(result.PayloadStreamMapGenerated && result.PayloadStreamMap != null, "Full-section package generation must emit a metadata-to-stream map.");
            Check(!result.ProductionConversionAccepted, "Candidate package generation must not claim production conversion acceptance before runtime proof.");
            Check(result.Blockers.Contains("kandra_full_section_runtime_registration_proof_missing"), "Candidate package must retain runtime registration proof blocker.");
            Check(result.Blockers.Contains("kandra_full_section_packed_weights_codec_requires_runtime_proof"), "Candidate package must retain packed-weight codec proof blocker.");
            Check(!result.CandidateMapApplicationExecuted && !result.ConversionExecuted, "Full-section package generation must not apply candidate maps or claim production conversion.");
            Check(!result.RuntimeRegistrationExecuted && !result.DownstreamWritesExecuted, "Full-section package generation must not invoke runtime registration or downstream writes.");

            KandraWriterResult writer = result.WriterResult ?? throw new InvalidOperationException("Full-section fixture expected a writer result.");
            KandraMeshRegistrationMetadata metadata = result.RegistrationMetadata ?? throw new InvalidOperationException("Full-section fixture expected registration metadata.");
            KandraFullSectionPayloadStreamMap streamMap = result.PayloadStreamMap ?? throw new InvalidOperationException("Full-section fixture expected a stream map.");
            Check(metadata.ModDirectory == "FixtureFullSection", "Generated metadata must carry the selected Kandra modDirectory.");
            Check(metadata.Name == "Mesh_Custom_Armour_Candidate", "Generated metadata must carry the selected Kandra mesh name.");
            Check(metadata.VertexCount == 3 && metadata.IndicesCount == 3 && metadata.BindposesCount == 1 && metadata.BlendshapeCount == 0, "Generated metadata must carry stream counts.");
            Check(metadata.ExpectedMeshDataByteCount == streamMap.MeshDataByteCount, "Generated metadata mesh byte count must match the stream map.");
            Check(metadata.ExpectedIndicesDataByteCount == streamMap.IndicesDataByteCount, "Generated metadata index byte count must match the stream map.");
            Check(streamMap.CompressedVertexPlus16ByteOffset == 16, "Stream map must expose the disputed compressed-vertex +16 field offset.");
            Check(streamMap.MeshSections.Count == 5, "Stream map must enumerate every recovered .mdkandra section.");
            Check(streamMap.MeshSections[0].SectionId == "compressed_vertices" && streamMap.MeshSections[0].ByteOffset == 0 && streamMap.MeshSections[0].ByteCount == 60, "Stream map must locate compressed vertices first.");
            Check(streamMap.MeshSections[1].SectionId == "additional_vertex_data" && streamMap.MeshSections[1].ByteOffset == 60 && streamMap.MeshSections[1].ByteCount == 24, "Stream map must locate additional vertex data after compressed vertices.");
            Check(streamMap.MeshSections[2].SectionId == "packed_bones_weights" && streamMap.MeshSections[2].ByteOffset == 84 && streamMap.MeshSections[2].ByteCount == 36, "Stream map must locate packed bones and weights after additional vertex data.");
            Check(streamMap.MeshSections[3].SectionId == "bindposes_float3x4" && streamMap.MeshSections[3].ByteOffset == 120 && streamMap.MeshSections[3].ByteCount == 48, "Stream map must locate bindposes after packed weights.");
            Check(streamMap.IndicesSection.SectionId == "indices_u16" && streamMap.IndicesSection.ByteCount == 6, "Stream map must describe .ixkandra indices as packed u16 data.");
            Check(File.Exists(writer.MeshDataPath), "Full-section package generation must write .mdkandra.");
            Check(File.Exists(writer.IndicesDataPath), "Full-section package generation must write .ixkandra.");
            byte[] meshBytes = File.ReadAllBytes(writer.MeshDataPath);
            byte[] indexBytes = File.ReadAllBytes(writer.IndicesDataPath);
            Check(meshBytes.Length == KandraRuntimePayloadLayout.ExpectedMeshPayloadByteCount(3, 1, 0), "Candidate mesh payload must match the recovered section lengths.");
            Check(meshBytes.Length == streamMap.MeshDataByteCount, "Candidate mesh payload length must match generated stream metadata.");
            Check(indexBytes.Length == streamMap.IndicesDataByteCount, "Candidate index payload length must match generated stream metadata.");
            Check(indexBytes.SequenceEqual(new byte[] { 0, 0, 1, 0, 2, 0 }), "Candidate indices must be raw little-endian ushort data.");
            Check(BitConverter.ToSingle(meshBytes, 0) == 0f, "Candidate payload must write position X at compressed vertex +0.");
            Check(BitConverter.ToSingle(meshBytes, 20) == 1f, "Candidate payload must write the second position X at the next compressed vertex.");
            Check(BitConverter.ToSingle(meshBytes, checked((3 * KandraRuntimePayloadLayout.CompressedVertexByteSize) + 4)) == 1f, "Candidate additional data must write tangentW after UV.");

            ArmorImportResult proof = new ArmorImporter().Import(
                CreateRequest(
                    "kandra-full-section-candidate-live-proof",
                    Baseline,
                    CreateVisualRuntimeObservation(
                        "observed:live FoA hero/body renderer context",
                        "observed:live FoA equip renderer context",
                        "blocked_unproven:tpp-target-animation-clip-binding",
                        "observed:full-section-candidate-package-validation",
                        "observed:True",
                        "observed:True")));
            KandraPackageValidationResult package = new KandraPackageValidationStage().Validate(
                new KandraPackageValidationRequest("fixture.kandra-full-section.package-validation", writer, proof));
            KandraRegistrationPreflightResult preflight = new KandraRegistrationPreflightStage().Validate(
                new KandraRegistrationPreflightRequest("fixture.kandra-full-section.registration-preflight", metadata, package));
            Check(package.PackageValidationAccepted, "Generated full-section candidate package must pass loose package validation with supplied live proof.");
            Check(preflight.RegistrationPreflightAccepted, "Generated full-section metadata must pass registration preflight against the written streams.");
            Check(preflight.MetadataModDirectoryMatches && preflight.MetadataNameMatches && preflight.MetadataLayoutVersionMatches, "Preflight must prove generated metadata identity matches the package.");
            Check(!preflight.CustomArmourRegistrationAllowed && !preflight.RuntimeRegistrationExecuted, "Preflight must stay no-registration/no-write.");
        }
        finally
        {
            CleanupDirectory(fixtureRoot);
        }
    }

    private static byte[] CreateSameMeshAbSourcePayload()
    {
        byte[] compressed = new byte[KandraRuntimePayloadLayout.CompressedVertexByteSize * 4];
        WriteFloat(compressed, 0, 0f);
        WriteFloat(compressed, 4, 0f);
        WriteFloat(compressed, 8, 0f);
        WriteUInt32(compressed, 12, 0x11111111u);
        WriteUInt32(compressed, 16, 0x21111111u);
        WriteFloat(compressed, 20, 1f);
        WriteFloat(compressed, 24, 0f);
        WriteFloat(compressed, 28, 0f);
        WriteUInt32(compressed, 32, 0x11111112u);
        WriteUInt32(compressed, 36, 0x21111112u);
        WriteFloat(compressed, 40, 1f);
        WriteFloat(compressed, 44, 1f);
        WriteFloat(compressed, 48, 0f);
        WriteUInt32(compressed, 52, 0x11111113u);
        WriteUInt32(compressed, 56, 0x21111113u);
        WriteFloat(compressed, 60, 0f);
        WriteFloat(compressed, 64, 1f);
        WriteFloat(compressed, 68, 0f);
        WriteUInt32(compressed, 72, 0x11111114u);
        WriteUInt32(compressed, 76, 0x21111114u);

        byte[] additional = new byte[KandraRuntimePayloadLayout.AdditionalVertexDataByteSize * 4];
        WriteUInt32(additional, 0, 0x00000000u);
        WriteFloat(additional, 4, 1f);
        WriteUInt32(additional, 8, 0x00003c00u);
        WriteFloat(additional, 12, 1f);
        WriteUInt32(additional, 16, 0x3c003c00u);
        WriteFloat(additional, 20, 1f);
        WriteUInt32(additional, 24, 0x3c000000u);
        WriteFloat(additional, 28, 1f);

        byte[] weights = Bytes(KandraRuntimePayloadLayout.PackedBonesWeightsByteSize * 4, 90);
        byte[] bindposes = Bytes(KandraRuntimePayloadLayout.Float3x4ByteSize, 120);
        byte[] payload = new byte[compressed.Length + additional.Length + weights.Length + bindposes.Length];
        Buffer.BlockCopy(compressed, 0, payload, 0, compressed.Length);
        Buffer.BlockCopy(additional, 0, payload, compressed.Length, additional.Length);
        Buffer.BlockCopy(weights, 0, payload, compressed.Length + additional.Length, weights.Length);
        Buffer.BlockCopy(bindposes, 0, payload, compressed.Length + additional.Length + weights.Length, bindposes.Length);
        return payload;
    }

    private static void WriteFloat(byte[] target, int offset, float value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        Buffer.BlockCopy(bytes, 0, target, offset, bytes.Length);
    }

    private static void WriteUInt32(byte[] target, int offset, uint value)
    {
        target[offset] = (byte)(value & 0xff);
        target[offset + 1] = (byte)((value >> 8) & 0xff);
        target[offset + 2] = (byte)((value >> 16) & 0xff);
        target[offset + 3] = (byte)((value >> 24) & 0xff);
    }

    private static VisualRuntimeObservationRequest ReadVisualRuntimeObservation(JsonElement requestJson)
    {
        string routeId = ReadString(requestJson, "routeId");
        Check(
            string.Equals(routeId, VisualRuntimeObservationRequest.A2KT34RouteId, StringComparison.Ordinal),
            "Live packet routeId must match the A2K-T34 core route.");

        return VisualRuntimeObservationRequest.CreateA2KT34NoWriteObserver(
            ReadString(requestJson, "triggerSource"),
            ReadString(requestJson, "evidenceSource"),
            ReadString(requestJson, "activeScene"),
            ReadString(requestJson, "liveBodyObservationStatus"),
            ReadString(requestJson, "liveEquipObservationStatus"),
            ReadString(requestJson, "poseBindingStatus"),
            ReadRows(RequiredProperty(requestJson, "rows")));
    }

    private static KandraRuntimeRegistrationContractFingerprint ReadKandraRuntimeRegistrationContractFingerprint(JsonElement packetJson)
    {
        JsonElement runtimeContext = RequiredProperty(packetJson, "runtimeContext");
        JsonElement diagnostic = RequiredProperty(runtimeContext, "kandraRegistrationContractDiagnostic");
        return new KandraRuntimeRegistrationContractFingerprint(
            ReadString(diagnostic, "status"),
            ReadString(diagnostic, "assemblyName"),
            ReadString(diagnostic, "assemblyLocation"),
            ReadString(diagnostic, "assemblySha256"),
            ReadString(diagnostic, "managerType"),
            ReadBool(diagnostic, "managerInstancePresent"),
            ReadString(diagnostic, "registrationOwner"),
            ReadString(diagnostic, "registrationMethodSignature"),
            ReadString(diagnostic, "registrationMethodFingerprint"),
            ReadBool(diagnostic, "registrationMethodPresent"),
            ReadBool(diagnostic, "registrationMethodPublic"),
            ReadString(diagnostic, "rendererType"),
            ReadBool(diagnostic, "rendererTypePresent"),
            ReadBool(diagnostic, "rendererOnEnableMethodPresent"),
            ReadBool(diagnostic, "rendererOnDisableMethodPresent"),
            ReadBool(diagnostic, "rendererDataFieldPresent"),
            ReadBool(diagnostic, "rendererRenderingIdPropertyPresent"),
            ReadBool(diagnostic, "requiredRendererDataMembersPresent"),
            ReadBool(diagnostic, "requiredKandraMeshMembersPresent"),
            ReadBool(diagnostic, "managerDependencyMethodsPresent"),
            ReadBool(diagnostic, "finalizeRegistrationMethodPresent"),
            ReadBool(diagnostic, "onEarlyUpdateBeginMethodPresent"),
            ReadBool(diagnostic, "onBeginRenderingMethodPresent"),
            ReadBool(diagnostic, "queueStateFieldsPresent"),
            ReadString(diagnostic, "diagnosticBoundary"),
            ReadBool(diagnostic, "registrationMethodInvoked"),
            ReadBool(diagnostic, "canRegisterMethodsInvoked"),
            ReadBool(diagnostic, "createdOrActivatedKandraObject"),
            ReadBool(diagnostic, "runtimeRegistrationInvocationAllowed"),
            ReadBool(diagnostic, "runtimeRegistrationExecuted"),
            ReadBool(diagnostic, "candidateMapApplicationExecuted"),
            ReadBool(diagnostic, "conversionExecuted"),
            ReadBool(diagnostic, "itemEquipMutationExecuted"),
            ReadBool(diagnostic, "saveMutationExecuted"),
            ReadBool(diagnostic, "nativeGameWriteExecuted"),
            ReadBool(diagnostic, "downstreamWritesExecuted"));
    }

    private static KandraRuntimeRegistrationHostProofResult ValidateLiveRegistrationHostProofArtifact()
    {
        string receiptPath = ResolveFixturePath(
            "mods",
            "tainted-armour",
            "tests",
            "real-samples",
            "live-registration",
            "real-skinned-triangle",
            "kandra-runtime-registration.receipt.json");
        KandraRuntimeRegistrationHostReceipt receipt = ReadKandraRuntimeRegistrationHostReceipt(receiptPath);
        return new KandraRuntimeRegistrationHostProofStage().Validate(
            new KandraRuntimeRegistrationHostProofRequest(
                "fixture.kandra-runtime-registration-host-proof.real-skinned-triangle",
                receipt));
    }

    private static KandraRuntimeRegistrationHostReceipt ReadKandraRuntimeRegistrationHostReceipt(string receiptPath)
    {
        if (!File.Exists(receiptPath))
        {
            throw new FileNotFoundException("Live Kandra runtime registration receipt not found.", receiptPath);
        }

        using JsonDocument document = JsonDocument.Parse(File.ReadAllText(receiptPath));
        JsonElement root = document.RootElement;
        JsonElement context = RequiredProperty(root, "Context");
        JsonElement attempt = RequiredProperty(root, "Attempt");
        return new KandraRuntimeRegistrationHostReceipt(
            ReadString(root, "StageVersion"),
            ReadString(root, "WrittenAtUtc"),
            ReadString(root, "Trigger"),
            new KandraRuntimeRegistrationInvocationContext(
                ReadString(context, "RequestId"),
                ReadString(context, "DryRunRequestId"),
                ReadString(context, "ApprovalId"),
                ReadString(context, "ModDirectory"),
                ReadString(context, "Name"),
                ReadInt(context, "VertexCount"),
                ReadLong(context, "IndicesCount"),
                ReadInt(context, "BindposesCount"),
                ReadInt(context, "BlendshapeCount"),
                ReadString(context, "LayoutVersion"),
                ReadString(context, "KandraDirectory"),
                ReadString(context, "MeshDataPath"),
                ReadString(context, "IndicesDataPath"),
                ReadLong(context, "MeshDataByteCount"),
                ReadLong(context, "IndicesDataByteCount"),
                ReadString(context, "MeshDataSha256"),
                ReadString(context, "IndicesDataSha256"),
                ReadString(context, "RegistrationOwner"),
                ReadString(context, "RegistrationMethodSignature"),
                ReadString(context, "RegistrationMethodFingerprint"),
                ReadString(context, "RegistrationAssemblySha256")),
            new KandraRuntimeRegistrationInvocationAttempt(
                ReadBool(attempt, "RegistrationMethodInvoked"),
                ReadBool(attempt, "CanRegisterMethodsInvoked"),
                ReadBool(attempt, "CreatedOrActivatedKandraObject"),
                ReadBool(attempt, "RuntimeRegistrationExecuted"),
                ReadBool(attempt, "RuntimeRegistrationSucceeded"),
                ReadString(attempt, "KandraIsRegisteredStatus"),
                ReadString(attempt, "KandraTryGetMeshMemoryStatus"),
                ReadString(attempt, "RegisteredRendererIdentity"),
                ReadString(attempt, "RegisteredMeshMemoryIdentity"),
                ReadStringArray(RequiredProperty(attempt, "Blockers")),
                ReadStringArray(RequiredProperty(attempt, "Warnings"))),
            ReadBool(root, "CandidateMapApplicationExecuted"),
            ReadBool(root, "ConversionExecuted"),
            ReadBool(root, "ItemEquipMutationExecuted"),
            ReadBool(root, "SaveMutationExecuted"),
            ReadBool(root, "NativeGameWriteExecuted"),
            ReadBool(root, "NonRegistrationDownstreamWritesExecuted"));
    }

    private static IEnumerable<VisualRuntimeObservationRow> ReadRows(JsonElement rowsJson)
    {
        Check(rowsJson.ValueKind == JsonValueKind.Array, "Live packet rows must be an array.");
        foreach (JsonElement row in rowsJson.EnumerateArray())
        {
            yield return new VisualRuntimeObservationRow(
                ReadString(row, "sex"),
                ReadString(row, "deformationReceiver"),
                ReadInt(row, "inversionRecordCount"),
                ReadString(row, "liveObjectPath"),
                ReadString(row, "rendererName"),
                ReadString(row, "meshName"),
                ReadString(row, "materialNames"),
                ReadString(row, "animatorClipNames"),
                ReadString(row, "normalDotClassification"),
                ReadString(row, "referenceTransportSemantics"),
                ReadString(row, "localWindingSemantics"),
                ReadString(row, "clippingStatus"),
                ReadString(row, "seamStatus"),
                ReadString(row, "bodyCoverageStatus"),
                ReadString(row, "materialShadowStatus"),
                ReadString(row, "layerStatus"),
                ReadString(row, "notes"),
                ReadString(row, "kandraIsRegisteredStatus"),
                ReadString(row, "kandraTryGetMeshMemoryStatus"));
        }
    }

    private static JsonElement RequiredProperty(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out JsonElement value))
        {
            throw new InvalidOperationException("Missing packet property: " + propertyName);
        }

        return value;
    }

    private static string ReadString(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out JsonElement value) && value.ValueKind != JsonValueKind.Null
            ? value.GetString() ?? string.Empty
            : string.Empty;

    private static int ReadInt(JsonElement element, string propertyName) =>
        RequiredProperty(element, propertyName).GetInt32();

    private static long ReadLong(JsonElement element, string propertyName) =>
        RequiredProperty(element, propertyName).GetInt64();

    private static bool ReadBool(JsonElement element, string propertyName)
    {
        JsonElement value = RequiredProperty(element, propertyName);
        if (value.ValueKind == JsonValueKind.True)
        {
            return true;
        }

        if (value.ValueKind == JsonValueKind.False)
        {
            return false;
        }

        if (value.ValueKind == JsonValueKind.String && bool.TryParse(value.GetString(), out bool parsed))
        {
            return parsed;
        }

        throw new InvalidOperationException("Packet property must be boolean: " + propertyName);
    }

    private static bool IsObservedTrue(string value) =>
        value.StartsWith("observed:True", StringComparison.Ordinal);

    private static string[] ReadStringArray(JsonElement element)
    {
        Check(element.ValueKind == JsonValueKind.Array, "Receipt property must be an array.");
        return element.EnumerateArray()
            .Select(value => value.GetString() ?? string.Empty)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .ToArray();
    }

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

    private static void MismatchedPoseFailsClosed()
    {
        ArmorImportResult result = Import(
            "mismatched-pose",
            new[]
            {
                new ArmorVector3(0d, 0d, 0d),
                new ArmorVector3(1d, 0d, 0d),
                new ArmorVector3(0d, 1d, 0d),
                new ArmorVector3(0d, 0d, 1d),
            });
        Check(result.Status == ArmorImportStatus.DeformationBlocked, "A pose vertex-count mismatch must block deformation.");
        Check(result.Deformation.IndeterminateTriangleCount == 1, "A pose vertex-count mismatch must produce indeterminate triangle evidence.");
        Check(
            result.Deformation.Poses.Single().TriangleEvidence.Single().IndeterminateReason == "pose_vertex_count_mismatch",
            "The fail-closed reason must be retained in typed evidence.");
    }

    private static ArmorImportResult Import(string id, IEnumerable<ArmorVector3> posed) =>
        new ArmorImporter().Import(CreateRequest(id, posed));

    private static ArmorImportRequest CreateRequest(string id, IEnumerable<ArmorVector3> posed, VisualRuntimeObservationRequest? visualRuntimeObservation = null)
    {
        var source = new ArmorSourceContract(
            "armor.fixture",
            "sha256:source",
            "body-mesh",
            0,
            "body-material",
            "sha256:rig",
            "sha256:bind-pose",
            "sha256:weights",
            3,
            1,
            3,
            4,
            0);
        var target = new ArmorTargetContract(
            "native.body",
            "male",
            "Armor",
            "sha256:target",
            "sha256:candidate-map",
            "candidate_map_ready_not_applied");
        var deformation = new DeformationValidationRequest(
            new PoseGeometrySnapshot("baseline", "rest", 0d, "sha256:baseline", Baseline),
            new[] { new PoseGeometrySnapshot("pose." + id, "clip.fixture", 0.5d, "sha256:pose:" + id, posed) },
            new[] { new ArmorTriangle(0, 0, 1, 2) },
            new[] { 0, 1, 2 },
            DeformationPolicy.CreateT29T31ZeroTolerance());
        return new ArmorImportRequest("request." + id, source, target, deformation, visualRuntimeObservation);
    }

    private static VisualRuntimeObservationRequest CreateVisualRuntimeObservation(
        string liveBodyStatus,
        string liveEquipStatus,
        string poseBindingStatus,
        string metricStatus,
        string kandraIsRegisteredStatus = "",
        string kandraTryGetMeshMemoryStatus = "") =>
        VisualRuntimeObservationRequest.CreateA2KT34NoWriteObserver(
            "fixture",
            "fixture://a2k-t34",
            "fixture-scene",
            liveBodyStatus,
            liveEquipStatus,
            poseBindingStatus,
            new[]
            {
                CreateVisualRuntimeRow("female", "neck_02", 3, metricStatus, kandraIsRegisteredStatus, kandraTryGetMeshMemoryStatus),
                CreateVisualRuntimeRow("female", "spine_04", 208, metricStatus, kandraIsRegisteredStatus, kandraTryGetMeshMemoryStatus),
                CreateVisualRuntimeRow("female", "spine_05", 11, metricStatus, kandraIsRegisteredStatus, kandraTryGetMeshMemoryStatus),
                CreateVisualRuntimeRow("male", "neck_02", 3, metricStatus, kandraIsRegisteredStatus, kandraTryGetMeshMemoryStatus),
                CreateVisualRuntimeRow("male", "spine_04", 208, metricStatus, kandraIsRegisteredStatus, kandraTryGetMeshMemoryStatus),
                CreateVisualRuntimeRow("male", "spine_05", 11, metricStatus, kandraIsRegisteredStatus, kandraTryGetMeshMemoryStatus),
            });

    private static VisualRuntimeObservationRow CreateVisualRuntimeRow(
        string sex,
        string receiver,
        int inversionRecordCount,
        string metricStatus,
        string kandraIsRegisteredStatus = "",
        string kandraTryGetMeshMemoryStatus = "") =>
        new VisualRuntimeObservationRow(
            sex,
            receiver,
            inversionRecordCount,
            "/Root/UI/Spawned/CharacterCreator:0:HeroRenderer:0",
            "Renderer_" + sex + "_" + receiver,
            "Mesh_" + sex + "_" + receiver,
            "Mat_Body",
            "Anim_Hero_TPP_Base_Knockdown_Air_Loop",
            "t56-normal-dot-classification-retained",
            metricStatus,
            metricStatus,
            metricStatus,
            metricStatus,
            metricStatus,
            metricStatus,
            metricStatus,
            "fixture row",
            kandraIsRegisteredStatus,
            kandraTryGetMeshMemoryStatus);

    private static string Signature(ArmorImportResult result)
    {
        TriangleDeformationEvidence record = result.Deformation.Poses.Single().TriangleEvidence.Single();
        return string.Join(
            "|",
            result.Status,
            result.Deformation.StageVersion,
            result.Deformation.MetricVersion,
            result.Deformation.CollapsedTriangleCount,
            result.Deformation.OrientationReversedTriangleCount,
            result.Deformation.IndeterminateTriangleCount,
            record.BaselineArea.ToString("R", System.Globalization.CultureInfo.InvariantCulture),
            record.PosedArea.ToString("R", System.Globalization.CultureInfo.InvariantCulture),
            record.NormalizedNormalDot.ToString("R", System.Globalization.CultureInfo.InvariantCulture),
            string.Join(",", result.Blockers));
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private static byte[] Bytes(int length, int seed)
    {
        byte[] bytes = new byte[length];
        for (int index = 0; index < bytes.Length; index++)
        {
            bytes[index] = (byte)((seed + index) & 0xFF);
        }

        return bytes;
    }

    private sealed class SampledMeshRecord
    {
        private readonly ArmorVector3[] baseline;
        private readonly ArmorTriangle[] triangles;

        public SampledMeshRecord(string assetId, string fingerprint, string meshId, ArmorVector3[] baseline, ArmorTriangle[] triangles)
        {
            AssetId = assetId;
            Fingerprint = fingerprint;
            MeshId = meshId;
            this.baseline = baseline;
            this.triangles = triangles;
        }

        public string AssetId { get; }
        public string Fingerprint { get; }
        public string MeshId { get; }

        public ArmorImportRequest ToImportRequest(ArmorVector3[] posed)
        {
            var source = new ArmorSourceContract(
                AssetId,
                Fingerprint,
                MeshId,
                0,
                "body-material",
                "sha256:rig",
                "sha256:bind-pose",
                "sha256:weights",
                baseline.Length,
                triangles.Length,
                baseline.Length,
                4,
                0);
            var target = new ArmorTargetContract(
                "native.body",
                "male",
                "Armor",
                "sha256:target",
                "sha256:candidate-map",
                "candidate_map_ready_not_applied");
            var deformation = new DeformationValidationRequest(
                new PoseGeometrySnapshot("baseline", "rest", 0d, "sha256:baseline", baseline),
                new[] { new PoseGeometrySnapshot("pose.adapter", "clip.adapter", 0.5d, "sha256:pose", posed) },
                triangles,
                Enumerable.Range(0, baseline.Length),
                DeformationPolicy.CreateT29T31ZeroTolerance());
            return new ArmorImportRequest("request.adapter", source, target, deformation);
        }
    }
}
