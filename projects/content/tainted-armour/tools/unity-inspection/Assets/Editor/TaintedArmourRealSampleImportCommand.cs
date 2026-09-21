using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Tainted.Armour;
using Tainted.Armour.Unity;
using UnityEditor;
using UnityEngine;

namespace TaintedArmour.Editor
{
    public static class TaintedArmourRealSampleImportCommand
    {
        private const string ReadyMarker = "TAINTED_ARMOUR_REAL_SAMPLE_IMPORT_EXECUTED_DEFORMATION_BLOCKED_DOWNSTREAM_FALSE";
        private const string BlockedMarker = "TAINTED_ARMOUR_REAL_SAMPLE_IMPORT_BLOCKED";
        private const string SourceAssetPath = "Assets/DragonKnightArmorA2KT20/SK_Dragon_knight_UE5_no_Cape.fbx";
        private const string TargetPose = "fall";
        private const string TargetClip = "Anim_Hero_TPP_Base_Knockdown_Air_Loop";
        private const int SourceSubmeshIndex = 3;
        private const int ExpectedTotalOrientationReversed = 444;
        private const int ExpectedTotalCollapsed = 0;
        private const float DisplacementEpsilon = 0.000001f;

        private static readonly RequiredRow[] RequiredRows =
        {
            new RequiredRow("female", "neck_02", "extra_neck", 252, 617, 3),
            new RequiredRow("female", "spine_04", "extra_spine", 8255, 15245, 208),
            new RequiredRow("female", "spine_05", "extra_spine", 370, 879, 11),
            new RequiredRow("male", "neck_02", "extra_neck", 252, 617, 3),
            new RequiredRow("male", "spine_04", "extra_spine", 8255, 15245, 208),
            new RequiredRow("male", "spine_05", "extra_spine", 370, 879, 11),
        };

        public static void Run()
        {
            string reportPath = GetArgument("-taintedArmourRealSampleReport", string.Empty, false);
            var report = new RealSampleReport
            {
                marker = BlockedMarker,
                generatedAtUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture),
                unityVersion = Application.unityVersion,
                projectRoot = ProjectRoot,
                sourceAssetPath = SourceAssetPath,
                sourceSubmeshIndex = SourceSubmeshIndex,
                targetPose = TargetPose,
                targetClip = TargetClip,
                expectedTotalOrientationReversed = ExpectedTotalOrientationReversed,
                expectedTotalCollapsed = ExpectedTotalCollapsed,
                validationTransportStagingUsed = true,
                downstream = new DownstreamBoundary(),
                rows = Array.Empty<RowReceipt>(),
                blockers = Array.Empty<string>(),
                errors = Array.Empty<string>(),
            };
            int exitCode = 1;

            try
            {
                if (string.IsNullOrWhiteSpace(reportPath))
                {
                    throw new ArgumentException("Missing -taintedArmourRealSampleReport.");
                }

                string poseDriverPath = GetArgument("-taintedArmourPoseDriverInput", string.Empty);
                string bundleRoot = GetArgument("-taintedArmourBundleRoot", string.Empty);
                string fullGeometryCaptureRoot = GetArgument("-taintedArmourFullGeometryCaptureRoot", string.Empty, false);
                report.fullGeometryCaptureRoot = fullGeometryCaptureRoot;
                Execute(report, poseDriverPath, bundleRoot);
                exitCode = report.marker == ReadyMarker ? 0 : 1;
            }
            catch (Exception exception)
            {
                report.errors = new[] { exception.ToString() };
                report.blockers = report.blockers
                    .Concat(new[] { "real_sample_import_exception" })
                    .Distinct(StringComparer.Ordinal)
                    .ToArray();
            }

            if (!string.IsNullOrWhiteSpace(reportPath))
            {
                WriteReport(reportPath, report);
            }

            if (exitCode == 0)
            {
                Debug.Log(report.marker + ": " + reportPath);
            }
            else
            {
                Debug.LogError(report.marker + ": " + reportPath + " blockers=" + string.Join(",", report.blockers));
            }

            EditorApplication.Exit(exitCode);
        }

        private static void Execute(RealSampleReport report, string poseDriverPath, string bundleRoot)
        {
            if (!File.Exists(poseDriverPath)) throw new FileNotFoundException("Pose-driver input was not found.", poseDriverPath);
            if (!Directory.Exists(bundleRoot)) throw new DirectoryNotFoundException("FoA bundle root was not found: " + bundleRoot);

            string physicalSourcePath = Path.Combine(ProjectRoot, SourceAssetPath.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(physicalSourcePath)) throw new FileNotFoundException("Staged source FBX was not found.", physicalSourcePath);
            report.sourceFingerprintBefore = "sha256:" + HashFile(physicalSourcePath);

            T26Record t26 = JsonUtility.FromJson<T26Record>(File.ReadAllText(poseDriverPath));
            if (t26 == null || !string.Equals(t26.marker, "DRAGON_KNIGHT_ARMOR_A2K_T26_TAINTED_GRAIL_NATIVE_POSE_DRIVER_SOURCE_PACKET_READY", StringComparison.Ordinal))
            {
                throw new InvalidDataException("The pose-driver input is not the ready A2K-T26 record.");
            }

            PoseDriverRow fallPose = (t26.poseDriverRows ?? Array.Empty<PoseDriverRow>())
                .FirstOrDefault(row => row != null && string.Equals(row.pose, TargetPose, StringComparison.Ordinal));
            if (fallPose == null) throw new InvalidDataException("The pose-driver input has no fall row.");

            GameObject sourceAsset = AssetDatabase.LoadAssetAtPath<GameObject>(SourceAssetPath);
            if (sourceAsset == null) throw new InvalidOperationException("Unity could not load the staged source FBX.");

            GameObject instance = UnityEngine.Object.Instantiate(sourceAsset);
            instance.name = "TaintedArmourRealSampleImport";
            instance.hideFlags = HideFlags.HideAndDontSave;
            AssetBundle retainedBundle = null;
            AvatarBuildResult avatarResult = null;

            try
            {
                SkinnedMeshRenderer renderer = SelectCombinedRenderer(instance.GetComponentsInChildren<SkinnedMeshRenderer>(true));
                if (renderer == null || renderer.sharedMesh == null)
                {
                    throw new InvalidOperationException("No skinned renderer with source submesh 3 was found.");
                }

                Mesh sourceMesh = renderer.sharedMesh;
                report.selectedRenderer = renderer.name;
                report.sourceMeshName = sourceMesh.name;
                report.sourceVertexCount = sourceMesh.vertexCount;
                report.sourceSubmeshTriangleCount = sourceMesh.GetTriangles(SourceSubmeshIndex).Length / 3;
                if (report.sourceVertexCount != 43159 || report.sourceSubmeshTriangleCount != 25660)
                {
                    throw new InvalidDataException(
                        "The real sample geometry does not match the T56 source: vertices="
                        + report.sourceVertexCount.ToString(CultureInfo.InvariantCulture)
                        + " triangles="
                        + report.sourceSubmeshTriangleCount.ToString(CultureInfo.InvariantCulture)
                        + ".");
                }

                avatarResult = BuildTransientHumanoidAvatar(instance);
                if (avatarResult.avatar == null || !avatarResult.avatar.isValid || !avatarResult.avatar.isHuman)
                {
                    throw new InvalidOperationException("The transient source avatar is not valid and human.");
                }

                report.sourceAvatarName = avatarResult.avatar.name;
                report.sourceAvatarHumanBoneCount = avatarResult.humanBoneCount;
                report.sourceAvatarMissingHumanBoneNames = avatarResult.missingHumanBoneNames;

                Animator animator = instance.GetComponentInChildren<Animator>();
                if (animator == null) animator = instance.AddComponent<Animator>();
                animator.avatar = avatarResult.avatar;
                animator.applyRootMotion = false;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

                LoadedClip loaded = LoadTargetClip(fallPose, bundleRoot, out retainedBundle);
                report.clipBundlePath = loaded.bundlePath;
                report.clipLength = loaded.clip.length;
                report.sampleTime = loaded.clip.length <= 0f
                    ? 0f
                    : Mathf.Min(loaded.clip.length * 0.5f, Mathf.Max(0f, loaded.clip.length - 0.001f));

                var adapter = new UnityGeometrySnapshotAdapter();
                report.coreAssemblySha256 = HashAssembly(typeof(ArmorImporter).Assembly);
                report.adapterAssemblySha256 = HashAssembly(typeof(UnityGeometrySnapshotAdapter).Assembly);

                TransformState[] baselineTransforms = TransformState.Capture(instance);
                PoseGeometrySnapshot baseline = adapter.CaptureBakedSnapshot(renderer, "baseline", "rest", 0d);
                ArmorTriangle[] submeshTriangles = adapter.CaptureSubmeshTriangles(sourceMesh, SourceSubmeshIndex);
                WriteFullGeometryCapture(report, sourceMesh, renderer, instance.transform, baseline, submeshTriangles);

                TransformState[] beforeSample = TransformState.Capture(instance);
                loaded.clip.SampleAnimation(instance, report.sampleTime);
                TransformState[] afterSample = TransformState.Capture(instance);
                report.changedTransformCount = TransformState.CountChanged(beforeSample, afterSample);
                if (report.changedTransformCount == 0)
                {
                    throw new InvalidOperationException("The fall Air_Loop clip did not change any source transform.");
                }

                PoseGeometrySnapshot posed = adapter.CaptureBakedSnapshot(renderer, TargetPose, TargetClip, report.sampleTime);
                report.baselineGeometryFingerprint = baseline.GeometryFingerprint;
                report.posedGeometryFingerprint = posed.GeometryFingerprint;

                Transform[] bones = renderer.bones ?? Array.Empty<Transform>();
                BoneWeight[] boneWeights = sourceMesh.boneWeights ?? Array.Empty<BoneWeight>();
                report.sourceBoneCount = bones.Length;
                report.sourceBoneWeightCount = boneWeights.Length;
                if (boneWeights.Length != sourceMesh.vertexCount)
                {
                    throw new InvalidDataException("The source bone-weight count does not match the source vertex count.");
                }

                string rigFingerprint = HashText(string.Join("|", bones.Select(bone => RelativeTransformPath(instance.transform, bone))));
                string bindPoseFingerprint = HashMatrices(sourceMesh.bindposes ?? Array.Empty<Matrix4x4>());
                string boneWeightFingerprint = HashBoneWeights(boneWeights);
                int invalidInfluenceCount = CountInvalidInfluences(boneWeights, bones.Length);
                string materialSlot = renderer.sharedMaterials != null
                    && renderer.sharedMaterials.Length > SourceSubmeshIndex
                    && renderer.sharedMaterials[SourceSubmeshIndex] != null
                    ? renderer.sharedMaterials[SourceSubmeshIndex].name
                    : "submesh:" + SourceSubmeshIndex.ToString(CultureInfo.InvariantCulture);

                var rowReceipts = new List<RowReceipt>();
                foreach (RequiredRow required in RequiredRows)
                {
                    WeightedGeometry weighted = SelectWeightedGeometry(required, bones, boneWeights, submeshTriangles);
                    var source = new ArmorSourceContract(
                        SourceAssetPath,
                        report.sourceFingerprintBefore,
                        sourceMesh.name,
                        SourceSubmeshIndex,
                        materialSlot,
                        rigFingerprint,
                        bindPoseFingerprint,
                        boneWeightFingerprint,
                        sourceMesh.vertexCount,
                        weighted.triangles.Length,
                        weighted.vertexIndices.Length,
                        4,
                        invalidInfluenceCount);
                    var target = new ArmorTargetContract(
                        "a2k-t56.deformation-target",
                        required.targetGender,
                        "Cuirass",
                        "sha256:" + HashText(TargetPose + "|" + TargetClip + "|" + required.targetGender),
                        "not-applied:" + HashText("candidate-map-not-consumed"),
                        "not_applied_validation_only");
                    var request = new ArmorImportRequest(
                        "real-sample." + required.targetGender + "." + required.sourceBoneRaw,
                        source,
                        target,
                        new DeformationValidationRequest(
                            baseline,
                            new[] { posed },
                            weighted.triangles,
                            weighted.vertexIndices,
                            DeformationPolicy.CreateT29T31ZeroTolerance()));

                    ArmorImportResult result = new ArmorImporter().Import(request);
                    rowReceipts.Add(ToReceipt(required, result, weighted));
                }

                report.rows = rowReceipts.ToArray();
                TransformState.Restore(baselineTransforms);
            }
            finally
            {
                if (retainedBundle != null) retainedBundle.Unload(false);
                UnityEngine.Object.DestroyImmediate(instance);
                if (avatarResult != null && avatarResult.avatar != null)
                {
                    UnityEngine.Object.DestroyImmediate(avatarResult.avatar);
                }
            }

            report.sourceFingerprintAfter = "sha256:" + HashFile(physicalSourcePath);
            report.sourceFbxMutationExecuted = !string.Equals(
                report.sourceFingerprintBefore,
                report.sourceFingerprintAfter,
                StringComparison.OrdinalIgnoreCase);
            Complete(report);
        }

        private static RowReceipt ToReceipt(RequiredRow required, ArmorImportResult result, WeightedGeometry weighted)
        {
            PoseDeformationResult pose = result.Deformation.Poses.Single();
            return new RowReceipt
            {
                requestId = result.RequestId,
                targetGender = required.targetGender,
                sourceBoneRaw = required.sourceBoneRaw,
                policyFamily = required.policyFamily,
                weightedVertexCount = weighted.vertexIndices.Length,
                expectedWeightedVertexCount = required.expectedWeightedVertexCount,
                weightedTriangleCount = weighted.triangles.Length,
                expectedWeightedTriangleCount = required.expectedWeightedTriangleCount,
                importInvoked = true,
                importStatus = result.Status.ToString(),
                deformationAccepted = result.Deformation.DeformationAccepted,
                calibrationStageVersion = result.Deformation.Controls.CalibrationStageVersion,
                controlsPassed = result.Deformation.Controls.Passed,
                controlFailureCount = result.Deformation.ControlFailureCount,
                outOfPlaneRigidRotationPassed = result.Deformation.Controls.OutOfPlaneRigidRotationPassed,
                outOfPlaneRigidRotationDiagnosticFailed = result.Deformation.Warnings.Contains(
                    "control_out_of_plane_rigid_rotation_diagnostic_failed:" + result.Deformation.MetricVersion),
                metricCalibrationFailed = result.Deformation.Blockers.Contains("metric_calibration_failed:" + result.Deformation.MetricVersion),
                candidateSupplementalMetricCompared = result.Deformation.Controls.CandidateSupplementalMetricCompared,
                candidateMetricVersion = result.Deformation.Controls.CandidateMetricVersion,
                candidateMetricComparisonCount = result.Deformation.Controls.CandidateMetricComparisonCount,
                candidateMetricExpectedMatchCount = result.Deformation.Controls.CandidateMetricExpectedMatchCount,
                candidateMetricDisagreementCount = result.Deformation.Controls.CandidateMetricDisagreementCount,
                candidateMetricAffectsAcceptance = result.Deformation.Controls.CandidateMetricAffectsAcceptance,
                orientationReversedTriangleCount = result.Deformation.OrientationReversedTriangleCount,
                expectedOrientationReversedTriangleCount = required.expectedOrientationReversedTriangleCount,
                orientationReversedTriangleOrdinals = pose.TriangleEvidence
                    .Where(value => value.OrientationReversed)
                    .Select(value => value.TriangleOrdinal)
                    .ToArray(),
                collapsedTriangleCount = result.Deformation.CollapsedTriangleCount,
                indeterminateTriangleCount = result.Deformation.IndeterminateTriangleCount,
                movedVertexCount = pose.MovedVertexCount,
                maxVertexDisplacement = pose.MaxVertexDisplacement,
                meanVertexDisplacement = pose.MeanVertexDisplacement,
                candidateMapApplicationAllowed = result.CandidateMapApplicationAllowed,
                candidateMapApplicationExecuted = result.CandidateMapApplicationExecuted,
                conversionAllowed = result.ConversionAllowed,
                conversionExecuted = result.ConversionExecuted,
                sidecarsGenerated = result.SidecarsGenerated,
                sourceFbxMutationExecuted = result.SourceFbxMutationExecuted,
                unityProjectAssetMutationExecuted = result.UnityProjectAssetMutationExecuted,
                visualRuntimeStageVersion = result.VisualRuntime.StageVersion,
                visualRuntimeRouteId = result.VisualRuntime.RouteId,
                visualRuntimeRequested = result.VisualRuntime.Requested,
                liveRuntimeMetricsObserved = result.VisualRuntime.LiveRuntimeMetricsObserved,
                visualRuntimeAccepted = result.VisualRuntime.VisualRuntimeAccepted,
                visualRuntimeObserverMissing = result.VisualRuntime.Blockers.Contains("a2k_t34_observer_request_missing"),
                visualRuntimeBlockedRowCount = result.VisualRuntime.BlockedRowCount,
                runtimeLoaderChanged = result.RuntimeLoaderChanged,
                runtimeRegistrationExecuted = result.RuntimeRegistrationExecuted,
                itemRegistrationExecuted = result.ItemRegistrationExecuted,
                inventoryEquipSaveMutationExecuted = result.InventoryEquipSaveMutationExecuted,
                saveWriteExecuted = result.SaveWriteExecuted,
                nativeGameWriteExecuted = result.NativeGameWriteExecuted,
                releaseReady = result.ReleaseReady,
                blockers = result.Blockers.ToArray(),
                warnings = result.Deformation.Warnings.ToArray(),
            };
        }

        private static void Complete(RealSampleReport report)
        {
            RowReceipt[] rows = report.rows ?? Array.Empty<RowReceipt>();
            int totalReversed = rows.Sum(row => row.orientationReversedTriangleCount);
            int totalCollapsed = rows.Sum(row => row.collapsedTriangleCount);
            int totalIndeterminate = rows.Sum(row => row.indeterminateTriangleCount);
            bool rowParity = rows.Length == RequiredRows.Length
                && rows.All(row => row.weightedVertexCount == row.expectedWeightedVertexCount)
                && rows.All(row => row.weightedTriangleCount == row.expectedWeightedTriangleCount)
                && rows.All(row => row.orientationReversedTriangleCount == row.expectedOrientationReversedTriangleCount)
                && rows.All(row => row.collapsedTriangleCount == 0)
                && rows.All(row => row.indeterminateTriangleCount == 0);
            bool productionFlow = rows.Length == RequiredRows.Length
                && rows.All(row => row.importInvoked)
                && rows.All(row => row.importStatus == ArmorImportStatus.DeformationBlocked.ToString())
                && rows.All(row => !row.deformationAccepted)
                && rows.All(row => row.controlsPassed)
                && rows.All(row => row.controlFailureCount == 0)
                && rows.All(row => !row.outOfPlaneRigidRotationPassed)
                && rows.All(row => row.outOfPlaneRigidRotationDiagnosticFailed)
                && rows.All(row => !row.metricCalibrationFailed)
                && rows.All(row => !row.candidateSupplementalMetricCompared)
                && rows.All(row => row.candidateMetricComparisonCount == 0)
                && rows.All(row => row.candidateMetricExpectedMatchCount == 0)
                && rows.All(row => row.candidateMetricDisagreementCount == 0)
                && rows.All(row => !row.candidateMetricAffectsAcceptance)
                && rows.All(row => !row.visualRuntimeRequested)
                && rows.All(row => !row.liveRuntimeMetricsObserved)
                && rows.All(row => !row.visualRuntimeAccepted)
                && rows.All(row => !row.visualRuntimeObserverMissing)
                && rows.All(row => row.visualRuntimeBlockedRowCount == 0);
            bool resultBoundary = rows.All(row =>
                !row.candidateMapApplicationAllowed
                && !row.candidateMapApplicationExecuted
                && !row.conversionAllowed
                && !row.conversionExecuted
                && !row.sidecarsGenerated
                && !row.sourceFbxMutationExecuted
                && !row.unityProjectAssetMutationExecuted
                && !row.runtimeLoaderChanged
                && !row.runtimeRegistrationExecuted
                && !row.itemRegistrationExecuted
                && !row.inventoryEquipSaveMutationExecuted
                && !row.saveWriteExecuted
                && !row.nativeGameWriteExecuted
                && !row.releaseReady);
            bool downstreamBoundary = report.downstream != null && report.downstream.AllFalse;
            bool sourceStable = !report.sourceFbxMutationExecuted
                && string.Equals(report.sourceFingerprintBefore, report.sourceFingerprintAfter, StringComparison.OrdinalIgnoreCase);

            report.summary = new Summary
            {
                importInvocationCount = rows.Length,
                totalOrientationReversedTriangles = totalReversed,
                totalCollapsedTriangles = totalCollapsed,
                totalIndeterminateTriangles = totalIndeterminate,
                rowParityWithT56 = rowParity,
                productionImporterFlowVerified = productionFlow,
                metricCalibrationBoundaryVerified = productionFlow,
                candidateSupplementalMetricBoundaryVerified = productionFlow && rows.All(row => !row.candidateSupplementalMetricCompared),
                visualRuntimeObserverBoundaryVerified = productionFlow && rows.All(row => !row.visualRuntimeRequested),
                candidateMetricDisagreementCount = rows.Sum(row => row.candidateMetricDisagreementCount),
                candidateMapAndDownstreamResultsFalse = resultBoundary,
                everyDownstreamWriteFalse = downstreamBoundary,
                sourceFingerprintStable = sourceStable,
            };

            var blockers = new List<string>();
            if (!rowParity) blockers.Add("real_sample_row_parity_failed");
            if (totalReversed != ExpectedTotalOrientationReversed) blockers.Add("t56_orientation_reversal_total_mismatch");
            if (totalCollapsed != ExpectedTotalCollapsed) blockers.Add("t56_collapse_total_mismatch");
            if (totalIndeterminate != 0) blockers.Add("indeterminate_geometry_records_present");
            if (!productionFlow) blockers.Add("production_importer_flow_not_verified");
            if (!resultBoundary) blockers.Add("candidate_map_or_pipeline_result_boundary_not_false");
            if (!downstreamBoundary) blockers.Add("downstream_write_boundary_not_false");
            if (!sourceStable) blockers.Add("source_fbx_fingerprint_changed");
            if (report.changedTransformCount <= 0) blockers.Add("native_clip_did_not_change_source_transforms");
            report.blockers = blockers.ToArray();
            report.marker = blockers.Count == 0 ? ReadyMarker : BlockedMarker;
        }

        private static WeightedGeometry SelectWeightedGeometry(
            RequiredRow required,
            Transform[] bones,
            BoneWeight[] boneWeights,
            ArmorTriangle[] submeshTriangles)
        {
            int boneIndex = Array.FindIndex(
                bones,
                bone => bone != null && string.Equals(bone.name, required.sourceBoneRaw, StringComparison.Ordinal));
            if (boneIndex < 0) throw new InvalidDataException("Source bone was not found: " + required.sourceBoneRaw);

            var submeshVertices = new HashSet<int>();
            foreach (ArmorTriangle triangle in submeshTriangles)
            {
                submeshVertices.Add(triangle.Vertex0);
                submeshVertices.Add(triangle.Vertex1);
                submeshVertices.Add(triangle.Vertex2);
            }

            int[] weightedVertices = submeshVertices
                .Where(index => index >= 0 && index < boneWeights.Length && InfluenceWeight(boneWeights[index], boneIndex) > 0f)
                .OrderBy(index => index)
                .ToArray();
            var weightedSet = new HashSet<int>(weightedVertices);
            ArmorTriangle[] triangles = submeshTriangles
                .Where(triangle =>
                    weightedSet.Contains(triangle.Vertex0)
                    || weightedSet.Contains(triangle.Vertex1)
                    || weightedSet.Contains(triangle.Vertex2))
                .ToArray();
            return new WeightedGeometry { vertexIndices = weightedVertices, triangles = triangles };
        }

        private static float InfluenceWeight(BoneWeight weight, int boneIndex)
        {
            float total = 0f;
            if (weight.boneIndex0 == boneIndex) total += weight.weight0;
            if (weight.boneIndex1 == boneIndex) total += weight.weight1;
            if (weight.boneIndex2 == boneIndex) total += weight.weight2;
            if (weight.boneIndex3 == boneIndex) total += weight.weight3;
            return total;
        }

        private static int CountInvalidInfluences(BoneWeight[] weights, int boneCount)
        {
            int invalid = 0;
            foreach (BoneWeight weight in weights)
            {
                if (weight.weight0 > 0f && (weight.boneIndex0 < 0 || weight.boneIndex0 >= boneCount)) invalid++;
                if (weight.weight1 > 0f && (weight.boneIndex1 < 0 || weight.boneIndex1 >= boneCount)) invalid++;
                if (weight.weight2 > 0f && (weight.boneIndex2 < 0 || weight.boneIndex2 >= boneCount)) invalid++;
                if (weight.weight3 > 0f && (weight.boneIndex3 < 0 || weight.boneIndex3 >= boneCount)) invalid++;
            }
            return invalid;
        }

        private static LoadedClip LoadTargetClip(PoseDriverRow fallPose, string bundleRoot, out AssetBundle retainedBundle)
        {
            ClipRecord record = (fallPose.clipRecords ?? Array.Empty<ClipRecord>())
                .FirstOrDefault(item => item != null && string.Equals(item.clip, TargetClip, StringComparison.Ordinal));
            if (record == null) throw new InvalidDataException("The fall pose has no Air_Loop clip record.");

            string bundlePath = !string.IsNullOrWhiteSpace(record.bundlePath)
                ? record.bundlePath
                : Path.Combine(bundleRoot, record.bundle ?? string.Empty);
            if (!File.Exists(bundlePath)) throw new FileNotFoundException("The native clip bundle was not found.", bundlePath);

            retainedBundle = AssetBundle.LoadFromFile(bundlePath);
            if (retainedBundle == null) throw new InvalidOperationException("Unity could not load the native clip bundle.");
            AnimationClip clip = retainedBundle.LoadAsset<AnimationClip>(record.clip);
            if (clip == null)
            {
                clip = retainedBundle.LoadAllAssets<AnimationClip>()
                    .FirstOrDefault(candidate => candidate != null && string.Equals(candidate.name, record.clip, StringComparison.Ordinal));
            }
            if (clip == null) throw new InvalidOperationException("The native Air_Loop clip was not found in its bundle.");
            return new LoadedClip { clip = clip, bundlePath = bundlePath };
        }

        private static SkinnedMeshRenderer SelectCombinedRenderer(IEnumerable<SkinnedMeshRenderer> renderers)
        {
            return (renderers ?? Array.Empty<SkinnedMeshRenderer>())
                .Where(renderer => renderer != null && renderer.sharedMesh != null && renderer.sharedMesh.subMeshCount > SourceSubmeshIndex)
                .OrderByDescending(renderer => renderer.sharedMesh.vertexCount)
                .ThenBy(renderer => renderer.name, StringComparer.Ordinal)
                .FirstOrDefault();
        }

        private static AvatarBuildResult BuildTransientHumanoidAvatar(GameObject instance)
        {
            var transforms = instance.GetComponentsInChildren<Transform>(true)
                .GroupBy(transform => transform.name, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
            var human = new List<HumanBone>();
            var missing = new List<string>();

            AddHumanBone(human, transforms, HumanBodyBones.Hips, "pelvis", missing);
            AddHumanBone(human, transforms, HumanBodyBones.Spine, "spine_01", missing);
            AddHumanBone(human, transforms, HumanBodyBones.Chest, "spine_03", missing);
            AddHumanBone(human, transforms, HumanBodyBones.UpperChest, "spine_05", missing);
            AddHumanBone(human, transforms, HumanBodyBones.Neck, "neck_01", missing);
            AddHumanBone(human, transforms, HumanBodyBones.Head, "head", missing);
            AddHumanBone(human, transforms, HumanBodyBones.LeftShoulder, "clavicle_l", missing);
            AddHumanBone(human, transforms, HumanBodyBones.LeftUpperArm, "upperarm_l", missing);
            AddHumanBone(human, transforms, HumanBodyBones.LeftLowerArm, "lowerarm_l", missing);
            AddHumanBone(human, transforms, HumanBodyBones.LeftHand, "hand_l", missing);
            AddHumanBone(human, transforms, HumanBodyBones.RightShoulder, "clavicle_r", missing);
            AddHumanBone(human, transforms, HumanBodyBones.RightUpperArm, "upperarm_r", missing);
            AddHumanBone(human, transforms, HumanBodyBones.RightLowerArm, "lowerarm_r", missing);
            AddHumanBone(human, transforms, HumanBodyBones.RightHand, "hand_r", missing);
            AddHumanBone(human, transforms, HumanBodyBones.LeftUpperLeg, "thigh_l", missing);
            AddHumanBone(human, transforms, HumanBodyBones.LeftLowerLeg, "calf_l", missing);
            AddHumanBone(human, transforms, HumanBodyBones.LeftFoot, "foot_l", missing);
            AddHumanBone(human, transforms, HumanBodyBones.LeftToes, "ball_l", missing);
            AddHumanBone(human, transforms, HumanBodyBones.RightUpperLeg, "thigh_r", missing);
            AddHumanBone(human, transforms, HumanBodyBones.RightLowerLeg, "calf_r", missing);
            AddHumanBone(human, transforms, HumanBodyBones.RightFoot, "foot_r", missing);
            AddHumanBone(human, transforms, HumanBodyBones.RightToes, "ball_r", missing);
            AddFingerBones(human, transforms, missing, true);
            AddFingerBones(human, transforms, missing, false);

            var description = new HumanDescription
            {
                human = human.ToArray(),
                skeleton = transforms.Values.Select(ToSkeletonBone).ToArray(),
                upperArmTwist = 0.5f,
                lowerArmTwist = 0.5f,
                upperLegTwist = 0.5f,
                lowerLegTwist = 0.5f,
                armStretch = 0.05f,
                legStretch = 0.05f,
                feetSpacing = 0f,
                hasTranslationDoF = false,
            };
            Avatar avatar = AvatarBuilder.BuildHumanAvatar(instance, description);
            if (avatar != null)
            {
                avatar.name = "TaintedArmourTransientHumanoidAvatar";
                avatar.hideFlags = HideFlags.HideAndDontSave;
            }
            return new AvatarBuildResult
            {
                avatar = avatar,
                humanBoneCount = human.Count,
                missingHumanBoneNames = missing.ToArray(),
            };
        }

        private static void AddFingerBones(
            List<HumanBone> human,
            Dictionary<string, Transform> transforms,
            List<string> missing,
            bool left)
        {
            string side = left ? "l" : "r";
            AddFinger(human, transforms, missing, left ? HumanBodyBones.LeftThumbProximal : HumanBodyBones.RightThumbProximal, "thumb_01_" + side);
            AddFinger(human, transforms, missing, left ? HumanBodyBones.LeftThumbIntermediate : HumanBodyBones.RightThumbIntermediate, "thumb_02_" + side);
            AddFinger(human, transforms, missing, left ? HumanBodyBones.LeftThumbDistal : HumanBodyBones.RightThumbDistal, "thumb_03_" + side);
            AddFinger(human, transforms, missing, left ? HumanBodyBones.LeftIndexProximal : HumanBodyBones.RightIndexProximal, "index_01_" + side);
            AddFinger(human, transforms, missing, left ? HumanBodyBones.LeftIndexIntermediate : HumanBodyBones.RightIndexIntermediate, "index_02_" + side);
            AddFinger(human, transforms, missing, left ? HumanBodyBones.LeftIndexDistal : HumanBodyBones.RightIndexDistal, "index_03_" + side);
            AddFinger(human, transforms, missing, left ? HumanBodyBones.LeftMiddleProximal : HumanBodyBones.RightMiddleProximal, "middle_01_" + side);
            AddFinger(human, transforms, missing, left ? HumanBodyBones.LeftMiddleIntermediate : HumanBodyBones.RightMiddleIntermediate, "middle_02_" + side);
            AddFinger(human, transforms, missing, left ? HumanBodyBones.LeftMiddleDistal : HumanBodyBones.RightMiddleDistal, "middle_03_" + side);
            AddFinger(human, transforms, missing, left ? HumanBodyBones.LeftRingProximal : HumanBodyBones.RightRingProximal, "ring_01_" + side);
            AddFinger(human, transforms, missing, left ? HumanBodyBones.LeftRingIntermediate : HumanBodyBones.RightRingIntermediate, "ring_02_" + side);
            AddFinger(human, transforms, missing, left ? HumanBodyBones.LeftRingDistal : HumanBodyBones.RightRingDistal, "ring_03_" + side);
            AddFinger(human, transforms, missing, left ? HumanBodyBones.LeftLittleProximal : HumanBodyBones.RightLittleProximal, "pinky_01_" + side);
            AddFinger(human, transforms, missing, left ? HumanBodyBones.LeftLittleIntermediate : HumanBodyBones.RightLittleIntermediate, "pinky_02_" + side);
            AddFinger(human, transforms, missing, left ? HumanBodyBones.LeftLittleDistal : HumanBodyBones.RightLittleDistal, "pinky_03_" + side);
        }

        private static void AddFinger(
            List<HumanBone> human,
            Dictionary<string, Transform> transforms,
            List<string> missing,
            HumanBodyBones humanBone,
            string sourceBoneName)
        {
            AddHumanBone(human, transforms, humanBone, sourceBoneName, missing);
        }

        private static void AddHumanBone(
            List<HumanBone> human,
            Dictionary<string, Transform> transforms,
            HumanBodyBones humanBone,
            string sourceBoneName,
            List<string> missing)
        {
            if (!transforms.ContainsKey(sourceBoneName))
            {
                missing.Add(HumanTrait.BoneName[(int)humanBone] + "=" + sourceBoneName);
                return;
            }
            human.Add(new HumanBone
            {
                humanName = HumanTrait.BoneName[(int)humanBone],
                boneName = sourceBoneName,
                limit = new HumanLimit { useDefaultValues = true },
            });
        }

        private static SkeletonBone ToSkeletonBone(Transform transform)
        {
            return new SkeletonBone
            {
                name = transform.name,
                position = transform.localPosition,
                rotation = transform.localRotation,
                scale = transform.localScale,
            };
        }

        private static string HashAssembly(Assembly assembly)
        {
            return string.IsNullOrWhiteSpace(assembly.Location)
                ? "unavailable"
                : "sha256:" + HashFile(assembly.Location);
        }

        private static string HashFile(string path)
        {
            using (SHA256 sha256 = SHA256.Create())
            using (FileStream stream = File.OpenRead(path))
            {
                return ToHex(sha256.ComputeHash(stream));
            }
        }

        private static string HashText(string value)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                return ToHex(sha256.ComputeHash(Encoding.UTF8.GetBytes(value ?? string.Empty)));
            }
        }

        private static string HashMatrices(IEnumerable<Matrix4x4> matrices)
        {
            var text = new StringBuilder();
            foreach (Matrix4x4 matrix in matrices ?? Array.Empty<Matrix4x4>())
            {
                for (int row = 0; row < 4; row++)
                {
                    for (int column = 0; column < 4; column++)
                    {
                        text.Append(matrix[row, column].ToString("R", CultureInfo.InvariantCulture)).Append('|');
                    }
                }
            }
            return "sha256:" + HashText(text.ToString());
        }

        private static string HashBoneWeights(IEnumerable<BoneWeight> weights)
        {
            var text = new StringBuilder();
            foreach (BoneWeight weight in weights ?? Array.Empty<BoneWeight>())
            {
                text.Append(weight.boneIndex0).Append(':').Append(weight.weight0.ToString("R", CultureInfo.InvariantCulture)).Append('|');
                text.Append(weight.boneIndex1).Append(':').Append(weight.weight1.ToString("R", CultureInfo.InvariantCulture)).Append('|');
                text.Append(weight.boneIndex2).Append(':').Append(weight.weight2.ToString("R", CultureInfo.InvariantCulture)).Append('|');
                text.Append(weight.boneIndex3).Append(':').Append(weight.weight3.ToString("R", CultureInfo.InvariantCulture)).Append('|');
            }
            return "sha256:" + HashText(text.ToString());
        }

        private static string ToHex(byte[] bytes)
        {
            var text = new StringBuilder(bytes.Length * 2);
            foreach (byte value in bytes) text.Append(value.ToString("x2", CultureInfo.InvariantCulture));
            return text.ToString();
        }

        private static void WriteFullGeometryCapture(
            RealSampleReport report,
            Mesh sourceMesh,
            SkinnedMeshRenderer renderer,
            Transform instanceRoot,
            PoseGeometrySnapshot baseline,
            ArmorTriangle[] submeshTriangles)
        {
            if (string.IsNullOrWhiteSpace(report.fullGeometryCaptureRoot)) return;
            if (sourceMesh == null) throw new ArgumentNullException(nameof(sourceMesh));
            if (renderer == null) throw new ArgumentNullException(nameof(renderer));
            if (instanceRoot == null) throw new ArgumentNullException(nameof(instanceRoot));
            if (baseline == null) throw new ArgumentNullException(nameof(baseline));
            if (submeshTriangles == null) throw new ArgumentNullException(nameof(submeshTriangles));
            if (!BitConverter.IsLittleEndian)
            {
                throw new PlatformNotSupportedException("Full-geometry capture requires an explicit little-endian writer on this platform.");
            }

            Vector3[] normals = sourceMesh.normals ?? Array.Empty<Vector3>();
            Vector4[] tangents = sourceMesh.tangents ?? Array.Empty<Vector4>();
            Vector2[] uv0 = sourceMesh.uv ?? Array.Empty<Vector2>();
            BoneWeight[] boneWeights = sourceMesh.boneWeights ?? Array.Empty<BoneWeight>();
            Matrix4x4[] bindposes = sourceMesh.bindposes ?? Array.Empty<Matrix4x4>();
            Transform[] bones = renderer.bones ?? Array.Empty<Transform>();
            if (normals.Length != sourceMesh.vertexCount) throw new InvalidDataException("Full-geometry capture requires one normal per source vertex.");
            if (tangents.Length != sourceMesh.vertexCount) throw new InvalidDataException("Full-geometry capture requires one tangent per source vertex.");
            if (uv0.Length != sourceMesh.vertexCount) throw new InvalidDataException("Full-geometry capture requires one UV0 per source vertex.");
            if (boneWeights.Length != sourceMesh.vertexCount) throw new InvalidDataException("Full-geometry capture requires one bone-weight record per source vertex.");

            string root = Path.GetFullPath(report.fullGeometryCaptureRoot);
            string rawDirectory = Path.Combine(root, "raw");
            Directory.CreateDirectory(rawDirectory);
            string positionsFile = Path.Combine(rawDirectory, "positions.f32le.vec3.bin");
            string normalsFile = Path.Combine(rawDirectory, "normals.f32le.vec3.bin");
            string tangentsFile = Path.Combine(rawDirectory, "tangents.f32le.vec4.bin");
            string uv0File = Path.Combine(rawDirectory, "uv0.f32le.vec2.bin");
            string boneWeightsFile = Path.Combine(rawDirectory, "bone-weights.i32f32.x4.bin");
            string bindposesFile = Path.Combine(rawDirectory, "bindposes.f32le.mat4x4.bin");
            string indicesFile = Path.Combine(rawDirectory, "submesh3.indices.u32le.bin");
            byte[] positionsBytes = WritePositionBytes(baseline.Positions);
            byte[] normalsBytes = WriteVector3Bytes(normals);
            byte[] tangentsBytes = WriteVector4Bytes(tangents);
            byte[] uv0Bytes = WriteVector2Bytes(uv0);
            byte[] boneWeightsBytes = WriteBoneWeightBytes(boneWeights);
            byte[] bindposesBytes = WriteMatrix4x4Bytes(bindposes);
            byte[] indicesBytes = WriteIndexBytes(submeshTriangles);
            File.WriteAllBytes(positionsFile, positionsBytes);
            File.WriteAllBytes(normalsFile, normalsBytes);
            File.WriteAllBytes(tangentsFile, tangentsBytes);
            File.WriteAllBytes(uv0File, uv0Bytes);
            File.WriteAllBytes(boneWeightsFile, boneWeightsBytes);
            File.WriteAllBytes(bindposesFile, bindposesBytes);
            File.WriteAllBytes(indicesFile, indicesBytes);

            var capture = new FullGeometryCaptureReceipt
            {
                marker = "TAINTED_ARMOUR_REAL_SAMPLE_FULL_GEOMETRY_CAPTURE_READY_DOWNSTREAM_FALSE",
                schemaVersion = "tainted-armour.full-geometry-capture/1",
                unityVersion = report.unityVersion,
                sourceAssetPath = report.sourceAssetPath,
                sourceFingerprintBefore = report.sourceFingerprintBefore,
                sourceFingerprintAfter = report.sourceFingerprintBefore,
                sourceFbxMutationExecuted = false,
                selectedRenderer = report.selectedRenderer,
                sourceMeshName = report.sourceMeshName,
                sourceVertexCount = report.sourceVertexCount,
                sourceSubmeshIndex = report.sourceSubmeshIndex,
                sourceSubmeshTriangleCount = report.sourceSubmeshTriangleCount,
                baselineGeometryFingerprint = baseline.GeometryFingerprint,
                positionsBlobFile = "raw/positions.f32le.vec3.bin",
                positionsComponentType = "F32",
                positionsElementShape = "VEC3",
                positionsCount = baseline.Positions.Count,
                positionsByteLength = positionsBytes.Length,
                positionsSha256 = "sha256:" + HashBytes(positionsBytes),
                normalsBlobFile = "raw/normals.f32le.vec3.bin",
                normalsComponentType = "F32",
                normalsElementShape = "VEC3",
                normalsCount = normals.Length,
                normalsByteLength = normalsBytes.Length,
                normalsSha256 = "sha256:" + HashBytes(normalsBytes),
                tangentsBlobFile = "raw/tangents.f32le.vec4.bin",
                tangentsComponentType = "F32",
                tangentsElementShape = "VEC4",
                tangentsCount = tangents.Length,
                tangentsByteLength = tangentsBytes.Length,
                tangentsSha256 = "sha256:" + HashBytes(tangentsBytes),
                uv0BlobFile = "raw/uv0.f32le.vec2.bin",
                uv0ComponentType = "F32",
                uv0ElementShape = "VEC2",
                uv0Count = uv0.Length,
                uv0ByteLength = uv0Bytes.Length,
                uv0Sha256 = "sha256:" + HashBytes(uv0Bytes),
                boneWeightsBlobFile = "raw/bone-weights.i32f32.x4.bin",
                boneWeightsComponentType = "I32/F32",
                boneWeightsElementShape = "JOINTS4_WEIGHTS4",
                boneWeightsCount = boneWeights.Length,
                boneWeightsByteLength = boneWeightsBytes.Length,
                boneWeightsSha256 = "sha256:" + HashBytes(boneWeightsBytes),
                bindposesBlobFile = "raw/bindposes.f32le.mat4x4.bin",
                bindposesComponentType = "F32",
                bindposesElementShape = "MAT4",
                bindposesCount = bindposes.Length,
                bindposesByteLength = bindposesBytes.Length,
                bindposesSha256 = "sha256:" + HashBytes(bindposesBytes),
                indicesBlobFile = "raw/submesh3.indices.u32le.bin",
                indicesComponentType = "U32",
                indicesElementShape = "SCALAR",
                indicesCount = checked(submeshTriangles.Length * 3),
                indicesByteLength = indicesBytes.Length,
                indicesSha256 = "sha256:" + HashBytes(indicesBytes),
                boneCount = bones.Length,
                boneNames = bones.Select(bone => bone != null ? bone.name : string.Empty).ToArray(),
                bonePaths = bones.Select(bone => RelativeTransformPath(instanceRoot, bone)).ToArray(),
                materialNames = (renderer.sharedMaterials ?? Array.Empty<Material>())
                    .Select(material => material != null ? material.name : string.Empty)
                    .ToArray(),
                blendshapeCount = sourceMesh.blendShapeCount,
                blendshapeNames = Enumerable.Range(0, sourceMesh.blendShapeCount)
                    .Select(sourceMesh.GetBlendShapeName)
                    .ToArray(),
                candidateMapApplicationExecuted = false,
                conversionExecuted = false,
                downstreamWritesExecuted = false,
            };
            File.WriteAllText(
                Path.Combine(root, "capture.geometry.json"),
                JsonUtility.ToJson(capture, true),
                new UTF8Encoding(false));
            report.fullGeometryCaptureWritten = true;
            report.fullGeometryCaptureMarker = capture.marker;
            report.fullGeometryPositionsSha256 = capture.positionsSha256;
            report.fullGeometryIndicesSha256 = capture.indicesSha256;
        }

        private static byte[] WritePositionBytes(IReadOnlyList<ArmorVector3> positions)
        {
            if (positions == null || positions.Count == 0)
            {
                throw new ArgumentException("Full-geometry capture requires one or more positions.", nameof(positions));
            }

            byte[] bytes = new byte[checked(positions.Count * 3 * sizeof(float))];
            int offset = 0;
            foreach (ArmorVector3 position in positions)
            {
                WriteSingle(bytes, ref offset, (float)position.X);
                WriteSingle(bytes, ref offset, (float)position.Y);
                WriteSingle(bytes, ref offset, (float)position.Z);
            }

            return bytes;
        }

        private static byte[] WriteVector3Bytes(IReadOnlyList<Vector3> values)
        {
            if (values == null || values.Count == 0)
            {
                throw new ArgumentException("Full-geometry capture requires one or more Vector3 values.", nameof(values));
            }

            byte[] bytes = new byte[checked(values.Count * 3 * sizeof(float))];
            int offset = 0;
            foreach (Vector3 value in values)
            {
                WriteSingle(bytes, ref offset, value.x);
                WriteSingle(bytes, ref offset, value.y);
                WriteSingle(bytes, ref offset, value.z);
            }

            return bytes;
        }

        private static byte[] WriteVector4Bytes(IReadOnlyList<Vector4> values)
        {
            if (values == null || values.Count == 0)
            {
                throw new ArgumentException("Full-geometry capture requires one or more Vector4 values.", nameof(values));
            }

            byte[] bytes = new byte[checked(values.Count * 4 * sizeof(float))];
            int offset = 0;
            foreach (Vector4 value in values)
            {
                WriteSingle(bytes, ref offset, value.x);
                WriteSingle(bytes, ref offset, value.y);
                WriteSingle(bytes, ref offset, value.z);
                WriteSingle(bytes, ref offset, value.w);
            }

            return bytes;
        }

        private static byte[] WriteVector2Bytes(IReadOnlyList<Vector2> values)
        {
            if (values == null || values.Count == 0)
            {
                throw new ArgumentException("Full-geometry capture requires one or more Vector2 values.", nameof(values));
            }

            byte[] bytes = new byte[checked(values.Count * 2 * sizeof(float))];
            int offset = 0;
            foreach (Vector2 value in values)
            {
                WriteSingle(bytes, ref offset, value.x);
                WriteSingle(bytes, ref offset, value.y);
            }

            return bytes;
        }

        private static byte[] WriteBoneWeightBytes(IReadOnlyList<BoneWeight> weights)
        {
            if (weights == null || weights.Count == 0)
            {
                throw new ArgumentException("Full-geometry capture requires one or more bone-weight records.", nameof(weights));
            }

            byte[] bytes = new byte[checked(weights.Count * ((4 * sizeof(int)) + (4 * sizeof(float))))];
            int offset = 0;
            foreach (BoneWeight weight in weights)
            {
                WriteInt32(bytes, ref offset, weight.boneIndex0);
                WriteInt32(bytes, ref offset, weight.boneIndex1);
                WriteInt32(bytes, ref offset, weight.boneIndex2);
                WriteInt32(bytes, ref offset, weight.boneIndex3);
                WriteSingle(bytes, ref offset, weight.weight0);
                WriteSingle(bytes, ref offset, weight.weight1);
                WriteSingle(bytes, ref offset, weight.weight2);
                WriteSingle(bytes, ref offset, weight.weight3);
            }

            return bytes;
        }

        private static byte[] WriteMatrix4x4Bytes(IReadOnlyList<Matrix4x4> matrices)
        {
            byte[] bytes = new byte[checked(matrices.Count * 16 * sizeof(float))];
            int offset = 0;
            foreach (Matrix4x4 matrix in matrices)
            {
                WriteSingle(bytes, ref offset, matrix.m00);
                WriteSingle(bytes, ref offset, matrix.m01);
                WriteSingle(bytes, ref offset, matrix.m02);
                WriteSingle(bytes, ref offset, matrix.m03);
                WriteSingle(bytes, ref offset, matrix.m10);
                WriteSingle(bytes, ref offset, matrix.m11);
                WriteSingle(bytes, ref offset, matrix.m12);
                WriteSingle(bytes, ref offset, matrix.m13);
                WriteSingle(bytes, ref offset, matrix.m20);
                WriteSingle(bytes, ref offset, matrix.m21);
                WriteSingle(bytes, ref offset, matrix.m22);
                WriteSingle(bytes, ref offset, matrix.m23);
                WriteSingle(bytes, ref offset, matrix.m30);
                WriteSingle(bytes, ref offset, matrix.m31);
                WriteSingle(bytes, ref offset, matrix.m32);
                WriteSingle(bytes, ref offset, matrix.m33);
            }

            return bytes;
        }

        private static byte[] WriteIndexBytes(IReadOnlyList<ArmorTriangle> triangles)
        {
            if (triangles == null || triangles.Count == 0)
            {
                throw new ArgumentException("Full-geometry capture requires one or more triangles.", nameof(triangles));
            }

            byte[] bytes = new byte[checked(triangles.Count * 3 * sizeof(uint))];
            int offset = 0;
            foreach (ArmorTriangle triangle in triangles)
            {
                WriteUInt32(bytes, ref offset, checked((uint)triangle.Vertex0));
                WriteUInt32(bytes, ref offset, checked((uint)triangle.Vertex1));
                WriteUInt32(bytes, ref offset, checked((uint)triangle.Vertex2));
            }

            return bytes;
        }

        private static void WriteSingle(byte[] bytes, ref int offset, float value)
        {
            byte[] scalar = BitConverter.GetBytes(value);
            Buffer.BlockCopy(scalar, 0, bytes, offset, scalar.Length);
            offset += scalar.Length;
        }

        private static void WriteInt32(byte[] bytes, ref int offset, int value)
        {
            byte[] scalar = BitConverter.GetBytes(value);
            Buffer.BlockCopy(scalar, 0, bytes, offset, scalar.Length);
            offset += scalar.Length;
        }

        private static void WriteUInt32(byte[] bytes, ref int offset, uint value)
        {
            byte[] scalar = BitConverter.GetBytes(value);
            Buffer.BlockCopy(scalar, 0, bytes, offset, scalar.Length);
            offset += scalar.Length;
        }

        private static string HashBytes(byte[] bytes)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                return ToHex(sha256.ComputeHash(bytes));
            }
        }

        private static string GetArgument(string name, string defaultValue, bool required = true)
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int index = 0; index < args.Length - 1; index++)
            {
                if (string.Equals(args[index], name, StringComparison.OrdinalIgnoreCase)) return args[index + 1];
            }
            if (required && string.IsNullOrWhiteSpace(defaultValue)) throw new ArgumentException("Missing " + name + ".");
            return defaultValue;
        }

        private static void WriteReport(string path, RealSampleReport report)
        {
            string fullPath = Path.GetFullPath(path);
            string directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory)) Directory.CreateDirectory(directory);
            File.WriteAllText(fullPath, JsonUtility.ToJson(report, true));
        }

        private static string ProjectRoot
        {
            get { return Directory.GetParent(Application.dataPath).FullName; }
        }

        private static string RelativeTransformPath(Transform root, Transform transform)
        {
            if (root == null || transform == null || transform == root) return string.Empty;
            var parts = new Stack<string>();
            Transform current = transform;
            while (current != null && current != root)
            {
                parts.Push(current.name);
                current = current.parent;
            }
            return string.Join("/", parts.ToArray());
        }

        private sealed class RequiredRow
        {
            public readonly string targetGender;
            public readonly string sourceBoneRaw;
            public readonly string policyFamily;
            public readonly int expectedWeightedVertexCount;
            public readonly int expectedWeightedTriangleCount;
            public readonly int expectedOrientationReversedTriangleCount;

            public RequiredRow(
                string targetGender,
                string sourceBoneRaw,
                string policyFamily,
                int expectedWeightedVertexCount,
                int expectedWeightedTriangleCount,
                int expectedOrientationReversedTriangleCount)
            {
                this.targetGender = targetGender;
                this.sourceBoneRaw = sourceBoneRaw;
                this.policyFamily = policyFamily;
                this.expectedWeightedVertexCount = expectedWeightedVertexCount;
                this.expectedWeightedTriangleCount = expectedWeightedTriangleCount;
                this.expectedOrientationReversedTriangleCount = expectedOrientationReversedTriangleCount;
            }
        }

        private sealed class WeightedGeometry
        {
            public int[] vertexIndices;
            public ArmorTriangle[] triangles;
        }

        private sealed class LoadedClip
        {
            public AnimationClip clip;
            public string bundlePath;
        }

        private sealed class AvatarBuildResult
        {
            public Avatar avatar;
            public int humanBoneCount;
            public string[] missingHumanBoneNames;
        }

        private struct TransformState
        {
            public Transform transform;
            public Vector3 localPosition;
            public Quaternion localRotation;
            public Vector3 localScale;

            public static TransformState[] Capture(GameObject root)
            {
                return root.GetComponentsInChildren<Transform>(true)
                    .Select(transform => new TransformState
                    {
                        transform = transform,
                        localPosition = transform.localPosition,
                        localRotation = transform.localRotation,
                        localScale = transform.localScale,
                    })
                    .ToArray();
            }

            public static void Restore(IEnumerable<TransformState> states)
            {
                foreach (TransformState state in states ?? Array.Empty<TransformState>())
                {
                    if (state.transform == null) continue;
                    state.transform.localPosition = state.localPosition;
                    state.transform.localRotation = state.localRotation;
                    state.transform.localScale = state.localScale;
                }
            }

            public static int CountChanged(TransformState[] before, TransformState[] after)
            {
                int count = 0;
                int length = Mathf.Min(before != null ? before.Length : 0, after != null ? after.Length : 0);
                for (int index = 0; index < length; index++)
                {
                    if (before[index].transform == null || after[index].transform == null) continue;
                    if (Vector3.Distance(before[index].localPosition, after[index].localPosition) > DisplacementEpsilon
                        || Quaternion.Angle(before[index].localRotation, after[index].localRotation) > 0.001f
                        || Vector3.Distance(before[index].localScale, after[index].localScale) > DisplacementEpsilon)
                    {
                        count++;
                    }
                }
                return count;
            }
        }

        [Serializable]
        private sealed class T26Record
        {
            public string marker;
            public PoseDriverRow[] poseDriverRows;
        }

        [Serializable]
        private sealed class PoseDriverRow
        {
            public string pose;
            public ClipRecord[] clipRecords;
        }

        [Serializable]
        private sealed class ClipRecord
        {
            public string clip;
            public string bundle;
            public string bundlePath;
        }

        [Serializable]
        private sealed class RealSampleReport
        {
            public string marker;
            public string generatedAtUtc;
            public string unityVersion;
            public string projectRoot;
            public bool validationTransportStagingUsed;
            public string sourceAssetPath;
            public string sourceFingerprintBefore;
            public string sourceFingerprintAfter;
            public bool sourceFbxMutationExecuted;
            public string selectedRenderer;
            public string sourceMeshName;
            public int sourceVertexCount;
            public int sourceSubmeshIndex;
            public int sourceSubmeshTriangleCount;
            public int sourceBoneCount;
            public int sourceBoneWeightCount;
            public string sourceAvatarName;
            public int sourceAvatarHumanBoneCount;
            public string[] sourceAvatarMissingHumanBoneNames;
            public string targetPose;
            public string targetClip;
            public string clipBundlePath;
            public float clipLength;
            public float sampleTime;
            public int changedTransformCount;
            public string baselineGeometryFingerprint;
            public string posedGeometryFingerprint;
            public string fullGeometryCaptureRoot;
            public bool fullGeometryCaptureWritten;
            public string fullGeometryCaptureMarker;
            public string fullGeometryPositionsSha256;
            public string fullGeometryIndicesSha256;
            public string coreAssemblySha256;
            public string adapterAssemblySha256;
            public int expectedTotalOrientationReversed;
            public int expectedTotalCollapsed;
            public RowReceipt[] rows;
            public Summary summary;
            public DownstreamBoundary downstream;
            public string[] blockers;
            public string[] errors;
        }

        [Serializable]
        private sealed class RowReceipt
        {
            public string requestId;
            public string targetGender;
            public string sourceBoneRaw;
            public string policyFamily;
            public int weightedVertexCount;
            public int expectedWeightedVertexCount;
            public int weightedTriangleCount;
            public int expectedWeightedTriangleCount;
            public bool importInvoked;
            public string importStatus;
            public bool deformationAccepted;
            public string calibrationStageVersion;
            public bool controlsPassed;
            public int controlFailureCount;
            public bool outOfPlaneRigidRotationPassed;
            public bool outOfPlaneRigidRotationDiagnosticFailed;
            public bool metricCalibrationFailed;
            public bool candidateSupplementalMetricCompared;
            public string candidateMetricVersion;
            public int candidateMetricComparisonCount;
            public int candidateMetricExpectedMatchCount;
            public int candidateMetricDisagreementCount;
            public bool candidateMetricAffectsAcceptance;
            public int orientationReversedTriangleCount;
            public int expectedOrientationReversedTriangleCount;
            public int[] orientationReversedTriangleOrdinals;
            public int collapsedTriangleCount;
            public int indeterminateTriangleCount;
            public int movedVertexCount;
            public double maxVertexDisplacement;
            public double meanVertexDisplacement;
            public bool candidateMapApplicationAllowed;
            public bool candidateMapApplicationExecuted;
            public bool conversionAllowed;
            public bool conversionExecuted;
            public bool sidecarsGenerated;
            public bool sourceFbxMutationExecuted;
            public bool unityProjectAssetMutationExecuted;
            public string visualRuntimeStageVersion;
            public string visualRuntimeRouteId;
            public bool visualRuntimeRequested;
            public bool liveRuntimeMetricsObserved;
            public bool visualRuntimeAccepted;
            public bool visualRuntimeObserverMissing;
            public int visualRuntimeBlockedRowCount;
            public bool runtimeLoaderChanged;
            public bool runtimeRegistrationExecuted;
            public bool itemRegistrationExecuted;
            public bool inventoryEquipSaveMutationExecuted;
            public bool saveWriteExecuted;
            public bool nativeGameWriteExecuted;
            public bool releaseReady;
            public string[] blockers;
            public string[] warnings;
        }

        [Serializable]
        private sealed class FullGeometryCaptureReceipt
        {
            public string marker;
            public string schemaVersion;
            public string unityVersion;
            public string sourceAssetPath;
            public string sourceFingerprintBefore;
            public string sourceFingerprintAfter;
            public bool sourceFbxMutationExecuted;
            public string selectedRenderer;
            public string sourceMeshName;
            public int sourceVertexCount;
            public int sourceSubmeshIndex;
            public int sourceSubmeshTriangleCount;
            public string baselineGeometryFingerprint;
            public string positionsBlobFile;
            public string positionsComponentType;
            public string positionsElementShape;
            public int positionsCount;
            public int positionsByteLength;
            public string positionsSha256;
            public string normalsBlobFile;
            public string normalsComponentType;
            public string normalsElementShape;
            public int normalsCount;
            public int normalsByteLength;
            public string normalsSha256;
            public string tangentsBlobFile;
            public string tangentsComponentType;
            public string tangentsElementShape;
            public int tangentsCount;
            public int tangentsByteLength;
            public string tangentsSha256;
            public string uv0BlobFile;
            public string uv0ComponentType;
            public string uv0ElementShape;
            public int uv0Count;
            public int uv0ByteLength;
            public string uv0Sha256;
            public string boneWeightsBlobFile;
            public string boneWeightsComponentType;
            public string boneWeightsElementShape;
            public int boneWeightsCount;
            public int boneWeightsByteLength;
            public string boneWeightsSha256;
            public string bindposesBlobFile;
            public string bindposesComponentType;
            public string bindposesElementShape;
            public int bindposesCount;
            public int bindposesByteLength;
            public string bindposesSha256;
            public string indicesBlobFile;
            public string indicesComponentType;
            public string indicesElementShape;
            public int indicesCount;
            public int indicesByteLength;
            public string indicesSha256;
            public int boneCount;
            public string[] boneNames;
            public string[] bonePaths;
            public string[] materialNames;
            public int blendshapeCount;
            public string[] blendshapeNames;
            public bool candidateMapApplicationExecuted;
            public bool conversionExecuted;
            public bool downstreamWritesExecuted;
        }

        [Serializable]
        private sealed class Summary
        {
            public int importInvocationCount;
            public int totalOrientationReversedTriangles;
            public int totalCollapsedTriangles;
            public int totalIndeterminateTriangles;
            public bool rowParityWithT56;
            public bool productionImporterFlowVerified;
            public bool metricCalibrationBoundaryVerified;
            public bool candidateSupplementalMetricBoundaryVerified;
            public bool visualRuntimeObserverBoundaryVerified;
            public int candidateMetricDisagreementCount;
            public bool candidateMapAndDownstreamResultsFalse;
            public bool everyDownstreamWriteFalse;
            public bool sourceFingerprintStable;
        }

        [Serializable]
        private sealed class DownstreamBoundary
        {
            public bool candidateMapApplicationExecuted;
            public bool conversionExecuted;
            public bool payloadGenerationExecuted;
            public bool sidecarGenerationExecuted;
            public bool runtimeLoaderChangeExecuted;
            public bool runtimeRegistrationExecuted;
            public bool itemRegistrationExecuted;
            public bool equipExecuted;
            public bool inventoryWriteExecuted;
            public bool saveWriteExecuted;
            public bool nativeGameFileWriteExecuted;
            public bool assetBundleBuildExecuted;
            public bool addressablesBuildExecuted;
            public bool releaseReady;

            public bool AllFalse
            {
                get
                {
                    return !candidateMapApplicationExecuted
                        && !conversionExecuted
                        && !payloadGenerationExecuted
                        && !sidecarGenerationExecuted
                        && !runtimeLoaderChangeExecuted
                        && !runtimeRegistrationExecuted
                        && !itemRegistrationExecuted
                        && !equipExecuted
                        && !inventoryWriteExecuted
                        && !saveWriteExecuted
                        && !nativeGameFileWriteExecuted
                        && !assetBundleBuildExecuted
                        && !addressablesBuildExecuted
                        && !releaseReady;
                }
            }
        }
    }
}
