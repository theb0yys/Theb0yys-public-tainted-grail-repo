using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public static class DragonKnightArmorA2KT47NoWriteSkinWeightCandidateSearch
{
    private const string ZeroCandidateMarker = "DRAGON_KNIGHT_ARMOR_A2K_T47_NO_WRITE_SKIN_WEIGHT_CANDIDATE_SEARCH_ZERO_INVERSION_CANDIDATE_READY";
    private const string RouteFailureMarker = "DRAGON_KNIGHT_ARMOR_A2K_T47_NO_WRITE_SKIN_WEIGHT_CANDIDATE_SEARCH_ROUTE_FAILURE_NO_ZERO_CANDIDATE";
    private const string BlockedMarker = "DRAGON_KNIGHT_ARMOR_A2K_T47_NO_WRITE_SKIN_WEIGHT_CANDIDATE_SEARCH_BLOCKED";
    private const string ExpectedStagedFbxSha256 = "AFF181589CD022405F5D536230B6C656681396CB337E208F80F5698A5A12B9DB";
    private const string ImportedFbxPath = "Assets/DragonKnightArmorA2KT20/SK_Dragon_knight_UE5_no_Cape.fbx";
    private const string TargetPose = "fall";
    private const string TargetClip = "Anim_Hero_TPP_Base_Knockdown_Air_Loop";
    private const int SourceSubmeshIndex = 3;
    private const int ExpectedExactRecordCount = 474;
    private const int ExpectedExactRowCount = 6;
    private const float DisplacementEpsilon = 0.000001f;
    private const float TriangleAreaEpsilon = 0.00000001f;

    public static void Search()
    {
        string reportPath = GetArgument("-dragonKnightArmorT47Report", string.Empty);
        var report = new CandidateSearchReport
        {
            marker = BlockedMarker,
            generatedAt = "2026-08-08",
            unityVersion = Application.unityVersion,
            projectRoot = ProjectRoot,
            sourceAssetPath = ImportedFbxPath,
            sourceBasis = "tainted_grail_foa_native_only",
            targetPose = TargetPose,
            targetClip = TargetClip,
            expectedStagedFbxSha256 = ExpectedStagedFbxSha256,
            sourceEditsAllowed = false,
            sourceEditsExecuted = false,
            sourceFbxMutationExecuted = false,
            sourceWeightMutationExecuted = false,
            candidateMapApplicationAllowed = false,
            candidateMapApplicationExecuted = false,
            conversionAllowed = false,
            conversionExecuted = false,
            sidecarGenerationExecuted = false,
            runtimeLoaderChangeExecuted = false,
            itemRegistrationExecuted = false,
            equipInventorySaveWriteExecuted = false,
            nativeGameFileWriteExecuted = false,
            assetBundleBuildExecuted = false,
            addressablesBuildExecuted = false,
            unityProjectAssetMutationDuringSampling = false,
        };

        var blockers = new List<string>();
        var required = new List<RequiredFieldResult>();

        try
        {
            string t26Path = GetArgument("-dragonKnightArmorT26Input", string.Empty);
            string t31Path = GetArgument("-dragonKnightArmorT31Input", string.Empty);
            string t46Path = GetArgument("-dragonKnightArmorT46Input", string.Empty);
            string bundleRoot = GetArgument("-dragonKnightFoaBundleRoot", string.Empty, false);

            if (string.IsNullOrWhiteSpace(reportPath)) throw new ArgumentException("Missing -dragonKnightArmorT47Report.");
            if (string.IsNullOrWhiteSpace(t26Path)) throw new ArgumentException("Missing -dragonKnightArmorT26Input.");
            if (string.IsNullOrWhiteSpace(t31Path)) throw new ArgumentException("Missing -dragonKnightArmorT31Input.");
            if (string.IsNullOrWhiteSpace(t46Path)) throw new ArgumentException("Missing -dragonKnightArmorT46Input.");

            T26Record t26 = ReadJson<T26Record>(t26Path);
            T31Record t31 = ReadJson<T31Record>(t31Path);
            T46Record t46 = ReadJson<T46Record>(t46Path);

            report.t26Marker = t26.marker;
            report.t31Marker = t31.marker;
            report.t46Marker = t46.marker;
            report.t31TotalExactTriangleRecords = t31.summary != null ? t31.summary.totalExactTriangleRecords : 0;
            report.t31TotalInvertedTriangles = t31.summary != null ? t31.summary.totalInvertedTriangles : 0;
            report.t31TotalCollapsedTriangles = t31.summary != null ? t31.summary.totalCollapsedTriangles : 0;
            report.t31ExactTriangleRowsCaptured = t31.summary != null ? t31.summary.exactTriangleRowsCaptured : 0;
            report.t31ExactTriangleCaptureMatchesT29Aggregate = t31.summary != null && t31.summary.exactTriangleCaptureMatchesT29Aggregate;

            string stagedFbxFullPath = Path.Combine(ProjectRoot, ImportedFbxPath.Replace("/", Path.DirectorySeparatorChar.ToString()));
            string stagedHash = File.Exists(stagedFbxFullPath) ? Sha256File(stagedFbxFullPath) : string.Empty;
            report.stagedFbxFullPath = stagedFbxFullPath;
            report.stagedFbxSha256 = stagedHash;
            report.stagedFbxHashMatchesExpected = string.Equals(stagedHash, ExpectedStagedFbxSha256, StringComparison.OrdinalIgnoreCase);

            GameObject sourceAsset = AssetDatabase.LoadAssetAtPath<GameObject>(ImportedFbxPath);
            PoseDriverRow fallPose = (t26.poseDriverRows ?? Array.Empty<PoseDriverRow>())
                .FirstOrDefault(row => row != null && string.Equals(row.pose, TargetPose, StringComparison.Ordinal));

            AddRequired(required, "t46_route_ready", t46.marker == "DRAGON_KNIGHT_ARMOR_A2K_T46_POST_RETOPOLOGY_EXACT_INVERSION_NEW_REMEDIATION_ROUTE_READY", "T46 marker=" + t46.marker + ".");
            AddRequired(required, "t46_no_write_harness_allowed", t46.summary != null && t46.summary.noWriteHarnessExecutionAllowedAfterThisPacket && !t46.summary.sourceWeightMutationAllowed && !t46.summary.candidateMapApplicationAllowed && !t46.summary.conversionAllowed, "T46 selected route=" + (t46.summary != null ? t46.summary.selectedNewRoute : string.Empty) + ".");
            AddRequired(required, "staged_fbx_hash_matches_aff181", report.stagedFbxHashMatchesExpected, "Staged FBX SHA-256=" + stagedHash + ".");
            AddRequired(required, "t26_ready", t26.marker == "DRAGON_KNIGHT_ARMOR_A2K_T26_TAINTED_GRAIL_NATIVE_POSE_DRIVER_SOURCE_PACKET_READY", "T26 marker=" + t26.marker + ".");
            AddRequired(required, "t31_post_retopology_exact_records_ready", report.t31ExactTriangleRowsCaptured == ExpectedExactRowCount && report.t31TotalExactTriangleRecords == ExpectedExactRecordCount && report.t31TotalInvertedTriangles == ExpectedExactRecordCount && report.t31ExactTriangleCaptureMatchesT29Aggregate, "T31 rows=" + report.t31ExactTriangleRowsCaptured.ToString(CultureInfo.InvariantCulture) + " exactRecords=" + report.t31TotalExactTriangleRecords.ToString(CultureInfo.InvariantCulture) + " totalInverted=" + report.t31TotalInvertedTriangles.ToString(CultureInfo.InvariantCulture) + ".");
            AddRequired(required, "source_asset_already_imported", sourceAsset != null, "Imported source asset path=" + ImportedFbxPath + ".");
            AddRequired(required, "bundle_root_available", !string.IsNullOrWhiteSpace(bundleRoot) && Directory.Exists(bundleRoot), "FoA bundle root=" + bundleRoot + ".");
            AddRequired(required, "fall_air_loop_driver_available", fallPose != null && (fallPose.matchedDriverNames ?? Array.Empty<string>()).Contains(TargetClip), "T26 fall matched drivers=" + string.Join("; ", fallPose != null ? fallPose.matchedDriverNames ?? Array.Empty<string>() : Array.Empty<string>()) + ".");

            if (!report.stagedFbxHashMatchesExpected) blockers.Add("staged_fbx_hash_mismatch");
            if (sourceAsset == null) blockers.Add("source_asset_not_already_imported_from_t20");
            if (string.IsNullOrWhiteSpace(bundleRoot) || !Directory.Exists(bundleRoot)) blockers.Add("foa_bundle_root_missing");
            if (fallPose == null || !(fallPose.matchedDriverNames ?? Array.Empty<string>()).Contains(TargetClip)) blockers.Add("fall_air_loop_driver_missing");
            if (report.t31ExactTriangleRowsCaptured != ExpectedExactRowCount || report.t31TotalExactTriangleRecords != ExpectedExactRecordCount || report.t31TotalInvertedTriangles != ExpectedExactRecordCount || !report.t31ExactTriangleCaptureMatchesT29Aggregate) blockers.Add("post_retopology_t31_exact_record_input_not_ready");
            if (t46.summary == null || !t46.summary.noWriteHarnessExecutionAllowedAfterThisPacket || t46.summary.sourceWeightMutationAllowed || t46.summary.candidateMapApplicationAllowed || t46.summary.conversionAllowed) blockers.Add("t46_no_write_harness_authorization_missing_or_overbroad");

            if (blockers.Count == 0)
            {
                ExecuteSearch(sourceAsset, fallPose, t31, bundleRoot, report, required, blockers);
            }

            report.requiredFieldResults = required.ToArray();
            report.blockedReasons = blockers.Distinct(StringComparer.Ordinal).ToArray();
            report.notRun = BuildNotRun(report).ToArray();
            CompleteMarker(report);
            WriteReport(reportPath, report);
            Debug.Log(report.marker + ": " + reportPath);
            EditorApplication.Exit(report.marker == BlockedMarker ? 1 : 0);
        }
        catch (Exception ex)
        {
            blockers.Add("candidate_search_exception");
            report.errors = new[] { ex.ToString() };
            report.requiredFieldResults = required.ToArray();
            report.blockedReasons = blockers.Distinct(StringComparer.Ordinal).ToArray();
            report.notRun = BuildNotRun(report).ToArray();
            CompleteMarker(report);
            Debug.LogError(report.marker + ": " + ex);
            WriteReport(reportPath, report);
            EditorApplication.Exit(1);
        }
    }

    private static void ExecuteSearch(GameObject sourceAsset, PoseDriverRow fallPose, T31Record t31, string bundleRoot, CandidateSearchReport report, List<RequiredFieldResult> required, List<string> blockers)
    {
        GameObject instance = UnityEngine.Object.Instantiate(sourceAsset);
        instance.name = "DragonKnightArmorA2KT47NoWriteSkinWeightCandidateSearch";
        instance.hideFlags = HideFlags.HideAndDontSave;

        AssetBundle retainedBundle = null;
        Mesh clonedMesh = null;
        Mesh originalMesh = null;
        SkinnedMeshRenderer renderer = null;

        try
        {
            renderer = SelectCombinedRenderer(instance.GetComponentsInChildren<SkinnedMeshRenderer>(true));
            if (renderer == null || renderer.sharedMesh == null)
            {
                blockers.Add("combined_source_renderer_missing");
                AddRequired(required, "combined_source_renderer", false, "No source SkinnedMeshRenderer with submesh 3 was found.");
                return;
            }

            originalMesh = renderer.sharedMesh;
            clonedMesh = UnityEngine.Object.Instantiate(originalMesh);
            clonedMesh.name = "DragonKnightArmorA2KT47InMemoryCandidateMesh";
            clonedMesh.hideFlags = HideFlags.HideAndDontSave;
            renderer.sharedMesh = clonedMesh;

            Transform[] bones = renderer.bones ?? Array.Empty<Transform>();
            BoneWeight[] originalWeights = clonedMesh.boneWeights ?? Array.Empty<BoneWeight>();
            int[] sourceTriangles = clonedMesh.GetTriangles(SourceSubmeshIndex);
            var sourcePaths = new HashSet<string>(instance.GetComponentsInChildren<Transform>(true).Select(transform => RelativeTransformPath(instance.transform, transform)), StringComparer.Ordinal);

            AvatarBuildResult avatarResult = ResolveSourceAvatar(instance);
            Animator animator = instance.GetComponentInChildren<Animator>();
            if (animator == null) animator = instance.AddComponent<Animator>();
            Avatar sourceAvatar = avatarResult.avatar;
            if (sourceAvatar != null) animator.avatar = sourceAvatar;
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

            report.selectedRenderer = renderer.name;
            report.sourceVertexCount = clonedMesh.vertexCount;
            report.sourceSubmeshIndex = SourceSubmeshIndex;
            report.sourceSubmeshTriangleCount = sourceTriangles.Length / 3;
            report.sourceBoneCount = bones.Length;
            report.sourceBoneWeightCount = originalWeights.Length;
            report.importedMaxInfluenceCount = MaxInfluenceCount(originalWeights);
            report.sourceAvatarName = sourceAvatar != null ? sourceAvatar.name : string.Empty;
            report.sourceAvatarBuildMode = avatarResult.mode;
            report.sourceAvatarIsValid = sourceAvatar != null && sourceAvatar.isValid;
            report.sourceAvatarIsHuman = sourceAvatar != null && sourceAvatar.isHuman;
            report.sourceAvatarHumanBoneCount = avatarResult.humanBoneCount;
            report.sourceAvatarMissingHumanBoneNames = avatarResult.missingHumanBoneNames;
            report.animatorPresent = animator != null;
            report.inMemoryMeshCloneCreated = clonedMesh != null;
            report.inMemoryMeshCloneAssignedOnlyToTransientInstance = true;

            AddRequired(required, "combined_source_renderer", clonedMesh.vertexCount > 0 && clonedMesh.subMeshCount > SourceSubmeshIndex, "Renderer=" + renderer.name + " vertices=" + clonedMesh.vertexCount.ToString(CultureInfo.InvariantCulture) + " submeshes=" + clonedMesh.subMeshCount.ToString(CultureInfo.InvariantCulture) + ".");
            AddRequired(required, "source_avatar_available", sourceAvatar != null && sourceAvatar.isValid && sourceAvatar.isHuman, "Mode=" + avatarResult.mode + " Avatar=" + (sourceAvatar != null ? sourceAvatar.name : string.Empty) + " valid=" + (sourceAvatar != null && sourceAvatar.isValid).ToString(CultureInfo.InvariantCulture) + " human=" + (sourceAvatar != null && sourceAvatar.isHuman).ToString(CultureInfo.InvariantCulture) + " mappedBones=" + avatarResult.humanBoneCount.ToString(CultureInfo.InvariantCulture) + ".");

            LoadedClip loadedClip = LoadTargetClip(fallPose, bundleRoot, out retainedBundle, blockers);
            AddRequired(required, "target_clip_loaded", loadedClip != null && loadedClip.clip != null, "Target clip=" + TargetClip + " bundlePath=" + (loadedClip != null ? loadedClip.bundlePath : string.Empty) + ".");
            if (loadedClip == null || loadedClip.clip == null) return;

            ExactRowData[] exactRows = BuildExactRows(t31, bones, sourceTriangles, clonedMesh.vertexCount, blockers);
            int exactRecordCount = exactRows.Sum(row => row.triangles.Length);
            bool rowCountOk = exactRows.Length == ExpectedExactRowCount;
            bool recordCountOk = exactRecordCount == ExpectedExactRecordCount;
            bool tripletsOk = exactRows.All(row => row.triangleTripletsMatchCurrentMesh);

            AddRequired(required, "t31_exact_rows_loaded", rowCountOk && recordCountOk, "Rows=" + exactRows.Length.ToString(CultureInfo.InvariantCulture) + " exactRecords=" + exactRecordCount.ToString(CultureInfo.InvariantCulture) + ".");
            AddRequired(required, "t31_exact_triplets_match_staged_mesh", tripletsOk, "Rows with staged triplet match=" + exactRows.Count(row => row.triangleTripletsMatchCurrentMesh).ToString(CultureInfo.InvariantCulture) + "/" + exactRows.Length.ToString(CultureInfo.InvariantCulture) + ".");
            AddRequired(required, "in_memory_weight_mutation_only", clonedMesh != null && renderer.sharedMesh == clonedMesh, "The harness mutates BoneWeight arrays only on a transient mesh clone.");

            if (!rowCountOk || !recordCountOk) blockers.Add("t31_exact_rows_or_records_not_loaded");
            if (!tripletsOk) blockers.Add("t31_exact_triplets_do_not_match_staged_mesh");
            if (blockers.Count > 0) return;

            var affectedVertices = new HashSet<int>(exactRows.SelectMany(row => row.affectedVertices));
            var oneRingVertices = ComputeOneRingVertices(sourceTriangles, affectedVertices);
            var expandedOneRing = new HashSet<int>(affectedVertices);
            expandedOneRing.UnionWith(oneRingVertices);

            report.exactRows = exactRows.Select(row => row.ToReport()).ToArray();
            report.affectedVertexCount = affectedVertices.Count;
            report.oneRingNeighborVertexCount = oneRingVertices.Count;
            report.affectedPlusOneRingVertexCount = expandedOneRing.Count;
            report.exactTriangleRecordsLoaded = exactRecordCount;
            report.uniqueExactSourceTriangleOrdinalCount = exactRows.SelectMany(row => row.triangles).Select(triangle => triangle.sourceTriangleOrdinal).Distinct().Count();
            report.uniqueExactSourceTriangleOrdinalRanges = BuildRanges(exactRows.SelectMany(row => row.triangles).Select(triangle => triangle.sourceTriangleOrdinal).Distinct().OrderBy(value => value).ToArray());

            CandidateDefinition[] candidates = BuildCandidates(exactRows, bones, originalWeights, affectedVertices, oneRingVertices).ToArray();
            report.candidateCountPlanned = candidates.Length;

            var results = new List<CandidateResult>();
            float sampleTime = loadedClip.clip.length <= 0f ? 0f : Mathf.Min(loadedClip.clip.length * 0.5f, Mathf.Max(0f, loadedClip.clip.length - 0.001f));
            TransformState[] baselinePose = TransformState.Capture(instance);

            foreach (CandidateDefinition candidate in candidates)
            {
                BoneWeight[] workingWeights = new BoneWeight[originalWeights.Length];
                Array.Copy(originalWeights, workingWeights, originalWeights.Length);

                int changedRows = ApplyCandidate(candidate, workingWeights, originalWeights);
                clonedMesh.boneWeights = workingWeights;

                SampleEvaluation evaluation = EvaluateCandidate(instance, renderer, animator, baselinePose, exactRows, loadedClip.clip, sampleTime, sourcePaths);
                CandidateMetric metric = evaluation.metric;
                CandidateResult result = new CandidateResult
                {
                    candidateId = candidate.candidateId,
                    description = candidate.description,
                    scope = candidate.scope,
                    overrideVertexCount = candidate.overrides.Count,
                    changedWeightRowCount = changedRows,
                    conflictingVertexAssignmentCount = candidate.conflictingVertexAssignments,
                    candidateMaxInfluenceCount = MaxInfluenceCount(workingWeights),
                    sampleTime = sampleTime,
                    clipLength = loadedClip.clip.length,
                    bundlePath = loadedClip.bundlePath,
                    evaluationMode = evaluation.evaluationMode,
                    curveBindingCount = evaluation.binding.curveBindingCount,
                    matchingBindingPaths = evaluation.binding.matchingBindingPaths,
                    changedTransformCount = evaluation.binding.changedTransformCount,
                    clipBindingApplied = evaluation.binding.changedTransformCount > 0 || metric.maxVertexDisplacement > DisplacementEpsilon,
                    exactTriangleRowsEvaluated = metric.rows.Length,
                    exactTriangleRecordsEvaluated = metric.totalExactTriangleRecords,
                    totalExactInvertedTriangles = metric.totalExactInvertedTriangles,
                    totalExactCollapsedTriangles = metric.totalExactCollapsedTriangles,
                    maxVertexDisplacement = metric.maxVertexDisplacement,
                    meanVertexDisplacement = metric.meanVertexDisplacement,
                    movedAffectedVertexCount = metric.movedAffectedVertexCount,
                    rowMetrics = metric.rows,
                    remainingInvertedUniqueOrdinalCount = metric.remainingInvertedUniqueOrdinals.Length,
                    remainingInvertedUniqueOrdinalRanges = BuildRanges(metric.remainingInvertedUniqueOrdinals),
                    remainingInvertedUniqueOrdinalsFirst32 = metric.remainingInvertedUniqueOrdinals.Take(32).ToArray(),
                    zeroInversionCandidate = metric.totalExactInvertedTriangles == 0 && metric.totalExactCollapsedTriangles == 0 && metric.rows.All(row => row.invertedTriangleCount == 0 && row.collapsedTriangleCount == 0),
                };
                results.Add(result);
            }

            clonedMesh.boneWeights = originalWeights;
            TransformState.Restore(baselinePose);

            CandidateResult best = results
                .OrderBy(result => result.totalExactInvertedTriangles)
                .ThenBy(result => result.totalExactCollapsedTriangles)
                .ThenBy(result => result.changedWeightRowCount)
                .ThenBy(result => result.candidateId, StringComparer.Ordinal)
                .FirstOrDefault();
            CandidateResult zero = results.FirstOrDefault(result => result.zeroInversionCandidate);

            report.candidateResults = results.ToArray();
            report.summary = new CandidateSearchSummary
            {
                preconditionsReady = blockers.Count == 0,
                searchExecuted = results.Count > 0,
                stagedFbxHashMatchesExpected = report.stagedFbxHashMatchesExpected,
                exactTriangleRecordInputCount = exactRecordCount,
                exactTriangleRowsLoaded = exactRows.Length,
                affectedVertexCount = affectedVertices.Count,
                oneRingNeighborVertexCount = oneRingVertices.Count,
                candidateCountPlanned = candidates.Length,
                candidatesEvaluated = results.Count,
                zeroInversionCandidateFound = zero != null,
                zeroInversionCandidateId = zero != null ? zero.candidateId : string.Empty,
                bestCandidateId = best != null ? best.candidateId : string.Empty,
                bestExactInvertedTriangles = best != null ? best.totalExactInvertedTriangles : 0,
                bestExactCollapsedTriangles = best != null ? best.totalExactCollapsedTriangles : 0,
                baselineOriginalExactInvertedTriangles = results.Count > 0 ? results[0].totalExactInvertedTriangles : 0,
                baselineOriginalExactCollapsedTriangles = results.Count > 0 ? results[0].totalExactCollapsedTriangles : 0,
                sameT31FallAirLoopEvaluationPathExecuted = true,
                fullT29MatrixEvaluationExecuted = false,
                routeFailureReceiptEmitted = zero == null,
                sourceEditsExecuted = false,
                sourceWeightMutationExecuted = false,
                sourceFbxMutationExecuted = false,
                candidateMapApplicationExecuted = false,
                conversionExecuted = false,
                sidecarGenerationExecuted = false,
                runtimeLoaderChangeExecuted = false,
                itemRegistrationExecuted = false,
                equipInventorySaveWriteExecuted = false,
                nativeGameFileWriteExecuted = false,
                assetBundleBuildExecuted = false,
                addressablesBuildExecuted = false,
            };

            report.bestCandidate = best;
            report.zeroInversionCandidate = zero;
        }
        finally
        {
            if (renderer != null && originalMesh != null) renderer.sharedMesh = originalMesh;
            if (clonedMesh != null) UnityEngine.Object.DestroyImmediate(clonedMesh);
            if (retainedBundle != null) retainedBundle.Unload(false);
            UnityEngine.Object.DestroyImmediate(instance);
        }
    }

    private static SampleEvaluation EvaluateCandidate(
        GameObject instance,
        SkinnedMeshRenderer renderer,
        Animator animator,
        TransformState[] baselinePose,
        ExactRowData[] exactRows,
        AnimationClip clip,
        float sampleTime,
        HashSet<string> sourcePaths)
    {
        TransformState.Restore(baselinePose);
        Vector3[] directBaseline = BakeVertices(renderer);
        TransformState[] directBefore = TransformState.Capture(instance);
        clip.SampleAnimation(instance, sampleTime);
        TransformState[] directAfter = TransformState.Capture(instance);
        Vector3[] directPosed = BakeVertices(renderer);
        CandidateMetric directMetric = MeasureExactRows(exactRows, directBaseline, directPosed);
        BindingReport directBinding = InspectBindings(clip, sourcePaths, directBefore, directAfter);
        if (directBinding.changedTransformCount > 0 || directMetric.maxVertexDisplacement > DisplacementEpsilon || animator == null || animator.avatar == null)
        {
            return new SampleEvaluation
            {
                evaluationMode = "AnimationClip.SampleAnimation",
                metric = directMetric,
                binding = directBinding,
            };
        }

        TransformState.Restore(baselinePose);
        animator.Rebind();
        animator.Update(0f);
        TransformState.Restore(baselinePose);
        Vector3[] playableBaseline = BakeVertices(renderer);
        TransformState[] playableBefore = TransformState.Capture(instance);
        EvaluatePlayable(animator, clip, sampleTime);
        TransformState[] playableAfter = TransformState.Capture(instance);
        Vector3[] playablePosed = BakeVertices(renderer);

        return new SampleEvaluation
        {
            evaluationMode = "AnimationClipPlayable",
            metric = MeasureExactRows(exactRows, playableBaseline, playablePosed),
            binding = InspectBindings(clip, sourcePaths, playableBefore, playableAfter),
        };
    }

    private static void EvaluatePlayable(Animator animator, AnimationClip clip, float sampleTime)
    {
        PlayableGraph graph = PlayableGraph.Create("DragonKnightArmorA2KT47FallAirLoopSample");
        try
        {
            graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
            AnimationClipPlayable playable = AnimationClipPlayable.Create(graph, clip);
            playable.SetApplyFootIK(true);
            playable.SetApplyPlayableIK(false);
            AnimationPlayableOutput output = AnimationPlayableOutput.Create(graph, "DragonKnightArmorA2KT47Output", animator);
            output.SetSourcePlayable(playable);
            graph.Play();
            graph.Evaluate(Mathf.Max(0.00001f, sampleTime));
        }
        finally
        {
            if (graph.IsValid()) graph.Destroy();
        }
    }

    private static Vector3[] BakeVertices(SkinnedMeshRenderer renderer)
    {
        Mesh baked = new Mesh();
        try
        {
            renderer.BakeMesh(baked, true);
            return baked.vertices;
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(baked);
        }
    }

    private static CandidateMetric MeasureExactRows(ExactRowData[] exactRows, Vector3[] baselineVertices, Vector3[] posedVertices)
    {
        var rowMetrics = new List<RowCandidateMetric>();
        var allAffected = new HashSet<int>();
        float displacementSum = 0f;
        int displacementCount = 0;
        float maxDisplacement = 0f;

        if (baselineVertices == null || posedVertices == null || baselineVertices.Length != posedVertices.Length)
        {
            return new CandidateMetric
            {
                rows = Array.Empty<RowCandidateMetric>(),
                remainingInvertedUniqueOrdinals = Array.Empty<int>(),
            };
        }

        foreach (ExactRowData row in exactRows ?? Array.Empty<ExactRowData>())
        {
            foreach (int vertexIndex in row.affectedVertices) allAffected.Add(vertexIndex);
            RowCandidateMetric metric = MeasureExactRow(row, baselineVertices, posedVertices);
            rowMetrics.Add(metric);
        }

        foreach (int vertexIndex in allAffected)
        {
            if (vertexIndex < 0 || vertexIndex >= baselineVertices.Length) continue;
            float distance = Vector3.Distance(baselineVertices[vertexIndex], posedVertices[vertexIndex]);
            displacementSum += distance;
            displacementCount++;
            if (distance > maxDisplacement) maxDisplacement = distance;
        }

        int[] remainingOrdinals = rowMetrics
            .SelectMany(row => row.remainingInvertedSourceTriangleOrdinals ?? Array.Empty<int>())
            .Distinct()
            .OrderBy(value => value)
            .ToArray();

        return new CandidateMetric
        {
            rows = rowMetrics.ToArray(),
            totalExactTriangleRecords = rowMetrics.Sum(row => row.exactTriangleRecordsEvaluated),
            totalExactInvertedTriangles = rowMetrics.Sum(row => row.invertedTriangleCount),
            totalExactCollapsedTriangles = rowMetrics.Sum(row => row.collapsedTriangleCount),
            maxVertexDisplacement = maxDisplacement,
            meanVertexDisplacement = displacementCount == 0 ? 0f : displacementSum / displacementCount,
            movedAffectedVertexCount = allAffected.Count(vertexIndex => vertexIndex >= 0 && vertexIndex < baselineVertices.Length && Vector3.Distance(baselineVertices[vertexIndex], posedVertices[vertexIndex]) > DisplacementEpsilon),
            remainingInvertedUniqueOrdinals = remainingOrdinals,
        };
    }

    private static RowCandidateMetric MeasureExactRow(ExactRowData row, Vector3[] baselineVertices, Vector3[] posedVertices)
    {
        int collapsed = 0;
        int inverted = 0;
        float rowMaxDisplacement = 0f;
        float rowSumDisplacement = 0f;
        int rowMoveCount = 0;
        var remainingOrdinals = new List<int>();

        foreach (int vertexIndex in row.affectedVertices)
        {
            if (vertexIndex < 0 || vertexIndex >= baselineVertices.Length) continue;
            float distance = Vector3.Distance(baselineVertices[vertexIndex], posedVertices[vertexIndex]);
            rowSumDisplacement += distance;
            if (distance > rowMaxDisplacement) rowMaxDisplacement = distance;
            if (distance > DisplacementEpsilon) rowMoveCount++;
        }

        foreach (ExactTriangleData triangle in row.triangles)
        {
            if (!TriangleIndicesValid(triangle, baselineVertices.Length)) continue;
            Vector3 ba = baselineVertices[triangle.vertexIndex0];
            Vector3 bb = baselineVertices[triangle.vertexIndex1];
            Vector3 bc = baselineVertices[triangle.vertexIndex2];
            Vector3 pa = posedVertices[triangle.vertexIndex0];
            Vector3 pb = posedVertices[triangle.vertexIndex1];
            Vector3 pc = posedVertices[triangle.vertexIndex2];
            Vector3 baselineNormal = Vector3.Cross(bb - ba, bc - ba);
            Vector3 posedNormal = Vector3.Cross(pb - pa, pc - pa);
            float posedArea = posedNormal.magnitude * 0.5f;
            if (posedArea <= TriangleAreaEpsilon) collapsed++;
            if (baselineNormal.sqrMagnitude > 0f && posedNormal.sqrMagnitude > 0f)
            {
                float normalizedDot = Vector3.Dot(baselineNormal.normalized, posedNormal.normalized);
                if (normalizedDot < 0f)
                {
                    inverted++;
                    remainingOrdinals.Add(triangle.sourceTriangleOrdinal);
                }
            }
        }

        int[] uniqueOrdinals = remainingOrdinals.Distinct().OrderBy(value => value).ToArray();
        return new RowCandidateMetric
        {
            targetGender = row.targetGender,
            targetSlot = row.targetSlot,
            nativeEquipmentType = row.nativeEquipmentType,
            sourceBoneRaw = row.sourceBoneRaw,
            policyFamily = row.policyFamily,
            exactTriangleRecordsEvaluated = row.triangles.Length,
            affectedVertexCount = row.affectedVertices.Length,
            invertedTriangleCount = inverted,
            collapsedTriangleCount = collapsed,
            remainingInvertedUniqueOrdinalCount = uniqueOrdinals.Length,
            remainingInvertedSourceTriangleOrdinalRanges = BuildRanges(uniqueOrdinals),
            remainingInvertedSourceTriangleOrdinals = uniqueOrdinals.Take(32).ToArray(),
            maxVertexDisplacement = rowMaxDisplacement,
            meanVertexDisplacement = row.affectedVertices.Length == 0 ? 0f : rowSumDisplacement / row.affectedVertices.Length,
            movedAffectedVertexCount = rowMoveCount,
            rowZeroInversion = inverted == 0 && collapsed == 0,
        };
    }

    private static bool TriangleIndicesValid(ExactTriangleData triangle, int vertexCount)
    {
        return triangle.vertexIndex0 >= 0 && triangle.vertexIndex0 < vertexCount
            && triangle.vertexIndex1 >= 0 && triangle.vertexIndex1 < vertexCount
            && triangle.vertexIndex2 >= 0 && triangle.vertexIndex2 < vertexCount;
    }

    private static ExactRowData[] BuildExactRows(T31Record t31, Transform[] bones, int[] sourceTriangles, int vertexCount, List<string> blockers)
    {
        var rows = new List<ExactRowData>();
        foreach (T31RowLocalization row in t31.rowLocalizations ?? Array.Empty<T31RowLocalization>())
        {
            var exactTriangles = new List<ExactTriangleData>();
            var affectedVertices = new HashSet<int>();
            bool tripletsMatch = true;
            bool verticesInRange = true;

            foreach (T31InvertedTriangle triangle in row.invertedTriangles ?? Array.Empty<T31InvertedTriangle>())
            {
                bool triangleValid = TriangleRecordInRange(triangle, sourceTriangles, vertexCount);
                if (!triangleValid)
                {
                    tripletsMatch = false;
                    verticesInRange = false;
                }
                else
                {
                    int triangleBase = triangle.sourceTriangleOrdinal * 3;
                    bool orderedTripletMatches = sourceTriangles[triangleBase] == triangle.vertexIndex0
                        && sourceTriangles[triangleBase + 1] == triangle.vertexIndex1
                        && sourceTriangles[triangleBase + 2] == triangle.vertexIndex2;
                    if (!orderedTripletMatches) tripletsMatch = false;
                }

                exactTriangles.Add(new ExactTriangleData
                {
                    sourceTriangleOrdinal = triangle.sourceTriangleOrdinal,
                    vertexIndex0 = triangle.vertexIndex0,
                    vertexIndex1 = triangle.vertexIndex1,
                    vertexIndex2 = triangle.vertexIndex2,
                });
                affectedVertices.Add(triangle.vertexIndex0);
                affectedVertices.Add(triangle.vertexIndex1);
                affectedVertices.Add(triangle.vertexIndex2);
            }

            int sourceBoneIndex = FindBoneIndex(bones, row.sourceBoneRaw);
            int parentBoneIndex = sourceBoneIndex >= 0 && bones[sourceBoneIndex] != null && bones[sourceBoneIndex].parent != null
                ? FindBoneIndexByTransform(bones, bones[sourceBoneIndex].parent)
                : -1;

            if (sourceBoneIndex < 0) blockers.Add("source_bone_not_found_" + row.sourceBoneRaw);
            if (!verticesInRange) blockers.Add("t31_exact_vertices_out_of_range_" + row.targetGender + "_" + row.sourceBoneRaw);
            if (!tripletsMatch) blockers.Add("t31_exact_triplets_mismatch_" + row.targetGender + "_" + row.sourceBoneRaw);

            rows.Add(new ExactRowData
            {
                targetGender = row.targetGender,
                targetSlot = row.targetSlot,
                nativeEquipmentType = row.nativeEquipmentType,
                sourceBoneRaw = row.sourceBoneRaw,
                policyFamily = row.policyFamily,
                materialName = row.materialName,
                materialSlotIndex = row.materialSlotIndex,
                sourceBoneIndex = sourceBoneIndex,
                parentBoneIndex = parentBoneIndex,
                parentBoneName = parentBoneIndex >= 0 && bones[parentBoneIndex] != null ? bones[parentBoneIndex].name : string.Empty,
                triangles = exactTriangles.ToArray(),
                affectedVertices = affectedVertices.OrderBy(value => value).ToArray(),
                triangleTripletsMatchCurrentMesh = tripletsMatch,
                verticesInsideCurrentMeshRange = verticesInRange,
            });
        }
        return rows.ToArray();
    }

    private static bool TriangleRecordInRange(T31InvertedTriangle triangle, int[] sourceTriangles, int vertexCount)
    {
        if (triangle.sourceTriangleOrdinal < 0) return false;
        int triangleBase = triangle.sourceTriangleOrdinal * 3;
        if (triangleBase < 0 || triangleBase + 2 >= sourceTriangles.Length) return false;
        return triangle.vertexIndex0 >= 0 && triangle.vertexIndex0 < vertexCount
            && triangle.vertexIndex1 >= 0 && triangle.vertexIndex1 < vertexCount
            && triangle.vertexIndex2 >= 0 && triangle.vertexIndex2 < vertexCount;
    }

    private static HashSet<int> ComputeOneRingVertices(int[] sourceTriangles, HashSet<int> affectedVertices)
    {
        var oneRing = new HashSet<int>();
        for (int index = 0; index + 2 < sourceTriangles.Length; index += 3)
        {
            int a = sourceTriangles[index];
            int b = sourceTriangles[index + 1];
            int c = sourceTriangles[index + 2];
            if (!affectedVertices.Contains(a) && !affectedVertices.Contains(b) && !affectedVertices.Contains(c)) continue;
            if (!affectedVertices.Contains(a)) oneRing.Add(a);
            if (!affectedVertices.Contains(b)) oneRing.Add(b);
            if (!affectedVertices.Contains(c)) oneRing.Add(c);
        }
        return oneRing;
    }

    private static IEnumerable<CandidateDefinition> BuildCandidates(ExactRowData[] rows, Transform[] bones, BoneWeight[] originalWeights, HashSet<int> affectedVertices, HashSet<int> oneRingVertices)
    {
        var candidates = new List<CandidateDefinition>();
        var affected = affectedVertices.OrderBy(value => value).ToArray();
        var expanded = affectedVertices.Concat(oneRingVertices).Distinct().OrderBy(value => value).ToArray();

        AddCandidate(candidates, "baseline_original_weights", "No candidate override; records current post-retopology exact inversion baseline.", "none", new Dictionary<int, BoneWeight>(), 0);
        AddDominantCandidate(candidates, "affected_dominant_existing_single", "Affected vertices assigned to their imported dominant influence only.", "affected_t31_exact_vertices", affected, originalWeights, bones, false);
        AddDominantCandidate(candidates, "affected_parent_of_dominant_single", "Affected vertices assigned to the parent of their imported dominant influence where available.", "affected_t31_exact_vertices", affected, originalWeights, bones, true);

        string[] strategicBones = { "pelvis", "spine_01", "spine_02", "spine_03", "spine_04", "spine_05", "neck_01", "neck_02" };
        foreach (string boneName in strategicBones)
        {
            AddSingleBoneCandidate(candidates, "affected_" + boneName + "_single", "Affected vertices assigned to " + boneName + " only.", "affected_t31_exact_vertices", affected, FindBoneIndex(bones, boneName));
        }

        AddRowSingleCandidate(candidates, "row_source_bone_single", "Each row's exact vertices assigned to that row's source bone only.", "row_affected_t31_exact_vertices", rows, row => row.sourceBoneIndex);
        AddRowSingleCandidate(candidates, "row_parent_bone_single", "Each row's exact vertices assigned to that row source bone's parent only.", "row_affected_t31_exact_vertices", rows, row => row.parentBoneIndex);
        AddRowBlendCandidate(candidates, "row_source_parent_even_blend", "Each row's exact vertices assigned to 0.5 source bone and 0.5 parent bone.", "row_affected_t31_exact_vertices", rows);

        AddDominantCandidate(candidates, "affected_plus_onering_dominant_existing_single", "Affected plus one-ring vertices assigned to their imported dominant influence only.", "affected_t31_exact_vertices_plus_one_ring_neighbors", expanded, originalWeights, bones, false);
        AddDominantCandidate(candidates, "affected_plus_onering_parent_of_dominant_single", "Affected plus one-ring vertices assigned to the parent of their imported dominant influence where available.", "affected_t31_exact_vertices_plus_one_ring_neighbors", expanded, originalWeights, bones, true);
        foreach (string boneName in new[] { "spine_03", "spine_04", "spine_05", "neck_01", "neck_02" })
        {
            AddSingleBoneCandidate(candidates, "affected_plus_onering_" + boneName + "_single", "Affected plus one-ring vertices assigned to " + boneName + " only.", "affected_t31_exact_vertices_plus_one_ring_neighbors", expanded, FindBoneIndex(bones, boneName));
        }

        return candidates;
    }

    private static void AddCandidate(List<CandidateDefinition> candidates, string id, string description, string scope, Dictionary<int, BoneWeight> overrides, int conflicts)
    {
        if (candidates.Any(candidate => string.Equals(candidate.candidateId, id, StringComparison.Ordinal))) return;
        candidates.Add(new CandidateDefinition
        {
            candidateId = id,
            description = description,
            scope = scope,
            overrides = overrides,
            conflictingVertexAssignments = conflicts,
        });
    }

    private static void AddSingleBoneCandidate(List<CandidateDefinition> candidates, string id, string description, string scope, IEnumerable<int> vertices, int boneIndex)
    {
        if (boneIndex < 0) return;
        var overrides = new Dictionary<int, BoneWeight>();
        int conflicts = 0;
        foreach (int vertexIndex in vertices) SetOverride(overrides, vertexIndex, SingleWeight(boneIndex), ref conflicts);
        AddCandidate(candidates, id, description, scope, overrides, conflicts);
    }

    private static void AddDominantCandidate(List<CandidateDefinition> candidates, string id, string description, string scope, IEnumerable<int> vertices, BoneWeight[] originalWeights, Transform[] bones, bool parent)
    {
        var overrides = new Dictionary<int, BoneWeight>();
        int conflicts = 0;
        foreach (int vertexIndex in vertices)
        {
            if (vertexIndex < 0 || vertexIndex >= originalWeights.Length) continue;
            int boneIndex = DominantBoneIndex(originalWeights[vertexIndex]);
            if (parent) boneIndex = ParentBoneIndex(bones, boneIndex);
            if (boneIndex < 0) continue;
            SetOverride(overrides, vertexIndex, SingleWeight(boneIndex), ref conflicts);
        }
        AddCandidate(candidates, id, description, scope, overrides, conflicts);
    }

    private static void AddRowSingleCandidate(List<CandidateDefinition> candidates, string id, string description, string scope, ExactRowData[] rows, Func<ExactRowData, int> boneSelector)
    {
        var overrides = new Dictionary<int, BoneWeight>();
        int conflicts = 0;
        foreach (ExactRowData row in rows)
        {
            int boneIndex = boneSelector(row);
            if (boneIndex < 0) continue;
            foreach (int vertexIndex in row.affectedVertices)
            {
                SetOverride(overrides, vertexIndex, SingleWeight(boneIndex), ref conflicts);
            }
        }
        AddCandidate(candidates, id, description, scope, overrides, conflicts);
    }

    private static void AddRowBlendCandidate(List<CandidateDefinition> candidates, string id, string description, string scope, ExactRowData[] rows)
    {
        var overrides = new Dictionary<int, BoneWeight>();
        int conflicts = 0;
        foreach (ExactRowData row in rows)
        {
            if (row.sourceBoneIndex < 0 || row.parentBoneIndex < 0) continue;
            BoneWeight blend = BlendWeight(row.sourceBoneIndex, row.parentBoneIndex, 0.5f, 0.5f);
            foreach (int vertexIndex in row.affectedVertices)
            {
                SetOverride(overrides, vertexIndex, blend, ref conflicts);
            }
        }
        AddCandidate(candidates, id, description, scope, overrides, conflicts);
    }

    private static void SetOverride(Dictionary<int, BoneWeight> overrides, int vertexIndex, BoneWeight weight, ref int conflicts)
    {
        BoneWeight existing;
        if (overrides.TryGetValue(vertexIndex, out existing) && !SameBoneWeight(existing, weight)) conflicts++;
        overrides[vertexIndex] = weight;
    }

    private static int ApplyCandidate(CandidateDefinition candidate, BoneWeight[] workingWeights, BoneWeight[] originalWeights)
    {
        int changed = 0;
        foreach (KeyValuePair<int, BoneWeight> entry in candidate.overrides)
        {
            if (entry.Key < 0 || entry.Key >= workingWeights.Length) continue;
            if (!SameBoneWeight(originalWeights[entry.Key], entry.Value)) changed++;
            workingWeights[entry.Key] = entry.Value;
        }
        return changed;
    }

    private static BoneWeight SingleWeight(int boneIndex)
    {
        return new BoneWeight
        {
            boneIndex0 = boneIndex,
            weight0 = 1f,
            boneIndex1 = 0,
            weight1 = 0f,
            boneIndex2 = 0,
            weight2 = 0f,
            boneIndex3 = 0,
            weight3 = 0f,
        };
    }

    private static BoneWeight BlendWeight(int boneIndex0, int boneIndex1, float weight0, float weight1)
    {
        float total = Mathf.Max(0.000001f, weight0 + weight1);
        return new BoneWeight
        {
            boneIndex0 = boneIndex0,
            weight0 = weight0 / total,
            boneIndex1 = boneIndex1,
            weight1 = weight1 / total,
            boneIndex2 = 0,
            weight2 = 0f,
            boneIndex3 = 0,
            weight3 = 0f,
        };
    }

    private static bool SameBoneWeight(BoneWeight left, BoneWeight right)
    {
        return left.boneIndex0 == right.boneIndex0
            && left.boneIndex1 == right.boneIndex1
            && left.boneIndex2 == right.boneIndex2
            && left.boneIndex3 == right.boneIndex3
            && Mathf.Abs(left.weight0 - right.weight0) <= 0.000001f
            && Mathf.Abs(left.weight1 - right.weight1) <= 0.000001f
            && Mathf.Abs(left.weight2 - right.weight2) <= 0.000001f
            && Mathf.Abs(left.weight3 - right.weight3) <= 0.000001f;
    }

    private static int DominantBoneIndex(BoneWeight weight)
    {
        int boneIndex = weight.boneIndex0;
        float best = weight.weight0;
        if (weight.weight1 > best) { best = weight.weight1; boneIndex = weight.boneIndex1; }
        if (weight.weight2 > best) { best = weight.weight2; boneIndex = weight.boneIndex2; }
        if (weight.weight3 > best) { boneIndex = weight.boneIndex3; }
        return boneIndex;
    }

    private static int ParentBoneIndex(Transform[] bones, int boneIndex)
    {
        if (boneIndex < 0 || boneIndex >= bones.Length || bones[boneIndex] == null || bones[boneIndex].parent == null) return -1;
        return FindBoneIndexByTransform(bones, bones[boneIndex].parent);
    }

    private static int FindBoneIndex(Transform[] bones, string boneName)
    {
        return Array.FindIndex(bones ?? Array.Empty<Transform>(), bone => bone != null && string.Equals(bone.name, boneName, StringComparison.Ordinal));
    }

    private static int FindBoneIndexByTransform(Transform[] bones, Transform transform)
    {
        if (transform == null) return -1;
        return Array.FindIndex(bones ?? Array.Empty<Transform>(), bone => bone == transform);
    }

    private static int MaxInfluenceCount(BoneWeight[] weights)
    {
        int max = 0;
        foreach (BoneWeight weight in weights ?? Array.Empty<BoneWeight>())
        {
            int count = 0;
            if (weight.weight0 > 0f) count++;
            if (weight.weight1 > 0f) count++;
            if (weight.weight2 > 0f) count++;
            if (weight.weight3 > 0f) count++;
            if (count > max) max = count;
        }
        return max;
    }

    private static BindingReport InspectBindings(AnimationClip clip, HashSet<string> sourcePaths, TransformState[] before, TransformState[] after)
    {
        var report = new BindingReport();
        EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(clip);
        report.curveBindingCount = bindings.Length;
        report.matchingBindingPaths = bindings
            .Select(binding => binding.path ?? string.Empty)
            .Distinct(StringComparer.Ordinal)
            .Count(path => sourcePaths.Contains(path));
        report.changedTransformCount = TransformState.CountChanged(before, after);
        return report;
    }

    private static LoadedClip LoadTargetClip(PoseDriverRow fallPose, string bundleRoot, out AssetBundle retainedBundle, List<string> blockers)
    {
        retainedBundle = null;
        ClipRecord record = (fallPose.clipRecords ?? Array.Empty<ClipRecord>())
            .FirstOrDefault(item => item != null && string.Equals(item.clip, TargetClip, StringComparison.Ordinal));
        if (record == null)
        {
            blockers.Add("target_clip_record_missing");
            return null;
        }

        string bundlePath = !string.IsNullOrWhiteSpace(record.bundlePath)
            ? record.bundlePath
            : Path.Combine(bundleRoot, record.bundle ?? string.Empty);
        if (string.IsNullOrWhiteSpace(bundlePath) || !File.Exists(bundlePath))
        {
            blockers.Add("target_clip_bundle_missing");
            return null;
        }

        retainedBundle = AssetBundle.LoadFromFile(bundlePath);
        if (retainedBundle == null)
        {
            blockers.Add("target_clip_bundle_load_failed");
            return null;
        }

        AnimationClip clip = retainedBundle.LoadAsset<AnimationClip>(record.clip);
        if (clip == null)
        {
            AnimationClip[] allClips = retainedBundle.LoadAllAssets<AnimationClip>();
            clip = allClips.FirstOrDefault(candidate => candidate != null && string.Equals(candidate.name, record.clip, StringComparison.Ordinal));
        }
        if (clip == null)
        {
            blockers.Add("target_clip_asset_missing");
            return null;
        }

        return new LoadedClip { clip = clip, bundlePath = bundlePath };
    }

    private static SkinnedMeshRenderer SelectCombinedRenderer(SkinnedMeshRenderer[] renderers)
    {
        return (renderers ?? Array.Empty<SkinnedMeshRenderer>())
            .Where(renderer => renderer != null && renderer.sharedMesh != null && renderer.sharedMesh.subMeshCount > SourceSubmeshIndex)
            .OrderByDescending(renderer => renderer.sharedMesh.vertexCount)
            .ThenBy(renderer => renderer.name, StringComparer.Ordinal)
            .FirstOrDefault();
    }

    private static Avatar LoadSourceAvatar()
    {
        return AssetDatabase.LoadAllAssetsAtPath(ImportedFbxPath)
            .OfType<Avatar>()
            .OrderByDescending(avatar => avatar != null && avatar.isHuman)
            .ThenBy(avatar => avatar != null ? avatar.name : string.Empty, StringComparer.Ordinal)
            .FirstOrDefault();
    }

    private static AvatarBuildResult ResolveSourceAvatar(GameObject instance)
    {
        Avatar imported = LoadSourceAvatar();
        if (imported != null)
        {
            return new AvatarBuildResult
            {
                avatar = imported,
                mode = "imported_fbx_avatar",
                humanBoneCount = 0,
                missingHumanBoneNames = Array.Empty<string>(),
            };
        }
        return BuildTransientHumanoidAvatar(instance);
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
        AddHumanBone(human, transforms, HumanBodyBones.LeftThumbProximal, "thumb_01_l", missing);
        AddHumanBone(human, transforms, HumanBodyBones.LeftThumbIntermediate, "thumb_02_l", missing);
        AddHumanBone(human, transforms, HumanBodyBones.LeftThumbDistal, "thumb_03_l", missing);
        AddHumanBone(human, transforms, HumanBodyBones.LeftIndexProximal, "index_01_l", missing);
        AddHumanBone(human, transforms, HumanBodyBones.LeftIndexIntermediate, "index_02_l", missing);
        AddHumanBone(human, transforms, HumanBodyBones.LeftIndexDistal, "index_03_l", missing);
        AddHumanBone(human, transforms, HumanBodyBones.LeftMiddleProximal, "middle_01_l", missing);
        AddHumanBone(human, transforms, HumanBodyBones.LeftMiddleIntermediate, "middle_02_l", missing);
        AddHumanBone(human, transforms, HumanBodyBones.LeftMiddleDistal, "middle_03_l", missing);
        AddHumanBone(human, transforms, HumanBodyBones.LeftRingProximal, "ring_01_l", missing);
        AddHumanBone(human, transforms, HumanBodyBones.LeftRingIntermediate, "ring_02_l", missing);
        AddHumanBone(human, transforms, HumanBodyBones.LeftRingDistal, "ring_03_l", missing);
        AddHumanBone(human, transforms, HumanBodyBones.LeftLittleProximal, "pinky_01_l", missing);
        AddHumanBone(human, transforms, HumanBodyBones.LeftLittleIntermediate, "pinky_02_l", missing);
        AddHumanBone(human, transforms, HumanBodyBones.LeftLittleDistal, "pinky_03_l", missing);
        AddHumanBone(human, transforms, HumanBodyBones.RightThumbProximal, "thumb_01_r", missing);
        AddHumanBone(human, transforms, HumanBodyBones.RightThumbIntermediate, "thumb_02_r", missing);
        AddHumanBone(human, transforms, HumanBodyBones.RightThumbDistal, "thumb_03_r", missing);
        AddHumanBone(human, transforms, HumanBodyBones.RightIndexProximal, "index_01_r", missing);
        AddHumanBone(human, transforms, HumanBodyBones.RightIndexIntermediate, "index_02_r", missing);
        AddHumanBone(human, transforms, HumanBodyBones.RightIndexDistal, "index_03_r", missing);
        AddHumanBone(human, transforms, HumanBodyBones.RightMiddleProximal, "middle_01_r", missing);
        AddHumanBone(human, transforms, HumanBodyBones.RightMiddleIntermediate, "middle_02_r", missing);
        AddHumanBone(human, transforms, HumanBodyBones.RightMiddleDistal, "middle_03_r", missing);
        AddHumanBone(human, transforms, HumanBodyBones.RightRingProximal, "ring_01_r", missing);
        AddHumanBone(human, transforms, HumanBodyBones.RightRingIntermediate, "ring_02_r", missing);
        AddHumanBone(human, transforms, HumanBodyBones.RightRingDistal, "ring_03_r", missing);
        AddHumanBone(human, transforms, HumanBodyBones.RightLittleProximal, "pinky_01_r", missing);
        AddHumanBone(human, transforms, HumanBodyBones.RightLittleIntermediate, "pinky_02_r", missing);
        AddHumanBone(human, transforms, HumanBodyBones.RightLittleDistal, "pinky_03_r", missing);

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
        if (avatar != null) avatar.name = "DragonKnightArmorA2KT47TransientHumanoidAvatar";
        return new AvatarBuildResult
        {
            avatar = avatar,
            mode = "transient_humanoid_avatar",
            humanBoneCount = human.Count,
            missingHumanBoneNames = missing.ToArray(),
        };
    }

    private static void AddHumanBone(List<HumanBone> human, Dictionary<string, Transform> transforms, HumanBodyBones humanBone, string sourceBoneName, List<string> missing)
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

    private static void CompleteMarker(CandidateSearchReport report)
    {
        if (report.summary != null && report.summary.searchExecuted && report.summary.zeroInversionCandidateFound)
        {
            report.marker = ZeroCandidateMarker;
            return;
        }
        if (report.summary != null && report.summary.searchExecuted && !report.summary.zeroInversionCandidateFound)
        {
            report.marker = RouteFailureMarker;
            return;
        }
        report.marker = BlockedMarker;
    }

    private static List<string> BuildNotRun(CandidateSearchReport report)
    {
        var notRun = new List<string>();
        if (report.summary == null || !report.summary.fullT29MatrixEvaluationExecuted) notRun.Add("full_t29_120_row_matrix_candidate_evaluation");
        notRun.Add("source_edits");
        notRun.Add("source_weight_mutation");
        notRun.Add("source_fbx_mutation");
        notRun.Add("unity_staged_fbx_mutation_after_t45");
        notRun.Add("candidate_map_application");
        notRun.Add("conversion");
        notRun.Add("sidecar_generation");
        notRun.Add("runtime_loader_change");
        notRun.Add("item_registration");
        notRun.Add("inventory_equip");
        notRun.Add("save_write");
        notRun.Add("native_game_file_write");
        notRun.Add("assetbundle_build");
        notRun.Add("addressables_build");
        notRun.Add("public_api_or_release_validation");
        return notRun;
    }

    private static void AddRequired(List<RequiredFieldResult> rows, string field, bool passed, string evidence)
    {
        rows.Add(new RequiredFieldResult { field = field, status = passed ? "pass" : "blocked", evidence = evidence });
    }

    private static T ReadJson<T>(string path)
    {
        if (!File.Exists(path)) throw new FileNotFoundException("JSON input not found.", path);
        return JsonUtility.FromJson<T>(File.ReadAllText(path));
    }

    private static void WriteReport(string path, CandidateSearchReport report)
    {
        string fullPath = Path.GetFullPath(path);
        string directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory)) Directory.CreateDirectory(directory);
        using (var stream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.Read))
        using (var writer = new StreamWriter(stream))
        {
            writer.Write(JsonUtility.ToJson(report, true));
        }
    }

    private static string Sha256File(string path)
    {
        using (var sha = SHA256.Create())
        using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
            return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", string.Empty);
        }
    }

    private static string[] BuildRanges(int[] values)
    {
        if (values == null || values.Length == 0) return Array.Empty<string>();
        var ranges = new List<string>();
        int start = values[0];
        int previous = values[0];
        for (int index = 1; index < values.Length; index++)
        {
            int current = values[index];
            if (current == previous + 1)
            {
                previous = current;
                continue;
            }
            ranges.Add(start == previous ? start.ToString(CultureInfo.InvariantCulture) : start.ToString(CultureInfo.InvariantCulture) + "-" + previous.ToString(CultureInfo.InvariantCulture));
            start = current;
            previous = current;
        }
        ranges.Add(start == previous ? start.ToString(CultureInfo.InvariantCulture) : start.ToString(CultureInfo.InvariantCulture) + "-" + previous.ToString(CultureInfo.InvariantCulture));
        return ranges.ToArray();
    }

    private static string GetArgument(string name, string defaultValue, bool required = true)
    {
        string[] args = Environment.GetCommandLineArgs();
        for (int index = 0; index < args.Length - 1; index++)
        {
            if (string.Equals(args[index], name, StringComparison.OrdinalIgnoreCase))
            {
                return args[index + 1];
            }
        }
        if (required && string.IsNullOrWhiteSpace(defaultValue)) throw new ArgumentException("Missing " + name + ".");
        return defaultValue;
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

    private sealed class CandidateDefinition
    {
        public string candidateId;
        public string description;
        public string scope;
        public Dictionary<int, BoneWeight> overrides;
        public int conflictingVertexAssignments;
    }

    private sealed class ExactRowData
    {
        public string targetGender;
        public string targetSlot;
        public string nativeEquipmentType;
        public string sourceBoneRaw;
        public string policyFamily;
        public string materialName;
        public int materialSlotIndex;
        public int sourceBoneIndex;
        public int parentBoneIndex;
        public string parentBoneName;
        public ExactTriangleData[] triangles;
        public int[] affectedVertices;
        public bool triangleTripletsMatchCurrentMesh;
        public bool verticesInsideCurrentMeshRange;

        public ExactRowReport ToReport()
        {
            int[] ordinals = triangles.Select(triangle => triangle.sourceTriangleOrdinal).Distinct().OrderBy(value => value).ToArray();
            return new ExactRowReport
            {
                targetGender = targetGender,
                targetSlot = targetSlot,
                nativeEquipmentType = nativeEquipmentType,
                sourceBoneRaw = sourceBoneRaw,
                policyFamily = policyFamily,
                materialName = materialName,
                materialSlotIndex = materialSlotIndex,
                sourceBoneIndex = sourceBoneIndex,
                parentBoneIndex = parentBoneIndex,
                parentBoneName = parentBoneName,
                exactTriangleRecordCount = triangles.Length,
                affectedVertexCount = affectedVertices.Length,
                uniqueSourceTriangleOrdinalCount = ordinals.Length,
                uniqueSourceTriangleOrdinalRanges = BuildRanges(ordinals),
                triangleTripletsMatchCurrentMesh = triangleTripletsMatchCurrentMesh,
                verticesInsideCurrentMeshRange = verticesInsideCurrentMeshRange,
            };
        }
    }

    private struct ExactTriangleData
    {
        public int sourceTriangleOrdinal;
        public int vertexIndex0;
        public int vertexIndex1;
        public int vertexIndex2;
    }

    private sealed class LoadedClip
    {
        public AnimationClip clip;
        public string bundlePath;
    }

    private sealed class AvatarBuildResult
    {
        public Avatar avatar;
        public string mode;
        public int humanBoneCount;
        public string[] missingHumanBoneNames;
    }

    private sealed class BindingReport
    {
        public int curveBindingCount;
        public int matchingBindingPaths;
        public int changedTransformCount;
    }

    private sealed class SampleEvaluation
    {
        public string evaluationMode;
        public CandidateMetric metric;
        public BindingReport binding;
    }

    private sealed class CandidateMetric
    {
        public RowCandidateMetric[] rows;
        public int totalExactTriangleRecords;
        public int totalExactInvertedTriangles;
        public int totalExactCollapsedTriangles;
        public float maxVertexDisplacement;
        public float meanVertexDisplacement;
        public int movedAffectedVertexCount;
        public int[] remainingInvertedUniqueOrdinals;
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

        public static void Restore(TransformState[] states)
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
            if (before == null || after == null) return 0;
            int count = 0;
            int length = Mathf.Min(before.Length, after.Length);
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
        public string[] matchedDriverNames;
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
    private sealed class T31Record
    {
        public string marker;
        public T31Summary summary;
        public T31RowLocalization[] rowLocalizations;
    }

    [Serializable]
    private sealed class T31Summary
    {
        public int exactTriangleRowsCaptured;
        public int totalInvertedTriangles;
        public int totalCollapsedTriangles;
        public int totalExactTriangleRecords;
        public bool exactTriangleCaptureMatchesT29Aggregate;
    }

    [Serializable]
    private sealed class T31RowLocalization
    {
        public string targetGender;
        public string targetSlot;
        public string nativeEquipmentType;
        public string sourceBoneRaw;
        public string policyFamily;
        public string materialName;
        public int materialSlotIndex;
        public T31InvertedTriangle[] invertedTriangles;
    }

    [Serializable]
    private sealed class T31InvertedTriangle
    {
        public int sourceTriangleOrdinal;
        public int vertexIndex0;
        public int vertexIndex1;
        public int vertexIndex2;
    }

    [Serializable]
    private sealed class T46Record
    {
        public string marker;
        public T46Summary summary;
    }

    [Serializable]
    private sealed class T46Summary
    {
        public string selectedNewRoute;
        public bool noWriteHarnessExecutionAllowedAfterThisPacket;
        public bool sourceWeightMutationAllowed;
        public bool candidateMapApplicationAllowed;
        public bool conversionAllowed;
    }

    [Serializable]
    private sealed class CandidateSearchReport
    {
        public string marker;
        public string generatedAt;
        public string unityVersion;
        public string projectRoot;
        public string sourceAssetPath;
        public string stagedFbxFullPath;
        public string sourceBasis;
        public string targetPose;
        public string targetClip;
        public string expectedStagedFbxSha256;
        public string stagedFbxSha256;
        public bool stagedFbxHashMatchesExpected;
        public string t26Marker;
        public string t31Marker;
        public string t46Marker;
        public int t31ExactTriangleRowsCaptured;
        public int t31TotalExactTriangleRecords;
        public int t31TotalInvertedTriangles;
        public int t31TotalCollapsedTriangles;
        public bool t31ExactTriangleCaptureMatchesT29Aggregate;
        public string selectedRenderer;
        public int sourceVertexCount;
        public int sourceSubmeshIndex;
        public int sourceSubmeshTriangleCount;
        public int sourceBoneCount;
        public int sourceBoneWeightCount;
        public int importedMaxInfluenceCount;
        public string sourceAvatarName;
        public string sourceAvatarBuildMode;
        public bool sourceAvatarIsValid;
        public bool sourceAvatarIsHuman;
        public int sourceAvatarHumanBoneCount;
        public string[] sourceAvatarMissingHumanBoneNames;
        public bool animatorPresent;
        public bool inMemoryMeshCloneCreated;
        public bool inMemoryMeshCloneAssignedOnlyToTransientInstance;
        public int exactTriangleRecordsLoaded;
        public int uniqueExactSourceTriangleOrdinalCount;
        public string[] uniqueExactSourceTriangleOrdinalRanges;
        public int affectedVertexCount;
        public int oneRingNeighborVertexCount;
        public int affectedPlusOneRingVertexCount;
        public int candidateCountPlanned;
        public bool sourceEditsAllowed;
        public bool sourceEditsExecuted;
        public bool sourceFbxMutationExecuted;
        public bool sourceWeightMutationExecuted;
        public bool candidateMapApplicationAllowed;
        public bool candidateMapApplicationExecuted;
        public bool conversionAllowed;
        public bool conversionExecuted;
        public bool sidecarGenerationExecuted;
        public bool runtimeLoaderChangeExecuted;
        public bool itemRegistrationExecuted;
        public bool equipInventorySaveWriteExecuted;
        public bool nativeGameFileWriteExecuted;
        public bool assetBundleBuildExecuted;
        public bool addressablesBuildExecuted;
        public bool unityProjectAssetMutationDuringSampling;
        public CandidateSearchSummary summary;
        public RequiredFieldResult[] requiredFieldResults;
        public ExactRowReport[] exactRows;
        public CandidateResult[] candidateResults;
        public CandidateResult bestCandidate;
        public CandidateResult zeroInversionCandidate;
        public string[] blockedReasons;
        public string[] notRun;
        public string[] errors;
    }

    [Serializable]
    private sealed class CandidateSearchSummary
    {
        public bool preconditionsReady;
        public bool searchExecuted;
        public bool stagedFbxHashMatchesExpected;
        public int exactTriangleRecordInputCount;
        public int exactTriangleRowsLoaded;
        public int affectedVertexCount;
        public int oneRingNeighborVertexCount;
        public int candidateCountPlanned;
        public int candidatesEvaluated;
        public bool zeroInversionCandidateFound;
        public string zeroInversionCandidateId;
        public string bestCandidateId;
        public int bestExactInvertedTriangles;
        public int bestExactCollapsedTriangles;
        public int baselineOriginalExactInvertedTriangles;
        public int baselineOriginalExactCollapsedTriangles;
        public bool sameT31FallAirLoopEvaluationPathExecuted;
        public bool fullT29MatrixEvaluationExecuted;
        public bool routeFailureReceiptEmitted;
        public bool sourceEditsExecuted;
        public bool sourceWeightMutationExecuted;
        public bool sourceFbxMutationExecuted;
        public bool candidateMapApplicationExecuted;
        public bool conversionExecuted;
        public bool sidecarGenerationExecuted;
        public bool runtimeLoaderChangeExecuted;
        public bool itemRegistrationExecuted;
        public bool equipInventorySaveWriteExecuted;
        public bool nativeGameFileWriteExecuted;
        public bool assetBundleBuildExecuted;
        public bool addressablesBuildExecuted;
    }

    [Serializable]
    private sealed class RequiredFieldResult
    {
        public string field;
        public string status;
        public string evidence;
    }

    [Serializable]
    private sealed class ExactRowReport
    {
        public string targetGender;
        public string targetSlot;
        public string nativeEquipmentType;
        public string sourceBoneRaw;
        public string policyFamily;
        public string materialName;
        public int materialSlotIndex;
        public int sourceBoneIndex;
        public int parentBoneIndex;
        public string parentBoneName;
        public int exactTriangleRecordCount;
        public int affectedVertexCount;
        public int uniqueSourceTriangleOrdinalCount;
        public string[] uniqueSourceTriangleOrdinalRanges;
        public bool triangleTripletsMatchCurrentMesh;
        public bool verticesInsideCurrentMeshRange;
    }

    [Serializable]
    private sealed class CandidateResult
    {
        public string candidateId;
        public string description;
        public string scope;
        public int overrideVertexCount;
        public int changedWeightRowCount;
        public int conflictingVertexAssignmentCount;
        public int candidateMaxInfluenceCount;
        public float sampleTime;
        public float clipLength;
        public string bundlePath;
        public string evaluationMode;
        public int curveBindingCount;
        public int matchingBindingPaths;
        public int changedTransformCount;
        public bool clipBindingApplied;
        public int exactTriangleRowsEvaluated;
        public int exactTriangleRecordsEvaluated;
        public int totalExactInvertedTriangles;
        public int totalExactCollapsedTriangles;
        public float maxVertexDisplacement;
        public float meanVertexDisplacement;
        public int movedAffectedVertexCount;
        public int remainingInvertedUniqueOrdinalCount;
        public string[] remainingInvertedUniqueOrdinalRanges;
        public int[] remainingInvertedUniqueOrdinalsFirst32;
        public bool zeroInversionCandidate;
        public RowCandidateMetric[] rowMetrics;
    }

    [Serializable]
    private sealed class RowCandidateMetric
    {
        public string targetGender;
        public string targetSlot;
        public string nativeEquipmentType;
        public string sourceBoneRaw;
        public string policyFamily;
        public int exactTriangleRecordsEvaluated;
        public int affectedVertexCount;
        public int invertedTriangleCount;
        public int collapsedTriangleCount;
        public int remainingInvertedUniqueOrdinalCount;
        public string[] remainingInvertedSourceTriangleOrdinalRanges;
        public int[] remainingInvertedSourceTriangleOrdinals;
        public float maxVertexDisplacement;
        public float meanVertexDisplacement;
        public int movedAffectedVertexCount;
        public bool rowZeroInversion;
    }
}
